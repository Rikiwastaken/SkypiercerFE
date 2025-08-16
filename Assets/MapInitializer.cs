using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using static UnitScript;

public class MapInitializer : MonoBehaviour
{

    public int numberofplayables;

    public List<Vector2> playablepos;

    private DataScript DataScript;

    public GameObject BaseCharacter;

    public GameObject Characters;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        InitializePlayers();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void InitializePlayers()
    {
        if(DataScript == null)
        {
            DataScript = FindAnyObjectByType<DataScript>();
        }


        int index = 0;
        foreach(Character playable in DataScript.PlayableCharacterList)
        {
            GameObject newcharacter = Instantiate(BaseCharacter);
            newcharacter.GetComponent<UnitScript>().UnitCharacteristics = playable; 
            playable.position = playablepos[index];
            newcharacter.transform.parent = Characters.transform;
            newcharacter.transform.position = new Vector3(playablepos[index].x,0, playablepos[index].y);
            index++;
        }
    }

}
