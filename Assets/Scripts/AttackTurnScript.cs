using System.Collections.Generic;
using UnityEngine;
using static DataScript;
using static UnitScript;
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

    public float TimebeforeAnimationAttack;

    public PhaseTextScript phaseTextScript;
    private MinimapScript minimapScript;

    public ForesightScript foresightScript;

    private SaveManager saveManager;

    public int attackanimationhappeningcnt;

    private GameObject previousattacker;
    private GameObject previoustarget;

    private CombatSceneLoader combatsceneloader;

    private GameObject currentenemytarget;

    private BattleInfotext battleInfotextScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        minimapScript = FindAnyObjectByType<MinimapScript>();
        TurnManager = GetComponent<TurnManger>();
        gridScript = GetComponent<GridScript>();
        battlecamera = FindAnyObjectByType<cameraScript>();
        saveManager = FindAnyObjectByType<SaveManager>();
        combatsceneloader = FindAnyObjectByType<CombatSceneLoader>();
        battleInfotextScript = FindAnyObjectByType<BattleInfotext>(FindObjectsInactive.Include);
    }

    // Update is called once per frame
    void Update()
    {

        if (attackanimationhappeningcnt > 0)
        {
            attackanimationhappeningcnt--;
            return;
        }

        if (previousattacker != null)
        {
            previousattacker.GetComponent<UnitScript>().disableLifebar = false;
            previousattacker.GetComponent<UnitScript>().ManageLifebars();
            AttackWithAnimationEndOfFight(previousattacker, previoustarget);
            if (previoustarget != null)
            {
                previoustarget.GetComponent<UnitScript>().disableLifebar = false;
                previoustarget.GetComponent<UnitScript>().ManageLifebars();
            }
            previousattacker = null;
            previoustarget = null;
            gridScript.ResetAllSelections();
            waittingforcamera = false;
            triggercleanup = true;
            MapEventManager.instance.TriggerEventCheck();
        }

        if (phaseTextScript.moveText)
        {
            return;
        }

        if (combatTextScript == null)
        {
            combatTextScript = FindAnyObjectByType<CombatTextScript>(FindObjectsInactive.Include);
        }

        if (TurnManager.currentlyplaying != "enemy")
        {
            CurrentEnemy = null;
        }
        if (TurnManager.currentlyplaying != "other")
        {
            CurrentOther = null;
        }
        if (TurnManager.currentlyplaying != "playable" && ActionManager.instance != null)
        {
            ActionManager.instance.preventfromlockingafteraction = true;
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
                            ActionManager.instance.Wait(unit);
                            continue;
                        }

                        CurrentEnemy = unit;
                        battleInfotextScript.previousEnemy = unit;
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
                    currentenemytarget = null;
                    GridSquareScript Destination = null;

                    if (CurrentEnemy.GetComponent<UnitScript>().UnitCharacteristics.enemyStats.bossiD > 0)
                    {
                        CurrentEnemy.GetComponent<BossScript>().nextTarget = CalculateDestinationForBoss(CurrentEnemy);
                        Destination = CurrentEnemy.GetComponent<UnitScript>().UnitCharacteristics.currentTile[0];
                    }
                    else
                    {
                        (Destination, currentenemytarget) = CalculateDestinationForOffensiveUnitsV2(CurrentEnemy);
                    }


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
            else if (CurrentEnemy != null && !CharCurrentEnemy.alreadyplayed)
            {
                if (CurrentEnemy.GetComponent<UnitScript>().UnitCharacteristics.enemyStats.bossiD > 0)
                {
                    CurrentEnemy.GetComponent<BossScript>().TriggerBossAttack();
                    CharCurrentEnemy.alreadyplayed = true;
                }
                else if (saveManager.Options.BattleAnimations)
                {
                    if (Vector2.Distance(CharCurrentEnemy.currentTile[0].GridCoordinates, new Vector2(CurrentEnemy.transform.position.x, CurrentEnemy.transform.position.z)) <= 1f)
                    {
                        ManageAttackWithAnimation(CurrentEnemy);
                    }
                }
                else
                {
                    ManageAttackWithoutAnimation(CurrentEnemy);
                }

            }
            else
            {
                CurrentEnemy = null;
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
                        if (Vector2.Distance(CurrentPlayableChar.currentTile[0].GridCoordinates, new Vector2(CurrentPlayable.transform.position.x, CurrentPlayable.transform.position.z)) <= 1f)
                        {
                            ManageAttackWithAnimation(CurrentPlayable);
                        }
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
                        battleInfotextScript.previousOther = unit;
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
                    currentenemytarget = null;
                    GridSquareScript Destination = null;
                    (Destination, currentenemytarget) = CalculateDestinationForOffensiveUnitsV2(CurrentOther);

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
            else if (CurrentOther != null && !Charcurrentother.alreadyplayed)
            {
                if (saveManager.Options.BattleAnimations)
                {
                    if (Vector2.Distance(Charcurrentother.currentTile[0].GridCoordinates, new Vector2(CurrentOther.transform.position.x, CurrentOther.transform.position.z)) <= 1f)
                    {
                        ManageAttackWithAnimation(CurrentOther);
                    }

                }
                else
                {
                    ManageAttackWithoutAnimation(CurrentOther);
                }
            }
            else
            {
                CurrentOther = null;
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

        if (charunit.enemyStats.personality.ToLower() == "hunter" || charunit.enemyStats.bossiD > 0)
        {
            return true;
        }


        Vector2 originalpos = charunit.position;
        gridScript.ShowMovementOfUnit(unit);

        GridSquareScript Destination = null;
        GameObject target = null;

        (Destination, target) = CalculateDestinationForOffensiveUnitsV2(unit);

        if (Destination == null)
        {
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
            (Destination, target) = CalculateDestinationForOffensiveUnitsV2(unit);
            unit.GetComponent<UnitScript>().MoveTo(originalpos);
            unit.GetComponent<UnitScript>().ResetPath();
            if (target == null && (Destination == null || Destination.GridCoordinates == originalpos))
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


        if (commandID == 47) //Transfuse
        {
            foresightScript.CreateAction(3, User, Target);
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
            foresightScript.CreateAction(3, User, Target);
            CharTarget.alreadymoved = false;
            CharTarget.alreadyplayed = false;
            Target.GetComponent<UnitScript>().AddNumber(0, true, "Motivate");
        }
        else if (commandID == 49) //Swap
        {
            foresightScript.CreateAction(3, User, Target);
            Vector2 previoususerpos = CharUser.position;
            User.GetComponent<UnitScript>().MoveTo(CharTarget.position);
            Target.GetComponent<UnitScript>().MoveTo(previoususerpos);
            Target.GetComponent<UnitScript>().AddNumber(0, true, "Swap");
            User.GetComponent<UnitScript>().AddNumber(0, true, "Swap");
        }
        else if (commandID == 50) //Reinvigorate
        {
            foresightScript.CreateAction(3, User, Target);
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
            foresightScript.CreateAction(3, User);
            GridSquareScript targettile = Target.GetComponent<GridSquareScript>();
            Vector2 coorddiff = targettile.GridCoordinates - gridScript.GetTile(CharUser.position).GridCoordinates;

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
            foresightScript.CreateAction(3, User);
            GridSquareScript tile = CharUser.currentTile[0];


            tile.type = "Fortification";
            User.GetComponent<UnitScript>().AddNumber(0, true, "Fortify");
        }
        else if (commandID == 53) // Smoke Bomb
        {
            GridSquareScript tile = CharUser.currentTile[0];
            foresightScript.CreateAction(3, User);
            tile.type = "Fog";
            User.GetComponent<UnitScript>().AddNumber(0, true, "Smoke Bomb");
        }
        else if (commandID == 54) // Chakra
        {
            foresightScript.CreateAction(3, User);
            int healthrestored = (int)((CharUser.AjustedStats.HP - CharUser.currentHP) * 0.25f);
            CharUser.currentHP += healthrestored;
            User.GetComponent<UnitScript>().AddNumber(healthrestored, true, "Chakra");
        }
        else if (commandID == 56) // Copy
        {
            foresightScript.CreateAction(3, User, CharTarget.UnitSkill);
            if (CharTarget.UnitSkill != 0 && !Target.GetComponent<UnitScript>().copied)
            {
                Target.GetComponent<UnitScript>().copied = true;
                DataScript.instance.SaveCopyFlag(CharTarget.enemyStats.CopyID);
                bool itemadded = false;
                foreach (InventoryItem item in DataScript.instance.PlayerInventory.inventoryItems)
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
                CharUser.TemporarySkill = CharTarget.UnitSkill;
                Target.GetComponent<UnitScript>().AddNumber(0, true, DataScript.instance.SkillList[CharTarget.UnitSkill].name + " copied");
                User.GetComponent<UnitScript>().AddNumber(0, true, DataScript.instance.SkillList[CharTarget.UnitSkill].name + " copied");
            }
        }
        else if (commandID == 59) // Sundance
        {
            foresightScript.CreateAction(3, User);
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
                    tile.RemainingRainTurns = 0;
                    tile.RemainingSunTurns = 2;
                }
            }
        }
        else if (commandID == 60) // RainDance
        {
            foresightScript.CreateAction(3, User);
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
                    tile.RemainingRainTurns = 2;
                    tile.RemainingSunTurns = 0;
                }
            }
        }
        else if (commandID == 70) // Blade Conversion
        {
            foresightScript.CreateAction(3, User);
            int healthrestored = (int)Mathf.Min((CharUser.AjustedStats.HP - CharUser.currentHP), CharUser.AjustedStats.HP * 0.5f);
            CharUser.currentHP += healthrestored;
            User.GetComponent<UnitScript>().GetFirstWeapon().Currentuses = 0;
            User.GetComponent<UnitScript>().AddNumber(healthrestored, true, "Blade Conversion");
        }
        else if (commandID == 71) // Blade Sacrifice
        {
            foresightScript.CreateAction(3, User);
            int healthlost = (int)Mathf.Min(CharUser.currentHP - 1, CharUser.AjustedStats.HP * 0.5f);
            CharUser.currentHP -= healthlost;
            User.GetComponent<UnitScript>().GetFirstWeapon().Currentuses = 0;
            User.GetComponent<UnitScript>().AddNumber(healthlost, false, "Blade Sacrifice");
        }
        else if (commandID == 76) // Taunt
        {
            foresightScript.CreateAction(3, User, Target);
            User.GetComponent<UnitScript>().UnitCharacteristics.TauntTurns = 1;
            User.GetComponent<UnitScript>().AddNumber(0, true, "Taunt");
        }
        else if (commandID == 79) // Break
        {
            foresightScript.CreateAction(3, User, Target);
            int lostdurability = (int)Mathf.Max(1, CharUser.AjustedStats.Strength / (Target.GetComponent<UnitScript>().GetFirstWeapon().Grade));
            Target.GetComponent<UnitScript>().GetFirstWeapon().Currentuses = Mathf.Max(Target.GetComponent<UnitScript>().GetFirstWeapon().Currentuses - lostdurability, 0);
            Target.GetComponent<UnitScript>().AddNumber(0, true, "Break");
        }

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
                    target = currentenemytarget;
                }
                else
                {
                    target = ActionsMenu.targetlist[ActionsMenu.activetargetid];
                }
                if (target == null)
                {
                    expdistributed = true;
                }
                else if (CharAttacker.affiliation == "playable" && levelupbonuses != new List<int>())
                {
                    expbar.gameObject.SetActive(true);
                    if (!expbar.setupdone)
                    {
                        expbar.SetupBar(CharAttacker, expgained, levelupbonuses);
                    }

                }
                else if (target.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable" && levelupbonuses != new List<int>())
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
                target = currentenemytarget;
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
                            bool ishealing = Attacker.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "staff" && (CharAttacker.affiliation == target.GetComponent<UnitScript>().UnitCharacteristics.affiliation || (CharAttacker.affiliation == "playable" && target.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "other" && !target.GetComponent<UnitScript>().UnitCharacteristics.attacksfriends) || (target.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable" && CharAttacker.affiliation == "other" && CharAttacker.attacksfriends));
                            if (ishealing)
                            {
                                doubleattacker = null;
                                triple = false;
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

                            bool ishealing = Attacker.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "staff" && (CharAttacker.affiliation == target.GetComponent<UnitScript>().UnitCharacteristics.affiliation || (target.GetComponent<UnitScript>().UnitCharacteristics.affiliation.ToLower() == "other" && !target.GetComponent<UnitScript>().UnitCharacteristics.attacksfriends) || (Attacker.GetComponent<UnitScript>().UnitCharacteristics.affiliation.ToLower() == "other" && !Attacker.GetComponent<UnitScript>().UnitCharacteristics.attacksfriends));
                            combatTextScript.UpdateInfo(damage, hits, crits, CharAttacker, target.GetComponent<UnitScript>().UnitCharacteristics, ishealing);
                            if ((target.GetComponent<UnitScript>().UnitCharacteristics.currentHP <= 0 && CharAttacker.affiliation == "playable" && CharAttacker.currentHP > 0) || (CharAttacker.currentHP <= 0 && target.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable" && target.GetComponent<UnitScript>().UnitCharacteristics.currentHP > 0)) // distribute exp if a character died and a ally lived
                            {
                                waittingforexp = true;
                                counterafterattack = (int)(delayafterAttack / Time.fixedDeltaTime);
                                waittingforcamera = false;
                                EndOfCombatTrigger(Attacker, target);
                            }
                            else if (ishealing) //distribute exp if attacker healed (no counterattack)
                            {
                                waittingforexp = true;
                                waittingforcamera = false;
                                EndOfCombatTrigger(Attacker, target);
                            }
                            else if ((CharAttacker.affiliation == "playable" && CharAttacker.currentHP <= 0) || (target.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable" && target.GetComponent<UnitScript>().UnitCharacteristics.currentHP <= 0)) // end fight if attacker died and attacker was allied
                            {
                                counterafterattack = (int)(delayafterAttack * 2f / Time.fixedDeltaTime);
                                waittingforexp = true;
                                expdistributed = true;
                                waittingforcamera = false;
                                EndOfCombatTrigger(Attacker, target);
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
                                EndOfCombatTrigger(Attacker, target);



                            }
                            else // distribute exp if no one died and one of the characters is playable
                            {
                                waittingforexp = true;
                                counterafterattack = (int)(delayafterAttack / Time.fixedDeltaTime);
                                waittingforcamera = false;
                                EndOfCombatTrigger(Attacker, target);
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
                                    EndOfCombatTrigger(Attacker, target);
                                }
                                else
                                {
                                    EndOfCombatTrigger(Attacker, target);
                                    waittingforexp = true;
                                    counterafterattack = (int)(delayafterAttack / Time.fixedDeltaTime);
                                    waittingforcamera = false;
                                }

                                //if (target != null)
                                //{
                                //    EndOfCombatTrigger(Attacker, target);
                                //    waittingforcamera = false;
                                //}
                                //else
                                //{
                                //    EndOfCombatTrigger(Attacker, target);
                                //    waittingforcamera = false;
                                //}
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
                target = currentenemytarget;
            }
            else
            {
                target = ActionsMenu.targetlist[ActionsMenu.activetargetid];
            }
            if (target != null)
            {

                waittingforcamera = true;

                foresightScript.CreateAction(0, Attacker, target);


                battlecamera.Destination = battlecamera.GoToFightCamera(Attacker, target);
                unitalreadyattacked = false;
                counterbeforeFirstAttack = (int)(delaybeforeFirstAttack / Time.fixedDeltaTime);
                attacktrigger = true;
                if (combatTextScript == null)
                {
                    combatTextScript = FindAnyObjectByType<CombatTextScript>(FindObjectsInactive.Include);
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
        Attacker.GetComponent<UnitScript>().disableLifebar = false;
        if (Target != null)
        {

            Target.GetComponent<UnitScript>().disableLifebar = false;
            EndOfCombatTrigger(Attacker, Target);
        }
        else
        {
            EndOfCombatTrigger(Attacker);
        }

        if (Attacker.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable")
        {
            ActionsMenu.FinalizeAttack();
        }
    }


    public void ManageAttackWithAnimation(GameObject Attacker)
    {
        if (attackanimationhappeningcnt == 0)
        {
            attackanimationhappeningcnt = (int)(TimebeforeAnimationAttack / Time.deltaTime);
            Character CharAttacker = Attacker.GetComponent<UnitScript>().UnitCharacteristics;

            Character Attackercopy = Attacker.GetComponent<UnitScript>().CreateCopy();




            triggercleanup = true;

            GameObject target = null;
            if (CharAttacker.affiliation != "playable")
            {
                target = currentenemytarget;
            }
            else
            {
                target = ActionsMenu.targetlist[ActionsMenu.activetargetid];
            }

            if (target != null)
            {

                Attacker.GetComponent<UnitScript>().disableLifebar = true;
                target.GetComponent<UnitScript>().disableLifebar = true;

                previousattacker = Attacker;
                previoustarget = target;



                Character Chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;
                Character Targetcopy = target.GetComponent<UnitScript>().CreateCopy();


                (GameObject doubleattacker, bool triple) = ActionsMenu.CalculatedoubleAttack(Attacker, target);
                bool ishealing = Attacker.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "staff" && (CharAttacker.affiliation == target.GetComponent<UnitScript>().UnitCharacteristics.affiliation || (CharAttacker.affiliation == "playable" && target.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "other" && !target.GetComponent<UnitScript>().UnitCharacteristics.attacksfriends) || (target.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable" && CharAttacker.affiliation == "other" && CharAttacker.attacksfriends));
                if (ishealing)
                {
                    foresightScript.CreateAction(1, Attacker, target);
                }
                else
                {
                    foresightScript.CreateAction(0, Attacker, target);
                }

                (int attackerhits, int attackercrits, int attackerdamage, int attackerexp, List<int> attackerlevelbonus) = ActionsMenu.ApplyDamage(Attacker, target, false);

                int defenderdodged = 0;

                int basenumberofhits = 0;

                if (doubleattacker == Attacker)
                {
                    if (triple)
                    {
                        basenumberofhits = 3;
                    }
                    else
                    {
                        basenumberofhits = 2;
                    }
                }
                else
                {
                    basenumberofhits = 1;
                }

                defenderdodged = basenumberofhits - attackerhits;

                bool defenderattacks = true;

                if (target.GetComponent<UnitScript>().UnitCharacteristics.currentHP <= 0)
                {
                    defenderattacks = false;
                }
                //if ((target.GetComponent<UnitScript>().UnitCharacteristics.currentHP <= 0 && CharAttacker.affiliation == "playable" && CharAttacker.currentHP > 0) || (CharAttacker.currentHP <= 0 && target.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable" && target.GetComponent<UnitScript>().UnitCharacteristics.currentHP > 0))
                //{
                //    defenderattacks = false;
                //}

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
                int attackerdodged = 0;
                int defenderhits = 0;
                int defenderdamage = 0;
                int defendercrits = 0;
                int defenderexp = 0;
                List<int> defenderlevelbonus = new List<int>();
                if (defenderattacks)
                {
                    (defenderhits, defendercrits, defenderdamage, defenderexp, defenderlevelbonus) = ActionsMenu.ApplyDamage(Attacker, target, true);

                    basenumberofhits = 0;

                    if (doubleattacker == target)
                    {
                        if (triple)
                        {
                            basenumberofhits = 3;
                        }
                        else
                        {
                            basenumberofhits = 2;
                        }
                    }
                    else
                    {
                        basenumberofhits = 1;
                    }

                    attackerdodged = basenumberofhits - defenderhits;

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

                int expearned = 1;

                List<int> levelbonus = new List<int>();

                if (attackerexp != 1)
                {
                    expearned = attackerexp;
                    levelbonus = attackerlevelbonus;
                }
                else if (defenderexp != 1)
                {
                    expearned = defenderexp;
                    levelbonus = defenderlevelbonus;
                }

                if (attackerlevelbonus != null && attackerlevelbonus.Count > 0)
                {
                    levelbonus = attackerlevelbonus;
                }
                else if (defenderlevelbonus != null && defenderlevelbonus.Count > 0)
                {
                    levelbonus = defenderlevelbonus;
                }


                CharAttacker.alreadyplayed = true;
                combatsceneloader.ActivateCombatScene(CharAttacker, Chartarget, Attacker.GetComponent<UnitScript>().GetFirstWeapon(), target.GetComponent<UnitScript>().GetFirstWeapon(), Chardoubleattacker, triple, ishealing, attackerdodged, defenderattacks, defenderdodged, attackerdied, defenderdied, expearned, levelbonus, Attackercopy, Targetcopy, attackerdamage, defenderdamage, attackercrits, defendercrits);

            }
            else
            {
                ActionManager.instance.Wait(Attacker);
            }
        }
    }

    private bool checkifattackanimationisplaying(GameObject attacker, GameObject target)
    {
        return attacker.GetComponent<UnitScript>().isinattackanimation() || target.GetComponent<UnitScript>().isinattackanimation();
    }
    private void EndOfCombatTrigger(GameObject unit1, GameObject unit2 = null)
    {
        MinimapScript.instance.UpdateMinimap();
        if (unit1 != null)
        {
            unit1.GetComponent<UnitScript>().UpdateWeaponModel();
        }

        Character charunit1 = unit1.GetComponent<UnitScript>().UnitCharacteristics;
        if (unit1.GetComponent<UnitScript>().GetSkill(32)) // Survivor
        {
            unit1.GetComponent<UnitScript>().SurvivorStacks++;
            unit1.GetComponent<UnitScript>().AddNumber(unit1.GetComponent<UnitScript>().SurvivorStacks, true, "Survivor");
        }

        if (charunit1.affiliation == "playable")
        {
            DataScript.instance.SpreadBonds(unit1);
        }

        if (unit2 != null)
        {
            Character charunit2 = unit2.GetComponent<UnitScript>().UnitCharacteristics;
            if (unit2.GetComponent<UnitScript>().GetSkill(32)) // Survivor
            {
                unit2.GetComponent<UnitScript>().SurvivorStacks++;
                unit2.GetComponent<UnitScript>().AddNumber(unit2.GetComponent<UnitScript>().SurvivorStacks, true, "Survivor");
            }
            if (charunit2.affiliation == "playable")
            {
                DataScript.instance.SpreadBonds(unit2);
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

    private void WeaponDecison(GameObject unit)
    {
        List<equipment> weaponlist = new List<equipment>();

        int staffID = -1;

        Character unitchar = unit.GetComponent<UnitScript>().UnitCharacteristics;

        foreach (equipment weapon in unitchar.equipments)
        {
            if (weapon.Name != "" && weapon.Name != null && weapon.BaseDamage != 0)
            {
                weaponlist.Add(weapon);
            }
            else
            {
                continue;
            }
            if (weapon.type != "" && weapon.type != null && weapon.type.ToLower() == "staff")
            {
                staffID = weaponlist.IndexOf(weapon);
            }

        }

        if (weaponlist.Count > 1)
        {
            if (staffID != -1)
            {
                GameObject healingtarget = null;

                (int range, bool melee) = unit.GetComponent<UnitScript>().GetRangeAndMele(weaponlist[staffID]);
                gridScript.ShowAttack(range, melee, true, false, unitchar.enemyStats.monsterStats.size, unitchar);
                foreach (GameObject otherunit in gridScript.allunitGOs)
                {
                    if (unit == otherunit)
                    {
                        continue;
                    }

                    if (gridScript.healingtiles.Contains(otherunit.GetComponent<UnitScript>().UnitCharacteristics.currentTile[0]))
                    {
                        if (unitchar.affiliation.ToLower() == otherunit.GetComponent<UnitScript>().UnitCharacteristics.affiliation.ToLower() || (unitchar.affiliation.ToLower() == "other" && !unitchar.attacksfriends))
                        {
                            if (otherunit.GetComponent<UnitScript>().UnitCharacteristics.currentHP / otherunit.GetComponent<UnitScript>().UnitCharacteristics.AjustedStats.HP <= 0.5f)
                            {
                                healingtarget = otherunit;
                                break;
                            }
                        }
                    }
                }

                if (healingtarget != null)
                {
                    int safeguard = 0;
                    while (unit.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() != "staff" && safeguard < 30)
                    {
                        safeguard++;
                        unit.GetComponent<UnitScript>().GetNextWeapon();
                    }
                    return;
                }
                else
                {
                    int safeguard = 0;
                    while (unit.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "staff" && safeguard < 30)
                    {
                        safeguard++;
                        unit.GetComponent<UnitScript>().GetNextWeapon();
                    }
                    return;
                }

            }
            if (unit.GetComponent<RandomScript>().GetPersonalityValue() < 20)
            {
                unit.GetComponent<UnitScript>().GetNextWeapon();
            }
        }

    }


    /// <summary>
    /// Calculate attack Destination and target for AI Characters (second version)
    /// </summary>
    /// <param name="currentCharacter"></param>
    /// <returns></returns>
    private (GridSquareScript, GameObject) CalculateDestinationForOffensiveUnitsV2(GameObject currentCharacter)
    {
        WeaponDecison(currentCharacter);
        Character character = currentCharacter.GetComponent<UnitScript>().UnitCharacteristics;

        List<GridSquareScript> movementtouse = gridScript.movementtiles;
        (int attackerrange, bool attackermelee) = currentCharacter.GetComponent<UnitScript>().GetRangeAndMele();


        List<GridSquareScript> attacktiles = null;

        if (character.enemyStats.personality.ToLower() == "guard")
        {
            movementtouse = new List<GridSquareScript>() { character.currentTile[0] };

            attacktiles = gridScript.GetAttack(attackerrange, attackermelee, character.currentTile[0], character.enemyStats.monsterStats.size, character);
        }
        else
        {
            gridScript.ShowAttack(attackerrange, attackermelee, currentCharacter.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "staff", false, character.enemyStats.monsterStats.size, character);
            attacktiles = gridScript.attacktiles;
        }

        if (currentCharacter.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "staff")
        {
            attacktiles = gridScript.healingtiles;
        }

        List<GameObject> potentialtargets = new List<GameObject>();

        foreach (GameObject unit in gridScript.allunitGOs)
        {
            Character otherchar = unit.GetComponent<UnitScript>().UnitCharacteristics;

            bool skip = true;

            if (unit == currentCharacter)
            {
                continue;
            }

            if (currentCharacter.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "staff")
            {
                if (character.affiliation.ToLower() == otherchar.affiliation.ToLower() || (character.affiliation.ToLower() == "other" && !character.attacksfriends))
                {
                    skip = false;
                }
            }
            else
            {
                if (character.affiliation.ToLower() == "playable" && ((otherchar.affiliation.ToLower() == "other" && otherchar.attacksfriends) || otherchar.affiliation.ToLower() == "enemy"))
                {
                    skip = false;
                }
                if (character.affiliation.ToLower() == "other" && ((otherchar.affiliation.ToLower() == "playable" && character.attacksfriends) || otherchar.affiliation.ToLower() == "enemy"))
                {
                    skip = false;
                }
                if (character.affiliation.ToLower() == "enemy" && ((otherchar.affiliation.ToLower() == "other" && !otherchar.attacksfriends) || otherchar.affiliation.ToLower() == "playable"))
                {
                    skip = false;
                }
            }


            if (skip)
            {
                continue;
            }

            foreach (GridSquareScript tile in attacktiles)
            {
                if (otherchar.currentTile.Contains(tile))
                {
                    potentialtargets.Add(unit);
                    continue;
                }
            }


        }

        GameObject truetarget = null;

        if (potentialtargets.Count > 0)
        {


            float maxreward = 0;
            if (currentCharacter.GetComponent<UnitScript>().UnitCharacteristics.enemyStats.bossiD > 0)
            {
                maxreward = -10000;
            }

            if (character.enemyStats.personality.ToLower() == "deviant" || (character.enemyStats.personality.ToLower() == "coward" && character.currentHP > character.AjustedStats.HP * 0.3f))
            {
                if (potentialtargets.Count > 1)
                {
                    float randomvalue = (float)currentCharacter.GetComponent<RandomScript>().GetPersonalityValue() / 100f;

                    int rankinlist = (int)Mathf.Max(0, (randomvalue * potentialtargets.Count - 1));

                    truetarget = potentialtargets[rankinlist];

                }
                else
                {
                    truetarget = potentialtargets[0];
                }
            }
            else
            {

                foreach (GameObject target in potentialtargets)
                {
                    float reward = calculateRewardforAttacking(currentCharacter, target);
                    if (reward > maxreward)
                    {
                        maxreward = reward;
                        truetarget = target;
                    }
                }
            }

            if (truetarget == null)
            {
                return (CalculateDestinationIfTargetNotFound(currentCharacter), null);
            }

            List<GridSquareScript> targepositiontiles = truetarget.GetComponent<UnitScript>().UnitCharacteristics.currentTile;

            (int range, bool melee) = currentCharacter.GetComponent<UnitScript>().GetRangeAndMele();

            List<GridSquareScript> potentialmovementtile = new List<GridSquareScript>();

            foreach (GridSquareScript movementtile in movementtouse)
            {
                foreach (GridSquareScript attacktilesFromPosition in gridScript.GetAttack(range, melee, movementtile, character.enemyStats.monsterStats.size, character))
                {
                    if (targepositiontiles.Contains(attacktilesFromPosition))
                    {
                        potentialmovementtile.Add(movementtile);
                        continue;
                    }
                }
            }


            List<GridSquareScript> potentialmovementtilesecondFilter = new List<GridSquareScript>();

            Vector2 baseposition = character.currentTile[0].GridCoordinates;


            foreach (GridSquareScript tile in potentialmovementtile)
            {
                currentCharacter.GetComponent<UnitScript>().MoveTo(tile.GridCoordinates);

                if (ActionsMenu.CheckifInRange(truetarget, currentCharacter) && !ActionsMenu.CheckifInRange(currentCharacter, truetarget))
                {
                    potentialmovementtilesecondFilter.Add(tile);
                }
            }
            currentCharacter.GetComponent<UnitScript>().MoveTo(baseposition);
            GridSquareScript finaltile = null;

            if (potentialmovementtilesecondFilter.Count == 1)
            {
                finaltile = potentialmovementtilesecondFilter[0];
            }
            else if (potentialmovementtilesecondFilter.Count > 1)
            {
                int mindistance = 999;

                foreach (GridSquareScript tile in potentialmovementtilesecondFilter)
                {
                    int distance = ManhattanDistance(tile.GridCoordinates, character.currentTile[0].GridCoordinates);
                    if (distance < mindistance)
                    {
                        finaltile = tile;
                        mindistance = distance;
                    }
                }
            }
            else
            {
                int mindistance = 999;

                foreach (GridSquareScript tile in potentialmovementtile)
                {
                    int distance = ManhattanDistance(tile.GridCoordinates, character.currentTile[0].GridCoordinates);
                    if (distance < mindistance)
                    {
                        finaltile = tile;
                        mindistance = distance;
                    }
                }
            }

            return (finaltile, truetarget);
        }
        else
        {

            int personalityvalue = currentCharacter.GetComponent<RandomScript>().GetPersonalityValue();



            if (personalityvalue <= 5 || character.enemyStats.personality.ToLower() == "deviant" || (character.enemyStats.personality.ToLower() == "coward" && character.currentHP > character.AjustedStats.HP * 0.3f))
            {
                float randomvalue = (float)personalityvalue / 100f;
                int rankinlist = (int)Mathf.Max(0, (randomvalue * gridScript.movementtiles.Count - 1));

                return (gridScript.movementtiles[rankinlist], null);
            }
            else
            {

                return (CalculateDestinationIfTargetNotFound(currentCharacter), null);
            }
        }
    }

    private GridSquareScript CalculateDestinationIfTargetNotFound(GameObject currentCharacter)
    {
        Character Character = currentCharacter.GetComponent<UnitScript>().UnitCharacteristics;

        List<GridSquareScript> movementtiles = gridScript.movementtiles;
        foreach (GridSquareScript square in movementtiles)
        {
            if (square == Character.currentTile[0] || !gridScript.CheckIfFree(square.GridCoordinates, Character))
            {
                continue;
            }
            foreach (GameObject unit in gridScript.allunitGOs)
            {
                Character otherchar = unit.GetComponent<UnitScript>().UnitCharacteristics;

                bool skip = true;

                if (unit == currentCharacter)
                {
                    continue;
                }

                if (currentCharacter.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "staff")
                {
                    if (Character.affiliation.ToLower() == otherchar.affiliation.ToLower() || (Character.affiliation.ToLower() == "other" && !Character.attacksfriends))
                    {
                        skip = false;
                    }
                }
                else
                {
                    if (Character.affiliation.ToLower() == "other" && ((otherchar.affiliation.ToLower() == "playable" && Character.attacksfriends) || otherchar.affiliation.ToLower() == "enemy"))
                    {

                        skip = false;
                    }
                    if (Character.affiliation.ToLower() == "enemy" && ((otherchar.affiliation.ToLower() == "other" && !otherchar.attacksfriends) || otherchar.affiliation.ToLower() == "playable"))
                    {
                        skip = false;
                    }
                }



                if (skip)
                {
                    continue;
                }

                int distancediff = ManhattanDistance(otherchar, Character) - (Character.movements - 2) - ManhattanDistance(otherchar.currentTile[0].GridCoordinates, square.GridCoordinates);

                if (distancediff <= 0)
                {
                    return square;
                }
            }
        }
        return null;
    }


    /// <summary>
    /// Calculate attack Destination and target for AI Characters (second version)
    /// </summary>
    /// <param name="currentCharacter"></param>
    /// <returns></returns>
    private GameObject CalculateDestinationForBoss(GameObject currentCharacter)
    {
        WeaponDecison(currentCharacter);
        Character character = currentCharacter.GetComponent<UnitScript>().UnitCharacteristics;

        List<GridSquareScript> attacktiles = gridScript.attacktiles;

        List<GameObject> potentialtargets = new List<GameObject>();

        foreach (GameObject unit in gridScript.allunitGOs)
        {
            Character otherchar = unit.GetComponent<UnitScript>().UnitCharacteristics;

            bool skip = true;

            if (unit == currentCharacter)
            {
                continue;
            }

            if (character.affiliation.ToLower() == "playable" && ((otherchar.affiliation.ToLower() == "other" && otherchar.attacksfriends) || otherchar.affiliation.ToLower() == "enemy"))
            {
                skip = false;
            }
            if (character.affiliation.ToLower() == "other" && ((otherchar.affiliation.ToLower() == "playable" && character.attacksfriends) || otherchar.affiliation.ToLower() == "enemy"))
            {
                skip = false;
            }
            if (character.affiliation.ToLower() == "enemy" && (otherchar.affiliation.ToLower() == "playable" || (otherchar.affiliation.ToLower() == "other" && !otherchar.attacksfriends)))
            {
                skip = false;
            }


            if (!skip)
            {
                potentialtargets.Add(unit);
            }


        }

        GameObject truetarget = null;

        float maxreward = 0;

        foreach (GameObject target in potentialtargets)
        {
            float reward = calculateRewardforAttacking(currentCharacter, target, true);
            if (reward > maxreward)
            {
                maxreward = reward;
                truetarget = target;
            }
        }

        return truetarget;
    }

    /// <summary>
    /// Calculate Reward for attacking target
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    private float calculateRewardforAttacking(GameObject attacker, GameObject target, bool isboss = false)
    {
        float reward = 0f;

        Character attackerChar = attacker.GetComponent<UnitScript>().UnitCharacteristics;
        Character targetChar = target.GetComponent<UnitScript>().UnitCharacteristics;

        if (targetChar.TauntTurns > 0)
        {
            reward += 999f;
        }

        if (attacker.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "staff")
        {
            reward = 10f * (targetChar.AjustedStats.HP - targetChar.currentHP) / targetChar.AjustedStats.HP;
        }
        else
        {
            float killFactor = 10f;
            float NoCounterFactor = 5f;
            float hitchanceFactor = 2f;
            float DodgeChanceFactor = 2f;
            float SurvivesFactor = 3f;

            if (isboss)
            {
                NoCounterFactor = 0f;
                DodgeChanceFactor = 0f;
                reward += 50 - ManhattanDistance(attackerChar, targetChar);
            }

            if (attackerChar.enemyStats.personality.ToLower() == "survivor" || (attackerChar.enemyStats.personality.ToLower() == "coward" && attackerChar.currentHP < attackerChar.AjustedStats.HP * 0.1f))
            {
                DodgeChanceFactor *= 3f;
                SurvivesFactor *= 3f;
                NoCounterFactor *= 2;
            }

            if (attackerChar.enemyStats.personality.ToLower() == "daredevil")
            {
                DodgeChanceFactor = 0f;
                SurvivesFactor = 0f;
                NoCounterFactor = 0f;
            }

            int rawdamage = ActionsMenu.CalculateDamage(attacker, target);
            int rawdamagetaken = ActionsMenu.CalculateDamage(target, attacker);
            int hitrate = ActionsMenu.CalculateHit(attacker, target);
            int dodgerate = 100 - ActionsMenu.CalculateHit(target, attacker);

            bool inrange = ActionsMenu.CheckifInRange(attacker, target);
            if (!inrange)
            {
                rawdamagetaken = 0;
            }


            float ratioofhptaken = Mathf.Max(0, targetChar.currentHP - rawdamage) / targetChar.currentHP;

            reward += killFactor * ratioofhptaken;

            if (rawdamage >= targetChar.currentHP)
            {
                reward += killFactor;
            }

            if (rawdamagetaken == 0)
            {
                reward += NoCounterFactor;
            }

            reward += hitchanceFactor * (float)hitrate / 100f;
            reward -= DodgeChanceFactor * (float)(1 - dodgerate) / 100f;

            float ratioofhptakenbyattacker = Mathf.Max(0, attackerChar.currentHP - rawdamagetaken) / attackerChar.currentHP;

            reward += SurvivesFactor * ratioofhptakenbyattacker;
        }



        return reward;
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
        List<GridSquareScript> potentialAttackPosition = gridScript.GetAttack(range, melee, position, charunit.enemyStats.monsterStats.size, charunit);
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
        if (!SaveManager.instance.Options.BattleAnimations)
        {
            MapEventManager.instance.TriggerEventCheck();
        }

        minimapScript.UpdateMinimap();
    }

}