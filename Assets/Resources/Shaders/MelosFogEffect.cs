using UnityEngine;
using System.Collections;

public class MelosFogEffect : MonoBehaviour {

    [HideInInspector]
    public Material material;
    public bool useFog = true;
    [Header("Fog Mode")]
    public bool linear = true;
    public float fogNear = 15f;
    public float fogFar = 250f;
    [Tooltip("Maximum possible fog coordinate")]
    public float maxDistance = 800f;
    [Range(0,1)]
    public float finalColorStart = 0.7f;
    [Range(0,1)]
    public float finalColorEnd = 0.9f;
    public Color finalColor = new Color(1, 1, 1);

    [Tooltip("Reduce below 1 to prevent skybox from being totally fogged in some settings")]
    public float intensity = 1f;
    public bool exponential = false;
    public bool exponentialSquared = false;
    public float density = 0.02f;
    [Header("Blend Mode")]
    public bool UseScreenBlendMode = false;
    public bool UseAddBlendMode = false;
    public bool UseMultiplyBlendMode = false;
    [Header("Don't change")]
    public bool moveThisToCameraAndDestroy = false;

    void Awake() {
        Camera camera = GameObject.Find("MedBigCam").GetComponent<Camera>();
        if (moveThisToCameraAndDestroy) {
            MelosFogEffect sf = camera.gameObject.GetComponent<MelosFogEffect>();
            sf.useFog = useFog;

            sf.linear = linear;
            sf.fogNear = fogNear;
            sf.fogFar = fogFar;
            sf.exponential = exponential;
            sf.exponentialSquared = exponentialSquared;
            sf.density = density;
            sf.intensity = intensity;
            sf.maxDistance = maxDistance;
            sf.finalColorStart = finalColorStart;
            sf.finalColor = finalColor;

            sf.UseScreenBlendMode = UseScreenBlendMode;
            sf.UseAddBlendMode = UseAddBlendMode;
            sf.UseMultiplyBlendMode = UseMultiplyBlendMode;
            sf.moveThisToCameraAndDestroy = false;
            Destroy(this);
        } else {
            material = new Material(Shader.Find("Hidden/SeanFog"));
            camera.depthTextureMode = DepthTextureMode.Depth;
        }


    }

    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if (!useFog) {
            Graphics.Blit(source, destination);
            return;
        }

        material.SetColor("_FinalColor", finalColor);
        material.SetInt("_UseScreen", UseScreenBlendMode ? 1 : 0);
        material.SetInt("_UseAdd", UseAddBlendMode ? 1 : 0);
        material.SetInt("_UseMultiply", UseMultiplyBlendMode ? 1 : 0);
        material.SetInt("_FogLinear", linear ? 1 : 0);
        material.SetInt("_FogExp", exponential ? 1 : 0);
        material.SetInt("_FogExp2", exponentialSquared ? 1 : 0);
        material.SetFloat("_Density", density);
        material.SetFloat("_FinalColorStart", finalColorStart);
        material.SetFloat("_FinalColorEnd", finalColorEnd);
        material.SetFloat("_FogFar", fogFar);
        material.SetFloat("_FogNear", fogNear);
        material.SetFloat("_Intensity", intensity);
        material.SetFloat("_MaxDistance", maxDistance);
        Graphics.Blit(source, destination, material,0);
    }
}