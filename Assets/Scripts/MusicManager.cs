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

    public AudioSource LevelUpAS;

    public int CurrentDialogueMusic;

    public bool lowerdialogue;
    public bool lowermap;

    [Serializable]
    public class Audios
    {
        public AudioClip Intro;
        public AudioClip Music;
        public string note;
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

    public List<Audios> PlayableMusics;
    public List<Audios> EnemyMusics;
    public List<Audios> OtherMusics;
    public List<Audios> PrepMusics;

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

    public float timebeforemusicplays;
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
            if (actionsMenu == null)
            {
                actionsMenu = ActionsMenu.instance;
            }
        }

        if ((DialogueAudioSource.isPlaying || DialogueAudioSource2.isPlaying) && (textBubbleScript == null || TextBubbleScript.Instance == null || textBubbleScript.indialogue == false))
        {
            ChangeVolume(DialogueAudioSource, 0f);
            ChangeVolume(DialogueAudioSourceIntro, 0f);
            ChangeVolume(DialogueAudioSource2, 0f);
            ChangeVolume(DialogueAudioSource2Intro, 0f);

            if (DialogueAudioSource.volume <= 0.001f)
            {
                DialogueAudioSource.Stop();
            }
            if (DialogueAudioSource2.volume <= 0.001f)
            {
                DialogueAudioSource2.Stop();
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
            ChangeVolume(CutSceneMusic, 1f);
            ChangeVolume(CutSceneMusicintro, 1f);
            return;
        }

        if (PrepFinished && !PlayableAudioSource.isPlaying)
        {
            BeforeCombat.Stop();
            PlayMusic(2, maxvolume);
        }

        if (currentscene == "Camp")
        {
            // FIX: previously checked "!CampMusic.isPlaying && !CampMusicintro.isPlaying" every frame.
            // AudioSource.isPlaying can read false for a scheduled clip until its dspTime is actually
            // reached, so this was re-triggering PlayMusic(1, ...) (which calls StopAllMusic()) over
            // and over, cancelling Camp music before it ever really started - and cancelling dialogue
            // music playing alongside it. Tracking the currently-requested music type is reliable.
            if (currentMusicType != 1)
            {
                PlayMusic(1, 0f, true);
            }

            ChangeVolume(CampMusic, maxvolume);
            ChangeVolume(CampMusicintro, maxvolume);
            ChangeVolume(WorldMapMusic, 0f);
            ChangeVolume(WorldMapMusicintro, 0f);

        }
        else if (currentscene == "WorldMap")
        {
            // FIX: same reasoning as the Camp block above - avoid retriggering off isPlaying.
            if (currentMusicType != 6)
            {
                PlayMusic(6, 0f, true);
                PlayMusicWithIntro(7, 0f, true);
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
            if (LevelUpAS.isPlaying)
            {
                ChangeVolume(PlayableAudioSource, 0f);
                ChangeVolume(PlayableAudioSourceIntro, 0f);
                ChangeVolume(EnemyAudioSource, 0f);
                ChangeVolume(EnemyAudioSourceIntro, 0f);
                ChangeVolume(OtherAudioSource, 0f);
                ChangeVolume(OtherAudioSourceIntro, 0f);
            }
            else if (TurnManager != null)
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
            if (currentDialogueAudioSource != null && (currentDialogueAudioSource.isPlaying || currentDialogueAudioSourceIntro.isPlaying) && (currentDialogueAudioSource.volume > 0 || currentDialogueAudioSourceIntro.volume > 0) && CurrentDialogueMusic != -1)
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

    public void PlayLevelUpJingle()
    {
        LevelUpAS.Play();
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

        currentscene = nextscene.name;
        if (nextscene.name == "CutsceneScene")
        {
            return;
        }

        // FIX (root cause of "no music in Camp/WorldMap"): InitializeMusics() unconditionally
        // Stop()s CampMusic/WorldMapMusic/ShipMusic/BeforeCombat/CutSceneMusic. It used to run
        // AFTER the block below had already started Camp/WorldMap music, so that music was killed
        // the instant it was scheduled. This was only ever "working" because the Update() watchdog
        // polled AudioSource.isPlaying and blindly restarted whatever it found stopped - a timing
        // accident, not a real fix, and it's what silently papered over this bug before. Running
        // InitializeMusics() first, then starting the scene's music, removes the need for that
        // accident entirely.
        InitializeMusics(currentscene);

        if (nextscene.name == "Camp")
        {
            ResetAll();
            PlayMusic(1);
        }
        else if (nextscene.name == "WorldMap")
        {
            ResetAll();
            PlayMusic(6);
            // FIX: Ship music used to only ever get scheduled by the Update() watchdog the first
            // time it saw WorldMap/Ship not playing. Now that the watchdog is gated on
            // currentMusicType (see Update()) instead of isPlaying, that first trigger would never
            // happen once PlayMusic(6) above already sets currentMusicType to 6. Scheduling it here
            // keeps behavior identical to before without depending on watchdog timing.
            PlayMusicWithIntro(7, 0f, true);
        }
        else if (nextscene.name == "MainMenu")
        {
            ResetAll();
            PlayMusic(8, maxvolume);
        }
    }

    public void StopDialogueMusic()
    {
        currentDialogueAudioSource = null;
        currentDialogueAudioSourceIntro = null;
        DialogueAudioSource.Stop();
        DialogueAudioSource2.Stop();
        DialogueAudioSourceIntro.Stop();
        DialogueAudioSource2Intro.Stop();
        // FIX: this was never reset, so if the next dialogue reused the same musictoplay ID,
        // SetDialogueMusic's "musicID != CurrentDialogueMusic" guard would skip re-triggering it
        // even though the actual audio sources above were just stopped and nulled out.
        CurrentDialogueMusic = -1;
    }

    public void InitializeMusics(string ChapterToLoad)
    {
        Debug.Log("did this play");
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
                    PlayableAudioSource.clip = PlayableMusics[MusicClass.PlayableMusicID].Music;
                    PlayableAudioSourceIntro.clip = PlayableMusics[MusicClass.PlayableMusicID].Intro;
                    EnemyAudioSource.clip = EnemyMusics[MusicClass.EnemyMusicID].Music;
                    EnemyAudioSourceIntro.clip = EnemyMusics[MusicClass.EnemyMusicID].Intro;
                    OtherAudioSource.clip = OtherMusics[MusicClass.OtherrMusicID].Music;
                    OtherAudioSourceIntro.clip = OtherMusics[MusicClass.OtherrMusicID].Intro;
                    BeforeCombat.clip = PrepMusics[MusicClass.PrepMusicID].Music;
                    BeforeCombatintro.clip = PrepMusics[MusicClass.PrepMusicID].Intro;
                    break;
                }
            }
            if (Chapter != 0)
            {
                PlayPrepMusic = true;
            }

        }
    }

    private void ResetAll()
    {
        StopAllCoroutines();
        StopAllMusic();
    }
    void PlayMusic(int type, float startvolume = 0f, bool ignoreStartOfset = false)
    {
        //if (currentMusicType == type)
        //{
        //    if (type != 9)
        //    {
        //        return;
        //    }

        //}

        // FIX: dialogue music (type 5) used to fall through to StopAllMusic() below just like every
        // other type. That meant starting a dialogue line hard-stopped Camp/WorldMap/Playable/Enemy/
        // Other/Ship/MainMenu music instead of just ducking it (which Update() already does via
        // ChangeVolume fades). It also overwrote currentMusicType with 5, so the very next frame the
        // Camp/WorldMap watchdogs above thought the scene music wasn't playing and restarted it from
        // scratch - which is why Camp music never came back after a dialogue line. Dialogue is a layer
        // on top of whatever scene music is active, so it should not touch either of those.
        if (type != 5)
        {
            StopAllMusic();
            currentMusicType = type;
        }

        if (type == 2 || type == 3)
        {
            PlayMusicWithIntro(2, startvolume, ignoreStartOfset);
            PlayMusicWithIntro(3, startvolume, ignoreStartOfset);
        }
        else
        {
            PlayMusicWithIntro(type, startvolume, ignoreStartOfset);
        }


    }
    private void PlayMusicWithIntro(int TypeID, float startvolume, bool ignorestartoffset = false)
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

        float startofset = timebeforemusicplays;
        if (ignorestartoffset)
        {
            startofset = 0f;
        }
        double dsptime = AudioSettings.dspTime;
        if (intro.clip == null)
        {
            Main.PlayScheduled(dsptime + startofset);
        }
        else
        {
            intro.volume = startvolume;



            intro.PlayScheduled(dsptime + startofset);

            double introduration = (double)intro.clip.samples / intro.clip.frequency;


            Main.PlayScheduled(dsptime + introduration + startofset);
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
            // FIX: was PlayMusic(5, maxvolume) with ignoreStartOfset left as false, which meant the
            // dialogue clip was scheduled to start "timebeforemusicplays" seconds in the future -
            // an audible silent gap every time dialogue music kicked in. Camp/WorldMap already pass
            // ignoreStartOfset: true for the same reason; dialogue should too.
            PlayMusic(5, maxvolume, true);
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
            case ("tutorial"): // tutorial is just playable phase
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