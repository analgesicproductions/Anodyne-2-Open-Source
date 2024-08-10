using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCamAroundPt : MonoBehaviour {


	public float rotateSpeedPerSecond = 24f;
	// Use this for initialization
	public Camera cam;
	public Transform origin;
	void Start () {
		
	}

	// Update is called once per frame
	void Update () {

		cam.transform.RotateAround(origin.position,Vector3.up,rotateSpeedPerSecond*Time.deltaTime);
	}
}
