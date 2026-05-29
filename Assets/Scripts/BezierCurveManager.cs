using System.Collections.Generic;
using UnityEngine;

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

    private GameObject previousLine;
    public float smoothinglength;
    public int sections;
    public List<Vector3> points;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DrawLine(List<Vector3> points)
    {

        BezierCurve curve = new BezierCurve(points);
        Vector3[] segments = curve.GetSegments(sections);
        InitializeLineHolder(segments.Length);
        Debug.Log("got " + segments.Length + " segments");
        for (int i = 0; i < segments.Length - 1; i++)
        {

            previousLine.transform.GetChild(i).GetComponent<LineRenderer>().SetPositions(new Vector3[2] { segments[i], segments[i + 1] });
        }
        previousLine.transform.GetChild(segments.Length - 1).GetComponent<LineRenderer>().SetPositions(new Vector3[2] { segments[segments.Length - 1], points[2] });
    }

    private void InitializeLineHolder(int numberofsections)
    {
        if (previousLine == null)
        {
            previousLine = new GameObject();
            previousLine.name = "lineHolder";

        }
        previousLine.SetActive(true);

        int numberofchildren = previousLine.transform.childCount;

        for (int i = numberofchildren; i < numberofsections; i++)
        {
            GameObject linechild = new GameObject();
            linechild.name = "lineChild" + i;
            linechild.AddComponent<LineRenderer>();
            linechild.transform.parent = previousLine.transform;
        }

    }

#if UNITY_EDITOR
    [ContextMenu("Try to draw curve")]
    void Testdraw()
    {
        DrawLine(points);
        UnityEditor.EditorUtility.SetDirty(previousLine.gameObject);
    }




#endif
}
