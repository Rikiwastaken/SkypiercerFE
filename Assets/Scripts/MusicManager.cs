using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;


public class MusicManager : MonoBehaviour
{
    public AudioSource incombat;
    public AudioSource incombatintro;
    public AudioSource outcombat;
    public AudioSource outcombatintro;
    public AudioSource BeforeCombat;
    public AudioSource BeforeCombatintro;

    public AudioSource CampMusic;
    public AudioSource CampMusicintro;
    public AudioSource WorldMapMusic;
    public AudioSource WorldMapMusicintro;


    public AudioSource MainMenuMusic;
    public AudioSource MainMenuMusicintro;


    public AudioSource DialogueAudioSource;
    public AudioSource DialogueAudioSourceIntro;
    public AudioSource DialogueAudioSource2;
    public AudioSource DialogueAudioSource2Intro;

    private AudioSource currentDialogueAudioSource;
    private AudioSource currentDialogueAudioSourceIntro;

    public int CurrentDialogueMusic;

    private bool lowerdialogue;
    public bool lowermap;

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

    private cameraScript cameraScript;

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
    }

    public List<MapBattleMusic> MusicList;

    private bool PlayPrepMusic;

    private bool PrepFinished;

    private GameOverScript GameOverScript;

    public string currentscene;

    public bool inCombatBool;

    private TextBubbleScript textBubbleScript;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        SceneManager.sceneLoaded += OnSceneLoad;
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

    public void ChangeVolume()
    {
        mixer.SetFloat("MusicVol", Mathf.Log10(SaveManager.Options.musicvolume) * 20f);
        mixer.SetFloat("SEVol", Mathf.Log10(SaveManager.Options.SEVolume) * 20f);
    }

    void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "BattleScene")
        {
            return;
        }
        if (currentDialogueAudioSource != null)
        {
            currentDialogueAudioSource.volume = 0f;
            currentDialogueAudioSourceIntro.volume = 0f;
        }
        if ((scene.name == "Camp" || scene.name == "WorldMap") && currentscene != scene.name)
        {

            PlayMusicWithIntro(1, 0f);
            PlayMusicWithIntro(6, 0f);
        }

        currentscene = scene.name;
        MusicManager.instance.InitializeMusics(currentscene);
    }

    public void InitializeMusics(string ChapterToLoad)
    {
        int Chapter = -1;
        if (ChapterToLoad.Contains("Chapter"))
        {
            ChapterToLoad = ChapterToLoad.Replace("Chapter", "");
            Chapter = int.Parse(ChapterToLoad);
        }
        if (ChapterToLoad.Contains("Prologue") || ChapterToLoad.Contains("TestMap"))
        {
            Chapter = 0;
        }
        incombat.Stop();
        outcombat.Stop();
        BeforeCombat.Stop();

        if (Chapter != -1)
        {

            foreach (MapBattleMusic MusicClass in MusicList)
            {
                if (MusicClass.Chapters.Contains(Chapter))
                {
                    incombat.clip = MusicClass.BattleMusic;
                    incombatintro.clip = MusicClass.BattleMusicIntro;
                    outcombat.clip = MusicClass.MapMusic;
                    outcombatintro.clip = MusicClass.MapMusicIntro;
                    BeforeCombat.clip = MusicClass.PrepMusic;
                    BeforeCombatintro.clip = MusicClass.PrepMusicIntro;
                    break;
                }
            }
            PlayPrepMusic = true;
        }
    }



    private void Update()
    {
        if (cameraScript == null)
        {
            cameraScript = FindAnyObjectByType<cameraScript>();
        }

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

        if (SceneLoader.instance.LoadingImage.gameObject.activeSelf)
        {
            return;
        }
        if (PlayPrepMusic)
        {
            PlayPrepMusic = false;
            PlayMusicWithIntro(4, maxvolume);
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
            if (TurnManager.currentlyplaying != "" && !incombat.isPlaying)
            {
                PrepFinished = true;
            }
            else
            {
                PrepFinished = false;
            }
        }

        if (PrepFinished && !incombat.isPlaying)
        {
            BeforeCombat.Stop();
            PlayMusicWithIntro(2, maxvolume);
            PlayMusicWithIntro(3, maxvolume);
            incombat.volume = 0f;
        }

        if (incombat.isPlaying && !(GameOverScript != null && GameOverScript.gameObject.activeSelf))
        {
            if (FreezeFrameCapture.instance != null && FreezeFrameCapture.instance.ShowingLevelUp)
            {
                if (incombat.volume > 0)
                {
                    ChangeVolume(incombat, 0.1f);
                    ChangeVolume(incombatintro, 0.1f);

                    ChangeVolume(outcombat, 0f);
                    ChangeVolume(outcombatintro, 0f);
                }
                else
                {
                    ChangeVolume(incombat, 0f);
                    ChangeVolume(incombatintro, 0f);

                    ChangeVolume(outcombat, 0.1f);
                    ChangeVolume(outcombatintro, 0.1f);
                }

            }
            else if (!lowermap && (cameraScript != null && cameraScript.incombat) || inCombatBool)
            {
                ChangeVolume(incombat, maxvolume);
                ChangeVolume(incombatintro, maxvolume);

                ChangeVolume(outcombat, 0f);
                ChangeVolume(outcombatintro, 0f);


            }
            else
            {
                ChangeVolume(outcombat, maxvolume);
                ChangeVolume(outcombatintro, maxvolume);

                ChangeVolume(incombat, 0f);
                ChangeVolume(incombatintro, 0f);
            }

        }

        if (currentscene == "Camp")
        {
            if (!CampMusic.isPlaying && !CampMusicintro.isPlaying)
            {
                PlayMusicWithIntro(1, 0f);
            }
            ChangeVolume(MainMenuMusic, 0f);
            ChangeVolume(MainMenuMusicintro, 0f);

            ChangeVolume(CampMusic, maxvolume);
            ChangeVolume(CampMusicintro, maxvolume);
            ChangeVolume(WorldMapMusic, 0f);
            ChangeVolume(WorldMapMusicintro, 0f);

        }
        else if (currentscene == "WorldMap")
        {
            if (!WorldMapMusic.isPlaying && !WorldMapMusicintro.isPlaying)
            {
                PlayMusicWithIntro(6, 0f);
            }
            ChangeVolume(MainMenuMusic, 0f);
            ChangeVolume(MainMenuMusicintro, 0f);

            ChangeVolume(CampMusic, 0f);
            ChangeVolume(CampMusicintro, 0f);
            ChangeVolume(WorldMapMusic, maxvolume);
            ChangeVolume(WorldMapMusicintro, maxvolume);
        }
        else if (currentscene == "MainMenu")
        {
            if (!MainMenuMusic.isPlaying)
            {
                PlayMusicWithIntro(0, maxvolume);
            }
            ChangeVolume(CampMusic, 0f);
            ChangeVolume(CampMusicintro, 0f);
        }
        else
        {
            ChangeVolume(MainMenuMusic, 0f);
            ChangeVolume(MainMenuMusicintro, 0f);
            ChangeVolume(CampMusic, 0f);
            ChangeVolume(CampMusicintro, 0f);
        }

        if (textBubbleScript != null && textBubbleScript.indialogue)
        {
            if (lowermap)
            {
                ChangeVolume(outcombat, 0f);
                ChangeVolume(outcombatintro, 0f);

                ChangeVolume(incombat, 0f);
                ChangeVolume(incombatintro, 0f);
            }
            if (currentDialogueAudioSource != null && (currentDialogueAudioSource.isPlaying || currentDialogueAudioSourceIntro.isPlaying) && currentDialogueAudioSource.volume > 0 && CurrentDialogueMusic != -1)
            {
                ChangeVolume(MainMenuMusic, 0f);
                ChangeVolume(MainMenuMusicintro, 0f);
                ChangeVolume(CampMusic, 0f);
                ChangeVolume(CampMusicintro, 0f);
                if (outcombat.volume > 0)
                {
                    outcombat.volume -= Time.fixedDeltaTime * 2;
                }
                if (incombat.volume > 0)
                {
                    incombat.volume -= Time.fixedDeltaTime * 2;
                }
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

    private void PlayMusicWithIntro(int TypeID, float startvolume)
    {
        AudioSource Main = null;
        AudioSource intro = null;

        switch (TypeID)
        {
            case (0): //MainMenu
                Main = MainMenuMusic;
                intro = MainMenuMusicintro;
                lowerdialogue = true;
                lowermap = true;
                break;
            case (1): //Camp
                Main = CampMusic;
                intro = CampMusicintro;
                lowerdialogue = true;
                lowermap = true;
                break;
            case (2): //OutCombat
                Main = outcombat;
                intro = outcombatintro;
                lowerdialogue = true;
                lowermap = false;
                break;
            case (3): //InCombat
                Main = incombat;
                intro = incombatintro;
                lowerdialogue = true;
                lowermap = false;
                break;
            case (4): //BeforeComabt
                Main = BeforeCombat;
                intro = BeforeCombatintro;
                incombat.Stop();
                outcombat.Stop();
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
            PlayMusicWithIntro(5, maxvolume);
        }
        else if (musicID == -1)
        {
            CurrentDialogueMusic = -1;
        }
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
}
