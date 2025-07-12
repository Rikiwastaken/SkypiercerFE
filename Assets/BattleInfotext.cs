using TMPro;
using UnityEngine;
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

        Character selectedunit = GridScript.GetSelectedUnit();

        if (selectedunit == null)
        {
            stringtoshow = string.Empty;
        }
        else
        {
            stringtoshow = selectedunit.name + "       Level : "+ selectedunit.level + "\nHealth : "+ selectedunit.currentHP+"/"+selectedunit.stats.HP+ "       Equiped weapon : \n";
        }
        TMP.text = stringtoshow;
    }
}
