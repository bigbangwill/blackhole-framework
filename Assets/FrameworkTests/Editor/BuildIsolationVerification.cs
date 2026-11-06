using System;
using System.IO;
using System.Linq;
using Blackhole.Framework.Composition.Editor;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Blackhole.Development.Editor
{
    /// <summary>Builds real players and records their packed assets and assembly boundaries.</summary>
    public static class BuildIsolationVerification
    {
        public const string AuditRoot = "TestResults/BuildIsolation-20261106";

        public static void Schedule(bool server, bool probe, BuildTarget target = BuildTarget.StandaloneWindows64)
        {
            EditorApplication.CallbackFunction run = null;
            run = () =>
            {
                if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
                EditorApplication.update -= run;
                Build(server, probe, target);
            };
            EditorApplication.update += run;
        }

        private static void Build(bool server, bool probe, BuildTarget target)
        {
            string label = (probe ? "probe-" : "final-") + (target == BuildTarget.StandaloneLinux64 ? "linux-" : string.Empty) + (server ? "server" : "client");
            Directory.CreateDirectory(AuditRoot);
            File.WriteAllText(AuditRoot + "/" + label + "-state.txt", "Building");
            try
            {
                var errors = FrameworkStartupValidator.Validate();
                if (errors.Count != 0) throw new InvalidOperationException(string.Join("\n", errors));
                var options = new BuildPlayerOptions
                {
                    scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(),
                    target = target,
                    subtarget = (int)(server ? StandaloneBuildSubtarget.Server : StandaloneBuildSubtarget.Player),
                    locationPathName = Path.GetFullPath(AuditRoot + "/" + label + "/Blackhole" + (server ? "Server" : "Client") + (target == BuildTarget.StandaloneLinux64 ? ".x86_64" : ".exe")),
                    options = BuildOptions.DetailedBuildReport,
                    extraScriptingDefines = probe ? new[] { "BLACKHOLE_BUILD_AUDIT" } : Array.Empty<string>()
                };
                BuildReport report = BuildPipeline.BuildPlayer(options);
                SaveReport(report, server, probe, label);
            }
            catch (Exception exception)
            {
                File.WriteAllText(AuditRoot + "/" + label + "-state.txt", "Failed: " + exception);
                Debug.LogException(exception);
            }
        }

        private static void SaveReport(BuildReport report, bool server, bool probe, string label)
        {
            string role = server ? "Server" : "Client";
            string opposite = server ? "Client" : "Server";
            string[] assets = report.packedAssets.SelectMany(packed => packed.contents).Select(item => item.sourceAssetPath).Where(path => !string.IsNullOrEmpty(path)).Distinct().OrderBy(path => path).ToArray();
            string root = Path.GetDirectoryName(report.summary.outputPath);
            string[] assemblies = Directory.Exists(root) ? Directory.GetFiles(root, "*.dll", SearchOption.AllDirectories).Select(Path.GetFileNameWithoutExtension).Distinct().OrderBy(name => name).ToArray() : Array.Empty<string>();
            string[] forbidden = assemblies.Where(name => name == "Blackhole.Networking.Composition." + opposite || name == "Blackhole.Networking.Infrastructure." + opposite || name.StartsWith("Blackhole.") && (name.EndsWith(".Editor") || name.EndsWith("Tests") || name.EndsWith(".TestSupport")) || name.StartsWith("UnityEditor") || name.StartsWith("nunit")).ToArray();
            string oppositeAsset = "Assets/BlackholeFramework/Networking/Data/" + (server ? "NetworkingInstaller" : "NetworkingServerInstaller") + ".asset";
            string selectedAsset = "Assets/BlackholeFramework/Networking/Data/" + (server ? "NetworkingServerInstaller" : "NetworkingInstaller") + ".asset";
            var result = new AuditReport
            {
                role = role,
                probe = probe,
                buildResult = report.summary.result.ToString(),
                output = report.summary.outputPath,
                seconds = report.summary.totalTime.TotalSeconds,
                errors = report.summary.totalErrors,
                warnings = report.summary.totalWarnings,
                assets = assets,
                assemblies = assemblies,
                forbiddenAssemblies = forbidden,
                oppositeInstallerPacked = assets.Contains(oppositeAsset),
                selectedInstallerPacked = assets.Contains(selectedAsset),
                messages = report.steps.SelectMany(step => step.messages).Where(message => message.type == LogType.Warning || message.type == LogType.Error).Select(message => message.content).Distinct().ToArray()
            };
            result.passed = report.summary.result == BuildResult.Succeeded && forbidden.Length == 0 && !result.oppositeInstallerPacked && result.selectedInstallerPacked && assemblies.Contains("Blackhole.Networking.Infrastructure." + role) && assemblies.Contains("Blackhole.Networking.Composition." + role);
            File.WriteAllText(AuditRoot + "/" + label + "-report.json", JsonUtility.ToJson(result, true));
            File.WriteAllText(AuditRoot + "/" + label + "-state.txt", result.passed ? "Passed" : "Failed audit: inspect report");
        }

        [Serializable]
        private sealed class AuditReport
        {
            public bool passed;
            public string role;
            public bool probe;
            public string buildResult;
            public string output;
            public double seconds;
            public int errors;
            public int warnings;
            public bool oppositeInstallerPacked;
            public bool selectedInstallerPacked;
            public string[] assets;
            public string[] assemblies;
            public string[] forbiddenAssemblies;
            public string[] messages;
        }
    }
}
