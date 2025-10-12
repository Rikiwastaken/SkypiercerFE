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
            if (!LT.activeSelf)
            {
                LT.SetActive(true);
            }
        }
        else
        {
            LT.SetActive(false);
        }

        if (InputManager.NextTargetpressed)
        {
            if (!RT.activeSelf)
            {
                RT.SetActive(true);
            }
        }
        else
        {
            RT.SetActive(false);
        }

        if (InputManager.PreviousWeaponpressed)
        {
            if (!LB.activeSelf)
            {
                LB.SetActive(true);
            }
        }
        else
        {
            LB.SetActive(false);
        }

        if (InputManager.NextWeaponpressed)
        {
            if (!RB.activeSelf)
            {
                RB.SetActive(true);
            }

        }
        else
        {
            RB.SetActive(false);
        }

        if (InputManager.activatepressed)
        {
            if (SouthButton.activeSelf == false)
            {
                SouthButton.SetActive(true);
            }
        }
        else
        {
            SouthButton.SetActive(false);
        }

        if (InputManager.cancelpressed)
        {
            if (!EastButton.activeSelf)
            {
                EastButton.SetActive(true);
            }
        }
        else
        {
            EastButton.SetActive(false);
        }

        if (InputManager.ShowDetailspressed)
        {
            if (!WestButton.activeSelf)
            {
                WestButton.SetActive(true);
            }
        }
        else
        {
            WestButton.SetActive(false);
        }

        if (InputManager.Telekinesispressed)
        {
            if (!NorthButton.activeSelf)
            {
                NorthButton.SetActive(true);
            }
        }
        else
        {
            NorthButton.SetActive(false);
        }

        if (InputManager.Startpressed)
        {
            if (!StartButton.activeSelf)
            {
                StartButton.SetActive(true);
            }
        }
        else
        {
            StartButton.SetActive(false);
        }

        if (InputManager.Selectpressed)
        {
            if (!SelectButton.activeSelf)
            {
                SelectButton.SetActive(true);
            }
        }
        else
        {
            SelectButton.SetActive(false);
        }

    }
}
