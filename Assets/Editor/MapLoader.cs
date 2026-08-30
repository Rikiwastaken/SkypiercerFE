#if UNITY_EDITOR
using System;
using System.Collections.Generic;
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

    public List<Vector2> newplayablepos = new List<Vector2>();

    [Serializable]
    public class AllColors
    {
        public Color wall;
        public Color pit;
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
        public Color TeleporterColor;
        public Color UnitColor;
        public Color FinishTileColor;
    }

    private AllColors colors;

    [MenuItem("Map Edition/Map Creator")]
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
            if (ObstacleMap != null)
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
        if (GridObject.childCount > 0)
        {
            Debug.LogError("GridObject is not empty.");
            return;
        }
        InitializeColors();
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
                ManageRain(newtile, x, y);
                ManageUnitsAndFinish(newtile, x, y);

                if (newplayablepos != null && newplayablepos.Count > 0)
                {
                    MapInitializer.playablepos = newplayablepos;
                }

                newtile.GetComponent<GridSquareScript>().activated = true;
                newtile.transform.parent = GridObject;
                newtile.transform.localRotation = Quaternion.Euler(-90, 0, 0);

                string tilename = "";

                if (newtile.GetComponent<GridSquareScript>().isobstacle)
                {
                    if (newtile.GetComponent<GridSquareScript>().IsPit)
                    {
                        tilename = "Pit";
                    }
                    else

                    {
                        tilename = "wall";
                    }

                }

                else if (newtile.GetComponent<GridSquareScript>().isfinishtile)
                {
                    tilename = "Finish";
                }
                else if (newtile.GetComponent<GridSquareScript>().isstairs)
                {
                    tilename = "stairs";
                }
                else if (newtile.GetComponent<GridSquareScript>().type != "")
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
        if (x == 0)
        {
            pixelColor = ObstacleMap.GetPixel(x + 1, y);
        }


        if (SameColor(pixelColor, colors.wall, 0.1f) || SameColor(pixelColor, colors.pit, 0.1f))
        {
            Tile.GetComponent<GridSquareScript>().isobstacle = true;
        }
        else
        {
            Tile.GetComponent<GridSquareScript>().isobstacle = false;
        }
        if (SameColor(pixelColor, colors.pit, 0.1f))
        {
            Tile.GetComponent<GridSquareScript>().IsPit = true;
        }
    }

    private void ManageUnitsAndFinish(GameObject Tile, int x, int y)
    {
        if (x == 0 || UnitMap == null)
        {
            return;
        }
        Color pixelColor = UnitMap.GetPixel(x, y);

        if (pixelColor.Equals(colors.UnitColor))
        {
            newplayablepos.Add(new Vector2(x, y));

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
        if (ActivationMap == null)
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

    private bool SameColor(Color a, Color b, float eps = 0.005f)
    {
        return Mathf.Abs(a.r - b.r) < eps &&
               Mathf.Abs(a.g - b.g) < eps &&
               Mathf.Abs(a.b - b.b) < eps &&
               Mathf.Abs(a.a - b.a) < eps;
    }

    private void ManageMechanism(GameObject Tile, int x, int y)
    {
        if (MechanismMap == null)
        {
            return;
        }
        if (x != 0)
        {

            Color truepixelColor = MechanismMap.GetPixel(x, y);

            Color pixelColor = ApproximateColor(truepixelColor);

            if (SameColor(pixelColor, colors.LeverColor))
            {
                MechanismClass Mechanism = new MechanismClass();
                Mechanism.type = 2;
                Tile.GetComponent<GridSquareScript>().Mechanism = Mechanism;
            }
            else if (SameColor(pixelColor, colors.DoorColor))
            {
                MechanismClass Mechanism = new MechanismClass();
                Mechanism.type = 1;
                Tile.GetComponent<GridSquareScript>().Mechanism = Mechanism;
            }
            else if (SameColor(pixelColor, colors.TeleporterColor))
            {
                MechanismClass Mechanism = new MechanismClass();
                Mechanism.type = 3;
                Tile.GetComponent<GridSquareScript>().Mechanism = Mechanism;
            }
            if (SameColor(pixelColor, colors.StairsColor))
            {
                Tile.GetComponent<GridSquareScript>().isstairs = true;
            }
            if (SameColor(pixelColor, colors.ForestColor))
            {
                Tile.GetComponent<GridSquareScript>().type = "Forest";
            }
            else if (SameColor(pixelColor, colors.RuinsColor))
            {
                Tile.GetComponent<GridSquareScript>().type = "Ruins";
            }
            else if (SameColor(pixelColor, colors.FireColor))
            {
                Tile.GetComponent<GridSquareScript>().type = "Fire";
            }
            else if (SameColor(pixelColor, colors.WaterColor))
            {
                Tile.GetComponent<GridSquareScript>().type = "Water";
            }
            else if (SameColor(pixelColor, colors.FortificationColor))
            {
                Tile.GetComponent<GridSquareScript>().type = "Fortification";
            }
            else if (SameColor(pixelColor, colors.FogColor))
            {
                Tile.GetComponent<GridSquareScript>().type = "Fog";
            }
            else if (SameColor(pixelColor, colors.MedicinalWaterColor))
            {
                Tile.GetComponent<GridSquareScript>().type = "MedicinalWater";
            }
            else if (SameColor(pixelColor, colors.DesertColor))
            {
                Tile.GetComponent<GridSquareScript>().type = "Desert";
            }
        }
        else
        {
            Color pixelColor = MechanismMap.GetPixel(x + 1, y);
            if (SameColor(pixelColor, colors.StairsColor))
            {
                Tile.GetComponent<GridSquareScript>().isstairs = true;
            }
            else if (SameColor(pixelColor, colors.ForestColor))
            {
                Tile.GetComponent<GridSquareScript>().type = "Forest";
            }
            else if (SameColor(pixelColor, colors.RuinsColor))
            {
                Tile.GetComponent<GridSquareScript>().type = "Ruins";
            }
            else if (SameColor(pixelColor, colors.FireColor))
            {
                Tile.GetComponent<GridSquareScript>().type = "Fire";
            }
            else if (SameColor(pixelColor, colors.WaterColor))
            {
                Tile.GetComponent<GridSquareScript>().type = "Water";
            }
            else if (SameColor(pixelColor, colors.FortificationColor))
            {
                Tile.GetComponent<GridSquareScript>().type = "Fortification";
            }
            else if (SameColor(pixelColor, colors.FogColor))
            {
                Tile.GetComponent<GridSquareScript>().type = "Fog";
            }
            else if (SameColor(pixelColor, colors.MedicinalWaterColor))
            {
                Tile.GetComponent<GridSquareScript>().type = "MedicinalWater";
            }
            else if (SameColor(pixelColor, colors.DesertColor))
            {
                Tile.GetComponent<GridSquareScript>().type = "Desert";
            }

        }
    }

    private Color ApproximateColor(Color color)
    {
        return new Color((float)System.Math.Round(color.r, 2), (float)System.Math.Round(color.g, 2), (float)System.Math.Round(color.b, 2));
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

            if (SameColor(pixelColor, RainColor))
            {
                Tile.GetComponent<GridSquareScript>().RemainingRainTurns = 2;
            }
            else if (SameColor(pixelColor, SunColor))
            {
                Tile.GetComponent<GridSquareScript>().RemainingSunTurns = 2;
            }
        }
        else
        {
            Color pixelColor = RainmMap.GetPixel(x + 1, y);
            if (SameColor(pixelColor, RainColor))
            {
                Tile.GetComponent<GridSquareScript>().RemainingRainTurns = 2;
            }
            else if (SameColor(pixelColor, SunColor))
            {
                Tile.GetComponent<GridSquareScript>().RemainingSunTurns = 2;
            }

        }
    }

    private void ManageElevation(GameObject Tile, int x, int y)
    {

        if (ElevationMap == null)
        {
            Tile.GetComponent<GridSquareScript>().elevation = 0;
            return;
        }

        Color pixelColor = new Color();
        if (x == 0 && y <= 8)
        {
            pixelColor = ElevationMap.GetPixel(x + 1, y);
        }
        else
        {
            pixelColor = ElevationMap.GetPixel(x, y);
        }


        if (SameColor(pixelColor, colors.Elevation0))
        {
            Tile.GetComponent<GridSquareScript>().elevation = 0;
        }
        else if (SameColor(pixelColor, colors.Elevation1))
        {
            Tile.GetComponent<GridSquareScript>().elevation = 1;
        }
        else if (SameColor(pixelColor, colors.Elevation2))
        {
            Tile.GetComponent<GridSquareScript>().elevation = 2;
        }
        else if (SameColor(pixelColor, colors.Elevation3))
        {
            Tile.GetComponent<GridSquareScript>().elevation = 3;
        }
        else if (SameColor(pixelColor, colors.Elevation4))
        {
            Tile.GetComponent<GridSquareScript>().elevation = 4;
        }
        else if (SameColor(pixelColor, colors.ElevationNeg1))
        {
            Tile.GetComponent<GridSquareScript>().elevation = -1;
        }
        else if (SameColor(pixelColor, colors.ElevationNeg2))
        {
            Tile.GetComponent<GridSquareScript>().elevation = -2;
        }
        else if (SameColor(pixelColor, colors.ElevationNeg3))
        {
            Tile.GetComponent<GridSquareScript>().elevation = -3;
        }
        else if (SameColor(pixelColor, colors.ElevationNeg4))
        {
            Tile.GetComponent<GridSquareScript>().elevation = -4;
        }

    }

    private void InitializeColors()
    {

        AllColors NewColor = new AllColors();

        Color wallcolor = ApproximateColor(ObstacleMap.GetPixel(0, 0));
        Color pitcolor = ApproximateColor(ObstacleMap.GetPixel(0, 1));
        NewColor.wall = wallcolor;
        if (pitcolor.g > 0.5f || pitcolor == wallcolor)
        {
            NewColor.pit = Color.blue;
        }
        else
        {
            NewColor.pit = pitcolor;
        }

        if (ElevationMap != null)
        {
            NewColor.Elevation0 = ApproximateColor(ElevationMap.GetPixel(0, 4));
            NewColor.Elevation1 = ApproximateColor(ElevationMap.GetPixel(0, 5));
            NewColor.Elevation2 = ApproximateColor(ElevationMap.GetPixel(0, 6));
            NewColor.Elevation3 = ApproximateColor(ElevationMap.GetPixel(0, 7));
            NewColor.Elevation4 = ApproximateColor(ElevationMap.GetPixel(0, 8));
            NewColor.ElevationNeg1 = ApproximateColor(ElevationMap.GetPixel(0, 3));
            NewColor.ElevationNeg2 = ApproximateColor(ElevationMap.GetPixel(0, 2));
            NewColor.ElevationNeg3 = ApproximateColor(ElevationMap.GetPixel(0, 1));
            NewColor.ElevationNeg4 = ApproximateColor(ElevationMap.GetPixel(0, 0));
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
        NewColor.DesertColor = Color.white;
        NewColor.TeleporterColor = Color.white;

        if (MechanismMap != null)
        {
            NewColor.LeverColor = ApproximateColor(MechanismMap.GetPixel(0, 1));
            NewColor.DoorColor = ApproximateColor(MechanismMap.GetPixel(0, 2));
            NewColor.StairsColor = ApproximateColor(MechanismMap.GetPixel(0, 3));
            NewColor.ForestColor = ApproximateColor(MechanismMap.GetPixel(0, 4));
            NewColor.RuinsColor = ApproximateColor(MechanismMap.GetPixel(0, 5));
            NewColor.FireColor = ApproximateColor(MechanismMap.GetPixel(0, 6));
            NewColor.WaterColor = ApproximateColor(MechanismMap.GetPixel(0, 7));
            NewColor.FortificationColor = ApproximateColor(MechanismMap.GetPixel(0, 8));
            NewColor.FogColor = ApproximateColor(MechanismMap.GetPixel(0, 9));
            NewColor.MedicinalWaterColor = ApproximateColor(MechanismMap.GetPixel(0, 10));
            NewColor.DesertColor = ApproximateColor(MechanismMap.GetPixel(0, 11));
            NewColor.TeleporterColor = ApproximateColor(MechanismMap.GetPixel(0, 12));
        }

        if (UnitMap != null)
        {
            NewColor.UnitColor = ApproximateColor(UnitMap.GetPixel(0, 0));
            NewColor.FinishTileColor = ApproximateColor(UnitMap.GetPixel(0, 1));
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