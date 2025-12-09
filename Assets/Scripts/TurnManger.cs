using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static UnitScript;
using static UnityEngine.UI.CanvasScaler;

public class TurnManger : MonoBehaviour
{

    public static TurnManger instance;

    public int currentTurn;

    public string currentlyplaying = ""; // playable, enemy, other

    public List<Character> playableunit;
    public List<GameObject> playableunitGO;

    public List<Character> enemyunit;
    public List<GameObject> enemyunitGO;

    public List<Character> otherunits;
    public List<GameObject> otherunitsGO;

    public TextMeshProUGUI turntext;

    public GameOverScript GameOverScript;

    private GridScript GridScript;

    private bool updatevisuals;

    public bool waittingforstart;

    private InputManager InputManager;

    public PreBattleMenuScript preBattleMenuScript;

    private WeatherManager weatherManager;

    public PhaseTextScript phaseTextScript;

    private MinimapScript minimapScript;

    public TextBubbleScript textBubbleScript;
    public GameObject TutorialWindows;

    public bool skipothersTurn;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    private void Start()
    {
        weatherManager = GetComponent<WeatherManager>();
        minimapScript = FindAnyObjectByType<MinimapScript>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        InputManager = InputManager.instance;

        if (GetComponent<AttackTurnScript>().attackanimationhappeningcnt > 0)
        {
            return;
        }

        if (GridScript == null)
        {
            GridScript = GridScript.instance;
        }

        if (InputManager.Startpressed && waittingforstart && !preBattleMenuScript.gameObject.activeSelf && !textBubbleScript.indialogue && !TutorialWindows.activeSelf)
        {
            waittingforstart = false;
        }


        if (!textBubbleScript.indialogue && !preBattleMenuScript.gameObject.activeSelf && currentlyplaying == "" && !TutorialWindows.activeSelf)
        {
            preBattleMenuScript.gameObject.SetActive(true);
        }

        if (updatevisuals)
        {
            UpdateVisuals();
            updatevisuals = false;
        }

        UpdateText();
        if (!waittingforstart)
        {
            if (currentTurn == 0)
            {
                currentTurn = 1;
            }
            if (currentlyplaying == "")
            {
                ActionManager.instance.frameswherenotlock = 5;
                currentlyplaying = "playable";
                phaseTextScript.SetupText(currentlyplaying);
                BeginningOfTurnsTrigger(playableunitGO);
                weatherManager.UpdateRain();
                MapEventManager.instance.EventInitialization();
                MapEventManager.instance.TriggerEventCheck();
            }
            ManageTurnRotation();
        }
        InitializeUnitLists(GridScript.allunitGOs);
    }

    /// <summary>
    /// Skills, blade durability recovery and Protagonist effects to trigger when a new turn starts
    /// </summary>
    /// <param name="charactertoappy"></param>
    private void BeginningOfTurnsTrigger(List<GameObject> charactertoappy)
    {

        ForesightScript FSS = GetComponent<GridScript>().ForesightMenu.GetComponent<ForesightScript>();

        if (charactertoappy == playableunitGO)
        {
            FSS.CreateAction(4, 0, -1);
        }
        else if (charactertoappy == enemyunitGO)
        {
            FSS.CreateAction(4, 1, -1);
        }
        else if (charactertoappy == otherunitsGO)
        {
            FSS.CreateAction(4, 2, -1);
        }

        foreach (GameObject unit in charactertoappy)
        {



            unit.GetComponent<UnitScript>().waittedbonusturns--;
            Character unitchar = unit.GetComponent<UnitScript>().UnitCharacteristics;



            //Kira Battalion Side Effect
            if (unitchar.playableStats.battalion.ToLower() == "kira")
            {

                unit.GetComponent<UnitScript>().AddNumber(Mathf.Min((int)(unitchar.AjustedStats.HP * 0.1f), (int)unitchar.AjustedStats.HP - unitchar.currentHP), true, "Kira's Battalion");
                unitchar.currentHP += (int)(unitchar.AjustedStats.HP * 0.1f);
                //Loyal
                if (unit.GetComponent<UnitScript>().GetSkill(35))
                {
                    unit.GetComponent<UnitScript>().AddNumber(Mathf.Max((int)(unitchar.AjustedStats.HP * 0.1f), (int)unitchar.AjustedStats.HP - unitchar.currentHP), true, "Loyal");
                    unitchar.currentHP += (int)(unitchar.AjustedStats.HP * 0.1f);
                }
                if (unitchar.currentHP > unitchar.AjustedStats.HP)
                {
                    unitchar.currentHP = (int)unitchar.AjustedStats.HP;
                }
            }


            // Restoring unequiped blade durability

            foreach (equipment weapon in unitchar.equipments)
            {
                if (weapon != unit.GetComponent<UnitScript>().GetFirstWeapon() && weapon.Maxuses != 0)
                {
                    weapon.Currentuses++;
                    if (unit.GetComponent<UnitScript>().GetSkill(7)) // full of beans
                    {
                        weapon.Currentuses++;
                    }
                    if (weapon.Currentuses > weapon.Maxuses)
                    {
                        weapon.Currentuses = weapon.Maxuses;
                    }
                }
                unit.GetComponent<UnitScript>().UpdateWeaponModel();
            }

            //First aid
            if (unit.GetComponent<UnitScript>().GetSkill(9))
            {
                unit.GetComponent<UnitScript>().AddNumber(Mathf.Min((int)(unitchar.AjustedStats.HP * 0.1f), (int)unitchar.AjustedStats.HP - unitchar.currentHP), true, "First Aid");
                unitchar.currentHP += (int)(unitchar.AjustedStats.HP * 0.1f);
                if (unitchar.currentHP > unitchar.AjustedStats.HP)
                {
                    unitchar.currentHP = (int)unitchar.AjustedStats.HP;
                }
            }
            //Medic
            if (unit.GetComponent<UnitScript>().GetSkill(12))
            {
                foreach (GameObject otherunit in charactertoappy)
                {
                    Character otherunitchar = otherunit.GetComponent<UnitScript>().UnitCharacteristics;
                    if (Mathf.Abs(otherunitchar.position.x - unitchar.position.x) <= 1 && Mathf.Abs(otherunitchar.position.y - unitchar.position.y) <= 1)
                    {
                        otherunit.GetComponent<UnitScript>().AddNumber(Mathf.Min((int)(otherunitchar.AjustedStats.HP * 0.1f), (int)otherunitchar.AjustedStats.HP - otherunitchar.currentHP), true, "Medic");
                        otherunitchar.currentHP += (int)(otherunitchar.AjustedStats.HP * 0.1f);
                        if (otherunitchar.currentHP > otherunitchar.AjustedStats.HP)
                        {
                            otherunitchar.currentHP = (int)otherunitchar.AjustedStats.HP;
                        }
                    }
                }
            }

            // Machine Sun HP Regen
            if (unitchar.enemyStats.monsterStats.ismachine && unit.GetComponent<UnitScript>().GetWeatherType() == "sun")
            {
                unit.GetComponent<UnitScript>().AddNumber(Mathf.Min((int)(unitchar.AjustedStats.HP * 0.1f), (int)unitchar.AjustedStats.HP - unitchar.currentHP), true, "Solar Power");
                unitchar.currentHP += (int)(unitchar.AjustedStats.HP * 0.1f);
                if (unitchar.currentHP > unitchar.AjustedStats.HP)
                {
                    unitchar.currentHP = (int)unitchar.AjustedStats.HP;
                }
            }

            //Reset Verso movements
            unit.GetComponent<UnitScript>().tilesmoved = 0;
        }


        if (charactertoappy == playableunitGO) // true change of turns;
        {
            MapEventManager.instance.TriggerEventCheck(currentTurn);
            //reset mechanisms
            foreach (List<GameObject> list in GridScript.instance.Grid)
            {
                foreach (GameObject tile in list)
                {
                    tile.GetComponent<GridSquareScript>().ReinitializeMechanismIfPairednotactive();
                }
            }

            // Tile Effects
            TileEffects(playableunitGO);
            TileEffects(enemyunitGO);
            TileEffects(otherunitsGO);
        }

        minimapScript.UpdateMinimap();
    }

    /// <summary>
    /// Manage players, enemy and other turn rotation
    /// </summary>
    private void ManageTurnRotation()
    {
        if (phaseTextScript.moveText)
        {
            return;
        }
        if (currentlyplaying == "playable")
        {
            bool alldone = true;
            foreach (Character character in playableunit)
            {
                if (!character.alreadyplayed || !character.alreadymoved)
                {
                    alldone = false;
                }
            }
            if (alldone)
            {
                foreach (Character character in enemyunit)
                {
                    character.alreadyplayed = false;
                    character.alreadymoved = false;
                }
                currentlyplaying = "enemy";
                if (enemyunit.Count > 0)
                {
                    phaseTextScript.SetupText(currentlyplaying);
                }
                BeginningOfTurnsTrigger(enemyunitGO);
                updatevisuals = true;
                return;
            }
        }
        if (currentlyplaying == "enemy")
        {
            bool alldone = true;
            foreach (Character character in enemyunit)
            {
                if (!character.alreadyplayed || !character.alreadymoved)
                {
                    alldone = false;
                }
            }
            if (alldone)
            {
                if (skipothersTurn)
                {
                    foreach (Character character in playableunit)
                    {
                        character.alreadyplayed = false;
                        character.alreadymoved = false;
                    }
                    ActionManager.instance.preventfromlockingafteraction = true;
                    GridScript.instance.ResetAllSelections();
                    currentlyplaying = "playable";
                    phaseTextScript.SetupText(currentlyplaying);
                    currentTurn++;
                    BeginningOfTurnsTrigger(playableunitGO);
                }
                else
                {
                    foreach (Character character in otherunits)
                    {
                        character.alreadyplayed = false;
                        character.alreadymoved = false;
                    }
                    BeginningOfTurnsTrigger(otherunitsGO);
                    currentlyplaying = "other";
                    if (otherunits.Count > 0)
                    {
                        phaseTextScript.SetupText(currentlyplaying);
                    }
                }

                updatevisuals = true;
                return;
            }
        }
        if (currentlyplaying == "other")
        {
            bool alldone = true;
            foreach (Character character in otherunits)
            {
                if (!character.alreadyplayed || !character.alreadymoved)
                {
                    alldone = false;
                }
            }
            if (alldone)
            {
                foreach (Character character in playableunit)
                {
                    character.alreadyplayed = false;
                    character.alreadymoved = false;
                }
                ActionManager.instance.preventfromlockingafteraction = true;
                GridScript.instance.ResetAllSelections();
                currentlyplaying = "playable";
                phaseTextScript.SetupText(currentlyplaying);
                currentTurn++;
                BeginningOfTurnsTrigger(playableunitGO);
                updatevisuals = true;
                return;
            }
        }
    }

    private void TileEffects(List<GameObject> grouptoapplyto)
    {
        foreach (GameObject unit in grouptoapplyto)
        {
            Character unitchar = unit.GetComponent<UnitScript>().UnitCharacteristics;
            foreach (GridSquareScript tile in unitchar.currentTile)
            {
                switch (tile.type.ToLower())
                {
                    case "fire":
                        int hplost = unitchar.currentHP - (int)Mathf.Max(1, unitchar.currentHP - unitchar.AjustedStats.HP / 3);
                        unitchar.currentHP = (int)Mathf.Max(1, unitchar.currentHP - unitchar.AjustedStats.HP / 3);
                        unit.GetComponent<UnitScript>().AddNumber(hplost, false, "Fire");
                        break;
                    case "desert":
                        int hplost = unitchar.currentHP - (int)Mathf.Max(1, unitchar.currentHP - unitchar.AjustedStats.HP / 10);
                        unitchar.currentHP = (int)Mathf.Max(1, unitchar.currentHP - unitchar.AjustedStats.HP / 10);
                        unit.GetComponent<UnitScript>().AddNumber(hplost, false, "Desert");
                        break;
                    case "fortification":
                        int hpgained = unitchar.currentHP - (int)Mathf.Min(unitchar.AjustedStats.HP, unitchar.currentHP + unitchar.AjustedStats.HP / 10);
                        unitchar.currentHP = (int)Mathf.Min(unitchar.AjustedStats.HP, unitchar.currentHP + unitchar.AjustedStats.HP / 10);
                        unit.GetComponent<UnitScript>().AddNumber(hpgained, true, "Fortification");
                        break;
                    case "medicinalwater":
                        int hpgained = unitchar.currentHP - (int)Mathf.Min(unitchar.AjustedStats.HP, unitchar.currentHP + unitchar.AjustedStats.HP / 2);
                        unitchar.currentHP = (int)Mathf.Min(unitchar.AjustedStats.HP, unitchar.currentHP + unitchar.AjustedStats.HP / 2);
                        unit.GetComponent<UnitScript>().AddNumber(hpgained, true, "Medicinal Water");
                        break;
                }
            }
        }
    }

    private void UpdateVisuals()
    {
        if (GridScript == null)
        {
            GridScript = GridScript.instance;
        }

        GridScript.ResetAllSelections();

        if (GridScript.selection != null)
        {
            GridScript.ShowMovement();
        }
    }
    private void UpdateText()
    {
        if (currentlyplaying == "playable")
        {
            int numberremaining = 0;
            foreach (Character character in playableunit)
            {
                if (!character.alreadyplayed || !character.alreadymoved)
                {
                    numberremaining++;
                }
            }
            turntext.text = "Turn " + currentTurn + " : Allies \nRemaining : " + numberremaining;
        }
        else if (currentlyplaying == "enemy")
        {
            int numberremaining = 0;
            foreach (Character character in enemyunit)
            {
                if (!character.alreadyplayed || !character.alreadymoved)
                {
                    numberremaining++;
                }
            }
            turntext.text = "Turn " + currentTurn + " : Enemies \nRemaining : " + numberremaining;
        }
        else if (currentlyplaying == "other")
        {
            int numberremaining = 0;
            foreach (Character character in otherunits)
            {
                if (!character.alreadyplayed || !character.alreadymoved)
                {
                    numberremaining++;
                }
            }
            turntext.text = "Turn " + currentTurn + " : Others \nRemaining : " + numberremaining;
        }
        else
        {
            turntext.text = "The battle is about to begin...";
        }
    }

    public void InitializeUnitLists(List<GameObject> allunits)
    {
        playableunit = new List<Character>();
        enemyunit = new List<Character>();
        otherunits = new List<Character>();
        playableunitGO = new List<GameObject>();
        enemyunitGO = new List<GameObject>();
        otherunitsGO = new List<GameObject>();
        foreach (GameObject character in allunits)
        {
            if (character == null)
            {
                continue;
            }
            if (character.GetComponent<UnitScript>().UnitCharacteristics.currentTile == null)
            {
                continue;
            }
            if (character.name == "" || !character.GetComponent<UnitScript>().CheckIfOnActivated())
            {
                continue;
            }
            if (character.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable")
            {
                playableunitGO.Add(character);
                playableunit.Add(character.GetComponent<UnitScript>().UnitCharacteristics);
            }
            else if (character.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "enemy")
            {
                enemyunitGO.Add(character);
                enemyunit.Add(character.GetComponent<UnitScript>().UnitCharacteristics);
            }
            else
            {
                otherunitsGO.Add(character);
                otherunits.Add(character.GetComponent<UnitScript>().UnitCharacteristics);
            }

        }
    }
}
