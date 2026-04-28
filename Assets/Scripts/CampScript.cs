using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static TextBubbleScript;
using static UnitScript;

public class CampScript : MonoBehaviour
{

    SaveManager saveManager;

    public Transform BaseMenu;

    [Serializable]
    public class StartDialogue
    {
        public int Chapter;
        public List<TextBubbleInfo> Dialogue;
    }

    public List<StartDialogue> StartDialogueList;

    private bool previousindialogue;

    public Transform SaveButtonList;

    public Transform SittingGO;
    public Transform Sitting2GO;
    public Transform Sitting3GO;
    public Transform LeaningGO;

    private Animator SittingCharacter;
    private Animator Sitting2Character;
    private Animator Sitting3Character;
    private Animator LeaningCharacter;

    public GameObject CharacterPrefab;
    public RuntimeAnimatorController CampCharacterController;

    private TextBubbleScript textBubbleScript;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        saveManager = SaveManager.instance;

        if (DataScript.instance == null)
        {
            SceneManager.LoadScene("FirstScene");
        }
        EventSystem.current.SetSelectedGameObject(BaseMenu.GetChild(0).gameObject);

        textBubbleScript = FindAnyObjectByType<TextBubbleScript>(FindObjectsInactive.Include);

        foreach (StartDialogue startdialogue in StartDialogueList)
        {
            if (startdialogue.Chapter == SaveManager.instance.currentchapter)
            {
                textBubbleScript.InitializeDialogue(startdialogue.Dialogue);
                break;
            }
        }

        List<Button> buttons = new List<Button>();
        for (int i = 0; i < SaveButtonList.childCount - 1; i++)
        {
            buttons.Add(SaveButtonList.GetChild(i).GetComponent<Button>());
        }
        saveManager.InitializeSaveButtons(buttons);


        InitializeCharacters();

        SittingCharacter.SetBool("Sitting", true);
        Sitting2Character.SetBool("Sitting2", true);
        Sitting3Character.SetBool("Sitting3", true);
        LeaningCharacter.SetBool("Leaning", true);
    }

    private void Update()
    {

        if (textBubbleScript.indialogue && BaseMenu.gameObject.activeSelf)
        {
            BaseMenu.gameObject.SetActive(false);
        }

        if (previousindialogue && !textBubbleScript.indialogue)
        {
            if (!BaseMenu.gameObject.activeSelf)
            {
                BaseMenu.gameObject.SetActive(true);
            }
        }

        if (EventSystem.current.currentSelectedGameObject == null || !EventSystem.current.currentSelectedGameObject.activeSelf || !EventSystem.current.currentSelectedGameObject.transform.parent.gameObject.activeSelf)
        {
            EventSystem.current.SetSelectedGameObject(BaseMenu.GetChild(0).gameObject);
        }


        previousindialogue = textBubbleScript.indialogue;
    }

    public void LoadNextChapter()
    {


        SceneLoader.instance.LoadScene("WorldMap");

    }



    private void InitializeCharacters()
    {
        List<Character> allcharacters = DataScript.instance.PlayableCharacterList;
        List<Character> characterstouse = new List<Character>();
        foreach (Character chara in allcharacters)
        {
            if (chara.playableStats.unlocked)
            {
                characterstouse.Add(chara);
            }
        }

        SittingCharacter = CreateAndApplyAnimator(SittingGO, characterstouse);
        Sitting2Character = CreateAndApplyAnimator(Sitting2GO, characterstouse);
        Sitting3Character = CreateAndApplyAnimator(Sitting3GO, characterstouse);
        LeaningCharacter = CreateAndApplyAnimator(LeaningGO, characterstouse);


    }

    private Animator CreateAndApplyAnimator(Transform Parent, List<Character> list)
    {
        int modelIDtoinstantiate = GetRandomModelIDFromList(list);
        GameObject Character = Instantiate(CharacterPrefab.GetComponent<UnitScript>().ModelList[modelIDtoinstantiate].wholeModel);

        Character.transform.parent = Parent;
        Character.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        Animator animator = Character.GetComponentInChildren<Animator>();
        animator.runtimeAnimatorController = CampCharacterController;

        return animator;
    }
    private int GetRandomModelIDFromList(List<Character> list)
    {

        Character character = list[UnityEngine.Random.Range(0, list.Count)];

        list.Remove(character);

        return character.modelID;
    }

}
