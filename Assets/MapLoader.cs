using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class MapLoader : MonoBehaviour
{
    public GameObject Tileprefab;
    public Texture2D ObstacleMap;
    public Texture2D ElevationMap;
    public Texture2D ActivationMap;
    public Transform GridObject;
    void Awake()
    {
        
    }

    [ContextMenu("Generate Map From Image (Edit Mode)")]
    void LoadMap()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("Cannot generate persistent map in Play Mode. Exit Play Mode first.");
            return;
        }

        GridSquareScript lasttile = null;
        int number = 0;
        if (GridObject.childCount>0)
        {
            Debug.LogError("GridObject is not empty.");
            return;
        }
        for (int y = 0; y < ObstacleMap.height; y++)
        {
            for (int x = 0; x < ObstacleMap.width; x++)
            {

                Color pixelColor = ObstacleMap.GetPixel(x, y);

                // Skip transparent pixels
                if (pixelColor.a == 0)
                    continue;

                Vector3 position = new Vector3(x, 0, y);

                var newtileObject = PrefabUtility.InstantiatePrefab(Tileprefab);

                GameObject newtile = (GameObject)newtileObject;
                newtile.transform.position = position;

                ManageActivation(newtile, x, y);
                ManageObstable(newtile, x, y);
                ManageElevation(newtile, x, y);

                newtile.GetComponent<GridSquareScript>().activated = true;
                newtile.transform.parent = GridObject;
                newtile.transform.localRotation = Quaternion.Euler(-90,0,0);
                newtile.name = "Tile_" + number;
                
                number++;
                lasttile = newtile.GetComponent<GridSquareScript>();
            }
        }
        FindAnyObjectByType<GridScript>().lastSquare = lasttile;
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }

    [ContextMenu("Delete Map (Edit Mode)")]
    void DeletePreviousMap()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("Cannot generate persistent map in Play Mode. Exit Play Mode first.");
            return;
        }

        while (GridObject.childCount > 0)
        {
            Transform child = GridObject.GetChild(0);
            DestroyImmediate(child.gameObject);
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

    }

    private void ManageObstable(GameObject Tile, int x, int y)
    {
        Color pixelColor = ObstacleMap.GetPixel(x, y);
        (Color wall, Color Elevation0, Color Elevation1, Color Elevation2, Color Elevation3) = InitializeColors();
        if (pixelColor.Equals(wall))
        {
            Tile.GetComponent<GridSquareScript>().isobstacle = true;
        }
        else
        {
            Tile.GetComponent<GridSquareScript>().isobstacle = false;
        }
    }

    private void ManageActivation(GameObject Tile, int x, int y)
    {
        Color pixelColor = ActivationMap.GetPixel(x, y);
        (Color wall, Color Elevation0, Color Elevation1, Color Elevation2, Color Elevation3) = InitializeColors();
        if (pixelColor.Equals(wall))
        {
            Tile.GetComponent<GridSquareScript>().activated = false;
        }
        else
        {
            Tile.GetComponent<GridSquareScript>().activated = true;
        }
    }

    private void ManageElevation(GameObject Tile, int x, int y)
    {
        Color pixelColor = new Color();
        if (x==0 && y<=4)
        {
            pixelColor = ElevationMap.GetPixel(x+1, y);
        }
        else
        {
            pixelColor = ElevationMap.GetPixel(x, y);
        }


        (Color wall, Color Elevation0, Color Elevation1, Color Elevation2, Color Elevation3) = InitializeColors();
        if (pixelColor.Equals(Elevation0))
        {
            Tile.GetComponent<GridSquareScript>().elevation = 0;
        }
        else if (pixelColor.Equals(Elevation1))
        {
            Tile.GetComponent<GridSquareScript>().elevation = 1;
        }
        else if (pixelColor.Equals(Elevation2))
        {
            Tile.GetComponent<GridSquareScript>().elevation = 2;
        }
        else if (pixelColor.Equals(Elevation3))
        {
            Tile.GetComponent<GridSquareScript>().elevation = 3;
        }
        
    }

    private (Color,Color,Color,Color,Color) InitializeColors()
    {
        Color grey = ObstacleMap.GetPixel(0, 0);
        Color Elevation3 = ElevationMap.GetPixel(0, 1);
        Color Elevation2 = ElevationMap.GetPixel(0, 2);
        Color Elevation1 = ElevationMap.GetPixel(0, 3);
        Color Elevation0 = ElevationMap.GetPixel(0, 4);
        return (grey,Elevation0,Elevation1,Elevation2,Elevation3);
    }
}
