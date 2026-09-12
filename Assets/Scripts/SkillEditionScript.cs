using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static DataScript;
using static UnitScript;

public class SkillEditionScript : MonoBehaviour
{

    private GridScript gridscript;

    public TextMeshProUGUI BattalionText;

    public TextMeshProUGUI UnitsDeployedText;

    public Character selectedcharacter;

    private int characterwindowindex = 0;
    private int skillwindowindex = 0;
    public GameObject SkillList;

    public TextMeshProUGUI PageNumberText;
    public TextMeshProUGUI SkillPageNumberText;
    private List<InventoryItem> InventorySkillList;

    public TextMeshProUGUI UnitSkillText;
    public TextMeshProUGUI EquipedSkillText;
    public TextMeshProUGUI SkillDescriptionText;
    public TextMeshProUGUI SkillPointsText;
    public GameObject FirstUnitSKillIcon;
    public GameObject SecondUnitSKillIcon;
    public List<GameObject> EquipedSkillsIcons;
    public GameObject CurrentDescribedSkillIcon;


    public List<GameObject> PreBattleMenu;

    public List<GameObject> BondNewImages;

    public bool inCamp;
    public bool IsBonds;
    public GameObject CampMenu;

    private TextBubbleScript TextBubbleScript;

    private List<Character> unlockedplayables;

    private InputAction _CancelAction;
    private InputAction _NextWeaponAction;
    private InputAction _PreviousWeaponAction;
    public BondsScript BondsScript;

    public int previousselectedbutton;
    private Color basebuttoncolor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (!inCamp)
        {
            gridscript = GridScript.instance;
            TextBubbleScript = gridscript.GetComponent<ActionManager>().TextBubbleScript;
        }
        else
        {
            TextBubbleScript = FindAnyObjectByType<TextBubbleScript>(FindObjectsInactive.Include);
        }


        InitializeButtons();
    }

    private void Start()
    {
        _PreviousWeaponAction = InputSystem.actions.FindAction("PreviousWeapon");
        _NextWeaponAction = InputSystem.actions.FindAction("NextWeapon");
        _CancelAction = InputSystem.actions.FindAction("Cancel");
        Calculateplayables();
        basebuttoncolor = transform.GetChild(0).GetComponent<Button>().colors.normalColor;
    }

    // Update is called once per frame
    void Update()
    {
        InitializeInventorySkillList();

        if (!inCamp)
        {
            gridscript.movementbuffercounter = 3;
            if (_CancelAction.WasPressedThisFrame())
            {
                ChangeColorOfButton(previousselectedbutton, basebuttoncolor);
                if (SkillList.activeSelf)
                {
                    SkillList.SetActive(false);
                    EventSystem.current.SetSelectedGameObject(transform.GetChild(previousselectedbutton).gameObject);
                }
                else
                {
                    if (!PreBattleMenu[0].activeSelf)
                    {
                        foreach (GameObject item in PreBattleMenu)
                        {
                            item.SetActive(true);
                        }
                        EventSystem.current.SetSelectedGameObject(PreBattleMenu[0]);
                        gameObject.SetActive(false);
                    }
                }

                return;

            }
        }
        else
        {
            if (_CancelAction.WasPressedThisFrame())
            {
                ChangeColorOfButton(previousselectedbutton, basebuttoncolor);
                if (!IsBonds)
                {
                    if (SkillList.activeSelf)
                    {
                        SkillList.SetActive(false);
                        EventSystem.current.SetSelectedGameObject(transform.GetChild(previousselectedbutton).gameObject);
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




        PageNumberText.text = (characterwindowindex + 1) + "/" + Mathf.Max(1, (unlockedplayables.Count / 10));
        if (SkillPageNumberText.gameObject.activeSelf)
        {
            SkillPageNumberText.text = (skillwindowindex + 1) + "/" + Mathf.Max(1, ((InventorySkillList.Count / 10) + 1));
        }


        if (_PreviousWeaponAction.WasPressedThisFrame())
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

        if (_NextWeaponAction.WasPressedThisFrame())
        {
            if (SkillList.activeSelf)
            {
                if (skillwindowindex * 10 < InventorySkillList.Count - 10)
                {
                    skillwindowindex++;
                    InitializeSkillButtons();
                }
            }
            else
            {
                if (characterwindowindex * 10 < unlockedplayables.Count - 10)
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
                int remainingskillpoints = UpdateSkillPointText();
                UpdateEquipedSkillText();

                UpdateSkillDescriptionText(EventSystem.current.currentSelectedGameObject.GetComponent<UnitDeploymentButton>(), remainingskillpoints);
            }
            else
            {
                Character currentchar = EventSystem.current.currentSelectedGameObject.GetComponent<UnitDeploymentButton>().Character;
            }

        }


        if (IsBonds)
        {
            changeBondsbuttoncolor();
        }

    }

    private void Calculateplayables()
    {
        unlockedplayables = new List<Character>();
        foreach (Character character in DataScript.instance.PlayableCharacterList)
        {
            if (character.playableStats.unlocked)
            {
                unlockedplayables.Add(character);
            }
        }
    }
    private void UpdateEquipedSkillText()
    {





        Character unitchar = selectedcharacter;

        string UnitSkillTexts = selectedcharacter.name + "'s Skills:\n";

        if (unitchar.UnitSkill != 0)
        {
            Skill FirstSkill = DataScript.instance.SkillList[unitchar.UnitSkill];
            UnitSkillTexts += FirstSkill.name + "\n";
            if (!FirstUnitSKillIcon.activeSelf)
            {
                FirstUnitSKillIcon.SetActive(true);
            }
            FirstUnitSKillIcon.GetComponent<SkillIconScript>().InitializeIcon(FirstSkill.SkillIconInfo);
        }
        else
        {
            UnitSkillTexts += "None\n";
            if (FirstUnitSKillIcon.activeSelf)
            {
                FirstUnitSKillIcon.SetActive(false);
            }
        }

        if (unitchar.SecondSkillUnlocked && unitchar.SecondUnitSkill != 0)
        {
            Skill SecondSkill = DataScript.instance.SkillList[unitchar.SecondUnitSkill];
            UnitSkillTexts += SecondSkill.name;
            if (!SecondUnitSKillIcon.activeSelf)
            {
                SecondUnitSKillIcon.SetActive(true);
            }
            SecondUnitSKillIcon.GetComponent<SkillIconScript>().InitializeIcon(SecondSkill.SkillIconInfo);
        }
        else
        {
            UnitSkillTexts += "Locked";
            if (SecondUnitSKillIcon.activeSelf)
            {
                SecondUnitSKillIcon.SetActive(false);
            }
        }

        string equipedskills = "";
        int nbrofequipedskills = 0;

        for (int i = 0; i < Mathf.Min(unitchar.EquipedSkills.Count, 4); i++)
        {
            if (unitchar.EquipedSkills[i] != 0)
            {
                nbrofequipedskills++;
            }
        }

        equipedskills += "Equiped Skills " + nbrofequipedskills + "/4 :\n";

        for (int i = 0; i < 4; i++)
        {
            if (unitchar.EquipedSkills != null && unitchar.EquipedSkills.Count > i && unitchar.EquipedSkills[i] != 0)
            {
                Skill equipedskill = DataScript.instance.SkillList[unitchar.EquipedSkills[i]];
                equipedskills += equipedskill.name + "\n";
                if (!EquipedSkillsIcons[i].activeSelf)
                {
                    EquipedSkillsIcons[i].SetActive(true);
                }
                EquipedSkillsIcons[i].GetComponent<SkillIconScript>().InitializeIcon(equipedskill.SkillIconInfo);
            }
            else
            {
                if (EquipedSkillsIcons[i].activeSelf)
                {
                    EquipedSkillsIcons[i].SetActive(false);
                }
            }
        }
        UnitSkillText.text = UnitSkillTexts;
        EquipedSkillText.text = equipedskills;
    }
    private void UpdateSkillDescriptionText(UnitDeploymentButton SkillButton, int remainingSkillPoints)
    {
        if (SkillButton.Item != null)
        {
            int SkillID = SkillButton.Item.ID;
            if (SkillID > 0)
            {
                Skill skill = DataScript.instance.SkillList[SkillID];
                string DescriptionText = "<align=center>" + skill.name + "\n";
                string Color = "";
                if (remainingSkillPoints < skill.Cost)
                {
                    Color = "<color=red>";
                }

                DescriptionText += "<align=left>Cost: " + Color + skill.Cost + "</color>\n";
                if (skill.IsCommand)
                {
                    DescriptionText += "Type : Command\n";
                }
                else
                {
                    DescriptionText += "Type : Skill\n";
                }
                DescriptionText += "<align=left>Effect: " + skill.Descriptions;
                SkillDescriptionText.text = DescriptionText;
                if (!CurrentDescribedSkillIcon.activeSelf)
                {
                    CurrentDescribedSkillIcon.SetActive(true);
                }
                CurrentDescribedSkillIcon.GetComponent<SkillIconScript>().InitializeIcon(skill.SkillIconInfo);
            }
            else
            {
                SkillDescriptionText.text = "";
                if (CurrentDescribedSkillIcon.activeSelf)
                {
                    CurrentDescribedSkillIcon.SetActive(false);
                }
            }

        }
    }

    private int UpdateSkillPointText()
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
        return unitchar.playableStats.MaxSkillpoints - equipedskillpoitns;
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

    public void InitializeButtons()
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

    private void changeBondsbuttoncolor()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (!transform.GetChild(i).GetComponent<Button>() && !transform.GetChild(i).GetComponent<UnitDeploymentButton>())
            {
                continue;
            }
            if (BondNewImages.Count > i)
            {
                if (BondNewImages[i].activeSelf)
                {
                    BondNewImages[i].SetActive(false);
                }
            }


        }
        for (int i = 0; i < transform.childCount; i++)
        {
            if (!transform.GetChild(i).GetComponent<Button>() && !transform.GetChild(i).GetComponent<UnitDeploymentButton>())
            {
                continue;
            }
            if (transform.GetChild(i).GetComponent<UnitDeploymentButton>().CharacterID != -1)
            {
                foreach (Bonds bond in DataScript.instance.BondsList)
                {
                    if (BondsScript.instance.CheckIfBondCanIncrease(bond))
                    {
                        foreach (int ID in bond.Characters)
                        {
                            if (ID == transform.GetChild(i).GetComponent<UnitDeploymentButton>().CharacterID)
                            {
                                if (BondNewImages.Count > i)
                                {
                                    if (!BondNewImages[i].activeSelf)
                                    {
                                        BondNewImages[i].SetActive(true);
                                    }
                                }
                            }
                        }
                    }
                }
            }

        }
    }

    private void InitializeSkillButtons()
    {
        for (int i = 0; i < Mathf.Min(InventorySkillList.Count - 10 * (skillwindowindex), 10); i++)
        {
            InventoryItem item = InventorySkillList[i + 10 * (skillwindowindex)];
            SkillList.transform.GetChild(i).GetComponent<UnitDeploymentButton>().Item = item;
            SkillList.transform.GetChild(i).GetComponentInChildren<SkillIconScript>().InitializeIcon(DataScript.instance.SkillList[item.ID].SkillIconInfo);
        }
        for (int i = Mathf.Min(InventorySkillList.Count - 10 * (skillwindowindex), 10); i < 10; i++)
        {
            SkillList.transform.GetChild(i).GetComponent<UnitDeploymentButton>().Item = null;
            SkillList.transform.GetChild(i).GetComponentInChildren<SkillIconScript>().DisableIcon();
        }

        EventSystem.current.SetSelectedGameObject(SkillList.transform.GetChild(0).gameObject);
    }

    public void SelectUnit(int ButtonID)
    {
        if (TextBubbleScript.indialogue)
        {
            return;
        }
        if (transform.GetChild(ButtonID).GetComponent<UnitDeploymentButton>().Character != null)
        {
            if (transform.GetChild(ButtonID).GetComponent<UnitDeploymentButton>().Character.name != "")
            {
                selectedcharacter = transform.GetChild(ButtonID).GetComponent<UnitDeploymentButton>().Character;
                previousselectedbutton = ButtonID;
                if (IsBonds)
                {
                    BondsScript.instance.LoadCharacterBonds(selectedcharacter);
                }
                else
                {
                    ChangeColorOfButton(ButtonID, Color.red);
                    skillwindowindex = 0;
                    InitializeSkillButtons();
                    SkillList.SetActive(true);

                }
            }
        }
    }


    private void ChangeColorOfButton(int ButtonID, Color newcolor)
    {
        ColorBlock colors = transform.GetChild(ButtonID).GetComponent<Button>().colors;
        colors.normalColor = newcolor;
        transform.GetChild(ButtonID).GetComponent<Button>().colors = colors;
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
