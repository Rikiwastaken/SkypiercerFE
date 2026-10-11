using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ButtonImage : MonoBehaviour
{

    [SerializeField] private Sprite AButton;
    [SerializeField] private Sprite BButton;
    [SerializeField] private Sprite XButton;
    [SerializeField] private Sprite YButton;
    [SerializeField] private Sprite LBButton;
    [SerializeField] private Sprite RBButton;
    [SerializeField] private Sprite LTButton;
    [SerializeField] private Sprite RTButton;
    [SerializeField] private Sprite DPadUp;
    [SerializeField] private Sprite DPadDown;
    [SerializeField] private Sprite DPadLeft;
    [SerializeField] private Sprite DPadRight;
    [SerializeField] private Sprite Stick;
    [SerializeField] private Sprite KeyBoardKey;
    [SerializeField] private TextMeshProUGUI keyboardtext;

    [SerializeField] private string WhattoShow;

    private Image image;

    private bool previousstate;
    private void Update()
    {
        if ((Gamepad.current != null) != previousstate)
        {
            OnEnable();
        }
        previousstate = (Gamepad.current != null);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        image = GetComponent<Image>();
        if (Gamepad.current != null)
        {
            keyboardtext.fontSize = 40;
            keyboardtext.text = "";
            switch (WhattoShow)
            {
                case ("A"):
                    image.sprite = AButton;
                    break;
                case ("B"):
                    image.sprite = BButton;
                    break;
                case ("X"):
                    image.sprite = XButton;
                    break;
                case ("Y"):
                    image.sprite = YButton;
                    break;
                case ("LB"):
                    image.sprite = LBButton;
                    break;
                case ("RB"):
                    image.sprite = RBButton;
                    break;
                case ("LT"):
                    image.sprite = LTButton;
                    break;
                case ("RT"):
                    image.sprite = RTButton;
                    break;
                case ("DPadUp"):
                    image.sprite = DPadUp;
                    break;
                case ("DPadDown"):
                    image.sprite = DPadDown;
                    break;
                case ("DPadLeft"):
                    image.sprite = DPadLeft;
                    break;
                case ("DPadRight"):
                    image.sprite = DPadRight;
                    break;
                case ("LeftStick"):
                    image.sprite = Stick;
                    keyboardtext.text = "L";
                    break;
                case ("RightStick"):
                    image.sprite = Stick;
                    keyboardtext.text = "R";
                    break;
            }
        }
        else
        {
            keyboardtext.fontSize = 40;
            image.sprite = KeyBoardKey;
            switch (WhattoShow)
            {
                case ("A"):
                    keyboardtext.text = "K";

                    break;
                case ("B"):
                    keyboardtext.text = "L";
                    break;
                case ("X"):
                    keyboardtext.text = "I";
                    break;
                case ("Y"):
                    keyboardtext.text = "O";
                    break;
                case ("LB"):
                    keyboardtext.text = "A";
                    break;
                case ("RB"):
                    keyboardtext.text = "P";
                    break;
                case ("LT"):
                    keyboardtext.text = "&";
                    break;
                case ("RT"):
                    keyboardtext.text = "�";
                    break;
                case ("DPadUp"):
                    keyboardtext.text = "";
                    break;
                case ("DPadDown"):
                    image.sprite = DPadDown;
                    break;
                case ("DPadLeft"):
                    image.sprite = DPadLeft;
                    break;
                case ("DPadRight"):
                    image.sprite = DPadRight;
                    break;
                case ("LeftStick"):
                    keyboardtext.text = "ZQSD";
                    break;
                case ("RightStick"):
                    keyboardtext.fontSize = 25;
                    keyboardtext.text = "Arrows";
                    break;
            }
        }

    }
}
