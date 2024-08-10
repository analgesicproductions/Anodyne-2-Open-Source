using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Anodyne {
    public class Boomer : MonoBehaviour {

        SpriteAnimator anim;
        Vacuumable vac;
        Rigidbody2D rb;

        public bool breaksOutsideRoom = false;
        public bool hasShield = false;
        public SpriteRenderer shieldSR;
        int shieldHealth = 0;

        int mode = 0;

        public List<GameObject> children;

        Vector2 initpos;
        void Start() {
            initpos = transform.position;
            vac = GetComponent<Vacuumable>();
            anim = GetComponent<SpriteAnimator>();
            rb = GetComponent<Rigidbody2D>();
            HF.GetPlayer(ref player);
            vac.BreaksWhenShotOutsideRoom = false;
            if (hasShield) {
                shieldHealth = 3;
                vac.IsBreakable = false;
            }
        }

        Vector2 tempVel = new Vector2();
        Vector2 tempPos = new Vector2();
        public float maxSpeed = 7f;
        public float accel = 5f;
        public float lerpStrength = 2.5f;


        void resetState() {
            if (hasShield) {
                shieldSR.enabled = true;
                vac.IsBreakable = false;
                shieldHealth = 3;
            }
            mode = 0;
            vac.enabled = true;
            rb.velocity = Vector2.zero;
            if (!vac.isBreaking()) {
                anim.Play("idle");
            }
            transform.position = initpos;
        }

        float tDamageCooldown = 0;
        void Update() {


            if (hasShield && shieldHealth < 3) {
                if (!player.InSameRoomAs(initpos)) {
                    resetState();
                }
            }

            if (mode == 0) {
                if (vac.isPickedUp()) {
                    tDamageCooldown = 0.25f;
                    mode = 1;
                    if (hasShield) {
                        shieldSR.enabled = false;
                    }
                }
            } else if (mode == 1) {
                if (vac.isMoving()) {
                    vac.enabled = false;
                    anim.Play("move");
                    if (hasShield && shieldHealth > 0) shieldSR.enabled = true;
                    if (Mathf.Abs(rb.velocity.x) > 0) {
                        mode = 2;
                    } else {
                        mode = 3;
                    }
                } else if (!player.InSameRoomAs(initpos)) {
                    // swallowed
                    resetState();
                }
            } else if (mode == 2) {
                if (tDamageCooldown > 0) tDamageCooldown -= Time.deltaTime;
                float xPos = transform.position.x;
                float px = player.transform.position.x;
                bool inRoom = player.InSameRoomAs(transform.position);
                tempVel = rb.velocity;
                if (xPos < px && rb.velocity.x < maxSpeed) {
                    tempVel.x += accel * Time.deltaTime;
                    if (!inRoom && rb.velocity.x < 0) tempVel.x += 2.5f * accel * Time.deltaTime; // reduce collision issues when going outside of the current room?
                } else if (xPos >= px && rb.velocity.x > -maxSpeed) {
                    tempVel.x -= accel * Time.deltaTime;
                    if (!inRoom && rb.velocity.x > 0) tempVel.x -= 2.5f * accel * Time.deltaTime;
                }
                tempVel.y = 0;
                rb.velocity = tempVel;

                tempPos = transform.position;
                tempPos.y = Mathf.Lerp(tempPos.y, player.transform.position.y, Time.deltaTime * lerpStrength);
                transform.position = tempPos;
                if (vac.isBroken()) {
                    mode = 4;
                }

                if (!player.InSameRoomAs(initpos)) {
                    resetState();
                }
            } else if (mode == 3) {
                if (tDamageCooldown > 0) tDamageCooldown -= Time.deltaTime;
                float yPos = transform.position.y;
                float py = player.transform.position.y;
                bool inRoom = player.InSameRoomAs(transform.position);

                if (!inRoom && breaksOutsideRoom) {
                    MyBreak();
                }

                tempVel = rb.velocity;
                if (yPos < py && rb.velocity.y < maxSpeed) {
                    tempVel.y += accel * Time.deltaTime;
                    if (!inRoom && rb.velocity.y < 0) tempVel.y += 2.5f * accel * Time.deltaTime;
                } else if (yPos >= py && rb.velocity.y > -maxSpeed) {
                    tempVel.y -= accel * Time.deltaTime;
                    if (!inRoom && rb.velocity.y > 0) tempVel.y -= 2.5f * accel * Time.deltaTime;
                }
                tempVel.x = 0;
                rb.velocity = tempVel;

                tempPos = transform.position;
                tempPos.x = Mathf.Lerp(tempPos.x, player.transform.position.x, Time.deltaTime * lerpStrength);
                transform.position = tempPos;

                if (vac.isBroken()) {
                    mode = 4;
                }

                if (!player.InSameRoomAs(initpos)) {
                    resetState();
                }
            } else if (mode == 4) { // Broken
                mode = 5;
                HF.SendSignal(children);
            } else if (mode == 5) {
                if (!player.InSameRoomAs(initpos)) {
                    resetState();
                }
            }



            if (tmFlickerShieldTime > 0) {
                tmFlickerShieldTime -= Time.deltaTime;
                tFlickerShield += Time.deltaTime;
                if (tFlickerShield > 0.05f) {
                    tFlickerShield -= 0.05f;
                    shieldSR.enabled = !shieldSR.enabled;
                }
                if (tmFlickerShieldTime < 0) {
                    shieldSR.enabled = true;
                }
            }
        }

        AnoControl2D player;

        private void OnCollisionEnter2D(Collision2D collision) {
            Collider2D col = collision.collider;
            bool isBreakableWhileFlying = mode == 2 || mode == 3;
            bool doBreak = false;
            if (vac.IsBeingSucked() == false && col.GetComponent<AnoControl2D>() != null) {

                if (tDamageCooldown <= 0 && !player.CameraIsChangingRooms() && player.InSameRoomAs(initpos)) {
                    col.GetComponent<AnoControl2D>().Damage(1);
                }

                if (tDamageCooldown <= 0 && isBreakableWhileFlying && vac.IsBreakable) {
                    doBreak = true;
                } else {
                    return;
                }
            }
            if (shieldHealth > 0) return;

            // Vacuumable is off, control breaking
            if (isBreakableWhileFlying && vac.IsBreakable) {
                doBreak = true;
            }

            if (doBreak) {
                MyBreak();
            }
            // Breaking while in mode = 0 handled by Vacuumable code
        }

        public void MaybeMyBreak() {
            if ((mode == 2 || mode == 3) && vac.IsBreakable) {
                MyBreak();
            }
        }

        public void MyBreak() {
            vac.enabled = true;
            vac.Break();
            mode = 4;
        }

        float tFlickerShield = 0f;
        float tmFlickerShieldTime = 0f;
        private void OnTriggerEnter2D(Collider2D collision) {
            if (collision.GetComponent<Spark2D>() != null && (collision.GetComponent<Spark2D>().justBroke  || collision.GetComponent<Spark2D>().IsAlive())) {
                if (hasShield && shieldHealth > 0) {
                    shieldHealth--;
                    AudioHelper.instance.playOneShot("sparkBarHit", 1, 0.9f + 0.2f * Random.value);
                    
                    if (shieldHealth == 0) {
                        vac.IsBreakable = true;
                        AudioHelper.instance.playOneShot("blockExplode");
                        shieldSR.enabled = false;
                        tmFlickerShieldTime = 0;
                    } else {
                        tmFlickerShieldTime = 0.5f;
                    }
                }
                return;
            }

        }
    }

}