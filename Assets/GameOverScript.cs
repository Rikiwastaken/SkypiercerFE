using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScript : MonoBehaviour
{

    private bool waittingforGameover;

    private int gameovercounter;

    public bool victory;

    private void OnEnable()
    {
        Time.timeScale = 0f;
        waittingforGameover= true;
        if(victory)
        {
            GetComponentInChildren<TextMeshProUGUI>().text = "Winner is you !\nRestarting soon...";
        }
    }


    private void Update()
    {
        if(waittingforGameover)
        {
            gameovercounter++;
            if(gameovercounter > 5f/Time.deltaTime)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }
}
