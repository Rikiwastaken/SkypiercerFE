using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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

    private TextBubbleScript TextBubbleScript;

    public Button BondButton;

    private int cancelcounter;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public DataScript debugdatascript;

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

        TextBubbleScript = FindAnyObjectByType<TextBubbleScript>(FindObjectsInactive.Include);

    }

    private void Update()
    {
        if (cancelcounter > 0)
        {
            cancelcounter--;
        }

        if (InputSystem.actions.FindAction("Cancel").IsPressed())
        {
            if (bondsSubMenu.activeSelf && cancelcounter <= 0)
            {
                cancelcounter = (int)(0.5f / Time.deltaTime);
                bondsSubMenu.SetActive(false);
                EventSystem.current.SetSelectedGameObject(BondsMenu.transform.GetChild(0).gameObject);
            }

        }

    }

    private void OnEnable()
    {
        ChangeColorIfBondsCanBeIncreased();
    }

    private void ChangeColorIfBondsCanBeIncreased()
    {
        if (GetComponent<CampScript>().BaseMenu.gameObject.activeSelf)
        {
            if (DataScript.instance != null)
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
                bondsSubMenu.transform.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
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

    public bool CheckIfBondCanIncrease(Bonds bond)
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
                TextBubbleScript.InitializeDialogue(PertinentbondsDialogue[buttonID].dialogueLvl1);
                if (bondsSubMenu.activeSelf)
                {
                    BondsMenu.gameObject.SetActive(false);
                    bondsSubMenu.SetActive(false);
                    EventSystem.current.SetSelectedGameObject(BondsMenu.transform.GetChild(0).gameObject);
                }
            }
            else if (PertinentbondsDialogue[buttonID].Bond.BondLevel == 2)
            {
                TextBubbleScript.InitializeDialogue(PertinentbondsDialogue[buttonID].dialogueLvl2);
                if (bondsSubMenu.activeSelf)
                {
                    BondsMenu.gameObject.SetActive(false);
                    bondsSubMenu.SetActive(false);
                    EventSystem.current.SetSelectedGameObject(BondsMenu.transform.GetChild(0).gameObject);
                }
            }
            else if (PertinentbondsDialogue[buttonID].Bond.BondLevel == 3)
            {
                TextBubbleScript.InitializeDialogue(PertinentbondsDialogue[buttonID].dialogueLvl3);
                if (bondsSubMenu.activeSelf)
                {
                    BondsMenu.gameObject.SetActive(false);
                    bondsSubMenu.SetActive(false);
                    EventSystem.current.SetSelectedGameObject(BondsMenu.transform.GetChild(0).gameObject);
                }
            }
            else if (PertinentbondsDialogue[buttonID].Bond.BondLevel == 4)
            {
                TextBubbleScript.InitializeDialogue(PertinentbondsDialogue[buttonID].dialogueLvl4);
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

        AllbondsDialogue = new List<BondsDialogueClass>();

        foreach (DataScript.Bonds bond in debugdatascript.BondsList)
        {
            AllbondsDialogue.Add(new BondsDialogueClass() { Bond = bond, BondID = bond.ID });
        }


        foreach (BondsDialogueToLoadClass bonddialogue in wrapper.AllBondDialogueToLoadClass)
        {
            foreach (BondsDialogueClass bond in AllbondsDialogue)
            {
                if (bonddialogue.BondID == bond.BondID)
                {
                    if (bonddialogue.dialogueLvl1.Count() > 1)
                    {
                        List<TextBubbleInfo> newdialogue1list = new List<TextBubbleInfo>();
                        for (int i = 0; i < bonddialogue.dialogueLvl1.Count; i++)
                        {
                            newdialogue1list.Add(new TextBubbleInfo() { characterindex = bonddialogue.dialogueLvl1[i].CharacterID, text = bonddialogue.dialogueLvl1[i].text });
                        }
                        bond.dialogueLvl1 = newdialogue1list;
                    }

                    if (bonddialogue.dialogueLvl2.Count() > 1)
                    {
                        List<TextBubbleInfo> newdialogue2list = new List<TextBubbleInfo>();
                        for (int i = 0; i < bonddialogue.dialogueLvl2.Count; i++)
                        {
                            newdialogue2list.Add(new TextBubbleInfo() { characterindex = bonddialogue.dialogueLvl2[i].CharacterID, text = bonddialogue.dialogueLvl2[i].text });
                        }
                        bond.dialogueLvl2 = newdialogue2list;
                    }

                    if (bonddialogue.dialogueLvl3.Count() > 1)
                    {
                        List<TextBubbleInfo> newdialogue3list = new List<TextBubbleInfo>();
                        for (int i = 0; i < bonddialogue.dialogueLvl3.Count; i++)
                        {
                            newdialogue3list.Add(new TextBubbleInfo() { characterindex = bonddialogue.dialogueLvl3[i].CharacterID, text = bonddialogue.dialogueLvl3[i].text });
                        }
                        bond.dialogueLvl3 = newdialogue3list;
                    }

                    if (bonddialogue.dialogueLvl4.Count() > 1)
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

        Debug.Log("bonds loaded");
    }

    private List<SimplifiedDialogues> TurnDialogueListIntoSimplifiedList(List<TextBubbleInfo> list)
    {
        List<SimplifiedDialogues> newlist = new List<SimplifiedDialogues>();

        foreach (TextBubbleInfo txt in list)
        {
            newlist.Add(new SimplifiedDialogues() { CharacterID = txt.characterindex, text = txt.text });
        }
        return newlist;
    }

    [ContextMenu("Create Bonds Dialogues JSON")]
    public void CreateBondsJSON()
    {
        string path = EditorUtility.OpenFilePanel("Select Bond JSON File", "", "json");
        if (string.IsNullOrEmpty(path))
            return;


        AllBondsDialogueToLoadClass allbondsclass = new AllBondsDialogueToLoadClass() { AllBondDialogueToLoadClass = new List<BondsDialogueToLoadClass>() };

        foreach (BondsDialogueClass bond in AllbondsDialogue)
        {

            if (bond.dialogueLvl1 != null && bond.dialogueLvl1.Count > 2)
            {
                allbondsclass.AllBondDialogueToLoadClass.Add(new BondsDialogueToLoadClass() { BondID = bond.BondID, dialogueLvl1 = TurnDialogueListIntoSimplifiedList(bond.dialogueLvl1), dialogueLvl2 = TurnDialogueListIntoSimplifiedList(bond.dialogueLvl2), dialogueLvl3 = TurnDialogueListIntoSimplifiedList(bond.dialogueLvl3), dialogueLvl4 = TurnDialogueListIntoSimplifiedList(bond.dialogueLvl4) });
            }
        }

        foreach (Bonds bond in debugdatascript.BondsList)
        {
            bool bondexists = false;
            foreach (BondsDialogueToLoadClass donebond in allbondsclass.AllBondDialogueToLoadClass)
            {
                if (donebond.BondID == bond.ID)
                {
                    bondexists = true;
                    break;
                }
            }
            if (!bondexists)
            {
                BondsDialogueToLoadClass newbondtoadd = new BondsDialogueToLoadClass();
                newbondtoadd.BondID = bond.ID;
                UnitScript.Character firstcharacter = debugdatascript.PlayableCharacterList[bond.Characters[0]];
                UnitScript.Character secondcharacter = debugdatascript.PlayableCharacterList[bond.Characters[1]];
                newbondtoadd.dialogueLvl1 = new List<SimplifiedDialogues>() { new SimplifiedDialogues() { CharacterID = bond.Characters[0], text = "bond 1 between " + firstcharacter.name + " and " + secondcharacter.name + ", with " + firstcharacter.name + " talking" }, new SimplifiedDialogues() { CharacterID = bond.Characters[1], text = "bond 1 between " + secondcharacter.name + " and " + firstcharacter.name + ", with " + secondcharacter.name + " talking" } };
                newbondtoadd.dialogueLvl2 = new List<SimplifiedDialogues>();
                newbondtoadd.dialogueLvl3 = new List<SimplifiedDialogues>();
                newbondtoadd.dialogueLvl4 = new List<SimplifiedDialogues>();
                if (bond.MaxLevel > 1)
                {
                    newbondtoadd.dialogueLvl2.Add(new SimplifiedDialogues() { CharacterID = bond.Characters[0], text = "bond 2 between " + firstcharacter.name + " and " + secondcharacter.name + ", with " + firstcharacter.name + " talking" });
                    newbondtoadd.dialogueLvl2.Add(new SimplifiedDialogues() { CharacterID = bond.Characters[1], text = "bond 2 between " + secondcharacter.name + " and " + firstcharacter.name + ", with " + secondcharacter.name + " talking" });
                }
                if (bond.MaxLevel > 2)
                {
                    newbondtoadd.dialogueLvl3.Add(new SimplifiedDialogues() { CharacterID = bond.Characters[0], text = "bond 3 between " + firstcharacter.name + " and " + secondcharacter.name + ", with " + firstcharacter.name + " talking" });
                    newbondtoadd.dialogueLvl3.Add(new SimplifiedDialogues() { CharacterID = bond.Characters[1], text = "bond 3 between " + secondcharacter.name + " and " + firstcharacter.name + ", with " + secondcharacter.name + " talking" });
                }
                if (bond.MaxLevel > 3)
                {
                    newbondtoadd.dialogueLvl4.Add(new SimplifiedDialogues() { CharacterID = bond.Characters[0], text = "bond 4 between " + firstcharacter.name + " and " + secondcharacter.name + ", with " + firstcharacter.name + " talking" });
                    newbondtoadd.dialogueLvl4.Add(new SimplifiedDialogues() { CharacterID = bond.Characters[1], text = "bond 4 between " + secondcharacter.name + " and " + firstcharacter.name + ", with " + secondcharacter.name + " talking" });
                }
                allbondsclass.AllBondDialogueToLoadClass.Add(newbondtoadd);
            }
        }

        string json = JsonUtility.ToJson(allbondsclass, true);

        try
        {
            AssetDatabase.StartAssetEditing();
            File.WriteAllText(path, json);
            Debug.Log($"Skill Saved : {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error when saving options : {e.Message}");
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

    }

#endif
}
