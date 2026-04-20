using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnitScript;

public class ActionManager : MonoBehaviour
{

    public static ActionManager instance;

    private TurnManger TurnManager;

    private InputManager InputManager;

    private GridScript GridScript;

    private Vector2 previouscoordinates;

    public GameObject currentcharacter;

    public int frameswherenotlock = 5;

    public int framestoskip;

    private cameraScript battlecamera;

    public GameObject actionsMenu;

    public bool preventfromlockingafteraction;

    private TextBubbleScript TextBubbleScript;

    public List<GridSquareScript> currentpath;
    private GridSquareScript previoustile;

    public GameObject NeutralMenu;

    public ForesightScript Foresight;

    public int NeutralMenuCD;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        TurnManager = GetComponent<TurnManger>();
        InputManager = InputManager.instance;
        GridScript = GetComponent<GridScript>();
        battlecamera = FindAnyObjectByType<cameraScript>();
        TextBubbleScript = FindAnyObjectByType<TextBubbleScript>(FindObjectsInactive.Include);
    }

    // Update is called once per frame
    void FixedUpdate()
    {


        if (NeutralMenu.activeSelf || GridScript.actionsMenu.activeSelf)
        {
            NeutralMenuCD = 5;
        }
        else if (NeutralMenuCD > 0)
        {
            NeutralMenuCD--;
        }

        if (actionsMenu.activeSelf)
        {
            if (InputManager.Telekinesisjustpressed && !battlecamera.incombat && GameObject.Find("Attackwindow") == null && currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable" && !currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.alreadyplayed && allowtelekinesisChangeFromTutorial())
            {
                currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.telekinesisactivated = !currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.telekinesisactivated;
                if (currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.statusEffects.ConcussionTunrs > 0)
                {
                    currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.telekinesisactivated = false;
                }
                currentcharacter.GetComponent<UnitScript>().UpdateWeaponModel();
                (int weaponrange, bool melee, string type) = currentcharacter.GetComponent<UnitScript>().GetRangeMeleeAndType();
                GridScript.ShowAttackAfterMovement(weaponrange, melee, new List<GridSquareScript>() { GridScript.selection }, type.ToLower() == "staff", currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.enemyStats.monsterStats.size, currentcharacter.GetComponent<UnitScript>().UnitCharacteristics);
            }
            return;
        }

        if (framestoskip > 0)
        {
            framestoskip--;
            return;
        }

        if ((TurnManager.currentlyplaying == "playable" || TurnManager.currentlyplaying == "tutorial") && (GameOverScript.instance == null || !GameOverScript.instance.gameObject.activeSelf))
        {
            if (currentcharacter != null)
            {
                if (currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.alreadymoved && currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.alreadyplayed)
                {
                    currentcharacter = null;
                }
            }


            if (currentcharacter == null)
            {
                GridScript.lockselection = false;
            }



            if (!GridScript.lockselection)
            {
                currentcharacter = GridScript.GetSelectedUnitGameObject();
                if (currentcharacter != null)
                {
                    if (!currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.alreadyplayed && currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable" && InputManager.activatejustpressed && !preventfromlockingafteraction && frameswherenotlock == 0 && !TextBubbleScript.indialogue)
                    {
                        GridScript.lockselection = true;
                        GridScript.LockcurrentSelection();
                        GridScript.Recolor();
                    }
                    else if (currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.affiliation != "playable" && InputManager.activatejustpressed && (TurnManager.currentlyplaying == "playable" || TurnManager.currentlyplaying == "tutorial") && NeutralMenuCD == 0 && !TextBubbleScript.indialogue)
                    {
                        NeutralMenu.SetActive(true);
                        EventSystem.current.SetSelectedGameObject(NeutralMenu.transform.GetChild(0).gameObject);
                    }
                    if (InputManager.Telekinesisjustpressed && !battlecamera.incombat && !currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.alreadyplayed && currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable" && allowtelekinesisChangeFromTutorial())
                    {

                        currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.telekinesisactivated = !currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.telekinesisactivated;
                        if (currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.statusEffects.ConcussionTunrs > 0)
                        {
                            currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.telekinesisactivated = false;
                        }
                        currentcharacter.GetComponent<UnitScript>().UpdateWeaponModel();
                        GridScript.ShowMovement();
                    }

                }
                else
                {
                    if (InputManager.activatejustpressed && (TurnManager.currentlyplaying == "playable" || TurnManager.currentlyplaying == "tutorial") && NeutralMenuCD == 0)
                    {
                        NeutralMenu.SetActive(true);
                        EventSystem.current.SetSelectedGameObject(NeutralMenu.transform.GetChild(0).gameObject);
                    }
                }
            }
            else
            {

                if (currentcharacter != null)
                {
                    if (InputManager.NextWeaponjustpressed)
                    {
                        currentcharacter.GetComponent<UnitScript>().GetNextWeapon();
                        WeaponChange(currentcharacter);
                        GridScript.ShowMovement();
                    }
                    if (InputManager.PreviousWeaponjustpressed)
                    {
                        currentcharacter.GetComponent<UnitScript>().GetPreviousWeapon();
                        WeaponChange(currentcharacter);
                        GridScript.ShowMovement();
                    }
                    if (InputManager.Telekinesisjustpressed && !battlecamera.incombat && !currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.alreadyplayed && currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable" && allowtelekinesisChangeFromTutorial())
                    {

                        currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.telekinesisactivated = !currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.telekinesisactivated;
                        if (currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.statusEffects.ConcussionTunrs > 0)
                        {
                            currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.telekinesisactivated = false;
                        }
                        WeaponChange(currentcharacter);
                        currentcharacter.GetComponent<UnitScript>().UpdateWeaponModel();
                        GridScript.ShowMovement();
                    }
                    if (InputManager.canceljustpressed && !currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.alreadymoved)
                    {
                        if (!currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.alreadyplayed)
                        {
                            currentcharacter = null;
                            GridScript.ResetAllSelections();
                            GridScript.Recolor();
                            currentpath = null;
                        }


                    }
                }
                else
                {
                    currentpath = null;
                }

                ManagePath();

                if (GridScript.checkifvalidpos(GridScript.lockedmovementtiles, GridScript.selection.GridCoordinates, currentcharacter) && InputManager.activatejustpressed && !currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.alreadymoved)
                {

                    MoveCharacterToSelection();
                }
                else if ((GridScript.lockedattacktiles.Contains(GridScript.selection) || GridScript.lockedhealingtiles.Contains(GridScript.selection)) && InputManager.activatejustpressed && !currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.alreadymoved)
                {
                    if (TutorialScript.instance == null || !TutorialScript.instance.enabled)
                    {
                        AttackDirectly();
                    }

                }
            }


        }
        if (frameswherenotlock > 0)
        {
            frameswherenotlock--;
            GridScript.UnlockSelection();
        }
        preventfromlockingafteraction = false;
    }

    private void MoveCharacterToSelection()
    {
        if (TutorialScript.instance != null && TutorialScript.instance.enabled)
        {
            if (!TutorialScript.instance.firstenemyattacked)
            {
                if (GridScript.selection != TutorialScript.instance.ZackTargetTile)
                {
                    return;
                }
            }
            else if (!TutorialScript.instance.secondenemyattacked)
            {
                if (GridScript.selection != TutorialScript.instance.ElwynTargetTile)
                {
                    return;
                }
            }
            else
            {
                if (GridScript.selection != TutorialScript.instance.LeaTargetTile)
                {
                    return;
                }
            }
        }
        currentpath = null;
        previouscoordinates = currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.position;
        currentcharacter.GetComponent<UnitScript>().MoveTo(GridScript.selection.GridCoordinates);
        currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.alreadymoved = true;
        if (currentcharacter.GetComponent<UnitScript>().GetSkill(31) || currentcharacter.GetComponent<UnitScript>().GetFirstWeapon().Name.ToLower().Contains("abyssal")) //verso or abyssal
        {
            int movements = currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.movements;
            if (currentcharacter.GetComponent<UnitScript>().GetSkill(1))//checking if unit is using Retreat
            {
                movements -= 2;
            }
            if (currentcharacter.GetComponent<UnitScript>().GetSkill(5)) // checking if unit is using Fast Legs
            {
                movements += 1;
            }
            currentcharacter.GetComponent<UnitScript>().tilesmoved = GridScript.findshortestpath(GridScript.GetTile(previouscoordinates), GridScript.selection, movements);
        }
        GridScript.UnlockSelection();
        if (currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.alreadyplayed)
        {
            GridScript.ResetAllSelections();
            currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.alreadymoved = true;
            GridScript.Recolor();
        }
        else
        {
            (int weaponrange, bool melee, string type) = currentcharacter.GetComponent<UnitScript>().GetRangeMeleeAndType();
            GridScript.ShowAttackAfterMovement(weaponrange, melee, new List<GridSquareScript>() { GridScript.selection }, type.ToLower() == "staff", currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.enemyStats.monsterStats.size, currentcharacter.GetComponent<UnitScript>().UnitCharacteristics);
            GridScript.LockcurrentSelection();
            if (!GridScript.actionsMenu.activeSelf)
            {
                GridScript.actionsMenu.SetActive(true);
            }

            for (int i = 0; i < GridScript.actionsMenu.transform.childCount; i++)
            {
                if (!GridScript.actionsMenu.transform.GetChild(i).gameObject.activeSelf)
                {
                    GridScript.actionsMenu.transform.GetChild(i).gameObject.SetActive(true);
                }

                if (i == 0)
                {
                    GridScript.actionsMenu.transform.GetChild(i).GetComponent<Button>().Select();
                }
            }
        }
    }

    private void AttackDirectly()
    {
        GameObject otherunit = GridScript.GetUnit(GridScript.selection);
        Character otherunitChar = otherunit.GetComponent<UnitScript>().UnitCharacteristics;

        Character currentChar = currentcharacter.GetComponent<UnitScript>().UnitCharacteristics;

        List<GridSquareScript> potentialmovementtiles = new List<GridSquareScript>();

        bool Ishealing = (otherunitChar.affiliation == "playable" || (otherunitChar.affiliation == "other" && !otherunitChar.attacksfriends));
        if (Ishealing)
        {
            if (otherunitChar.currentHP >= otherunitChar.AjustedStats.HP)
            {
                return;
            }
            // if we are healing it's kinda the same, but first check if the unit has a staff
            bool hasstaff = false;
            foreach (equipment equ in currentcharacter.GetComponent<UnitScript>().GetAllWeapons())
            {
                if (equ.type.ToLower() == "staff" && equ.Currentuses > 0)
                {
                    hasstaff = true;
                    currentcharacter.GetComponent<UnitScript>().EquipWeapon(equ);
                    break;
                }
            }
            if (hasstaff)
            {


                // first, we find movement tiles in which attack tiles englobe the unit because we can attack them
                (int newweaponrange, bool newmelee, string newtype) = currentcharacter.GetComponent<UnitScript>().GetRangeMeleeAndType();
                foreach (GridSquareScript movementtile in GridScript.lockedmovementtiles)
                {
                    GridScript.ShowAttackAfterMovement(newweaponrange, newmelee, new List<GridSquareScript>() { movementtile }, true, currentChar.enemyStats.monsterStats.size, currentChar);
                    if (GridScript.healingtiles.Contains(otherunitChar.currentTile[0]))
                    {
                        potentialmovementtiles.Add(movementtile);
                    }
                }

                //Then we get the closest tile

                int movements = currentChar.movements;
                if (currentcharacter.GetComponent<UnitScript>().GetSkill(1))//checking if unit is using Retreat
                {
                    movements -= 2;
                }
                if (currentcharacter.GetComponent<UnitScript>().GetSkill(5)) // checking if unit is using Fast Legs
                {
                    movements += 1;
                }

                int shortestdistance = 99;
                GridSquareScript besttile = null;
                foreach (GridSquareScript potentialtargettile in potentialmovementtiles)
                {
                    int pathdistance = GridScript.findshortestpath(currentChar.currentTile[0], potentialtargettile, movements);
                    if (pathdistance < shortestdistance)
                    {
                        besttile = potentialtargettile;
                        shortestdistance = pathdistance;
                    }
                }

                // then we move to said tile

                if (besttile != null)
                {
                    previouscoordinates = currentChar.position;
                    currentcharacter.GetComponent<UnitScript>().MoveTo(besttile.GridCoordinates);
                    currentChar.alreadymoved = true;
                    if (currentcharacter.GetComponent<UnitScript>().GetSkill(31) || currentcharacter.GetComponent<UnitScript>().GetFirstWeapon().Name.ToLower().Contains("abyssal")) //verso or abyssal
                    {
                        currentcharacter.GetComponent<UnitScript>().tilesmoved = GridScript.findshortestpath(GridScript.GetTile(previouscoordinates), GridScript.selection, movements);
                    }
                    GridScript.UnlockSelection();
                    (int weaponrange, bool melee, string type) = currentcharacter.GetComponent<UnitScript>().GetRangeMeleeAndType();
                    GridScript.ShowAttackAfterMovement(weaponrange, melee, new List<GridSquareScript>() { GridScript.selection }, type.ToLower() == "staff", currentChar.enemyStats.monsterStats.size, currentChar);
                    GridScript.LockcurrentSelection();
                    if (!GridScript.actionsMenu.activeSelf)
                    {
                        GridScript.actionsMenu.SetActive(true);
                    }

                    for (int i = 0; i < GridScript.actionsMenu.transform.childCount; i++)
                    {
                        if (!GridScript.actionsMenu.transform.GetChild(i).gameObject.activeSelf)
                        {
                            GridScript.actionsMenu.transform.GetChild(i).gameObject.SetActive(true);
                        }

                        if (i == 0)
                        {
                            GridScript.actionsMenu.transform.GetChild(i).GetComponent<Button>().Select();
                        }
                    }
                    GridScript.selection = besttile;
                    actionsMenu.GetComponent<ActionsMenu>().target = currentcharacter;
                    actionsMenu.GetComponent<ActionsMenu>().AttackCommand(true);
                    currentpath = null;
                }
            }
        }
        else
        {
            // first, we find movement tiles in which attack tiles englobe the unit because we can attack them
            foreach (GridSquareScript movementtile in GridScript.lockedmovementtiles)
            {
                (int newweaponrange, bool newmelee, string newtype) = currentcharacter.GetComponent<UnitScript>().GetRangeMeleeAndType();
                GridScript.ShowAttackAfterMovement(newweaponrange, newmelee, new List<GridSquareScript>() { movementtile }, false, currentChar.enemyStats.monsterStats.size, currentChar);
                if (GridScript.attacktiles.Contains(otherunitChar.currentTile[0]))
                {
                    potentialmovementtiles.Add(movementtile);
                }
            }

            //Then we get the closest tile

            int movements = currentChar.movements;
            if (currentcharacter.GetComponent<UnitScript>().GetSkill(1))//checking if unit is using Retreat
            {
                movements -= 2;
            }
            if (currentcharacter.GetComponent<UnitScript>().GetSkill(5)) // checking if unit is using Fast Legs
            {
                movements += 1;
            }

            int shortestdistance = 99;
            GridSquareScript besttile = null;
            foreach (GridSquareScript potentialtargettile in potentialmovementtiles)
            {
                int pathdistance = GridScript.findshortestpath(currentChar.currentTile[0], potentialtargettile, movements);
                if (pathdistance < shortestdistance)
                {
                    besttile = potentialtargettile;
                    shortestdistance = pathdistance;
                }
            }

            // then we move to said tile

            if (besttile != null)
            {
                previouscoordinates = currentChar.position;
                currentcharacter.GetComponent<UnitScript>().MoveTo(besttile.GridCoordinates);
                currentChar.alreadymoved = true;
                if (currentcharacter.GetComponent<UnitScript>().GetSkill(31) || currentcharacter.GetComponent<UnitScript>().GetFirstWeapon().Name.ToLower().Contains("abyssal")) //verso or abyssal
                {
                    currentcharacter.GetComponent<UnitScript>().tilesmoved = GridScript.findshortestpath(GridScript.GetTile(previouscoordinates), GridScript.selection, movements);
                }
                GridScript.UnlockSelection();
                (int weaponrange, bool melee, string type) = currentcharacter.GetComponent<UnitScript>().GetRangeMeleeAndType();
                GridScript.ShowAttackAfterMovement(weaponrange, melee, new List<GridSquareScript>() { GridScript.selection }, type.ToLower() == "staff", currentChar.enemyStats.monsterStats.size, currentChar);
                GridScript.LockcurrentSelection();
                if (!GridScript.actionsMenu.activeSelf)
                {
                    GridScript.actionsMenu.SetActive(true);
                }

                for (int i = 0; i < GridScript.actionsMenu.transform.childCount; i++)
                {
                    if (!GridScript.actionsMenu.transform.GetChild(i).gameObject.activeSelf)
                    {
                        GridScript.actionsMenu.transform.GetChild(i).gameObject.SetActive(true);
                    }

                    if (i == 0)
                    {
                        GridScript.actionsMenu.transform.GetChild(i).GetComponent<Button>().Select();
                    }
                }
                GridScript.selection = besttile;
                actionsMenu.GetComponent<ActionsMenu>().target = currentcharacter;
                actionsMenu.GetComponent<ActionsMenu>().AttackCommand(false);
                currentpath = null;
            }


        }
    }

    public void ManagePath()
    {
        if (currentcharacter == null)
        {
            currentpath = null;
        }
        else if (!currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.currentTile.Contains(GridScript.selection) && GridScript.selection != previoustile && GridScript.lockedmovementtiles.Contains(GridScript.selection))
        {
            previoustile = GridScript.selection;
            bool legalposition = true;
            GridSquareScript tile = GridScript.selection;
            if (tile.isobstacle || !tile.activated)
            {
                legalposition = false;
            }
            GameObject unit = GridScript.GetUnit(tile);
            if (unit != null)
            {
                Character unitchar = unit.GetComponent<UnitScript>().UnitCharacteristics;
                if (unitchar != currentcharacter.GetComponent<UnitScript>().UnitCharacteristics && unitchar.affiliation != currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.affiliation)
                {
                    legalposition = false;
                }
            }
            if (legalposition)
            {
                List<Vector2> path = new List<Vector2>();
                path = GridScript.FindPath(currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.position, GridScript.selection.GridCoordinates, currentcharacter.GetComponent<UnitScript>().UnitCharacteristics);
                currentpath = new List<GridSquareScript>();
                foreach (Vector2 coord in path)
                {
                    currentpath.Add(GridScript.GetTile(coord));
                }
            }
        }
        else if (currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.currentTile.Contains(GridScript.selection))
        {
            currentpath = null;
        }
    }

    private void WeaponChange(GameObject unit)
    {
        (int range, bool frapperenmelee) = unit.GetComponent<UnitScript>().GetRangeAndMele();
        bool usestaff = unit.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "staff";
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        GridScript.ShowAttack(range, frapperenmelee, usestaff, true, charunit.enemyStats.monsterStats.size, charunit);
        GridScript.lockedattacktiles = GridScript.attacktiles;
        GridScript.lockedhealingtiles = GridScript.healingtiles;
        GridScript.Recolor();
    }

    public void ResetAction()
    {
        if (currentcharacter != null)
        {
            currentcharacter.GetComponent<UnitScript>().MoveTo(previouscoordinates);
            currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.alreadymoved = false;
        }

        currentcharacter = null;
        GridScript.ResetAllSelections();
        GridScript.ResetColor();
        GridScript.ShowMovement();
    }

    public void WaitButton()
    {
        Wait();
    }

    public void Wait(GameObject Unit = null) // permet d'attendre
    {
        GameObject Unittouse = currentcharacter;
        if (Unit != null)
        {
            Unittouse = Unit;
        }

        Foresight.CreateAction(2, Unittouse);

        Unittouse.GetComponent<UnitScript>().UnitCharacteristics.alreadyplayed = true;
        Unittouse.GetComponent<UnitScript>().UnitCharacteristics.alreadymoved = true;
        Unittouse.GetComponent<UnitScript>().numberoftimeswaitted++;
        Unittouse.GetComponent<UnitScript>().waittedbonusturns = 2;

        //Readiness
        if (Unittouse.GetComponent<UnitScript>().GetSkill(42))
        {
            Unittouse.GetComponent<UnitScript>().AddNumber(0, true, "Readiness");
        }
        //patient
        if (Unittouse.GetComponent<UnitScript>().GetSkill(20))
        {
            Character currentcharacterchar = Unittouse.GetComponent<UnitScript>().UnitCharacteristics;
            Unittouse.GetComponent<UnitScript>().AddNumber(Mathf.Min((int)(currentcharacterchar.AjustedStats.HP * 0.1f), (int)currentcharacterchar.AjustedStats.HP - currentcharacterchar.currentHP), true, "Patient");
            currentcharacterchar.currentHP += (int)(currentcharacterchar.AjustedStats.HP * 0.1f);
            if (currentcharacterchar.currentHP > currentcharacterchar.AjustedStats.HP)
            {
                currentcharacterchar.currentHP = (int)currentcharacterchar.AjustedStats.HP;
            }

            Unittouse.GetComponent<UnitScript>().RestoreUses(1);
            if (Unittouse.GetComponent<UnitScript>().GetSkill(7)) // full of beans
            {
                Unittouse.GetComponent<UnitScript>().AddNumber(0, true, "Full of Beans");
                Unittouse.GetComponent<UnitScript>().RestoreUses(1);
            }

        }

        Unittouse.GetComponent<UnitScript>().RestoreUses(1);
        GridScript.ResetAllSelections();
        GridScript.ResetColor();
        GridScript.lockselection = false;
        frameswherenotlock = 10;
        currentcharacter = null;

        MapEventManager.instance.TriggerEventCheck();

    }

    public void Interract(GameObject Unit = null, GridSquareScript tilechanged = null, GameObject SpecialCommandUnit = null) // Appele quand l'unite parle a une autre unite ou qu'elle interragit avec un objet
    {
        GameObject Unittouse = currentcharacter;
        if (Unit != null)
        {
            Unittouse = Unit;
        }





        if (tilechanged != null)
        {
            Foresight.CreateAction(6, Unittouse);


        }
        if (SpecialCommandUnit != null)
        {
            Foresight.CreateAction(5, Unittouse, SpecialCommandUnit);
        }

        Unittouse.GetComponent<UnitScript>().UnitCharacteristics.alreadyplayed = true;
        Unittouse.GetComponent<UnitScript>().UnitCharacteristics.alreadymoved = true;

        Unittouse.GetComponent<UnitScript>().RestoreUses(1);
        GridScript.ResetAllSelections();
        GridScript.ResetColor();
        GridScript.lockselection = false;
        frameswherenotlock = 10;
        currentcharacter = null;
    }

    public void Attack()
    {
        ActionsMenu actionsMenu = FindAnyObjectByType<ActionsMenu>();
        actionsMenu.target = currentcharacter;
        actionsMenu.AttackCommand(false);

    }

    public void Heal()
    {
        ActionsMenu actionsMenu = FindAnyObjectByType<ActionsMenu>();
        actionsMenu.target = currentcharacter;
        actionsMenu.AttackCommand(true);

    }

    private bool allowtelekinesisChangeFromTutorial()
    {
        if (TutorialScript.instance != null && TutorialScript.instance.enabled && !TutorialScript.instance.firstenemyattacked)
        {
            return false;
        }
        return true;
    }
}
