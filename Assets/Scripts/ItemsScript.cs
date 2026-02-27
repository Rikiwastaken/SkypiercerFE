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

    [Header("Blade Stats")]

    public GameObject statstext;

    public TextMeshProUGUI BladeName;
    public TextMeshProUGUI BladeType;
    public TextMeshProUGUI BladeDamage;
    public TextMeshProUGUI BladeHitRate;
    public TextMeshProUGUI BladeCritRate;
    public TextMeshProUGUI BladeUses;
    public TextMeshProUGUI BladeRange;
    public TextMeshProUGUI BladeMod;
    public TextMeshProUGUI BladeGrade;

    private Vector3 initialpos;

    private void Start()
    {
        initialpos = ItemActionsMenu.transform.position;
    }

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
            statstext.SetActive(false);
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
                        statstext.SetActive(false);
                    }
                    else if (equ.Name == null)
                    {
                        statstext.SetActive(false);
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
                        if (modifer == null || modifer == "" || modifer == "Basic")
                        {
                            modifer = "Basic";
                        }

                        string typeicon = "";

                        switch (equ.type.ToLower())
                        {
                            case ("sword"):
                                typeicon += "<sprite=0>";
                                break;
                            case ("spear"):
                                typeicon += "<sprite=1>";
                                break;
                            case ("greatsword"):
                                typeicon += "<sprite=2>";
                                break;
                            case ("bow"):
                                typeicon += "<sprite=3>";
                                break;
                            case ("scythe"):
                                typeicon += "<sprite=4>";
                                break;
                            case ("shield"):
                                typeicon += "<sprite=6>";
                                break;
                            case ("staff"):
                                typeicon += "<sprite=7>";
                                break;
                            default:
                                typeicon += "<sprite=5>";
                                break;
                        }

                        BladeName.text = equ.Name;
                        BladeType.text = typeicon;
                        BladeDamage.text = "Damage: " + equ.BaseDamage;
                        BladeHitRate.text = "Hit Rate: " + equ.BaseHit + "%";
                        BladeCritRate.text = "Crit Rate: " + equ.BaseCrit + "%";
                        BladeUses.text = "Uses : " + equ.Currentuses + " / " + equ.Maxuses;
                        BladeRange.text = "Range: " + equ.Range;
                        BladeMod.text = "Modifier: " + modifer;
                        BladeGrade.text = "Grade: " + grade;
                        statstext.SetActive(true);
                    }
                    else
                    {
                        statstext.SetActive(false);
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

    private void FillWithEmpty()
    {
        BladeName.text = "";
        BladeType.text = "";
        BladeDamage.text = "";
        BladeHitRate.text = "";
        BladeCritRate.text = "";
        BladeUses.text = "";
        BladeRange.text = "";
        BladeMod.text = "";
        BladeGrade.text = "";
    }

    public void PlaceNextToSelected(int id)
    {
        ItemActionsMenu.transform.position = new Vector3(initialpos.x + 35, buttons[id].transform.position.y, initialpos.z);
        ItemActionsMenu.GetComponent<ItemActionsMenuScript>().slotID = id;
        ItemActionsMenu.GetComponent<ItemActionsMenuScript>().character = target;
        if (target.equipments[id].Name == null)
        {
            FillWithEmpty();
            ItemActionsMenu.gameObject.SetActive(false);
            statstext.SetActive(false);
            buttons[id].GetComponent<Button>().Select();
        }
        else if (target.equipments[id].Name.ToLower() == "fists" || target.equipments[id].Name.ToLower() == "fist" || target.equipments[id].Name == "")
        {
            FillWithEmpty();
            ItemActionsMenu.gameObject.SetActive(false);
            statstext.SetActive(false);
            buttons[id].GetComponent<Button>().Select();

        }
        else
        {

            equipment equ = target.equipments[id];

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
            if (modifer == null || modifer == "" || modifer == "Basic")
            {
                modifer = "Basic";
            }

            string typeicon = "";

            switch (equ.type.ToLower())
            {
                case ("sword"):
                    typeicon += "<sprite=0>";
                    break;
                case ("spear"):
                    typeicon += "<sprite=1>";
                    break;
                case ("greatsword"):
                    typeicon += "<sprite=2>";
                    break;
                case ("bow"):
                    typeicon += "<sprite=3>";
                    break;
                case ("scythe"):
                    typeicon += "<sprite=4>";
                    break;
                case ("shield"):
                    typeicon += "<sprite=6>";
                    break;
                case ("staff"):
                    typeicon += "<sprite=7>";
                    break;
                default:
                    typeicon += "<sprite=5>";
                    break;
            }

            BladeName.text = equ.Name;
            BladeType.text = typeicon;
            BladeDamage.text = "Damage: " + equ.BaseDamage;
            BladeHitRate.text = "Hit Rate: " + equ.BaseHit + "%";
            BladeCritRate.text = "Crit Rate: " + equ.BaseCrit + "%";
            BladeUses.text = "Uses : " + equ.Currentuses + " / " + equ.Maxuses;
            BladeRange.text = "Range: " + equ.Range;
            BladeMod.text = "Modifier: " + modifer;
            BladeGrade.text = "Grade: " + grade;
            statstext.SetActive(true);
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
                        switch (target.equipments[i].type.ToLower())
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
