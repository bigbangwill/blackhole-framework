using Blackhole.ModuleSystem.Composition;
using UnityEditor;
using UnityEngine;

namespace Blackhole.Framework.Composition.Editor
{
    [CustomEditor(typeof(BlackholeProjectSettingsSO))]
    public sealed class BlackholeProjectSettingsSOEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("moduleCatalog"), new GUIContent("Module Catalog"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("startingModule"), new GUIContent("Game Starting Module"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("frameworkScene"), new GUIContent("Framework Scene"));

            ModuleManifestSO startingModule = serializedObject.FindProperty("startingModule").objectReferenceValue as ModuleManifestSO;
            if (startingModule != null)
            {
                SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(startingModule.ModuleScene);
                EditorGUILayout.ObjectField("Starting Scene", scene, typeof(SceneAsset), false);
                if (scene == null)
                    EditorGUILayout.HelpBox("The selected module must reference an existing scene.", MessageType.Error);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
