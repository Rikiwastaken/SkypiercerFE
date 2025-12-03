using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;


public class MusicManager : MonoBehaviour
{
    public AudioSource incombat;
    public AudioSource outcombat;
    public AudioSource BeforeCombat;

    public float maxvolume;

    public AudioMixer mixer;

    private cameraScript cameraScript;

    private TurnManger TurnManager;
    
    private SaveManager SaveManager;

    private float beforecombatmusicvol;

    public static MusicManager instance;

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
        if(instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        beforecombatmusicvol = BeforeCombat.volume;
        SaveManager = FindAnyObjectByType<SaveManager>();
        ChangeVolume();

        if (BeforeCombat.isPlaying)
        {
            BeforeCombat.volume = beforecombatmusicvol;
        }
    }

    public void ChangeVolume()
    {
        mixer.SetFloat("MusicVol",Mathf.Log10(SaveManager.Options.musicvolume)*20f);
    }

    public void InitializeMusics(string ChapterToLoad)
    {
        int Chapter = -1;
        if(ChapterToLoad.Contains("Chapter"))
        {
            ChapterToLoad = ChapterToLoad.Replace("Chapter", "");
            Chapter = int.Parse(ChapterToLoad);
        }
        if(ChapterToLoad.Contains("Prologue") || ChapterToLoad.Contains("TestMap"))
        {
            Chapter = 0;
        }
        incombat.Stop();
        outcombat.Stop();
        BeforeCombat.Stop();

        if (Chapter!=-1)
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
        if(cameraScript == null)
        {
            cameraScript = FindAnyObjectByType<cameraScript>();
        }
        
        if(TurnManager == null)
        {
            TurnManager = FindAnyObjectByType<TurnManger>();
        }

        if(PlayPrepMusic)
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

        if(incombat.isPlaying)
        {
            if ((cameraScript!=null && cameraScript.incombat) || SceneManager.GetActiveScene().name == "BattleScene")
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

          
    }

}
