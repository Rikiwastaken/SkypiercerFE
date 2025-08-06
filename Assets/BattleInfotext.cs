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
    private EnemyTurnScript enemyTurnScript;

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

        if(enemyTurnScript == null)
        {
            enemyTurnScript = FindAnyObjectByType<EnemyTurnScript>();
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
        }
        else
        {
            Character selectedunitCharacter = null;
            if (turnManger.currentlyplaying=="playable")
            {
                selectedunitCharacter = selectedunit.GetComponent<UnitScript>().UnitCharacteristics;
            }
            else if (turnManger.currentlyplaying == "enemy" && enemyTurnScript.CurrentEnemy!=null)
            {
                selectedunit = enemyTurnScript.CurrentEnemy;
                selectedunitCharacter = selectedunit.GetComponent<UnitScript>().UnitCharacteristics;
            }
            else if(enemyTurnScript.CurrentOther!=null)
            {
                selectedunit = enemyTurnScript.CurrentOther;
                selectedunitCharacter = selectedunit.GetComponent<UnitScript>().UnitCharacteristics;
            }
            if(selectedunit!=null && selectedunitCharacter!=null)
            {
                stringtoshow = selectedunitCharacter.name + "       Level : " + selectedunitCharacter.level + "    Health : " + selectedunitCharacter.currentHP + " / " + selectedunitCharacter.stats.HP + "\nWeapon : " + selectedunit.GetComponent<UnitScript>().GetFirstWeapon().Name + " (" + selectedunit.GetComponent<UnitScript>().GetFirstWeapon().type + ")";
                if (selectedunitCharacter.telekinesisactivated)
                {
                    stringtoshow += "\nTelekinesis : on";
                }
                else
                {
                    stringtoshow += "\nTelekinesis : off";
                }
                Color color = transform.parent.GetComponent<Image>().color;
                color.a = 1f;
                transform.parent.GetComponent<Image>().color = color;
            }
            else
            {
                stringtoshow = "";
            }
            
        }
        TMP.text = stringtoshow;


    }
}
