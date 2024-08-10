using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerChecker : MonoBehaviour {

    public string ThingToCheckFor = "";
    [System.NonSerialized]
    public bool onThingToCheckFor = false;
    [System.NonSerialized]
    public bool onPlayer2D = false;

    public bool checkForSpark = false;

    // rn the namecheck... only works when UsesJustTriggered set to true
    public bool NameCheckJustNeedsSubstring = false;
    public bool UsesJustTriggered = false;
    float tJustTriggered = 0;
    public float tmJustTriggered = 0.1f;
    [System.NonSerialized]
    public Anodyne.Vacuumable vac;


    public bool JustTriggered() {
        return tJustTriggered > 0;

    }

    private void Update() {
        if (UsesJustTriggered) {
            if (tJustTriggered > 0) {
                tJustTriggered -= Time.deltaTime;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {


        if (checkForSpark) {
            if (collision.GetComponent<Anodyne.Spark2D>() && collision.GetComponent<Anodyne.Spark2D>().IsAlive()) {
                onThingToCheckFor = true;
            } else if (collision.name == Registry.PLAYERNAME2D) {
                onPlayer2D = true;
            }
            return;
        }

        if (UsesJustTriggered) {
            if (NameCheckJustNeedsSubstring && collision.name.IndexOf(ThingToCheckFor) != -1) {
                vac = collision.GetComponent<Anodyne.Vacuumable>();
                if (vac != null && vac.isMoving()) {
                    tJustTriggered = tmJustTriggered;
                    vac = collision.GetComponent<Anodyne.Vacuumable>();
                }
                return;
            }
        }

        if (collision.name == Registry.PLAYERNAME2D) {
            onPlayer2D = true;
        } else if (collision.name == ThingToCheckFor) {
            onThingToCheckFor = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {


        if (collision.name == Registry.PLAYERNAME2D) {
            onPlayer2D = false;
        } else if (collision.name == ThingToCheckFor) {
            onThingToCheckFor = false;
        }
    }

}
