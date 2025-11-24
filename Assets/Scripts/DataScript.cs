using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using static DataScript;
using static UnitScript;

public class DataScript : MonoBehaviour
{

    public static DataScript instance;

    public List<equipment> equipmentList;

    public List<equipment> BasicGradeList;

    public List<ClassInfo> ClassList;

    public List<Skill> SkillList;

    public List<Character> PlayableCharacterList;

    public float manualgamespeed = -1;

    public Inventory PlayerInventory;

    public List<Character> DefaultPlayableCharacterList;
    public Inventory DefaultInventory;

    public List<Sprite> DialogueSpriteList;

    [Serializable]
    public class ClassInfo
    {
        public string name;
        public BaseStats BaseStats;
        public StatGrowth StatGrowth;
        public int ID;
    }

    [Serializable]
    public class Inventory
    {
        public List<InventoryItem> inventoryItems;
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
        public int ID;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        Application.targetFrameRate = 60;
        DontDestroyOnLoad(gameObject);
        if (SceneManager.GetActiveScene().name == "FirstScene")
        {
            SceneManager.LoadScene("MainMenu");
        }
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

        foreach (Character character in PlayableCharacterList)
        {
            DefaultPlayableCharacterList.Add(CopyChararacter(character));
        }
        foreach (InventoryItem item in PlayerInventory.inventoryItems)
        {
            DefaultInventory.inventoryItems.Add(CopyInventoryItem(item));
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

            if (equipmentList[i].Grade > 0)
            {
                equipment equipemnttoappy = BasicGradeList[equipmentList[i].Grade]; ;
                equipmentList[i].BaseDamage = equipemnttoappy.BaseDamage;
                equipmentList[i].BaseHit = equipemnttoappy.BaseHit;
                equipmentList[i].BaseCrit = equipemnttoappy.BaseCrit;
                if (equipmentList[i].Name.Contains("Machine"))
                {
                    equipmentList[i].Currentuses = 99;
                    equipmentList[i].Maxuses = 99;
                }
                else
                {
                    equipmentList[i].Currentuses = equipemnttoappy.Maxuses;
                    equipmentList[i].Maxuses = equipemnttoappy.Maxuses;
                }

                if (equipmentList[i].type.ToLower() == "bow")
                {
                    equipmentList[i].Range = 2;
                    equipmentList[i].equipmentmodel.localposition = new Vector3(-0.04f, 0.065f, 0);
                }
                else
                {
                    equipmentList[i].Range = 1;
                    equipmentList[i].equipmentmodel.localposition = Vector3.zero;
                }
                equipmentList[i].equipmentmodel.localrotation = Vector3.zero;
                equipmentList[i].equipmentmodel.localscale = Vector3.one;

            }

            equipmentList[i].ID = i;
        }
    }

    public void UpdatePlayableUnits()
    {
        UnitScript[] unitscripts = GameObject.FindObjectsByType<UnitScript>(FindObjectsSortMode.None);

        List< Character > playablecopy = new List< Character >();

        foreach( Character c in PlayableCharacterList)
        {
            UnitScript uS = new UnitScript();
            playablecopy.Add(uS.CreateCopy(c));
        }

        foreach (UnitScript unitscript in unitscripts)
        {
            foreach (Character character in PlayableCharacterList)
            {
                Character currentchar = unitscript.UnitCharacteristics;
                if(currentchar.affiliation=="playable" && character.ID==currentchar.ID)
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
        UpdateEquipmentID(Character);
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
                }
            }
        }
        Character.equipments = newequipmentlist;
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
        EditorUtility.SetDirty(this);
    }

    [ContextMenu("Initialize Unfilled Masteries")]
    void InitializeMasteries()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("Cannot generate persistent map in Play Mode. Exit Play Mode first.");
            return;
        }

        foreach(Character character in PlayableCharacterList)
        {
            if(character.Masteries.Count <7)
            {
                List<WeaponMastery> masteries = new List<WeaponMastery>();
                WeaponMastery swordmastery = new WeaponMastery() {weapontype= "sword",Level=-1,Exp=0 };
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
        EditorUtility.SetDirty(this);
    }

#endif
}
