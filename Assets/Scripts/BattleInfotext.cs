
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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

    private battlecameraScript battlecamera;
    private TurnManger turnManger;
    private AttackTurnScript attackTurnScript;
    public TextMeshProUGUI Skilltext;
    public List<Button> SkillButtonList;
    private List<int> SkillButtonIDList = new List<int>();
    public TextMeshProUGUI SkillDescription;

    private DataScript dataScript;

    private InputManager inputManager;

    private GridScript gridScript;

    public GameObject Attackwindows;
    public GameObject ItemAction;

    public GameObject PreBattleMenu;

    public bool indescription;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TMP = GetComponent<TextMeshProUGUI>();
        dataScript = FindAnyObjectByType<DataScript>();
        inputManager = FindAnyObjectByType<InputManager>();
        gridScript = FindAnyObjectByType<GridScript>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (Attackwindows.activeSelf || ItemAction.activeSelf)
        {
            transform.parent.GetChild(1).gameObject.SetActive(false);
            transform.parent.GetChild(2).gameObject.SetActive(false);
            return;
        }
        else if (!PreBattleMenu.activeSelf)
        {
            transform.parent.GetChild(1).gameObject.SetActive(true);
            transform.parent.GetChild(2).gameObject.SetActive(true);
        }
        else
        {
            transform.parent.GetChild(1).gameObject.SetActive(false);
            transform.parent.GetChild(2).gameObject.SetActive(false);
        }

        if (GridScript == null)
        {
            GridScript = FindAnyObjectByType<GridScript>();
        }

        if (battlecamera == null)
        {
            battlecamera = FindAnyObjectByType<battlecameraScript>();
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


        if ((GridScript.GetSelectedUnitGameObject() == null && GridScript.lockedmovementtiles.Count == 0) || battlecamera.incombat || (PreBattleMenu.activeSelf && !PreBattleMenu.GetComponent<PreBattleMenuScript>().ChangingUnitPlace))
        {
            stringtoshow = string.Empty;
            Color color = transform.parent.GetComponent<Image>().color;
            color.a = 0f;
            transform.parent.GetComponent<Image>().color = color;
            Skilltext.transform.parent.gameObject.SetActive(false);
            SkillDescription.transform.parent.gameObject.SetActive(false);
            if (!(PreBattleMenu.activeSelf && !PreBattleMenu.GetComponent<PreBattleMenuScript>().ChangingUnitPlace))
            {
                FindAnyObjectByType<EventSystem>().SetSelectedGameObject(null);
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
                if(attackTurnScript.CurrentEnemy != null)
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
            if (selectedunit!=null && selectedunitCharacter!=null)
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


                stringtoshow =" " +selectedunitCharacter.name + "\n";
                stringtoshow += " Level : " + selectedunitCharacter.level + "\n";
                stringtoshow += " Health : " + selectedunitCharacter.currentHP + " / " + selectedunitCharacter.stats.HP;
                if(selectedunitCharacter.enemyStats.RemainingLifebars>0)
                {
                    stringtoshow += " x " + (selectedunitCharacter.enemyStats.RemainingLifebars+1);
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
                    stringtoshow += " Strength : <color=#017a01>" + (selectedunitCharacter.stats.Strength + statsmods.Strength) + "</color>\n";
                }
                else if (statsmods.Strength < 0)
                {
                    stringtoshow += " Strength : <color=red>" + (selectedunitCharacter.stats.Strength + statsmods.Strength) + "</color>\n";
                }
                else
                {
                    stringtoshow += " Strength : " + selectedunitCharacter.stats.Strength + "\n";
                }
                if (statsmods.Psyche > 0)
                {
                    stringtoshow += " Psyche : <color=#017a01>" + (selectedunitCharacter.stats.Psyche + statsmods.Psyche) + "</color>\n";
                }
                else if (statsmods.Psyche < 0)
                {
                    stringtoshow += " Psyche : <color=red>" + (selectedunitCharacter.stats.Psyche + statsmods.Psyche) + "</color>\n";
                }
                else
                {
                    stringtoshow += " Psyche : " + selectedunitCharacter.stats.Psyche + "\n";
                }

                if (statsmods.Defense > 0)
                {
                    stringtoshow += " Defense : <color=#017a01>" + (selectedunitCharacter.stats.Defense + statsmods.Defense) + "</color>\n";
                }
                else if (statsmods.Defense < 0)
                {
                    stringtoshow += " Defense : <color=red>" + (selectedunitCharacter.stats.Defense + statsmods.Defense) + "</color>\n";
                }
                else
                {
                    stringtoshow += " Defense : " + selectedunitCharacter.stats.Defense + "\n";
                }

                if (statsmods.Resistance > 0)
                {
                    stringtoshow += " Resistance : <color=#017a01>" + (selectedunitCharacter.stats.Resistance + statsmods.Resistance) + "</color>\n";
                }
                else if (statsmods.Resistance < 0)
                {
                    stringtoshow += " Resistance : <color=red>" + (selectedunitCharacter.stats.Resistance + statsmods.Resistance) + "</color>\n";
                }
                else
                {
                    stringtoshow += " Resistance : " + selectedunitCharacter.stats.Resistance + "\n";
                }

                if (statsmods.Dexterity > 0)
                {
                    stringtoshow += " Dexterity : <color=#017a01>" + (selectedunitCharacter.stats.Dexterity + statsmods.Dexterity) + "</color>\n";
                }
                else if (statsmods.Dexterity < 0)
                {
                    stringtoshow += " Dexterity : <color=red>" + (selectedunitCharacter.stats.Dexterity + statsmods.Dexterity) + "</color>\n";
                }
                else
                {
                    stringtoshow += " Dexterity : " + selectedunitCharacter.stats.Dexterity + "\n";
                }

                if (statsmods.Speed > 0)
                {
                    stringtoshow += " Speed : <color=#017a01>" + (selectedunitCharacter.stats.Speed + statsmods.Speed) + "</color>\n\n";
                }
                else if (statsmods.Speed < 0)
                {
                    stringtoshow += " Speed : <color=red>" + (selectedunitCharacter.stats.Speed + statsmods.Speed) + "</color>\n\n";
                }
                else
                {
                    stringtoshow += " Speed : " + selectedunitCharacter.stats.Speed + "\n";
                }

                string weapontype = selectedunit.GetComponent<UnitScript>().GetFirstWeapon().type;
                if(selectedunit.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower()=="greatsword")
                {
                    weapontype = "Gr.Sword";
                }

                stringtoshow += "\nWeapon : " + selectedunit.GetComponent<UnitScript>().GetFirstWeapon().Name + " (" + weapontype + " " + gradeletter + ")  "+ selectedunit.GetComponent<UnitScript>().GetFirstWeapon().Currentuses + " / "+ selectedunit.GetComponent<UnitScript>().GetFirstWeapon().Maxuses + "\n";
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
            FindAnyObjectByType<EventSystem>().SetSelectedGameObject(null);
        }
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
        for (int i = 0; i < SkillButtonList.Count; i++)
        {
            if (SkillButtonList[i].gameObject == currentSelected)
            {
                SkillDescription.text = dataScript.SkillList[SkillButtonIDList[i]].Descriptions;
                SkillDescription.transform.parent.gameObject.SetActive(true);
                gridScript.movementbuffercounter = 5;
                return;
            }
        }
        SkillDescription.transform.parent.gameObject.SetActive(false);
    }
    private void ManagedSkillVisuals(Character unit)
    {
        SkillButtonIDList = new List<int>();
        if (unit.UnitSkill != 0)
        {
            SkillButtonList[0].gameObject.SetActive(true);
            SkillButtonList[0].GetComponentInChildren<TextMeshProUGUI>().text = dataScript.SkillList[unit.UnitSkill].name;
            SkillButtonIDList.Add(dataScript.SkillList[unit.UnitSkill].ID);
            for (int i = 0; i < Mathf.Min(unit.EquipedSkills.Count, 4); i++)
            {
                SkillButtonList[i + 1].gameObject.SetActive(true);
                SkillButtonList[i + 1].GetComponentInChildren<TextMeshProUGUI>().text = dataScript.SkillList[unit.EquipedSkills[i]].name;
                SkillButtonIDList.Add(dataScript.SkillList[unit.EquipedSkills[i]].ID);
            }
            for (int i = Mathf.Min(unit.EquipedSkills.Count, 4); i < 4; i++)
            {
                SkillButtonList[i + 1].gameObject.SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < Mathf.Min(unit.EquipedSkills.Count, 4); i++)
            {
                SkillButtonList[i].gameObject.SetActive(true);
                SkillButtonList[i].GetComponentInChildren<TextMeshProUGUI>().text = dataScript.SkillList[unit.EquipedSkills[i]].name;
                SkillButtonIDList.Add(dataScript.SkillList[unit.EquipedSkills[unit.EquipedSkills[i]]].ID);
            }
            for (int i = Mathf.Min(unit.EquipedSkills.Count, 4); i < 4; i++)
            {
                SkillButtonList[i].gameObject.SetActive(false);
            }
        }

        if (unit.UnitSkill != 0 || unit.EquipedSkills.Count > 0)
        {
            Skilltext.transform.parent.gameObject.SetActive(true);
        }
        else
        {
            Skilltext.transform.parent.gameObject.SetActive(false);
        }
    }
}
