using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using static TextBubbleScript;

public class CutsceneManager : MonoBehaviour
{

    private TextBubbleScript _textBubbleScript;

    public PlayableDirector director;

    private int DialogueIDToplay;

    [Serializable]
    public class DialogueList
    {
        public List<TextBubbleInfo> Dialogue;
    }

    public List<DialogueList> DialogueBubblesList;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _textBubbleScript = TextBubbleScript.Instance;
        director.Play();
    }

    // Update is called once per frame
    void Update()
    {

        if (_textBubbleScript.indialogue && director.state != PlayState.Paused)
        {

            director.Pause();
        }
        if (!_textBubbleScript.indialogue && director.state == PlayState.Paused)
        {
            director.Play();
        }
    }

    public void PlayDialogue()
    {
        _textBubbleScript.InitializeDialogue(DialogueBubblesList[DialogueIDToplay].Dialogue);
        DialogueIDToplay++;
    }
}
