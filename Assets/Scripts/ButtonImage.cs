using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ButtonImage : MonoBehaviour
{

    public Sprite AButton;
    public Sprite BButton;
    public Sprite XButton;
    public Sprite YButton;
    public Sprite LBButton;
    public Sprite RBButton;
    public Sprite LTButton;
    public Sprite RTButton;
    public Sprite DPadUp;
    public Sprite DPadDown;
    public Sprite DPadLeft;
    public Sprite DPadRight;
    public Sprite Stick;
    public Sprite KeyBoardKey;
    public TextMeshProUGUI keyboardtext;

    public string WhattoShow;

    private Image image;

    private bool previousstate;
    private void Update()
    {
        if((Gamepad.current!=null) !=previousstate)
        {
            OnEnable();
        }
        previousstate = (Gamepad.current != null);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        image = GetComponent<Image>();
        if(Gamepad.current!=null)
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
                    keyboardtext.text = "à";
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
