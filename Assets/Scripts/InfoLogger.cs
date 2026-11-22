using System;
using System.Collections.Generic;
using UnityEngine;

public class InfoLogger : MonoBehaviour
{


    public static InfoLogger instance;

    [Serializable]
    public class SessionLog
    {
        // Device Varibales
        public string OperatingSystem;
        public string CPUName;
        public int CPUCores;
        public int RAM_MB;

        public string GPUName;
        public string GPUVendor;
        public int GPUMemory_MB;
        public string GPUVersion;

        public string DeviceModel;
        public string DeviceType;

        public string UnityVersion;
        public string GameVersion;
        public string Platform;
        public string Language;


        //Play SessionVariables

        public long StartTime;

        public int SessionID;

        public List<BattleLog> battleLogs = new List<BattleLog>();

        public SessionLog(string operatingsystem)
        {
            //System
            OperatingSystem = operatingsystem;
            CPUName = SystemInfo.processorType;
            CPUCores = SystemInfo.processorCount;
            RAM_MB = SystemInfo.systemMemorySize;
            GPUName = SystemInfo.graphicsDeviceName;
            GPUVendor = SystemInfo.graphicsDeviceVendor;
            GPUMemory_MB = SystemInfo.graphicsMemorySize;
            GPUVersion = SystemInfo.graphicsDeviceVersion;

            // Device
            DeviceModel = SystemInfo.deviceModel;
            DeviceType = SystemInfo.deviceType.ToString();

            // Unity
            UnityVersion = Application.unityVersion;
            GameVersion = Application.version;
            Platform = Application.platform.ToString();
            Language = Application.systemLanguage.ToString();

            //Play Session
            StartTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            SessionID = InfoLogger.instance.GetNextFreeIndex(StartTime);

        }
    }

    [Serializable]
    public class BattleLog
    {
        public int ChapterID;
        public bool IsSideStory;
        public int Outcome;
        public int numberOfForsightUsed;
        public List<CharacterBattleLog> CharacterBattleLogList;
        public long TimeForBattleStart;
        public long TimeForBattleEnd;
        public int NumberOfTurns;

        public BattleLog(int _ChapterID, bool _IsSideStory)
        {
            ChapterID = _ChapterID;
            IsSideStory = _IsSideStory;
            TimeForBattleStart = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            numberOfForsightUsed = 0;
            CharacterBattleLogList = new List<CharacterBattleLog>();
        }
    }
    [Serializable]
    public class CharacterBattleLog
    {
        public int CharacterID;
        public string CharacterName;
        public int numberofKills;
        public int numberOfTimeHealed;
        public int numberOfAttacks;
        public int NumberOfTimesAttacked;
        public int NumberOfTimesBeingHealed;
        public int numberOfHits;
        public int numberOfHitsTaken;
        public int NumberOfMisses;
        public int NumberOfCrits;
        public int NumberOfDodges;
        public int NumberOfDoubleAttacks;
        public int NumberOfTripleAttacks;
        public List<int> EquipedSkills;
        public int LevelAtStart;
        public int LevelAtEnd;
        public List<UnitScript.WeaponMastery> BladeLevelsAtStart;
        public List<UnitScript.WeaponMastery> BladeLevelsAtEnd;
        public UnitScript.BaseStats StatsAtStart;
        public UnitScript.BaseStats StatsAtEnd;
        public bool CharacterLived;
    }

    [SerializeField] private SessionLog _SessionLog;
    private List<UnitScript.Character> CharacterInBattle;





    private void Awake()
    {
        instance = this;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _SessionLog = new SessionLog(SystemInfo.operatingSystem);
    }

    public void initializeBattleLog(TurnManger turnManger, MapInitializer mapInitializer, GridScript gridScript)
    {
        BattleLog newLog = new BattleLog(mapInitializer.ChapterID, mapInitializer.IsSideStory);
        _SessionLog.battleLogs.Add(newLog);


        CharacterInBattle = new List<UnitScript.Character>();
        foreach (UnitScript.Character Unit in gridScript.allunits)
        {
            if (Unit.affiliation == "playable")
            {
                CharacterInBattle.Add(Unit);
                CharacterBattleLog _CharacterBattleLog = new CharacterBattleLog();
                _CharacterBattleLog.CharacterID = Unit.ID;
                _CharacterBattleLog.EquipedSkills = Unit.EquipedSkills;
                _CharacterBattleLog.BladeLevelsAtStart = new List<UnitScript.WeaponMastery>();
                _CharacterBattleLog.LevelAtStart = Unit.level;
                foreach (UnitScript.WeaponMastery mastery in Unit.Masteries)
                {
                    _CharacterBattleLog.BladeLevelsAtStart.Add(new UnitScript.WeaponMastery() { Level = mastery.Level, weapontype = mastery.weapontype });
                }
                _CharacterBattleLog.StatsAtStart = CalculationToolsScript.CreateCopy(Unit.stats);
                newLog.CharacterBattleLogList.Add(_CharacterBattleLog);
            }
        }

    }


    public void AppendForeSight()
    {
        _SessionLog.battleLogs[_SessionLog.battleLogs.Count - 1].numberOfForsightUsed++;
    }


    public void EndBattleLog(int outcometype, int numberofturns)// 0: Victory, 1: Defeat, 2: ReturnedToMainMenu
    {
        BattleLog CurrentLog = _SessionLog.battleLogs[_SessionLog.battleLogs.Count - 1];
        CurrentLog.TimeForBattleEnd = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        CurrentLog.NumberOfTurns = numberofturns;
        CurrentLog.Outcome = outcometype;
        foreach (UnitScript.Character Character in CharacterInBattle)
        {
            foreach (CharacterBattleLog _CharacterBattleLog in CurrentLog.CharacterBattleLogList)
            {
                if (_CharacterBattleLog.CharacterID == Character.ID)
                {
                    if (Character.currentHP > 0)
                    {
                        _CharacterBattleLog.CharacterLived = true;
                    }
                    _CharacterBattleLog.CharacterName = Character.name;
                    _CharacterBattleLog.LevelAtEnd = Character.level;
                    _CharacterBattleLog.BladeLevelsAtEnd = new List<UnitScript.WeaponMastery>();
                    foreach (UnitScript.WeaponMastery mastery in Character.Masteries)
                    {
                        _CharacterBattleLog.BladeLevelsAtEnd.Add(new UnitScript.WeaponMastery() { Level = mastery.Level, weapontype = mastery.weapontype });
                    }
                    _CharacterBattleLog.StatsAtEnd = CalculationToolsScript.CreateCopy(Character.stats);
                }
            }
        }
        SaveAsATxt();
    }

    public void AddDefense(int CharacterDefending, bool washealed, int numberOfHitsTaken, int numberOfDodges)
    {
        foreach (CharacterBattleLog charlog in _SessionLog.battleLogs[_SessionLog.battleLogs.Count - 1].CharacterBattleLogList)
        {
            if (CharacterDefending == charlog.CharacterID)
            {

                if (washealed)
                {
                    charlog.NumberOfTimesBeingHealed++;
                }
                else
                {
                    charlog.NumberOfTimesAttacked++;
                }
                charlog.numberOfHitsTaken += numberOfHitsTaken;
                charlog.NumberOfDodges += numberOfDodges;
            }
        }
    }

    public void AddAttack(int CharacterAttacking, bool Ishealing, bool killed, int hits, int misses, int crits, bool doubleattacked, bool tripleattacked)
    {
        foreach (CharacterBattleLog charlog in _SessionLog.battleLogs[_SessionLog.battleLogs.Count - 1].CharacterBattleLogList)
        {
            if (CharacterAttacking == charlog.CharacterID)
            {
                if (Ishealing)
                {
                    charlog.numberOfTimeHealed++;
                }
                else
                {
                    charlog.numberOfAttacks++;
                }
                if (killed)
                {
                    charlog.numberofKills++;
                }
                charlog.numberOfHits += hits;
                charlog.NumberOfMisses += misses;
                charlog.NumberOfCrits += crits;
                if (tripleattacked)
                {
                    charlog.NumberOfTripleAttacks++;
                }
                else if (doubleattacked)
                {
                    charlog.NumberOfDoubleAttacks++;
                }

            }
        }
    }

    private void SaveAsATxt()
    {
        string DateStart = DateTimeOffset.FromUnixTimeSeconds(_SessionLog.StartTime).LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss");

        string FileName = DateTimeOffset.FromUnixTimeSeconds(_SessionLog.StartTime).LocalDateTime.ToString("yyyy-MM-dd") + "_" + _SessionLog.GameVersion + "_" + _SessionLog.SessionID + ".txt";

        string FileContent = "SESSION LOG\n";
        FileContent += "Session started at: " + DateStart + "\n";
        FileContent += "-----\n\n";


        FileContent += "SYSTEM INFORMATION\n";

        FileContent += "Operating System: " + _SessionLog.OperatingSystem + "\n";
        FileContent += "CPU: " + _SessionLog.CPUName + "\n";
        FileContent += "CPU Cores: " + _SessionLog.CPUCores + "\n";
        FileContent += "RAM: " + _SessionLog.RAM_MB + " MB\n";

        FileContent += "\nGPU INFORMATION\n";
        FileContent += "GPU: " + _SessionLog.GPUName + "\n";
        FileContent += "GPU Vendor: " + _SessionLog.GPUVendor + "\n";
        FileContent += "GPU Memory: " + _SessionLog.GPUMemory_MB + " MB\n";
        FileContent += "GPU Version: " + _SessionLog.GPUVersion + "\n";

        FileContent += "\nDEVICE INFORMATION\n";
        FileContent += "Device Model: " + _SessionLog.DeviceModel + "\n";
        FileContent += "Device Type: " + _SessionLog.DeviceType + "\n";
        FileContent += "Platform: " + _SessionLog.Platform + "\n";
        FileContent += "Language: " + _SessionLog.Language + "\n";

        FileContent += "\nGAME INFORMATION\n";
        FileContent += "Unity Version: " + _SessionLog.UnityVersion + "\n";
        FileContent += "Game Version: " + _SessionLog.GameVersion + "\n";

        FileContent += "\n-----\n";
        FileContent += "BATTLE LOGS\n";

        // Battle logs
        foreach (BattleLog battle in _SessionLog.battleLogs)
        {

            string MapName = "Chapter";
            if (battle.IsSideStory)
            {
                MapName = "SideStory";
            }

            MapName += battle.ChapterID;

            FileContent += "\n" + MapName + "\n";

            FileContent += " \nBattle started at " + DateTimeOffset.FromUnixTimeSeconds(battle.TimeForBattleStart).LocalDateTime.ToString("yyyy-MM-dd_HH-mm-ss") + "\n";
            FileContent += "Battle ended at " + DateTimeOffset.FromUnixTimeSeconds(battle.TimeForBattleEnd).LocalDateTime.ToString("yyyy-MM-dd_HH-mm-ss") + "\n";
            switch (battle.Outcome)
            {
                case 0:
                    FileContent += "Battle outcome: Victory\n";
                    break;
                case 1:
                    FileContent += "Battle outcome: Defeat\n";
                    break;
                case 2:
                    FileContent += "Battle outcome: Left the map\n";
                    break;
            }

            long Battletime = battle.TimeForBattleEnd - battle.TimeForBattleStart;
            int hours = (int)(Battletime / 60) / 60;
            int minutes = (int)(Battletime % 60) / 60;
            int seconds = (int)(Battletime % 60) % 60;
            FileContent += "Battle time: " + hours + "-" + minutes + "-" + seconds + "\n";
            FileContent += "\nBattle lasted " + battle.NumberOfTurns + " turns";
            FileContent += "\nForesight used " + battle.numberOfForsightUsed + " times\n";

            FileContent += "\nCHARACTER DETAILS\n\n";

            foreach (CharacterBattleLog CharacterLog in battle.CharacterBattleLogList)
            {
                FileContent += "Character " + CharacterLog.CharacterID + ": " + CharacterLog.CharacterName + "\n";
                if (CharacterLog.CharacterLived)
                {
                    FileContent += "Status: Survived\n";
                }
                else
                {
                    FileContent += "Status: Died\n";
                }
                FileContent += "Level:  " + CharacterLog.LevelAtStart + " > " + CharacterLog.LevelAtEnd + "\n";
                for (int i = 0; i < CharacterLog.BladeLevelsAtStart.Count; i++)
                {
                    UnitScript.WeaponMastery masteryatstart = CharacterLog.BladeLevelsAtStart[i];
                    if (CharacterLog.BladeLevelsAtEnd.Count > i && masteryatstart.Level >= 0)
                    {
                        UnitScript.WeaponMastery masteryatend = CharacterLog.BladeLevelsAtEnd[i];
                        FileContent += masteryatstart.weapontype + " Mastery: " + masteryatstart.Level + " > " + masteryatend.Level + "\n";
                    }

                }
                if (CharacterLog.EquipedSkills != null && CharacterLog.EquipedSkills.Count > 0)
                {
                    FileContent += "Equiped Skills: ";
                    foreach (string skillname in GetSkillNamesFromID(CharacterLog.EquipedSkills))
                    {
                        FileContent += skillname + ", ";
                    }
                    FileContent += "\n";
                }
                FileContent += "STATS:\n";
                FileContent += "   HP: " + CharacterLog.StatsAtStart.HP + " > " + CharacterLog.StatsAtEnd.HP + "\n";
                FileContent += "   Strength: " + CharacterLog.StatsAtStart.Strength + " > " + CharacterLog.StatsAtEnd.Strength + "\n";
                FileContent += "   Defense: " + CharacterLog.StatsAtStart.Defense + " > " + CharacterLog.StatsAtEnd.Defense + "\n";
                FileContent += "   Psyche: " + CharacterLog.StatsAtStart.Psyche + " > " + CharacterLog.StatsAtEnd.Psyche + "\n";
                FileContent += "   Resistance: " + CharacterLog.StatsAtStart.Resistance + " > " + CharacterLog.StatsAtEnd.Resistance + "\n";
                FileContent += "   Speed: " + CharacterLog.StatsAtStart.Speed + " > " + CharacterLog.StatsAtEnd.Speed + "\n";
                FileContent += "   Dexterity: " + CharacterLog.StatsAtStart.Dexterity + " > " + CharacterLog.StatsAtEnd.Dexterity + "\n";
                FileContent += "   Luck: " + CharacterLog.StatsAtStart.Luck + " > " + CharacterLog.StatsAtEnd.Luck + "\n";
                FileContent += "Attacked " + CharacterLog.numberOfAttacks + " times\n";
                FileContent += "Healed " + CharacterLog.numberOfTimeHealed + " times\n";
                FileContent += "Was attacker " + CharacterLog.NumberOfTimesAttacked + " times\n";
                FileContent += "Killed " + CharacterLog.numberofKills + " enemies\n";
                FileContent += "Hit " + CharacterLog.numberOfHits + " times\n";
                FileContent += "Missed " + CharacterLog.NumberOfMisses + " times\n";
                FileContent += "Crited " + CharacterLog.NumberOfCrits + " times\n";
                FileContent += "Dodged " + CharacterLog.NumberOfDodges + " times\n";
                FileContent += "Was hit " + CharacterLog.numberOfHitsTaken + " times\n";
                FileContent += "Doubled " + CharacterLog.NumberOfDoubleAttacks + " times\n";
                FileContent += "Tripled " + CharacterLog.NumberOfTripleAttacks + " times\n";
                FileContent += "\n-\n";

            }
            FileContent += "\n---\n";
        }

        FileContent += "\n-----\n";
        FileContent += "END OF SESSION LOG\n";

#if UNITY_EDITOR


#else

        // Save file
        string DirectoryPath = System.IO.Path.Combine(
            Application.persistentDataPath,
            "SessionLogs"
        );

        System.IO.Directory.CreateDirectory(DirectoryPath);

        string FilePath = System.IO.Path.Combine(DirectoryPath, FileName);

        System.IO.File.WriteAllText(FilePath, FileContent);

        Debug.Log("Session log saved at: " + FilePath);
#endif
    }

    private int GetNextFreeIndex(long startTime)
    {
        string DirectoryPath = System.IO.Path.Combine(
            Application.persistentDataPath,
            "SessionLogs"
        );

        System.IO.Directory.CreateDirectory(DirectoryPath);

        string DatePrefix = DateTimeOffset
            .FromUnixTimeSeconds(startTime)
            .LocalDateTime.ToString("yyyy-MM-dd")
            + "_" + Application.version;

        int Index = 0;

        while (System.IO.File.Exists(
            System.IO.Path.Combine(
                DirectoryPath,
                DatePrefix + "_" + Index + ".txt"
            )))
        {
            Index++;
        }

        return Index;
    }

    private List<string> GetSkillNamesFromID(List<int> skillIDs)
    {
        List<string> skillnames = new List<string>();
        foreach (DataScript.Skill skill in DataScript.instance.SkillList)
        {
            if (skill.ID != 0 && skillIDs.Contains(skill.ID))
            {
                skillnames.Add(skill.name);
            }
        }
        return skillnames;
    }


}

