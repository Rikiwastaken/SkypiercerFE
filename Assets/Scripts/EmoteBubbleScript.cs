using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmoteBubbleScript : MonoBehaviour
{

    public float timetomaxscale;

    public float timebeforedisappear;

    private Coroutine IncreaseCoroutine;
    private Coroutine DisappearCoroutine;

    public float elevation;

    private Camera cam;

    public List<Sprite> emojifaces;

    public Image EmojiImage;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (cam == null || !cam.gameObject.activeSelf)
        {
            cam = FindAnyObjectByType<Camera>();
        }

        transform.LookAt(cam.transform.position);

    }

    public void Initialize(int emojiID)
    {

        transform.localPosition = new Vector3(0, elevation, 0);

        gameObject.SetActive(true);
        EmojiImage.sprite = emojifaces[emojiID];

        if (IncreaseCoroutine != null)
        {
            StopCoroutine(IncreaseCoroutine);
        }

        IncreaseCoroutine = StartCoroutine(IncreasetoMaxSize());

        if (DisappearCoroutine != null)
        {
            StopCoroutine(DisappearCoroutine);
        }

        DisappearCoroutine = StartCoroutine(WaitAndDecrease());



    }

    private IEnumerator IncreasetoMaxSize()
    {
        float timeelapsed = 0;
        while (timeelapsed < timetomaxscale)
        {
            timeelapsed += Time.deltaTime;

            float ratio = (timeelapsed / timetomaxscale);

            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, ratio);

            yield return null;

        }
        transform.localScale = Vector3.one;
        IncreaseCoroutine = null;
    }

    private IEnumerator WaitAndDecrease()
    {
        float timeelapsed = Time.deltaTime;
        while (timeelapsed < timebeforedisappear)
        {
            timeelapsed += Time.deltaTime;

            yield return null;

        }

        timeelapsed = 0;
        while (timeelapsed < timetomaxscale)
        {
            timeelapsed += Time.deltaTime;

            float ratio = (timeelapsed / timetomaxscale);

            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, ratio);

            yield return null;

        }

        transform.localScale = Vector3.zero;
        DisappearCoroutine = null;
        gameObject.SetActive(false);
    }

}
