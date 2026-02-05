using TMPro;
using UnityEngine;

public class PhaseTextScript : MonoBehaviour
{

    public float restingXpos;
    public float speed;

    public bool moveText;

    private int timeinthemiddlecounter;
    public float timeinthemiddle;

    private TextMeshProUGUI TMP;
    private TextMeshProUGUI BackTMP;

    private TextBubbleScript textbubblescript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.localPosition = new Vector2(-restingXpos, 0);
        TMP = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        BackTMP = GetComponent<TextMeshProUGUI>();
        textbubblescript = FindAnyObjectByType<TextBubbleScript>(FindObjectsInactive.Include);
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (textbubblescript.indialogue)
        {
            return;
        }
        if (moveText)
        {
            if (transform.localPosition.x <= -100)
            {
                transform.localPosition += new Vector3(speed * Time.fixedDeltaTime, 0f, 0f);
                timeinthemiddlecounter = (int)(timeinthemiddle / Time.fixedDeltaTime);
            }
            else
            {
                if (timeinthemiddlecounter > 0)
                {
                    timeinthemiddlecounter--;
                    transform.localPosition += new Vector3(speed * Time.fixedDeltaTime / 20f, 0f, 0f);
                }
                else if (transform.localPosition.x < restingXpos)
                {
                    transform.localPosition += new Vector3(speed * Time.fixedDeltaTime, 0f, 0f);
                }
                else
                {
                    moveText = false;
                }
            }
        }
        else
        {
            transform.localPosition = new Vector2(-restingXpos, 0);
        }
    }

    public void SetupText(string currentlyplaying)
    {
        transform.localPosition = new Vector2(-restingXpos, 0);
        if (currentlyplaying == "playable")
        {
            moveText = true;
            TMP.text = "Ally Phase";
            BackTMP.text = "Ally Phase";
            TMP.color = Color.blue;
        }
        else if (currentlyplaying == "enemy")
        {
            moveText = true;
            TMP.text = "Enemy Phase";
            BackTMP.text = "Enemy Phase";
            TMP.color = Color.red;
        }
        else if (currentlyplaying == "other")
        {
            moveText = true;
            TMP.text = "Other Phase";
            BackTMP.text = "Other Phase";
            TMP.color = Color.yellow;
        }
    }

}
