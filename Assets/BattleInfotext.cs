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
        
        if (GridScript.GetSelectedUnitGameObject()!=null)
        {
            selectedunit = GridScript.GetSelectedUnitGameObject();
        }
        

        if (GridScript.GetSelectedUnitGameObject() == null && GridScript.lockedmovementtiles.Count ==0) {
            stringtoshow = string.Empty;
            Color color = transform.parent.GetComponent<Image>().color;
            color.a = 0f;
            transform.parent.GetComponent<Image>().color = color;
        }
        else
        {
            Character selectedunitCharacter = selectedunit.GetComponent<UnitScript>().UnitCharacteristics;
            stringtoshow = selectedunitCharacter.name + "       Level : "+ selectedunitCharacter.level + "\nHealth : "+ selectedunitCharacter.currentHP+" / "+ selectedunitCharacter.stats.HP+ "       Weapon : " + selectedunit.GetComponent<UnitScript>().GetFirstWeapon().Name;
            if(selectedunitCharacter.telekinesisactivated)
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
        TMP.text = stringtoshow;
    }
}
