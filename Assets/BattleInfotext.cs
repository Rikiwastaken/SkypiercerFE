using TMPro;
using UnityEngine;
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TMP = GetComponent<TextMeshProUGUI>();
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


    private void ManagedSkillVisuals(Character unit)
    {
        DataScript dataScript = FindAnyObjectByType<DataScript>();
        bool showtext = false;
        string text = "Skills :\n";
        if(unit.UnitSkill!=0)
        {
            text += "- " + dataScript.SkillList[unit.UnitSkill].name+"\n";
            showtext = true;
        }
        foreach(int skillID in unit.EquipedSkills)
        {
            if(skillID !=0)
            {
                text += "- "+dataScript.SkillList[skillID].name + "\n";
                showtext = true;
            }
        }

        if(showtext)
        {
            if(Skilltext!=null)
            {
                Skilltext.text = text;
            }
        }
        else
        {
            Skilltext.text = "";
        }

        Skilltext.transform.parent.gameObject.SetActive(showtext);
    }
}
