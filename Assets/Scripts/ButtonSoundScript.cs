using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonSoundScript : MonoBehaviour
{

    [SerializeField] private AudioMixerGroup SESound;
    [SerializeField] private float volume;
    [SerializeField] private AudioClip SFX;
    [SerializeField] private Button[] buttons;

    [SerializeField] private bool automaticallyplay = false;

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
