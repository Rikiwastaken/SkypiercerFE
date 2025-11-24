using UnityEngine;
using System.Collections.Generic;

public class FPSCounter : MonoBehaviour
{
    private Queue<float> frameTimes = new Queue<float>();
    private float timeAccumulator = 0f;

    void Update()
    {
        float dt = Time.unscaledDeltaTime;
        frameTimes.Enqueue(dt);
        timeAccumulator += dt;

        while (timeAccumulator > 1f)
        {
            timeAccumulator -= frameTimes.Dequeue();
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

        int fps = frameTimes.Count;                   
        float avgFrameTime = 1f / Mathf.Max(fps, 1);  

        string text = $"{(avgFrameTime * 1000f):0.0} ms  ({fps} fps)";
        GUI.Label(rect, text, style);
    }
}
