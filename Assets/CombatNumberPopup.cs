using System.Collections;
using TMPro;
using UnityEngine;

public class CombatNumberPopup : MonoBehaviour
{

    private Camera Camera;
    public TextMeshProUGUI TMPUGUI;

    public Vector3 offset;

    public float durationofText;

    private Vector3 BasePos;

    private float whentodispawn;

    public float randomoffset;

    // Update is called once per frame
    void Update()
    {
        transform.forward = Camera.transform.forward;
        if (Time.time > whentodispawn)
        {
            Color newcolor = TMPUGUI.color;
            newcolor.a -= Time.deltaTime;
            if (newcolor.a <= 0)
            {
                Destroy(gameObject);
            }
            else
            {
                TMPUGUI.color = newcolor;
            }
        }
    }

    private IEnumerator MoveCoroutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < durationofText)
        {
            elapsedTime += Time.deltaTime;

            float ratio = elapsedTime / durationofText;

            transform.position = Vector2.Lerp(BasePos, BasePos + offset, ratio);

            yield return null; // wait for next frame
        }
    }

    public void InitializeTMP(string text, Vector3 positionwheretospawn, bool iscritical, bool ishealing)
    {
        Camera = GameObject.Find("Main Camera").GetComponent<Camera>();
        transform.position = positionwheretospawn + offset + new Vector3(Random.Range(-randomoffset, randomoffset), 0, Random.Range(-randomoffset, randomoffset));
        BasePos = transform.position;

        TMPUGUI.text = text;
        if (ishealing)
        {
            TMPUGUI.color = Color.green;
        }
        else if (iscritical)
        {
            TMPUGUI.color = Color.yellow;
        }
        else
        {
            TMPUGUI.color = Color.white;
        }

        whentodispawn = Time.time + durationofText;

        StartCoroutine(MoveCoroutine());
    }

}
