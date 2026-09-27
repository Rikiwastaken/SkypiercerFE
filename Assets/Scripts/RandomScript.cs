using System;
using System.Collections.Generic;
using UnityEngine;
using static UnitScript;

public class RandomScript : MonoBehaviour
{

    [Header("Random Values")]
    [SerializeField]
    public List<RandomValuesDup> HitValues;
    [SerializeField]
    public int hitvaluesindex;
    [SerializeField]
    public List<RandomValuesDup> CritValues;
    [SerializeField]
    public int CritValuesindex;
    [SerializeField]
    public List<RandomValuesDup> personalityValues;
    [SerializeField]
    public int personalityvaluesindex;
    [SerializeField]
    public List<RandomLevelValues> levelValues;
    [SerializeField]
    public int levelvaluesindex;



    [Serializable]
    public class RandomLevelValues
    {
        public List<RandomValuesDup> HPRandomValue;
        public List<RandomValuesDup> StrengthRandomValue;
        public List<RandomValuesDup> PsycheRandomValue;
        public List<RandomValuesDup> DefenseRandomValue;
        public List<RandomValuesDup> ResistanceRandomValue;
        public List<RandomValuesDup> SpeedRandomValue;
        public List<RandomValuesDup> DexterityRandomValue;
        public List<RandomValuesDup> LuckRandomValue;
    }

    [Serializable]
    public class RandomValuesDup
    {
        public int NormalValue;
        public int TwoRNValue;
    }

    [SerializeField]
    public List<RandomLevelValues> LevelValues;
    [SerializeField]
    private bool initialized;

    private Character UnitCharacter;

    [Header("Random Values Settings")]

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

        HitValues = new List<RandomValuesDup>();
        CritValues = new List<RandomValuesDup>();
        personalityValues = new List<RandomValuesDup>();
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
                    newlevelvalues.HPRandomValue = new List<RandomValuesDup>() { CalculateAValue(0), CalculateAValue(0), CalculateAValue(0) };
                    newlevelvalues.StrengthRandomValue = new List<RandomValuesDup>() { CalculateAValue(0), CalculateAValue(0), CalculateAValue(0) };
                    newlevelvalues.PsycheRandomValue = new List<RandomValuesDup>() { CalculateAValue(0), CalculateAValue(0), CalculateAValue(0) };
                    newlevelvalues.DefenseRandomValue = new List<RandomValuesDup>() { CalculateAValue(0), CalculateAValue(0), CalculateAValue(0) };
                    newlevelvalues.ResistanceRandomValue = new List<RandomValuesDup>() { CalculateAValue(0), CalculateAValue(0), CalculateAValue(0) };
                    newlevelvalues.SpeedRandomValue = new List<RandomValuesDup>() { CalculateAValue(0), CalculateAValue(0), CalculateAValue(0) };
                    newlevelvalues.DexterityRandomValue = new List<RandomValuesDup>() { CalculateAValue(0), CalculateAValue(0), CalculateAValue(0) };
                    newlevelvalues.LuckRandomValue = new List<RandomValuesDup>() { CalculateAValue(0), CalculateAValue(0), CalculateAValue(0) };
                    levelValues.Add(newlevelvalues);
                }
            }
        }
    }

    public int GetHitValue(int target)
    {
#if UNITY_EDITOR

        if (DataScript.instance == null)
        {
            int randomvalue = 0;
            if (target > 50)
            {
                randomvalue = (UnityEngine.Random.Range(1, 101) + UnityEngine.Random.Range(1, 101)) / 2;
            }
            else
            {
                randomvalue = UnityEngine.Random.Range(1, 101);
            }
            return randomvalue;
        }

#endif
        if (hitvaluesindex >= HitValues.Count)
        {
            hitvaluesindex = 0;
        }
        int value = 0;
        if (target > 50)
        {
            value = HitValues[hitvaluesindex].TwoRNValue;
        }
        else
        {
            value = HitValues[hitvaluesindex].NormalValue;
        }
        hitvaluesindex++;
        return value;
    }

    public int GetCritValue(int target)
    {
#if UNITY_EDITOR

        if (DataScript.instance == null)
        {
            int randomvalue = 0;
            if (target > 50)
            {
                randomvalue = (UnityEngine.Random.Range(1, 101) + UnityEngine.Random.Range(1, 101)) / 2;
            }
            else
            {
                randomvalue = UnityEngine.Random.Range(1, 101);
            }
            return randomvalue;
        }

#endif
        if (CritValuesindex >= CritValues.Count)
        {
            CritValuesindex = 0;
        }
        int value = 0;
        if (target > 50)
        {
            value = CritValues[CritValuesindex].TwoRNValue;
        }
        else
        {
            value = CritValues[CritValuesindex].NormalValue;
        }
        CritValuesindex++;
        return value;
    }

    public int GetPersonalityValue(int target)
    {
#if UNITY_EDITOR

        if (DataScript.instance == null)
        {
            int randomvalue = 0;
            if (target > 50)
            {
                randomvalue = (UnityEngine.Random.Range(1, 101) + UnityEngine.Random.Range(1, 101)) / 2;
            }
            else
            {
                randomvalue = UnityEngine.Random.Range(1, 101);
            }
            return randomvalue;
        }

#endif
        if (personalityvaluesindex >= personalityValues.Count)
        {
            personalityvaluesindex = 0;
        }
        int value = 0;
        if (target > 50)
        {
            value = personalityValues[personalityvaluesindex].TwoRNValue;
        }
        else
        {
            value = personalityValues[personalityvaluesindex].NormalValue;
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

    private RandomValuesDup CalculateAValue(int SystemToUse = -1) // -1 is value determined by the use2RN bool, 0 is just one value, 1 is two values
    {

        return new RandomValuesDup { TwoRNValue = (UnityEngine.Random.Range(1, 101) + UnityEngine.Random.Range(1, 101)) / 2, NormalValue = UnityEngine.Random.Range(1, 101) };
    }


}
