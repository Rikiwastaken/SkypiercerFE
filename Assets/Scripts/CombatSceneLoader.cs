using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnitScript;

public class CombatSceneLoader : MonoBehaviour
{
    [Header("Scene Names")]
    public string MainSceneName;
    public string CombatSceneName;

    [Header("Fade Settings")]
    public CanvasGroup fadeCanvasGroup; // full screen black overlay
    public float fadeDuration = 1f;

    private Scene combatScene;
    private Scene mainScene;
    public bool combatLoaded = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Preload Combat Scene
    public void LoadCombatScene()
    {
        Debug.Log("combat scene loaded : " + combatLoaded);
        if (!combatLoaded)
            StartCoroutine(LoadCombatSceneRoutine());
    }

    private IEnumerator LoadCombatSceneRoutine()
    {
        // Cache main scene reference
        mainScene = SceneManager.GetActiveScene();

        // Load combat scene additively
        var asyncLoad = SceneManager.LoadSceneAsync(CombatSceneName, LoadSceneMode.Additive);
        asyncLoad.allowSceneActivation = true;

        yield return new WaitUntil(() => asyncLoad.isDone);

        combatScene = SceneManager.GetSceneByName(CombatSceneName);
        FindAnyObjectByType<CombaSceneManager>().LoadEnvironment(MainSceneName, combatScene);
        // Hide combat scene root objects
        SetSceneVisible(combatScene, false);

        combatLoaded = true;
        Debug.Log("Combat scene preloaded and hidden.");
    }

    // Activate combat scene
    public void ActivateCombatScene(
        Character attacker,
        Character defender,
        equipment attackerWeapon,
        equipment defenderWeapon,
        Character doubleAttacker,
        bool tripleHit,
        bool healing,
        int attackerDodged,
        bool defenderAttacks,
        int defenderDodged,
        bool attackerDied,
        bool defenderDied,
        int expgained,
        List<int> levelupbonuses,
        Character attackerbeforecombat,
        Character defenderbeforecombat,
        int attackerdamage,
        int defenderdamage,
        int attackercrits,
        int defendercrits)
    {
        if (!combatLoaded)
        {
            Debug.LogWarning("Combat scene not loaded yet! Call LoadCombatScene() first.");
            return;
        }

        mainScene = SceneManager.GetSceneByName(MainSceneName);

        MusicManager.instance.inCombatBool = true;

        StartCoroutine(SwitchSceneRoutine(
            fromScene: mainScene,
            toScene: combatScene,
            onSceneActivated: () =>
            {
                var combatManager = FindAnyObjectByType<CombaSceneManager>();
                if (combatManager != null)
                {
                    combatManager.SetupScene(attacker, defender, attackerWeapon, defenderWeapon,
                        doubleAttacker, tripleHit, healing, attackerDodged, defenderAttacks,
                        defenderDodged, attackerDied, defenderDied, expgained, levelupbonuses, attackerbeforecombat, defenderbeforecombat, attackerdamage, defenderdamage, attackercrits, defendercrits);
                }
            }));
    }

    // Return to Main Scene
    public void ActivateMainScene()
    {
        if (!combatLoaded)
        {
            Debug.LogWarning("Scenes not fully initialized.");
            return;
        }

        MusicManager.instance.inCombatBool = false;

        StartCoroutine(SwitchSceneRoutine(
            fromScene: combatScene,
            toScene: mainScene,
            onSceneActivated: null));
    }

    // Scene Switch
    private IEnumerator SwitchSceneRoutine(Scene fromScene, Scene toScene, System.Action onSceneActivated)
    {
        yield return Fade(1); // fade out

        // Always get fresh roots (important!)
        SetSceneVisible(fromScene, false);
        SetSceneVisible(toScene, true);

        SceneManager.SetActiveScene(toScene);

        onSceneActivated?.Invoke();

        yield return Fade(0); // fade in
    }

    // Enable/Disable all root GameObjects in a scene
    private void SetSceneVisible(Scene scene, bool visible)
    {
        if (!scene.IsValid()) return;

        var roots = scene.GetRootGameObjects();
        foreach (var go in roots)
        {
            if (go != null)
                go.SetActive(visible);
        }
    }

    // Fade Helper
    private IEnumerator Fade(float targetAlpha)
    {
        if (fadeCanvasGroup == null)
            yield break;

        float startAlpha = fadeCanvasGroup.alpha;
        float t = 0f;

        while (t < fadeDuration)
        {
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t / fadeDuration);
            t += Time.deltaTime;
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
    }
}
