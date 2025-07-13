using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnitScript;

public class BattleInfotext : MonoBehaviour
{

    private GridScript GridScript;

    private string stringtoshow;

    TextMeshProUGUI TMP;

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

        GameObject selectedunit = GridScript.GetSelectedUnitGameObject();

        if (selectedunit == null) {
            stringtoshow = string.Empty;
            Color color = transform.parent.GetComponent<Image>().color;
            color.a = 0f;
            transform.parent.GetComponent<Image>().color = color;
        }
        else
        {
            Character selectedunitCharacter = selectedunit.GetComponent<UnitScript>().UnitCharacteristics;
            stringtoshow = selectedunitCharacter.name + "       Level : "+ selectedunitCharacter.level + "\nHealth : "+ selectedunitCharacter.currentHP+" / "+ selectedunitCharacter.stats.HP+ "       Equiped weapon : " + selectedunit.GetComponent<UnitScript>().GetFirstWeapon().Name;
            Color color = transform.parent.GetComponent<Image>().color;
            color.a = 1f;
            transform.parent.GetComponent<Image>().color = color;
        }
        TMP.text = stringtoshow;
    }
}
