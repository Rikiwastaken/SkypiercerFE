#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static DataScript;
using static UnitScript;

public class StatCalculator : EditorWindow
{

    private DataScript DS;
    private UnitScript US;

    // Class dropdown
    private List<string> PlayableCharactersStats = new List<string>();
    private List<string> NPCClasses = new List<string>();

    // Weapon type dropdown
    private List<string> WeaponClasses = new List<string>()
    {
        "Sword",
        "Spear",
        "Greatsword",
        "Bow",
        "Scythe",
        "Shield",
        "Staff",
        "Dagger"
    };

    [Serializable]
    public class CurrentStatCalculation
    {
        public bool UseNPC;
        public int ClassID;
        public int WeaponID;
        public BaseStats BaseStats;
        public StatGrowth StatGrowth;
        public bool usingGenius;
        public bool IsProtag;
        public int currentlevel;
        public int TargetLevel;
    }


    public CurrentStatCalculation currentStatCalculation;

    private SerializedProperty Prop;

    SerializedObject SerializedObject;

    private int BaseWidth = 100;
    private int StatWidth = 70;

    private BaseStats FinalStats;
    private string FinalInfo;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    void OnEnable()
    {
        SerializedObject = new SerializedObject(this);
    }

    // Update is called once per frame
    void Update()
    {

    }

    [MenuItem("Tools/Stat Calculator")]
    public static void ShowWindow()
    {




        var window = CreateWindow<StatCalculator>("Stat Calculator");
        window.minSize = new Vector2(500, 600);

    }

    private void OnGUI()
    {


        if (DS == null || US == null)
        {
            FindDatascriptAndUnitScript();
            FilloutClasses();
            Prop = SerializedObject.FindProperty("currentStatCalculation");
        }

        SerializedObject.Update();



        EditorGUILayout.PropertyField(Prop.FindPropertyRelative("UseNPC"));
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.PropertyField(Prop.FindPropertyRelative("usingGenius"));

        EditorGUILayout.PropertyField(Prop.FindPropertyRelative("IsProtag"));
        EditorGUILayout.EndHorizontal();

        SerializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();



        List<string> nametodisplay = new List<string>();
        List<int> IDs = new List<int>();

        if (currentStatCalculation.UseNPC)
        {
            nametodisplay = NPCClasses;
            EditorGUILayout.LabelField("Currently using NPC Classes");
            currentStatCalculation.currentlevel = 1;
        }
        else
        {
            nametodisplay = PlayableCharactersStats;
            EditorGUILayout.LabelField("Currently using Playable Character Classes");

        }


        EditorGUILayout.BeginHorizontal();
        DrawIDListDropdown(Prop.FindPropertyRelative("ClassID"), nametodisplay);



        SerializedObject.ApplyModifiedProperties();

        if (!currentStatCalculation.UseNPC)
        {
            currentStatCalculation.currentlevel = DS.PlayableCharacterList[currentStatCalculation.ClassID].level;
        }


        EditorGUILayout.LabelField("Current Level: " + currentStatCalculation.currentlevel);
        EditorGUILayout.PropertyField(Prop.FindPropertyRelative("TargetLevel"));

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Weapon Class Used
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Weapon Class Used");
        DrawIDListDropdown(Prop.FindPropertyRelative("WeaponID"), WeaponClasses);

        EditorGUILayout.Space();

        GetBaseStatAndGrowth();

        // Growth and stat Table

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical();


        StatGrowth bonusgrowth = CalculateBonusGrowth();

        EditorGUILayout.LabelField("Stat", GUILayout.Width(StatWidth));
        EditorGUILayout.LabelField("HP", GUILayout.Width(StatWidth));
        EditorGUILayout.LabelField("Str", GUILayout.Width(StatWidth));
        EditorGUILayout.LabelField("Psyche", GUILayout.Width(StatWidth));
        EditorGUILayout.LabelField("Defense", GUILayout.Width(StatWidth));
        EditorGUILayout.LabelField("Resistance", GUILayout.Width(StatWidth));
        EditorGUILayout.LabelField("Speed", GUILayout.Width(StatWidth));
        EditorGUILayout.LabelField("Dexterity", GUILayout.Width(StatWidth));
        EditorGUILayout.LabelField("Luck", GUILayout.Width(StatWidth));
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Stats", GUILayout.Width(BaseWidth));
        EditorGUILayout.LabelField(currentStatCalculation.BaseStats.HP.ToString(), GUILayout.Width(BaseWidth));
        EditorGUILayout.LabelField(currentStatCalculation.BaseStats.Strength.ToString(), GUILayout.Width(BaseWidth));
        EditorGUILayout.LabelField(currentStatCalculation.BaseStats.Psyche.ToString(), GUILayout.Width(BaseWidth));
        EditorGUILayout.LabelField(currentStatCalculation.BaseStats.Defense.ToString(), GUILayout.Width(BaseWidth));
        EditorGUILayout.LabelField(currentStatCalculation.BaseStats.Resistance.ToString(), GUILayout.Width(BaseWidth));
        EditorGUILayout.LabelField(currentStatCalculation.BaseStats.Speed.ToString(), GUILayout.Width(BaseWidth));
        EditorGUILayout.LabelField(currentStatCalculation.BaseStats.Dexterity.ToString(), GUILayout.Width(BaseWidth));
        EditorGUILayout.LabelField(currentStatCalculation.BaseStats.Luck.ToString(), GUILayout.Width(BaseWidth));

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Growth", GUILayout.Width(BaseWidth));
        EditorGUILayout.LabelField(generateGrowthstring(currentStatCalculation.StatGrowth.HPGrowth, bonusgrowth.HPGrowth), GUILayout.Width(BaseWidth));
        EditorGUILayout.LabelField(generateGrowthstring(currentStatCalculation.StatGrowth.StrengthGrowth, bonusgrowth.StrengthGrowth), GUILayout.Width(BaseWidth));
        EditorGUILayout.LabelField(generateGrowthstring(currentStatCalculation.StatGrowth.PsycheGrowth, bonusgrowth.PsycheGrowth), GUILayout.Width(BaseWidth));
        EditorGUILayout.LabelField(generateGrowthstring(currentStatCalculation.StatGrowth.DefenseGrowth, bonusgrowth.DefenseGrowth), GUILayout.Width(BaseWidth));
        EditorGUILayout.LabelField(generateGrowthstring(currentStatCalculation.StatGrowth.ResistanceGrowth, bonusgrowth.ResistanceGrowth), GUILayout.Width(BaseWidth));
        EditorGUILayout.LabelField(generateGrowthstring(currentStatCalculation.StatGrowth.SpeedGrowth, bonusgrowth.SpeedGrowth), GUILayout.Width(BaseWidth));
        EditorGUILayout.LabelField(generateGrowthstring(currentStatCalculation.StatGrowth.DexterityGrowth, bonusgrowth.DexterityGrowth), GUILayout.Width(BaseWidth));
        EditorGUILayout.LabelField(generateGrowthstring(currentStatCalculation.StatGrowth.LuckGrowth, bonusgrowth.LuckGrowth), GUILayout.Width(BaseWidth));

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();


        SerializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Calculate Level Up"))
        {
            LevelupButton();
        }

        SerializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();



        if (FinalStats != null)
        {
            EditorGUILayout.LabelField(FinalInfo);

            // Show the final stats
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("Stat", GUILayout.Width(StatWidth));
            EditorGUILayout.LabelField("HP", GUILayout.Width(StatWidth));
            EditorGUILayout.LabelField("Str", GUILayout.Width(StatWidth));
            EditorGUILayout.LabelField("Psyche", GUILayout.Width(StatWidth));
            EditorGUILayout.LabelField("Defense", GUILayout.Width(StatWidth));
            EditorGUILayout.LabelField("Resistance", GUILayout.Width(StatWidth));
            EditorGUILayout.LabelField("Speed", GUILayout.Width(StatWidth));
            EditorGUILayout.LabelField("Dexterity", GUILayout.Width(StatWidth));
            EditorGUILayout.LabelField("Luck", GUILayout.Width(StatWidth));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Stats (Rounded)", GUILayout.Width(BaseWidth));
            EditorGUILayout.LabelField(((int)FinalStats.HP).ToString(), GUILayout.Width(BaseWidth));
            EditorGUILayout.LabelField(((int)FinalStats.Strength).ToString(), GUILayout.Width(BaseWidth));
            EditorGUILayout.LabelField(((int)FinalStats.Psyche).ToString(), GUILayout.Width(BaseWidth));
            EditorGUILayout.LabelField(((int)FinalStats.Defense).ToString(), GUILayout.Width(BaseWidth));
            EditorGUILayout.LabelField(((int)FinalStats.Resistance).ToString(), GUILayout.Width(BaseWidth));
            EditorGUILayout.LabelField(((int)FinalStats.Speed).ToString(), GUILayout.Width(BaseWidth));
            EditorGUILayout.LabelField(((int)FinalStats.Dexterity).ToString(), GUILayout.Width(BaseWidth));
            EditorGUILayout.LabelField(((int)FinalStats.Luck).ToString(), GUILayout.Width(BaseWidth));

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Stats (Raw)", GUILayout.Width(BaseWidth));
            EditorGUILayout.LabelField(FinalStats.HP.ToString(), GUILayout.Width(BaseWidth));
            EditorGUILayout.LabelField(FinalStats.Strength.ToString(), GUILayout.Width(BaseWidth));
            EditorGUILayout.LabelField(FinalStats.Psyche.ToString(), GUILayout.Width(BaseWidth));
            EditorGUILayout.LabelField(FinalStats.Defense.ToString(), GUILayout.Width(BaseWidth));
            EditorGUILayout.LabelField(FinalStats.Resistance.ToString(), GUILayout.Width(BaseWidth));
            EditorGUILayout.LabelField(FinalStats.Speed.ToString(), GUILayout.Width(BaseWidth));
            EditorGUILayout.LabelField(FinalStats.Dexterity.ToString(), GUILayout.Width(BaseWidth));
            EditorGUILayout.LabelField(FinalStats.Luck.ToString(), GUILayout.Width(BaseWidth));


            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            SerializedObject.ApplyModifiedProperties();
        }
    }

    private void LevelupButton()
    {
        int numberoflevelups = currentStatCalculation.TargetLevel - currentStatCalculation.currentlevel;
        if (numberoflevelups > 0)
        {

            //Calculate the final Stats

            BaseStats StartStats = new BaseStats()
            {
                HP = currentStatCalculation.BaseStats.HP,
                Strength = currentStatCalculation.BaseStats.Strength,
                Psyche = currentStatCalculation.BaseStats.Psyche,
                Defense = currentStatCalculation.BaseStats.Defense,
                Resistance = currentStatCalculation.BaseStats.Resistance,
                Speed = currentStatCalculation.BaseStats.Speed,
                Dexterity = currentStatCalculation.BaseStats.Dexterity,
                Luck = currentStatCalculation.BaseStats.Luck
            };

            FinalStats = CalculateLevelUps(numberoflevelups, StartStats, currentStatCalculation);
            string name = "";
            if (currentStatCalculation.UseNPC)
            {
                name = NPCClasses[currentStatCalculation.ClassID];
            }
            else
            {
                name = PlayableCharactersStats[currentStatCalculation.ClassID];

            }
            FinalInfo = "Result for " + name + " at level " + currentStatCalculation.TargetLevel + " (" + numberoflevelups + " Level Ups)";

        }
    }

    private void FilloutClasses()
    {
        NPCClasses = new List<string>();
        foreach (ClassInfo classInfo in DS.ClassList)
        {
            NPCClasses.Add(classInfo.name);
        }

        PlayableCharactersStats = new List<string>();
        foreach (Character character in DS.PlayableCharacterList)
        {
            PlayableCharactersStats.Add(character.name);
        }
    }

    private void GetBaseStatAndGrowth()
    {
        BaseStats basestats = new BaseStats();
        StatGrowth Growth = new StatGrowth();
        if (currentStatCalculation.UseNPC)
        {
            basestats = DS.ClassList[currentStatCalculation.ClassID].BaseStats;
            Growth = DS.ClassList[currentStatCalculation.ClassID].StatGrowth;
        }
        else
        {
            basestats = DS.PlayableCharacterList[currentStatCalculation.ClassID].stats;
            Growth = DS.PlayableCharacterList[currentStatCalculation.ClassID].growth;
        }
        currentStatCalculation.BaseStats = basestats;
        currentStatCalculation.StatGrowth = Growth;


    }

    private BaseStats CalculateLevelUps(int remainingLevelUps, BaseStats stats, CurrentStatCalculation currentStatCalculation)
    {
        if (remainingLevelUps <= 0)
        {
            return stats;
        }
        else
        {

            float growthboostByLuck = US.growthPerLuckPoint;

            int luckboost = (int)((int)stats.Luck * growthboostByLuck);

            if (currentStatCalculation.IsProtag)
            {
                luckboost += US.cystalheartgrowthboost;
            }

            if (currentStatCalculation.usingGenius)
            {
                luckboost += US.geniusgrowthboost;
            }

            StatGrowth BladeBonusGrowth = US.CalculateGrowthBonusByBlade(WeaponClasses[currentStatCalculation.WeaponID]);



            StatGrowth truegrowth = new StatGrowth();
            truegrowth.HPGrowth = currentStatCalculation.StatGrowth.HPGrowth + luckboost + BladeBonusGrowth.HPGrowth;
            truegrowth.StrengthGrowth = currentStatCalculation.StatGrowth.StrengthGrowth + luckboost + BladeBonusGrowth.StrengthGrowth;
            truegrowth.PsycheGrowth = currentStatCalculation.StatGrowth.PsycheGrowth + luckboost + BladeBonusGrowth.PsycheGrowth;
            truegrowth.DefenseGrowth = currentStatCalculation.StatGrowth.DefenseGrowth + luckboost + BladeBonusGrowth.DefenseGrowth;
            truegrowth.ResistanceGrowth = currentStatCalculation.StatGrowth.ResistanceGrowth + luckboost + BladeBonusGrowth.ResistanceGrowth;
            truegrowth.SpeedGrowth = currentStatCalculation.StatGrowth.SpeedGrowth + luckboost + BladeBonusGrowth.SpeedGrowth;
            truegrowth.DexterityGrowth = currentStatCalculation.StatGrowth.DexterityGrowth + luckboost + BladeBonusGrowth.DexterityGrowth;
            truegrowth.LuckGrowth = currentStatCalculation.StatGrowth.LuckGrowth + luckboost + BladeBonusGrowth.LuckGrowth;

            stats.HP += truegrowth.HPGrowth / 100f;
            stats.Strength += truegrowth.StrengthGrowth / 100f;
            stats.Psyche += truegrowth.PsycheGrowth / 100f;
            stats.Defense += truegrowth.DefenseGrowth / 100f;
            stats.Resistance += truegrowth.ResistanceGrowth / 100f;
            stats.Speed += truegrowth.SpeedGrowth / 100f;
            stats.Dexterity += truegrowth.DexterityGrowth / 100f;
            stats.Luck += truegrowth.LuckGrowth / 100f;



            currentStatCalculation.currentlevel += 1;


            return CalculateLevelUps(remainingLevelUps - 1, stats, currentStatCalculation);
        }


    }

    private StatGrowth CalculateBonusGrowth()
    {
        StatGrowth BonusGrowth = US.CalculateGrowthBonusByBlade(WeaponClasses[currentStatCalculation.WeaponID]);

        int globalbonus = 0;

        if (currentStatCalculation.IsProtag)
        {
            globalbonus += US.cystalheartgrowthboost;
        }

        if (currentStatCalculation.usingGenius)
        {
            globalbonus += US.geniusgrowthboost;
        }

        BonusGrowth.HPGrowth += globalbonus;
        BonusGrowth.StrengthGrowth += globalbonus;
        BonusGrowth.PsycheGrowth += globalbonus;
        BonusGrowth.DefenseGrowth += globalbonus;
        BonusGrowth.ResistanceGrowth += globalbonus;
        BonusGrowth.SpeedGrowth += globalbonus;
        BonusGrowth.DexterityGrowth += globalbonus;
        BonusGrowth.LuckGrowth += globalbonus;

        return BonusGrowth;
    }

    private string generateGrowthstring(float basegrowth, float bonusgrowth)
    {
        string growthstring = ((basegrowth + bonusgrowth) + "");
        if (bonusgrowth > 0)
        {
            string modstring = " (" + basegrowth + " + " + bonusgrowth + ")";
            growthstring += modstring;
        }
        else if (bonusgrowth < 0)
        {
            string modstring = " (" + basegrowth + " - " + (int)Mathf.Abs(bonusgrowth) + ")";
            growthstring += modstring;
        }
        return growthstring;
    }

    private void DrawIDListDropdown(SerializedProperty listProp, List<string> displayNames, List<int> IDs)
    {
        if (listProp == null)
        {
            Debug.LogError("lisrProp is null");
            return;
        }
        if (IDs.Count == 0)
        {
            Debug.LogError("IDs is null");
            return;
        }


        EditorGUILayout.BeginHorizontal();
        SerializedProperty element = listProp;

        int currentIndex = Mathf.Max(0, IDs.IndexOf(element.intValue));
        // Scrollable popup
        int selectedIndex = EditorGUILayout.Popup(currentIndex, displayNames.ToArray());
        if (selectedIndex >= 0 && selectedIndex < IDs.Count)
            element.intValue = IDs[selectedIndex];

        EditorGUILayout.EndHorizontal();



    }

    private void DrawIDListDropdown(SerializedProperty listProp, List<string> displayNames)
    {



        if (listProp == null)
        {
            Debug.LogError("lisrProp is null");
            return;
        }
        List<int> IDs = new List<int>();
        for (int i = 0; i < displayNames.Count; i++)
        {
            IDs.Add(i);
        }

        DrawIDListDropdown(listProp, displayNames, IDs);

    }

    private void FindDatascriptAndUnitScript()
    {
        string[] scriptGUIDs = UnityEditor.AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });
        foreach (string guid in scriptGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject GO = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (GO.GetComponent<DataScript>())
            {
                DS = GO.GetComponent<DataScript>();
            }

            if (GO.GetComponent<UnitScript>() != null)
            {
                US = GO.GetComponent<UnitScript>();
            }

        }
        if (DS != null)
        {
            Debug.Log("DataScript found");
        }
        else
        {
            Debug.LogError("Datascript not found");
        }

        if (US != null)
        {
            Debug.Log("UnitScript found");
        }
        else
        {
            Debug.LogError("UnitScript not found");
        }

    }

}
#endif