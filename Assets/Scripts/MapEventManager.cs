using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static TextBubbleScript;
using static UnitScript;



public class MapEventManager : MonoBehaviour
{

    public static MapEventManager instance;

    [System.Serializable]
    private class Wrapper<T>
    {
        public T Data;
        public Wrapper(T data) => Data = data;
    }

    [Serializable]
    public class EventCondition
    {
        public string eventname;
        public int ID;
        public List<Character> UnitList;
        public List<string> NameUnitList;
        public List<int> EventsToWatch;
        public List<GridSquareScript> TilesList;
        public List<SmallerCondition> SmallerConditions;
        public int triggertype;
        /* 1 : one of the allies reached one of the tiles
         * 2 : one of the enemies reached one of the tiles
         * 3 : one of the others reached one of the tiles
         * 4 : one of the units died
         * 5 : all of the units died
         * 6 : small conditions are verified
         * 7 : Battle Starts
         * 8 : EventID Given are all triggered
         * 9 : TileList Mechanisms are all activated
         * 10 : Beginning of turns listed
         * 11 : Characters Talked To
         */
        public int initializationtype;
        /* 1 : Get Units From Names
         * 2 : Get Playable Units
         * 3 : Get Enemy Units
         * 4 : Get Other Units
         * 5 : Get Finish Tiles
         * 6 : 
         */
        public bool triggered;
        public int triggerEffectType;
        /* 1 : Win the game
         * 2 : Lose the game
         * 3 : ModifyTiles
         * 4 : ShowDialogue
         * 5 : Show Tutorial Window
         * 6 : Spawn Units
         * 7 : Change Units From Enemy To Other
         */
        public List<TextBubbleInfo> dialoguetoShow;
        public List<int> UnitsToUnlockID;
        public List<int> UnitsToLockID;
        public List<SkillsToAdd> skillsToAdd;
        public List<int> turnswheretotrigger;
        public TileModification tileModification;
        public UnitPlacement UnitPlacement;
        public TutorialWindow TutorialWindow;
        public List<EnemyStats> CharactersToSpawn;
    }

    [Serializable]
    public class SkillsToAdd
    {
        public int SkillID; //ID of skill to add
        public int SkillQuantity; //Quantity of skill to add
    }

    [Serializable]
    public class SmallerCondition
    {
        public string Unit1; //names of the unit
        public string Unit2; //names of the unit
        public int triggertype;
        /* 1 : Unit1 is Deployed
         * 2 : Unit1 is next to Unit2
         * 3 : Unit1 is alive
         * 4 : Unit1 is not Deployed
         * 5 : 
         * 6 : 
         */
    }

    [Serializable]
    public class UnitPlacement
    {
        public List<string> UnitToPlaceManually;
        public List<Vector2> WhereToManuallyPlaceUnits;
        public List<Vector2> RemainingSpots;
        public Vector2 CameraPosition;
    }

    [Serializable]
    public class TileModification
    {
        public List<Vector2Int> TilesIdCouples;
        public List<GridSquareScript> tilestomodify;
        public string newtype;
        public int modiftype;
        /* 1 : activate
         * 2 : remove wall
         * 3 : become wall
         * 4 : change tile type
         * 5 : 
         * 6 : 
         */



    }

    [Serializable]
    public class TutorialWindow
    {
        public Vector2Int WindowDimensions;
        public string text;
    }

    public List<EventCondition> EventsToMonitor;



    private GridScript GridScript;
    private TurnManger turnManger;

    public GameOverScript GameOverScript;
    private TextBubbleScript TextBubbleScript;

    private bool eventinitialized;

    public int ManualEventTrigger = -1;

    private GameObject grid;

    public TutorialWindowScript TutorialwindowScript;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GridScript = GetComponent<GridScript>();
        grid = GameObject.Find("Grid");
        turnManger = GetComponent<TurnManger>();
        TextBubbleScript = FindAnyObjectByType<TextBubbleScript>(FindObjectsInactive.Include);
    }

    private void FixedUpdate()
    {
        if ((GetComponent<TurnManger>().currentlyplaying == "playable" || GetComponent<TurnManger>().currentlyplaying == "other" || GetComponent<TurnManger>().currentlyplaying == "enemy") && !eventinitialized)
        {
            eventinitialized = true;
            EventInitialization();
        }
        if (ManualEventTrigger >= 0)
        {
            foreach (EventCondition evnt in EventsToMonitor)
            {
                if (evnt.ID == ManualEventTrigger && !evnt.triggered)
                {
                    TriggerEvent(evnt, -1);
                    evnt.triggered = true;
                }
            }
        }
    }

    public List<EventCondition> CloneEvents()
    {
        if (EventsToMonitor == null)
            return null;

        string json = JsonUtility.ToJson(new Wrapper<List<EventCondition>>(EventsToMonitor));
        Wrapper<List<EventCondition>> cloneWrapper = JsonUtility.FromJson<Wrapper<List<EventCondition>>>(json);
        return cloneWrapper.Data;
    }

    public void initializeEventList(TileModification TileMod)
    {
        TileMod.tilestomodify = new List<GridSquareScript>();
        foreach (Vector2Int IDCouple in TileMod.TilesIdCouples)
        {
            if (IDCouple.y >= IDCouple.x && IDCouple.y < grid.transform.childCount)
            {
                for (int i = IDCouple.x; i <= IDCouple.y; i++)
                {
                    TileMod.tilestomodify.Add(grid.transform.GetChild(i).GetComponent<GridSquareScript>());
                }
            }
        }
    }


    public void ApplyTilesModification(TileModification TileMod)
    {
        initializeEventList(TileMod);
        switch (TileMod.modiftype)
        {
            case 1:
                foreach (GridSquareScript tile in TileMod.tilestomodify)
                {
                    tile.activated = true;
                }
                break;
            case 2:
                foreach (GridSquareScript tile in TileMod.tilestomodify)
                {
                    tile.isobstacle = false;
                }
                break;
            case 3:
                foreach (GridSquareScript tile in TileMod.tilestomodify)
                {
                    tile.isobstacle = true;
                }
                break;
            case 4:
                foreach (GridSquareScript tile in TileMod.tilestomodify)
                {
                    tile.type = TileMod.newtype;
                }
                break;
        }
    }

    public void ManageUnitPlacement(UnitPlacement unitPlacement)
    {
        List<GameObject> movedunits = new List<GameObject>();
        foreach (string unitname in unitPlacement.UnitToPlaceManually)
        {

            int rankinList = unitPlacement.UnitToPlaceManually.IndexOf(unitname);
            foreach (GameObject unit in GridScript.allunitGOs)
            {
                if (unit.GetComponent<UnitScript>().UnitCharacteristics.name.ToLower() == unitname.ToLower())
                {
                    if (unitPlacement.WhereToManuallyPlaceUnits.Count > rankinList)
                    {
                        unit.GetComponent<UnitScript>().MoveTo(unitPlacement.WhereToManuallyPlaceUnits[rankinList], false, true);
                        movedunits.Add(unit);
                    }
                }
            }
        }
        int otherpositionsrank = 0;
        foreach (GameObject unit in GridScript.allunitGOs)
        {
            if (unit.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable" && !movedunits.Contains(unit))
            {
                if (unitPlacement.RemainingSpots.Count > otherpositionsrank)
                {
                    unit.GetComponent<UnitScript>().MoveTo(unitPlacement.RemainingSpots[otherpositionsrank], false, true);
                    otherpositionsrank++;
                    movedunits.Add(unit);
                }
            }
        }
        if (unitPlacement.CameraPosition != Vector2.zero)
        {
            FindAnyObjectByType<cameraScript>().Destination = unitPlacement.CameraPosition;
        }

    }

    public void TriggerEvent(EventCondition Event, int currentturn)
    {
        ManageUnitPlacement(Event.UnitPlacement);
        UnitAddTrigger(Event.UnitsToUnlockID, true);
        UnitAddTrigger(Event.UnitsToLockID, false);
        AddSkillToInventory(Event.skillsToAdd);
        switch (Event.triggerEffectType)
        {
            case 1:
                Debug.Log("victory trigger");
                GameOverScript.victory = true;
                GameOverScript.gameObject.SetActive(true);
                break;
            case 2:
                Debug.Log("gameover trigger");
                GameOverScript.gameObject.SetActive(true);
                break;
            case 3:
                Debug.Log("tilechange trigger");
                if (Event.tileModification != null)
                {
                    ApplyTilesModification(Event.tileModification);
                }
                TriggerEventCheck(currentturn);
                break;
            case 4:
                Debug.Log("dialogue trigger");
                TextBubbleScript.InitializeDialogue(Event.dialoguetoShow);
                break;
            case 5:
                Debug.Log("Tutorial Window trigger");

                TutorialwindowScript.InitializeWindow(Event.TutorialWindow.WindowDimensions, Event.TutorialWindow.text);
                TriggerEventCheck(currentturn);
                break;
            case 6:
                Debug.Log("spawn enemy trigger");
                SpawnnewEnemies(Event);
                TriggerEventCheck(currentturn);
                break;
            case 7:
                Debug.Log("Change Unit Status trigger");
                ChangeEnemiesToOther(Event);
                TriggerEventCheck(currentturn);
                break;
        }

    }

    public void EventInitialization()
    {
        if (EventsToMonitor != null)
        {
            foreach (EventCondition e in EventsToMonitor)
            {
                switch (e.initializationtype)
                {
                    case (1):
                        e.UnitList = new List<Character>();
                        foreach (string name in e.NameUnitList)
                        {
                            Transform AllUnitGOsHolder = GameObject.Find("Characters").transform;
                            if (AllUnitGOsHolder != null)
                            {
                                foreach (Transform unitTransform in AllUnitGOsHolder)
                                {
                                    if (unitTransform.GetComponent<UnitScript>().UnitCharacteristics.name.ToLower() == name.ToLower())
                                    {
                                        e.UnitList.Add(unitTransform.GetComponent<UnitScript>().UnitCharacteristics);
                                    }
                                }
                            }
                        }
                        break;
                    case (2):
                        e.UnitList = turnManger.playableunit;
                        break;
                    case (3):
                        e.UnitList = turnManger.enemyunit;
                        break;
                    case (4):
                        e.UnitList = turnManger.otherunits;
                        break;
                    case (5):
                        e.TilesList = new List<GridSquareScript>();
                        foreach (List<GameObject> line in GridScript.instance.Grid)
                        {
                            foreach (GameObject tile in line)
                            {
                                if (tile.GetComponent<GridSquareScript>().isfinishtile)
                                {
                                    e.TilesList.Add(tile.GetComponent<GridSquareScript>());
                                }
                            }
                        }
                        break;
                    case (6):

                        break;

                }
            }
        }
    }


    public void TriggerEventCheck(int beginningofTurn = -1)
    {
        if (SceneManager.GetActiveScene().name == "BattleScene" || TextBubbleScript.indialogue)
        {
            return;
        }
        if (EventsToMonitor != null)
        {
            EventInitialization();
            foreach (EventCondition e in EventsToMonitor)
            {
                if (!e.triggered)
                {
                    if (CheckSmallerConditionsAreChecked(e.SmallerConditions))
                    {
                        switch (e.triggertype)
                        {
                            case (1):
                                if (reachedTiles("playable", e.TilesList))
                                {
                                    e.triggered = true;
                                    TriggerEvent(e, beginningofTurn);
                                    EventInitialization();
                                    return;
                                }
                                break;
                            case (2):
                                if (reachedTiles("enemy", e.TilesList))
                                {
                                    e.triggered = true;
                                    TriggerEvent(e, beginningofTurn);
                                    EventInitialization();
                                    return;
                                }
                                break;
                            case (3):
                                if (reachedTiles("other", e.TilesList))
                                {
                                    e.triggered = true;
                                    TriggerEvent(e, beginningofTurn);
                                    EventInitialization();
                                    return;
                                }
                                break;
                            case (4):
                                if (checkIfOneDead(e.UnitList))
                                {
                                    e.triggered = true;
                                    TriggerEvent(e, beginningofTurn);
                                    EventInitialization();
                                    return;
                                }
                                break;
                            case (5):
                                if (checkIfAllDead(e.UnitList))
                                {
                                    e.triggered = true;
                                    TriggerEvent(e, beginningofTurn);
                                    EventInitialization();
                                    return;
                                }
                                break;
                            case (6):
                                e.triggered = true;
                                TriggerEvent(e, beginningofTurn);
                                EventInitialization();
                                return;
                            case (7):
                                e.triggered = true;
                                TriggerEvent(e, beginningofTurn);
                                EventInitialization();
                                return;
                            case (8):
                                if (CheckIfEventsAreTriggered(e.EventsToWatch))
                                {
                                    e.triggered = true;
                                    TriggerEvent(e, beginningofTurn);
                                    EventInitialization();
                                    return;
                                }
                                break;
                            case (9):
                                if (CheckIfMechanismsActivated(e))
                                {
                                    e.triggered = true;
                                    TriggerEvent(e, beginningofTurn);
                                    EventInitialization();
                                    return;
                                }
                                break;
                            case (10):
                                if (e.turnswheretotrigger != null && e.turnswheretotrigger.Contains(beginningofTurn))
                                {
                                    e.triggered = true;
                                    TriggerEvent(e, beginningofTurn);
                                    EventInitialization();
                                    return;
                                }
                                break;
                            case (11):
                                if (CheckIfAllCharactersTalkedTo(e))
                                {
                                    e.triggered = true;
                                    TriggerEvent(e, beginningofTurn);
                                    EventInitialization();
                                    return;
                                }
                                break;

                        }
                    }
                }
            }
        }
    }


    private bool CheckIfAllCharactersTalkedTo(EventCondition ev)
    {
        bool allcharacterstalkedto = true;

        foreach (Character chararacter in ev.UnitList)
        {
            if (!chararacter.enemyStats.talkable || !chararacter.enemyStats.talkedto)
            {
                allcharacterstalkedto = false;
                break;
            }
        }

        return allcharacterstalkedto;
    }

    private bool CheckIfEventsAreTriggered(List<int> eventIDs)
    {
        foreach (int id in eventIDs)
        {
            bool found = false;
            foreach (EventCondition e in EventsToMonitor)
            {
                if (e.ID == id)
                {
                    found = true;
                    if (!e.triggered)
                    {
                        return false;
                    }
                }
            }
            if (!found)
            {
                return false; //if one of the event was not found, we consider the condition as not verified
            }
        }
        return true;
    }


    private bool CheckSmallerConditionsAreChecked(List<SmallerCondition> smallerConditions)
    {
        foreach (SmallerCondition condition in smallerConditions)
        {
            GameObject unit1 = null;
            GameObject unit2 = null;
            foreach (GameObject unit in GridScript.allunitGOs)
            {
                if (unit != null)
                {
                    if (unit.GetComponent<UnitScript>().UnitCharacteristics.name.ToLower() == condition.Unit1.ToLower())
                    {
                        unit1 = unit;
                    }
                    if (unit.GetComponent<UnitScript>().UnitCharacteristics.name.ToLower() == condition.Unit2.ToLower())
                    {
                        unit2 = unit;
                    }
                }
            }
            switch (condition.triggertype)
            {
                case (1):
                    if (unit1 == null)
                    {
                        return false; //if unit1 was not found, then she was not deployed or is dead
                    }
                    break;
                case (2):
                    if (unit1 == null || unit2 == null)
                    {
                        return false;
                    }
                    else
                    {
                        Character charunit1 = unit1.GetComponent<UnitScript>().UnitCharacteristics;
                        Character charunit2 = unit2.GetComponent<UnitScript>().UnitCharacteristics;
                        if (ManhattanDistance(charunit1, charunit2) > 1)
                        {
                            return false;
                        }
                    }
                    break;
                case (3):
                    if (unit1 == null)
                    {
                        return false;
                    }
                    break;
                case (4):
                    if (unit1 != null)
                    {
                        return false;
                    }
                    break;
                case (5):

                    break;
                case (6):

                    break;

            }
        }


        return true;
    }

    private bool CheckIfMechanismsActivated(EventCondition eventtocheck)
    {
        bool mechanismallactivated = true;
        foreach (GridSquareScript tile in eventtocheck.TilesList)
        {
            if (tile.Mechanism != null)
            {
                if (!tile.Mechanism.isactivated)
                {
                    mechanismallactivated = false;
                    break;
                }
            }
        }

        return mechanismallactivated;
    }

    private bool reachedTiles(string affiliation, List<GridSquareScript> tiles)
    {

        List<Character> list = null;
        if (affiliation == "enemy")
        {
            list = turnManger.enemyunit;
        }
        else if (affiliation == "playable")
        {
            list = turnManger.playableunit;
        }
        else
        {
            list = turnManger.otherunits;
        }

        foreach (Character c in list)
        {
            foreach (GridSquareScript tile in c.currentTile)
            {
                if (tiles.Contains(tile))
                {
                    return true;
                }
            }

        }

        return false;
    }

    private bool checkIfAllDead(List<Character> units)
    {
        if (FindAnyObjectByType<TurnManger>().currentlyplaying == "")
        {
            return false;
        }
        if (units.Count == 0)
        {
            return true;
        }
        else
        {
            foreach (Character unit in units)
            {
                if (unit != null)
                {
                    if (unit.currentHP > 0)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    private bool checkIfOneDead(List<Character> units)
    {
        if (FindAnyObjectByType<TurnManger>().currentlyplaying == "")
        {
            return false;
        }
        if (units.Count == 0)
        {
            return true;
        }
        else
        {
            foreach (Character unit in units)
            {
                if (unit == null)
                {
                    return true;
                }
                else if (unit.currentHP <= 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void UnitAddTrigger(List<int> UnitsToUnlockID, bool unlock)
    {
        List<Character> units = DataScript.instance.PlayableCharacterList;
        foreach (Character unit in units)
        {
            if (UnitsToUnlockID.Contains(unit.ID))
            {
                Debug.Log("unlocking : " + unit.name);
                unit.playableStats.unlocked = unlock;
                unit.playableStats.deployunit = unlock;
            }
        }
    }
    /// <summary>
    /// Adds Skill to Inventory.
    /// </summary>
    private void AddSkillToInventory(List<SkillsToAdd> SkillsToAdd)
    {

        DataScript DS = DataScript.instance;

        if (DS != null)
        {
            foreach (SkillsToAdd skill in SkillsToAdd)
            {
                foreach (DataScript.InventoryItem inventoryItem in DS.PlayerInventory.inventoryItems)
                {
                    if (inventoryItem.type == 1 && inventoryItem.ID == skill.SkillID)
                    {
                        inventoryItem.Quantity += skill.SkillQuantity;
                    }
                    continue;
                }
            }

        }
    }

    private int ManhattanDistance(Character unit, Character otherunit)
    {
        return (int)(Mathf.Abs(unit.position.x - otherunit.position.x) + Mathf.Abs(unit.position.y - otherunit.position.y));
    }

    private void SpawnnewEnemies(EventCondition e)
    {
        if (e.CharactersToSpawn != null && e.CharactersToSpawn.Count > 0)
        {
            foreach (EnemyStats enemyStats in e.CharactersToSpawn)
            {
                FindAnyObjectByType<MapInitializer>().InitializeNonPlayable(enemyStats);
            }
            GridScript.InitializeGOList();

        }
    }

    private void ChangeEnemiesToOther(EventCondition e)
    {
        if (e.UnitList != null && e.UnitList.Count > 0)
        {
            foreach (Character charactertochange in e.UnitList)
            {
                charactertochange.affiliation = "other";
            }
            GridScript.InitializeGOList();

        }
    }
}
