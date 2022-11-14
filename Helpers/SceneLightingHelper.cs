using System;
using UnityEngine;

namespace TrombLoader.Helpers;

public class SceneLightingHelper : MonoBehaviour
{
    public Color ambientSkyColor = Color.black;
    public Color ambientEquatorColor = Color.black;

    public float  ambientIntensity = 0f;
    public float  reflectionIntensity = 0f;
    public Material skybox = null;
    public Light sun = null;
    
    //By default, the initial values disable ambient lighting in a scene
    public void Start()
    {
        RenderSettings.ambientSkyColor = ambientSkyColor;
        RenderSettings.ambientEquatorColor = ambientEquatorColor;
        //RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientIntensity = ambientIntensity;
        RenderSettings.reflectionIntensity = reflectionIntensity;
        RenderSettings.skybox = skybox;
        RenderSettings.sun = sun;
    }
}