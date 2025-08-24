using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using static UnitScript;
using static UnityEngine.GraphicsBuffer;
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

    private bool triggercleanup;

    private CombatTextScript combatTextScript;

    public int delaybeforenxtunit;

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

        if (combatTextScript == null)
        {
            combatTextScript = FindAnyObjectByType<CombatTextScript>();
        }

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
            if (delaybeforenxtunit > 0)
            {
                delaybeforenxtunit--;
                return;
            }
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
            if (ActionsMenu.confirmattack && CurrentPlayable != null)
            {
                if (ActionsMenu.CommandUsedID == 0)
                {
                    ManageAttack(CurrentPlayable);
                }
                else
                {
                    ManageCommand(CurrentPlayable);
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
            if (delaybeforenxtunit > 0)
            {
                delaybeforenxtunit--;
                return;
            }
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

        if (!battlecamera.incombat && triggercleanup)
        {
            DeathCleanup();

        }
    }

    private void ManageCommand(GameObject User)
    {
        GameObject Target = ActionsMenu.targetlist[ActionsMenu.activetargetid];
        Character CharUser = User.GetComponent<UnitScript>().UnitCharacteristics;
        Character CharTarget = null;
        if (Target.GetComponent<UnitScript>() != null)
        {
            CharTarget = Target.GetComponent<UnitScript>().UnitCharacteristics;
        }

        int commandID = ActionsMenu.CommandUsedID;

        if (commandID == 47) //Transfuse
        {
            int previousHPUser = CharUser.currentHP;
            int previousHPTarget = CharTarget.currentHP;

            float UserPercent = (float)CharUser.currentHP / (float)CharUser.stats.HP;
            float TargetPercent = (float)CharTarget.currentHP / (float)CharTarget.stats.HP;

            CharUser.currentHP = (int)(TargetPercent * (float)CharUser.stats.HP);
            CharTarget.currentHP = (int)(UserPercent * (float)CharTarget.stats.HP);

            if (previousHPTarget > CharTarget.currentHP)
            {
                Target.GetComponent<UnitScript>().AddNumber(previousHPTarget - CharTarget.currentHP, false, "Transfuse");
            }
            else
            {
                Target.GetComponent<UnitScript>().AddNumber(CharTarget.currentHP - previousHPTarget, true, "Transfuse");
            }

            if (previousHPUser > CharUser.currentHP)
            {
                User.GetComponent<UnitScript>().AddNumber(previousHPUser - CharUser.currentHP, false, "Transfuse");
            }
            else
            {
                User.GetComponent<UnitScript>().AddNumber(CharUser.currentHP - previousHPUser, true, "Transfuse");
            }

        }
        else if (commandID == 48) //Motivate
        {
            CharTarget.alreadymoved = false;
            CharTarget.alreadyplayed = false;
            Target.GetComponent<UnitScript>().AddNumber(0, true, "Motivate");
        }
        else if (commandID == 49) //Swap
        {
            Vector2 previoususerpos = CharUser.position;
            CharUser.position = CharTarget.position;
            CharTarget.position = previoususerpos;
            Target.GetComponent<UnitScript>().AddNumber(0, true, "Swap");
            User.GetComponent<UnitScript>().AddNumber(0, true, "Swap");
        }
        else if (commandID == 50) //Reinvigorate
        {
            foreach (equipment equ in CharTarget.equipments)
            {
                if (equ.Currentuses < equ.Maxuses)
                {
                    equ.Currentuses++;
                }
            }
            Target.GetComponent<UnitScript>().AddNumber(0, true, "Reinvigorate");
        }
        else if (commandID == 51) // Jump
        {
            GridSquareScript targettile = Target.GetComponent<GridSquareScript>();
            Vector2 coorddiff = targettile.GridCoordinates - FindAnyObjectByType<GridScript>().GetTile(CharUser.position).GridCoordinates;

            int normalizedx = 0;
            int normalizedy = 0;

            if (coorddiff.x != 0)
            {
                normalizedx = (int)(coorddiff.x / Mathf.Abs(coorddiff.x));
            }
            if (coorddiff.y != 0)
            {
                normalizedy = (int)(coorddiff.y / Mathf.Abs(coorddiff.y));
            }
            Vector2 offset = new Vector2(normalizedx, normalizedy);

            CharUser.position = targettile.GridCoordinates + offset;

            User.GetComponent<UnitScript>().AddNumber(0, true, "Jump");
        }
        else if (commandID == 52) // Fortify
        {
            FindAnyObjectByType<GridScript>().GetTile(CharUser.position).type = "Fortification";
            User.GetComponent<UnitScript>().AddNumber(0, true, "Fortify");
        }
        else if (commandID == 53) // Smoke Bomb
        {
            FindAnyObjectByType<GridScript>().GetTile(CharUser.position).type = "Fog";
            User.GetComponent<UnitScript>().AddNumber(0, true, "Smoke Bomb");
        }
        else if (commandID == 54) // Chakra
        {
            int healthrestored = (int)((CharUser.stats.HP - CharUser.currentHP) * 0.25f);
            CharUser.currentHP += healthrestored;
            User.GetComponent<UnitScript>().AddNumber(healthrestored, true, "Chakra");
        }

        ActionsMenu.FinalizeAttack();

    }

    private void ManageAttack(GameObject Attacker)
    {
        triggercleanup = true;
        Character CharAttacker = Attacker.GetComponent<UnitScript>().UnitCharacteristics;
        if (counterbeforeattack > 0)
        {
            counterbeforeattack--;

        }
        else if (counterafterattack > 0)
        {
            counterafterattack--;
        }
        // exp distribution
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
        // Waitting for camera to be placed before fight officially begins.
        else if (waittingforcamera)
        {
            //Seting up target
            GameObject target = null;
            if (CharAttacker.affiliation != "playable")
            {
                target = CalculateBestTargetForOffensiveUnits(Attacker, CharAttacker.attacksfriends);
            }
            else
            {
                target = ActionsMenu.targetlist[ActionsMenu.activetargetid];
            }

            if (target == null) //end fight if no target
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
            else if (CurrentOther == null && CurrentEnemy == null && CurrentPlayable == null && CharAttacker.affiliation != "playable") //end fight if no attacker
            {
                battlecamera.incombat = false;
                waittingforcamera = false;
                waittingforexp = true;
                expdistributed = true;
                unitalreadyattacked = false;
            }
            else // begin fight
            {


                target.transform.GetChild(1).forward = Attacker.transform.GetChild(1).position - target.transform.GetChild(1).position;
                Attacker.transform.GetChild(1).forward = target.transform.GetChild(1).position - Attacker.transform.GetChild(1).position;
                target.GetComponent<UnitScript>().ResetForward();
                Attacker.GetComponent<UnitScript>().ResetForward();
                battlecamera.Destination = battlecamera.GoToFightCamera(Attacker, target);
                //waitting for camera to be fully placed
                if (Vector2.Distance(battlecamera.Destination, new Vector2(battlecamera.transform.position.x, battlecamera.transform.position.z)) <= 0.1f)
                {
                    //delay before first attack
                    if (counterbeforeFirstAttack > 0)
                    {
                        counterbeforeFirstAttack--;
                        if (attacktrigger)
                        {
                            attacktrigger = false;

                            (GameObject doubleattacker, bool triple) = ActionsMenu.CalculatedoubleAttack(Attacker, target);
                            if (doubleattacker == Attacker)
                            {
                                if (triple)
                                {
                                    Attacker.GetComponentInChildren<Animator>().SetTrigger("TripleAttack");
                                }
                                else
                                {
                                    Attacker.GetComponentInChildren<Animator>().SetTrigger("DoubleAttack");
                                }
                            }
                            else
                            {
                                Attacker.GetComponentInChildren<Animator>().SetTrigger("Attack");
                            }
                        }
                    }
                    //attacks
                    else if (!checkifattackanimationisplaying(Attacker, target))
                    {
                        //first attack (attacker)
                        if (!unitalreadyattacked)
                        {
                            (int hits, int crits, int damage, int exp, List<int> levelbonus) = ActionsMenu.ApplyDamage(Attacker, target, unitalreadyattacked);
                            expgained = exp;
                            levelupbonuses = levelbonus;

                            bool ishealing = Attacker.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "staff" && CharAttacker.affiliation == target.GetComponent<UnitScript>().UnitCharacteristics.affiliation;
                            combatTextScript.UpdateInfo(damage, hits, crits, CharAttacker, target.GetComponent<UnitScript>().UnitCharacteristics, ishealing);
                            if ((target.GetComponent<UnitScript>().UnitCharacteristics.currentHP <= 0 && CharAttacker.affiliation == "playable" && CharAttacker.currentHP > 0) || (CharAttacker.currentHP <= 0 && target.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable" && target.GetComponent<UnitScript>().UnitCharacteristics.currentHP > 0)) // distribute exp if a character died and a ally lived
                            {
                                waittingforexp = true;
                                counterafterattack = (int)(delayafterAttack / Time.fixedDeltaTime);
                                waittingforcamera = false;
                            }
                            else if (ishealing) //distribute exp if attacker healed (no counterattack)
                            {
                                waittingforexp = true;
                                waittingforcamera = false;
                            }
                            else if ((CharAttacker.affiliation == "playable" && CharAttacker.currentHP <= 0) || (target.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable" && target.GetComponent<UnitScript>().UnitCharacteristics.currentHP <= 0)) // end fight if attacker died and attacker was allied
                            {
                                counterafterattack = (int)(delayafterAttack * 2f / Time.fixedDeltaTime);
                                waittingforexp = true;
                                expdistributed = true;
                                waittingforcamera = false;
                            }
                            else if (ActionsMenu.CheckifInRange(Attacker, target) || target.GetComponent<UnitScript>().GetSkill(38)) // counterattack
                            {
                                unitalreadyattacked = true;
                                counterbetweenattack = (int)(delaybetweenAttack / Time.fixedDeltaTime);
                                (GameObject doubleattacker, bool triple) = ActionsMenu.CalculatedoubleAttack(Attacker, target);
                                if (doubleattacker == target)
                                {
                                    if (triple)
                                    {
                                        target.GetComponentInChildren<Animator>().SetTrigger("TripleAttack");
                                    }
                                    else
                                    {
                                        target.GetComponentInChildren<Animator>().SetTrigger("DoubleAttack");
                                    }
                                }
                                else
                                {
                                    target.GetComponentInChildren<Animator>().SetTrigger("Attack");
                                }

                            }
                            else if (CharAttacker.affiliation != "playable" && target.GetComponent<UnitScript>().UnitCharacteristics.affiliation != "playable") // end directly the fight if no one died
                            {
                                counterafterattack = (int)(delayafterAttack * 2f / Time.fixedDeltaTime);
                                waittingforexp = true;
                                expdistributed = true;
                                waittingforcamera = false;
                            }
                            else // distribute exp if no one died and one of the characters is playable
                            {
                                waittingforexp = true;
                                counterafterattack = (int)(delayafterAttack / Time.fixedDeltaTime);
                                waittingforcamera = false;
                            }
                        }
                        else
                        {
                            // wait a while before counterattack
                            if (counterbetweenattack > 0 && !checkifattackanimationisplaying(Attacker, target))
                            {
                                counterbetweenattack--;
                            }
                            else // does counterattack
                            {
                                (int hits, int crits, int damage, int exp, List<int> levelbonus) = ActionsMenu.ApplyDamage(Attacker, target, unitalreadyattacked);
                                expgained = exp;
                                levelupbonuses = levelbonus;
                                combatTextScript.UpdateInfo(damage, hits, crits, target.GetComponent<UnitScript>().UnitCharacteristics, Attacker.GetComponent<UnitScript>().UnitCharacteristics);
                                unitalreadyattacked = true;
                                counterafterattack = (int)(delayafterAttack / Time.fixedDeltaTime);
                                if ((CharAttacker.affiliation != "playable" && target.GetComponent<UnitScript>().UnitCharacteristics.affiliation != "playable") || (CharAttacker.affiliation == "playable" && CharAttacker.currentHP <= 0))
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
                if (combatTextScript == null)
                {
                    combatTextScript = FindAnyObjectByType<CombatTextScript>();
                }
                combatTextScript.ResetInfo();
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

    private bool checkifattackanimationisplaying(GameObject attacker, GameObject target)
    {
        return attacker.GetComponent<UnitScript>().isinattackanimation() || target.GetComponent<UnitScript>().isinattackanimation();
    }
    private void EndOfCombatTrigger(GameObject unit1, GameObject unit2 = null)
    {
        Character charunit1 = unit1.GetComponent<UnitScript>().UnitCharacteristics;
        if (unit1.GetComponent<UnitScript>().GetSkill(32)) // Survivor
        {
            unit1.GetComponent<UnitScript>().SurvivorStacks++;
            unit1.GetComponent<UnitScript>().AddNumber(unit1.GetComponent<UnitScript>().SurvivorStacks, true, "Survivor");
        }

        if (unit2 != null)
        {
            Character charunit2 = unit2.GetComponent<UnitScript>().UnitCharacteristics;
            if (unit2.GetComponent<UnitScript>().GetSkill(32)) // Survivor
            {
                unit2.GetComponent<UnitScript>().SurvivorStacks++;
                unit2.GetComponent<UnitScript>().AddNumber(unit2.GetComponent<UnitScript>().SurvivorStacks, true, "Survivor");
            }

            if (unit1.GetComponent<UnitScript>().GetSkill(15)) // Sore Loser
            {
                if (charunit1.currentHP == 0)
                {
                    unit2.GetComponent<UnitScript>().AddNumber(charunit2.currentHP - 1, false, "Sore Loser");
                    charunit2.currentHP = 1;

                }
                else if (charunit1.currentHP < charunit2.currentHP)
                {
                    unit2.GetComponent<UnitScript>().AddNumber(charunit2.currentHP - charunit1.currentHP, false, "Sore Loser");
                    charunit2.currentHP = charunit1.currentHP;
                }
            }

            if (unit2.GetComponent<UnitScript>().GetSkill(15)) // Sore Loser
            {
                if (charunit2.currentHP == 0)
                {
                    unit1.GetComponent<UnitScript>().AddNumber(charunit1.currentHP - 1, false, "Sore Loser");
                    charunit1.currentHP = 1;

                }
                else if (charunit2.currentHP < charunit1.currentHP)
                {
                    unit1.GetComponent<UnitScript>().AddNumber(charunit1.currentHP - charunit2.currentHP, false, "Sore Loser");
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

                    if (unit.GetComponent<UnitScript>().enemyStats.personality.ToLower() == "deviant" || (unit.GetComponent<UnitScript>().enemyStats.personality.ToLower() == "coward" && charunit.currentHP <= charunit.stats.HP * 0.33f))
                    {
                        reward += Random.Range(-50, 50);
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
                    if (charunit.currentHP < charunit.stats.HP)
                    {
                        reward += Mathf.Min((10 - ManhattanDistance(charunit, charotherunit) - charunit.equipments[0].Range), 0);
                    }
                }

                if (otherunit.GetComponent<UnitScript>().UnitCharacteristics.position == position.GridCoordinates)
                {
                    reward -= 9999;
                }
            }

            if (unit.GetComponent<UnitScript>().enemyStats.personality.ToLower() == "deviant" || (unit.GetComponent<UnitScript>().enemyStats.personality.ToLower() == "coward" && charunit.currentHP <= charunit.stats.HP * 0.33f))
            {
                reward += Random.Range(-30, 30);
            }

            if (!FindIfAnyTarget(potentialAttackPosition, charunit.affiliation) && charunit.currentHP == charunit.stats.HP)
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
        triggercleanup = false;
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