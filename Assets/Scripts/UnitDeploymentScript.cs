using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnitScript;

public class UnitDeploymentScript : MonoBehaviour
{

    private MapInitializer MapInitializer;

    private int numberofunitstodeplay;

    private List<int> forcedunits;

    private GridScript gridscript;

    private DataScript DataScript;

    private InputManager inputmanager;

    public List<GameObject> PreBattleMenuItems;

    public TextMeshProUGUI BattalionText;

    public TextMeshProUGUI UnitsDeployedText;

    public TextMeshProUGUI UnitDescription;

    public Transform Mastery;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        DataScript = DataScript.instance;
        MapInitializer = FindAnyObjectByType<MapInitializer>();
        gridscript = GridScript.instance;
        inputmanager = InputManager.instance;
        numberofunitstodeplay = MapInitializer.playablepos.Count;
        forcedunits = MapInitializer.ForcedCharacters;
        InitializeButtons();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        gridscript.movementbuffercounter = 3;

        if (inputmanager.canceljustpressed)
        {
            if (numberofSelectedUnits() > 0)
            {
                foreach (GameObject go in PreBattleMenuItems)
                {
                    go.SetActive(true);
                }
                gameObject.SetActive(false);
                gridscript.InitializeGOList();
                return;
            }
        }



        GameObject currentselected = EventSystem.current.currentSelectedGameObject;
        bool buttonselected = false;
        if (currentselected != null)
        {
            for (int i = 0; i < 20; i++)
            {
                if (transform.GetChild(i).gameObject == currentselected)
                {
                    buttonselected = true; break;
                }
            }
        }
        if (!buttonselected || currentselected == null)
        {

            EventSystem.current.SetSelectedGameObject(transform.GetChild(0).gameObject);
        }
        Character currentchar = EventSystem.current.currentSelectedGameObject.GetComponent<UnitDeploymentButton>().Character;
        if (currentchar.name != "")
        {
            ManageMasteryVisuals(currentchar);
            string unitbattallion = currentchar.playableStats.battalion;
            BattalionText.text = "Battallion :\n" + unitbattallion + "\n Change with : ";
            string unitdescriptiontxt = currentchar.name + "\n";
            unitdescriptiontxt += "Level : " + currentchar.level + "\n";
            unitdescriptiontxt += "Exp : " + currentchar.experience + " / 100\n\n";

            unitdescriptiontxt += "Strength : " + currentchar.stats.Strength + "\n";
            unitdescriptiontxt += "Psyche : " + currentchar.stats.Psyche + "\n";
            unitdescriptiontxt += "Defense : " + currentchar.stats.Defense + "\n";
            unitdescriptiontxt += "Resistance : " + currentchar.stats.Resistance + "\n";
            unitdescriptiontxt += "Dexterity : " + currentchar.stats.Dexterity + "\n";
            unitdescriptiontxt += "Speed : " + currentchar.stats.Speed + "\n\n";

            string grade = "";
            if (currentchar.equipmentsIDs.Count > 0)
            {
                switch (DataScript.equipmentList[currentchar.equipmentsIDs[0]].Grade)
                {
                    case 0:
                        grade = "E";
                        break;
                    case 1:
                        grade = "D";
                        break;
                    case 2:
                        grade = "C";
                        break;
                    case 3:
                        grade = "B";
                        break;
                    case 4:
                        grade = "A";
                        break;
                    case 5:
                        grade = "S";
                        break;

                }
                unitdescriptiontxt += "Weapon : " + DataScript.equipmentList[currentchar.equipmentsIDs[0]].Name + " (" + DataScript.equipmentList[currentchar.equipmentsIDs[0]].type + " " + grade + ")";
            }
            else
            {
                grade = "E";
                unitdescriptiontxt += "Weapon : " + DataScript.equipmentList[0].Name + " (" + DataScript.equipmentList[0].type + " " + grade + ")";
            }
            UnitDescription.text = unitdescriptiontxt;
        }
        else
        {
            BattalionText.text = "";
            UnitDescription.text = "";
        }



        UnitsDeployedText.text = "Units deployed :\r\n" + numberofSelectedUnits() + " / " + numberofunitstodeplay;


    }



    private List<Character> InitializeCharactersToShow()
    {
        List<Character> characterstoshow = new List<Character>();
        if (SceneManager.GetActiveScene().name == "TestMap")
        {
            foreach (Character character in DataScript.PlayableCharacterList)
            {
                characterstoshow.Add(character);
            }
        }
        else
        {
            foreach (Character character in DataScript.PlayableCharacterList)
            {
                if (character.playableStats.unlocked)
                {
                    characterstoshow.Add(character);
                }
            }
        }

        return characterstoshow;
    }

    private void ManageMasteryVisuals(Character unit)
    {
        TextMeshProUGUI MasteryText = Mastery.GetComponentInChildren<TextMeshProUGUI>();
        if (unit.affiliation == "playable")
        {
            MasteryText.transform.parent.gameObject.SetActive(true);
        }
        else
        {
            MasteryText.transform.parent.gameObject.SetActive(false);
            return;
        }

        List<Transform> MasteryExpBars = new List<Transform>();
        for (int i = 1; i < Mastery.childCount; i++)
        {
            MasteryExpBars.Add(Mastery.GetChild(i).transform);
        }

        MasteryText.text = "";

        List<WeaponMastery> masteries = unit.Masteries;
        int barID = 0;
        for (int i = 0; i < masteries.Count; i++)
        {
            MasteryExpBars[i].gameObject.SetActive(true);
            string masterylevel = "";
            DataScript ds = DataScript.instance;
            switch (masteries[i].Level)
            {
                case (-1):
                    continue;
                case (0):
                    MasteryExpBars[barID].GetChild(0).GetComponent<Image>().fillAmount = (float)masteries[i].Exp / ds.MasteryforLevel0;
                    masterylevel = "X";
                    break;
                case (1):
                    masterylevel = "D";
                    MasteryExpBars[barID].GetChild(0).GetComponent<Image>().fillAmount = (float)masteries[i].Exp / ds.MasteryforLevel1;
                    break;
                case (2):
                    masterylevel = "C";
                    MasteryExpBars[barID].GetChild(0).GetComponent<Image>().fillAmount = (float)masteries[i].Exp / ds.MasteryforLevel2;
                    break;
                case (3):
                    masterylevel = "B";
                    MasteryExpBars[barID].GetChild(0).GetComponent<Image>().fillAmount = (float)masteries[i].Exp / ds.MasteryforLevel3;
                    break;
                case (4):
                    masterylevel = "A";
                    MasteryExpBars[barID].GetChild(0).GetComponent<Image>().fillAmount = 1f;
                    break;
            }
            barID++;
            MasteryText.text += masteries[i].weapontype[0] + (masteries[i].weapontype[1] + " : " + masterylevel + "\n");
        }
        for (int i = barID; i < MasteryExpBars.Count; i++)
        {
            MasteryExpBars[i].gameObject.SetActive(false);
        }

    }
    private void InitializeButtons()
    {
        OrderUnits();
        List<Character> characterstoshow = InitializeCharactersToShow();

        for (int i = 0; i < Mathf.Min(characterstoshow.Count, 20); i++)
        {
            transform.GetChild(i).GetComponent<UnitDeploymentButton>().Character = characterstoshow[i];
            transform.GetChild(i).GetComponent<UnitDeploymentButton>().CharacterID = i;
            if (i < numberofunitstodeplay)
            {
                transform.GetChild(i).GetComponent<UnitDeploymentButton>().Character.playableStats.deployunit = true;
            }
        }
        for (int i = characterstoshow.Count; i < Mathf.Min(characterstoshow.Count, 20); i++)
        {
            transform.GetChild(i).GetComponent<UnitDeploymentButton>().Character = null;
        }

    }

    private int numberofSelectedUnits()
    {
        bool intestmap = SceneManager.GetActiveScene().name == "TestMap";
        int numberofunits = 0;
        foreach (Character character in DataScript.PlayableCharacterList)
        {
            if (intestmap)
            {
                numberofunits++;
                continue;
            }
            if (character.playableStats.deployunit && character.playableStats.unlocked)
            {
                numberofunits++;
            }

        }
        return numberofunits;
    }

    private void OrderUnits()
    {
        bool intestmap = SceneManager.GetActiveScene().name == "TestMap";
        List<Character> newcharacterlist = new List<Character>();
        foreach (Character character in DataScript.PlayableCharacterList)
        {
            if (character.playableStats.deployunit && (character.playableStats.unlocked || intestmap))
            {
                newcharacterlist.Add(character);
                character.playableStats.deployunit = false;
            }

        }
        foreach (Character character in DataScript.PlayableCharacterList)
        {
            if (!newcharacterlist.Contains(character) && (character.playableStats.unlocked || intestmap))
            {
                newcharacterlist.Add(character);
            }
        }
        DataScript.PlayableCharacterList = newcharacterlist;
    }

}
