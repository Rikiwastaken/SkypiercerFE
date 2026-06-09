using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using static TextBubbleScript;
using static UnitScript;

public class CutsceneManager : MonoBehaviour
{

    private TextBubbleScript _textBubbleScript;



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

        public int DialogueIDToplay;
        public int AnimsequenceToplay;
        [Header("Environment")]
        public GameObject EnvironmentPrefab;
        public Vector3 EnvironmentPosition;
        public Vector3 EnvironmentRotation;
        public Vector3 EnvironmentScale;
        public List<LightVariables> LightData;
        public List<CameraVariables> CameraData;

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

    public PlayableDirector director;

    [Header("Global Parameters")]
    public int CurrentCutscene;
    public List<Light> lights;
    public GameObject CharacterPrefab;
    public RuntimeAnimatorController AnimatorController;
    public List<Transform> Cameras;


    [Header("Scene Parameters")]
    public List<CutScene> Cutscenes;

    private List<GameObject> SpawnedCharacters;



    private DataScript DataScript;



    public bool cutscenefinished;

    private GameObject CurrentEnvironment;

    [Header("Debug Tools")]
    public int CurrentCutsceneToDebug;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _textBubbleScript = TextBubbleScript.Instance;
        DataScript = DataScript.instance;

        InitializeCutscene(0);
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
    public void InitializeCutscene(int cutscenetoload)
    {
        CurrentCutscene = cutscenetoload;
        director.playableAsset = Cutscenes[CurrentCutscene].Timeline;

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
        CurrentCutscene = CurrentCutsceneToDebug;
        CleanSpawnCharacters();
        CreateCharacters();
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
