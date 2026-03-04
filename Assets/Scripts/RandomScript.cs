using System;
using System.Collections.Generic;
using UnityEngine;
using static UnitScript;

public class RandomScript : MonoBehaviour
{

    [Header("Random Values")]

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

    [Header("Random Values Settings")]

    public bool use2RN;

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
            HitValues.Add(CalculateAValue());
            CritValues.Add(CalculateAValue());
            if (UnitCharacter.affiliation == "playable")
            {
                if (i < numberofLevelValues)
                {
                    RandomLevelValues newlevelvalues = new RandomLevelValues();
                    newlevelvalues.HPRandomValue = CalculateAValue();
                    newlevelvalues.StrengthRandomValue = CalculateAValue();
                    newlevelvalues.PsycheRandomValue = CalculateAValue();
                    newlevelvalues.DefenseRandomValue = CalculateAValue();
                    newlevelvalues.ResistanceRandomValue = CalculateAValue();
                    newlevelvalues.SpeedRandomValue = CalculateAValue();
                    newlevelvalues.DexterityRandomValue = CalculateAValue();
                    levelValues.Add(newlevelvalues);
                }
            }
            else
            {
                personalityValues.Add(CalculateAValue());
            }

        }
    }

    public int GetHitValue()
    {
        if (hitvaluesindex >= HitValues.Count)
        {
            hitvaluesindex = 0;
        }
        int value = HitValues[hitvaluesindex];
        hitvaluesindex++;
        return value;
    }

    public int GetCritValue()
    {
        if (CritValuesindex >= CritValues.Count)
        {
            CritValuesindex = 0;
        }
        int value = CritValues[CritValuesindex];
        CritValuesindex++;
        return value;
    }

    public int GetPersonalityValue()
    {
        if (personalityvaluesindex >= personalityValues.Count)
        {
            personalityvaluesindex = 0;
        }
        int value = personalityValues[personalityvaluesindex];
        personalityvaluesindex++;
        return value;
    }

    public RandomLevelValues GetLevelUpRandomValues()
    {
        if (levelvaluesindex >= levelValues.Count)
        {
            levelvaluesindex = 0;
        }
        RandomLevelValues randomLevelValues = levelValues[levelvaluesindex];
        levelvaluesindex++;
        return randomLevelValues;
    }

    private int CalculateAValue()
    {
        if (use2RN)
        {
            return (UnityEngine.Random.Range(1, 101) + UnityEngine.Random.Range(1, 101)) / 2;
        }
        else
        {
            return UnityEngine.Random.Range(1, 101);
        }
    }


}
