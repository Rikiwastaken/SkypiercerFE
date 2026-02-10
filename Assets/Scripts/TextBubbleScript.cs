using System;
using System.Collections;
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
        public int musictoplay = -1;
        public float voicepitch = 0f;
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
    private float currentcharacterpitch;
    private MusicManager musicManager;

    private Coroutine fixTMPRoutine;

    void Awake()
    {
        if (charactername != null)
            charactername.ForceMeshUpdate();

        if (sentence != null)
            sentence.ForceMeshUpdate();
        if (Instance == null)
        {
            Instance = this;
        }
    }


    private bool ignoreInputThisFrame;

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
    void Update()
    {
        if (ignoreInputThisFrame)
        {
            ignoreInputThisFrame = false;
            return;
        }

        if (Dialogue == null)
            return;

        if (currentTextBubble >= Dialogue.Count)
        {
            EndDialogue();
            return;
        }
        if (musicManager == null)
            musicManager = MusicManager.instance;

        indialogue = true;

        //if (isPrinting && charIndex < texttodisplay.Length)
        int totalChars = sentence.textInfo.characterCount;

        if (isPrinting && charIndex < totalChars)
        {
            charTimer += Time.deltaTime;


            if (charTimer >= charDelay)
            {
                charTimer = 0f;
                charIndex = Mathf.Min(charIndex + 1, totalChars);
                sentence.maxVisibleCharacters = charIndex;
                musicManager.PlayVoiceSE(currentcharacterpitch);
            }
        }

        if (InputManager.activatejustpressed || InputManager.canceljustpressed)
        {
            if (charIndex < totalChars)
            {
                if (charIndex > totalChars * 0.05f)
                {
                    charIndex = totalChars;
                    sentence.maxVisibleCharacters = charIndex;
                }
            }
            else
            {
                GoToNextPage();
            }
        }

        if (InputManager.Startjustpressed)
        {
            EndDialogue();
        }
    }

    private void FixTMPVisibility()
    {
        if (fixTMPRoutine != null)
            StopCoroutine(fixTMPRoutine);

        fixTMPRoutine = StartCoroutine(FixTMPNextFrame());
    }

    private IEnumerator FixTMPNextFrame()
    {
        yield return null; // wait 1 frame

        sentence.ForceMeshUpdate();
        sentence.maxVisibleCharacters = charIndex;
    }

    private void GoToNextPage()
    {
        currentTextBubble++;

        if (currentTextBubble < Dialogue.Count)
        {
            ActivateBubble();




            var info = Dialogue[currentTextBubble];


            if (info.ImageToShow != null)
            {
                Imagetoshow.color = Color.white;
                Imagetoshow.sprite = info.ImageToShow;
            }
            else if (Imagetoshow.color != Color.clear)
            {
                Imagetoshow.color = Color.clear;
            }
            currentcharacterpitch = 0;
            Character character = GetCharacter();

            if (character != null)
            {
                charactername.text = character.name;
                characterportrait.sprite = DataScript.instance.DialogueSpriteList[character.ID];
                if (character.DialoguePitch != 0)
                {
                    currentcharacterpitch = character.DialoguePitch;
                }
            }
            else
            {
                if (info.Portrait != null)
                {
                    characterportrait.sprite = info.Portrait;
                }
                currentcharacterpitch = info.voicepitch;
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
            sentence.maxVisibleCharacters = 0;

            FixTMPVisibility();

            if (musicManager == null)
            {
                musicManager = MusicManager.instance;
            }
            musicManager.SetDialogueMusic(info.musictoplay);

        }
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        DeactivateBubble();
        gameObject.SetActive(false);
        if (MapEventManager.instance != null)
        {
            MapEventManager.instance.TriggerEventCheck();
        }
    }

    public void InitializeDialogue(List<TextBubbleInfo> dialogue)
    {
        gameObject.SetActive(true);
        ignoreInputThisFrame = true;
        currentTextBubble = -1;
        Dialogue = dialogue;
        indialogue = true;
        sentence.maxVisibleCharacters = 0;
        GoToNextPage();
        if (gridScript != null)
        {
            gridScript.ShowMovement();
        }

    }

    private Character GetCharacter()
    {
        if (indialogue)
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
        if (character != null)
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

        Dialogue = null;
        currentTextBubble = -1;
    }
}
