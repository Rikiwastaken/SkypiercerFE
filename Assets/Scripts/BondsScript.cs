using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static DataScript;
using static TextBubbleScript;
using static UnitScript;

public class BondsScript : MonoBehaviour
{

    public static BondsScript instance;

    [Serializable]

    public class BondsDialogueClass
    {
        public int BondID;
        public Bonds Bond;
        public List<TextBubbleInfo> dialogueLvl1;
        public List<TextBubbleInfo> dialogueLvl2;
        public List<TextBubbleInfo> dialogueLvl3;
        public List<TextBubbleInfo> dialogueLvl4;
    }

    [Serializable]

    public class AllBondsDialogueToLoadClass
    {
        public List<BondsDialogueToLoadClass> AllBondDialogueToLoadClass;
    }


    [Serializable]

    public class BondsDialogueToLoadClass
    {
        public int BondID;
        public List<SimplifiedDialogues> dialogueLvl1;
        public List<SimplifiedDialogues> dialogueLvl2;
        public List<SimplifiedDialogues> dialogueLvl3;
        public List<SimplifiedDialogues> dialogueLvl4;
    }

    [Serializable]

    public class SimplifiedDialogues
    {
        public int CharacterID;
        public string text;
    }

    public List<BondsDialogueClass> AllbondsDialogue;

    public Transform BondsMenu;
    public GameObject bondsSubMenu;

    private List<int> SubMenuCharactersID;

    private List<BondsDialogueClass> PertinentbondsDialogue;

    private InputManager inputmanager;


    public Button BondButton;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (BondsDialogueClass bonddialogue in AllbondsDialogue)
        {
            foreach (Bonds bond in DataScript.instance.BondsList)
            {
                if (bond.ID == bonddialogue.BondID)
                {
                    bonddialogue.Bond = bond;
                }
            }
        }
        inputmanager = InputManager.instance;
    }

    private void Update()
    {
        if (inputmanager.canceljustpressed)
        {
            if (bondsSubMenu.activeSelf)
            {
                bondsSubMenu.SetActive(false);
                EventSystem.current.SetSelectedGameObject(BondsMenu.transform.GetChild(0).gameObject);
            }

        }
        if (GetComponent<CampScript>().BaseMenu.gameObject.activeSelf)
        {
            bool anybondcanincrease = false;
            foreach (Bonds bond in DataScript.instance.BondsList)
            {
                if (CheckIfBondCanIncrease(bond))
                {
                    anybondcanincrease = true;
                    break;
                }
            }

            if (anybondcanincrease)
            {
                var colors = BondButton.colors;
                colors.normalColor = Color.green;
                BondButton.colors = colors;
            }
            else
            {
                var colors = BondButton.colors;
                colors.normalColor = Color.white;
                BondButton.colors = colors;
            }
        }
    }

    public void LoadCharacterBonds(Character Character)
    {
        bondsSubMenu.SetActive(true);
        SubMenuCharactersID = new List<int>();
        PertinentbondsDialogue = new List<BondsDialogueClass>();
        foreach (BondsDialogueClass bond in AllbondsDialogue)
        {
            if (bond.Bond.Characters.Contains(Character.ID))
            {

                bool skipcharacter = false;

                foreach (Character character in DataScript.instance.PlayableCharacterList)
                {
                    if (bond.Bond.Characters.Contains(character.ID) && !character.playableStats.unlocked)
                    {
                        skipcharacter = true;
                        continue;
                    }
                }

                if (skipcharacter)
                {
                    continue;
                }

                PertinentbondsDialogue.Add(bond);
                foreach (int ID in bond.Bond.Characters)
                {
                    if (ID != Character.ID)
                    {
                        SubMenuCharactersID.Add(ID);
                        break;
                    }
                }
            }
        }


        for (int i = 0; i < Mathf.Min(bondsSubMenu.transform.childCount, SubMenuCharactersID.Count); i++)
        {

            int othercharacterID = SubMenuCharactersID[i];

            Character othercharacter = null;



            foreach (Character character in DataScript.instance.PlayableCharacterList)
            {
                if (character.ID == othercharacterID)
                {
                    othercharacter = character;
                    break;
                }
            }

            bondsSubMenu.transform.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text = othercharacter.name + "\nBond Lvl " + PertinentbondsDialogue[i].Bond.BondLevel + " (Max " + PertinentbondsDialogue[i].Bond.MaxLevel + ")";

            if (CheckIfBondCanIncrease(PertinentbondsDialogue[i].Bond))
            {
                var colors = bondsSubMenu.transform.GetChild(i).GetComponent<Button>().colors;
                colors.normalColor = Color.blue;
                bondsSubMenu.transform.GetChild(i).GetComponent<Button>().colors = colors;
                bondsSubMenu.transform.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
            }
            else
            {
                var colors = bondsSubMenu.transform.GetChild(i).GetComponent<Button>().colors;
                colors.normalColor = Color.white;
                bondsSubMenu.transform.GetChild(i).GetComponent<Button>().colors = colors;
                bondsSubMenu.transform.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
                switch (PertinentbondsDialogue[i].Bond.BondLevel)
                {
                    case 0:
                        bondsSubMenu.transform.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text += " (" + PertinentbondsDialogue[i].Bond.BondPoints + "/10)";
                        break;
                    case 1:
                        bondsSubMenu.transform.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text += " (" + PertinentbondsDialogue[i].Bond.BondPoints + "/35)";
                        break;
                    case 2:
                        bondsSubMenu.transform.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text += " (" + PertinentbondsDialogue[i].Bond.BondPoints + "/85)";
                        break;
                    case 3:
                        bondsSubMenu.transform.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text += " (Max)";
                        break;
                }

            }
        }

        for (int i = Mathf.Min(bondsSubMenu.transform.childCount, SubMenuCharactersID.Count); i < bondsSubMenu.transform.childCount; i++)
        {

            bondsSubMenu.transform.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text = "None";

        }

        EventSystem.current.SetSelectedGameObject(bondsSubMenu.transform.GetChild(0).gameObject);
    }

    private bool CheckIfBondCanIncrease(Bonds bond)
    {
        bool result = false;


        switch (bond.BondLevel)
        {
            case 0:
                if (bond.BondPoints >= 10)
                {
                    result = true;
                }
                break;
            case 1:
                if (bond.BondPoints >= 35)
                {
                    result = true;
                }
                break;
            case 2:
                if (bond.BondPoints >= 85 && bond.MaxLevel > 2)
                {
                    result = true;
                }
                break;
            case 3:
                if (bond.BondPoints >= 85 && CheckIfBondLevel4isAvailable(bond))
                {
                    result = true;
                }
                break;
        }
        return result;
    }

    private bool CheckIfBondLevel4isAvailable(Bonds bond)
    {

        if (bond.MaxLevel <= 3)
        {
            return false;
        }

        foreach (Bonds otherbond in DataScript.instance.BondsList)
        {
            if (bond.ID != otherbond.ID)
            {
                if ((otherbond.Characters.Contains(bond.Characters[0]) || otherbond.Characters.Contains(bond.Characters[1])) && otherbond.BondLevel >= 4)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void OpenBondDialogue(int buttonID)
    {
        if (PertinentbondsDialogue.Count > buttonID && CheckIfBondCanIncrease(PertinentbondsDialogue[buttonID].Bond))
        {
            PertinentbondsDialogue[buttonID].Bond.BondLevel++;
            if (PertinentbondsDialogue[buttonID].Bond.BondLevel == 1)
            {
                TextBubbleScript.Instance.InitializeDialogue(PertinentbondsDialogue[buttonID].dialogueLvl1);
                if (bondsSubMenu.activeSelf)
                {
                    BondsMenu.gameObject.SetActive(false);
                    bondsSubMenu.SetActive(false);
                    EventSystem.current.SetSelectedGameObject(BondsMenu.transform.GetChild(0).gameObject);
                }
            }
            else if (PertinentbondsDialogue[buttonID].Bond.BondLevel == 2)
            {
                TextBubbleScript.Instance.InitializeDialogue(PertinentbondsDialogue[buttonID].dialogueLvl2);
                if (bondsSubMenu.activeSelf)
                {
                    BondsMenu.gameObject.SetActive(false);
                    bondsSubMenu.SetActive(false);
                    EventSystem.current.SetSelectedGameObject(BondsMenu.transform.GetChild(0).gameObject);
                }
            }
            else if (PertinentbondsDialogue[buttonID].Bond.BondLevel == 3)
            {
                TextBubbleScript.Instance.InitializeDialogue(PertinentbondsDialogue[buttonID].dialogueLvl3);
                if (bondsSubMenu.activeSelf)
                {
                    BondsMenu.gameObject.SetActive(false);
                    bondsSubMenu.SetActive(false);
                    EventSystem.current.SetSelectedGameObject(BondsMenu.transform.GetChild(0).gameObject);
                }
            }
            else if (PertinentbondsDialogue[buttonID].Bond.BondLevel == 4)
            {
                TextBubbleScript.Instance.InitializeDialogue(PertinentbondsDialogue[buttonID].dialogueLvl4);
                if (bondsSubMenu.activeSelf)
                {
                    BondsMenu.gameObject.SetActive(false);
                    bondsSubMenu.SetActive(false);
                    EventSystem.current.SetSelectedGameObject(BondsMenu.transform.GetChild(0).gameObject);
                }
            }
        }
    }
#if UNITY_EDITOR

    [ContextMenu("Load Bonds Dialogues")]
    public void LoadBonds()
    {
        string path = EditorUtility.OpenFilePanel("Select Bond JSON File", "", "json");
        if (string.IsNullOrEmpty(path))
            return;

        string json = File.ReadAllText(path);

        AllBondsDialogueToLoadClass wrapper = JsonUtility.FromJson<AllBondsDialogueToLoadClass>(json);
        if (wrapper == null || wrapper.AllBondDialogueToLoadClass == null)
        {
            Debug.LogError("JSON file format invalid. Needs { \"AllBondDialogueToLoadClass\": [ ... ] }");
            return;
        }

        foreach (BondsDialogueToLoadClass bonddialogue in wrapper.AllBondDialogueToLoadClass)
        {
            foreach (BondsDialogueClass bond in AllbondsDialogue)
            {
                if (bonddialogue.BondID == bond.BondID)
                {
                    if (bonddialogue.dialogueLvl1.Count() > 0)
                    {
                        List<TextBubbleInfo> newdialogue1list = new List<TextBubbleInfo>();
                        for (int i = 0; i < bonddialogue.dialogueLvl1.Count; i++)
                        {
                            newdialogue1list.Add(new TextBubbleInfo() { characterindex = bonddialogue.dialogueLvl1[i].CharacterID, text = bonddialogue.dialogueLvl1[i].text });
                        }
                        bond.dialogueLvl1 = newdialogue1list;
                    }

                    if (bonddialogue.dialogueLvl2.Count() > 0)
                    {
                        List<TextBubbleInfo> newdialogue2list = new List<TextBubbleInfo>();
                        for (int i = 0; i < bonddialogue.dialogueLvl2.Count; i++)
                        {
                            newdialogue2list.Add(new TextBubbleInfo() { characterindex = bonddialogue.dialogueLvl2[i].CharacterID, text = bonddialogue.dialogueLvl2[i].text });
                        }
                        bond.dialogueLvl2 = newdialogue2list;
                    }

                    if (bonddialogue.dialogueLvl3.Count() > 0)
                    {
                        List<TextBubbleInfo> newdialogue3list = new List<TextBubbleInfo>();
                        for (int i = 0; i < bonddialogue.dialogueLvl3.Count; i++)
                        {
                            newdialogue3list.Add(new TextBubbleInfo() { characterindex = bonddialogue.dialogueLvl3[i].CharacterID, text = bonddialogue.dialogueLvl3[i].text });
                        }
                        bond.dialogueLvl3 = newdialogue3list;
                    }

                    if (bonddialogue.dialogueLvl4.Count() > 0)
                    {
                        List<TextBubbleInfo> newdialogue4list = new List<TextBubbleInfo>();
                        for (int i = 0; i < bonddialogue.dialogueLvl4.Count; i++)
                        {
                            newdialogue4list.Add(new TextBubbleInfo() { characterindex = bonddialogue.dialogueLvl4[i].CharacterID, text = bonddialogue.dialogueLvl4[i].text });
                        }
                        bond.dialogueLvl4 = newdialogue4list;
                    }

                }
            }
        }
        EditorUtility.SetDirty(this);
    }

#endif
}
