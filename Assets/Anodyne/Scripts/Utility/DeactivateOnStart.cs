using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeactivateOnStart : MonoBehaviour {
    public string FlagMustbeNonZeroToBeOn = "";
	void Start () {
        if (FlagMustbeNonZeroToBeOn != "") {
            if (FlagMustbeNonZeroToBeOn == "NONE") {

            } else if (DataLoader.instance.getDS(FlagMustbeNonZeroToBeOn) == 0) {
                gameObject.SetActive(false);
            }
        } else {
            gameObject.SetActive(false);
        }
	}
}
