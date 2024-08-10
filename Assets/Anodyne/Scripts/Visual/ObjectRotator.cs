using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRotator : MonoBehaviour {


	public float localXRotateSpeed = 10f;
	public float localYRotateSpeed = 10f;
	public float localZRotateSpeed = 10f;
	public bool rotateInWorld = true;
	// Use this for initialization
	void Start () {
		
	}

	// Update is called once per frame
	void Update () {

		if (rotateInWorld == false) {
			transform.Rotate(localXRotateSpeed*Time.deltaTime,localYRotateSpeed*Time.deltaTime,localZRotateSpeed*Time.deltaTime,Space.Self);
		} else {
			transform.Rotate(localXRotateSpeed*Time.deltaTime,localYRotateSpeed*Time.deltaTime,localZRotateSpeed*Time.deltaTime,Space.World);
		}
	}
}
