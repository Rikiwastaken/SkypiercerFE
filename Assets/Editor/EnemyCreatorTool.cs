using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EnemyStatsEditorWindow : EditorWindow
{
    private MapInitializer targetInitializer;
    private DataScript dataScriptPrefab;
    private SerializedObject so;
    private SerializedProperty enemyListProp;
    private Vector2 scrollPos;

    // Class dropdown
    private List<string> classNames = new List<string>();
    private List<int> classIDs = new List<int>();

    // Skill dropdown
    private List<string> skillNames = new List<string>();
    private List<int> skillIDs = new List<int>();

    // Equipment dropdown
    private List<string> equipmentNames = new List<string>();
    private List<int> equipmentIDs = new List<int>();



    private static readonly string[] PersonalityOptions = new string[]
    {
        "nothing : basic",
        "Deviant : High Random",
        "Coward : deviant if below 33% hp, survivor if below 10%",
        "Daredevil : never takes into account their own HP",
        "Survivor : Always avoid enemies and attacks",
        "Guard : Does not move",
        "Hunter : Looks for enemies regardless of HP and distance"
    };

    [MenuItem("Tools/Enemy Stats Editor")]
    public static void ShowWindow()
    {
        var w = GetWindow<EnemyStatsEditorWindow>("Enemy Stats Editor");
        w.minSize = new Vector2(650, 500);
    }

    private void OnEnable()
    {
        RefreshTarget();
        RefreshDataLists();
    }

    private void OnHierarchyChange()
    {
        RefreshTarget();
        RefreshDataLists();
        Repaint();
    }

    private void RefreshTarget()
    {
        if (targetInitializer == null)
            targetInitializer = FindAnyObjectByType<MapInitializer>();

        if (targetInitializer != null)
        {
            so = new SerializedObject(targetInitializer);
            enemyListProp = so.FindProperty("EnemyList");
        }
        else
        {
            so = null;
            enemyListProp = null;
        }

        if (dataScriptPrefab == null)
        {
            // Load prefab from project if not in scene
            string[] guids = AssetDatabase.FindAssets("DataObject");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                dataScriptPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<DataScript>();
            }
        }
    }

    private void RefreshDataLists()
    {
        classNames.Clear();
        classIDs.Clear();
        skillNames.Clear();
        skillIDs.Clear();
        equipmentNames.Clear();
        equipmentIDs.Clear();

        if (dataScriptPrefab != null)
        {
            // Classes
            if (dataScriptPrefab.ClassList != null)
            {
                foreach (var c in dataScriptPrefab.ClassList)
                {
                    classNames.Add($"{c.ID} : {c.name}");
                    classIDs.Add(c.ID);
                }
            }

            // Skills
            if (dataScriptPrefab.SkillList != null)
            {
                foreach (var s in dataScriptPrefab.SkillList)
                {
                    string label = s.ID +" : "+s.name + (s.IsCommand ? " (command)" : " (passive)");
                    skillNames.Add(label);
                    skillIDs.Add(s.ID);
                }
            }

            // Equipments
            if (dataScriptPrefab.equipmentList != null)
            {
                foreach (var e in dataScriptPrefab.equipmentList)
                {
                    string label = e.ID +$" : {e.type} {e.Grade}";
                    equipmentNames.Add(label);
                    equipmentIDs.Add(e.ID);
                }
            }
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();

        // Allow manual prefab assignment
        dataScriptPrefab = (DataScript)EditorGUILayout.ObjectField("DataScript Prefab", dataScriptPrefab, typeof(DataScript), false);
        if (GUILayout.Button("Refresh Lists"))
            RefreshDataLists();

        EditorGUILayout.Space();

        if (targetInitializer == null)
        {
            EditorGUILayout.HelpBox("No MapInitializer found in the scene.", MessageType.Warning);
            if (GUILayout.Button("Create new GameObject with MapInitializer"))
            {
                var go = new GameObject("MapInitializer");
                go.AddComponent<MapInitializer>();
                RefreshTarget();
            }
            return;
        }

        if (so == null || enemyListProp == null)
            RefreshTarget();

        so.Update();

        EditorGUILayout.LabelField("Editing: " + targetInitializer.gameObject.name, EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add New Enemy"))
        {
            enemyListProp.arraySize++;
            so.ApplyModifiedProperties();
            SerializedProperty newEnemy = enemyListProp.GetArrayElementAtIndex(enemyListProp.arraySize - 1);
            InitializeNewEnemy(newEnemy);
        }

        if (enemyListProp.arraySize > 0)
        {
            if (GUILayout.Button("Clear All Enemies"))
            {
                if (EditorUtility.DisplayDialog("Clear all enemies?", "Are you sure you want to remove all enemies from this MapInitializer?", "Yes", "No"))
                {
                    enemyListProp.ClearArray();
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        for (int i = 0; i < enemyListProp.arraySize; i++)
        {
            SerializedProperty eProp = enemyListProp.GetArrayElementAtIndex(i);
            DrawEnemyProperty(eProp, i);
            EditorGUILayout.Space();
            EditorGUILayout.Separator();
        }

        EditorGUILayout.EndScrollView();

        so.ApplyModifiedProperties();
        HideCharacter();
        ShowCharacter();
    }

    private void OnDisable()
    {
        HideCharacter();
    }

    private void InitializeNewEnemy(SerializedProperty eProp)
    {
        eProp.FindPropertyRelative("Name").stringValue = "New Enemy";
        eProp.FindPropertyRelative("classID").intValue = classIDs.Count > 0 ? classIDs[0] : 0;
        eProp.FindPropertyRelative("desiredlevel").intValue = 1;
        eProp.FindPropertyRelative("itemtodropID").intValue = 0;
        eProp.FindPropertyRelative("usetelekinesis").boolValue = false;
        eProp.FindPropertyRelative("personality").stringValue = "nothing";
        eProp.FindPropertyRelative("startpos").vector2Value = Vector2.zero;
        eProp.FindPropertyRelative("isboss").boolValue = false;
        eProp.FindPropertyRelative("isother").boolValue = false;
        eProp.FindPropertyRelative("talkable").boolValue = false;
        eProp.FindPropertyRelative("RemainingLifebars").intValue = 0;
        eProp.FindPropertyRelative("modelID").intValue = 0;

        eProp.FindPropertyRelative("equipments").ClearArray();
        eProp.FindPropertyRelative("Skills").ClearArray();

        SerializedProperty monsterStats = eProp.FindPropertyRelative("monsterStats");
        if (monsterStats != null)
        {
            monsterStats.FindPropertyRelative("size").intValue = 0;
            monsterStats.FindPropertyRelative("ispluvial").boolValue = false;
            monsterStats.FindPropertyRelative("ismachine").boolValue = false;
        }

        so.ApplyModifiedProperties();
    }

    private void DrawEnemyProperty(SerializedProperty eProp, int index)
    {
        eProp.isExpanded = EditorGUILayout.Foldout(eProp.isExpanded, $"[{index}] {eProp.FindPropertyRelative("Name").stringValue}", true);
        if (!eProp.isExpanded) return;

        EditorGUI.indentLevel++;
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(eProp.FindPropertyRelative("Name"));
        if (GUILayout.Button("Remove", GUILayout.MaxWidth(80)))
        {
            enemyListProp.DeleteArrayElementAtIndex(index);
            so.ApplyModifiedProperties();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
            return;
        }
        EditorGUILayout.EndHorizontal();

        // Class dropdown
        SerializedProperty classProp = eProp.FindPropertyRelative("classID");
        if (classIDs.Count > 0)
        {
            int currentIndex = Mathf.Max(0, classIDs.IndexOf(classProp.intValue));
            int selectedIndex = EditorGUILayout.Popup("Class", currentIndex, classNames.ToArray());
            classProp.intValue = classIDs[selectedIndex];
        }

        EditorGUILayout.PropertyField(eProp.FindPropertyRelative("desiredlevel"));
        EditorGUILayout.PropertyField(eProp.FindPropertyRelative("itemtodropID"));
        EditorGUILayout.PropertyField(eProp.FindPropertyRelative("usetelekinesis"));

        // Personality dropdown
        var personalityProp = eProp.FindPropertyRelative("personality");
        int indexChoice = Mathf.Max(0, System.Array.FindIndex(PersonalityOptions, s => s.StartsWith(personalityProp.stringValue)));
        indexChoice = EditorGUILayout.Popup("Personality", indexChoice, PersonalityOptions);
        personalityProp.stringValue = PersonalityOptions[indexChoice].Split(':')[0].Trim();

        EditorGUILayout.PropertyField(eProp.FindPropertyRelative("startpos"));

        // --- Equipments ---
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Equipments");
        DrawIDListDropdown(eProp.FindPropertyRelative("equipments"), equipmentNames, equipmentIDs);

        // --- Skills ---
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Skills");
        DrawIDListDropdown(eProp.FindPropertyRelative("Skills"), skillNames, skillIDs);

        EditorGUILayout.PropertyField(eProp.FindPropertyRelative("isboss"));
        EditorGUILayout.PropertyField(eProp.FindPropertyRelative("isother"));
        EditorGUILayout.PropertyField(eProp.FindPropertyRelative("talkable"));

        EditorGUILayout.LabelField("Monster Stats", EditorStyles.boldLabel);
        SerializedProperty monsterStats = eProp.FindPropertyRelative("monsterStats");
        if (monsterStats != null)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(monsterStats.FindPropertyRelative("size"));
            EditorGUILayout.PropertyField(monsterStats.FindPropertyRelative("ispluvial"));
            EditorGUILayout.PropertyField(monsterStats.FindPropertyRelative("ismachine"));
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.PropertyField(eProp.FindPropertyRelative("RemainingLifebars"));
        EditorGUILayout.PropertyField(eProp.FindPropertyRelative("modelID"));

        EditorGUILayout.EndVertical();
        EditorGUI.indentLevel--;
    }

    private void DrawIDListDropdown(SerializedProperty listProp, List<string> displayNames, List<int> IDs)
    {
        if (listProp == null || IDs.Count == 0) return;

        int toRemove = -1;

        for (int i = 0; i < listProp.arraySize; i++)
        {
            EditorGUILayout.BeginHorizontal();
            SerializedProperty element = listProp.GetArrayElementAtIndex(i);

            int currentIndex = Mathf.Max(0, IDs.IndexOf(element.intValue));
            // Scrollable popup
            int selectedIndex = EditorGUILayout.Popup(currentIndex, displayNames.ToArray());
            if (selectedIndex >= 0 && selectedIndex < IDs.Count)
                element.intValue = IDs[selectedIndex];

            if (GUILayout.Button("x", GUILayout.MaxWidth(20)))
                toRemove = i;

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add"))
        {
            listProp.arraySize++;
            listProp.GetArrayElementAtIndex(listProp.arraySize - 1).intValue = IDs[0];
        }
        EditorGUILayout.EndHorizontal();

        if (toRemove >= 0)
            listProp.DeleteArrayElementAtIndex(toRemove);
    }


    [MenuItem("Tools/Show Positions")]
    public static void ShowCharacter()
    {
        Transform gridGO = GameObject.Find("Grid").transform;
        MapInitializer mapInitializer = FindAnyObjectByType<MapInitializer>();
        List<Vector2> enemypositions = new List<Vector2>();
        List<Vector2> otherpositions = new List<Vector2>();
        List<Vector2> playablepositions = new List<Vector2>();
        foreach (var character in mapInitializer.EnemyList)
        {
            if(character.isother)
            {
                otherpositions.Add(character.startpos);
            }
            else
            {
                enemypositions.Add(character.startpos);
                if(character.monsterStats.size>1)
                {
                    enemypositions.Add(character.startpos + new Vector2(0,1));
                    enemypositions.Add(character.startpos + new Vector2(-1, 0));
                    enemypositions.Add(character.startpos + new Vector2(-1, 1));
                }
            }
                
        }
        foreach (Vector2 playablepos in mapInitializer.playablepos)
        {
            playablepositions.Add(playablepos);
        }

        for (int i = 0; i < gridGO.childCount; i++)
        {
            Vector2 position = new Vector2(gridGO.GetChild(i).position.x, gridGO.GetChild(i).position.z);
            if(enemypositions.Contains(position))
            {
                gridGO.GetChild(i).GetChild(0).GetComponent<SpriteRenderer>().color = Color.red;
                enemypositions.Remove(position);
            }
            if(playablepositions.Contains(position))
            {
                gridGO.GetChild(i).GetChild(0).GetComponent<SpriteRenderer>().color = Color.blue;
                enemypositions.Remove(position);
            }
            if (otherpositions.Contains(position))
            {
                gridGO.GetChild(i).GetChild(0).GetComponent<SpriteRenderer>().color = Color.yellow;
                enemypositions.Remove(position);
            }
            if(gridGO.GetChild(i).GetComponent<GridSquareScript>().isobstacle)
            {
                gridGO.GetChild(i).GetChild(0).GetComponent<SpriteRenderer>().color = Color.black;
            }
        }

    }

    [MenuItem("Tools/Hide Characters Positions")]
    public static void HideCharacter()
    {
        if(GameObject.Find("Grid"))
        {
            Transform gridGO = GameObject.Find("Grid").transform;
            for (int i = 0; i < gridGO.childCount; i++)
            {
                Color newcolor = Color.white;
                newcolor.a = 0f;
                gridGO.GetChild(i).GetChild(0).GetComponent<SpriteRenderer>().color = newcolor;
            }
        }
    }
}
