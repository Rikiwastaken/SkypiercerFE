using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnitScript;

public class BattallionChoiceScript : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI BattallionNameTMP;
    [SerializeField] private TextMeshProUGUI UnitEffectTMP;
    [SerializeField] private TextMeshProUGUI CommanderEffectTMP;
    [SerializeField] private TextMeshProUGUI UnitsInBattallionTMP;
    [SerializeField] private Image BattallionLeaderImage;
    [SerializeField] private Image UnitImage;
    [SerializeField] private Button ButtonToReturnToAfterClosingMenu;

    private Character CurrentSelectedChar;
    [SerializeField] private List<Character> CharacterList = new List<Character>();
    private DataScript _DataScript;

    [SerializeField] private List<Button> _Buttons;

    private GameObject PreviouslySelectedGO;
    private GameObject SelectedGO;
    private InputAction _MovementAction;
    private InputAction _MovementCam;
    private InputAction _CancelAction;
    private int ListPositionOffset = 0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_DataScript == null)
        {
            _DataScript = DataScript.instance;
        }
        _MovementAction = InputSystem.actions.FindAction("Movement");
        _MovementCam = InputSystem.actions.FindAction("MoveCam");
        _CancelAction = InputSystem.actions.FindAction("Cancel");
    }

    // Update is called once per frame
    void Update()
    {
        if (_CancelAction.WasPerformedThisFrame())
        {
            gameObject.SetActive(false);
            ButtonToReturnToAfterClosingMenu.Select();
        }



        SelectedGO = EventSystem.current.currentSelectedGameObject;

        if (SelectedGO == null)
        {
            _Buttons[0].Select();
            SelectedGO = _Buttons[0].gameObject;
            InitializeButtons();
        }

        if (SelectedGO == PreviouslySelectedGO)
        {
            Vector2 MovementValue = Vector2.zero;
            if (_MovementAction.WasPerformedThisFrame())
            {
                MovementValue = _MovementAction.ReadValue<Vector2>();
            }
            else if (_MovementCam.WasPerformedThisFrame())
            {
                MovementValue = _MovementCam.ReadValue<Vector2>();
            }

            if (SelectedGO == _Buttons[0].gameObject && MovementValue.y > 0)
            {
                //Monter dans la liste
                if (ListPositionOffset > 0)
                {
                    ListPositionOffset--;
                    InitializeButtons();
                }

            }
            if (SelectedGO == _Buttons[_Buttons.Count - 1].gameObject && MovementValue.y < 0)
            {
                //Descendre dans la liste
                if (_Buttons.Count + ListPositionOffset <= CharacterList.Count - 1)
                {
                    ListPositionOffset++;
                    InitializeButtons();
                }
            }

        }
        else
        {
            InitializeHUD();
        }


        PreviouslySelectedGO = SelectedGO;
    }

    public void Initialization()
    {
        if (_DataScript == null)
        {
            _DataScript = DataScript.instance;
        }
        CurrentSelectedChar = null;
        CharacterList = new List<Character>();
        foreach (Character character in _DataScript.PlayableCharacterList)
        {
            if (character.playableStats.unlocked && !character.PermaDied && !character.playableStats.protagonist)
            {
                CharacterList.Add(character);
            }
        }
        _Buttons[0].Select();
        SelectedGO = _Buttons[0].gameObject;
        ListPositionOffset = 0;
        InitializeButtons();
    }

    private void InitializeButtons()
    {
        for (int i = 0; i < _Buttons.Count; i++)
        {
            if (CharacterList.Count > i + ListPositionOffset)
            {
                _Buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = CharacterList[i + ListPositionOffset].name;
            }
            else
            {
                _Buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = "";
            }

        }
        InitializeHUD();
    }

    private void InitializeHUD()
    {
        int currentButtonIndex = 0;
        if (SelectedGO != null)
        {
            if (!_Buttons.Contains(SelectedGO.GetComponent<Button>()))
            {
                return;
            }
            currentButtonIndex = _Buttons.IndexOf(SelectedGO.GetComponent<Button>());
        }
        Character currentChar = null;
        if ((currentButtonIndex + ListPositionOffset) >= 0 && (currentButtonIndex + ListPositionOffset) <= CharacterList.Count - 1)
        {
            currentChar = CharacterList[currentButtonIndex + ListPositionOffset];
        }

        if (currentChar != null)
        {
            UnitImage.sprite = _DataScript.DialogueCharacterList[currentChar.ID].Portrait;
            string battallion = currentChar.playableStats.battalion;
            int numberofUnitsInBattallion = 0;
            switch (battallion.ToLower())
            {
                case "zack":
                    foreach (Character character in _DataScript.PlayableCharacterList)
                    {
                        if (character.playableStats.unlocked && !character.PermaDied && character.playableStats.battalion.ToLower() == battallion.ToLower())
                        {
                            numberofUnitsInBattallion++;
                        }
                    }
                    UnitsInBattallionTMP.text = "Units in battallion : " + numberofUnitsInBattallion;
                    BattallionLeaderImage.sprite = _DataScript.DialogueCharacterList[0].Portrait;
                    UnitEffectTMP.text = "Effect for Unit:\n+7% hit rate\n+3% crit rate";
                    CommanderEffectTMP.text = "Effect for Commander:\n+" + (numberofUnitsInBattallion) + "% hit rate\n+" + (numberofUnitsInBattallion) + "% crit rate";
                    BattallionNameTMP.text = "Crimson Landfallers";
                    break;
                case "kira":
                    int psybonus = 0;
                    foreach (Character character in _DataScript.PlayableCharacterList)
                    {
                        if (character.playableStats.unlocked && !character.PermaDied && character.playableStats.battalion.ToLower() == battallion.ToLower())
                        {
                            psybonus += Mathf.Max(1, (int)character.AjustedStats.Psyche / 20);
                            numberofUnitsInBattallion++;
                        }
                    }
                    UnitsInBattallionTMP.text = "Units in battallion : " + numberofUnitsInBattallion;
                    BattallionLeaderImage.sprite = _DataScript.DialogueCharacterList[9].Portrait;
                    UnitEffectTMP.text = "Effect for Unit:\n10% HP recovery at beginning of turn";
                    CommanderEffectTMP.text = "Effect for Commander:\n+" + psybonus + " Psyche.";
                    BattallionNameTMP.text = "Aegean Occidens";
                    break;
                case "gale":
                    int strbonus = 0;
                    int defbonus = 0;
                    foreach (Character character in _DataScript.PlayableCharacterList)
                    {
                        if (character.playableStats.unlocked && !character.PermaDied && character.playableStats.battalion.ToLower() == battallion.ToLower())
                        {
                            strbonus += Mathf.Max(1, (int)character.AjustedStats.Strength / 20);
                            defbonus += Mathf.Max(1, (int)character.AjustedStats.Defense / 20);
                            numberofUnitsInBattallion++;
                        }
                    }
                    UnitsInBattallionTMP.text = "Units in battallion : " + numberofUnitsInBattallion;
                    BattallionLeaderImage.sprite = _DataScript.DialogueCharacterList[13].Portrait;
                    UnitEffectTMP.text = "Effect for Unit:\nIncrease nearby allies' Strength by " + (int)(currentChar.AjustedStats.Strength / 5) + " and Defense by " + (int)(currentChar.AjustedStats.Defense / 5);
                    CommanderEffectTMP.text = "Effect for Commander:\n+" + strbonus + " Strength and +" + defbonus + " Defense";
                    BattallionNameTMP.text = "Eton Archadians";
                    break;
            }

        }



    }

    public void ChooseCharacterButton(int ButtonID)
    {

    }

}
