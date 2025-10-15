using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisplayInput : MonoBehaviour
{

    public GameObject LT;
    public GameObject RT;
    public GameObject LB;
    public GameObject RB;
    public GameObject NorthButton;
    public GameObject SouthButton;
    public GameObject EastButton;
    public GameObject WestButton;
    public GameObject StartButton;
    public GameObject SelectButton;
    public GameObject DPadUp;
    public GameObject DPadDown;
    public GameObject DPadLeft;
    public GameObject DPadRight;
    public GameObject LeftStick;
    public GameObject RightStick;

    private Vector2 LeftStickBasepos;
    private Vector2 RightStickBasepos;

    public float stickmovement;

    private InputManager InputManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InputManager = FindAnyObjectByType<InputManager>();
        LeftStickBasepos = LeftStick.transform.localPosition;
        RightStickBasepos = RightStick.transform.localPosition;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        LeftStick.transform.localPosition = LeftStickBasepos + InputManager.movementValue * stickmovement;
        RightStick.transform.localPosition = RightStickBasepos + InputManager.cammovementValue * stickmovement;

        if (InputManager.PreviousTargetpressed)
        {
            ActivateButton(LT);
        }
        else
        {
            DeactivateButton(LT);
        }

        if (InputManager.NextTargetpressed)
        {
            ActivateButton(RT);
        }
        else
        {
            DeactivateButton(RT);
        }

        if (InputManager.PreviousWeaponpressed)
        {
            ActivateButton(LB);
        }
        else
        {
            DeactivateButton(LB);
        }

        if (InputManager.NextWeaponpressed)
        {
            ActivateButton(RB);

        }
        else
        {
            DeactivateButton(RB);
        }

        if (InputManager.activatepressed)
        {
            ActivateButton(SouthButton);
        }
        else
        {
            DeactivateButton(SouthButton);
        }

        if (InputManager.cancelpressed)
        {
            ActivateButton(EastButton);
        }
        else
        {
            DeactivateButton(EastButton);
        }

        if (InputManager.ShowDetailspressed)
        {
            ActivateButton(WestButton);
        }
        else
        {
            DeactivateButton(WestButton);
        }

        if (InputManager.Telekinesispressed)
        {
            ActivateButton(NorthButton);
        }
        else
        {
            DeactivateButton(NorthButton);
        }

        if (InputManager.Startpressed)
        {
            ActivateButton(StartButton);
        }
        else
        {
            DeactivateButton(StartButton);
        }

        if (InputManager.Selectpressed)
        {
            ActivateButton(SelectButton);
        }
        else
        {
            DeactivateButton(SelectButton);
        }

    }

    private void ActivateButton(GameObject button)
    {
        if(!button.GetComponent<Image>().enabled)
        {
            button.GetComponent<Image>().enabled = true;
        }
        
        if(button.transform.childCount > 1)
        {
            if(!button.transform.GetChild(0).GetComponent<Image>().enabled)
            {
                button.transform.GetChild(0).GetComponent<Image>().enabled = true;
                button.transform.GetChild(1).GetComponent<TextMeshProUGUI>().color = Color.white;
            }
        }
        else
        {
            button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;
        }
    }
    private void DeactivateButton(GameObject button)
    {
        if (button.GetComponent<Image>().enabled)
        {
            button.GetComponent<Image>().enabled = false;
        }

        if (button.transform.childCount > 1)
        {
            if (button.transform.GetChild(0).GetComponent<Image>().enabled)
            {
                button.transform.GetChild(0).GetComponent<Image>().enabled = false;
                button.transform.GetChild(1).GetComponent<TextMeshProUGUI>().color = Color.red;
            }
        }
        else
        {
            button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.red;
        }
    }

}
