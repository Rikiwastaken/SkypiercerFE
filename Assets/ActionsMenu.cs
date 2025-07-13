using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnitScript;
public class ActionsMenu : MonoBehaviour
{

    public GameObject target;

    public Button CancelButton;

    public GameObject ItemsScript;

    private InputManager inputManager;

    public TextMeshProUGUI unitAttackText;
    public TextMeshProUGUI targetAttackText;

    private GridScript GridScript;

    private List<GameObject> targetlist;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        target = GameObject.Find("Zack");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(inputManager == null)
        {
            inputManager = FindAnyObjectByType<InputManager>();
        }

        if (inputManager.canceljustpressed && !ItemsScript.activeSelf)
        {
            CancelButton.onClick.Invoke();
        }
    }
    
    public void FindAttackers()
    {

        targetlist = new List<GameObject>();

        if(GridScript== null)
        {
            GridScript = FindAnyObjectByType<GridScript>();
        }

        foreach(GridSquareScript tile in  GridScript.lockedattacktiles)
        {
            GameObject potentialtarget = GridScript.GetUnit(tile);
            if(potentialtarget != null)
            {
                targetlist.Add(potentialtarget);
            }
        }

        if(targetlist.Count > 0)
        {
            initializeAttackWindows(target, targetlist[0]);
        }
        

    }

    public void initializeAttackWindows(GameObject unit, GameObject target)
    {
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;
        unitAttackText.transform.parent.parent.gameObject.SetActive(true);

        string UnitText= "\n"+ charunit.name+"\n";
        UnitText += "HP : " + charunit.currentHP + " / " + charunit.stats.HP + "\n";
        UnitText += "Wpn : " + unit.GetComponent<UnitScript>().GetFirstWeapon().Name + "\n";
        UnitText += "Uses : " + unit.GetComponent<UnitScript>().GetFirstWeapon().Currentuses + " / " + unit.GetComponent<UnitScript>().GetFirstWeapon().Maxuses + "\n";
        UnitText += "Dmg : " + CalculateDamage(unit, target) + "\n";
        UnitText += "Hit : " + CalculateHit(unit, target) + " %\n";
        UnitText += "Crit : " + CalculateCrit(unit, target) + " %\n";
        Debug.Log(UnitText);

        string TargetText = "\n" + chartarget.name + "\n";
        TargetText += "HP : " + chartarget.currentHP + " / " + chartarget.stats.HP + "\n";
        TargetText += "Wpn : " + target.GetComponent<UnitScript>().GetFirstWeapon().Name + "\n";
        TargetText += "Uses : " + target.GetComponent<UnitScript>().GetFirstWeapon().Currentuses + " / " + target.GetComponent<UnitScript>().GetFirstWeapon().Maxuses + "\n";
        TargetText += "Dmg : " + CalculateDamage(target, unit) + "\n";
        TargetText += "Hit : " + CalculateHit(target, unit) + " %\n";
        TargetText += "Crit : " + CalculateCrit(target, unit) + " %\n";
        Debug.Log(TargetText);

        unitAttackText.text = UnitText;
        targetAttackText.text = TargetText;

    }

    public int CalculateDamage(GameObject unit, GameObject target)
    {
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;

        int baseweapondamage = unit.GetComponent<UnitScript>().GetFirstWeapon().BaseDamage;
        int basestatdamage = charunit.stats.Strength;
        if(charunit.telekinesisactivated)
        {
            baseweapondamage = (int)(baseweapondamage*0.75f);
            basestatdamage = charunit.stats.Psyche;
        }

        int unitbasedamage = baseweapondamage + basestatdamage;

        int basestatdef = chartarget.stats.Defense;
        if (charunit.telekinesisactivated)
        {
            basestatdef = charunit.stats.Resistance;
        }
        if(unit.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower()=="greatsword")
        {
            basestatdef = (int)(basestatdef * 0.9f);
        }
        if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "shield")
        {
            basestatdef += (int)(chartarget.stats.Strength * 0.2f);
        }

        int finaldamage = unitbasedamage - basestatdef;

        if(unit.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "wand")
        {
            finaldamage = (int)(finaldamage / 2f);
        }


        if(finaldamage<0)
        {
            finaldamage = 0;
        }

        return finaldamage;

    }

    public int CalculateHit(GameObject unit, GameObject target)
    {
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;

        int hitrateweapon = unit.GetComponent<UnitScript>().GetFirstWeapon().BaseHit;

        int dexunit = charunit.stats.Dexterity;
        if (unit.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "sword")
        {
            dexunit = (int)(dexunit * 1.1f);
        }
        if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "spear")
        {
            dexunit = (int)(dexunit * 0.9f);
        }

        int spdtarget = chartarget.stats.Speed;
        if(target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower()=="sword")
        {
            spdtarget = (int)(spdtarget * 1.1f);
        }
        if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "greatsword")
        {
            spdtarget = (int)(spdtarget * 0.9f);
        }

        int finalhitrate = (int)(hitrateweapon + (dexunit - spdtarget) * 0.2f);

        if(finalhitrate<0)
        {
            finalhitrate = 0;
        }
        if(finalhitrate>100)
        {
            finalhitrate = 100;
        }

        return finalhitrate;

    }

    public int CalculateCrit(GameObject unit, GameObject target)
    {
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;

        int critweapon = unit.GetComponent<UnitScript>().GetFirstWeapon().BaseCrit;

        int dexunit = charunit.stats.Dexterity;
        if (unit.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "sword")
        {
            dexunit = (int)(dexunit * 1.1f);
        }
        if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "spear")
        {
            dexunit = (int)(dexunit * 0.9f);
        }

        int spdtarget = chartarget.stats.Speed;
        if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "sword")
        {
            spdtarget = (int)(spdtarget * 1.1f);
        }
        if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "greatsword")
        {
            spdtarget = (int)(spdtarget * 0.9f);
        }

        int finalcritrate = (int)(critweapon + dexunit/15f - spdtarget/20f);

        if (finalcritrate < 0)
        {
            finalcritrate = 0;
        }
        if (finalcritrate > 100)
        {
            finalcritrate = 100;
        }

        return finalcritrate;
    }

}
