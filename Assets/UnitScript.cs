using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UnitScript : MonoBehaviour
{
    [Serializable]
    public class Character
    {
        public bool protagonist;
        public string name;
        public BaseStats stats;
        public int level;
        public int experience;
        public bool isennemy;
        public StatGrowth growth;
        public int currentHP;
        public int movements;
        public Vector2 position;
    }

    [Serializable]
    public class BaseStats 
    {
        public int HP;
        public int Strength;
        public int Psyche;
        public int Defense;
        public int Resistance;
        public int Speed;
        public int Dexterity;
    }

    [Serializable]
    public class StatGrowth
    {
        public int HPGrowth;
        public int StrengthGrowth;
        public int PsycheGrowth;
        public int DefenseGrowth;
        public int ResistanceGrowth;
        public int SpeedGrowth;
        public int DexterityGrowth;
    }

    public Character UnitCharacteristics;

    public bool trylvlup;
    public bool fixedgrowth;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        transform.position = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), Mathf.Round(transform.position.z));
        UnitCharacteristics.position = new Vector2((int)transform.position.x, (int)transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        if(trylvlup)
        {
            trylvlup = false;
            LevelUp(fixedgrowth);
        }
    }

    public List<int> LevelUp(bool fixedgrowth)
    {
        List<int> lvlupresult = new List<int>();
        UnitCharacteristics.experience -=100;
        UnitCharacteristics.level +=1;
        if(fixedgrowth)
        {
            UnitCharacteristics.stats.HP += (int)(UnitCharacteristics.growth.HPGrowth/10f);

            UnitCharacteristics.currentHP += (int)(UnitCharacteristics.growth.HPGrowth / 10f);
            lvlupresult.Add((int)(UnitCharacteristics.growth.HPGrowth / 10f));

            UnitCharacteristics.stats.Strength += (int)(UnitCharacteristics.growth.StrengthGrowth / 10f);

            lvlupresult.Add((int)(UnitCharacteristics.growth.StrengthGrowth / 10f));

            UnitCharacteristics.stats.Psyche += (int)(UnitCharacteristics.growth.PsycheGrowth / 10f);

            lvlupresult.Add((int)(UnitCharacteristics.growth.PsycheGrowth / 10f));

            UnitCharacteristics.stats.Defense += (int)(UnitCharacteristics.growth.DefenseGrowth / 10f);

            lvlupresult.Add((int)(UnitCharacteristics.growth.DefenseGrowth / 10f));


            UnitCharacteristics.stats.Resistance += (int)(UnitCharacteristics.growth.ResistanceGrowth / 10f);

            lvlupresult.Add((int)(UnitCharacteristics.growth.ResistanceGrowth / 10f));

            UnitCharacteristics.stats.Speed += (int)(UnitCharacteristics.growth.SpeedGrowth / 10f);

            lvlupresult.Add((int)(UnitCharacteristics.growth.SpeedGrowth / 10f));

            UnitCharacteristics.stats.Dexterity += (int)(UnitCharacteristics.growth.DexterityGrowth / 10f);

            lvlupresult.Add((int)(UnitCharacteristics.growth.DexterityGrowth / 10f));

        }
        else
        {
            int rd = UnityEngine.Random.Range(0, 100);
            if(rd <= UnitCharacteristics.growth.HPGrowth)
            {
                UnitCharacteristics.stats.HP += 10;
                UnitCharacteristics.currentHP += 10;
                lvlupresult.Add(10);
            }
            else
            {
                UnitCharacteristics.stats.HP += 1;
                UnitCharacteristics.currentHP += 1;
                lvlupresult.Add(1);
            }

            rd = UnityEngine.Random.Range(0, 100);
            if (rd <= UnitCharacteristics.growth.StrengthGrowth)
            {
                UnitCharacteristics.stats.Strength += 10;
                lvlupresult.Add(10);
            }
            else
            {
                UnitCharacteristics.stats.Strength += 1;
                lvlupresult.Add(1);
            }

            rd = UnityEngine.Random.Range(0, 100);
            if (rd <= UnitCharacteristics.growth.PsycheGrowth)
            {
                UnitCharacteristics.stats.Psyche += 10;
                lvlupresult.Add(10);
            }
            else
            {
                UnitCharacteristics.stats.Psyche += 1;
                lvlupresult.Add(1);
            }

            rd = UnityEngine.Random.Range(0, 100);
            if (rd <= UnitCharacteristics.growth.DefenseGrowth)
            {
                UnitCharacteristics.stats.Defense += 10;
                lvlupresult.Add(10);
            }
            else
            {
                UnitCharacteristics.stats.Defense += 1;
                lvlupresult.Add(1);
            }


            rd = UnityEngine.Random.Range(0, 100);
            if (rd <= UnitCharacteristics.growth.ResistanceGrowth)
            {
                UnitCharacteristics.stats.Resistance += 10;
                lvlupresult.Add(10);
            }
            else
            {
                UnitCharacteristics.stats.Resistance += 1;
                lvlupresult.Add(1);
            }

            rd = UnityEngine.Random.Range(0, 100);
            if (rd <= UnitCharacteristics.growth.SpeedGrowth)
            {
                UnitCharacteristics.stats.Speed += 10;
                lvlupresult.Add(10);
            }
            else
            {
                UnitCharacteristics.stats.Speed += 1;
                lvlupresult.Add(1);
            }

            rd = UnityEngine.Random.Range(0, 100);
            if (rd <= UnitCharacteristics.growth.DexterityGrowth)
            {
                UnitCharacteristics.stats.Dexterity += 10;
                lvlupresult.Add(10);
            }
            else
            {
                UnitCharacteristics.stats.Dexterity += 1;
                lvlupresult.Add(1);
            }
        }
        return lvlupresult;
    }
}
