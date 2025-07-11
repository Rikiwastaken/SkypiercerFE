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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InstantiateGrid();
        foreach (UnitScript character in FindObjectsByType<UnitScript>(FindObjectsSortMode.None))
        {
            allunits.Add(character.UnitCharacteristics);
        }
    }

    private void FixedUpdate()
    {
        if(inputManager == null)
        {
            inputManager = FindAnyObjectByType<InputManager>();
        }

        if(inputManager.movementValue!=Vector2.zero && inputManager.movementValue!= previousmovevalue)
        {
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
                Grid[x][y].GetComponent<GridSquareScript>().fillwithNothing();
            }
        }
        movementtiles = new List<GridSquareScript>();
        foreach (Character unit in allunits)
        {
            if(unit.position == selection.GridCoordinates)
            {
                SpreadMovements(unit.position, unit.movements, movementtiles);
            }
            ShowAttack();
        }
        foreach(GridSquareScript gridSquareScript in movementtiles)
        {
            gridSquareScript.fillwithblue();
        }

    }

    public void ShowAttack()
    {
        attacktiles = new List<GridSquareScript>();
        foreach (GridSquareScript tile in movementtiles)
        {
            //pour l'instant que pour un de range
            if (tile.GridCoordinates.x > 0)
            {
                attacktiles.Add(Grid[(int)(tile.GridCoordinates.x - 1)][(int)(tile.GridCoordinates.y)].GetComponent<GridSquareScript>());
            }
            if (tile.GridCoordinates.x < Grid.Count - 1)
            {
                attacktiles.Add(Grid[(int)(tile.GridCoordinates.x + 1)][(int)(tile.GridCoordinates.y)].GetComponent<GridSquareScript>());
            }
            if (tile.GridCoordinates.y > 0)
            {
                attacktiles.Add(Grid[(int)(tile.GridCoordinates.x)][(int)(tile.GridCoordinates.y - 1)].GetComponent<GridSquareScript>());
            }
            if (tile.GridCoordinates.y < Grid[0].Count - 1)
            {
                attacktiles.Add(Grid[(int)(tile.GridCoordinates.x)][(int)(tile.GridCoordinates.y + 1)].GetComponent<GridSquareScript>());
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

}
