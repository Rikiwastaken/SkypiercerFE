using UnityEngine;

public class WeaponParticleManager : MonoBehaviour
{

    private ParticleSystem ParticleSystem;

    private UnitScript unitScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ParticleSystem = GetComponent<ParticleSystem>();
        unitScript = GetComponentInParent<UnitScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (unitScript.enabled && unitScript.isinattackanimation())
        {

            if (!ParticleSystem.isPlaying)
            {
                ParticleSystem.Play();
            }
        }
        else
        {
            if (ParticleSystem.isPlaying)
            {
                ParticleSystem.Stop();
            }
        }

    }


}
