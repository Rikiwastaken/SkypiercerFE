using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using static UnitScript;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;
public class EnemyTurnScript : MonoBehaviour
{

    private TurnManger TurnManager;

    private GameObject CurrentEnemy;
    private GameObject CurrentPlayable;
    private GameObject CurrentOther;

    private GridScript gridScript;

    private battlecameraScript battlecamera;

    public ActionsMenu ActionsMenu;

    public float delaybeforeMove;
    private int counterbeforemove;
    public float delaybeforeAttack;
    private int counterbeforeattack;
    private bool waittingforcamera;

    public float delaybeforeFirstAttack;
    private int counterbeforeFirstAttack;

    private bool unitalreadyattacked;

    public float delaybetweenAttack;
    private int counterbetweenattack;

    public float delayafterAttack;
    private int counterafterattack;

    private bool attacktrigger;

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
        if (TurnManager.currentlyplaying != "enemy")
        {
            CurrentEnemy = null;
        }
        if (TurnManager.currentlyplaying != "other")
        {
            CurrentOther = null;
        }
        if(TurnManager.currentlyplaying != "playable" && FindAnyObjectByType<ActionManager>() != null)
        {
            FindAnyObjectByType<ActionManager>().preventfromlockingafteraction = true;
        }

        if (CurrentEnemy == null && TurnManager.currentlyplaying == "enemy")
        {
            foreach (GameObject unit in gridScript.allunitGOs)
            {
                if(unit.gameObject != null && unit!=null)
                {
                    Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
                    if (charunit.affiliation == "enemy" && !charunit.alreadyplayed)
                    {
                        CurrentEnemy = unit;
                        counterbeforemove = (int)(delaybeforeMove / Time.fixedDeltaTime);
                        gridScript.ShowMovementOfUnit(unit);

                    }
                }
                
            }
        }
        else if (TurnManager.currentlyplaying == "enemy")
        {

            Character CharCurrentEnemy = CurrentEnemy.GetComponent<UnitScript>().UnitCharacteristics;
            battlecamera.Destination = CharCurrentEnemy.position;
            if (!CharCurrentEnemy.alreadymoved)
            {
                if (counterbeforemove > 0)
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
                        counterbeforeattack = (int)(delaybeforeAttack / Time.fixedDeltaTime);
                        CharCurrentEnemy.alreadymoved = true;
                    }
                    else
                    {
                        CharCurrentEnemy.position = DestinationVector;
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
                else if (waittingforcamera)
                {
                    GameObject target = CalculateBestTargetForOffensiveUnits(CurrentEnemy);
                    if (target == null)
                    {
                        CharCurrentEnemy.alreadyplayed = true;
                        CurrentEnemy = null;
                        battlecamera.incombat = false;
                        waittingforcamera = false;
                    }
                    else if (CurrentEnemy == null)
                    {
                        battlecamera.incombat = false;
                        waittingforcamera = false;
                    }
                    else
                    {
                        target.transform.forward = CurrentEnemy.transform.position- target.transform.position;
                        CurrentEnemy.transform.forward = target.transform.position- CurrentEnemy.transform.position;
                        Debug.Log(target.name);
                        Debug.Log(CurrentEnemy.name);
                        battlecamera.Destination = battlecamera.GoToFightCamera(CurrentEnemy, target);
                        if (Vector2.Distance(battlecamera.Destination, new Vector2(battlecamera.transform.position.x, battlecamera.transform.position.z)) <= 0.1f)
                        {
                            if(counterbeforeFirstAttack>0)
                            {
                                counterbeforeFirstAttack--;
                                if(attacktrigger)
                                {
                                    attacktrigger = false;
                                    CurrentEnemy.GetComponentInChildren<Animator>().SetTrigger("Attack");
                                }
                            }
                            else
                            {
                                if (!unitalreadyattacked)
                                {
                                    (int hits, int crits, int damage) = ActionsMenu.ApplyDamage(CurrentEnemy, target, unitalreadyattacked);
                                    FindAnyObjectByType<CombatTextScript>().UpdateInfo(damage, hits, crits, CurrentEnemy.GetComponent<UnitScript>().UnitCharacteristics, target.GetComponent<UnitScript>().UnitCharacteristics);
                                    unitalreadyattacked = true;
                                    counterbetweenattack = (int)(delaybetweenAttack / Time.fixedDeltaTime);
                                    target.GetComponentInChildren<Animator>().SetTrigger("Attack");
                                }
                                else
                                {
                                    if (counterbetweenattack > 0)
                                    {
                                        counterbetweenattack--;
                                    }
                                    else if (counterafterattack == 0)
                                    {
                                        (int hits, int crits, int damage) = ActionsMenu.ApplyDamage(CurrentEnemy, target, unitalreadyattacked);
                                        
                                        FindAnyObjectByType<CombatTextScript>().UpdateInfo(damage,hits, crits, target.GetComponent<UnitScript>().UnitCharacteristics, CurrentEnemy.GetComponent<UnitScript>().UnitCharacteristics);
                                        unitalreadyattacked = true;
                                        counterafterattack = (int)(delayafterAttack / Time.fixedDeltaTime);
                                    }
                                    else
                                    {
                                        counterafterattack--;
                                        if (counterafterattack <= 0)
                                        {
                                            CharCurrentEnemy.alreadyplayed = true;
                                            CurrentEnemy = null;
                                            battlecamera.incombat = false;
                                            waittingforcamera = false;
                                        }
                                    }
                                }
                            }
                        }
                    }


                }
                else
                {
                    GameObject target = CalculateBestTargetForOffensiveUnits(CurrentEnemy);
                    if (target != null)
                    {
                        waittingforcamera = true;
                        battlecamera.Destination = battlecamera.GoToFightCamera(CurrentEnemy, target);
                        unitalreadyattacked = false;
                        counterbeforeFirstAttack = (int)(delaybeforeFirstAttack / Time.fixedDeltaTime);
                        attacktrigger = true;
                        FindAnyObjectByType<CombatTextScript>().ResetInfo();
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

        //playerattack
        if (TurnManager.currentlyplaying == "playable")
        {
            if(ActionsMenu.target==null)
            {
                return;
            }
            Debug.Log("turn is player");
            GameObject CurrentPlayable = ActionsMenu.target;
            Character CurrentPlayableChar = CurrentPlayable.GetComponent<UnitScript>().UnitCharacteristics;
            if (ActionsMenu.confirmattack)
            {
                if (counterbeforeattack > 0)
                {
                    Debug.Log("Waitting before attack");
                    counterbeforeattack--;
                }
                else if(waittingforcamera)
                {
                    Debug.Log("Camera Moving");
                    GameObject target = ActionsMenu.targetlist[ActionsMenu.activetargetid];
                    if(target==null)
                    {
                        Debug.Log("Target null");
                        CurrentPlayableChar.alreadyplayed = true;
                        battlecamera.incombat = false;
                        waittingforcamera = false;
                    }
                    else if(CurrentPlayable == null)
                    {
                        Debug.Log("Defender null");
                        battlecamera.incombat = false;
                        waittingforcamera = false;
                    }
                    else
                    {
                        target.transform.forward = CurrentPlayable.transform.position - target.transform.position;
                        CurrentPlayable.transform.forward = target.transform.position - CurrentPlayable.transform.position;
                        battlecamera.Destination = battlecamera.GoToFightCamera(CurrentPlayable, target);
                        if (Vector2.Distance(battlecamera.Destination, new Vector2(battlecamera.transform.position.x, battlecamera.transform.position.z)) <= 0.1f)
                        {
                            if(counterbeforeFirstAttack> 0)
                            {
                                counterbeforeFirstAttack--;
                                if(attacktrigger)
                                {
                                    attacktrigger = false;
                                    CurrentPlayable.GetComponentInChildren<Animator>().SetTrigger("Attack");
                                }
                            }
                            else
                            {
                                Debug.Log("Camera placed");
                                if (!unitalreadyattacked)
                                {
                                    Debug.Log("attacker turn");
                                    (int hits, int crits, int damage) = ActionsMenu.ApplyDamage(CurrentPlayable, target, unitalreadyattacked);
                                    target.GetComponentInChildren<Animator>().SetTrigger("Attack");
                                    FindAnyObjectByType<CombatTextScript>().UpdateInfo(damage,hits, crits, CurrentPlayable.GetComponent<UnitScript>().UnitCharacteristics, target.GetComponent<UnitScript>().UnitCharacteristics);
                                    unitalreadyattacked = true;
                                    counterbetweenattack = (int)(delaybetweenAttack / Time.fixedDeltaTime);
                                }
                                else
                                {

                                    if (counterbetweenattack > 0)
                                    {
                                        Debug.Log("Waitting for Defender turn");
                                        counterbetweenattack--;
                                    }
                                    else if (counterafterattack == 0)
                                    {
                                        Debug.Log("Defender turn");
                                        (int hits, int crits, int damage) = ActionsMenu.ApplyDamage(CurrentPlayable, target, unitalreadyattacked);
                                        FindAnyObjectByType<CombatTextScript>().UpdateInfo(damage,hits, crits, target.GetComponent<UnitScript>().UnitCharacteristics, CurrentPlayable.GetComponent<UnitScript>().UnitCharacteristics);
                                        unitalreadyattacked = true;
                                        counterafterattack = (int)(delayafterAttack / Time.fixedDeltaTime);
                                    }
                                    else
                                    {
                                        counterafterattack--;
                                        if (counterafterattack <= 0)
                                        {
                                            Debug.Log("End of combat");
                                            battlecamera.incombat = false;
                                            ActionsMenu.FinalizeAttack();
                                        }
                                    }
                                }
                            }
                                



                        }
                    }
                        
                    
                }
                else
                {
                    Debug.Log("Seting Up Camera");
                    GameObject target = ActionsMenu.targetlist[ActionsMenu.activetargetid];
                    if (target != null)
                    {
                        waittingforcamera = true;
                        battlecamera.Destination = battlecamera.GoToFightCamera(CurrentPlayable, target);
                        unitalreadyattacked = false;
                        counterbeforeFirstAttack = (int)(delaybeforeFirstAttack/ Time.fixedDeltaTime);
                        attacktrigger = true;
                        FindAnyObjectByType<CombatTextScript>().ResetInfo();
                    }
                    else
                    {
                        ActionsMenu.FinalizeAttack();
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
                    GridSquareScript Destination = CalculateDestinationForOffensiveUnits(CurrentOther, CharCurrentOther.attacksfriends);
                    Vector2 DestinationVector = CharCurrentOther.position;
                    if (Destination != null)
                    {
                        DestinationVector = Destination.GridCoordinates;
                        CharCurrentOther.position = DestinationVector;
                        counterbeforeattack = (int)(delaybeforeAttack / Time.fixedDeltaTime);
                        CharCurrentOther.alreadymoved = true;
                    }
                    else
                    {
                        CharCurrentOther.position = DestinationVector;
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
                else if (waittingforcamera)
                {
                    GameObject target = CalculateBestTargetForOffensiveUnits(CurrentOther,CharCurrentOther.attacksfriends);
                    if (target == null)
                    {
                        CharCurrentOther.alreadyplayed = true;
                        CurrentOther = null;
                        battlecamera.incombat = false;
                        waittingforcamera = false;
                    }
                    else if (CurrentOther == null)
                    {
                        battlecamera.incombat = false;
                        waittingforcamera = false;
                    }
                    else
                    {
                        target.transform.forward = CurrentOther.transform.position- target.transform.position;
                        CurrentOther.transform.forward = target.transform.position- CurrentOther.transform.position;
                        battlecamera.Destination = battlecamera.GoToFightCamera(CurrentOther, target);
                        if (Vector2.Distance(battlecamera.Destination, new Vector2(battlecamera.transform.position.x, battlecamera.transform.position.z)) <= 0.1f)
                        {
                            if(counterbeforeFirstAttack>0)
                            {
                                counterbeforeFirstAttack--;
                                if (attacktrigger)
                                {
                                    attacktrigger = false;
                                    CurrentOther.GetComponentInChildren<Animator>().SetTrigger("Attack");
                                }
                            }
                            else
                            {
                                if (!unitalreadyattacked)
                                {
                                    (int hits, int crits, int damage) = ActionsMenu.ApplyDamage(CurrentOther, target, unitalreadyattacked);
                                    
                                    FindAnyObjectByType<CombatTextScript>().UpdateInfo(damage,hits, crits, CurrentOther.GetComponent<UnitScript>().UnitCharacteristics, target.GetComponent<UnitScript>().UnitCharacteristics);
                                    unitalreadyattacked = true;
                                    counterbetweenattack = (int)(delaybetweenAttack / Time.fixedDeltaTime);
                                    target.GetComponentInChildren<Animator>().SetTrigger("Attack");
                                }
                                else
                                {
                                    if (counterbetweenattack > 0)
                                    {
                                        counterbetweenattack--;
                                    }
                                    else if (counterafterattack == 0)
                                    {
                                        (int hits, int crits, int damage) = ActionsMenu.ApplyDamage(CurrentOther, target, unitalreadyattacked);
                                        
                                        FindAnyObjectByType<CombatTextScript>().UpdateInfo(damage,hits, crits, target.GetComponent<UnitScript>().UnitCharacteristics, CurrentOther.GetComponent<UnitScript>().UnitCharacteristics);
                                        unitalreadyattacked = true;
                                        counterafterattack = (int)(delayafterAttack / Time.fixedDeltaTime);
                                    }
                                    else
                                    {
                                        counterafterattack--;
                                        if (counterafterattack <= 0)
                                        {
                                            CharCurrentOther.alreadyplayed = true;
                                            CurrentOther = null;
                                            battlecamera.incombat = false;
                                            waittingforcamera = false;
                                        }
                                    }
                                }
                            }
                            



                        }
                    }


                }
                else
                {
                    GameObject target = CalculateBestTargetForOffensiveUnits(CurrentOther, CharCurrentOther.attacksfriends);
                    if (target != null)
                    {
                        waittingforcamera = true;
                        battlecamera.Destination = battlecamera.GoToFightCamera(CurrentOther, target);
                        unitalreadyattacked = false;
                        counterbeforeFirstAttack = (int)(delaybeforeFirstAttack / Time.fixedDeltaTime);
                        attacktrigger = true;
                        FindAnyObjectByType<CombatTextScript>().ResetInfo();
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

        if(!battlecamera.incombat)
        {
            DeathCleanup();
        }
    }


    public GameObject CalculateBestTargetForOffensiveUnits(GameObject unit, bool attacksfriend = true)
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
                if (!attacksfriend)
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


                    if (reward > maxreward)
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
            if (RewardForDestination(currentCharacter, movement, attacksfriend) > maxreward)
            {
                bestsquare = movement;
                maxreward = RewardForDestination(currentCharacter, movement, attacksfriend);
            }
        }
        return bestsquare;
    }

    private int RewardForDestination(GameObject unit, GridSquareScript position, bool attacksfriend = true)
    {
        int reward = 0;
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        (int range, bool melee) = unit.GetComponent<UnitScript>().GetRangeAndMele();
        List<GridSquareScript> potentialAttackPosition = gridScript.GetAttack(range, melee, position);
        foreach (GridSquareScript tile in potentialAttackPosition)
        {
            foreach (GameObject otherunit in gridScript.allunitGOs)
            {
                Character charotherunit = otherunit.GetComponent<UnitScript>().UnitCharacteristics;
                string affiliationtoattack = "playable";
                if (!attacksfriend)
                {
                    affiliationtoattack = "enemy";
                }
                if (charotherunit.affiliation == affiliationtoattack && charotherunit.position == tile.GridCoordinates)
                {
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
                }
                else
                {
                    reward += ManhattanDistance(charunit, charotherunit) - charunit.equipments[0].Range;
                }
            }

            foreach (GameObject otherunit in gridScript.allunitGOs)
            {
                if (otherunit.GetComponent<UnitScript>().UnitCharacteristics.position == position.GridCoordinates)
                {
                    reward -= 9999;
                }
            }
        }
        return reward;

    }

    private int ManhattanDistance(Character unit, Character otherunit)
    {
        return (int)(Mathf.Abs(unit.position.x - otherunit.position.x) + Mathf.Abs(unit.position.y - otherunit.position.y));
    }

    private void DeathCleanup()
    {
        List<GameObject> objecttodelete = new List<GameObject>();
        foreach(GameObject unit in gridScript.allunitGOs)
        {
            if(unit.GetComponent<UnitScript>().UnitCharacteristics.currentHP<=0)
            {
                Destroy(unit);
                objecttodelete.Add(unit);
                
            }
        }
        foreach(GameObject unittodelete in objecttodelete)
        {
            gridScript.allunitGOs.Remove(unittodelete);
        }
    }

}
