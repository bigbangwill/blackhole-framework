using System;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditor.Compilation;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Blackhole.Framework.Composition.Editor
{
    // Keep Unity's importer state, list callbacks, validation and persistence. Only change its layout.
    [CustomEditor(typeof(AssemblyDefinitionImporter))]
    [CanEditMultipleObjects]
    public sealed class AssemblyDefinitionInspector : AssetImporterEditor
    {
        private const string PlatformsExpandedKey = "Blackhole.Framework.Editor.AssemblyDefinition.PlatformsExpanded";
        private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly Type BuiltInType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.AssemblyDefinitionImporterInspector", true);
        private static readonly Type StylesType = BuiltInType.GetNestedType("Styles", BindingFlags.NonPublic);
        private static readonly PropertyInfo DataProperty = typeof(AssetImporterEditor).GetProperty("extraDataSerializedObject", InstanceFlags);
        private static readonly MethodInfo SetAssetEditor = typeof(AssetImporterEditor).GetMethod("InternalSetAssetImporterTargetEditor", InstanceFlags);
        private static readonly FieldInfo AssetEditorField = typeof(AssetImporterEditor).GetField("m_AssetEditor", InstanceFlags);
        private AssetImporterEditor _builtIn;
        private SerializedObject _data;
        private Action _updateData;
        private Action _applyData;
        private bool _customLayout;

        public override bool showImportedObject => false;
        protected override bool needsApplyRevert => _customLayout && base.needsApplyRevert;

        public override void OnEnable()
        {
            // Initialize the original importer first so it owns the extra-data copy shared by both editors.
            _builtIn = (AssetImporterEditor)CreateEditor(targets, BuiltInType);
            base.OnEnable();
            try
            {
                _data = (SerializedObject)DataProperty.GetValue(_builtIn);
                _updateData = (Action)Delegate.CreateDelegate(typeof(Action), _data, _data.GetType().GetMethod("Update", InstanceFlags));
                _applyData = (Action)Delegate.CreateDelegate(typeof(Action), _data, _data.GetType().GetMethod("ApplyModifiedProperties", InstanceFlags));
                _customLayout = HasExpectedLayout();
            }
            catch (Exception exception) when (exception is ArgumentException || exception is MemberAccessException || exception is NullReferenceException)
            {
                _customLayout = false;
            }
        }

        public override void OnDisable()
        {
            try { base.OnDisable(); }
            finally
            {
                if (_builtIn != null) DestroyImmediate(_builtIn);
            }
        }

        public override bool HasModified() => _builtIn != null && _builtIn.HasModified();

        protected override void Apply() => _builtIn.SaveChanges();

        // Draw the buttons before Platforms; leave Unity's postprocessor section at the bottom.
        protected override bool OnApplyRevertGUI() => false;

        public override void DiscardChanges()
        {
            _builtIn.DiscardChanges();
            base.DiscardChanges();
        }

        public override void OnInspectorGUI()
        {
            if (!_customLayout)
            {
                // A changed Unity internal API should restore the standard inspector, never lose settings.
                SetAssetEditor.Invoke(_builtIn, new[] { AssetEditorField.GetValue(this) });
                _builtIn.OnInspectorGUI();
                hasUnsavedChanges = _builtIn.hasUnsavedChanges;
                return;
            }

            if (Field<Exception>("initializeException") is Exception exception)
            {
                Invoke("ShowLoadErrorExceptionGUI", exception);
                DrawApplyRevertButtons();
                ApplyRevertGUI();
                return;
            }

            _updateData();
            using (new EditorGUI.DisabledScope(Field<bool>("m_AssetIsReadonly")))
            {
                DrawNameAndGeneralOptions();
                DrawReferences();
            }

            // Commit pending controls before Unity checks whether Apply/Revert should be enabled.
            _applyData();
            DrawApplyRevertButtons();

            using (new EditorGUI.DisabledScope(Field<bool>("m_AssetIsReadonly")))
            {
                GUILayout.Space(6f);
                bool expanded = SessionState.GetBool(PlatformsExpandedKey, false);
                bool nextExpanded;
                using (new EditorGUI.IndentLevelScope()) nextExpanded = EditorGUILayout.Foldout(expanded, Label("platforms"), true);
                if (nextExpanded != expanded) SessionState.SetBool(PlatformsExpandedKey, nextExpanded);
                if (nextExpanded) DrawPlatforms();
                GUILayout.Space(6f);
                DrawDefineConstraints();
                GUILayout.Space(6f);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label(Label("versionDefines"), EditorStyles.boldLabel);
                Field<ReorderableList>("m_VersionDefineList").DoLayoutList();
                EditorGUILayout.EndVertical();
            }

            _applyData();
            ApplyRevertGUI();
        }

        private void DrawApplyRevertButtons()
        {
            hasUnsavedChanges = HasModified();
            if (!needsApplyRevert) return;
            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (base.OnApplyRevertGUI()) GUIUtility.ExitGUI();
            }
        }

        private void DrawNameAndGeneralOptions()
        {
            if (Field<bool>("m_AssetIsReadonly"))
                EditorGUILayout.LabelField(targets.Length > 1 ? "One of the selected assembly definition files is read-only." : "The selected assembly definition file is read-only.");

            if (targets.Length > 1)
            {
                using (new EditorGUI.DisabledScope(true))
                    EditorGUILayout.TextField(Label("name"), string.Join(", ", _data.targetObjects.Select(item => item.name)), EditorStyles.textField);
            }
            else DrawProperty("assemblyName", "name");

            GUILayout.Space(6f);
            GUILayout.Label(Label("generalOptions"), EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            DrawProperty("allowUnsafeCode", "allowUnsafeCode");
            DrawProperty("autoReferenced", "autoReferenced");
            DrawProperty("noEngineReferences", "noEngineReferences");
            DrawProperty("overrideReferences", "overrideReferences");
            DrawProperty("rootNamespace", "rootNamespace");
            EditorGUILayout.EndVertical();
            GUILayout.Space(6f);
        }

        private void DrawReferences()
        {
            GUILayout.Label(Label("references"), EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            var references = Field<ReorderableList>("m_ReferencesList");
            using (new EditorGUI.DisabledScope(references.serializedProperty.arraySize == 0)) DrawProperty("useGUIDs", "useGUIDs");
            EditorGUILayout.EndVertical();
            references.DoLayoutList();
            DrawMissingReferences("references", "asset");

            SerializedProperty overrides = _data.FindProperty("overrideReferences");
            if (!overrides.boolValue || overrides.hasMultipleDifferentValues) return;
            GUILayout.Label(Label("precompiledReferences"), EditorStyles.boldLabel);
            Invoke("UpdatePrecompiledReferenceListEntry");
            Field<ReorderableList>("m_PrecompiledReferencesList").DoLayoutList();
            DrawMissingReferences("precompiledReferences", "path");
        }

        private void DrawMissingReferences(string listName, string member)
        {
            foreach (Object item in _data.targetObjects)
            {
                using var state = new SerializedObject(item);
                SerializedProperty list = state.FindProperty(listName);
                for (int i = 0; i < list.arraySize; i++)
                {
                    SerializedProperty entry = list.GetArrayElementAtIndex(i);
                    SerializedProperty value = entry.FindPropertyRelative(member);
                    bool missing = member == "asset" ? value.objectReferenceValue == null : string.IsNullOrEmpty(value.stringValue) && !string.IsNullOrEmpty(entry.FindPropertyRelative("name").stringValue);
                    if (!missing) continue;
                    EditorGUILayout.HelpBox("The grayed out assembly references are missing and will not be referenced during compilation.", MessageType.Info);
                    return;
                }
            }
        }

        private void DrawPlatforms()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            SerializedProperty any = _data.FindProperty("compatibleWithAnyPlatform");
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(any, Label("anyPlatform"));
                if (change.changed)
                {
                    bool compatible = any.boolValue;
                    Object[] invert = _data.targetObjects.Where(item => (bool)item.GetType().GetField("compatibleWithAnyPlatform").GetValue(item) != compatible).ToArray();
                    _applyData();
                    foreach (Object item in invert) InvokeStatic("InversePlatformCompatibility", item);
                    _updateData();
                }
            }

            if (!any.hasMultipleDifferentValues)
            {
                GUILayout.Label(Label(any.boolValue ? "excludePlatforms" : "includePlatforms"), EditorStyles.boldLabel);
                AssemblyDefinitionPlatform[] platforms = CompilationPipeline.GetAssemblyDefinitionPlatforms();
                SerializedProperty compatibility = _data.FindProperty("platformCompatibility");
                for (int i = 0; i < platforms.Length; i++)
                {
                    if (i >= compatibility.arraySize)
                    {
                        compatibility.arraySize++;
                        compatibility.GetArrayElementAtIndex(i).boolValue = false;
                    }
                    EditorGUILayout.PropertyField(compatibility.GetArrayElementAtIndex(i), new GUIContent(platforms[i].DisplayName));
                }

                EditorGUILayout.Space();
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(Label("selectAll"))) SetPlatforms(compatibility, true);
                    if (GUILayout.Button(Label("deselectAll"))) SetPlatforms(compatibility, false);
                    GUILayout.FlexibleSpace();
                }
            }
            EditorGUILayout.EndVertical();
        }

        private static void SetPlatforms(SerializedProperty compatibility, bool value)
        {
            for (int i = 0; i < compatibility.arraySize; i++) compatibility.GetArrayElementAtIndex(i).boolValue = value;
        }

        private void DrawDefineConstraints()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(Label("defineConstraints"), EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            var constraints = Field<ReorderableList>("m_DefineConstraints");
            if (constraints.serializedProperty.arraySize > 0)
            {
                string[] defines = (string[])Invoke("GetDefines");
                bool compatible = true;
                for (int i = 0; i < constraints.serializedProperty.arraySize; i++)
                {
                    string constraint = constraints.serializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue;
                    compatible &= CompilationPipeline.IsDefineConstraintsCompatible(defines, new[] { constraint });
                }
                var rect = new Rect(GUILayoutUtility.GetLastRect());
                rect.x = rect.width - 4;
                rect.width = 16;
                rect.height = 16;
                var icon = (Texture2D)StylesType.GetProperty(compatible ? "validDefineConstraint" : "invalidDefineConstraint").GetValue(null);
                string tooltip = (string)StylesType.GetMethod("GetTitleTooltipFromDefineConstraintCompatibility").Invoke(null, new object[] { compatible });
                EditorGUI.LabelField(rect, new GUIContent(icon, tooltip));
            }
            constraints.DoLayoutList();
        }

        private bool HasExpectedLayout()
        {
            string[] fields = { "initializeException", "m_AssetIsReadonly", "m_ReferencesList", "m_PrecompiledReferencesList", "m_DefineConstraints", "m_VersionDefineList" };
            string[] methods = { "ShowLoadErrorExceptionGUI", "UpdatePrecompiledReferenceListEntry", "GetDefines" };
            string[] properties = { "assemblyName", "rootNamespace", "allowUnsafeCode", "autoReferenced", "noEngineReferences", "overrideReferences", "useGUIDs", "compatibleWithAnyPlatform", "platformCompatibility" };
            return StylesType != null && fields.All(name => BuiltInType.GetField(name, InstanceFlags) != null) && methods.All(name => BuiltInType.GetMethod(name, InstanceFlags) != null) && properties.All(name => _data.FindProperty(name) != null) && BuiltInType.GetMethod("InversePlatformCompatibility", StaticFlags) != null;
        }

        private void DrawProperty(string name, string label) => EditorGUILayout.PropertyField(_data.FindProperty(name), Label(label));
        private static GUIContent Label(string name) => (GUIContent)StylesType.GetField(name, StaticFlags).GetValue(null);
        private T Field<T>(string name) => (T)BuiltInType.GetField(name, InstanceFlags).GetValue(_builtIn);
        private object Invoke(string name, params object[] arguments) => InvokeMethod(BuiltInType.GetMethod(name, InstanceFlags), _builtIn, arguments);
        private static object InvokeStatic(string name, params object[] arguments) => InvokeMethod(BuiltInType.GetMethod(name, StaticFlags), null, arguments);

        private static object InvokeMethod(MethodInfo method, object target, object[] arguments)
        {
            try { return method.Invoke(target, arguments); }
            catch (TargetInvocationException exception) when (exception.InnerException != null)
            {
                // ExitGUIException must reach Unity directly when Apply triggers an import/domain reload.
                ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
                throw;
            }
        }
    }
}
