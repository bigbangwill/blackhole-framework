using UnityEditor;
using UnityEngine.UIElements;

namespace BigBang.UIToolkitComponent.Editor
{
    [InitializeOnLoad]
    internal static class DefaultPanelTransitionProfileGenerator
    {
        private const string RootPath = "Assets/BlackholeFramework/UIToolkitComponent/PanelTransition";
        private const string ResourcesPath = RootPath + "/Resources";
        private const string TransitionResourcesPath = ResourcesPath + "/PanelTransition";
        private const string ProfilesPath = TransitionResourcesPath + "/Profiles";
        private const string StylesPath = RootPath + "/Styles";

        private static readonly string[] ProfileNames =
        {
            "FromTop",
            "Fade",
            "ScaleFade",
            "ZoomFade",
            "SlideLeft",
            "SlideRight",
            "SlideUp",
            "SlideDown"
        };

        static DefaultPanelTransitionProfileGenerator()
        {
            EditorApplication.delayCall += CreateMissingProfiles;
        }

        [MenuItem("Tools/BigBang/UI Toolkit/Create Default Panel Transition Profiles")]
        private static void CreateMissingProfiles()
        {
            EnsureProfilesFolderExists();
            bool createdAny = false;

            foreach (string profileName in ProfileNames)
                createdAny |= CreateProfileIfMissing(profileName);

            if (createdAny)
                AssetDatabase.SaveAssets();
        }

        private static bool CreateProfileIfMissing(string profileName)
        {
            string profilePath = $"{ProfilesPath}/{profileName}.asset";
            if (AssetDatabase.LoadAssetAtPath<PanelTransitionProfileSO>(profilePath) != null)
                return false;

            string stylePath = $"{StylesPath}/PanelTransition{profileName}.uss";
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(stylePath);
            if (styleSheet == null)
                return false;

            var profile = UnityEngine.ScriptableObject.CreateInstance<PanelTransitionProfileSO>();
            var serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("styleSheet").objectReferenceValue = styleSheet;
            serializedProfile.FindProperty("baseClass").stringValue = GetBaseClass(profileName);
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(profile, profilePath);
            return true;
        }

        private static string GetBaseClass(string profileName)
        {
            return profileName switch
            {
                "FromTop" => "panel-transition-from-top",
                "Fade" => "panel-transition-fade",
                "ScaleFade" => "panel-transition-scale-fade",
                "ZoomFade" => "panel-transition-zoom-fade",
                "SlideLeft" => "panel-transition-slide-left",
                "SlideRight" => "panel-transition-slide-right",
                "SlideUp" => "panel-transition-slide-up",
                "SlideDown" => "panel-transition-slide-down",
                _ => "panel-transition"
            };
        }

        private static void EnsureProfilesFolderExists()
        {
            if (!AssetDatabase.IsValidFolder(ResourcesPath))
                AssetDatabase.CreateFolder(RootPath, "Resources");

            if (!AssetDatabase.IsValidFolder(TransitionResourcesPath))
                AssetDatabase.CreateFolder(ResourcesPath, "PanelTransition");

            if (!AssetDatabase.IsValidFolder(ProfilesPath))
                AssetDatabase.CreateFolder(TransitionResourcesPath, "Profiles");
        }
    }
}
