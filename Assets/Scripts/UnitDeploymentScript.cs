using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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

    public List<GameObject> PreBattleMenuItems;

    public TextMeshProUGUI BattalionText;

    public TextMeshProUGUI UnitsDeployedText;

    public TextMeshProUGUI UnitDescription;

    public Transform Mastery;

    public List<TextMeshProUGUI> MasteryTexts;
    public List<Image> MasteryImages;
    public List<Transform> MasteryExpBars;
    public List<Sprite> WeaponClassImages;

    private InputAction _CancelAction;

    private List<Character> DeployableUnitList;

    public TextMeshProUGUI SortTMP;

    private int skillwindowindex;

    public List<GameObject> topbuttons;
    public List<GameObject> bottombuttons;

    private InputAction _TelekinesisInputAction;

    private int CurrentSort = 0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        DataScript = DataScript.instance;
        MapInitializer = FindAnyObjectByType<MapInitializer>();
        gridscript = GridScript.instance;
        numberofunitstodeplay = MapInitializer.playablepos.Count;
        forcedunits = MapInitializer.ForcedCharacters;

        Debug.Log("forced units :");
        foreach (int i in forcedunits)
        {
            Debug.Log($"{i}");
        }
        OrderUnits();
        List<Character> characterstoshow = InitializeCharactersToShow();
        InitializeButtons(characterstoshow, true);
    }

    private void Start()
    {
        _CancelAction = InputSystem.actions.FindAction("Cancel");
        _TelekinesisInputAction = InputSystem.actions.FindAction("TelekinesisToggle");
    }

    // Update is called once per frame
    void Update()
    {
        gridscript.movementbuffercounter = 3;

        if (_CancelAction.WasPressedThisFrame())
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


        //Change sort
        if (_TelekinesisInputAction.WasPressedThisFrame())
        {

            if (CurrentSort < 5)
            {
                CurrentSort++;

            }
            else
            {
                CurrentSort = 0;
            }

            SortUnits(CurrentSort);
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

            unitdescriptiontxt += "Strength : " + currentchar.AjustedStats.Strength + "\n";
            unitdescriptiontxt += "Psyche : " + currentchar.AjustedStats.Psyche + "\n";
            unitdescriptiontxt += "Defense : " + currentchar.AjustedStats.Defense + "\n";
            unitdescriptiontxt += "Resistance : " + currentchar.AjustedStats.Resistance + "\n";
            unitdescriptiontxt += "Dexterity : " + currentchar.AjustedStats.Dexterity + "\n";
            unitdescriptiontxt += "Speed : " + currentchar.AjustedStats.Speed + "\n\n";

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
            foreach (Character character in DeployableUnitList)
            {
                character.playableStats.unlocked = true;
            }
        }

        foreach (Character character in DeployableUnitList)
        {
            if (forcedunits.Contains(character.ID))
            {
                characterstoshow.Add(character);
                character.playableStats.unlocked = true;
            }
            else if (character.playableStats.unlocked)
            {
                characterstoshow.Add(character);
            }
        }


        return characterstoshow;
    }

    private void ManageMasteryVisuals(Character unit)
    {
        if (unit.affiliation == "playable")
        {
            if (MasteryTexts[0].transform.parent.gameObject.activeSelf == false)
            {
                MasteryTexts[0].transform.parent.gameObject.SetActive(true);
            }

        }
        else
        {
            if (MasteryTexts[0].transform.parent.gameObject.activeSelf)
            {
                MasteryTexts[0].transform.parent.gameObject.SetActive(false);
            }

            return;
        }

        List<WeaponMastery> masteries = unit.Masteries;
        int barID = 0;
        for (int i = 0; i < masteries.Count; i++)
        {
            if (MasteryExpBars == null || MasteryExpBars.Count <= i || MasteryExpBars[i] == null)
            {
                continue;
            }
            if (MasteryTexts == null || MasteryTexts.Count <= i || MasteryTexts[i] == null)
            {
                continue;
            }
            if (MasteryImages == null || MasteryImages.Count <= i || MasteryImages[i] == null)
            {
                continue;
            }

            if (!MasteryExpBars[i].parent.gameObject.activeSelf)
            {
                MasteryExpBars[i].parent.gameObject.SetActive(true);
            }
            DataScript ds = DataScript.instance;
            string masterylevel = "";

            switch (masteries[i].Level)
            {
                case (-1):
                    continue;
                case (0):
                    MasteryExpBars[barID].GetComponent<Image>().fillAmount = (float)masteries[i].Exp / ds.MasteryforLevel0;
                    masterylevel = "X";
                    break;
                case (1):
                    masterylevel = "D";
                    MasteryExpBars[barID].GetComponent<Image>().fillAmount = (float)masteries[i].Exp / ds.MasteryforLevel1;
                    break;
                case (2):
                    masterylevel = "C";
                    MasteryExpBars[barID].GetComponent<Image>().fillAmount = (float)masteries[i].Exp / ds.MasteryforLevel2;
                    break;
                case (3):
                    masterylevel = "B";
                    MasteryExpBars[barID].GetComponent<Image>().fillAmount = (float)masteries[i].Exp / ds.MasteryforLevel3;
                    break;
                case (4):
                    masterylevel = "A";
                    MasteryExpBars[barID].GetComponent<Image>().fillAmount = 1f;
                    break;
            }

            MasteryImages[barID].sprite = GetWeaponIcons(masteries[i].weapontype);
            MasteryTexts[barID].text = masterylevel;


            barID++;

        }
        for (int i = barID; i < MasteryExpBars.Count; i++)
        {
            if (MasteryExpBars[i].parent.gameObject.activeSelf)
            {
                MasteryExpBars[i].parent.gameObject.SetActive(false);
            }


        }

    }

    private Sprite GetWeaponIcons(string weapontype)
    {

        switch (weapontype.ToLower())
        {
            default:
                return WeaponClassImages[0];
            case "sword":
                return WeaponClassImages[1];
            case "spear":
                return WeaponClassImages[2];
            case "greatsword":
                return WeaponClassImages[3];
            case "bow":
                return WeaponClassImages[4];
            case "scythe":
                return WeaponClassImages[5];
            case "shield":
                return WeaponClassImages[6];
            case "staff":
                return WeaponClassImages[7];
            case "dagger":
                return WeaponClassImages[8];
        }
    }


    private void InitializeButtons(List<Character> characterstoshow, bool firstactivation = false)
    {


        for (int i = 0; i < Mathf.Min(characterstoshow.Count, 20); i++)
        {
            transform.GetChild(i).GetComponent<UnitDeploymentButton>().Character = characterstoshow[i];
            transform.GetChild(i).GetComponent<UnitDeploymentButton>().CharacterID = i;
            if (firstactivation)
            {
                if (forcedunits.Contains(characterstoshow[i].ID))
                {

                    transform.GetChild(i).GetComponent<UnitDeploymentButton>().Character.playableStats.deployunit = true;
                }
                else
                {
                    transform.GetChild(i).GetComponent<UnitDeploymentButton>().Character.playableStats.deployunit = false;
                }
            }

        }
        if (firstactivation)
        {
            int remainingcharacterstoplace = numberofunitstodeplay - forcedunits.Count;

            for (int i = 0; i < Mathf.Min(characterstoshow.Count, 20); i++)
            {
                if (remainingcharacterstoplace > 0 && !transform.GetChild(i).GetComponent<UnitDeploymentButton>().Character.playableStats.deployunit)
                {
                    transform.GetChild(i).GetComponent<UnitDeploymentButton>().Character.playableStats.deployunit = true;
                    remainingcharacterstoplace--;
                }
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
        foreach (Character character in DeployableUnitList)
        {
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
            if (!newcharacterlist.Contains(character))
            {
                newcharacterlist.Add(character);
            }
        }
        DeployableUnitList = newcharacterlist;
    }


    private void SortUnits(int type)
    {
        switch (type)
        {
            case 0: // alphabetically
                DeployableUnitList.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));
                SortTMP.text = "Sort: A...Z";
                break;
            case 1: // reverse alphabetically
                DeployableUnitList.Sort((a, b) => string.Compare(b.name, a.name, StringComparison.OrdinalIgnoreCase));
                SortTMP.text = "Sort: Z...A";
                break;
            case 2: // Forced first
                DeployableUnitList.Sort((a, b) =>
                {
                    // Forced first
                    int commandCompare = forcedunits.Contains(b.ID).CompareTo(forcedunits.Contains(a.ID));
                    if (commandCompare != 0)
                        return commandCompare;

                    // Then alphabetical
                    return string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase);
                });
                SortTMP.text = "Sort: Forced Units First";
                break;
            case 3: //Zack's battalion
                OrderByBattalion("zack");
                SortTMP.text = "Sort: Zack's Battallion First";
                break;
            case 4: //Kira's Battallio
                OrderByBattalion("kira");
                SortTMP.text = "Sort: Kira's Battallion First";
                break;
            case 5: // Gale's Battallion
                OrderByBattalion("gale");
                SortTMP.text = "Sort: Gale's Battallion First";
                break;

        }
        skillwindowindex = 0;
        InitializeButtons(InitializeCharactersToShow());
        EventSystem.current.SetSelectedGameObject(topbuttons[0]);
    }

    private void OrderByBattalion(string battalion)
    {

        //First order alphabettically
        DeployableUnitList.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));


        // Then get all units of battallion
        List<Character> OrderedCharacterList = new List<Character>();
        List<Character> characternotfrombattallion = new List<Character>();
        foreach (Character unit in DeployableUnitList)
        {
            if (unit.playableStats.battalion.ToLower() == battalion.ToLower())
            {
                OrderedCharacterList.Add(unit);
            }
            else
            {
                characternotfrombattallion.Add(unit);
            }
        }

        // Then add all remaining units
        foreach (Character unit in characternotfrombattallion)
        {
            OrderedCharacterList.Add(unit);
        }

        DeployableUnitList = OrderedCharacterList;
    }



}
