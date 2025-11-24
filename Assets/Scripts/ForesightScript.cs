using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static DataScript;
using static UnitScript;
using static GridSquareScript;

public class ForesightScript : MonoBehaviour
{

    [Serializable]
    public class Action
    {
        public UnitScript Unit;
        public int actiontype;
        /*
        0 Attack
        1 Heal
        2 Wait
        3 Command
        4 beginningOfTurn
        5 Talk
        6 Examined
         */
        public List<GridSquareScript> previousPosition;
        public List<Character> ModifiedCharacters;
        public AttackData AttackData;
        public int commandID;
        public List<TileModification> ModifiedTiles;
        public int skilltoremovefrominventory = -1;
        public int beginningofturn = -1; //0 : player, 1 : enemy, 2 : other
        public List<MapEventManager.EventCondition> PreviousEvents;
    }


    [Serializable]
    public class TileModification
    {
        public GridSquareScript tile;
        public string type;
        public int previousremainingsun;
        public int previousremainingrain;
        public MechanismClass MechanismClass;
    }

    [Serializable]
    public class AttackData
    {
        public UnitScript defender;
        public int previousattackerhitindex;
        public int previousdefenderhitindex;
        public int previousattackercritindex;
        public int previousdefendercritindex;
        public int previousattackerlvlupindex;
        public int previousdefenderlvlupindex;
        public int attackersurvivorstats = -1;
        public int defendersurvivorstats = -1;
    }

    public List<Action> actions = new List<Action>();

    private GameObject CharacterHolder;

    public List<Button> Buttons;

    public List<int> ButtonIDs;


    private InputManager InputManager;

    public GameObject neutralmenu;

    public MinimapScript MinimapScript;

    private int framesuppressed;
    private int framesdownpressed;

    private void OnEnable()
    {
        ButtonInitialization();
        InputManager = InputManager.instance;

    }


    private void FixedUpdate()
    {

        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
        if (currentSelected != null)
        {
            if (currentSelected.transform.parent != transform)
            {
                Debug.Log("Resetting selected to first button");
                EventSystem.current.SetSelectedGameObject(transform.GetChild(0).gameObject);
            }

            if (currentSelected == transform.GetChild(0).gameObject && ((InputManager.movementjustpressed && InputManager.movementValue.y > 0) || (InputManager.movecamjustpressed && InputManager.cammovementValue.y > 0)))
            {
                ChangeButtonID(-1);
            }

            if (currentSelected == transform.GetChild(Buttons.Count - 1).gameObject && ((InputManager.movementjustpressed && InputManager.movementValue.y < 0) || (InputManager.movecamjustpressed && InputManager.cammovementValue.y < 0)))
            {
                ChangeButtonID(1);
            }

            if (currentSelected == transform.GetChild(0).gameObject && (InputManager.movementValue.y > 0 || InputManager.cammovementValue.y > 0))
            {
                framesuppressed++;
                if (framesuppressed > 0.15f / Time.fixedDeltaTime)
                {
                    framesuppressed = 0;
                    ChangeButtonID(-1);
                }

            }
            else
            {
                framesuppressed = 0;
            }

            if (currentSelected == transform.GetChild(Buttons.Count - 1).gameObject && (InputManager.movementValue.y < 0 || InputManager.cammovementValue.y < 0))
            {
                framesdownpressed++;
                if (framesdownpressed > 0.15f / Time.fixedDeltaTime)
                {
                    framesdownpressed = 0;
                    ChangeButtonID(1);
                }

            }
            else
            {
                framesdownpressed = 0;
            }

        }


        if (InputManager.canceljustpressed)
        {
            gameObject.SetActive(false);
            neutralmenu.SetActive(true);
            Debug.Log("Resetting selected to first button");
            EventSystem.current.SetSelectedGameObject(neutralmenu.transform.GetChild(0).gameObject);
        }

    }

    private void ChangeButtonID(int direction)
    {
        if (direction == 1)
        {
            if (ButtonIDs[Buttons.Count - 1] < actions.Count - 1)
            {
                for (int i = 0; i < Buttons.Count; i++)
                {
                    ButtonIDs[i]++;
                }
            }
            UpdateButtonVisuals();
        }
        else if (direction == -1)
        {
            if (ButtonIDs[0] > 0)
            {
                for (int i = 0; i < Buttons.Count; i++)
                {
                    ButtonIDs[i]--;
                }
            }
            UpdateButtonVisuals();
        }
    }

    private void ButtonInitialization()
    {
        ButtonIDs = new List<int>();
        for (int i = 0; i < Buttons.Count; i++)
        {
            ButtonIDs.Add(-1);
        }
        int indexmod = 0;
        for (int i = Buttons.Count - 1; i >= 0; i--)
        {
            int newindex = actions.Count - 1 - indexmod;
            if (newindex >= 0)
            {
                ButtonIDs[i] = newindex;
            }

            indexmod++;
        }


        UpdateButtonVisuals();
    }

    private void UpdateButtonVisuals()
    {
        for (int i = 0; i < ButtonIDs.Count; i++)
        {
            if (ButtonIDs[i] == -1)
            {
                Buttons[i].GetComponent<Image>().color = Color.grey;
                Buttons[i].transform.GetChild(0).gameObject.SetActive(false);
            }
            else
            {
                string text = "";
                Buttons[i].transform.GetChild(0).gameObject.SetActive(true);
                Action currentaction = actions[ButtonIDs[i]];
                UnitScript unitwhoattacks = currentaction.Unit;
                if (unitwhoattacks != null)
                {
                    if (unitwhoattacks.UnitCharacteristics.affiliation == "playable")
                    {
                        Buttons[i].GetComponent<Image>().color = Color.blue;
                        Buttons[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;
                    }
                    else if (unitwhoattacks.UnitCharacteristics.affiliation == "enemy")
                    {
                        Buttons[i].GetComponent<Image>().color = Color.red;
                        Buttons[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;
                    }
                    else
                    {
                        Buttons[i].GetComponent<Image>().color = Color.yellow;
                        Buttons[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.black;
                    }
                    text = unitwhoattacks.UnitCharacteristics.name;
                    switch (currentaction.actiontype)
                    {
                        case 0:
                            text += " attacked " + currentaction.AttackData.defender.UnitCharacteristics.name + ".";
                            break;
                        case 1:
                            text += " healed " + currentaction.AttackData.defender.UnitCharacteristics.name + ".";
                            break;
                        case 2:
                            text += " waited.";
                            break;
                        case 3:
                            text += " used " + DataScript.instance.SkillList[currentaction.commandID].name + ".";
                            break;
                        case 5:
                            text += " talked with " + currentaction.AttackData.defender.UnitCharacteristics.name + ".";
                            break;
                        case 6:
                            text += " examined a device.";
                            break;
                    }
                }




                if (currentaction.beginningofturn == 0)
                {
                    text = "Beginning of Player Phase.";
                    Buttons[i].GetComponent<Image>().color = Color.blue;
                    Buttons[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;
                }
                else if (currentaction.beginningofturn == 1)
                {
                    Buttons[i].GetComponent<Image>().color = Color.red;
                    Buttons[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;
                    text = "Beginning of Enemy Phase.";
                }
                else if (currentaction.beginningofturn == 2)
                {
                    Buttons[i].GetComponent<Image>().color = Color.yellow;
                    Buttons[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.black;
                    text = "Beginning of Other Phase.";
                }

                Buttons[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;

            }
        }
    }


    public void RevertButton(int ButtonID)
    {
        if (ButtonID != -1)
        {
            RevertTo(ButtonIDs[ButtonID]);
            gameObject.SetActive(false);
            // neutralmenu.SetActive(true);
            // EventSystem.current.SetSelectedGameObject(neutralmenu.transform.GetChild(0).gameObject);
        }

    }
    private void RevertTo(int ID)
    {
        int actionLength = actions.Count;
        for (int i = actionLength - 1; i > ID; i--)
        {
            Action ActionToRevert = actions[i];

            if (CharacterHolder == null)
            {
                CharacterHolder = GameObject.Find("Characters");
            }

            MapEventManager.instance.EventsToMonitor = ActionToRevert.PreviousEvents;

            for (int j = 0; j < CharacterHolder.transform.childCount; j++)
            {
                GameObject GO = CharacterHolder.transform.GetChild(j).gameObject;
                foreach (Character Character in ActionToRevert.ModifiedCharacters)
                {
                    if (GO.GetComponent<UnitScript>().UnitCharacteristics.ID == Character.ID)
                    {
                        Debug.Log("reverting : " + Character.name);
                        GO.GetComponent<UnitScript>().UnitCharacteristics = Character;
                    }

                }
            }
            UpdateCharacterLists();
            if (ActionToRevert.Unit != null)
            {
                foreach (GameObject GO in GridScript.instance.allunitGOs)
                {

                    if (GO.GetComponent<UnitScript>().UnitCharacteristics.ID == ActionToRevert.Unit.UnitCharacteristics.ID)
                    {
                        GO.GetComponent<UnitScript>().MoveTo(ActionToRevert.previousPosition[0].GridCoordinates);
                        GO.GetComponent<UnitScript>().UnitCharacteristics.alreadymoved = false;
                        foreach (GridSquareScript tile in GO.GetComponent<UnitScript>().UnitCharacteristics.currentTile)
                        {
                            tile.UpdateInsideSprite(true, GO.GetComponent<UnitScript>().UnitCharacteristics);
                        }
                    }
                }
            }

            switch (ActionToRevert.actiontype)
            {
                case 0:
                    if (ActionToRevert.AttackData.attackersurvivorstats != -1)
                    {
                        ActionToRevert.Unit.SurvivorStacks = ActionToRevert.AttackData.attackersurvivorstats;
                    }
                    if (ActionToRevert.AttackData.defendersurvivorstats != -1)
                    {
                        ActionToRevert.AttackData.defender.SurvivorStacks = ActionToRevert.AttackData.defendersurvivorstats;
                    }
                    break;
                case 1:

                    break;
                case 2:

                    break;
                case 3:
                    foreach (TileModification tileModification in ActionToRevert.ModifiedTiles)
                    {
                        tileModification.tile.type = tileModification.type;
                        tileModification.tile.RemainingRainTurns = tileModification.previousremainingrain;
                        tileModification.tile.RemainingSunTurns = tileModification.previousremainingsun;
                        if (tileModification.MechanismClass != null)
                        {
                            tileModification.tile.Mechanism = tileModification.MechanismClass;
                        }
                    }
                    if (ActionToRevert.skilltoremovefrominventory != -1)
                    {
                        foreach (InventoryItem item in DataScript.instance.PlayerInventory.inventoryItems)
                        {
                            if (item.type == 1 && item.ID == ActionToRevert.skilltoremovefrominventory)
                            {
                                item.Quantity--;
                            }
                        }
                        foreach (GameObject GO in GridScript.instance.allunitGOs)
                        {
                            if (GO.GetComponent<UnitScript>().UnitCharacteristics.affiliation != "playable" && GO.GetComponent<UnitScript>().UnitCharacteristics.UnitSkill == ActionToRevert.skilltoremovefrominventory)
                            {
                                GO.GetComponent<UnitScript>().copied = false;
                            }
                        }
                    }
                    break;
                case 6:
                    foreach (TileModification tileModification in ActionToRevert.ModifiedTiles)
                    {
                        tileModification.tile.type = tileModification.type;
                        tileModification.tile.RemainingRainTurns = tileModification.previousremainingrain;
                        tileModification.tile.RemainingSunTurns = tileModification.previousremainingsun;
                        if (tileModification.MechanismClass != null)
                        {
                            tileModification.tile.Mechanism = tileModification.MechanismClass;
                        }
                    }
                    if (ActionToRevert.skilltoremovefrominventory != -1)
                    {
                        foreach (InventoryItem item in DataScript.instance.PlayerInventory.inventoryItems)
                        {
                            if (item.type == 1 && item.ID == ActionToRevert.skilltoremovefrominventory)
                            {
                                item.Quantity--;
                            }
                        }
                        foreach (GameObject GO in GridScript.instance.allunitGOs)
                        {
                            if (GO.GetComponent<UnitScript>().UnitCharacteristics.affiliation != "playable" && GO.GetComponent<UnitScript>().UnitCharacteristics.UnitSkill == ActionToRevert.skilltoremovefrominventory)
                            {
                                GO.GetComponent<UnitScript>().copied = false;
                            }
                        }
                    }
                    break;
            }
            GridScript.instance.InitializeGOList();
            RevertRolls(ActionToRevert);
            actions.Remove(ActionToRevert);
        }
        MinimapScript.UpdateMinimap();
    }


    public void AddAction(Action actiontoAdd)
    {
        actiontoAdd.PreviousEvents = MapEventManager.instance.CloneEvents();
        actions.Add(actiontoAdd);
    }

    private void RevertRolls(Action action)
    {

        switch (action.actiontype)
        {
            case 0:
                action.Unit.GetComponent<RandomScript>().hitvaluesindex = action.AttackData.previousattackerhitindex;
                action.Unit.GetComponent<RandomScript>().CritValuesindex = action.AttackData.previousattackercritindex;
                action.Unit.GetComponent<RandomScript>().levelvaluesindex = action.AttackData.previousattackerlvlupindex;
                action.AttackData.defender.GetComponent<RandomScript>().hitvaluesindex = action.AttackData.previousdefenderhitindex;
                action.AttackData.defender.GetComponent<RandomScript>().CritValuesindex = action.AttackData.previousdefendercritindex;
                action.AttackData.defender.GetComponent<RandomScript>().levelvaluesindex = action.AttackData.previousdefenderlvlupindex;
                break;
            case 1:

                break;
            case 2:

                break;
            case 3:
                action.Unit.GetComponent<RandomScript>().hitvaluesindex = action.AttackData.previousattackerhitindex;
                action.Unit.GetComponent<RandomScript>().CritValuesindex = action.AttackData.previousattackercritindex;
                action.Unit.GetComponent<RandomScript>().levelvaluesindex = action.AttackData.previousattackerlvlupindex;
                if (action.AttackData.defender != null)
                {
                    action.AttackData.defender.GetComponent<RandomScript>().hitvaluesindex = action.AttackData.previousdefenderhitindex;
                    action.AttackData.defender.GetComponent<RandomScript>().CritValuesindex = action.AttackData.previousdefendercritindex;
                    action.AttackData.defender.GetComponent<RandomScript>().levelvaluesindex = action.AttackData.previousdefenderlvlupindex;
                }
                break;
        }
    }

    private void UpdateCharacterLists()
    {
        if (CharacterHolder == null)
        {
            CharacterHolder = GameObject.Find("Characters");
        }
        for (int i = 0; i < CharacterHolder.transform.childCount; i++)
        {
            GameObject currentGO = CharacterHolder.transform.GetChild(i).gameObject;
            if (currentGO.activeSelf && !GridScript.instance.allunitGOs.Contains(currentGO))
            {
                GridScript.instance.allunitGOs.Add(currentGO);
                currentGO.SetActive(true);
            }
            else if (!currentGO.activeSelf && currentGO.GetComponent<UnitScript>().UnitCharacteristics.currentHP > 0)
            {
                if (!GridScript.instance.allunitGOs.Contains(currentGO))
                {
                    GridScript.instance.allunitGOs.Add(currentGO);
                    currentGO.SetActive(true);
                }
            }
        }
        GridScript.instance.GetComponent<TurnManger>().InitializeUnitLists(GridScript.instance.allunitGOs);

    }

}
