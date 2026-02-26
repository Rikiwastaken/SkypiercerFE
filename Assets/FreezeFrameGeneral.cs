using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FreezeFrameGeneral : MonoBehaviour
{

    public static FreezeFrameGeneral instance;

    [Header("BaseVariables")]
    public Camera cam;
    public RenderTexture RenderTextureObj;
    public Texture2D captured;
    public Image image;
    public Image imageBG;
    public RectTransform Canvas;

    [Header("MoveVariables")]
    public float timetorotate;
    public float rotation;
    public float targetscale;

    private void Awake()
    {
        instance = this;
    }

    private Texture2D Capture()
    {
        cam.targetTexture = RenderTextureObj;
        cam.Render();

        RenderTexture.active = RenderTextureObj;
        captured = new Texture2D(RenderTextureObj.width, RenderTextureObj.height, TextureFormat.RGBA32, false, false);
        captured.ReadPixels(new Rect(0, 0, RenderTextureObj.width, RenderTextureObj.height), 0, 0);
        captured.Apply();

        cam.targetTexture = null;
        RenderTexture.active = null;

        return captured;
    }

    private void SetupRectTransforms(Texture2D tex)
    {


        image.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        image.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

        image.rectTransform.pivot = new Vector2(0.5f, 0.5f);

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

        image.rectTransform.sizeDelta = size;

        image.rectTransform.anchoredPosition = Vector2.zero;
    }

    IEnumerator MoveCamera()
    {
        float elapsed = 0f;

        Quaternion startCamRot = image.rectTransform.localRotation;
        Quaternion targetCamRot = Quaternion.Euler(image.rectTransform.localRotation.eulerAngles + new Vector3(0, 0, rotation));

        Vector3 baseScale = image.rectTransform.localScale;
        Vector3 targetScale = Vector3.one * targetscale;


        while (elapsed < timetorotate)
        {
            elapsed += Time.deltaTime;
            float time = elapsed / timetorotate;

            // Smooth interpolation (ease in/out)
            time = Mathf.SmoothStep(0f, 1f, time);



            image.rectTransform.localRotation = Quaternion.Slerp(startCamRot, targetCamRot, time);

            image.rectTransform.localScale = Vector3.Lerp(baseScale, targetScale, time);

            yield return null;
        }

        // Snap exactly to final values (avoids floating precision issues)
        image.rectTransform.localRotation = targetCamRot;
        image.rectTransform.localScale = targetScale;



    }

    public void Show()
    {
        image.rectTransform.localScale = Vector3.one;
        image.rectTransform.localRotation = Quaternion.identity;
        image.gameObject.SetActive(true);
        imageBG.gameObject.SetActive(true);
        Texture2D tex = Capture();


        SetupRectTransforms(tex);

        int halfWidth = tex.width / 2;

        image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));


        // Keep anchors centered to preserve aspect ratio
        image.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        image.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

        StartCoroutine(MoveCamera());

    }

    public IEnumerator Hide()
    {
        yield return null;
        image.gameObject.SetActive(false);
        imageBG.gameObject.SetActive(false);
    }

}
