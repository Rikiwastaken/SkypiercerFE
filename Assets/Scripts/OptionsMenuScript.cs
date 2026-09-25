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

    public Button ResMain;
    public Button ResLess;
    public Button ResMore;

    public TextMeshProUGUI fullscreentext;

    public TextMeshProUGUI BattleAnimations;

    public TextMeshProUGUI FixedGrowth;

    private SaveManager SaveManager;

    public TextMeshProUGUI musictext;
    public TextMeshProUGUI SEtext;

    public TextMeshProUGUI ResText;

    public ButtonSoundScript ButtonSoundScript;

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
        //if (SaveManager.Options.BattleAnimations)
        //{
        //    BattleAnimations.text = "Battle Animations : On";
        //}
        //else
        //{
        //    BattleAnimations.text = "Battle Animations : Off";
        //}
        BattleAnimations.text = "Battle Animations : Locked";
        if (SaveManager.Options.FixedGrowth)
        {
            FixedGrowth.text = "Fixed Growth : On";
        }
        else
        {
            FixedGrowth.text = "Fixed Growth : Off";
        }
        musictext.text = "Music : " + (int)(SaveManager.Options.musicvolume * 100);
        SEtext.text = "Sound : " + (int)(SaveManager.Options.SEVolume * 100);
        ResText.text = "Resolution : " + (int)(SaveManager.Resolutions[SaveManager.Options.ResolutionID].x) + "x" + (int)(SaveManager.Resolutions[SaveManager.Options.ResolutionID].y);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
        ManageMusicVol(currentSelected);
        ManageSEVol(currentSelected);
        ManageResolution(currentSelected);
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
        musictext.text = "Music : " + (int)(SaveManager.Options.musicvolume * 100);
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
            ButtonSoundScript.PlayButtonSFX();
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
            ButtonSoundScript.PlayButtonSFX();
        }
        SEtext.text = "Sound : " + (int)(SaveManager.Options.SEVolume * 100);
    }

    private void ManageResolution(GameObject selected)
    {
        if (selected == ResLess.gameObject)
        {

            EventSystem.current.SetSelectedGameObject(ResMain.gameObject);
            SaveManager.Options.ResolutionID--;
            if (SaveManager.Options.ResolutionID < 0)
            {
                SaveManager.Options.ResolutionID = 0;
            }
            SaveManager.SaveOptions();
        }
        if (selected == ResMore.gameObject)
        {

            EventSystem.current.SetSelectedGameObject(ResMain.gameObject);
            SaveManager.Options.ResolutionID++;
            if (SaveManager.Options.ResolutionID >= SaveManager.Resolutions.Count)
            {
                SaveManager.Options.ResolutionID = SaveManager.Resolutions.Count - 1;
            }
            SaveManager.SaveOptions();
        }
        ResText.text = "Resolution : " + (int)(SaveManager.Resolutions[SaveManager.Options.ResolutionID].x) + "x" + (int)(SaveManager.Resolutions[SaveManager.Options.ResolutionID].y);
    }


    public void ToggleFullscreen()
    {
        SaveManager.Options.Fullscreen = !Screen.fullScreen;
        SaveManager.SaveOptions();
        Screen.fullScreen = SaveManager.Options.Fullscreen;
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
        SaveManager.Options.BattleAnimations = false;
        SaveManager.SaveOptions();
        if (SaveManager.Options.BattleAnimations)
        {
            BattleAnimations.text = "Battle Animations : On";
        }
        else
        {
            BattleAnimations.text = "Battle Animations : Locked";
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
