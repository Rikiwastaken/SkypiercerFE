using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using static UnitScript;
using TMPro;

public class TextBubbleScript : MonoBehaviour
{

    [Serializable]
    public class TextBubbleInfo
    {
        public string name;
        public string text;
        public Sprite Portrait;
    }

    private List<TextBubbleInfo> Dialogue;

    public int currentTextBubble;

    private InputManager InputManager;

    private ActionManager ActionManager;

    public bool indialogue;

    public Image characterportrait;
    public TextMeshProUGUI charactername;
    public TextMeshProUGUI sentence;

    private string texttodisplay;

    private bool isPrinting;
    private int charIndex;

    public float charTimer;

    public float charDelay;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InputManager = FindAnyObjectByType<InputManager>();
        ActionManager = FindAnyObjectByType<ActionManager>();
        //DialogueExample();
    }

    private void DialogueExample()
    {
        TextBubbleInfo ex1 = new TextBubbleInfo();
        ex1.text = "I'm ready to roll !";
        ex1.name = "Genny";
        TextBubbleInfo ex2 = new TextBubbleInfo();
        ex2.text = "I'M READY TO ROLL";
        ex2.name = "Genny";
        InitializeDialogue(new List<TextBubbleInfo> { ex1, ex2 });
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(Dialogue==null)
        {
            DeactivateBubble();
        }
        else if (Dialogue.Count <= currentTextBubble)
        {
            DeactivateBubble();
        }
        else
        {
            if(ActionManager!=null)
            {
                ActionManager.frameswherenotlock = 5;
            }
            else
            {
                ActionManager = FindAnyObjectByType<ActionManager>();
            }

            indialogue = true;

            FindAnyObjectByType<GridScript>().lockselection = false;

            if (isPrinting && charIndex < texttodisplay.Length)
            {
                charTimer += Time.fixedDeltaTime;
                if (charTimer >= charDelay)
                {
                    charTimer = 0f;
                    charIndex++;
                    sentence.text = texttodisplay.Substring(0, charIndex);
                }
            }
            else if(isPrinting)
            {
                isPrinting = false;
            }


            if (InputManager.activatejustpressed || InputManager.canceljustpressed)
            {
                if (charIndex < texttodisplay.Length - 1)
                {
                    charIndex = texttodisplay.Length - 1;
                }
                else
                {
                    GoToNextPage();
                }

            }
        }
    }

    private void GoToNextPage()
    {
        currentTextBubble++;
        if(currentTextBubble < Dialogue.Count)
        {
            ActivateBubble();
            if(Dialogue[currentTextBubble].Portrait!=null)
            {
                characterportrait.sprite = Dialogue[currentTextBubble].Portrait;
            }
            texttodisplay = Dialogue[currentTextBubble].text;
            charactername.text = Dialogue[currentTextBubble].name;
            charIndex = 0;
            isPrinting = true;
        }
        else
        {
            DeactivateBubble();
        }
            


    }

    public void InitializeDialogue(List<TextBubbleInfo> dialogue)
    {
        currentTextBubble = -1;
        Dialogue = dialogue;
        GoToNextPage();
        indialogue = true;
    }

    private void ActivateBubble()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    private void DeactivateBubble()
    {
        indialogue = false;
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
        currentTextBubble = 0;
        Dialogue = null;
    }
}
