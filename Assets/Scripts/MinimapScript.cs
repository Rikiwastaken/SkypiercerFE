using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnitScript;

public class MinimapScript : MonoBehaviour
{

    public static MinimapScript instance;

    public Image minimapImage;
    public Image minimapBGImage;

    private Texture2D minimapTexture;
    private Texture2D minimapBackgroundTexture;

    private GridScript gridScript;

    private ActionsMenu _ActionsMenu;

    private int waitforinitialization = 5;

    private int updatedelay;

    private int showposition;

    private GridSquareScript previoustile;

    private bool launchupdate;

    private List<GameObject> PlayableCharacterPins = new List<GameObject>();
    private List<GameObject> EnemyCharacterPins = new List<GameObject>();
    private List<GameObject> OtherCharacterPins = new List<GameObject>();
    private List<GameObject> ContraptionPins = new List<GameObject>();

    private GameObject SelectedTileIcon;

    public Sprite flagpoleSprite;
    public Sprite LockedDoorSprite;
    public Sprite InterruptorSprite;
    public Sprite CurrentPositionSprite;

    public GameObject CharacterPinPrefab;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        gridScript = GridScript.instance;
        _ActionsMenu = ActionsMenu.instance;
    }

    private void Update()
    {
        if (waitforinitialization > 0)
        {
            waitforinitialization--;
            if (waitforinitialization == 0)
            {
                CreateMinimap();
            }
        }

        if (updatedelay > 0)
        {
            updatedelay--;
        }
        if (updatedelay <= 0)
        {
            updatedelay = (int)(0.5f / Time.deltaTime);
            showposition += 1;
            if (showposition > 8)
            {
                showposition = 0;
            }
            UpdateMinimap();
        }



        if (launchupdate)
        {

            launchupdate = false;
            ChangeMinimap();
        }

        if (_ActionsMenu.incombat)
        {
            minimapImage.enabled = false;
        }
        else
        {
            minimapImage.enabled = true;
        }

    }

    public void CreateMinimap()
    {
        if (minimapTexture == null)
        {
            int gridHeight = gridScript.Grid[0].Count;
            int gridWidth = gridScript.Grid.Count;
            minimapTexture = new Texture2D(gridWidth, gridHeight, TextureFormat.RGBA32, false);
            minimapTexture.filterMode = FilterMode.Point;

            Color[] pixels = new Color[gridWidth * gridHeight];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear;
            }

            minimapTexture.SetPixels(pixels);

            minimapImage.sprite = Sprite.Create(minimapTexture,
                new Rect(0, 0, minimapTexture.width, minimapTexture.height),
                new Vector2(0.5f, 0.5f),
                1, // pixels per unit
                0,
                SpriteMeshType.FullRect
            );

            minimapImage.rectTransform.sizeDelta = new Vector2(gridWidth, gridHeight);

            float zoom = 8f;
            minimapImage.rectTransform.sizeDelta = new Vector2(gridWidth * zoom, gridHeight * zoom);
            minimapImage.GetComponent<RectTransform>().anchorMin = new Vector2(1, 0);
            minimapImage.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0);

            // Pivot also bottom-left
            minimapImage.GetComponent<RectTransform>().pivot = new Vector2(1, 0);

            // Position with offset (e.g., 10px from edges)
            //minimapImage.GetComponent<RectTransform>().anchoredPosition = new Vector2(-5, 5);
        }

        if (minimapBackgroundTexture == null)
        {
            int gridHeight = gridScript.Grid[0].Count;
            int gridWidth = gridScript.Grid.Count;
            minimapBackgroundTexture = new Texture2D(gridWidth, gridHeight, TextureFormat.RGBA32, false);
            minimapBackgroundTexture.filterMode = FilterMode.Point;

            Color[] pixels = new Color[gridWidth * gridHeight];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear;
            }

            minimapBackgroundTexture.SetPixels(pixels);
            minimapBackgroundTexture.Apply();

            minimapBGImage.sprite = Sprite.Create(minimapBackgroundTexture,
                new Rect(0, 0, minimapBackgroundTexture.width, minimapBackgroundTexture.height),
                new Vector2(0.5f, 0.5f),
                1, // pixels per unit
                0,
                SpriteMeshType.FullRect
            );

            minimapBGImage.rectTransform.sizeDelta = new Vector2(gridWidth, gridHeight);

            float zoom = 8f;
            minimapBGImage.rectTransform.sizeDelta = new Vector2(gridWidth * zoom, gridHeight * zoom);
            minimapBGImage.GetComponent<RectTransform>().anchorMin = new Vector2(1, 0);
            minimapBGImage.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0);

            // Pivot also bottom-left
            minimapBGImage.GetComponent<RectTransform>().pivot = new Vector2(1, 0);

            // Position with offset (e.g., 10px from edges)
            minimapBGImage.GetComponent<RectTransform>().anchoredPosition = new Vector2(-5, 5);
        }
        FirstInitializationMinimapBG();
        UpdateMinimap();
    }

    public void FirstInitializationMinimapBG()
    {
        if (waitforinitialization <= 0)
        {
            for (int i = 0; i < gridScript.Grid.Count; i++)
            {
                for (int j = 0; j < gridScript.Grid[i].Count; j++)
                {
                    GridSquareScript tile = gridScript.GetTile(i, j);
                    if ((tile.Mechanism != null && (tile.Mechanism.type == 1 || tile.Mechanism.type == 2)))
                    {
                        SetBGTileColor(i, j, Color.white);
                    }
                    if (tile.activated)
                    {
                        if (tile.isobstacle)
                        {
                            SetBGTileColor(i, j, Color.grey);
                        }
                        else
                        {
                            SetBGTileColor(i, j, Color.white);
                        }
                        switch (tile.type.ToLower())
                        {
                            case "forest":
                                SetBGTileColor((int)tile.GridCoordinates.x, (int)tile.GridCoordinates.y, Color.green);
                                break;
                            case "ruins":
                                SetBGTileColor((int)tile.GridCoordinates.x, (int)tile.GridCoordinates.y, Color.gray);
                                break;
                            case "fire":
                                SetBGTileColor((int)tile.GridCoordinates.x, (int)tile.GridCoordinates.y, Color.red);

                                break;
                            case "water":
                                SetBGTileColor((int)tile.GridCoordinates.x, (int)tile.GridCoordinates.y, Color.cyan);
                                break;


                            case "fortification":
                                SetBGTileColor((int)tile.GridCoordinates.x, (int)tile.GridCoordinates.y, new Color(0.545f, 0.271f, 0.075f));

                                break;
                            case "fog":
                                SetBGTileColor((int)tile.GridCoordinates.x, (int)tile.GridCoordinates.y, Color.black);

                                break;
                            case "medicinalwater":
                                SetBGTileColor((int)tile.GridCoordinates.x, (int)tile.GridCoordinates.y, new Color(0.5510659f, 0.8608279f, 0.9371068f));
                                break;
                            case "desert":
                                SetBGTileColor((int)tile.GridCoordinates.x, (int)tile.GridCoordinates.y, Color.yellow);
                                break;
                        }
                    }
                    PlaceEndTilePins(tile);

                }
            }
            minimapBackgroundTexture.Apply();
        }
    }

    public Vector2 GridToMinimapPosition(Vector2 gridCoordinates)
    {
        float cellSize = 8f;

        return new Vector2((gridCoordinates.x + 0.5f) * cellSize, (gridCoordinates.y + 0.5f) * cellSize);
    }

    private void SetTileColor(Texture2D TextureToPaint, int x, int y, Color color, float alpha = 1f)
    {
        color.a = alpha;
        TextureToPaint.SetPixel(x, y, color);


    }

    private void SetBGTileColor(int x, int y, Color color, float alpha = 0.75f)
    {
        color.a = alpha;
        minimapBackgroundTexture.SetPixel(x, y, color);

    }

    public void PlaceCharacterPins()
    {
        int playablePinsIndex = 0;
        int EnemyPinsIndex = 0;
        int OtherPinsIndex = 0;
        foreach (Character character in gridScript.allunits)
        {
            if (character == null || character.currentTile == null)
            {
                continue;
            }

            Vector2 coordinates = character.currentTile.GridCoordinates;

            if (character.affiliation.ToLower() == "playable")
            {
                UpdateCharacterPin(0, coordinates, playablePinsIndex);
                playablePinsIndex++;
            }
            else if (character.affiliation.ToLower() == "enemy")
            {
                UpdateCharacterPin(1, coordinates, EnemyPinsIndex);
                EnemyPinsIndex++;
            }
            else if (character.affiliation.ToLower() == "other")
            {
                UpdateCharacterPin(2, coordinates, OtherPinsIndex);
                OtherPinsIndex++;
            }
        }
        DisableUselessPins(PlayableCharacterPins, playablePinsIndex);
        DisableUselessPins(EnemyCharacterPins, EnemyPinsIndex);
        DisableUselessPins(OtherCharacterPins, OtherPinsIndex);

    }

    public void PlaceEndTilePins(GridSquareScript currentTile)
    {
        if (currentTile == null || !currentTile.activated)
        {
            return;
        }
        if (currentTile.isfinishtile)
        {
            GameObject newPin = Instantiate(CharacterPinPrefab);
            newPin.name = "End Pin";
            newPin.transform.SetParent(transform.parent);


            RectTransform iconRect = newPin.GetComponent<RectTransform>();

            iconRect.anchorMin = new Vector2(0, 0);
            iconRect.anchorMax = new Vector2(0, 0);
            iconRect.pivot = new Vector2(0.5f, 0.5f);

            iconRect.anchoredPosition = GridToMinimapPosition(currentTile.GridCoordinates);

            Color targetcolor = new Color(1f, 1f, 1f, 1f);

            iconRect.transform.GetChild(0).GetComponent<Image>().color = targetcolor;
            iconRect.transform.GetChild(0).GetComponent<Image>().sprite = flagpoleSprite;
        }

    }

    private void UpdateCharacterPin(int type, Vector2 gridPosition, int currentindex) // type is 0 for playable, 1 for enemy, 2 for other
    {
        List<GameObject> CurrentPinList = new List<GameObject>();
        string name = "";
        Color targetColor = Color.white;
        switch (type)
        {
            case 0:
                CurrentPinList = PlayableCharacterPins;
                name = "Playable Pin " + currentindex;
                targetColor = Color.blue;
                break;
            case 1:
                CurrentPinList = EnemyCharacterPins;
                name = "Enemy Pin " + currentindex;
                targetColor = Color.red;
                break;
            case 2:
                CurrentPinList = OtherCharacterPins;
                name = "Other Pin " + currentindex;
                targetColor = Color.yellow;
                break;
        }
        if (CurrentPinList.Count <= currentindex)
        {
            GameObject newPin = Instantiate(CharacterPinPrefab);
            newPin.name = name;
            newPin.transform.SetParent(transform.parent);
            CurrentPinList.Add(newPin);
        }

        if (!CurrentPinList[currentindex].activeSelf)
        {
            CurrentPinList[currentindex].SetActive(true);
        }

        RectTransform iconRect = CurrentPinList[currentindex].GetComponent<RectTransform>();

        iconRect.anchorMin = new Vector2(0, 0);
        iconRect.anchorMax = new Vector2(0, 0);
        iconRect.pivot = new Vector2(0.5f, 0.5f);

        iconRect.anchoredPosition = GridToMinimapPosition(gridPosition);

        if (CurrentPinList[currentindex].transform.GetChild(0).GetComponent<Image>().color != targetColor)
        {
            CurrentPinList[currentindex].transform.GetChild(0).GetComponent<Image>().color = targetColor;
        }
    }

    private void DisableUselessPins(List<GameObject> pinlist, int currentindex)
    {
        for (int i = currentindex; i < pinlist.Count; i++)
        {
            if (pinlist[i].activeSelf)
            {
                pinlist[i].SetActive(false);
            }
        }
    }

    public void UpdateMinimap()
    {

        if (waitforinitialization <= 0)
        {
            launchupdate = true;
        }


    }

    private void ChangeMinimap()
    {
        int currentContraptionIndex = 0;
        for (int i = 0; i < gridScript.Grid.Count; i++)
        {
            for (int j = 0; j < gridScript.Grid[i].Count; j++)
            {


                GridSquareScript tile = gridScript.GetTile(i, j);

                if (tile != null && tile.activated && tile.Mechanism != null && (tile.Mechanism.type == 1 || tile.Mechanism.type == 2))
                {
                    manageContraptionIcon(tile, currentContraptionIndex);
                    currentContraptionIndex++;
                }



                SetTileColor(minimapTexture, i, j, Color.clear, 0f);
                if (gridScript.attacktiles.Contains(tile) || gridScript.lockedattacktiles.Contains(tile))
                {
                    //SetTileColor(i, j, new Color(245f / 255f, 176f / 255f, 66f / 255f)); //orange
                    SetTileColor(minimapTexture, i, j, Color.red);
                }
                if (gridScript.healingtiles.Contains(tile) || gridScript.lockedhealingtiles.Contains(tile))
                {
                    SetTileColor(minimapTexture, i, j, new Color(66f / 255f, 245f / 255f, 170f / 255f));
                }
                if (gridScript.movementtiles.Contains(tile) || gridScript.lockedmovementtiles.Contains(tile))
                {
                    SetTileColor(minimapTexture, i, j, Color.blue);
                }
                if (!tile.activated)
                {
                    SetTileColor(minimapTexture, (int)tile.GridCoordinates.x, (int)tile.GridCoordinates.y, Color.yellow, 0f);
                }


            }
        }

        DisableUselessContraptionPins(currentContraptionIndex);
        PlaceCharacterPins();
        manageselectionicon();

        minimapTexture.Apply();
    }

    private void manageContraptionIcon(GridSquareScript tile, int currentContraptionIndex)
    {
        if (ContraptionPins.Count <= currentContraptionIndex)
        {
            GameObject newPin = Instantiate(CharacterPinPrefab);
            newPin.name = "Contraption Pin " + currentContraptionIndex;
            newPin.transform.SetParent(transform.parent);
            ContraptionPins.Add(newPin);
        }

        if (tile.Mechanism.isactivated)
        {
            if (tile.Mechanism.type == 1)
            {
                if (ContraptionPins[currentContraptionIndex].activeSelf)
                {
                    ContraptionPins[currentContraptionIndex].SetActive(false);
                }
            }
            else if (tile.Mechanism.type == 2)
            {
                if (!ContraptionPins[currentContraptionIndex].activeSelf)
                {
                    ContraptionPins[currentContraptionIndex].SetActive(true);
                }
                ContraptionPins[currentContraptionIndex].GetComponent<Image>().color = Color.green;
                ContraptionPins[currentContraptionIndex].transform.GetChild(0).GetComponent<Image>().color = Color.white;
                ContraptionPins[currentContraptionIndex].transform.GetChild(0).GetComponent<Image>().sprite = InterruptorSprite;
            }
        }
        else
        {
            if (tile.Mechanism.type == 1)
            {
                if (!ContraptionPins[currentContraptionIndex].activeSelf)
                {
                    ContraptionPins[currentContraptionIndex].SetActive(true);
                }
                ContraptionPins[currentContraptionIndex].GetComponent<Image>().color = Color.white;
                ContraptionPins[currentContraptionIndex].transform.GetChild(0).GetComponent<Image>().color = Color.white;
                ContraptionPins[currentContraptionIndex].transform.GetChild(0).GetComponent<Image>().sprite = LockedDoorSprite;
            }
            else if (tile.Mechanism.type == 2)
            {
                if (!ContraptionPins[currentContraptionIndex].activeSelf)
                {
                    ContraptionPins[currentContraptionIndex].SetActive(true);
                }
                ContraptionPins[currentContraptionIndex].GetComponent<Image>().color = Color.red;
                ContraptionPins[currentContraptionIndex].transform.GetChild(0).GetComponent<Image>().color = Color.white;
                ContraptionPins[currentContraptionIndex].transform.GetChild(0).GetComponent<Image>().sprite = InterruptorSprite;
            }
        }


        RectTransform iconRect = ContraptionPins[currentContraptionIndex].GetComponent<RectTransform>();

        iconRect.anchorMin = new Vector2(0, 0);
        iconRect.anchorMax = new Vector2(0, 0);
        iconRect.pivot = new Vector2(0.5f, 0.5f);

        iconRect.anchoredPosition = GridToMinimapPosition(tile.GridCoordinates);
    }

    private void DisableUselessContraptionPins(int lastcontraptionID)
    {
        for (int i = lastcontraptionID; i < ContraptionPins.Count; i++)
        {
            if (ContraptionPins[i].activeSelf)
            {
                ContraptionPins[i].SetActive(false);
            }
        }
    }


    private void manageselectionicon()
    {
        if (gridScript.selection != null) // show selection as a red and yellow ring
        {
            Vector2 targetpos = gridScript.selection.GridCoordinates;

            if (SelectedTileIcon == null)
            {
                SelectedTileIcon = new GameObject();
                SelectedTileIcon.name = "Selected Tile icon";
                RectTransform rect = SelectedTileIcon.AddComponent<RectTransform>();
                SelectedTileIcon.AddComponent<Image>();
                SelectedTileIcon.GetComponent<Image>().color = Color.white;
                SelectedTileIcon.GetComponent<Image>().sprite = CurrentPositionSprite;

                rect.rect.Set(0, 0, 150, 150);
                rect.localScale = Vector2.one * 0.06f;
                rect.SetParent(transform.parent);
            }

            RectTransform iconRect = SelectedTileIcon.GetComponent<RectTransform>();

            iconRect.anchorMin = new Vector2(0, 0);
            iconRect.anchorMax = new Vector2(0, 0);
            iconRect.pivot = new Vector2(0.5f, 0.5f);

            iconRect.anchoredPosition = GridToMinimapPosition(targetpos);
        }
    }

}
