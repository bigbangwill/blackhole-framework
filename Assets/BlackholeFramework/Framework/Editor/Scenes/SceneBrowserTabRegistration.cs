using System;
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
            // Unity has no public hook for every window's Add Tab menu.
            // Append one item without replacing the mode's built-in pane list.
            const BindingFlags staticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            try
            {
                Type hostView = typeof(EditorWindow).Assembly.GetType("UnityEditor.HostView");
                EventInfo menuEvent = hostView?.GetEvent("populateDefaultMenuItems", staticFlags);
                MethodInfo addHandler = menuEvent?.GetAddMethod(true);
                MethodInfo removeHandler = menuEvent?.GetRemoveMethod(true);
                if (addHandler == null || removeHandler == null)
                {
                    Debug.LogWarning("Scenes could not register in Add Tab. Use Window > General > Scene Browser.");
                    return;
                }

                Action<GenericMenu, EditorWindow> handler = AddSceneBrowser;
                removeHandler.Invoke(null, new object[] { handler });
                addHandler.Invoke(null, new object[] { handler });
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Scenes could not register in Add Tab. Use Window > General > Scene Browser. {exception.Message}");
            }
        }

        private static void AddSceneBrowser(GenericMenu menu, EditorWindow window)
        {
            menu.AddItem(new GUIContent("Add Tab/Scenes"), false, () => EditorWindow.CreateWindow<SceneBrowserWindow>("Scenes", window.GetType()));
        }
    }
}
