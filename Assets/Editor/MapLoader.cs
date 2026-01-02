#if UNITY_EDITOR
using System.Collections.Generic;
using System;
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
    private Texture2D RainmMap;
    private Texture2D UnitMap;
    private Transform GridObject;
    private MapInitializer MapInitializer;

    [Serializable]
    public class AllColors
    {
        public Color wall;
        public Color Elevation0;
        public Color Elevation1;
        public Color Elevation2;
        public Color Elevation3;
        public Color Elevation4;
        public Color ElevationNeg1;
        public Color ElevationNeg2;
        public Color ElevationNeg3;
        public Color ElevationNeg4;
        public Color LeverColor;
        public Color DoorColor;
        public Color StairsColor;
        public Color ForestColor;
        public Color RuinsColor;
        public Color FireColor;
        public Color WaterColor;
        public Color FortificationColor;
        public Color FogColor;
        public Color MedicinalWaterColor;
        public Color DesertColor;
        public Color UnitColor;
        public Color FinishTileColor;
    }

    private AllColors colors;

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
        MapInitializer = (MapInitializer)EditorGUILayout.ObjectField("Map Initialiszer Script", MapInitializer, typeof(MapInitializer), true);

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

        // Allow manual prefab assignment
        RainmMap = (Texture2D)EditorGUILayout.ObjectField("Rain Map Image", RainmMap, typeof(Texture2D), false);

        // Allow manual prefab assignment
        UnitMap = (Texture2D)EditorGUILayout.ObjectField("Unit Map Image", UnitMap, typeof(Texture2D), false);


        if (GUILayout.Button("Create Map"))
        {
            if(ObstacleMap!=null)
            {
                FindGridSquareScriptPrefab();
                LoadMap();
            }
            else
            {
                EditorGUILayout.HelpBox("Please add an obstacle map.", MessageType.Warning);
            }
            
        }
            

        if (GUILayout.Button("Delete Map"))
            DeletePreviousMap();

        EditorGUILayout.Space();

      
    }

    private void RefreshTarget()
    {
        if (GridObject == null)
        {
            GridObject = GameObject.Find("Grid").transform;
        }

        if (MapInitializer == null)
        {
            MapInitializer = FindAnyObjectByType<MapInitializer>();
        }

        MapInitializer.playablepos = new List<Vector2>();

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

                InitializeColors();

                ManageActivation(newtile, x, y);
                ManageObstable(newtile, x, y);
                ManageElevation(newtile, x, y);
                ManageMechanism(newtile, x, y);
                ManageRain(newtile, x, y);

                newtile.GetComponent<GridSquareScript>().activated = true;
                newtile.transform.parent = GridObject;
                newtile.transform.localRotation = Quaternion.Euler(-90,0,0);

                string tilename = "";

                if(newtile.GetComponent<GridSquareScript>().isobstacle)
                {
                    tilename = "wall";
                }
                else if (newtile.GetComponent<GridSquareScript>().isfinishtile)
                {
                    tilename = "Finish";
                }
                else if (newtile.GetComponent<GridSquareScript>().isstairs)
                {
                    tilename = "stairs";
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
        if (x==0)
        {
            pixelColor = ObstacleMap.GetPixel(x+1, y);
        }

        
        
        if (pixelColor.Equals(colors.wall))
        {
            Tile.GetComponent<GridSquareScript>().isobstacle = true;
        }
        else
        {
            Tile.GetComponent<GridSquareScript>().isobstacle = false;
        }
    }

    private void ManageUnitsAndFinish(GameObject Tile, int x, int y)
    {
        if (x == 0)
        {
            return;
        }
        Color pixelColor = UnitMap.GetPixel(x, y);

        if (pixelColor.Equals(colors.UnitColor))
        {
            MapInitializer.playablepos.Add(new Vector2(x, y));
        }


        if (pixelColor.Equals(colors.FinishTileColor))
        {
            Tile.GetComponent<GridSquareScript>().isfinishtile = true;
        }
        else
        {
            Tile.GetComponent<GridSquareScript>().isfinishtile = false;
        }




    }

    private void ManageActivation(GameObject Tile, int x, int y)
    {
        if(ActivationMap==null)
        {
            Tile.GetComponent<GridSquareScript>().activated = true;
            return;
        }
        Color pixelColor = ActivationMap.GetPixel(x, y);
        
        if (pixelColor.Equals(colors.wall))
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
        if(MechanismMap==null)
        {
            return ;
        }
        if (x != 0)
        {
            Color pixelColor = MechanismMap.GetPixel(x, y);

            if (pixelColor.Equals(colors.LeverColor))
            {
                MechanismClass Mechanism = new MechanismClass();
                Mechanism.type = 2;
                Tile.GetComponent<GridSquareScript>().Mechanism = Mechanism;
            }
            else if (pixelColor.Equals(colors.DoorColor))
            {
                MechanismClass Mechanism = new MechanismClass();
                Mechanism.type = 1;
                Tile.GetComponent<GridSquareScript>().Mechanism = Mechanism;
            }
            else if (pixelColor.Equals(colors.StairsColor))
            {
                Debug.Log("stairsfound");
                Tile.GetComponent<GridSquareScript>().isstairs = true;
            }
            else if (pixelColor.Equals(colors.ForestColor))
            {
                Tile.GetComponent<GridSquareScript>().type = "Forest";
            }
            else if (pixelColor.Equals(colors.RuinsColor))
            {
                Tile.GetComponent<GridSquareScript>().type = "Ruins";
            }
            else if (pixelColor.Equals(colors.FireColor))
            {
                Tile.GetComponent<GridSquareScript>().type = "Fire";
            }
            else if (pixelColor.Equals(colors.WaterColor))
            {
                Tile.GetComponent<GridSquareScript>().type = "Water";
            }
            else if (pixelColor.Equals(colors.FortificationColor))
            {
                Tile.GetComponent<GridSquareScript>().type = "Fortification";
            }
            else if (pixelColor.Equals(colors.FogColor))
            {
                Tile.GetComponent<GridSquareScript>().type = "Fog";
            }
            else if (pixelColor.Equals(colors.MedicinalWaterColor))
            {
                Tile.GetComponent<GridSquareScript>().type = "MedicinalWater";
            }
        }
        else
        {
            Color pixelColor = MechanismMap.GetPixel(x + 1, y);
            if (pixelColor.Equals(colors.StairsColor))
            {
                Debug.Log("stairsfound");
                Tile.GetComponent<GridSquareScript>().isstairs = true;
            }
            else if (pixelColor.Equals(colors.ForestColor))
            {
                Tile.GetComponent<GridSquareScript>().type = "Forest";
            }
            else if (pixelColor.Equals(colors.RuinsColor))
            {
                Tile.GetComponent<GridSquareScript>().type = "Ruins";
            }
            else if (pixelColor.Equals(colors.FireColor))
            {
                Tile.GetComponent<GridSquareScript>().type = "Fire";
            }
            else if (pixelColor.Equals(colors.WaterColor))
            {
                Tile.GetComponent<GridSquareScript>().type = "Water";
            }
            else if (pixelColor.Equals(colors.FortificationColor))
            {
                Tile.GetComponent<GridSquareScript>().type = "Fortification";
            }
            else if (pixelColor.Equals(colors.FogColor))
            {
                Tile.GetComponent<GridSquareScript>().type = "Fog";
            }
            else if (pixelColor.Equals(colors.MedicinalWaterColor))
            {
                Tile.GetComponent<GridSquareScript>().type = "MedicinalWater";
            }

        }
    }

    private void ManageRain(GameObject Tile, int x, int y)
    {
        if (RainmMap == null)
        {
            return;
        }

        Color RainColor = RainmMap.GetPixel(0, 1);
        Color SunColor = RainmMap.GetPixel(0, 2);


        if (x != 0)
        {
            Color pixelColor = RainmMap.GetPixel(x, y);

            if (pixelColor.Equals(RainColor))
            {
                Tile.GetComponent<GridSquareScript>().RemainingRainTurns = 3;
            }
            else if (pixelColor.Equals(SunColor))
            {
                Tile.GetComponent<GridSquareScript>().RemainingSunTurns = 3;
            }
        }
        else
        {
            Color pixelColor = RainmMap.GetPixel(x + 1, y);
            if (pixelColor.Equals(RainColor))
            {
                Tile.GetComponent<GridSquareScript>().RemainingRainTurns = 3;
            }
            else if (pixelColor.Equals(SunColor))
            {
                Tile.GetComponent<GridSquareScript>().RemainingSunTurns = 3;
            }

        }
    }

    private void ManageElevation(GameObject Tile, int x, int y)
    {

        if(ElevationMap==null)
        {
            Tile.GetComponent<GridSquareScript>().elevation = 0;
            return;
        }

        Color pixelColor = new Color();
        if (x==0 && y<=8)
        {
            pixelColor = ElevationMap.GetPixel(x+1, y);
        }
        else
        {
            pixelColor = ElevationMap.GetPixel(x, y);
        }


        if (pixelColor.Equals(colors.Elevation0))
        {
            Tile.GetComponent<GridSquareScript>().elevation = 0;
        }
        else if (pixelColor.Equals(colors.Elevation1))
        {
            Tile.GetComponent<GridSquareScript>().elevation = 1;
        }
        else if (pixelColor.Equals(colors.Elevation2))
        {
            Tile.GetComponent<GridSquareScript>().elevation = 2;
        }
        else if (pixelColor.Equals(colors.Elevation3))
        {
            Tile.GetComponent<GridSquareScript>().elevation = 3;
        }
        else if (pixelColor.Equals(colors.Elevation4))
        {
            Tile.GetComponent<GridSquareScript>().elevation = 4;
        }
        else if (pixelColor.Equals(colors.ElevationNeg1))
        {
            Tile.GetComponent<GridSquareScript>().elevation = -1;
        }
        else if (pixelColor.Equals(colors.ElevationNeg2))
        {
            Tile.GetComponent<GridSquareScript>().elevation = -2;
        }
        else if (pixelColor.Equals(colors.ElevationNeg3))
        {
            Tile.GetComponent<GridSquareScript>().elevation = -3;
        }
        else if (pixelColor.Equals(colors.ElevationNeg4))
        {
            Tile.GetComponent<GridSquareScript>().elevation = -4;
        }
        else
        {
            Debug.LogError("Unrecognized color in Elevation Map at (" + x + "," + y + "): " + pixelColor);
        }
        
    }

    private void InitializeColors()
    {

        AllColors NewColor = new AllColors();

        NewColor.wall = ObstacleMap.GetPixel(0, 0);
        if(ElevationMap!=null)
        {
            NewColor.Elevation0 = ElevationMap.GetPixel(0, 4);
            NewColor.Elevation1 = ElevationMap.GetPixel(0, 5);
            NewColor.Elevation2 = ElevationMap.GetPixel(0, 6);
            NewColor.Elevation3 = ElevationMap.GetPixel(0, 7);
            NewColor.Elevation4 = ElevationMap.GetPixel(0, 8);
            NewColor.ElevationNeg1 = ElevationMap.GetPixel(0, 3);
            NewColor.ElevationNeg2 = ElevationMap.GetPixel(0, 2);
            NewColor.ElevationNeg3 = ElevationMap.GetPixel(0, 1);
            NewColor.ElevationNeg4 = ElevationMap.GetPixel(0, 0);
        }
        
        NewColor.LeverColor = Color.white;
        NewColor.DoorColor = Color.white;
        NewColor.StairsColor = Color.white;
        NewColor.ForestColor = Color.white;
        NewColor.RuinsColor = Color.white;
        NewColor.FireColor = Color.white;
        NewColor.WaterColor = Color.white;
        NewColor.FortificationColor = Color.white;
        NewColor.FogColor = Color.white;
        NewColor.MedicinalWaterColor = Color.white;

        if (MechanismMap!=null)
        {
            NewColor.LeverColor = MechanismMap.GetPixel(0, 1);
            NewColor.DoorColor = MechanismMap.GetPixel(0, 2);
            NewColor.StairsColor = MechanismMap.GetPixel(0, 3);
            NewColor.ForestColor = MechanismMap.GetPixel(0, 4);
            NewColor.RuinsColor = MechanismMap.GetPixel(0, 5);
            NewColor.FireColor = MechanismMap.GetPixel(0, 6);
            NewColor.WaterColor = MechanismMap.GetPixel(0, 7);
            NewColor.FortificationColor = MechanismMap.GetPixel(0, 8);
            NewColor.FogColor = MechanismMap.GetPixel(0, 9);
            NewColor.MedicinalWaterColor = MechanismMap.GetPixel(0, 10);
        }

        if(UnitMap=null)
        {
            NewColor.UnitColor = UnitMap.GetPixel(0, 0);
            NewColor.FinishTileColor = UnitMap.GetPixel(0, 1);
        }

        colors = NewColor; 
    }

    private void FindGridSquareScriptPrefab()
    {
        Tileprefab = null;

        // Search all prefabs in project
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                // Include inactive children
                GridSquareScript[] tmsArray = prefab.GetComponentsInChildren<GridSquareScript>(true);
                if (tmsArray.Length > 0)
                {
                    Tileprefab = prefab;
                    break;
                }
            }
        }
    }
}
#endif