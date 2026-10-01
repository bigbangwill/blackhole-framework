import io
import json
from pathlib import Path
import tarfile
import tempfile
import unittest

from export_unitypackage import build_package, collect_assets, release_contents


class PackageExportTests(unittest.TestCase):
    def test_configured_game_references_and_session_are_removed(self):
        guid = "a" * 32
        external_guid = "b" * 32
        catalog = f"moduleManifests:\n  - {{fileID: 11400000, guid: {guid}, type: 2}}\n  - {{fileID: 11400000, guid: {external_guid}, type: 2}}\n"
        exported = release_contents("Assets/BlackholeFramework/ModuleSystem/ModuleManifestCatalog.asset", catalog.encode(), {guid})
        self.assertIn(guid.encode(), exported)
        self.assertNotIn(external_guid.encode(), exported)
        session = b"  AccessToken: saved-login\n  RefreshToken: saved-refresh\n  ExpiresIn: 14400\n  TokenType: Bearer\n"
        exported = release_contents("Assets/BlackholeFramework/Authentication/Settings/SessionTokenSO.asset", session, set())
        self.assertNotIn(b"saved-login", exported)
        self.assertNotIn(b"saved-refresh", exported)
        self.assertIn(b"ExpiresIn: 0", exported)

    def test_unknown_serialized_reference_is_rejected(self):
        with self.assertRaisesRegex(ValueError, "outside the framework"):
            release_contents("Assets/BlackholeFramework/settings.asset", b"setting: {fileID: 11400000, guid: cccccccccccccccccccccccccccccccc, type: 2}", set())

    def test_ui_asset_references_are_validated_and_line_endings_normalized(self):
        with self.assertRaisesRegex(ValueError, "outside the framework"):
            release_contents("Assets/BlackholeFramework/ui.uxml", b'<Style src="project://database/Assets/Game/style.uss?fileID=1&amp;guid=cccccccccccccccccccccccccccccccc" />', set())
        self.assertEqual(release_contents("Assets/BlackholeFramework/Version.json", b'{"version":"1.2.3"}\r\n', set()), b'{"version":"1.2.3"}\n')

    def test_missing_metadata_is_rejected(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            framework = root / "Assets/BlackholeFramework"
            framework.mkdir(parents=True)
            framework.with_suffix(".meta").write_text("guid: " + "a" * 32 + "\n")
            (framework / "Missing.cs").write_text("// untracked Unity asset\n")
            with self.assertRaisesRegex(ValueError, "Missing metadata"):
                collect_assets(root)

    def test_duplicate_guid_is_rejected(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            framework = root / "Assets/BlackholeFramework"
            framework.mkdir(parents=True)
            metadata = "guid: " + "a" * 32 + "\n"
            framework.with_suffix(".meta").write_text(metadata)
            (framework / "Duplicate.cs").write_text("// duplicate\n")
            (framework / "Duplicate.cs.meta").write_text(metadata)
            with self.assertRaisesRegex(ValueError, "duplicate GUID"):
                collect_assets(root)

    def test_release_is_reproducible_and_contains_unity_archive_entries(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            framework = root / "Assets/BlackholeFramework"
            framework.mkdir(parents=True)
            framework.with_suffix(".meta").write_text("guid: " + "a" * 32 + "\n")
            (framework / "Version.json").write_text(json.dumps({"version": "1.2.3", "unity": "6000.5.0f1", "dependencies": {}}))
            (framework / "Version.json.meta").write_text("guid: " + "b" * 32 + "\n")
            first = build_package(root, "v1.2.3", root / "first").read_bytes()
            second = build_package(root, "v1.2.3", root / "second").read_bytes()
            self.assertEqual(first, second)
            with tarfile.open(fileobj=io.BytesIO(first), mode="r:gz") as archive:
                self.assertEqual(archive.extractfile("b" * 32 + "/pathname").read(), b"Assets/BlackholeFramework/Version.json")
                self.assertIn("a" * 32 + "/asset.meta", archive.getnames())
                self.assertNotIn("a" * 32 + "/asset", archive.getnames())
            with self.assertRaisesRegex(ValueError, "does not match"):
                build_package(root, "v1.2.4", root / "mismatch")


if __name__ == "__main__":
    unittest.main()
