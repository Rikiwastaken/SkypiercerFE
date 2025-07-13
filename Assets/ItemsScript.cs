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
    public GameObject ItemMenu;
    public TextMeshProUGUI statstext;
    private void FixedUpdate()
    {
        if (inputManager==null)
        {
            inputManager = FindAnyObjectByType<InputManager>();
        }
        if (inputManager.canceljustpressed)
        {
            if(ItemActionsMenu.activeSelf)
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
    }

    public void InitializeButtons()
    {
        target = FindAnyObjectByType<ActionsMenu>().target;
        if(target!= null)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                if (target.equipments[i].Name == "" || target.equipments[i].Name == null)
                {
                    buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = "";
                    statstext.text = "Empty Slot";
                }
                else
                {
                    buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = target.equipments[i].Name + " " + target.equipments[i].Currentuses + "/" + target.equipments[i].Maxuses;
                    statstext.text = "Dmg : "+ target.equipments[i].BaseDamage+ " Hit :"+ target.equipments[i].BaseHit+"% Crit : "+ target.equipments[i].BaseCrit + "% Range : " + target.equipments[i].Range + "  Uses : " + target.equipments[i].Currentuses+" / " + target.equipments[i].Maxuses;
                } 
                    
            }
        }
    }
}
