using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Add this to a SceneData gameobject , can set stuff per scene that would otherwise be boudn to prefabs.
public class SceneData : MonoBehaviour {


	public Color ambientLight;
	public Color backgroundColor;

    public bool cameraDoesntFollow = false;
	public bool cameraStartsInFixedFollow = false;
	public bool cameraFixedTopdown = false;
	public Transform fixedFollowInitialPos;
	public float NEAR = 0.4f;
	public float FAR  = 1000f;

	public bool hasFog = true;
	public Color fogColor;
	public float fogDensity = 0f;
	public float fogStart = 0f;
	public float fogEnd = 100f;
	public bool fogIsLinear = false;
	public bool fogIsExponential = true;

	public bool hasHeightFog = false;
	public float heightFogHeight = 30f;
	public float heightFogDensity = 0.25f; // Interesting effect when 0.34 and height is correct, high FOV on camera.
	public float heightFogStartDistance = 1.5f;

    [Range(0, 1)]
    public float skyboxExposure = 1;
    public bool editsSkybox = false;
    Material skyboxMat;

	void Start () {


		if (cameraStartsInFixedFollow) {
			Camera.main.transform.position = fixedFollowInitialPos.position;
            Camera.main.GetComponent<Cinemachine.CinemachineBrain>().enabled = true;
		}
        if (cameraDoesntFollow) {
            Camera.main.GetComponent<MedBigCam>().enabled = false;
            Camera.main.transform.position = fixedFollowInitialPos.position;
        }
        gameRunning = true;


        if (gameRunning) {
            if (skyboxMat == null && RenderSettings.skybox != null) {
                skyboxMat = new Material(RenderSettings.skybox);
                RenderSettings.skybox = skyboxMat;
            }
        }
    }
    bool gameRunning = false;
	void OnValidate () {
		backgroundColor.a = 1;
		Camera.main.backgroundColor = backgroundColor;
		Camera.main.nearClipPlane = NEAR;
		Camera.main.farClipPlane = FAR;
		RenderSettings.ambientLight = ambientLight;

		RenderSettings.fog = hasFog;
		if (fogIsLinear) RenderSettings.fogMode = FogMode.Linear;
		if (fogIsExponential) RenderSettings.fogMode = FogMode.Exponential;
		RenderSettings.fogColor = fogColor;
		RenderSettings.fogDensity = fogDensity;
		RenderSettings.fogStartDistance = fogStart;
		RenderSettings.fogEndDistance = fogEnd;
		
		if (Camera.main.GetComponent<MedBigCam>() != null) {
			MedBigCam cc = Camera.main.GetComponent<MedBigCam>();
			cc.fixedFollow = cameraStartsInFixedFollow;
			cc.fixedFollowTopDown = cameraFixedTopdown;
		}
	}

	void Update() {
        if (editsSkybox && gameRunning) {
            skyboxMat.SetFloat("_Exposure", skyboxExposure);
        }
    }
}
