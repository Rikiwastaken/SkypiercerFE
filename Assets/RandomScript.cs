using System;
using System.Collections.Generic;
using UnityEngine;
using static UnitScript;

public class RandomScript : MonoBehaviour
{

    public List<int> HitValues;

    public int hitvaluesindex;

    public List<int> CritValues;

    public int CritValuesindex;

    public List<int> personalityValues;

    public int personalityvaluesindex;

    public List<RandomLevelValues> levelValues;

    public int levelvaluesindex;

    [Serializable]
    public class RandomLevelValues
    {
        public int HPRandomValue;
        public int StrengthRandomValue;
        public int PsycheRandomValue;
        public int DefenseRandomValue;
        public int ResistanceRandomValue;
        public int SpeedRandomValue;
        public int DexterityRandomValue;
    }

    public List<RandomLevelValues> LevelValues;

    private bool initialized;

    private Character UnitCharacter;

    public int numberofRandomValues;

    public int numberofLevelValues;

    // Update is called once per frame
    void Update()
    {
        if (!initialized && GetComponent<UnitScript>().UnitCharacteristics != null)
        {
            InitializeRandomValues();
        }
    }

    public void InitializeRandomValues()
    {
        initialized = true;
        UnitCharacter = GetComponent<UnitScript>().UnitCharacteristics;

        HitValues = new List<int>();
        CritValues = new List<int>();
        personalityValues = new List<int>();
        levelValues = new List<RandomLevelValues> { };
        for (int i = 0; i < numberofRandomValues; i++)
        {
            HitValues.Add((int)UnityEngine.Random.Range(0, 100));
            CritValues.Add((int)UnityEngine.Random.Range(0, 100));
            if (UnitCharacter.affiliation == "playable")
            {
                if (i < numberofLevelValues)
                {
                    RandomLevelValues newlevelvalues = new RandomLevelValues();
                    newlevelvalues.HPRandomValue = (int)UnityEngine.Random.Range(0, 100);
                    newlevelvalues.StrengthRandomValue = (int)UnityEngine.Random.Range(0, 100);
                    newlevelvalues.PsycheRandomValue = (int)UnityEngine.Random.Range(0, 100);
                    newlevelvalues.DefenseRandomValue = (int)UnityEngine.Random.Range(0, 100);
                    newlevelvalues.ResistanceRandomValue = (int)UnityEngine.Random.Range(0, 100);
                    newlevelvalues.SpeedRandomValue = (int)UnityEngine.Random.Range(0, 100);
                    newlevelvalues.DexterityRandomValue = (int)UnityEngine.Random.Range(0, 100);
                    levelValues.Add(newlevelvalues);
                }
            }
            else
            {
                personalityValues.Add((int)UnityEngine.Random.Range(0, 100));
            }

        }
    }

    public int GetHitValue()
    {
        int value = HitValues[hitvaluesindex];
        hitvaluesindex++;
        return value;
    }

    public int GetCritValue()
    {
        int value = CritValues[CritValuesindex];
        CritValuesindex++;
        return value;
    }

    public int GetPersonalityValue()
    {
        int value = personalityValues[personalityvaluesindex];
        personalityvaluesindex++;
        return value;
    }

    public RandomLevelValues GetLevelUpRandomValues()
    {
        RandomLevelValues randomLevelValues = levelValues[levelvaluesindex];
        levelvaluesindex++;
        return randomLevelValues;
    }

}
