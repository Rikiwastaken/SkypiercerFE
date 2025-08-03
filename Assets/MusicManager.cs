using UnityEngine;


public class MusicManager : MonoBehaviour
{
    public AudioSource incombat;
    public AudioSource outcombat;



    public battlecameraScript battlecameraScript;
    private void FixedUpdate()
    {
        if(battlecameraScript.incombat)
        {
            if(incombat.volume<1f)
            {
                incombat.volume +=Time.fixedDeltaTime;
            }
            if(outcombat.volume>0f)
            {
                outcombat.volume -= Time.fixedDeltaTime;
            }
        }
        else
        {
            if (outcombat.volume < 1f)
            {
                outcombat.volume += Time.fixedDeltaTime;
            }
            if (incombat.volume > 0f)
            {
                incombat.volume -= Time.fixedDeltaTime;
            }
        }
    }

}
