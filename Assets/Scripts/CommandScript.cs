using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static DataScript;


public class CommandScript : MonoBehaviour
{

    public GameObject target;

    public List<GameObject> buttons;
    public List<Skill> CommandList;
    public Button ItemMenuCancelButton;
    private InputManager inputManager;
    public GameObject ItemActionsMenu;
    public TextMeshProUGUI statstext;
    private ActionsMenu ActionsMenu;

    private void Update()
    {

        inputManager = InputManager.instance;

        if (inputManager.cancelpressed)
        {
            ItemMenuCancelButton.onClick.Invoke();

        }

        if (ActionsMenu == null)
        {
            ActionsMenu = FindAnyObjectByType<ActionsMenu>();
        }

    }

    public void InitializeButtons()
    {
        if (target != null)
        {
            for (int i = 0; i < CommandList.Count; i++)
            {
                buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = CommandList[i].name;
                var colors = buttons[i].GetComponent<Button>().colors;
                colors.normalColor = Color.white;
                colors.pressedColor = Color.white;
                buttons[i].GetComponent<Button>().colors = colors;
            }
            for (int i = CommandList.Count; i < 5; i++)
            {
                buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = "None";
                var colors = buttons[i].GetComponent<Button>().colors;
                colors.normalColor = Color.gray;
                colors.pressedColor = Color.gray;
                buttons[i].GetComponent<Button>().colors = colors;
            }
        }
    }

    public void ActivateButton(int i)
    {
        if (i < CommandList.Count)
        {
            gameObject.SetActive(false);
            ActionsMenu.ActivateCommand(CommandList[i].ID);
            ActionsMenu.target = GridScript.instance.GetSelectedUnitGameObject();

        }
    }

}
