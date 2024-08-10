using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DustFloater : MonoBehaviour {


    SpriteRenderer shadow;
    
	// Use this for initialization
	void Start () {
        shadow = transform.Find("Shadow").GetComponent<SpriteRenderer>();

        startfall(falldistance);
	}
    int mode = 0;

    public float amp = 0.4f;
    public float period = 1.8f;
    float t = 0;

    public float fallspeed = 1f;
    public float falldistance = 3f;

    Anodyne.Vacuumable vac;
    Vector2 groundpos = new Vector2();
    Vector2 offset = new Vector2();

    public void startfall(float distance=3f) {
        falldistance = distance;
        groundpos = transform.position;
        groundpos.y += falldistance;
        transform.position = groundpos;
        groundpos.y -= falldistance;
        shadow.transform.position = groundpos;
        offset.y = falldistance;
        t = Random.value * period;
        vac = GetComponent<Anodyne.Vacuumable>();
    }
	void Update () {

        vac.CanBeSucked_SetExternally = true;
        if (falldistance > 0.75f) vac.CanBeSucked_SetExternally = false;


		if (mode == 0) {

            groundpos = transform.position;
            groundpos.x -= offset.x;
            offset.x = amp * Mathf.Sin(6.28f * (t / period));
            t += Time.deltaTime;
            if (t >= period) t -= period;
            groundpos.x += offset.x;


            groundpos.y -= offset.y;

            falldistance -= Time.deltaTime * fallspeed;
            offset.y = falldistance;
            shadow.transform.position = groundpos;
            if (falldistance <= 0) {
                offset.y = 0;
                mode = 1;
            }

            groundpos.y += offset.y;
            transform.position = groundpos;

            if (mode == 1) {
                shadow.transform.position = transform.position;
                shadow.gameObject.SetActive(false);
            }

        } else if (mode == 1) {

        }
	}
}
