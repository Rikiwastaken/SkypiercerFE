using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static DataScript;
using static UnitScript;

public class SkillShopScript : MonoBehaviour
{
    [SerializeField] private Transform ButtonHolder;

    [SerializeField] private TextMeshProUGUI SkillPageNumberText;
    [SerializeField] private TextMeshProUGUI DescriptionText;
    [SerializeField] private TextMeshProUGUI InventoryText;
    [SerializeField] private TextMeshProUGUI BuyText;
    [SerializeField] private Image BuyImage;
    [SerializeField] private TextMeshProUGUI SkillCoinsText;
    [SerializeField] private Button SkillShopButton;

    private List<Button> Buttons;

    private List<InventoryItem> InventorySkillList;

    [SerializeField] private List<Skill> SkillsToShow;

    private int skillwindowindex;

    private InputManager inputmanager;

    private GameObject previousselected;

    private int necessarycost;

    [SerializeField] private float timenecessarytobuyitem;
    private float timeforbuy;

    // Update is called once per frame
    void Update()
    {

        if (inputmanager == null)
        {
            inputmanager = InputManager.instance;
        }


        // Close Menu
        if (inputmanager.cancelpressed)
        {
            gameObject.SetActive(false);
            SkillShopButton.Select();

            return;

        }


        // Change Windows
        if (inputmanager.PreviousWeaponjustpressed)
        {
            if (skillwindowindex > 0)
            {
                skillwindowindex--;
                InitializeSkillButtons();
            }
        }

        if (inputmanager.NextWeaponjustpressed)
        {
            if (skillwindowindex * 10 < SkillsToShow.Count - 9)
            {
                skillwindowindex++;
                InitializeSkillButtons();
            }

        }



        GameObject CurrentSelected = EventSystem.current.currentSelectedGameObject;


        // If the current selected button is a skill button, update the description and buy text, and if the player presses the buy button and has enough skill coins, buy the skill and update the inventory and skill coin text, also updates the buy image to show the progress of buying the skill
        if (CurrentSelected.GetComponent<UnitDeploymentButton>())
        {
            if (previousselected != CurrentSelected)
            {
                InitializeInventoryText(CurrentSelected.GetComponent<UnitDeploymentButton>());
                UpdateSkillDescriptionText(CurrentSelected.GetComponent<UnitDeploymentButton>());
            }

            if (inputmanager.activatepressed && DataScript.instance.SkillCoins >= necessarycost)
            {
                float ratio = 1f - (timeforbuy - Time.time) / timenecessarytobuyitem;
                BuyImage.fillAmount = ratio;
                if (Time.time >= timeforbuy)
                {
                    timeforbuy = Time.time + timenecessarytobuyitem;
                    DataScript.instance.SkillCoins -= necessarycost;
                    foreach (InventoryItem skill in DataScript.instance.PlayerInventory.inventoryItems)
                    {
                        if (skill.ID == CurrentSelected.GetComponent<UnitDeploymentButton>().Item.ID)
                        {
                            skill.Quantity++;
                            break;
                        }
                    }

                    InitializeSkillButtons();
                    InitializeInventorySkillList();
                    SkillCoinsText.text = "Skill Coins: " + DataScript.instance.SkillCoins;
                    InitializeInventoryText(CurrentSelected.GetComponent<UnitDeploymentButton>());
                    UpdateSkillDescriptionText(CurrentSelected.GetComponent<UnitDeploymentButton>());

                }
            }
            else
            {
                timeforbuy = Time.time + timenecessarytobuyitem;
                BuyImage.fillAmount = 0;
            }

        }

        if (InventorySkillList != null && SkillsToShow.Count > 0)
        {
            SkillPageNumberText.text = (skillwindowindex + 1) + "/" + (SkillsToShow.Count / 10 + 1);
        }



        previousselected = CurrentSelected;
    }


    //// <summary>/ Function called when pressing the button to open the skill shop, initializes the skills to show and the buttons, and selects the first button
    //// </summary>
    public void ActivateShop()
    {
        SkillsToShow = new List<Skill>();

        List<int> skillIDshown = new List<int>();

        foreach (SkillPerMap skillpermap in DataScript.instance.skillsPerMap)
        {
            if (skillpermap.SkillsOnTheMap == null && skillpermap.SkillsOnTheMap.Count == 0)
            {
                continue;
            }
            foreach (int skillID in skillpermap.SkillsOnTheMap)
            {
                if (!skillIDshown.Contains(skillID))
                {
                    skillIDshown.Add(skillID);
                }
            }
        }

        foreach (Character character in DataScript.instance.PlayableCharacterList)
        {
            if (character.playableStats.unlocked && !skillIDshown.Contains(character.UnitSkill))
            {
                skillIDshown.Add(character.UnitSkill);
            }
        }

        foreach (Skill skill in DataScript.instance.SkillList)
        {
            if (skill.buyable && skillIDshown.Contains(skill.ID))
            {
                SkillsToShow.Add(skill);
            }
        }
        InitializeSkillButtons();
        EventSystem.current.SetSelectedGameObject(Buttons[0].gameObject);
        InitializeInventorySkillList();

        SkillCoinsText.text = "Skill Coins: " + DataScript.instance.SkillCoins;
        timeforbuy = Time.time + timenecessarytobuyitem;
    }

    /// <summary>/ Function to update the description text when selecting a skill, also updates the buy text to show the cost and if the player can afford it
    /// </summary>
    private void UpdateSkillDescriptionText(UnitDeploymentButton SkillButton)
    {
        if (SkillButton.Item != null)
        {
            int SkillID = SkillButton.Item.ID;
            if (SkillID > 0)
            {
                Skill skill = DataScript.instance.SkillList[SkillID];

                DescriptionText.text = "Cost : " + skill.Cost + "\n";
                if (skill.IsCommand)
                {
                    DescriptionText.text += "Type : Command\n";
                }
                else
                {
                    DescriptionText.text += "Type : Skill\n";
                }
                DescriptionText.text += skill.Descriptions;
                necessarycost = skill.Cost;

                if (DataScript.instance.SkillCoins < necessarycost)
                {
                    BuyText.text = "Buy : <color=red>" + necessarycost + "</color> SC";
                }
                else
                {
                    BuyText.text = "Buy : " + necessarycost + " SC";
                }



            }
            else
            {
                DescriptionText.text = "None";
                BuyText.text = "";
            }

        }
    }

    /// <summary>
    /// Function to update the inventory text when selecting a skill, shows how many of that skill the player has in their inventory
    /// </summary>
    /// <param name="SkillButton"></param>
    private void InitializeInventoryText(UnitDeploymentButton SkillButton)
    {
        int numberheld = 0;
        if (SkillButton.Item != null)
        {
            int SkillID = SkillButton.Item.ID;

            foreach (InventoryItem skillheld in InventorySkillList)
            {
                if (skillheld.ID == SkillID)
                {
                    numberheld = skillheld.Quantity;
                    break;
                }
            }

        }
        InventoryText.text = "In Inventory : " + numberheld;
    }

    /// <summary>
    /// Function to initialize the skill buttons, shows the skills in the current window and sets the item of the button to the corresponding inventory item if it exists, otherwise sets it to null
    /// </summary>
    private void InitializeSkillButtons()
    {
        Buttons = new List<Button>();
        foreach (Transform child in ButtonHolder)
        {
            Buttons.Add(child.GetComponent<Button>());
        }
        for (int i = 0; i < Mathf.Min(SkillsToShow.Count - 10 * (skillwindowindex), 10); i++)
        {
            InventoryItem item = null;
            foreach (InventoryItem invitem in DataScript.instance.PlayerInventory.inventoryItems)
            {
                if (invitem.ID == SkillsToShow[i + 10 * (skillwindowindex)].ID)
                {
                    item = invitem;
                }
            }


            Buttons[i].GetComponent<UnitDeploymentButton>().Item = item;
        }
        for (int i = Mathf.Min(SkillsToShow.Count - 10 * (skillwindowindex), 10); i < 10; i++)
        {
            Buttons[i].GetComponent<UnitDeploymentButton>().Item = null;
        }


    }

    /// <summary>
    /// Function to initialize the list of skills in the player's inventory, checks the player's inventory and adds any skill that is in the inventory and has a quantity greater than 0, also adds any skill that is equipped by a playable character, even if the player doesn't have it in their inventory, to allow the player to see the description of the skill and buy it if they want to
    /// </summary>
    private void InitializeInventorySkillList()
    {
        InventorySkillList = new List<InventoryItem>();
        foreach (InventoryItem item in DataScript.instance.PlayerInventory.inventoryItems)
        {
            if (!InventorySkillList.Contains(item) && item.type == 1 && item.Quantity > 0)
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
                    if (!InventorySkillList.Contains(item) && item.ID == playablechar.UnitSkill)
                    {
                        InventorySkillList.Add(item);
                    }
                }
                foreach (int EquipedSkillID in playablechar.EquipedSkills)
                {
                    foreach (InventoryItem item in DataScript.instance.PlayerInventory.inventoryItems)
                    {
                        if (!InventorySkillList.Contains(item) && item.type == 1 && item.ID == EquipedSkillID)
                        {
                            InventorySkillList.Add(item);
                        }
                    }
                }
            }

        }
    }
}
