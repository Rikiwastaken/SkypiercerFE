using UnityEngine;

public class LightFlickerScript : MonoBehaviour
{
    [Header("Base Settings")]
    private float baseIntensity;
    public float intensityVariation = 0.5f;

    private float baseRange;
    public float rangeVariation = 1f;

    [Header("Flicker Settings")]
    public float flickerSpeed = 5f;

    private Light fireLight;
    private float randomOffset;

    void Awake()
    {
        fireLight = GetComponent<Light>();
        randomOffset = Random.Range(0f, 1000f);
        baseIntensity = fireLight.intensity;
        baseRange = fireLight.range;
    }

    void Update()
    {
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, randomOffset);

        fireLight.intensity = baseIntensity + (noise - 0.5f) * intensityVariation;
        fireLight.range = baseRange + (noise - 0.5f) * rangeVariation;
    }
}
