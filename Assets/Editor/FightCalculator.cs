#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static DataScript;
using static UnitScript;

public class FightCalculator : EditorWindow
{

    private DataScript DS;
    private UnitScript US;
    private UnitScript USForEnemy;
    private ActionsMenu AM;

    // Class dropdown
    private List<string> PlayableCharactersStats = new List<string>();

    // Skill List
    private List<string> SkillNames = new List<string>();

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

    // Classes
    private List<int> BaseEnemyClasses = new List<int>() { 0, 1, 2, 3, 5, 6, 7, 8, 12, 13, 15, 18, 24 };
    private List<int> AdvancedEnemyClasses = new List<int>() { 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37 };
    private List<int> ClassesThatForceTelekinesis = new List<int>() { 1, 6, 27, 31 };
    private List<int> ClassesThatMayHaveTelekinesis = new List<int>() { 13, 35 };

    [Serializable]
    public class CurrentStatCalculation
    {
        public int ClassID;
        public int WeaponID;
        public BaseStats BaseStats;
        public StatGrowth StatGrowth;
        public int currentlevel;
        public int TargetLevel;
        public int BattlesToSimulate;
        public int WeaponLevel = 1;
        public int Skill1;
        public int Skill2;
        public int Skill3;
        public int Skill4;
        public bool UseTelekinesis;
        public int StartWithSpecificNumberOfHP;
    }


    public CurrentStatCalculation currentStatCalculation;

    private Character CurrentChar;

    private SerializedProperty Prop;

    SerializedObject SerializedObject;

    private int BaseWidth = 100;
    private int StatWidth = 70;

    private bool CharacterLoaded;

    private bool CalculationsLaunched;

    private bool CalculationsLaunchedZeroSimulations;

    [Serializable]
    public class BattleSimulationClass
    {
        public int EnemyClassID;
        public int EnemyWeaponID;
        public bool usingtelekinesis;
        public BaseStats BaseStats;
        public StatGrowth StatGrowth;
        public int currentlevel;
        public int EquipedWeaponID;
        public List<int> EquipedSkills;
        public int result; // -1 = defeat, 0 = draw, 1 = victory
        public bool enemyusingtelekinesis;
        public float numberOfTurns;
        public int numberOfPlayerHitTaken;
        public int NumberOfPlayerHits;
        public int NumberOfPlayerCrits;
    }

    private List<BattleSimulationClass> Results;


    void OnEnable()
    {
        SerializedObject = new SerializedObject(this);
    }


    [MenuItem("Tools/Fight Calculator")]
    public static void ShowWindow()
    {
        var window = CreateWindow<FightCalculator>("Fight Calculator");
        window.minSize = new Vector2(500, 600);

    }

    private void OnGUI()
    {


        if (DS == null || US == null || AM == null || USForEnemy == null)
        {
            FindDatascriptAndUnitScript();
            FilloutClassesAndSkills();
            Prop = SerializedObject.FindProperty("currentStatCalculation");
        }

        SerializedObject.Update();

        SerializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();



        List<string> nametodisplay = new List<string>();
        List<int> IDs = new List<int>();

        nametodisplay = PlayableCharactersStats;


        EditorGUILayout.BeginHorizontal();
        DrawIDListDropdown(Prop.FindPropertyRelative("ClassID"), nametodisplay);



        SerializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Load New Character"))
        {
            CharacterLoaded = true;

            CurrentChar = US.CreateCopy(DS.PlayableCharacterList[currentStatCalculation.ClassID]);
            CurrentChar.SecondSkillUnlocked = true;
            currentStatCalculation.currentlevel = DS.PlayableCharacterList[currentStatCalculation.ClassID].level;
            US.UnitCharacteristics = CurrentChar;


        }


        EditorGUILayout.LabelField("Current Level: " + currentStatCalculation.currentlevel);
        EditorGUILayout.PropertyField(Prop.FindPropertyRelative("TargetLevel"));

        EditorGUILayout.PropertyField(Prop.FindPropertyRelative("WeaponLevel"));
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.Space();

        //Skills
        EditorGUILayout.BeginHorizontal();
        DrawIDListDropdown(Prop.FindPropertyRelative("Skill1"), SkillNames);
        DrawIDListDropdown(Prop.FindPropertyRelative("Skill2"), SkillNames);
        DrawIDListDropdown(Prop.FindPropertyRelative("Skill3"), SkillNames);
        DrawIDListDropdown(Prop.FindPropertyRelative("Skill4"), SkillNames);
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.Space();

        //Telekinesis
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(Prop.FindPropertyRelative("UseTelekinesis"));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(Prop.FindPropertyRelative("StartWithSpecificNumberOfHP"));

        EditorGUILayout.EndHorizontal();


        EditorGUILayout.Space();

        // Weapon Class Used
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Weapon Class Used");
        DrawIDListDropdown(Prop.FindPropertyRelative("WeaponID"), WeaponClasses);

        EditorGUILayout.Space();

        GetBaseStatAndGrowth();

        SerializedObject.ApplyModifiedProperties();

        if (currentStatCalculation.WeaponLevel > 4)
        {
            currentStatCalculation.WeaponLevel = 4;
        }
        else if (currentStatCalculation.WeaponLevel < 1)
        {
            currentStatCalculation.WeaponLevel = 1;
        }

        if (CharacterLoaded)
        {
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
            EditorGUILayout.LabelField(CurrentChar.stats.HP.ToString(), GUILayout.Width(BaseWidth));
            EditorGUILayout.LabelField(CurrentChar.stats.Strength.ToString(), GUILayout.Width(BaseWidth));
            EditorGUILayout.LabelField(CurrentChar.stats.Psyche.ToString(), GUILayout.Width(BaseWidth));
            EditorGUILayout.LabelField(CurrentChar.stats.Defense.ToString(), GUILayout.Width(BaseWidth));
            EditorGUILayout.LabelField(CurrentChar.stats.Resistance.ToString(), GUILayout.Width(BaseWidth));
            EditorGUILayout.LabelField(CurrentChar.stats.Speed.ToString(), GUILayout.Width(BaseWidth));
            EditorGUILayout.LabelField(CurrentChar.stats.Dexterity.ToString(), GUILayout.Width(BaseWidth));
            EditorGUILayout.LabelField(CurrentChar.stats.Luck.ToString(), GUILayout.Width(BaseWidth));

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Growth", GUILayout.Width(BaseWidth));
            EditorGUILayout.LabelField(generateGrowthstring(CurrentChar.growth.HPGrowth, bonusgrowth.HPGrowth), GUILayout.Width(BaseWidth));
            EditorGUILayout.LabelField(generateGrowthstring(CurrentChar.growth.StrengthGrowth, bonusgrowth.StrengthGrowth), GUILayout.Width(BaseWidth));
            EditorGUILayout.LabelField(generateGrowthstring(CurrentChar.growth.PsycheGrowth, bonusgrowth.PsycheGrowth), GUILayout.Width(BaseWidth));
            EditorGUILayout.LabelField(generateGrowthstring(CurrentChar.growth.DefenseGrowth, bonusgrowth.DefenseGrowth), GUILayout.Width(BaseWidth));
            EditorGUILayout.LabelField(generateGrowthstring(CurrentChar.growth.ResistanceGrowth, bonusgrowth.ResistanceGrowth), GUILayout.Width(BaseWidth));
            EditorGUILayout.LabelField(generateGrowthstring(CurrentChar.growth.SpeedGrowth, bonusgrowth.SpeedGrowth), GUILayout.Width(BaseWidth));
            EditorGUILayout.LabelField(generateGrowthstring(CurrentChar.growth.DexterityGrowth, bonusgrowth.DexterityGrowth), GUILayout.Width(BaseWidth));
            EditorGUILayout.LabelField(generateGrowthstring(CurrentChar.growth.LuckGrowth, bonusgrowth.LuckGrowth), GUILayout.Width(BaseWidth));

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();


            SerializedObject.ApplyModifiedProperties();

            EditorGUILayout.BeginHorizontal();


            EditorGUILayout.PropertyField(Prop.FindPropertyRelative("BattlesToSimulate"));
            SerializedObject.ApplyModifiedProperties();

            CurrentChar.EquipedSkills = new List<int>() { currentStatCalculation.Skill1, currentStatCalculation.Skill2, currentStatCalculation.Skill3, currentStatCalculation.Skill4 };

            EditorGUILayout.Space();
            if (GUILayout.Button("Simulate Battles"))
            {
                LevelupButton();
                int weaponID = 0;

                foreach (equipment weapon in DS.equipmentList)
                {
                    if (weapon.type.ToLower() == WeaponClasses[currentStatCalculation.WeaponID].ToLower() && weapon.Grade == currentStatCalculation.WeaponLevel)
                    {
                        weaponID = weapon.ID;
                        break;
                    }
                }

                CurrentChar.equipmentsIDs = new List<int>() { weaponID };
                CurrentChar.equipments = new List<equipment>() { DS.GenerateEquipementCopy(DS.equipmentList[weaponID], CurrentChar) };
                if (currentStatCalculation.BattlesToSimulate <= 0)
                {
                    CalculationsLaunched = false;
                    CalculationsLaunchedZeroSimulations = true;
                }
                else
                {
                    CalculationsLaunched = true;
                    CalculationsLaunchedZeroSimulations = false;
                    LaunchSimulations();
                }

            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical();
            if (CalculationsLaunchedZeroSimulations)
            {
                EditorGUILayout.LabelField("Calculate more than 0 battles please");
            }
            if (CalculationsLaunched)
            {


                int numberofvictories = 0;
                int numberofvictoriesWithTelek = 0;
                int numberofdefeats = 0;
                int numberofdefeatsWithTelek = 0;
                int numberofDraws = 0;
                int numberofDrawsWithTelek = 0;
                int totalMatchesAgainstTelek = 0;
                float averagematchlength = 0;
                float averagehitsgiven = 0;
                float averagehitstaken = 0;
                float averagecrits = 0;
                int VictoriesAfter1Turn = 0;
                int DefeatsAfter1Turn = 0;
                List<int> victoryPerClass = new List<int>();
                foreach (ClassInfo classinfo in DS.ClassList)
                {
                    victoryPerClass.Add(0);
                }
                List<int> victoryPerSkill = new List<int>();
                foreach (Skill skill in DS.SkillList)
                {
                    victoryPerSkill.Add(0);
                }
                foreach (BattleSimulationClass simulation in Results)
                {
                    if (simulation.result > 0)
                    {
                        numberofvictories++;
                        if (simulation.enemyusingtelekinesis)
                        {
                            numberofvictoriesWithTelek++;
                            totalMatchesAgainstTelek++;
                        }
                        if (simulation.numberOfTurns == 1)
                        {
                            VictoriesAfter1Turn++;
                        }

                    }
                    else if (simulation.result < 0)
                    {
                        numberofdefeats++;
                        if (simulation.enemyusingtelekinesis)
                        {
                            numberofdefeatsWithTelek++;
                            totalMatchesAgainstTelek++;
                        }
                        victoryPerClass[simulation.EnemyClassID]++;
                        foreach (int skillID in simulation.EquipedSkills)
                        {
                            if (skillID != 0)
                            {
                                victoryPerSkill[skillID]++;
                            }
                        }
                        if (simulation.numberOfTurns == 1)
                        {
                            DefeatsAfter1Turn++;
                        }
                    }
                    else
                    {
                        numberofDraws++;
                        if (simulation.enemyusingtelekinesis)
                        {
                            numberofDrawsWithTelek++;
                            totalMatchesAgainstTelek++;
                        }
                    }
                    averagematchlength += simulation.numberOfTurns;
                    averagehitsgiven += simulation.NumberOfPlayerHits;
                    averagehitstaken += simulation.numberOfPlayerHitTaken;
                    averagecrits += simulation.NumberOfPlayerCrits;
                }
                averagematchlength /= (float)Results.Count;
                averagehitsgiven /= (float)Results.Count;
                averagehitstaken /= (float)Results.Count;
                averagecrits /= (float)Results.Count;


                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Result after " + Results.Count + " simulations (" + totalMatchesAgainstTelek + " against telekinesis)");
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Victories: " + numberofvictories + " (" + numberofvictoriesWithTelek + " against telekinesis) (" + VictoriesAfter1Turn + " Victories in 1 Turn)");
                EditorGUILayout.LabelField("Defeats: " + numberofdefeats + " (" + numberofdefeatsWithTelek + " against telekinesis) (" + DefeatsAfter1Turn + " defeats in 1 Turn)");
                EditorGUILayout.LabelField("Draws: " + numberofDraws + " (" + numberofDraws + " against telekinesis)");
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Average Match Length: " + averagematchlength + " turns.");
                EditorGUILayout.LabelField("Average Hits Per Match: " + averagehitsgiven);
                EditorGUILayout.LabelField("Average Hits Taken Per Match: " + averagehitstaken);
                EditorGUILayout.LabelField("Average Crits Per Match: " + averagecrits);
                EditorGUILayout.EndVertical();


                EditorGUILayout.BeginVertical();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(("Win Rate: " + (int)(((float)numberofvictories / (float)Results.Count) * 100f)) + "%.");
                EditorGUILayout.LabelField(("Win Rate (Physical only): " + (int)(((float)(numberofvictories - numberofvictoriesWithTelek) / (float)(Results.Count - totalMatchesAgainstTelek)) * 100f)) + "%.");
                EditorGUILayout.LabelField(("Win Rate (Telekinesis only): " + (int)(((float)(numberofvictoriesWithTelek) / (float)(totalMatchesAgainstTelek)) * 100f)) + "%.");
                EditorGUILayout.Space();

                //best skills
                (List<int> firstbestskill, List<int> secondbestskill, List<int> thirdbestskill) = GetThreeBestIDsFromList(victoryPerSkill);
                EditorGUILayout.LabelField("Best Enemy Skills: " + firstbestskill[0] + " " + DS.SkillList[firstbestskill[0]].name + " (" + firstbestskill[1] + "), " + secondbestskill[0] + " " + DS.SkillList[secondbestskill[0]].name + " (" + secondbestskill[1] + "), " + thirdbestskill[0] + " " + DS.SkillList[thirdbestskill[0]].name + " (" + thirdbestskill[1] + ")");
                (List<int> firstbestclass, List<int> secondbestclass, List<int> thirdbestclass) = GetThreeBestIDsFromList(victoryPerClass);
                EditorGUILayout.LabelField("Best Enemy Classes: " + firstbestclass[0] + " " + DS.ClassList[firstbestclass[0]].name + " (" + firstbestclass[1] + "), " + secondbestclass[0] + " " + DS.ClassList[secondbestclass[0]].name + " (" + secondbestclass[1] + "), " + thirdbestclass[0] + " " + DS.ClassList[thirdbestclass[0]].name + " (" + thirdbestclass[1] + ")");


                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();

            SerializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

        }





    }

    private (List<int>, List<int>, List<int>) GetThreeBestIDsFromList(List<int> list)
    {
        List<int> best = new List<int> { -1, int.MinValue };
        List<int> second = new List<int> { -1, int.MinValue };
        List<int> third = new List<int> { -1, int.MinValue };

        for (int id = 0; id < list.Count; id++)
        {
            int value = list[id];

            if (value > best[1])
            {
                third = second;
                second = best;
                best = new List<int> { id, value };
            }
            else if (value > second[1])
            {
                third = second;
                second = new List<int> { id, value };
            }
            else if (value > third[1])
            {
                third = new List<int> { id, value };
            }
        }

        return (best, second, third);
    }

    private void LaunchSimulations()
    {
        List<int> EnemyClassestouse = BaseEnemyClasses;
        if (currentStatCalculation.currentlevel >= 20)
        {
            EnemyClassestouse = AdvancedEnemyClasses;
        }

        Results = new List<BattleSimulationClass>();
        for (int i = 0; i < currentStatCalculation.BattlesToSimulate; i++)
        {
            // first we reset the weapon of the unit
            int weaponID = 0;

            foreach (equipment weapon in DS.equipmentList)
            {
                if (weapon.type.ToLower() == WeaponClasses[currentStatCalculation.WeaponID].ToLower() && weapon.Grade == currentStatCalculation.WeaponLevel)
                {
                    weaponID = weapon.ID;
                    break;
                }
            }
            CurrentChar.equipmentsIDs = new List<int>() { weaponID };
            CurrentChar.equipments = new List<equipment>() { DS.GenerateEquipementCopy(DS.equipmentList[weaponID], CurrentChar) };
            CurrentChar.telekinesisactivated = currentStatCalculation.UseTelekinesis;


            // then we create the enemy
            BattleSimulationClass simulationClass = new BattleSimulationClass();
            Results.Add(simulationClass);
            simulationClass.EnemyClassID = EnemyClassestouse[UnityEngine.Random.Range(0, EnemyClassestouse.Count)];
            simulationClass.EnemyWeaponID = UnityEngine.Random.Range(0, WeaponClasses.Count);
            simulationClass.currentlevel = currentStatCalculation.currentlevel;
            Character newChar = new Character();

            USForEnemy.UnitCharacteristics = newChar;
            newChar.stats = new BaseStats();
            newChar.stats.Strength = DS.ClassList[simulationClass.EnemyClassID].BaseStats.Strength;
            newChar.stats.Psyche = DS.ClassList[simulationClass.EnemyClassID].BaseStats.Psyche;
            newChar.stats.Defense = DS.ClassList[simulationClass.EnemyClassID].BaseStats.Defense;
            newChar.stats.Resistance = DS.ClassList[simulationClass.EnemyClassID].BaseStats.Resistance;
            newChar.stats.Speed = DS.ClassList[simulationClass.EnemyClassID].BaseStats.Speed;
            newChar.stats.Dexterity = DS.ClassList[simulationClass.EnemyClassID].BaseStats.Dexterity;
            newChar.stats.Luck = DS.ClassList[simulationClass.EnemyClassID].BaseStats.Luck;
            simulationClass.BaseStats = newChar.stats;
            newChar.AjustedStats = new BaseStats();
            newChar.level = 1;

            if (ClassesThatForceTelekinesis.Contains(simulationClass.EnemyClassID))
            {
                newChar.telekinesisactivated = true;
                simulationClass.usingtelekinesis = true;
            }
            else if (ClassesThatMayHaveTelekinesis.Contains(simulationClass.EnemyClassID))
            {
                bool telekinesisactivated = UnityEngine.Random.Range(0, 10) < 5;
                newChar.telekinesisactivated = telekinesisactivated;
                simulationClass.usingtelekinesis = telekinesisactivated;
            }
            else
            {
                newChar.telekinesisactivated = false;
                simulationClass.usingtelekinesis = false;
            }


            newChar.growth = DS.ClassList[simulationClass.EnemyClassID].StatGrowth;

            newChar.name = DS.ClassList[simulationClass.EnemyClassID].name;
            newChar.statusEffects = new StatusEffects();
            newChar.playableStats = new PlayableStats();
            newChar.affiliation = "enemy";
            newChar.enemyStats = new EnemyStats();

            newChar.enemyStats.monsterStats = new MonsterStats();
            newChar.playableStats = new PlayableStats();

            int typevalue = UnityEngine.Random.Range(0, 3);

            newChar.enemyStats.monsterStats.ispluvial = (typevalue == 0);
            newChar.enemyStats.monsterStats.ismachine = (typevalue == 1);

            CalculateLevelUps(currentStatCalculation.currentlevel, newChar, simulationClass.EnemyWeaponID);

            weaponID = 0;

            foreach (equipment weapon in DS.equipmentList)
            {
                if (weapon.type.ToLower() == WeaponClasses[simulationClass.EnemyWeaponID].ToLower() && weapon.Grade == currentStatCalculation.WeaponLevel)
                {
                    weaponID = weapon.ID;
                    break;
                }
            }
            simulationClass.EquipedWeaponID = weaponID;
            newChar.equipmentsIDs = new List<int>() { weaponID };
            newChar.equipments = new List<equipment>() { DS.GenerateEquipementCopy(DS.equipmentList[weaponID], newChar) };
            newChar.EquipedSkills = new List<int>();


            int numberofskills = UnityEngine.Random.Range(0, 5);

            for (int j = 0; j < numberofskills; j++)
            {
                newChar.EquipedSkills.Add(GetRandomSkill());
            }
            simulationClass.EquipedSkills = newChar.EquipedSkills;
            simulationClass.enemyusingtelekinesis = newChar.telekinesisactivated;
            BattleBetweenTwoCharacters(simulationClass);
        }

    }


    private void BattleBetweenTwoCharacters(BattleSimulationClass Simulation)
    {
        US.calculateStats();
        USForEnemy.calculateStats();
        US.unitkilled = 0;
        US.SurvivorStacks = 0;
        US.numberoftimeswaitted = 0;
        US.waittedbonusturns = 0;
        USForEnemy.SurvivorStacks = 0;
        USForEnemy.unitkilled = 0;
        USForEnemy.numberoftimeswaitted = 0;
        USForEnemy.waittedbonusturns = 0;

        if (currentStatCalculation.StartWithSpecificNumberOfHP > 0)
        {
            US.UnitCharacteristics.currentHP = (int)Mathf.Min((int)US.UnitCharacteristics.AjustedStats.HP, currentStatCalculation.StartWithSpecificNumberOfHP);
        }
        else
        {
            US.UnitCharacteristics.currentHP = (int)US.UnitCharacteristics.AjustedStats.HP;
        }

        USForEnemy.UnitCharacteristics.currentHP = (int)USForEnemy.UnitCharacteristics.AjustedStats.HP;

        int NumberOfTurns = 0;
        bool EnemyAttacks = UnityEngine.Random.Range(0, 10) < 5;

        int playableTurnBeginningOfTurnCD = 0;
        int enemyTurnBeginningOfTurnCD = 0;

        while (NumberOfTurns < 50 && US.UnitCharacteristics.currentHP > 0 && USForEnemy.UnitCharacteristics.currentHP > 0)
        {

            if (EnemyAttacks)
            {
                if (enemyTurnBeginningOfTurnCD > 0)
                {
                    enemyTurnBeginningOfTurnCD--;
                }
                else
                {
                    USForEnemy.BeginningofTurnTrigger(new List<GameObject>() { USForEnemy.gameObject });
                    enemyTurnBeginningOfTurnCD = 1;
                }
            }
            else
            {
                if (playableTurnBeginningOfTurnCD > 0)
                {
                    playableTurnBeginningOfTurnCD--;
                }
                else
                {
                    US.BeginningofTurnTrigger(new List<GameObject>() { US.gameObject });
                    playableTurnBeginningOfTurnCD = 1;
                }

            }
            (int numberofhits, int numberofcritials, int finaldamage, int exp, List<int> levelup, List<int> Damagelist, List<int> Critlist, bool allforoneactive, bool unyieldingactivated, bool compassionused, bool invigoratingused) = AM.ApplyDamage(US.gameObject, USForEnemy.gameObject, EnemyAttacks, false);
            if (!EnemyAttacks)
            {
                int numberofcrits = 0;
                foreach (int attack in Critlist)
                {
                    if (attack > 0)
                    {
                        numberofcrits++;
                    }
                }
                Simulation.NumberOfPlayerCrits += numberofcrits;
                foreach (int attack in Damagelist)
                {
                    if (attack > 0)
                    {
                        Simulation.NumberOfPlayerHits++;
                    }
                }
            }
            else
            {
                foreach (int attack in Damagelist)
                {
                    if (attack > 0)
                    {
                        Simulation.numberOfPlayerHitTaken++;
                    }
                }
            }
            EnemyAttacks = !EnemyAttacks;
            NumberOfTurns++;

        }
        Simulation.numberOfTurns = (float)NumberOfTurns / 2f;
        if (US.UnitCharacteristics.currentHP > 0 && USForEnemy.UnitCharacteristics.currentHP < 0)
        {
            Simulation.result = 1;
        }
        else if (USForEnemy.UnitCharacteristics.currentHP > 0 && US.UnitCharacteristics.currentHP < 0)
        {
            Simulation.result = -1;
        }
        else
        {
            Simulation.result = 0;
        }
    }


    private int GetRandomSkill()
    {
        int Randomnumber = UnityEngine.Random.Range(0, DS.SkillList.Count);
        if (DS.SkillList[Randomnumber].buyable)
        {
            return Randomnumber;
        }
        else
        {
            return GetRandomSkill();
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

            CalculateLevelUps(numberoflevelups, CurrentChar, currentStatCalculation.WeaponID);
            currentStatCalculation.currentlevel = CurrentChar.level;
        }
    }

    private void FilloutClassesAndSkills()
    {
        PlayableCharactersStats = new List<string>();
        foreach (Character character in DS.PlayableCharacterList)
        {
            PlayableCharactersStats.Add(character.name);
        }

        SkillNames = new List<string>();
        foreach (Skill skill in DS.SkillList)
        {
            SkillNames.Add(skill.ID + " " + skill.name);
        }
    }

    private void GetBaseStatAndGrowth()
    {
        BaseStats basestats = new BaseStats();
        StatGrowth Growth = new StatGrowth();
        basestats = DS.PlayableCharacterList[currentStatCalculation.ClassID].stats;
        Growth = DS.PlayableCharacterList[currentStatCalculation.ClassID].growth;
        currentStatCalculation.BaseStats = basestats;
        currentStatCalculation.StatGrowth = Growth;


    }

    private Character CalculateLevelUps(int remainingLevelUps, Character character, int weaponID)
    {
        if (remainingLevelUps <= 0)
        {
            return character;
        }
        else
        {

            float growthboostByLuck = US.growthPerLuckPoint;

            int luckboost = (int)((int)character.stats.Luck * growthboostByLuck);

            if (US.GetSkill(57) || US.GetSkill(72) || US.GetSkill(73))
            {
                luckboost += US.cystalheartgrowthboost;
            }

            if (US.GetSkill(10))
            {
                luckboost += US.geniusgrowthboost;
            }
            StatGrowth BladeBonusGrowth = null;
            BladeBonusGrowth = US.CalculateGrowthBonusByBlade(WeaponClasses[weaponID]);




            StatGrowth truegrowth = new StatGrowth();
            truegrowth.HPGrowth = character.growth.HPGrowth + luckboost + BladeBonusGrowth.HPGrowth;
            truegrowth.StrengthGrowth = character.growth.StrengthGrowth + luckboost + BladeBonusGrowth.StrengthGrowth;
            truegrowth.PsycheGrowth = character.growth.PsycheGrowth + luckboost + BladeBonusGrowth.PsycheGrowth;
            truegrowth.DefenseGrowth = character.growth.DefenseGrowth + luckboost + BladeBonusGrowth.DefenseGrowth;
            truegrowth.ResistanceGrowth = character.growth.ResistanceGrowth + luckboost + BladeBonusGrowth.ResistanceGrowth;
            truegrowth.SpeedGrowth = character.growth.SpeedGrowth + luckboost + BladeBonusGrowth.SpeedGrowth;
            truegrowth.DexterityGrowth = character.growth.DexterityGrowth + luckboost + BladeBonusGrowth.DexterityGrowth;
            truegrowth.LuckGrowth = character.growth.LuckGrowth + luckboost + BladeBonusGrowth.LuckGrowth;

            character.stats.HP += truegrowth.HPGrowth / 100f;
            character.stats.Strength += truegrowth.StrengthGrowth / 100f;
            character.stats.Psyche += truegrowth.PsycheGrowth / 100f;
            character.stats.Defense += truegrowth.DefenseGrowth / 100f;
            character.stats.Resistance += truegrowth.ResistanceGrowth / 100f;
            character.stats.Speed += truegrowth.SpeedGrowth / 100f;
            character.stats.Dexterity += truegrowth.DexterityGrowth / 100f;
            character.stats.Luck += truegrowth.LuckGrowth / 100f;



            character.level += 1;


            return CalculateLevelUps(remainingLevelUps - 1, character, weaponID);
        }


    }

    private StatGrowth CalculateBonusGrowth()
    {
        StatGrowth BonusGrowth = US.CalculateGrowthBonusByBlade(WeaponClasses[currentStatCalculation.WeaponID]);

        int globalbonus = 0;

        if (US.GetSkill(57) || US.GetSkill(72) || US.GetSkill(73))
        {
            globalbonus += US.cystalheartgrowthboost;
        }

        if (US.GetSkill(10))
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
                if (GO.name.ToLower().Contains("variant1"))
                {
                    USForEnemy = GO.GetComponent<UnitScript>();
                }
                else if (GO.name.ToLower().Contains("variant2"))
                {
                    US = GO.GetComponent<UnitScript>();
                }

            }
            if (GO != null && GO.transform.childCount > 0)
            {
                foreach (Transform child in GO.transform)
                {
                    if (child.GetComponent<GridScript>() != null)
                    {
                        AM = child.GetComponent<GridScript>().actionsMenu.GetComponent<ActionsMenu>();
                        break;
                    }
                }
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

        if (USForEnemy != null)
        {
            Debug.Log("UnitScript Variant found");
        }
        else
        {
            Debug.LogError("UnitScript Variant not found");
        }

        if (AM != null)
        {
            Debug.Log("ActionsMenu found");
        }
        else
        {
            Debug.LogError("ActionsMenu not found");
        }

    }

}
#endif