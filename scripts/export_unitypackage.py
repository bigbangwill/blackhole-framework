"""Build a deterministic Unity asset archive without running a licensed Editor."""

import argparse
import gzip
import hashlib
import io
import json
from pathlib import Path
import re
import tarfile


FRAMEWORK_PATH = Path("Assets/BlackholeFramework")
GUID_PATTERN = re.compile(r"^guid: ([a-f0-9]{32})\r?$", re.MULTILINE)
REFERENCE_PATTERN = re.compile(r"(?:\{fileID:\s*-?\d+,\s*guid:\s*|GUID:|[?&](?:amp;)?guid=)([a-f0-9]{32})")
TAG_PATTERN = re.compile(r"v(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-([0-9A-Za-z.-]+))?")
TEXT_EXTENSIONS = {".cs", ".asset", ".unity", ".prefab", ".mat", ".mixer", ".asmdef", ".json", ".uxml", ".uss", ".md", ".shader", ".hlsl", ".xml", ".nuspec", ".inputactions"}
# Input System's UI module/default actions and Unity UI's EventSystem.
DEPENDENCY_GUIDS = {"01614664b831546d2ae94a42149d80ac", "ca9f5fa95ffab41fb9a615ab714db018", "76c392e42b5098c458856cdf6ecaaaa1"}
SECRET_PATTERN = re.compile(r"eyJ[A-Za-z0-9_-]{20,}\.[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+|gh[pousr]_[A-Za-z0-9]{20,}|-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----")


def collect_assets(project):
    root = project / FRAMEWORK_PATH
    if not root.is_dir():
        raise ValueError(f"Missing framework folder: {FRAMEWORK_PATH}")
    metas = [root.with_suffix(".meta"), *sorted(root.rglob("*.meta"))]
    result = []
    seen = set()
    for meta in metas:
        if not meta.is_file():
            raise ValueError(f"Missing metadata: {meta.relative_to(project)}")
        asset = meta.with_suffix("")
        metadata = meta.read_bytes().replace(b"\r\n", b"\n")
        folder = re.search(rb"^folderAsset: yes$", metadata, re.MULTILINE) is not None
        # Git stores an empty Unity folder's .meta file but not the directory itself.
        if not asset.exists() and not folder:
            raise ValueError(f"Orphan metadata: {meta.relative_to(project)}")
        if folder and asset.exists() and not asset.is_dir():
            raise ValueError(f"Folder metadata points to a file: {meta.relative_to(project)}")
        match = GUID_PATTERN.search(metadata.decode("utf-8-sig"))
        if not match or match[1] in seen:
            raise ValueError(f"Invalid or duplicate GUID: {meta.relative_to(project)}")
        seen.add(match[1])
        if asset.is_symlink() or not asset.resolve().is_relative_to(root.resolve()):
            raise ValueError(f"Unsafe asset path: {asset.relative_to(project)}")
        result.append((asset.relative_to(project).as_posix(), match[1], metadata, None if folder or asset.is_dir() else asset.read_bytes()))
    for asset in root.rglob("*"):
        if asset.name == ".signature.p7s":
            continue  # NuGet's signing envelope is not imported as a Unity asset.
        if asset.name.startswith(".") or asset.is_symlink():
            raise ValueError(f"Unexpected hidden file or symlink: {asset.relative_to(project)}")
        if not asset.name.endswith(".meta") and not Path(str(asset) + ".meta").is_file():
            raise ValueError(f"Missing metadata: {asset.relative_to(project)}")
    return result


def release_contents(asset_path, contents, included_guids):
    if contents is None:
        return None
    if Path(asset_path).suffix not in TEXT_EXTENSIONS:
        return contents
    text = contents.decode("utf-8-sig").replace("\r\n", "\n")
    if asset_path.endswith("/Framework/BlackholeProjectSettings.asset"):
        text = re.sub(r"^  startingModule:.*$", "  startingModule: {fileID: 0}", text, flags=re.MULTILINE)
    if asset_path.endswith("/ModuleSystem/ModuleManifestCatalog.asset"):
        text = "\n".join(line for line in text.splitlines() if not (match := REFERENCE_PATTERN.search(line)) or match[1] in included_guids)
        text += "\n"
    if asset_path.endswith("/AudioPlayer/Settings/SoundLibrary.asset"):
        text = re.sub(r"  entries:[\s\S]*$", "  entries: []\n", text)
    if asset_path.endswith("/Authentication/Settings/SessionTokenSO.asset"):
        text = re.sub(r"^  (AccessToken|RefreshToken|TokenType):.*$", r"  \1:", text, flags=re.MULTILINE)
        text = re.sub(r"^  ExpiresIn:.*$", "  ExpiresIn: 0", text, flags=re.MULTILINE)
    if asset_path.endswith("/Authentication/Settings/AuthenticationSettingSO.asset"):
        text = re.sub(r"^  _baseUrl:.*$", "  _baseUrl:", text, flags=re.MULTILINE)
    if asset_path.endswith("/RestApiSets/Settings/RestApiSetSettingSO.asset"):
        text = re.sub(r"^    Url:.*$", "    Url:", text, flags=re.MULTILINE)
    if asset_path.endswith("/EncryptionSaveData/Settings/EncryptionSettingSO.asset"):
        text = re.sub(r"^  key:.*$", "  key: configure-a-project-specific-key", text, flags=re.MULTILINE)
    if SECRET_PATTERN.search(text):
        raise ValueError(f"Credential-like value in release asset: {asset_path}")
    if "Packages/com.bigbang.blackhole-framework" in text:
        raise ValueError(f"Obsolete package path in release asset: {asset_path}")
    # Assembly definitions can reference third-party packages. Serialized scene/asset
    # references must otherwise resolve within the exported framework or Input System.
    if Path(asset_path).suffix in {".asset", ".unity", ".prefab", ".mat", ".mixer", ".uxml", ".uss"}:
        for match in REFERENCE_PATTERN.finditer(text):
            if match[1] not in included_guids | DEPENDENCY_GUIDS and not match[1].startswith("0000000000000000"):
                raise ValueError(f"Reference outside the framework in release asset: {asset_path} ({match[1]})")
    return text.encode("utf-8")


def add_member(archive, name, contents):
    info = tarfile.TarInfo(name)
    info.size = len(contents)
    info.mode = 0o644
    info.mtime = 0
    archive.addfile(info, io.BytesIO(contents))


def build_package(project, tag, output):
    match = TAG_PATTERN.fullmatch(tag)
    if not match:
        raise ValueError("Release tag must have the form v1.2.3 or v1.2.3-preview.1")
    version = json.loads((project / FRAMEWORK_PATH / "Version.json").read_text(encoding="utf-8-sig"))
    if tag[1:] != version["version"]:
        raise ValueError(f"Tag {tag} does not match Version.json ({version['version']})")
    assets = collect_assets(project)
    included_guids = {asset[1] for asset in assets}
    package_assets = [(asset_path, guid, meta, release_contents(asset_path, contents, included_guids)) for asset_path, guid, meta, contents in assets]
    output.mkdir(parents=True, exist_ok=True)
    package = output / f"BlackholeFramework-{version['version']}.unitypackage"
    with package.open("wb") as raw, gzip.GzipFile(fileobj=raw, mode="wb", filename="", mtime=0) as zipped:
        with tarfile.open(fileobj=zipped, mode="w", format=tarfile.GNU_FORMAT) as archive:
            for asset_path, guid, meta, contents in package_assets:
                add_member(archive, f"{guid}/pathname", asset_path.encode("utf-8"))
                add_member(archive, f"{guid}/asset.meta", meta)
                if contents is not None:
                    add_member(archive, f"{guid}/asset", contents)
    digest = hashlib.sha256(package.read_bytes()).hexdigest()
    (output / "SHA256SUMS.txt").write_text(f"{digest}  {package.name}\n", encoding="utf-8")
    inventory = {"version": version["version"], "unity": version["unity"], "dependencies": version["dependencies"], "assets": [asset[0] for asset in package_assets]}
    (output / "package-inventory.json").write_text(json.dumps(inventory, indent=2) + "\n", encoding="utf-8")
    print(f"Created {package.name}: {len(package_assets)} assets; SHA256 {digest}")
    return package


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--tag", required=True)
    parser.add_argument("--project", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--output", type=Path, default=Path("dist"))
    arguments = parser.parse_args()
    try:
        build_package(arguments.project.resolve(), arguments.tag, arguments.output.resolve())
    except (ValueError, OSError, KeyError, json.JSONDecodeError) as error:
        parser.exit(1, f"Export failed: {error}\n")


if __name__ == "__main__":
    main()
