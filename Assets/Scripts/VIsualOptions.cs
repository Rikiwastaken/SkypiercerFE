using UnityEngine;

[DefaultExecutionOrder(-100)]
public class VisualOptions : MonoBehaviour
{
    [Tooltip("Target FPS when in exclusive fullscreen.")]
    public int targetFullscreenFPS = 60;

    [Tooltip("Margin below refresh rate for windowed mode.")]
    public int windowedFPSMargin = 2;

    private FullScreenMode lastScreenMode;

    void Awake()
    {
        // Initial setup
        QualitySettings.vSyncCount = 0;
        QualitySettings.maxQueuedFrames = 1;

        lastScreenMode = Screen.fullScreenMode;
        ApplyFPSSettings(lastScreenMode);

        Debug.Log($"[DynamicFrameRateController] Initial setup: fullscreenMode={lastScreenMode}, targetFPS={Application.targetFrameRate}");
    }

    void Update()
    {
        // Detect if the player switched between windowed/fullscreen
        if (Screen.fullScreenMode != lastScreenMode)
        {
            lastScreenMode = Screen.fullScreenMode;
            ApplyFPSSettings(lastScreenMode);

            Debug.Log($"[DynamicFrameRateController] Screen mode changed to {lastScreenMode}, targetFPS={Application.targetFrameRate}");
        }
    }

    private void ApplyFPSSettings(FullScreenMode mode)
    {
        int refreshRate = (int)Screen.currentResolution.refreshRateRatio.numerator / (int)Screen.currentResolution.refreshRateRatio.denominator;
        if (refreshRate <= 0) refreshRate = 60; // fallback

        if (mode == FullScreenMode.ExclusiveFullScreen)
        {
            // Exclusive fullscreen -> exact target FPS
            Application.targetFrameRate = targetFullscreenFPS;
        }
        else
        {
            // Windowed / borderless -> slightly below refresh to minimize wait times
            Application.targetFrameRate = Mathf.Max(1, refreshRate - windowedFPSMargin);
        }

        // Keep maxQueuedFrames = 1 to prevent main thread stalls
        QualitySettings.maxQueuedFrames = 1;

        // VSync off in both modes
        QualitySettings.vSyncCount = 0;
    }
}
