using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections.Generic;
using UnityEngine;

public class FMODEventManager : MonoBehaviour
{

    public static FMODEventManager instance;

    private TurnManger _TurnManger;

    private Bank AllyBank;
    private Bank EnemyBank;
    private Bank OtherBank;
    private Bank PrepBank;
    private Bank VoiceSFXBank;

    [SerializeField] private string AllyBankRef;
    [SerializeField] private string EnemyBankRef;
    [SerializeField] private string OtherBankRef;
    [SerializeField] private string PrepBankRef;
    [SerializeField] private string VoiceSFXBankRef;

    private EventInstance currentEvent;
    private EventDescription prepMusicEvent;
    private EventDescription AllyMusic;
    private EventDescription EnemyMusic;
    private EventDescription OtherMusic;


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

    private string previouslyplaying;

    [SerializeField] private List<MapMusic> musicList;


    private void Awake()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RuntimeManager.StudioSystem.getBank(AllyBankRef, out AllyBank);
        RuntimeManager.StudioSystem.getBank(EnemyBankRef, out EnemyBank);
        RuntimeManager.StudioSystem.getBank(OtherBankRef, out OtherBank);
        RuntimeManager.StudioSystem.getBank(PrepBankRef, out PrepBank);
        RuntimeManager.StudioSystem.getBank(VoiceSFXBankRef, out VoiceSFXBank);
    }

    // Update is called once per frame
    void Update()
    {
        if (_TurnManger == null)
        {
            _TurnManger = TurnManger.instance;

        }

        if (_TurnManger != null) // if turnmanager was found, then that means we're in a battle map
        {
            if (_TurnManger.currentlyplaying != previouslyplaying)
            {
                previouslyplaying = _TurnManger.currentlyplaying;
                ChangeBattleTheme(_TurnManger.currentlyplaying);
            }
        }
        else
        {
            previouslyplaying = "none";
            StopCurrentMusic();
        }
    }

    private void StopCurrentMusic()
    {
        if (currentEvent.isValid())
        {
            currentEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            currentEvent.release();
            currentEvent.clearHandle();
        }
    }

    public void ChangeBattleTheme(string currentlyplaying)
    {

        if (currentlyplaying == "")
        {
            if (!currentEvent.isValid() || !IsSameEvent(currentEvent, prepMusicEvent))
            {
                StopCurrentMusic();
                prepMusicEvent.createInstance(out currentEvent);
                currentEvent.start();
            }

        }
        else if (currentlyplaying == "playable" || currentlyplaying == "tutorial")
        {
            if (!currentEvent.isValid() || !IsSameEvent(currentEvent, AllyMusic))
            {
                StopCurrentMusic();
                AllyMusic.createInstance(out currentEvent);
                currentEvent.start();
            }
        }
        else if (currentlyplaying == "enemy")
        {
            if (!currentEvent.isValid() || !IsSameEvent(currentEvent, EnemyMusic))
            {
                StopCurrentMusic();
                EnemyMusic.createInstance(out currentEvent);
                currentEvent.start();
            }
        }
        else if (currentlyplaying == "other")
        {
            if (!currentEvent.isValid() || !IsSameEvent(currentEvent, OtherMusic))
            {
                StopCurrentMusic();
                OtherMusic.createInstance(out currentEvent);
                currentEvent.start();
            }
        }
        else
        {
            StopCurrentMusic();
        }
    }

    public void InitializeMapMusic(string SceneToload)
    {
        if (!SceneToload.ToLower().Contains("chapter") && !SceneToload.ToLower().Contains("sidestory") && !SceneToload.ToLower().Contains("prologue"))
        {
            return;
        }
        bool IsSideStory = false;
        int sceneID = 0;
        if (SceneToload.ToLower().Contains("sidestory"))
        {
            sceneID = int.Parse(SceneToload.ToLower().Replace("sidestory", ""));
        }
        else if (SceneToload.ToLower().Contains("chapter"))
        {
            sceneID = int.Parse(SceneToload.ToLower().Replace("chapter", ""));
        }

        foreach (MapMusic music in musicList)
        {
            if (music.useforSideStory == IsSideStory && music.Chapters.Contains(sceneID))
            {
                prepMusicEvent = GetEventFromBank(music.PrepMusicID, PrepBank);
                AllyMusic = GetEventFromBank(music.PlayableMusicID, AllyBank);
                EnemyMusic = GetEventFromBank(music.EnemyMusicID, EnemyBank);
                OtherMusic = GetEventFromBank(music.OtherrMusicID, OtherBank);
                return;
            }
        }
    }


    public void CreateVoiceFE(float pitch)
    {

        EventDescription SFXEvent = GetRandomEventFromBank(VoiceSFXBank);

        SFXEvent.createInstance(out EventInstance SFXEventInstance);
        SFXEventInstance.setPitch(pitch);
        SFXEventInstance.start();
        SFXEventInstance.release();


    }

    private bool IsSameEvent(EventInstance instance, EventDescription description)
    {
        if (!instance.isValid() || !description.isValid())
            return false;

        instance.getDescription(out EventDescription currentDescription);

        currentDescription.getID(out FMOD.GUID currentGUID);
        description.getID(out FMOD.GUID targetGUID);

        return currentGUID.Equals(targetGUID);
    }

    private EventDescription GetRandomEventFromBank(Bank bank)
    {
        bank.getEventList(out EventDescription[] events);

        int ID = UnityEngine.Random.Range(0, events.Length);

        return events[ID];
    }

    private EventDescription GetEventFromBank(int ID, Bank bank)
    {
        bank.getEventList(out EventDescription[] events);

        if (ID < 0 || ID >= events.Length)
        {
            Debug.LogError("Event ID out of range");
            return default;
        }

        return events[ID];
    }


}
