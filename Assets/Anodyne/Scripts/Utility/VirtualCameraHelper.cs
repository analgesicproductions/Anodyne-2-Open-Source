using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class VirtualCameraHelper : MonoBehaviour {

    float movementRate = 0;
    int movementStyle = 0;
    float pathDestination = 0;
    float pathStart = 0;
    float totalLerpTime = 0;
    CinemachineVirtualCamera vc;
    [Header("GIF Help")]
    public bool RequireKeypress = false;
    public float GIF_Destination = 1;
    public float GIF_Start = 0;
    [Header("GIF Linear Movement")]
    public bool GIF_MoveOnStart = false;
    public float GIF_movementRate = 0;
    [Header("GIF Lerping")]
    public bool GIF_SmoothLerpPingpong = false;
    public float GIF_Lerptime = 1;
    CinemachineTrackedDolly dolly;

	void Start () {
        vc = GetComponent<CinemachineVirtualCamera>();
        dolly = vc.GetCinemachineComponent<CinemachineTrackedDolly>();
        if (GIF_SmoothLerpPingpong) {
            StartLerpingToPathPosition(GIF_Destination, GIF_Start, GIF_Lerptime);
        } else if (GIF_MoveOnStart) {
            movementRate = movementRate + 1;
        }
	}

    public void MoveToPathPosition(float destination) {
        dolly.m_PathPosition = destination;
    }

    public void StartLerpingToPathPosition(float _pathDestination, float _pathStart, float _time) {
        DoMove = true;
        movementStyle = 0;
        pathDestination = _pathDestination;
        pathStart = _pathStart;
        totalLerpTime = _time;
        dolly.m_PathPosition = _pathStart;
        m = 0;
    }

    public void StartMovingToPathPosition(float _pathDestination = 0, float _movementrate = 1, int _movementStyle = 0) {
        DoMove = true;
        pathDestination = _pathDestination;
        movementStyle = _movementStyle;
        movementRate = _movementrate;
    }

    bool DoMove = false;
    int m = 0;
    float t = 0;
	void Update () {
        if (RequireKeypress) {
            if (MyInput.jpConfirm) {
                RequireKeypress = false;
            }
            return;
        }
		if (DoMove) {
            // Smoothed, pingpong.
            if (movementStyle == 0) {
                if (m == 0) {
                    t += Time.deltaTime;
                    dolly.m_PathPosition = Mathf.SmoothStep(pathStart, pathDestination, t / totalLerpTime);
                    if (t >= totalLerpTime) {
                        m = 1;
                        t = 0;
                    }
                } else if (m == 1) {
                    t += Time.deltaTime;
                    if (t > 1) {
                        m = 2;
                        t = 0;
                    }
                } else if (m == 2) {
                    t += Time.deltaTime;
                    dolly.m_PathPosition = Mathf.SmoothStep(pathDestination, pathStart, t / totalLerpTime);
                    if (t >= totalLerpTime) {
                        m = 3;
                    }
                }
            }
        }
	}
}
