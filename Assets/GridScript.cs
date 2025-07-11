using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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

    private List<GridSquareScript> movementtiles;
    private List<GridSquareScript> attacktiles;

    public GridSquareScript lastSquare;

    private int moveCD;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //InstantiateGrid();
        foreach (UnitScript character in FindObjectsByType<UnitScript>(FindObjectsSortMode.None))
        {
            allunits.Add(character.UnitCharacteristics);
        }
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

        if(inputManager.movementValue!=Vector2.zero && inputManager.movementValue!= previousmovevalue && moveCD<=0)
        {
            moveCD =(int)(0.1f/Time.deltaTime);
            previousmovevalue = inputManager.movementValue;
            MoveSelection(previousmovevalue);
        }
        else
        {
            previousmovevalue = Vector2.zero;
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

    public void ShowMovement()
    {
        for(int x=0; x<Grid.Count; x++)
        {
            for(int y=0; y<Grid[x].Count; y++)
            {
                if(Grid[x][y].GetComponent<GridSquareScript>().isobstacle)
                {
                    Grid[x][y].GetComponent<GridSquareScript>().fillwithGrey();
                }
                else
                {
                    Grid[x][y].GetComponent<GridSquareScript>().fillwithNothing();
                }
                    
            }
        }
        movementtiles = new List<GridSquareScript>();
        foreach (Character unit in allunits)
        {
            if(unit.position == selection.GridCoordinates)
            {
                SpreadMovements(unit.position, unit.movements, movementtiles);
            }
            ShowAttack(3,false);
        }
        foreach(GridSquareScript gridSquareScript in movementtiles)
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

    public void ShowAttack(int range, bool frapperenmelee)
    {
        attacktiles = new List<GridSquareScript>();
        foreach (GridSquareScript tile in movementtiles)
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
        foreach (GridSquareScript gridSquareScript in attacktiles)
        {
            gridSquareScript.fillwithRed();
        }
    }

    private void SpreadMovements(Vector2 Coordinates, int remainingMovements, List<GridSquareScript> tilestolight)
    {
        if (!tilestolight.Contains(Grid[(int)Coordinates.x][(int)Coordinates.y].GetComponent<GridSquareScript>()))
        {
            tilestolight.Add(Grid[(int)Coordinates.x][(int)Coordinates.y].GetComponent<GridSquareScript>());
        }
        if(remainingMovements > 0)
        {
            if(Coordinates.x>0)
            {
                if(CheckIfFree(new Vector2(Coordinates.x - 1, Coordinates.y)))
                {
                    SpreadMovements(new Vector2(Coordinates.x - 1, Coordinates.y), remainingMovements - 1, tilestolight);
                } 
            }
            if (Coordinates.x < Grid.Count-1)
            {
                if (CheckIfFree(new Vector2(Coordinates.x + 1, Coordinates.y)))
                {
                    SpreadMovements(new Vector2(Coordinates.x + 1, Coordinates.y), remainingMovements - 1, tilestolight);
                }
            }
            if (Coordinates.y > 0)
            {
                if (CheckIfFree(new Vector2(Coordinates.x, Coordinates.y - 1)))
                {
                    SpreadMovements(new Vector2(Coordinates.x, Coordinates.y - 1), remainingMovements - 1, tilestolight);
                }
            }
            if (Coordinates.y < Grid[0].Count - 1)
            {
                if (CheckIfFree(new Vector2(Coordinates.x, Coordinates.y + 1)))
                {
                    SpreadMovements(new Vector2(Coordinates.x, Coordinates.y + 1), remainingMovements - 1, tilestolight);
                }
                
            }
        }
    }

    private bool CheckIfFree(Vector2 position)
    {

        if (Grid[(int)position.x][(int)position.y].GetComponent<GridSquareScript>().isobstacle)
        {
            return false;

        }
        foreach (Character unit in allunits)
        {
            if (unit.position == position && unit.isennemy)
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

}
