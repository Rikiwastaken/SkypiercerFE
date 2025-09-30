using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnitScript;

public class MinimapScript : MonoBehaviour
{
    public Image minimapImage;

    private Texture2D minimapTexture;

    private GridScript gridScript;

    private battlecameraScript battlecameraScript;

    private int waitforinitialization = 5;

    private int updatedelay;

    private int showposition;

    private void Start()
    {
        gridScript = FindAnyObjectByType<GridScript>();
        battlecameraScript = FindAnyObjectByType<battlecameraScript>();
    }

    private void FixedUpdate()
    {
        if(waitforinitialization > 0)
        {
            waitforinitialization--;
            if(waitforinitialization == 0 )
            {
                CreateMinimap();
            }
        }

        if(updatedelay > 0)
        {
            updatedelay--;
        }
        if(updatedelay <= 0)
        {
            updatedelay = (int)(0.1f/Time.deltaTime);
            showposition += 1;
            if(showposition >8)
            {
                showposition = 0;
            }
            UpdateMinimap();
        }


        if(battlecameraScript.incombat)
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
        if(minimapTexture == null)
        {
            int gridHeight = gridScript.Grid[0].Count;
            int gridWidth = gridScript.Grid.Count;
            minimapTexture = new Texture2D(gridWidth, gridHeight, TextureFormat.RGBA32, false);
            minimapTexture.filterMode = FilterMode.Point;

            Color[] pixels = new Color[gridWidth * gridHeight];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;
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

            float zoom = 7f;
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

    private void SetTileColor(int x, int y, Color color)
    {
        color.a = 0.75f;
        minimapTexture.SetPixel(x, y, color);
        minimapTexture.Apply();
    }

    public void UpdateMinimap()
    {
        if(waitforinitialization<=0)
        {
            for (int i = 0; i < gridScript.Grid.Count; i++)
            {
                for (int j = 0; j < gridScript.Grid[i].Count; j++)
                {
                    GridSquareScript tile = gridScript.GetTile(i, j);
                    if (gridScript.GetTile(i, j).isobstacle)
                    {
                        SetTileColor(i, j, Color.grey);
                    }
                    else
                    {
                        SetTileColor(i, j, Color.white);
                    }
                    if (gridScript.attacktiles.Contains(tile) || gridScript.lockedattacktiles.Contains(tile))
                    {
                        SetTileColor(i, j, new Color(245f/255f, 176f / 255f, 66f / 255f)); //orange
                    }
                    if (gridScript.healingtiles.Contains(tile) || gridScript.lockedhealingtiles.Contains(tile))
                    {
                        SetTileColor(i, j, new Color(66f / 255f, 245f / 255f, 170f / 255f));
                    }
                    if (gridScript.movementtiles.Contains(tile) || gridScript.lockedmovementtiles.Contains(tile))
                    {
                        SetTileColor(i, j, Color.blue);
                    }
                }
            }
            foreach (Character character in gridScript.allunits)
            {
                foreach (GridSquareScript tile in character.currentTile)
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
                }
            }

            


            if(gridScript.selection!=null && showposition<=4)
            {
                SetTileColor((int)gridScript.selection.GridCoordinates.x, (int)gridScript.selection.GridCoordinates.y, Color.green);
            }
        }
        
    }

}
