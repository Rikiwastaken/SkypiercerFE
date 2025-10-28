using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnitScript;
using static DataScript;

public class SkillEditionScript : MonoBehaviour
{

    private GridScript gridscript;

    private InputManager inputmanager;

    public List<GameObject> PreBattleMenuItems;

    public TextMeshProUGUI BattalionText;

    public TextMeshProUGUI UnitsDeployedText;

    public Character selectedcharacter;

    private int characterwindowindex = 0;
    private int skillwindowindex = 0;
    public GameObject SkillList;

    public TextMeshProUGUI PageNumberText;
    public TextMeshProUGUI SkillPageNumberText;
    private List<InventoryItem> InventorySkillList;

    public TextMeshProUGUI EquipedSkillText;
    public TextMeshProUGUI SkillDescriptionText;
    public TextMeshProUGUI SkillPointsText;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        gridscript = GridScript.instance;
        inputmanager = InputManager.instance;
        InitializeButtons();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        InitializeInventorySkillList();

        gridscript.movementbuffercounter = 3;

        if (inputmanager.canceljustpressed)
        {
            if (SkillList.activeSelf)
            {
                SkillList.SetActive(false);
            }
            else
            {
                if (numberofSelectedUnits() > 0)
                {
                    foreach (GameObject go in PreBattleMenuItems)
                    {
                        go.SetActive(true);
                    }
                    gameObject.SetActive(false);
                    gridscript.InitializeGOList();
                    return;
                }
            }

        }

        PageNumberText.text = (characterwindowindex + 1) + "/" + (DataScript.instance.PlayableCharacterList.Count / 10 + 1);
        if (SkillPageNumberText.gameObject.activeSelf)
        {
            SkillPageNumberText.text = (skillwindowindex + 1) + "/" + (InventorySkillList.Count / 10 + 1);
        }


        if (inputmanager.PreviousWeaponjustpressed)
        {
            if (SkillList.activeSelf)
            {
                if (skillwindowindex > 0)
                {
                    skillwindowindex--;
                    InitializeSkillButtons();
                }
            }
            else
            {
                if (characterwindowindex > 0)
                {
                    characterwindowindex--;
                    InitializeButtons();
                }
            }

        }

        if (inputmanager.NextWeaponjustpressed)
        {
            if (SkillList.activeSelf)
            {
                if (skillwindowindex * 10 < InventorySkillList.Count - 9)
                {
                    skillwindowindex++;
                    InitializeSkillButtons();
                }
            }
            else
            {
                if (characterwindowindex * 10 < DataScript.instance.PlayableCharacterList.Count - 9)
                {
                    characterwindowindex++;
                    InitializeButtons();
                }
            }

        }



        GameObject currentselected = EventSystem.current.currentSelectedGameObject;
        bool buttonselected = false;
        if (currentselected != null)
        {

            for (int i = 0; i < 10; i++)
            {
                if (transform.GetChild(i).gameObject == currentselected)
                {
                    buttonselected = true; break;
                }
            }
        }
        if ((!buttonselected || currentselected == null) && !SkillList.activeSelf)
        {
            EventSystem.current.SetSelectedGameObject(transform.GetChild(0).gameObject);
        }
        if (EventSystem.current.currentSelectedGameObject.GetComponent<UnitDeploymentButton>())
        {
            if (SkillList.activeSelf)
            {
                UpdateEquipedSkillText();
                UpdateSkillPointText();
                UpdateSkillDescriptionText(EventSystem.current.currentSelectedGameObject.GetComponent<UnitDeploymentButton>());
            }
            else
            {
                Character currentchar = EventSystem.current.currentSelectedGameObject.GetComponent<UnitDeploymentButton>().Character;
            }

        }


    }

    private void UpdateEquipedSkillText()
    {
        string equipedskills = "";

        Character unitchar = selectedcharacter;

        if (unitchar.UnitSkill != 0)
        {
            equipedskills = "Unit Skill :\n" + DataScript.instance.SkillList[unitchar.UnitSkill].name + "\n";
        }
        else
        {
            equipedskills = "Unit Skill :\nNone\n";
        }

        int nbrofequipedskills = 0;

        for (int i = 0; i < Mathf.Min(unitchar.EquipedSkills.Count, 4); i++)
        {
            if (unitchar.EquipedSkills[i] != 0)
            {
                nbrofequipedskills++;
            }
        }

        equipedskills += "Equ Skills " + nbrofequipedskills + "/4 :\n";

        for (int i = 0; i < Mathf.Min(unitchar.EquipedSkills.Count, 4); i++)
        {
            if (unitchar.EquipedSkills[i] != 0)
            {
                equipedskills += DataScript.instance.SkillList[unitchar.EquipedSkills[i]].name + "\n";
            }
        }

        EquipedSkillText.text = equipedskills;
    }
    private void UpdateSkillDescriptionText(UnitDeploymentButton SkillButton)
    {
        if (SkillButton.Item != null)
        {
            int SkillID = SkillButton.Item.ID;
            if (SkillID > 0)
            {
                Skill skill = DataScript.instance.SkillList[SkillID];

                SkillDescriptionText.text = "Cost : " + skill.Cost + "\n";
                if (skill.IsCommand)
                {
                    SkillDescriptionText.text += "Type : Command\n";
                }
                else
                {
                    SkillDescriptionText.text += "Type : Skill\n";
                }
                SkillDescriptionText.text += skill.Descriptions;
            }
            else
            {
                SkillDescriptionText.text = "None";
            }

        }
    }

    private void UpdateSkillPointText()
    {

        Character unitchar = selectedcharacter;

        int equipedskillpoitns = 0;

        for (int i = 0; i < Mathf.Min(unitchar.EquipedSkills.Count, 4); i++)
        {
            if (unitchar.EquipedSkills[i] != 0)
            {
                equipedskillpoitns += DataScript.instance.SkillList[unitchar.EquipedSkills[i]].Cost;
            }
        }

        SkillPointsText.text = "Skill Pts : " + equipedskillpoitns + "/" + unitchar.playableStats.MaxSkillpoints;
    }

    private void InitializeInventorySkillList()
    {
        InventorySkillList = new List<InventoryItem>();
        foreach (InventoryItem item in DataScript.instance.PlayerInventory.inventoryItems)
        {
            if (item.type == 1 && item.Quantity > 0)
            {
                InventorySkillList.Add(item);
            }
        }
    }

    private void InitializeButtons()
    {
        for (int i = 0; i < Mathf.Min(DataScript.instance.PlayableCharacterList.Count - 10 * (characterwindowindex), 10); i++)
        {
            transform.GetChild(i).GetComponent<UnitDeploymentButton>().Character = DataScript.instance.PlayableCharacterList[i + 10 * (characterwindowindex)];
            transform.GetChild(i).GetComponent<UnitDeploymentButton>().CharacterID = i + 10 * (characterwindowindex);
        }
        for (int i = Mathf.Min(DataScript.instance.PlayableCharacterList.Count - 10 * (characterwindowindex), 10); i < 10; i++)
        {
            transform.GetChild(i).GetComponent<UnitDeploymentButton>().Character = null;
        }
        EventSystem.current.SetSelectedGameObject(transform.GetChild(0).gameObject);
    }

    private void InitializeSkillButtons()
    {
        for (int i = 0; i < Mathf.Min(InventorySkillList.Count - 10 * (skillwindowindex), 10); i++)
        {
            SkillList.transform.GetChild(i).GetComponent<UnitDeploymentButton>().Item = InventorySkillList[i + 10 * (skillwindowindex)];
        }
        for (int i = Mathf.Min(InventorySkillList.Count - 10 * (skillwindowindex), 10); i < 10; i++)
        {
            SkillList.transform.GetChild(i).GetComponent<UnitDeploymentButton>().Item = null;
        }
        EventSystem.current.SetSelectedGameObject(SkillList.transform.GetChild(0).gameObject);
    }

    public void SelectUnit(int ButtonID)
    {
        if (transform.GetChild(ButtonID).GetComponent<UnitDeploymentButton>().Character != null)
        {
            if (transform.GetChild(ButtonID).GetComponent<UnitDeploymentButton>().Character.name != "")
            {
                selectedcharacter = DataScript.instance.PlayableCharacterList[ButtonID + characterwindowindex * 10];
                SkillList.SetActive(true);
                InitializeSkillButtons();
                skillwindowindex = 0;
            }
        }
    }

    public void EquipUnequipSkill(int childID)
    {
        if (SkillList.transform.GetChild(childID).GetComponent<UnitDeploymentButton>().Item != null)
        {
            if (SkillList.transform.GetChild(childID).GetComponent<UnitDeploymentButton>().Item.ID != 0)
            {
                int SkillID = SkillList.transform.GetChild(childID).GetComponent<UnitDeploymentButton>().Item.ID;
                if (selectedcharacter.EquipedSkills.Contains(SkillID))
                {
                    selectedcharacter.EquipedSkills.Remove(SkillID);
                    foreach (InventoryItem item in DataScript.instance.PlayerInventory.inventoryItems)
                    {
                        if (item.type == 1 && item.ID == SkillID)
                        {
                            item.Quantity++;
                        }
                    }
                }
                else
                {
                    if (selectedcharacter.UnitSkill != SkillID && selectedcharacter.EquipedSkills.Count < 4)
                    {
                        int equipedcost = 0;
                        foreach (int equskillID in selectedcharacter.EquipedSkills)
                        {
                            equipedcost += DataScript.instance.SkillList[equskillID].Cost;
                        }

                        if (equipedcost + DataScript.instance.SkillList[SkillID].Cost <= selectedcharacter.playableStats.MaxSkillpoints)
                        {
                            foreach (InventoryItem item in DataScript.instance.PlayerInventory.inventoryItems)
                            {
                                if (item.type == 1 && item.ID == SkillID && item.Quantity > 0)
                                {
                                    item.Quantity--;
                                    selectedcharacter.EquipedSkills.Add(SkillID);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

    }

    private int numberofSelectedUnits()
    {
        int numberofunits = 0;
        foreach (Character character in DataScript.instance.PlayableCharacterList)
        {
            if (character.playableStats.deployunit)
            {
                numberofunits++;
            }

        }
        return numberofunits;
    }

}
