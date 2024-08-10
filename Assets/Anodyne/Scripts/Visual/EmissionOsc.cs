using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmissionOsc : MonoBehaviour {

	public float tm = 2;
	public float min = 0;
		public float max = 0.1f;
	public Color col;

	float t;

	Material m = null;

	// Use this for initialization
	void Start () {
		m = GetComponent<MeshRenderer>().material;
		m.EnableKeyword("_EMISSION");
	}
	
	// Update is called once per frame
	void Update () {
		t += Time.deltaTime;
		if (t > tm) t -= tm;

		float deg = 360f * (t/tm);
		deg *= Mathf.Deg2Rad;
		deg = (Mathf.Sin(deg) + 1)/2.0f; // send to 0,1
		deg = min + max*deg;

		m.SetColor("_EmissionColor",Color.Lerp(Color.black,col,deg));
	}
}
