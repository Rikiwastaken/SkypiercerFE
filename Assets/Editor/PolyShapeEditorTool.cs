
using System.Collections.Generic;
using System.Linq;

using UnityEditor;

using UnityEngine;
using UnityEngine.ProBuilder;


public class CustomPolygonTool : EditorWindow
{
    private List<Vector3> points = new List<Vector3>()
    {
    };




    private float extrudeHeight = 1f;
    private Material faceMaterial;
    private Vector2 scrollPos;
    private int zoneindex;
    private List<List<GridSquareScript>> zones;
    private List<List<GridSquareScript>> Grid;

    private int percentagetomakesummit;
    private int maxheight;

    [MenuItem("Tools/Create Custom Polygon")]
    public static void ShowWindow() => GetWindow<CustomPolygonTool>("Polygon Tool");


    void OnGUI()
    {
        GUILayout.Label("Polygon Creator", EditorStyles.boldLabel);

        extrudeHeight = EditorGUILayout.FloatField("Extrude Height", extrudeHeight);
        zoneindex = EditorGUILayout.IntField("Select Zone", zoneindex);
        percentagetomakesummit = EditorGUILayout.IntField("Chance to make a summit (%)", percentagetomakesummit);
        maxheight = EditorGUILayout.IntField("Max Height", maxheight);
        faceMaterial = (Material)EditorGUILayout.ObjectField("Material", faceMaterial, typeof(Material), false);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
        for (int i = 0; i < points.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            points[i] = EditorGUILayout.Vector3Field($"Point {i}", points[i]);
            if (GUILayout.Button("X", GUILayout.Width(20))) { points.RemoveAt(i); i--; }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Add Point")) points.Add(Vector3.zero);
        if (GUILayout.Button("Create Polygon Mesh")) CreatePolygon();
        if (GUILayout.Button("Create Polygon Mesh From Zone")) CreatePolygoneFromZones();
    }


    void GetZones()
    {
        InstantiateGrid();
        zones = new List<List<GridSquareScript>>();
        for(int x = 0;x<Grid.Count;x++)
        {
            for (int y = 0; y < Grid.Count; y++)
            {
                bool foundlist = false;
                GridSquareScript tiletocheck = Grid[x][y];
                if(tiletocheck.isobstacle)
                {
                    foreach (List<GridSquareScript> zone in zones)
                    {
                        if (tiletocheck.elevation == zone[0].elevation)
                        {
                            foreach (GridSquareScript tile in zone)
                            {
                                if (ManhattanDistance(tile.GridCoordinates, tiletocheck.GridCoordinates) < 2)
                                {
                                    zone.Add(tiletocheck);
                                    foundlist = true;
                                    break;
                                }
                            }
                        }
                        if (foundlist)
                        {
                            break;
                        }
                    }
                    if (!foundlist)
                    {
                        List<GridSquareScript> newzone = new List<GridSquareScript>() { tiletocheck };
                        zones.Add(newzone);
                    }
                }
                
            }
        }

    }

    int ManhattanDistance(Vector2 point1, Vector2 point2)
    {
        return (int)(Mathf.Abs(point1.x - point2.x) + Mathf.Abs(point1.y - point2.y));
    }

    void InstantiateGrid()
    {
        GameObject GridHolder = GameObject.Find("Grid");
        Grid = new List<List<GridSquareScript>>();
        GridSquareScript[] tilelist = FindObjectsByType<GridSquareScript>(FindObjectsSortMode.None);
        GridSquareScript lastSquare = GridHolder.transform.GetChild(GameObject.Find("Grid").transform.childCount - 1).GetComponent<GridSquareScript>();
        lastSquare.InitializePosition();
        for (int x = 0; x <= lastSquare.GridCoordinates.x; x++)
        {
            Grid.Add(new List<GridSquareScript>());
            for (int y = 0; y <= lastSquare.GridCoordinates.y; y++)
            {
                foreach (GridSquareScript tile in tilelist)
                {
                    tile.InitializePosition();
                    if ((int)tile.GridCoordinates.x == x && (int)tile.GridCoordinates.y == y)
                    {
                        Grid[x].Add(tile);
                    }
                }
            }
        }
    }

    void CreatePolygoneFromZones()
    {
        GetZones();
        List<GridSquareScript> zonetouse = zones[zoneindex];
        List<GridSquareScript> onlyoutside = RemoveInside(zonetouse);

        List<GridSquareScript> onlyinside = new List<GridSquareScript>();
        List<GameObject> whattodelete = new List<GameObject>();
        foreach (GridSquareScript tile in zonetouse)
        {
            if (!onlyoutside.Contains(tile))
            {
                GameObject newtileGO = Instantiate(tile.gameObject);
                DestroyImmediate(newtileGO.GetComponent<GridSquareScript>());
                newtileGO.AddComponent<GridSquareScript>();
                
                GridSquareScript newtile = newtileGO.GetComponent<GridSquareScript>();
                newtile.GridCoordinates = tile.GridCoordinates;
                newtile.elevation = Random.Range(0,maxheight);
                onlyinside.Add(newtile);
                whattodelete.Add(newtileGO);
            }
        }

        List<GridSquareScript> newonlyoutside = new List<GridSquareScript>();
        foreach (GridSquareScript tile in onlyoutside)
        {
            GameObject newtileGO = Instantiate(tile.gameObject);
            DestroyImmediate(newtileGO.GetComponent<GridSquareScript>());
            newtileGO.AddComponent<GridSquareScript>();

            GridSquareScript newtile = newtileGO.GetComponent<GridSquareScript>();
            newtile.GridCoordinates = tile.GridCoordinates;
            newtile.elevation = 0;
            newonlyoutside.Add(newtile);
            whattodelete.Add(newtileGO);
        }
        onlyoutside = newonlyoutside;

        List<Vector2> edges = GetOuterEdgesOfSquares(onlyoutside, 1);

        Debug.Log("onlyinside : "+onlyinside.Count);

        List<List<GridSquareScript>> elevationList = new List<List<GridSquareScript>>() { onlyoutside };

        // Build fast lookup of onlyInside by elevation
        Dictionary<int, List<GridSquareScript>> insideByElevation = new Dictionary<int, List<GridSquareScript>>();
        foreach (var t in onlyinside)
        {
            int e = t.elevation;
            if (!insideByElevation.ContainsKey(e)) insideByElevation[e] = new List<GridSquareScript>();
            insideByElevation[e].Add(t);
        }

        float shrinkFactor = 0.9f; // 0..1, lower => faster shrink. Tweak to taste.

        var uniqueElevations = insideByElevation.Keys.OrderBy(e => e).ToList();

        foreach (int elevation in uniqueElevations)
        {
            if(elevation == 0)
            {
                continue;
            }
            List<GridSquareScript> prevCrown = elevationList.LastOrDefault();
            if (prevCrown == null || prevCrown.Count == 0) break;

            List<GridSquareScript> candidates = insideByElevation[elevation];
            if (candidates == null || candidates.Count == 0) continue;

            // Same centroid / shrink logic as before
            List<Vector2> prevPts = prevCrown
                .Select(t => new Vector2(t.GridCoordinates.x, t.GridCoordinates.y))
                .Distinct()
                .ToList();

            List<Vector2> polygon = OrderAroundCentroid(new List<Vector2>(prevPts));

            Vector2 centroid = Vector2.zero;
            foreach (var p in prevPts) centroid += p;
            centroid /= prevPts.Count;

            float avgDist = prevPts.Average(p => Vector2.Distance(p, centroid));
            float maxAllowedDist = avgDist * shrinkFactor;

            HashSet<GridSquareScript> newCrownSet = new HashSet<GridSquareScript>();

            foreach (var tile in candidates)
            {
                Vector2 tpos = new Vector2(tile.GridCoordinates.x, tile.GridCoordinates.y);

                if (!PointInPolygon(tpos, polygon)) continue;
                if (Vector2.Distance(tpos, centroid) > maxAllowedDist) continue;

                bool hasNeighbor = prevCrown.Any(prev =>
                    Mathf.Abs(prev.GridCoordinates.x - tile.GridCoordinates.x) <= 1 &&
                    Mathf.Abs(prev.GridCoordinates.y - tile.GridCoordinates.y) <= 1);

                if (!hasNeighbor)
                {
                    //Debug.Log(tile.GridCoordinates+" has no neighbors");
                    //continue;
                }

                if (Random.Range(0f, 100f) <= percentagetomakesummit)
                {
                    newCrownSet.Add(tile);
                }
                    
            }

            if (newCrownSet.Count > 0)
            { 
                List<Vector2> hulllist = new List<Vector2>();
                foreach (GridSquareScript tile in newCrownSet.ToList())
                {
                    hulllist.Add(tile.GridCoordinates);
                }
                hulllist = GetConvexHull(hulllist);

                List<GridSquareScript> newHull = new List<GridSquareScript>();
                foreach (Vector2 tile in hulllist)
                {
                    GameObject newtileGO = Instantiate(newCrownSet.ToList()[0].gameObject);
                    DestroyImmediate(newtileGO.GetComponent<GridSquareScript>());
                    newtileGO.AddComponent<GridSquareScript>();

                    GridSquareScript newtile = newtileGO.GetComponent<GridSquareScript>();
                    newtile.GridCoordinates = tile;
                    newtile.elevation = elevation;
                    newHull.Add(newtile);
                    whattodelete.Add(newtileGO);
                }

                Debug.Log("elevation : " + elevation + ", " + newHull.Count + " were added");
                elevationList.Add(newHull);
            }
            
        }

        // --- build final points list from elevationList (tiles -> Vector3 positions) ---
        List<Vector3> finalPoints = new List<Vector3>();
        // bottom ring (y = 0)
        foreach (var t in elevationList[0])
            finalPoints.Add(new Vector3(t.GridCoordinates.x, 0, t.GridCoordinates.y));
        // upper crowns (use tile.elevation to be safe)
        for (int i = 1; i < elevationList.Count; i++)
        {
            foreach (var t in elevationList[i])
                finalPoints.Add(new Vector3(t.GridCoordinates.x, t.elevation, t.GridCoordinates.y));
        }

        points = finalPoints;
        CreateLayeredMesh();
        foreach(GameObject GO in whattodelete)
        {
            DestroyImmediate(GO);
        }
    }

    bool PointInPolygon(Vector2 p, List<Vector2> poly)
    {
        if (poly == null || poly.Count < 3) return false;
        bool inside = false;
        for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
        {
            Vector2 pi = poly[i];
            Vector2 pj = poly[j];
            bool intersect = ((pi.y > p.y) != (pj.y > p.y)) &&
                             (p.x < (pj.x - pi.x) * (p.y - pi.y) / (pj.y - pi.y + Mathf.Epsilon) + pi.x);
            if (intersect) inside = !inside;
        }
        return inside;
    }

    List<Vector2> OrderAroundCentroid(List<Vector2> points)
    {
        if (points == null || points.Count == 0) return new List<Vector2>();
        Vector2 centroid = Vector2.zero;
        foreach (var p in points) centroid += p;
        centroid /= points.Count;
        points.Sort((a, b) =>
        {
            float aAng = Mathf.Atan2(a.y - centroid.y, a.x - centroid.x);
            float bAng = Mathf.Atan2(b.y - centroid.y, b.x - centroid.x);
            return aAng.CompareTo(bAng);
        });
        return points;
    }

    //void CreatePolygoneFromZones()
    //{
    //    GetZones();
    //    List<GridSquareScript> zonetouse = zones[zoneindex];
    //    List<GridSquareScript> onlyoutside = RemoveInside(zonetouse);

    //    List<GridSquareScript> onlyinside = new List<GridSquareScript>();
    //    List<GameObject> whattodelete = new List<GameObject>();
    //    foreach (GridSquareScript tile in zonetouse)
    //    {
    //        if (!onlyoutside.Contains(tile))
    //        {
    //            GameObject newtileGO = Instantiate(tile.gameObject);
    //            DestroyImmediate(newtileGO.GetComponent<GridSquareScript>());
    //            newtileGO.AddComponent<GridSquareScript>();

    //            GridSquareScript newtile = newtileGO.GetComponent<GridSquareScript>();
    //            newtile.GridCoordinates = tile.GridCoordinates;
    //            newtile.elevation = Random.Range(0, maxheight);
    //            onlyinside.Add(newtile);
    //            whattodelete.Add(newtileGO);
    //        }
    //    }

    //    List<GridSquareScript> newonlyoutside = new List<GridSquareScript>();
    //    foreach (GridSquareScript tile in onlyoutside)
    //    {
    //        GameObject newtileGO = Instantiate(tile.gameObject);
    //        DestroyImmediate(newtileGO.GetComponent<GridSquareScript>());
    //        newtileGO.AddComponent<GridSquareScript>();

    //        GridSquareScript newtile = newtileGO.GetComponent<GridSquareScript>();
    //        newtile.GridCoordinates = tile.GridCoordinates;
    //        newtile.elevation = 0;
    //        newonlyoutside.Add(newtile);
    //        whattodelete.Add(newtileGO);
    //    }
    //    onlyoutside = newonlyoutside;

    //    Debug.Log("onlyinside : " + onlyinside.Count);

    //    // Initialize the first crown
    //    List<List<GridSquareScript>> elevationList = new List<List<GridSquareScript>>() { onlyoutside };

    //    List<int> minmaxcoords = new List<int>() { -99, -99, 99, 99 };
    //    int minx = 0;
    //    int miny = 0;
    //    int maxx = 0;
    //    int maxy = 0;
    //    foreach (GridSquareScript tile in onlyoutside)
    //    {

    //        if (tile.GridCoordinates.x < minx)
    //        {
    //            minx = (int)tile.GridCoordinates.x;
    //        }
    //        if (tile.GridCoordinates.y < miny)
    //        {
    //            miny = (int)tile.GridCoordinates.y;
    //        }
    //        if (tile.GridCoordinates.x > maxx)
    //        {
    //            maxx = (int)tile.GridCoordinates.x;
    //        }
    //        if (tile.GridCoordinates.y > maxy)
    //        {
    //            maxx = (int)tile.GridCoordinates.y;
    //        }
    //    }
    //    Debug.Log(minx + " " + miny + " " + maxx + " " + maxy);

    //    minmaxcoords = new List<int>() { minx, miny, maxx, maxy };

    //    // Loop through elevations
    //    for (int elevation = 1; elevation <= maxheight; elevation++)
    //    {
    //        List<GridSquareScript> newcrown = new List<GridSquareScript>();
    //        foreach (GridSquareScript tile in onlyinside)
    //        {
    //            if (tile.elevation == elevation)
    //            {
    //                Vector2 coord = tile.GridCoordinates;
    //                if (((coord.x >= minmaxcoords[0] && coord.x <= 0) || (coord.x <= minmaxcoords[2] && coord.x >= 0)) && ((coord.y >= minmaxcoords[1] && coord.y <= 0) || (coord.y <= minmaxcoords[3] && coord.y >= 0)))
    //                {
    //                    newcrown.Add(tile);
    //                }
    //            }
    //        }
    //        newcrown = RemoveInside(newcrown);
    //        Debug.Log(newcrown.Count);
    //        minx = -10;
    //        miny = -10;
    //        maxx = 10;
    //        maxy = 10;
    //        foreach (GridSquareScript tile in newcrown)
    //        {

    //            if (tile.GridCoordinates.x < minx)
    //            {
    //                minx = (int)tile.GridCoordinates.x;
    //            }
    //            if (tile.GridCoordinates.y < miny)
    //            {
    //                miny = (int)tile.GridCoordinates.y;
    //            }
    //            if (tile.GridCoordinates.x > maxx)
    //            {
    //                maxx = (int)tile.GridCoordinates.x;
    //            }
    //            if (tile.GridCoordinates.y > maxy)
    //            {
    //                maxx = (int)tile.GridCoordinates.y;
    //            }
    //        }

    //        minmaxcoords = new List<int>() { minx, miny, maxx, maxy };
    //        Debug.Log(minx + " " + miny + " " + maxx + " " + maxy);
    //        Debug.Log("In elevation : " + elevation + " there are " + newcrown.Count + " tiles");
    //        elevationList.Add(newcrown);
    //    }

    //    Debug.Log(elevationList.Count);


    //    List<Vector2> edges = GetOuterEdgesOfSquares(onlyoutside, 1);

    //    List<Vector3> finalpoints = new List<Vector3>();

    //    foreach (Vector2 edge in edges)
    //    {
    //        finalpoints.Add(new Vector3(edge.x, 0, edge.y));
    //    }

    //    for (int i = 1; i < elevationList.Count; i++)
    //    {
    //        foreach (GridSquareScript tile in elevationList[i])
    //        {
    //            finalpoints.Add(new Vector3(tile.GridCoordinates.x, tile.elevation, tile.GridCoordinates.y));
    //            Debug.Log("adding : " + new Vector3(tile.GridCoordinates.x, tile.elevation, tile.GridCoordinates.y) + " with elevation : " + i);
    //        }
    //    }
    //    points = finalpoints;
    //    CreateLayeredMesh();
    //    foreach (GameObject GO in whattodelete)
    //    {
    //        DestroyImmediate(GO);
    //    }
    //}

    public static List<Vector2> GetOuterEdgesOfSquares(List<GridSquareScript> centers, float size)
    {
        float halfSize = size / 2f;
        List<Vector2> allCorners = new List<Vector2>();

        // Collecte des coins des carrés
        foreach (GridSquareScript center in centers)
        {
            allCorners.Add(center.GridCoordinates + new Vector2(-halfSize, -halfSize));
            allCorners.Add(center.GridCoordinates + new Vector2(-halfSize, halfSize));
            allCorners.Add(center.GridCoordinates + new Vector2(halfSize, halfSize));
            allCorners.Add(center.GridCoordinates + new Vector2(halfSize, -halfSize));
        }

        // Récupère l'enveloppe convexe
        return  GetConvexHull(allCorners);

        
    }


    public static List<Vector2> GetConvexHull(List<Vector2> points)
    {
        if (points.Count <= 3)
            return new List<Vector2>(points); // Already convex

        // Step 1: Find the point with the lowest y (and leftmost if tie)
        Vector2 pivot = points.OrderBy(p => p.y).ThenBy(p => p.x).First();

        // Step 2: Sort points by polar angle with pivot
        var sortedPoints = points
            .OrderBy(p => Mathf.Atan2(p.y - pivot.y, p.x - pivot.x))
            .ToList();

        // Step 3: Graham scan
        List<Vector2> hull = new List<Vector2>();
        hull.Add(pivot);
        hull.Add(sortedPoints[1]);

        for (int i = 2; i < sortedPoints.Count; i++)
        {
            Vector2 top = hull[hull.Count - 1];
            Vector2 nextToTop = hull[hull.Count - 2];
            Vector2 current = sortedPoints[i];

            while (hull.Count >= 2 && Cross(nextToTop, top, current) <= 0)
            {
                hull.RemoveAt(hull.Count - 1);
                if (hull.Count >= 2)
                {
                    top = hull[hull.Count - 1];
                    nextToTop = hull[hull.Count - 2];
                }
            }

            hull.Add(current);
        }

        // Liste finale avec interpolation
        List<Vector2> expanded = new List<Vector2>();

        for (int i = 0; i < hull.Count; i++)
        {
            Vector2 start = hull[i];
            Vector2 end = hull[(i + 1) % hull.Count];

            // Ajoute toujours le point de départ
            expanded.Add(start);

            // Calcul du delta
            Vector2 dir = end - start;

            // On force un mouvement uniquement horizontal ou vertical
            if (Mathf.Abs(dir.x) > 0 && Mathf.Abs(dir.y) > 0)
            {
                //throw new System.Exception("Erreur: diagonale détectée dans le hull, interdit.");
            }

            // Nombre de pas entiers de 1 unité
            int steps = Mathf.RoundToInt(Mathf.Max(Mathf.Abs(dir.x), Mathf.Abs(dir.y)));

            // Ajout des points intermédiaires
            if (steps > 1)
            {
                Vector2 stepDir = new Vector2(
                    dir.x == 0 ? 0 : Mathf.Sign(dir.x),
                    dir.y == 0 ? 0 : Mathf.Sign(dir.y)
                );

                for (int s = 1; s < steps; s++)
                {
                    expanded.Add(start + stepDir * s);
                }
            }
        }

        return expanded;
    }

    public static List<GridSquareScript> RemoveInside(List<GridSquareScript> points)
    {
        if (points.Count <= 3)
            return new List<GridSquareScript>(points); // Already convex

        // Step 1: Find the point with the lowest y (and leftmost if tie)
        GridSquareScript pivot = points.OrderBy(p => p.GridCoordinates.y).ThenBy(p => p.GridCoordinates.x).First();

        // Step 2: Sort points by polar angle with pivot
        var sortedPoints = points
            .OrderBy(p => Mathf.Atan2(p.GridCoordinates.y - pivot.GridCoordinates.y, p.GridCoordinates.x - pivot.GridCoordinates.x))
            .ToList();

        // Step 3: Graham scan
        List<GridSquareScript> hull = new List<GridSquareScript>();
        hull.Add(pivot);
        hull.Add(sortedPoints[1]);

        for (int i = 2; i < sortedPoints.Count; i++)
        {
            GridSquareScript top = hull[hull.Count - 1];
            GridSquareScript nextToTop = hull[hull.Count - 2];
            GridSquareScript current = sortedPoints[i];

            while (hull.Count >= 2 && Cross(nextToTop.GridCoordinates, top.GridCoordinates, current.GridCoordinates) <= 0)
            {
                hull.RemoveAt(hull.Count - 1);
                if (hull.Count >= 2)
                {
                    top = hull[hull.Count - 1];
                    nextToTop = hull[hull.Count - 2];
                }
            }

            hull.Add(current);
        }

        return hull;
    }


    void CreatePolygon()
    {
        if (points.Count < 3)
        {
            Debug.LogWarning("Need at least 3 points to create a polygon!");
            return;
        }

        // Step 0: Compute centroid in XZ plane
        Vector3 centroid = Vector3.zero;
        foreach (var p in points) centroid += new Vector3(p.x, 0, p.z);
        centroid /= points.Count;

        // Step 1: Sort points counter-clockwise in XZ plane
        points.Sort((a, b) =>
        {
            float angleA = Mathf.Atan2(a.z - centroid.z, a.x - centroid.x);
            float angleB = Mathf.Atan2(b.z - centroid.z, b.x - centroid.x);
            return angleA.CompareTo(angleB);
        });

        int pointCount = points.Count;

        // Step 2: Create vertices (bottom + top)
        List<Vertex> verts = new List<Vertex>();
        for (int i = 0; i < pointCount; i++)
            verts.Add(new Vertex { position = points[i] }); // bottom
        for (int i = 0; i < pointCount; i++)
            verts.Add(new Vertex { position = points[i] + Vector3.up * extrudeHeight }); // top

        // Step 3: Triangulate bottom & top using Ear Clipping (XZ projection)
        List<int> bottomIndices = EarClippingTriangulate(points);
        List<int> topIndices = new List<int>();
        foreach (int i in bottomIndices) topIndices.Add(i + pointCount);

        // Bottom face: reverse order to flip normals downward
        for (int i = 0; i < bottomIndices.Count; i += 3)
            bottomIndices.Reverse(i, 3);

        // Step 4: Create faces
        List<Face> faces = new List<Face>();

        // Bottom face
        for (int i = 0; i < bottomIndices.Count; i += 3)
            faces.Add(new Face(new int[] { bottomIndices[i], bottomIndices[i + 1], bottomIndices[i + 2] }));

        // Top face
        for (int i = 0; i < topIndices.Count; i += 3)
            faces.Add(new Face(new int[] { topIndices[i], topIndices[i + 1], topIndices[i + 2] }));

        // Side faces (connect top and bottom)
        for (int i = 0; i < pointCount; i++)
        {
            int next = (i + 1) % pointCount;

            int b0 = i;
            int b1 = next;
            int t0 = i + pointCount;
            int t1 = next + pointCount;

            // Each quad split into two triangles
            faces.Add(new Face(new int[] { b0, t0, t1 }));
            faces.Add(new Face(new int[] { b0, t1, b1 }));
        }

        // Step 5: Shared vertices (avoid cracks)
        List<SharedVertex> sharedVerts = new List<SharedVertex>();
        for (int i = 0; i < verts.Count; i++)
            sharedVerts.Add(new SharedVertex(new int[] { i }));

        // Step 6: Create ProBuilder mesh
        ProBuilderMesh pbMesh = ProBuilderMesh.Create(
            verts,
            faces,
            sharedVerts,
            null,
            faceMaterial != null ? new List<Material> { faceMaterial } : null
        );

        pbMesh.ToMesh();
        pbMesh.Refresh();
        pbMesh.gameObject.name = "CustomPolygon";
        UnityEditor.Selection.activeGameObject = pbMesh.gameObject;
    }


    // ---------- Ear Clipping Triangulation ----------
    List<int> EarClippingTriangulate(List<Vector3> poly)
    {
        List<int> indices = new List<int>();
        List<int> V = new List<int>();
        int n = poly.Count;
        if (n < 3) return indices;

        for (int i = 0; i < n; i++) V.Add(i);

        int count = 2 * n; // maximum iterations
        int m = 0;
        int v = n - 1;

        while (n > 2)
        {
            if ((count--) <= 0) break; // fail-safe

            int u = v;
            if (n <= u) u = 0;
            v = u + 1;
            if (n <= v) v = 0;
            int w = v + 1;
            if (n <= w) w = 0;

            Vector3 A = poly[V[u]];
            Vector3 B = poly[V[v]];
            Vector3 C = poly[V[w]];

            if (IsEar(A, B, C, poly, V))
            {
                indices.Add(V[u]);
                indices.Add(V[v]);
                indices.Add(V[w]);
                V.RemoveAt(v);
                n--;
                count = 2 * n;
            }
        }

        return indices;
    }

    bool IsEar(Vector3 a, Vector3 b, Vector3 c, List<Vector3> poly, List<int> V)
    {
        if (Vector3.Cross(b - a, c - a).y <= 0) return false; // CCW check in XZ plane

        for (int i = 0; i < V.Count; i++)
        {
            Vector3 p = poly[V[i]];
            if (p == a || p == b || p == c) continue;
            if (PointInTriangleXZ(p, a, b, c)) return false;
        }

        return true;
    }

    bool PointInTriangleXZ(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
    {
        Vector2 pt = new Vector2(p.x, p.z);
        Vector2 v0 = new Vector2(c.x - a.x, c.z - a.z);
        Vector2 v1 = new Vector2(b.x - a.x, b.z - a.z);
        Vector2 v2 = new Vector2(pt.x - a.x, pt.y - a.z);

        float dot00 = Vector2.Dot(v0, v0);
        float dot01 = Vector2.Dot(v0, v1);
        float dot02 = Vector2.Dot(v0, v2);
        float dot11 = Vector2.Dot(v1, v1);
        float dot12 = Vector2.Dot(v1, v2);

        float invDenom = 1f / (dot00 * dot11 - dot01 * dot01);
        float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
        float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

        return (u >= 0) && (v >= 0) && (u + v < 1);
    }

    void CreateLayeredMesh()
    {
        if (points.Count < 3)
        {
            Debug.LogWarning("Need at least 3 points!");
            return;
        }

        var layers = points.GroupBy(p => Mathf.RoundToInt(p.y))
                   .OrderBy(g => g.Key)
                   .Select(g => SortLayerCCW(g.ToList()))
                   .ToList();

        List<Vertex> verts = new List<Vertex>();
        List<Face> faces = new List<Face>();
        List<SharedVertex> sharedVerts = new List<SharedVertex>();

        int vertOffset = 0;

        // Sort layers CCW and add vertices
        foreach (var layer in layers)
        {
            Vector3 centroid = Vector3.zero;
            foreach (var p in layer) centroid += new Vector3(p.x, 0, p.z);
            centroid /= layer.Count;

            layer.Sort((a, b) =>
            {
                float angleA = Mathf.Atan2(a.z - centroid.z, a.x - centroid.x);
                float angleB = Mathf.Atan2(b.z - centroid.z, b.x - centroid.x);
                return angleA.CompareTo(angleB);
            });

            foreach (var p in layer)
            {
                verts.Add(new Vertex { position = p });
                sharedVerts.Add(new SharedVertex(new int[] { vertOffset }));
                vertOffset++;
            }
        }

        // Stitch consecutive layers
        for (int l = 0; l < layers.Count - 1; l++)
        {
            StitchLayers(layers[l], layers[l + 1], l, layers, faces);
        }

        // Cap top and bottom
        faces.AddRange(TriangulateLayer(layers[0], 0, verts, invertNormal: true));
        faces.AddRange(TriangulateLayer(layers.Last(), verts.Count - layers.Last().Count, verts, invertNormal: false));

        ProBuilderMesh pbMesh = ProBuilderMesh.Create(
            verts,
            faces,
            sharedVerts,
            null,
            faceMaterial != null ? new List<Material> { faceMaterial } : null
        );

        pbMesh.ToMesh();
        pbMesh.Refresh();
        pbMesh.gameObject.name = "LayeredMesh";
        UnityEditor.Selection.activeGameObject = pbMesh.gameObject;
    }

    List<Vector3> SortLayerCCW(List<Vector3> layer)
    {
        if (layer.Count < 3) return new List<Vector3>(layer);

        // Step 1: Convert to 2D (XZ plane)
        List<Vector2> points2D = layer.Select(p => new Vector2(p.x, p.z)).ToList();

        // Step 2: Find convex hull indices
        List<int> hullIndices = ConvexHull(points2D);

        // Step 3: Build CCW sorted 3D points
        List<Vector3> sorted = hullIndices.Select(i => layer[i]).ToList();

        return sorted;
    }

    // Simple Graham scan convex hull returning indices of points in CCW order
    List<int> ConvexHull(List<Vector2> points)
    {
        if (points.Count < 3) return Enumerable.Range(0, points.Count).ToList();

        // Find the pivot (lowest y, then lowest x)
        int pivotIndex = 0;
        for (int i = 1; i < points.Count; i++)
            if (points[i].y < points[pivotIndex].y || (points[i].y == points[pivotIndex].y && points[i].x < points[pivotIndex].x))
                pivotIndex = i;

        Vector2 pivot = points[pivotIndex];
        List<int> sortedIndices = Enumerable.Range(0, points.Count).Where(i => i != pivotIndex)
                                            .OrderBy(i =>
                                            {
                                                Vector2 dir = points[i] - pivot;
                                                return Mathf.Atan2(dir.y, dir.x);
                                            }).ToList();

        List<int> hull = new List<int> { pivotIndex, sortedIndices[0], sortedIndices[1] };

        for (int i = 2; i < sortedIndices.Count; i++)
        {
            int idx = sortedIndices[i];
            while (hull.Count >= 2 && Cross(points[hull[hull.Count - 2]], points[hull[hull.Count - 1]], points[idx]) <= 0)
                hull.RemoveAt(hull.Count - 1);
            hull.Add(idx);
        }

        return hull;
    }

    static float Cross(Vector2 O, Vector2 A, Vector2 B)
    {
        return (A.x - O.x) * (B.y - O.y) - (A.y - O.y) * (B.x - O.x);
    }

    void StitchLayers(List<Vector3> lower, List<Vector3> upper, int lowerLayerIndex, List<List<Vector3>> layers, List<Face> faces)
    {
        int li = 0, ui = 0;
        int lowerCount = lower.Count;
        int upperCount = upper.Count;

        // Walk until we have consumed all edges
        while (li < lowerCount || ui < upperCount)
        {
            int liNext = (li + 1) % lowerCount;
            int uiNext = (ui + 1) % upperCount;

            int lo0Index = GetVertexIndex(lowerLayerIndex, li % lowerCount, layers);
            int lo1Index = GetVertexIndex(lowerLayerIndex, liNext % lowerCount, layers);
            int up0Index = GetVertexIndex(lowerLayerIndex + 1, ui % upperCount, layers);
            int up1Index = GetVertexIndex(lowerLayerIndex + 1, uiNext % upperCount, layers);

            Vector2 loEdge = new Vector2(lower[liNext % lowerCount].x - lower[li % lowerCount].x,
                                         lower[liNext % lowerCount].z - lower[li % lowerCount].z);
            Vector2 upEdge = new Vector2(upper[uiNext % upperCount].x - upper[ui % upperCount].x,
                                         upper[uiNext % upperCount].z - upper[ui % upperCount].z);

            // Choose which vertex to advance based on shorter edge length
            if (li < lowerCount && (ui >= upperCount || loEdge.sqrMagnitude <= upEdge.sqrMagnitude))
            {
                faces.Add(new Face(new int[] { lo0Index, up0Index, up1Index }));
                faces.Add(new Face(new int[] { lo0Index, up1Index, lo1Index }));
                li++;
            }
            else
            {
                faces.Add(new Face(new int[] { lo0Index, up0Index, up1Index }));
                faces.Add(new Face(new int[] { lo0Index, up1Index, lo1Index }));
                ui++;
            }
        }
    }



    int GetVertexIndex(int layerIndex, int pointIndex, List<List<Vector3>> layers)
    {
        int offset = 0;
        for (int i = 0; i < layerIndex; i++)
            offset += layers[i].Count;
        return offset + pointIndex;
    }

    List<Face> TriangulateLayer(List<Vector3> layer, int vertOffset, List<Vertex> verts, bool invertNormal)
    {
        List<Face> faces = new List<Face>();
        if (layer.Count < 3) return faces;

        // Simple fan triangulation since layer is already sorted CCW
        for (int i = 1; i < layer.Count - 1; i++)
        {
            if (invertNormal)
                faces.Add(new Face(new int[] { vertOffset, vertOffset + i + 1, vertOffset + i }));
            else
                faces.Add(new Face(new int[] { vertOffset, vertOffset + i, vertOffset + i + 1 }));
        }
        return faces;
    }



}
