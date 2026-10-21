using System;
using System.Collections.Generic;
using UnityEngine;
using static UnitScript;

public class CombatSceneManagerV3 : MonoBehaviour
{

    public static CombatSceneManagerV3 instance;

    [SerializeField] private GameObject EventSystem;

    [Serializable]
    public class BattleSummary
    {
        public Character AttackerCopyBeforeCombat;
        public Character DefenderCopyBeforeCombat;
        public OffensiveSummary AttackerToDefender = null;
        public OffensiveSummary DefenderToAttacker = null;
    }

    [Serializable]
    public class OffensiveSummary
    {
        [Header("CharacterA Attacked Character B")]
        public Character CharacterA;
        public Character CharacterB;
        public List<int> TypeOfHits; // -1 for Dodges, 1 for hits, 2 for crits
        public List<int> DamageTaken;
        public List<int> CharacterASkillTriggers;
        public List<int> CharacterBSkillTriggers;
    }


}
