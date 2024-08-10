using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetInactiveBasedOnFlag : MonoBehaviour {

    public string FlagName = "";
    public int CompareValue = 0;
    public CMP_ comparer = CMP_.IsNotEqual;
    public enum CMP_ { IsEqual, IsNotEqual}
	void Start () {
        bool doit = false;
        if (comparer == CMP_.IsEqual) if (DataLoader.instance.getDS(FlagName) == CompareValue) doit = true;
        if (comparer == CMP_.IsNotEqual) if (DataLoader.instance.getDS(FlagName) != CompareValue) doit = true;
        if (doit) {
             gameObject.SetActive(false);
        }
    }
}
