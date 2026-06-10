using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using static TextBubbleScript;
using static UnitScript;

public class CutsceneManager : MonoBehaviour
{

    private TextBubbleScript _textBubbleScript;
    private SceneLoader _sceneLoader;
    private CombatSceneLoader _combbatSceneLoader;


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
        public TimelineAsset Timeline;
        public List<CutSceneCharacter> Characters; // Characters present in teh cutscene
        public List<DialogueList> DialogueBubblesList; // dialogues in the cutscenes
        public List<AnimSequence> animSequences; // character animations in the cutscene
        public List<MovementSequence> MovementSequences;
        public int DialogueIDToplay;
        public int AnimsequenceToplay;
        public int MovementSequenceToPlay;
        [Header("Environment")]
        public GameObject EnvironmentPrefab;
        public Vector3 EnvironmentPosition;
        public Vector3 EnvironmentRotation;
        public Vector3 EnvironmentScale;
        public List<LightVariables> LightData;
        public List<CameraVariables> CameraData;
        public int CutSceneToPlayAfterThisOne = -1; // load this cutscene after the cutscene is over
        public string SceneToLoad; // If CutSceneToPlayAfterThisOne is -1, will load this scene instead.

    }

    [Serializable]
    public class LightVariables
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public float intensity;
        public Color LightColor;
        public LightType LightType;
    }

    [Serializable]
    public class CameraVariables
    {
        public Vector3 Position;
        public Vector3 Rotation;
    }

    [Serializable]
    public class MovementSequence
    {
        public bool look; //if true, the character will look at the "Movement" Vector added to its position instead 
        public List<int> CharactersToMove;
        public List<Vector3> Movement;
        public List<float> TimeToMove;
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
        public int characterTalkingID;
        public int talkinganimationID = 3;
        public List<TextBubbleInfo> Dialogue;
    }

    public PlayableDirector director;

    [Header("Global Parameters")]
    public int CurrentCutscene;
    public List<Light> lights;
    public GameObject CharacterPrefab;
    public RuntimeAnimatorController AnimatorController;
    public List<Transform> Cameras;
    public UnityEngine.UI.Image FadeToBlackImage;
    public float BaseFadeImageX;
    public float FadeTime;
    private Coroutine FadeToBlacKCoroutine;


    [Header("Scene Parameters")]
    public List<CutScene> Cutscenes;

    private List<GameObject> SpawnedCharacters;

    private DataScript DataScript;



    public bool cutscenefinished;

    private GameObject CurrentEnvironment;
    public List<float> Movementhappenning = new List<float>();

    [Header("Debug Tools")]
    public int CurrentCutsceneToDebug;

    private bool paused;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _textBubbleScript = TextBubbleScript.Instance;
        DataScript = DataScript.instance;
        if (DataScript == null)
        {
            SceneManager.LoadScene("FirstScene");
        }
        _sceneLoader = SceneLoader.instance;
        _combbatSceneLoader = DataScript.GetComponent<CombatSceneLoader>();

        if (DataScript == null)
        {
            SceneManager.LoadScene("FirstScene");
        }

        if (_sceneLoader.Cutscenetoplay != -1)
        {
            CurrentCutscene = _sceneLoader.Cutscenetoplay;
            _sceneLoader.Cutscenetoplay = -1;
        }


        if (CurrentCutscene != -1)
        {
            PlayCutsceneWithFade(CurrentCutscene);
        }


    }

    // Update is called once per frame
    void Update()
    {
        if (FadeToBlacKCoroutine != null)
        {
            return;
        }
        if (!cutscenefinished)
        {
            if (_textBubbleScript.indialogue && director.state != PlayState.Paused)
            {
                paused = true;
                director.Pause();
            }
            if (Movementhappenning.Count > 0 && director.state != PlayState.Paused)
            {
                paused = true;
                director.Pause();
            }
            if (!_textBubbleScript.indialogue && director.state == PlayState.Paused && Movementhappenning.Count == 0 && paused)
            {
                paused = false;
                director.Play();
            }
            if (director.time >= director.duration)
            {
                cutscenefinished = true;
            }
            UpdateMovementTimeList();
        }
        else
        {
            LoadNextEvent(Cutscenes[CurrentCutscene]);
        }

    }


    private void LoadNextEvent(CutScene _CutScene) // load either another dialogue or a scene
    {

        CleanEnvironment();
        CleanSpawnCharacters();

        CurrentCutscene = -1;
        if (_CutScene.CutSceneToPlayAfterThisOne != -1)
        {
            StartCoroutine(DoubleFadeToLoadNewCutscene(_CutScene.CutSceneToPlayAfterThisOne));
        }
        else if (_CutScene.SceneToLoad != "")
        {
            LoadChapterAfterCutscene(_CutScene.SceneToLoad);
        }
        else
        {
            _combbatSceneLoader.ActivateMainSceneFromCutsceneScene();
        }
    }

    public void PlayDialogue()// function called by a signal to play the next dialogue in the cue
    {
        DialogueList currentdialoguelist = Cutscenes[CurrentCutscene].DialogueBubblesList[Cutscenes[CurrentCutscene].DialogueIDToplay];
        int sceneCharacterID = currentdialoguelist.characterTalkingID;
        int animation = currentdialoguelist.talkinganimationID;
        //PlayAnimation(sceneCharacterID, animation);
        // kinda doesn't work that well
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

    public void PlayMovementSequence() // function called by a signal to play the next group of movement in the cue
    {
        if (Cutscenes.Count < CurrentCutscene || Cutscenes[CurrentCutscene].animSequences.Count < Cutscenes[CurrentCutscene].AnimsequenceToplay)
        {
            return;
        }
        MovementSequence currentsequence = Cutscenes[CurrentCutscene].MovementSequences[Cutscenes[CurrentCutscene].MovementSequenceToPlay];

        for (int i = 0; i < currentsequence.CharactersToMove.Count; i++)
        {
            if (currentsequence.Movement.Count > i)
            {
                PlayMovement(currentsequence.CharactersToMove[i], currentsequence.Movement[i], currentsequence.TimeToMove[i], currentsequence.look);
            }
        }

        Cutscenes[CurrentCutscene].MovementSequenceToPlay++;

    }
    public void InitializeCutscene(int cutscenetoload)
    {
        cutscenefinished = false;
        paused = false;
        CurrentCutscene = cutscenetoload;
        director.playableAsset = Cutscenes[CurrentCutscene].Timeline;

        Setup();
    }

    private void UpdateMovementTimeList()
    {
        List<float> newtimelist = new List<float>();
        foreach (float time in Movementhappenning)
        {
            if (Time.time < time)
            {
                newtimelist.Add(time);
            }
        }
        Movementhappenning = newtimelist;
    }

    private void Setup()
    {
        CleanSpawnCharacters();
        CreateCharacters();
        CleanEnvironment();
        CreateEnvironment();
        SetupLights();
        SetupCameras();
    }

    private void SetupLights()
    {
        int lastLightIDActivated = 0;
        foreach (LightVariables LightData in Cutscenes[CurrentCutscene].LightData)
        {
            lights[lastLightIDActivated].gameObject.SetActive(true);

            Light light = lights[lastLightIDActivated].GetComponent<Light>();
            light.type = LightData.LightType;
            light.intensity = LightData.intensity;
            light.color = LightData.LightColor;
            light.transform.position = LightData.Position;
            light.transform.rotation = Quaternion.Euler(LightData.Rotation);

            lastLightIDActivated++;
        }
        for (int i = lastLightIDActivated; i < lights.Count; i++)
        {
            lights[i].gameObject.SetActive(false);
        }
    }

    private void SetupCameras()
    {
        int lastID = 0;
        foreach (CameraVariables CamData in Cutscenes[CurrentCutscene].CameraData)
        {

            Transform cam = Cameras[lastID];

            cam.position = CamData.Position;
            cam.rotation = Quaternion.Euler(CamData.Rotation);

            lastID++;
        }
    }

    private void CleanEnvironment()
    {
        if (CurrentEnvironment != null)
        {
            Destroy(CurrentEnvironment);
        }
        CurrentEnvironment = null;
    }

    private void CreateEnvironment()
    {
        CurrentEnvironment = Instantiate(Cutscenes[CurrentCutscene].EnvironmentPrefab);
        CurrentEnvironment.transform.position = Cutscenes[CurrentCutscene].EnvironmentPosition;
        CurrentEnvironment.transform.rotation = Quaternion.Euler(Cutscenes[CurrentCutscene].EnvironmentRotation);
        CurrentEnvironment.transform.localScale = Cutscenes[CurrentCutscene].EnvironmentScale;
        CurrentEnvironment.transform.parent = transform;
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

    private IEnumerator FadeCoroutine(bool ToBlack, int Cutscenetostart = -1, string chapterToLoad = "")
    {
        if (ToBlack)
        {
            FadeToBlackImage.rectTransform.localPosition = new Vector2(-BaseFadeImageX, 0);

            float timeelapsed = 0f;
            while (timeelapsed < FadeTime)
            {
                timeelapsed += Time.deltaTime;
                float ratio = timeelapsed / FadeTime;
                FadeToBlackImage.rectTransform.localPosition = Vector2.Lerp(new Vector2(-BaseFadeImageX, 0), Vector2.zero, ratio);
                yield return null;
            }
            if (chapterToLoad != "")
            {
                _sceneLoader.LoadScene(chapterToLoad);
            }

        }
        else
        {
            FadeToBlackImage.rectTransform.localPosition = new Vector2(0, 0);
            bool firstframeplayed = false;
            if (Cutscenetostart != -1)
            {
                InitializeCutscene(Cutscenetostart);
                director.Play();
            }

            float timeelapsed = 0f;
            while (timeelapsed < FadeTime)
            {
                if (!firstframeplayed && timeelapsed != 0f)
                {
                    firstframeplayed = true;
                    director.Pause();
                }

                timeelapsed += Time.deltaTime;
                float ratio = timeelapsed / FadeTime;
                FadeToBlackImage.rectTransform.localPosition = Vector2.Lerp(Vector2.zero, new Vector2(BaseFadeImageX, 0), ratio);
                yield return null;
            }
            director.Play();
        }
        FadeToBlacKCoroutine = null;
    }

    private IEnumerator DoubleFadeToLoadNewCutscene(int CutsceneID)
    {
        FadeToBlacKCoroutine = StartCoroutine(FadeCoroutine(true));
        while (FadeToBlacKCoroutine != null)
        {
            yield return true;
        }
        FadeToBlacKCoroutine = StartCoroutine(FadeCoroutine(false, CutsceneID));
    }

    public void PlayCutsceneWithFade(int CutsceneID)
    {

        FadeToBlacKCoroutine = StartCoroutine(FadeCoroutine(false, CutsceneID));
    }

    private void LoadChapterAfterCutscene(string chapter)
    {
        FadeToBlacKCoroutine = StartCoroutine(FadeCoroutine(true, -1, chapter));
    }

    private void PlayAnimation(int characterID, int AnimationID)
    {
        GetCharacterFromID(characterID).Animator.SetInteger("AnimationToPlay", AnimationID);
        switch (AnimationID)
        {
            case 3:
                GetCharacterFromID(characterID).Animator.SetTrigger("Talking");
                break;
            case 4:
                GetCharacterFromID(characterID).Animator.SetTrigger("Talking");
                break;
            case 5:
                GetCharacterFromID(characterID).Animator.SetTrigger("Talking");
                break;
        }
    }
    private void PlayMovement(int characterID, Vector3 Movement, float TimeToMove, bool islooking)
    {
        StartCoroutine(MovementCoroutine(characterID, Movement, TimeToMove, islooking));
        Movementhappenning.Add(Time.time + TimeToMove);
    }

    private IEnumerator MovementCoroutine(int characterID, Vector3 Movement, float TimeToMove, bool islooking)
    {
        CutSceneCharacter Character = GetCharacterFromID(characterID);
        Vector3 basepos = Character.Animator.transform.position;
        Vector3 direction = Character.Animator.transform.position + Movement - Character.Animator.transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        Quaternion baserotation = Character.Animator.transform.rotation;
        float timeelapsed = 0f;
        while (timeelapsed < TimeToMove)
        {
            timeelapsed += Time.deltaTime;
            float ratio = timeelapsed / TimeToMove;
            if (islooking)
            {
                Character.Animator.transform.rotation = Quaternion.Lerp(baserotation, targetRotation, ratio);
            }
            else
            {
                Character.Animator.transform.position = Vector3.Lerp(basepos, basepos + Movement, ratio);
                Character.Animator.transform.LookAt(Character.Animator.transform.position + Movement);
            }

            yield return null;
        }

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

    [ContextMenu("Create CutScene Characters and map")]
    public void CreateCharactersMenuOption()
    {
        CurrentCutscene = CurrentCutsceneToDebug;
        Setup();
    }

    [ContextMenu("Destroy CutSceneCharacters")]
    public void DestroyCharactersMenuOption()
    {
        CleanSpawnCharacters();
        UnityEditor.EditorUtility.SetDirty(this);
    }

    [ContextMenu("Load Dialogue From JSON")]
    public void LoadBonds()
    {
        string path = UnityEditor.EditorUtility.OpenFilePanel("Select Bond JSON File", "", "json");
        if (string.IsNullOrEmpty(path))
            return;

        string json = File.ReadAllText(path);

        MapEventManager.Dialoguewrapper wrapper = JsonUtility.FromJson<MapEventManager.Dialoguewrapper>(json);
        if (wrapper == null || wrapper.dialguesToLoad == null)
        {
            Debug.LogError("JSON file format invalid. Needs { \"dialguesToLoad\": [ ... ] }");
            return;
        }

        List<DialogueList> newdialogue = new List<DialogueList>();
        foreach (MapEventManager.DialgueToLoad dialgueToLoad in wrapper.dialguesToLoad)
        {

            TextBubbleInfo textBubbleInfo = new TextBubbleInfo() { text = dialgueToLoad.text, characterindex = dialgueToLoad.characterindex };


            newdialogue.Add(new DialogueList() { Dialogue = new List<TextBubbleInfo>() { textBubbleInfo } });
        }
        Cutscenes[CurrentCutsceneToDebug].DialogueBubblesList = newdialogue;
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log("Loaded " + wrapper.dialguesToLoad.Count + " dialogue parts into the Cutscenes number " + CurrentCutsceneToDebug + "!");

    }

#endif
}
