using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


public class CameraTrigger : MonoBehaviour {

	public float fadeInTime = 0.25f;
    CinemachineBrain brain;
    public CinemachineVirtualCamera virtualCameraToUse;
    MediumControl player;
    UIManagerAno2 ui;
    [Tooltip("Uses the Cinemachine Brain's default transition between VCs")]
    public bool UsesDefaultTransition = false;

	void Start () {
        HF.GetPlayer(ref player);
        brain = Camera.main.GetComponent<CinemachineBrain>();
        ui = GameObject.Find("3D UI").GetComponent<UIManagerAno2>();
	}

    float tmFade;
    int mode = 0;
    public static string prevVCName = "";
    public static int prevVCPriority = 0;
    public static bool PausedByCameraTrigger = false;
	void Update () {
        if (mode == 0) {
            if (OnPlayer) {

                OnPlayer = false;
                mode = 1;
                if (UsesDefaultTransition) {
                    tmFade = -0.1f;
                } else {
                    // UI is already fading out when entering the scene, so dont start a new fade (so the camera transition is hidden)
                    if (ui.fadeMode == 2) {
                        tmFade = -0.1f;
                    } else {
                        ui.StartFade(fadeInTime, true);
                        tmFade = fadeInTime + 0.05f;
                    }
                    PausedByCameraTrigger = true;
                }
            }
        } else if (mode == 1) {
            // screen is black, make the camera change
            tmFade -= Time.deltaTime;
            if (tmFade < -0.1f) {
                // if vcam is null then return to normal view
                if (prevVCName != "" && GameObject.Find(prevVCName) != null) {
                    GameObject.Find(prevVCName).GetComponent<CinemachineVirtualCamera>().Priority = prevVCPriority;
                    prevVCName = "";
                }

                if (virtualCameraToUse == null) {
                    if (smoothsFromVCtoNormal) {
                        Camera.main.GetComponent<MedBigCam>().smoothLerpFromVCToNormal(tmSmoothsFromVCtoNormal);
                    } else {
                        Camera.main.GetComponent<MedBigCam>().setCameraEulerYRotation(Camera.main.transform.eulerAngles.y);
                    }
                    brain.enabled = false;
                    prevVCName = "nullCamTrig_";
                    Camera.main.fieldOfView = SaveManager.fieldOfView;
                } else {
                    brain.enabled = true;
                    if (UsesDefaultTransition) {
                        brain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
                    } else {
                        player.SetFixedCamEulerYTillNotPressingDirection(Camera.main.transform.eulerAngles.y);
                        brain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
                    }
                    prevVCPriority = virtualCameraToUse.Priority;
                    virtualCameraToUse.Priority = 1000;
                    prevVCName = virtualCameraToUse.name;
                }

                if (UsesDefaultTransition) {
                } else {
                    ui.StartFade(fadeInTime, false);
                    PausedByCameraTrigger = false;
                    tmFade = fadeInTime + 0.05f;
                }
                mode = 2;
            }
        } else if (mode == 2) {
            tmFade -= Time.deltaTime;
            if (tmFade < 0) {
                mode = 0;
            }
        }

	}
    [Tooltip("If true, then if virtualCameraToUse is null, when this trigger is activated, camera will transition using the tm below")]
    public bool smoothsFromVCtoNormal = false;
    public float tmSmoothsFromVCtoNormal = 3f;
    bool OnPlayer = false;
    public bool turnOnBrain = false;
	void OnTriggerEnter(Collider c) {
		if (c.CompareTag("Player")) {
            if (turnOnBrain) {
                brain.enabled = true;
            }
            // Ignore fading from same trigger
            if (virtualCameraToUse != null && prevVCName == virtualCameraToUse.name) {
                return;
            // ignore 
            } else if (virtualCameraToUse == null && !brain.enabled ) {
                return;
            // ignore same name
            } else if (virtualCameraToUse != null && brain.enabled && brain.ActiveVirtualCamera != null && brain.ActiveVirtualCamera.Name == virtualCameraToUse.name) {
                return;
            }
            OnPlayer = true;
		}
	}
}
