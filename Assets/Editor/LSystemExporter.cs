using System.IO;
using UnityEditor;
using UnityEngine;

namespace LSystem
{
    public class LSystemExporter
    {
        // Creates a new menu item 'PG649 > Create Prefab' in the main menu.
        [MenuItem("PG649/Export LSystem Settings")]
        private static void ExportLSystem()
        {
            // Keep track of the currently selected GameObject
            GameObject go = Selection.activeObject as GameObject;

            if (go == null || go.TryGetComponent(out LSystemEditor skeleton) == false)
            {
                EditorUtility.DisplayDialog(
                                "Select LSystemEditor",
                                "Thou shalt select a gameobject with an attached LSystemEditor Component!",
                                "Sure thing!");
                return;
            }

            string path = EditorUtility.SaveFilePanel(
            "Save texture as JSON",
            "Packages/com.pg649.creaturegenerator",
            go.name + " lsystem",
            "json");// absolute path

            if (path.Length != 0)
            {
                var prop = skeleton.GenerateProperties();
                var jsonData = JsonUtility.ToJson(prop);
                StreamWriter writer = new(path, false);
                writer.WriteLine(jsonData);
                writer.Close();
                Debug.Log("JSON was saved successfully at: " + path);
                var projectpath = Application.dataPath[..Application.dataPath.LastIndexOf("/")];
                // Re-import the file to update the reference in the editor
                if (path.StartsWith(projectpath)) // if path is under project make path relative to project to import into assetDatabase
                {
                    path = path[(projectpath.Length + 1)..];
                    AssetDatabase.ImportAsset(path);
                }
            }
        }

        // Disable the menu item if no selection is in place or editor is not in play mode.
        [MenuItem("PG649/Export LSystem Settings", true)]
        private static bool ValidateExportLSystem()
        {
            return Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<LSystemEditor>() != null && !EditorUtility.IsPersistent(Selection.activeGameObject);
        }


        // Creates a new menu item 'PG649 > Create Prefab' in the main menu.
        [MenuItem("PG649/Import LSystem Settings")]
        private static void ImportLSystem()
        {
            // Keep track of the currently selected GameObject
            GameObject go = Selection.activeObject as GameObject;

            if (go == null || go.TryGetComponent(out LSystemEditor skeleton) == false)
            {
                EditorUtility.DisplayDialog(
                                "Select LSystemEditor",
                                "Thou shalt select a gameobject with an attached LSystemEditor Component!",
                                "Sure thing!");
                return;
            }

            string path = EditorUtility.OpenFilePanel(
            "Load L-System settings JSON",
            "Packages/com.pg649.creaturegenerator",
            "json");// absolute path

            if (path.Length != 0)
            {
                StreamReader reader = new(path);
                string json = reader.ReadToEnd();
                reader.Close();

                LSystemProperties prop = JsonUtility.FromJson<LSystemProperties>(json);
                skeleton.ApplyLSystemSettings(prop);
                Debug.Log("L-System settings successfully imported");
            }
            // now also marks the changed object as dirty to actually save the settings
            EditorUtility.SetDirty(go);
        }

        // Disable the menu item if no selection is in place or editor is not in play mode.
        [MenuItem("PG649/Import LSystem Settings", true)]
        private static bool ValidateImportLSystem()
        {
            return Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<LSystemEditor>() != null && !EditorUtility.IsPersistent(Selection.activeGameObject);
        }
    }
}