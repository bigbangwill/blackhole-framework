# Client/server build isolation verification

These development tools are excluded from framework release archives.

`Blackhole.Development.Editor.BuildIsolationVerification.Schedule(server, probe, target)` builds a real player using the enabled scenes and writes a packed-asset/assembly report under `TestResults/BuildIsolation-20261106`. `target` defaults to Windows64; pass `BuildTarget.StandaloneLinux64` for a Linux server. This does not change the project's scripting backend or persistent scripting defines.

Use `probe: false` for ordinary output. Use `probe: true` only for runtime verification builds. The latter adds `BLACKHOLE_BUILD_AUDIT` for that build alone, enabling the otherwise excluded `Blackhole.Development.BuildIsolation` assembly. Its runtime initializer checks framework startup, manifest selection, field layout and transport registration, then performs an echo exchange and writes a JSON result before exiting. Start the server first, wait for `BLACKHOLE_AUDIT_READY`, then start the matching client. Both support `-blackhole-audit-port <port>` and `-blackhole-audit-result <absolute-json-path>`.

`InspectPlayer.ps1` reads every managed DLL in a completed build using Unity's bundled Cecil library. It checks assembly names and references, implementation type definitions, the manifest getter IL and the concrete transport registered by the installer. Example:

```powershell
./Assets/FrameworkTests/BuildIsolation/InspectPlayer.ps1 -PlayerDirectory ./TestResults/BuildIsolation-20261106/final-linux-server -Role Server -CecilPath 'D:/Unity Editor/6000.5.0f1/Editor/Data/Managed/Unity.Cecil.dll' -ReportPath ./TestResults/server-metadata.json
```

The selected role's implementation must exist. The other role's implementation, installer field, assembly references and DLLs must not. Editor and test-runner assemblies are also rejected. Shared networking contracts intentionally remain in both players. Add `-Probe` when inspecting an instrumented build; normal builds must not contain the audit probe.

Packed-asset reports additionally verify that the unselected installer asset is absent. Both authoring references remain available in the Editor. A passing binary audit does not claim that all game information is secret: shared contracts, common framework modules and the selected role's code are shipped deliberately.

The 2026-11-06 audit built and inspected a Windows64 Mono client and Linux64 Mono Dedicated Server. It did not execute the Linux binary on the Windows host, which had no Linux runtime. The ordinary client was launched headlessly for a startup smoke check. See the generated report for exact coverage and results; no two-player native runtime exchange is claimed for that run.
