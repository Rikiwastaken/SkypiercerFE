using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonSoundScript : MonoBehaviour
{

    public AudioMixerGroup SESound;
    public float volume;
    public AudioClip SFX;
    public Button[] buttons;

    public bool automaticallyplay = true;

    void Start()
    {
        if (automaticallyplay)
        {
            buttons = GameObject.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var b in buttons)
            {
                UnityAction l = delegate { StartCoroutine(PlayButtonSFX()); };
                b.onClick.AddListener(l);
            }
        }

    }


    public IEnumerator PlayButtonSFX()
    {
        GameObject GO = new GameObject();
        GO.transform.parent = transform;
        AudioSource audioSource = GO.AddComponent<AudioSource>();
        audioSource.clip = SFX;
        audioSource.outputAudioMixerGroup = SESound;
        audioSource.volume = volume;
        audioSource.Play();
        yield return new WaitForSeconds(SFX.length);
        Destroy(GO);
    }
}
