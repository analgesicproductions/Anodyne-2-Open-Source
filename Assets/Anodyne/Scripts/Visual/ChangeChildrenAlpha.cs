using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeChildrenAlpha : MonoBehaviour {

    public bool childrenAreSkinMaterials = false;
    public bool smoothStep = true;
    public float lerpTime = 1.5f;
    public float startAlpha = 0f;
    public float endAlpha = 1f;
    public bool waitForSignal = true;

    MeshRenderer[] mrs;

    float t = 0;
	// Use this for initialization
	void Start () {

        mrs = new MeshRenderer[transform.childCount];

        for (int i =0; i < transform.childCount; i++) {
            mrs[i] = transform.GetChild(i).GetComponent<MeshRenderer>();
        }
	}
	
    public void StartFade() {
        waitForSignal = false;
    }

    bool done = false;
	// Update is called once per frame
	void Update () {
        if (waitForSignal) return;
        if (done) return;
        t += Time.deltaTime;
        float nextAlpha = Mathf.SmoothStep(startAlpha, endAlpha, t / lerpTime);
        Color c = mrs[0].material.GetColor("_TintColor");
        c.a = nextAlpha;
        for (int i = 0; i < transform.childCount; i++) {
            mrs[i].material.SetColor("_TintColor", c);
        }

        if (t >= lerpTime) done = true;
 	}
}
