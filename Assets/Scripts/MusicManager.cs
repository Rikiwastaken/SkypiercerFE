using UnityEngine;


public class MusicManager : MonoBehaviour
{
    public AudioSource incombat;
    public AudioSource outcombat;
    public AudioSource BeforeCombat;

    public float maxvolume;


    public battlecameraScript battlecameraScript;

    private TurnManger TurnManager;
    
    private SaveManager SaveManager;

    private float beforecombatmusicvol;

    private void Start()
    {
        beforecombatmusicvol = BeforeCombat.volume;
        SaveManager = FindAnyObjectByType<SaveManager>();
        float volumemult = (float)SaveManager.Options.musicvolume / 100f;

        if (BeforeCombat.isPlaying)
        {
            BeforeCombat.volume = beforecombatmusicvol * volumemult;
        }
    }

    private void FixedUpdate()
    {
        if(TurnManager == null)
        {
            TurnManager=FindAnyObjectByType<TurnManger>();
        }

        float volumemult = (float)SaveManager.Options.musicvolume / 100f;

        if(BeforeCombat.isPlaying)
        {
            BeforeCombat.volume = beforecombatmusicvol * volumemult;
        }



        if (TurnManager.currentlyplaying!="" && !incombat.isPlaying)
        {
            BeforeCombat.Stop();
            double startTime = AudioSettings.dspTime + 0.1; // small delay to guarantee readiness

            incombat.PlayScheduled(startTime);
            outcombat.PlayScheduled(startTime);
        }

        if(battlecameraScript.incombat)
        {
            if(incombat.volume< maxvolume* volumemult)
            {
                incombat.volume +=Time.fixedDeltaTime;
            }
            else
            {
                incombat.volume = maxvolume* volumemult;
            }
            if (outcombat.volume>0)
            {
                outcombat.volume -= Time.fixedDeltaTime;
            }
        }
        else
        {
            if (outcombat.volume < maxvolume* volumemult)
            {
                outcombat.volume += Time.fixedDeltaTime;
            }
            else
            {
                outcombat.volume = maxvolume * volumemult;
            }
            if (incombat.volume > 0f)
            {
                incombat.volume -= Time.fixedDeltaTime;
            }
        }
    }

}
