using System;
using UnityEngine;
using static UnitScript;

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
        if(actionsMenu.activeSelf)
        {
            return;
        }

        if(TurnManager.currentlyplaying=="playable")
        {
            if(!GridScript.lockselection)
            {
                currentcharacter = GridScript.GetSelectedUnitGameObject();
                if (currentcharacter != null)
                {
                    if (currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable" && InputManager.activatejustpressed)
                    {
                        GridScript.lockselection = true;
                        GridScript.LockcurrentSelection();
                        GridScript.Recolor();
                    }
                }
            }
            else if(GridScript.checkifvalidpos(GridScript.lockedmovementtiles, GridScript.selection.GridCoordinates) && InputManager.activatejustpressed && !currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.alreadymoved)
            {
                previouscoordinates = currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.position;
                currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.position = GridScript.selection.GridCoordinates;
                currentcharacter.transform.position = new Vector3(GridScript.selection.GridCoordinates.x, currentcharacter.transform.position.y, GridScript.selection.GridCoordinates.y);
                currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.alreadymoved = true;
                GridScript.UnlockSelection();

                (int weaponrange,bool melee) = currentcharacter.GetComponent<UnitScript>().GetRangeAndMele();
                GridScript.ShowAttackAfterMovement(weaponrange, melee, GridScript.selection);
                GridScript.LockcurrentSelection();
                GridScript.actionsMenu.SetActive(true);
            }

        }
    }

    public void ResetAction()
    {
        currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.position = previouscoordinates;
        currentcharacter.transform.position = new Vector3(previouscoordinates.x, currentcharacter.transform.position.y, previouscoordinates.y);
        currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.alreadymoved = false;
        currentcharacter = null;
        GridScript.UnlockSelection();
        GridScript.ResetColor();
    }

    public (AttackStats,AttackStats) AttackValuesCalculator(Character attacker, Character target)
    {
        return (new AttackStats(),new AttackStats());
    }
}
