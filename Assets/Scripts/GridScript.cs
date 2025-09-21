using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
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
    private SkillEditionScript SkillEditionScript;

    private TextBubbleScript textBubble;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lockedmovementtiles = new List<GridSquareScript>();
        lockedattacktiles = new List<GridSquareScript>();
        lockedhealingtiles = new List<GridSquareScript>();
        InitializeGOList();
        textBubble = FindAnyObjectByType<TextBubbleScript>();

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
                previousmovevalue = inputManager.movementValue;
                MoveSelection(previousmovevalue);
            }
            else
            {
                previousmovevalue = Vector2.zero;
            }

            if (inputManager.NextWeaponjustpressed || inputManager.PreviousWeaponjustpressed)
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
                    else if (!SkillEditionScript.gameObject.activeSelf)
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

        previousmovementvalueforbuffer = inputManager.movementValue;
        UpdateTileText();
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
        }
        tiletext.text = text;
    }

    public void MoveSelection(Vector2 input)
    {
        if (selection == null)
        {
            selection = Grid[0][0].GetComponent<GridSquareScript>();
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
        foreach (GameObject unitGO in allunitGOs)
        {
            if (unitGO != null)
            {
                Character unit = unitGO.GetComponent<UnitScript>().UnitCharacteristics;
                if (unit.position == selection.GridCoordinates && !unit.alreadymoved)
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
                    SpreadMovements(unit.position, movements, movementtiles, unitGO);
                    (int range, bool melee, string type) = unitGO.GetComponent<UnitScript>().GetRangeMeleeAndType();
                    ShowAttack(range, melee, type.ToLower() == "staff");
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
        foreach (GameObject unitGO in allunitGOs)
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
                SpreadMovements(unit.position, movements, movementtiles, unitGO);
                (int range, bool melee, string type) = unitGO.GetComponent<UnitScript>().GetRangeMeleeAndType();
                ShowAttack(range, melee, type.ToLower() == "staff");
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
        Character unitchar = unit.GetComponent<UnitScript>().UnitCharacteristics;
        string tiletype = GetTile((int)unitchar.position.x, (int)unitchar.position.y).type;
        int movements = remainingmovements;
        if (tiletype.ToLower() == "fire" || tiletype.ToLower() == "water") //checking if movement reducing effect
        {
            movements -= 1;
        }
        SpreadMovements(unitchar.position, movements, movementtiles, unit);
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
            if (unit.GetComponent<UnitScript>().UnitCharacteristics.position == tile.GridCoordinates)
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
                if (unit.GetComponent<UnitScript>().UnitCharacteristics.position == selection.GridCoordinates)
                {
                    SelectedUnit = unit;
                    break;
                }
            }

        }
        return SelectedUnit;
    }

    public void ShowAttack(int range, bool frapperenmelee, bool usingstaff, bool uselockedmovementtile = false)
    {
        attacktiles = new List<GridSquareScript>();
        healingtiles = new List<GridSquareScript>();
        List<GridSquareScript> tilestouse = new List<GridSquareScript>();
        if (uselockedmovementtile)
        {
            tilestouse = lockedmovementtiles;
        }
        else
        {
            tilestouse = movementtiles;
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
        if (!lockselection)
        {
            foreach (GridSquareScript gridSquareScript in attacktiles)
            {
                gridSquareScript.fillwithRed();
            }
        }

        foreach (GridSquareScript gridSquareScript in lockedattacktiles)
        {
            gridSquareScript.fillwithRed();
        }

        if (!lockselection)
        {
            foreach (GridSquareScript gridSquareScript in healingtiles)
            {
                gridSquareScript.fillwithGreen();
            }
        }

        foreach (GridSquareScript gridSquareScript in lockedhealingtiles)
        {
            gridSquareScript.fillwithGreen();
        }
    }


    public List<GridSquareScript> GetAttack(int range, bool frapperenmelee, GridSquareScript tile)
    {
        List<GridSquareScript> newattacktiles = new List<GridSquareScript>();
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
                            newattacktiles.Add(Grid[(int)(vectorforattack.x)][(int)(vectorforattack.y)].GetComponent<GridSquareScript>());
                        }
                        vectorforattack = tile.GridCoordinates + new Vector2(x, -y);
                        if (CheckIfPositionIsLegal(vectorforattack))
                        {
                            newattacktiles.Add(Grid[(int)(vectorforattack.x)][(int)(vectorforattack.y)].GetComponent<GridSquareScript>());
                        }
                        vectorforattack = tile.GridCoordinates + new Vector2(-x, y);
                        if (CheckIfPositionIsLegal(vectorforattack))
                        {
                            newattacktiles.Add(Grid[(int)(vectorforattack.x)][(int)(vectorforattack.y)].GetComponent<GridSquareScript>());
                        }
                        vectorforattack = tile.GridCoordinates + new Vector2(-x, -y);
                        if (CheckIfPositionIsLegal(vectorforattack))
                        {
                            newattacktiles.Add(Grid[(int)(vectorforattack.x)][(int)(vectorforattack.y)].GetComponent<GridSquareScript>());
                        }
                    }
                }
            }
            if (tile.GridCoordinates.x >= i)
            {
                newattacktiles.Add(Grid[(int)(tile.GridCoordinates.x - i)][(int)(tile.GridCoordinates.y)].GetComponent<GridSquareScript>());
            }
            if (tile.GridCoordinates.x < Grid.Count - i)
            {
                newattacktiles.Add(Grid[(int)(tile.GridCoordinates.x + i)][(int)(tile.GridCoordinates.y)].GetComponent<GridSquareScript>());
            }
            if (tile.GridCoordinates.y >= i)
            {
                newattacktiles.Add(Grid[(int)(tile.GridCoordinates.x)][(int)(tile.GridCoordinates.y - i)].GetComponent<GridSquareScript>());
            }
            if (tile.GridCoordinates.y < Grid[0].Count - i)
            {
                newattacktiles.Add(Grid[(int)(tile.GridCoordinates.x)][(int)(tile.GridCoordinates.y + i)].GetComponent<GridSquareScript>());
            }
        }
        return newattacktiles;
    }

    public void ShowAttackAfterMovement(int range, bool frapperenmelee, GridSquareScript tile, bool usingstaff)
    {
        movementtiles.Clear();
        attacktiles = new List<GridSquareScript>();
        lockedattacktiles = new List<GridSquareScript>();
        lockedhealingtiles = new List<GridSquareScript>();
        healingtiles = new List<GridSquareScript>();
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

    private void SpreadMovements(Vector2 Coordinates, int remainingMovements, List<GridSquareScript> tilestolight, GameObject selectedunit)
    {
        GridSquareScript tile = GetTile((int)Coordinates.x, (int)Coordinates.y);
        string tiletype = tile.type;
        if (tiletype.ToLower() == "fire" || tiletype.ToLower() == "water")
        {
            remainingMovements -= 1;
        }
        if (!tilestolight.Contains(Grid[(int)Coordinates.x][(int)Coordinates.y].GetComponent<GridSquareScript>()))
        {
            tilestolight.Add(Grid[(int)Coordinates.x][(int)Coordinates.y].GetComponent<GridSquareScript>());
        }
        if (remainingMovements > 0)
        {
            if (Coordinates.x > 0)
            {
                if (CheckIfFree(new Vector2(Coordinates.x - 1, Coordinates.y), selectedunit.GetComponent<UnitScript>().UnitCharacteristics))
                {
                    Vector2 newpos = new Vector2(Coordinates.x - 1, Coordinates.y);
                    GridSquareScript newtile = GetTile(newpos);
                    if (newtile.elevation > tile.elevation)
                    {
                        SpreadMovements(newpos, remainingMovements - 1 + newtile.elevation - tile.elevation, tilestolight, selectedunit);
                    }
                    else
                    {
                        SpreadMovements(newpos, remainingMovements - 1, tilestolight, selectedunit);
                    }

                }
            }
            if (Coordinates.x < Grid.Count - 1)
            {
                if (CheckIfFree(new Vector2(Coordinates.x + 1, Coordinates.y), selectedunit.GetComponent<UnitScript>().UnitCharacteristics))
                {
                    Vector2 newpos = new Vector2(Coordinates.x + 1, Coordinates.y);
                    GridSquareScript newtile = GetTile(newpos);
                    if (newtile.elevation > tile.elevation)
                    {
                        SpreadMovements(newpos, remainingMovements - 1 + newtile.elevation - tile.elevation, tilestolight, selectedunit);
                    }
                    else
                    {
                        SpreadMovements(newpos, remainingMovements - 1, tilestolight, selectedunit);
                    }

                }
            }
            if (Coordinates.y > 0)
            {
                if (CheckIfFree(new Vector2(Coordinates.x, Coordinates.y - 1), selectedunit.GetComponent<UnitScript>().UnitCharacteristics))
                {
                    Vector2 newpos = new Vector2(Coordinates.x, Coordinates.y - 1);
                    GridSquareScript newtile = GetTile(newpos);
                    if (newtile.elevation > tile.elevation)
                    {
                        SpreadMovements(newpos, remainingMovements - 1 + newtile.elevation - tile.elevation, tilestolight, selectedunit);
                    }
                    else
                    {
                        SpreadMovements(newpos, remainingMovements - 1, tilestolight, selectedunit);
                    }
                }
            }
            if (Coordinates.y < Grid[0].Count - 1)
            {
                if (CheckIfFree(new Vector2(Coordinates.x, Coordinates.y + 1), selectedunit.GetComponent<UnitScript>().UnitCharacteristics))
                {
                    Vector2 newpos = new Vector2(Coordinates.x, Coordinates.y + 1);
                    GridSquareScript newtile = GetTile(newpos);
                    if (newtile.elevation > tile.elevation)
                    {
                        SpreadMovements(newpos, remainingMovements - 1 + newtile.elevation - tile.elevation, tilestolight, selectedunit);
                    }
                    else
                    {
                        SpreadMovements(newpos, remainingMovements - 1, tilestolight, selectedunit);
                    }
                }

            }
        }
    }

    public bool CheckIfFree(Vector2 position, Character selectedunit)
    {

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

        return true;
    }

    public bool CheckIfPositionIsLegal(Vector2 position)
    {
        if (position.x < 0 || position.x >= Grid.Count || position.y < 0 || position.y >= Grid[0].Count)
        {
            return false;
        }
        else if (!GetTile(position).activated)
        {
            return false ;
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
}
