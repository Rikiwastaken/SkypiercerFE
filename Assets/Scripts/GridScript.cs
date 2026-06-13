using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static UnitScript;
public class GridScript : MonoBehaviour
{

    public static GridScript instance;

    public GameObject GridSquarePrefab;

    public List<List<GameObject>> Grid;

    public Vector2 GridDimensions;

    public GridSquareScript selection;

    public GridSquareScript previousselection;

    private Vector2 previousmovevalue;

    public List<Character> allunits;
    public List<GameObject> allunitGOs;

    public List<GridSquareScript> movementtiles;
    public List<GridSquareScript> attacktiles;
    public List<GridSquareScript> healingtiles;
    public List<GridSquareScript> lockedmovementtiles;
    public List<GridSquareScript> lockedattacktiles;
    public List<GridSquareScript> lockedhealingtiles;

    public List<GridSquareScript> DangerousTiles;

    public GridSquareScript lastSquare;

    private int moveCD;

    public bool lockselection;

    public GameObject actionsMenu;

    public int movementbuffercounter;
    public float movementbuffer;

    private Vector2 previousmovementvalueforbuffer;

    public TextMeshProUGUI tiletext;
    public TextMeshProUGUI Elevationtiletext;
    public GameObject TileImageContainer;
    private SkillEditionScript SkillEditionScript;

    private TextBubbleScript textBubble;

    private cameraScript battlecamera;
    public GameObject NeutralMenu;

    public GameObject ForesightMenu;
    public GameObject TutorialWindowMenu;

    public GameObject MapModel;

    private int mapchangecnt;

    private bool ShowDangerousTiles;

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

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        if (DataScript.instance == null)
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
        textBubble = GetComponent<ActionManager>().TextBubbleScript;
        battlecamera = FindAnyObjectByType<cameraScript>();

    }

    private void Update()
    {

        if (SkillEditionScript == null)
        {
            SkillEditionScript = FindAnyObjectByType<SkillEditionScript>(FindObjectsInactive.Include);
        }

        if (InputSystem.actions.FindAction("ShowDangerousTiles").WasPressedThisFrame())
        {
            ShowDangerousTiles = !ShowDangerousTiles;
            Recolor();
        }

        if (moveCD > 0)
        {
            moveCD--;
        }

        if (movementbuffercounter > 0)
        {
            movementbuffercounter--;
        }
        Vector2 movement = InputSystem.actions.FindAction("Movement").ReadValue<Vector2>();
        if (movement == Vector2.zero)
        {
            movementbuffercounter = 0;
        }

        if (moveCD <= 0 && !actionsMenu.activeSelf && (GetComponent<TurnManger>().currentlyplaying == "playable" || GetComponent<TurnManger>().currentlyplaying == "tutorial" || GetComponent<TurnManger>().currentlyplaying == "") && movementbuffercounter <= 0 && !textBubble.indialogue && !NeutralMenu.activeSelf && !ForesightMenu.activeSelf && !TutorialWindowMenu.activeSelf && !(GameOverScript.instance != null && GameOverScript.instance.gameObject.activeSelf))
        {


            if (movement != Vector2.zero && movement != previousmovevalue)
            {
                moveCD = (int)(0.1f / Time.deltaTime);
                previousmovevalue = battlecamera.DetermineDirection(movement);
                MoveSelection(previousmovevalue);
            }
            else
            {
                previousmovevalue = Vector2.zero;
            }

            if ((InputSystem.actions.FindAction("NextWeapon").WasPressedThisFrame() || InputSystem.actions.FindAction("PreviousWeapon").WasPressedThisFrame()) && (GetComponent<TurnManger>().currentlyplaying == "playable" || GetComponent<TurnManger>().currentlyplaying == "tutorial") && !lockselection)
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
                    else if (SkillEditionScript != null)
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
                            if (InputSystem.actions.FindAction("NextWeapon").WasPressedThisFrame())
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
                ActionManager.instance.frameswherenotlock = 5;
            }

        }


        if (previousmovementvalueforbuffer != movement && previousmovementvalueforbuffer == Vector2.zero)
        {
            movementbuffercounter = (int)(movementbuffer / Time.fixedDeltaTime);
        }

        previousmovementvalueforbuffer = battlecamera.DetermineDirection(movement);
        UpdateTileText();
        if (lockselection && movementtiles.Count == 0 && lockedmovementtiles.Count == 0 && attacktiles.Count == 0 && lockedattacktiles.Count == 0 && healingtiles.Count == 0 && lockedhealingtiles.Count == 0)
        {
            lockselection = false;
        }

        if (selection != previousselection)
        {
            if (mapchangecnt == 0)
            {
                mapchangecnt = (int)(0.5f / Time.timeScale);
                MinimapScript.instance.UpdateMinimap();
                previousselection = selection;
            }
            else
            {
                mapchangecnt--;
            }

        }
    }

    public void InstantiateGrid()
    {
        Grid = new List<List<GameObject>>();
        GridSquareScript[] tilelist = FindObjectsByType<GridSquareScript>(FindObjectsSortMode.None);
        if (lastSquare == null)
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

    public void CalculateDangerousTiles()
    {
        DangerousTiles = new List<GridSquareScript>();

        foreach (GameObject unit in allunitGOs)
        {
            Character UnitChar = unit.GetComponent<UnitScript>().UnitCharacteristics;
            if (UnitChar.currentTile == null || !UnitChar.currentTile.activated)
            {
                continue;
            }
            if (UnitChar.affiliation.ToLower() != "enemy")
            {
                continue;
            }

            ShowMovementOfUnit(unit, false);

            (int range, bool melee) = unit.GetComponent<UnitScript>().GetRangeAndMele();

            foreach (GridSquareScript tile in movementtiles)
            {
                List<GridSquareScript> newattacktiles = GetAttack(range, melee, tile, UnitChar);

                foreach (GridSquareScript attacktile in newattacktiles)
                {
                    if (!DangerousTiles.Contains(attacktile))
                    {
                        DangerousTiles.Add(attacktile);
                    }
                }
            }
        }

    }

    private void UpdateTileText()
    {
        string text = "";

        if (selection != null)
        {
            if (selection.Mechanism != null && selection.Mechanism.type != 0)
            {
                switch (selection.Mechanism.type)
                {
                    case (1):
                        if (selection.Mechanism.isactivated)
                        {
                            text += "Open Door\nNo effect";
                        }
                        else
                        {
                            text += "Closed Door\nNo effect";
                        }
                        break;
                    case (2):
                        if (selection.Mechanism.isactivated)
                        {
                            text += "Activated Mechanism\nNo effect";
                        }
                        else
                        {
                            text += "Mechanism\nNo effect";
                        }
                        break;
                }
            }
            else if (selection.isobstacle)
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
                text = "Forest \n+20% Dodge";
            }
            if (selection.type.ToLower() == "ruins")
            {
                text = "Ruins \n+10% Dodge\n+10% Hit";
            }
            if (selection.type.ToLower() == "fire")
            {
                text = "Fire \n-1 mvt\n-10% Def/Res\n-33% HP";
            }
            if (selection.type.ToLower() == "water")
            {
                text = "Water \n-1 mvt\n-20% Dodge";
            }
            if (selection.type.ToLower() == "medicinalwater")
            {
                text = "Medicinal Water \n+50% HP";
            }
            if (selection.type.ToLower() == "fortification")
            {
                text = "Fortification \n+5% Dodge\n+15% Hit\n+10% HP";
            }
            if (selection.type.ToLower() == "fog")
            {
                text = "Fog \n+20% Dodge\n-20% Hit";
            }
            if (selection.type.ToLower() == "desert")
            {
                text = "Desert \n-10% HP";
            }
            Elevationtiletext.text = "Elevation : " + selection.elevation;
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
        else if (tile.RemainingSunTurns > 0)
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
            battlecamera.transform.position = new Vector3(selection.GridCoordinates.x, battlecamera.transform.position.y, selection.GridCoordinates.y);
        }
        else
        {
            if (input.x > 0 && (int)selection.GridCoordinates.x < Grid.Count - 1)
            {
                GridSquareScript newtile = Grid[(int)(selection.GridCoordinates.x) + 1][(int)(selection.GridCoordinates.y)].GetComponent<GridSquareScript>();
                if (newtile.activated)
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

        Transform unitholder = GameObject.Find("Characters").transform;


        for (int i = 0; i < unitholder.childCount; i++)
        {
            if (unitholder.GetChild(i).gameObject.activeSelf)
            {
                UnitScript character = unitholder.GetChild(i).GetComponent<UnitScript>();
                allunits.Add(character.UnitCharacteristics);
                allunitGOs.Add(character.gameObject);
            }
        }
        GetComponent<TurnManger>().InitializeUnitLists(allunitGOs);
        BondVisualsScript.instance.InitializeBondList();
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
                if (unit.currentTile == selection)
                {
                    int movements = unitGO.GetComponent<UnitScript>().CalculateNumberOfMovements();
                    SpreadMovements(unit.position, movements, movementtiles, unitGO, new Dictionary<GridSquareScript, int>());
                    (int range, bool melee, string type) = unitGO.GetComponent<UnitScript>().GetRangeMeleeAndType();
                    ShowAttack(range, melee, type.ToLower() == "staff", false, unit);
                }
            }


        }
        Recolor();
    }



    public void ShowMovementOfUnit(GameObject Target, bool color = true)
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
                    int movements = unitGO.GetComponent<UnitScript>().CalculateNumberOfMovements();
                    SpreadMovements(unit.position, movements, movementtiles, unitGO, new Dictionary<GridSquareScript, int>());
                    (int range, bool melee, string type) = unitGO.GetComponent<UnitScript>().GetRangeMeleeAndType();
                    ShowAttack(range, melee, type.ToLower() == "staff", false, unit);

                }
            }


        }
        if (color)
        {
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
            if (unit.GetComponent<UnitScript>().UnitCharacteristics.currentTile != null && unit.GetComponent<UnitScript>().UnitCharacteristics.currentTile == tile)
            {
                return unit;

            }
            else if (unit.GetComponent<UnitScript>().UnitCharacteristics.position == tile.GridCoordinates)
            {
                return unit;
            }
        }
        return null;
    }

    public GameObject GetUnit(string name)
    {
        foreach (GameObject unit in allunitGOs)
        {
            if (unit.Equals(null) || unit == null)
            {
                continue;
            }
            if (unit.GetComponent<UnitScript>().UnitCharacteristics.name.ToLower() == name.ToLower())
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
                if (unit.GetComponent<UnitScript>().UnitCharacteristics.currentTile == selection)
                {
                    SelectedUnit = unit;
                    break;
                }
            }

        }
        return SelectedUnit;
    }

    public void ShowAttack(int range, bool frapperenmelee, bool usingstaff, bool uselockedmovementtile = false, Character attacker = null)
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

                            int distance = Mathf.Abs(x) + Mathf.Abs(y);

                            if (distance > i)
                                continue;

                            if (distance == 0)
                                continue;


                            List<Vector2> Attackvectors = new List<Vector2>()
                            {
                                tile.GridCoordinates + new Vector2(x, y),
                                tile.GridCoordinates + new Vector2(x, -y),
                                tile.GridCoordinates + new Vector2(-x, y),
                                tile.GridCoordinates + new Vector2(-x, -y)
                            };

                            foreach (Vector2 vectorforattack in Attackvectors)
                            {
                                if (CheckIfPositionIsLegal(vectorforattack))
                                {
                                    if (usingstaff)
                                    {
                                        AddIfNotPresent(healingtiles, Grid[(int)(vectorforattack.x)][(int)(vectorforattack.y)].GetComponent<GridSquareScript>());
                                    }
                                    else
                                    {
                                        AddIfNotPresent(attacktiles, Grid[(int)(vectorforattack.x)][(int)(vectorforattack.y)].GetComponent<GridSquareScript>());
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
                        AddIfNotPresent(healingtiles, Grid[(int)(tile.GridCoordinates.x - i)][(int)(tile.GridCoordinates.y)].GetComponent<GridSquareScript>());
                    }
                    else
                    {
                        AddIfNotPresent(attacktiles, Grid[(int)(tile.GridCoordinates.x - i)][(int)(tile.GridCoordinates.y)].GetComponent<GridSquareScript>());
                    }
                }
                if (tile.GridCoordinates.x < Grid.Count - i)
                {
                    if (usingstaff)
                    {
                        AddIfNotPresent(healingtiles, Grid[(int)(tile.GridCoordinates.x + i)][(int)(tile.GridCoordinates.y)].GetComponent<GridSquareScript>());
                    }
                    else
                    {
                        AddIfNotPresent(attacktiles, Grid[(int)(tile.GridCoordinates.x + i)][(int)(tile.GridCoordinates.y)].GetComponent<GridSquareScript>());
                    }
                }
                if (tile.GridCoordinates.y >= i)
                {
                    if (usingstaff)
                    {
                        AddIfNotPresent(healingtiles, Grid[(int)(tile.GridCoordinates.x)][(int)(tile.GridCoordinates.y - i)].GetComponent<GridSquareScript>());

                    }
                    else
                    {
                        AddIfNotPresent(attacktiles, Grid[(int)(tile.GridCoordinates.x)][(int)(tile.GridCoordinates.y - i)].GetComponent<GridSquareScript>());

                    }

                }
                if (tile.GridCoordinates.y < Grid[0].Count - i)
                {
                    if (usingstaff)
                    {
                        AddIfNotPresent(healingtiles, Grid[(int)(tile.GridCoordinates.x)][(int)(tile.GridCoordinates.y + i)].GetComponent<GridSquareScript>());

                    }
                    else
                    {
                        AddIfNotPresent(attacktiles, Grid[(int)(tile.GridCoordinates.x)][(int)(tile.GridCoordinates.y + i)].GetComponent<GridSquareScript>());

                    }
                }
            }
        }

        if (attacker != null)
        {
            if (attacktiles.Contains(attacker.currentTile))
            {
                attacktiles.Remove(attacker.currentTile);
            }
            if (healingtiles.Contains(attacker.currentTile))
            {
                healingtiles.Remove(attacker.currentTile);
            }
        }
        Recolor();
    }

    private void AddIfNotPresent(List<GridSquareScript> list, GridSquareScript item)
    {
        if (!list.Contains(item))
        {
            list.Add(item);
        }
    }


    /// <summary>
    /// Shows attack of an Unit from their current position
    /// </summary>
    /// <param name="range"></param>
    /// <param name="frapperenmelee"></param>
    /// <param name="tile"></param>

    /// <param name="unit"></param>
    /// <returns></returns>
    public List<GridSquareScript> GetAttack(int range, bool frapperenmelee, GridSquareScript tile, Character unit = null)
    {
        List<GridSquareScript> newattacktiles = new List<GridSquareScript>();
        List<GridSquareScript> tilestouse = new List<GridSquareScript>() { tile };

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
                            if (Mathf.Abs(x) + Mathf.Abs(y) == i)
                            {
                                Vector2 vectorforattack = tile.GridCoordinates + new Vector2(x, y);
                                if (CheckIfPositionIsLegal(vectorforattack))
                                {
                                    AddIfNotPresent(newattacktiles, Grid[(int)(vectorforattack.x)][(int)(vectorforattack.y)].GetComponent<GridSquareScript>());
                                }


                            }


                        }
                    }
                }
                if (newtile.GridCoordinates.x >= i)
                {
                    GridSquareScript othertile = GetTile((int)(newtile.GridCoordinates.x - i), (int)(newtile.GridCoordinates.y));
                    AddIfNotPresent(newattacktiles, othertile);


                }
                if (newtile.GridCoordinates.x < Grid.Count - i)
                {
                    GridSquareScript othertile = GetTile((int)(newtile.GridCoordinates.x + i), (int)(newtile.GridCoordinates.y));
                    AddIfNotPresent(newattacktiles, othertile);
                }
                if (newtile.GridCoordinates.y >= i)
                {
                    GridSquareScript othertile = GetTile((int)(newtile.GridCoordinates.x), (int)(newtile.GridCoordinates.y - i));
                    AddIfNotPresent(newattacktiles, othertile);
                }
                if (newtile.GridCoordinates.y < Grid[0].Count - i)
                {
                    GridSquareScript othertile = GetTile((int)(newtile.GridCoordinates.x), (int)(newtile.GridCoordinates.y + i));
                    AddIfNotPresent(newattacktiles, othertile);

                }
            }
        }
        if (unit != null)
        {
            if (newattacktiles.Contains(unit.currentTile))
            {
                newattacktiles.Remove(unit.currentTile);
            }

        }
        return newattacktiles;
    }


    public void ShowAttackAfterMovement(int range, bool frapperenmelee, List<GridSquareScript> tiles, bool usingstaff, Character unit)
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
                            foreach (GridSquareScript tile in tiles)
                            {
                                Vector2 vectorforattack = tile.GridCoordinates + new Vector2(x, y);
                                if (CheckIfPositionIsLegal(vectorforattack))
                                {
                                    if (usingstaff)
                                    {
                                        AddIfNotPresent(healingtiles, Grid[(int)(vectorforattack.x)][(int)(vectorforattack.y)].GetComponent<GridSquareScript>());
                                    }
                                    else
                                    {
                                        AddIfNotPresent(attacktiles, Grid[(int)(vectorforattack.x)][(int)(vectorforattack.y)].GetComponent<GridSquareScript>());
                                    }
                                }
                            }

                        }

                    }
                }
            }
            foreach (GridSquareScript tile in tiles)
            {
                if (tile.GridCoordinates.x >= i)
                {
                    if (usingstaff)
                    {
                        AddIfNotPresent(healingtiles, Grid[(int)(tile.GridCoordinates.x - i)][(int)(tile.GridCoordinates.y)].GetComponent<GridSquareScript>());
                    }
                    else
                    {
                        AddIfNotPresent(attacktiles, Grid[(int)(tile.GridCoordinates.x - i)][(int)(tile.GridCoordinates.y)].GetComponent<GridSquareScript>());

                    }

                }
                if (tile.GridCoordinates.x < Grid.Count - i)
                {
                    if (usingstaff)
                    {
                        AddIfNotPresent(healingtiles, Grid[(int)(tile.GridCoordinates.x + i)][(int)(tile.GridCoordinates.y)].GetComponent<GridSquareScript>());
                    }
                    else
                    {
                        AddIfNotPresent(attacktiles, Grid[(int)(tile.GridCoordinates.x + i)][(int)(tile.GridCoordinates.y)].GetComponent<GridSquareScript>());

                    }
                }
                if (tile.GridCoordinates.y >= i)
                {
                    if (usingstaff)
                    {
                        AddIfNotPresent(healingtiles, Grid[(int)(tile.GridCoordinates.x)][(int)(tile.GridCoordinates.y - i)].GetComponent<GridSquareScript>());
                    }
                    else
                    {
                        AddIfNotPresent(attacktiles, Grid[(int)(tile.GridCoordinates.x)][(int)(tile.GridCoordinates.y - i)].GetComponent<GridSquareScript>());
                    }

                }
                if (tile.GridCoordinates.y < Grid[0].Count - i)
                {
                    if (usingstaff)
                    {
                        AddIfNotPresent(healingtiles, Grid[(int)(tile.GridCoordinates.x)][(int)(tile.GridCoordinates.y + i)].GetComponent<GridSquareScript>());
                    }
                    else
                    {
                        AddIfNotPresent(attacktiles, Grid[(int)(tile.GridCoordinates.x)][(int)(tile.GridCoordinates.y + i)].GetComponent<GridSquareScript>());
                    }
                }
            }
        }
        if (unit != null)
        {
            if (attacktiles.Contains(unit.currentTile))
            {
                attacktiles.Remove(unit.currentTile);
            }
            if (healingtiles.Contains(unit.currentTile))
            {
                healingtiles.Remove(unit.currentTile);
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

        if (ShowDangerousTiles)
        {
            foreach (GridSquareScript gridSquareScript in DangerousTiles)
            {
                gridSquareScript.fillwithPurple();
            }
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

    public void SpreadMovements(Vector2 Coordinates, int remainingMovements, List<GridSquareScript> tilestolight, GameObject selectedunit, Dictionary<GridSquareScript, int> visited)
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
                if (newtile.elevation > tile.elevation && !tile.isstairs)
                {
                    cost += (newtile.elevation - tile.elevation) * 2;
                }


                if (remainingMovements - cost > 0)
                {
                    SpreadMovements(newpos, remainingMovements - cost, tilestolight, selectedunit, visited);
                }
            }
        }
    }


    public bool CheckIfFree(Vector2 position, Character selectedunit)
    {

        if (!CheckIfPositionIsLegal(position))
        {
            return false;
        }
        if (Grid[(int)position.x][(int)position.y].GetComponent<GridSquareScript>().isobstacle)
        {
            return false;

        }
        foreach (Character unit in allunits)
        {
            bool compatibleaffiliation = (selectedunit.affiliation == "playable" && unit.affiliation == "other" && unit.attacksfriends) || (selectedunit.affiliation == "other" && unit.affiliation == "playable" && selectedunit.attacksfriends) || selectedunit.affiliation == "enemy" || unit.affiliation == "enemy";
            if (unit.currentTile.GridCoordinates == position && compatibleaffiliation && unit.currentHP > 0)
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
            return false;
        }

        return true;
    }

    public bool checkifvalidpos(List<GridSquareScript> tilelist, Vector2 position, GameObject currentunit)
    {

        foreach (GridSquareScript tile in tilelist)
        {
            if (tile == null)
            {
                continue;
            }
            if ((int)tile.GridCoordinates.x == (int)position.x && (int)tile.GridCoordinates.y == (int)position.y && (GetUnit(tile) == null || GetUnit(tile) == currentunit))
            {
                return true;
            }
        }

        return false;

    }

    public bool checkifvalidpos(Vector2 position, GameObject currentunit, int range)
    {

        List<GridSquareScript> tiles = new List<GridSquareScript>() { GetTile(position) };

        for (int i = 0; i < range; i++)
        {
            List<GridSquareScript> tilestoadd = new List<GridSquareScript>();

            foreach (GridSquareScript tile in tiles)
            {
                GridSquareScript newtile = GetTile(tile.GridCoordinates + new Vector2(0, 1));
                if (!tilestoadd.Contains(newtile) && !tiles.Contains(newtile))
                {
                    tilestoadd.Add(newtile);
                }

                newtile = GetTile(tile.GridCoordinates + new Vector2(0, -1));
                if (!tilestoadd.Contains(newtile) && !tiles.Contains(newtile))
                {
                    tilestoadd.Add(newtile);
                }

                newtile = GetTile(tile.GridCoordinates + new Vector2(1, 0));
                if (!tilestoadd.Contains(newtile) && !tiles.Contains(newtile))
                {
                    tilestoadd.Add(newtile);
                }

                newtile = GetTile(tile.GridCoordinates + new Vector2(-1, 0));
                if (!tilestoadd.Contains(newtile) && !tiles.Contains(newtile))
                {
                    tilestoadd.Add(newtile);
                }
            }

            foreach (GridSquareScript tile in tilestoadd)
            {
                tiles.Add(tile);
            }

        }

        return checkifvalidpos(tiles, position, currentunit);

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

    public GameObject GetTileGO(int x, int y)
    {
        if (x <= Grid.Count - 1 && x >= 0 && y <= Grid[0].Count - 1 && y >= 0)
        {
            return Grid[x][y];
        }
        return null;
    }

    public GameObject GetTileGO(Vector2 position)
    {
        int x = (int)position.x;
        int y = (int)position.y;
        return GetTileGO(x, y);
    }

    public GridSquareScript GetFirstClosestTile(Vector2 pos)
    {
        for (int x = 0; x < Grid.Count; x++)
        {
            for (int y = 0; y < Grid[x].Count; y++)
            {
                if (x == 0 && y == 0)
                {
                    continue;
                }
                List<Vector2> list = new List<Vector2>() { new Vector2((int)(x + pos.x), (int)(y + pos.y)), new Vector2((int)(x + pos.x), (int)(pos.y - y)), new Vector2((int)(pos.x + x), (int)(y + pos.y)), new Vector2((int)(pos.x - x), (int)(pos.y - y)) };
                foreach (Vector2 newpos in list)
                {

                    if (CheckIfPositionIsLegal(newpos))
                    {
                        GridSquareScript Tile = Grid[(int)newpos.x][(int)newpos.y].GetComponent<GridSquareScript>();
                        if (!Tile.isobstacle && GetUnit(Tile) == null && newpos != pos)
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
                    cost += (neighborTile.elevation - currentTile.elevation) * 2;

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
