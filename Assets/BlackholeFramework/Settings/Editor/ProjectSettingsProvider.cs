using Blackhole.Settings.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Blackhole.Settings.Editor
{
    public class ProjectSettingsProvider
    {
        private sealed class SettingEntry
        {
            public string Section;
            public int Order;
            public ScriptableObject Asset;
            public UnityEditor.Editor Editor;
            public bool Expanded = true;
        }

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            var entries = new List<SettingEntry>();

            var provider = new SettingsProvider("Project/Blackhole", SettingsScope.Project)
            {
                label = "Blackhole",

                activateHandler = (_, _) => DiscoverSettings(entries),

                deactivateHandler = () => DisposeEditors(entries),

                guiHandler = _ => DrawSettings(entries)
            };

            return provider;
        }

        private static void DiscoverSettings(List<SettingEntry> entries)
        {
            DisposeEditors(entries);
            entries.Clear();

            foreach (Type type in TypeCache.GetTypesWithAttribute<ProjectSettingsAttribute>())
            {

                if (!typeof(ScriptableObject).IsAssignableFrom(type))
                    continue;

                ProjectSettingsAttribute attribute = type.GetCustomAttribute<ProjectSettingsAttribute>();


                foreach (string guid in AssetDatabase.FindAssets($"t:{type.Name}"))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath(path, type) as ScriptableObject;

                    if(asset == null || asset.GetType() != type)
                        continue;

                    entries.Add(new SettingEntry
                    {
                        Section = attribute.Section,
                        Order = attribute.Order,
                        Asset = asset
                    });
                }
            }

            entries.Sort((left, right) =>
            {
                int sectionComparision = string.Compare(left.Section, right.Section, StringComparison.Ordinal);
                return sectionComparision != 0 ? sectionComparision : left.Order.CompareTo(right.Order);
            });
        }

        private static void DrawSettings(List<SettingEntry> entries)
        {
            if (entries.Count == 0)
            {
                EditorGUILayout.HelpBox("No settings was found.", MessageType.Info);
                return;
            }

            foreach (IGrouping<string, SettingEntry> group in entries.GroupBy(entry => entry.Section))
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.LabelField(group.Key, EditorStyles.boldLabel);

                foreach (SettingEntry entry in group)
                {
                    DrawSetting(entry);
                }
            }

        }

        private static void DrawSetting(SettingEntry entry)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                entry.Expanded = EditorGUILayout.InspectorTitlebar(entry.Expanded, entry.Asset);
                if(!entry.Expanded)
                    return;

                UnityEditor.Editor.CreateCachedEditor(entry.Asset, null, ref entry.Editor);
                using (new EditorGUI.IndentLevelScope())
                    entry.Editor.OnInspectorGUI();
            }
        }


        private static void DisposeEditors(List<SettingEntry> entries)
        {
            foreach (SettingEntry entry in entries)
            {
                if (entry.Editor != null)
                    UnityEngine.Object.DestroyImmediate(entry.Editor);
            }

        }
    }
}