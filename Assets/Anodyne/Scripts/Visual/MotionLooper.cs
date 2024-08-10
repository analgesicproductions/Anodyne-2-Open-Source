using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionLooper : MonoBehaviour {


	public Vector3 initVelocity;
	public float maxTime = 3;
	public float startTime = 0;
	// Use this for initialization

	Vector3 initpos;
	public Vector3 nextpos;
	void Start () {

		initpos = transform.position;
		nextpos = transform.position + initVelocity * startTime;

	}
	
	// Update is called once per frame
	void Update () {
		startTime += Time.deltaTime;
		if (startTime >= maxTime) startTime -= maxTime;
		nextpos = initpos + initVelocity * startTime;
		transform.position = nextpos;
	}
}
