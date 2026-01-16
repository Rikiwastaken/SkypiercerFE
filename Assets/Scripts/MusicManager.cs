using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;


public class MusicManager : MonoBehaviour
{
    public AudioSource incombat;
    public AudioSource outcombat;
    public AudioSource BeforeCombat;

    public AudioSource CampMusic;
    public AudioSource MainMenuMusic;


    public AudioSource DialogueAudioSource;
    public AudioSource DialogueAudioSource2;

    private AudioSource currentAudioSource;

    public int CurrentDialogueMusic;

    public List<AudioClip> DialogueMusics;

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
        public AudioClip MapMusic;
        public AudioClip PrepMusic;
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
                    outcombat.clip = MusicClass.MapMusic;
                    BeforeCombat.clip = MusicClass.PrepMusic;
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
            BeforeCombat.Play();
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
            double startTime = AudioSettings.dspTime + 0.1; // small delay to guarantee readiness

            incombat.PlayScheduled(startTime);
            outcombat.PlayScheduled(startTime);
        }

        if (incombat.isPlaying)
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
            if (CampMusic.volume < maxvolume)
            {
                CampMusic.volume += Time.fixedDeltaTime;
            }
            if (!CampMusic.isPlaying)
            {
                CampMusic.Play();
            }
            if (MainMenuMusic.volume > 0)
            {
                MainMenuMusic.volume -= Time.fixedDeltaTime;
            }
        }
        else if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            if (MainMenuMusic.volume < maxvolume)
            {
                MainMenuMusic.volume += Time.fixedDeltaTime;
            }
            if (!MainMenuMusic.isPlaying)
            {
                MainMenuMusic.Play();
            }
            if (CampMusic.volume > 0)
            {
                CampMusic.volume -= Time.fixedDeltaTime;
            }
        }
        else
        {
            if (CampMusic.volume > 0)
            {
                CampMusic.volume -= Time.fixedDeltaTime;
            }
            if (MainMenuMusic.volume > 0)
            {
                MainMenuMusic.volume -= Time.fixedDeltaTime;
            }
        }

        if (TextBubbleScript.Instance != null && TextBubbleScript.Instance.indialogue)
        {

            if (currentAudioSource != null && currentAudioSource.isPlaying && currentAudioSource.volume > 0 && CurrentDialogueMusic != -1)
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

                if (currentAudioSource == DialogueAudioSource)
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


                if (currentAudioSource.volume <= maxvolume)
                {
                    currentAudioSource.volume += maxvolume;
                }
            }
            else
            {
                if (currentAudioSource != null && currentAudioSource.volume > 0)
                {
                    currentAudioSource.volume -= Time.fixedDeltaTime * 2;
                }
            }
        }

    }

    public void SetDialogueMusic(int musicID = 0)
    {
        if (musicID > 0 && musicID != CurrentDialogueMusic)
        {

            if (currentAudioSource == DialogueAudioSource)
            {
                currentAudioSource = DialogueAudioSource2;
            }
            else
            {
                currentAudioSource = DialogueAudioSource;
            }

            CurrentDialogueMusic = musicID;
            currentAudioSource.clip = DialogueMusics[CurrentDialogueMusic];
            currentAudioSource.volume = Time.deltaTime;
            currentAudioSource.Play();
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
