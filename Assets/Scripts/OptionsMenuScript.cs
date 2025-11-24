using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OptionsMenuScript : MonoBehaviour
{

    public Button MusicMain;
    public Button MusicLess;
    public Button MusicMore;

    public Button SEMain;
    public Button SELess;
    public Button SEMore;

    public TextMeshProUGUI fullscreentext;

    public TextMeshProUGUI BattleAnimations;

    public TextMeshProUGUI FixedGrowth;

    private SaveManager SaveManager;

    private void Start()
    {
        SaveManager = SaveManager.instance;
        if (SaveManager.Options.Fullscreen)
        {
            fullscreentext.text = "Fullscreen : On";
        }
        else
        {
            fullscreentext.text = "Fullscreen : Off";
        }
        if (SaveManager.Options.BattleAnimations)
        {
            BattleAnimations.text = "Battle Animations : On";
        }
        else
        {
            BattleAnimations.text = "Battle Animations : Off";
        }
        if (SaveManager.Options.FixedGrowth)
        {
            FixedGrowth.text = "Fixed Growth : On";
        }
        else
        {
            FixedGrowth.text = "Fixed Growth : Off";
        }
        MusicMain.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Music : " + (int)(SaveManager.Options.musicvolume * 100);
        SEMain.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Sound : " + (int)(SaveManager.Options.SEVolume * 100);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
        ManageMusicVol(currentSelected);
        ManageSEVol(currentSelected);
    }

    private void ManageMusicVol(GameObject selected)
    {
        if (selected == MusicLess.gameObject)
        {

            EventSystem.current.SetSelectedGameObject(MusicMain.gameObject);
            SaveManager.Options.musicvolume -= 0.1f;
            if (SaveManager.Options.musicvolume < 0.000001f)
            {
                SaveManager.Options.musicvolume = 0.000001f;
            }
            SaveManager.SaveOptions();
        }
        if (selected == MusicMore.gameObject)
        {

            EventSystem.current.SetSelectedGameObject(MusicMain.gameObject);
            SaveManager.Options.musicvolume += 0.1f;
            if (SaveManager.Options.musicvolume > 2.000001f)
            {
                SaveManager.Options.musicvolume = 2.000001f;
            }
            SaveManager.SaveOptions();
        }
        MusicMain.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Music : " + (int)(SaveManager.Options.musicvolume * 100);
    }

    private void ManageSEVol(GameObject selected)
    {
        if (selected == SELess.gameObject)
        {

            EventSystem.current.SetSelectedGameObject(SEMain.gameObject);
            SaveManager.Options.SEVolume -= 0.1f;
            if (SaveManager.Options.SEVolume < 0.000001f)
            {
                SaveManager.Options.SEVolume = 0.000001f;
            }
            SaveManager.SaveOptions();
        }
        if (selected == SEMore.gameObject)
        {

            EventSystem.current.SetSelectedGameObject(SEMain.gameObject);
            SaveManager.Options.SEVolume += 0.1f;
            if (SaveManager.Options.SEVolume > 2.000001f)
            {
                SaveManager.Options.SEVolume = 2.000001f;
            }
            SaveManager.SaveOptions();
        }
        SEMain.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Sound : " + (int)(SaveManager.Options.SEVolume * 100);
    }


    public void ToggleFullscreen()
    {
        SaveManager.Options.Fullscreen = !Screen.fullScreen;
        SaveManager.SaveOptions();
        if (SaveManager.Options.Fullscreen)
        {
            fullscreentext.text = "Fullscreen : On";
        }
        else
        {
            fullscreentext.text = "Fullscreen : Off";
        }
    }

    public void TogglebattleAnimations()
    {
        SaveManager.Options.BattleAnimations = !SaveManager.Options.BattleAnimations;
        SaveManager.SaveOptions();
        if (SaveManager.Options.BattleAnimations)
        {
            BattleAnimations.text = "Battle Animations : On";
        }
        else
        {
            BattleAnimations.text = "Battle Animations : Off";
        }
    }

    public void ToggleFixedGrowth()
    {
        SaveManager.Options.FixedGrowth = !SaveManager.Options.FixedGrowth;
        SaveManager.SaveOptions();
        if (SaveManager.Options.FixedGrowth)
        {
            FixedGrowth.text = "Fixed Growth : On";
        }
        else
        {
            FixedGrowth.text = "Fixed Growth : Off";
        }
    }
}
