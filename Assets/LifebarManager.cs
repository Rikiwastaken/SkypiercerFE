using UnityEngine;
using UnityEngine.UI;

public class LifebarManager : MonoBehaviour
{

    [SerializeField] private Image LifebarFilling;
    [SerializeField] private float TargetRatio;
    [SerializeField] private float filltchangepersecond;
    [SerializeField] private float Threshold;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Mathf.Abs(LifebarFilling.fillAmount - TargetRatio) > Threshold)
        {
            if (LifebarFilling.fillAmount < TargetRatio)
            {
                LifebarFilling.fillAmount += filltchangepersecond * Time.deltaTime;
            }
            else
            {
                LifebarFilling.fillAmount -= filltchangepersecond * Time.deltaTime;
            }
        }
        else
        {
            LifebarFilling.fillAmount = TargetRatio;
        }
    }


    public void SetTargetRatio(float ratio)
    {
        TargetRatio = ratio;
    }

    public void SetTargetRatio(int currentHP, int MaxHP)
    {
        TargetRatio = (float)(currentHP) / (float)MaxHP;
    }

    public void ChangeLifeBarColorAccordingToRemaining(int remainingLifebars)
    {
        switch (remainingLifebars)
        {
            case 0:
                LifebarFilling.color = Color.green;
                break;
            case 1:
                LifebarFilling.color = Color.blue;
                break;
            case 2:
                LifebarFilling.color = Color.magenta;
                break;
            case 3:
                LifebarFilling.color = Color.yellow;
                break;
            case 4:
                LifebarFilling.color = Color.gray;
                break;
        }
    }
}
