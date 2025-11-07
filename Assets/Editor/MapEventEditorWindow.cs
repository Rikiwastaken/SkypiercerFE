// Place this file in Assets/Editor/MapEventEditorWindow.cs
// An EditorWindow to visually create & edit MapEventManager.EventCondition entries at runtime
// Works by finding the active MapEventManager in the scene and exposing its serialized fields.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MapEventEditorWindow : EditorWindow
{
    private MapEventManager targetManager;
    private SerializedObject so;
    private SerializedProperty eventsProp;
    private Vector2 scrollPos;
    private DataScript dataScriptPrefab;

    // Character / data dropdown
    private List<string> characternames = new List<string>();
    private List<int> characterIDs = new List<int>();

    // Additional data lists (for EnemyStats editing)
    private List<string> classNames = new List<string>();
    private List<int> classIDs = new List<int>();

    private List<string> skillNames = new List<string>();
    private List<int> skillIDs = new List<int>();

    private List<string> equipmentNames = new List<string>();
    private List<int> equipmentIDs = new List<int>();

    private static readonly string[] TriggerTypeLabels = new string[]
    {
        "0 : None",
        "1 : Ally reaches tile",
        "2 : Enemy reaches tile",
        "3 : Other reaches tile",
        "4 : One of units died",
        "5 : All units died",
        "6 : Smaller conditions met",
        "7 : Battle starts",
        "8 : Watched events triggered",
        "9 : TileList Mechanisms are all activated",
        "10 : Beginning of turns listed"
    };

    private static readonly string[] InitializationTypeLabels = new string[]
    {
        "0 : None",
        "1 : Get Units From Names",
        "2 : Get Playable Units",
        "3 : Get Enemy Units",
        "4 : Get Other Units",
        "5 : (unused)",
        "6 : (unused)"
    };

    private static readonly string[] TriggerEffectTypeLabels = new string[]
    {
        "0 : None",
        "1 : Win the game",
        "2 : Lose the game",
        "3 : Modify Tiles",
        "4 : Show Dialogue",
        "5 : Show Tutorial Window",
        "6 : Spawn Units"
    };

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

    [MenuItem("Tools/Map Event Editor")]
    public static void ShowWindow()
    {
        var w = GetWindow<MapEventEditorWindow>("Map Event Editor");
        w.minSize = new Vector2(600, 300);
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
        if (targetManager == null)
        {
            targetManager = FindAnyObjectByType<MapEventManager>();
        }

        if (targetManager != null)
        {
            so = new SerializedObject(targetManager);
            eventsProp = so.FindProperty("EventsToMonitor");
        }
        else
        {
            so = null;
            eventsProp = null;
        }

        if (dataScriptPrefab == null)
        {
            // Load prefab from project if not in scene (searching "DataObject")
            string[] guids = AssetDatabase.FindAssets("DataObject");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go != null)
                {
                    dataScriptPrefab = go.GetComponent<DataScript>();
                }
            }
        }

        // Fill playable character lists if DataScript has them
        characternames.Clear();
        characterIDs.Clear();
        if (dataScriptPrefab != null && dataScriptPrefab.PlayableCharacterList != null)
        {
            foreach (var e in dataScriptPrefab.PlayableCharacterList)
            {
                characternames.Add(e.name);
                characterIDs.Add(e.ID);
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
                    string label = s.ID + " : " + s.name + (s.IsCommand ? " (command)" : " (passive)");
                    skillNames.Add(label);
                    skillIDs.Add(s.ID);
                }
            }

            // Equipments
            if (dataScriptPrefab.equipmentList != null)
            {
                foreach (var e in dataScriptPrefab.equipmentList)
                {
                    string label = e.ID + $" : {e.type} {e.Grade}";
                    equipmentNames.Add(label);
                    equipmentIDs.Add(e.ID);
                }
            }

            // Also refresh playable characters names/ids again (if DataScript assigned)
            characternames.Clear();
            characterIDs.Clear();
            if (dataScriptPrefab.PlayableCharacterList != null)
            {
                foreach (var e in dataScriptPrefab.PlayableCharacterList)
                {
                    characternames.Add(e.name);
                    characterIDs.Add(e.ID);
                }
            }
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();

        // Allow manual DataScript assignment
        dataScriptPrefab = (DataScript)EditorGUILayout.ObjectField("DataScript Prefab", dataScriptPrefab, typeof(DataScript), false);
        if (GUILayout.Button("Refresh Lists"))
            RefreshDataLists();

        EditorGUILayout.Space();

        if (GUILayout.Button("Refresh target MapEventManager"))
        {
            targetManager = null;
            RefreshTarget();
            RefreshDataLists();
        }

        EditorGUILayout.Space();

        if (targetManager == null)
        {
            EditorGUILayout.HelpBox("No MapEventManager found in the scene. Add the MapEventManager script to a GameObject and try again.", MessageType.Warning);
            if (GUILayout.Button("Create new GameObject with MapEventManager"))
            {
                var go = new GameObject("MapEventManager");
                go.AddComponent<MapEventManager>();
                RefreshTarget();
            }
            return;
        }

        if (so == null || eventsProp == null)
        {
            RefreshTarget();
            RefreshDataLists();
        }

        so.Update();

        EditorGUILayout.LabelField("Editing: " + targetManager.gameObject.name, EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add New Event"))
        {
            eventsProp.arraySize++;
            so.ApplyModifiedProperties();
            SerializedProperty newEvent = eventsProp.GetArrayElementAtIndex(eventsProp.arraySize - 1);
            InitializeNewEvent(newEvent);
        }

        if (eventsProp.arraySize > 0)
        {
            if (GUILayout.Button("Clear All Events"))
            {
                if (EditorUtility.DisplayDialog("Clear all events?", "Are you sure you want to remove all events from this MapEventManager? This cannot be undone.", "Yes", "No"))
                {
                    eventsProp.ClearArray();
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        for (int i = 0; i < eventsProp.arraySize; i++)
        {
            SerializedProperty eProp = eventsProp.GetArrayElementAtIndex(i);
            DrawEventProperty(eProp, i);
            EditorGUILayout.Space();
            EditorGUILayout.Separator();
        }

        EditorGUILayout.EndScrollView();

        so.ApplyModifiedProperties();
    }

    private void InitializeNewEvent(SerializedProperty eProp)
    {
        eProp.FindPropertyRelative("eventname").stringValue = "New Event";
        eProp.FindPropertyRelative("ID").intValue = GenerateUniqueEventID();
        eProp.FindPropertyRelative("UnitList").ClearArray();
        eProp.FindPropertyRelative("NameUnitList").ClearArray();
        eProp.FindPropertyRelative("EventsToWatch").ClearArray();
        eProp.FindPropertyRelative("TilesList").ClearArray();
        eProp.FindPropertyRelative("SmallerConditions").ClearArray();
        eProp.FindPropertyRelative("triggertype").intValue = 1;
        eProp.FindPropertyRelative("initializationtype").intValue = 1;
        eProp.FindPropertyRelative("triggered").boolValue = false;
        eProp.FindPropertyRelative("triggerEffectType").intValue = 4;
        eProp.FindPropertyRelative("dialoguetoShow").ClearArray();
        eProp.FindPropertyRelative("UnitsToUnlockID").ClearArray();
        eProp.FindPropertyRelative("UnitsToLockID").ClearArray();
        eProp.FindPropertyRelative("turnswheretotrigger").ClearArray();

        // Initialize CharactersToSpawn (EnemyStats list)
        SerializedProperty charsToSpawnProp = eProp.FindPropertyRelative("CharactersToSpawn");
        if (charsToSpawnProp != null)
            charsToSpawnProp.ClearArray();

        SerializedProperty tileMod = eProp.FindPropertyRelative("tileModification");
        if (tileMod != null)
        {
            if (tileMod.FindPropertyRelative("TilesIdCouples") != null) tileMod.FindPropertyRelative("TilesIdCouples").ClearArray();
            if (tileMod.FindPropertyRelative("tilestomodify") != null) tileMod.FindPropertyRelative("tilestomodify").ClearArray();
            if (tileMod.FindPropertyRelative("newtype") != null) tileMod.FindPropertyRelative("newtype").stringValue = "";
            if (tileMod.FindPropertyRelative("modiftype") != null) tileMod.FindPropertyRelative("modiftype").intValue = 1;
        }

        SerializedProperty unitPlacement = eProp.FindPropertyRelative("UnitPlacement");
        if (unitPlacement != null)
        {
            if (unitPlacement.FindPropertyRelative("UnitToPlaceManually") != null) unitPlacement.FindPropertyRelative("UnitToPlaceManually").ClearArray();
            if (unitPlacement.FindPropertyRelative("WhereToManuallyPlaceUnits") != null) unitPlacement.FindPropertyRelative("WhereToManuallyPlaceUnits").ClearArray();
            if (unitPlacement.FindPropertyRelative("RemainingSpots") != null) unitPlacement.FindPropertyRelative("RemainingSpots").ClearArray();
            if (unitPlacement.FindPropertyRelative("CameraPosition") != null) unitPlacement.FindPropertyRelative("CameraPosition").vector2Value = Vector2.zero;
        }

        so.ApplyModifiedProperties();
    }

    private int GenerateUniqueEventID()
    {
        int max = 0;
        for (int i = 0; i < eventsProp.arraySize; i++)
        {
            var idProp = eventsProp.GetArrayElementAtIndex(i).FindPropertyRelative("ID");
            if (idProp != null)
            {
                if (idProp.intValue > max) max = idProp.intValue;
            }
        }
        return max + 1;
    }

    private void DrawEventProperty(SerializedProperty eProp, int index)
    {
        eProp.isExpanded = EditorGUILayout.Foldout(eProp.isExpanded, string.Format("[{0}] {1}", index, eProp.FindPropertyRelative("eventname").stringValue), true);
        if (!eProp.isExpanded) return;

        EditorGUI.indentLevel++;
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(eProp.FindPropertyRelative("eventname"));
        EditorGUILayout.PropertyField(eProp.FindPropertyRelative("ID"), GUIContent.none, GUILayout.MaxWidth(120));

        if (GUILayout.Button("Duplicate", GUILayout.MaxWidth(80)))
        {
            DuplicateEvent(eProp);
        }

        if (GUILayout.Button("Remove", GUILayout.MaxWidth(80)))
        {
            eventsProp.DeleteArrayElementAtIndex(index);
            so.ApplyModifiedProperties();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
            return;
        }
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.LabelField("Trigger settings", EditorStyles.boldLabel);

        // Dropdowns instead of int fields
        var trigTypeProp = eProp.FindPropertyRelative("triggertype");
        trigTypeProp.intValue = EditorGUILayout.Popup("Trigger Type", trigTypeProp.intValue, TriggerTypeLabels);

        var initTypeProp = eProp.FindPropertyRelative("initializationtype");
        initTypeProp.intValue = EditorGUILayout.Popup("Initialization Type", initTypeProp.intValue, InitializationTypeLabels);

        var effectTypeProp = eProp.FindPropertyRelative("triggerEffectType");
        effectTypeProp.intValue = EditorGUILayout.Popup("Trigger Effect", effectTypeProp.intValue, TriggerEffectTypeLabels);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Unit names (NameUnitList)");
        DrawStringList(eProp.FindPropertyRelative("NameUnitList"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Events to watch (EventsToWatch)");
        DrawIntList(eProp.FindPropertyRelative("EventsToWatch"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Tiles list (TilesList)");
        DrawObjectList(eProp.FindPropertyRelative("TilesList"), typeof(GridSquareScript));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Smaller Conditions");
        DrawSmallerConditionsList(eProp.FindPropertyRelative("SmallerConditions"));

        EditorGUILayout.PropertyField(eProp.FindPropertyRelative("dialoguetoShow"), true);
        EditorGUILayout.PropertyField(eProp.FindPropertyRelative("tileModification"), true);
        EditorGUILayout.PropertyField(eProp.FindPropertyRelative("UnitPlacement"), true);

        // --- CharactersToSpawn (EnemyStats) ---
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Characters to Spawn (EnemyStats)", EditorStyles.boldLabel);
        DrawCharactersToSpawnList(eProp.FindPropertyRelative("CharactersToSpawn"));

        //--- Tutorial Window ---
        SerializedProperty tutorialWindowProp = eProp.FindPropertyRelative("TutorialWindow");
        if (tutorialWindowProp != null)
        {
            EditorGUILayout.LabelField("Tutorial Window", EditorStyles.boldLabel);

            // Draw the WindowDimensions normally
            EditorGUILayout.PropertyField(tutorialWindowProp.FindPropertyRelative("WindowDimensions"));

            // Draw a large multiline text area for the 'text' field
            SerializedProperty textProp = tutorialWindowProp.FindPropertyRelative("text");
            if (textProp != null)
            {
                // Set a minimum height for the text area
                textProp.stringValue = EditorGUILayout.TextArea(textProp.stringValue, GUILayout.Height(150));
            }
        }

        // --- Equipments ---
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Units To Unlock:");
        DrawIDListDropdown(eProp.FindPropertyRelative("UnitsToUnlockID"), characternames, characterIDs);
        EditorGUILayout.LabelField("Units To Lock:");
        DrawIDListDropdown(eProp.FindPropertyRelative("UnitsToLockID"), characternames, characterIDs);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Turns when event triggers (turnswheretotrigger)");
        DrawIntList(eProp.FindPropertyRelative("turnswheretotrigger"));

        EditorGUILayout.EndVertical();
        EditorGUI.indentLevel--;
    }

    private void DrawStringList(SerializedProperty listProp)
    {
        if (listProp == null) return;
        int toRemove = -1;
        for (int i = 0; i < listProp.arraySize; i++)
        {
            EditorGUILayout.BeginHorizontal();
            SerializedProperty element = listProp.GetArrayElementAtIndex(i);
            element.stringValue = EditorGUILayout.TextField(element.stringValue);
            if (GUILayout.Button("x", GUILayout.MaxWidth(20))) toRemove = i;
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add"))
        {
            listProp.arraySize++;
            listProp.GetArrayElementAtIndex(listProp.arraySize - 1).stringValue = "";
        }
        EditorGUILayout.EndHorizontal();
        if (toRemove >= 0)
        {
            listProp.DeleteArrayElementAtIndex(toRemove);
        }
    }

    private void DrawIntList(SerializedProperty listProp)
    {
        if (listProp == null) return;
        int toRemove = -1;
        for (int i = 0; i < listProp.arraySize; i++)
        {
            EditorGUILayout.BeginHorizontal();
            SerializedProperty element = listProp.GetArrayElementAtIndex(i);
            element.intValue = EditorGUILayout.IntField(element.intValue);
            if (GUILayout.Button("x", GUILayout.MaxWidth(20))) toRemove = i;
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add"))
        {
            listProp.arraySize++;
            listProp.GetArrayElementAtIndex(listProp.arraySize - 1).intValue = 0;
        }
        EditorGUILayout.EndHorizontal();
        if (toRemove >= 0)
        {
            listProp.DeleteArrayElementAtIndex(toRemove);
        }
    }

    private void DrawObjectList(SerializedProperty listProp, System.Type objectType)
    {
        if (listProp == null) return;
        int toRemove = -1;
        for (int i = 0; i < listProp.arraySize; i++)
        {
            EditorGUILayout.BeginHorizontal();
            SerializedProperty element = listProp.GetArrayElementAtIndex(i);
            EditorGUILayout.PropertyField(element, GUIContent.none);
            if (GUILayout.Button("x", GUILayout.MaxWidth(20))) toRemove = i;
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add"))
        {
            listProp.arraySize++;
        }
        EditorGUILayout.EndHorizontal();
        if (toRemove >= 0)
        {
            listProp.DeleteArrayElementAtIndex(toRemove);
        }
    }

    private void DrawSmallerConditionsList(SerializedProperty listProp)
    {
        if (listProp == null) return;
        int toRemove = -1;
        for (int i = 0; i < listProp.arraySize; i++)
        {
            SerializedProperty element = listProp.GetArrayElementAtIndex(i);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            element.isExpanded = EditorGUILayout.Foldout(element.isExpanded, "Condition " + i, true);
            if (GUILayout.Button("x", GUILayout.MaxWidth(20))) toRemove = i;
            EditorGUILayout.EndHorizontal();
            if (element.isExpanded)
            {
                EditorGUILayout.PropertyField(element.FindPropertyRelative("Unit1"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("Unit2"));
                var trigType = element.FindPropertyRelative("triggertype");
                trigType.intValue = EditorGUILayout.IntPopup("Trigger Type", trigType.intValue, new string[] { "0 : None", "1 : Unit1 deployed", "2 : Unit1 next to Unit2", "3 : Unit1 alive", "4 : Unit1 not deployed", "5 : (unused)", "6 : (unused)" }, new int[] { 1, 2, 3, 4, 5, 6 });
            }
            EditorGUILayout.EndVertical();
        }
        if (GUILayout.Button("Add Condition"))
        {
            listProp.arraySize++;
            var newElem = listProp.GetArrayElementAtIndex(listProp.arraySize - 1);
            newElem.FindPropertyRelative("Unit1").stringValue = "";
            newElem.FindPropertyRelative("Unit2").stringValue = "";
            newElem.FindPropertyRelative("triggertype").intValue = 1;
        }
        if (toRemove >= 0)
        {
            listProp.DeleteArrayElementAtIndex(toRemove);
        }
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

    // ---------------------
    // CharactersToSpawn section (EnemyStats editor inside the Event editor)
    // ---------------------
    private void DrawCharactersToSpawnList(SerializedProperty listProp)
    {
        if (listProp == null) return;

        int toRemove = -1;

        for (int i = 0; i < listProp.arraySize; i++)
        {
            SerializedProperty eProp = listProp.GetArrayElementAtIndex(i);
            eProp.isExpanded = EditorGUILayout.Foldout(eProp.isExpanded, $"[{i}] {eProp.FindPropertyRelative("Name").stringValue}", true);

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Character #{i}", EditorStyles.boldLabel);
            if (GUILayout.Button("x", GUILayout.MaxWidth(20)))
            {
                toRemove = i;
            }
            EditorGUILayout.EndHorizontal();

            if (eProp.isExpanded)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(eProp.FindPropertyRelative("Name"));
                if (GUILayout.Button("Remove", GUILayout.MaxWidth(80)))
                {
                    toRemove = i;
                }
                EditorGUILayout.EndHorizontal();

                // Class dropdown (if available)
                SerializedProperty classProp = eProp.FindPropertyRelative("classID");
                if (classIDs.Count > 0 && classProp != null)
                {
                    int currentIndex = Mathf.Max(0, classIDs.IndexOf(classProp.intValue));
                    int selectedIndex = EditorGUILayout.Popup("Class", currentIndex, classNames.ToArray());
                    classProp.intValue = classIDs[Mathf.Clamp(selectedIndex, 0, classIDs.Count - 1)];
                }
                else if (classProp != null)
                {
                    EditorGUILayout.PropertyField(classProp);
                }

                EditorGUILayout.PropertyField(eProp.FindPropertyRelative("desiredlevel"));
                EditorGUILayout.PropertyField(eProp.FindPropertyRelative("itemtodropID"));
                EditorGUILayout.PropertyField(eProp.FindPropertyRelative("usetelekinesis"));

                // Personality
                var personalityProp = eProp.FindPropertyRelative("personality");
                if (personalityProp != null)
                {
                    int indexChoice = Mathf.Max(0, System.Array.FindIndex(PersonalityOptions, s => s.StartsWith(personalityProp.stringValue)));
                    indexChoice = EditorGUILayout.Popup("Personality", indexChoice, PersonalityOptions);
                    personalityProp.stringValue = PersonalityOptions[indexChoice].Split(':')[0].Trim();
                }

                EditorGUILayout.PropertyField(eProp.FindPropertyRelative("startpos"));

                // Equipments list (IDs)
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Equipments");
                DrawIDListDropdown(eProp.FindPropertyRelative("equipments"), equipmentNames, equipmentIDs);

                // Skills list (IDs)
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Skills");
                DrawIDListDropdown(eProp.FindPropertyRelative("Skills"), skillNames, skillIDs);

                EditorGUILayout.PropertyField(eProp.FindPropertyRelative("isboss"));
                EditorGUILayout.PropertyField(eProp.FindPropertyRelative("isother"));

                // MonsterStats block (if present)
                SerializedProperty monsterStats = eProp.FindPropertyRelative("monsterStats");
                if (monsterStats != null)
                {
                    EditorGUILayout.LabelField("Monster Stats", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    if (monsterStats.FindPropertyRelative("size") != null) EditorGUILayout.PropertyField(monsterStats.FindPropertyRelative("size"));
                    if (monsterStats.FindPropertyRelative("ispluvial") != null) EditorGUILayout.PropertyField(monsterStats.FindPropertyRelative("ispluvial"));
                    if (monsterStats.FindPropertyRelative("ismachine") != null) EditorGUILayout.PropertyField(monsterStats.FindPropertyRelative("ismachine"));
                    EditorGUI.indentLevel--;
                }

                if (eProp.FindPropertyRelative("RemainingLifebars") != null) EditorGUILayout.PropertyField(eProp.FindPropertyRelative("RemainingLifebars"));
                if (eProp.FindPropertyRelative("modelID") != null) EditorGUILayout.PropertyField(eProp.FindPropertyRelative("modelID"));
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Character To Spawn"))
        {
            listProp.arraySize++;
            var newElem = listProp.GetArrayElementAtIndex(listProp.arraySize - 1);
            InitializeNewSpawnCharacter(newElem);
        }
        EditorGUILayout.EndHorizontal();

        if (toRemove >= 0)
        {
            listProp.DeleteArrayElementAtIndex(toRemove);
        }
    }

    private void InitializeNewSpawnCharacter(SerializedProperty eProp)
    {
        if (eProp == null) return;

        if (eProp.FindPropertyRelative("Name") != null) eProp.FindPropertyRelative("Name").stringValue = "New Enemy";
        if (eProp.FindPropertyRelative("classID") != null) eProp.FindPropertyRelative("classID").intValue = classIDs.Count > 0 ? classIDs[0] : 0;
        if (eProp.FindPropertyRelative("desiredlevel") != null) eProp.FindPropertyRelative("desiredlevel").intValue = 1;
        if (eProp.FindPropertyRelative("itemtodropID") != null) eProp.FindPropertyRelative("itemtodropID").intValue = 0;
        if (eProp.FindPropertyRelative("usetelekinesis") != null) eProp.FindPropertyRelative("usetelekinesis").boolValue = false;
        if (eProp.FindPropertyRelative("personality") != null) eProp.FindPropertyRelative("personality").stringValue = "nothing";
        if (eProp.FindPropertyRelative("startpos") != null) eProp.FindPropertyRelative("startpos").vector2Value = Vector2.zero;
        if (eProp.FindPropertyRelative("isboss") != null) eProp.FindPropertyRelative("isboss").boolValue = false;
        if (eProp.FindPropertyRelative("isother") != null) eProp.FindPropertyRelative("isother").boolValue = false;
        if (eProp.FindPropertyRelative("RemainingLifebars") != null) eProp.FindPropertyRelative("RemainingLifebars").intValue = 0;
        if (eProp.FindPropertyRelative("modelID") != null) eProp.FindPropertyRelative("modelID").intValue = 0;

        if (eProp.FindPropertyRelative("equipments") != null) eProp.FindPropertyRelative("equipments").ClearArray();
        if (eProp.FindPropertyRelative("Skills") != null) eProp.FindPropertyRelative("Skills").ClearArray();

        SerializedProperty monsterStats = eProp.FindPropertyRelative("monsterStats");
        if (monsterStats != null)
        {
            if (monsterStats.FindPropertyRelative("size") != null) monsterStats.FindPropertyRelative("size").intValue = 0;
            if (monsterStats.FindPropertyRelative("ispluvial") != null) monsterStats.FindPropertyRelative("ispluvial").boolValue = false;
            if (monsterStats.FindPropertyRelative("ismachine") != null) monsterStats.FindPropertyRelative("ismachine").boolValue = false;
        }

        so.ApplyModifiedProperties();
    }
    private void DuplicateEvent(SerializedProperty eventProp)
    {
        if (eventProp == null || eventsProp == null) return;

        // Serialize the event to JSON and back to create a deep copy
        string json = JsonUtility.ToJson(eventProp.serializedObject.targetObject, true);

        // Create a temporary GameObject to hold the new event
        MapEventManager.EventCondition tempEvent = JsonUtility.FromJson<MapEventManager.EventCondition>(json);

        // Add a new element to the events list
        eventsProp.arraySize++;
        SerializedProperty newEventProp = eventsProp.GetArrayElementAtIndex(eventsProp.arraySize - 1);

        // Copy all fields from the tempEvent into the new SerializedProperty
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(tempEvent), newEventProp.serializedObject.targetObject);

        // Assign a new unique ID to avoid conflicts
        var idProp = newEventProp.FindPropertyRelative("ID");
        if (idProp != null) idProp.intValue = GenerateUniqueEventID();

        so.ApplyModifiedProperties();
    }

}
