using UnityEngine;


public class MusicManager : MonoBehaviour
{
    public AudioSource incombat;
    public AudioSource outcombat;
    public AudioSource BeforeCombat;

    public float maxvolume;


    public battlecameraScript battlecameraScript;

    private TurnManger TurnManager;

    private void FixedUpdate()
    {
        if(TurnManager == null)
        {
            TurnManager=FindAnyObjectByType<TurnManger>();
        }

        if(TurnManager.currentlyplaying!="" && !incombat.isPlaying)
        {
            BeforeCombat.Stop();
            incombat.Play();
            outcombat.Play();
        }

        if(battlecameraScript.incombat)
        {
            if(incombat.volume< maxvolume)
            {
                incombat.volume +=Time.fixedDeltaTime;
            }
            else
            {
                incombat.volume = maxvolume;
            }
            if (outcombat.volume>0)
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
