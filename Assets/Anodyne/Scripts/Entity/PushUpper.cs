using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushUpper : MonoBehaviour {

	public float y_force = 50f;

	void OnTriggerStay(Collider other) {
		if (other.CompareTag("Player")) {
			Rigidbody rb = other.GetComponent<Rigidbody>();
			rb.AddForce(0,y_force,0);
		}
	}
}
