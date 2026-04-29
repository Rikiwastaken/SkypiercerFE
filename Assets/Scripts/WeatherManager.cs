
using System.Collections.Generic;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{

    private GridScript gridScript;

    private List<List<GameObject>> grid;

    public bool rainymap;

    public bool alwayssunny;

    public bool alwaysrainy;

    public List<List<GridSquareScript>> sunnytilesgroups;
    public List<List<GridSquareScript>> rainytilesgroups;
    public List<List<GridSquareScript>> cloudytilesgroups;

    public Color SunnyTileColor;
    public Color CloudyTileColor;
    public Color rainyTileColor;

    public Sprite SunnySprite;
    public Sprite CloudySprite;
    public Sprite RainySprite;

    private List<GameObject> SpritesUsed = new List<GameObject>();

    private bool FirstGroupCreation = true;

    public bool showingweather;

    private InputManager inputmanager;

    private int weatherTogglecooldown;

    public Transform WeatherGOHolder;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gridScript = GetComponent<GridScript>();
        inputmanager = InputManager.instance;
        WeatherGOHolder.transform.position = Vector3.zero;
    }

    private void Update()
    {
        if (FirstGroupCreation && gridScript.Grid != null && gridScript.Grid.Count > 0)
        {
            FirstGroupCreation = false;
            fillGroups();
        }

        if (inputmanager.ShowWeatherTilespressed && weatherTogglecooldown <= 0)
        {
            ToggleWeatherVisuals();
            weatherTogglecooldown = (int)(0.5f / Time.deltaTime);
        }
        if (weatherTogglecooldown > 0)
        {
            weatherTogglecooldown--;
        }
    }

    public void UpdateRain()
    {
        if (!rainymap && !alwayssunny && !alwaysrainy)
        {
            return;
        }

        if (grid == null)
        {
            grid = gridScript.Grid;
        }

        for (int x = 0; x < grid.Count; x++)
        {
            for (int y = 0; y < grid[0].Count; y++)
            {
                GridSquareScript Tile = grid[x][y].GetComponent<GridSquareScript>();
                if (alwaysrainy)
                {
                    Tile.RemainingRainTurns = 99;
                }
                else if (alwayssunny)
                {
                    Tile.RemainingSunTurns = 99;
                }
                else
                {
                    if (Tile.RemainingRainTurns > 0)
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
        fillGroups();

    }

    // create groups of tiles which share the same weather
    private void fillGroups()
    {
        if (grid == null)
        {
            grid = gridScript.Grid;
        }
        sunnytilesgroups = new List<List<GridSquareScript>>();
        cloudytilesgroups = new List<List<GridSquareScript>>();
        rainytilesgroups = new List<List<GridSquareScript>>();
        List<GridSquareScript> alltiles = new List<GridSquareScript>();

        for (int x = 0; x < grid.Count; x++)
        {
            for (int y = 0; y < grid[0].Count; y++)
            {
                GridSquareScript Tile = grid[x][y].GetComponent<GridSquareScript>();
                alltiles.Add(Tile);

            }
        }

        // while alltiles is not empty, we remove the first tile and we spread the group*


        while (alltiles.Count > 0)
        {
            GridSquareScript currentTile = alltiles[0];
            alltiles.Remove(currentTile);
            if (currentTile.RemainingRainTurns > 0)
            {
                List<GridSquareScript> newRainGroup = new List<GridSquareScript>() { currentTile };
                rainytilesgroups.Add(newRainGroup);
                SpreadGroup(currentTile, newRainGroup, alltiles);
            }
            else if (currentTile.RemainingSunTurns > 0)
            {
                List<GridSquareScript> newSunGroup = new List<GridSquareScript>() { currentTile };
                sunnytilesgroups.Add(newSunGroup);
                SpreadGroup(currentTile, newSunGroup, alltiles);
            }
            else
            {
                List<GridSquareScript> newCloudGroup = new List<GridSquareScript>() { currentTile };
                cloudytilesgroups.Add(newCloudGroup);
                SpreadGroup(currentTile, newCloudGroup, alltiles);
            }
        }
    }

    private void SpreadGroup(GridSquareScript tile, List<GridSquareScript> currentgroup, List<GridSquareScript> alltiles)
    {

        // we start by getting all the neighbors of the tile and checking if they haven't been dealt with and if they share the same weather

        List<GridSquareScript> neighbortiles = new List<GridSquareScript>();
        GridSquareScript neighboreast = gridScript.GetTile(tile.GridCoordinates + new Vector2(1, 0));
        GridSquareScript neighborwest = gridScript.GetTile(tile.GridCoordinates + new Vector2(-1, 0));
        GridSquareScript neighbornorth = gridScript.GetTile(tile.GridCoordinates + new Vector2(0, 1));
        GridSquareScript neighborsouth = gridScript.GetTile(tile.GridCoordinates + new Vector2(0, -1));
        if (neighboreast != null && alltiles.Contains(neighboreast) && CheckIfSameWeather(tile, neighboreast))
        {
            neighbortiles.Add(neighboreast);
        }
        if (neighborwest != null && alltiles.Contains(neighborwest) && CheckIfSameWeather(tile, neighborwest))
        {
            neighbortiles.Add(neighborwest);
        }
        if (neighbornorth != null && alltiles.Contains(neighbornorth) && CheckIfSameWeather(tile, neighbornorth))
        {
            neighbortiles.Add(neighbornorth);
        }
        if (neighborsouth != null && alltiles.Contains(neighborsouth) && CheckIfSameWeather(tile, neighborsouth))
        {
            neighbortiles.Add(neighborsouth);
        }

        // we remove the neighbors from the all tiles list and we add them to the current group
        foreach (GridSquareScript neighbor in neighbortiles)
        {
            alltiles.Remove(neighbor);
            currentgroup.Add(neighbor);
        }

        // we spread the group from the neighbors
        foreach (GridSquareScript neighbor in neighbortiles)
        {
            SpreadGroup(neighbor, currentgroup, alltiles);
        }
    }

    private bool CheckIfSameWeather(GridSquareScript tileA, GridSquareScript tileB)
    {
        if (tileA.RemainingRainTurns > 0 && tileB.RemainingRainTurns > 0)
        {
            return true;
        }
        if (tileA.RemainingSunTurns > 0 && tileB.RemainingSunTurns > 0)
        {
            return true;
        }
        if (tileA.RemainingRainTurns == 0 && tileA.RemainingSunTurns == 0 && tileB.RemainingRainTurns == 0 && tileB.RemainingSunTurns == 0)
        {
            return true;
        }
        return false;
    }

    private void ToggleWeatherVisuals()
    {
        showingweather = !showingweather;
        if (showingweather)
        {

            int currentspriteusedID = 0;

            foreach (List<GridSquareScript> group in sunnytilesgroups)
            {
                Vector2 averageposition = new Vector2();
                foreach (GridSquareScript tile in group)
                {
                    tile.WeatherColorSprite.color = SunnyTileColor;
                    averageposition += tile.GridCoordinates;
                }
                averageposition = averageposition / group.Count;
                currentspriteusedID = ManageSpriteGOList(currentspriteusedID, SunnySprite, averageposition, group.Count);
            }
            foreach (List<GridSquareScript> group in cloudytilesgroups)
            {
                Vector2 averageposition = new Vector2();
                foreach (GridSquareScript tile in group)
                {
                    tile.WeatherColorSprite.color = CloudyTileColor;
                    averageposition += tile.GridCoordinates;
                }
                averageposition = averageposition / group.Count;
                currentspriteusedID = ManageSpriteGOList(currentspriteusedID, CloudySprite, averageposition, group.Count);
            }
            foreach (List<GridSquareScript> group in rainytilesgroups)
            {
                Vector2 averageposition = new Vector2();
                foreach (GridSquareScript tile in group)
                {
                    tile.WeatherColorSprite.color = rainyTileColor;
                    averageposition += tile.GridCoordinates;
                }
                averageposition = averageposition / group.Count;
                currentspriteusedID = ManageSpriteGOList(currentspriteusedID, RainySprite, averageposition, group.Count);
            }
        }
        else
        {
            for (int x = 0; x < grid.Count; x++)
            {
                for (int y = 0; y < grid[0].Count; y++)
                {
                    GridSquareScript Tile = grid[x][y].GetComponent<GridSquareScript>();
                    Tile.WeatherColorSprite.color = new Color(1f, 1f, 1f, 0f);

                }
            }
            foreach (GameObject spriteGO in SpritesUsed)
            {
                spriteGO.SetActive(false);
            }
        }
    }

    private int ManageSpriteGOList(int currentID, Sprite Spritetouse, Vector2 position, int groupsize)
    {
        if (currentID < SpritesUsed.Count)
        {
            SpritesUsed[currentID].GetComponent<SpriteRenderer>().sprite = Spritetouse;
            SpritesUsed[currentID].transform.position = new Vector3(position.x, 0.5f, position.y) + new Vector3(0f, Random.Range(-0.05f, 0.05f), 0f);
            SpritesUsed[currentID].SetActive(true);
            SpritesUsed[currentID].transform.localScale = Vector3.one * 0.1f * Mathf.Min(groupsize, 3);
            SpritesUsed[currentID].transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));
        }
        else
        {
            GameObject newsprite = new GameObject();
            newsprite.name = "Weather Sprite " + (currentID);
            newsprite.AddComponent<SpriteRenderer>();
            newsprite.GetComponent<SpriteRenderer>().sprite = Spritetouse;
            newsprite.transform.parent = WeatherGOHolder;
            newsprite.transform.position = new Vector3(position.x, 0.5f, position.y) + new Vector3(0f, Random.Range(-0.05f, 0.05f), 0f); ;
            newsprite.transform.localScale = Vector3.one * 0.1f * Mathf.Min(groupsize, 3);
            newsprite.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));
            newsprite.gameObject.layer = LayerMask.NameToLayer("Grid");
            SpritesUsed.Add(newsprite);
        }
        return currentID + 1;
    }

    private void GetRain(GridSquareScript Tile)
    {
        bool rainingTilenear = false;

        if (Tile.GridCoordinates.x > 0)
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
        if (Tile.GridCoordinates.x < grid.Count - 1)
        {
            GridSquareScript newtile = grid[(int)Tile.GridCoordinates.x + 1][(int)Tile.GridCoordinates.y].GetComponent<GridSquareScript>();
            if (newtile.RemainingRainTurns > 0 && !newtile.justbecamerain)
            {
                rainingTilenear = true;
            }
        }
        if (Tile.GridCoordinates.y < grid[0].Count - 1)
        {

            GridSquareScript newtile = grid[(int)Tile.GridCoordinates.x][(int)Tile.GridCoordinates.y + 1].GetComponent<GridSquareScript>();
            if (newtile.RemainingRainTurns > 0 && !newtile.justbecamerain)
            {
                rainingTilenear = true;
            }
        }

        int randomvalue = Tile.GetRandomNumber();

        if (rainingTilenear)
        {
            if (randomvalue < 30)
            {
                Tile.RemainingRainTurns = 2;
                Tile.justbecamerain = true;
            }
        }
        else
        {
            if (randomvalue < 15)
            {
                Tile.RemainingRainTurns = 2;
                Tile.justbecamerain = true;
            }
        }
    }
}
