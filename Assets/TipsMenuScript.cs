using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TipsMenuScript : MonoBehaviour
{

    public List<Button> Buttons;

    public List<int> ButtonIDs;


    private InputManager InputManager;

    private int framesuppressed;
    private int framesdownpressed;

    public GameObject neutralmenu;

    public TextMeshProUGUI descriptionText;

    [Serializable]
    public class Tip
    {
        public string name;
        public string description;
        public int chapterWhereUnlocks;
    }

    public List<Tip> Tips;

    private GameObject previousselection;

    private void OnEnable()
    {
        ButtonInitialization();

        InputManager = InputManager.instance;

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
        if (currentSelected != null)
        {
            if (currentSelected.transform.parent != transform)
            {
                EventSystem.current.SetSelectedGameObject(transform.GetChild(0).gameObject);
            }

            if (currentSelected == transform.GetChild(0).gameObject && ((InputManager.movementjustpressed && InputManager.movementValue.y > 0) || (InputManager.movecamjustpressed && InputManager.cammovementValue.y > 0)) && previousselection == currentSelected)
            {
                ChangeButtonID(-1);
            }

            if (currentSelected == transform.GetChild(Buttons.Count - 1).gameObject && ((InputManager.movementjustpressed && InputManager.movementValue.y < 0) || (InputManager.movecamjustpressed && InputManager.cammovementValue.y < 0)) && previousselection == currentSelected)
            {
                ChangeButtonID(1);
            }

            if (Buttons.IndexOf(currentSelected.GetComponent<Button>()) < ButtonIDs.Count && Buttons.IndexOf(currentSelected.GetComponent<Button>()) !=-1 && ButtonIDs[Buttons.IndexOf(currentSelected.GetComponent<Button>())] != -1)
            {
                if (!descriptionText.transform.parent.gameObject.activeSelf)
                {
                    descriptionText.transform.parent.gameObject.SetActive(true);
                }
                descriptionText.text = Tips[ButtonIDs[Buttons.IndexOf(currentSelected.GetComponent<Button>())]].description;
            }
            else
            {
                if (descriptionText.transform.parent.gameObject.activeSelf)
                {
                    descriptionText.transform.parent.gameObject.SetActive(false);
                }
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
            EventSystem.current.SetSelectedGameObject(neutralmenu.transform.GetChild(0).gameObject);
        }

        previousselection = currentSelected;
    }

    private void ChangeButtonID(int direction)
    {
        if (direction == 1)
        {
            if (ButtonIDs[Buttons.Count - 1] < Tips.Count - 1)
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
                Buttons[i].transform.GetChild(0).gameObject.SetActive(true);
                Buttons[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = Tips[ButtonIDs[i]].name;

            }
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
            int newindex = (int)Math.Min(Tips.Count, Buttons.Count) - 1 - indexmod;
            if (newindex >= 0)
            {
                ButtonIDs[i] = newindex;
            }

            indexmod++;
        }


        UpdateButtonVisuals();
    }
}
