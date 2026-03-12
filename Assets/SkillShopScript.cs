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

        if (inputmanager.cancelpressed)
        {
            gameObject.SetActive(false);
            SkillShopButton.Select();

            return;

        }

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



    public void ActivateShop()
    {
        SkillsToShow = new List<Skill>();
        foreach (Skill skill in DataScript.instance.SkillList)
        {
            if (skill.buyable)
            {
                SkillsToShow.Add(skill);
            }
        }
        InitializeSkillButtons();
        EventSystem.current.SetSelectedGameObject(Buttons[0].gameObject);
        InitializeInventorySkillList();

        SkillCoinsText.text = "Skill Coins: " + DataScript.instance.SkillCoins;
    }

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
