using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightOscillator : MonoBehaviour {

	// Use this for initialization

	Light l;

	float initIntensity;
	float initRange;
	public float ampRange = 4f;
	public float ampIntensity = 1f;

	private float t;
	public float tm = 4f;
	public float start_delay = 0f;
	void Start () {
		l = GetComponent<Light>();
		initIntensity = l.intensity;
		initRange = l.range;
	}

	// Update is called once per frame
	void Update () {
		if (start_delay > 0) {
			start_delay  -= Time.deltaTime;
			return;
		}
		t += Time.deltaTime;
		if (t >= tm) t -= tm;

		float sin = Mathf.Sin(2*Mathf.PI * (t/tm));

		if (l.type == LightType.Point || l.type == LightType.Spot) {
			l.range = initRange + ampRange*sin;
		}
		l.intensity = initIntensity + ampIntensity*sin;

	}
}
