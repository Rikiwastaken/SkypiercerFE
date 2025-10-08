using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static UnitScript;

public class TurnManger : MonoBehaviour
{
    public int currentTurn;

    public string currentlyplaying; // playable, enemy, other

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

    private PreBattleMenuScript preBattleMenuScript;

    private WeatherManager weatherManager;
    private ActionManager actionManager;
    private MapEventManager mapEventManager;

    public PhaseTextScript phaseTextScript;

    private MinimapScript minimapScript;
    private void Start()
    {
        weatherManager = GetComponent<WeatherManager>();
        mapEventManager = GetComponent<MapEventManager>();
        minimapScript = FindAnyObjectByType<MinimapScript>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (InputManager == null)
        {
            InputManager = FindAnyObjectByType<InputManager>();
        }

        if (preBattleMenuScript == null)
        {
            preBattleMenuScript = FindAnyObjectByType<PreBattleMenuScript>();
        }

        if (GridScript == null)
        {
            GridScript = FindAnyObjectByType<GridScript>();
        }

        if (actionManager == null)
        {
            actionManager = FindAnyObjectByType<ActionManager>();
        }

        if (InputManager.Startpressed && waittingforstart && !preBattleMenuScript.gameObject.activeSelf)
        {
            waittingforstart = false;
        }



        if (updatevisuals)
        {
            UpdateVisuals();
            updatevisuals = false;
        }

        //if (playableunit.Count == 0)
        //{
        //    GameOverScript.gameObject.SetActive(true);
        //    return;
        //}
        //if (enemyunit.Count == 0)
        //{
        //    GameOverScript.victory = true;
        //    GameOverScript.gameObject.SetActive(true);
        //    return;
        //}
        UpdateText();
        if (!waittingforstart)
        {
            if (currentTurn == 0)
            {
                currentTurn = 1;
            }
            if (currentlyplaying == "")
            {
                actionManager.frameswherenotlock = 5;
                currentlyplaying = "playable";
                phaseTextScript.SetupText(currentlyplaying);
                BeginningOfTurnsTrigger(playableunitGO);
                weatherManager.UpdateRain();
                mapEventManager.EventInitialization();
                mapEventManager.TriggerEventCheck();
            }
            ManageTurnRotation();
        }
        InitializeUnitLists(GridScript.allunitGOs);
    }

    private void BeginningOfTurnsTrigger(List<GameObject> charactertoappy)
    {
        foreach (GameObject unit in charactertoappy)
        {
            unit.GetComponent<UnitScript>().waittedbonusturns--;
            Character unitchar = unit.GetComponent<UnitScript>().UnitCharacteristics;

            //Kira Battalion Side Effect
            if (unitchar.playableStats.battalion == "Kira")
            {
                unit.GetComponent<UnitScript>().AddNumber(Mathf.Min((int)(unitchar.AjustedStats.HP * 0.1f), (int)unitchar.AjustedStats.HP - unitchar.currentHP), true, "Kira Battalion");
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
                    if (Mathf.Abs(otherunitchar.position.x - unitchar.position.x) == 1 || Mathf.Abs(otherunitchar.position.y - unitchar.position.y) == 1)
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
        minimapScript.UpdateMinimap();
    }
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
                if (FindAnyObjectByType<ActionManager>())
                {
                    FindAnyObjectByType<ActionManager>().preventfromlockingafteraction = true;
                    FindAnyObjectByType<GridScript>().ResetAllSelections();
                }
                BeginningOfTurnsTrigger(playableunitGO);
                currentlyplaying = "playable";
                phaseTextScript.SetupText(currentlyplaying);
                currentTurn++;
                updatevisuals = true;
                return;
            }
        }
    }

    private void UpdateVisuals()
    {
        if (GridScript == null)
        {
            GridScript = FindAnyObjectByType<GridScript>();
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
            turntext.text = "Turn : Allies \nRemaining : " + numberremaining;
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
            turntext.text = "Turn : Enemies \nRemaining : " + numberremaining;
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
            turntext.text = "Turn : Others \nRemaining : " + numberremaining;
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
