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

    private DataScript DataScript;

    private MapInitializer MapInitializer;

    private int numberofunitstodeplay;

    private GridScript gridscript;

    private InputManager inputmanager;

    public List<GameObject> PreBattleMenuItems;

    public TextMeshProUGUI BattalionText;

    public TextMeshProUGUI UnitsDeployedText;

    public TextMeshProUGUI UnitDescription;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        DataScript = FindAnyObjectByType<DataScript>();
        MapInitializer = FindAnyObjectByType<MapInitializer>();
        gridscript = FindAnyObjectByType<GridScript>();
        inputmanager = FindAnyObjectByType<InputManager>();
        numberofunitstodeplay = MapInitializer.playablepos.Count;
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
        if(SceneManager.GetActiveScene().name== "TestMap")
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
        int numberofunits = 0;
        foreach (Character character in DataScript.PlayableCharacterList)
        {
            if (character.playableStats.deployunit)
            {
                numberofunits++;
            }

        }
        return numberofunits;
    }

    private void OrderUnits()
    {
        List<Character> newcharacterlist = new List<Character>();
        foreach (Character character in DataScript.PlayableCharacterList)
        {
            if (character.playableStats.deployunit)
            {
                newcharacterlist.Add(character);
                character.playableStats.deployunit = false;
            }

        }
        foreach (Character character in DataScript.PlayableCharacterList)
        {
            if (!newcharacterlist.Contains(character))
            {
                newcharacterlist.Add(character);
            }
        }
        DataScript.PlayableCharacterList = newcharacterlist;
    }

}
