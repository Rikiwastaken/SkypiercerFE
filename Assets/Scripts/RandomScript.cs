using System;
using System.Collections.Generic;
using UnityEngine;
using static UnitScript;

public class RandomScript : MonoBehaviour
{

    [Header("Random Values")]
    [SerializeField]
    public List<List<int>> HitValues;
    [SerializeField]
    public int hitvaluesindex;
    [SerializeField]
    public List<List<int>> CritValues;
    [SerializeField]
    public int CritValuesindex;
    [SerializeField]
    public List<List<int>> personalityValues;
    [SerializeField]
    public int personalityvaluesindex;
    [SerializeField]
    public List<RandomLevelValues> levelValues;
    [SerializeField]
    public int levelvaluesindex;



    [Serializable]
    public class RandomLevelValues
    {
        public List<List<int>> HPRandomValue;
        public List<List<int>> StrengthRandomValue;
        public List<List<int>> PsycheRandomValue;
        public List<List<int>> DefenseRandomValue;
        public List<List<int>> ResistanceRandomValue;
        public List<List<int>> SpeedRandomValue;
        public List<List<int>> DexterityRandomValue;
        public List<List<int>> LuckRandomValue;
    }
    [SerializeField]
    public List<RandomLevelValues> LevelValues;
    [SerializeField]
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
                    newlevelvalues.HPRandomValue = new List<List<int>>() { CalculateAValue(0), CalculateAValue(0), CalculateAValue(0) };
                    newlevelvalues.StrengthRandomValue = new List<List<int>>() { CalculateAValue(0), CalculateAValue(0), CalculateAValue(0) };
                    newlevelvalues.PsycheRandomValue = new List<List<int>>() { CalculateAValue(0), CalculateAValue(0), CalculateAValue(0) };
                    newlevelvalues.DefenseRandomValue = new List<List<int>>() { CalculateAValue(0), CalculateAValue(0), CalculateAValue(0) };
                    newlevelvalues.ResistanceRandomValue = new List<List<int>>() { CalculateAValue(0), CalculateAValue(0), CalculateAValue(0) };
                    newlevelvalues.SpeedRandomValue = new List<List<int>>() { CalculateAValue(0), CalculateAValue(0), CalculateAValue(0) };
                    newlevelvalues.DexterityRandomValue = new List<List<int>>() { CalculateAValue(0), CalculateAValue(0), CalculateAValue(0) };
                    newlevelvalues.LuckRandomValue = new List<List<int>>() { CalculateAValue(0), CalculateAValue(0), CalculateAValue(0) };
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
            value = HitValues[hitvaluesindex][0];
        }
        else
        {
            value = HitValues[hitvaluesindex][1];
        }
        hitvaluesindex++;
        Debug.Log(value);
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

    private List<int> CalculateAValue(int SystemToUse = -1) // -1 is value determined by the use2RN bool, 0 is just one value, 1 is two values
    {

        bool use2RNSystem = use2RN;
        switch (SystemToUse)
        {

            case 0:
                use2RNSystem = false;
                break;
            case 1:
                use2RNSystem = true;
                break;
        }

        if (use2RNSystem)
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
