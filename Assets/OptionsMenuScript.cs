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

    private SaveManager SaveManager;

    private void Start()
    {
        SaveManager = FindAnyObjectByType<SaveManager>();
        if(SaveManager.Options.Fullscreen)
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
            SaveManager.Options.musicvolume -= 10;
            if (SaveManager.Options.musicvolume < 0)
            {
                SaveManager.Options.musicvolume = 0;
            }
            SaveManager.SaveOptions();
        }
        if (selected == MusicMore.gameObject)
        {
            EventSystem.current.SetSelectedGameObject(MusicMain.gameObject);
            SaveManager.Options.musicvolume += 10;
            if (SaveManager.Options.musicvolume > 200)
            {
                SaveManager.Options.musicvolume = 200;
            }
            SaveManager.SaveOptions();
        }
        MusicMain.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Music : "+SaveManager.Options.musicvolume;
    }

    private void ManageSEVol(GameObject selected)
    {
        if (selected == SELess.gameObject)
        {
            EventSystem.current.SetSelectedGameObject(SEMain.gameObject);
            SaveManager.Options.SEVolume -= 10;
            if (SaveManager.Options.SEVolume < 0)
            {
                SaveManager.Options.SEVolume = 0;
            }
            SaveManager.SaveOptions();
        }
        if (selected == SEMore.gameObject)
        {
            EventSystem.current.SetSelectedGameObject(SEMain.gameObject);
            SaveManager.Options.SEVolume += 10;
            if (SaveManager.Options.SEVolume > 200)
            {
                SaveManager.Options.SEVolume = 200;
            }
            SaveManager.SaveOptions();
        }
        SEMain.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Sound : " + SaveManager.Options.SEVolume;
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
}
