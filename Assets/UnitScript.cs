using JetBrains.Annotations;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.UI;
using static DataScript;

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
        public List<int> equipmentsIDs;
        public List<equipment> equipments;
        public bool isboss;
        public bool attacksfriends;
    }

    [Serializable]
    public class EnemyStats
    {
        public int classID;
        public int desiredlevel;
        public int itemtodropID;
        public bool usetelekinesis;
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
        public int ID;
        public int Grade;
        public equipmentmodel equipmentmodel;
    }

    [Serializable]
    public class equipmentmodel
    {
        public GameObject Model;
        public Vector3 localposition;
        public Vector3 localscale;
        public Vector3 localrotation;
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
    public EnemyStats enemyStats;

    public bool trylvlup;
    public bool fixedgrowth;

    public equipment Fists;

    public GameObject ModelHand;

    public float movespeed;

    private battlecameraScript battlecameraScript;

    private Animator animator;

    private Transform armature;

    private Vector3 initialpos;
    private Vector3 initialforward;

    private GameObject currentequipmentmodel;

    public Material AllyMat;
    public Material EnemyMat;
    public GameObject Head;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FindAnyObjectByType<DataScript>().GenerateEquipmentList(UnitCharacteristics);
        LevelSetup();
        Fists = FindAnyObjectByType<DataScript>().equipmentList[0];
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
        UpdateWeaponModel();
        if(UnitCharacteristics.affiliation=="playable")
        {
            Head.GetComponent<SkinnedMeshRenderer>().material=AllyMat;
        }
        else
        {
            Head.GetComponent<SkinnedMeshRenderer>().material = EnemyMat;
        }
        

    }

    // Update is called once per frame
    void FixedUpdate()
    {

        Debug.DrawLine(transform.GetChild(1).position, transform.GetChild(1).position +Vector3.Normalize(transform.GetChild(1).forward - transform.GetChild(1).position)*2f, Color.red);

        if (battlecameraScript == null)
        {
            battlecameraScript = FindAnyObjectByType<battlecameraScript>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (armature == null)
        {
            armature = animator.transform;
            initialpos = armature.localPosition;
            initialforward = armature.forward;
        }
        if(animator!=null)
        {
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            {
                if (Vector3.Distance(armature.localPosition, initialpos) > 0.1f)
                {
                    armature.localPosition += (initialpos - armature.localPosition).normalized * 0.2f* Time.deltaTime;
                }
                armature.rotation = Quaternion.LookRotation(initialforward, Vector3.up);

            }
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
            if (!battlecameraScript.incombat)
            {
                transform.forward = new Vector3(direction.x, 0f, direction.y);
            }


        }
        else
        {
            transform.position = new Vector3(UnitCharacteristics.position.x, transform.position.y, UnitCharacteristics.position.y);
            if (animator.gameObject.activeSelf)
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
        UpdateRendererLayer();

    }

    public void ResetForward()
    {
        initialforward = armature.forward;
    }
    private void UpdateWeaponModel()
    {
        if(currentequipmentmodel !=null)
        {
            Destroy(currentequipmentmodel);
        }
        if(GetFirstWeapon().Grade!=0)
        {
            equipmentmodel equipmentmodel = GetFirstWeapon().equipmentmodel;
            currentequipmentmodel = Instantiate(equipmentmodel.Model);
            currentequipmentmodel.transform.SetParent(ModelHand.transform);
            currentequipmentmodel.transform.localPosition = equipmentmodel.localposition;
            currentequipmentmodel.transform.localScale = equipmentmodel.localscale;
            currentequipmentmodel.transform.localRotation = Quaternion.Euler(equipmentmodel.localrotation);
        }
    }
    private void LevelSetup()
    {
        if (UnitCharacteristics.affiliation != "playable")
        {

            UnitCharacteristics.telekinesisactivated = enemyStats.usetelekinesis;

            ClassInfo classtoapply = FindAnyObjectByType<DataScript>().ClassList[enemyStats.classID];
            UnitCharacteristics.stats.HP = classtoapply.BaseStats.HP;
            UnitCharacteristics.stats.Strength = classtoapply.BaseStats.Strength;
            UnitCharacteristics.stats.Psyche = classtoapply.BaseStats.Psyche;
            UnitCharacteristics.stats.Defense = classtoapply.BaseStats.Defense;
            UnitCharacteristics.stats.Resistance = classtoapply.BaseStats.Resistance;
            UnitCharacteristics.stats.Speed = classtoapply.BaseStats.Speed;
            UnitCharacteristics.stats.Dexterity = classtoapply.BaseStats.Dexterity;
            UnitCharacteristics.growth.HPGrowth = classtoapply.StatGrowth.HPGrowth;
            UnitCharacteristics.growth.StrengthGrowth = classtoapply.StatGrowth.StrengthGrowth;
            UnitCharacteristics.growth.PsycheGrowth = classtoapply.StatGrowth.PsycheGrowth;
            UnitCharacteristics.growth.DefenseGrowth = classtoapply.StatGrowth.DefenseGrowth;
            UnitCharacteristics.growth.ResistanceGrowth = classtoapply.StatGrowth.ResistanceGrowth;
            UnitCharacteristics.growth.SpeedGrowth = classtoapply.StatGrowth.SpeedGrowth;
            UnitCharacteristics.growth.DexterityGrowth = classtoapply.StatGrowth.DexterityGrowth;
            fixedgrowth = true;
            int numberoflevelups = enemyStats.desiredlevel - UnitCharacteristics.level;

            for (int i = 0; i < numberoflevelups; i++)
            {

                List<int> statsgained = LevelUp();
                string statsgainedstr = "";
                foreach (int level in statsgained)
                {
                    statsgainedstr += level.ToString() + " , ";
                }
            }

        }

    }

    private void UpdateRendererLayer()
    {
        if (UnitCharacteristics.alreadyplayed)
        {
            foreach (MeshRenderer Renderer in GetComponentsInChildren<MeshRenderer>())
            {
                Renderer.renderingLayerMask = 0;
            }

        }
        else
        {
            foreach (MeshRenderer Renderer in GetComponentsInChildren<MeshRenderer>())
            {
                switch (UnitCharacteristics.affiliation)
                {
                    case "playable":
                        Renderer.renderingLayerMask = 1;
                        break;
                    case "enemy":
                        Renderer.renderingLayerMask = 2;
                        break;
                    case "other":
                        Renderer.renderingLayerMask = 3;
                        break;

                }
            }
        }

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
            SynchroniseWeaponIDs();
            return UnitCharacteristics.equipments[0];
        }
        else
        {
            return null;
        }

    }

    private void SynchroniseWeaponIDs()
    {
        UnitCharacteristics.equipmentsIDs = new List<int>();
        foreach (equipment equipment in UnitCharacteristics.equipments)
        {
            UnitCharacteristics.equipmentsIDs.Add(equipment.ID);
        }
        UpdateWeaponModel();
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
        SynchroniseWeaponIDs();
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
            SynchroniseWeaponIDs();
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

    public (int, bool, string) GetRangeMeleeAndType()
    {
        equipment firstweapon = GetFirstWeapon();
        int range = firstweapon.Range;
        bool melee = true;
        string type = firstweapon.type;
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

        return (range, melee, type);
    }
}
