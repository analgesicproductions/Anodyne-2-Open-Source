using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionChecker : MonoBehaviour {

    public float requiredVelocityToBeHurt = 4f;
    public bool onlyChecksForMovingVacs = false;

    bool justCollidedWithPlayer = false;

    bool justCollided = false;
    public bool checkAndResetJustCollided() {
        if (justCollided) {
            justCollided = false;
            return true;
        } else {
            return false;
        }
    }


    public bool checkAndResetJustCollidedPlayer() {
        if (justCollidedWithPlayer) {
            justCollidedWithPlayer = false;
            return true;
        } else {
            return false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (onlyChecksForMovingVacs && collision.collider.GetComponent<Anodyne.Vacuumable>() != null) {
            if (collision.relativeVelocity.magnitude > requiredVelocityToBeHurt) {
                justCollided = true;
            }
        } else if (collision.collider.name == Registry.PLAYERNAME2D) {
            justCollidedWithPlayer = true;
        }
    }
}
