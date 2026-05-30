using System.Collections;
using UnityEngine;

public class TargettingLineScript : MonoBehaviour
{

    private int numberofsections;


    private Coroutine Fadeincoroutine;

    public void InitializeLines(float timetoappear)
    {
        numberofsections = transform.childCount;
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
            }
        }
        if (Fadeincoroutine != null)
        {
            StopCoroutine(Fadeincoroutine);
        }
        Fadeincoroutine = StartCoroutine(FadeinCoroutine(timetoappear));
    }

    private IEnumerator FadeinCoroutine(float timetoappear)
    {
        float timetofinish = Time.time + timetoappear;
        while (timetofinish > Time.time)
        {
            float timeratio = 1f - (timetofinish - Time.time) / timetoappear;

            for (int i = 0; i < (int)(numberofsections * timeratio); i++)
            {
                if (i < transform.childCount)
                {
                    if (!transform.GetChild(i).gameObject.activeSelf)
                    {
                        transform.GetChild(i).gameObject.SetActive(true);
                    }
                }
            }
            yield return null;
        }
        Fadeincoroutine = null;
    }

}
