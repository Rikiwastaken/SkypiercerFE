using UnityEngine;
using UnityEngine.UI;
using static UnitScript;
public class ActionsMenu : MonoBehaviour
{

    public Character target;

    public Button CancelButton;

    public GameObject ItemsScript;

    private InputManager inputManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        target = GameObject.Find("Zack").GetComponent<UnitScript>().UnitCharacteristics;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(inputManager == null)
        {
            inputManager = FindAnyObjectByType<InputManager>();
        }

        if (inputManager.canceljustpressed && !ItemsScript.activeSelf)
        {
            CancelButton.onClick.Invoke();
        }
    }
    
}
