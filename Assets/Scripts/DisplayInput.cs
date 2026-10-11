using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisplayInput : MonoBehaviour
{

    [SerializeField] private GameObject LT;
    [SerializeField] private GameObject RT;
    [SerializeField] private GameObject LB;
    [SerializeField] private GameObject RB;
    [SerializeField] private GameObject NorthButton;
    [SerializeField] private GameObject SouthButton;
    [SerializeField] private GameObject EastButton;
    [SerializeField] private GameObject WestButton;
    [SerializeField] private GameObject StartButton;
    [SerializeField] private GameObject SelectButton;
    [SerializeField] private GameObject DPadUp;
    [SerializeField] private GameObject DPadDown;
    [SerializeField] private GameObject DPadLeft;
    [SerializeField] private GameObject DPadRight;
    [SerializeField] private GameObject LeftStick;
    [SerializeField] private GameObject RightStick;

    private Vector2 LeftStickBasepos;
    private Vector2 RightStickBasepos;

    [SerializeField] private float stickmovement;

    private InputManager InputManager;
    public bool ActivateInput;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InputManager = InputManager.instance;
        LeftStickBasepos = LeftStick.transform.localPosition;
        RightStickBasepos = RightStick.transform.localPosition;
        if (!ActivateInput)
        {
            DeactivateTextAndImages(transform);
        }
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

        if (InputManager.ShowDangerousTilespressed)
        {
            ActivateButton(DPadUp);
        }
        else
        {
            DeactivateButton(DPadUp);
        }

        if (InputManager.ShowWeatherTilespressed)
        {
            ActivateButton(DPadLeft);
        }
        else
        {
            DeactivateButton(DPadLeft);
        }

    }

    private void DeactivateTextAndImages(Transform _Transform)
    {
        if (_Transform.GetComponent<TextMeshProUGUI>())
        {
            _Transform.GetComponent<TextMeshProUGUI>().enabled = false;
        }
        if (_Transform.GetComponent<Image>())
        {
            _Transform.GetComponent<Image>().enabled = false;
        }
        foreach (Transform child in _Transform)
        {
            DeactivateTextAndImages(child);
        }
    }

    private void ActivateButton(GameObject button)
    {
        if (!ActivateInput)
        {
            return;
        }
        if (!button.GetComponent<Image>().enabled)
        {
            button.GetComponent<Image>().enabled = true;
        }

        if (button.transform.childCount > 1)
        {
            if (!button.transform.GetChild(0).GetComponent<Image>().enabled)
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
