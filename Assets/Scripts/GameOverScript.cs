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
        waittingforGameover = true;
        if (victory)
        {
            GetComponentInChildren<TextMeshProUGUI>().text = "Winner is you !\nReturning to main Menu";
        }
        else
        {
            GetComponentInChildren<TextMeshProUGUI>().text = "Game Over...\nReturning to main Menu";
        }
        
    }


    private void Update()
    {
        if (waittingforGameover)
        {
            gameovercounter++;
            if (gameovercounter > 5f / Time.deltaTime)
            {
                FindAnyObjectByType<SaveManager>().SaveCurrentSlot();
                SceneManager.LoadScene("MainMenu");
            }
        }
    }
}
