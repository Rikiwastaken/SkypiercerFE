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
            stringtoshow = "Name : "+selectedunit.name + "\n"+"Health : "+ selectedunit.currentHP+"/"+selectedunit.stats.HP+"\n" + "Equiped weapon : \n" + "Level : "+ selectedunit.level+"\n";
        }
        TMP.text = stringtoshow;
    }
}
