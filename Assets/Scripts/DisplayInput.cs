using UnityEngine;

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
            LT.SetActive(true);
        }
        else
        {
            LT.SetActive(false);
        }

        if (InputManager.NextTargetpressed)
        {
            RT.SetActive(true);
        }
        else
        {
            RT.SetActive(false);
        }

        if (InputManager.PreviousWeaponpressed)
        {
            LB.SetActive(true);
        }
        else
        {
            LB.SetActive(false);
        }

        if (InputManager.NextWeaponpressed)
        {
            RB.SetActive(true);
        }
        else
        {
            RB.SetActive(false);
        }

        if (InputManager.activatepressed)
        {
            SouthButton.SetActive(true);
        }
        else
        {
            SouthButton.SetActive(false);
        }

        if (InputManager.cancelpressed)
        {
            EastButton.SetActive(true);
        }
        else
        {
            EastButton.SetActive(false);
        }

        if (InputManager.ShowDetailspressed)
        {
            WestButton.SetActive(true);
        }
        else
        {
            WestButton.SetActive(false);
        }

        if(InputManager.Telekinesispressed)
        {
            NorthButton.SetActive(true);
        }
        else
        {
            NorthButton.SetActive(false);
        }

        if(InputManager.Startpressed)
        {
            StartButton.SetActive(true);
        }
        else
        {
            StartButton.SetActive(false);
        }

        if(InputManager.Selectpressed)
        {
            SelectButton.SetActive(true);
        }
        else
        {
            SelectButton.SetActive(false);
        }

    }
}
