using System.Collections.Generic;
using UnityEngine;

public class PathVisualsScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Sprite PathPiece;
    public Sprite PathPieceEnd;
    public Sprite emptysprite;
    public Transform PathPiecesHolder;

    public List<GameObject> PathGOList;

    private List<GridSquareScript> path;

    public float elevation;

    private List<GridSquareScript> lastpath;

    void Start()
    {
        PathGOList = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        path = ActionManager.instance.currentpath;

        if (lastpath != path && path != null)
        {
            ManagePath();
        }

        lastpath = path;

    }

    private void InitializeGOList()
    {

        int PathGOListLength = PathGOList.Count;

        int safeguard = 0;

        while (path.Count != PathGOListLength && safeguard < 100)
        {
            if (PathGOListLength > path.Count)
            {
                GameObject lastobject = PathGOList[PathGOList.Count - 1];
                PathGOList.Remove(lastobject);
                Destroy(lastobject);
                PathGOListLength = PathGOList.Count;
            }
            else
            {
                GameObject newpiece = new GameObject();
                newpiece.transform.parent = PathPiecesHolder;
                newpiece.AddComponent<SpriteRenderer>();
                newpiece.layer = LayerMask.NameToLayer("Grid");
                GameObject newpiecechild = new GameObject();
                newpiecechild.transform.parent = newpiece.transform;
                newpiecechild.AddComponent<SpriteRenderer>();
                newpiecechild.layer = LayerMask.NameToLayer("Grid");
                PathGOList.Add(newpiece);
                PathGOListLength = PathGOList.Count;
            }

            safeguard++;
        }

        for (int i = 0; i < path.Count; i++)
        {

            GridSquareScript tile = path[i];
            PathGOList[i].transform.position = new Vector3(tile.GridCoordinates.x, elevation + tile.transform.position.y, tile.GridCoordinates.y);
        }

    }

    private void ManagePath()
    {

        InitializeGOList();

        for (int i = 0; i < path.Count; i++)
        {

            SpriteRenderer firstpart = PathGOList[i].GetComponent<SpriteRenderer>();
            if (i == 0)
            {
                firstpart.sprite = emptysprite;
            }
            else
            {

                firstpart.sprite = PathPiece;
            }


            SpriteRenderer secondpart = PathGOList[i].transform.GetChild(0).GetComponent<SpriteRenderer>();
            secondpart.sprite = PathPiece;
            if (i > 0)
            {
                firstpart.sprite = PathPiece;
                Vector3 direction = path[i - 1].transform.position - path[i].transform.position;
                direction.y = 0f; // ignore vertical offset
                direction.Normalize(); // optional, just to avoid scaling issues
                float angleY = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                firstpart.transform.rotation = Quaternion.Euler(90f, angleY, 0f);



            }
            if (i < path.Count - 1)
            {
                secondpart.sprite = PathPiece;
                Vector3 direction = path[i + 1].transform.position - path[i].transform.position;
                direction.y = 0f; // ignore vertical offset
                direction.Normalize(); // optional, just to avoid scaling issues
                float angleY = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                secondpart.transform.rotation = Quaternion.Euler(90f, angleY, 0f);

            }
            if (i == path.Count - 1)
            {
                secondpart.sprite = PathPieceEnd;
                Vector3 direction = path[i].transform.position - path[i - 1].transform.position;
                direction.y = 0f; // ignore vertical offset
                direction.Normalize(); // optional, just to avoid scaling issues
                float angleY = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                secondpart.transform.rotation = Quaternion.Euler(-90f, angleY, 0f);
            }

        }
    }
}
