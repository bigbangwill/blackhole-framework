using System;
using System.Linq;
using Blackhole.ModuleSystem.Composition;
using UnityEditor;
using UnityEngine;

namespace Blackhole.ModuleSystem.Editor
{
    [CustomEditor(typeof(ModuleManifestCatalogSO))]
    public sealed class ModuleManifestCatalogEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Find All Module Manifests"))
                FindAllModuleManifests();
        }

        private void FindAllModuleManifests()
        {
            ModuleManifestSO[] manifests = AssetDatabase
                .FindAssets($"t:{nameof(ModuleManifestSO)}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<ModuleManifestSO>)
                .Where(manifest => manifest != null)
                .Distinct()
                .OrderBy(manifest => manifest.ModuleID, StringComparer.Ordinal)
                .ToArray();

            serializedObject.Update();

            SerializedProperty manifestsProperty = serializedObject.FindProperty("moduleManifests");

            if (manifestsProperty == null)
            {
                Debug.LogError("Could not find the serialized field 'moduleManifests'.", target);
                return;
            }

            manifestsProperty.arraySize = manifests.Length;

            for (int i = 0; i < manifests.Length; i++)
                manifestsProperty.GetArrayElementAtIndex(i).objectReferenceValue = manifests[i];

            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();

            Debug.Log($"Found and registered {manifests.Length} module manifests.", target);
        }
    }
}
