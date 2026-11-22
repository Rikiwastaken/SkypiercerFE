using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CalculationToolsScript;
using static DataScript;
using static TextBubbleScript;
using static UnitScript;
public class AttackTurnScript : MonoBehaviour
{

    [Header("Pointers")]
    private TurnManger TurnManager;
    [SerializeField] private ExpBarScript expbar;

    [SerializeField] private UnitScript CurrentEnemy;
    [SerializeField] private UnitScript CurrentPlayable;
    [SerializeField] private UnitScript CurrentOther;

    private GridScript gridScript;

    [SerializeField] public ActionsMenu ActionsMenu;
    [SerializeField] public PhaseTextScript phaseTextScript;

    private cameraScriptV2 camScriptV2;

    private bool TurnInProgress;

    [Header("combat timing variables")]

    [SerializeField] private float delaybeforeMove;
    private int counterbeforemove;
    [SerializeField] private float delaybeforeAttack;
    private int counterbeforeattack;

    [SerializeField] private float delaybeforeFirstAttack;
    private int counterbeforeFirstAttack;

    private bool unitalreadyattacked;

    [SerializeField] private float delaybetweenAttack;
    private int counterbetweenattack;

    [SerializeField] private float delayafterAttack;
    private int counterafterattack;

    private bool attacktrigger;

    public bool waittingforexp;

    public bool expdistributed;



    private int expgained;

    private List<int> levelupbonuses;

    private bool triggercleanup;

    private CombatTextScript combatTextScript;

    public int delaybeforenxtunit;

    [SerializeField] private float TimebeforeAnimationAttack;


    private MinimapScript minimapScript;

    [SerializeField] private ForesightScript foresightScript;

    private SaveManager saveManager;

    public int attackanimationhappeningcnt;

    public bool triggerEndOfFightWithAnimations;

    private UnitScript previousattacker;
    private UnitScript previoustarget;

    private CombatSceneLoader combatsceneloader;

    private GameObject currentenemytarget;

    private BattleInfotext battleInfotextScript;

    public Coroutine AttackCoroutine;

    private TextBubbleScript _TextBubbleScript;

    public bool mapwascompleted;


    private UnitScript cachedDecisionUnit;
    private GridSquareScript cachedDecisionDestination;
    private GameObject cachedDecisionTarget;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        minimapScript = FindAnyObjectByType<MinimapScript>();
        TurnManager = GetComponent<TurnManger>();
        gridScript = GetComponent<GridScript>();
        saveManager = FindAnyObjectByType<SaveManager>();
        combatsceneloader = FindAnyObjectByType<CombatSceneLoader>();
        battleInfotextScript = FindAnyObjectByType<BattleInfotext>(FindObjectsInactive.Include);
        camScriptV2 = cameraScriptV2.instance;
    }

    // Update is called once per frame
    void Update()
    {
        // bool that activates when map objectives has been achieved;
        if (mapwascompleted)
        {
            return;
        }

        if (triggerEndOfFightWithAnimations)
        {
            attackanimationhappeningcnt = 0;
        }

        // If in the combat animation scene, ignore the update of this script to avoid bugs with the combat animation scene and to avoid the combat animation scene being skipped when an attack happens while the combat animation scene is playing
        if (attackanimationhappeningcnt > 0)
        {
            attackanimationhappeningcnt--;
            return;
        }

        // Ignore update if level up screen is open to avoid bugs with attacking while level up screen is open and to avoid the level up screen closing when an attack happens
        if (FreezeFrameCapture.instance != null && FreezeFrameCapture.instance.ShowingLevelUp)
        {
            return;
        }

        // ignore update if dialogue happening
        if (_TextBubbleScript == null)
        {
            _TextBubbleScript = GetComponent<ActionManager>().TextBubbleScript;
        }
        if (_TextBubbleScript.indialogue)
        {
            return;
        }

        // If Combat Animation is over, reset the lifebars of the attacker and target, trigger the end of fight attack animation if the attacker is still alive, reset all selections on the grid, trigger the end of fight event, and reset variables related to the end of fight and combat animation
        if (triggerEndOfFightWithAnimations && previousattacker != null && AttackCoroutine == null && !combatsceneloader.isCombatSceneActive())
        {
            triggerEndOfFightWithAnimations = false;
            previousattacker.GetComponent<UnitScript>().disableLifebar = false;
            previousattacker.GetComponent<UnitScript>().ManageLifebars();
            gridScript.ResetAllSelections();
            AttackWithAnimationEndOfFight(previousattacker, previoustarget);
            if (previoustarget != null)
            {
                previoustarget.GetComponent<UnitScript>().disableLifebar = false;
                previoustarget.GetComponent<UnitScript>().ManageLifebars();
            }
            previousattacker = null;
            previoustarget = null;

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

        // If the current turn is not the player's turn and the action manager exists, prevent the action manager from locking after an action is taken to allow the enemy and other units to move and attack without being locked out of taking further actions until their turn is over
        if ((TurnManager.currentlyplaying != "playable" && TurnManager.currentlyplaying != "tutorial") && ActionManager.instance != null)
        {
            ActionManager.instance.preventfromlockingafteraction = true;
        }

        // Select the next enemy unit to move and attack if there is no current enemy unit selected
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
                    UnitScript UnitUS = unit.GetComponent<UnitScript>();
                    Character charunit = UnitUS.UnitCharacteristics;
                    if (charunit.affiliation == "enemy" && !charunit.alreadyplayed && UnitUS.CheckIfOnActivated())
                    {

                        if (!determineifActionifTaken(UnitUS))
                        {
                            ActionManager.instance.Wait(UnitUS);
                            continue;
                        }

                        CurrentEnemy = UnitUS;
                        battleInfotextScript.previousEnemy = unit;
                        counterbeforemove = (int)(delaybeforeMove / Time.fixedDeltaTime);
                        gridScript.ShowMovementOfUnit(UnitUS);
                        break;

                    }
                }

            }
        }

        // If the current turn is the enemy's turn and there is a current enemy, move the current enemy and attack 
        else if (TurnManager.currentlyplaying == "enemy")
        {
            if (NPCAttackFunction(CurrentEnemy))
            {
                CurrentEnemy = null;
            }

        }

        // Select the next other unit to move and attack if there is no current other unit selected
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
                    UnitScript UnitUS = unit.GetComponent<UnitScript>();

                    Character charunit = UnitUS.UnitCharacteristics;
                    if (charunit.affiliation == "other" && !charunit.alreadyplayed && UnitUS.CheckIfOnActivated())
                    {

                        if (!determineifActionifTaken(UnitUS))
                        {
                            ActionManager.instance.Wait(UnitUS);
                            continue;
                        }

                        CurrentOther = UnitUS;
                        battleInfotextScript.previousOther = unit;
                        counterbeforemove = (int)(delaybeforeMove / Time.fixedDeltaTime);
                        gridScript.ShowMovementOfUnit(UnitUS);
                        break;

                    }
                }

            }
        }

        else if (TurnManager.currentlyplaying == "other")
        {

            if (NPCAttackFunction(CurrentOther))
            {
                CurrentOther = null;
            }
        }

        //playerattack
        if (TurnManager.currentlyplaying == "playable" || TurnManager.currentlyplaying == "tutorial")
        {
            if (ActionsMenu.target == null || ActionsMenu.target.GetComponent<UnitScript>().UnitCharacteristics.currentHP <= 0)
            {
                return;
            }
            UnitScript CurrentPlayable = ActionsMenu.target.GetComponent<UnitScript>();
            Character CurrentPlayableChar = CurrentPlayable.UnitCharacteristics;
            if (ActionsMenu.confirmattack && CurrentPlayable != null)
            {
                if (ActionsMenu.CommandUsedID == 0)
                {
                    Character CharAttacker = CurrentPlayable.UnitCharacteristics;
                    GameObject target = null;
                    if (CharAttacker.affiliation != "playable")
                    {
                        target = currentenemytarget;
                    }
                    else
                    {
                        target = ActionsMenu.targetlist[ActionsMenu.activetargetid];
                    }
                    if (saveManager.Options.BattleAnimations && (target != null && target.GetComponent<UnitScript>().UnitCharacteristics.affiliation != "breakable"))
                    {
                        if (Vector2.Distance(CurrentPlayableChar.currentTile.GridCoordinates, new Vector2(CurrentPlayable.transform.position.x, CurrentPlayable.transform.position.z)) <= 1f)
                        {
                            ManageAttackWithAnimation(CurrentPlayable);
                        }
                    }
                    else
                    {
                        if (AttackCoroutine == null)
                        {
                            AttackCoroutine = StartCoroutine(ManageAttackWithoutAnimation(CurrentPlayable));
                        }
                    }
                }
                else
                {
                    ManageCommand(CurrentPlayable);
                }

            }
        }



        if (!ActionsMenu.incombat && triggercleanup)
        {
            DeathCleanup();

        }
    }

    private bool NPCAttackFunction(UnitScript AttackerUS)
    {
        Character charAttacker = AttackerUS.UnitCharacteristics;
        if (!ActionsMenu.incombat)
        {
            camScriptV2.Destination = charAttacker.currentTile.GridCoordinates;
        }


        if (!charAttacker.alreadymoved)
        {
            if (counterbeforemove > 0)
            {
                counterbeforemove--;
            }
            else
            {
                currentenemytarget = null;
                GridSquareScript Destination = null;

                // Calculate Destination
                if (AttackerUS.UnitCharacteristics.enemyStats.bossiD > 0)
                {
                    AttackerUS.GetComponent<BossScript>().nextTarget = CalculateDestinationForBoss(AttackerUS);
                    Destination = AttackerUS.UnitCharacteristics.currentTile;
                }
                else
                {
                    if (cachedDecisionUnit == AttackerUS)
                    {
                        Destination = cachedDecisionDestination;
                        currentenemytarget = cachedDecisionTarget;
                        cachedDecisionUnit = null;
                    }
                    else
                    {
                        (Destination, currentenemytarget) = CalculateDestinationForOffensiveUnitsV2(AttackerUS, false);
                    }
                }

                // Move Enemy
                Vector2 DestinationVector = charAttacker.currentTile.GridCoordinates;
                if (Destination != null)
                {
                    DestinationVector = Destination.GridCoordinates;
                    AttackerUS.MoveTo(DestinationVector);
                    counterbeforeattack = (int)(delaybeforeAttack / Time.fixedDeltaTime);
                    charAttacker.alreadymoved = true;
                }
                else
                {

                    AttackerUS.MoveTo(DestinationVector);
                    counterbeforeattack = 0;
                    charAttacker.alreadymoved = true;
                }
            }
        }
        else if (AttackerUS != null && !charAttacker.alreadyplayed)
        {
            if (AttackerUS.UnitCharacteristics.enemyStats.bossiD > 0)
            {
                AttackerUS.GetComponent<BossScript>().TriggerBossAttack();
                charAttacker.alreadyplayed = true;
            }
            else
            {

                GameObject target = null;
                if (charAttacker.affiliation != "playable")
                {
                    target = currentenemytarget;
                }
                else
                {
                    target = ActionsMenu.targetlist[ActionsMenu.activetargetid];
                }
                if (saveManager.Options.BattleAnimations && (target != null && target.GetComponent<UnitScript>().UnitCharacteristics.affiliation != "breakable"))
                {
                    if (Vector2.Distance(charAttacker.currentTile.GridCoordinates, new Vector2(AttackerUS.transform.position.x, AttackerUS.transform.position.z)) <= 1f)
                    {
                        ManageAttackWithAnimation(AttackerUS);
                    }
                }
                else
                {
                    if (AttackCoroutine == null)
                    {
                        AttackCoroutine = StartCoroutine(ManageAttackWithoutAnimation(AttackerUS));
                    }
                }
            }

        }
        else
        {
            return true;
        }
        return false;
    }

    //returns true if unit does an action

    private bool determineifActionifTaken(UnitScript unit)
    {
        cachedDecisionUnit = null;
        cachedDecisionDestination = null;
        cachedDecisionTarget = null;
        Character charunit = unit.UnitCharacteristics;


        if (charunit.enemyStats.personality.ToLower() == "hunter" || charunit.enemyStats.bossiD > 0)
        {
            return true;
        }

        List<Character> enemies = null;
        if (charunit.affiliation == "enemy")
        {
            enemies = TurnManager.playableunit;
            foreach (Character other in TurnManager.otherunits)
            {
                if (!other.attacksfriends)
                {
                    enemies.Add(other);
                }
            }
        }
        else if (charunit.affiliation == "other")
        {
            if (charunit.attacksfriends)
            {
                enemies = TurnManager.playableunit;
            }
            else
            {
                enemies = TurnManager.enemyunit;
            }


        }




        // start by calculating min dist to every other unit

        int distanceformovement = 10;

        int mindistance = 99;

        foreach (Character otherunit in enemies)
        {
            int distance = ManhattanDistance(otherunit, charunit);
            if (distance < mindistance)
            {
                mindistance = distance;
                if (mindistance < distanceformovement)
                {
                    break;
                }
            }
        }

        if (mindistance >= distanceformovement)
        {
            return false;
        }


        Vector2 originalpos = charunit.currentTile.GridCoordinates;
        gridScript.ShowMovementOfUnit(unit);

        GridSquareScript Destination = null;
        GameObject target = null;

        (Destination, target) = CalculateDestinationForOffensiveUnitsV2(unit, true);

        cachedDecisionUnit = unit;
        cachedDecisionDestination = Destination;
        cachedDecisionTarget = target;

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
            (Destination, target) = CalculateDestinationForOffensiveUnitsV2(unit, true);
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
    private void ManageCommand(UnitScript User)
    {
        GameObject Target = ActionsMenu.targetlist[ActionsMenu.activetargetid];
        Character CharUser = User.UnitCharacteristics;
        Character CharTarget = null;
        if (Target.GetComponent<UnitScript>() != null)
        {
            CharTarget = Target.GetComponent<UnitScript>().UnitCharacteristics;
        }

        int commandID = ActionsMenu.CommandUsedID;


        if (commandID == 47) //Transfuse
        {
            foresightScript.CreateAction(3, User, Target.GetComponent<UnitScript>());
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
                User.AddNumber(previousHPUser - CharUser.currentHP, false, "Transfuse");
            }
            else
            {
                User.AddNumber(CharUser.currentHP - previousHPUser, true, "Transfuse");
            }

        }
        else if (commandID == 48) //Motivate
        {
            foresightScript.CreateAction(3, User, Target.GetComponent<UnitScript>());
            CharTarget.alreadymoved = false;
            CharTarget.alreadyplayed = false;
            CharTarget.motivateusedthisturn = true;
            Target.GetComponent<UnitScript>().AddNumber(0, true, "Motivate");
        }
        else if (commandID == 49) //Swap
        {
            foresightScript.CreateAction(3, User, Target.GetComponent<UnitScript>());
            Vector2 previoususerpos = CharUser.currentTile.GridCoordinates;
            User.MoveTo(CharTarget.currentTile.GridCoordinates);
            Target.GetComponent<UnitScript>().MoveTo(previoususerpos);
            Target.GetComponent<UnitScript>().AddNumber(0, true, "Swap");
            User.AddNumber(0, true, "Swap");
        }
        else if (commandID == 50) //Reinvigorate
        {
            foresightScript.CreateAction(3, User, Target.GetComponent<UnitScript>());
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
            Vector2 coorddiff = targettile.GridCoordinates - CharUser.currentTile.GridCoordinates;

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

            User.MoveTo(targettile.GridCoordinates + offset, true);

            User.AddNumber(0, true, "Jump");
        }
        else if (commandID == 52) // Fortify
        {
            foresightScript.CreateAction(3, User);
            GridSquareScript tile = CharUser.currentTile;


            tile.type = "Fortification";
            User.AddNumber(0, true, "Fortify");
        }
        else if (commandID == 53) // Smoke Bomb
        {
            GridSquareScript tile = CharUser.currentTile;
            foresightScript.CreateAction(3, User);
            tile.type = "Fog";
            User.AddNumber(0, true, "Smoke Bomb");
        }
        else if (commandID == 54) // Chakra
        {
            foresightScript.CreateAction(3, User);
            List<int> Skillvalues = DataScript.instance.SkillList[54].SkillValues;
            float multiplier = User.GetSkillLevelMultiplier(54);
            float HealRatio = (Skillvalues[0] / 100f * multiplier);
            int healthrestored = (int)((CharUser.AjustedStats.HP - CharUser.currentHP) * HealRatio);
            CharUser.currentHP += healthrestored;
            User.RemoveStatusAilments();
            User.AddNumber(healthrestored, true, "Chakra");
        }
        else if (commandID == 56) // Copy
        {
            foresightScript.CreateAction(3, User, CharTarget.UnitSkill);
            if (CharTarget.UnitSkill != 0 && !Target.GetComponent<UnitScript>().copied)
            {
                Target.GetComponent<UnitScript>().copied = true;
                DataScript.instance.SaveCopyFlag(CharTarget.enemyStats.CopyID, MapInitializer.instance.IsSideStory);
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
                User.AddNumber(0, true, DataScript.instance.SkillList[CharTarget.UnitSkill].name + " copied");
            }
        }
        else if (commandID == 59) // Sundance
        {
            foresightScript.CreateAction(3, User);
            WeatherManager weathermanager = FindAnyObjectByType<WeatherManager>();
            if (weathermanager.rainymap)
            {
                List<GridSquareScript> tilestochange = new List<GridSquareScript>();
                GridSquareScript tile = User.UnitCharacteristics.currentTile;
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

                foreach (GridSquareScript newtilestile in tilestochange)
                {
                    if (newtilestile != null)
                    {
                        newtilestile.RemainingRainTurns = 0;
                        newtilestile.RemainingSunTurns = 2;
                    }

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
                GridSquareScript tile = User.UnitCharacteristics.currentTile;
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
                foreach (GridSquareScript newtile in tilestochange)
                {
                    newtile.RemainingRainTurns = 2;
                    newtile.RemainingSunTurns = 0;
                }
            }
        }
        else if (commandID == 70) // Blade Conversion
        {
            foresightScript.CreateAction(3, User);
            List<int> Skillvalues = DataScript.instance.SkillList[70].SkillValues;
            float multiplier = User.GetSkillLevelMultiplier(70);
            float Ratio = (Skillvalues[0] / 100f * multiplier);
            int healthrestored = (int)Mathf.Min((CharUser.AjustedStats.HP - CharUser.currentHP), CharUser.AjustedStats.HP * Ratio);
            CharUser.currentHP += healthrestored;
            User.GetFirstWeapon().Currentuses = 0;
            User.AddNumber(healthrestored, true, "Blade Conversion");
        }
        else if (commandID == 71) // Blade Sacrifice
        {
            foresightScript.CreateAction(3, User);
            List<int> Skillvalues = DataScript.instance.SkillList[71].SkillValues;
            float multiplier = User.GetSkillLevelMultiplier(71);
            float Ratio = (Skillvalues[0] / 100f * multiplier);
            int healthlost = (int)Mathf.Min(CharUser.currentHP - 1, CharUser.AjustedStats.HP * Ratio);
            CharUser.currentHP -= healthlost;
            User.GetFirstWeapon().Currentuses = 0;
            User.AddNumber(healthlost, false, "Blade Sacrifice");
        }
        else if (commandID == 76) // Taunt
        {
            foresightScript.CreateAction(3, User, Target.GetComponent<UnitScript>());
            User.UnitCharacteristics.TauntTurns = 1;
            User.AddNumber(0, true, "Taunt");
        }
        else if (commandID == 79) // Break
        {
            foresightScript.CreateAction(3, User, Target.GetComponent<UnitScript>());
            int lostdurability = (int)Mathf.Max(1, CharUser.AjustedStats.Strength / (Target.GetComponent<UnitScript>().GetFirstWeapon().Grade));
            Target.GetComponent<UnitScript>().GetFirstWeapon().Currentuses = Mathf.Max(Target.GetComponent<UnitScript>().GetFirstWeapon().Currentuses - lostdurability, 0);
            Target.GetComponent<UnitScript>().AddNumber(0, true, "Break");
        }
        else if (commandID == 81) // Hook
        {
            foresightScript.CreateAction(3, User, Target.GetComponent<UnitScript>());



            Vector2 targetposition = Target.GetComponent<UnitScript>().UnitCharacteristics.currentTile.GridCoordinates;
            Vector2 Userposition = User.UnitCharacteristics.currentTile.GridCoordinates;

            Vector2 direction = (targetposition - Userposition).normalized;
            Target.GetComponent<UnitScript>().MoveTo(Userposition + direction, true, true);
            Target.GetComponent<UnitScript>().AddNumber(0, true, "Hook");
        }
        else if (commandID == 82) // Throw
        {
            foresightScript.CreateAction(3, User, Target.GetComponent<UnitScript>());



            Vector2 targetposition = Target.GetComponent<UnitScript>().UnitCharacteristics.currentTile.GridCoordinates;
            Vector2 Userposition = User.UnitCharacteristics.currentTile.GridCoordinates;

            Vector2 direction = (targetposition - Userposition).normalized;


            GridSquareScript potentialblockerTile = GridScript.instance.GetTile(Userposition + direction * 3);
            GameObject potentialblocker = GridScript.instance.GetUnit(potentialblockerTile);
            if (potentialblocker == null)
            {
                Target.GetComponent<UnitScript>().MoveTo(Userposition + direction * 3, true, true);
                Target.GetComponent<UnitScript>().AddNumber(0, true, "Throw");
            }
            else
            {
                potentialblockerTile = GridScript.instance.GetTile(Userposition + direction * 2);
                potentialblocker = GridScript.instance.GetUnit(potentialblockerTile);
                Target.GetComponent<UnitScript>().MoveTo(Userposition + direction * 2, true, true);
                Target.GetComponent<UnitScript>().AddNumber(0, true, "Throw");
            }
        }
        else if (commandID == 84) // Reverse Blade Technique
        {
            foresightScript.CreateAction(3, User);
            int healthrestored = (int)(CharUser.AjustedStats.HP - CharUser.currentHP);
            CharUser.currentHP += healthrestored;

            foreach (equipment equ in CharUser.equipments)
            {
                equ.Currentuses = equ.Maxuses;
            }

            CharUser.statusEffects.StunTurns++;
            User.AddNumber(healthrestored, true, "Reverse Blade Technique");
        }
        else if (commandID == 88) //Cure
        {
            foresightScript.CreateAction(3, User, Target.GetComponent<UnitScript>());
            Target.GetComponent<UnitScript>().RemoveStatusAilments();
        }
        else if (commandID == 91) // All In
        {
            foresightScript.CreateAction(3, User);
            int healthlost = CharUser.currentHP - 1;
            CharUser.currentHP = 1;
            CharUser.statusEffects.PowerTurns += 3;
            CharUser.statusEffects.RegenTurns += 3;
            CharUser.statusEffects.AccelerationTurns += 3;
            User.AddNumber(healthlost, false, "All In");
        }
        else if (commandID == 96) // Power Transfer
        {
            List<int> Skillvalues = DataScript.instance.SkillList[95].SkillValues;
            float multiplier = User.GetSkillLevelMultiplier(96);
            int ExamodeGain = (int)(Skillvalues[0] * multiplier);
            User.IncreaseExamodeGauge(ExamodeGain);
            User.AddNumber(0, true, "Power Transfer");
        }
        else if (commandID == 114) // Study
        {
            foresightScript.CreateAction(3, User, Target.GetComponent<UnitScript>());

            int skilltoStudy = 0;

            if (CharTarget.UnitSkill > 0)
            {
                skilltoStudy = CharTarget.UnitSkill;
            }
            else if (CharTarget.SecondUnitSkill > 0 && CharTarget.SecondSkillUnlocked)
            {
                skilltoStudy = CharTarget.SecondUnitSkill;
            }
            else if (CharTarget.EquipedSkills.Count > 0)
            {
                skilltoStudy = CharTarget.EquipedSkills[0];
            }

            CharUser.TemporarySkill = skilltoStudy;
            User.AddNumber(0, true, "Study");
        }
        ActionsMenu.FinalizeAttack();

    }

    public void AttackWithAnimationEndOfFight(UnitScript Attacker, UnitScript Target)
    {

        StartCoroutine(FreezeFrameGeneral.instance.Hide());

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


    public void ManageAttackWithAnimation(UnitScript AttackerUS)
    {

        if (attackanimationhappeningcnt == 0)
        {
            attackanimationhappeningcnt = (int)(TimebeforeAnimationAttack / Time.deltaTime);
            Character CharAttacker = AttackerUS.UnitCharacteristics;

            Character Attackercopy = CreateCopy(CharAttacker);




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

                FreezeFrameGeneral.instance.Show();
                UnitScript TargetUS = target.GetComponent<UnitScript>();

                AttackerUS.disableLifebar = true;
                TargetUS.disableLifebar = true;

                previousattacker = AttackerUS;
                previoustarget = TargetUS;



                Character Chartarget = TargetUS.UnitCharacteristics;
                Character Targetcopy = CreateCopy(Chartarget);


                AllStatsSkillBonus AttackerStatBonus = AttackerUS.GetStatSkillBonus(TargetUS, false);
                AllStatsSkillBonus DefenderStatBonus = TargetUS.GetStatSkillBonus(AttackerUS, false);


                (UnitScript doubleattacker, bool triple) = ActionsMenu.CalculatedoubleAttack(AttackerUS, TargetUS, AttackerStatBonus, DefenderStatBonus);
                bool ishealing = AttackerUS.GetFirstWeapon().type.ToLower() == "staff" && (CharAttacker.affiliation == TargetUS.UnitCharacteristics.affiliation || (CharAttacker.affiliation == "playable" && TargetUS.UnitCharacteristics.affiliation == "other" && !TargetUS.UnitCharacteristics.attacksfriends) || (TargetUS.UnitCharacteristics.affiliation == "playable" && CharAttacker.affiliation == "other" && !CharAttacker.attacksfriends));
                if (ishealing)
                {
                    foresightScript.CreateAction(1, AttackerUS, TargetUS);
                }
                else
                {
                    foresightScript.CreateAction(0, AttackerUS, TargetUS);
                }

                int attackerhit = ActionsMenu.CalculateHit(AttackerUS, TargetUS, AttackerStatBonus, DefenderStatBonus);
                int defenderhit = ActionsMenu.CalculateHit(TargetUS, AttackerUS, DefenderStatBonus, AttackerStatBonus);

                int attackerdmg = 0;
                int attackercrit = 0;

                if (ishealing)
                {
                    attackerdmg = ActionsMenu.CalculateHealing(AttackerUS, AttackerStatBonus);
                }
                else
                {
                    attackerdmg = ActionsMenu.CalculateDamage(AttackerUS, AttackerStatBonus, TargetUS, DefenderStatBonus);
                    attackercrit = ActionsMenu.CalculateCrit(AttackerUS, TargetUS, AttackerStatBonus, DefenderStatBonus);
                }

                int defenderdmg = ActionsMenu.CalculateDamage(TargetUS, DefenderStatBonus, AttackerUS, AttackerStatBonus);
                int defendercrit = ActionsMenu.CalculateCrit(TargetUS, AttackerUS, DefenderStatBonus, AttackerStatBonus);




                (List<int> attackerhits, int attackerdamage, int attackerexp, List<int> attackerlevelbonus, List<int> attackerdamagelist, List<int> AttackerSkillTriggers, List<int> DefenderSkillTriggers) = ActionsMenu.ApplyDamage(AttackerUS, TargetUS, false, ishealing);



                List<int> defenderhits = new List<int>();
                int defenderdamage = 0;
                int defenderexp = 0;
                List<int> defenderlevelbonus = new List<int>();
                List<int> defenderdamagelist = new List<int>();
                List<int> AttackerSkillTriggersDuringCounterAttack = new List<int>();
                List<int> DefenderSkillTriggersDuringCounterAttack = new List<int>();

                if ((ActionsMenu.CheckifInRange(AttackerUS, TargetUS) || TargetUS.GetSkill(38) && Chartarget.statusEffects.StunTurns <= 0) && !ishealing)
                {

                    if (Chartarget.currentHP > 0)
                    {
                        (defenderhits, defenderdamage, defenderexp, defenderlevelbonus, defenderdamagelist, AttackerSkillTriggersDuringCounterAttack, DefenderSkillTriggersDuringCounterAttack) = ActionsMenu.ApplyDamage(AttackerUS, TargetUS, true, false);
                    }
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
                combatsceneloader.ActivateCombatScene(Attackercopy, Targetcopy, attackerhits, attackerdamagelist, AttackerSkillTriggers, DefenderSkillTriggers, defenderhits, defenderdamagelist, AttackerSkillTriggersDuringCounterAttack, DefenderSkillTriggersDuringCounterAttack, attackerdmg, attackerhit, attackercrit, defenderdmg, defenderhit, defendercrit, expearned, levelbonus);

            }
            else
            {
                ActionManager.instance.Wait(AttackerUS);
            }
        }
    }

    public IEnumerator ManageAttackWithoutAnimation(UnitScript Attacker)
    {
        int safeguard = 0;
        Character CharAttacker = Attacker.UnitCharacteristics;
        Character Attackercopy = CreateCopy(CharAttacker);

        //setup target
        GameObject target = null;

        if (CharAttacker.affiliation != "playable")
        {
            target = currentenemytarget;
        }
        else
        {
            target = ActionsMenu.targetlist[ActionsMenu.activetargetid];
        }


        triggercleanup = true;


        //Begin fight
        if (target != null)
        {
            UnitScript TargetUS = target.GetComponent<UnitScript>();
            if (combatTextScript == null)
            {
                combatTextScript = FindAnyObjectByType<CombatTextScript>(FindObjectsInactive.Include);
            }
            combatTextScript.ResetInfo();



            //Vector3 targetlook = new Vector3(Attacker.transform.position.x, target.transform.position.y, Attacker.transform.position.z);
            //target.transform.LookAt(targetlook);
            //Vector3 attackerlook = new Vector3(target.transform.position.x, Attacker.transform.position.y, target.transform.position.z);
            //Attacker.transform.LookAt(attackerlook);
            //target.transform.GetChild(1).LookAt(Attacker.transform.GetChild(1));
            //Attacker.transform.GetChild(1).LookAt(target.transform.GetChild(1));
            float timer = 0;
            while (Attacker.pathtotake != null && Attacker.pathtotake.Count > 0 && timer < 10f)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            yield return null;
            MakeCharacterLookAtEachOther(Attacker.gameObject, target);

            foreach (ModelInfo modelinfo in Attacker.ModelList)
            {
                if (modelinfo.ID == 1 && modelinfo.active)
                {
                    Attacker.transform.GetChild(1).localRotation = Quaternion.Euler(0, 90, 0);
                }
            }
            combatTextScript.SetupCombat(Attacker, TargetUS);
            ActionsMenu.incombat = true;
            yield return new WaitForSeconds(delaybeforeAttack);


            Character Chartarget = TargetUS.UnitCharacteristics;
            Character Targetcopy = CreateCopy(Chartarget);

            AllStatsSkillBonus AttackerStatBonus = Attacker.GetStatSkillBonus(TargetUS, true);
            AllStatsSkillBonus TargetStatBonus = TargetUS.GetStatSkillBonus(TargetUS, true);

            (UnitScript doubleattacker, bool triple) = ActionsMenu.CalculatedoubleAttack(Attacker, TargetUS, AttackerStatBonus, TargetStatBonus);
            bool ishealing = Attacker.GetFirstWeapon().type.ToLower() == "staff" && (CharAttacker.affiliation == TargetUS.UnitCharacteristics.affiliation || (CharAttacker.affiliation == "playable" && TargetUS.UnitCharacteristics.affiliation == "other" && !TargetUS.UnitCharacteristics.attacksfriends) || (TargetUS.UnitCharacteristics.affiliation == "playable" && CharAttacker.affiliation == "other" && !CharAttacker.attacksfriends));
            if (ishealing)
            {
                foresightScript.CreateAction(1, Attacker, TargetUS);
            }
            else
            {
                foresightScript.CreateAction(0, Attacker, TargetUS);
            }


            if (doubleattacker == Attacker)
            {
                if (triple)
                {
                    Attacker.PlayAttackAnimation(true, true, ishealing);
                }
                else
                {
                    Attacker.PlayAttackAnimation(true, false, ishealing);
                }
            }
            else
            {
                Attacker.PlayAttackAnimation(false, false, ishealing);
            }


            yield return new WaitForSeconds(0.5f);


            //play animation
            safeguard = 0;
            while (checkifattackanimationisplaying(Attacker.gameObject, target) && safeguard < 600)
            {
                safeguard++;
                yield return null;
            }


            // apply damage
            (List<int> attackerhits, int attackerdamage, int attackerexp, List<int> attackerlevelbonus, List<int> attackerdamagelist, List<int> AttackerSkillTrigger, List<int> DefenderSkillTrigger) = ActionsMenu.ApplyDamage(Attacker, TargetUS, false, ishealing);

            combatTextScript.UpdateInfo(attackerdamage, attackerhits, CharAttacker, Chartarget, ishealing);

            bool defenderattacks = ((ActionsMenu.CheckifInRange(Attacker, TargetUS) || TargetUS.GetSkill(38) && Chartarget.statusEffects.StunTurns <= 0) && !ishealing && Chartarget.currentHP > 0);

            List<int> defenderhits = new List<int>();
            int defenderdamage = 0;
            int defenderexp = 0;
            List<int> defenderlevelbonus = new List<int>();
            List<int> defenderdamagelist = new List<int>();
            List<int> AttackerSkillTriggerDuringCounterAttack = new List<int>();
            List<int> DefenderSkilLTriggerDuringCounterAttack = new List<int>();


            if (defenderattacks)
            {

                //play defender animation

                yield return new WaitForSeconds(delaybetweenAttack);


                if (doubleattacker == target)
                {
                    if (triple)
                    {
                        TargetUS.PlayAttackAnimation(true, true);
                    }
                    else
                    {
                        TargetUS.PlayAttackAnimation(true, false);
                    }
                }
                else
                {
                    TargetUS.PlayAttackAnimation(false, false);
                }

                yield return new WaitForSeconds(0.5f);

                safeguard = 0;
                while (checkifattackanimationisplaying(target, Attacker.gameObject) && safeguard < 600)
                {
                    safeguard++;
                    yield return null;
                }

                // deal damage
                (defenderhits, defenderdamage, defenderexp, defenderlevelbonus, defenderdamagelist, AttackerSkillTriggerDuringCounterAttack, DefenderSkilLTriggerDuringCounterAttack) = ActionsMenu.ApplyDamage(Attacker, TargetUS, true, false);
                combatTextScript.UpdateInfo(defenderdamage, defenderhits, Chartarget, CharAttacker, false);

            }
            yield return new WaitForSeconds(delayafterAttack);

            int expreceived = -1;
            List<int> levelupbonuses = new List<int>();
            Character characterwhogainedexp = null;
            // manage exp

            waittingforexp = true;
            if (CharAttacker.affiliation == "playable" && CharAttacker.currentHP > 0)
            {
                expreceived = attackerexp;
                levelupbonuses = attackerlevelbonus;
                characterwhogainedexp = CharAttacker;
            }
            else if (Chartarget.affiliation == "playable" && Chartarget.currentHP > 0)
            {
                expreceived = defenderexp;
                levelupbonuses = defenderlevelbonus;
                characterwhogainedexp = Chartarget;
            }



            // exp distribution
            if (expreceived != -1)
            {
                if (!expbar.gameObject.activeSelf)
                {
                    expbar.gameObject.SetActive(true);
                }

                if (!expbar.setupdone)
                {
                    expbar.SetupBar(characterwhogainedexp, expreceived, levelupbonuses);
                }


                safeguard = 0;
                while (!expdistributed && safeguard < 600)
                {
                    safeguard++;
                    yield return null;
                }
                expbar.gameObject.SetActive(false);
                if (CharAttacker.affiliation == "playable")
                {
                    ActionsMenu.target = Attacker.gameObject;
                    ActionsMenu.FinalizeAttack();
                }

            }
            waittingforexp = false;
            expdistributed = false;
            ActionsMenu.incombat = false;
            EndOfCombatTrigger(Attacker, TargetUS);

            CharAttacker.alreadyplayed = true;
        }
        else
        {
            ActionManager.instance.Wait(Attacker);
        }

        AttackCoroutine = null;
        ActionsMenu.confirmattack = false;
        DeathCleanup();

    }

    private void MakeCharacterLookAtEachOther(GameObject Character1, GameObject Character2)
    {
        Transform model1 = Character1.transform;
        Transform model2 = Character2.transform;
        Vector3 Model2LookPoint = model1.transform.position;
        Model2LookPoint.y = model2.position.y;
        Vector3 Model1LookPoint = model2.transform.position;
        Model1LookPoint.y = model1.position.y;
        model1.LookAt(Model1LookPoint);
        model2.LookAt(Model2LookPoint);
    }

    private bool checkifattackanimationisplaying(GameObject attacker, GameObject target)
    {
        return attacker.GetComponent<UnitScript>().isinattackanimation() || target.GetComponent<UnitScript>().isinattackanimation();
    }
    private void EndOfCombatTrigger(UnitScript unit1, UnitScript unit2 = null)
    {
        if (FreezeFrameGeneral.instance.image.gameObject.activeSelf)
        {
            StartCoroutine(FreezeFrameGeneral.instance.Hide());
        }
        MinimapScript.instance.UpdateMinimap();
        if (unit1 != null)
        {
            unit1.UpdateWeaponModel();
        }

        Character charunit1 = unit1.UnitCharacteristics;


        if (charunit1.affiliation == "playable")
        {
            DataScript.instance.SpreadBonds(unit1);
            unit1.IncreaseExamodeGauge();
        }




        if (unit2 != null)
        {
            Character charunit2 = unit2.UnitCharacteristics;
            if (unit1.GetSkill(32)) // Survivor
            {
                if (charunit1.affiliation == "enemy" || charunit2.affiliation == "enemy" && charunit1.affiliation != charunit2.affiliation)
                {
                    unit1.SurvivorStacks++;
                    unit1.AddNumber(unit1.SurvivorStacks, true, "Survivor");
                }
            }
            if (unit2.GetSkill(32)) // Survivor
            {
                if (charunit1.affiliation == "enemy" || charunit2.affiliation == "enemy" && charunit1.affiliation != charunit2.affiliation)
                {
                    unit2.SurvivorStacks++;
                    unit2.AddNumber(unit2.SurvivorStacks, true, "Survivor");
                }

            }
            if (charunit2.affiliation == "playable")
            {
                DataScript.instance.SpreadBonds(unit2);
                unit1.IncreaseExamodeGauge();
            }

            if (unit1.GetSkill(15)) // Sore Loser
            {
                if (charunit1.currentHP <= 0)
                {
                    unit2.AddNumber(charunit2.currentHP - 1, false, "Sore Loser");
                    charunit2.currentHP = 1;

                }
                else if (charunit1.currentHP < charunit2.currentHP)
                {
                    unit2.AddNumber(charunit2.currentHP - charunit1.currentHP, false, "Sore Loser");
                    charunit2.currentHP = charunit1.currentHP;
                }
            }

            if (unit2.GetSkill(15)) // Sore Loser
            {
                if (charunit2.currentHP <= 0)
                {
                    unit1.AddNumber(charunit1.currentHP - 1, false, "Sore Loser");
                    charunit1.currentHP = 1;

                }
                else if (charunit2.currentHP < charunit1.currentHP)
                {
                    unit1.AddNumber(charunit1.currentHP - charunit2.currentHP, false, "Sore Loser");
                    charunit1.currentHP = charunit2.currentHP;
                }
            }



            TwistOfFateTrigger(unit1, unit2);
            TwistOfFateTrigger(unit2, unit1);

            unit2.UpdateWeaponModel();

        }



    }

    private void TwistOfFateTrigger(UnitScript unit1, UnitScript unit2)
    {
        Character charunit1 = unit1.UnitCharacteristics;
        Character charunit2 = unit2.UnitCharacteristics;
        if (charunit1.currentHP > 0 && charunit2.currentHP > 0)
        {
            if (unit1.GetSkill(112) && charunit1.currentHP < charunit2.currentHP) //Twist of Fate
            {
                int luckstat = (int)charunit1.AjustedStats.Luck + unit1.GetStatSkillBonus(unit2, true).Luck;
                int targetvalue = luckstat + unit1.GetTriggerLuckModificator();
                if (unit1.GetComponent<RandomScript>().GetPersonalityValue(targetvalue) <= targetvalue)
                {
                    int unit1previousHP = charunit1.currentHP;
                    charunit1.currentHP = Mathf.Min(charunit2.currentHP, (int)charunit1.AjustedStats.HP);
                    charunit2.currentHP = Mathf.Min(unit1previousHP, (int)charunit2.AjustedStats.HP); ;
                    unit1.AddNumber(charunit1.currentHP, true, "Twist of Fate");
                    unit2.AddNumber(charunit2.currentHP, false, "Twist of Fate");
                }
            }
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

    private void WeaponDecison(UnitScript unit, bool IsSimulation)
    {
        List<equipment> weaponlist = new List<equipment>();

        int staffID = -1;

        Character unitchar = unit.UnitCharacteristics;

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

                (int range, bool melee) = unit.GetRangeAndMele(weaponlist[staffID]);
                gridScript.ShowAttack(range, melee, true, false, unitchar);
                foreach (GameObject otherunit in gridScript.allunitGOs)
                {
                    if (unit == otherunit)
                    {
                        continue;
                    }

                    if (gridScript.healingtiles.Contains(otherunit.GetComponent<UnitScript>().UnitCharacteristics.currentTile))
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
                    while (unit.GetFirstWeapon().type.ToLower() != "staff" && safeguard < 30)
                    {
                        safeguard++;
                        unit.GetNextWeapon();
                    }
                    return;
                }
                else
                {
                    int safeguard = 0;
                    while (unit.GetFirstWeapon().type.ToLower() == "staff" && safeguard < 30)
                    {
                        safeguard++;
                        unit.GetNextWeapon();
                    }
                    return;
                }

            }
            if (unit.GetComponent<RandomScript>().GetPersonalityValue(20) < 20 && !IsSimulation)
            {
                unit.GetNextWeapon();
            }
        }

    }


    /// <summary>
    /// Calculate attack Destination and target for AI Characters (second version)
    /// </summary>
    /// <param name="currentCharacter"></param>
    /// <returns></returns>
    private (GridSquareScript, GameObject) CalculateDestinationForOffensiveUnitsV2(UnitScript currentCharacter, bool IsSimulation)
    {
        WeaponDecison(currentCharacter, IsSimulation); // Decide which weapon to use before calculating destination and target
        Character character = currentCharacter.UnitCharacteristics;

        List<GridSquareScript> movementtouse = gridScript.movementtiles;
        (int attackerrange, bool attackermelee) = currentCharacter.GetRangeAndMele();



        List<GridSquareScript> newattacktiles = null;

        if (character.enemyStats.personality.ToLower() == "guard") // Guard units only consider attacking from their current position
        {
            movementtouse = new List<GridSquareScript>() { character.currentTile };

            newattacktiles = gridScript.GetAttack(attackerrange, attackermelee, character.currentTile, character);
        }
        else // Other personalities consider attacking from all possible movement positions
        {
            gridScript.ShowAttack(attackerrange, attackermelee, currentCharacter.GetFirstWeapon().type.ToLower() == "staff", false, character);
            newattacktiles = gridScript.attacktiles;
        }

        // If the weapon is a staff, consider healing tiles instead of attack tiles
        if (currentCharacter.GetFirstWeapon().type.ToLower() == "staff")
        {
            newattacktiles = gridScript.healingtiles;
        }

        List<GameObject> potentialtargets = new List<GameObject>();

        foreach (GameObject unit in gridScript.allunitGOs)
        {
            Character otherchar = unit.GetComponent<UnitScript>().UnitCharacteristics;

            bool skip = true;


            if (unit == currentCharacter || !newattacktiles.Contains(otherchar.currentTile) || otherchar.enemyStats.hidden) // Skip self, units not in attack range, and hidden units
            {
                continue;
            }

            // Determine if this unit is a valid target based on affiliations and weapon type
            if (currentCharacter.GetFirstWeapon().type.ToLower() == "staff")
            {
                if (otherchar.currentHP < (int)otherchar.AjustedStats.HP && (character.affiliation.ToLower() == otherchar.affiliation.ToLower() || (character.affiliation.ToLower() == "other" && !character.attacksfriends && otherchar.affiliation.ToLower() == "playable")))
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

            potentialtargets.Add(unit);
        }

        GameObject truetarget = null;

        if (potentialtargets.Count > 0)
        {


            float maxreward = 0;
            if (currentCharacter.GetComponent<UnitScript>().UnitCharacteristics.enemyStats.bossiD > 0)
            {
                maxreward = -10000;
            }

            if ((character.enemyStats.personality.ToLower() == "deviant" || (character.enemyStats.personality.ToLower() == "coward" && character.currentHP > character.AjustedStats.HP * 0.3f)) && !IsSimulation)
            {
                if (potentialtargets.Count > 1)
                {
                    float randomvalue = (float)currentCharacter.GetComponent<RandomScript>().GetPersonalityValue(0) / 100f;

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
                    float reward = calculateRewardforAttacking(currentCharacter, target.GetComponent<UnitScript>());
                    if (reward > maxreward && reward > 0)
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

            UnitScript TrueTargetUS = truetarget.GetComponent<UnitScript>();

            GridSquareScript targepositiontiles = TrueTargetUS.UnitCharacteristics.currentTile;


            List<GridSquareScript> potentialmovementtile = new List<GridSquareScript>();

            for (int i = -attackerrange; i <= attackerrange; i++)
            {
                for (int j = -attackerrange; j <= attackerrange; j++)
                {
                    if (Mathf.Abs(i) + Mathf.Abs(j) <= attackerrange && (i != 0 || j != 0))
                    {
                        GridSquareScript tile = gridScript.GetTile(targepositiontiles.GridCoordinates + new Vector2Int(i, j));
                        if (movementtouse.Contains(tile))
                        {
                            potentialmovementtile.Add(tile);
                        }

                    }
                }
            }

            // These tiles are the tiles in which the character can attack the target withouth being attacked

            List<GridSquareScript> potentialmovementtilesecondFilter = new List<GridSquareScript>();

            Vector2 baseposition = character.currentTile.GridCoordinates;


            foreach (GridSquareScript tile in potentialmovementtile)
            {


                if (ActionsMenu.CheckifInRange(TrueTargetUS, currentCharacter, TrueTargetUS.UnitCharacteristics.currentTile, tile) && !ActionsMenu.CheckifInRange(currentCharacter, TrueTargetUS, tile))
                {
                    potentialmovementtilesecondFilter.Add(tile);
                }
            }

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
                    int distance = ManhattanDistance(tile.GridCoordinates, character.currentTile.GridCoordinates);
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
                    int distance = ManhattanDistance(tile.GridCoordinates, character.currentTile.GridCoordinates);
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

            int personalityvalue = currentCharacter.GetComponent<RandomScript>().GetPersonalityValue(0);



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

    private GridSquareScript CalculateDestinationIfTargetNotFound(UnitScript unitScript)
    {
        GameObject currentCharacter = unitScript.gameObject;
        Character character = unitScript.UnitCharacteristics;

        // Guard units don't move
        if (character.enemyStats.personality.ToLower() == "guard")
        {
            return character.currentTile;
        }

        List<GridSquareScript> movementTiles = gridScript.movementtiles;

        GridSquareScript bestSquare = null;
        int bestDistance = int.MaxValue;

        foreach (GridSquareScript square in movementTiles)
        {
            // Skip current tile or occupied tiles
            if (square == character.currentTile || !gridScript.CheckIfFree(square.GridCoordinates, character))
            {
                continue;
            }


            foreach (GameObject unit in gridScript.allunitGOs)
            {
                if (unit == currentCharacter || unit.GetComponent<UnitScript>().UnitCharacteristics.enemyStats.hidden)
                {
                    continue;
                }


                UnitScript otherUnitScript = unit.GetComponent<UnitScript>();
                Character otherChar = otherUnitScript.UnitCharacteristics;

                // Check if this unit is a valid target
                if (!IsValidTarget(unitScript, character, otherChar))
                {
                    continue;
                }


                // Distance from this tile to the target
                int distance = ManhattanDistance(otherChar.currentTile.GridCoordinates, square.GridCoordinates);

                // Prefer tiles that get closer to targets
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestSquare = square;
                }
            }
        }

        return bestSquare;
    }

    private bool IsValidTarget(UnitScript currentUnit, Character self, Character other)
    {
        string selfAff = self.affiliation.ToLower();
        string otherAff = other.affiliation.ToLower();
        string weaponType = currentUnit.GetFirstWeapon().type.ToLower();

        // Staff users (healers / support)
        if (weaponType == "staff")
        {
            if (selfAff == otherAff)
            {
                return true;
            }


            if (selfAff == "other" && !self.attacksfriends)
            {
                return true;
            }


            return false;
        }

        // Offensive units
        if (selfAff == "other")
        {
            if ((otherAff == "playable" && self.attacksfriends) || otherAff == "enemy")
            {
                return true;
            }

        }

        if (selfAff == "enemy")
        {
            if ((otherAff == "other" && !other.attacksfriends) || otherAff == "playable")
            {
                return true;
            }
        }

        return false;
    }


    /// <summary>
    /// Calculate attack Destination and target for AI Characters (second version)
    /// </summary>
    /// <param name="currentCharacter"></param>
    /// <returns></returns>
    private GameObject CalculateDestinationForBoss(UnitScript currentCharacter)
    {
        WeaponDecison(currentCharacter, false);
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

        if (MapInitializer.instance.ChapterID == 16 && !MapInitializer.instance.IsSideStory) // The boss in this chapter can only target Bob and Gale
        {

            GameObject BobGO = gridScript.GetUnit("Bob");
            GameObject GaleGO = gridScript.GetUnit("Gale");
            potentialtargets = new List<GameObject>();
            if (BobGO != null)
            {
                potentialtargets.Add(BobGO);
            }
            if (GaleGO != null)
            {
                potentialtargets.Add(GaleGO);
            }
        }

        GameObject truetarget = null;

        float maxreward = 0;

        foreach (GameObject target in potentialtargets)
        {
            float reward = calculateRewardforAttacking(currentCharacter, target.GetComponent<UnitScript>(), true);
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
    private float calculateRewardforAttacking(UnitScript attacker, UnitScript target, bool isboss = false)
    {
        float reward = 10f;

        Character attackerChar = attacker.UnitCharacteristics;
        Character targetChar = target.UnitCharacteristics;

        GridSquareScript tiletouse = GetClosestTileToattack(attacker, target, gridScript.movementtiles);

        if (targetChar.TauntTurns > 0)
        {
            reward += 999f;
        }

        if (attacker.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "staff")
        {
            reward += 10f * (targetChar.AjustedStats.HP - targetChar.currentHP) / targetChar.AjustedStats.HP;
        }
        else
        {

            float DamageFactor = 2f;
            float DamageTakenFactor = 1f;
            float killFactor = 5f;
            float NoCounterFactor = 5f;
            float SurvivesFactor = 2f;
            float enemyoutofrangebonus = 7f;

            if (isboss)
            {
                NoCounterFactor = 0f;

                reward += 50 - ManhattanDistance(attackerChar, targetChar);
            }

            if (attackerChar.enemyStats.personality.ToLower() == "survivor" || (attackerChar.enemyStats.personality.ToLower() == "coward" && attackerChar.currentHP < attackerChar.AjustedStats.HP * 0.1f))
            {

                SurvivesFactor *= 3f;
                NoCounterFactor *= 2;
            }

            if (attackerChar.enemyStats.personality.ToLower() == "daredevil")
            {
                SurvivesFactor = 0f;
                NoCounterFactor = 0f;
            }

            AllStatsSkillBonus AttackerStatBonus = attacker.GetComponent<UnitScript>().GetStatSkillBonus(target, false);
            AllStatsSkillBonus TargetStatBonus = target.GetComponent<UnitScript>().GetStatSkillBonus(attacker, false);

            int rawdamage = ActionsMenu.CalculateDamage(attacker, AttackerStatBonus, target, TargetStatBonus);
            int rawdamagetaken = ActionsMenu.CalculateDamage(target, TargetStatBonus, attacker, AttackerStatBonus);
            int hitrate = ActionsMenu.CalculateHit(attacker, target, AttackerStatBonus, TargetStatBonus);
            int EnemyHitrate = ActionsMenu.CalculateHit(target, attacker, TargetStatBonus, AttackerStatBonus);

            bool inrange = ActionsMenu.CheckifInRange(attacker, target, tiletouse);

            if (!inrange)
            {
                rawdamagetaken = 0;
                reward += enemyoutofrangebonus;
            }


            float ratioofhptaken = Mathf.Min(1f, (float)rawdamage / (float)targetChar.AjustedStats.HP);

            reward += killFactor * ratioofhptaken;

            if (rawdamage >= targetChar.currentHP)
            {
                reward += killFactor * (float)hitrate / 100f;
            }

            if (rawdamagetaken == 0)
            {
                reward += NoCounterFactor;
            }
            float ratioofhptakenbyattacker = Mathf.Min(1f, (float)rawdamagetaken / (float)attackerChar.AjustedStats.HP);

            reward += DamageFactor * ratioofhptaken * (float)hitrate / 100f;
            reward -= DamageTakenFactor * ratioofhptakenbyattacker * (float)(EnemyHitrate) / 100f;

            if (attackerChar.currentHP > rawdamagetaken)
            {
                float remainingHPRatio = Mathf.Max(0f, 1f - (float)rawdamagetaken / attackerChar.currentHP);

                reward += SurvivesFactor * remainingHPRatio;
            }



        }



        return reward;
    }


    private GridSquareScript GetClosestTileToattack(UnitScript Unit, UnitScript Target, List<GridSquareScript> movementtilestouse)
    {
        Character UnitChar = Unit.UnitCharacteristics;
        Character TargetChar = Target.UnitCharacteristics;
        GridSquareScript targettile = UnitChar.currentTile;
        (int range, bool melee) = Unit.GetRangeAndMele();

        int maxdist = 999;
        foreach (GridSquareScript tile in movementtilestouse)
        {
            bool keeptile = false;
            if (tile == TargetChar.currentTile)
            {
                continue;
            }

            if (Vector2.Distance(tile.GridCoordinates, TargetChar.currentTile.GridCoordinates) == 1)
            {
                if (melee)
                {
                    keeptile = true;
                }
            }
            else if (Vector2.Distance(tile.GridCoordinates, TargetChar.currentTile.GridCoordinates) <= range)
            {
                keeptile = true;
            }


            if (keeptile && (int)Vector2.Distance(tile.GridCoordinates, TargetChar.currentTile.GridCoordinates) < maxdist)
            {
                targettile = tile;
                maxdist = (int)Vector2.Distance(tile.GridCoordinates, TargetChar.currentTile.GridCoordinates);
            }
        }

        return targettile;
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
                    if (DataScript.instance != null && charunit.affiliation == "playable" && DataScript.instance.Permadeath)
                    {
                        charunit.PermaDied = true;
                    }
                    charunit.experience = 0;
                    if (charunit.enemyStats == null)
                    {
                        if (charunit.currentTile != null)
                        {
                            charunit.currentTile.UpdateInsideSprite(false);

                        }
                        unit.SetActive(false);
                        objecttodelete.Add(unit);
                    }
                    else if (charunit.enemyStats.RemainingLifebars > 0)
                    {
                        charunit.enemyStats.RemainingLifebars--;
                        charunit.currentHP = (int)charunit.AjustedStats.HP;
                        unit.GetComponent<UnitScript>().AddNumber((int)charunit.AjustedStats.HP, true, (charunit.enemyStats.RemainingLifebars + 1) * charunit.AjustedStats.HP + " HP remaining.");
                    }
                    else
                    {
                        if (charunit.currentTile != null)
                        {
                            charunit.currentTile.UpdateInsideSprite(false);
                        }
                        unit.SetActive(false);
                        objecttodelete.Add(unit);
                    }


                }
            }

        }

        bool dialoguegenerated = false;

        foreach (GameObject unittodelete in objecttodelete)
        {
            gridScript.allunitGOs.Remove(unittodelete);
            gridScript.allunits.Remove(unittodelete.GetComponent<UnitScript>().UnitCharacteristics);
            if (unittodelete.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable")
            {
                dialoguegenerated = true;
                GenerateDeathDialogue(unittodelete.GetComponent<UnitScript>().UnitCharacteristics);

            }

        }
        GetComponent<TurnManger>().InitializeUnitLists(GetComponent<GridScript>().allunitGOs);
        if (!dialoguegenerated && !SaveManager.instance.Options.BattleAnimations)
        {
            MapEventManager.instance.TriggerEventCheck();
        }

        if (BattleInfotext.instance != null)
        {
            BattleInfotext.instance.ResetPreviousSelectedGO();
        }

        minimapScript.UpdateMinimap();
        GridScript.instance.CalculateDangerousTiles();
    }

    private void GenerateDeathDialogue(Character charawhodied)
    {
        List<TextBubbleInfo> DeathDialogue = new List<TextBubbleInfo>();

        TextBubbleInfo characterdeathquote = new TextBubbleInfo();
        characterdeathquote.text = charawhodied.characterDialogues.DeathQuote;
        characterdeathquote.characterindex = charawhodied.ID;

        DeathDialogue.Add(characterdeathquote);

        if (!charawhodied.playableStats.protagonist)
        {
            TextBubbleInfo Leaderquote = new TextBubbleInfo();

            switch (charawhodied.playableStats.battalion.ToLower())
            {
                case ("zack"):
                    Leaderquote.text = charawhodied.characterDialogues.DeathReaction_Zack;
                    Leaderquote.characterindex = 0;
                    DeathDialogue.Add(Leaderquote);
                    break;
                case ("kira"):
                    Leaderquote.text = charawhodied.characterDialogues.DeathReaction_Kira;
                    Leaderquote.characterindex = 9;
                    DeathDialogue.Add(Leaderquote);
                    break;
                case ("gale"):
                    Leaderquote.text = charawhodied.characterDialogues.DeathReaction_Gale;
                    Leaderquote.characterindex = 13;
                    DeathDialogue.Add(Leaderquote);
                    break;
            }
        }



        _TextBubbleScript.InitializeDialogue(DeathDialogue);

    }

}