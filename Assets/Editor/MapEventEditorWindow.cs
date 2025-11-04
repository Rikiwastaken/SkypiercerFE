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

    // Equipment dropdown
    private List<string> characternames = new List<string>();
    private List<int> characterIDs = new List<int>();

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
        "9 : TileList Mechanisms are all activated"
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
        "6 : Unused"
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
    }

    private void OnHierarchyChange()
    {
        RefreshTarget();
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
            // Load prefab from project if not in scene
            string[] guids = AssetDatabase.FindAssets("DataObject");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                dataScriptPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<DataScript>();
            }
        }

        if (dataScriptPrefab.PlayableCharacterList != null)
        {
            foreach (var e in dataScriptPrefab.PlayableCharacterList)
            {
                characternames.Add(e.name);
                characterIDs.Add(e.ID);
            }
        }

    }

    private void OnGUI()
    {
        EditorGUILayout.Space();

        if (GUILayout.Button("Refresh target MapEventManager"))
        {
            targetManager = null;
            RefreshTarget();
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

        SerializedProperty tileMod = eProp.FindPropertyRelative("tileModification");
        if (tileMod != null)
        {
            tileMod.FindPropertyRelative("TilesIdCouples").ClearArray();
            tileMod.FindPropertyRelative("tilestomodify").ClearArray();
            tileMod.FindPropertyRelative("newtype").stringValue = "";
            tileMod.FindPropertyRelative("modiftype").intValue = 1;
        }

        SerializedProperty unitPlacement = eProp.FindPropertyRelative("UnitPlacement");
        if (unitPlacement != null)
        {
            unitPlacement.FindPropertyRelative("UnitToPlaceManually").ClearArray();
            unitPlacement.FindPropertyRelative("WhereToManuallyPlaceUnits").ClearArray();
            unitPlacement.FindPropertyRelative("RemainingSpots").ClearArray();
            unitPlacement.FindPropertyRelative("CameraPosition").vector2Value = Vector2.zero;
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

        //--- Tutorial Window ---

        SerializedProperty tutorialWindowProp = eProp.FindPropertyRelative("TutorialWindow");
        if (tutorialWindowProp != null)
        {
           
        }

        // --- Equipments ---
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Units To Unlock:");
        DrawIDListDropdown(eProp.FindPropertyRelative("UnitsToUnlockID"), characternames, characterIDs);
        EditorGUILayout.LabelField("Units To Lock:");
        DrawIDListDropdown(eProp.FindPropertyRelative("UnitsToLockID"), characternames, characterIDs);

        


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
                trigType.intValue = EditorGUILayout.IntPopup("Trigger Type", trigType.intValue, new string[] {"0 : None", "1 : Unit1 deployed", "2 : Unit1 next to Unit2", "3 : Unit1 alive", "4 : Unit1 not deployed", "5 : (unused)", "6 : (unused)" }, new int[] { 1, 2, 3, 4, 5, 6 });
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
}