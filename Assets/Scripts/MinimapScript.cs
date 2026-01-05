using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnitScript;

public class MinimapScript : MonoBehaviour
{

    public static MinimapScript instance;

    public Image minimapImage;

    private Texture2D minimapTexture;

    private GridScript gridScript;

    private cameraScript cameraScript;

    private int waitforinitialization = 5;

    private int updatedelay;

    private int showposition;

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
        cameraScript = FindAnyObjectByType<cameraScript>();
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
            updatedelay = (int)(0.25f / Time.deltaTime);
            showposition += 1;
            if (showposition > 8)
            {
                showposition = 0;
            }
            UpdateMinimap();
        }


        if (cameraScript.incombat)
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
            minimapTexture = new Texture2D(gridWidth * 8, gridHeight * 8, TextureFormat.RGBA32, false);
            minimapTexture.filterMode = FilterMode.Point;

            Color[] pixels = new Color[gridWidth * 8 * gridHeight * 8];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear;
            }

            minimapTexture.SetPixels(pixels);
            minimapTexture.Apply();

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
            GetComponent<RectTransform>().anchorMin = new Vector2(1, 0);
            GetComponent<RectTransform>().anchorMax = new Vector2(1, 0);

            // Pivot also bottom-left
            GetComponent<RectTransform>().pivot = new Vector2(1, 0);

            // Position with offset (e.g., 10px from edges)
            GetComponent<RectTransform>().anchoredPosition = new Vector2(-5, 5);
        }

        UpdateMinimap();
    }

    private void SetTileColor(int x, int y, Color color, float alpha = 0.75f)
    {
        color.a = alpha;
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                minimapTexture.SetPixel(x * 8 + i, y * 8 + j, color);
            }

        }

        minimapTexture.Apply();
    }

    public void UpdateMinimap()
    {
        if (waitforinitialization <= 0)
        {
            for (int i = 0; i < gridScript.Grid.Count; i++)
            {
                for (int j = 0; j < gridScript.Grid[i].Count; j++)
                {
                    GridSquareScript tile = gridScript.GetTile(i, j);
                    if (tile.isobstacle)
                    {
                        SetTileColor(i, j, Color.grey);
                    }
                    else
                    {
                        SetTileColor(i, j, Color.white);
                    }
                    switch (tile.type.ToLower())
                    {
                        case "forest":
                            SetTileColor((int)tile.GridCoordinates.x, (int)tile.GridCoordinates.y, Color.green);
                            break;
                        case "ruins":
                            SetTileColor((int)tile.GridCoordinates.x, (int)tile.GridCoordinates.y, Color.gray);
                            break;
                        case "fire":
                            SetTileColor((int)tile.GridCoordinates.x, (int)tile.GridCoordinates.y, Color.red);

                            break;
                        case "water":
                            SetTileColor((int)tile.GridCoordinates.x, (int)tile.GridCoordinates.y, Color.cyan);
                            break;


                        case "fortification":
                            SetTileColor((int)tile.GridCoordinates.x, (int)tile.GridCoordinates.y, new Color(0.545f, 0.271f, 0.075f));

                            break;
                        case "fog":
                            SetTileColor((int)tile.GridCoordinates.x, (int)tile.GridCoordinates.y, Color.black);

                            break;
                        case "medicinalwater":
                            SetTileColor((int)tile.GridCoordinates.x, (int)tile.GridCoordinates.y, new Color(0.5510659f, 0.8608279f, 0.9371068f));
                            break;
                        case "desert":
                            SetTileColor((int)tile.GridCoordinates.x, (int)tile.GridCoordinates.y, Color.yellow);
                            break;
                    }
                    if (gridScript.attacktiles.Contains(tile) || gridScript.lockedattacktiles.Contains(tile))
                    {
                        SetTileColor(i, j, new Color(245f / 255f, 176f / 255f, 66f / 255f)); //orange
                    }
                    if (gridScript.healingtiles.Contains(tile) || gridScript.lockedhealingtiles.Contains(tile))
                    {
                        SetTileColor(i, j, new Color(66f / 255f, 245f / 255f, 170f / 255f));
                    }
                    if (gridScript.movementtiles.Contains(tile) || gridScript.lockedmovementtiles.Contains(tile))
                    {
                        SetTileColor(i, j, Color.blue);
                    }
                    manageContraptionLeverIcon(tile);
                    manageEndMapIncon(tile);
                    if (!tile.activated)
                    {
                        SetTileColor((int)tile.GridCoordinates.x, (int)tile.GridCoordinates.y, Color.yellow, 0f);
                    }

                }
            }
            foreach (Character character in gridScript.allunits)
            {
                foreach (GridSquareScript tile in character.currentTile)
                {
                    if (character.currentTile.Count > 0)
                    {
                        if (character.affiliation == "playable")
                        {
                            SetTileColor((int)tile.GridCoordinates.x, (int)tile.GridCoordinates.y, Color.cyan);
                        }
                        else if (character.affiliation == "enemy")
                        {
                            SetTileColor((int)tile.GridCoordinates.x, (int)tile.GridCoordinates.y, Color.red);
                        }
                        else if (character.affiliation == "other")
                        {
                            SetTileColor((int)tile.GridCoordinates.x, (int)tile.GridCoordinates.y, Color.yellow);
                        }
                        if (!tile.activated)
                        {
                            SetTileColor((int)tile.GridCoordinates.x, (int)tile.GridCoordinates.y, Color.yellow, 0f);
                        }
                    }

                }
            }

            manageselectionicon();



        }

    }

    private void manageEndMapIncon(GridSquareScript tile)
    {
        if (tile.isfinishtile) // show selection as a red and yellow ring
        {
            int selectedx = (int)tile.GridCoordinates.x;
            int selectedy = (int)tile.GridCoordinates.y;


            for (int i = 1; i < 7; i++)
            {
                minimapTexture.SetPixel(selectedx * 8 + i, selectedy * 8 + 1, Color.white);
            }
            for (int i = 2; i < 6; i++)
            {
                minimapTexture.SetPixel(selectedx * 8 + i, selectedy * 8 + 2, Color.white);
            }
            for (int i = 3; i < 7; i++)
            {
                minimapTexture.SetPixel(selectedx * 8 + 3, selectedy * 8 + i, Color.black);
            }
            for (int i = 4; i < 7; i++)
            {
                minimapTexture.SetPixel(selectedx * 8 + 4, selectedy * 8 + i, Color.red);
            }
            minimapTexture.SetPixel(selectedx * 8 + 5, selectedy * 8 + 4, Color.red);
            minimapTexture.SetPixel(selectedx * 8 + 5, selectedy * 8 + 5, Color.red);
            minimapTexture.SetPixel(selectedx * 8 + 6, selectedy * 8 + 4, Color.red);


            minimapTexture.Apply();
        }
    }

    private void manageContraptionLeverIcon(GridSquareScript tile)
    {
        if (tile.Mechanism != null && tile.Mechanism.type == 2 && !tile.Mechanism.isactivated) // show selection as a red and yellow ring
        {
            int selectedx = (int)tile.GridCoordinates.x;
            int selectedy = (int)tile.GridCoordinates.y;


            minimapTexture.SetPixel(selectedx * 8 + 1, selectedy * 8 + 2, Color.yellow);
            minimapTexture.SetPixel(selectedx * 8 + 2, selectedy * 8 + 2, Color.yellow);
            minimapTexture.SetPixel(selectedx * 8 + 5, selectedy * 8 + 2, Color.yellow);
            minimapTexture.SetPixel(selectedx * 8 + 7, selectedy * 8 + 2, Color.yellow);

            minimapTexture.SetPixel(selectedx * 8 + 0, selectedy * 8 + 3, Color.yellow);
            minimapTexture.SetPixel(selectedx * 8 + 3, selectedy * 8 + 3, Color.yellow);
            minimapTexture.SetPixel(selectedx * 8 + 5, selectedy * 8 + 3, Color.yellow);
            minimapTexture.SetPixel(selectedx * 8 + 7, selectedy * 8 + 3, Color.yellow);

            minimapTexture.SetPixel(selectedx * 8 + 0, selectedy * 8 + 4, Color.yellow);
            minimapTexture.SetPixel(selectedx * 8 + 3, selectedy * 8 + 4, Color.yellow);
            minimapTexture.SetPixel(selectedx * 8 + 4, selectedy * 8 + 4, Color.yellow);
            minimapTexture.SetPixel(selectedx * 8 + 5, selectedy * 8 + 4, Color.yellow);
            minimapTexture.SetPixel(selectedx * 8 + 6, selectedy * 8 + 4, Color.yellow);
            minimapTexture.SetPixel(selectedx * 8 + 7, selectedy * 8 + 4, Color.yellow);

            minimapTexture.SetPixel(selectedx * 8 + 1, selectedy * 8 + 5, Color.yellow);
            minimapTexture.SetPixel(selectedx * 8 + 2, selectedy * 8 + 5, Color.yellow);




            minimapTexture.Apply();
        }
    }

    private void manageselectionicon()
    {
        //if (gridScript.selection != null && showposition <= 4)
        if (gridScript.selection != null) // show selection as a red and yellow ring
        {
            int selectedx = (int)gridScript.selection.GridCoordinates.x;
            int selectedy = (int)gridScript.selection.GridCoordinates.y;



            minimapTexture.SetPixel(selectedx * 8 + 2, selectedy * 8 + 0, Color.yellow);
            minimapTexture.SetPixel(selectedx * 8 + 3, selectedy * 8 + 0, Color.red);
            minimapTexture.SetPixel(selectedx * 8 + 4, selectedy * 8 + 0, Color.red);
            minimapTexture.SetPixel(selectedx * 8 + 5, selectedy * 8 + 0, Color.yellow);
            minimapTexture.SetPixel(selectedx * 8 + 1, selectedy * 8 + 1, Color.yellow);
            minimapTexture.SetPixel(selectedx * 8 + 6, selectedy * 8 + 1, Color.yellow);
            minimapTexture.SetPixel(selectedx * 8 + 0, selectedy * 8 + 2, Color.yellow);
            minimapTexture.SetPixel(selectedx * 8 + 7, selectedy * 8 + 2, Color.yellow);
            minimapTexture.SetPixel(selectedx * 8 + 0, selectedy * 8 + 3, Color.red);
            minimapTexture.SetPixel(selectedx * 8 + 7, selectedy * 8 + 3, Color.red);
            minimapTexture.SetPixel(selectedx * 8 + 0, selectedy * 8 + 4, Color.red);
            minimapTexture.SetPixel(selectedx * 8 + 7, selectedy * 8 + 4, Color.red);
            minimapTexture.SetPixel(selectedx * 8 + 0, selectedy * 8 + 5, Color.yellow);
            minimapTexture.SetPixel(selectedx * 8 + 7, selectedy * 8 + 5, Color.yellow);
            minimapTexture.SetPixel(selectedx * 8 + 1, selectedy * 8 + 6, Color.yellow);
            minimapTexture.SetPixel(selectedx * 8 + 6, selectedy * 8 + 6, Color.yellow);
            minimapTexture.SetPixel(selectedx * 8 + 2, selectedy * 8 + 7, Color.yellow);
            minimapTexture.SetPixel(selectedx * 8 + 3, selectedy * 8 + 7, Color.red);
            minimapTexture.SetPixel(selectedx * 8 + 4, selectedy * 8 + 7, Color.red);
            minimapTexture.SetPixel(selectedx * 8 + 5, selectedy * 8 + 7, Color.yellow);

            minimapTexture.Apply();
        }
    }

}
