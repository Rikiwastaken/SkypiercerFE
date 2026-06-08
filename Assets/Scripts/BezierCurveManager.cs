using System.Collections.Generic;
using UnityEngine;
using static UnitScript;

public class BezierCurveManager : MonoBehaviour
{
    [SerializeField]
    public class BezierCurve
    {
        public List<Vector3> Points;

        public BezierCurve(List<Vector3> points)
        {
            Points = points;
        }

        public BezierCurve()
        {
            Points = new List<Vector3>();
        }

        public Vector3 GetPoint(int index)
        {
            if (Points != null && Points.Count > index)
            {
                return Points[index];
            }
            return Points[0];
        }

        public Vector3 GetStartingPoint()
        {
            return GetPoint(0);
        }
        public Vector3 GetEndpoint()
        {
            if (Points != null)
            {
                return Points[Points.Count - 1];
            }
            return Points[0];
        }

        //{B} (t)=\mathbf {P} _{1}+(1-t)^{2}(\mathbf {P} _{0}-\mathbf {P} _{1})+t^{2}(\mathbf {P} _{2}-\mathbf {P} _{1})

        public Vector3 GetSegmentFor3Points(float Time)
        {
            Time = Mathf.Clamp01(Time);
            float inversetime = 1 - Time;
            return Points[1] + Mathf.Pow(inversetime, 2) * (Points[0] - Points[1]) + Mathf.Pow(Time, 2) * (Points[2] - Points[1]);
        }


        public Vector3[] GetSegments(int Subdivisions)
        {
            Vector3[] segments = new Vector3[Subdivisions];
            if (Points == null || Points.Count < 3)
            {
                return segments;
            }

            float time = 0f;
            for (int i = 0; i < Subdivisions; i++)
            {
                time = (float)i / Subdivisions;
                segments[i] = GetSegmentFor3Points(time);
            }
            return segments;
        }
    }
    [Header("Curve Variables")]
    private List<GameObject> SpawnedLines = new List<GameObject>();
    public int sections;
    public float elongationRatio;
    public Material material;
    public float middlepointElevation;
    public Color RendererColor;
    public float width;


    [Header("Line Fadein variables")]
    public float timeforalltoappear;

    public void DrawLineBetween2Characters(Character selectedChar, Character OtherChar, int lineID)
    {
        Vector3 point1 = OtherChar.currentTile.transform.position;
        Vector3 point2 = selectedChar.currentTile.transform.position;
        Vector3 middlepoint = (point1 + point2) / 2f + new Vector3(0, middlepointElevation, 0);
        List<Vector3> points = new List<Vector3>() { point1, middlepoint, point2 };
        DrawLine(points, lineID);
    }



    public void DrawLineBetween2Tiles(GridSquareScript selectedChar, GridSquareScript OtherChar, int lineID)
    {
        Vector3 point1 = OtherChar.transform.position;
        Vector3 point2 = selectedChar.transform.position;
        Vector3 middlepoint = (point1 + point2) / 2f + new Vector3(0, middlepointElevation, 0);
        List<Vector3> points = new List<Vector3>() { point1, middlepoint, point2 };
        DrawLine(points, lineID);
    }

    public void DisableLines()
    {
        foreach (GameObject line in SpawnedLines)
        {
            if (line.activeSelf)
            {
                line.SetActive(false);
            }
        }
    }

    public void DrawLine(List<Vector3> points, int lineID)
    {
        BezierCurve curve = new BezierCurve(points);
        Vector3[] segments = curve.GetSegments(sections);
        InitializeLineHolder(segments.Length, lineID);
        for (int i = 0; i < segments.Length - 1; i++)
        {
            Vector3[] newpositions = CalculateRendererPositions(segments[i], segments[i + 1]);
            SpawnedLines[lineID].transform.GetChild(i).GetComponent<LineRenderer>().SetPositions(newpositions);
        }

        Vector3[] positions = CalculateRendererPositions(segments[segments.Length - 1], points[2]);
        SpawnedLines[lineID].transform.GetChild(segments.Length - 1).GetComponent<LineRenderer>().SetPositions(positions);
        SpawnedLines[lineID].GetComponent<TargettingLineScript>().InitializeLines(timeforalltoappear);
    }

    private void InitializeLineHolder(int numberofsections, int lineID)
    {
        if (SpawnedLines == null)
        {
            SpawnedLines = new List<GameObject>();
        }
        for (int i = 0; i < SpawnedLines.Count; i++)
        {
            if (SpawnedLines[i] == null)
            {
                SpawnedLines.RemoveAt(i);
            }
        }
        if (SpawnedLines.Count - 1 < lineID)
        {
            GameObject LinetoSpawn = new GameObject();
            LinetoSpawn.name = "lineHolder" + lineID;
            SpawnedLines.Add(LinetoSpawn);

            LinetoSpawn.transform.parent = transform;
            LinetoSpawn.AddComponent<TargettingLineScript>();
        }

        SpawnedLines[lineID].SetActive(true);

        int numberofchildren = SpawnedLines[lineID].transform.childCount;

        for (int i = numberofchildren; i < numberofsections; i++)
        {
            GameObject linechild = new GameObject();
            linechild.name = "lineChild" + i;
            linechild.AddComponent<LineRenderer>();
            linechild.transform.parent = SpawnedLines[lineID].transform;
            linechild.GetComponent<LineRenderer>().material = material;
            linechild.GetComponent<LineRenderer>().startColor = RendererColor;
            linechild.GetComponent<LineRenderer>().endColor = RendererColor;
            linechild.GetComponent<LineRenderer>().startWidth = width;
            linechild.GetComponent<LineRenderer>().endWidth = width;
            linechild.layer = LayerMask.NameToLayer("Players");
        }

    }

    private Vector3[] CalculateRendererPositions(Vector3 Start, Vector3 End)
    {
        Vector3[] positions = new Vector3[2];

        positions[0] = Start - (End - Start).normalized * elongationRatio;
        positions[1] = End + (End - Start).normalized * elongationRatio;



        return positions;
    }
}
