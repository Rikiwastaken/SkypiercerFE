using UnityEngine;
using static UnitScript;

public class BossScript : MonoBehaviour
{

    private GridScript GridScript;

    public GameObject nextTarget;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GridScript = GridScript.instance;
    }

    public void TriggerBossAttack(GameObject characterwhoblocked = null)
    {

        Debug.Log(nextTarget.name);

        for (int i = 0; i < GridScript.Grid.Count; i++)
        {
            for (int j = 0; j < GridScript.Grid[0].Count; j++)
            {
                GridSquareScript tile = GridScript.Grid[i][j].GetComponent<GridSquareScript>();
                tile.isbossAttackTile = false;
                //dodamage
            }
        }


        switch (GetComponent<UnitScript>().UnitCharacteristics.enemyStats.bossiD)
        {
            case 1:
                DetermineNextAttackTilesRagnall(nextTarget);
                break;
        }
    }


    public void DetermineNextAttackTilesRagnall(GameObject target)
    {
        Character character = GetComponent<UnitScript>().UnitCharacteristics;

        Character targetcharacter = target.GetComponent<UnitScript>().UnitCharacteristics;

        Vector2 RagnallPosition = character.currentTile[0].GridCoordinates;

        Vector2 TargetPosition = targetcharacter.currentTile[0].GridCoordinates;

        int distance = ManhanttanDistance(RagnallPosition, TargetPosition);

        if (distance <= 2)
        {
            for (int i = 0; i < GridScript.Grid.Count; i++)
            {
                for (int j = 0; j < GridScript.Grid[0].Count; j++)
                {
                    GridSquareScript tile = GridScript.Grid[i][j].GetComponent<GridSquareScript>();
                    if (ManhanttanDistance(RagnallPosition, tile.GridCoordinates) <= 2 && tile != character.currentTile[0])
                    {
                        tile.isbossAttackTile = true;
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
                    if (ManhanttanDistance(RagnallPosition, tile.GridCoordinates) <= 4 && ManhanttanDistance(RagnallPosition, tile.GridCoordinates) > 2 && tile != character.currentTile[0])
                    {
                        tile.isbossAttackTile = true;
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
                            }
                            else if (TargetPosition.y > RagnallPosition.y && tile.GridCoordinates.y > RagnallPosition.y)
                            {
                                tile.isbossAttackTile = true;
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
                            }
                            else if (TargetPosition.x > RagnallPosition.x && tile.GridCoordinates.x > RagnallPosition.x)
                            {
                                tile.isbossAttackTile = true;
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
