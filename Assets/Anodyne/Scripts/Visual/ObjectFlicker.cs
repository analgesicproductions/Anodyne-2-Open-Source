using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectFlicker : MonoBehaviour {


	public float onTime = 0.2f;
	public float offTime = 0.2f;

	int mode = 0;
	float timer = 0;

	MeshRenderer mr;
	// Use this for initialization
	void Start () {

		mr = GetComponent<MeshRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		if (mr == null) return;
		if (mode == 0) {
			timer += Time.deltaTime;
			if (timer >= onTime) {
				timer = 0;
				mode = 1;
				mr.enabled = false;
			}
		} else if (mode == 1) {
			timer += Time.deltaTime;
			if (timer >= offTime) {
				timer = 0;
				mode = 0;
				mr.enabled = true;
			}
		}
		
	}
}
