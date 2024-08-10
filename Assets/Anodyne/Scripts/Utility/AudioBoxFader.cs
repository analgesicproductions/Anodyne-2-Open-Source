using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioBoxFader : MonoBehaviour {

//	BoxCollider bc;
	AudioSource src;
	public float maxFadeTime = 2.0f;
	public float minVol = 0f;
	public float maxVol = 1f;


	// Use this for initialization
	void Start () {
//		bc = GetComponent<BoxCollider>();
		src = GetComponent<AudioSource>();
	}

	int mode = 0;
	// Update is called once per frame
	void Update () {
		if (mode == 0) {
			if (src.volume > minVol) {
				src.volume -= (Time.deltaTime/maxFadeTime)*(maxVol-minVol);
			} else {
				src.volume = minVol;
			}
		} else if (mode == 1) {
			if (src.volume < maxVol) {
				src.volume += (Time.deltaTime/maxFadeTime)*(maxVol-minVol);
			} else {
				src.volume = maxVol;
			}
			
		}
	}

	void OnTriggerEnter(Collider other) {
		if (other.CompareTag("Player")) {
			mode = 1;
		}
	}
	void OnTriggerExit(Collider other) {
		if (other.CompareTag("Player")) {
			mode = 0;
		}
	}
}
