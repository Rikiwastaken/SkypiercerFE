using System.Collections.Generic;
using System.ComponentModel.Design;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static DataScript;
using static UnitScript;


public class SpecialCommandsScript : MonoBehaviour
{

    public GameObject target;

    public List<GameObject> buttons;
    public List<GameObject> SpecialInteractos;
    public Button ItemMenuCancelButton;
    private InputManager inputManager;
    private ActionsMenu ActionsMenu;

    private void FixedUpdate()
    {

        inputManager = InputManager.instance;

        if (inputManager.canceljustpressed)
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
            for (int i = 0; i < SpecialInteractos.Count; i++)
            {

                if (SpecialInteractos[i].GetComponent<UnitScript>()!=null)
                {
                    buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = "Talk";
                }
                else
                {
                    buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = "Examine";
                }


                var colors = buttons[i].GetComponent<Button>().colors;
                colors.normalColor = Color.white;
                colors.pressedColor = Color.white;
                buttons[i].GetComponent<Button>().colors = colors;
            }
            for (int i = SpecialInteractos.Count; i < buttons.Count; i++)
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
        if (SpecialInteractos[i].GetComponent<UnitScript>() != null)
        {
            ActionManager.instance.Interract(target, null, SpecialInteractos[i].GetComponent<UnitScript>());
            SpecialInteractos[i].GetComponent<UnitScript>().UnitCharacteristics.enemyStats.talkedto = true;
            MapEventManager.instance.TriggerEventCheck();
            
        }
        else if(SpecialInteractos[i].GetComponent<GridSquareScript>() != null)
        {
            ActionManager.instance.Interract(target, SpecialInteractos[i].GetComponent<GridSquareScript>());
            SpecialInteractos[i].GetComponent<GridSquareScript>().Mechanism.isactivated = true;
            MapEventManager.instance.TriggerEventCheck();
        }
    }

}
