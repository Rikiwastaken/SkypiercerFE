using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.UI;

public class UnitScript : MonoBehaviour
{
    [Serializable]
    public class Character
    {
        public bool protagonist;
        public string name;
        public BaseStats stats;
        public int level;
        public int experience;
        public string affiliation; // playable, enemy, other
        public StatGrowth growth;
        public int currentHP;
        public int movements;
        public Vector2 position;
        public bool alreadyplayed;
        public bool alreadymoved;
        public bool telekinesisactivated;
        public List<equipment> equipments;
        public bool isboss;
        public bool attacksfriends;
    }

    [Serializable]
    public class equipment
    {
        public string Name;
        public int BaseDamage;
        public int BaseHit;
        public int BaseCrit;
        public int Range;
        public string type;
        public int Currentuses;
        public int Maxuses;
    }

    [Serializable]
    public class BaseStats
    {
        public int HP;
        public int Strength;
        public int Psyche;
        public int Defense;
        public int Resistance;
        public int Speed;
        public int Dexterity;
    }

    [Serializable]
    public class StatGrowth
    {
        public int HPGrowth;
        public int StrengthGrowth;
        public int PsycheGrowth;
        public int DefenseGrowth;
        public int ResistanceGrowth;
        public int SpeedGrowth;
        public int DexterityGrowth;
    }

    public Character UnitCharacteristics;

    public bool trylvlup;
    public bool fixedgrowth;

    public equipment Fists;

    public float movespeed;

    private battlecameraScript battlecameraScript;

    private Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        transform.position = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), Mathf.Round(transform.position.z));
        UnitCharacteristics.position = new Vector2((int)transform.position.x, (int)transform.position.z);
        if (UnitCharacteristics.equipments == null)
        {
            UnitCharacteristics.equipments = new List<equipment>();
            for (int i = 0; i < 6; i++)
            {
                UnitCharacteristics.equipments.Add(new equipment());
            }
        }
        else if (UnitCharacteristics.equipments.Count < 6)
        {
            for (int i = UnitCharacteristics.equipments.Count; i < 6; i++)
            {
                UnitCharacteristics.equipments.Add(new equipment());
            }
        }
        UnitCharacteristics.currentHP = UnitCharacteristics.stats.HP;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (battlecameraScript == null)
        {
            battlecameraScript = FindAnyObjectByType<battlecameraScript>();
        }

        if(animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (trylvlup)
        {
            trylvlup = false;
            LevelUp();
        }
        transform.GetChild(0).GetChild(1).GetComponent<Image>().type = Image.Type.Filled;
        transform.GetChild(0).GetChild(1).GetComponent<Image>().fillAmount = (float)UnitCharacteristics.currentHP / (float)UnitCharacteristics.stats.HP;

        if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), UnitCharacteristics.position) > 0.1f)
        {
            animator.SetBool("Walk", true);
            Vector2 direction = (UnitCharacteristics.position - new Vector2(transform.position.x, transform.position.z)).normalized;
            transform.position += new Vector3(direction.x, 0f, direction.y) * movespeed * Time.fixedDeltaTime;
            if(!battlecameraScript.incombat)
            {
                transform.forward = new Vector3(direction.x, 0f, direction.y);
            }
            
            
        }
        else
        {
            transform.position = new Vector3(UnitCharacteristics.position.x, transform.position.y, UnitCharacteristics.position.y);
            if(animator.gameObject.activeSelf)
            {
                animator.SetBool("Walk", false);
            }
            
        }


        if (battlecameraScript.incombat)
        {
            if (battlecameraScript.fighter1 == gameObject || battlecameraScript.fighter2 == gameObject)
            {
                animator.gameObject.SetActive(true);
            }
            else
            {
                animator.gameObject.SetActive(false);
            }
        }
        else if (!animator.gameObject.activeSelf)
        {
            animator.gameObject.SetActive(true);
        }

        //TemporaryColor();

    }

    private void TemporaryColor()
    {

        Color newcolor = Color.white;
        if (UnitCharacteristics.affiliation == "playable")
        {
            newcolor = Color.blue;
            if (UnitCharacteristics.alreadyplayed)
            {
                newcolor *= 0.5f;
                newcolor.a = 1f;
            }
        }
        else if (UnitCharacteristics.affiliation == "enemy")
        {
            newcolor = Color.red;
            if (UnitCharacteristics.alreadyplayed)
            {
                newcolor *= 0.5f;
                newcolor.a = 1f;
            }
        }
        else
        {
            newcolor = Color.yellow;
            if (UnitCharacteristics.alreadyplayed)
            {
                newcolor *= 0.5f;
                newcolor.a = 1f;
            }
        }

        if (battlecameraScript.incombat)
        {
            newcolor.a = 0f;
            if (battlecameraScript.fighter1 == gameObject || battlecameraScript.fighter2 == gameObject)
            {
                newcolor.a = 1f;
            }
        }


        GetComponent<MeshRenderer>().material.color = newcolor;
    }

    public List<int> LevelUp()
    {
        List<int> lvlupresult = new List<int>();
        UnitCharacteristics.experience -= 100;
        UnitCharacteristics.level += 1;
        if (fixedgrowth)
        {
            UnitCharacteristics.stats.HP += (int)(UnitCharacteristics.growth.HPGrowth / 10f);

            UnitCharacteristics.currentHP += (int)(UnitCharacteristics.growth.HPGrowth / 10f);
            lvlupresult.Add((int)(UnitCharacteristics.growth.HPGrowth / 10f));

            UnitCharacteristics.stats.Strength += (int)(UnitCharacteristics.growth.StrengthGrowth / 10f);

            lvlupresult.Add((int)(UnitCharacteristics.growth.StrengthGrowth / 10f));

            UnitCharacteristics.stats.Psyche += (int)(UnitCharacteristics.growth.PsycheGrowth / 10f);

            lvlupresult.Add((int)(UnitCharacteristics.growth.PsycheGrowth / 10f));

            UnitCharacteristics.stats.Defense += (int)(UnitCharacteristics.growth.DefenseGrowth / 10f);

            lvlupresult.Add((int)(UnitCharacteristics.growth.DefenseGrowth / 10f));


            UnitCharacteristics.stats.Resistance += (int)(UnitCharacteristics.growth.ResistanceGrowth / 10f);

            lvlupresult.Add((int)(UnitCharacteristics.growth.ResistanceGrowth / 10f));

            UnitCharacteristics.stats.Speed += (int)(UnitCharacteristics.growth.SpeedGrowth / 10f);

            lvlupresult.Add((int)(UnitCharacteristics.growth.SpeedGrowth / 10f));

            UnitCharacteristics.stats.Dexterity += (int)(UnitCharacteristics.growth.DexterityGrowth / 10f);

            lvlupresult.Add((int)(UnitCharacteristics.growth.DexterityGrowth / 10f));

        }
        else
        {
            int rd = UnityEngine.Random.Range(0, 100);
            if (rd <= UnitCharacteristics.growth.HPGrowth)
            {
                UnitCharacteristics.stats.HP += 10;
                UnitCharacteristics.currentHP += 10;
                lvlupresult.Add(10);
            }
            else
            {
                UnitCharacteristics.stats.HP += 1;
                UnitCharacteristics.currentHP += 1;
                lvlupresult.Add(1);
            }

            rd = UnityEngine.Random.Range(0, 100);
            if (rd <= UnitCharacteristics.growth.StrengthGrowth)
            {
                UnitCharacteristics.stats.Strength += 10;
                lvlupresult.Add(10);
            }
            else
            {
                UnitCharacteristics.stats.Strength += 1;
                lvlupresult.Add(1);
            }

            rd = UnityEngine.Random.Range(0, 100);
            if (rd <= UnitCharacteristics.growth.PsycheGrowth)
            {
                UnitCharacteristics.stats.Psyche += 10;
                lvlupresult.Add(10);
            }
            else
            {
                UnitCharacteristics.stats.Psyche += 1;
                lvlupresult.Add(1);
            }

            rd = UnityEngine.Random.Range(0, 100);
            if (rd <= UnitCharacteristics.growth.DefenseGrowth)
            {
                UnitCharacteristics.stats.Defense += 10;
                lvlupresult.Add(10);
            }
            else
            {
                UnitCharacteristics.stats.Defense += 1;
                lvlupresult.Add(1);
            }


            rd = UnityEngine.Random.Range(0, 100);
            if (rd <= UnitCharacteristics.growth.ResistanceGrowth)
            {
                UnitCharacteristics.stats.Resistance += 10;
                lvlupresult.Add(10);
            }
            else
            {
                UnitCharacteristics.stats.Resistance += 1;
                lvlupresult.Add(1);
            }

            rd = UnityEngine.Random.Range(0, 100);
            if (rd <= UnitCharacteristics.growth.SpeedGrowth)
            {
                UnitCharacteristics.stats.Speed += 10;
                lvlupresult.Add(10);
            }
            else
            {
                UnitCharacteristics.stats.Speed += 1;
                lvlupresult.Add(1);
            }

            rd = UnityEngine.Random.Range(0, 100);
            if (rd <= UnitCharacteristics.growth.DexterityGrowth)
            {
                UnitCharacteristics.stats.Dexterity += 10;
                lvlupresult.Add(10);
            }
            else
            {
                UnitCharacteristics.stats.Dexterity += 1;
                lvlupresult.Add(1);
            }
        }
        return lvlupresult;
    }

    public equipment GetFirstWeapon()
    {
        for (int i = 0; i < UnitCharacteristics.equipments.Count; i++)
        {
            if (UnitCharacteristics.equipments[i].type != "item" && UnitCharacteristics.equipments[i].Currentuses > 0)
            {
                return UnitCharacteristics.equipments[i];
            }
        }

        return Fists;
    }

    public List<equipment> GetAllWeapons()
    {
        List<equipment> weapons = new List<equipment>();
        for (int i = 0; i < UnitCharacteristics.equipments.Count; i++)
        {
            if (UnitCharacteristics.equipments[i].type != "item" && UnitCharacteristics.equipments[i].Currentuses > 0)
            {
                weapons.Add(UnitCharacteristics.equipments[i]);
            }
        }
        if (weapons.Count == 0)
        {
            weapons.Add(Fists);
        }
        return weapons;
    }

    public equipment GetNextWeapon()
    {
        List<equipment> listweapons = new List<equipment>();
        List<equipment> rest = new List<equipment>();
        for (int i = 0; i < UnitCharacteristics.equipments.Count; i++)
        {
            if (UnitCharacteristics.equipments[i].type != "item" && UnitCharacteristics.equipments[i].Currentuses > 0)
            {
                listweapons.Add(UnitCharacteristics.equipments[i]);
            }
            else
            {
                rest.Add(UnitCharacteristics.equipments[i]);
            }
        }
        if (listweapons.Count > 0)
        {
            UnitCharacteristics.equipments = new List<equipment>();
            for (int i = 1; i < listweapons.Count; i++)
            {
                UnitCharacteristics.equipments.Add(listweapons[i]);
            }
            UnitCharacteristics.equipments.Add(listweapons[0]);
            for (int i = 0; i < rest.Count; i++)
            {
                UnitCharacteristics.equipments.Add(rest[i]);
            }
            return UnitCharacteristics.equipments[0];
        }
        else
        {
            return null;
        }

    }

    public void EquipWeapon(equipment weapon)
    {
        if (UnitCharacteristics.equipments.Contains(weapon))
        {
            int index = UnitCharacteristics.equipments.IndexOf(weapon);
            int safegard = 0;
            while (index != 0 && safegard < 20)
            {
                GetNextWeapon();
                index = UnitCharacteristics.equipments.IndexOf(weapon);
            }
        }
    }

    public equipment GetPreviousWeapon()
    {
        List<equipment> listweapons = new List<equipment>();
        List<equipment> rest = new List<equipment>();
        for (int i = 0; i < UnitCharacteristics.equipments.Count; i++)
        {
            if (UnitCharacteristics.equipments[i].type != "item" && UnitCharacteristics.equipments[i].Currentuses > 0)
            {
                listweapons.Add(UnitCharacteristics.equipments[i]);
            }
            else
            {
                rest.Add(UnitCharacteristics.equipments[i]);
            }
        }
        if (listweapons.Count > 0)
        {
            UnitCharacteristics.equipments = new List<equipment>();
            UnitCharacteristics.equipments.Add(listweapons[listweapons.Count - 1]);
            for (int i = 0; i < listweapons.Count - 1; i++)
            {
                UnitCharacteristics.equipments.Add(listweapons[i]);
            }
            for (int i = 0; i < rest.Count; i++)
            {
                UnitCharacteristics.equipments.Add(rest[i]);
            }
            return UnitCharacteristics.equipments[0];
        }
        else
        {
            return null;
        }

    }

    public void RestoreUses(int number)
    {
        for (int i = 0; i < UnitCharacteristics.equipments.Count; i++)
        {
            if (UnitCharacteristics.equipments[i].type != "item" && UnitCharacteristics.equipments[i].type != null)
            {
                UnitCharacteristics.equipments[i].Currentuses += number;
                if (UnitCharacteristics.equipments[i].Currentuses > UnitCharacteristics.equipments[i].Maxuses)
                {
                    UnitCharacteristics.equipments[i].Currentuses = UnitCharacteristics.equipments[i].Maxuses;
                }
            }
        }
    }
    public (int, bool) GetRangeAndMele()
    {
        equipment firstweapon = GetFirstWeapon();
        int range = firstweapon.Range;
        bool melee = true;
        if (firstweapon.type == "bow")
        {
            melee = false;
            if (UnitCharacteristics.telekinesisactivated)
            {
                range += 2;
            }
        }
        else
        {
            if (UnitCharacteristics.telekinesisactivated)
            {
                range += 1;
            }
        }

        return (range, melee);
    }
}
