using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static DataScript;
using static UnitScript;
using static UnityEngine.GraphicsBuffer;
public class AttackTurnScript : MonoBehaviour
{

    private TurnManger TurnManager;

    public GameObject CurrentEnemy;
    public GameObject CurrentPlayable;
    public GameObject CurrentOther;

    private GridScript gridScript;

    private cameraScript battlecamera;

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

    private MapEventManager mapEventManager;

    public PhaseTextScript phaseTextScript;
    private MinimapScript minimapScript;

    public ForesightScript.Action CurrentAction;

    public ForesightScript foresightScript;

    private SaveManager saveManager;

    public bool attackanimationhappening;

    private GameObject previousattacker;
    private GameObject previoustarget;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        minimapScript = FindAnyObjectByType<MinimapScript>();
        TurnManager = GetComponent<TurnManger>();
        gridScript = GetComponent<GridScript>();
        battlecamera = FindAnyObjectByType<cameraScript>();
        mapEventManager = GetComponent<MapEventManager>();
        saveManager = FindAnyObjectByType<SaveManager>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if(previousattacker != null)
        {
            AttackWithAnimationEndOfFight(previousattacker,previoustarget);
            previousattacker = null;
            previoustarget = null;
        }

        if (phaseTextScript.moveText)
        {
            return;
        }

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
                    if (charunit.affiliation == "enemy" && !charunit.alreadyplayed && unit.GetComponent<UnitScript>().CheckIfOnActivated())
                    {

                        if (!determineifActionifTaken(unit))
                        {
                            GetComponent<ActionManager>().Wait(unit);
                            continue;
                        }

                        CurrentEnemy = unit;
                        counterbeforemove = (int)(delaybeforeMove / Time.fixedDeltaTime);
                        gridScript.ShowMovementOfUnit(unit);
                        break;

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
                        CurrentEnemy.GetComponent<UnitScript>().MoveTo(DestinationVector);
                        counterbeforeattack = (int)(delaybeforeAttack / Time.fixedDeltaTime);
                        CharCurrentEnemy.alreadymoved = true;
                    }
                    else
                    {
                        CurrentEnemy.GetComponent<UnitScript>().MoveTo(DestinationVector);
                        counterbeforeattack = 0;
                        CharCurrentEnemy.alreadymoved = true;
                    }

                }
            }
            else
            {
                if(saveManager.Options.BattleAnimations)
                {
                    ManageAttackWithAnimation(CurrentEnemy);
                }
                else
                {
                    ManageAttackWithoutAnimation(CurrentEnemy);
                }
                    
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
                    if (saveManager.Options.BattleAnimations)
                    {
                        ManageAttackWithAnimation(CurrentPlayable);
                    }
                    else
                    {
                        ManageAttackWithoutAnimation(CurrentPlayable);
                    }
                }
                else
                {
                    ManageCommand(CurrentPlayable);
                }

            }
        }

        if (CurrentOther == null && TurnManager.currentlyplaying == "other")
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
                    if (charunit.affiliation == "other" && !charunit.alreadyplayed && unit.GetComponent<UnitScript>().CheckIfOnActivated())
                    {

                        if (!determineifActionifTaken(unit))
                        {
                            charunit.alreadyplayed = true;
                            charunit.alreadymoved = true;
                            continue;
                        }

                        CurrentOther = unit;
                        counterbeforemove = (int)(delaybeforeMove / Time.fixedDeltaTime);
                        gridScript.ShowMovementOfUnit(unit);
                        break;

                    }
                }

            }
        }
        else if (TurnManager.currentlyplaying == "other")
        {
            Character Charcurrentother = CurrentOther.GetComponent<UnitScript>().UnitCharacteristics;
            if (!battlecamera.incombat)
            {
                battlecamera.Destination = Charcurrentother.position;
            }


            if (!Charcurrentother.alreadymoved)
            {
                if (counterbeforemove > 0)
                {
                    counterbeforemove--;
                }
                else
                {
                    GridSquareScript Destination = CalculateDestinationForOffensiveUnits(CurrentOther);
                    Vector2 DestinationVector = Charcurrentother.position;
                    if (Destination != null)
                    {
                        DestinationVector = Destination.GridCoordinates;
                        CurrentOther.GetComponent<UnitScript>().MoveTo(DestinationVector);
                        counterbeforeattack = (int)(delaybeforeAttack / Time.fixedDeltaTime);
                        Charcurrentother.alreadymoved = true;
                    }
                    else
                    {
                        CurrentOther.GetComponent<UnitScript>().MoveTo(DestinationVector);
                        counterbeforeattack = 0;
                        Charcurrentother.alreadymoved = true;
                    }

                }
            }
            else
            {
                if (saveManager.Options.BattleAnimations)
                {
                    ManageAttackWithAnimation(CurrentOther);
                }
                else
                {
                    ManageAttackWithoutAnimation(CurrentOther);
                }
            }
        }

        if (!battlecamera.incombat && triggercleanup)
        {
            DeathCleanup();

        }
    }

    private bool determineifActionifTaken(GameObject unit)
    {

        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        Vector2 originalpos = charunit.position;
        gridScript.ShowMovementOfUnit(unit);
        GridSquareScript Destination = CalculateDestinationForOffensiveUnits(unit, charunit.attacksfriends);
        if (Destination == null)
        {

            GameObject target = CalculateBestTargetForOffensiveUnits(unit, charunit.attacksfriends);
            if (target == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else
        {
            unit.GetComponent<UnitScript>().MoveTo(Destination.GridCoordinates);
            GameObject target = CalculateBestTargetForOffensiveUnits(unit, charunit.attacksfriends);
            unit.GetComponent<UnitScript>().MoveTo(originalpos);
            unit.GetComponent<UnitScript>().ResetPath();
            if (target == null && Destination.GridCoordinates == originalpos)
            {
                return false;
            }
            else
            {
                return true;
            }
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

        CurrentAction = new ForesightScript.Action();

        CurrentAction.actiontype = 3;
        CurrentAction.commandID = commandID;
        CurrentAction.Unit = User.GetComponent<UnitScript>();
        CurrentAction.previousPosition = User.GetComponent<UnitScript>().previousposition;
        CurrentAction.ModifiedCharacters = new List<Character>() { User.GetComponent<UnitScript>().CreateCopy() };

        if (commandID == 47) //Transfuse
        {
            CurrentAction.ModifiedCharacters.Add(Target.GetComponent<UnitScript>().CreateCopy());
            int previousHPUser = CharUser.currentHP;
            int previousHPTarget = CharTarget.currentHP;

            float UserPercent = (float)CharUser.currentHP / (float)CharUser.AjustedStats.HP;
            float TargetPercent = (float)CharTarget.currentHP / (float)CharTarget.AjustedStats.HP;

            CharUser.currentHP = (int)(TargetPercent * (float)CharUser.AjustedStats.HP);
            CharTarget.currentHP = (int)(UserPercent * (float)CharTarget.AjustedStats.HP);

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
            CurrentAction.ModifiedCharacters.Add(Target.GetComponent<UnitScript>().CreateCopy());
            CharTarget.alreadymoved = false;
            CharTarget.alreadyplayed = false;
            Target.GetComponent<UnitScript>().AddNumber(0, true, "Motivate");
        }
        else if (commandID == 49) //Swap
        {
            CurrentAction.ModifiedCharacters.Add(Target.GetComponent<UnitScript>().CreateCopy());
            Vector2 previoususerpos = CharUser.position;
            User.GetComponent<UnitScript>().MoveTo(CharTarget.position);
            Target.GetComponent<UnitScript>().MoveTo(previoususerpos);
            Target.GetComponent<UnitScript>().AddNumber(0, true, "Swap");
            User.GetComponent<UnitScript>().AddNumber(0, true, "Swap");
        }
        else if (commandID == 50) //Reinvigorate
        {
            CurrentAction.ModifiedCharacters.Add(Target.GetComponent<UnitScript>().CreateCopy());
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

            User.GetComponent<UnitScript>().MoveTo(targettile.GridCoordinates + offset, true);

            User.GetComponent<UnitScript>().AddNumber(0, true, "Jump");
        }
        else if (commandID == 52) // Fortify
        {
            GridSquareScript tile = CharUser.currentTile[0];
            CurrentAction.ModifiedTiles = new List<ForesightScript.TileModification>() { new ForesightScript.TileModification() {tile= tile, type= tile.type, previousremainingrain= tile.RemainingRainTurns,previousremainingsun = tile.RemainingSunTurns } };

            tile.type = "Fortification";
            User.GetComponent<UnitScript>().AddNumber(0, true, "Fortify");
        }
        else if (commandID == 53) // Smoke Bomb
        {
            GridSquareScript tile = CharUser.currentTile[0];
            CurrentAction.ModifiedTiles = new List<ForesightScript.TileModification>() { new ForesightScript.TileModification() { tile = tile, type = tile.type, previousremainingrain = tile.RemainingRainTurns, previousremainingsun = tile.RemainingSunTurns } };
            tile.type = "Fog";
            User.GetComponent<UnitScript>().AddNumber(0, true, "Smoke Bomb");
        }
        else if (commandID == 54) // Chakra
        {
            int healthrestored = (int)((CharUser.AjustedStats.HP - CharUser.currentHP) * 0.25f);
            CharUser.currentHP += healthrestored;
            User.GetComponent<UnitScript>().AddNumber(healthrestored, true, "Chakra");
        }
        else if (commandID == 56) // Copy
        {
            CurrentAction.skilltoremovefrominventory = CharTarget.UnitSkill;
            if (CharTarget.UnitSkill != 0 && !Target.GetComponent<UnitScript>().copied)
            {
                DataScript datascript = FindAnyObjectByType<DataScript>();
                Target.GetComponent<UnitScript>().copied = true;
                bool itemadded = false;
                foreach (InventoryItem item in datascript.PlayerInventory.inventoryItems)
                {
                    if (item.type == 1 && item.ID == CharTarget.UnitSkill)
                    {
                        item.Quantity++;
                        itemadded = true;
                    }
                }
                if (!itemadded)
                {
                    InventoryItem newitem = new InventoryItem();
                    newitem.type = 1;
                    newitem.Quantity = 1;
                    newitem.ID = CharTarget.UnitSkill;
                }

                Target.GetComponent<UnitScript>().AddNumber(0, true, datascript.SkillList[CharTarget.UnitSkill].name + " copied");
                User.GetComponent<UnitScript>().AddNumber(0, true, datascript.SkillList[CharTarget.UnitSkill].name + " copied");
            }
        }
        else if (commandID == 59) // Sundance
        {   
            CurrentAction.ModifiedTiles = new List<ForesightScript.TileModification>() {  };
            WeatherManager weathermanager = FindAnyObjectByType<WeatherManager>();
            if (weathermanager.rainymap)
            {
                List<GridSquareScript> tilestochange = new List<GridSquareScript>();
                foreach (GridSquareScript tile in User.GetComponent<UnitScript>().UnitCharacteristics.currentTile)
                {
                    if (!tilestochange.Contains(tile))
                    {
                        tilestochange.Add(tile);
                    }
                    List<GridSquareScript> othertiles = new List<GridSquareScript>() { gridScript.GetTile(tile.GridCoordinates + new Vector2(0, 1)), gridScript.GetTile(tile.GridCoordinates + new Vector2(0, -1)), gridScript.GetTile(tile.GridCoordinates + new Vector2(1, 0)), gridScript.GetTile(tile.GridCoordinates + new Vector2(-1, 0)) };
                    foreach (GridSquareScript addedtile in othertiles)
                    {
                        if (!tilestochange.Contains(addedtile))
                        {
                            tilestochange.Add(addedtile);
                        }
                    }

                }
                foreach (GridSquareScript tile in tilestochange)
                {
                    CurrentAction.ModifiedTiles.Add(new ForesightScript.TileModification() { tile = tile, type = tile.type, previousremainingrain = tile.RemainingRainTurns, previousremainingsun = tile.RemainingSunTurns });
                    tile.RemainingRainTurns = 0;
                    tile.RemainingSunTurns = 3;
                }
            }
        }
        else if (commandID == 60) // RainDance
        {
            CurrentAction.ModifiedTiles = new List<ForesightScript.TileModification>() { };
            WeatherManager weathermanager = FindAnyObjectByType<WeatherManager>();
            if (weathermanager.rainymap)
            {
                List<GridSquareScript> tilestochange = new List<GridSquareScript>();
                foreach (GridSquareScript tile in User.GetComponent<UnitScript>().UnitCharacteristics.currentTile)
                {
                    if (!tilestochange.Contains(tile))
                    {
                        tilestochange.Add(tile);
                    }
                    List<GridSquareScript> othertiles = new List<GridSquareScript>() { gridScript.GetTile(tile.GridCoordinates + new Vector2(0, 1)), gridScript.GetTile(tile.GridCoordinates + new Vector2(0, -1)), gridScript.GetTile(tile.GridCoordinates + new Vector2(1, 0)), gridScript.GetTile(tile.GridCoordinates + new Vector2(-1, 0)) };
                    foreach (GridSquareScript addedtile in othertiles)
                    {
                        if (!tilestochange.Contains(addedtile))
                        {
                            tilestochange.Add(addedtile);
                        }
                    }

                }
                foreach (GridSquareScript tile in tilestochange)
                {
                    CurrentAction.ModifiedTiles.Add(new ForesightScript.TileModification() { tile = tile, type = tile.type, previousremainingrain = tile.RemainingRainTurns, previousremainingsun = tile.RemainingSunTurns });
                    tile.RemainingRainTurns = 3;
                    tile.RemainingSunTurns = 0;
                }
            }
        }
        foresightScript.actions.Add(CurrentAction);
        ActionsMenu.FinalizeAttack();

    }

    private void ManageAttackWithoutAnimation(GameObject Attacker)
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
                foresightScript.actions.Add(CurrentAction);
            }
            else if (CurrentOther == null && CurrentEnemy == null && CurrentPlayable == null && CharAttacker.affiliation != "playable") //end fight if no attacker
            {
                battlecamera.incombat = false;
                waittingforcamera = false;
                waittingforexp = true;
                expdistributed = true;
                unitalreadyattacked = false;
                foresightScript.actions.Add(CurrentAction);
            }
            else // begin fight
            {

                

                target.transform.LookAt(Attacker.transform);
                Attacker.transform.LookAt(target.transform);
                target.transform.GetChild(1).LookAt(Attacker.transform.GetChild(1));
                Attacker.transform.GetChild(1).LookAt(target.transform.GetChild(1));

                foreach (ModelInfo modelinfo in Attacker.GetComponent<UnitScript>().ModelList)
                {
                    if (modelinfo.ID == 1 && modelinfo.active)
                    {
                        Attacker.transform.GetChild(1).localRotation = Quaternion.Euler(0, 90, 0);
                    }
                }

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
                            bool ishealing = Attacker.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "staff" && (CharAttacker.affiliation == target.GetComponent<UnitScript>().UnitCharacteristics.affiliation || (CharAttacker.affiliation=="playable" && target.GetComponent<UnitScript>().UnitCharacteristics.affiliation=="other" && !target.GetComponent<UnitScript>().UnitCharacteristics.attacksfriends) || (target.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable" && CharAttacker.affiliation == "other" && CharAttacker.attacksfriends));
                            if(ishealing)
                            {
                                CurrentAction.actiontype = 1;
                            }
                            if (doubleattacker == Attacker)
                            {
                                if (triple)
                                {
                                    Attacker.GetComponent<UnitScript>().PlayAttackAnimation(true, true, ishealing);
                                }
                                else
                                {
                                    Attacker.GetComponent<UnitScript>().PlayAttackAnimation(true, false, ishealing);
                                }
                            }
                            else
                            {
                                Attacker.GetComponent<UnitScript>().PlayAttackAnimation(false, false, ishealing);
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

                            bool ishealing = Attacker.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "staff" && (CharAttacker.affiliation == target.GetComponent<UnitScript>().UnitCharacteristics.affiliation || (target.GetComponent<UnitScript>().UnitCharacteristics.affiliation.ToLower() == "other" && !target.GetComponent<UnitScript>().UnitCharacteristics.attacksfriends));
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
                                        target.GetComponent<UnitScript>().PlayAttackAnimation(true, true);
                                    }
                                    else
                                    {
                                        target.GetComponent<UnitScript>().PlayAttackAnimation(true);
                                    }
                                }
                                else
                                {
                                    target.GetComponent<UnitScript>().PlayAttackAnimation();
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


                CurrentAction = new ForesightScript.Action();
                foresightScript.actions.Add(CurrentAction);
                CurrentAction.actiontype = 0;
                CurrentAction.Unit = Attacker.GetComponent<UnitScript>();
                CurrentAction.previousPosition = Attacker.GetComponent<UnitScript>().previousposition;
                CurrentAction.ModifiedCharacters = new List<Character>() { Attacker.GetComponent<UnitScript>().CreateCopy() };
                CurrentAction.AttackData = new ForesightScript.AttackData();
                CurrentAction.AttackData.previousattackerhitindex = Attacker.GetComponent<RandomScript>().hitvaluesindex;
                CurrentAction.AttackData.previousattackercritindex = Attacker.GetComponent<RandomScript>().CritValuesindex;
                CurrentAction.AttackData.previousattackerlvlupindex = Attacker.GetComponent<RandomScript>().levelvaluesindex;

                CurrentAction.AttackData.defender = target.GetComponent<UnitScript>();
                CurrentAction.ModifiedCharacters.Add(target.GetComponent<UnitScript>().CreateCopy());
                CurrentAction.AttackData.previousdefenderhitindex = target.GetComponent<RandomScript>().hitvaluesindex;
                CurrentAction.AttackData.previousdefendercritindex = target.GetComponent<RandomScript>().CritValuesindex;
                CurrentAction.AttackData.previousdefenderlvlupindex = target.GetComponent<RandomScript>().levelvaluesindex;


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

    public void AttackWithAnimationEndOfFight(GameObject Attacker, GameObject Target)
    {

        if (Target != null)
        {
            EndOfCombatTrigger(Attacker, Target);
        }
        else
        {
            EndOfCombatTrigger(Attacker);
        }

        if(Attacker.GetComponent<UnitScript>().UnitCharacteristics.affiliation=="playable")
        {
            ActionsMenu.FinalizeAttack();
        }

    }


    public void ManageAttackWithAnimation(GameObject Attacker)
    {
        if(!attackanimationhappening)
        {
            attackanimationhappening = true;
            Character CharAttacker = Attacker.GetComponent<UnitScript>().UnitCharacteristics;

            Character Attackercopy = Attacker.GetComponent<UnitScript>().CreateCopy();

            triggercleanup = true;

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

                previousattacker = Attacker;
                previoustarget = target;

                Character Chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;

                Character Targetcopy = target.GetComponent<UnitScript>().CreateCopy();

                CurrentAction = new ForesightScript.Action();
                foresightScript.actions.Add(CurrentAction);
                CurrentAction.actiontype = 0;
                CurrentAction.Unit = Attacker.GetComponent<UnitScript>();
                CurrentAction.previousPosition = Attacker.GetComponent<UnitScript>().previousposition;
                CurrentAction.ModifiedCharacters = new List<Character>() { Attacker.GetComponent<UnitScript>().CreateCopy() };
                CurrentAction.AttackData = new ForesightScript.AttackData();
                CurrentAction.AttackData.previousattackerhitindex = Attacker.GetComponent<RandomScript>().hitvaluesindex;
                CurrentAction.AttackData.previousattackercritindex = Attacker.GetComponent<RandomScript>().CritValuesindex;
                CurrentAction.AttackData.previousattackerlvlupindex = Attacker.GetComponent<RandomScript>().levelvaluesindex;

                CurrentAction.AttackData.defender = target.GetComponent<UnitScript>();
                CurrentAction.ModifiedCharacters.Add(target.GetComponent<UnitScript>().CreateCopy());
                CurrentAction.AttackData.previousdefenderhitindex = target.GetComponent<RandomScript>().hitvaluesindex;
                CurrentAction.AttackData.previousdefendercritindex = target.GetComponent<RandomScript>().CritValuesindex;
                CurrentAction.AttackData.previousdefenderlvlupindex = target.GetComponent<RandomScript>().levelvaluesindex;
                foresightScript.actions.Add(CurrentAction);

                (GameObject doubleattacker, bool triple) = ActionsMenu.CalculatedoubleAttack(Attacker, target);
                bool ishealing = Attacker.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "staff" && (CharAttacker.affiliation == target.GetComponent<UnitScript>().UnitCharacteristics.affiliation || (CharAttacker.affiliation == "playable" && target.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "other" && !target.GetComponent<UnitScript>().UnitCharacteristics.attacksfriends) || (target.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable" && CharAttacker.affiliation == "other" && CharAttacker.attacksfriends));
                if (ishealing)
                {
                    CurrentAction.actiontype = 1;
                }

                (int attackerhits, int attackercrits, int attackerdamage, int attackerexp, List<int> attackerlevelbonus) = ActionsMenu.ApplyDamage(Attacker, target, false);

                bool defenderdodged = false;
                if (attackerhits == 0)
                {
                    defenderdodged = true;
                }

                bool defenderattacks = true;


                if ((target.GetComponent<UnitScript>().UnitCharacteristics.currentHP <= 0 && CharAttacker.affiliation == "playable" && CharAttacker.currentHP > 0) || (CharAttacker.currentHP <= 0 && target.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable" && target.GetComponent<UnitScript>().UnitCharacteristics.currentHP > 0))
                {
                    defenderattacks = false;
                }
                if ((CharAttacker.affiliation == "playable" && CharAttacker.currentHP <= 0) || (target.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable" && target.GetComponent<UnitScript>().UnitCharacteristics.currentHP <= 0))
                {
                    defenderattacks = false;
                }
                if (ishealing)
                {
                    defenderattacks = false;
                }
                if (!ActionsMenu.CheckifInRange(Attacker, target) && !target.GetComponent<UnitScript>().GetSkill(38)) // counterattack
                {
                    defenderattacks = false;
                }

                bool defenderdied = false;
                if (Chartarget.currentHP <= 0)
                {
                    defenderdied = true;
                }
                bool attackerdodged = false;
                int defenderhits = 0;
                int defenderdamage = 0;
                int defendercrits = 0;
                int defenderexp = 0;
                List<int> defenderlevelbonus = new List<int>();
                if (defenderattacks)
                {
                    (defenderhits, defendercrits, defenderdamage, defenderexp, defenderlevelbonus) = ActionsMenu.ApplyDamage(Attacker, target, true);
                    if (defenderhits == 0)
                    {
                        attackerdodged = true;
                    }
                }

                bool attackerdied = false;
                if (CharAttacker.currentHP <= 0)
                {
                    attackerdied = true;
                }
                Character Chardoubleattacker = null;
                if (doubleattacker != null)
                {
                    Chardoubleattacker = doubleattacker.GetComponent<UnitScript>().UnitCharacteristics;
                }

                int expearned = 0;

                List<int> levelbonus = new List<int>();

                if (attackerexp!=0)
                {
                    expearned = attackerexp;
                    levelbonus = attackerlevelbonus;
                }
                else if (defenderexp!=0)
                {
                    expearned = defenderexp;
                    levelbonus = defenderlevelbonus;
                }


                Debug.Log("attacker damage : "+attackerdamage);
                Debug.Log("defender damage : " + defenderdamage);

                FindAnyObjectByType<CombatSceneLoader>().ActivateCombatScene(CharAttacker, Chartarget, Attacker.GetComponent<UnitScript>().GetFirstWeapon(), target.GetComponent<UnitScript>().GetFirstWeapon(), Chardoubleattacker, triple, ishealing, attackerdodged, defenderattacks, defenderdodged, attackerdied, defenderdied, expearned, levelbonus, Attackercopy,Targetcopy);

            }
        }
    }

    private bool checkifattackanimationisplaying(GameObject attacker, GameObject target)
    {
        return attacker.GetComponent<UnitScript>().isinattackanimation() || target.GetComponent<UnitScript>().isinattackanimation();
    }
    private void EndOfCombatTrigger(GameObject unit1, GameObject unit2 = null)
    {
        if (unit1 != null)
        {
            unit1.GetComponent<UnitScript>().UpdateWeaponModel();
        }

        Character charunit1 = unit1.GetComponent<UnitScript>().UnitCharacteristics;
        if (unit1.GetComponent<UnitScript>().GetSkill(32)) // Survivor
        {
            CurrentAction.AttackData.attackersurvivorstats = unit1.GetComponent<UnitScript>().SurvivorStacks;
            unit1.GetComponent<UnitScript>().SurvivorStacks++;
            unit1.GetComponent<UnitScript>().AddNumber(unit1.GetComponent<UnitScript>().SurvivorStacks, true, "Survivor");
        }

        if (unit2 != null)
        {
            Character charunit2 = unit2.GetComponent<UnitScript>().UnitCharacteristics;
            if (unit2.GetComponent<UnitScript>().GetSkill(32)) // Survivor
            {
                CurrentAction.AttackData.attackersurvivorstats = unit2.GetComponent<UnitScript>().SurvivorStacks;
                unit2.GetComponent<UnitScript>().SurvivorStacks++;
                unit2.GetComponent<UnitScript>().AddNumber(unit2.GetComponent<UnitScript>().SurvivorStacks, true, "Survivor");
            }

            if (unit1.GetComponent<UnitScript>().GetSkill(15)) // Sore Loser
            {
                if (charunit1.currentHP <= 0)
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
                if (charunit2.currentHP <= 0)
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

            unit2.GetComponent<UnitScript>().UpdateWeaponModel();
           
        }



        
    }

    private List<string> Whotoattack(string affiliation, bool attackfriend)
    {
        List<string> affiliationtoattack = new List<string>() { "playable", "other" };
        if (affiliation == "other")
        {
            if (attackfriend)
            {
                affiliationtoattack = new List<string>() { "playable", "enemy" };
            }
            else
            {
                affiliationtoattack = new List<string>() { "enemy" };
            }
        }
        return affiliationtoattack;
    }


    public GameObject CalculateBestTargetForOffensiveUnits(GameObject unit, bool attacksfriend = true)
    {
        int maxreward = 0;
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        (int range, bool melee) = unit.GetComponent<UnitScript>().GetRangeAndMele();
        List<GridSquareScript> potentialAttackPosition = gridScript.GetAttack(range, melee, gridScript.GetTile(charunit.position), charunit.enemyStats.monsterStats.size,charunit);
        List<string> affiliationtoattack = Whotoattack(charunit.affiliation, attacksfriend);
        GameObject chosenUnit = null;

        foreach (GridSquareScript tile in potentialAttackPosition)
        {
            foreach (GameObject otherunit in gridScript.allunitGOs)
            {
                Character charotherunit = otherunit.GetComponent<UnitScript>().UnitCharacteristics;
                if (affiliationtoattack.Contains(charotherunit.affiliation.ToLower()) && charotherunit.position == tile.GridCoordinates)
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
                    else if (charunit.enemyStats.personality.ToLower() != "survivor")
                    {
                        reward = -999;
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

                    if (charunit.enemyStats.personality.ToLower() != "daredevil")
                    {
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


                    if (charunit.enemyStats.personality.ToLower() == "deviant" || (charunit.enemyStats.personality.ToLower() == "coward" && charunit.currentHP <= charunit.AjustedStats.HP * 0.33f))
                    {
                        reward += unit.GetComponent<RandomScript>().GetPersonalityValue() - 50;
                    }

                    if (charunit.enemyStats.personality.ToLower() != "survivor")
                    {
                        if (potentialdamagetaken == 0)
                        {
                            reward += 50;
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
                        reward -= 999;
                    }


                    if (reward > maxreward || chosenUnit == null)
                    {
                        chosenUnit = otherunit;
                        maxreward = reward;
                    }
                }
            }
        }
        if (charunit.enemyStats.personality.ToLower() == "survivor")
        {
            chosenUnit = null;
        }
        return chosenUnit;
    }

    /// <summary>
    /// Calculate movement Destination for AI Characters 
    /// </summary>
    /// <param name="currentCharacter"></param>
    /// <param name="attacksfriend"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Calculate Reward for target position for AI Unit
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="position"></param>
    /// <param name="attacksfriend"></param>
    /// <returns></returns>
    private int RewardForDestination(GameObject unit, GridSquareScript position, bool attacksfriend = true)
    {

        int reward = 0;
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        if (charunit.enemyStats.personality.ToLower() == "guard" && position.GridCoordinates != charunit.position)
        {
            return reward - 9999;
        }
        (int range, bool melee) = unit.GetComponent<UnitScript>().GetRangeAndMele();
        List<GridSquareScript> potentialAttackPosition = gridScript.GetAttack(range, melee, position, charunit.enemyStats.monsterStats.size,charunit);
        List<string> affiliationtoattack = Whotoattack(charunit.affiliation, attacksfriend);

        foreach (GridSquareScript tile in potentialAttackPosition)
        {
            foreach (GameObject otherunit in gridScript.allunitGOs)
            {



                Character charotherunit = otherunit.GetComponent<UnitScript>().UnitCharacteristics;

                if (affiliationtoattack.Contains(charotherunit.affiliation.ToLower()) && charunit.enemyStats.personality.ToLower() == "hunter")
                {
                    reward += Mathf.Max(100 - ManhattanDistance(charunit, charotherunit), 0);
                }

                if (affiliationtoattack.Contains(charotherunit.affiliation.ToLower()) && charotherunit.position == tile.GridCoordinates)
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

                    if (charunit.enemyStats.personality.ToLower() != "daredevil")
                    {
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
                    if (charunit.enemyStats.personality.ToLower() != "survivor")
                    {
                        if (potentialdamagetaken == 0)
                        {
                            reward += 50;
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
                }
                else
                {
                    if (charunit.currentHP < charunit.AjustedStats.HP)
                    {
                        (int newrange, bool newmelee) = unit.GetComponent<UnitScript>().GetRangeAndMele();
                        reward += Mathf.Max(ManhattanDistance(charunit, charotherunit) - newrange, 0);
                    }


                    if (charunit.enemyStats.personality.ToLower() != "survivor" && (!charunit.attacksfriends && charotherunit.affiliation == "enemy"))
                    {
                        reward += ManhattanDistance(charunit, charotherunit) * 5;
                    }

                }

                if (otherunit.GetComponent<UnitScript>().UnitCharacteristics.position == position.GridCoordinates)
                {
                    reward -= 9999;
                }
            }

            if (charunit.enemyStats.personality.ToLower() == "deviant" || (charunit.enemyStats.personality.ToLower() == "coward" && charunit.currentHP <= charunit.AjustedStats.HP * 0.33f))
            {
                int value = (int)(((float)unit.GetComponent<RandomScript>().GetPersonalityValue() / 100f) * 60f) - 30;
                reward += value;
            }

            if (!FindIfAnyTarget(potentialAttackPosition, charunit.affiliation) && charunit.currentHP == charunit.AjustedStats.HP && charunit.enemyStats.personality.ToLower() != "hunter")
            {
                reward -= 9999;
            }

            if (charunit.enemyStats.personality.ToLower() == "survivor" || (charunit.enemyStats.personality.ToLower() == "coward" && charunit.currentHP <= charunit.AjustedStats.HP * 0.1f) || charunit.enemyStats.personality.ToLower() == "survivor")
            {
                if (FindIfAnyTarget(potentialAttackPosition, charunit.affiliation))
                {
                    reward -= 99;
                }
            }

            if (charunit.enemyStats.monsterStats.size > 1)
            {
                GridSquareScript tile1 = gridScript.GetTile(position.GridCoordinates + new Vector2(-1, 0));
                GridSquareScript tile2 = gridScript.GetTile(position.GridCoordinates + new Vector2(-1, 1));
                GridSquareScript tile3 = gridScript.GetTile(position.GridCoordinates + new Vector2(0, 1));
                if (tile1.isobstacle || tile2.isobstacle || tile3.isobstacle)
                {
                    return reward - 9999;
                }
                if (gridScript.GetUnit(tile1) != unit || gridScript.GetUnit(tile2) != unit || gridScript.GetUnit(tile3) != unit)
                {
                    return reward - 9999;
                }
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
                if (unit.GetComponent<UnitScript>().UnitCharacteristics.affiliation != affiliation || (unit.GetComponent<UnitScript>().UnitCharacteristics.affiliation != "playable" && affiliation == "other"))
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

    private int ManhattanDistance(Vector2 unit, Vector2 otherunit)
    {
        return (int)(Mathf.Abs(unit.x - otherunit.x) + Mathf.Abs(unit.y - otherunit.y));
    }

    public void DeathCleanup()
    {
        triggercleanup = false;
        List<GameObject> objecttodelete = new List<GameObject>();
        foreach (GameObject unit in gridScript.allunitGOs)
        {
            if (unit != null)
            {
                Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
                if (charunit.currentHP <= 0)
                {

                    if (charunit.enemyStats == null)
                    {
                        if (charunit.currentTile != null)
                        {
                            foreach (GridSquareScript tile in charunit.currentTile)
                            {
                                tile.UpdateInsideSprite(false);
                            }
                        }
                        unit.SetActive(false);
                        objecttodelete.Add(unit);
                    }
                    else if (charunit.enemyStats.RemainingLifebars > 0)
                    {
                        charunit.enemyStats.RemainingLifebars--;
                        charunit.currentHP = (int)charunit.AjustedStats.HP;
                        unit.GetComponent<UnitScript>().AddNumber((int)charunit.AjustedStats.HP, true, charunit.enemyStats.RemainingLifebars + " Healthbars remaining.");
                    }
                    else
                    {
                        if (charunit.currentTile != null)
                        {
                            foreach (GridSquareScript tile in charunit.currentTile)
                            {
                                tile.UpdateInsideSprite(false);
                            }
                        }
                        unit.SetActive(false);
                        objecttodelete.Add(unit);
                    }


                }
            }

        }
        foreach (GameObject unittodelete in objecttodelete)
        {
            gridScript.allunitGOs.Remove(unittodelete);
            gridScript.allunits.Remove(unittodelete.GetComponent<UnitScript>().UnitCharacteristics);
        }
        GetComponent<TurnManger>().InitializeUnitLists(GetComponent<GridScript>().allunitGOs);
        mapEventManager.TriggerEventCheck();
        minimapScript.UpdateMinimap();
    }

}