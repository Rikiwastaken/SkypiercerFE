using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatSceneManager : MonoBehaviour
{

    public string currentSceneName;

    public string CombatSceneName;

    private Scene CombatScene;

    private Scene MainScene;

    void OnSceneLoaded()
    {
        if(SceneManager.GetSceneByName(CombatSceneName)==null)
        {
            MainScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(CombatSceneName, LoadSceneMode.Additive);
            CombatScene = SceneManager.GetSceneByName(CombatSceneName);
        }
    }

    public void ActivateCombatScene()
    {
        SceneManager.SetActiveScene(CombatScene);
    }

    public void ActivateMainScene()
    {
        SceneManager.SetActiveScene(MainScene);
    }

}
