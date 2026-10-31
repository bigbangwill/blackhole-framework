using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Blackhole.Development.Editor
{
    [InitializeOnLoad]
    public static class CameraMovementTestVerification
    {
        private const string StateKey = "Blackhole.CameraMovementVerification.Mode";

        static CameraMovementTestVerification()
        {
            if (!string.IsNullOrEmpty(SessionState.GetString(StateKey, string.Empty))) TestRunnerApi.RegisterTestCallback(new ResultWriter());
        }

        [MenuItem("Blackhole/Development/Verify Camera Movement/EditMode")]
        public static void EditMode() => Run(TestMode.EditMode, "Blackhole.FrameworkDevelopment.EditModeTests");

        [MenuItem("Blackhole/Development/Verify Camera Movement/PlayMode")]
        public static void PlayMode() => Run(TestMode.PlayMode, "Blackhole.FrameworkDevelopment.PlayModeTests");

        private static void Run(TestMode mode, string assembly)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            {
                Debug.LogWarning("Finish compilation and leave Play Mode before starting camera test verification.");
                return;
            }
            SessionState.SetString(StateKey, mode.ToString());
            TestRunnerApi.RegisterTestCallback(new ResultWriter());
            TestRunnerApi api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.Execute(new ExecutionSettings(new Filter { testMode = mode, assemblyNames = new[] { assembly } }));
        }

        private sealed class ResultWriter : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun) { }
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }

            public void RunFinished(ITestResultAdaptor result)
            {
                string mode = SessionState.GetString(StateKey, string.Empty);
                if (string.IsNullOrEmpty(mode)) return;
                Directory.CreateDirectory("TestResults");
                TestRunnerApi.SaveResultToFile(result, $"TestResults/camera-scene-{mode}.xml");
                SessionState.EraseString(StateKey);
                Debug.Log($"Camera test {mode}: {result.PassCount} passed, {result.FailCount} failed.");
            }
        }
    }
}
