using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using static TextBubbleScript;
using static UnitScript;

public class CutsceneManager : MonoBehaviour
{

    private TextBubbleScript _textBubbleScript;

    public PlayableDirector director;

    public int CurrentCutscene;

    [Serializable]
    public class CutSceneCharacter
    {
        public int ID; // ID used to find get the character in the cutscene
        public int characterID = -1; // playable character ID used to fill infos;
        public GameObject CharacterGO; // Gameobject assigned to character
        public GameObject modelToUse; // use this to directly assign a model if characterID is null
        public Vector3 BasePosition; //start position
        public Vector3 BaseRotation; // start rotation
        public Animator Animator; // Animator to use for animations during cutscenes
        public UnitScript.EnemyStats EnemyStats; // to use if character is an enemy 
        public int startanimation = 1; //plays this animation with this ID at start
    }

    [Serializable]
    public class CutScene
    {
        public List<CutSceneCharacter> Characters; // Characters present in teh cutscene
        public List<DialogueList> DialogueBubblesList; // dialogues in the cutscenes
        public List<AnimSequence> animSequences; // character animations in the cutscene
        public int DialogueIDToplay;
        public int AnimsequenceToplay;
    }

    [Serializable]
    public class AnimSequence
    {
        public List<int> CharactersToAnimate;
        public List<int> AnimationIDs;
    }

    [Serializable]
    public class DialogueList
    {
        public List<TextBubbleInfo> Dialogue;
    }

    public List<CutScene> Cutscenes;

    private List<GameObject> SpawnedCharacters;

    public GameObject CharacterPrefab;

    private DataScript DataScript;

    public RuntimeAnimatorController AnimatorController;

    public bool cutscenefinished;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _textBubbleScript = TextBubbleScript.Instance;
        DataScript = DataScript.instance;

        InitializeCutscene();
        director.Play();

    }

    // Update is called once per frame
    void Update()
    {
        if (!cutscenefinished)
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

    }

    public void CutsceneFinished() // function called by a signal to indicate the cutscene is over
    {
        cutscenefinished = true;
    }

    public void PlayDialogue()// function called by a signal to play the next dialogue in the cue
    {
        _textBubbleScript.InitializeDialogue(Cutscenes[CurrentCutscene].DialogueBubblesList[Cutscenes[CurrentCutscene].DialogueIDToplay].Dialogue);
        Cutscenes[CurrentCutscene].DialogueIDToplay++;
    }

    public void PlayAnimationSequence() // function called by a signal to play the next group of animations in the cue
    {
        if (Cutscenes.Count < CurrentCutscene || Cutscenes[CurrentCutscene].animSequences.Count < Cutscenes[CurrentCutscene].AnimsequenceToplay)
        {
            return;
        }
        AnimSequence currentsequence = Cutscenes[CurrentCutscene].animSequences[Cutscenes[CurrentCutscene].AnimsequenceToplay];

        for (int i = 0; i < currentsequence.CharactersToAnimate.Count; i++)
        {
            if (currentsequence.AnimationIDs.Count > i)
            {
                PlayAnimation(currentsequence.CharactersToAnimate[i], currentsequence.AnimationIDs[i]);
            }
        }

        Cutscenes[CurrentCutscene].AnimsequenceToplay++;

    }
    public void InitializeCutscene()
    {
        CleanSpawnCharacters();
        CreateCharacters();


    }

    private void CreateCharacters()
    {
        DataScript = DataScript.instance;
        if (DataScript == null)
        {
            DataScript = GameObject.FindAnyObjectByType<DataScript>(FindObjectsInactive.Include);
        }
        int ID = 0;
        // create GameObject for each character and assign it their materials and positions
        foreach (CutSceneCharacter CurrentCharacter in Cutscenes[CurrentCutscene].Characters)
        {
            Debug.Log("instantiating character with ID: " + CurrentCharacter.characterID);
            GameObject newcharacter = Instantiate(CharacterPrefab, transform);
            if (CurrentCharacter.characterID != -1 && DataScript != null)
            {

                newcharacter.GetComponent<UnitScript>().UnitCharacteristics = newcharacter.GetComponent<UnitScript>().CreateCopy(DataScript.PlayableCharacterList[CurrentCharacter.characterID]);
            }
            else if (CurrentCharacter.EnemyStats != null)
            {
                Character Character = newcharacter.GetComponent<UnitScript>().UnitCharacteristics;
                // in the cae of pluvials, randomly take a model.
                if (CurrentCharacter.EnemyStats.monsterStats.ispluvial)
                {
                    Character.modelID = UnityEngine.Random.Range(3, newcharacter.GetComponent<UnitScript>().ModelList.Count);
                }
                else
                {
                    Character.modelID = CurrentCharacter.EnemyStats.modelID;
                }
                Character.enemyStats = CurrentCharacter.EnemyStats;
                Character.name = CurrentCharacter.EnemyStats.Name;
                Character.equipmentsIDs = CurrentCharacter.EnemyStats.equipments;
            }
            newcharacter.name = newcharacter.GetComponent<UnitScript>().UnitCharacteristics.name;
            newcharacter.GetComponent<UnitScript>().InstantiateCharacterModel();
            newcharacter.GetComponent<UnitScript>().enabled = false;
            CurrentCharacter.CharacterGO = newcharacter;
            CurrentCharacter.Animator = newcharacter.GetComponentInChildren<Animator>();
            CurrentCharacter.ID = ID;

            newcharacter.transform.position = Vector3.zero;
            Vector3 scale = CurrentCharacter.Animator.transform.lossyScale;
            newcharacter.transform.localScale = Vector3.one;
            CurrentCharacter.Animator.transform.localScale = scale;
            CurrentCharacter.Animator.transform.position = CurrentCharacter.BasePosition;
            CurrentCharacter.Animator.transform.rotation = Quaternion.Euler(CurrentCharacter.BaseRotation);
            CurrentCharacter.Animator.runtimeAnimatorController = AnimatorController;
            PlayAnimation(ID, CurrentCharacter.startanimation);


            // deactivate useless objects
            newcharacter.GetComponentInChildren<Canvas>().gameObject.SetActive(false);
            ID++;
        }
    }

    private void PlayAnimation(int characterID, int AnimationID)
    {
        GetCharacterFromID(characterID).Animator.SetInteger("AnimationToPlay", AnimationID);
    }

    private CutSceneCharacter GetCharacterFromID(int ID)
    {
        if (Cutscenes.Count > CurrentCutscene && Cutscenes[CurrentCutscene].Characters.Count > ID)
        {
            return Cutscenes[CurrentCutscene].Characters[ID];
        }
        return null;
    }

    private void CleanSpawnCharacters() // delete characters from previouscutscenes
    {
        if (SpawnedCharacters != null && SpawnedCharacters.Count > 0)
        {
            int numberofcharacters = SpawnedCharacters.Count;
            for (int i = 0; i < numberofcharacters; i++)
            {
                GameObject character = SpawnedCharacters[0];
                SpawnedCharacters.Remove(character);
                DestroyImmediate(character);
            }
            SpawnedCharacters = new List<GameObject>();
        }


        //Ensure all are destroyed by also destroying the children of the transform


        while (transform.childCount > 0)
        {
            GameObject child = transform.GetChild(0).gameObject;
            child.transform.parent = null;
            DestroyImmediate(child);
        }

    }

#if UNITY_EDITOR

    [ContextMenu("Create CutSceneCharacters")]
    public void CreateCharactersMenuOption()
    {
        CleanSpawnCharacters();
        CreateCharacters();
    }

#endif
}
