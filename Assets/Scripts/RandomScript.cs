using System;
using System.Collections.Generic;
using UnityEngine;
using static UnitScript;

public class RandomScript : MonoBehaviour
{

    [Header("Random Values")]

    public List<List<int>> HitValues;

    public int hitvaluesindex;

    public List<List<int>> CritValues;

    public int CritValuesindex;

    public List<List<int>> personalityValues;

    public int personalityvaluesindex;

    public List<RandomLevelValues> levelValues;

    public int levelvaluesindex;



    [Serializable]
    public class RandomLevelValues
    {
        public List<int> HPRandomValue;
        public List<int> StrengthRandomValue;
        public List<int> PsycheRandomValue;
        public List<int> DefenseRandomValue;
        public List<int> ResistanceRandomValue;
        public List<int> SpeedRandomValue;
        public List<int> DexterityRandomValue;
        public List<int> LuckRandomValue;
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

        HitValues = new List<List<int>>();
        CritValues = new List<List<int>>();
        personalityValues = new List<List<int>>();
        levelValues = new List<RandomLevelValues> { };
        for (int i = 0; i < numberofRandomValues; i++)
        {
            HitValues.Add(CalculateAValue());
            CritValues.Add(CalculateAValue());
            personalityValues.Add(CalculateAValue());
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
                    newlevelvalues.LuckRandomValue = CalculateAValue();
                    levelValues.Add(newlevelvalues);
                }
            }
        }
    }

    public int GetHitValue(int target)
    {
        if (hitvaluesindex >= HitValues.Count)
        {
            hitvaluesindex = 0;
        }
        int value = 0;
        if (target > 50)
        {
            value = HitValues[hitvaluesindex][0];
        }
        else
        {
            value = HitValues[hitvaluesindex][1];
        }
        hitvaluesindex++;
        return value;
    }

    public int GetCritValue(int target)
    {
        if (CritValuesindex >= CritValues.Count)
        {
            CritValuesindex = 0;
        }
        int value = 0;
        if (target > 50)
        {
            value = CritValues[CritValuesindex][0];
        }
        else
        {
            value = CritValues[CritValuesindex][1];
        }
        CritValuesindex++;
        return value;
    }

    public int GetPersonalityValue(int target)
    {
        if (personalityvaluesindex >= personalityValues.Count)
        {
            personalityvaluesindex = 0;
        }
        int value = 0;
        if (target > 50)
        {
            value = personalityValues[personalityvaluesindex][0];
        }
        else
        {
            value = personalityValues[personalityvaluesindex][1];
        }
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

    private List<int> CalculateAValue()
    {
        if (use2RN)
        {
            return new List<int> { (UnityEngine.Random.Range(1, 101) + UnityEngine.Random.Range(1, 101)) / 2, UnityEngine.Random.Range(1, 101) };
        }
        else
        {
            int value = UnityEngine.Random.Range(1, 101);
            return new List<int> { value, value };
        }
    }


}
