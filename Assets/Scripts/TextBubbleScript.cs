using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnitScript;

public class TextBubbleScript : MonoBehaviour
{

    public static TextBubbleScript Instance;

    [Serializable]
    public class TextBubbleInfo
    {
        public string Charactername;
        public string text;
        public Vector3 CameraDestination;
        public Sprite Portrait;
        public int characterindex = -1;
        public Sprite ImageToShow;
    }

    private List<TextBubbleInfo> Dialogue;

    public int currentTextBubble;

    private InputManager InputManager;


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

    private cameraScript cameraScript;

    public Image Imagetoshow;

    void Awake()
    {
        if (charactername != null)
            charactername.ForceMeshUpdate();

        if (sentence != null)
            sentence.ForceMeshUpdate();
        if(Instance==null)
        {
            Instance = this;
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InputManager = InputManager.instance;
        gridScript = GridScript.instance;
        cameraScript = FindAnyObjectByType<cameraScript>();
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
        if(Dialogue == null && !indialogue)
        {
            return;
        }
        if (Dialogue == null && indialogue)
        {
            DeactivateBubble();
            if(MapEventManager.instance!=null)
            {
                MapEventManager.instance.TriggerEventCheck();
            }
            
        }
        else if (Dialogue.Count <= currentTextBubble)
        {

            DeactivateBubble();
            if (MapEventManager.instance != null)
            {
                MapEventManager.instance.TriggerEventCheck();
            }
        }
        else
        {
            if(ActionManager.instance!=null)
            {
                ActionManager.instance.frameswherenotlock = 5;
            }
            

            indialogue = true;

            if(gridScript!=null)
            {
                gridScript.lockselection = false;
            }
            
            if(cameraScript!=null)
            {
                if (Dialogue[currentTextBubble].CameraDestination != Vector3.zero)
                {
                    cameraScript.Destination = new Vector2(Dialogue[currentTextBubble].CameraDestination.x, Dialogue[currentTextBubble].CameraDestination.z);
                }
                else if (GetCharacterCoordinates() != Vector2.zero)
                {
                    cameraScript.Destination = GetCharacterCoordinates();
                }
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
            else if (isPrinting)
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

        if (currentTextBubble < Dialogue.Count)
        {
            ActivateBubble();

            


            var info = Dialogue[currentTextBubble];


            if(info.ImageToShow != null)
            {
                Imagetoshow.color = Color.white;
                Imagetoshow.sprite = info.ImageToShow;
            }
            else if(Imagetoshow.color != Color.clear)
            {
                Imagetoshow.color = Color.clear;
            }

            Character character = GetCharacter();

            if (character!=null)
            {
                charactername.text = character.name;
                characterportrait.sprite = DataScript.instance.DialogueSpriteList[character.ID];
            }
            else
            {
                if (info.Portrait != null)
                {
                    characterportrait.sprite = info.Portrait;
                }
                charactername.text = info.Charactername;
            }
                

            texttodisplay = info.text;
            

            sentence.textWrappingMode = TextWrappingModes.NoWrap; // Temporarily disable wrapping
            sentence.text = texttodisplay;
            sentence.ForceMeshUpdate(); // Let TMP calculate layout
            sentence.textWrappingMode = TextWrappingModes.Normal;

            sentence.maxVisibleCharacters = 0;

            charIndex = 0;
            charTimer = 0f;
            isPrinting = true;
        }
        else
        {
            DeactivateBubble();
            if(MapEventManager.instance!=null)
            {
                MapEventManager.instance.TriggerEventCheck();
            }
        }
    }

    public void InitializeDialogue(List<TextBubbleInfo> dialogue)
    {
        currentTextBubble = -1;
        Dialogue = dialogue;
        indialogue = true;
        GoToNextPage();
        if(gridScript != null)
        {
            gridScript.ShowMovement();
        }
        
    }

    private Character GetCharacter()
    {
        if(indialogue)
        {
            if (currentTextBubble < Dialogue.Count)
            {
                var info = Dialogue[currentTextBubble];
                if (info.characterindex >= 0)
                {
                    foreach (Character character in DataScript.instance.PlayableCharacterList)
                    {
                        if (character.ID == info.characterindex)
                        {
                            return character;
                        }
                    }
                }
            }
        }
        return null;
        
    }

    private Vector2 GetCharacterCoordinates()
    {
        Character character = GetCharacter();
        if(character != null)
        {
            foreach (Character otherchar in gridScript.allunits)
            {
                if (otherchar.ID == character.ID)
                {
                    return otherchar.currentTile[0].GridCoordinates;
                }
            }
        }
        
        return Vector2.zero;
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
