using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacePlayer : MonoBehaviour {


	GameObject player;
	// Use this for initialization
	void Start () {
		player = GameObject.Find("OverworldPlayer");
		Vector3 sc = transform.localScale;
		sc.Set(0,0,0);
		transform.localScale = sc;
	}
	
	// Update is called once per frame
	void Update () {
		transform.LookAt(player.transform);
		transform.Rotate(90,0,0);

		if (Input.GetKey(KeyCode.Space)) {
			Vector3 sc = transform.localScale;
			sc.x += Time.deltaTime;
			sc.y += Time.deltaTime;
			sc.z += Time.deltaTime;
			transform.localScale = sc;
		}
	}
}
