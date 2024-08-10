using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeInactiveBasedOnState : MonoBehaviour {


	public bool deactivateInStart = true;
	public string thisFlagMustBe0 = "";
	// Use this for initialization
	void Start () {

		if (deactivateInStart) {
			if (check()) {
				gameObject.SetActive(false);
			} else if (thisFlagMustBe0 == "") {
                gameObject.SetActive(false);
            }
		}
	}

	bool check() {
		if (DataLoader.instance.getDS(thisFlagMustBe0) != 0) return true;
		return false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
