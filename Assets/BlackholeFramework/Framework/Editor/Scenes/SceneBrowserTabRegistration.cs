using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Blackhole.Framework.Composition.Editor
{
    internal static class SceneBrowserTabRegistration
    {
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            QueueRegistration();
            EditorApplication.projectChanged += QueueRegistration;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state) => QueueRegistration();

        private static void QueueRegistration()
        {
            EditorApplication.delayCall -= Register;
            EditorApplication.delayCall += Register;
        }

        private static void Register()
        {
            // Unity has no public registry for adding a pane to every window's Add Tab menu.
            // Extend only the default mode's in-memory list.
            const BindingFlags staticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            try
            {
                Assembly editorAssembly = typeof(EditorWindow).Assembly;
                Type modeService = editorAssembly.GetType("UnityEditor.ModeService");
                Array modes = modeService?.GetProperty("modes", staticFlags)?.GetValue(null) as Array;
                MethodInfo getBuiltInPanes = editorAssembly.GetType("UnityEditor.HostView")?.GetMethod("GetBuiltInPaneTypes", staticFlags);
                if (modes == null || getBuiltInPanes == null)
                {
                    Debug.LogWarning("Scenes could not register in Add Tab. Use Window > General > Scene Browser.");
                    return;
                }

                foreach (object mode in modes)
                {
                    Type modeType = mode.GetType();
                    if (modeType.GetField("id", instanceFlags)?.GetValue(mode) as string != "default") continue;
                    if (modeType.GetField("data", instanceFlags)?.GetValue(mode) is not IDictionary data) continue;

                    List<object> panes = data["pane_types"] is IList<object> existing
                        ? new List<object>(existing)
                        : ((IEnumerable<Type>)getBuiltInPanes.Invoke(null, null)).Select(type => (object)type.FullName).ToList();
                    string sceneBrowserType = typeof(SceneBrowserWindow).FullName;
                    if (!panes.Contains(sceneBrowserType)) panes.Add(sceneBrowserType);
                    data["pane_types"] = panes;
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Scenes could not register in Add Tab. Use Window > General > Scene Browser. {exception.Message}");
            }
        }
    }
}
