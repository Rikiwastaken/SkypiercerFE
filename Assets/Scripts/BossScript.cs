using System.Collections.Generic;
using UnityEngine;
using static UnitScript;

public class BossScript : MonoBehaviour
{

    public static BossScript instance;

    private GridScript GridScript;

    public GameObject nextTarget;

    private ActionsMenu ActionsMenu;


    private void Awake()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GridScript = GridScript.instance;
    }

    public void TriggerBossAttack(GameObject characterwhoblocked = null)
    {

        if (ActionsMenu == null)
        {
            ActionsMenu = FindAnyObjectByType<ActionsMenu>(FindObjectsInactive.Include);
        }

        List<GameObject> unitsinthezone = new List<GameObject>();

        bool intercepted = false;
        GameObject intercepter = null;
        for (int i = 0; i < GridScript.Grid.Count; i++)
        {
            for (int j = 0; j < GridScript.Grid[0].Count; j++)
            {
                GridSquareScript tile = GridScript.Grid[i][j].GetComponent<GridSquareScript>();
                if (tile.isbossAttackTile)
                {
                    tile.isbossAttackTile = false;
                    GameObject UnitOnTile = GridScript.GetUnit(tile);
                    if (UnitOnTile != null && UnitOnTile.GetComponent<UnitScript>().UnitCharacteristics.affiliation.ToLower() != GetComponent<UnitScript>().UnitCharacteristics.affiliation.ToLower())
                    {
                        unitsinthezone.Add(UnitOnTile);
                        if (UnitOnTile.GetComponent<UnitScript>().UnitCharacteristics.isintercepting)
                        {
                            intercepted = true;
                            intercepter = UnitOnTile;
                        }
                    }
                }

            }
        }

        foreach (GameObject unit in unitsinthezone)
        {
            ActionsMenu.ApplyDamage(gameObject, unit, true, false, true, intercepted, intercepter == unit);
        }


        switch (GetComponent<UnitScript>().UnitCharacteristics.enemyStats.bossiD)
        {
            case 1:
                DetermineNextAttackTilesRagnallFirstEncounter(nextTarget);
                break;
            case 2:
                DetermineNextAttackTilesKayFirstEncounter(nextTarget);
                break;
            case 3:
                DetermineNextAttackTilesKaySecondEncounter(nextTarget);
                break;
            case 4:
                DetermineNextAttackTilesRagnallSecondEncounter(nextTarget);
                break;
        }
    }


    public void DetermineNextAttackTilesRagnallFirstEncounter(GameObject target)
    {
        Character character = GetComponent<UnitScript>().UnitCharacteristics;

        Character targetcharacter = target.GetComponent<UnitScript>().UnitCharacteristics;

        Vector2 RagnallPosition = character.currentTile.GridCoordinates;

        Vector2 TargetPosition = targetcharacter.currentTile.GridCoordinates;

        int distance = ManhanttanDistance(RagnallPosition, TargetPosition);

        if (distance <= 2)
        {
            for (int i = 0; i < GridScript.Grid.Count; i++)
            {
                for (int j = 0; j < GridScript.Grid[0].Count; j++)
                {
                    GridSquareScript tile = GridScript.Grid[i][j].GetComponent<GridSquareScript>();
                    if (ManhanttanDistance(RagnallPosition, tile.GridCoordinates) <= 2 && tile != character.currentTile)
                    {
                        tile.isbossAttackTile = true;
                        tile.BossTileChanged();
                    }
                }
            }
        }
        else if (distance <= 4)
        {
            for (int i = 0; i < GridScript.Grid.Count; i++)
            {
                for (int j = 0; j < GridScript.Grid[0].Count; j++)
                {
                    GridSquareScript tile = GridScript.Grid[i][j].GetComponent<GridSquareScript>();
                    if (ManhanttanDistance(RagnallPosition, tile.GridCoordinates) <= 4 && ManhanttanDistance(RagnallPosition, tile.GridCoordinates) > 2 && tile != character.currentTile)
                    {
                        tile.isbossAttackTile = true;
                        tile.BossTileChanged();
                    }
                }
            }
        }
        else
        {
            if (Mathf.Abs(TargetPosition.x - RagnallPosition.x) < Mathf.Abs(TargetPosition.y - RagnallPosition.y))
            {
                for (int i = 0; i < GridScript.Grid.Count; i++)
                {
                    for (int j = 0; j < GridScript.Grid[0].Count; j++)
                    {
                        GridSquareScript tile = GridScript.Grid[i][j].GetComponent<GridSquareScript>();
                        if (Mathf.Abs(tile.GridCoordinates.x - RagnallPosition.x) <= 1)
                        {
                            if (TargetPosition.y < RagnallPosition.y && tile.GridCoordinates.y < RagnallPosition.y)
                            {
                                tile.isbossAttackTile = true;
                                tile.BossTileChanged();
                            }
                            else if (TargetPosition.y > RagnallPosition.y && tile.GridCoordinates.y > RagnallPosition.y)
                            {
                                tile.isbossAttackTile = true;
                                tile.BossTileChanged();
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < GridScript.Grid.Count; i++)
                {
                    for (int j = 0; j < GridScript.Grid[0].Count; j++)
                    {
                        GridSquareScript tile = GridScript.Grid[i][j].GetComponent<GridSquareScript>();
                        if (Mathf.Abs(tile.GridCoordinates.y - RagnallPosition.y) <= 1)
                        {
                            if (TargetPosition.x < RagnallPosition.x && tile.GridCoordinates.x < RagnallPosition.x)
                            {
                                tile.isbossAttackTile = true;
                                tile.BossTileChanged();
                            }
                            else if (TargetPosition.x > RagnallPosition.x && tile.GridCoordinates.x > RagnallPosition.x)
                            {
                                tile.isbossAttackTile = true;
                                tile.BossTileChanged();
                            }
                        }
                    }
                }
            }
        }

    }

    public void DetermineNextAttackTilesRagnallSecondEncounter(GameObject target)
    {
        Character character = GetComponent<UnitScript>().UnitCharacteristics;

        Character targetcharacter = target.GetComponent<UnitScript>().UnitCharacteristics;

        Vector2 RagnallPosition = character.currentTile.GridCoordinates;

        Vector2 TargetPosition = targetcharacter.currentTile.GridCoordinates;

        int distance = ManhanttanDistance(RagnallPosition, TargetPosition);

        if (distance <= 2)
        {
            for (int i = 0; i < GridScript.Grid.Count; i++)
            {
                for (int j = 0; j < GridScript.Grid[0].Count; j++)
                {
                    GridSquareScript tile = GridScript.Grid[i][j].GetComponent<GridSquareScript>();
                    if (ManhanttanDistance(RagnallPosition, tile.GridCoordinates) <= 2 && tile != character.currentTile)
                    {
                        tile.isbossAttackTile = true;
                        tile.BossTileChanged();
                    }
                }
            }
        }
        else if (distance <= 4)
        {
            for (int i = 0; i < GridScript.Grid.Count; i++)
            {
                for (int j = 0; j < GridScript.Grid[0].Count; j++)
                {
                    GridSquareScript tile = GridScript.Grid[i][j].GetComponent<GridSquareScript>();

                    if (targetcharacter.currentTile == tile || ManhanttanDistance(TargetPosition, tile.GridCoordinates) == 2 || ManhanttanDistance(TargetPosition, tile.GridCoordinates) == 3)
                    {
                        tile.isbossAttackTile = true;
                        tile.BossTileChanged();
                    }
                }
            }
        }
        else
        {

            /*
             * oooCooo
             * ooxxxoo
             * oxxxxxo
             * oxxxxxo
             */
            for (int i = 0; i < GridScript.Grid.Count; i++)
            {
                for (int j = 0; j < GridScript.Grid[0].Count; j++)
                {
                    GridSquareScript tile = GridScript.Grid[i][j].GetComponent<GridSquareScript>();
                    if (tile.GridCoordinates.y >= RagnallPosition.y)
                    {
                        continue;

                    }

                    if (tile.GridCoordinates.y == RagnallPosition.y - 1 && Mathf.Abs(RagnallPosition.x - tile.GridCoordinates.x) <= 1)
                    {
                        tile.isbossAttackTile = true;
                        tile.BossTileChanged();
                    }
                    else if (tile.GridCoordinates.y <= RagnallPosition.y - 2 && Mathf.Abs(RagnallPosition.x - tile.GridCoordinates.x) <= 2)
                    {
                        GridSquareScript tileabove = GridScript.GetTile(tile.GridCoordinates + new Vector2(0, 1));
                        if (tileabove != null && tileabove.isobstacle) // ignore tiles below a wall
                        {
                            continue;
                        }
                        tile.isbossAttackTile = true;
                        tile.BossTileChanged();
                        continue;
                    }

                }
            }

        }

    }

    public void DetermineNextAttackTilesKaySecondEncounter(GameObject target)
    {
        Character character = GetComponent<UnitScript>().UnitCharacteristics;

        Character targetcharacter = target.GetComponent<UnitScript>().UnitCharacteristics;

        Vector2 KayPosition = character.currentTile.GridCoordinates;

        Vector2 TargetPosition = targetcharacter.currentTile.GridCoordinates;

        int distance = ManhanttanDistance(KayPosition, TargetPosition);

        if (distance <= 2)
        {
            for (int i = 0; i < GridScript.Grid.Count; i++)
            {
                for (int j = 0; j < GridScript.Grid[0].Count; j++)
                {
                    GridSquareScript tile = GridScript.Grid[i][j].GetComponent<GridSquareScript>();
                    if (ManhanttanDistance(KayPosition, tile.GridCoordinates) <= 2)
                    {
                        if (tile != character.currentTile)
                        {
                            tile.isbossAttackTile = true;
                            tile.BossTileChanged();
                        }

                    }
                    else if (ManhanttanDistance(KayPosition, tile.GridCoordinates) == 3 && (Mathf.Abs(KayPosition.x) == Mathf.Abs(TargetPosition.x) || Mathf.Abs(KayPosition.y) == Mathf.Abs(TargetPosition.y)))
                    {
                        tile.isbossAttackTile = true;
                        tile.BossTileChanged();
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < GridScript.Grid.Count; i++)
            {
                for (int j = 0; j < GridScript.Grid[0].Count; j++)
                {
                    GridSquareScript tile = GridScript.Grid[i][j].GetComponent<GridSquareScript>();

                    GridSquareScript tileabove = GridScript.GetTile(tile.GridCoordinates + new Vector2(0, 1));

                    if (tile.GridCoordinates.y < character.currentTile.GridCoordinates.y)
                    {
                        if (!(tileabove != null && tileabove.isobstacle))
                        {
                            tile.isbossAttackTile = true;
                            tile.BossTileChanged();
                        }

                    }
                }
            }
        }

    }

    public void DetermineNextAttackTilesKayFirstEncounter(GameObject target)
    {
        Character character = GetComponent<UnitScript>().UnitCharacteristics;

        Character targetcharacter = target.GetComponent<UnitScript>().UnitCharacteristics;

        Vector2 KayPosition = character.currentTile.GridCoordinates;

        Vector2 TargetPosition = targetcharacter.currentTile.GridCoordinates;

        int distance = ManhanttanDistance(KayPosition, TargetPosition);

        if (distance <= 2)
        {
            for (int i = 0; i < GridScript.Grid.Count; i++)
            {
                for (int j = 0; j < GridScript.Grid[0].Count; j++)
                {
                    GridSquareScript tile = GridScript.Grid[i][j].GetComponent<GridSquareScript>();
                    if (ManhanttanDistance(KayPosition, tile.GridCoordinates) <= 2)
                    {
                        if (tile != character.currentTile)
                        {
                            tile.isbossAttackTile = true;
                            tile.BossTileChanged();
                        }

                    }
                    else if (ManhanttanDistance(KayPosition, tile.GridCoordinates) == 3 && (Mathf.Abs(KayPosition.x) == Mathf.Abs(TargetPosition.x) || Mathf.Abs(KayPosition.y) == Mathf.Abs(TargetPosition.y)))
                    {
                        tile.isbossAttackTile = true;
                        tile.BossTileChanged();
                    }
                }
            }
        }
        else
        {
            int personnalityValue = GetComponent<RandomScript>().GetPersonalityValue();
            if (personnalityValue <= 50)
            {
                for (int i = 0; i < GridScript.Grid.Count; i++)
                {
                    for (int j = 0; j < GridScript.Grid[0].Count; j++)
                    {
                        GridSquareScript tile = GridScript.Grid[i][j].GetComponent<GridSquareScript>();
                        if (ManhanttanDistance(TargetPosition, tile.GridCoordinates) <= 2)
                        {
                            if (tile != character.currentTile)
                            {
                                tile.isbossAttackTile = true;
                                tile.BossTileChanged();
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < GridScript.Grid.Count; i++)
                {
                    for (int j = 0; j < GridScript.Grid[0].Count; j++)
                    {
                        GridSquareScript tile = GridScript.Grid[i][j].GetComponent<GridSquareScript>();
                        if (ManhanttanDistance(TargetPosition, tile.GridCoordinates) == 0 || ManhanttanDistance(TargetPosition, tile.GridCoordinates) == 2 || ManhanttanDistance(TargetPosition, tile.GridCoordinates) == 4)
                        {
                            if (tile != character.currentTile)
                            {
                                tile.isbossAttackTile = true;
                                tile.BossTileChanged();
                            }
                        }
                    }
                }
            }
        }

    }


    private int ManhanttanDistance(Vector2 a, Vector2 b)
    {
        return (int)(Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y));
    }

}
