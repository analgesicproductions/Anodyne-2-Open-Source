using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * How to use this:
 * [MODE 1]: onlyATrigger = true 
 *  - Simply transitions the current global lighting state to whatever is on this script. 
 *  - There should be a childed BoxCollider called 'entertrigger'.
 * [MODE 2]: usesSpline = true
 *  - Needs 4 triggers: entertrigger, exittrigger, endGate and endGate2
 * 	- the endGates will stop the transitioning code from running. 
 *  - the enter and exit trigger start the transitioning code
 * 	- needs a child BGCurve which tracks the player and transitions. the data defined in this script will be the endpoint state.
 * 		the startpoint state is automatically set based on the global scene data when entering the scene.
 * 
 * */
public class LightTrigger : MonoBehaviour {


	public bool onlyATrigger = true;
	public float transitionTime = 2.0f;
	float t_transition = 0f;

	BoxCollider startgate1;
	BoxCollider startgate2;
	BoxCollider endgate1;
	BoxCollider endgate2;


	public bool usesSpline = false;

	public Color ambientLight;
	public Color backgroundColor;
	public Color fogColor;
	public float fogDensity = 0f;
	public float fogStart = 0f;
	public float fogEnd = 100f;
	public float heightFogHeight = 30f;
	public float heightFogDensity = 0.25f; // Interesting effect when 0.34 and height is correct, high FOV on camera.
	public float heightFogStartDistance = 1.5f;
	public float hfdoa;
	public string[] lightNamesToFadeIn;
	Light[] lightsToFadeIn;
	float[] lightsToFadeInIntensities;

	Transform player;

	// Use this for initialization
	void Start () {

		player = GameObject.Find("OverworldPlayer").transform;
		startgate1 = transform.Find("startGate").GetComponent<BoxCollider>();
		if (usesSpline) {
			startgate2 = transform.Find("startGate2").GetComponent<BoxCollider>();	
			endgate1 = transform.Find("endGate").GetComponent<BoxCollider>();	
			endgate2 = transform.Find("endGate2").GetComponent<BoxCollider>();	
		}


		if (lightNamesToFadeIn != null && lightNamesToFadeIn.Length > 0) {
			lightsToFadeIn = new Light[lightNamesToFadeIn.Length];
			lightsToFadeInIntensities = new float[lightNamesToFadeIn.Length];
			for (int i = 0; i < lightNamesToFadeIn.Length;i++) {
				lightsToFadeIn[i] = GameObject.Find(lightNamesToFadeIn[i]).GetComponent<Light>();
				lightsToFadeInIntensities[i] = lightsToFadeIn[i].intensity;
			}
		}

		mode = MODE.WaitForPlayer;

		if (usesSpline) setStartState();
	}

	MODE mode = 0;
	// Update is called once per frame

	enum MODE {
		WaitForPlayer, Transitioning, Done
	}



	 Color SambientLight;
	 Color SbackgroundColor;
	 Color SfogColor;
	 float SfogDensity = 0f;
	 float SfogStart = 0f;
	 float SfogEnd = 100f;
	// float SheightFogHeight = 30f;
	 //float SheightFogDensity = 0.25f; // Interesting effect when 0.34 and height is correct, high FOV on camera.
	 //float SheightFogStartDistance = 1.5f;
	float Shfdoa;

	//GlobalFog gf;

	void setStartState ()
	{
		SbackgroundColor = new Color (Camera.main.backgroundColor.r, Camera.main.backgroundColor.g, Camera.main.backgroundColor.b, 1);
		SambientLight = new Color (RenderSettings.ambientLight.r, RenderSettings.ambientLight.g, RenderSettings.ambientLight.b);
		SfogColor = new Color (RenderSettings.fogColor.r, RenderSettings.fogColor.g, RenderSettings.fogColor.b);
	}

    // 2019 03 - broken
	float getRatioAlongSpline() {
        return 0;
	}

	void Update () {

		// Non-spline mode - only for setting lighting near door entrances or one-way plcaces (otherwise use smooth transition)
		if (mode == MODE.WaitForPlayer) {

			t_transition = 0;
			if (!usesSpline) {
				if (HF.PointInOABB(player.position,startgate1)) {
					mode = MODE.Transitioning;	
					setStartState (); 
				}
			} else {
				if (HF.PointInOABB(player.position,startgate1) || HF.PointInOABB(player.position,startgate2)) {
					mode = MODE.Transitioning;
					// Don't need to set start state since it was already set at the start of the Scene.
				}
			}
		} else if (mode == MODE.Transitioning) {
			t_transition += Time.deltaTime;
			float t = 0;
			if (usesSpline) {
				t = getRatioAlongSpline();
			} else {
				t = Mathf.SmoothStep(0,1,t_transition/transitionTime);
			}

			if (lightsToFadeIn != null) {
				Light l = null;
				for (int i = 0; i < lightsToFadeIn.Length;i++) {
					l = lightsToFadeIn[i];
					l.enabled = true;
					l.intensity = Mathf.Lerp(0,lightsToFadeInIntensities[i],t); 
				}
			}


			Camera.main.backgroundColor = Color.Lerp(SbackgroundColor,backgroundColor,t);
			RenderSettings.ambientLight = Color.Lerp(SambientLight,ambientLight,t);
			RenderSettings.fogColor= Color.Lerp(SfogColor,fogColor,t);


			RenderSettings.fogDensity = Mathf.Lerp(SfogDensity,fogDensity,t);
			RenderSettings.fogStartDistance = Mathf.Lerp(SfogStart,fogStart,t);
			RenderSettings.fogEndDistance = Mathf.Lerp(SfogEnd,fogEnd,t);

			if (!usesSpline) {
				if (t_transition > transitionTime) {
					mode = MODE.Done;
				}
			} else {
				if (HF.PointInOABB(player.position,endgate1) || HF.PointInOABB(player.position,endgate2)) {
					mode = MODE.WaitForPlayer;
				}
			}
		}


		
	}


}
