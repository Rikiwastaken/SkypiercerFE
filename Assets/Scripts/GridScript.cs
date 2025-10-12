using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnitScript;
public class GridScript : MonoBehaviour
{

    public GameObject GridSquarePrefab;

    public List<List<GameObject>> Grid;

    public Vector2 GridDimensions;

    public GridSquareScript selection;

    private InputManager inputManager;

    private Vector2 previousmovevalue;

    public List<Character> allunits;
    public List<GameObject> allunitGOs;

    public List<GridSquareScript> movementtiles;
    public List<GridSquareScript> attacktiles;
    public List<GridSquareScript> healingtiles;
    public List<GridSquareScript> lockedmovementtiles;
    public List<GridSquareScript> lockedattacktiles;
    public List<GridSquareScript> lockedhealingtiles;
    public List<GridSquareScript> dangerousTiles;

    public GridSquareScript lastSquare;

    private int moveCD;

    public bool lockselection;

    public GameObject actionsMenu;

    public int movementbuffercounter;
    public float movementbuffer;

    private Vector2 previousmovementvalueforbuffer;

    public TextMeshProUGUI tiletext;
    public GameObject TileImageContainer;
    private SkillEditionScript SkillEditionScript;

    private TextBubbleScript textBubble;

    private battlecameraScript battlecamera;

    public class Node
    {
        public Vector2 Position;
        public Node Parent;
        public float G; // cost from start
        public float H; // heuristic (distance to goal)
        public float F => G + H; // total score

        public Node(Vector2 pos, Node parent, float g, float h)
        {
            Position = pos;
            Parent = parent;
            G = g;
            H = h;
        }
    }

    private void Awake()
    {
        if (FindAnyObjectByType<DataScript>() == null)
        {
            SceneManager.LoadScene("FirstScene");
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lockedmovementtiles = new List<GridSquareScript>();
        lockedattacktiles = new List<GridSquareScript>();
        lockedhealingtiles = new List<GridSquareScript>();
        InitializeGOList();
        textBubble = FindAnyObjectByType<TextBubbleScript>();
        battlecamera = FindAnyObjectByType<battlecameraScript>();

    }

    private void FixedUpdate()
    {

        if (SkillEditionScript == null)
        {
            SkillEditionScript = FindAnyObjectByType<SkillEditionScript>();
        }


        if (inputManager == null)
        {
            inputManager = FindAnyObjectByType<InputManager>();
        }

        if (moveCD > 0)
        {
            moveCD--;
        }

        if (movementbuffercounter > 0)
        {
            movementbuffercounter--;
        }

        if (inputManager.movementValue == Vector2.zero)
        {
            movementbuffercounter = 0;
        }

        if (moveCD <= 0 && !actionsMenu.activeSelf && (GetComponent<TurnManger>().currentlyplaying == "playable" || GetComponent<TurnManger>().currentlyplaying == "") && movementbuffercounter <= 0 && !textBubble.indialogue)
        {
            

            if (inputManager.movementValue != Vector2.zero && inputManager.movementValue != previousmovevalue)
            {
                moveCD = (int)(0.1f / Time.deltaTime);
                previousmovevalue = battlecamera.DetermineDirection(inputManager.movementValue);
                MoveSelection(previousmovevalue);
            }
            else
            {
                previousmovevalue = Vector2.zero;
            }

            if ((inputManager.NextWeaponjustpressed || inputManager.PreviousWeaponjustpressed) && GetComponent<TurnManger>().currentlyplaying == "playable")
            {
                GameObject GOSelected = GetUnit(selection);
                if (GOSelected != null)
                {
                    if (GOSelected.GetComponent<UnitScript>().UnitCharacteristics.alreadyplayed || GOSelected.GetComponent<UnitScript>().UnitCharacteristics.affiliation != "playable")
                    {
                        foreach (GameObject characterGO in allunitGOs)
                        {
                            Character character = characterGO.GetComponent<UnitScript>().UnitCharacteristics;
                            if (character.affiliation == "playable" && character.alreadyplayed == false)
                            {
                                selection = GetTile(character.position);
                                break;
                            }
                        }
                    }
                    else if(SkillEditionScript!=null)
                    {
                        if (!SkillEditionScript.gameObject.activeSelf)
                        {
                            GameObject Characters = GameObject.Find("Characters");
                            List<GameObject> listplayable = new List<GameObject>();
                            int index = 0;
                            int currentunitindex = -1;
                            for (int i = 0; i < Characters.transform.childCount; i++)
                            {
                                GameObject unit = Characters.transform.GetChild(i).gameObject;
                                if (unit.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable" && !unit.GetComponent<UnitScript>().UnitCharacteristics.alreadyplayed)
                                {
                                    if (unit == GOSelected)
                                    {
                                        currentunitindex = index;
                                    }
                                    listplayable.Add(unit);
                                    index++;
                                }

                            }
                            if (inputManager.NextWeaponjustpressed)
                            {
                                if (currentunitindex >= listplayable.Count - 1 || currentunitindex == -1)
                                {
                                    selection = GetTile(listplayable[0].GetComponent<UnitScript>().UnitCharacteristics.position);

                                }
                                else
                                {
                                    selection = GetTile(listplayable[currentunitindex + 1].GetComponent<UnitScript>().UnitCharacteristics.position);
                                }
                            }
                            else
                            {
                                if (currentunitindex == -1)
                                {
                                    selection = GetTile(listplayable[0].GetComponent<UnitScript>().UnitCharacteristics.position);

                                }
                                else if (currentunitindex > 0)
                                {
                                    selection = GetTile(listplayable[currentunitindex - 1].GetComponent<UnitScript>().UnitCharacteristics.position);
                                }
                                else
                                {
                                    selection = GetTile(listplayable[listplayable.Count - 1].GetComponent<UnitScript>().UnitCharacteristics.position);
                                }
                            }

                        }
                    }
                    
                }
                else
                {
                    foreach (GameObject characterGO in allunitGOs)
                    {
                        Character character = characterGO.GetComponent<UnitScript>().UnitCharacteristics;
                        if (character.affiliation == "playable" && character.alreadyplayed == false)
                        {
                            selection = GetTile(character.position);
                            break;
                        }
                    }
                }
                ShowMovement();
                GetComponent<ActionManager>().frameswherenotlock = 5;
            }

        }

        //if (inputManager.movementValue != Vector2.zero && inputManager.movementValue != previousmovevalue && moveCD <= 0 && !actionsMenu.activeSelf && (GetComponent<TurnManger>().currentlyplaying == "playable" || GetComponent<TurnManger>().currentlyplaying == "") && movementbuffercounter <= 0)
        //{
        //    moveCD = (int)(0.1f / Time.deltaTime);
        //    previousmovevalue = inputManager.movementValue;
        //    MoveSelection(previousmovevalue);
        //}
        //else
        //{
        //    previousmovevalue = Vector2.zero;
        //}



        if (previousmovementvalueforbuffer != inputManager.movementValue && previousmovementvalueforbuffer == Vector2.zero)
        {
            movementbuffercounter = (int)(movementbuffer / Time.fixedDeltaTime);
        }

        previousmovementvalueforbuffer = battlecamera.DetermineDirection(inputManager.movementValue);
        UpdateTileText();
        if(lockselection && movementtiles.Count==0 && lockedmovementtiles.Count==0 && attacktiles.Count == 0 && lockedattacktiles.Count == 0 && healingtiles.Count == 0 && lockedhealingtiles.Count == 0)
        {
            lockselection = false;
        }
    }

    public void InstantiateGrid()
    {
        Grid = new List<List<GameObject>>();
        GridSquareScript[] tilelist = FindObjectsByType<GridSquareScript>(FindObjectsSortMode.None);
        if(lastSquare == null)
        {
            lastSquare = GameObject.Find("Grid").transform.GetChild(GameObject.Find("Grid").transform.childCount - 1).GetComponent<GridSquareScript>();
        }
        lastSquare.InitializePosition();
        for (int x = 0; x <= lastSquare.GridCoordinates.x; x++)
        {
            Grid.Add(new List<GameObject>());
            for (int y = 0; y <= lastSquare.GridCoordinates.y; y++)
            {
                foreach (GridSquareScript tile in tilelist)
                {
                    tile.InitializePosition();
                    if ((int)tile.GridCoordinates.x == x && (int)tile.GridCoordinates.y == y)
                    {
                        Grid[x].Add(tile.gameObject);
                    }
                }
            }
        }
        for (int x = 0; x < Grid.Count; x++)
        {
            for (int y = 0; y < Grid[x].Count; y++)
            {
                if (Grid[x][y].GetComponent<GridSquareScript>().isobstacle)
                {
                    Grid[x][y].GetComponent<GridSquareScript>().fillwithGrey();
                }
                else
                {
                    Grid[x][y].GetComponent<GridSquareScript>().fillwithNothing();
                }

            }
        }
        GridDimensions = new Vector2(Grid.Count, Grid[0].Count);
        selection = GetTile(GetComponent<MapInitializer>().playablepos[0]);
        
    }

    private void UpdateTileText()
    {
        string text = "";

        if (selection != null)
        {
            if (selection.isobstacle)
            {
                text += "Wall\nNo effect";
            }
            else if (selection.isstairs)
            {
                text += "Stairs\nNo elevation penalty";
            }
            else
            {
                text = "Ground\nNo effect";
            }

            if (selection.type.ToLower() == "forest")
            {
                text = "Forest \n+30% Dodge";
            }
            if (selection.type.ToLower() == "ruins")
            {
                text = "Ruins \n+10% Dodge\n-10% Hit";
            }
            if (selection.type.ToLower() == "fire")
            {
                text = "Fire \n-1 mvt\n-10% Def/Res";
            }
            if (selection.type.ToLower() == "water")
            {
                text = "Water \n-1 mvt\n-10% Dodge";
            }
            if (selection.type.ToLower() == "highground")
            {
                text = "High Ground \n+20% Hit\n+10% Dodge";
            }
            if (selection.type.ToLower() == "fortification")
            {
                text = "Fortification \n+5% Dodge\n+15% Hit";
            }
            if (selection.type.ToLower() == "fog")
            {
                text = "Fog \n+20% Dodge\n-20% Hit";
            }
            text += "\n Elevation : " + selection.elevation;
            UpdateWeatherImage(selection);
        }
        tiletext.text = text;
    }

    private void UpdateWeatherImage(GridSquareScript tile)
    {
        TileImageContainer.transform.GetChild(0).gameObject.SetActive(false);
        TileImageContainer.transform.GetChild(1).gameObject.SetActive(false);
        TileImageContainer.transform.GetChild(2).gameObject.SetActive(false);
        if (tile.RemainingRainTurns > 0)
        {
            TileImageContainer.transform.GetChild(0).gameObject.SetActive(true);
        }
        else if(tile.RemainingSunTurns > 0)
        {
            TileImageContainer.transform.GetChild(1).gameObject.SetActive(true);
        }
        else
        {
            TileImageContainer.transform.GetChild(2).gameObject.SetActive(true);
        }
    }

    public void MoveSelection(Vector2 input)
    {
        if (selection == null)
        {
            selection = GetTile(GetComponent<MapInitializer>().playablepos[0]);
            FindAnyObjectByType<battlecameraScript>().transform.position = new Vector3(selection.GridCoordinates.x, FindAnyObjectByType<battlecameraScript>().transform.position.y,selection.GridCoordinates.y);
        }
        else
        {
            if (input.x > 0 && (int)selection.GridCoordinates.x < Grid.Count - 1)
            {
                GridSquareScript newtile = Grid[(int)(selection.GridCoordinates.x) + 1][(int)(selection.GridCoordinates.y)].GetComponent<GridSquareScript>();
                if(newtile.activated)
                {
                    selection = newtile;
                }
            }
            if (input.x < 0 && (int)selection.GridCoordinates.x > 0)
            {
                GridSquareScript newtile = Grid[(int)(selection.GridCoordinates.x) - 1][(int)(selection.GridCoordinates.y)].GetComponent<GridSquareScript>();
                if (newtile.activated)
                {
                    selection = newtile;
                }
            }
            if (input.y > 0 && (int)selection.GridCoordinates.y < Grid[0].Count - 1)
            {
                GridSquareScript newtile = Grid[(int)(selection.GridCoordinates.x)][(int)(selection.GridCoordinates.y) + 1].GetComponent<GridSquareScript>();
                if (newtile.activated)
                {
                    selection = newtile;
                }
            }
            if (input.y < 0 && (int)selection.GridCoordinates.y > 0)
            {
                GridSquareScript newtile = Grid[(int)(selection.GridCoordinates.x)][(int)(selection.GridCoordinates.y) - 1].GetComponent<GridSquareScript>();
                if (newtile.activated)
                {
                    selection = newtile;
                }
            }
        }
        ShowMovement();

    }

    public void LockcurrentSelection()
    {
        lockedmovementtiles = new List<GridSquareScript>();
        foreach (GridSquareScript tile in movementtiles)
        {
            lockedmovementtiles.Add(tile);
        }
        lockedattacktiles = new List<GridSquareScript>();
        foreach (GridSquareScript tile in attacktiles)
        {
            lockedattacktiles.Add(tile);
        }
        lockedhealingtiles = new List<GridSquareScript>();
        foreach (GridSquareScript tile in healingtiles)
        {
            lockedhealingtiles.Add(tile);
        }
    }

    public void InitializeGOList()
    {
        allunits = new List<Character>();
        allunitGOs = new List<GameObject>();
        foreach (UnitScript character in FindObjectsByType<UnitScript>(FindObjectsSortMode.None))
        {
            allunits.Add(character.UnitCharacteristics);
            allunitGOs.Add(character.gameObject);
        }
        GetComponent<TurnManger>().InitializeUnitLists(allunitGOs);
    }
    public void UnlockSelection()
    {
        lockedmovementtiles = new List<GridSquareScript>();
        lockedattacktiles = new List<GridSquareScript>();
        lockedhealingtiles = new List<GridSquareScript>();
        lockselection = false;
    }

    public List<GridSquareScript> ExpandSelection(List<GridSquareScript> selection, bool ignoreobstacles)
    {
        List<GridSquareScript> newlist = new List<GridSquareScript>();
        foreach (GridSquareScript tile in selection)
        {
            List<Vector2> newpositions = new List<Vector2>
            {
                new Vector2(1,0), new Vector2(0,1), new Vector2(-1,0),new Vector2(0,-1)
            };

            foreach (Vector2 position in newpositions)
            {
                GridSquareScript newtile = GetTile(position + tile.GridCoordinates);
                if (newtile != null && !newlist.Contains(newtile) && (!newtile.isobstacle || ignoreobstacles))
                {
                    newlist.Add(newtile);
                }
            }
        }
        return newlist;
    }

    public void ResetAllSelections()
    {
        lockedmovementtiles = new List<GridSquareScript>();
        lockedattacktiles = new List<GridSquareScript>();
        lockedhealingtiles = new List<GridSquareScript>();
        movementtiles = new List<GridSquareScript>();
        attacktiles = new List<GridSquareScript>();
        healingtiles = new List<GridSquareScript>();
        lockselection = false;
    }

    public void ShowMovement()
    {
        for (int x = 0; x < Grid.Count; x++)
        {
            for (int y = 0; y < Grid[x].Count; y++)
            {
                if (Grid[x][y].GetComponent<GridSquareScript>().isobstacle && !lockedattacktiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()) && !lockedmovementtiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()) && !lockedhealingtiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()))
                {
                    Grid[x][y].GetComponent<GridSquareScript>().fillwithGrey();
                }
                else if (!lockedattacktiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()) && !lockedmovementtiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()) && !lockedhealingtiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()))
                {
                    Grid[x][y].GetComponent<GridSquareScript>().fillwithNothing();
                }

            }
        }
        movementtiles = new List<GridSquareScript>();
        attacktiles = new List<GridSquareScript>();
        healingtiles = new List<GridSquareScript>();
        foreach (GameObject unitGO in allunitGOs)
        {
            if (unitGO != null)
            {
                Character unit = unitGO.GetComponent<UnitScript>().UnitCharacteristics;
                if (unit.currentTile.Contains(selection) && !unit.alreadymoved)
                {
                    string tiletype = GetTile((int)unit.position.x, (int)unit.position.y).type;
                    int movements = unit.movements;
                    if (tiletype.ToLower() == "fire" || tiletype.ToLower() == "water") //checking if movement reducing effect
                    {
                        movements -= 1;
                    }
                    if (unitGO.GetComponent<UnitScript>().GetSkill(1))//checking if unit is using canto/Retreat
                    {
                        movements -= 2;
                    }
                    if (unitGO.GetComponent<UnitScript>().GetSkill(5)) // checking if unit is using Fast Legs
                    {
                        movements += 1;
                    }
                    SpreadMovements(unit.position, movements, movementtiles, unitGO, new Dictionary<GridSquareScript, int>());
                    if (unit.enemyStats.monsterStats.size > 1)
                    {
                        CheckMovementsForBigUnits(unit);
                    }
                    (int range, bool melee, string type) = unitGO.GetComponent<UnitScript>().GetRangeMeleeAndType();
                    ShowAttack(range, melee, type.ToLower() == "staff", false, unit.enemyStats.monsterStats.size);
                }
            }


        }
        if (!lockselection)
        {
            foreach (GridSquareScript gridSquareScript in movementtiles)
            {
                gridSquareScript.fillwithblue();
            }
        }

        foreach (GridSquareScript gridSquareScript in lockedmovementtiles)
        {
            gridSquareScript.fillwithblue();
        }

    }

    private void CheckMovementsForBigUnits(Character unit)
    {
        List<Vector2> Otherpositions  = new List<Vector2>() { new Vector2(0, 0),new Vector2(0,1),new Vector2(-1,0), new Vector2(-1,1) };
        List<GridSquareScript> tilestoremove = new List<GridSquareScript>();
        foreach(GridSquareScript tile in movementtiles)
        {
            foreach (Vector2 newpos in Otherpositions)
            {
                GridSquareScript newtile = GetTile(tile.GridCoordinates+newpos);
                if(newtile.isobstacle || !newtile.activated || tile.elevation != newtile.elevation)
                {
                    tilestoremove.Add(tile);
                    continue;
                }
                GameObject newunit = GetUnit(newtile);
                if (newunit!=null)
                {
                    Character character = newunit.GetComponent<UnitScript>().UnitCharacteristics;
                    if(character != unit && character !=null)
                    {
                        tilestoremove.Add(tile);
                        continue;
                    }
                }
            }
        }
        foreach(GridSquareScript tile in tilestoremove)
        {
            movementtiles.Remove(tile);
        }
    }

    public void ShowMovementOfUnit(GameObject Target)
    {
        for (int x = 0; x < Grid.Count; x++)
        {
            for (int y = 0; y < Grid[x].Count; y++)
            {
                if (Grid[x][y].GetComponent<GridSquareScript>().isobstacle && !lockedattacktiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()) && !lockedmovementtiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()) && !lockedhealingtiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()))
                {
                    Grid[x][y].GetComponent<GridSquareScript>().fillwithGrey();
                }
                else if (!lockedattacktiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()) && !lockedmovementtiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()) && !lockedhealingtiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()))
                {
                    Grid[x][y].GetComponent<GridSquareScript>().fillwithNothing();
                }

            }
        }
        movementtiles = new List<GridSquareScript>();
        attacktiles = new List<GridSquareScript>();
        foreach (GameObject unitGO in allunitGOs)
        {
            if (unitGO != null)
            {
                Character unit = unitGO.GetComponent<UnitScript>().UnitCharacteristics;
                if (unit.position == Target.GetComponent<UnitScript>().UnitCharacteristics.position && !unit.alreadyplayed)
                {
                    string tiletype = GetTile((int)unit.position.x, (int)unit.position.y).type;
                    int movements = unit.movements;
                    if (tiletype.ToLower() == "fire" || tiletype.ToLower() == "water") //checking if movement reducing effect
                    {
                        movements -= 1;
                    }
                    if (unitGO.GetComponent<UnitScript>().GetSkill(1))//checking if unit is using canto/Retreat
                    {
                        movements -= 2;
                    }
                    if (unitGO.GetComponent<UnitScript>().GetSkill(5)) // checking if unit is using Fast Legs
                    {
                        movements += 1;
                    }
                    SpreadMovements(unit.position, movements, movementtiles, unitGO, new Dictionary<GridSquareScript, int>());
                    (int range, bool melee, string type) = unitGO.GetComponent<UnitScript>().GetRangeMeleeAndType();
                    ShowAttack(range, melee, type.ToLower() == "staff", false, unit.enemyStats.monsterStats.size);
                }
            }


        }
        if (!lockselection)
        {
            foreach (GridSquareScript gridSquareScript in movementtiles)
            {
                gridSquareScript.fillwithblue();
            }
        }

        foreach (GridSquareScript gridSquareScript in lockedmovementtiles)
        {
            gridSquareScript.fillwithblue();
        }

    }

    public void ShowLimitedMovementOfUnit(GameObject unit, int remainingmovements)
    {
        for (int x = 0; x < Grid.Count; x++)
        {
            for (int y = 0; y < Grid[x].Count; y++)
            {
                if (Grid[x][y].GetComponent<GridSquareScript>().isobstacle && !lockedattacktiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()) && !lockedmovementtiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()) && !lockedhealingtiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()))
                {
                    Grid[x][y].GetComponent<GridSquareScript>().fillwithGrey();
                }
                else if (!lockedattacktiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()) && !lockedmovementtiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()) && !lockedhealingtiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()))
                {
                    Grid[x][y].GetComponent<GridSquareScript>().fillwithNothing();
                }

            }
        }
        movementtiles = new List<GridSquareScript>();
        attacktiles = new List<GridSquareScript>();
        Character unitchar = unit.GetComponent<UnitScript>().UnitCharacteristics;
        string tiletype = GetTile((int)unitchar.position.x, (int)unitchar.position.y).type;
        int movements = remainingmovements;
        if (tiletype.ToLower() == "fire" || tiletype.ToLower() == "water") //checking if movement reducing effect
        {
            movements -= 1;
        }
        SpreadMovements(unitchar.position, movements, movementtiles, unit, new Dictionary<GridSquareScript, int>());
        if (!lockselection)
        {
            foreach (GridSquareScript gridSquareScript in movementtiles)
            {
                gridSquareScript.fillwithblue();
            }
        }

        foreach (GridSquareScript gridSquareScript in lockedmovementtiles)
        {
            gridSquareScript.fillwithblue();
        }

    }

    public Character GetSelectedUnit()
    {
        Character SelectedUnit = null;


        foreach (Character unit in allunits)
        {
            if (unit.position == selection.GridCoordinates)
            {
                SelectedUnit = unit;
                break;
            }
        }

        return SelectedUnit;
    }

    public List<GameObject> GetUnitFromTileList(List<GridSquareScript> listtiles)
    {
        List<GameObject> list = new List<GameObject>();
        foreach (GridSquareScript tile in listtiles)
        {
            GameObject newobject = GetUnit(tile);
            if (newobject != null)
            {
                list.Add(newobject);
            }
        }
        return list;
    }

    public GameObject GetUnit(GridSquareScript tile)
    {
        foreach (GameObject unit in allunitGOs)
        {
            if (unit.Equals(null) || unit == null)
            {
                continue;
            }
            if(unit.GetComponent<UnitScript>().UnitCharacteristics.currentTile.Count>0)
            {
                if(unit.GetComponent<UnitScript>().UnitCharacteristics.currentTile.Contains(tile))
                {
                    return unit;
                }
            }
            else if (unit.GetComponent<UnitScript>().UnitCharacteristics.position == tile.GridCoordinates)
            {
                return unit;
            }
        }
        return null;
    }

    public GameObject GetSelectedUnitGameObject()
    {
        GameObject SelectedUnit = null;


        foreach (GameObject unit in allunitGOs)
        {
            if (!unit.Equals(null) || unit != null)
            {
                if (unit.GetComponent<UnitScript>().UnitCharacteristics.currentTile.Contains(selection))
                {
                    SelectedUnit = unit;
                    break;
                }
            }

        }
        return SelectedUnit;
    }

    public void ShowAttack(int range, bool frapperenmelee, bool usingstaff, bool uselockedmovementtile = false, int size = 0)
    {
        attacktiles = new List<GridSquareScript>();
        healingtiles = new List<GridSquareScript>();
        List<GridSquareScript> tilestousetemp = new List<GridSquareScript>();
        List<GridSquareScript> tilestouse = new List<GridSquareScript>();
        if (uselockedmovementtile)
        {
            tilestousetemp = lockedmovementtiles;
        }
        else
        {
            tilestousetemp = movementtiles;
        }
        
        if(size >1)
        {
            List<Vector2> addedvectors = new List<Vector2>() { new Vector2(0, 0), new Vector2(0, 1), new Vector2(-1, 1), new Vector2(-1, 0) };
            foreach (GridSquareScript tile in tilestousetemp)
            {
                foreach(Vector2 vector in addedvectors)
                {
                    if (!tilestouse.Contains(GetTile(tile.GridCoordinates + vector)))
                    {
                        tilestouse.Add(GetTile(tile.GridCoordinates + vector));
                    }
                }
            }
        }
        else
        {
            tilestouse = tilestousetemp;
        }
        foreach (GridSquareScript tile in tilestouse)
        {
            for (int i = 1; i <= range; i++)
            {
                if (i == 1 && !frapperenmelee)
                {
                    continue;
                }
                if (i > 1)
                {
                    for (int x = 1; x <= i - 1; x++)
                    {
                        for (int y = 1; y <= i - 1; y++)
                        {
                            Vector2 vectorforattack = tile.GridCoordinates + new Vector2(x, y);
                            if (CheckIfPositionIsLegal(vectorforattack))
                            {
                                if (usingstaff)
                                {
                                    healingtiles.Add(Grid[(int)(vectorforattack.x)][(int)(vectorforattack.y)].GetComponent<GridSquareScript>());
                                }
                                else
                                {
                                    attacktiles.Add(Grid[(int)(vectorforattack.x)][(int)(vectorforattack.y)].GetComponent<GridSquareScript>());
                                }
                            }
                            vectorforattack = tile.GridCoordinates + new Vector2(x, -y);
                            if (CheckIfPositionIsLegal(vectorforattack))
                            {
                                if (usingstaff)
                                {
                                    healingtiles.Add(Grid[(int)(vectorforattack.x)][(int)(vectorforattack.y)].GetComponent<GridSquareScript>());
                                }
                                else
                                {
                                    attacktiles.Add(Grid[(int)(vectorforattack.x)][(int)(vectorforattack.y)].GetComponent<GridSquareScript>());
                                }
                            }
                            vectorforattack = tile.GridCoordinates + new Vector2(-x, y);
                            if (CheckIfPositionIsLegal(vectorforattack))
                            {
                                if (usingstaff)
                                {
                                    healingtiles.Add(Grid[(int)(vectorforattack.x)][(int)(vectorforattack.y)].GetComponent<GridSquareScript>());
                                }
                                else
                                {
                                    attacktiles.Add(Grid[(int)(vectorforattack.x)][(int)(vectorforattack.y)].GetComponent<GridSquareScript>());
                                }
                            }
                            vectorforattack = tile.GridCoordinates + new Vector2(-x, -y);
                            if (CheckIfPositionIsLegal(vectorforattack))
                            {
                                if (usingstaff)
                                {
                                    healingtiles.Add(Grid[(int)(vectorforattack.x)][(int)(vectorforattack.y)].GetComponent<GridSquareScript>());
                                }
                                else
                                {
                                    attacktiles.Add(Grid[(int)(vectorforattack.x)][(int)(vectorforattack.y)].GetComponent<GridSquareScript>());
                                }
                            }
                        }
                    }
                }
                if (tile.GridCoordinates.x >= i)
                {
                    if (usingstaff)
                    {
                        healingtiles.Add(Grid[(int)(tile.GridCoordinates.x - i)][(int)(tile.GridCoordinates.y)].GetComponent<GridSquareScript>());
                    }
                    else
                    {
                        attacktiles.Add(Grid[(int)(tile.GridCoordinates.x - i)][(int)(tile.GridCoordinates.y)].GetComponent<GridSquareScript>());
                    }
                }
                if (tile.GridCoordinates.x < Grid.Count - i)
                {
                    if (usingstaff)
                    {
                        healingtiles.Add(Grid[(int)(tile.GridCoordinates.x + i)][(int)(tile.GridCoordinates.y)].GetComponent<GridSquareScript>());
                    }
                    else
                    {
                        attacktiles.Add(Grid[(int)(tile.GridCoordinates.x + i)][(int)(tile.GridCoordinates.y)].GetComponent<GridSquareScript>());
                    }
                }
                if (tile.GridCoordinates.y >= i)
                {
                    if (usingstaff)
                    {
                        healingtiles.Add(Grid[(int)(tile.GridCoordinates.x)][(int)(tile.GridCoordinates.y - i)].GetComponent<GridSquareScript>());
                    }
                    else
                    {
                        attacktiles.Add(Grid[(int)(tile.GridCoordinates.x)][(int)(tile.GridCoordinates.y - i)].GetComponent<GridSquareScript>());
                    }

                }
                if (tile.GridCoordinates.y < Grid[0].Count - i)
                {
                    if (usingstaff)
                    {
                        healingtiles.Add(Grid[(int)(tile.GridCoordinates.x)][(int)(tile.GridCoordinates.y + i)].GetComponent<GridSquareScript>());
                    }
                    else
                    {
                        attacktiles.Add(Grid[(int)(tile.GridCoordinates.x)][(int)(tile.GridCoordinates.y + i)].GetComponent<GridSquareScript>());
                    }
                }
            }
        }
        Recolor();
    }

    /// <summary>
    /// Shows attack of an Unit from their current position
    /// </summary>
    /// <param name="range"></param>
    /// <param name="frapperenmelee"></param>
    /// <param name="tile"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public List<GridSquareScript> GetAttack(int range, bool frapperenmelee, GridSquareScript tile, int size = 0)
    {
        List<GridSquareScript> newattacktiles = new List<GridSquareScript>();
        List<GridSquareScript> tilestouse = new List<GridSquareScript>() { tile };
        if (size > 1)
        {
            List<Vector2> addedvectors = new List<Vector2>() { new Vector2(0, 1), new Vector2(-1, 1), new Vector2(-1, 0) };
            foreach (Vector2 vector in addedvectors)
            {
                GridSquareScript newtile = GetTile(tile.GridCoordinates + vector);
                if (!tilestouse.Contains(newtile))
                {
                    tilestouse.Add(newtile);
                }
            }
        }
        foreach (GridSquareScript newtile in tilestouse)
        {
            for (int i = 1; i <= range; i++)
            {
                if (i == 1 && !frapperenmelee)
                {
                    continue;
                }
                if (i > 1)
                {
                    for (int x = 1; x <= i - 1; x++)
                    {
                        for (int y = 1; y <= i - 1; y++)
                        {

                            Vector2 vectorforattack = newtile.GridCoordinates + new Vector2(x, y);
                            if (CheckIfPositionIsLegal(vectorforattack) && !newattacktiles.Contains(GetTile(vectorforattack)))
                            {
                                newattacktiles.Add(GetTile(vectorforattack));
                            }
                            vectorforattack = newtile.GridCoordinates + new Vector2(x, -y);
                            if (CheckIfPositionIsLegal(vectorforattack) && !newattacktiles.Contains(GetTile(vectorforattack)))
                            {
                                newattacktiles.Add(GetTile(vectorforattack));
                            }
                            vectorforattack = newtile.GridCoordinates + new Vector2(-x, y);
                            if (CheckIfPositionIsLegal(vectorforattack) && !newattacktiles.Contains(GetTile(vectorforattack)))
                            {
                                newattacktiles.Add(GetTile(vectorforattack));
                            }
                            vectorforattack = newtile.GridCoordinates + new Vector2(-x, -y);
                            if (CheckIfPositionIsLegal(vectorforattack) && !newattacktiles.Contains(GetTile(vectorforattack)))
                            {
                                newattacktiles.Add(GetTile(vectorforattack));
                            }


                        }
                    }
                }
                if (newtile.GridCoordinates.x >= i)
                {
                    GridSquareScript othertile = GetTile((int)(newtile.GridCoordinates.x - i), (int)(newtile.GridCoordinates.y));
                    if(!newattacktiles.Contains(othertile))
                    {
                        newattacktiles.Add(othertile);
                    }
                    
                }
                if (newtile.GridCoordinates.x < Grid.Count - i)
                {
                    GridSquareScript othertile = GetTile((int)(newtile.GridCoordinates.x + i), (int)(newtile.GridCoordinates.y));
                    if (!newattacktiles.Contains(othertile))
                    {
                        newattacktiles.Add(othertile);
                    }
                }
                if (newtile.GridCoordinates.y >= i)
                {
                    GridSquareScript othertile = GetTile((int)(newtile.GridCoordinates.x), (int)(newtile.GridCoordinates.y - i));
                    if (!newattacktiles.Contains(othertile))
                    {
                        newattacktiles.Add(othertile);
                    }
                }
                if (newtile.GridCoordinates.y < Grid[0].Count - i)
                {
                    GridSquareScript othertile = GetTile((int)(newtile.GridCoordinates.x), (int)(newtile.GridCoordinates.y + i));
                    if (!newattacktiles.Contains(othertile))
                    {
                        newattacktiles.Add(othertile);
                    }
                }
            }
        }
        return newattacktiles;
    }

    public void ShowAttackAfterMovement(int range, bool frapperenmelee, List<GridSquareScript> tiles, bool usingstaff, int size)
    {
        movementtiles.Clear();
        attacktiles = new List<GridSquareScript>();
        lockedattacktiles = new List<GridSquareScript>();
        lockedhealingtiles = new List<GridSquareScript>();
        healingtiles = new List<GridSquareScript>();
        List<GridSquareScript> newtileslist = new List<GridSquareScript>();
        if (size > 1)
        {
            List<Vector2> addedvectors = new List<Vector2>() { new Vector2(0, 1), new Vector2(-1, 1), new Vector2(-1, 0) };
            foreach (GridSquareScript tile in tiles)
            {
                if (!newtileslist.Contains(tile))
                {
                    newtileslist.Add(tile);
                }
                foreach (Vector2 vector in addedvectors)
                {
                    GridSquareScript newtile = GetTile(tile.GridCoordinates + vector);
                    if (!newtileslist.Contains(newtile))
                    {
                        newtileslist.Add(newtile);
                    }
                }

            }
        }
        else
        {
            newtileslist = tiles;
        }
        for (int i = 1; i <= range; i++)
        {
            if (i == 1 && !frapperenmelee)
            {
                continue;
            }
            if (i > 1)
            {
                for (int x = -i; x <= i; x++)
                {
                    for (int y = -i; y <= i; y++)
                    {
                        if (Mathf.Abs(x) + Mathf.Abs(y) == i)
                        {
                            foreach(GridSquareScript tile in newtileslist)
                            {
                                Vector2 vectorforattack = tile.GridCoordinates + new Vector2(x, y);
                                if (CheckIfPositionIsLegal(vectorforattack))
                                {
                                    if (usingstaff)
                                    {
                                        if (!healingtiles.Contains(Grid[(int)(vectorforattack.x)][(int)(vectorforattack.y)].GetComponent<GridSquareScript>()))
                                        {
                                            healingtiles.Add(Grid[(int)(vectorforattack.x)][(int)(vectorforattack.y)].GetComponent<GridSquareScript>());
                                        }
                                    }
                                    else
                                    {
                                        if (!attacktiles.Contains(Grid[(int)(vectorforattack.x)][(int)(vectorforattack.y)].GetComponent<GridSquareScript>()))
                                        {
                                            attacktiles.Add(Grid[(int)(vectorforattack.x)][(int)(vectorforattack.y)].GetComponent<GridSquareScript>());
                                        }

                                    }
                                }
                            }
                            
                        }

                    }
                }
            }
            foreach (GridSquareScript tile in newtileslist)
            {
                if (tile.GridCoordinates.x >= i)
                {
                    if (usingstaff)
                    {
                        if (!healingtiles.Contains(Grid[(int)(tile.GridCoordinates.x - i)][(int)(tile.GridCoordinates.y)].GetComponent<GridSquareScript>()))
                        {
                            healingtiles.Add(Grid[(int)(tile.GridCoordinates.x - i)][(int)(tile.GridCoordinates.y)].GetComponent<GridSquareScript>());
                        }
                    }
                    else
                    {
                        if (!attacktiles.Contains(Grid[(int)(tile.GridCoordinates.x - i)][(int)(tile.GridCoordinates.y)].GetComponent<GridSquareScript>()))
                        {
                            attacktiles.Add(Grid[(int)(tile.GridCoordinates.x - i)][(int)(tile.GridCoordinates.y)].GetComponent<GridSquareScript>());
                        }

                    }

                }
                if (tile.GridCoordinates.x < Grid.Count - i)
                {
                    if (usingstaff)
                    {
                        if (!healingtiles.Contains(Grid[(int)(tile.GridCoordinates.x + i)][(int)(tile.GridCoordinates.y)].GetComponent<GridSquareScript>()))
                        {
                            healingtiles.Add(Grid[(int)(tile.GridCoordinates.x + i)][(int)(tile.GridCoordinates.y)].GetComponent<GridSquareScript>());
                        }
                    }
                    else
                    {
                        if (!attacktiles.Contains(Grid[(int)(tile.GridCoordinates.x + i)][(int)(tile.GridCoordinates.y)].GetComponent<GridSquareScript>()))
                        {
                            attacktiles.Add(Grid[(int)(tile.GridCoordinates.x + i)][(int)(tile.GridCoordinates.y)].GetComponent<GridSquareScript>());
                        }

                    }
                }
                if (tile.GridCoordinates.y >= i)
                {
                    if (usingstaff)
                    {
                        if (!healingtiles.Contains(Grid[(int)(tile.GridCoordinates.x)][(int)(tile.GridCoordinates.y - i)].GetComponent<GridSquareScript>()))
                        {
                            healingtiles.Add(Grid[(int)(tile.GridCoordinates.x)][(int)(tile.GridCoordinates.y - i)].GetComponent<GridSquareScript>());
                        }

                    }
                    else
                    {
                        if (!attacktiles.Contains(Grid[(int)(tile.GridCoordinates.x)][(int)(tile.GridCoordinates.y - i)].GetComponent<GridSquareScript>()))
                        {
                            attacktiles.Add(Grid[(int)(tile.GridCoordinates.x)][(int)(tile.GridCoordinates.y - i)].GetComponent<GridSquareScript>());
                        }
                    }

                }
                if (tile.GridCoordinates.y < Grid[0].Count - i)
                {
                    if (usingstaff)
                    {
                        if (!healingtiles.Contains(Grid[(int)(tile.GridCoordinates.x)][(int)(tile.GridCoordinates.y + i)].GetComponent<GridSquareScript>()))
                        {
                            healingtiles.Add(Grid[(int)(tile.GridCoordinates.x)][(int)(tile.GridCoordinates.y + i)].GetComponent<GridSquareScript>());
                        }

                    }
                    else
                    {
                        if (!attacktiles.Contains(Grid[(int)(tile.GridCoordinates.x)][(int)(tile.GridCoordinates.y + i)].GetComponent<GridSquareScript>()))
                        {
                            attacktiles.Add(Grid[(int)(tile.GridCoordinates.x)][(int)(tile.GridCoordinates.y + i)].GetComponent<GridSquareScript>());
                        }
                    }
                }
            }
        }
        Recolor();
    }

    public void ResetColor()
    {
        for (int x = 0; x < Grid.Count; x++)
        {
            for (int y = 0; y < Grid[x].Count; y++)
            {
                if (Grid[x][y].GetComponent<GridSquareScript>().isobstacle && !lockedattacktiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()) && !lockedmovementtiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()) && !lockedhealingtiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()))
                {
                    Grid[x][y].GetComponent<GridSquareScript>().fillwithGrey();
                }
                else if (!lockedattacktiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()) && !lockedmovementtiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()) && !lockedhealingtiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()))
                {
                    Grid[x][y].GetComponent<GridSquareScript>().fillwithNothing();
                }

            }
        }
    }

    public void Recolor()
    {
        for (int x = 0; x < Grid.Count; x++)
        {
            for (int y = 0; y < Grid[x].Count; y++)
            {
                if (Grid[x][y].GetComponent<GridSquareScript>().isobstacle && !lockedattacktiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()) && !lockedmovementtiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()) && !lockedhealingtiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()))
                {
                    Grid[x][y].GetComponent<GridSquareScript>().fillwithGrey();
                }
                else if (!lockedattacktiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()) && !lockedmovementtiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()) && !lockedhealingtiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()))
                {
                    Grid[x][y].GetComponent<GridSquareScript>().fillwithNothing();
                }

            }
        }

        foreach (GridSquareScript tile in dangerousTiles)
        {
            tile.fillwithPurple();
        }

        if (!lockselection)
        {
            foreach (GridSquareScript gridSquareScript in attacktiles)
            {
                gridSquareScript.fillwithRed();
            }
            foreach (GridSquareScript gridSquareScript in healingtiles)
            {
                gridSquareScript.fillwithGreen();
            }
            foreach (GridSquareScript gridSquareScript in movementtiles)
            {
                gridSquareScript.fillwithblue();
            }

        }

        foreach (GridSquareScript gridSquareScript in lockedattacktiles)
        {
            gridSquareScript.fillwithRed();
        }
        foreach (GridSquareScript gridSquareScript in lockedhealingtiles)
        {
            gridSquareScript.fillwithGreen();
        }

        foreach (GridSquareScript gridSquareScript in lockedmovementtiles)
        {
            gridSquareScript.fillwithblue();
        }


    }

    public int findshortestpath(GridSquareScript starttile, GridSquareScript targettile, int maxDeplacements)
    {
        int rows = Grid.Count;
        int cols = Grid[0].Count;
        bool[,] visited = new bool[rows, cols];
        Queue<(int x, int y, int distance)> queue = new Queue<(int, int, int)>();
        int[][] directions = new int[][]
        {
            new int[] {-1, 0}, // haut
            new int[] {1, 0},  // bas
            new int[] {0, -1}, // gauche
            new int[] {0, 1}   // droite
        };

        queue.Enqueue(((int)starttile.GridCoordinates.x, (int)starttile.GridCoordinates.y, 0));
        visited[(int)starttile.GridCoordinates.x, (int)starttile.GridCoordinates.y] = true;

        while (queue.Count > 0)
        {
            var (x, y, distance) = queue.Dequeue();

            if (distance > maxDeplacements)
            {
                continue; // Ignore les chemins qui d�passent la limite
            }


            if ((x, y) == ((int)targettile.GridCoordinates.x, (int)targettile.GridCoordinates.y))
            {
                return distance;
            }

            foreach (var dir in directions)
            {
                int nx = x + dir[0];
                int ny = y + dir[1];

                if (nx >= 0 && nx < rows && ny >= 0 && ny < cols)
                {
                    if (!visited[nx, ny] && !Grid[nx][ny].GetComponent<GridSquareScript>().isobstacle)
                    {
                        visited[nx, ny] = true;
                        if (Grid[nx][ny].GetComponent<GridSquareScript>().type.ToLower() == "fire" || Grid[nx][ny].GetComponent<GridSquareScript>().type.ToLower() == "water")
                        {
                            queue.Enqueue((nx, ny, distance + 2));
                        }
                        else
                        {
                            queue.Enqueue((nx, ny, distance + 1));
                        }

                    }
                }
            }

        }
        return -1;
    }

    private void SpreadMovements(Vector2 Coordinates, int remainingMovements, List<GridSquareScript> tilestolight, GameObject selectedunit,Dictionary<GridSquareScript, int> visited)
    {
        GridSquareScript tile = GetTile((int)Coordinates.x, (int)Coordinates.y);
        if (tile == null) return;

        // already visited with equal or more moves left -> no need to continue
        if (visited.ContainsKey(tile) && visited[tile] >= remainingMovements)
            return;

        // record the best remaining moves we've seen for this tile
        visited[tile] = remainingMovements;

        // adjust cost for terrain
        string tiletype = tile.type.ToLower();
        if (tiletype == "fire" || tiletype == "water")
            remainingMovements -= 1;

        if (!tilestolight.Contains(tile))
            tilestolight.Add(tile);

        if (remainingMovements <= 0) return;

        // neighbor directions
        Vector2[] dirs = {
        new Vector2(-1, 0), new Vector2(1, 0),
        new Vector2(0, -1), new Vector2(0, 1)
    };

        foreach (var dir in dirs)
        {
            Vector2 newpos = Coordinates + dir;

            if (newpos.x < 0 || newpos.x >= Grid.Count || newpos.y < 0 || newpos.y >= Grid[0].Count)
                continue;

            if (CheckIfFree(newpos, selectedunit.GetComponent<UnitScript>().UnitCharacteristics))
            {
                GridSquareScript newtile = GetTile(newpos);

                int cost = 1;
                if (newtile.elevation > tile.elevation)
                    cost += newtile.elevation - tile.elevation;

                SpreadMovements(newpos, remainingMovements - cost, tilestolight, selectedunit, visited);
            }
        }
    }


    public bool CheckIfFree(Vector2 position, Character selectedunit)
    {

        if(!CheckIfPositionIsLegal(position))
        {
            return false;
        }
        if (Grid[(int)position.x][(int)position.y].GetComponent<GridSquareScript>().isobstacle)
        {
            return false;

        }
        foreach (Character unit in allunits)
        {
            if (unit.position == position && unit.affiliation != selectedunit.affiliation && unit.currentHP > 0)
            {
                return false;
            }
        }
        if(selectedunit.enemyStats.monsterStats.size > 1)
        {
            List<Vector2> otherpositions = new List<Vector2>() { position + new Vector2(-1, 0), position + new Vector2(-1, 1), position + new Vector2(0, 1) };
            foreach (Character unit in allunits)
            {
                foreach(Vector2 newposition in otherpositions)
                {
                    GridSquareScript newtile = GetTile(newposition);
                    if(!CheckIfPositionIsLegal(newposition, selectedunit.enemyStats.monsterStats.size))
                    {
                        return false;
                    }
                    if(unit.currentTile.Contains(newtile) && unit!=selectedunit && unit.affiliation!=selectedunit.affiliation)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    public bool CheckIfPositionIsLegal(Vector2 position, int size = 0)
    {
        if (position.x < 0 || position.x >= Grid.Count || position.y < 0 || position.y >= Grid[0].Count)
        {
            return false;
        }
        else if (!GetTile(position).activated)
        {
            return false ;
        }
        if(size > 0)
        {
            if(!(CheckIfPositionIsLegal(position + new Vector2(-1,0)) && CheckIfPositionIsLegal(position + new Vector2(0, 1)) && CheckIfPositionIsLegal(position + new Vector2(-1, 1)) ))
            {
                return false;
            }
        }
        return true;
    }

    public bool checkifvalidpos(List<GridSquareScript> tilelist, Vector2 position, GameObject currentunit)
    {

        foreach (GridSquareScript tile in tilelist)
        {
            if ((int)tile.GridCoordinates.x == (int)position.x && (int)tile.GridCoordinates.y == (int)position.y && (GetUnit(tile) == null || GetUnit(tile) == currentunit))
            {
                return true;
            }
        }

        return false;

    }
    public GridSquareScript GetTile(int x, int y)
    {
        if (x <= Grid.Count - 1 && x >= 0 && y <= Grid[0].Count - 1 && y >= 0)
        {
            return Grid[x][y].GetComponent<GridSquareScript>();
        }
        return null;
    }

    public GridSquareScript GetTile(Vector2 position)
    {
        int x = (int)position.x;
        int y = (int)position.y;
        return GetTile(x, y);
    }

    public GridSquareScript GetFirstClosestTile(Vector2 pos)
    {
        for(int x=0; x<Grid.Count; x++)
        {
            for(int y=0; y<Grid[x].Count; y++)
            {
                if(x==0 && y==0)
                {
                    continue;
                }
                List<Vector2> list = new List<Vector2>() { new Vector2((int)(x + pos.x), (int)(y + pos.y)) , new Vector2((int)(x + pos.x), (int)(pos.y - y)), new Vector2((int)(pos.x + x), (int)(y + pos.y)), new Vector2((int)(pos.x - x), (int)(pos.y - y)) };
                foreach(Vector2 newpos in list)
                {
                    
                    if (CheckIfPositionIsLegal(newpos))
                    {
                        GridSquareScript Tile = Grid[(int)newpos.x][(int)newpos.y].GetComponent<GridSquareScript>();
                        if (!Tile.isobstacle && GetUnit(Tile) == null && newpos!=pos)
                        {
                            return Grid[(int)newpos.x][(int)newpos.y].GetComponent<GridSquareScript>();
                        }
                    }
                }
               

            }
        }
        return null;
    }

    public List<Vector2> FindPath(Vector2 start, Vector2 goal, Character unit)
    {
        List<Node> open = new List<Node>();
        HashSet<Vector2> closed = new HashSet<Vector2>();

        Node startNode = new Node(start, null, 0, Manhattandistance(start, goal));
        open.Add(startNode);

        while (open.Count > 0)
        {
            // 1. Get node with lowest F cost
            Node current = open.OrderBy(n => n.F).First();

            // 2. If goal reached, reconstruct path
            if (current.Position == goal)
                return ReconstructPath(current);

            // 3. Move from open → closed
            open.Remove(current);
            closed.Add(current.Position);

            // 4. Check neighbors
            foreach (Vector2 neighborPos in GetNeighbors(current.Position))
            {
                if (closed.Contains(neighborPos))
                    continue;

                GridSquareScript neighborTile = GetTile((int)neighborPos.x, (int)neighborPos.y);
                if (neighborTile == null)
                    continue;

                if (!CheckIfFree(neighborPos, unit))
                    continue;

                GridSquareScript currentTile = GetTile((int)current.Position.x, (int)current.Position.y);

                int cost = 1;
                if (neighborTile.elevation > currentTile.elevation)
                    cost += neighborTile.elevation - currentTile.elevation;

                float tentativeG = current.G + cost;

                Node existing = open.FirstOrDefault(n => n.Position == neighborPos);
                if (existing == null)
                {
                    Node neighborNode = new Node(
                        neighborPos,
                        current,
                        tentativeG,
                        Manhattandistance(neighborPos, goal)
                    );
                    open.Add(neighborNode);
                }
                else if (tentativeG < existing.G)
                {
                    existing.G = tentativeG;
                    existing.Parent = current;
                }
            }
        }

        // no path found
        return new List<Vector2>();
    }

    private float Manhattandistance(Vector2 a, Vector2 b)
    {
        // Manhattan distance (good for 4-directional grids)
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private IEnumerable<Vector2> GetNeighbors(Vector2 pos)
    {
        Vector2[] dirs = {
        new Vector2(-1, 0),
        new Vector2(1, 0),
        new Vector2(0, -1),
        new Vector2(0, 1)
    };

        foreach (var dir in dirs)
        {
            Vector2 np = pos + dir;
            if (np.x >= 0 && np.x < Grid.Count && np.y >= 0 && np.y < Grid[0].Count)
                yield return np;
        }
    }

    private List<Vector2> ReconstructPath(Node node)
    {
        List<Vector2> path = new List<Vector2>();
        while (node != null)
        {
            path.Add(node.Position);
            node = node.Parent;
        }
        path.Reverse();
        return path;
    }

}
