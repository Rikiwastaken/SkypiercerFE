using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;


public class MusicManager : MonoBehaviour
{
    public AudioSource PlayableAudioSource;
    public AudioSource PlayableAudioSourceIntro;
    public AudioSource EnemyAudioSource;
    public AudioSource EnemyAudioSourceIntro;
    public AudioSource OtherAudioSource;
    public AudioSource OtherAudioSourceIntro;
    public AudioSource BeforeCombat;
    public AudioSource BeforeCombatintro;

    public AudioSource CampMusic;
    public AudioSource CampMusicintro;
    public AudioSource WorldMapMusic;
    public AudioSource WorldMapMusicintro;
    public AudioSource ShipMusic;
    public AudioSource ShipMusicintro;

    public AudioSource MainMenuMusic;
    public AudioSource MainMenuMusicintro;


    public AudioSource DialogueAudioSource;
    public AudioSource DialogueAudioSourceIntro;
    public AudioSource DialogueAudioSource2;
    public AudioSource DialogueAudioSource2Intro;

    private AudioSource currentDialogueAudioSource;
    private AudioSource currentDialogueAudioSourceIntro;

    public AudioSource CutSceneMusic;
    public AudioSource CutSceneMusicintro;

    public int CurrentDialogueMusic;

    private bool lowerdialogue;
    private bool lowermap;

    [Serializable]
    public class Audios
    {
        public AudioClip Intro;
        public AudioClip Music;
    }
    public List<Audios> DialogueMusicsWithIntro;

    public float maxvolume;

    public float SFXVolume;

    public AudioMixer mixer;

    private TurnManger TurnManager;

    private SaveManager SaveManager;

    private float beforecombatmusicvol;

    public static MusicManager instance;

    public GameObject GeneratedSoundHolder;

    public List<AudioClip> VoiceSFXList;

    [Serializable]
    public class MapBattleMusic
    {
        public AudioClip BattleMusic;
        public AudioClip BattleMusicIntro;
        public AudioClip MapMusic;
        public AudioClip MapMusicIntro;
        public AudioClip PrepMusic;
        public AudioClip PrepMusicIntro;
        public List<int> Chapters;
        public bool useforSideStory;
    }

    [Serializable]
    public class MapMusic
    {
        public int PlayableMusicID;
        public int EnemyMusicID;
        public int OtherrMusicID;
        public int PrepMusicID;
        public List<int> Chapters;
        public bool useforSideStory;
    }

    public List<MapBattleMusic> MusicList;

    public List<MapMusic> MusicPerMap;

    private bool PlayPrepMusic;

    private bool PrepFinished;

    private GameOverScript GameOverScript;

    public string currentscene;

    public bool inCombatBool;

    private TextBubbleScript textBubbleScript;

    private ActionsMenu actionsMenu;

    private int currentMusicType = -1;

    string previousFaction;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        SceneManager.activeSceneChanged += OnSceneLoad;
    }

    private void Start()
    {
        beforecombatmusicvol = BeforeCombat.volume;
        SaveManager = FindAnyObjectByType<SaveManager>();


        if (BeforeCombat.isPlaying)
        {
            BeforeCombat.volume = beforecombatmusicvol;
        }
    }

    private void Update()
    {

        if (actionsMenu == null)
        {
            actionsMenu = ActionsMenu.instance;
        }

        string currentscenename = SceneManager.GetActiveScene().name;
        if (currentscenename.Contains("SideStory") || currentscenename.Contains("Chapter") || currentscenename.Contains("Prologue") || currentscenename.Contains("TestMap"))
        {


            if (TurnManager == null)
            {
                TurnManager = FindAnyObjectByType<TurnManger>();
            }

            if (GameOverScript == null)
            {
                GameOverScript = FindAnyObjectByType<GameOverScript>(FindObjectsInactive.Include);
            }

            if (textBubbleScript == null)
            {
                textBubbleScript = FindAnyObjectByType<TextBubbleScript>(FindObjectsInactive.Include);
            }
        }


        if (SceneLoader.instance.LoadingImage.gameObject.activeSelf)
        {
            return;
        }
        if (PlayPrepMusic)
        {
            PlayPrepMusic = false;
            PlayMusic(4, maxvolume);
        }




        if (lowerdialogue && currentDialogueAudioSource != null)
        {
            currentDialogueAudioSource.volume -= Time.deltaTime;
        }


        if (BeforeCombat.isPlaying)
        {
            BeforeCombat.volume = beforecombatmusicvol;
        }

        if (TurnManager != null)
        {
            if (TurnManager.currentlyplaying != "" && !PlayableAudioSource.isPlaying)
            {
                PrepFinished = true;
            }
            else
            {
                PrepFinished = false;
            }
        }

        if (currentscene != "CutsceneScene")
        {
            ChangeVolume(CutSceneMusic, 0f);
            ChangeVolume(CutSceneMusicintro, 0f);

        }
        else
        {
            return;
        }

        if (PrepFinished && !PlayableAudioSource.isPlaying)
        {
            BeforeCombat.Stop();
            PlayMusic(2, maxvolume);
        }

        if (currentscene == "Camp")
        {
            if (!CampMusic.isPlaying && !CampMusicintro.isPlaying)
            {
                PlayMusic(1, 0f);
            }

            ChangeVolume(CampMusic, maxvolume);
            ChangeVolume(CampMusicintro, maxvolume);
            ChangeVolume(WorldMapMusic, 0f);
            ChangeVolume(WorldMapMusicintro, 0f);

        }
        else if (currentscene == "WorldMap")
        {
            if (!WorldMapMusic.isPlaying && !WorldMapMusicintro.isPlaying)
            {
                PlayMusic(6, 0f);
                PlayMusic(7, 0f);
            }

            ChangeVolume(CampMusic, 0f);
            ChangeVolume(CampMusicintro, 0f);
            if (worldmapController.instance.isshippingCounter > 0)
            {
                ChangeVolume(WorldMapMusic, 0f);
                ChangeVolume(WorldMapMusicintro, 0f);
                ChangeVolume(ShipMusic, maxvolume);
                ChangeVolume(ShipMusicintro, maxvolume);
            }
            else
            {
                ChangeVolume(ShipMusic, 0f);
                ChangeVolume(ShipMusicintro, 0f);
                ChangeVolume(WorldMapMusic, maxvolume);
                ChangeVolume(WorldMapMusicintro, maxvolume);
            }

        }
        else
        {
            ChangeVolume(CampMusic, 0f);
            ChangeVolume(CampMusicintro, 0f);
            if (TurnManager != null)
            {
                ManageMusicTurnRotation();
            }

        }



        if (textBubbleScript != null && textBubbleScript.indialogue)
        {
            if (lowermap)
            {
                ChangeVolume(PlayableAudioSource, 0f);
                ChangeVolume(PlayableAudioSourceIntro, 0f);

                ChangeVolume(EnemyAudioSource, 0f);
                ChangeVolume(EnemyAudioSourceIntro, 0f);

                ChangeVolume(OtherAudioSource, 0f);
                ChangeVolume(OtherAudioSourceIntro, 0f);
            }
            if (currentDialogueAudioSource != null && (currentDialogueAudioSource.isPlaying || currentDialogueAudioSourceIntro.isPlaying) && currentDialogueAudioSource.volume > 0 && CurrentDialogueMusic != -1)
            {
                ChangeVolume(CampMusic, 0f);
                ChangeVolume(CampMusicintro, 0f);

                if (BeforeCombat.volume > 0)
                {
                    BeforeCombat.volume -= Time.fixedDeltaTime * 2;
                }

                if (currentDialogueAudioSource == DialogueAudioSource)
                {
                    ChangeVolume(DialogueAudioSource2, 0f);

                }
                else
                {
                    ChangeVolume(DialogueAudioSource, 0f);

                }


                ChangeVolume(currentDialogueAudioSource, maxvolume);
            }
            else
            {
                if (currentDialogueAudioSource != null && currentDialogueAudioSource.volume > 0)
                {
                    ChangeVolume(currentDialogueAudioSource, 0f);
                }
            }
        }

    }


    public void ChangeVolume()
    {
        mixer.SetFloat("MusicVol", Mathf.Log10(SaveManager.Options.musicvolume) * 20f);
        mixer.SetFloat("SEVol", Mathf.Log10(SaveManager.Options.SEVolume) * 20f);
    }

    void OnSceneLoad(Scene activescene, Scene nextscene)
    {
        if (nextscene.name == "BattleScene")
        {
            return;
        }
        if (currentDialogueAudioSource != null)
        {
            currentDialogueAudioSource.volume = 0f;
            currentDialogueAudioSourceIntro.volume = 0f;
        }
        if (nextscene.name == "Camp")
        {
            PlayMusic(1);
        }
        else if (nextscene.name == "WorldMap")
        {
            PlayMusic(6);
        }
        else if (nextscene.name == "MainMenu")
        {
            PlayMusic(8, maxvolume);
        }


        currentscene = nextscene.name;
        if (nextscene.name == "CutsceneScene")
        {
            return;
        }
        InitializeMusics(currentscene);
    }

    public void InitializeMusics(string ChapterToLoad)
    {
        bool isSideStory = false;
        int Chapter = -1;
        if (ChapterToLoad.Contains("Chapter"))
        {
            ChapterToLoad = ChapterToLoad.Replace("Chapter", "");
            Chapter = int.Parse(ChapterToLoad);
        }
        if (ChapterToLoad.Contains("SideStory"))
        {
            ChapterToLoad = ChapterToLoad.Replace("SideStory", "");
            Chapter = int.Parse(ChapterToLoad);
            isSideStory = true;
        }
        if (ChapterToLoad.Contains("Prologue") || ChapterToLoad.Contains("TestMap"))
        {
            Chapter = 0;
        }
        BeforeCombat.Stop();
        CampMusic.Stop();
        WorldMapMusic.Stop();
        ShipMusic.Stop();
        CutSceneMusic.Stop();

        if (Chapter != -1)
        {

            foreach (MapMusic MusicClass in MusicPerMap)
            {
                if (MusicClass.Chapters.Contains(Chapter) && MusicClass.useforSideStory == isSideStory)
                {
                    PlayableAudioSource.clip = DialogueMusicsWithIntro[MusicClass.PlayableMusicID].Music;
                    PlayableAudioSourceIntro.clip = DialogueMusicsWithIntro[MusicClass.PlayableMusicID].Intro;
                    EnemyAudioSource.clip = DialogueMusicsWithIntro[MusicClass.EnemyMusicID].Music;
                    EnemyAudioSourceIntro.clip = DialogueMusicsWithIntro[MusicClass.EnemyMusicID].Intro;
                    OtherAudioSource.clip = DialogueMusicsWithIntro[MusicClass.OtherrMusicID].Music;
                    OtherAudioSourceIntro.clip = DialogueMusicsWithIntro[MusicClass.OtherrMusicID].Intro;
                    BeforeCombat.clip = DialogueMusicsWithIntro[MusicClass.PrepMusicID].Music;
                    BeforeCombatintro.clip = DialogueMusicsWithIntro[MusicClass.PrepMusicID].Intro;
                    break;
                }
            }
            PlayPrepMusic = true;
        }
    }

    void PlayMusic(int type, float startvolume = 0f)
    {
        if (currentMusicType == type)
        {
            if (type != 9)
            {
                return;
            }

        }


        StopAllMusic();
        currentMusicType = type;

        if (type == 2 || type == 3)
        {
            PlayMusicWithIntro(2, startvolume);
            PlayMusicWithIntro(3, startvolume);
        }
        else
        {
            PlayMusicWithIntro(type, startvolume);
        }


    }
    private void PlayMusicWithIntro(int TypeID, float startvolume)
    {

        AudioSource Main = null;
        AudioSource intro = null;

        switch (TypeID)
        {
            case (0): //rien

                break;
            case (1): //Camp
                Main = CampMusic;
                intro = CampMusicintro;
                lowerdialogue = true;
                lowermap = true;
                break;
            case (2): //PlayableTurn
                Main = PlayableAudioSource;
                intro = PlayableAudioSourceIntro;
                lowerdialogue = true;
                lowermap = false;
                break;
            case (3): //EnemyTurn
                Main = EnemyAudioSource;
                intro = EnemyAudioSourceIntro;
                lowerdialogue = true;
                lowermap = false;
                break;
            case (4): //BeforeComabt
                Main = BeforeCombat;
                intro = BeforeCombatintro;
                lowerdialogue = true;
                lowermap = true;
                break;
            case (5): //DialogueAudio
                lowerdialogue = false;
                lowermap = true;
                Main = currentDialogueAudioSource;
                intro = currentDialogueAudioSourceIntro;
                break;
            case (6): //WorldMap
                lowerdialogue = true;
                lowermap = true;
                Main = WorldMapMusic;
                intro = WorldMapMusicintro;
                break;
            case (7): //Ship
                lowerdialogue = true;
                lowermap = true;
                Main = ShipMusic;
                intro = ShipMusicintro;
                break;
            case (8):  //MainMenu
                lowerdialogue = true;
                lowermap = true;
                Main = MainMenuMusic;
                intro = MainMenuMusicintro;
                break;
            case (9):  //CutScene
                lowerdialogue = true;
                lowermap = true;
                Main = CutSceneMusic;
                intro = CutSceneMusicintro;
                break;
            case (10): //otherTurn
                Main = OtherAudioSource;
                intro = OtherAudioSourceIntro;
                break;



        }
        Main.volume = startvolume;
        if (intro.clip == null)
        {
            Main.PlayScheduled(AudioSettings.dspTime);
        }
        else
        {
            intro.volume = startvolume;

            intro.PlayScheduled(AudioSettings.dspTime);

            double introduration = (double)intro.clip.samples / intro.clip.frequency;


            Main.PlayScheduled(AudioSettings.dspTime + introduration);
        }


    }

    public void SetDialogueMusic(int musicID = 0)
    {
        if (musicID > 0 && musicID != CurrentDialogueMusic)
        {

            if (currentDialogueAudioSource == DialogueAudioSource)
            {
                currentDialogueAudioSourceIntro = DialogueAudioSource2Intro;
                currentDialogueAudioSource = DialogueAudioSource2;
            }
            else
            {
                currentDialogueAudioSourceIntro = DialogueAudioSourceIntro;
                currentDialogueAudioSource = DialogueAudioSource;
            }

            CurrentDialogueMusic = musicID;
            currentDialogueAudioSource.clip = DialogueMusicsWithIntro[CurrentDialogueMusic].Music;
            currentDialogueAudioSourceIntro.clip = DialogueMusicsWithIntro[CurrentDialogueMusic].Intro;
            currentDialogueAudioSource.volume = maxvolume;
            currentDialogueAudioSourceIntro.volume = maxvolume;
            PlayMusic(5, maxvolume);
        }
        else if (musicID == -1)
        {
            CurrentDialogueMusic = -1;
        }
    }

    public void ManageMusicTurnRotation()
    {


        switch (TurnManager.currentlyplaying.ToLower())
        {
            case ("playable"):
                if (PlayableAudioSource.isPlaying)
                {
                    ChangeVolume(PlayableAudioSource, 1f);
                    ChangeVolume(PlayableAudioSourceIntro, 1f);
                }
                else
                {
                    PlayMusicWithIntro(2, 1f);
                }
                ChangeVolume(EnemyAudioSource, 0f);
                ChangeVolume(EnemyAudioSourceIntro, 0f);
                ChangeVolume(OtherAudioSource, 0f);
                ChangeVolume(OtherAudioSourceIntro, 0f);
                break;
            case ("enemy"):
                if (EnemyAudioSource.isPlaying)
                {
                    ChangeVolume(EnemyAudioSource, 1f);
                    ChangeVolume(EnemyAudioSourceIntro, 1f);
                }
                else
                {
                    PlayMusicWithIntro(3, 1f);
                }
                ChangeVolume(PlayableAudioSource, 0f);
                ChangeVolume(PlayableAudioSourceIntro, 0f);
                ChangeVolume(OtherAudioSource, 0f);
                ChangeVolume(OtherAudioSourceIntro, 0f);
                break;
            case ("other"):
                if (OtherAudioSource.isPlaying)
                {
                    ChangeVolume(OtherAudioSource, 1f);
                    ChangeVolume(OtherAudioSourceIntro, 1f);
                }
                else
                {
                    PlayMusicWithIntro(10, 1f);
                }
                ChangeVolume(PlayableAudioSource, 0f);
                ChangeVolume(PlayableAudioSourceIntro, 0f);
                ChangeVolume(EnemyAudioSource, 0f);
                ChangeVolume(EnemyAudioSourceIntro, 0f);
                break;
            default:
                if (PlayableAudioSource.isPlaying)
                {
                    PlayableAudioSource.Stop();
                    PlayableAudioSourceIntro.Stop();
                }
                if (EnemyAudioSource.isPlaying)
                {
                    EnemyAudioSource.Stop();
                    EnemyAudioSourceIntro.Stop();
                }
                if (OtherAudioSource.isPlaying)
                {
                    OtherAudioSource.Stop();
                    OtherAudioSourceIntro.Stop();
                }


                break;
        }

    }
    public void SetCutSceneMusic(int musicID = 0)
    {

        CutSceneMusic.clip = DialogueMusicsWithIntro[musicID].Music;
        CutSceneMusicintro.clip = DialogueMusicsWithIntro[musicID].Intro;

        PlayMusic(9, maxvolume);
    }

    private void ChangeVolume(AudioSource source, float targetvolume)
    {
        if (targetvolume != source.volume)
        {
            source.volume = Mathf.MoveTowards(source.volume, targetvolume, Time.deltaTime);
        }
    }

    public void PlayVoiceSE(float pitch)
    {
        StartCoroutine(CreateVoiceFE(pitch));
    }


    private IEnumerator CreateVoiceFE(float pitch)
    {
        GameObject SEholder = new GameObject();
        SEholder.transform.parent = GeneratedSoundHolder.transform;
        SEholder.AddComponent<AudioSource>();
        AudioSource AS = SEholder.GetComponent<AudioSource>();
        AS.outputAudioMixerGroup = mixer.FindMatchingGroups("SoundEffects")[0];
        AS.clip = VoiceSFXList[UnityEngine.Random.Range(0, VoiceSFXList.Count)];
        AS.volume = SFXVolume;
        AS.pitch = pitch + UnityEngine.Random.Range(-0.025f, 0.025f);
        AS.Play();
        yield return new WaitForSeconds(AS.clip.length);
        if (SEholder != null)
        {
            Destroy(SEholder);
        }

    }


    public GameObject PlaySFX(AudioClip clip, float pitch = 1f)
    {
        GameObject SEholder = new GameObject();
        StartCoroutine(CreateSFX(clip, SEholder, pitch));
        return SEholder;
    }


    private IEnumerator CreateSFX(AudioClip clip, GameObject SEholder, float pitch)
    {

        SEholder.transform.parent = GeneratedSoundHolder.transform;
        SEholder.AddComponent<AudioSource>();
        AudioSource AS = SEholder.GetComponent<AudioSource>();
        AS.outputAudioMixerGroup = mixer.FindMatchingGroups("SoundEffects")[0];
        AS.clip = clip;
        AS.volume = SFXVolume;
        AS.pitch = pitch + UnityEngine.Random.Range(-0.025f, 0.025f);
        AS.Play();
        yield return new WaitForSeconds(AS.clip.length);
        if (SEholder != null)
        {
            Destroy(SEholder);
        }
    }

    private void StopAllMusic()
    {
        PlayableAudioSource.Stop();
        PlayableAudioSourceIntro.Stop();
        EnemyAudioSource.Stop();
        EnemyAudioSourceIntro.Stop();
        OtherAudioSource.Stop();
        OtherAudioSourceIntro.Stop();
        CampMusic.Stop();
        CampMusicintro.Stop();
        WorldMapMusic.Stop();
        WorldMapMusicintro.Stop();
        ShipMusic.Stop();
        ShipMusicintro.Stop();
        MainMenuMusic.Stop();
        MainMenuMusicintro.Stop();
    }
}
