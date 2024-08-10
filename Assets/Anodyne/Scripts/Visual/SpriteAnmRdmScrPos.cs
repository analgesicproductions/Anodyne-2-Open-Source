using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnmRdmScrPos : MonoBehaviour {

    int myorderid = 0;
    static int orderID = 0;
    public static float speedmul = 1;
    float interval = 0.30f;
    float startdelay = 0;
    static float globaltime = 0;
    float maxglobaltime;
    float prevglobaltime;
    bool playing = false;
    // .25f second anim
	// Use this for initialization
	void Start () {

        maxglobaltime = interval * 18;
        startdelay = orderID * interval;
        myorderid = orderID;
        orderID++;  
        GetComponent<SpriteRenderer>().enabled = false;
        globaltime = maxglobaltime + 0.01f;
    }

   public  static bool init = false;
	// Update is called once per frame

	void Update () {

        if (!init) return;

        if (globaltime > startdelay && prevglobaltime <= startdelay) {
            GetComponent<SpriteRenderer>().enabled = true;
            GetComponent<Anodyne.SpriteAnimator>().Play("explode");
            Vector3 pos = Camera.main.transform.position;
            pos = Camera.main.ViewportToWorldPoint(new Vector3(.1f + .8f*Random.value, .1f + .8f * Random.value, 0));
            pos.z = 0;
            transform.position = pos;
            playing = true;
        }
        if (playing) {
            if (GetComponent<Anodyne.SpriteAnimator>().isPlaying == false) {
                playing = false;
                GetComponent<SpriteRenderer>().enabled = false;
            }
        }

        prevglobaltime = globaltime;
		if (myorderid == 0) {
            if (globaltime >= maxglobaltime) {
                globaltime -= maxglobaltime;
                GetComponent<SpriteRenderer>().enabled = true;
                GetComponent<Anodyne.SpriteAnimator>().Play("explode");
                playing = true;
            }
            globaltime += Time.deltaTime*speedmul;
            if (Input.GetKeyDown(KeyCode.Q)) {
                speedmul += 0.25f;
            } else if (Input.GetKeyDown(KeyCode.E)) {
                speedmul -= 0.25f;
            }


        }

	}
}
