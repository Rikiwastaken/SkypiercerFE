using System;
using System.Collections.Generic;
using UnityEngine;
using static UnitScript;
using static TextBubbleScript;


public class MapEventManager : MonoBehaviour
{

    [Serializable]
    public class EventCondition
    {
        public string name;
        public List<Character> UnitList;
        public List<string> NameUnitList;
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
         */
        public int initializationtype;
        /* 1 : Get Units From Names
         * 2 : Get Playable Units
         * 3 : Get Enemy Units
         * 4 : Get Other Units
         * 5 : 
         * 6 : 
         */
        public bool triggered;
        public int triggerEffectType;
        /* 1 : Win the game
         * 2 : Lose the game
         * 3 : ModifyTiles
         * 4 : ShowDialogue
         * 5 : 
         * 6 : 
         */
        public List<TextBubbleInfo> dialoguetoShow;

        public TileModification tileModification;
        public void TriggerEvent(GameOverScript GameOverScript)
        {
            switch (triggerEffectType)
            {
                case 1:
                    GameOverScript.victory = true;
                    GameOverScript.gameObject.SetActive(true);
                    break;
                case 2:
                    GameOverScript.gameObject.SetActive(true);
                    break;
                case 3:
                    if(tileModification != null)
                    {
                        tileModification.ApplyModification();
                    }
                    break;
                case 4:
                    FindAnyObjectByType<TextBubbleScript>().InitializeDialogue(dialoguetoShow);
                    break;
            }
            FindAnyObjectByType<MapEventManager>().EventInitialization();
        }

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
    public class TileModification
    {
        public List<Vector2Int> TilesIdCouples;
        private List<GridSquareScript> tilestomodify;
        public string newtype;
        public int modiftype;
        /* 1 : activate
         * 2 : remove wall
         * 3 : become wall
         * 4 : change tile type
         * 5 : 
         * 6 : 
         */

        public void initializelists()
        {
            GameObject grid = GameObject.Find("Grid");
            tilestomodify = new List<GridSquareScript>();
            foreach(Vector2Int IDCouple in TilesIdCouples)
            {
                if (IDCouple.y >= IDCouple.x && IDCouple.y < grid.transform.childCount)
                {
                    for (int i = IDCouple.x; i <= IDCouple.y; i++)
                    {
                        tilestomodify.Add(grid.transform.GetChild(i).GetComponent<GridSquareScript>());
                    }
                }
            }
        }
        public void ApplyModification()
        {
            initializelists();
            switch (modiftype)
            {
                case 1:
                    foreach(GridSquareScript tile in tilestomodify)
                    {
                        tile.activated = true ;
                    }
                    break;
                case 2:
                    foreach (GridSquareScript tile in tilestomodify)
                    {
                        tile.isobstacle = false;
                    }
                    break;
                case 3:
                    foreach (GridSquareScript tile in tilestomodify)
                    {
                        tile.isobstacle = true;
                    }
                    break;
                case 4:
                    foreach (GridSquareScript tile in tilestomodify)
                    {
                        tile.type = newtype;
                    }
                    break;
            }
        }
    }

    public List<EventCondition> EventsToMonitor;

    private GridScript GridScript;
    private TurnManger turnManger;

    public GameOverScript GameOverScript;

    private bool eventinitialized;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GridScript = GetComponent<GridScript>();
        turnManger = GetComponent<TurnManger>();
    }

    private void FixedUpdate()
    {
        if((GetComponent<TurnManger>().currentlyplaying=="playable" || GetComponent<TurnManger>().currentlyplaying == "other" || GetComponent<TurnManger>().currentlyplaying == "enemy") && !eventinitialized)
        {
            eventinitialized = true;
            EventInitialization();
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
                        foreach(string name in e.NameUnitList )
                        {
                            foreach(Character unit in GridScript.allunits)
                            {
                                if(unit.name.ToLower()==name.ToLower())
                                {
                                    e.UnitList.Add(unit);
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

                        break;
                    case (6):

                        break;

                }
            }
        }
    }


    public void TriggerEventCheck()
    {
        if (EventsToMonitor != null)
        {
            foreach (EventCondition e in EventsToMonitor)
            {
                if (!e.triggered)
                {
                    if(CheckSmallerConditionsAreChecked(e.SmallerConditions))
                    {
                        switch (e.triggertype)
                        {
                            case (1):
                                if (reachedTiles("playable", e.TilesList))
                                {
                                    e.TriggerEvent(GameOverScript);
                                    e.triggered = true;
                                }
                                break;
                            case (2):
                                if (reachedTiles("enemy", e.TilesList))
                                {
                                    e.TriggerEvent(GameOverScript);
                                    e.triggered = true;
                                }
                                break;
                            case (3):
                                if (reachedTiles("other", e.TilesList))
                                {
                                    e.TriggerEvent(GameOverScript);
                                    e.triggered = true;
                                }
                                break;
                            case (4):
                                if (checkIfOneDead(e.UnitList))
                                {
                                    e.TriggerEvent(GameOverScript);
                                    e.triggered = true;
                                }
                                break;
                            case (5):
                                if (checkIfAllDead(e.UnitList))
                                {
                                    e.TriggerEvent(GameOverScript);
                                    e.triggered = true;
                                }
                                break;
                            case (6):
                                e.TriggerEvent(GameOverScript);
                                e.triggered = true;
                                break;

                        }
                    }
                }
            }
        }
    }

    

    private bool CheckSmallerConditionsAreChecked(List<SmallerCondition> smallerConditions)
    {
        foreach(SmallerCondition condition in smallerConditions)
        {
            GameObject unit1 = null;
            GameObject unit2 = null;
            foreach (GameObject unit in GridScript.allunitGOs)
            {
                if(unit != null)
                {
                    Character unitcharacter = unit.GetComponent<UnitScript>().UnitCharacteristics;
                    if (unitcharacter.name.ToLower() == condition.Unit1.ToLower())
                    {
                        unit1 = unit;
                    }
                    if (unitcharacter.name.ToLower() == condition.Unit2.ToLower())
                    {
                        unit2 = unit;
                    }
                }
            }
            switch (condition.triggertype)
            {
                case (1):
                    if(unit1==null)
                    {
                        return false; //if unit1 was not found, then she was not deployed or is dead
                    }
                    break;
                case (2):
                    if(unit1== null || unit2== null)
                    {
                        return false;
                    }
                    else
                    {
                        Character charunit1 = unit1.GetComponent<UnitScript>().UnitCharacteristics;
                        Character charunit2 = unit2.GetComponent<UnitScript>().UnitCharacteristics;
                        if(ManhattanDistance(charunit1, charunit2)>1)
                        {
                            return false;
                        }
                    }
                    break;
                case (3):
                    if(unit1==null)
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
    private bool reachedTiles(string affiliation, List<GridSquareScript> tiles)
    {
        
        List<Character> list = null;
        if(affiliation=="enemy")
        {
            list = turnManger.enemyunit;
        }
        else if(affiliation=="playable")
        {
            list = turnManger.playableunit;
        }
        else
        {
            list = turnManger.otherunits;
        }

        foreach(Character c in list)
        {
            if(tiles.Contains(c.currentTile))
            {
                return true;
            }
        }

        return false;
    }

    private bool checkIfAllDead(List<Character> units)
    {
        if(units.Count==0)
        {
            return true;
        }
        else
        {
            foreach (Character unit in units)
            {
                if(unit != null)
                {
                    if(unit.currentHP > 0)
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

    private int ManhattanDistance(Character unit, Character otherunit)
    {
        return (int)(Mathf.Abs(unit.position.x - otherunit.position.x) + Mathf.Abs(unit.position.y - otherunit.position.y));
    }

}
