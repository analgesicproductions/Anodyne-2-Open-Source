using UnityEngine;
using System.Collections;

namespace Anodyne {
    public class BreakableWall : MonoBehaviour {

        SpriteAnimator animator;

        // Use this for initialization
        void Start() {
             animator = GetComponent<SpriteAnimator>();

        }

        // Update is called once per frame
        void Update() {

            if (isBreaking) {
                if (animator.isPlaying == false) {
                    GetComponent<SpriteRenderer>().enabled = false;
                }
            }
        }

        bool isBreaking = false;
        private void OnCollisionEnter2D(Collision2D collision) {
            if (isBreaking) return;
            Vacuumable rock = collision.gameObject.GetComponent<Vacuumable>();
            if (rock != null && (rock.isMoving() || rock.JustBrokeResetExternally)) {
                rock.GetComponent<Rigidbody2D>().velocity = rock.preCollisionVelocity;
                GetComponent<BoxCollider2D>().isTrigger = true;
                isBreaking = true;
                animator.ForcePlay("break");
            }
        }
    }
}

