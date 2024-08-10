using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ano2Parallax : MonoBehaviour {


    public Vector2 initpos = new Vector2();
    public Vector2 parallax = new Vector2();
	// Use this for initialization
	void Start () {
        cam = Camera.main;

	}
    Camera cam;
    Vector3 newpos = new Vector3();
	
	// Update is called once per frame
	void Update () {
        float dx = cam.transform.position.x - initpos.x;
        float dy = cam.transform.position.y - initpos.y;
        newpos.x = cam.transform.position.x - dx * parallax.x;
        newpos.y = cam.transform.position.y - dy * parallax.y;
        transform.position = newpos;
    }
}
