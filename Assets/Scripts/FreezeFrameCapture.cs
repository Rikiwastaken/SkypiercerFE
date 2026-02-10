using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnitScript;

public class FreezeFrameCapture : MonoBehaviour
{
    public static FreezeFrameCapture instance;

    [Header("BaseVariables")]
    public Camera cam;
    public RenderTexture RenderTextureObj;
    public Texture2D captured;
    public Image left;
    public Image right;
    public float splitDistance = 500f;
    public float splitDuration = 0.4f;
    public GameObject BackgroundImage;
    public GameObject CharacterSprite;
    private Vector2 CharacterSpriteBasePos;
    private Vector2 BackgroundImageBasePos;
    public RectTransform TextHolder;
    public InputActionReference activate;
    public float timebeforesplit;
    public RectTransform Canvas;

    public bool playsplit;
    private GameObject MusicGO;
    public bool ShowingLevelUp;

    public AudioClip LevelUpJingleClip;

    [Header("NameTextVariables")]
    public TextMeshProUGUI CharacterName;
    public TextMeshProUGUI CharacterNameBG;
    public float lefttextsizeratio;
    public float righttextsizeratio;
    public Vector2 BaseNamePosition;
    public Vector2 TargetNamePosition;
    public Vector2 NameBGOffset;

    [Header("LevelUpTextVariables")]
    public TextMeshProUGUI LvlUpText;
    public TextMeshProUGUI LvlUpTextBG;
    public float lefttextsizeratioLvlUp;
    public float righttextsizeratioLvlUp;
    public Vector2 BaseLvlUpPosition;
    public Vector2 TargetLvlUpPosition;
    public Vector2 LvlUpBGOffset;

    [Header("StatTextVariables")]
    public List<TextMeshProUGUI> StatTexts;
    public List<TextMeshProUGUI> StatTextBG;
    public RectTransform StatTextHolder;
    public Vector2 BaseStatPosition;
    public Vector2 TargetStatPosition;
    public Vector2 OffsetPerIndex;
    public Vector2 BGOffSet;
    public float timebetweenStats;

    [Header("ContinueVariables")]
    public bool continueAvailable;
    public TextMeshProUGUI Continuetxt;
    public TextMeshProUGUI ContinueBGtxt;
    public Vector2 BaseContinuePosition;
    public Vector2 TargetContinuePosition;
    public Vector2 ContinueOffSet;
    public float leftContinueratio;
    public float rightContinueratio;






    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        CharacterSpriteBasePos = CharacterSprite.GetComponent<RectTransform>().anchoredPosition;
        BackgroundImageBasePos = BackgroundImage.GetComponent<RectTransform>().anchoredPosition;
    }

    private void Update()
    {
        if (playsplit)
        {
            playsplit = false;
            List<int> list = new List<int>();
            list.Add(0);
            list.Add(0);
            list.Add(1);
            list.Add(0);
            list.Add(0);
            list.Add(0);
            list.Add(0);
            list.Add(0);
            list.Add(0);

            PlayFullAnimation(DataScript.instance.PlayableCharacterList[1], list);
        }

        if (ShowingLevelUp && InputManager.instance.cancelpressed)
        {
            StopAllCoroutines();
            StartCoroutine(Close());
        }

        if (continueAvailable && activate.ToInputAction().IsPressed())
        {
            StopAllCoroutines();
            StartCoroutine(Close());
        }

    }


    public void PlayFullAnimation(Character characterWhoLeveledUp, List<int> levelip)
    {
        MusicGO = MusicManager.instance.PlaySFX(LevelUpJingleClip);
        StopCoroutine(FullFreezeFrame(null, null));
        StopCoroutine(SplitCoroutine());
        StartCoroutine(FullFreezeFrame(characterWhoLeveledUp, levelip));
    }


    public IEnumerator FullFreezeFrame(Character characterWhoLeveledUp, List<int> levelip)
    {
        TextHolder.anchoredPosition = Vector2.zero;
        CharacterSprite.GetComponent<RectTransform>().anchoredPosition = CharacterSpriteBasePos;
        BackgroundImage.GetComponent<RectTransform>().anchoredPosition = BackgroundImageBasePos;
        ShowingLevelUp = true;
        Show(Capture());
        BackgroundImage.SetActive(true);
        CharacterSprite.SetActive(true);
        CharacterName.gameObject.SetActive(true);
        CharacterName.rectTransform.anchoredPosition = BaseNamePosition;
        CharacterNameBG.gameObject.SetActive(true);
        CharacterNameBG.rectTransform.anchoredPosition = BaseNamePosition + NameBGOffset;

        LvlUpText.gameObject.SetActive(true);
        LvlUpText.rectTransform.anchoredPosition = BaseLvlUpPosition;
        LvlUpTextBG.gameObject.SetActive(true);
        LvlUpTextBG.rectTransform.anchoredPosition = BaseLvlUpPosition;

        StatTextHolder.gameObject.SetActive(true);
        StatTextHolder.anchoredPosition = BaseStatPosition;

        Continuetxt.gameObject.SetActive(true);
        Continuetxt.rectTransform.anchoredPosition = BaseContinuePosition;
        ContinueBGtxt.gameObject.SetActive(true);
        ContinueBGtxt.rectTransform.anchoredPosition = BaseContinuePosition + ContinueOffSet;




        CharacterSprite.GetComponent<Image>().sprite = DataScript.instance.DialogueSpriteList[characterWhoLeveledUp.ID];


        ApplyDistortion(CharacterName, characterWhoLeveledUp.name, lefttextsizeratio, righttextsizeratio);
        ApplyDistortion(CharacterNameBG, characterWhoLeveledUp.name, lefttextsizeratio, righttextsizeratio);

        ApplyDistortion(LvlUpText, "Level " + characterWhoLeveledUp.level, lefttextsizeratioLvlUp, righttextsizeratioLvlUp);
        ApplyDistortion(LvlUpTextBG, "Level " + characterWhoLeveledUp.level, lefttextsizeratioLvlUp, righttextsizeratioLvlUp);

        ApplyDistortion(LvlUpText, "Level " + characterWhoLeveledUp.level, lefttextsizeratioLvlUp, righttextsizeratioLvlUp);
        ApplyDistortion(LvlUpTextBG, "Level " + characterWhoLeveledUp.level, lefttextsizeratioLvlUp, righttextsizeratioLvlUp);

        ApplyDistortion(Continuetxt, "continue", leftContinueratio, rightContinueratio);
        ApplyDistortion(ContinueBGtxt, "continue", leftContinueratio, rightContinueratio);


        string StatIncreaseTxt = "";
        if (levelip.Count >= 7)
        {
            if (levelip[0] == 0)
            {
                StatIncreaseTxt = "HP : " + characterWhoLeveledUp.AjustedStats.HP + " <sprite=16> " + characterWhoLeveledUp.AjustedStats.HP;
            }
            else
            {
                StatIncreaseTxt = "HP : " + (characterWhoLeveledUp.AjustedStats.HP - levelip[0]) + " <sprite=17> <color=yellow>" + characterWhoLeveledUp.AjustedStats.HP;
            }
            StatTexts[0].text = StatIncreaseTxt;
            string statincreaseBG = StatIncreaseTxt;
            statincreaseBG = statincreaseBG.Replace("<sprite=17>", "<sprite=18>");
            statincreaseBG = statincreaseBG.Replace("<sprite=16>", "<sprite=18>");
            statincreaseBG = statincreaseBG.Replace("<color=yellow>", "");
            StatTextBG[0].text = statincreaseBG;
            if (levelip[1] == 0)
            {
                StatIncreaseTxt = "Str : " + characterWhoLeveledUp.AjustedStats.Strength + " <sprite=16> " + characterWhoLeveledUp.AjustedStats.Strength;
            }
            else
            {
                StatIncreaseTxt = "Str : " + (characterWhoLeveledUp.AjustedStats.Strength - levelip[1]) + " <sprite=17> <color=yellow>" + characterWhoLeveledUp.AjustedStats.Strength;
            }
            StatTexts[1].text = StatIncreaseTxt;
            statincreaseBG = StatIncreaseTxt;
            statincreaseBG = statincreaseBG.Replace("<sprite=17>", "<sprite=18>");
            statincreaseBG = statincreaseBG.Replace("<sprite=16>", "<sprite=18>");
            statincreaseBG = statincreaseBG.Replace("<color=yellow>", "");
            StatTextBG[1].text = statincreaseBG;
            if (levelip[2] == 0)
            {
                StatIncreaseTxt = "Psy : " + characterWhoLeveledUp.AjustedStats.Psyche + " <sprite=16> " + characterWhoLeveledUp.AjustedStats.Psyche;
            }
            else
            {
                StatIncreaseTxt = "Psy : " + (characterWhoLeveledUp.AjustedStats.Psyche - levelip[2]) + " <sprite=17> <color=yellow>" + characterWhoLeveledUp.AjustedStats.Psyche;
            }
            StatTexts[2].text = StatIncreaseTxt;
            statincreaseBG = StatIncreaseTxt;
            statincreaseBG = statincreaseBG.Replace("<sprite=17>", "<sprite=18>");
            statincreaseBG = statincreaseBG.Replace("<sprite=16>", "<sprite=18>");
            statincreaseBG = statincreaseBG.Replace("<color=yellow>", "");
            StatTextBG[2].text = statincreaseBG;
            if (levelip[3] == 0)
            {
                StatIncreaseTxt = "Def : " + characterWhoLeveledUp.AjustedStats.Defense + " <sprite=16> " + characterWhoLeveledUp.AjustedStats.Defense;
            }
            else
            {
                StatIncreaseTxt = "Def : " + (characterWhoLeveledUp.AjustedStats.Defense - levelip[3]) + " <sprite=17> <color=yellow>" + characterWhoLeveledUp.AjustedStats.Defense;
            }
            StatTexts[3].text = StatIncreaseTxt;
            statincreaseBG = StatIncreaseTxt;
            statincreaseBG = statincreaseBG.Replace("<sprite=17>", "<sprite=18>");
            statincreaseBG = statincreaseBG.Replace("<sprite=16>", "<sprite=18>");
            statincreaseBG = statincreaseBG.Replace("<color=yellow>", "");
            StatTextBG[3].text = statincreaseBG;
            if (levelip[4] == 0)
            {
                StatIncreaseTxt = "Res : " + characterWhoLeveledUp.AjustedStats.Resistance + " <sprite=16> " + characterWhoLeveledUp.AjustedStats.Resistance;
            }
            else
            {
                StatIncreaseTxt = "Res : " + (characterWhoLeveledUp.AjustedStats.Resistance - levelip[4]) + " <sprite=17> <color=yellow>" + characterWhoLeveledUp.AjustedStats.Resistance;
            }
            StatTexts[4].text = StatIncreaseTxt;
            statincreaseBG = StatIncreaseTxt;
            statincreaseBG = statincreaseBG.Replace("<sprite=17>", "<sprite=18>");
            statincreaseBG = statincreaseBG.Replace("<sprite=16>", "<sprite=18>");
            statincreaseBG = statincreaseBG.Replace("<color=yellow>", "");
            StatTextBG[4].text = statincreaseBG;
            if (levelip[5] == 0)
            {
                StatIncreaseTxt = "Spd : " + characterWhoLeveledUp.AjustedStats.Speed + " <sprite=16> " + characterWhoLeveledUp.AjustedStats.Speed;
            }
            else
            {
                StatIncreaseTxt = "Spd : " + (characterWhoLeveledUp.AjustedStats.Speed - levelip[5]) + " <sprite=17> <color=yellow>" + characterWhoLeveledUp.AjustedStats.Speed;
            }
            StatTexts[5].text = StatIncreaseTxt;
            statincreaseBG = StatIncreaseTxt;
            statincreaseBG = statincreaseBG.Replace("<sprite=17>", "<sprite=18>");
            statincreaseBG = statincreaseBG.Replace("<sprite=16>", "<sprite=18>");
            statincreaseBG = statincreaseBG.Replace("<color=yellow>", "");
            StatTextBG[5].text = statincreaseBG;
            if (levelip[6] == 0)
            {
                StatIncreaseTxt = "Dex : " + characterWhoLeveledUp.AjustedStats.Dexterity + " <sprite=16> " + characterWhoLeveledUp.AjustedStats.Dexterity;
            }
            else
            {
                StatIncreaseTxt = "Dex : " + (characterWhoLeveledUp.AjustedStats.Dexterity - levelip[6]) + " <sprite=17> <color=yellow>" + characterWhoLeveledUp.AjustedStats.Dexterity;
            }
            StatTexts[6].text = StatIncreaseTxt;
            statincreaseBG = StatIncreaseTxt;
            statincreaseBG = statincreaseBG.Replace("<sprite=17>", "<sprite=18>");
            statincreaseBG = statincreaseBG.Replace("<sprite=16>", "<sprite=18>");
            statincreaseBG = statincreaseBG.Replace("<color=yellow>", "");
            StatTextBG[6].text = statincreaseBG;
        }


        left.rectTransform.anchoredPosition = Vector2.zero;
        right.rectTransform.anchoredPosition = Vector2.zero;
        left.rectTransform.sizeDelta = new Vector2(left.rectTransform.sizeDelta.x / 2, left.rectTransform.sizeDelta.y);
        right.rectTransform.sizeDelta = new Vector2(right.rectTransform.sizeDelta.x / 2, right.rectTransform.sizeDelta.y);


        for (int i = 0; i < 7; i++)
        {
            StatTexts[i].rectTransform.anchoredPosition = BaseStatPosition + OffsetPerIndex * i;
            StartCoroutine(MoveStatCoroutine(i, i * timebetweenStats));
        }
        StartCoroutine(MoveContinueCoroutine(9f * timebetweenStats));
        yield return SplitCoroutine();
    }


    public Texture2D Capture()
    {
        cam.targetTexture = RenderTextureObj;
        cam.Render();

        RenderTexture.active = RenderTextureObj;
        captured = new Texture2D(RenderTextureObj.width, RenderTextureObj.height, TextureFormat.RGB24, false);
        captured.ReadPixels(new Rect(0, 0, RenderTextureObj.width, RenderTextureObj.height), 0, 0);
        captured.Apply();

        cam.targetTexture = null;
        RenderTexture.active = null;

        return captured;
    }

    public IEnumerator Close()
    {
        Vector2 leftbasepos = left.rectTransform.anchoredPosition;
        Vector2 rightbasepos = right.rectTransform.anchoredPosition;

        Vector2 BackgroundBasePos = BackgroundImage.GetComponent<RectTransform>().anchoredPosition;
        Vector2 CharacterSpriteBasePos = CharacterSprite.GetComponent<RectTransform>().anchoredPosition;

        Vector2 TextHolderPos = TextHolder.anchoredPosition;

        float t = 0f;

        //yield return new WaitForSeconds(1);

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / splitDuration; // unscaled pour marcher même si Time.timeScale = 0

            float eased = EaseOutCubic(t);

            left.rectTransform.anchoredPosition = Vector2.Lerp(leftbasepos, leftbasepos + new Vector2(0, 1000f), eased);
            right.rectTransform.anchoredPosition = Vector2.Lerp(rightbasepos, rightbasepos + new Vector2(0, 1000f), eased);
            TextHolder.anchoredPosition = Vector2.Lerp(TextHolderPos, TextHolderPos + new Vector2(0, 1000f), eased);
            BackgroundImage.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(BackgroundBasePos, BackgroundBasePos - new Vector2(0, 1000f), eased);
            CharacterSprite.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(CharacterSpriteBasePos, CharacterSpriteBasePos - new Vector2(0, 1000f), eased);

            yield return null;
        }


        if (MusicGO != null)
        {
            StartCoroutine(LowerMusic(MusicGO));
        }

        ShowingLevelUp = false;
        continueAvailable = false;
        BackgroundImage.SetActive(false);
        CharacterSprite.SetActive(false);
        CharacterName.gameObject.SetActive(false);
        CharacterNameBG.gameObject.SetActive(false);
        for (int i = 0; i < 7; i++)
        {
            StatTexts[i].rectTransform.anchoredPosition = BaseStatPosition + OffsetPerIndex * i;
            StatTextBG[i].rectTransform.anchoredPosition = BaseStatPosition + OffsetPerIndex * i + BGOffSet;
        }
        StatTextHolder.gameObject.SetActive(false);
        LvlUpText.gameObject.SetActive(false);
        LvlUpTextBG.gameObject.SetActive(false);

        Continuetxt.gameObject.SetActive(false);
        ContinueBGtxt.gameObject.SetActive(false);
        left.transform.localPosition = new Vector2(-1000, 0);
        right.transform.localPosition = new Vector2(1000, 0);
    }

    IEnumerator LowerMusic(GameObject musicGO)
    {
        float t = 0f;
        float basevol = musicGO.GetComponent<AudioSource>().volume;



        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / splitDuration; // unscaled pour marcher même si Time.timeScale = 0

            float eased = EaseOutCubic(t);
            if (musicGO != null)
            {
                musicGO.GetComponent<AudioSource>().volume = eased * basevol;
            }


            yield return null;
        }
        if (musicGO != null)
        {
            Destroy(musicGO);
        }
    }

    void SetupRectTransforms(Texture2D tex)
    {


        left.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        left.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        right.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        right.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

        left.rectTransform.pivot = new Vector2(1f, 0.5f);
        right.rectTransform.pivot = new Vector2(0f, 0.5f);

        // Preserve texture aspect ratio
        float texRatio = (float)tex.width / tex.height;
        float canvasRatio = Canvas.rect.width / Canvas.rect.height;

        Vector2 size;
        if (texRatio > canvasRatio)
        {
            // Texture is wider than canvas, fit width
            float height = Canvas.rect.width / texRatio;
            size = new Vector2(Canvas.rect.width, height);
        }
        else
        {
            // Texture is taller than canvas, fit height
            float width = Canvas.rect.height * texRatio;
            size = new Vector2(width, Canvas.rect.height);
        }

        left.rectTransform.sizeDelta = size;
        right.rectTransform.sizeDelta = size;

        left.rectTransform.anchoredPosition = Vector2.zero;
        right.rectTransform.anchoredPosition = Vector2.zero;
    }

    public void Show(Texture2D tex)
    {

        tex = ConvertToGrayscale(tex);

        SetupRectTransforms(tex);

        int halfWidth = tex.width / 2;

        Sprite leftSprite = Sprite.Create(
            tex,
            new Rect(0, 0, halfWidth, tex.height),
            new Vector2(1f, 0.5f)
        );

        Sprite rightSprite = Sprite.Create(
            tex,
            new Rect(halfWidth, 0, halfWidth, tex.height),
            new Vector2(0f, 0.5f)
        );

        left.sprite = leftSprite;
        right.sprite = rightSprite;

        // Keep anchors centered to preserve aspect ratio
        left.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        left.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        right.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        right.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
    }



    public void Split(float distance)
    {
        left.rectTransform.anchoredPosition = new Vector2(-distance, 0);
        right.rectTransform.anchoredPosition = new Vector2(distance, 0);
    }

    Texture2D ConvertToGrayscale(Texture2D original)
    {
        Texture2D gray = new Texture2D(original.width, original.height, TextureFormat.RGBA32, false);
        Color[] pixels = original.GetPixels();

        for (int i = 0; i < pixels.Length; i++)
        {
            float grayValue = pixels[i].grayscale; // Unity's built-in grayscale calculation
            pixels[i] = new Color(grayValue, grayValue, grayValue, pixels[i].a);
        }

        gray.SetPixels(pixels);
        gray.Apply();
        return gray;
    }

    IEnumerator SplitCoroutine()
    {
        float t = 0f;

        Vector2 leftStart = Vector2.zero;
        Vector2 rightStart = Vector2.zero;

        Vector2 leftEnd = new Vector2(-splitDistance, 0);
        Vector2 rightEnd = new Vector2(splitDistance, 0);

        yield return new WaitForSeconds(timebeforesplit);

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / splitDuration; // unscaled pour marcher même si Time.timeScale = 0

            float eased = EaseOutCubic(t);

            left.rectTransform.anchoredPosition = Vector2.Lerp(leftStart, leftEnd, eased);
            right.rectTransform.anchoredPosition = Vector2.Lerp(rightStart, rightEnd, eased);

            CharacterName.rectTransform.anchoredPosition = Vector2.Lerp(BaseNamePosition, TargetNamePosition, eased);
            CharacterNameBG.rectTransform.anchoredPosition = Vector2.Lerp(BaseNamePosition + NameBGOffset, TargetNamePosition + NameBGOffset, eased);

            LvlUpText.rectTransform.anchoredPosition = Vector2.Lerp(BaseLvlUpPosition, TargetLvlUpPosition, eased);
            LvlUpTextBG.rectTransform.anchoredPosition = Vector2.Lerp(BaseLvlUpPosition + LvlUpBGOffset, TargetLvlUpPosition + LvlUpBGOffset, eased);

            yield return null;
        }

        left.rectTransform.anchoredPosition = leftEnd;
        right.rectTransform.anchoredPosition = rightEnd;
    }


    IEnumerator MoveStatCoroutine(int ID, float durationbeforemove)
    {
        float t = 0f;

        yield return new WaitForSeconds(durationbeforemove);

        yield return new WaitForSeconds(timebeforesplit);

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / splitDuration; // unscaled pour marcher même si Time.timeScale = 0

            float eased = EaseOutCubic(t);

            StatTexts[ID].rectTransform.anchoredPosition = Vector2.Lerp(BaseStatPosition + OffsetPerIndex * ID, TargetStatPosition + OffsetPerIndex * ID, eased);
            StatTextBG[ID].rectTransform.anchoredPosition = Vector2.Lerp(BaseStatPosition + OffsetPerIndex * ID + BGOffSet, TargetStatPosition + OffsetPerIndex * ID + BGOffSet, eased);

            yield return null;
        }
    }

    IEnumerator MoveContinueCoroutine(float durationbeforemove)
    {
        float t = 0f;

        yield return new WaitForSeconds(durationbeforemove);

        yield return new WaitForSeconds(timebeforesplit);
        continueAvailable = true;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / splitDuration; // unscaled pour marcher même si Time.timeScale = 0

            float eased = EaseOutCubic(t);

            Continuetxt.rectTransform.anchoredPosition = Vector2.Lerp(BaseContinuePosition, TargetContinuePosition, eased);
            ContinueBGtxt.rectTransform.anchoredPosition = Vector2.Lerp(BaseContinuePosition + ContinueOffSet, TargetContinuePosition + ContinueOffSet, eased);

            yield return null;
        }

    }

    float EaseOutCubic(float t)
    {
        t = Mathf.Clamp01(t);
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    public static void ApplyDistortion(TMP_Text textMesh, string newText, float leftScale = 1f, float rightScale = 3f)
    {
        if (textMesh == null)
        {
            Debug.LogWarning("TMP_Text est null !");
            return;
        }

        // Appliquer le nouveau texte
        textMesh.text = newText;

        // Forcer la génération du mesh
        textMesh.ForceMeshUpdate();
        TMP_TextInfo textInfo = textMesh.textInfo;

        float textWidth = textMesh.rectTransform.rect.width;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible)
                continue;

            int vertexIndex = textInfo.characterInfo[i].vertexIndex;
            int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            // Position X du centre du caractère (normalisée 0 à 1)
            float charCenterX = (textInfo.characterInfo[i].bottomLeft.x + textInfo.characterInfo[i].topRight.x) / 2;
            float normalizedX = Mathf.InverseLerp(0, textWidth, charCenterX);

            // Calcul du facteur d'échelle interpolé entre gauche et droite pour X et Y
            float scaleX = Mathf.Lerp(leftScale, rightScale, normalizedX);
            float scaleY = Mathf.Lerp(leftScale, rightScale, normalizedX); // même logique pour Y

            // Centre du caractère
            Vector3 charCenter = (vertices[vertexIndex] + vertices[vertexIndex + 2]) / 2;

            // Appliquer la mise à l'échelle sur les 4 vertices
            for (int j = 0; j < 4; j++)
            {
                vertices[vertexIndex + j] = charCenter + Vector3.Scale(vertices[vertexIndex + j] - charCenter, new Vector3(scaleX, scaleY, 1));
            }
        }

        // Appliquer les modifications sur le mesh
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            textMesh.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }

}
