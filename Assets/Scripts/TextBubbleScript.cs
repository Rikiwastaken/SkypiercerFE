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
        public string Charactername;
        public string text;
        public Vector3 CameraDestination;
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

    private GridScript gridScript;

    private battlecameraScript battlecameraScript;

    void Awake()
    {
        if (charactername != null)
            charactername.ForceMeshUpdate();

        if (sentence != null)
            sentence.ForceMeshUpdate();

    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InputManager = FindAnyObjectByType<InputManager>();
        ActionManager = FindAnyObjectByType<ActionManager>();
        gridScript = FindAnyObjectByType<GridScript>();
        battlecameraScript = FindAnyObjectByType<battlecameraScript>();
        //DialogueExample();
    }

    private void DialogueExample()
    {
        TextBubbleInfo ex1 = new TextBubbleInfo();
        ex1.text = "I'm ready to roll !";
        ex1.Charactername = "Genny";
        TextBubbleInfo ex2 = new TextBubbleInfo();
        ex2.text = "I'M READY TO ROLL";
        ex2.Charactername = "Genny";
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

            if(Dialogue[currentTextBubble].CameraDestination != Vector3.zero)
            {
                battlecameraScript.Destination = new Vector2(Dialogue[currentTextBubble].CameraDestination.x, Dialogue[currentTextBubble].CameraDestination.z);
            }

            if (isPrinting && charIndex < texttodisplay.Length)
            {
                charTimer += Time.fixedDeltaTime;
                if (charTimer >= charDelay)
                {
                    charTimer = 0f;
                    charIndex++;
                    sentence.maxVisibleCharacters = charIndex;
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
            charactername.text = Dialogue[currentTextBubble].Charactername;
            charIndex = 0;
            isPrinting = true;
            sentence.textWrappingMode = TextWrappingModes.NoWrap; // Temporarily disable word wrapping
            sentence.text = texttodisplay;       // Set full text to calculate layout
            sentence.ForceMeshUpdate();
            sentence.textWrappingMode = TextWrappingModes.Normal;
        }
        else
        {
            DeactivateBubble();
            FindAnyObjectByType<MapEventManager>().TriggerEventCheck();
        }
            


    }

    public void InitializeDialogue(List<TextBubbleInfo> dialogue)
    {
        currentTextBubble = -1;
        Dialogue = dialogue;
        GoToNextPage();
        indialogue = true;
        gridScript.ShowMovement();
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
