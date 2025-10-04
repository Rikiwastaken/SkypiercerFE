using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using static DataScript;
using static UnitScript;

public class DataScript : MonoBehaviour
{

    public List<equipment> equipmentList;

    public List<equipment> BasicGradeList;

    public List<ClassInfo> ClassList;

    public List<Skill> SkillList;

    public List<Character> PlayableCharacterList;

    public float manualgamespeed = -1;

    public Inventory PlayerInventory;

    public List<Character> DefaultPlayableCharacterList;
    public Inventory DefaultInventory;

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
                inventoryItem.Quantity = 50;
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
                }
                else
                {
                    equipmentList[i].Range = 1;
                }

            }

            equipmentList[i].ID = i;
        }
    }

    public void GenerateEquipmentList(Character Character)
    {
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

                if(Character.name == "Zack")
                {
                    switch(newequipment.type.ToLower())
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
#endif
}
