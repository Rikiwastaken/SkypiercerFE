using System;
using System.Collections.Generic;
using UnityEngine;
using static UnitScript;

public class DataScript : MonoBehaviour
{

    public List<equipment> equipmentList;

    public List<equipment> BasicGradeList;

    public List<ClassInfo> ClassList;

    public List<Skill> SkillList;

    public List<Character> PlayableCharacterList;

    public float manualgamespeed = 1;

    [Serializable]
    public class ClassInfo
    {
        public string name;
        public BaseStats BaseStats;
        public StatGrowth StatGrowth;
        public int ID;
    }

    [Serializable]
    public class Skill
    {
        public string name;
        public string Descriptions;
        public int Cost;
        public int ID;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Application.targetFrameRate = 60;
        Setup();
    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = manualgamespeed;
    }

    private void Setup()
    {
        SetupEquipment();
        SetupClasses();
    }

    private void SetupClasses()
    {
        for (int i = 0; i < ClassList.Count; i++)
        {
            ClassList[i].ID = i;
        }
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
                equipmentList[i].Currentuses = equipemnttoappy.Maxuses;
                equipmentList[i].Maxuses = equipemnttoappy.Maxuses;
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
            }
        }
        Character.equipments = newequipmentlist;
    }
}
