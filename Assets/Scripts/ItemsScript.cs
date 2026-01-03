using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnitScript;


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

        inputManager = InputManager.instance;

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
            statstext.transform.parent.gameObject.SetActive(false);
        }

        GameObject currentselected = EventSystem.current.currentSelectedGameObject;

        if (currentselected != null)
        {
            if (buttons.Contains(currentselected))
            {
                if (target != null)
                {
                    equipment equ = target.equipments[buttons.IndexOf(currentselected)];
                    if (equ == null)
                    {
                        statstext.transform.parent.gameObject.SetActive(false);
                    }
                    else if (equ.Name == null)
                    {
                        statstext.transform.parent.gameObject.SetActive(false);
                    }
                    else if (equ.Name.ToLower() != "fists" && equ.Name.ToLower() != "fist" && equ.Name != "")
                    {
                        string grade = "";
                        switch (equ.Grade)
                        {
                            case (0):
                                grade = "E";
                                break;
                            case (1):
                                grade = "D";
                                break;
                            case (2):
                                grade = "C";
                                break;
                            case (3):
                                grade = "B";
                                break;
                            case (4):
                                grade = "A";
                                break;
                            case (5):
                                grade = "s";
                                break;
                        }

                        string modifer = equ.Modifier;
                        if (modifer==null || modifer=="")
                        {
                            modifer = "None";
                        }

                        statstext.text = equ.Name + "\nDmg : " + equ.BaseDamage + "\nHit : " + equ.BaseHit + " %\nCrit : " + equ.BaseCrit + " %\nRange : " + equ.Range + "\nType: " + equ.type + "\nGrade: " + grade +"\nModifier : "+ modifer + " \nUses : " + equ.Currentuses + " / " + equ.Maxuses;
                        statstext.transform.parent.gameObject.SetActive(true);
                    }
                    else
                    {
                        statstext.transform.parent.gameObject.SetActive(false);
                    }

                }

            }
        }
        else
        {
            statstext.transform.parent.gameObject.SetActive(false);
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
        if (target.equipments[id].Name == null)
        {
            statstext.text = "Empty Slot";
            ItemActionsMenu.gameObject.SetActive(false);
            statstext.transform.parent.gameObject.SetActive(false);
            buttons[id].GetComponent<Button>().Select();
        }
        else if (target.equipments[id].Name.ToLower() == "fists" || target.equipments[id].Name.ToLower() == "fist" || target.equipments[id].Name == "")
        {
            statstext.text = "Empty Slot";
            ItemActionsMenu.gameObject.SetActive(false);
            statstext.transform.parent.gameObject.SetActive(false);
            buttons[id].GetComponent<Button>().Select();

        }
        else
        {

            string grade = "";
            switch (target.equipments[id].Grade)
            {
                case (0):
                    grade = "E";
                    break;
                case (1):
                    grade = "D";
                    break;
                case (2):
                    grade = "C";
                    break;
                case (3):
                    grade = "B";
                    break;
                case (4):
                    grade = "A";
                    break;
                case (5):
                    grade = "s";
                    break;
            }

            statstext.text = target.equipments[id].Name + "\nDmg : " + target.equipments[id].BaseDamage + "\nHit : " + target.equipments[id].BaseHit + " %\nCrit : " + target.equipments[id].BaseCrit + " %\nRange : " + target.equipments[id].Range + "\nType: " + target.equipments[id].type + "\nGrade: " + grade + " \nUses : " + target.equipments[id].Currentuses + " / " + target.equipments[id].Maxuses;
            statstext.transform.parent.gameObject.SetActive(true);
        }
    }

    public void InitializeButtons()
    {
        target = FindAnyObjectByType<ActionsMenu>().target.GetComponent<UnitScript>().UnitCharacteristics;
        if (target != null)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                if (target.equipments.Count > i)
                {
                    if (target.equipments[i] == null)
                    {
                        buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = "";
                    }
                    else if (target.equipments[i].Name == null)
                    {
                        buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = "";
                    }
                    else if (target.equipments[i].Name.ToLower() == "fists" || target.equipments[i].Name.ToLower() == "fist" || target.equipments[i].Name == "" || target.equipments[i].Name == null)
                    {
                        buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = "";
                    }
                    else
                    {

                        int spriteID = 0;
                        switch(target.equipments[i].type.ToLower())
                        {
                            default:
                                spriteID = 5;
                                break;
                            case ("sword"):
                                spriteID = 0;
                                break;
                            case ("spear"):
                                spriteID = 1;
                                break;
                            case ("greatsword"):
                                spriteID = 2;
                                break;
                            case ("bow"):
                                spriteID = 3;
                                break;
                            case ("scythe"):
                                spriteID = 4;
                                break;
                            case ("shield"):
                                spriteID = 6;
                                break;
                            case ("staff"):
                                spriteID = 7;
                                break;
                            

                        }

                        buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = target.equipments[i].Name + " <sprite=" + spriteID + "> " + target.equipments[i].Currentuses + "/" + target.equipments[i].Maxuses;
                    }
                }
                else
                {
                    buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = "";
                }


            }
        }
    }
}
