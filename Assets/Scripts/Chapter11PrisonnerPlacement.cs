
using System.Collections.Generic;
using UnityEngine;
using static UnitScript;

public class Chapter11PrisonnerPlacement : MonoBehaviour
{

    public List<Vector2> prisonnersPlayablePosition;
    public List<int> PlayableCharactersToSpawn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        List<int> IDsToRemove = new List<int>();
        foreach (int ID in PlayableCharactersToSpawn)
        {
            if (!DataScript.instance.PlayableCharacterList[ID].playableStats.unlocked)
            {
                IDsToRemove.Add(ID);
            }
        }

        foreach (int ID in IDsToRemove)
        {
            PlayableCharactersToSpawn.Remove(ID);
        }

        List<Character> playablelist = DataScript.instance.PlayableCharacterList;
        for (int i = 0; i < Mathf.Min(prisonnersPlayablePosition.Count, PlayableCharactersToSpawn.Count); i++)
        {

            Character charactertospawn = playablelist[PlayableCharactersToSpawn[i]];
            EnemyStats enemyStats = new EnemyStats();
            enemyStats.startpos = prisonnersPlayablePosition[i];
            enemyStats.Name = charactertospawn.name;
            enemyStats.modelID = charactertospawn.modelID;
            enemyStats.isother = true;
            enemyStats.personality = "guard";
            enemyStats.Skills = new List<int> { charactertospawn.UnitSkill };
            enemyStats.classID = -1;
            if (charactertospawn.ID != 0)
            {
                enemyStats.PlayableSpriteID = charactertospawn.ID;
            }
            else
            {
                enemyStats.SpriteID = 5;
            }

            enemyStats.monsterStats = new MonsterStats();
            if (charactertospawn.SecondUnitSkill > 0)
            {
                enemyStats.Skills.Add(charactertospawn.SecondUnitSkill);
            }
            foreach (int skill in charactertospawn.EquipedSkills)
            {
                enemyStats.Skills.Add(skill);
            }
            enemyStats.equipments = charactertospawn.equipmentsIDs;

            MapInitializer.instance.InitializeNonPlayable(enemyStats, -1, charactertospawn);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
