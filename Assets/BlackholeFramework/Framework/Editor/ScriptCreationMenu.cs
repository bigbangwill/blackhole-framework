using UnityEditor;

namespace Blackhole.Framework.Composition.Editor
{
    public static class ScriptCreationMenu
    {
        private const string TemplateRoot = FrameworkProjectSetup.PackageRoot + "/Framework/Editor/ScriptTemplates/";

        [MenuItem("Assets/Create/Interface", false, -219)]
        private static void CreateInterface()
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(TemplateRoot + "Interface.cs.txt", "INewInterface.cs");
        }

        [MenuItem("Assets/Create/C# Class", false, -217)]
        private static void CreateClass()
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(TemplateRoot + "Class.cs.txt", "NewClass.cs");
        }
    }
}
