using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPrefabScript : MonoBehaviour
{
    [Serializable]
    public struct weaponVisuals
    {
        public string weaponType;
        public List<Material> materials;
        public GameObject weaponPrefabLvl1;
        public GameObject weaponPrefabLvl2;
        public GameObject weaponPrefabLvl3;
        public GameObject weaponPrefabLvl4;
        public Texture2D WeaponSpriteLvl1;
        public Texture2D WeaponSpriteLvl2;
        public Texture2D WeaponSpriteLvl3;
        public Texture2D WeaponSpriteLvl4;
        public Color WeaponEffectColor;

    }


    [Header("Weapon Gameobjects")]

    public List<weaponVisuals> WeaponVisualsList;
    public GameObject currentWeaponGO = null;

    [Header("Variables")]
    public float timetoappear;
    public float timetovanish;
    private UnitScript UnitScript;
    private weaponVisuals weapontypeequiped;

    private Coroutine AppearCoroutine;
    private weaponVisuals CurrentlyAppearing;
    private List<GameObject> weaponstovanish = new List<GameObject>();
    private int currentEquipedLevel;
    public Material TemplateMaterial;
    public Shader Shader;

    [Header("Placement Variables")]
    public Vector3 positionoffset;
    public Vector3 rotationoffset;

    public GameObject ParticleSystem;

    private void Awake()
    {
        UnitScript = GetComponentInParent<UnitScript>();
    }


    // Update is called once per frame
    void Update()
    {
        if (UnitScript == null)
        {

            UnitScript = GetComponentInParent<UnitScript>();
            return;
        }

        if (weaponstovanish.Count > 0)
        {
            StartCoroutine(MakeWeaponVanish(weaponstovanish[0]));
            weaponstovanish.RemoveAt(0);
        }

    }

    public void SwitchWeaponGO(string type, int level)
    {
        if (CurrentlyAppearing.weaponType != null && CurrentlyAppearing.weaponType.ToLower() == type.ToLower() && currentEquipedLevel == level)
        {
            return;
        }
        GameObject newweapon = currentWeaponGO;
        weaponVisuals weaponVis = WeaponVisualsList[0];
        foreach (weaponVisuals weapon in WeaponVisualsList)
        {
            weaponVis = weapon;
            if (weapon.weaponType == type.ToLower())
            {
                switch (level)
                {
                    case 1:
                        newweapon = Instantiate(weapon.weaponPrefabLvl1);

                        break;
                    case 2:
                        newweapon = Instantiate(weapon.weaponPrefabLvl2);

                        break;
                    case 3:
                        newweapon = Instantiate(weapon.weaponPrefabLvl3);

                        break;
                    case 4:
                        newweapon = Instantiate(weapon.weaponPrefabLvl4);

                        break;
                }

                newweapon.transform.localScale *= 0.5f;
                GameObject newparticlesystem = Instantiate(ParticleSystem);
                newparticlesystem.transform.parent = newweapon.transform;
                newparticlesystem.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(new Vector3(0, 90, 0)));
                newparticlesystem.transform.localScale = Vector3.one;

                newparticlesystem.SetActive(true);
                break;
            }
        }
        currentEquipedLevel = level;
        foreach (weaponVisuals weaponVisuals in WeaponVisualsList)
        {
            if (weaponVisuals.weaponType == type)
            {
                CurrentlyAppearing = weaponVisuals;
                weapontypeequiped = weaponVisuals;
                break;
            }
        }

        if (newweapon != currentWeaponGO)
        {
            if (newweapon != null)
            {
                CreateAndSetWeaponMaterial(newweapon, level, weaponVis);
                weaponVis.materials = new List<Material>();

                SetMaterialsToList(weaponVis.materials, newweapon);
            }
            if (currentWeaponGO != null)
            {
                weaponstovanish.Add(currentWeaponGO);
            }

            currentWeaponGO = newweapon;
            EquipWeaponVisuals();
        }


    }

    public void CreateAndSetWeaponMaterial(GameObject GO, int level, weaponVisuals WeaponVis)
    {
        Material newMaterial = new Material(TemplateMaterial);
        switch (level)
        {
            case 1:
                newMaterial.SetTexture("_Texture2D", WeaponVis.WeaponSpriteLvl1);
                break;
            case 2:
                newMaterial.SetTexture("_Texture2D", WeaponVis.WeaponSpriteLvl2);
                break;
            case 3:
                newMaterial.SetTexture("_Texture2D", WeaponVis.WeaponSpriteLvl3);
                break;
            case 4:
                newMaterial.SetTexture("_Texture2D", WeaponVis.WeaponSpriteLvl4);
                break;
        }
        newMaterial.SetColor("_OutlineColor", WeaponVis.WeaponEffectColor);

        SetMaterialToGO(GO, newMaterial);
    }

    // this function is used to place the weapon on the unit, it takes the weapon type and the parent transform as parameters, it then finds the corresponding weapon gameobject and sets its parent and local position and rotation based on the offset variables
    public void PlaceWeapon(string type, Transform parent, Vector3 modelpositionoffset, Vector3 modelscaleoffset, Vector3 modelrotationoffset)
    {

        foreach (weaponVisuals weapon in WeaponVisualsList)
        {
            if (weapon.weaponType == type.ToLower())
            {
                currentWeaponGO.transform.parent = parent;
                currentWeaponGO.transform.SetLocalPositionAndRotation(positionoffset + modelpositionoffset, Quaternion.Euler(rotationoffset + modelrotationoffset));
                currentWeaponGO.transform.localScale = new Vector3(currentWeaponGO.transform.localScale.x * modelscaleoffset.x, currentWeaponGO.transform.localScale.y * modelscaleoffset.y, currentWeaponGO.transform.localScale.z * modelscaleoffset.z);
                break;
            }
        }
    }
    // this function is used to get the weapon gameobject
    public GameObject GetWeaponGO()
    {

        return currentWeaponGO;
    }

    public void EquipWeaponVisuals()
    {
        if (currentWeaponGO != null)
        {

            SetDissolve(weapontypeequiped, 0, currentWeaponGO);


            //if (AppearCoroutine != null)
            //{
            //    StopCoroutine(AppearCoroutine);
            //}

            AppearCoroutine = StartCoroutine(MakeWeaponAppear());
        }
    }
    // this coroutine is used to make the weapon appear, it lerps the dissolve value from 1 to 0, if during the process the weapon to appear is not the same as the currently appearing weapon, it will stop lerping and just set the dissolve value to 0
    private IEnumerator MakeWeaponAppear()
    {
        List<Material> materials = new List<Material>();
        SetMaterialsToList(materials, currentWeaponGO);

        float elapsedTime = 0f;
        while (elapsedTime < timetoappear)
        {
            elapsedTime += Time.deltaTime;

            float lerpedDissolve = Mathf.Lerp(0f, 1.1f, elapsedTime / timetoappear);

            SetDissolve(materials, lerpedDissolve, currentWeaponGO);

            yield return null;
        }
        AppearCoroutine = null;
    }
    // this coroutine is used to make the weapon vanish, it lerps the dissolve value from 0 to 1 and then deactivates the gameobject, if during the process the weapon to vanish is not the same as the currently appearing weapon, it will stop lerping and just deactivate the gameobject
    private IEnumerator MakeWeaponVanish(GameObject GO)
    {
        List<Material> materials = new List<Material>();
        SetMaterialsToList(materials, GO);

        float elapsedTime = 0f;
        while (elapsedTime < timetovanish)
        {

            elapsedTime += Time.deltaTime;

            float lerpedDissolve = Mathf.Lerp(0.9f, 0f, elapsedTime / timetovanish);
            SetDissolve(materials, lerpedDissolve, GO);


            yield return null;
        }

        Destroy(GO);
    }

    // sets the dissolve value for all materials in the list
    private void SetDissolve(weaponVisuals weapon, float dissolve, GameObject GO)
    {
        if ((weapon.materials == null || weapon.materials.Count == 0) && weapon.weaponType != "")
        {
            SetMaterialsToList(weapon.materials, GO);
        }
        foreach (Material mat in weapon.materials)
        {
            mat.SetFloat("_DissolvePercent", dissolve);
        }
    }

    private void SetDissolve(List<Material> materials, float dissolve, GameObject GO)
    {
        if ((materials == null || materials.Count == 0) && GO != null)
        {
            SetMaterialsToList(materials, GO);
        }
        foreach (Material mat in materials)
        {
            mat.SetFloat("_DissolvePercent", dissolve);
        }
    }


    //recursively adds material to the list
    private void SetMaterialsToList(List<Material> listotuse, GameObject GOtouse)
    {
        // if object has renderer, add material to list
        if (GOtouse != null && GOtouse.GetComponent<Renderer>() != null && !listotuse.Contains(GOtouse.GetComponent<Renderer>().material))
        {
            listotuse.Add(GOtouse.GetComponent<Renderer>().material);
        }

        // then for each child we do the same.

        foreach (Transform child in GOtouse.transform)
        {
            SetMaterialsToList(listotuse, child.gameObject);
        }
    }

    private void SetMaterialToGO(GameObject GOtouse, Material Mattoadd)
    {
        // Sets material
        if (GOtouse.GetComponent<Renderer>())
        {
            GOtouse.GetComponent<Renderer>().material = Mattoadd;
        }

        // Also sets layer
        GOtouse.layer = LayerMask.NameToLayer("Players");

        // then for each child we do the same.

        foreach (Transform child in GOtouse.transform)
        {
            SetMaterialToGO(child.gameObject, Mattoadd);
        }
    }

}
