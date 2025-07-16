using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using static UnitScript;
public class ActionsMenu : MonoBehaviour
{

    public GameObject target;

    public Button ActionsCancelButton;
    public Button AttackButton;
    public Button AttackCancelButton;

    public GameObject ItemsScript;

    private InputManager inputManager;

    public TextMeshProUGUI unitAttackText;
    public TextMeshProUGUI targetAttackText;

    public Image UnitOrangeLifeBar;
    public Image UnitGreenLifebar;
    public Image TargetOrangeLifeBar;
    public Image TargetGreenLifebar;

    private GridScript GridScript;

    public List<GameObject> targetlist;

    private battlecameraScript battlecameraScript;

    private int activetargetid;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        target = GameObject.Find("Zack");
        battlecameraScript= FindAnyObjectByType<battlecameraScript>();

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
            ActionsCancelButton.onClick.Invoke();
        }

        if(inputManager.canceljustpressed && AttackButton.transform.parent.gameObject.activeSelf)
        {
            AttackCancelButton.onClick.Invoke();
        }
        
        if (targetlist != null && targetlist.Count>0)
        {
            if (inputManager.Telekinesisjustpressed)
            {
                ToggleTelekinesis(targetlist[activetargetid]);
            }
            if (inputManager.NextWeaponjustpressed)
            {
                NextWeapon(targetlist[activetargetid], target.GetComponent<UnitScript>().GetFirstWeapon());

            }
            if (inputManager.PreviousWeaponjustpressed)
            {
                PreviousWeapon(targetlist[activetargetid], target.GetComponent<UnitScript>().GetFirstWeapon());
            }

            if (inputManager.NextTargetjustpressed)
            {
                if(activetargetid< targetlist.Count-1)
                {
                    activetargetid++;
                }
                else
                {
                    activetargetid = 0;
                }
                initializeAttackWindows(target, targetlist[activetargetid]);
            }
            if (inputManager.PreviousTargetjustpressed)
            {
                if (activetargetid > 0)
                {
                    activetargetid--;
                }
                else
                {
                    activetargetid = targetlist.Count - 1;
                }
                initializeAttackWindows(target, targetlist[activetargetid]);
            }

            if(activetargetid<= targetlist.Count)
            {
                battlecameraScript.Destination = targetlist[activetargetid].GetComponent<UnitScript>().UnitCharacteristics.position;
            }
            else
            {
                battlecameraScript.Destination = target.GetComponent<UnitScript>().UnitCharacteristics.position;
            }
            
        }

    }

    private void WeaponChange()
    {


        (int range, bool frapperenmelee) = target.GetComponent<UnitScript>().GetRangeAndMele();
        GridScript.ShowAttackAfterMovement(range, frapperenmelee, GridScript.GetTile(target.GetComponent<UnitScript>().UnitCharacteristics.position));
        GridScript.lockedattacktiles = GridScript.attacktiles;
        GridScript.Recolor();
        
    }

    private void NextWeapon(GameObject PreviousFoe,equipment initialweapon)
    {
        target.GetComponent<UnitScript>().GetNextWeapon();
        WeaponChange();
        bool enemytargettable = false;
        foreach(GridSquareScript tile in GridScript.lockedattacktiles)
        {
            if((int)tile.GridCoordinates.x == PreviousFoe.GetComponent<UnitScript>().UnitCharacteristics.position.x && (int)tile.GridCoordinates.y == PreviousFoe.GetComponent<UnitScript>().UnitCharacteristics.position.y)
            {
                enemytargettable = true;
                break;
            }
        }
        if(enemytargettable || target.GetComponent<UnitScript>().UnitCharacteristics.equipments[0]==initialweapon)
        {
            FindAttackers();
        }
        else
        {
            NextWeapon(PreviousFoe,initialweapon);
        }
        
    }

    private void PreviousWeapon(GameObject PreviousFoe, equipment initialweapon)
    {
        target.GetComponent<UnitScript>().GetPreviousWeapon();
        WeaponChange();
        bool enemytargettable = false;
        foreach (GridSquareScript tile in GridScript.lockedattacktiles)
        {
            if ((int)tile.GridCoordinates.x == PreviousFoe.GetComponent<UnitScript>().UnitCharacteristics.position.x && (int)tile.GridCoordinates.y == PreviousFoe.GetComponent<UnitScript>().UnitCharacteristics.position.y)
            {
                enemytargettable = true;
                break;
            }
        }
        if (enemytargettable || target.GetComponent<UnitScript>().UnitCharacteristics.equipments[0] == initialweapon)
        {
            FindAttackers();
        }
        else
        {
            PreviousWeapon(PreviousFoe, initialweapon);
        }

    }

    private void ToggleTelekinesis(GameObject PreviousFoe)
    {
        target.GetComponent<UnitScript>().UnitCharacteristics.telekinesisactivated = !target.GetComponent<UnitScript>().UnitCharacteristics.telekinesisactivated;
        WeaponChange();
        bool enemytargettable = false;
        foreach (GridSquareScript tile in GridScript.lockedattacktiles)
        {
            if ((int)tile.GridCoordinates.x == PreviousFoe.GetComponent<UnitScript>().UnitCharacteristics.position.x && (int)tile.GridCoordinates.y == PreviousFoe.GetComponent<UnitScript>().UnitCharacteristics.position.y)
            {
                enemytargettable = true;
                break;
            }
        }
        if (enemytargettable)
        {
            FindAttackers();
        }
        else
        {
            NextWeapon(PreviousFoe, target.GetComponent<UnitScript>().UnitCharacteristics.equipments[0]);
        }
        enemytargettable = false;
        foreach (GridSquareScript tile in GridScript.lockedattacktiles)
        {
            if ((int)tile.GridCoordinates.x == PreviousFoe.GetComponent<UnitScript>().UnitCharacteristics.position.x && (int)tile.GridCoordinates.y == PreviousFoe.GetComponent<UnitScript>().UnitCharacteristics.position.y)
            {
                enemytargettable = true;
                break;
            }
        }
        if(enemytargettable)
        {
            FindAttackers();
        }
        else
        {
            target.GetComponent<UnitScript>().UnitCharacteristics.telekinesisactivated = !target.GetComponent<UnitScript>().UnitCharacteristics.telekinesisactivated;
            WeaponChange();
            FindAttackers();
        }
    }

    public void ResetTargets()
    {
        targetlist = null;
    }
    
    public void AttackCommand()
    {
        // on essaie de trouver un combo arme/telekinesie pour pouvoir attaquer un ennemi
        Character targetcharacter = target.GetComponent<UnitScript>().UnitCharacteristics;
        FindAttackers();
        if(targetlist.Count==0) //ici pas d'ennemi trouvé donc on essaie d'autres armes
        {
            List<equipment> weapons = target.GetComponent<UnitScript>().GetAllWeapons();
            Debug.Log(weapons.Count);
            foreach(equipment weapon in weapons)
            {
                Debug.Log(weapon.Name);
                int rangebonus = 0;
                bool frapperenmelee = true;
                if (targetcharacter.telekinesisactivated)
                {
                    if(weapon.type.ToLower() == "bow")
                    {
                        rangebonus = 2;
                    }
                    else
                    {
                        rangebonus = 1;
                    }
                }
                if (weapon.type.ToLower() == "bow")
                {
                    frapperenmelee = false;
                }

                GridScript.ShowAttackAfterMovement(weapon.Range + rangebonus, frapperenmelee, GridScript.GetTile(target.GetComponent<UnitScript>().UnitCharacteristics.position));
                GridScript.lockedattacktiles = GridScript.attacktiles;
                GridScript.Recolor();
                FindAttackers();
                if(targetlist.Count>0 && weapon!=target.GetComponent<UnitScript>().Fists)
                {
                    target.GetComponent<UnitScript>().EquipWeapon(weapon);
                    return;
                }
            }
            if (targetlist.Count==0) //ici toujours pas d'ennemi trouvé donc on essaie d'autres armes en chengeant le réglage de télékinésie
            {
                Debug.Log("tentative avec telekinesie");
                targetcharacter.telekinesisactivated = !targetcharacter.telekinesisactivated;
                foreach (equipment weapon in weapons)
                {
                    int rangebonus = 0;
                    bool frapperenmelee = true;
                    if (targetcharacter.telekinesisactivated)
                    {
                        if (weapon.type.ToLower() == "bow")
                        {
                            rangebonus = 2;
                        }
                        else
                        {
                            rangebonus = 1;
                        }
                    }
                    if (weapon.type.ToLower() == "bow")
                    {
                        frapperenmelee = false;
                    }

                    GridScript.ShowAttackAfterMovement(weapon.Range + rangebonus, frapperenmelee, GridScript.GetTile(target.GetComponent<UnitScript>().UnitCharacteristics.position));
                    GridScript.lockedattacktiles = GridScript.attacktiles;
                    GridScript.Recolor();
                    FindAttackers();
                    if (targetlist.Count > 0)
                    {
                        if (weapon != target.GetComponent<UnitScript>().Fists)
                        {
                            target.GetComponent<UnitScript>().EquipWeapon(weapon);
                        }
                        return;
                    }
                }
                if(targetlist.Count == 0)  //Finalement pas d'ennemi donc on remet le réglage original de télékinésie
                {
                    targetcharacter.telekinesisactivated = !targetcharacter.telekinesisactivated;
                }
            }
        }
    }

    public void ConfirmAttack()
    {
        if(targetlist.Count>0)
        {
            ApplyDamage(target, targetlist[activetargetid]);
            target.GetComponent<UnitScript>().UnitCharacteristics.alreadyplayed = true;
            targetlist = new List<GameObject>();
            target = null;           
            GridScript.Recolor();
            unitAttackText.transform.parent.parent.gameObject.SetActive(false);
            gameObject.SetActive(false);
            GridScript.ResetAllSelections();
            FindAnyObjectByType<ActionManager>().preventfromlockingafteraction = true;
        }
    }

    private void FindAttackers()
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
            activetargetid = 0;
            initializeAttackWindows(target, targetlist[activetargetid]);
            for(int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
            AttackButton.Select();
        }
        

    }

    public void initializeAttackWindows(GameObject unit, GameObject target)
    {
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;
        unitAttackText.transform.parent.parent.gameObject.SetActive(true);

        (GameObject doubleattacker,bool tripleattack) = CalculatedoubleAttack(unit, target);

        

        string UnitText= "\n"+ charunit.name+"\n";
        UnitText += "HP : " + charunit.currentHP + " / " + charunit.stats.HP + "\n";
        UnitText += "Wpn : " + unit.GetComponent<UnitScript>().GetFirstWeapon().Name + "\n";
        UnitText += "Uses : " + unit.GetComponent<UnitScript>().GetFirstWeapon().Currentuses + " / " + unit.GetComponent<UnitScript>().GetFirstWeapon().Maxuses + "\n";
        if(doubleattacker==unit)
        {
            if(tripleattack)
            {
                UnitText += "Dmg : " + CalculateDamage(unit, target) + " x 3 \n";
            }
            else
            {
                UnitText += "Dmg : " + CalculateDamage(unit, target) + " x 2 \n";
            }
                
        }
        else
        {
            UnitText += "Dmg : " + CalculateDamage(unit, target) + "\n";
        }
        UnitText += "Hit : " + CalculateHit(unit, target) + " %\n";
        UnitText += "Crit : " + CalculateCrit(unit, target) + " %\n";
        if (charunit.telekinesisactivated)
        {
            UnitText += "Telekinesis : On\n";
        }
        else
        {
            UnitText += "Telekinesis : Off\n";
        }
        Debug.Log(UnitText);

        

        string TargetText = "\n" + chartarget.name + "\n";
        TargetText += "HP : " + chartarget.currentHP + " / " + chartarget.stats.HP + "\n";
        TargetText += "Wpn : " + target.GetComponent<UnitScript>().GetFirstWeapon().Name + "\n";
        TargetText += "Uses : " + target.GetComponent<UnitScript>().GetFirstWeapon().Currentuses + " / " + target.GetComponent<UnitScript>().GetFirstWeapon().Maxuses + "\n";
        if(CheckifInRange(unit, target))
        {
            if (doubleattacker == target)
            {
                if (tripleattack)
                {
                    TargetText += "Dmg : " + CalculateDamage(target, unit) + " x 3 \n";
                }
                else
                {
                    TargetText += "Dmg : " + CalculateDamage(target, unit) + " x 2 \n";
                }

            }
            else
            {
                TargetText += "Dmg : " + CalculateDamage(target, unit) + "\n";
            }

            TargetText += "Hit : " + CalculateHit(target, unit) + " %\n";
            TargetText += "Crit : " + CalculateCrit(target, unit) + " %\n";
        }
        else
        {
            TargetText += "Dmg : -\n";
            TargetText += "Hit : -\n";
            TargetText += "Crit : -\n";
        }
        if (chartarget.telekinesisactivated)
        {
            TargetText += "Telekinesis : On\n";
        }
        else
        {
            TargetText += "Telekinesis : Off\n";
        }
        Debug.Log(TargetText);

        unitAttackText.text = UnitText;
        targetAttackText.text = TargetText;


        if (doubleattacker == unit)
        {
            if(tripleattack)
            {
                TargetGreenLifebar.fillAmount = (float)(chartarget.currentHP - CalculateDamage(unit, target) * 3) / (float)chartarget.stats.HP;
                TargetOrangeLifeBar.fillAmount = (float)(chartarget.currentHP) / (float)chartarget.stats.HP;
            }
            else
            {
                TargetGreenLifebar.fillAmount = (float)(chartarget.currentHP - CalculateDamage(unit, target) * 2) / (float)chartarget.stats.HP;
                TargetOrangeLifeBar.fillAmount = (float)(chartarget.currentHP) / (float)chartarget.stats.HP;
            }
                
        }
        else
        {
            TargetGreenLifebar.fillAmount = (float)(chartarget.currentHP - CalculateDamage(unit, target)) / (float)chartarget.stats.HP;
            TargetOrangeLifeBar.fillAmount = (float)(chartarget.currentHP) / (float)chartarget.stats.HP;
        }

        if (CheckifInRange(unit, target))
        {
            if (doubleattacker == target)
            {
                if (tripleattack)
                {
                    UnitGreenLifebar.fillAmount = (float)(charunit.currentHP - CalculateDamage(target, unit) * 3) / (float)charunit.stats.HP;
                    UnitOrangeLifeBar.fillAmount = (float)(charunit.currentHP) / (float)charunit.stats.HP;
                }
                else
                {
                    UnitGreenLifebar.fillAmount = (float)(charunit.currentHP - CalculateDamage(target, unit) * 2) / (float)charunit.stats.HP;
                    UnitOrangeLifeBar.fillAmount = (float)(charunit.currentHP) / (float)charunit.stats.HP;
                }
            }

            else
            {
                UnitGreenLifebar.fillAmount = (float)(charunit.currentHP - CalculateDamage(target, unit)) / (float)charunit.stats.HP;
                UnitOrangeLifeBar.fillAmount = (float)(charunit.currentHP) / (float)charunit.stats.HP;
            }
        }
        else
        {
            UnitGreenLifebar.fillAmount = (float)(charunit.currentHP - 0) / (float)charunit.stats.HP;
            UnitOrangeLifeBar.fillAmount = (float)(charunit.currentHP) / (float)charunit.stats.HP;
        }   
    }

    public bool CheckifInRange(GameObject unit, GameObject target)
    {
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;
        int Distance = (int)(Mathf.Abs(chartarget.position.x - charunit.position.x) + Mathf.Abs(chartarget.position.y - charunit.position.y));
        (int range,bool melee) = target.GetComponent<UnitScript>().GetRangeAndMele();
        if (Distance <=1)
        {
            if(!melee)
            {
                return false;
            }
        }
        else if (Distance>range)
        {
            return false ;
        }
        return true ;
    }

    public void ApplyDamage(GameObject unit, GameObject target)
    {
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;

        (GameObject doubleattacker, bool tripleattack) = CalculatedoubleAttack(unit, target);

        bool inrange = CheckifInRange(unit,target);

        int unithitrate = CalculateHit(unit, target);
        int targethitrate = CalculateHit(target, unit);

        int unitdamage = CalculateDamage(unit, target);
        int targetdamage = CalculateDamage(target, unit);

        int unitcrit = CalculateCrit(unit, target);
        int targetcrit = CalculateCrit(target, unit);

        if (doubleattacker == unit)
        {
            if (tripleattack)
            {
                //calculating hit for first attack
                int randomnumber = Random.Range(0,100);
                if(randomnumber < unithitrate)
                {
                    // calculating critical
                    randomnumber = Random.Range(0, 100);
                    if (randomnumber < unitcrit)
                    {
                        chartarget.currentHP -= unitdamage * 3;
                    }
                    else
                    {
                        chartarget.currentHP -= unitdamage;
                    }
                }

                //calculating hit for second attack
                randomnumber = Random.Range(0, 100);
                if (randomnumber < unithitrate)
                {
                    // calculating critical
                    randomnumber = Random.Range(0, 100);
                    if (randomnumber < unitcrit)
                    {
                        chartarget.currentHP -= unitdamage * 3;
                    }
                    else
                    {
                        chartarget.currentHP -= unitdamage;
                    }
                }
                //calculating hit for third attack
                randomnumber = Random.Range(0, 100);
                if (randomnumber < unithitrate)
                {
                    // calculating critical
                    randomnumber = Random.Range(0, 100);
                    if (randomnumber < unitcrit)
                    {
                        chartarget.currentHP -= unitdamage * 3;
                    }
                    else
                    {
                        chartarget.currentHP -= unitdamage;
                    }
                }
            }
            else
            {
                //calculating hit for first attack
                int randomnumber = Random.Range(0, 100);
                if (randomnumber < unithitrate)
                {
                    // calculating critical
                    randomnumber = Random.Range(0, 100);
                    if (randomnumber < unitcrit)
                    {
                        chartarget.currentHP -= unitdamage * 3;
                    }
                    else
                    {
                        chartarget.currentHP -= unitdamage;
                    }
                }

                //calculating hit for second attack
                randomnumber = Random.Range(0, 100);
                if (randomnumber < unithitrate)
                {
                    // calculating critical
                    randomnumber = Random.Range(0, 100);
                    if (randomnumber < unitcrit)
                    {
                        chartarget.currentHP -= unitdamage * 3;
                    }
                    else
                    {
                        chartarget.currentHP -= unitdamage;
                    }
                }
            }

        }
        else
        {
            //calculating hit for first attack
            int randomnumber = Random.Range(0, 100);
            if (randomnumber < unithitrate)
            {
                // calculating critical
                randomnumber = Random.Range(0, 100);
                if (randomnumber < unitcrit)
                {
                    chartarget.currentHP -= unitdamage * 3;
                }
                else
                {
                    chartarget.currentHP -= unitdamage;
                }
            }
        }

        //enemy attack
        if(chartarget.currentHP >0)
        {
            if (doubleattacker == target)
            {
                if (tripleattack)
                {
                    //calculating hit for first attack
                    int randomnumber = Random.Range(0, 100);
                    if (randomnumber < targethitrate)
                    {
                        // calculating critical
                        randomnumber = Random.Range(0, 100);
                        if (randomnumber < targetcrit)
                        {
                            charunit.currentHP -= targetdamage * 3;
                        }
                        else
                        {
                            charunit.currentHP -= targetdamage;
                        }
                    }
                    //calculating hit for second attack
                    randomnumber = Random.Range(0, 100);
                    if (randomnumber < targethitrate)
                    {
                        // calculating critical
                        randomnumber = Random.Range(0, 100);
                        if (randomnumber < targetcrit)
                        {
                            charunit.currentHP -= targetdamage * 3;
                        }
                        else
                        {
                            charunit.currentHP -= targetdamage;
                        }
                    }
                    //calculating hit for third attack
                    randomnumber = Random.Range(0, 100);
                    if (randomnumber < targethitrate)
                    {
                        // calculating critical
                        randomnumber = Random.Range(0, 100);
                        if (randomnumber < targetcrit)
                        {
                            charunit.currentHP -= targetdamage * 3;
                        }
                        else
                        {
                            charunit.currentHP -= targetdamage;
                        }
                    }
                }
                else
                {
                    //calculating hit for first attack
                    int randomnumber = Random.Range(0, 100);
                    if (randomnumber < targethitrate)
                    {
                        // calculating critical
                        randomnumber = Random.Range(0, 100);
                        if (randomnumber < targetcrit)
                        {
                            charunit.currentHP -= targetdamage * 3;
                        }
                        else
                        {
                            charunit.currentHP -= targetdamage;
                        }
                    }
                    //calculating hit for second attack
                    randomnumber = Random.Range(0, 100);
                    if (randomnumber < targethitrate)
                    {
                        // calculating critical
                        randomnumber = Random.Range(0, 100);
                        if (randomnumber < targetcrit)
                        {
                            charunit.currentHP -= targetdamage * 3;
                        }
                        else
                        {
                            charunit.currentHP -= targetdamage;
                        }
                    }
                }

            }
            else
            {
                //calculating hit for first attack
                int randomnumber = Random.Range(0, 100);
                if (randomnumber < targethitrate)
                {
                    // calculating critical
                    randomnumber = Random.Range(0, 100);
                    if (randomnumber < targetcrit)
                    {
                        charunit.currentHP -= targetdamage * 3;
                    }
                    else
                    {
                        charunit.currentHP -= targetdamage;
                    }
                }
            }
        }

        if(charunit.currentHP>0 && charunit.affiliation== "playable")
        {
            AwardExp(unit, target);
        }

    }

    private void AwardExp(GameObject unit, GameObject target)
    {
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;
        int baseexp = 15;
        if(chartarget.isboss)
        {
            baseexp = 50;
        }

        int adjustedexp = (int)(baseexp * (1f + (chartarget.level - charunit.level)/5f));

        if(chartarget.currentHP<=0)
        {
            adjustedexp *= 3;
        }
        if(adjustedexp < 0)
        {
            adjustedexp = 1;
        }
        if (adjustedexp > 100)
        {
            adjustedexp = 100;
        }
        charunit.experience += adjustedexp;
        if(charunit.experience>100)
        {
            unit.GetComponent<UnitScript>().LevelUp();
        }
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

    public (GameObject,bool) CalculatedoubleAttack(GameObject unit, GameObject target)
    {
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;


        int unitbasespeed = charunit.stats.Speed;

        if (unit.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "sword")
        {
            unitbasespeed = (int)(unitbasespeed * 1.1f);
        }
        if (unit.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "greatsword")
        {
            unitbasespeed = (int)(unitbasespeed * 0.9f);
        }

        int targetbasespeed = chartarget.stats.Speed;

        if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "sword")
        {
            targetbasespeed = (int)(targetbasespeed * 1.1f);
        }
        if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "greatsword")
        {
            targetbasespeed = (int)(targetbasespeed * 0.9f);
        }

        int SpeedDiff = unitbasespeed - targetbasespeed;
        Debug.Log(unitbasespeed);
        Debug.Log(targetbasespeed);
        Debug.Log(SpeedDiff);


        if (SpeedDiff >= 150)
        {
            return (unit, true);
        }
        else if (SpeedDiff >=50 )
        {
            return (unit,false); 
        }
        if (SpeedDiff <= -150)
        {
            return (target, true);
        }
        else if( SpeedDiff <= -50 )
        {
            return (target,false);
        }

        return (null,false);

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
