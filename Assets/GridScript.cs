using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
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
    public List<GridSquareScript> lockedmovementtiles;
    public List<GridSquareScript> lockedattacktiles;
    public List<GridSquareScript> dangerousTiles;

    public GridSquareScript lastSquare;

    private int moveCD;

    public bool lockselection;

    public GameObject actionsMenu;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lockedmovementtiles = new List<GridSquareScript>();
        lockedattacktiles = new List<GridSquareScript>();
        //InstantiateGrid();
        foreach (UnitScript character in FindObjectsByType<UnitScript>(FindObjectsSortMode.None))
        {
            allunits.Add(character.UnitCharacteristics);
            allunitGOs.Add(character.gameObject);
        }
        GetComponent<TurnManger>().InitializeUnitLists(allunits);
        Grid = new List<List<GameObject>>();
        GridSquareScript[] tilelist = FindObjectsByType<GridSquareScript>(FindObjectsSortMode.None);
        for (int x = 0; x <= lastSquare.GridCoordinates.x; x++)
        {
            Grid.Add(new List<GameObject>());
            for (int y = 0; y <= lastSquare.GridCoordinates.y; y++)
            {
                foreach(GridSquareScript tile in tilelist)
                {
                    if((int)tile.GridCoordinates.x == x && (int)tile.GridCoordinates.y == y)
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
        selection = Grid[0][0].GetComponent<GridSquareScript>();
    }

    private void FixedUpdate()
    {
        if(moveCD>0)
        {
            moveCD--;
        }
        if(inputManager == null)
        {
            inputManager = FindAnyObjectByType<InputManager>();
        }

        if(inputManager.movementValue!=Vector2.zero && inputManager.movementValue!= previousmovevalue && moveCD<=0 && !actionsMenu.activeSelf)
        {
            moveCD =(int)(0.1f/Time.deltaTime);
            previousmovevalue = inputManager.movementValue;
            MoveSelection(previousmovevalue);
        }
        else
        {
            previousmovevalue = Vector2.zero;
        }

        if(inputManager.Selectjustpressed)
        {
            ShowDangerousTiles();
        }

    }

    private void InstantiateGrid()
    {
        Grid = new List<List<GameObject>>();
        for(int i = 0; i < GridDimensions.x; i++)
        {
            Grid.Add(new List<GameObject>());
            for(int j = 0; j < GridDimensions.y; j++)
            {
                GameObject newtile = Instantiate(GridSquarePrefab,this.transform);
                newtile.transform.position = new Vector3(i,0,j);
                newtile.GetComponent<GridSquareScript>().GridCoordinates=new Vector2(i,j);
                Grid[i].Add(newtile);
            }
        }
        selection = Grid[0][0].GetComponent<GridSquareScript>();
    }

    public void MoveSelection(Vector2 input)
    {
        if(selection == null)
        {
            selection = Grid[0][0].GetComponent<GridSquareScript>();
        }
        else
        {
            if(input.x>0 && (int)selection.GridCoordinates.x<Grid.Count-1)
            {
                selection = Grid[(int)(selection.GridCoordinates.x) + 1][(int)(selection.GridCoordinates.y)].GetComponent<GridSquareScript>();
            }
            if (input.x < 0 && (int)selection.GridCoordinates.x > 0)
            {
                selection = Grid[(int)(selection.GridCoordinates.x) - 1][(int)(selection.GridCoordinates.y)].GetComponent<GridSquareScript>();
            }

            if (input.y > 0 && (int)selection.GridCoordinates.y < Grid[0].Count - 1)
            {
                selection = Grid[(int)(selection.GridCoordinates.x)][(int)(selection.GridCoordinates.y)+1].GetComponent<GridSquareScript>();
            }
            if (input.y < 0 && (int)selection.GridCoordinates.y > 0)
            {
                selection = Grid[(int)(selection.GridCoordinates.x)][(int)(selection.GridCoordinates.y)-1].GetComponent<GridSquareScript>();
            }
        }
        ShowMovement();

    }

    public void LockcurrentSelection()
    {
        lockedmovementtiles = new List<GridSquareScript> ();
        foreach (GridSquareScript tile in movementtiles)
        {
            lockedmovementtiles.Add (tile);
        }
        lockedattacktiles = new List<GridSquareScript>();
        foreach (GridSquareScript tile in attacktiles)
        {
            lockedattacktiles.Add(tile);
        }
    }

    public void UnlockSelection()
    {
        lockedmovementtiles = new List<GridSquareScript>();
        lockedattacktiles = new List<GridSquareScript>();
        lockselection = false;
    }

    public void ResetAllSelections()
    {
        lockedmovementtiles = new List<GridSquareScript>();
        lockedattacktiles = new List<GridSquareScript>();
        movementtiles = new List<GridSquareScript>();
        attacktiles = new List<GridSquareScript>();
        lockselection = false;
    }

    public void ShowMovement()
    {
        for(int x=0; x<Grid.Count; x++)
        {
            for(int y=0; y<Grid[x].Count; y++)
            {
                if(Grid[x][y].GetComponent<GridSquareScript>().isobstacle && !lockedattacktiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()) && !lockedmovementtiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()))
                {
                    Grid[x][y].GetComponent<GridSquareScript>().fillwithGrey();
                }
                else if (!lockedattacktiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()) && !lockedmovementtiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()))
                {
                    Grid[x][y].GetComponent<GridSquareScript>().fillwithNothing();
                }
                    
            }
        }
        movementtiles = new List<GridSquareScript>();
        foreach (GameObject unitGO in allunitGOs)
        {
            Character unit = unitGO.GetComponent<UnitScript>().UnitCharacteristics;
            if(unit.position == selection.GridCoordinates && !unit.alreadyplayed)
            {
                SpreadMovements(unit.position, unit.movements, movementtiles, unit);
                (int range, bool melee) = unitGO.GetComponent<UnitScript>().GetRangeAndMele();
                ShowAttack(range, melee);
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

    public Character GetSelectedUnit()
    {
        Character SelectedUnit = null;


        foreach (Character unit in allunits)
        {
            if(unit.position ==selection.GridCoordinates)
            {
                SelectedUnit = unit;
                break;
            }
        }

        return SelectedUnit;
    }

    public GameObject GetUnit(GridSquareScript tile)
    {
        foreach (GameObject unit in allunitGOs)
        {
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
            if (unit.GetComponent<UnitScript>().UnitCharacteristics.position == selection.GridCoordinates)
            {
                SelectedUnit = unit;
                break;
            }
        }

        return SelectedUnit;
    }

    public void ShowDangerousTiles()
    {
        dangerousTiles = new List<GridSquareScript>();
        foreach(GameObject unit in allunitGOs)
        {
            Character unitchar = unit.GetComponent<UnitScript>().UnitCharacteristics;
            if(unitchar.affiliation== "enemy")
            {
                SpreadMovements(unitchar.position, unitchar.movements, movementtiles, unitchar);
                (int range, bool melee) = unit.GetComponent<UnitScript>().GetRangeAndMele();
                ShowAttack(range, melee);
                foreach(GridSquareScript tile in attacktiles)
                {
                    if(!dangerousTiles.Contains(tile))
                    {
                        dangerousTiles.Add(tile);
                    }
                }
            }
        }
        foreach(GridSquareScript tile in dangerousTiles)
        {
            tile.fillwithPurple();
        }
    }

    public void ShowAttack(int range, bool frapperenmelee, bool uselockedmovementtile=false)
    {
        attacktiles = new List<GridSquareScript>();
        List < GridSquareScript > tilestouse = new List<GridSquareScript>();
        if(uselockedmovementtile)
        {
            tilestouse = lockedmovementtiles;
        }
        else
        {
            tilestouse = movementtiles;
        }
        foreach (GridSquareScript tile in tilestouse)
        {
            for(int i=1;i<=range;i++)
            {
                if(i==1 && !frapperenmelee)
                {
                    continue;
                }
                if(i>1)
                {
                    for (int x=1; x<=i-1; x++)
                    {
                        for(int y =1; y<=i-1; y++)
                        {
                            Vector2 vectorforattack = tile.GridCoordinates+new Vector2(x,y);
                            if(CheckIfPositionIsLegal(vectorforattack))
                            {
                                attacktiles.Add(Grid[(int)(vectorforattack.x)][(int)(vectorforattack.y)].GetComponent<GridSquareScript>());
                            }
                            vectorforattack = tile.GridCoordinates + new Vector2(x, -y);
                            if (CheckIfPositionIsLegal(vectorforattack))
                            {
                                attacktiles.Add(Grid[(int)(vectorforattack.x)][(int)(vectorforattack.y)].GetComponent<GridSquareScript>());
                            }
                            vectorforattack = tile.GridCoordinates + new Vector2(-x, y);
                            if (CheckIfPositionIsLegal(vectorforattack))
                            {
                                attacktiles.Add(Grid[(int)(vectorforattack.x)][(int)(vectorforattack.y)].GetComponent<GridSquareScript>());
                            }
                            vectorforattack = tile.GridCoordinates + new Vector2(-x, -y);
                            if (CheckIfPositionIsLegal(vectorforattack))
                            {
                                attacktiles.Add(Grid[(int)(vectorforattack.x)][(int)(vectorforattack.y)].GetComponent<GridSquareScript>());
                            }
                        }
                    }
                }
                if (tile.GridCoordinates.x >= i)
                {
                    attacktiles.Add(Grid[(int)(tile.GridCoordinates.x - i)][(int)(tile.GridCoordinates.y)].GetComponent<GridSquareScript>());
                }
                if (tile.GridCoordinates.x < Grid.Count - i)
                {
                    attacktiles.Add(Grid[(int)(tile.GridCoordinates.x + i)][(int)(tile.GridCoordinates.y)].GetComponent<GridSquareScript>());
                }
                if (tile.GridCoordinates.y >= i)
                {
                    attacktiles.Add(Grid[(int)(tile.GridCoordinates.x)][(int)(tile.GridCoordinates.y - i)].GetComponent<GridSquareScript>());
                }
                if (tile.GridCoordinates.y < Grid[0].Count - i)
                {
                    attacktiles.Add(Grid[(int)(tile.GridCoordinates.x)][(int)(tile.GridCoordinates.y + i)].GetComponent<GridSquareScript>());
                }
            }
            
        }
        if(!lockselection)
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
    }

    public void ShowAttackAfterMovement(int range, bool frapperenmelee, GridSquareScript tile )
    {
        movementtiles.Clear();
        attacktiles = new List<GridSquareScript>();
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
                            attacktiles.Add(Grid[(int)(vectorforattack.x)][(int)(vectorforattack.y)].GetComponent<GridSquareScript>());
                        }
                        vectorforattack = tile.GridCoordinates + new Vector2(x, -y);
                        if (CheckIfPositionIsLegal(vectorforattack))
                        {
                            attacktiles.Add(Grid[(int)(vectorforattack.x)][(int)(vectorforattack.y)].GetComponent<GridSquareScript>());
                        }
                        vectorforattack = tile.GridCoordinates + new Vector2(-x, y);
                        if (CheckIfPositionIsLegal(vectorforattack))
                        {
                            attacktiles.Add(Grid[(int)(vectorforattack.x)][(int)(vectorforattack.y)].GetComponent<GridSquareScript>());
                        }
                        vectorforattack = tile.GridCoordinates + new Vector2(-x, -y);
                        if (CheckIfPositionIsLegal(vectorforattack))
                        {
                            attacktiles.Add(Grid[(int)(vectorforattack.x)][(int)(vectorforattack.y)].GetComponent<GridSquareScript>());
                        }
                    }
                }
            }
            if (tile.GridCoordinates.x >= i)
            {
                attacktiles.Add(Grid[(int)(tile.GridCoordinates.x - i)][(int)(tile.GridCoordinates.y)].GetComponent<GridSquareScript>());
            }
            if (tile.GridCoordinates.x < Grid.Count - i)
            {
                attacktiles.Add(Grid[(int)(tile.GridCoordinates.x + i)][(int)(tile.GridCoordinates.y)].GetComponent<GridSquareScript>());
            }
            if (tile.GridCoordinates.y >= i)
            {
                attacktiles.Add(Grid[(int)(tile.GridCoordinates.x)][(int)(tile.GridCoordinates.y - i)].GetComponent<GridSquareScript>());
            }
            if (tile.GridCoordinates.y < Grid[0].Count - i)
            {
                attacktiles.Add(Grid[(int)(tile.GridCoordinates.x)][(int)(tile.GridCoordinates.y + i)].GetComponent<GridSquareScript>());
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
                if (Grid[x][y].GetComponent<GridSquareScript>().isobstacle && !lockedattacktiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()) && !lockedmovementtiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()))
                {
                    Grid[x][y].GetComponent<GridSquareScript>().fillwithGrey();
                }
                else if (!lockedattacktiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()) && !lockedmovementtiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()))
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
                if (Grid[x][y].GetComponent<GridSquareScript>().isobstacle && !lockedattacktiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()) && !lockedmovementtiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()))
                {
                    Grid[x][y].GetComponent<GridSquareScript>().fillwithGrey();
                }
                else if (!lockedattacktiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()) && !lockedmovementtiles.Contains(Grid[x][y].GetComponent<GridSquareScript>()))
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
        }

        foreach (GridSquareScript gridSquareScript in lockedattacktiles)
        {
            gridSquareScript.fillwithRed();
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
    private void SpreadMovements(Vector2 Coordinates, int remainingMovements, List<GridSquareScript> tilestolight, Character selectedunit)
    {
        if (!tilestolight.Contains(Grid[(int)Coordinates.x][(int)Coordinates.y].GetComponent<GridSquareScript>()))
        {
            tilestolight.Add(Grid[(int)Coordinates.x][(int)Coordinates.y].GetComponent<GridSquareScript>());
        }
        if(remainingMovements > 0)
        {
            if(Coordinates.x>0)
            {
                if(CheckIfFree(new Vector2(Coordinates.x - 1, Coordinates.y), selectedunit))
                {
                    SpreadMovements(new Vector2(Coordinates.x - 1, Coordinates.y), remainingMovements - 1, tilestolight, selectedunit);
                } 
            }
            if (Coordinates.x < Grid.Count-1)
            {
                if (CheckIfFree(new Vector2(Coordinates.x + 1, Coordinates.y), selectedunit))
                {
                    SpreadMovements(new Vector2(Coordinates.x + 1, Coordinates.y), remainingMovements - 1, tilestolight, selectedunit);
                }
            }
            if (Coordinates.y > 0)
            {
                if (CheckIfFree(new Vector2(Coordinates.x, Coordinates.y - 1), selectedunit))
                {
                    SpreadMovements(new Vector2(Coordinates.x, Coordinates.y - 1), remainingMovements - 1, tilestolight, selectedunit);
                }
            }
            if (Coordinates.y < Grid[0].Count - 1)
            {
                if (CheckIfFree(new Vector2(Coordinates.x, Coordinates.y + 1), selectedunit))
                {
                    SpreadMovements(new Vector2(Coordinates.x, Coordinates.y + 1), remainingMovements - 1, tilestolight, selectedunit);
                }
                
            }
        }
    }

    private bool CheckIfFree(Vector2 position, Character selectedunit)
    {

        if (Grid[(int)position.x][(int)position.y].GetComponent<GridSquareScript>().isobstacle)
        {
            return false;

        }
        foreach (Character unit in allunits)
        {
            if (unit.position == position && unit.affiliation != selectedunit.affiliation)
            {
                return false;
            }
        }

        return true;
    }

    private bool CheckIfPositionIsLegal(Vector2 position)
    {
        if (position.x<0 || position.x >= Grid.Count || position.y < 0 || position.y >= Grid[0].Count)
        {
            return false ;
        }
        return true;
    }

    public bool checkifvalidpos(List<GridSquareScript> tilelist, Vector2 position)
    {

        foreach(GridSquareScript tile in tilelist)
        {
            if((int)tile.GridCoordinates.x==(int)position.x && (int)tile.GridCoordinates.y == (int)position.y)
            {
                return true;
            }
        }

        return false;

    }

    public GridSquareScript GetTile(int x, int y)
    {
        if(x<=Grid.Count-1 && x>=0 && y <= Grid[0].Count - 1 && y >= 0)
        {
            return Grid[x][y].GetComponent<GridSquareScript>();
        }
        return null;
    }

    public GridSquareScript GetTile(Vector2 position)
    {
        int x = (int)position.x;
        int y = (int)position.y;
        if (x <= Grid.Count - 1 && x >= 0 && y <= Grid[0].Count - 1 && y >= 0)
        {
            return Grid[x][y].GetComponent<GridSquareScript>();
        }
        return null;
    }
}
