using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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

    public int activetargetid;

    public bool confirmattack;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        battlecameraScript = FindAnyObjectByType<battlecameraScript>();

    }
    private void OnEnable()
    {
        if (GridScript == null)
        {
            GridScript = FindAnyObjectByType<GridScript>();
        }
        target = GridScript.GetSelectedUnitGameObject();
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        SelectionSafeGuard();
        FindAnyObjectByType<ActionManager>().preventfromlockingafteraction = true;

        if (inputManager == null)
        {
            inputManager = FindAnyObjectByType<InputManager>();
        }

        if (inputManager.canceljustpressed && !ItemsScript.activeSelf)
        {
            ActionsCancelButton.onClick.Invoke();
        }

        if (inputManager.canceljustpressed && AttackButton.transform.parent.gameObject.activeSelf)
        {
            AttackCancelButton.onClick.Invoke();
        }

        if (targetlist != null && targetlist.Count > 0)
        {
            if (inputManager.Telekinesisjustpressed)
            {
                ToggleTelekinesis(targetlist[activetargetid]);
            }
            if (inputManager.NextWeaponjustpressed)
            {
                if (!(targetlist[activetargetid].GetComponent<UnitScript>().UnitCharacteristics.affiliation == target.GetComponent<UnitScript>().UnitCharacteristics.affiliation && target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "staff"))
                {
                    NextWeapon(targetlist[activetargetid], target.GetComponent<UnitScript>().GetFirstWeapon());
                }


            }
            if (inputManager.PreviousWeaponjustpressed)
            {
                if (!(targetlist[activetargetid].GetComponent<UnitScript>().UnitCharacteristics.affiliation == target.GetComponent<UnitScript>().UnitCharacteristics.affiliation && target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "staff"))
                {
                    PreviousWeapon(targetlist[activetargetid], target.GetComponent<UnitScript>().GetFirstWeapon());
                }

            }

            if (inputManager.NextTargetjustpressed)
            {
                if (activetargetid < targetlist.Count - 1)
                {
                    activetargetid++;
                }
                else
                {
                    activetargetid = 0;
                }
                if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "staff")
                {
                    initializeHealingWindows(target, targetlist[activetargetid]);
                }
                else
                {
                    initializeAttackWindows(target, targetlist[activetargetid]);
                }

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
                if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "staff")
                {
                    initializeHealingWindows(target, targetlist[activetargetid]);
                }
                else
                {
                    initializeAttackWindows(target, targetlist[activetargetid]);
                }
            }

            if (activetargetid <= targetlist.Count)
            {
                battlecameraScript.Destination = targetlist[activetargetid].GetComponent<UnitScript>().UnitCharacteristics.position;
            }
            else
            {
                battlecameraScript.Destination = target.GetComponent<UnitScript>().UnitCharacteristics.position;
            }

            CheckCorrectInfo(target, targetlist[activetargetid]);

        }

    }

    private void CheckCorrectInfo(GameObject unit, GameObject enemy)
    {
        string unitweaponname = unit.GetComponent<UnitScript>().GetFirstWeapon().Name;
        string enemyweaponname = enemy.GetComponent<UnitScript>().GetFirstWeapon().Name;

        if (!unitAttackText.text.Contains(unitweaponname) || !targetAttackText.text.Contains(enemyweaponname))
        {
            if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "staff")
            {
                initializeHealingWindows(target, targetlist[activetargetid]);
            }
            else
            {
                initializeAttackWindows(target, targetlist[activetargetid]);
            }
        }
    }

    private void WeaponChange()
    {


        (int range, bool frapperenmelee, string type) = target.GetComponent<UnitScript>().GetRangeMeleeAndType();
        GridScript.ShowAttackAfterMovement(range, frapperenmelee, GridScript.GetTile(target.GetComponent<UnitScript>().UnitCharacteristics.position), type.ToLower() == "staff");
        GridScript.lockedattacktiles = GridScript.attacktiles;
        GridScript.lockedhealingtiles = GridScript.healingtiles;
        GridScript.Recolor();

    }

    private void NextWeapon(GameObject PreviousFoe, equipment initialweapon)
    {

        target.GetComponent<UnitScript>().GetNextWeapon();
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
            NextWeapon(PreviousFoe, initialweapon);
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
        if (enemytargettable)
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
        if (targetlist.Count == 0) //ici pas d'ennemi trouve donc on essaie d'autres armes
        {
            if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() != "staff")
            {
                List<equipment> weapons = target.GetComponent<UnitScript>().GetAllWeapons();
                foreach (equipment weapon in weapons)
                {
                    if (weapon.type.ToLower() == "staff")
                    {
                        continue;
                    }
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
                        if (target.GetComponent<UnitScript>().GetSkill(33))
                        {
                            rangebonus += 1;
                        }
                    }
                    if (weapon.type.ToLower() == "bow")
                    {
                        frapperenmelee = false;
                    }

                    GridScript.ShowAttackAfterMovement(weapon.Range + rangebonus, frapperenmelee, GridScript.GetTile(target.GetComponent<UnitScript>().UnitCharacteristics.position), weapon.type.ToLower() == "staff");
                    GridScript.lockedattacktiles = GridScript.attacktiles;
                    GridScript.lockedhealingtiles = GridScript.healingtiles;
                    GridScript.Recolor();
                    FindAttackers();
                    if (targetlist.Count > 0 && weapon != target.GetComponent<UnitScript>().Fists)
                    {
                        target.GetComponent<UnitScript>().EquipWeapon(weapon);
                        return;
                    }
                }
                if (targetlist.Count == 0) //ici toujours pas d'ennemi trouve donc on essaie d'autres armes en chengeant le reglage de telekinesie
                {
                    targetcharacter.telekinesisactivated = !targetcharacter.telekinesisactivated;
                    foreach (equipment weapon in weapons)
                    {
                        if (weapon.type.ToLower() == "staff")
                        {
                            continue;
                        }
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
                            if (target.GetComponent<UnitScript>().GetSkill(33))
                            {
                                rangebonus += 1;
                            }
                        }
                        if (weapon.type.ToLower() == "bow")
                        {
                            frapperenmelee = false;
                        }

                        GridScript.ShowAttackAfterMovement(weapon.Range + rangebonus, frapperenmelee, GridScript.GetTile(target.GetComponent<UnitScript>().UnitCharacteristics.position), weapon.type.ToLower() == "staff");
                        GridScript.lockedattacktiles = GridScript.attacktiles;
                        GridScript.lockedhealingtiles = GridScript.healingtiles;
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
                    if (targetlist.Count == 0)  //Finalement pas d'ennemi donc on remet le reglage original de telekinesie
                    {
                        targetcharacter.telekinesisactivated = !targetcharacter.telekinesisactivated;

                    }
                }
            }
            else
            {
                targetcharacter.telekinesisactivated = !targetcharacter.telekinesisactivated;
                FindAttackers();
                if (targetlist.Count == 0)  //Finalement pas d'ennemi donc on remet le reglage original de telekinesie
                {
                    targetcharacter.telekinesisactivated = !targetcharacter.telekinesisactivated;

                }
            }

        }
    }

    public void FinalizeAttack()
    {
        target.GetComponent<UnitScript>().UnitCharacteristics.alreadyplayed = true;
        targetlist = new List<GameObject>();
        GameObject oldtarget = target;
        target = null;
        GridScript.Recolor();
        confirmattack = false;
        FindAnyObjectByType<ActionManager>().currentcharacter = null;
        FindAnyObjectByType<battlecameraScript>().incombat = false;
        FindAnyObjectByType<ActionManager>().preventfromlockingafteraction = true;
        oldtarget.GetComponent<UnitScript>().RetreatTrigger(); // Canto/Retreat (move again after action)
    }
    public void ConfirmAttack()
    {
        if (targetlist.Count > 0)
        {
            confirmattack = true;
            unitAttackText.transform.parent.parent.gameObject.SetActive(false);
            gameObject.SetActive(false);
            GridScript.ResetAllSelections();
        }
    }



    private void FindAttackers()
    {

        targetlist = new List<GameObject>();

        if (GridScript == null)
        {
            GridScript = FindAnyObjectByType<GridScript>();
        }

        if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "staff")
        {
            GridSquareScript positiontile = GridScript.GetTile(target.GetComponent<UnitScript>().UnitCharacteristics.position);
            (int range, bool melee) = target.GetComponent<UnitScript>().GetRangeAndMele();
            GridScript.ShowAttackAfterMovement(range, melee, positiontile, true);
            GridScript.lockedhealingtiles = GridScript.healingtiles;
            foreach (GridSquareScript tile in GridScript.lockedhealingtiles)
            {
                GameObject potentialtarget = GridScript.GetUnit(tile);
                if (potentialtarget != null && potentialtarget.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable" && potentialtarget != target)
                {
                    targetlist.Add(potentialtarget);
                }
            }
            if (targetlist.Count > 0)
            {
                activetargetid = 0;
                initializeHealingWindows(target, targetlist[activetargetid]);
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).gameObject.SetActive(false);
                }
                AttackButton.GetComponentInChildren<TextMeshProUGUI>().text = "Heal";
                AttackButton.Select();
            }
        }
        else
        {
            foreach (GridSquareScript tile in GridScript.lockedattacktiles)
            {
                GameObject potentialtarget = GridScript.GetUnit(tile);
                if (potentialtarget != null && potentialtarget.GetComponent<UnitScript>().UnitCharacteristics.affiliation != "playable")
                {
                    targetlist.Add(potentialtarget);
                }
            }
            if (targetlist.Count > 0)
            {
                activetargetid = 0;
                initializeAttackWindows(target, targetlist[activetargetid]);
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).gameObject.SetActive(false);
                }
                AttackButton.GetComponentInChildren<TextMeshProUGUI>().text = "Attack";
                AttackButton.Select();
            }
        }






    }

    private void SelectionSafeGuard()
    {
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

        List<GameObject> listofChildren = new List<GameObject>();
        for (int i = 0;i < transform.childCount;i++)
        {
            listofChildren.Add(transform.GetChild(i).gameObject);
        }

        if ((!currentSelected.activeSelf || !listofChildren.Contains(currentSelected)) && !ItemsScript.activeSelf)
        {
            FindAnyObjectByType<EventSystem>().SetSelectedGameObject(transform.GetChild(0).gameObject);
        }
    }

    public void initializeAttackWindows(GameObject unit, GameObject target)
    {
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;
        unitAttackText.transform.parent.parent.gameObject.SetActive(true);

        (GameObject doubleattacker, bool tripleattack) = CalculatedoubleAttack(unit, target);



        string UnitText = "\n" + charunit.name + "\n";
        UnitText += "HP : " + charunit.currentHP + " / " + charunit.stats.HP + "\n";
        UnitText += "Wpn : " + unit.GetComponent<UnitScript>().GetFirstWeapon().Name + "\n";
        UnitText += "Uses : " + unit.GetComponent<UnitScript>().GetFirstWeapon().Currentuses + " / " + unit.GetComponent<UnitScript>().GetFirstWeapon().Maxuses + "\n";
        if (doubleattacker == unit)
        {
            if (tripleattack)
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



        string TargetText = "\n" + chartarget.name + "\n";
        TargetText += "HP : " + chartarget.currentHP + " / " + chartarget.stats.HP + "\n";
        TargetText += "Wpn : " + target.GetComponent<UnitScript>().GetFirstWeapon().Name + "\n";
        TargetText += "Uses : " + target.GetComponent<UnitScript>().GetFirstWeapon().Currentuses + " / " + target.GetComponent<UnitScript>().GetFirstWeapon().Maxuses + "\n";
        if (CheckifInRange(unit, target) || target.GetComponent<UnitScript>().GetSkill(38)) //Spite
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

        unitAttackText.text = UnitText;
        targetAttackText.text = TargetText;


        if (doubleattacker == unit)
        {
            if (tripleattack)
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

        if (CheckifInRange(unit, target) || target.GetComponent<UnitScript>().GetSkill(38)) //Spite
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

    public void initializeHealingWindows(GameObject unit, GameObject target)
    {
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;
        unitAttackText.transform.parent.parent.gameObject.SetActive(true);



        string UnitText = "\n" + charunit.name + "\n";
        UnitText += "HP : " + charunit.currentHP + " / " + charunit.stats.HP + "\n";
        UnitText += "Wpn : " + unit.GetComponent<UnitScript>().GetFirstWeapon().Name + "\n";
        UnitText += "Uses : " + unit.GetComponent<UnitScript>().GetFirstWeapon().Currentuses + " / " + unit.GetComponent<UnitScript>().GetFirstWeapon().Maxuses + "\n";
        UnitText += "Healing : " + CalculateHealing(unit) + " \n";
        UnitText += "Hit : 100 %\n";
        UnitText += "Crit : - \n";
        if (charunit.telekinesisactivated)
        {
            UnitText += "Telekinesis : On\n";
        }
        else
        {
            UnitText += "Telekinesis : Off\n";
        }



        string TargetText = "\n" + chartarget.name + "\n";
        TargetText += "HP : " + chartarget.currentHP + " / " + chartarget.stats.HP + "\n";
        TargetText += "Wpn : " + target.GetComponent<UnitScript>().GetFirstWeapon().Name + "\n";
        TargetText += "Uses : " + target.GetComponent<UnitScript>().GetFirstWeapon().Currentuses + " / " + target.GetComponent<UnitScript>().GetFirstWeapon().Maxuses + "\n";
        TargetText += "Dmg : -\n";
        TargetText += "Hit : -\n";
        TargetText += "Crit : -\n";
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

        TargetGreenLifebar.fillAmount = Mathf.Max((float)(chartarget.currentHP + CalculateHealing(unit)) / (float)chartarget.stats.HP, 1f);
        TargetOrangeLifeBar.fillAmount = (float)(chartarget.currentHP) / (float)chartarget.stats.HP;

        UnitGreenLifebar.fillAmount = (float)(charunit.currentHP - 0) / (float)charunit.stats.HP;
        UnitOrangeLifeBar.fillAmount = (float)(charunit.currentHP) / (float)charunit.stats.HP;
    }

    public bool CheckifInRange(GameObject unit, GameObject target)
    {
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;
        int Distance = (int)(Mathf.Abs(chartarget.position.x - charunit.position.x) + Mathf.Abs(chartarget.position.y - charunit.position.y));
        (int range, bool melee) = target.GetComponent<UnitScript>().GetRangeAndMele();
        if (Distance <= 1)
        {
            if (!melee)
            {
                return false;
            }
        }
        else if (Distance > range)
        {
            return false;
        }
        return true;
    }

    public (int, int, int, int, List<int>) ApplyDamage(GameObject unit, GameObject target, bool unitalreadyattacked)
    {
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;
        List<int> levelup = null;
        int exp = 1;
        if (unit.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() != "staff")
        {
            (GameObject doubleattacker, bool tripleattack) = CalculatedoubleAttack(unit, target);

            bool inrange = CheckifInRange(unit, target) || target.GetComponent<UnitScript>().GetSkill(38);//spite

            int unithitrate = CalculateHit(unit, target);
            int targethitrate = CalculateHit(target, unit);

            int unitdamage = CalculateDamage(unit, target);
            int targetdamage = CalculateDamage(target, unit);

            int unitcrit = CalculateCrit(unit, target);
            int targetcrit = CalculateCrit(target, unit);

            int numberofhits = 0;
            int numberofcritials = 0;

            int finaldamage = 0;

            if (!unitalreadyattacked)
            {
                int totaldamage = 0;
                if (doubleattacker == unit)
                {
                    if (tripleattack)
                    {
                        //calculating hit for first attack
                        int randomnumber = Random.Range(0, 100);
                        if (randomnumber < unithitrate)
                        {

                            numberofhits++;
                            // calculating critical
                            randomnumber = Random.Range(0, 100);
                            if (randomnumber < unitcrit)
                            {
                                numberofcritials++;
                                totaldamage += unitdamage * 3;
                            }
                            else
                            {
                                totaldamage += unitdamage;
                            }
                            if (unit.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "scythe")
                            {
                                DealScytheDamage(unit, target);
                            }
                        }

                        //calculating hit for second attack
                        randomnumber = Random.Range(0, 100);
                        if (randomnumber < unithitrate)
                        {
                            numberofhits++;
                            // calculating critical
                            randomnumber = Random.Range(0, 100);
                            if (randomnumber < unitcrit)
                            {
                                numberofcritials++;
                                totaldamage += unitdamage * 3;
                            }
                            else
                            {
                                totaldamage += unitdamage;
                            }
                            if (unit.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "scythe")
                            {
                                DealScytheDamage(unit, target);
                            }
                        }
                        //calculating hit for third attack
                        randomnumber = Random.Range(0, 100);
                        if (randomnumber < unithitrate)
                        {
                            numberofhits++;
                            // calculating critical
                            randomnumber = Random.Range(0, 100);
                            if (randomnumber < unitcrit)
                            {
                                numberofcritials++;
                                totaldamage += unitdamage * 3;
                            }
                            else
                            {
                                totaldamage += unitdamage;
                            }
                            if (unit.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "scythe")
                            {
                                DealScytheDamage(unit, target);
                            }
                        }
                    }
                    else
                    {
                        //calculating hit for first attack
                        int randomnumber = Random.Range(0, 100);
                        if (randomnumber < unithitrate)
                        {
                            numberofhits++;
                            // calculating critical
                            randomnumber = Random.Range(0, 100);
                            if (randomnumber < unitcrit)
                            {
                                numberofcritials++;
                                totaldamage += unitdamage * 3;
                            }
                            else
                            {
                                totaldamage += unitdamage;
                            }
                            if (unit.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "scythe")
                            {
                                DealScytheDamage(unit, target);
                            }
                        }

                        //calculating hit for second attack
                        randomnumber = Random.Range(0, 100);
                        if (randomnumber < unithitrate)
                        {
                            numberofhits++;
                            // calculating critical
                            randomnumber = Random.Range(0, 100);
                            if (randomnumber < unitcrit)
                            {
                                numberofcritials++;
                                totaldamage += unitdamage * 3;
                            }
                            else
                            {
                                totaldamage += unitdamage;
                            }
                            if (unit.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "scythe")
                            {
                                DealScytheDamage(unit, target);
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
                        numberofhits++;
                        // calculating critical
                        randomnumber = Random.Range(0, 100);
                        if (randomnumber < unitcrit)
                        {
                            numberofcritials++;
                            totaldamage += unitdamage * 3;
                        }
                        else
                        {
                            totaldamage += unitdamage;
                        }
                        if (unit.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "scythe")
                        {
                            DealScytheDamage(unit, target);
                        }
                    }
                }

                AffectDamage(unit, target, totaldamage);

                OnDamageEffect(unit, unitdamage, false);
                finaldamage = unitdamage;
                if (chartarget.currentHP <= 0 || !(CheckifInRange(unit, target) || target.GetComponent<UnitScript>().GetSkill(38))) //Spite
                {
                    if (charunit.currentHP > 0 && charunit.affiliation == "playable")
                    {
                        (exp, levelup) = AwardExp(unit, target);
                    }
                }
            }
            else
            {
                //enemy attack

                int totaldamage = 0;

                if (chartarget.currentHP > 0)
                {
                    if (doubleattacker == target)
                    {
                        if (tripleattack)
                        {
                            //calculating hit for first attack
                            int randomnumber = Random.Range(0, 100);
                            if (randomnumber < targethitrate)
                            {
                                numberofhits++;
                                // calculating critical
                                randomnumber = Random.Range(0, 100);
                                if (randomnumber < targetcrit)
                                {
                                    numberofcritials++;
                                    totaldamage += targetdamage * 3;
                                }
                                else
                                {
                                    totaldamage += targetdamage;
                                }
                                if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "scythe")
                                {
                                    DealScytheDamage(target, unit);
                                }
                            }
                            //calculating hit for second attack
                            randomnumber = Random.Range(0, 100);
                            if (randomnumber < targethitrate)
                            {
                                numberofhits++;
                                // calculating critical
                                randomnumber = Random.Range(0, 100);
                                if (randomnumber < targetcrit)
                                {
                                    numberofcritials++;
                                    totaldamage += targetdamage * 3;
                                }
                                else
                                {
                                    totaldamage += targetdamage;
                                }
                                if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "scythe")
                                {
                                    DealScytheDamage(target, unit);
                                }
                            }
                            //calculating hit for third attack
                            randomnumber = Random.Range(0, 100);
                            if (randomnumber < targethitrate)
                            {
                                numberofhits++;
                                // calculating critical
                                randomnumber = Random.Range(0, 100);
                                if (randomnumber < targetcrit)
                                {
                                    numberofcritials++;
                                    totaldamage += targetdamage * 3;
                                }
                                else
                                {
                                    totaldamage += targetdamage;
                                }
                                if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "scythe")
                                {
                                    DealScytheDamage(target, unit);
                                }
                            }
                        }
                        else
                        {
                            //calculating hit for first attack
                            int randomnumber = Random.Range(0, 100);
                            if (randomnumber < targethitrate)
                            {
                                numberofhits++;
                                // calculating critical
                                randomnumber = Random.Range(0, 100);
                                if (randomnumber < targetcrit)
                                {
                                    numberofcritials++;
                                    totaldamage += targetdamage * 3;
                                }
                                else
                                {
                                    totaldamage += targetdamage;
                                }
                                if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "scythe")
                                {
                                    DealScytheDamage(target, unit);
                                }
                            }
                            //calculating hit for second attack
                            randomnumber = Random.Range(0, 100);
                            if (randomnumber < targethitrate)
                            {
                                numberofhits++;
                                // calculating critical
                                randomnumber = Random.Range(0, 100);
                                if (randomnumber < targetcrit)
                                {
                                    numberofcritials++;
                                    totaldamage += targetdamage * 3;
                                }
                                else
                                {
                                    totaldamage += targetdamage;
                                }
                                if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "scythe")
                                {
                                    DealScytheDamage(target, unit);
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
                            numberofhits++;
                            // calculating critical
                            randomnumber = Random.Range(0, 100);
                            if (randomnumber < targetcrit)
                            {
                                numberofcritials++;
                                totaldamage += targetdamage * 3;
                            }
                            else
                            {
                                totaldamage += targetdamage;
                            }
                            if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "scythe")
                            {
                                DealScytheDamage(target, unit);
                            }
                        }
                    }
                    AffectDamage(target, unit, totaldamage);
                    OnDamageEffect(target, targetdamage, false);
                    finaldamage = targetdamage;
                }
                if (charunit.currentHP > 0 && charunit.affiliation == "playable")
                {
                    (exp, levelup) = AwardExp(unit, target);
                }
                if (chartarget.currentHP > 0 && chartarget.affiliation == "playable")
                {
                    (exp, levelup) = AwardExp(target, unit);
                }
            }

            if (chartarget.currentHP <= 0)
            {
                unit.GetComponent<UnitScript>().unitkilled++;
            }
            if (charunit.currentHP <= 0)
            {
                target.GetComponent<UnitScript>().unitkilled++;
            }

            return (numberofhits, numberofcritials, finaldamage, exp, levelup);
        }
        //using a staff
        else
        {
            bool inrange = CheckifInRange(unit, target) || target.GetComponent<UnitScript>().GetSkill(38); //Spite

            int unitdamage = (int)Mathf.Max(CalculateHealing(unit), chartarget.stats.HP - chartarget.currentHP);

            int numberofhits = 1;
            int numberofcritials = 0;

            int finaldamage = 0;

            if (!unitalreadyattacked)
            {
                if (unitdamage + chartarget.currentHP > chartarget.stats.HP)
                {
                    unitdamage = chartarget.stats.HP - chartarget.currentHP;
                }
                chartarget.currentHP += unitdamage;

                OnDamageEffect(unit, unitdamage, true);
                finaldamage = unitdamage;
            }
            else
            {
                //When healing the other does nothing
                numberofcritials = 0;
                numberofhits = 0;
                finaldamage = 0;
            }
            if (charunit.currentHP > 0 && chartarget.affiliation == "playable" && charunit.affiliation == "playable")
            {
                (exp, levelup) = AwardExp(unit, target, true);
            }
            return (numberofhits, numberofcritials, finaldamage, exp, levelup);
        }

    }

    private void AffectDamage(GameObject Attacker, GameObject target, int damage)
    {

        Character charTarget = target.GetComponent<UnitScript>().UnitCharacteristics;

        List<GameObject> activelist = null;
        if (charTarget.affiliation == "playable")
        {
            activelist = FindAnyObjectByType<TurnManger>().playableunitGO;
        }
        else if (charTarget.affiliation == "enemy")
        {
            activelist = FindAnyObjectByType<TurnManger>().enemyunitGO;
        }
        else
        {
            activelist = FindAnyObjectByType<TurnManger>().otherunitsGO;
        }

        //All for one
        Character allforonetransfertarget = null;
        foreach (GameObject othertarget in activelist)
        {
            Character charOthertarget = othertarget.GetComponent<UnitScript>().UnitCharacteristics;
            if (othertarget.GetComponent<UnitScript>().GetSkill(40) && ManhattanDistance(charTarget, charOthertarget) <= 3)
            {
                allforonetransfertarget = charOthertarget;
                break;
            }
        }

        if (allforonetransfertarget != null)
        {
            target.GetComponent<UnitScript>().UnitCharacteristics.currentHP -= damage / 2;
            allforonetransfertarget.currentHP -= damage / 2;
        }
        else
        {
            target.GetComponent<UnitScript>().UnitCharacteristics.currentHP -= damage;
        }



    }
    private void OnDamageEffect(GameObject Attacker, int DamageDealt, bool healing)
    {


        if (healing)
        {
            if (Attacker.GetComponent<UnitScript>().GetSkill(30)) // Compassion
            {
                Attacker.GetComponent<UnitScript>().AddNumber(Mathf.Min(DamageDealt, Attacker.GetComponent<UnitScript>().UnitCharacteristics.stats.HP - Attacker.GetComponent<UnitScript>().UnitCharacteristics.currentHP), true, "Compassion");
                Attacker.GetComponent<UnitScript>().UnitCharacteristics.currentHP += DamageDealt;
                if (Attacker.GetComponent<UnitScript>().UnitCharacteristics.currentHP > Attacker.GetComponent<UnitScript>().UnitCharacteristics.stats.HP)
                {
                    Attacker.GetComponent<UnitScript>().UnitCharacteristics.currentHP = Attacker.GetComponent<UnitScript>().UnitCharacteristics.stats.HP;
                }
            }
            if (Attacker.GetComponent<UnitScript>().GetSkill(34)) // Rebound
            {
                List<GameObject> list = new List<GameObject>();
                if (Attacker.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable")
                {
                    list = FindAnyObjectByType<TurnManger>().playableunitGO;
                }
                else if (Attacker.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "enemy")
                {
                    list = FindAnyObjectByType<TurnManger>().enemyunitGO;
                }
                else
                {
                    list = FindAnyObjectByType<TurnManger>().otherunitsGO;
                }

                foreach (GameObject unit in list)
                {
                    Character unitchar = unit.GetComponent<UnitScript>().UnitCharacteristics;
                    unit.GetComponent<UnitScript>().AddNumber(Mathf.Min(unitchar.stats.HP - unitchar.currentHP, (int)(DamageDealt * 0.1f)), true, "Rebound");
                    unitchar.currentHP += (int)(DamageDealt * 0.1f);
                    if (unitchar.currentHP > unitchar.stats.HP)
                    {
                        unitchar.currentHP = unitchar.stats.HP;
                    }
                }
            }
        }
        else
        {
            if (Attacker.GetComponent<UnitScript>().GetSkill(14)) // Invigorating
            {
                Character AttackerChar = Attacker.GetComponent<UnitScript>().UnitCharacteristics;
                Attacker.GetComponent<UnitScript>().AddNumber(Mathf.Min(AttackerChar.stats.HP - AttackerChar.currentHP, (DamageDealt / 10)), true, "Invigorating");
                AttackerChar.currentHP += (DamageDealt / 10);
                if (AttackerChar.currentHP > AttackerChar.stats.HP)
                {
                    AttackerChar.currentHP = AttackerChar.stats.HP;
                }
            }

        }

        //Durability
        if (Attacker.GetComponent<UnitScript>().GetSkill(8)) //inexhaustible
        {
            return;
        }
        equipment weapon = Attacker.GetComponent<UnitScript>().GetFirstWeapon();
        if (weapon.Maxuses > 0)
        {
            weapon.Currentuses--;
        }

    }

    private void DealScytheDamage(GameObject attacker, GameObject target)
    {
        List<GameObject> targetlist = new List<GameObject>();
        Vector2 position = target.GetComponent<UnitScript>().UnitCharacteristics.position;

        if (position.x < GridScript.Grid.Count - 1)
        {
            GameObject newunit = GridScript.GetUnit(GridScript.GetTile((int)(position.x + 1), (int)position.y));
            if (newunit != null)
            {
                targetlist.Add(newunit);
            }
        }


        if (position.x > 0)
        {
            GameObject newunit = GridScript.GetUnit(GridScript.GetTile((int)(position.x - 1), (int)position.y));
            if (newunit != null)
            {
                targetlist.Add(newunit);
            }
        }

        if (position.y < GridScript.Grid[0].Count - 1)
        {
            GameObject newunit = GridScript.GetUnit(GridScript.GetTile((int)position.x, (int)(position.y + 1)));
            if (newunit != null)
            {
                targetlist.Add(newunit);
            }
        }


        if (position.y > 0)
        {
            GameObject newunit = GridScript.GetUnit(GridScript.GetTile((int)position.x, (int)(position.y - 1)));
            if (newunit != null)
            {
                targetlist.Add(newunit);
            }
        }


        foreach (GameObject potentialtarget in targetlist)
        {
            Character Chartarget = potentialtarget.GetComponent<UnitScript>().UnitCharacteristics;
            if (Chartarget.affiliation == target.GetComponent<UnitScript>().UnitCharacteristics.affiliation)
            {
                int damage = CalculateDamage(attacker, potentialtarget);
                potentialtarget.GetComponent<UnitScript>().AddNumber(damage / 4, false, "Scythe");
                //Cleaver
                if (attacker.GetComponent<UnitScript>().GetSkill(27))
                {
                    potentialtarget.GetComponent<UnitScript>().AddNumber(damage / 4, false, "Cleaver");
                    Chartarget.currentHP -= damage / 2;
                }
                else
                {
                    Chartarget.currentHP -= damage / 4;
                }

            }
        }
    }
    public (int, List<int>) AwardExp(GameObject unit, GameObject target, bool usingstaff = false)
    {
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;
        int baseexp = 15;
        if (chartarget.isboss)
        {
            baseexp = 50;
        }

        int adjustedexp = (int)(baseexp * (1f + (chartarget.level - charunit.level) / 5f));

        if (chartarget.currentHP <= 0)
        {
            adjustedexp *= 3;
        }
        if (adjustedexp < 0)
        {
            adjustedexp = 1;
        }
        if (adjustedexp > 100)
        {
            adjustedexp = 100;
        }
        if (usingstaff)
        {
            adjustedexp = 15;
        }
        charunit.experience += adjustedexp;
        List<int> levelup = new List<int>();
        if (charunit.experience > 100)
        {
            levelup = unit.GetComponent<UnitScript>().LevelUp();
        }
        return (adjustedexp, levelup);
    }

    public int CalculateDamage(GameObject unit, GameObject target)
    {
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;
        GridSquareScript targetTile = GridScript.GetTile((int)chartarget.position.x, (int)chartarget.position.y);


        AllStatsSkillBonus UnitSkillBonus = unit.GetComponent<UnitScript>().GetStatSkillBonus(target);
        AllStatsSkillBonus TargetSkillBonus = target.GetComponent<UnitScript>().GetStatSkillBonus(unit);

        int baseweapondamage = unit.GetComponent<UnitScript>().GetFirstWeapon().BaseDamage;
        int basestatdamage = charunit.stats.Strength + UnitSkillBonus.Strength;
        int basestatdef = chartarget.stats.Defense + TargetSkillBonus.Defense;
        if (charunit.telekinesisactivated)
        {
            baseweapondamage = (int)(baseweapondamage * 0.75f);
            basestatdamage = charunit.stats.Psyche + UnitSkillBonus.Psyche;
            basestatdef = charunit.stats.Resistance + TargetSkillBonus.Resistance;
        }

        if (unit.GetComponent<UnitScript>().GetFirstWeapon().Name.ToLower() == "reshine")
        {
            basestatdamage = charunit.stats.Psyche + UnitSkillBonus.Psyche;
            basestatdef = (int)Mathf.Min(chartarget.stats.Defense + TargetSkillBonus.Defense, charunit.stats.Resistance + TargetSkillBonus.Resistance);
        }

        int unitbasedamage = baseweapondamage + basestatdamage;

        if (unit.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "greatsword")
        {
            basestatdef = (int)(basestatdef * 0.9f);
        }
        if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "shield")
        {
            basestatdef += (int)(chartarget.stats.Strength * 0.2f);
        }


        int finaldamage = unitbasedamage - basestatdef;
        if (charunit.telekinesisactivated)
        {
            finaldamage = (int)(finaldamage * (1f + (float)UnitSkillBonus.TelekDamage / 100f));
        }
        else
        {
            finaldamage = (int)(finaldamage * (1f + (float)UnitSkillBonus.PhysDamage / 100f));
        }
        finaldamage = (int)(finaldamage / (1f + (float)TargetSkillBonus.DamageReduction / 100f));
        if (unit.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "staff")
        {
            finaldamage = (int)(finaldamage / 2f);
        }

        if (targetTile.type == "Fire")
        {
            finaldamage = (int)(finaldamage * 1.1f);
        }

        finaldamage = finaldamage + UnitSkillBonus.FixedDamageBonus - TargetSkillBonus.FixedDamageReduction;




        if (finaldamage < 0)
        {
            finaldamage = 0;
        }

        return finaldamage;

    }

    public int CalculateHealing(GameObject unit)
    {
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;

        int baseweapondamage = unit.GetComponent<UnitScript>().GetFirstWeapon().BaseDamage;
        AllStatsSkillBonus UnitSkillBonus = unit.GetComponent<UnitScript>().GetStatSkillBonus(target);
        int basestatdamage = charunit.stats.Psyche + UnitSkillBonus.Psyche;


        if (charunit.telekinesisactivated)
        {
            baseweapondamage = (int)(baseweapondamage * 0.25f);
        }

        int unitbasedamage = baseweapondamage + (int)(basestatdamage / 4f);

        return unitbasedamage;

    }

    public (GameObject, bool) CalculatedoubleAttack(GameObject unit, GameObject target)
    {
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;

        AllStatsSkillBonus UnitSkillBonus = unit.GetComponent<UnitScript>().GetStatSkillBonus(target);
        AllStatsSkillBonus TargetSkillBonus = target.GetComponent<UnitScript>().GetStatSkillBonus(unit);

        int unitbasespeed = charunit.stats.Speed + UnitSkillBonus.Speed;

        if (unit.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "sword")
        {
            unitbasespeed = (int)(unitbasespeed * 1.1f);
        }
        if (unit.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "greatsword")
        {
            unitbasespeed = (int)(unitbasespeed * 0.9f);
        }

        int targetbasespeed = chartarget.stats.Speed + TargetSkillBonus.Speed;

        if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "sword")
        {
            targetbasespeed = (int)(targetbasespeed * 1.1f);
        }
        if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "greatsword")
        {
            targetbasespeed = (int)(targetbasespeed * 0.9f);
        }

        int SpeedDiff = unitbasespeed - targetbasespeed;


        if (SpeedDiff >= 150 || unit.GetComponent<UnitScript>().GetSkill(39))
        {
            return (unit, true);
        }
        else if (SpeedDiff >= 50)
        {
            return (unit, false);
        }
        if (SpeedDiff <= -150)
        {
            return (target, true);
        }
        else if (SpeedDiff <= -50)
        {
            return (target, false);
        }

        return (null, false);

    }

    public int CalculateHit(GameObject unit, GameObject target)
    {
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;

        GridSquareScript unitTile = GridScript.GetTile((int)charunit.position.x, (int)charunit.position.y);
        GridSquareScript targetTile = GridScript.GetTile((int)chartarget.position.x, (int)chartarget.position.y);

        AllStatsSkillBonus UnitSkillBonus = unit.GetComponent<UnitScript>().GetStatSkillBonus(target);
        AllStatsSkillBonus TargetSkillBonus = target.GetComponent<UnitScript>().GetStatSkillBonus(unit);

        int hitrateweapon = unit.GetComponent<UnitScript>().GetFirstWeapon().BaseHit;

        int tilebonus = GetTileBonus(unitTile, targetTile);

        int dexunit = charunit.stats.Dexterity + UnitSkillBonus.Dexterity;
        if (unit.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "sword")
        {
            dexunit = (int)(dexunit * 1.1f);
        }
        if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "spear")
        {
            dexunit = (int)(dexunit * 0.9f);
        }

        int spdtarget = chartarget.stats.Speed + TargetSkillBonus.Speed;
        if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "sword")
        {
            spdtarget = (int)(spdtarget * 1.1f);
        }
        if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "greatsword")
        {
            spdtarget = (int)(spdtarget * 0.9f);
        }

        int finalhitrate = (int)(hitrateweapon + (dexunit - spdtarget) * 0.2f) + tilebonus + UnitSkillBonus.Hit - TargetSkillBonus.Dodge;

        if (finalhitrate < 0)
        {
            finalhitrate = 0;
        }
        if (finalhitrate > 100)
        {
            finalhitrate = 100;
        }

        return finalhitrate;

    }

    private int GetTileBonus(GridSquareScript unitTile, GridSquareScript targetTile)
    {
        int tilebonus = 0;

        string unittype = unitTile.type;
        string targettype = unitTile.type;

        if (unittype == "Forest")
        {
            tilebonus += 30;
        }
        else if (unittype == "Ruins")
        {
            tilebonus += 10;
        }
        else if (unittype == "HighGround")
        {
            tilebonus += 10;
        }
        else if (unittype == "Water")
        {
            tilebonus -= 10;
        }

        if (targettype == "Ruins")
        {
            tilebonus += 10;
        }
        else if (targettype == "HighGround")
        {
            tilebonus -= 10;
        }

        return tilebonus;
    }

    public int CalculateCrit(GameObject unit, GameObject target)
    {
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;

        AllStatsSkillBonus UnitSkillBonus = unit.GetComponent<UnitScript>().GetStatSkillBonus(target);
        AllStatsSkillBonus TargetSkillBonus = target.GetComponent<UnitScript>().GetStatSkillBonus(unit);

        int critweapon = unit.GetComponent<UnitScript>().GetFirstWeapon().BaseCrit;

        int dexunit = charunit.stats.Dexterity + UnitSkillBonus.Dexterity;
        if (unit.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "sword")
        {
            dexunit = (int)(dexunit * 1.1f);
        }
        if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "spear")
        {
            dexunit = (int)(dexunit * 0.9f);
        }

        int spdtarget = chartarget.stats.Speed + TargetSkillBonus.Speed;
        if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "sword")
        {
            spdtarget = (int)(spdtarget * 1.1f);
        }
        if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "greatsword")
        {
            spdtarget = (int)(spdtarget * 0.9f);
        }

        int finalcritrate = (int)(critweapon + dexunit / 15f - spdtarget / 20f + UnitSkillBonus.Crit);

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

    private int ManhattanDistance(Character unit, Character otherunit)
    {
        return (int)(Mathf.Abs(unit.position.x - otherunit.position.x) + Mathf.Abs(unit.position.y - otherunit.position.y));
    }

}
