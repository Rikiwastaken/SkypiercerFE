
using System.Collections.Generic;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{

    private GridScript gridScript;

    private List<List<GameObject>> grid;

    public bool rainymap;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gridScript = GetComponent<GridScript>();
    }

    public void UpdateRain()
    {
        if(!rainymap)
        {
            return;
        }

        if(grid == null)
        {
            grid = gridScript.Grid;
        }

        for (int x = 0; x < grid.Count; x++)
        {
            for(int y=0; y < grid[0].Count; y++)
            {
                GridSquareScript Tile = grid[x][y].GetComponent<GridSquareScript>();
                if(Tile.RemainingRainTurns > 0)
                {
                    Tile.RemainingRainTurns--;
                }
                else if (Tile.RemainingSunTurns > 0)
                {
                    Tile.RemainingSunTurns--;
                }
                else
                {
                    GetRain(Tile);
                }
            }
        }

    }

    private void GetRain(GridSquareScript Tile)
    {
        bool rainingTilenear = false;

        if(Tile.GridCoordinates.x>0)
        {
            GridSquareScript newtile = grid[(int)Tile.GridCoordinates.x - 1][(int)Tile.GridCoordinates.y].GetComponent<GridSquareScript>();
            if (newtile.RemainingRainTurns > 0 && !newtile.justbecamerain)
            {
                rainingTilenear = true;
            }
        }
        if (Tile.GridCoordinates.y > 0)
        {
            GridSquareScript newtile = grid[(int)Tile.GridCoordinates.x][(int)Tile.GridCoordinates.y - 1].GetComponent<GridSquareScript>();
            if (newtile.RemainingRainTurns > 0 && !newtile.justbecamerain)
            {
                rainingTilenear = true;
            }
        }
        if (Tile.GridCoordinates.x <grid.Count-1)
        {
            GridSquareScript newtile = grid[(int)Tile.GridCoordinates.x + 1][(int)Tile.GridCoordinates.y].GetComponent<GridSquareScript>();
            if (newtile.RemainingRainTurns > 0 && !newtile.justbecamerain)
            {
                rainingTilenear = true;
            }
        }
        if (Tile.GridCoordinates.y < grid.Count - 1)
        {
            GridSquareScript newtile = grid[(int)Tile.GridCoordinates.x][(int)Tile.GridCoordinates.y + 1].GetComponent<GridSquareScript>();
            if (newtile.RemainingRainTurns > 0 && !newtile.justbecamerain)
            {
                rainingTilenear = true;
            }
        }

        int randomvalue = Tile.GetRandomNumber() ;

        if(rainingTilenear)
        {
            if(randomvalue < 30)
            {
                Tile.RemainingRainTurns = 3;
                Tile.justbecamerain = true;
            }
        }
        else
        {
            if(randomvalue<15)
            {
                Tile.RemainingRainTurns = 3;
                Tile.justbecamerain = true;
            }
        }
    }
}
