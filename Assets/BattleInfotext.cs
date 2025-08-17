
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
        if(GridScript == null)
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

        if(attackTurnScript == null)
        {
            attackTurnScript = FindAnyObjectByType<AttackTurnScript>();
        }

        if (GridScript.GetSelectedUnitGameObject()!=null)
        {
            selectedunit = GridScript.GetSelectedUnitGameObject();
        }
        

        if ((GridScript.GetSelectedUnitGameObject() == null && GridScript.lockedmovementtiles.Count ==0) || battlecamera.incombat) {
            stringtoshow = string.Empty;
            Color color = transform.parent.GetComponent<Image>().color;
            color.a = 0f;
            transform.parent.GetComponent<Image>().color = color;
            Skilltext.transform.parent.gameObject.SetActive(false);
            SkillDescription.transform.parent.gameObject.SetActive(false);
            FindAnyObjectByType<EventSystem>().SetSelectedGameObject(null);
        }
        else
        {
            Character selectedunitCharacter = null;
            if (turnManger.currentlyplaying=="playable")
            {
                selectedunitCharacter = selectedunit.GetComponent<UnitScript>().UnitCharacteristics;
            }
            else if (turnManger.currentlyplaying == "enemy" && attackTurnScript.CurrentEnemy!=null)
            {
                selectedunit = attackTurnScript.CurrentEnemy;
                selectedunitCharacter = selectedunit.GetComponent<UnitScript>().UnitCharacteristics;
            }
            else if(attackTurnScript.CurrentOther!=null)
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
                
                stringtoshow = selectedunitCharacter.name + "       Level : " + selectedunitCharacter.level + "    Health : " + selectedunitCharacter.currentHP + " / " + selectedunitCharacter.stats.HP + "\nWeapon : " + selectedunit.GetComponent<UnitScript>().GetFirstWeapon().Name + " (" + selectedunit.GetComponent<UnitScript>().GetFirstWeapon().type + " "+ gradeletter+ ")";
                if (selectedunitCharacter.telekinesisactivated)
                {
                    stringtoshow += "\nTelekinesis : on";
                }
                else
                {
                    stringtoshow += "\nTelekinesis : off";
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
        for (int i=0; i<SkillButtonList.Count; i++)
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
            for (int i = 0; i < Mathf.Min(unit.EquipedSkills.Count,4); i++)
            {
                SkillButtonList[i+1].gameObject.SetActive(true);
                SkillButtonList[i+1].GetComponentInChildren<TextMeshProUGUI>().text = dataScript.SkillList[unit.EquipedSkills[i]].name;
                SkillButtonIDList.Add(dataScript.SkillList[unit.EquipedSkills[i]].ID);
            }
            for (int i = Mathf.Min(unit.EquipedSkills.Count, 4);i<4;i++)
            {
                SkillButtonList[i+1].gameObject.SetActive(false);
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

        if(unit.UnitSkill!=0 || unit.EquipedSkills.Count>0)
        {
            Skilltext.transform.parent.gameObject.SetActive(true);
        }
        else
        {
            Skilltext.transform.parent.gameObject.SetActive(false);
        }
    }
}
