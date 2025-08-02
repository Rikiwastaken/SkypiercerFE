using System;
using UnityEngine;
using static UnitScript;
using UnityEngine.UI;

public class ActionManager : MonoBehaviour
{

    private TurnManger TurnManager;

    private InputManager InputManager;

    private GridScript GridScript;

    private Vector2 previouscoordinates;

    private GameObject currentcharacter;

    [Serializable]
    public class AttackStats
    {
        public string attackername;
        public int Damage;
        public int hitchance;
        public int critchange;
    }

    public GameObject actionsMenu;

    public bool preventfromlockingafteraction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TurnManager = GetComponent<TurnManger>();
        InputManager = FindAnyObjectByType<InputManager>();
        GridScript = GetComponent<GridScript>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (actionsMenu.activeSelf)
        {
            return;
        }

        if (TurnManager.currentlyplaying == "playable")
        {
            if (!GridScript.lockselection)
            {
                currentcharacter = GridScript.GetSelectedUnitGameObject();
                if (currentcharacter != null)
                {
                    if (currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable" && InputManager.activatejustpressed && !preventfromlockingafteraction)
                    {
                        GridScript.lockselection = true;
                        GridScript.LockcurrentSelection();
                        GridScript.Recolor();
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
                        WeaponChange();
                    }
                    if (InputManager.PreviousWeaponjustpressed)
                    {
                        currentcharacter.GetComponent<UnitScript>().GetPreviousWeapon();
                        WeaponChange();
                    }
                    if (InputManager.Telekinesisjustpressed)
                    {
                        currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.telekinesisactivated = !currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.telekinesisactivated;
                        WeaponChange();
                    }
                    if (InputManager.canceljustpressed && !currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.alreadymoved)
                    {
                        currentcharacter = null;
                        GridScript.ResetAllSelections();
                        GridScript.Recolor();
                    }
                }



                if (GridScript.checkifvalidpos(GridScript.lockedmovementtiles, GridScript.selection.GridCoordinates) && InputManager.activatejustpressed && !currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.alreadymoved)
                {
                    previouscoordinates = currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.position;
                    currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.position = GridScript.selection.GridCoordinates;
                    currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.alreadymoved = true;
                    GridScript.UnlockSelection();

                    (int weaponrange, bool melee, string type) = currentcharacter.GetComponent<UnitScript>().GetRangeMeleeAndType();
                    GridScript.ShowAttackAfterMovement(weaponrange, melee, GridScript.selection,type.ToLower() == "staff");
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
        preventfromlockingafteraction = false;
    }

    private void WeaponChange()
    {
        (int range, bool frapperenmelee) = currentcharacter.GetComponent<UnitScript>().GetRangeAndMele();
        GridScript.ShowAttack(range, frapperenmelee, true);
        GridScript.lockedattacktiles = GridScript.attacktiles;
        GridScript.Recolor();
    }

    public void ResetAction()
    {
        currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.position = previouscoordinates;
        currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.alreadymoved = false;
        currentcharacter = null;
        GridScript.ResetAllSelections();
        GridScript.ResetColor();
        GridScript.ShowMovement();
    }

    public void Wait()
    {
        currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.alreadyplayed = true;
        currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.alreadymoved = true;
        currentcharacter.GetComponent<UnitScript>().RestoreUses(1);
        currentcharacter = null;
        GridScript.ResetAllSelections();
        GridScript.ResetColor();
        preventfromlockingafteraction = true;
    }

    public void Attack()
    {
        ActionsMenu actionsMenu = FindAnyObjectByType<ActionsMenu>();
        actionsMenu.target = currentcharacter;
        actionsMenu.AttackCommand();

    }

    public (AttackStats, AttackStats) AttackValuesCalculator(Character attacker, Character target)
    {
        return (new AttackStats(), new AttackStats());
    }
}
