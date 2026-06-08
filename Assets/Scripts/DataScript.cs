using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using static SaveManager;
using static UnitScript;

public class DataScript : MonoBehaviour
{

    public static DataScript instance;

    [Space]
    [Header("Character and inventory variables")]


    public List<ClassInfo> ClassList;

    public List<Character> PlayableCharacterList;

    public Inventory PlayerInventory;

    public List<Character> DefaultPlayableCharacterList;
    public Inventory DefaultInventory;

    public List<Sprite> DialogueSpriteList;

    public List<Sprite> EnemySprites;


    [Space]
    [Header("Story Advancement Variables")]

    public List<ChapterFlags> ChapterFlagsList;
    public List<ChapterFlags> SidestoryFlagList;
    public List<int> CompletedSideStories;





    [Serializable]
    public class SkillPerMap
    {
        public string SceneName;
        public int mapID = -1;
        public int SideStoryID = -1;
        public List<int> SkillsOnTheMap;
        public int averagelevel;
    }

    [Serializable]
    public class ClassInfo
    {
        public string name;
        public BaseStats BaseStats;
        public StatGrowth StatGrowth;
        public int movements = 6;
        public int ID;
    }

    [Serializable]
    public class Inventory
    {
        public List<InventoryItem> inventoryItems;
    }

    [Serializable]
    public class Bonds
    {
        public int ID;
        public string Name;
        public List<int> Characters;
        public int BondPoints;
        public int BondLevel;
        public int BondDialogueSeen;
        public int MaxLevel;
    }

    [Serializable]
    public class BondClassForLoading
    {
        public List<Bonds> BondList;
    }

    [Serializable]
    public class InventoryItem
    {
        public int type; //0 Item, 1 Skill
        public int ID;
        public int Quantity;
    }

    [Serializable]
    public class Skill
    {
        public string name;
        public string Descriptions;
        public bool IsCommand;
        public int Cost;
        public int targettype; // 0 enemies, 1 allies, 2 walls, 3 self
        public int range;
        public bool buyable;
        public bool AlwaysPresentInShop;
        public int ID;
    }

    [Serializable]
    private class SkillListWrapper
    {
        public List<Skill> SkillList;
    }

    [Serializable]
    private class CharacterDialogueWrapper
    {
        public List<CharacterDialogues> CharacterDialogues;
    }

    [Serializable]
    public class CharacterDialogues
    {
        public int CharacterID;
        public string DeathQuote;
        public string DeathReaction_Zack;
        public string DeathReaction_Kira;
        public string DeathReaction_Gale;
        public string GoodLvlUp;
        public string MidLvlUp;
        public string LowLvlUp;
    }
    [Space]
    [Header("Bond Variables")]

    public List<Bonds> BondsList;
    public int bondincreaseperaction;
    public int maxdistanceforbondincrease;
    [Space]
    [Header("Equipment Variables")]

    public List<equipment> equipmentList;

    public List<equipment> BasicGradeList;
    public int MasteryforLevel0;
    public int MasteryforLevel1;
    public int MasteryforLevel2;
    public int MasteryforLevel3;
    [Space]
    [Header("Skill Variables")]

    public List<Skill> SkillList;
    public List<SkillPerMap> skillsPerMap;
    public int SkillCoins;
    [Space]
    [Header("Examode Variables")]
    public Material ExamodeMaterial;
    public int ExamodeUnlockChapter_Zack;
    public int ExamodeUnlockChapter_Kira;
    public int ExamodeUnlockChapter_Gale;
    public int ExamodeMaxTurns;
    public int ExamodePointsForActivation;


    [Space]
    [Header("System Variables")]
    public float manualgamespeed = -1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        if (SceneManager.GetActiveScene().name == "FirstScene")
        {
            SceneManager.LoadScene("MainMenu");
        }
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (manualgamespeed > 0)
        {
            Time.timeScale = manualgamespeed;
        }

    }

    private void Setup()
    {
        SetupCharacterIDs();
        SetupEquipment();
        SetupClasses();
        SetupInventory();
        SetupDefaultCharacterList();
        CalculateMaxSP();
    }

    public void CalculateMaxSP()
    {
        GameObject USGO = new GameObject();
        USGO.AddComponent<UnitScript>();

        UnitScript US = USGO.GetComponent<UnitScript>();

        foreach (Character character in PlayableCharacterList)
        {
            US.CalculateSkillPoints(character);
        }

        DestroyImmediate(USGO);
    }

    private void SetupCharacterIDs()
    {
        for (int i = 0; i < PlayableCharacterList.Count; i++)
        {
            PlayableCharacterList[i].playableStats.ID = i;
        }
    }
    private void SetupInventory()
    {
        PlayerInventory = new Inventory();
        PlayerInventory.inventoryItems = new List<InventoryItem>();
        foreach (Skill skill in SkillList)
        {
            if (skill.ID != 0)
            {
                InventoryItem inventoryItem = new InventoryItem();
                inventoryItem.Quantity = 0;
                inventoryItem.type = 1;
                inventoryItem.ID = skill.ID;
                PlayerInventory.inventoryItems.Add(inventoryItem);

                if (skill.ID == 74)
                {
                    inventoryItem.Quantity = 99;
                }
            }
        }
    }


    public void SetupCharactersForChapter(int Chapter)
    {

        SaveManager SM = SaveManager.instance;

        SM.activeSlot = 0;

        SaveManager.SaveClass newsave = new SaveManager.SaveClass();

        newsave.versionID = SM.versionID;
        newsave.slot = 0;
        newsave.chapter = Chapter;

        Inventory inventory = new Inventory();
        inventory.inventoryItems = new List<InventoryItem>();

        foreach (InventoryItem item in DefaultInventory.inventoryItems)
        {
            inventory.inventoryItems.Add(item);
        }

        SetupInventoryForChapter(Chapter, inventory);

        PlayerInventory = inventory;

        int targetlevel = 0;

        foreach (SkillPerMap mapinfo in skillsPerMap)
        {
            if (mapinfo.mapID == Chapter)
            {
                targetlevel = mapinfo.averagelevel - 1;
                break;
            }
        }
        CharacterUnlockingSafeguard(Chapter);

        GameObject USGO = new GameObject();
        UnitScript US = USGO.AddComponent<UnitScript>();
        bool previousFixedGrowth = SaveManager.instance.Options.FixedGrowth;
        SaveManager.instance.Options.FixedGrowth = true;
        try
        {
            int targetmastery = 1;
            if (Chapter > 5)
            {
                targetmastery = 2;
            }

            foreach (Character unit in PlayableCharacterList)
            {
                if (unit.playableStats.unlocked)
                {
                    int baselevel = unit.level;
                    US.UnitCharacteristics = unit;
                    for (int i = 0; i < targetlevel - baselevel; i++)
                    {
                        US.LevelUp();
                    }
                    US.UnitCharacteristics.experience = 0;
                    foreach (WeaponMastery mastery in unit.Masteries)
                    {
                        if (mastery.Level != -1)
                        {
                            mastery.Level = targetmastery;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            SaveManager.instance.Options.FixedGrowth = previousFixedGrowth;
        }


        Destroy(US);
        //SM.CreateCharacterSaveList()
        //SM.ApplyCharacterSaves(PlayableCharacterList, newsave.PlayableCharacterList);

    }
    public void SetupInventoryForChapter(int Chapter, Inventory inventory)
    {
        int chapterlistlength = ChapterFlagsList.Count;

        while (chapterlistlength < Chapter)
        {
            ChapterFlags flags = new ChapterFlags();
            ChapterFlagsList.Add(flags);
            flags.talkflags = new List<bool>();
            flags.copyflags = new List<bool>();
            for (int i = 0; i < 10; i++)
            {
                flags.talkflags.Add(true);
                flags.copyflags.Add(true);
            }
            chapterlistlength = ChapterFlagsList.Count;
        }

        foreach (InventoryItem item in inventory.inventoryItems)
        {
            if (Chapter > 2)
            {
                if (item.ID == 56)
                {
                    item.Quantity += 2;
                }
            }
            if (Chapter > 3)
            {
                if (item.ID == 6)
                {
                    item.Quantity += 1;
                }
                if (item.ID == 3)
                {
                    item.Quantity += 1;
                }
                if (item.ID == 4)
                {
                    item.Quantity += 1;
                }
            }
            if (Chapter > 4)
            {
                if (item.ID == 61)
                {
                    item.Quantity += 1;
                }
                if (item.ID == 18)
                {
                    item.Quantity += 1;
                }
                if (item.ID == 40)
                {
                    item.Quantity += 1;
                }
            }
            if (Chapter > 5)
            {
                if (item.ID == 42)
                {
                    item.Quantity += 1;
                }
                if (item.ID == 41)
                {
                    item.Quantity += 1;
                }
                if (item.ID == 32)
                {
                    item.Quantity += 1;
                }
            }
            if (Chapter > 6)
            {
                if (item.ID == 11)
                {
                    item.Quantity += 1;
                }
                if (item.ID == 19)
                {
                    item.Quantity += 1;
                }
            }
            if (Chapter > 7)
            {
                if (item.ID == 64)
                {
                    item.Quantity += 1;
                }
                if (item.ID == 25)
                {
                    item.Quantity += 1;
                }
                if (item.ID == 79)
                {
                    item.Quantity += 1;
                }
            }
            if (Chapter > 8)
            {
                if (item.ID == 66)
                {
                    item.Quantity += 1;
                }
                if (item.ID == 81)
                {
                    item.Quantity += 1;
                }
                if (item.ID == 82)
                {
                    item.Quantity += 1;
                }
            }
            if (Chapter > 9)
            {
                if (item.ID == 69)
                {
                    item.Quantity += 1;
                }
                if (item.ID == 65)
                {
                    item.Quantity += 1;
                }
                if (item.ID == 39)
                {
                    item.Quantity += 1;
                }
                if (item.ID == 34)
                {
                    item.Quantity += 1;
                }
            }
            if (Chapter > 10)
            {
                if (item.ID == 54)
                {
                    item.Quantity += 1;
                }
                if (item.ID == 2)
                {
                    item.Quantity += 1;
                }
                if (item.ID == 21)
                {
                    item.Quantity += 1;
                }
                if (item.ID == 27)
                {
                    item.Quantity += 1;
                }
            }
        }





    }



    private void SetupClasses()
    {
        for (int i = 0; i < ClassList.Count; i++)
        {
            ClassList[i].ID = i;
        }
    }
    private void SetupDefaultCharacterList()
    {
        DefaultPlayableCharacterList = new List<Character>();
        DefaultInventory = new Inventory() { inventoryItems = new List<InventoryItem>() };
        GameObject USGO = new GameObject();
        UnitScript unitScript = USGO.AddComponent<UnitScript>();
        try
        {
            foreach (Character character in PlayableCharacterList)
            {
                DefaultPlayableCharacterList.Add(unitScript.CreateCopy(character));
            }
            foreach (InventoryItem item in PlayerInventory.inventoryItems)
            {
                DefaultInventory.inventoryItems.Add(CopyInventoryItem(item));
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
        finally
        {
            DestroyImmediate(USGO);
        }
    }

    public void RestoreBaseCharacterValues()
    {
        PlayableCharacterList = new List<Character>();
        PlayerInventory = new Inventory() { inventoryItems = new List<InventoryItem>() };
        foreach (Character character in DefaultPlayableCharacterList)
        {
            PlayableCharacterList.Add(CopyChararacter(character));
        }
        foreach (InventoryItem item in DefaultInventory.inventoryItems)
        {
            PlayerInventory.inventoryItems.Add(CopyInventoryItem(item));
        }
    }

    private Character CopyChararacter(Character charactertoCopy)
    {
        Character copy = new Character();
        copy.ID = charactertoCopy.ID;
        copy.name = charactertoCopy.name;
        copy.stats = charactertoCopy.stats;
        copy.movements = charactertoCopy.movements;
        copy.growth = charactertoCopy.growth;
        copy.equipmentsIDs = charactertoCopy.equipmentsIDs;
        copy.UnitSkill = charactertoCopy.UnitSkill;
        copy.EquipedSkills = charactertoCopy.EquipedSkills;
        copy.playableStats = charactertoCopy.playableStats;
        copy.level = charactertoCopy.level;
        copy.experience = charactertoCopy.experience;
        List<WeaponMastery> masteriescopy = new List<WeaponMastery>();
        foreach (WeaponMastery mastery in charactertoCopy.Masteries)
        {
            masteriescopy.Add(new WeaponMastery() { Exp = mastery.Exp, Level = mastery.Level, weapontype = mastery.weapontype });
        }
        copy.Masteries = masteriescopy;
        return copy;
    }

    private InventoryItem CopyInventoryItem(InventoryItem itemtocopy)
    {
        InventoryItem copy = new InventoryItem();
        copy.Quantity = itemtocopy.Quantity;
        copy.type = itemtocopy.type;
        copy.ID = itemtocopy.ID;
        return copy;
    }

    private void SetupEquipment()
    {
        for (int i = 0; i < equipmentList.Count; i++)
        {

            SetupBaseEquipment(equipmentList[i]);

            equipmentList[i].ID = i;
        }
    }

    private void SetupBaseEquipment(equipment equipmenttoSetup)
    {


        if (equipmenttoSetup.Grade > 0)
        {
            equipment equipemnttoappy = BasicGradeList[equipmenttoSetup.Grade];
            equipmenttoSetup.BaseDamage = equipemnttoappy.BaseDamage;
            equipmenttoSetup.BaseHit = equipemnttoappy.BaseHit;
            equipmenttoSetup.BaseCrit = equipemnttoappy.BaseCrit;
            if (equipmenttoSetup.Name.Contains("Machine"))
            {
                equipmenttoSetup.Currentuses = 99;
                equipmenttoSetup.Maxuses = 99;
            }
            else
            {
                equipmenttoSetup.Currentuses = equipemnttoappy.Maxuses;
                equipmenttoSetup.Maxuses = equipemnttoappy.Maxuses;
            }

            if (equipmenttoSetup.type.ToLower() == "bow")
            {
                equipmenttoSetup.Range = 2;
                equipmenttoSetup.equipmentmodel.localposition = new Vector3(-0.04f, 0.065f, 0);
            }
            else
            {
                equipmenttoSetup.Range = 1;
                equipmenttoSetup.equipmentmodel.localposition = Vector3.zero;
            }

            if (equipmenttoSetup.Name.Contains("Garland"))
            {
                equipmenttoSetup.Range = 2;
                equipmenttoSetup.Currentuses = 99;
                equipmenttoSetup.Maxuses = 99;
            }

            if (equipmenttoSetup.Name.Contains("Bonnie & Clyde"))
            {
                equipmenttoSetup.Currentuses = 99;
                equipmenttoSetup.Maxuses = 99;
            }

            switch (equipmenttoSetup.type.ToLower())
            {
                case ("sword"):
                    equipmenttoSetup.BaseCrit = equipemnttoappy.BaseCrit + 3 * equipmenttoSetup.Grade;
                    break;
                case ("spear"):
                    equipmenttoSetup.BaseHit = equipemnttoappy.BaseHit + 5 * equipmenttoSetup.Grade;
                    break;
                case ("greatsword"):
                    equipmenttoSetup.BaseDamage = equipemnttoappy.BaseDamage + 1 * equipmenttoSetup.Grade;
                    break;
                case ("bow"):
                    equipmenttoSetup.BaseDamage = equipemnttoappy.BaseDamage - 1 * equipmenttoSetup.Grade;
                    break;
                case ("scythe"):
                    equipmenttoSetup.BaseDamage = equipemnttoappy.BaseDamage + 1 * equipmenttoSetup.Grade;
                    break;
                case ("shield"):
                    equipmenttoSetup.BaseDamage = equipemnttoappy.BaseDamage - 1 * equipmenttoSetup.Grade;
                    break;
            }

            equipmenttoSetup.equipmentmodel.localrotation = Vector3.zero;
            equipmenttoSetup.equipmentmodel.localscale = Vector3.one;

        }
    }

    public void CalculateModifierStatChanges(equipment equipment)
    {
        if (equipment.Modifier == null || equipment.Modifier == "" || equipment.Modifier == "Basic")
        {
            return;
        }
        SetupBaseEquipment(equipment);
        switch (equipment.Modifier.ToLower())
        {
            case ("sharp"):
                equipment.BaseHit -= 5 * equipment.Grade;
                equipment.BaseCrit += 2 * equipment.BaseCrit;
                break;
            case ("handy"):
                equipment.BaseHit += 5 * equipment.Grade;
                equipment.BaseCrit -= 2 * equipment.BaseCrit;
                break;
            case ("raw"):
                equipment.BaseDamage += 1 * equipment.Grade;
                equipment.BaseCrit -= 2 * equipment.BaseCrit;
                break;
            case ("farsight"):
                equipment.Range += 1;
                equipment.BaseHit -= 20;
                break;
            case ("focused"):
                equipment.BaseDamage *= 2;
                equipment.Maxuses /= 2;
                break;
            case ("ranged"):
                equipment.BaseDamage /= 2;
                equipment.Range += 3;
                break;
        }
    }

    public List<Bonds> CreateBondsCopy()
    {
        List<Bonds> bondscopy = new List<Bonds>();
        foreach (Bonds bond in BondsList)
        {
            Bonds bondcopy = new Bonds();
            bondcopy.ID = bond.ID;
            bondcopy.Characters = bond.Characters;
            bondcopy.BondDialogueSeen = bond.BondDialogueSeen;
            bondcopy.BondLevel = bond.BondLevel;
            bondcopy.BondPoints = bond.BondPoints;
            bondcopy.MaxLevel = bond.MaxLevel;
            bondscopy.Add(bondcopy);
        }
        return bondscopy;
    }

    public void IncreaseBonds(GameObject MainUnit, GameObject OtherUnit)
    {

        int UnitID = MainUnit.GetComponent<UnitScript>().UnitCharacteristics.ID;
        int OtherUnitID = OtherUnit.GetComponent<UnitScript>().UnitCharacteristics.ID;

        TurnManger turnManger = TurnManger.instance;
        if (turnManger == null)
        {
            return;
        }



        foreach (Bonds bond in BondsList)
        {
            if (bond.Characters.Contains(UnitID) && bond.Characters.Contains(OtherUnitID))
            {



                if (OtherUnit.GetComponent<UnitScript>().GetSkill(66) && MainUnit.GetComponent<UnitScript>().GetSkill(66)) //Friendly
                {
                    bond.BondPoints += bondincreaseperaction * 3;

                }
                else if (OtherUnit.GetComponent<UnitScript>().GetSkill(66) || MainUnit.GetComponent<UnitScript>().GetSkill(66))
                {
                    bond.BondPoints += bondincreaseperaction * 2;

                }
                else
                {
                    bond.BondPoints += bondincreaseperaction;

                }
                MainUnit.GetComponent<UnitScript>().AddNumber(0, true, "", true);
                OtherUnit.GetComponent<UnitScript>().AddNumber(0, true, "", true);

                return;
            }
        }
    }

    public void CharacterUnlockingSafeguard(int chapter)
    {
        foreach (Character character in PlayableCharacterList)
        {
            switch (character.ID)
            {
                case 0: //Zack
                    character.playableStats.unlocked = true;
                    break;
                case 1: //Lea
                    character.playableStats.unlocked = true;
                    break;
                case 2: //Elwyn
                    character.playableStats.unlocked = true;
                    break;
                case 3: //Sorak
                    if (chapter >= 2)
                    {
                        character.playableStats.unlocked = true;
                    }
                    break;
                case 4: //Lyv
                    if (chapter >= 2)
                    {
                        character.playableStats.unlocked = true;
                    }
                    break;
                case 5: // Sieg
                    if (chapter >= 3)
                    {
                        character.playableStats.unlocked = true;
                    }

                    break;
                case 6: // Mir
                    if (chapter >= 1 && chapter != 2 && chapter < 11)
                    {
                        character.playableStats.unlocked = true;
                    }
                    else
                    {
                        character.playableStats.unlocked = false;
                    }
                    break;
                case 7: // Ruben
                    if (chapter >= 5)
                    {
                        character.playableStats.unlocked = true;
                    }
                    break;
                case 8: // Gusto
                    if (chapter >= 6)
                    {
                        character.playableStats.unlocked = true;
                    }
                    break;
                case 9: // Kira
                    if (chapter >= 6)
                    {
                        character.playableStats.unlocked = true;
                    }
                    break;
                case 10: // Gwenie
                    if (chapter >= 7 && ChapterFlagsList.Count > 7 && ChapterFlagsList[7].talkflags[0])
                    {
                        character.playableStats.unlocked = true;
                    }

                    break;


            }
        }
    }

    public void SpreadBonds(GameObject Unit)
    {
        TurnManger turnManger = TurnManger.instance;

        Character charunit = Unit.GetComponent<UnitScript>().UnitCharacteristics;

        if (charunit.affiliation.ToLower() != "playable" || turnManger == null)
        {
            return;
        }

        foreach (GameObject otherunit in turnManger.playableunitGO)
        {
            Character charother = otherunit.GetComponent<UnitScript>().UnitCharacteristics;
            if (charother != charunit && charother.currentHP > 0 && ManhattanDistance(charunit.currentTile.GridCoordinates, charother.currentTile.GridCoordinates) <= 2)
            {
                IncreaseBonds(Unit, otherunit);
            }
        }

    }

    public void UpdatePlayableUnits()
    {
        UnitScript[] unitscripts = GameObject.FindObjectsByType<UnitScript>(FindObjectsSortMode.None);

        List<Character> playablecopy = new List<Character>();

        foreach (Character c in PlayableCharacterList)
        {
            UnitScript uS = new UnitScript();
            playablecopy.Add(uS.CreateCopy(c));
        }

        foreach (UnitScript unitscript in unitscripts)
        {
            foreach (Character character in PlayableCharacterList)
            {
                Character currentchar = unitscript.UnitCharacteristics;
                if (currentchar.affiliation == "playable" && character.ID == currentchar.ID)
                {
                    playablecopy[PlayableCharacterList.IndexOf(character)] = currentchar;
                }
            }
        }

        PlayableCharacterList = playablecopy;

    }
    private void UpdateEquipmentID(Character character)
    {
        List<WeaponMastery> Masteries = character.Masteries;
        foreach (WeaponMastery weaponMastery in Masteries)
        {
            if (weaponMastery.Level > 0)
            {
                bool weaponfound = false;
                int equipmentIDLen = character.equipmentsIDs.Count;
                for (int i = 0; i < equipmentIDLen; i++)
                {
                    equipment currentequipment = equipmentList[character.equipmentsIDs[i]];
                    string currentequipmenttype = currentequipment.type;
                    int currentequipmentgrade = currentequipment.Grade;
                    if (weaponMastery.weapontype.ToLower() == currentequipmenttype.ToLower())
                    {
                        weaponfound = true;
                        if (currentequipmentgrade < weaponMastery.Level)
                        {
                            character.equipmentsIDs[i] += weaponMastery.Level - currentequipmentgrade;
                        }
                        break;
                    }
                }
                if (!weaponfound)
                {
                    character.equipmentsIDs.Add(GetWeaponID(weaponMastery.weapontype, weaponMastery.Level));
                }
            }
        }
    }

    public equipment GetWeaponFromID(int ID)
    {
        foreach (equipment equipment in equipmentList)
        {
            if (equipment.ID == ID)
            {
                return equipment;
            }
        }
        return null;
    }

    private int GetWeaponID(string type, int grade)
    {
        int weaponID = 0;

        switch (type.ToLower())
        {
            case "sword":
                weaponID = 1;
                break;
            case "spear":
                weaponID = 5;
                break;
            case "greatsword":
                weaponID = 9;
                break;
            case "bow":
                weaponID = 13;
                break;
            case "scythe":
                weaponID = 17;
                break;
            case "shield":
                weaponID = 21;
                break;
            case "staff":
                weaponID = 25;
                break;
        }

        return weaponID + grade - 1;
    }

    public void GenerateEquipmentList(Character Character)
    {
        int previouslyequipedweapon = Character.previouslyequipedweaponID;

        GameObject TempGO = new GameObject();
        TempGO.AddComponent<UnitScript>();
        TempGO.GetComponent<UnitScript>().enabled = false;
        TempGO.GetComponent<UnitScript>().UnitCharacteristics = Character;

        if (Character.previousTelekinesis == 1)
        {
            TempGO.GetComponent<UnitScript>().ToggleTelekinesis(true);
        }
        else if (Character.previousTelekinesis == 2)
        {
            TempGO.GetComponent<UnitScript>().ToggleTelekinesis(false);
        }

        foreach (WeaponMastery mastery in Character.Masteries)
        {
            TempGO.GetComponent<UnitScript>().GetNewWeaponFromMastery(mastery, Character);
        }
        //UpdateEquipmentID(Character);
        List<int> equipmentListIDs = Character.equipmentsIDs;
        List<equipment> newequipmentlist = new List<equipment>();
        foreach (int equipmentID in equipmentListIDs)
        {
            if (equipmentID >= 0 && equipmentID < equipmentList.Count)
            {
                equipment newequipment = new equipment();
                equipment equipmenttocopy = equipmentList[equipmentID];
                newequipment.Name = equipmenttocopy.Name;
                newequipment.BaseDamage = equipmenttocopy.BaseDamage;
                newequipment.BaseHit = equipmenttocopy.BaseHit;
                newequipment.BaseCrit = equipmenttocopy.BaseCrit;
                newequipment.Range = equipmenttocopy.Range;
                newequipment.type = equipmenttocopy.type;
                newequipment.Currentuses = equipmenttocopy.Currentuses;
                newequipment.Maxuses = equipmenttocopy.Maxuses;
                newequipment.ID = equipmenttocopy.ID;
                newequipment.Grade = equipmenttocopy.Grade;
                newequipment.equipmentmodel = equipmenttocopy.equipmentmodel;
                newequipmentlist.Add(newequipment);

                if (Character.name == "Zack")
                {
                    switch (newequipment.type.ToLower())
                    {
                        case "sword":
                            newequipment.Name = "Swino";
                            break;
                        case "spear":
                            newequipment.Name = "Spino";
                            break;
                        case "greatsword":
                            newequipment.Name = "Grino";
                            break;
                        case "bow":
                            newequipment.Name = "Bino";
                            break;
                        case "scythe":
                            newequipment.Name = "Scino";
                            break;
                        case "shield":
                            newequipment.Name = "Shino";
                            break;
                        case "staff":
                            newequipment.Name = "Stino";
                            break;
                    }
                    switch (newequipment.Grade)
                    {
                        case 2:
                            newequipment.Name += "+";
                            break;
                        case 3:
                            newequipment.Name += "Ex";
                            break;
                        case 4:
                            newequipment.Name += "Ult";
                            break;
                        case 5:
                            newequipment.Name += "Final";
                            break;
                    }
                }

                if (Character.name == "Kira")
                {
                    if (newequipment.type.ToLower() == "sword")
                    {
                        newequipment.Name = "Reshine";
                        switch (newequipment.Grade)
                        {
                            case 3:
                                newequipment.Name += "II";
                                break;
                            case 4:
                                newequipment.Name += "III";
                                break;
                            case 5:
                                newequipment.Name += "IV";
                                break;
                        }
                    }
                }

                if (Character.name == "Gale")
                {
                    if (newequipment.type.ToLower() == "greatsword")
                    {
                        newequipment.Name = "Abyssal";
                        switch (newequipment.Grade)
                        {
                            case 3:
                                newequipment.Name += " 2";
                                break;
                            case 4:
                                newequipment.Name += " 3";
                                break;
                            case 5:
                                newequipment.Name += " 4";
                                break;
                        }
                    }
                }

                foreach (WeaponMastery mastery in Character.Masteries)
                {
                    if (mastery.weapontype.ToLower() == equipmenttocopy.type.ToLower())
                    {
                        newequipment.Modifier = mastery.Modifier;
                        break;
                    }
                }

                if (newequipment.Modifier == null || newequipment.Modifier == "")
                {
                    newequipment.Modifier = "Basic";
                }

                CalculateModifierStatChanges(newequipment);

                newequipment.Currentuses = newequipment.Maxuses;
            }
        }
        Character.equipments = newequipmentlist;
        DestroyImmediate(TempGO);

        //if previouslyequipedweapon, then reorder the list so the weapon is first
        if (previouslyequipedweapon != -1)
        {
            equipment previousweaponequiped = null;



            foreach (equipment equp in Character.equipments)
            {
                if (equp.ID == previouslyequipedweapon)
                {
                    previousweaponequiped = equp;
                    break;
                }
            }
            if (previousweaponequiped != null)
            {
                List<equipment> newequlist = new List<equipment>() { previousweaponequiped };
                foreach (equipment equipment in Character.equipments)
                {
                    if (equipment != previousweaponequiped)
                    {
                        newequlist.Add(equipment);
                    }
                }
                Character.equipments = newequlist;
            }
        }


    }



    int ManhattanDistance(Vector2 point1, Vector2 point2)
    {
        return (int)(Mathf.Abs(point1.x - point2.x) + Mathf.Abs(point1.y - point2.y));
    }


    public void SaveCharacterTalkedToFlag(int characterTalkID, bool IsSideStory)
    {
        if (IsSideStory)
        {
            ChapterFlags currentflags = SidestoryFlagList[SaveManager.instance.currentchapter];

            int sizeofflaglist = currentflags.talkflags.Count;

            while (sizeofflaglist <= characterTalkID)
            {
                currentflags.talkflags.Add(false);
                sizeofflaglist = currentflags.talkflags.Count;
            }
            currentflags.talkflags[characterTalkID] = true;
        }
        else
        {
            ChapterFlags currentflags = ChapterFlagsList[SaveManager.instance.currentchapter];

            int sizeofflaglist = currentflags.talkflags.Count;

            while (sizeofflaglist <= characterTalkID)
            {
                currentflags.talkflags.Add(false);
                sizeofflaglist = currentflags.talkflags.Count;
            }
            currentflags.talkflags[characterTalkID] = true;
        }

    }

    public void SaveCopyFlag(int characterCopiedID, bool IsSideStory)
    {
        if (IsSideStory)
        {
            ChapterFlags currentflags = SidestoryFlagList[SaveManager.instance.currentchapter];

            int sizeofflaglist = currentflags.copyflags.Count;

            while (sizeofflaglist <= characterCopiedID)
            {
                currentflags.copyflags.Add(false);
                sizeofflaglist = currentflags.copyflags.Count;
            }

            currentflags.copyflags[characterCopiedID] = true;
        }
        else
        {
            ChapterFlags currentflags = ChapterFlagsList[SaveManager.instance.currentchapter];

            int sizeofflaglist = currentflags.copyflags.Count;

            while (sizeofflaglist <= characterCopiedID)
            {
                currentflags.copyflags.Add(false);
                sizeofflaglist = currentflags.copyflags.Count;
            }

            currentflags.copyflags[characterCopiedID] = true;
        }

    }


#if UNITY_EDITOR
    [ContextMenu("Calculate IDs and fillout out classes")]
    void CalculateIDs()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("Cannot generate persistent map in Play Mode. Exit Play Mode first.");
            return;
        }

        Setup();
        UnityEditor.EditorUtility.SetDirty(this);
    }

    [ContextMenu("Initialize Unfilled Masteries")]
    void InitializeMasteries()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("Cannot generate persistent map in Play Mode. Exit Play Mode first.");
            return;
        }

        foreach (Character character in PlayableCharacterList)
        {
            if (character.Masteries.Count < 7)
            {
                List<WeaponMastery> masteries = new List<WeaponMastery>();
                WeaponMastery swordmastery = new WeaponMastery() { weapontype = "sword", Level = -1, Exp = 0 };
                WeaponMastery spearmastery = new WeaponMastery() { weapontype = "spear", Level = -1, Exp = 0 };
                WeaponMastery greatswordmastery = new WeaponMastery() { weapontype = "greatsword", Level = -1, Exp = 0 };
                WeaponMastery bowmastery = new WeaponMastery() { weapontype = "bow", Level = -1, Exp = 0 };
                WeaponMastery scythemastery = new WeaponMastery() { weapontype = "scythe", Level = -1, Exp = 0 };
                WeaponMastery shieldmastery = new WeaponMastery() { weapontype = "shield", Level = -1, Exp = 0 };
                WeaponMastery staffmastery = new WeaponMastery() { weapontype = "staff", Level = -1, Exp = 0 };
                masteries.Add(swordmastery);
                masteries.Add(spearmastery);
                masteries.Add(greatswordmastery);
                masteries.Add(bowmastery);
                masteries.Add(scythemastery);
                masteries.Add(shieldmastery);
                masteries.Add(staffmastery);
                character.Masteries = masteries;
            }
        }
        UnityEditor.EditorUtility.SetDirty(this);
    }
    [ContextMenu("Load Skills From JSON")]
    public void LoadSkills()
    {
        string path = UnityEditor.EditorUtility.OpenFilePanel("Select Skill JSON File", "", "json");
        if (string.IsNullOrEmpty(path))
            return;

        string json = File.ReadAllText(path);

        SkillListWrapper wrapper = JsonUtility.FromJson<SkillListWrapper>(json);
        if (wrapper == null || wrapper.SkillList == null)
        {
            Debug.LogError("JSON file format invalid. Needs { \"SkillList\": [ ... ] }");
            return;
        }

        SkillList = wrapper.SkillList;
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log("Loaded " + wrapper.SkillList.Count + " skills into the SkillList!");
    }

    [ContextMenu("Save Skills To JSON")]
    public void SaveSkills()
    {
        string path = UnityEditor.EditorUtility.OpenFilePanel("Select Skill JSON File", "", "json");
        if (string.IsNullOrEmpty(path))
            return;

        SkillListWrapper wrapper = new SkillListWrapper() { SkillList = SkillList };

        string json = JsonUtility.ToJson(wrapper, true);

        try
        {
            AssetDatabase.StartAssetEditing();
            File.WriteAllText(path, json);
            Debug.Log($"Skill Saved : {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error when saving options : {e.Message}");
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
    }


    [ContextMenu("Load Character Qutoes From JSON")]
    public void LoadCharacterDialogues()
    {
        string path = UnityEditor.EditorUtility.OpenFilePanel("Select Character Quotes JSON File", "", "json");
        if (string.IsNullOrEmpty(path))
            return;

        string json = File.ReadAllText(path);

        CharacterDialogueWrapper wrapper = JsonUtility.FromJson<CharacterDialogueWrapper>(json);
        if (wrapper == null || wrapper.CharacterDialogues == null)
        {
            Debug.LogError("JSON file format invalid. Needs { \"CharacterDialogues\": [ ... ] }");
            return;
        }

        List<CharacterDialogues> CharaDialogues = wrapper.CharacterDialogues;

        for (int i = 0; i < PlayableCharacterList.Count; i++)
        {
            foreach (CharacterDialogues characterDialogue in CharaDialogues)
            {
                if (characterDialogue.CharacterID == i)
                {
                    PlayableCharacterList[i].characterDialogues = characterDialogue;
                }
            }
        }

        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log("Loaded " + wrapper.CharacterDialogues.Count + " Character Dialogues into the list!");
    }

    [ContextMenu("Load Bonds From JSON")]
    public void LoadBonds()
    {
        string path = UnityEditor.EditorUtility.OpenFilePanel("Select Bond JSON File", "", "json");
        if (string.IsNullOrEmpty(path))
            return;

        string json = File.ReadAllText(path);

        BondClassForLoading wrapper = JsonUtility.FromJson<BondClassForLoading>(json);
        if (wrapper == null || wrapper.BondList == null)
        {
            Debug.LogError("JSON file format invalid. Needs { \"BondList\": [ ... ] }");
            return;
        }

        BondsList = wrapper.BondList;
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log("Loaded " + wrapper.BondList.Count + " bonds into the BondsList!");
    }

    [ContextMenu("Load Skill Per Map")]
    public void LoadSkillPerMap()
    {
        string[] sceneGUIDs = UnityEditor.AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes/Maps" });

        skillsPerMap = new List<SkillPerMap>();

        foreach (string guid in sceneGUIDs)
        {
            float averagelevel = 0f; ;
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);

            Scene scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path, UnityEditor.SceneManagement.OpenSceneMode.Single);
            string FullSceneName = scene.name;
            string scenename = scene.name.ToLower();
            int sceneID = -1;
            int SideStoryID = -1;
            if (scenename != "prologue")
            {
                if (scenename.Contains("chapter"))
                {
                    scenename = scenename.Replace("chapter", "");
                    sceneID = int.Parse(scenename) + 1;
                }
                else if (scenename.Contains("sidestory"))
                {
                    scenename = scenename.Replace("sidestory", "");
                    SideStoryID = int.Parse(scenename) - 1;
                }
                else
                {
                    continue;
                }
            }

            List<int> skillspresent = new List<int>();

            MapInitializer target = FindAnyObjectByType<MapInitializer>(FindObjectsInactive.Include);




            if (target != null)
            {
                Debug.Log($"Found in {path}: {target.name}", target);
            }
            else
            {
                continue;
            }
            int numberofenemies = 0;
            foreach (EnemyStats enemy in target.EnemyList)
            {
                if (enemy.Skills.Count > 0 && enemy.Skills[0] > 0)
                {
                    skillspresent.Add(enemy.Skills[0]);
                }
                if (!enemy.isother)
                {
                    numberofenemies++;
                    averagelevel += enemy.desiredlevel;
                }

            }

            skillsPerMap.Add(new SkillPerMap() { SceneName = FullSceneName, mapID = sceneID, SideStoryID = SideStoryID, SkillsOnTheMap = skillspresent, averagelevel = (int)(averagelevel / (float)numberofenemies) });

        }
        UnityEditor.EditorUtility.SetDirty(this);
    }


#endif
}
