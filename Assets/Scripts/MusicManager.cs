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

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
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



    private void FixedUpdate()
    {
        if (cameraScript == null)
        {
            cameraScript = FindAnyObjectByType<cameraScript>();
        }

        if (TurnManager == null)
        {
            TurnManager = FindAnyObjectByType<TurnManger>();
        }
        if (PlayPrepMusic)
        {
            PlayPrepMusic = false;
            PlayMusicWithIntro(4);
        }

        if (SceneLoader.instance.LoadingImage.gameObject.activeSelf)
        {
            return;
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
            PlayMusicWithIntro(2);
            PlayMusicWithIntro(3);
        }

        if (incombat.isPlaying && !(FindAnyObjectByType<GameOverScript>() != null && FindAnyObjectByType<GameOverScript>().gameObject.activeSelf))
        {
            if ((cameraScript != null && cameraScript.incombat) || SceneManager.GetActiveScene().name == "BattleScene")
            {
                if (incombat.volume < maxvolume)
                {
                    incombat.volume += Time.fixedDeltaTime;
                }
                else
                {
                    incombat.volume = maxvolume;
                }
                if (outcombat.volume > 0)
                {
                    outcombat.volume -= Time.fixedDeltaTime;
                }
            }
            else
            {
                if (outcombat.volume < maxvolume)
                {
                    outcombat.volume += Time.fixedDeltaTime;
                }
                else
                {
                    outcombat.volume = maxvolume;
                }
                if (incombat.volume > 0f)
                {
                    incombat.volume -= Time.fixedDeltaTime;
                }
            }
        }

        if (SceneManager.GetActiveScene().name == "Camp")
        {
            if (!CampMusic.isPlaying && !CampMusicintro.isPlaying)
            {
                PlayMusicWithIntro(1);
            }
            if (MainMenuMusic.volume > 0)
            {
                MainMenuMusic.volume -= Time.fixedDeltaTime;
                MainMenuMusicintro.volume -= Time.fixedDeltaTime;
            }
        }
        else if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            if (!MainMenuMusic.isPlaying)
            {
                PlayMusicWithIntro(0);
            }
            if (CampMusic.volume > 0)
            {
                CampMusic.volume -= Time.fixedDeltaTime;
                CampMusicintro.volume -= Time.fixedDeltaTime;
            }
        }
        else
        {
            if (CampMusic.volume > 0)
            {
                CampMusic.volume -= Time.fixedDeltaTime;
                CampMusicintro.volume -= Time.fixedDeltaTime;
            }
            if (MainMenuMusic.volume > 0)
            {
                MainMenuMusic.volume -= Time.fixedDeltaTime;
                MainMenuMusicintro.volume -= Time.fixedDeltaTime;
            }
        }

        if (TextBubbleScript.Instance != null && TextBubbleScript.Instance.indialogue)
        {

            if (currentDialogueAudioSource != null && (currentDialogueAudioSource.isPlaying || currentDialogueAudioSourceIntro.isPlaying) && currentDialogueAudioSource.volume > 0 && CurrentDialogueMusic != -1)
            {
                if (CampMusic.volume > 0)
                {
                    CampMusic.volume -= Time.fixedDeltaTime * 2;
                }
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
                    if (DialogueAudioSource2.volume > 0)
                    {
                        DialogueAudioSource2.volume -= Time.fixedDeltaTime * 2;
                    }
                }
                else
                {
                    if (DialogueAudioSource.volume > 0)
                    {
                        DialogueAudioSource.volume -= Time.fixedDeltaTime * 2;
                    }
                }


                if (currentDialogueAudioSource.volume <= maxvolume)
                {
                    currentDialogueAudioSource.volume += maxvolume;
                }
            }
            else
            {
                if (currentDialogueAudioSource != null && currentDialogueAudioSource.volume > 0)
                {
                    currentDialogueAudioSource.volume -= Time.fixedDeltaTime * 2;
                }
            }
        }

    }

    private void PlayMusicWithIntro(int TypeID)
    {
        AudioSource Main = null;
        AudioSource intro = null;

        switch (TypeID)
        {
            case (0): //MainMenu
                Main = MainMenuMusic;
                intro = MainMenuMusicintro;
                lowerdialogue = true;
                break;
            case (1): //Camp
                Main = CampMusic;
                intro = CampMusicintro;
                lowerdialogue = true;
                break;
            case (2): //OutCombat
                Main = outcombat;
                intro = outcombatintro;
                lowerdialogue = true;
                break;
            case (3): //InCombat
                Main = incombat;
                intro = incombatintro;
                lowerdialogue = true;
                break;
            case (4): //BeforeComabt
                Main = BeforeCombat;
                intro = BeforeCombatintro;
                incombat.Stop();
                outcombat.Stop();
                lowerdialogue = true;
                break;
            case (5): //DialogueAudio
                lowerdialogue = false;
                Main = currentDialogueAudioSource;
                intro = currentDialogueAudioSourceIntro;
                break;

        }
        Main.volume = maxvolume;
        if (intro.clip == null)
        {
            Main.PlayScheduled(AudioSettings.dspTime);
        }
        else
        {
            intro.volume = maxvolume;

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
            PlayMusicWithIntro(5);
        }
        else if (musicID == -1)
        {
            CurrentDialogueMusic = -1;
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
        Destroy(SEholder);
    }
}
