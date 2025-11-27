#ifndef ADDITIONAL_LIGHT_INCLUDED
#define ADDITIONAL_LIGHT_INCLUDED

void MainLight_float(float3 WorldPos, out float3 Direction, out float3 Color, out float Attenuation)
{
    #ifdef SHADERGRAPH_PREVIEW
        Direction = normalize(float3(1.0f, 1.0f, 0.0f));
        Color = 1.0f;
        Attenuation = 1.0f;
    #else
        Light mainLight = GetMainLight();
        Direction = mainLight.direction;
        Color = mainLight.color;
        Attenuation = mainLight.distanceAttenuation;
    #endif
}

void AdditionalLights_float(float3 WorldPos, int lightID, out float3 Direction, out float3 Color, out float Attenuation)
{

    Direction = normalize(float3(1.0f, 1.0f, 0.0f));
    Color = 0.0f;
    Attenuation = 0.0f;
    #ifndef SHADERGRAPH_PREVIEW
        int lightCount = GetAdditionalLightsCount();
        if(lightID < lightCount)
        {
            Light light = GetAdditionalLight(lightID, WorldPos);
            Direction = light.direction;
            Color = light.color;
            Attenuation = light.distanceAttenuation;
        }
    #endif
}

void AdditionalLights_half(float3 WorldPos, int lightID, out float3 Direction, out float3 Color, out float Attenuation)
{

    Direction = normalize(float3(1.0f, 1.0f, 0.0f));
    Color = 0.0f;
    Attenuation = 0.0f;
    #ifndef SHADERGRAPH_PREVIEW
        int lightCount = GetAdditionalLightsCount();
        if(lightID < lightCount)
        {
            Light light = GetAdditionalLight(lightID, WorldPos);
            Direction = light.direction;
            Color = light.color;
            Attenuation = light.distanceAttenuation;
        }
    #endif
}

void AllAdditionalLights_float(float3 WorldPos,float3 WorldNormal,float2 CutOffThresholds, out float3 LightColor)
{
    LightColor = 0.0f;
    #ifndef SHADERGRAPH_PREVIEW

        int lightCount = GetAdditionalLightsCount();

        for(int i = 0; i < lightCount; i++)
        {
            Light light = GetAdditionalLight(i, WorldPos);
            float3 color = dot(light.direction, WorldNormal);
            color = smoothstep(CutOffThresholds.x, CutOffThresholds.y, color);
            color *= light.color;
            color *= light.distanceAttenuation;
            LightColor += color;
        }
    #endif
}


#endif // ADDITIONAL_LIGHT_INCLUDED