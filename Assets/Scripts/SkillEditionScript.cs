using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using static DataScript;
using static UnitScript;

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

    public bool inCamp;
    public bool IsBonds;
    public GameObject CampMenu;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (!inCamp)
        {
            gridscript = GridScript.instance;
        }

        inputmanager = InputManager.instance;

        InitializeButtons();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        InitializeInventorySkillList();

        if (!inCamp)
        {
            gridscript.movementbuffercounter = 3;
            if (inputmanager.canceljustpressed)
            {
                if (SkillList.activeSelf)
                {
                    SkillList.SetActive(false);
                }
                foreach (GameObject go in PreBattleMenuItems)
                {
                    go.SetActive(true);
                }
                gameObject.SetActive(false);
                gridscript.InitializeGOList();
                return;

            }
        }
        else
        {
            if (inputmanager.canceljustpressed)
            {
                if (IsBonds)
                {
                    if (!BondsScript.instance.bondsSubMenu.activeSelf)
                    {
                        if (SkillList.activeSelf)
                        {
                            SkillList.SetActive(false);
                        }
                        CampMenu.SetActive(true);
                        gameObject.SetActive(false);
                        EventSystem.current.SetSelectedGameObject(CampMenu.transform.GetChild(1).gameObject);
                    }
                }
                else
                {
                    if (SkillList.activeSelf)
                    {
                        SkillList.SetActive(false);
                        EventSystem.current.SetSelectedGameObject(transform.GetChild(0).gameObject);
                    }
                    else
                    {
                        CampMenu.SetActive(true);
                        gameObject.SetActive(false);
                        EventSystem.current.SetSelectedGameObject(CampMenu.transform.GetChild(0).gameObject);
                    }


                }

                return;

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
        if ((!buttonselected || currentselected == null) && !SkillList.activeSelf && !IsBonds)
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
            if (!InventorySkillList.Contains(item) &&  item.type == 1 && item.Quantity > 0)
            {
                InventorySkillList.Add(item);
            }
        }
        foreach (Character playablechar in DataScript.instance.PlayableCharacterList)
        {

            if (playablechar.playableStats.unlocked && playablechar.UnitSkill != 0)
            {
                foreach (InventoryItem item in DataScript.instance.PlayerInventory.inventoryItems)
                {
                    if (!InventorySkillList.Contains(item) &&  item.ID == playablechar.UnitSkill)
                    {
                        InventorySkillList.Add(item);
                    }
                }
                foreach (int EquipedSkillID in playablechar.EquipedSkills)
                {
                    foreach (InventoryItem item in DataScript.instance.PlayerInventory.inventoryItems)
                    {
                        if (!InventorySkillList.Contains(item) && item.type == 1 && item.ID==EquipedSkillID)
                        {
                            InventorySkillList.Add(item);
                        }
                    }
                }
            }
            
        }
    }

    private void InitializeButtons()
    {

        List<Character> ListToUse = new List<Character>();
        if (IsBonds)
        {
            List<int> characterswithbonds = new List<int>();

            foreach (Bonds bond in DataScript.instance.BondsList)
            {
                foreach (int ID in bond.Characters)
                {
                    if (!characterswithbonds.Contains(ID))
                    {
                        characterswithbonds.Add(ID);
                    }
                }
            }


            foreach (Character playablechar in DataScript.instance.PlayableCharacterList)
            {
                if (playablechar.playableStats.unlocked && characterswithbonds.Contains(playablechar.ID))
                {
                    ListToUse.Add(playablechar);
                }
            }
        }
        else
        {

            foreach (Character playablechar in DataScript.instance.PlayableCharacterList)
            {
                if (playablechar.playableStats.unlocked)
                {
                    ListToUse.Add(playablechar);
                }
            }
        }

        for (int i = 0; i < Mathf.Min(ListToUse.Count - 10 * (characterwindowindex), 10); i++)
        {
            transform.GetChild(i).GetComponent<UnitDeploymentButton>().Character = ListToUse[i + 10 * (characterwindowindex)];
            transform.GetChild(i).GetComponent<UnitDeploymentButton>().CharacterID = i + 10 * (characterwindowindex);
        }
        for (int i = Mathf.Min(ListToUse.Count - 10 * (characterwindowindex), 10); i < 10; i++)
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
        if (TextBubbleScript.Instance.indialogue)
        {
            return;
        }
        if (transform.GetChild(ButtonID).GetComponent<UnitDeploymentButton>().Character != null)
        {
            if (transform.GetChild(ButtonID).GetComponent<UnitDeploymentButton>().Character.name != "")
            {
                selectedcharacter = DataScript.instance.PlayableCharacterList[ButtonID + characterwindowindex * 10];
                if (IsBonds)
                {
                    BondsScript.instance.LoadCharacterBonds(selectedcharacter);
                }
                else
                {
                    SkillList.SetActive(true);
                    InitializeSkillButtons();
                    skillwindowindex = 0;
                }
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
            if (character.playableStats.unlocked)
            {
                numberofunits++;
            }

        }
        return numberofunits;
    }

}
