using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using static UnitScript;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;
public class AttackTurnScript : MonoBehaviour
{

    private TurnManger TurnManager;

    public GameObject CurrentEnemy;
    public GameObject CurrentPlayable;
    public GameObject CurrentOther;

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

    public bool waittingforexp;

    public bool expdistributed;

    public ExpBarScript expbar;

    private int expgained;

    private List<int> levelupbonuses;

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
        if (TurnManager.currentlyplaying != "playable" && FindAnyObjectByType<ActionManager>() != null)
        {
            FindAnyObjectByType<ActionManager>().preventfromlockingafteraction = true;
        }

        if (CurrentEnemy == null && TurnManager.currentlyplaying == "enemy")
        {
            foreach (GameObject unit in gridScript.allunitGOs)
            {
                if (unit.gameObject != null && unit != null)
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
            if (!battlecamera.incombat)
            {
                battlecamera.Destination = CharCurrentEnemy.position;
            }
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

                ManageAttack(CurrentEnemy);
            }
        }

        //playerattack
        if (TurnManager.currentlyplaying == "playable")
        {
            if (ActionsMenu.target == null)
            {
                return;
            }
            GameObject CurrentPlayable = ActionsMenu.target;
            Character CurrentPlayableChar = CurrentPlayable.GetComponent<UnitScript>().UnitCharacteristics;
            if (ActionsMenu.confirmattack)
            {
                ManageAttack(CurrentPlayable);
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
                ManageAttack(CurrentOther);
            }
        }

        if (!battlecamera.incombat)
        {
            DeathCleanup();

        }
    }


    private void ManageAttack(GameObject Attacker)
    {
        Character CharAttacker = Attacker.GetComponent<UnitScript>().UnitCharacteristics;
        if (counterbeforeattack > 0)
        {
            counterbeforeattack--;

        }
        else if (counterafterattack > 0)
        {
            counterafterattack--;
        }
        else if (waittingforexp)
        {
            if (expdistributed)
            {
                expdistributed = false;
                waittingforexp = false;
                expbar.gameObject.SetActive(false);
                if (CharAttacker.affiliation == "playable")
                {
                    ActionsMenu.FinalizeAttack();
                    unitalreadyattacked = false;
                }
                else
                {
                    CharAttacker.alreadyplayed = true;
                    CurrentOther = null;
                    CurrentEnemy = null;
                    CurrentPlayable = null;
                    battlecamera.incombat = false;
                    waittingforcamera = false;
                    unitalreadyattacked = false;

                }
            }
            else
            {
                GameObject target = null;
                if (CharAttacker.affiliation != "playable")
                {
                    target = CalculateBestTargetForOffensiveUnits(Attacker, CharAttacker.attacksfriends);
                }
                else
                {
                    target = ActionsMenu.targetlist[ActionsMenu.activetargetid];
                }
                if (target == null)
                {
                    expdistributed = true;
                }
                else if (CharAttacker.affiliation == "playable")
                {
                    expbar.gameObject.SetActive(true);
                    if (!expbar.setupdone)
                    {
                        expbar.SetupBar(CharAttacker, expgained, levelupbonuses);
                    }

                }
                else if (target.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable")
                {
                    expbar.gameObject.SetActive(true);
                    if (!expbar.setupdone)
                    {
                        expbar.SetupBar(target.GetComponent<UnitScript>().UnitCharacteristics, expgained, levelupbonuses);
                    }

                }
                else
                {
                    expdistributed = true;
                }

            }

        }
        else if (waittingforcamera)
        {
            GameObject target = null;
            if (CharAttacker.affiliation != "playable")
            {
                target = CalculateBestTargetForOffensiveUnits(Attacker, CharAttacker.attacksfriends);
            }
            else
            {
                target = ActionsMenu.targetlist[ActionsMenu.activetargetid];
            }

            if (target == null)
            {
                CharAttacker.alreadyplayed = true;
                CurrentOther = null;
                CurrentEnemy = null;
                CurrentPlayable = null;
                battlecamera.incombat = false;
                waittingforcamera = false;
                waittingforexp = true;
                expdistributed = true;
                unitalreadyattacked = false;
            }
            else if (CurrentOther == null && CurrentEnemy == null && CurrentPlayable == null && CharAttacker.affiliation != "playable")
            {
                battlecamera.incombat = false;
                waittingforcamera = false;
                waittingforexp = true;
                expdistributed = true;
                unitalreadyattacked = false;
            }
            else
            {


                target.transform.GetChild(1).forward = Attacker.transform.GetChild(1).position - target.transform.GetChild(1).position;
                Attacker.transform.GetChild(1).forward = target.transform.GetChild(1).position - Attacker.transform.GetChild(1).position;
                target.GetComponent<UnitScript>().ResetForward();
                Attacker.GetComponent<UnitScript>().ResetForward();
                battlecamera.Destination = battlecamera.GoToFightCamera(Attacker, target);
                if (Vector2.Distance(battlecamera.Destination, new Vector2(battlecamera.transform.position.x, battlecamera.transform.position.z)) <= 0.1f)
                {
                    if (counterbeforeFirstAttack > 0)
                    {
                        counterbeforeFirstAttack--;
                        if (attacktrigger)
                        {
                            attacktrigger = false;
                            Attacker.GetComponentInChildren<Animator>().SetTrigger("Attack");
                        }
                    }
                    else if (!Attacker.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Attack") && !target.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Attack"))
                    {
                        if (!unitalreadyattacked)
                        {
                            (int hits, int crits, int damage, int exp, List<int> levelbonus) = ActionsMenu.ApplyDamage(Attacker, target, unitalreadyattacked);
                            expgained = exp;
                            levelupbonuses = levelbonus;

                            bool ishealing = Attacker.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "staff" && CharAttacker.affiliation == target.GetComponent<UnitScript>().UnitCharacteristics.affiliation;
                            FindAnyObjectByType<CombatTextScript>().UpdateInfo(damage, hits, crits, CharAttacker, target.GetComponent<UnitScript>().UnitCharacteristics, ishealing);
                            if (target.GetComponent<UnitScript>().UnitCharacteristics.currentHP <= 0 && target && CharAttacker.currentHP>0)
                            {
                                waittingforexp = true;
                                counterafterattack = (int)(delayafterAttack / Time.fixedDeltaTime);
                                waittingforcamera = false;
                            }
                            else if (ishealing)
                            {
                                waittingforexp = true;
                                waittingforcamera = false;
                            }
                            else if (CharAttacker.affiliation == "playable" && CharAttacker.currentHP<=0)
                            {
                                counterafterattack = (int)(delayafterAttack * 2f / Time.fixedDeltaTime);
                                waittingforexp = true;
                                expdistributed = true;
                                waittingforcamera = false;
                            }
                            else if (ActionsMenu.CheckifInRange(Attacker, target) || target.GetComponent<UnitScript>().GetSkill(38))
                            {
                                unitalreadyattacked = true;
                                counterbetweenattack = (int)(delaybetweenAttack / Time.fixedDeltaTime);
                                target.GetComponentInChildren<Animator>().SetTrigger("Attack");
                            }
                            else if(CharAttacker.affiliation !="playable" && target.GetComponent<UnitScript>().UnitCharacteristics.affiliation!="playable")
                            {
                                counterafterattack = (int)(delayafterAttack * 2f / Time.fixedDeltaTime);
                                waittingforexp = true;
                                expdistributed = true;
                                waittingforcamera = false;
                            }
                            else
                            {
                                waittingforexp = true;
                                counterafterattack = (int)(delayafterAttack / Time.fixedDeltaTime);
                                waittingforcamera = false;
                            }
                        }
                        else
                        {
                            if (counterbetweenattack > 0)
                            {
                                counterbetweenattack--;
                            }
                            else
                            {
                                (int hits, int crits, int damage, int exp, List<int> levelbonus) = ActionsMenu.ApplyDamage(Attacker, target, unitalreadyattacked);
                                expgained = exp;
                                levelupbonuses = levelbonus;
                                FindAnyObjectByType<CombatTextScript>().UpdateInfo(damage, hits, crits, target.GetComponent<UnitScript>().UnitCharacteristics, Attacker.GetComponent<UnitScript>().UnitCharacteristics);
                                unitalreadyattacked = true;
                                counterafterattack = (int)(delayafterAttack / Time.fixedDeltaTime);
                                if((CharAttacker.affiliation != "playable" && target.GetComponent<UnitScript>().UnitCharacteristics.affiliation != "playable") || (CharAttacker.affiliation == "playable" && CharAttacker.currentHP <= 0))
                                {
                                    waittingforexp = true;
                                    expdistributed = true;
                                    counterafterattack = (int)(delayafterAttack / Time.fixedDeltaTime);
                                    waittingforcamera = false;
                                }
                                else
                                {
                                    waittingforexp = true;
                                    counterafterattack = (int)(delayafterAttack / Time.fixedDeltaTime);
                                    waittingforcamera = false;
                                }

                                if (target != null)
                                {
                                    EndOfCombatTrigger(Attacker, target);
                                    waittingforcamera = false;
                                }
                                else
                                {
                                    EndOfCombatTrigger(Attacker);
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
            GameObject target = null;
            if (CharAttacker.affiliation != "playable")
            {
                target = CalculateBestTargetForOffensiveUnits(Attacker, CharAttacker.attacksfriends);
            }
            else
            {
                target = ActionsMenu.targetlist[ActionsMenu.activetargetid];
            }
            if (target != null)
            {
                
                waittingforcamera = true;
                battlecamera.Destination = battlecamera.GoToFightCamera(Attacker, target);
                unitalreadyattacked = false;
                counterbeforeFirstAttack = (int)(delaybeforeFirstAttack / Time.fixedDeltaTime);
                attacktrigger = true;
                FindAnyObjectByType<CombatTextScript>().ResetInfo();
            }
            else
            {
                if (target != null)
                {
                    EndOfCombatTrigger(Attacker, target);
                    waittingforcamera = false;
                }
                else
                {
                    EndOfCombatTrigger(Attacker);
                    waittingforcamera = false;
                }
                if (CharAttacker.affiliation == "playable")
                {
                    
                    ActionsMenu.FinalizeAttack();
                    waittingforcamera = false;
                }
                else
                {
                    CharAttacker.alreadyplayed = true;
                    CurrentOther = null;
                    CurrentEnemy = null;
                    CurrentPlayable = null;
                    waittingforcamera = false;
                }

            }
            gridScript.ResetAllSelections();
        }
    }

    private void EndOfCombatTrigger(GameObject unit1, GameObject unit2 = null)
    {
        Character charunit1 = unit1.GetComponent<UnitScript>().UnitCharacteristics;
        if (unit1.GetComponent<UnitScript>().GetSkill(32)) // Survivor
        {
            unit1.GetComponent<UnitScript>().SurvivorStacks++;
        }

        if (unit2 != null)
        {
            Character charunit2 = unit2.GetComponent<UnitScript>().UnitCharacteristics;
            if (unit2.GetComponent<UnitScript>().GetSkill(32)) // Survivor
            {
                unit2.GetComponent<UnitScript>().SurvivorStacks++;
            }

            if (unit1.GetComponent<UnitScript>().GetSkill(15)) // Sore Loser
            {
                if (charunit1.currentHP == 0)
                {
                    charunit2.currentHP = 1;
                }
                else if (charunit1.currentHP <  charunit2.currentHP)
                {
                    charunit2.currentHP = charunit1.currentHP;
                }
            }

            if (unit2.GetComponent<UnitScript>().GetSkill(15)) // Sore Loser
            {
                if(charunit2.currentHP == 0)
                {
                    charunit1.currentHP = 1;
                }
                else if (charunit2.currentHP < charunit1.currentHP)
                {
                    charunit1.currentHP = charunit2.currentHP;
                }
            }

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
                if (!attacksfriend && charotherunit.affiliation == "other")
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


                    if (reward > maxreward || chosenUnit == null)
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
            if (!FindIfAnyTarget(potentialAttackPosition, charunit.affiliation))
            {
                reward -= 9999;
            }
        }
        return reward;

    }

    private bool FindIfAnyTarget(List<GridSquareScript> attacklist, string affiliation)
    {
        List<GridSquareScript> listextended = gridScript.ExpandSelection(attacklist, false);
        listextended = gridScript.ExpandSelection(listextended, false);
        foreach (GridSquareScript tile in listextended)
        {
            GameObject unit = gridScript.GetUnit(tile);
            if (unit != null)
            {
                if (unit.GetComponent<UnitScript>().UnitCharacteristics.affiliation != affiliation)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private int ManhattanDistance(Character unit, Character otherunit)
    {
        return (int)(Mathf.Abs(unit.position.x - otherunit.position.x) + Mathf.Abs(unit.position.y - otherunit.position.y));
    }

    private void DeathCleanup()
    {
        List<GameObject> objecttodelete = new List<GameObject>();
        foreach (GameObject unit in gridScript.allunitGOs)
        {
            if (unit.GetComponent<UnitScript>().UnitCharacteristics.currentHP <= 0)
            {
                Destroy(unit);
                objecttodelete.Add(unit);

            }
        }
        foreach (GameObject unittodelete in objecttodelete)
        {
            gridScript.allunitGOs.Remove(unittodelete);
            gridScript.allunits.Remove(unittodelete.GetComponent<UnitScript>().UnitCharacteristics);
        }
        GetComponent<TurnManger>().InitializeUnitLists(GetComponent<GridScript>().allunitGOs);

    }

}