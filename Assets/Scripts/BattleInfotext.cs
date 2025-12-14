
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnitScript;

public class BattleInfotext : MonoBehaviour
{

    private GridScript GridScript;

    private string stringtoshow;

    TextMeshProUGUI TMP;
    private GameObject selectedunit;

    private cameraScript battlecamera;
    private TurnManger turnManger;
    private AttackTurnScript attackTurnScript;
    public TextMeshProUGUI Skilltext;
    public List<Button> SkillButtonList;
    private List<int> SkillButtonIDList = new List<int>();
    public TextMeshProUGUI SkillDescription;
    public TextMeshProUGUI MasteryText;
    public List<Transform> MasteryExpBars;

    private InputManager inputManager;

    public GameObject ItemAction;

    public GameObject PreBattleMenu;
    public GameObject AttackMenu;

    public ActionsMenu ActionsMenu;

    public bool indescription;

    private TextBubbleScript textBubbleScript;

    public GameObject NeutralMenu;

    public GameObject ForeSightMenu;

    private EventSystem eventSystem;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TMP = GetComponent<TextMeshProUGUI>();
        inputManager = InputManager.instance;
        GridScript = GridScript.instance;
        textBubbleScript = FindAnyObjectByType<TextBubbleScript>();
        transform.parent.GetComponent<Image>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (textBubbleScript.indialogue || AttackMenu.activeSelf || NeutralMenu.activeSelf || ForeSightMenu.activeSelf)
        {
            GetComponent<TextMeshProUGUI>().enabled = false;
            transform.parent.GetComponent<Image>().enabled = false;
            if (MasteryText.transform.parent.gameObject.activeSelf)
            {
                MasteryText.transform.parent.gameObject.SetActive(false);
            }

        }
        else if (!GetComponent<TextMeshProUGUI>().enabled && (!PreBattleMenu.activeSelf || PreBattleMenu.GetComponent<PreBattleMenuScript>().ChangingUnitPlace))
        {
            GetComponent<TextMeshProUGUI>().enabled = true;
            transform.parent.GetComponent<Image>().enabled = true;
        }
        if ((AttackMenu.activeSelf || ItemAction.activeSelf || textBubbleScript.indialogue || NeutralMenu.activeSelf || ForeSightMenu.activeSelf))
        {
            if (transform.parent.GetChild(1).gameObject.activeSelf)
            {
                transform.parent.GetChild(1).gameObject.SetActive(false);
                transform.parent.GetChild(2).gameObject.SetActive(false);
            }

            return;
        }
        else if (!PreBattleMenu.activeSelf)
        {
            if (!transform.parent.GetChild(1).gameObject.activeSelf)
            {
                transform.parent.GetChild(1).gameObject.SetActive(true);
                transform.parent.GetChild(2).gameObject.SetActive(true);
            }

        }
        else
        {
            if (transform.parent.GetChild(1).gameObject.activeSelf)
            {
                transform.parent.GetChild(1).gameObject.SetActive(false);
                transform.parent.GetChild(2).gameObject.SetActive(false);
            }
        }

        if (battlecamera == null)
        {
            battlecamera = FindAnyObjectByType<cameraScript>();
        }

        if (eventSystem == null)
        {
            eventSystem = FindAnyObjectByType<EventSystem>();
        }

        if (turnManger == null)
        {
            turnManger = FindAnyObjectByType<TurnManger>();
        }

        if (attackTurnScript == null)
        {
            attackTurnScript = FindAnyObjectByType<AttackTurnScript>();
        }

        if (GridScript.GetSelectedUnitGameObject() != null)
        {
            selectedunit = GridScript.GetSelectedUnitGameObject();
        }


        if ((GridScript.GetSelectedUnitGameObject() == null && GridScript.lockedmovementtiles.Count == 0) || battlecamera.incombat || (PreBattleMenu.activeSelf && !PreBattleMenu.GetComponent<PreBattleMenuScript>().ChangingUnitPlace) || (!PreBattleMenu.activeSelf && GridScript.GetComponent<TurnManger>().currentlyplaying != "playable"))
        {

            //if(!Skilltext.transform.parent.gameObject.activeSelf)
            //{
            //    stringtoshow = string.Empty;
            //    Color color = transform.parent.GetComponent<Image>().color;
            //    color.a = 0f;
            //    transform.parent.GetComponent<Image>().color = color;
            //    Skilltext.transform.parent.gameObject.SetActive(false);
            //    SkillDescription.transform.parent.gameObject.SetActive(false);
            //    MasteryText.transform.parent.gameObject.SetActive(false);
            //}

            if (!(PreBattleMenu.activeSelf && !PreBattleMenu.GetComponent<PreBattleMenuScript>().ChangingUnitPlace) && !(GameOverScript.instance != null && GameOverScript.instance.gameObject.activeSelf))
            {
                eventSystem.SetSelectedGameObject(null);
            }

        }
        else
        {
            Character selectedunitCharacter = null;
            if (turnManger.currentlyplaying == "playable")
            {
                selectedunitCharacter = selectedunit.GetComponent<UnitScript>().UnitCharacteristics;
            }
            else if (turnManger.currentlyplaying == "enemy")
            {
                if (attackTurnScript.CurrentEnemy != null)
                {
                    selectedunit = attackTurnScript.CurrentEnemy;
                    selectedunitCharacter = selectedunit.GetComponent<UnitScript>().UnitCharacteristics;
                }
                else
                {
                    return;
                }

            }
            else if (attackTurnScript.CurrentOther != null)
            {
                selectedunit = attackTurnScript.CurrentOther;
                selectedunitCharacter = selectedunit.GetComponent<UnitScript>().UnitCharacteristics;
            }
            else if (turnManger.currentlyplaying == "")
            {
                selectedunit = GridScript.GetUnit(GridScript.selection);
                selectedunitCharacter = selectedunit.GetComponent<UnitScript>().UnitCharacteristics;

            }
            if (selectedunit != null && selectedunitCharacter != null)
            {
                string gradeletter = "E";
                int grade = selectedunit.GetComponent<UnitScript>().GetFirstWeapon().Grade;
                switch (grade)
                {
                    case 1:
                        gradeletter = "D";
                        break;
                    case 2:
                        gradeletter = "C";
                        break;
                    case 3:
                        gradeletter = "B";
                        break;
                    case 4:
                        gradeletter = "A";
                        break;
                    case 5:
                        gradeletter = "S";
                        break;
                }

                ManagedSkillVisuals(selectedunitCharacter);
                ManageSkillDescription();
                ManageMasteryVisuals(selectedunitCharacter);


                stringtoshow = " " + selectedunitCharacter.name + "\n";
                stringtoshow += " Level : " + selectedunitCharacter.level + "\n";
                stringtoshow += " Health : " + selectedunitCharacter.currentHP + " / " + selectedunitCharacter.AjustedStats.HP;
                if (selectedunitCharacter.enemyStats.RemainingLifebars > 0)
                {
                    stringtoshow += " x " + (selectedunitCharacter.enemyStats.RemainingLifebars + 1);
                }
                else
                {
                    stringtoshow += "\n";
                }
                if (selectedunitCharacter.affiliation == "playable")
                {
                    stringtoshow += " Exp : " + selectedunitCharacter.experience + " / 100\n\n";
                }
                else
                {
                    stringtoshow += "\n";
                }
                AllStatsSkillBonus statsmods = selectedunit.GetComponent<UnitScript>().GetStatSkillBonus(null);

                if (statsmods.Strength > 0)
                {
                    stringtoshow += " Strength : <color=#017a01>" + (selectedunitCharacter.AjustedStats.Strength + statsmods.Strength) + "</color>\n";
                }
                else if (statsmods.Strength < 0)
                {
                    stringtoshow += " Strength : <color=red>" + (selectedunitCharacter.AjustedStats.Strength + statsmods.Strength) + "</color>\n";
                }
                else
                {
                    stringtoshow += " Strength : " + selectedunitCharacter.AjustedStats.Strength + "\n";
                }
                if (statsmods.Psyche > 0)
                {
                    stringtoshow += " Psyche : <color=#017a01>" + (selectedunitCharacter.AjustedStats.Psyche + statsmods.Psyche) + "</color>\n";
                }
                else if (statsmods.Psyche < 0)
                {
                    stringtoshow += " Psyche : <color=red>" + (selectedunitCharacter.AjustedStats.Psyche + statsmods.Psyche) + "</color>\n";
                }
                else
                {
                    stringtoshow += " Psyche : " + selectedunitCharacter.AjustedStats.Psyche + "\n";
                }

                if (statsmods.Defense > 0)
                {
                    stringtoshow += " Defense : <color=#017a01>" + (selectedunitCharacter.AjustedStats.Defense + statsmods.Defense) + "</color>\n";
                }
                else if (statsmods.Defense < 0)
                {
                    stringtoshow += " Defense : <color=red>" + (selectedunitCharacter.AjustedStats.Defense + statsmods.Defense) + "</color>\n";
                }
                else
                {
                    stringtoshow += " Defense : " + selectedunitCharacter.AjustedStats.Defense + "\n";
                }

                if (statsmods.Resistance > 0)
                {
                    stringtoshow += " Resistance : <color=#017a01>" + (selectedunitCharacter.AjustedStats.Resistance + statsmods.Resistance) + "</color>\n";
                }
                else if (statsmods.Resistance < 0)
                {
                    stringtoshow += " Resistance : <color=red>" + (selectedunitCharacter.AjustedStats.Resistance + statsmods.Resistance) + "</color>\n";
                }
                else
                {
                    stringtoshow += " Resistance : " + selectedunitCharacter.AjustedStats.Resistance + "\n";
                }

                if (statsmods.Dexterity > 0)
                {
                    stringtoshow += " Dexterity : <color=#017a01>" + (selectedunitCharacter.AjustedStats.Dexterity + statsmods.Dexterity) + "</color>\n";
                }
                else if (statsmods.Dexterity < 0)
                {
                    stringtoshow += " Dexterity : <color=red>" + (selectedunitCharacter.AjustedStats.Dexterity + statsmods.Dexterity) + "</color>\n";
                }
                else
                {
                    stringtoshow += " Dexterity : " + selectedunitCharacter.AjustedStats.Dexterity + "\n";
                }

                if (statsmods.Speed > 0)
                {
                    stringtoshow += " Speed : <color=#017a01>" + (selectedunitCharacter.AjustedStats.Speed + statsmods.Speed) + "</color>\n\n";
                }
                else if (statsmods.Speed < 0)
                {
                    stringtoshow += " Speed : <color=red>" + (selectedunitCharacter.AjustedStats.Speed + statsmods.Speed) + "</color>\n\n";
                }
                else
                {
                    stringtoshow += " Speed : " + selectedunitCharacter.AjustedStats.Speed + "\n";
                }

                string weapontype = selectedunit.GetComponent<UnitScript>().GetFirstWeapon().type;
                if (selectedunit.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "greatsword")
                {
                    weapontype = "Gr.Sword";
                }



                stringtoshow += "\nWeapon : " + selectedunit.GetComponent<UnitScript>().GetFirstWeapon().Name + " (" + weapontype + " " + gradeletter + ")  " + selectedunit.GetComponent<UnitScript>().GetFirstWeapon().Currentuses + " / " + selectedunit.GetComponent<UnitScript>().GetFirstWeapon().Maxuses + "\n";

                int BaseDamage = ActionsMenu.CalculateDamage(selectedunit);

                stringtoshow += "\nBaseDmg: " + BaseDamage + "\nMvt: " + selectedunitCharacter.movements + "\n\n";


                if (selectedunitCharacter.telekinesisactivated)
                {
                    stringtoshow += "Telekinesis : on";
                }
                else
                {
                    stringtoshow += "Telekinesis : off";
                }
                Color color = transform.parent.GetComponent<Image>().color;
                color.a = 0.8f;
                transform.parent.GetComponent<Image>().color = color;
            }
            else
            {
                if (!Skilltext.transform.parent.gameObject.activeSelf)
                {
                    stringtoshow = string.Empty;
                    Color color = transform.parent.GetComponent<Image>().color;
                    color.a = 0f;
                    transform.parent.GetComponent<Image>().color = color;
                    Skilltext.transform.parent.gameObject.SetActive(false);
                    SkillDescription.transform.parent.gameObject.SetActive(false);
                    MasteryText.transform.parent.gameObject.SetActive(false);
                }
                stringtoshow = "";
            }

        }
        TMP.text = stringtoshow;
    }

    private void ManageSkillDescription()
    {
        if (inputManager.ShowDetailsjustpressed && SkillButtonIDList.Count > 0)
        {
            SkillButtonList[0].Select();
        }
        if (inputManager.canceljustpressed && SkillButtonIDList.Count > 0)
        {
            eventSystem.SetSelectedGameObject(null);
        }
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
        for (int i = 0; i < SkillButtonList.Count; i++)
        {
            if (SkillButtonList[i].gameObject == currentSelected)
            {
                SkillDescription.text = DataScript.instance.SkillList[SkillButtonIDList[i]].Descriptions;
                if (!SkillDescription.transform.parent.gameObject.activeSelf)
                {
                    SkillDescription.transform.parent.gameObject.SetActive(true);
                }

                GridScript.movementbuffercounter = 5;
                return;
            }
        }
        if (SkillDescription.transform.parent.gameObject.activeSelf)
        {
            SkillDescription.transform.parent.gameObject.SetActive(false);
        }
    }

    private void ManageMasteryVisuals(Character unit)
    {
        if (unit.affiliation == "playable")
        {
            if (MasteryText.transform.parent.gameObject.activeSelf == false)
            {
                MasteryText.transform.parent.gameObject.SetActive(true);
            }

        }
        else
        {
            if (MasteryText.transform.parent.gameObject.activeSelf)
            {
                MasteryText.transform.parent.gameObject.SetActive(false);
            }

            return;
        }

        MasteryText.text = "";

        List<WeaponMastery> masteries = unit.Masteries;
        int barID = 0;
        for (int i = 0; i < masteries.Count; i++)
        {
            if (MasteryExpBars[i].gameObject.activeSelf == false)
            {
                MasteryExpBars[i].gameObject.SetActive(true);
            }
            DataScript ds = DataScript.instance;
            string masterylevel = "";
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
            if (MasteryExpBars[i].gameObject.activeSelf)
            {
                MasteryExpBars[i].gameObject.SetActive(false);
            }

        }

    }
    private void ManagedSkillVisuals(Character unit)
    {
        SkillButtonIDList = new List<int>();
        if (unit.UnitSkill != 0)
        {
            if (!SkillButtonList[0].gameObject.activeSelf)
            {
                SkillButtonList[0].gameObject.SetActive(true);
            }

            DataScript.Skill unitskill = GetSkill(unit.UnitSkill);

            SkillButtonList[0].GetComponentInChildren<TextMeshProUGUI>().text = unitskill.name;
            SkillButtonIDList.Add(unitskill.ID);
            for (int i = 0; i < Mathf.Min(unit.EquipedSkills.Count, 4); i++)
            {
                if (!SkillButtonList[i + 1].gameObject.activeSelf)
                {
                    SkillButtonList[i + 1].gameObject.SetActive(true);
                }

                DataScript.Skill equipedskill = GetSkill(unit.EquipedSkills[i]);

                SkillButtonList[i + 1].GetComponentInChildren<TextMeshProUGUI>().text = equipedskill.name;
                SkillButtonIDList.Add(equipedskill.ID);
            }
            for (int i = Mathf.Min(unit.EquipedSkills.Count, 4); i < 4; i++)
            {
                if (SkillButtonList[i + 1].gameObject.activeSelf)
                {
                    SkillButtonList[i + 1].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            for (int i = 0; i < Mathf.Min(unit.EquipedSkills.Count, 4); i++)
            {
                if (!SkillButtonList[i].gameObject.activeSelf)
                {
                    SkillButtonList[i].gameObject.SetActive(true);
                }

                DataScript.Skill equipedskill = GetSkill(unit.EquipedSkills[i]);

                SkillButtonList[i].GetComponentInChildren<TextMeshProUGUI>().text = equipedskill.name;
                SkillButtonIDList.Add(equipedskill.ID);
            }
            for (int i = Mathf.Min(unit.EquipedSkills.Count, 4); i < 4; i++)
            {
                if (SkillButtonList[i].gameObject.activeSelf)
                {
                    SkillButtonList[i].gameObject.SetActive(false);
                }

            }
        }

        if (unit.UnitSkill != 0 || unit.EquipedSkills.Count > 0)
        {
            if (!Skilltext.transform.parent.gameObject.activeSelf)
            {
                Skilltext.transform.parent.gameObject.SetActive(true);
            }
        }
        else
        {
            if (Skilltext.transform.parent.gameObject.activeSelf)
            {
                Skilltext.transform.parent.gameObject.SetActive(false);
            }

        }
    }

    private DataScript.Skill GetSkill(int skillID)
    {
        DataScript.Skill unitskill = null;

        foreach (DataScript.Skill skill in DataScript.instance.SkillList)
        {
            if (skill.ID == skillID)
            {
                unitskill = skill;
                break;
            }
        }
        return unitskill;
    }
}
