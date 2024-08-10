using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Anodyne;
using UnityEngine.Tilemaps;
namespace Anodyne {

    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Vacuumable))]
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(SpriteAnimator))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class BounceMush : MonoBehaviour {

        // Boilerplate refs
        SpriteAnimator animator;
        Rigidbody2D rb;
        Vacuumable vac;
        AnoControl2D player;
        CircleCollider2D cc;
        EntityState2D state;

        // State related to vacuumable, changes roughly based on vacuumable's state, but 
        // entities use these to add events when changing states
        bool vacBroken = false;
        bool vacBreaking = false;

        // Entity specific
        public Vector2 InitialVel = new Vector2(0, 2);
        int mode = 0;

        void Start() {
            animator = GetComponent<SpriteAnimator>();
            rb = GetComponent<Rigidbody2D>();
            vac = GetComponent<Vacuumable>();
            cc = GetComponent<CircleCollider2D>();
            state = GetComponent<EntityState2D>();
            vac.overrideIdleDeceleration = true;

            player = GameObject.Find("2D Ano Player").GetComponent<AnoControl2D>();
            Reinitialize();
        }

        void Reinitialize() {
            transform.position = state.initPos;
            state.spawnTimer = state.initSpawnTimer;
            vacBroken = vacBreaking = false;
            rb.velocity = Vector2.zero;

            if (state.spawnTimer > 0) {
                vac.enabled = false;
                GetComponent<CircleCollider2D>().enabled = false;
                GetComponent<SpriteRenderer>().enabled = false;
            } else {
                vac.enabled = true;
                GetComponent<CircleCollider2D>().enabled = true;
                GetComponent<SpriteRenderer>().enabled = true;
            }
        }

        void Update() {
            if (!state.initializedEntity) {
                state.initializedEntity = true;
                HF.GetRoomPos(transform.position, ref state.curRoomPos);
            }

            if (vacBroken) {
                if (vac.state == Vacuumable.VacuumMode.Idle) {
                    vacBroken = false;
                    HF.GetRoomPos(transform.position, ref state.curRoomPos); // Reset room since it could have changed
                }
                return;
            }
            if (vacBreaking) {
                if (animator.isPlaying == false) {
                    vacBreaking = false;
                    vacBroken = true;
                    GetComponent<CircleCollider2D>().enabled = false;
                    GetComponent<SpriteRenderer>().enabled = false;
                    HF.SendSignal(state.children);
                }
                return;
            }

            if (state.spawnTimer > 0) {
                state.spawnTimer -= Time.deltaTime;
                if (state.spawnTimer < 0) {
                    vac.enabled = true;
                    GetComponent<CircleCollider2D>().enabled = true;
                    GetComponent<SpriteRenderer>().enabled = true;
                } else {
                    return;
                }
            }

            if (state.UpdateTouchDamage(player)) {
                player.BumpInDir(state.tempVel.x*2, state.tempVel.y*2);
            }

            if (mode == 0) {
                rb.velocity = InitialVel;
                state.tempVel = rb.velocity;
                mode = 1;
            } else if (mode == 1) {
                rb.velocity = state.tempVel;
                if (doBounceHorSurface) {
                    doBounceHorSurface = false;
                    state.tempVel = rb.velocity;
                    state.tempVel.y *= -1;
                    rb.velocity = state.tempVel;
                }

                if (doBounceVertSurface) {
                    doBounceVertSurface = false;
                    state.tempVel = rb.velocity;
                    state.tempVel.x *= -1;
                    rb.velocity = state.tempVel;
                }

            }

            // allow suck to influence velocity
            // reset when leaving room
            // bounce at edges

            if (state.UpdateRoomIfVacIsNonIdle(vac, transform)) {
                return;
            }


            state.tempPos = transform.position;
            HF.ConstrainVecToRoom(ref state.tempPos, state.curRoomPos.x, state.curRoomPos.y);
            transform.position = state.tempPos;
        }


        public void SendSignal(string signal) {
            if (signal == "reset") {
                animator.Play("idle");
                Reinitialize();
            }
        }

        bool doBounceHorSurface = false;
        bool doBounceVertSurface = false;
        private void OnCollisionEnter2D(Collision2D collision) {
            bool dobreak = false;
            //if (collision.gameObject.GetComponent<Spike>() != null)
            //}
            if (collision.collider.CompareTag("Player")) {
                state.onPlayer = true;
            } 

            ContactPoint2D[] pts = new ContactPoint2D[2];
            collision.GetContacts(pts);
            ContactPoint2D pt = pts[0];
            if (pt.point.x <  transform.position.x - cc.radius/2 || pt.point.x > transform.position.x + cc.radius / 2) {
                doBounceVertSurface = true;
            } else {
                doBounceHorSurface = true;
            }


            if (dobreak) {
                animator.Play("break");
                vac.Break(true);
                rb.velocity = Vector2.zero;
                vacBreaking = true;
            }
        }

        private void OnCollisionExit2D(Collision2D collision) {
            if (collision.collider.CompareTag("Player")) {
                state.onPlayer = false;
            }
        }
    }
}