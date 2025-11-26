#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static GridSquareScript;

public class MapLoader : EditorWindow
{
    private GameObject Tileprefab;
    private Texture2D ObstacleMap;
    private Texture2D ElevationMap;
    private Texture2D ActivationMap;
    private Texture2D MechanismMap;
    private Transform GridObject;


    [MenuItem("Tools/Map Creator")]
    public static void ShowWindow()
    {
        var w = GetWindow<MapLoader>("Map Editor");
        w.minSize = new Vector2(650, 500);
    }

    private void OnEnable()
    {
        RefreshTarget();
    }

    private void OnHierarchyChange()
    {
        RefreshTarget();
        Repaint();
    }

    private void OnGUI()
    {

        EditorGUILayout.Space();

        // Allow manual prefab assignment
        GridObject = (Transform)EditorGUILayout.ObjectField("Grid Transform", GridObject, typeof(Transform), true);

        EditorGUILayout.Space();

        // Allow manual prefab assignment
        Tileprefab = (GameObject)EditorGUILayout.ObjectField("Tile Prefab", Tileprefab, typeof(GameObject), false);

        EditorGUILayout.Space();

        // Allow manual prefab assignment
        ObstacleMap = (Texture2D)EditorGUILayout.ObjectField("Obstacle Map Image", ObstacleMap, typeof(Texture2D), false);

        EditorGUILayout.Space();

        // Allow manual prefab assignment
        ElevationMap = (Texture2D)EditorGUILayout.ObjectField("Elevation Map Image", ElevationMap, typeof(Texture2D), false);

        EditorGUILayout.Space();

        // Allow manual prefab assignment
        ActivationMap = (Texture2D)EditorGUILayout.ObjectField("Activation Map Image", ActivationMap, typeof(Texture2D), false);

        // Allow manual prefab assignment
        MechanismMap = (Texture2D)EditorGUILayout.ObjectField("Mechanism Map Image", MechanismMap, typeof(Texture2D), false);


        if (GUILayout.Button("Create Map"))
        {
            if (ObstacleMap == null)
            {
                EditorGUILayout.HelpBox("Please add an obstacle map.", MessageType.Warning);
            }
            else if (ElevationMap == null)
            {
                EditorGUILayout.HelpBox("Please add an elevation map.", MessageType.Warning);
            }
            else if (ActivationMap == null)
            {
                EditorGUILayout.HelpBox("Please add an activation map.", MessageType.Warning);
            }
            else
            {
                LoadMap();
            } 
        }
            

        if (GUILayout.Button("Delete Map"))
            DeletePreviousMap();

        EditorGUILayout.Space();

      
    }

    private void RefreshTarget()
    {
        if (GridObject == null)
            GridObject = GameObject.Find("Grid").transform;

        if (Tileprefab == null)
        {
            // Load prefab from project if not in scene
            string[] guids = AssetDatabase.FindAssets("gridsquare");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                Tileprefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
        }
    }
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

             

                Vector3 position = new Vector3(x, 0, y);

                var newtileObject = PrefabUtility.InstantiatePrefab(Tileprefab);

                GameObject newtile = (GameObject)newtileObject;
                newtile.transform.position = position;

                ManageActivation(newtile, x, y);
                ManageObstable(newtile, x, y);
                ManageElevation(newtile, x, y);
                ManageMechanism(newtile, x, y);

                newtile.GetComponent<GridSquareScript>().activated = true;
                newtile.transform.parent = GridObject;
                newtile.transform.localRotation = Quaternion.Euler(-90,0,0);

                string tilename = "";

                if(newtile.GetComponent<GridSquareScript>().isobstacle)
                {
                    tilename = "wall";
                }
                else if(newtile.GetComponent<GridSquareScript>().type!="")
                {
                    tilename = newtile.GetComponent<GridSquareScript>().type;
                }
                else
                {
                    tilename = "tile";
                }

                if (newtile.GetComponent<GridSquareScript>().Mechanism != null)
                {
                    if (newtile.GetComponent<GridSquareScript>().Mechanism.type == 1)
                    {
                        tilename = "door";
                    }
                    if (newtile.GetComponent<GridSquareScript>().Mechanism.type == 2)
                    {
                        tilename = "lever";
                    }
                }

                tilename += "_" + number;
                newtile.name = tilename;
                
                number++;
                lasttile = newtile.GetComponent<GridSquareScript>();
            }
        }
        GameObject.FindAnyObjectByType<GridScript>().lastSquare = lasttile;
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
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
        (Color wall, Color Elevation0, Color Elevation1, Color Elevation2, Color Elevation3, Color Elevation4, Color ElevationNeg1, Color ElevationNeg2, Color ElevationNeg3, Color ElevationNeg4, Color LeverColor, Color DoorColor, Color StairsColor) = InitializeColors();
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
        (Color wall, Color Elevation0, Color Elevation1, Color Elevation2, Color Elevation3, Color Elevation4, Color ElevationNeg1, Color ElevationNeg2, Color ElevationNeg3, Color ElevationNeg4, Color LeverColor, Color DoorColor, Color StairsColor) = InitializeColors();
        if (pixelColor.Equals(wall))
        {
            Tile.GetComponent<GridSquareScript>().activated = false;
        }
        else
        {
            Tile.GetComponent<GridSquareScript>().activated = true;
        }
    }

    private void ManageMechanism(GameObject Tile, int x, int y)
    {
        if(MechanismMap!=null)
        {
            if(x!=0)
            {
                Color pixelColor = MechanismMap.GetPixel(x, y);
                (Color wall, Color Elevation0, Color Elevation1, Color Elevation2, Color Elevation3, Color Elevation4, Color ElevationNeg1, Color ElevationNeg2, Color ElevationNeg3, Color ElevationNeg4, Color LeverColor, Color DoorColor, Color StairsColor) = InitializeColors();
                if (pixelColor.Equals(LeverColor))
                {
                    MechanismClass Mechanism = new MechanismClass();
                    Mechanism.type = 2;
                    Tile.GetComponent<GridSquareScript>().Mechanism = Mechanism;
                }
                else if (pixelColor.Equals(DoorColor))
                {
                    MechanismClass Mechanism = new MechanismClass();
                    Mechanism.type = 1;
                    Tile.GetComponent<GridSquareScript>().Mechanism = Mechanism;
                }
                else if (pixelColor.Equals(StairsColor))
                {
                    Debug.Log("stairsfound");
                    Tile.GetComponent<GridSquareScript>().isstairs = true;
                }
            }
        }
    }

    private void ManageElevation(GameObject Tile, int x, int y)
    {
        Color pixelColor = new Color();
        if (x==0 && y<=8)
        {
            pixelColor = ElevationMap.GetPixel(x+1, y);
        }
        else
        {
            pixelColor = ElevationMap.GetPixel(x, y);
        }


        (Color wall, Color Elevation0, Color Elevation1, Color Elevation2, Color Elevation3, Color Elevation4, Color ElevationNeg1, Color ElevationNeg2, Color ElevationNeg3, Color ElevationNeg4, Color LeverColor, Color DoorColor, Color StairsColor) = InitializeColors();
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
        else if (pixelColor.Equals(Elevation4))
        {
            Tile.GetComponent<GridSquareScript>().elevation = 4;
        }
        else if (pixelColor.Equals(ElevationNeg1))
        {
            Tile.GetComponent<GridSquareScript>().elevation = -1;
        }
        else if (pixelColor.Equals(ElevationNeg2))
        {
            Tile.GetComponent<GridSquareScript>().elevation = -2;
        }
        else if (pixelColor.Equals(ElevationNeg3))
        {
            Tile.GetComponent<GridSquareScript>().elevation = -3;
        }
        else if (pixelColor.Equals(ElevationNeg4))
        {
            Tile.GetComponent<GridSquareScript>().elevation = -4;
        }
        else
        {
            Debug.LogError("Unrecognized color in Elevation Map at (" + x + "," + y + "): " + pixelColor);
        }
        
    }

    private (Color,Color,Color,Color,Color, Color, Color, Color, Color, Color, Color, Color, Color) InitializeColors()
    {
        Color grey = ObstacleMap.GetPixel(0, 0);
        Color Elevation4 = ElevationMap.GetPixel(0, 8);
        Color Elevation3 = ElevationMap.GetPixel(0, 7);
        Color Elevation2 = ElevationMap.GetPixel(0, 6);
        Color Elevation1 = ElevationMap.GetPixel(0, 5);
        Color Elevation0 = ElevationMap.GetPixel(0, 4);
        Color ElevationNeg1 = ElevationMap.GetPixel(0, 3);
        Color ElevationNeg2 = ElevationMap.GetPixel(0, 2);
        Color ElevationNeg3= ElevationMap.GetPixel(0, 1);
        Color ElevationNeg4 = ElevationMap.GetPixel(0, 0);
        Color Trigger = Color.white;
        Color Door = Color.white;
        Color Stairs = Color.white;
        if (MechanismMap!=null)
        {
            Trigger = MechanismMap.GetPixel(0, 1);
            Door = MechanismMap.GetPixel(0, 2);
            Stairs = MechanismMap.GetPixel(0, 3);
            
        }
        return (grey,Elevation0,Elevation1,Elevation2,Elevation3, Elevation4, ElevationNeg1, ElevationNeg2, ElevationNeg3, ElevationNeg4, Trigger, Door, Stairs);
    }
}
#endif