using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using static UnitScript;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;
public class EnemyTurnScript : MonoBehaviour
{

    private TurnManger TurnManager;

    private GameObject CurrentEnemy;
    private GameObject CurrentOther;

    private GridScript gridScript;

    private battlecameraScript battlecamera;

    public ActionsMenu ActionsMenu;

    public float delaybeforeMove;
    private int counterbeforemove;
    public float delaybeforeAttack;
    private int counterbeforeattack;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TurnManager = GetComponent<TurnManger>();
        gridScript = GetComponent<GridScript>();
        battlecamera = FindAnyObjectByType<battlecameraScript>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(TurnManager.currentlyplaying != "enemy")
        {
            CurrentEnemy = null;
        }
        if (TurnManager.currentlyplaying != "other")
        {
            CurrentOther = null;
        }
        if (TurnManager.currentlyplaying != "enemy" && TurnManager.currentlyplaying != "other")
        {
            return;
        }

        if (CurrentEnemy == null && TurnManager.currentlyplaying == "enemy")
        {
            foreach(GameObject unit in gridScript.allunitGOs)
            {
                Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
                if(charunit.affiliation== "enemy" && !charunit.alreadyplayed)
                {
                    CurrentEnemy = unit;
                    counterbeforemove = (int)(delaybeforeMove / Time.fixedDeltaTime);
                    gridScript.ShowMovementOfUnit(unit);

                }
            }
        }
        else if (TurnManager.currentlyplaying == "enemy")
        {
            
            Character CharCurrentEnemy = CurrentEnemy.GetComponent<UnitScript>().UnitCharacteristics;
            battlecamera.Destination = CharCurrentEnemy.position;
            if (!CharCurrentEnemy.alreadymoved)
            {
                if(counterbeforemove > 0)
                {
                    counterbeforemove--;
                }
                else
                {
                    GridSquareScript Destination = CalculateDestinationForOffensiveUnits(CurrentEnemy);
                    Vector2 DestinationVector = CharCurrentEnemy.position;
                    if (Destination != null)
                    {
                        DestinationVector = Destination.GridCoordinates;
                        CharCurrentEnemy.position = DestinationVector;
                        CurrentEnemy.transform.position = new Vector3(DestinationVector.x, CurrentEnemy.transform.position.y, DestinationVector.y);
                        counterbeforeattack = (int)(delaybeforeAttack / Time.fixedDeltaTime);
                        CharCurrentEnemy.alreadymoved = true;
                    }
                    else
                    {
                        CharCurrentEnemy.position = DestinationVector;
                        CurrentEnemy.transform.position = new Vector3(DestinationVector.x, CurrentEnemy.transform.position.y, DestinationVector.y);
                        counterbeforeattack = 0;
                        CharCurrentEnemy.alreadymoved = true;
                    }
                    
                }
            }
            else
            {
                if (counterbeforeattack > 0)
                {
                    counterbeforeattack--;
                }
                else
                {
                    GameObject target = CalculateBestTargetForOffensiveUnits(CurrentEnemy);
                    if(target != null)
                    {
                        ActionsMenu.ApplyDamage(CurrentEnemy, target);
                        CharCurrentEnemy.alreadyplayed = true;
                        CurrentEnemy = null;
                    }
                    else
                    {
                        CharCurrentEnemy.alreadyplayed = true;
                        CurrentEnemy = null;
                    }
                    gridScript.ResetAllSelections();
                }
            }
        }

        if (CurrentOther == null && TurnManager.currentlyplaying == "other")
        {
            foreach (GameObject unit in gridScript.allunitGOs)
            {
                Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
                if (charunit.affiliation == "other" && !charunit.alreadyplayed)
                {
                    CurrentOther = unit;
                    counterbeforemove = (int)(delaybeforeMove / Time.fixedDeltaTime);
                    gridScript.ShowMovementOfUnit(unit);

                }
            }
        }
        else if (TurnManager.currentlyplaying == "other")
        {

            Character CharCurrentOther = CurrentOther.GetComponent<UnitScript>().UnitCharacteristics;
            battlecamera.Destination = CharCurrentOther.position;
            if (!CharCurrentOther.alreadymoved)
            {
                if (counterbeforemove > 0)
                {
                    counterbeforemove--;
                }
                else
                {
                    GridSquareScript Destination = CalculateDestinationForOffensiveUnits(CurrentOther,CharCurrentOther.attacksfriends);
                    Vector2 DestinationVector = CharCurrentOther.position;
                    if (Destination != null)
                    {
                        DestinationVector = Destination.GridCoordinates;
                        CharCurrentOther.position = DestinationVector;
                        CurrentOther.transform.position = new Vector3(DestinationVector.x, CurrentOther.transform.position.y, DestinationVector.y);
                        counterbeforeattack = (int)(delaybeforeAttack / Time.fixedDeltaTime);
                        CharCurrentOther.alreadymoved = true;
                    }
                    else
                    {
                        CharCurrentOther.position = DestinationVector;
                        CurrentOther.transform.position = new Vector3(DestinationVector.x, CurrentOther.transform.position.y, DestinationVector.y);
                        counterbeforeattack = 0;
                        CharCurrentOther.alreadymoved = true;
                    }

                }
            }
            else
            {
                if (counterbeforeattack > 0)
                {
                    counterbeforeattack--;
                }
                else
                {
                    GameObject target = CalculateBestTargetForOffensiveUnits(CurrentOther, CharCurrentOther.attacksfriends);
                    if (target != null)
                    {
                        ActionsMenu.ApplyDamage(CurrentOther, target);
                        CharCurrentOther.alreadyplayed = true;
                        CurrentOther = null;
                    }
                    else
                    {
                        CharCurrentOther.alreadyplayed = true;
                        CurrentOther = null;
                    }
                    gridScript.ResetAllSelections();
                }
            }
        }
    }


    private GameObject CalculateBestTargetForOffensiveUnits(GameObject unit, bool attacksfriend=true)
    {
        int maxreward = 0;
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        (int range, bool melee) = unit.GetComponent<UnitScript>().GetRangeAndMele();
        List<GridSquareScript> potentialAttackPosition = gridScript.GetAttack(range, melee, gridScript.GetTile(charunit.position));
        GameObject chosenUnit = null;

        foreach (GridSquareScript tile in potentialAttackPosition)
        {
            foreach (GameObject otherunit in gridScript.allunitGOs)
            {
                Character charotherunit = otherunit.GetComponent<UnitScript>().UnitCharacteristics;
                string affiliationtoattack = "playable";
                if(!attacksfriend)
                {
                    affiliationtoattack = "enemy";
                }
                if (charotherunit.affiliation == affiliationtoattack && charotherunit.position == tile.GridCoordinates)
                {
                    int reward = 0;
                    //that means that an enemy unit is in the zone
                    int rawdamage = ActionsMenu.CalculateDamage(unit, otherunit);
                    int rawdamagetaken = ActionsMenu.CalculateDamage(otherunit, unit);
                    int hitrate = ActionsMenu.CalculateHit(unit, otherunit);
                    int dodgerate = 100 - ActionsMenu.CalculateHit(otherunit, unit);

                    bool inrange = ActionsMenu.CheckifInRange(unit, otherunit);
                    if (!inrange)
                    {
                        rawdamagetaken = 0;
                    }

                    (GameObject doubleattacker, bool tripleattack) = ActionsMenu.CalculatedoubleAttack(unit, otherunit);
                    int potentialdamage = rawdamage;
                    if (doubleattacker == unit)
                    {
                        if (tripleattack)
                        {
                            potentialdamage *= 3;
                        }
                        else
                        {
                            potentialdamage *= 2;
                        }
                    }

                    if (potentialdamage > 0)
                    {
                        reward += 20;
                        if (potentialdamage >= charotherunit.currentHP)
                        {
                            reward += 20;
                        }
                        reward += (int)(hitrate / 10f);
                    }


                    int potentialdamagetaken = rawdamagetaken;

                    if (doubleattacker == otherunit)
                    {
                        if (tripleattack)
                        {
                            potentialdamagetaken *= 3;
                        }
                        else
                        {
                            potentialdamagetaken *= 2;
                        }
                    }

                    if (potentialdamagetaken == 0)
                    {
                        reward += 10;
                    }
                    else
                    {
                        reward += (int)(dodgerate / 10f);
                        if (potentialdamagetaken <= charunit.currentHP)
                        {
                            reward += 5;
                        }
                    }


                    if(reward > maxreward)
                    {
                        chosenUnit = otherunit;
                        maxreward = reward;
                    }
                }
            }
        }
        return chosenUnit;
    }
    
    private GridSquareScript CalculateDestinationForOffensiveUnits(GameObject currentCharacter, bool attacksfriend = true)
    {
        List<GridSquareScript> movementlist = gridScript.movementtiles;

        int maxreward = 0;

        GridSquareScript bestsquare = null;

        foreach (GridSquareScript movement in movementlist)
        {
            if(RewardForDestination(currentCharacter, movement, attacksfriend) > maxreward)
            {
                bestsquare = movement;
                maxreward = RewardForDestination(currentCharacter, movement, attacksfriend);
            }
        }
        return bestsquare;
    }

    private int RewardForDestination(GameObject unit,GridSquareScript position, bool attacksfriend = true)
    {
        int reward = 0;
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        (int range,bool melee) =unit.GetComponent<UnitScript>().GetRangeAndMele();
        List<GridSquareScript> potentialAttackPosition = gridScript.GetAttack(range, melee, position);
        foreach(GridSquareScript tile in potentialAttackPosition)
        {
            foreach (GameObject otherunit in gridScript.allunitGOs)
            {
                Character charotherunit = otherunit.GetComponent<UnitScript>().UnitCharacteristics;
                string affiliationtoattack = "playable";
                if (!attacksfriend)
                {
                    affiliationtoattack = "enemy";
                }
                if (charotherunit.affiliation == affiliationtoattack && charotherunit.position==tile.GridCoordinates)
                {
                    //that means that an enemy unit is in the zone
                    int rawdamage = ActionsMenu.CalculateDamage(unit, otherunit);
                    int rawdamagetaken = ActionsMenu.CalculateDamage(otherunit, unit);
                    int hitrate = ActionsMenu.CalculateHit(unit, otherunit);
                    int dodgerate =  100 - ActionsMenu.CalculateHit(otherunit, unit);

                    bool inrange = ActionsMenu.CheckifInRange(unit, otherunit);
                    if (!inrange)
                    {
                        rawdamagetaken = 0;
                    }

                    (GameObject doubleattacker, bool tripleattack) = ActionsMenu.CalculatedoubleAttack(unit, otherunit);
                    int potentialdamage = rawdamage;
                    if (doubleattacker == unit)
                    {
                        if(tripleattack)
                        {
                            potentialdamage *= 3;
                        }
                        else
                        {
                            potentialdamage *= 2;
                        }
                    }
                    
                    if (potentialdamage > 0)
                    {
                        reward += 20;
                        if (potentialdamage >= charotherunit.currentHP)
                        {
                            reward += 20;
                        }
                        reward += (int)(hitrate / 10f);
                    }
                    

                    int potentialdamagetaken = rawdamagetaken;

                    if(doubleattacker == otherunit)
                    {
                        if (tripleattack)
                        {
                            potentialdamagetaken *= 3;
                        }
                        else
                        {
                            potentialdamagetaken *= 2;
                        }
                    }

                    if (potentialdamagetaken == 0)
                    {
                        reward += 10;
                    }
                    else
                    {
                        reward += (int)(dodgerate / 10f);
                        if (potentialdamagetaken <= charunit.currentHP)
                        {
                            reward += 5;
                        }
                    } 
                }
                else 
                {
                    if (ManhattanDistance(charunit, charotherunit) <= 5)
                    {
                        reward += 1;
                    }     
                }
            }

            foreach(GameObject otherunit in gridScript.allunitGOs)
            {
                if(otherunit.GetComponent<UnitScript>().UnitCharacteristics.position==position.GridCoordinates)
                {
                    reward -= 9999;
                }
            }
        }
        return reward;

    }

    private int ManhattanDistance(Character unit, Character otherunit)
    {
        return (int) (Mathf.Abs(unit.position.x - otherunit.position.x)+ Mathf.Abs(unit.position.y - otherunit.position.y));
    }

}
