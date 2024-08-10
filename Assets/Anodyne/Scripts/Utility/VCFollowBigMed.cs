using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class VCFollowBigMed : MonoBehaviour {

    GameObject walkscale;
    GameObject ridescale;
    CinemachineVirtualCamera vc;
    public bool alsoLookAt = false;
    bool noride = false;
	void Start () {
        vc = GetComponent<CinemachineVirtualCamera>();
        walkscale = GameObject.Find(Registry.PLAYERNAME3D_Walkscale);
    }
    int mode = 0;
	void Update () {
        if (ridescale == null && !noride) {
            if (null == Camera.main.GetComponent<MedBigCam>().GetBigPlayerControl()) {
                noride = true;
                return;
            }
            ridescale = Camera.main.GetComponent<MedBigCam>().GetBigPlayerControl().gameObject;
        }
        if (mode == 0) {
            if (!noride && ridescale != null && ridescale.activeInHierarchy) {
                mode = 1;
                vc.m_Follow = ridescale.transform;
                if (alsoLookAt) {
                    vc.m_LookAt = ridescale.transform;
                }
            }
        } else if (mode == 1) {
            if (walkscale.activeInHierarchy) {
                mode = 0;
                vc.m_Follow = walkscale.transform;
                if (alsoLookAt) {
                    vc.m_LookAt = walkscale.transform;
                }
            }
        }
    }
}
