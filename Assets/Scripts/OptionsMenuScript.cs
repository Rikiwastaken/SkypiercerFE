using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OptionsMenuScript : MonoBehaviour
{

    [SerializeField] private Button MusicMain;
    [SerializeField] private Button MusicLess;
    [SerializeField] private Button MusicMore;

    [SerializeField] private Button SEMain;
    [SerializeField] private Button SELess;
    [SerializeField] private Button SEMore;

    [SerializeField] private Button ResMain;
    [SerializeField] private Button ResLess;
    [SerializeField] private Button ResMore;

    [SerializeField] private TextMeshProUGUI fullscreentext;

    [SerializeField] private TextMeshProUGUI BattleAnimations;

    private SaveManager SaveManager;

    [SerializeField] private TextMeshProUGUI musictext;
    [SerializeField] private TextMeshProUGUI SEtext;

    [SerializeField] private TextMeshProUGUI ResText;

    [SerializeField] private ButtonSoundScript ButtonSoundScript;

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
        SaveManager.Options.BattleAnimations = !SaveManager.Options.BattleAnimations;
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


}
