using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using static UnitScript;
using static UnityEngine.GraphicsBuffer;

public class ActionManager : MonoBehaviour
{

    private TurnManger TurnManager;

    private InputManager InputManager;

    private GridScript GridScript;

    private Vector2 previouscoordinates;

    public GameObject currentcharacter;

    public int frameswherenotlock = 5;

    public int framestoskip;

    private battlecameraScript battlecamera;

    public GameObject actionsMenu;

    public bool preventfromlockingafteraction;

    private TextBubbleScript TextBubbleScript;

    public List<GridSquareScript> currentpath;
    private bool recalculatingpath;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TurnManager = GetComponent<TurnManger>();
        InputManager = FindAnyObjectByType<InputManager>();
        GridScript = GetComponent<GridScript>();
        battlecamera = FindAnyObjectByType<battlecameraScript>();
        TextBubbleScript = FindAnyObjectByType<TextBubbleScript>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (actionsMenu.activeSelf)
        {
            if (InputManager.Telekinesisjustpressed && !battlecamera.incombat && GameObject.Find("Attackwindow") == null)
            {
                currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.telekinesisactivated = !currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.telekinesisactivated;
                (int weaponrange, bool melee, string type) = currentcharacter.GetComponent<UnitScript>().GetRangeMeleeAndType();
                GridScript.ShowAttackAfterMovement(weaponrange, melee, GridScript.selection, type.ToLower() == "staff");
            }
            return;
        }

        if (framestoskip > 0)
        {
            framestoskip--;
            return;
        }

        if (TurnManager.currentlyplaying == "playable")
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
                    if (currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable" && InputManager.activatejustpressed && !preventfromlockingafteraction && frameswherenotlock == 0 && !TextBubbleScript.indialogue)
                    {
                        GridScript.lockselection = true;
                        GridScript.LockcurrentSelection();
                        GridScript.Recolor();
                    }
                    if (InputManager.Telekinesisjustpressed && !battlecamera.incombat)
                    {
                        currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.telekinesisactivated = !currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.telekinesisactivated;
                        GridScript.ShowMovement();
                    }
                    if (InputManager.NextWeaponjustpressed)
                    {
                        currentcharacter.GetComponent<UnitScript>().GetNextWeapon();
                        GridScript.ShowMovement();
                    }
                    if (InputManager.PreviousWeaponjustpressed)
                    {
                        currentcharacter.GetComponent<UnitScript>().GetPreviousWeapon();
                        GridScript.ShowMovement();
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
                    if (InputManager.Telekinesisjustpressed && !battlecamera.incombat)
                    {
                        currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.telekinesisactivated = !currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.telekinesisactivated;
                        WeaponChange(currentcharacter);
                        GridScript.ShowMovement();
                    }
                    if (InputManager.canceljustpressed && !currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.alreadymoved)
                    {
                        currentcharacter = null;
                        GridScript.ResetAllSelections();
                        GridScript.Recolor();
                        currentpath = null;
                    }
                }
                else
                {
                    currentpath = null;
                }

                ManagePath();

                if (GridScript.checkifvalidpos(GridScript.lockedmovementtiles, GridScript.selection.GridCoordinates, currentcharacter) && InputManager.activatejustpressed && !currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.alreadymoved)
                {
                    currentpath = null;
                    previouscoordinates = currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.position;
                    currentcharacter.GetComponent<UnitScript>().MoveTo(GridScript.selection.GridCoordinates);
                    currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.alreadymoved = true;
                    if (currentcharacter.GetComponent<UnitScript>().GetSkill(31))
                    {
                        int movements = currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.movements;
                        if (currentcharacter.GetComponent<UnitScript>().GetSkill(1))//checking if unit is using canto/Retreat
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
                        GridScript.ShowAttackAfterMovement(weaponrange, melee, GridScript.selection, type.ToLower() == "staff");
                        GridScript.LockcurrentSelection();
                        GridScript.actionsMenu.SetActive(true);
                        for (int i = 0; i < GridScript.actionsMenu.transform.childCount; i++)
                        {
                            GridScript.actionsMenu.transform.GetChild(i).gameObject.SetActive(true);
                            if (i == 0)
                            {
                                GridScript.actionsMenu.transform.GetChild(i).GetComponent<Button>().Select();
                            }
                        }
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

    private void ManagePath()
    {
        if(recalculatingpath)
        {
            recalculatingpath = false;
            bool legalposition = true;
            GridSquareScript tile = GridScript.selection;
            if (tile.isobstacle || !tile.activated)
            {
                legalposition = false;
            }
            bool checkifinmovements = false;
            foreach (GridSquareScript movementtile in GridScript.lockedmovementtiles)
            {
                if (GridScript.selection == movementtile)
                {
                    Debug.Log("found in movements");
                    checkifinmovements = true;
                    break;
                }
            }
            if (!checkifinmovements)
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
            Debug.Log("legal position " + legalposition);
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
        if (InputManager.movementjustpressed)
        {
            recalculatingpath = true;
        }
    }

    private void WeaponChange(GameObject unit)
    {
        (int range, bool frapperenmelee) = unit.GetComponent<UnitScript>().GetRangeAndMele();
        bool usestaff = unit.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "staff";
        GridScript.ShowAttack(range, frapperenmelee, usestaff, true);
        GridScript.lockedattacktiles = GridScript.attacktiles;
        GridScript.lockedhealingtiles = GridScript.healingtiles;
        GridScript.Recolor();
    }

    public void ResetAction()
    {
        if (currentcharacter != null)
        {
            currentcharacter.GetComponent<UnitScript>().MoveTo( previouscoordinates);
            currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.alreadymoved = false;
        }

        currentcharacter = null;
        GridScript.ResetAllSelections();
        GridScript.ResetColor();
        GridScript.ShowMovement();
    }

    public void Wait() // permet d'attendre
    {
        currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.alreadyplayed = true;
        currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.alreadymoved = true;
        currentcharacter.GetComponent<UnitScript>().numberoftimeswaitted++;
        currentcharacter.GetComponent<UnitScript>().waittedbonusturns = 2;

        //Readiness
        if (currentcharacter.GetComponent<UnitScript>().GetSkill(42))
        {
            currentcharacter.GetComponent<UnitScript>().AddNumber(0, true, "Readiness");
        }
        //patient
        if (currentcharacter.GetComponent<UnitScript>().GetSkill(20))
        {
            Character currentcharacterchar = currentcharacter.GetComponent<UnitScript>().UnitCharacteristics;
            currentcharacter.GetComponent<UnitScript>().AddNumber(Mathf.Min((int)(currentcharacterchar.AjustedStats.HP * 0.1f), (int)currentcharacterchar.AjustedStats.HP - currentcharacterchar.currentHP), true, "Patient");
            currentcharacterchar.currentHP += (int)(currentcharacterchar.AjustedStats.HP * 0.1f);
            if (currentcharacterchar.currentHP > currentcharacterchar.AjustedStats.HP)
            {
                currentcharacterchar.currentHP = (int)currentcharacterchar.AjustedStats.HP;
            }

            currentcharacter.GetComponent<UnitScript>().RestoreUses(1);
            if (currentcharacter.GetComponent<UnitScript>().GetSkill(7)) // full of beans
            {
                currentcharacter.GetComponent<UnitScript>().AddNumber(0, true, "Full of Beans");
                currentcharacter.GetComponent<UnitScript>().RestoreUses(1);
            }

        }

        currentcharacter.GetComponent<UnitScript>().RestoreUses(1);
        GridScript.ResetAllSelections();
        GridScript.ResetColor();
        GridScript.lockselection = false;
        FindAnyObjectByType<ActionManager>().frameswherenotlock = 10;
        GameObject character = currentcharacter;
        currentcharacter = null;
        character.GetComponent<UnitScript>().RetreatTrigger(); // Canto/Retreat (move again after action)
    }

    public void Attack()
    {
        ActionsMenu actionsMenu = FindAnyObjectByType<ActionsMenu>();
        actionsMenu.target = currentcharacter;
        actionsMenu.AttackCommand();

    }
}
