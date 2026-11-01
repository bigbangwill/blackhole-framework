using System;
using System.Linq;
using Blackhole.Framework.Composition;
using Blackhole.Framework.Composition.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Blackhole.Development.Editor
{
    public static class MoveEventSystemToFrameworkRoot
    {
        [MenuItem("Blackhole/Development/Move EventSystem To Framework Root")]
        public static void Move()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Move the EventSystem in Edit Mode.");

            const string interactionPath = "Assets/BlackholeFramework/WorldInteraction/WorldInteractionScene.unity";
            Scene framework = SceneManager.GetSceneByPath(FrameworkLifetimeScope.FrameworkScenePath);
            Scene interaction = SceneManager.GetSceneByPath(interactionPath);
            bool frameworkWasOpen = framework.IsValid() && framework.isLoaded;
            bool interactionWasOpen = interaction.IsValid() && interaction.isLoaded;
            if ((frameworkWasOpen && framework.isDirty) || (interactionWasOpen && interaction.isDirty))
                throw new InvalidOperationException("Save the framework and interaction scenes before moving the EventSystem.");

            Scene previousActive = SceneManager.GetActiveScene();
            if (!frameworkWasOpen) framework = EditorSceneManager.OpenScene(FrameworkLifetimeScope.FrameworkScenePath, OpenSceneMode.Additive);
            if (!interactionWasOpen) interaction = EditorSceneManager.OpenScene(interactionPath, OpenSceneMode.Additive);
            try
            {
                EventSystem[] systems = new[] { framework, interaction }.SelectMany(scene => scene.GetRootGameObjects())
                    .SelectMany(root => root.GetComponentsInChildren<EventSystem>(true)).ToArray();
                if (systems.Length != 1)
                    throw new InvalidOperationException("Expected exactly one existing EventSystem across the framework and interaction scenes.");

                GameObject eventSystem = systems[0].gameObject;
                if (eventSystem.scene != framework)
                {
                    eventSystem.transform.SetParent(null);
                    SceneManager.MoveGameObjectToScene(eventSystem, framework);
                    EditorSceneManager.MarkSceneDirty(framework);
                    EditorSceneManager.MarkSceneDirty(interaction);
                    EditorSceneManager.SaveScene(framework);
                    EditorSceneManager.SaveScene(interaction);
                }
            }
            finally
            {
                if (previousActive.IsValid() && previousActive.isLoaded) SceneManager.SetActiveScene(previousActive);
                if (!interactionWasOpen) EditorSceneManager.CloseScene(interaction, true);
                if (!frameworkWasOpen) EditorSceneManager.CloseScene(framework, true);
            }

            var errors = FrameworkStartupValidator.Validate();
            if (errors.Count > 0) throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
            Debug.Log("Shared EventSystem moved to framework root; FrameworkStartupValidator passed.");
        }
    }
}
