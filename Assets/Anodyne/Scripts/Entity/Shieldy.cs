using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Anodyne {
    public class Shieldy : MonoBehaviour {
        public List<GameObject> children;

        int mode = 0;

        public Vector2 StartVelocity = new Vector2(2.5f, 0);
        CircleCollider2D cc;
        Rigidbody2D rb;
        int layermask;

        // Use this for initialization
        void Start() {
            cc = GetComponent<CircleCollider2D>();
            rb = GetComponent<Rigidbody2D>();
            rb.velocity = StartVelocity;
            layermask = LayerMask.NameToLayer("Enemies");
            animator = GetComponent<SpriteAnimator>();
        }

        SpriteAnimator animator;
        // Update is called once per frame
        void Update() {
            if (isdying) {
                if (animator.isPlaying == false) {
                    gameObject.SetActive(false);
                }
                return;
            }
            if (isstunned) {
                if (!animator.isPlaying) {
                    animator.Play("move");
                    isstunned = false;
                    rb.velocity = StartVelocity;
                }
                return;
            }
            if (mode == 0) {
                if (Physics2D.Raycast(transform.position, StartVelocity, cc.radius + 0.4f,layermask)) {
                    StartVelocity *= -1;
                    rb.velocity = StartVelocity;
                }
            }
        }

        bool isdying = false;
        bool isstunned = false;
        private void OnCollisionEnter2D(Collision2D collision) {
            
            // Seems to work ok.. maybe lol
            if (collision.collider.GetComponent<Vacuumable>() != null) {
                Vector2 vel = collision.relativeVelocity;
                // Rel = velocity of the other relative to this (so if I move S and other is still, rel velocity is +y)
                // rel = Other.vel - Me.vel. To retrieve other.vel, rel.y + me.y

                rb.velocity = Vector2.zero;
                if (vel.y + StartVelocity.y < -1) {
                    mode = -1;
                    HF.SendSignal(children);
                    animator.Play("die");
                    isdying = true;
                } else {
                    isstunned = true;
                    animator.Play("stun");
                    Rigidbody2D rba = collision.collider.GetComponent<Rigidbody2D>();
                    rba.velocity = -1 * (collision.relativeVelocity + StartVelocity);
                }
            }
        }
    }
}