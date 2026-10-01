using Blackhole.GlobalDialogue.Presentation;
using UnityEditor;
using UnityEngine;
using System.Text;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;

[CustomEditor(typeof(EffectSettingSO))]
public class EffectDataEditor : Editor
{

    private const string GeneratedFolder = "Assets/BlackholeFramework/GlobalDialogue/Scripts/Contract/Dto";
    private const string GeneratedFileName = "EffectType.g.cs";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var database = (EffectSettingSO)target;
        EditorGUILayout.Space();

        if (GUILayout.Button("Generate / Update Enums"))
        {
            GenerateNamesFile(database);
        }
    }

    private void GenerateNamesFile(EffectSettingSO database)
    {
        List<string> names = new List<string>();
        string tempName;
        StringBuilder sb = new();

        sb.AppendLine("// This is Auto-Generated");
        sb.AppendLine("namespace Blackhole.GlobalDialogue.Contract");
        sb.AppendLine("{");
        sb.AppendLine("\t public enum EffectType \n\t  {");
        foreach (var st in database.EffectDatas)
        {
            tempName = st.EffectName;
            tempName = tempName.Replace(" ", "");
            if (!names.Contains(tempName))
                sb.AppendLine("\t\t" + tempName + ", ");
            names.Add(tempName);
        }
        sb.AppendLine("\t  }");
        sb.AppendLine("}");

        // Make sure the folder exists
        if (!Directory.Exists(GeneratedFolder))
        {
            Directory.CreateDirectory(GeneratedFolder);
        }

        string filePath = Path.Combine(GeneratedFolder, GeneratedFileName);

        // Create or overwrite the file
        File.WriteAllText(filePath, sb.ToString());

        // Tell Unity that the asset changed
        AssetDatabase.Refresh();

        Debug.Log($"Generated EffectType.cs at: {filePath}");
    }

}

