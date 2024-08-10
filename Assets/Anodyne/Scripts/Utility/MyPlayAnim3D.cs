using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlayAnim3D : MonoBehaviour {

    public string animToPlay = "name";
    // Use this for initialization
    float delay = 0.1f;
	void Start () {
	}
    bool done = false;
	// Update is called once per frame
	void Update () {
        if (done) return;
        delay -= Time.deltaTime;
        if (delay < 0) {
            done = true;
            GetComponent<Animator>().Play(animToPlay);
         //   print(animToPlay);
        }
    }
}
