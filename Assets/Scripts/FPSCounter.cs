using System.Collections.Generic;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    private List<float> frameTimes = new List<float>();

    void Update()
    {
        float dt = Time.deltaTime;
        frameTimes.Add(dt);

        if (frameTimes.Count > 100)
        {
            frameTimes.RemoveAt(0);
        }
    }

    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();
        Rect rect = new Rect(0, 0, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 50;
        style.normal.textColor = Color.white;

        float sum = 0;
        foreach (float time in frameTimes)
        {
            sum += time;
        }
        float avgFrameTime = sum / frameTimes.Count;

        int fps = (int)(1 / avgFrameTime);

        string text = $"{(avgFrameTime * 1000f):0.0} ms  ({fps} fps)";
        GUI.Label(rect, text, style);
    }
}
