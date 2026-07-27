#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UselessScriptFinder : EditorWindow
{
    private string SceneFolder = "Assets/Scenes";

    [MenuItem("Tools/Find Unused Scripts")]
    public static void ShowWindow()
    {
        var window = CreateWindow<UselessScriptFinder>("Unused Script Finder");
        window.minSize = new Vector2(300, 300);
        window.maxSize = new Vector2(300, 300);

    }

    [Serializable]
    public class ScriptInfo
    {
        public string name;
        public string path;
        public System.Type type;
        public List<Scene> sceneswhereused;
    }

    private List<ScriptInfo> FindUnusedScripts()
    {

        Debug.Log("boom launching");
        string[] scriptGUIDs = UnityEditor.AssetDatabase.FindAssets("t:MonoScript", new[] { "Assets/Scripts" });

        List<ScriptInfo> ScriptList = new List<ScriptInfo>();



        // create list of class with script info
        foreach (string guid in scriptGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

            System.Type type = script.GetClass();
            if (type == null)
                continue; // No class or compile error

            ScriptList.Add(new ScriptInfo() { name = type.FullName, type = type, path = path, sceneswhereused = new List<Scene>() });
        }


        // cycle through scenes to find if script are used or not
        string[] sceneGUIDs = UnityEditor.AssetDatabase.FindAssets("t:Scene", new[] { SceneFolder });

        foreach (string guid in sceneGUIDs)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);

            Scene scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path, UnityEditor.SceneManagement.OpenSceneMode.Single);

            foreach (ScriptInfo script in ScriptList)
            {
                System.Type typeToUse = script.type;
                UnityEngine.Object obj = UnityEngine.Object.FindAnyObjectByType(typeToUse, FindObjectsInactive.Include);

                if (obj != null)
                {
                    script.sceneswhereused.Add(scene);
                }
            }
        }

        return ScriptList;

    }



    private void OnGUI()
    {

        EditorGUILayout.LabelField("Changes in the current scene will be lost! Proceed? (It might take a while)");
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Scripts Folder", GUILayout.Width(90));
        SceneFolder = EditorGUILayout.TextField(SceneFolder);
        EditorGUILayout.Space();
        if (GUILayout.Button("Browse", GUILayout.Width(70)))
        {
            string folder = EditorUtility.OpenFolderPanel("Select Scene Folder", Application.dataPath, "");

            if (!string.IsNullOrEmpty(folder))
            {
                // Convert absolute path to project-relative path
                if (folder.StartsWith(Application.dataPath))
                {
                    SceneFolder = "Assets" + folder.Substring(Application.dataPath.Length);
                }
                else
                {
                    EditorUtility.DisplayDialog(
                        "Invalid Folder",
                        "Please select a folder inside this Unity project.",
                        "OK");
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();


        if (GUILayout.Button("Find Useless Scripts"))
        {
            List<ScriptInfo> scriptinfo = FindUnusedScripts();
            foreach (ScriptInfo script in scriptinfo)
            {
                if (script.sceneswhereused == null || script.sceneswhereused.Count == 0)
                {
                    Debug.Log("Script " + script.name + " is never used anywhere. It has the following path:\n" + script.path);
                }
            }
        }


    }
}
#endif
