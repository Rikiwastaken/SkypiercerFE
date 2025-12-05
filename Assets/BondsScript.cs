using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
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
    }

    public List<BondsDialogueClass> AllbondsDialogue;

    public Transform BondsMenu;
    public GameObject bondsSubMenu;

    private List<int> SubMenuCharactersID;

    private List<BondsDialogueClass> PertinentbondsDialogue;

    private InputManager inputmanager;

    

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach(BondsDialogueClass bonddialogue in AllbondsDialogue)
        {
            foreach(Bonds bond in DataScript.instance.BondsList)
            {
                if(bond.ID==bonddialogue.BondID)
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
            if(bondsSubMenu.activeSelf)
            {
                bondsSubMenu.SetActive(false);
                EventSystem.current.SetSelectedGameObject(BondsMenu.transform.GetChild(0).gameObject); 
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
            if(bond.Bond.Characters.Contains(Character.ID))
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

                if(skipcharacter)
                {
                    continue;
                }

                PertinentbondsDialogue.Add(bond);
                foreach (int ID in bond.Bond.Characters)
                {
                    if(ID != Character.ID)
                    {
                        SubMenuCharactersID.Add(ID);
                        break;
                    }
                }
            }
        }


        for(int i = 0; i< Mathf.Min(bondsSubMenu.transform.childCount, SubMenuCharactersID.Count);i++)
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

            bondsSubMenu.transform.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text = othercharacter.name + "\nBond Lvl "+PertinentbondsDialogue[i].Bond.BondLevel;

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
                switch(PertinentbondsDialogue[i].Bond.BondLevel)
                {
                    case 0:
                        bondsSubMenu.transform.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text += " ("+ PertinentbondsDialogue[i].Bond .BondPoints+ "/10)";
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


        switch(bond.BondLevel)
        {
            case 0:
                if(bond.BondPoints>=10)
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
                if (bond.BondPoints >= 85)
                {
                    result = true;
                }
                break;
        }
        return result;
    }

    public void OpenBondDialogue(int buttonID)
    {
        if (PertinentbondsDialogue.Count> buttonID && CheckIfBondCanIncrease(PertinentbondsDialogue[buttonID].Bond))
        {
            PertinentbondsDialogue[buttonID].Bond.BondLevel++;
            if(PertinentbondsDialogue[buttonID].Bond.BondLevel==1)
            {
                TextBubbleScript.Instance.InitializeDialogue(PertinentbondsDialogue[buttonID].dialogueLvl1);
                if (bondsSubMenu.activeSelf)
                {
                    bondsSubMenu.SetActive(false);
                    EventSystem.current.SetSelectedGameObject(BondsMenu.transform.GetChild(0).gameObject);
                }
            }
            else if (PertinentbondsDialogue[buttonID].Bond.BondLevel == 2)
            {
                TextBubbleScript.Instance.InitializeDialogue(PertinentbondsDialogue[buttonID].dialogueLvl2);
                if (bondsSubMenu.activeSelf)
                {
                    bondsSubMenu.SetActive(false);
                    EventSystem.current.SetSelectedGameObject(BondsMenu.transform.GetChild(0).gameObject);
                }
            }
            else if (PertinentbondsDialogue[buttonID].Bond.BondLevel == 3)
            {
                TextBubbleScript.Instance.InitializeDialogue(PertinentbondsDialogue[buttonID].dialogueLvl3);
                if (bondsSubMenu.activeSelf)
                {
                    bondsSubMenu.SetActive(false);
                    EventSystem.current.SetSelectedGameObject(BondsMenu.transform.GetChild(0).gameObject);
                }
            }
        }           
    }
}
