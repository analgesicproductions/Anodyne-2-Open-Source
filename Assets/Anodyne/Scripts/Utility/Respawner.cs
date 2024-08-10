using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawner : MonoBehaviour {

    // Should exist in hierarch
    // Root
    // -- REspawnPoint
    // ------ Point
    // -- Respawner
    // -- Respawner ..

    public bool IsRespawnPoint = false;

    public string CurrentRespawnPoint = "";
    UIManagerAno2 ui;
    MediumControl player;
    MediumControl ridescale;
	void Start () {
        ui = HF.Get3DUI();
        HF.GetPlayer(ref player);
	}

    int mode = 0;
    float tFade = 0;
	void Update () {
		if (!IsRespawnPoint) {
            if (mode == 1) {
                if (!ui.isFading()) {
                    CameraTrigger.PausedByCameraTrigger = true;
                    ui.StartFade(0.25f, true, 1);
                    mode = 2;
                }
            }  else if (mode == 2) {
                tFade += Time.deltaTime;
                if (!ui.isFading() && tFade > 0.7f) {
                    if (movingRS) {
                        newpos.y += 0.5f;
                        ridescale.transform.position = newpos;
                        ridescale.accelTime = 0;
                        ridescale.rb.velocity = Vector3.zero;
                    } else {
                       player.transform.position = newpos;
                    }

                }

                if (tFade > 0.7f) {
                    if (!ui.isFading()) {
                        tFade = 0;
                        mode = 3;
                        ui.StartFade(0.25f, false);
                        CameraTrigger.PausedByCameraTrigger = false;
                    }
                }
            } else if (mode == 3 ) {
                if (!ui.isFading()) {
                    mode = 0;
                }
            }
        }
	}

    Vector3 newpos;
    bool movingRS = false;
    private void OnTriggerEnter(Collider other) {
        if (other.name != "MediumPlayer" && other.name != "BigPlayer") return;
        if (other.name == "BigPlayer") {
            ridescale = GameObject.Find("BigPlayer").GetComponent<MediumControl>();
            movingRS = true;
        } else {
            movingRS = false;
        }
       
        int childcount = transform.parent.childCount;
        GameObject child = null;
        Respawner r = null;
        for (int i = 0; i < childcount; i++) {
            child = transform.parent.GetChild(i).gameObject;
            r = child.GetComponent<Respawner>();
            if (r != null) {
                // Set all triggers to send player here
                if (IsRespawnPoint) {
                    if (!r.IsRespawnPoint) {
                        r.CurrentRespawnPoint = name;
                    }
                } else {  // Get pos of current trigger and move player
                    if (r.IsRespawnPoint) {
                        if (CurrentRespawnPoint == "") return;
                        newpos = transform.parent.Find(CurrentRespawnPoint).Find("Point").position;
                        mode = 1;
                    }
                }
            }
        }
    }
}
