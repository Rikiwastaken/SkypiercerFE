using UnityEngine;
using UnityEngine.UI;
using static UnitScript;
using System.Collections.Generic;
using TMPro;


public class ItemsScript : MonoBehaviour
{

    public Character target;

    public List<GameObject> buttons;

    public Button ItemMenuCancelButton;
    public Button ItemActionsMenuCancelButton;
    private InputManager inputManager;
    public GameObject ItemActionsMenu;
    public TextMeshProUGUI statstext;
    private void FixedUpdate()
    {
        if (inputManager == null)
        {
            inputManager = FindAnyObjectByType<InputManager>();
        }
        if (inputManager.canceljustpressed)
        {
            if (ItemActionsMenu.activeSelf)
            {
                ItemActionsMenuCancelButton.onClick.Invoke();
            }
            else
            {
                ItemMenuCancelButton.onClick.Invoke();
            }

        }
    }

    private void OnEnable()
    {
        InitializeButtons();
    }

    public void PlaceNextToSelected(int id)
    {
        Vector3 initialpos = ItemActionsMenu.transform.position;
        ItemActionsMenu.transform.position = new Vector3(initialpos.x, buttons[id].transform.position.y, initialpos.z);
        ItemActionsMenu.GetComponent<ItemActionsMenuScript>().slotID = id;
        ItemActionsMenu.GetComponent<ItemActionsMenuScript>().character = target;
        if (target.equipments[id].Name == "" || target.equipments[id].Name == null)
        {
            statstext.text = "Empty Slot";
        }
        else
        {
            statstext.text = "Dmg : " + target.equipments[id].BaseDamage + " Hit :" + target.equipments[id].BaseHit + "% Crit : " + target.equipments[id].BaseCrit + "% Range : " + target.equipments[id].Range + "  Uses : " + target.equipments[id].Currentuses + " / " + target.equipments[id].Maxuses;
        }
    }

    public void InitializeButtons()
    {
        target = FindAnyObjectByType<ActionsMenu>().target.GetComponent<UnitScript>().UnitCharacteristics;
        if (target != null)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                if (target.equipments[i].Name == "" || target.equipments[i].Name == null)
                {
                    buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = "";
                }
                else
                {
                    buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = target.equipments[i].Name + " " + target.equipments[i].Currentuses + "/" + target.equipments[i].Maxuses;
                }

            }
        }
    }
}
