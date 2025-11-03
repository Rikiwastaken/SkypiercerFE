#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class TipsEditorWindow : EditorWindow
{
    private GameObject sceneGOsPrefab;
    private TipsMenuScript tipsMenu;
    private SerializedObject so;
    private SerializedProperty tipsProp;
    private Vector2 scrollPos;

    [MenuItem("Tools/Tips Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<TipsEditorWindow>("Tips Editor");
        window.minSize = new Vector2(500, 400);
    }

    private void OnEnable()
    {
        RefreshTarget();
    }

    private void RefreshTarget()
    {
        tipsMenu = null;
        so = null;
        tipsProp = null;
        sceneGOsPrefab = null;

        // Search all prefabs in project
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                // Include inactive children
                TipsMenuScript[] tmsArray = prefab.GetComponentsInChildren<TipsMenuScript>(true);
                if (tmsArray.Length > 0)
                {
                    sceneGOsPrefab = prefab;
                    tipsMenu = tmsArray[0]; // take the first one
                    so = new SerializedObject(tipsMenu);
                    tipsProp = so.FindProperty("Tips");
                    break;
                }
            }
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();

        if (GUILayout.Button("Refresh TipsMenuScript Target"))
        {
            RefreshTarget();
        }

        if (tipsMenu == null)
        {
            EditorGUILayout.HelpBox("TipsMenuScript not found in any prefab in the project (including inactive objects).", MessageType.Warning);
            return;
        }

        so.Update();

        EditorGUILayout.LabelField("Editing Tips for prefab: " + sceneGOsPrefab.name, EditorStyles.boldLabel);
        EditorGUILayout.Space();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        for (int i = 0; i < tipsProp.arraySize; i++)
        {
            SerializedProperty tipProp = tipsProp.GetArrayElementAtIndex(i);
            SerializedProperty nameProp = tipProp.FindPropertyRelative("name");
            SerializedProperty descProp = tipProp.FindPropertyRelative("description");
            SerializedProperty chapterProp = tipProp.FindPropertyRelative("chapterWhereUnlocks");

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            nameProp.stringValue = EditorGUILayout.TextField("Tip Name", nameProp.stringValue);
            if (GUILayout.Button("Remove", GUILayout.MaxWidth(80)))
            {
                tipsProp.DeleteArrayElementAtIndex(i);
                break;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Description");
            descProp.stringValue = EditorGUILayout.TextArea(descProp.stringValue, GUILayout.Height(100));

            chapterProp.intValue = EditorGUILayout.IntField("Chapter Where Unlocks", chapterProp.intValue);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        if (GUILayout.Button("Add New Tip"))
        {
            tipsProp.arraySize++;
            SerializedProperty newTip = tipsProp.GetArrayElementAtIndex(tipsProp.arraySize - 1);
            newTip.FindPropertyRelative("name").stringValue = "New Tip";
            newTip.FindPropertyRelative("description").stringValue = "";
            newTip.FindPropertyRelative("chapterWhereUnlocks").intValue = 1;
        }

        EditorGUILayout.EndScrollView();

        so.ApplyModifiedProperties();

        if (GUILayout.Button("Save Changes to Prefab"))
        {
            PrefabUtility.SavePrefabAsset(sceneGOsPrefab);
            EditorUtility.DisplayDialog("Saved", "Tips changes saved to prefab!", "OK");
        }
    }
}
#endif
