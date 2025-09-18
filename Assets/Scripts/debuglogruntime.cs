using UnityEngine;
using System.Collections;
using TMPro;

public class ExampleClass : MonoBehaviour
{
    public string output = "";
    public string stack = "";

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        output = logString;
        stack = stackTrace;
        GetComponent<TextMeshProUGUI>().text = logString;
    }
}