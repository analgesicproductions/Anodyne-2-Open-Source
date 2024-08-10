using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

// Matches this CM vc's properties to the main camera
// Used so that when timeline starts a sequence that has multiple CM clips,
// this camera can be used as an initial camera in the timeline, if needed,
// so that you can get a smooth transition to the first shot
public class MatchCMCamToMainCam : MonoBehaviour {

    Camera mc;
    CinemachineVirtualCamera vc;
    CinemachineBrain brain;
    AnoControl2D player2D;
	void Start () {
        vc = GetComponent<CinemachineVirtualCamera>();
        mc = Camera.main;
        brain = mc.GetComponent<CinemachineBrain>();
        if (GameObject.Find("2D Ano Player") != null) {
            HF.GetPlayer(ref player2D);
        }
	}
	
	void Update () {

        bool Player2DInFixedCam = false;
        if (player2D != null) {
            Player2DInFixedCam = player2D.IsCinemachineOnButAllowingMovement();
        }

        if (brain.enabled == false && !AnoControl2D.CinemachineOn2D && !Player2DInFixedCam) {
            transform.position = mc.transform.position;
            transform.rotation = mc.transform.rotation;
            vc.m_Lens.FieldOfView = mc.fieldOfView;
            if (mc.orthographic) {
                vc.m_Lens.OrthographicSize = mc.orthographicSize;
            }
            vc.m_Lens.NearClipPlane = mc.nearClipPlane;
            vc.m_Lens.FarClipPlane = mc.farClipPlane;
        }
	}
}
