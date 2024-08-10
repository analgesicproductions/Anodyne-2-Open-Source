using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oscillate : MonoBehaviour {

	// Use this for initialization
	public float offset = 1f;
	private float t;
	public float tm = 4f;
	private float init_y;
	private Vector3 init;
	public float start_delay = 0f;
    public bool local = false;
    public bool doXToo = false;
    public bool doScale = false;

    private void Awake() {

        if (local) {
            init_y = transform.localPosition.y;
            init = transform.localPosition;
        } else {
            init_y = transform.position.y;
            init = transform.position;
        }

        if (doScale) {
            init = transform.localScale;
            init_y = transform.localScale.y;
        }
    }


    float cacheInitY;
    Vector3 cacheInit;
    int tempinvis = 0;
    public void UseTempPos(float x, float y) {
        tempinvis = 3;
        GetComponent<UnityEngine.UI.Image>().enabled = false;

        poscached = true;
        cacheInitY = init_y;
        cacheInit = init;
        init.x = x; init.y = y;
        init_y = y;
    }
    bool poscached = false;
    public void UncachePos() {
        if (!poscached) return;
        poscached = false;
        init = cacheInit;
        init_y = cacheInitY;
    }

    Vector3 tempp = new Vector3();
    public bool roundToInt = false;
	void Update () {

        if (tempinvis > 0) {
            tempinvis--;
            if (tempinvis == 0) {
                GetComponent<UnityEngine.UI.Image>().enabled = true;
            }
        }
		if (start_delay > 0) {
			start_delay  -= Time.deltaTime;
			return;
		}
		t += Time.deltaTime;
		if (t >= tm) t -= tm;


        if (doScale) {
            float val = init.x + offset * Mathf.Sin(6.28f * (t / tm));
            tempp.Set(val, val, val);
            transform.localScale = tempp;
            return;
        }

        if (doXToo) {
            tempp.Set(init.x + offset*Mathf.Cos(6.28f*(t/tm)), init_y + offset * Mathf.Sin(2 * Mathf.PI * (t / tm)), init.z);
        } else {
            tempp.Set(init.x, init_y + offset * Mathf.Sin(2 * Mathf.PI * (t / tm)), init.z);
        }
        if (roundToInt) {
            tempp.x = Mathf.RoundToInt(tempp.x);
            tempp.y = Mathf.RoundToInt(tempp.y);
            tempp.z = Mathf.RoundToInt(tempp.z);
        }
        if (local) {
            transform.localPosition = tempp;
        } else {
            transform.position = tempp;
        }
    }
}
