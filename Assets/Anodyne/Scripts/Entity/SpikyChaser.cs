using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Anodyne {
    public class SpikyChaser : MonoBehaviour {

        Rigidbody2D rb;
        Vacuumable vac;
        AnoControl2D player;
        EntityState2D state;
        SpriteAnimator anim;
        SpriteRenderer sr;
        CircleCollider2D cc;

        public SpriteRenderer shieldSR;
        public bool hasShield = false;
        int shieldHealth = 0;
        float tFlickerShield = 0;
        float tFlickerShieldTime = 0;

        // for the hardGem variant
        float sparkHealth = 3;
        float tFlicker = 0;
        float tFlickerInterval = 0;
        float tmFlickerInterval = 0.05f;

        public bool picoSparkImmune = false;
        public float movementVel = 3f;
        void Start() {
            state = GetComponent<EntityState2D>();
            cc = GetComponent<CircleCollider2D>();
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponent<SpriteRenderer>();
            HF.GetPlayer(ref player);
            vac = GetComponent<Vacuumable>();
            vac.overrideIdleDeceleration = true;
            state = GetComponent<EntityState2D>();
            anim = GetComponent<SpriteAnimator>();
            state.usesDefaultPlayerDetection = false;

            if (name == "CerealZera" || name == "MilkZera") {
                sparkHealth = 10;
            }
            if (name == "ReguloidZera") {
                sparkHealth = 1;
            }
            if (name == "HackSnowmanDONTRENAME") {
                chestHackMode = 1;
                hackChest = GameObject.Find("HackChestDONTRENAME").GetComponent<Chest>();
            }

            if (hasShield) {
                shieldHealth = 3;
                vac.IsBreakable = false;
            }
        }
        Chest hackChest;
        GameObject hackChestObj;
        int chestHackMode = 0;

        int mode = 0;
        public bool followsPlayer = true;
        public bool isSnowman = false;
        public float snowmanSpawnRadius = 3f;
        float t_snowmanSuicide = 0;
        Vector2 tempVel = new Vector2();
        void Update() {
            if (player.IsDying()) return;


            if (tFlickerShieldTime > 0) {
                tFlickerShieldTime -= Time.deltaTime;
                tFlickerShield += Time.deltaTime;
                if (tFlickerShield > 0.05f) {
                    tFlickerShield -= 0.05f;
                    shieldSR.enabled = !shieldSR.enabled;
                }
                if (tFlickerShieldTime < 0) {
                    shieldSR.enabled = true;
                }
            }
            if (hasShield && shieldHealth < 3 && !player.InSameRoomAs(transform.position)) {
                shieldHealth = 3;
                shieldSR.enabled = true;
                vac.IsBreakable = false;
                tFlickerShieldTime = 0;
                transform.position = state.initPos;
            }

            // Scene start. both before/active r visible
            if (chestHackMode == 1) {
                // If open, let after be visible
                if (hackChest.IsOpen()) {
                    GameObject.Find("SnowmenBefore").SetActive(false);
                    chestHackMode = 3;
                // else hide after
                } else {
                    hackChestObj = GameObject.Find("SnowmenAfter");
                    GameObject.Find("SnowmenAfter").SetActive(false);
                    chestHackMode = 2;
                }
            } else if (chestHackMode == 2) {
                if (hackChest.IsOpen()) {
                    GameObject.Find("SnowmenBefore").SetActive(false);
                    hackChestObj.SetActive(true);
                    chestHackMode = 3;
                }
            }


            if (tFlicker > 0) {
                tFlicker -= Time.deltaTime;
                tFlickerInterval += Time.deltaTime;
                if (tFlickerInterval > tmFlickerInterval) {
                    tFlickerInterval = 0;
                    GetComponent<SpriteRenderer>().enabled = !GetComponent<SpriteRenderer>().enabled;
                }
                if (tFlicker <= 0) GetComponent<SpriteRenderer>().enabled = true;
            }


            if (mode == 0) {
                if (HF.AreTheseInTheSameroom(transform, player.transform) && !player.CameraIsChangingRooms()) {
                    if (isSnowman) {
                        state.touchDmg = 2;
                        if (Vector2.Distance(transform.position, player.transform.position) < snowmanSpawnRadius) {
                            anim.Play("appear");
                            anim.ScheduleFollowUp("idle");
                            mode = 100;
                            AudioHelper.instance.playOneShot("darkRattle");
                        }
                        if (t_snowmanSuicide >= 4) {
                            t_snowmanSuicide = 0;
                            mode = 3;
                        }
                    } else {
                        mode = 1;
                    }
                }
            } else if (mode == 100) {
                t_snowmanSuicide += Time.deltaTime;
                if (t_snowmanSuicide < 0.18f && t_snowmanSuicide + Time.deltaTime >= 0.18f) {
                    AudioHelper.instance.playOneShot("nanobotCry", 1, 1.5f+0.3f*Random.value, 2);

                }
                if (anim.CurrentAnimationName() == "idle" || anim.CurrentAnimationName() == "hidden") {
                    if (anim.CurrentAnimationName() == "hidden") {
                        anim.Play("idle");
                    }
                    mode = 1;
                    t_snowmanSuicide = 0;
                }


                if (t_snowmanSuicide >= 4) {
                    t_snowmanSuicide = 0;
                    mode = 3;
                }
            } else if (mode == 1) {

                if (state.NeedsToResetIfPlayerOutsideRoom(player)) {
                    Reinitialize();
                    return;
                }
                if (player.CameraIsChangingRooms()) {
                    rb.velocity = Vector2.zero;
                    return;
                } else if (state.UpdateTouchDamage(player)) {
                    if (isSnowman) {
                        SnowmanDie();
                        mode = 2;
                    }
                    player.Bump(true);
                }
                //state.ConstrainPositionToCurrentRoom();
             
                if (!hasShield) {
                    rb.velocity = Vector2.zero;
                    if (followsPlayer) state.SetVelocityTowardsDestination(rb, player.transform, movementVel);
                }
                if (vac.IsBeingSucked()) {
                    if (hasShield) {
                        state.SetVelocityTowardsDestination(rb, player.transform, 1.5f*2.5f);
                    } else {
                        state.SetVelocityTowardsDestination(rb, player.transform, movementVel * 2.5f);
                    }
                }
                if (isSnowman) {
                    if (vac.isBreaking()) t_snowmanSuicide = 3.1f;
                    t_snowmanSuicide += Time.deltaTime;
                    if (t_snowmanSuicide > 3) {
                        SnowmanDie();
                        mode = 2;
                    }
                }
                
            } else if (mode == 2) {
                if (anim.isPlaying == false) {
                    sr.enabled = false;
                    mode = 3;
                }
            } else if (mode == 3) {
                if (state.NeedsToResetIfPlayerOutsideRoom(player)) {
                    Reinitialize();
                }
            }
        }

        void SnowmanDie() {
            transform.GetComponentInChildren<ParticleSystem>().Play();
            AudioHelper.instance.playOneShot("playerCrystalHit");
            AudioHelper.instance.playOneShot("fireGateBurn");
            player.ScreenShake(0.08f, 0.2f, false);
            vac.Break();
        }

        private void FixedUpdate() {
            if (mode == 1 && hasShield) {
                tempVel = rb.velocity;
                tempVel *= 0.975f * 60f * Time.fixedDeltaTime;
                rb.velocity = tempVel;
            }
        }

        void Reinitialize() {
            transform.position = state.initPos;
            mode = 0;
            rb.velocity = Vector2.zero;
            sr.enabled = true;
            anim.PlayInitialAnim();
            t_snowmanSuicide = 0;
            state.onPlayer = false;
            if (shadowFormOn) {
                ToggleShadowForm(false);
            }
        }

        public float customDieVelocity = -1f;
        private void OnCollisionEnter2D(Collision2D collision) {

            if (hasShield && !vac.IsBreakable) {
                return;
            }
            if (collision.collider.CompareTag("Player")) {
                state.onPlayer = true;
            } else {
                if (shadowFormOn) return;

                if (collision.collider.name.IndexOf("Snowman") != -1) {
                    if (isSnowman) {
                        t_snowmanSuicide = 4f;
                    } else {
                        MyBreak();
                    }
                    return;
                } else if (isSnowman && collision.collider.GetComponent<SpikyChaser>() != null) {
                    t_snowmanSuicide = 4f;
                    return;
                }

                if (customDieVelocity == -1f) {
                    customDieVelocity = 5f;
                }
                if (collision.relativeVelocity.magnitude > customDieVelocity || collision.collider.GetComponent<Boomer>() != null) {
                    if (isSnowman) {
                        t_snowmanSuicide = 4f;
                    } else {
                        HF.SendSignal(state.children);
                        vac.Break();
                        rb.velocity = Vector2.zero;
                        mode = 2;
                    }
                }
            }
        }

        [System.NonSerialized]
        public bool shadowFormOn = false;
        void ToggleShadowForm(bool on) {
            if (on) {
                anim.Play("idleshadow");
                shadowFormOn = true;
                cc.isTrigger = true;
                followsPlayer = true;
                movementVel = 1;
            } else {
                anim.Play("idle");
                cc.isTrigger = false;
                shadowFormOn = false;
                followsPlayer = false;
            }
        }

        [Tooltip("Mostly for ocean")]
        public bool hurtsWhenTrigger = false;
        private void OnTriggerExit2D(Collider2D collision) {
            if (collision.CompareTag("Player") && (hurtsWhenTrigger || shadowFormOn)) {
                state.onPlayer = false;
            }
        }
        private void OnTriggerEnter2D(Collider2D collision) {
            if (collision.CompareTag("Player") && (hurtsWhenTrigger || shadowFormOn)) {
                state.onPlayer = true;
            }

            if (collision.GetComponent<Bullet>() != null && collision.GetComponent<Bullet>().name.IndexOf("Lethal") != -1 && collision.GetComponent<SpriteRenderer>().enabled == true) {
                MyBreak();
                return;
            }

            if (hasShield && collision.GetComponent<Spark2D>() != null && (collision.GetComponent<Spark2D>().justBroke || collision.GetComponent<Spark2D>().IsAlive())) {
                if (shieldHealth > 0) {
                    shieldHealth--;
                    AudioHelper.instance.playOneShot("sparkBarHit", 1, 0.9f + 0.2f * Random.value);

                    if (shieldHealth == 0) {
                        vac.IsBreakable = true;
                        AudioHelper.instance.playOneShot("blockExplode");
                        shieldSR.enabled = false;
                        tFlickerShieldTime = 0;
                    } else {
                        tFlickerShieldTime = 0.5f;
                    }
                }
                return;
            }
            if (hasShield && !vac.IsBreakable) {
                return;
            }


            if (vac.inPicoScene && vac.isIdle() && !picoSparkImmune) {
                if (collision.name.IndexOf("2DSpark") == 0 && (collision.GetComponent<Spark2D>().justBroke || collision.GetComponent<Spark2D>().IsAlive())) {
                    sparkHealth--;
                    AudioHelper.instance.playOneShot("swing_broom_1", 0.7f, 0.95f + 0.1f * Random.value, 3);
                    tFlicker = 0.3f;
                    if (sparkHealth == 0) {
                        HF.SendSignal(state.children);
                        vac.Break();
                        mode = 2;
                        rb.velocity = Vector2.zero;
                        tFlicker = tFlickerInterval = 0;
                        sparkHealth = 3;
                        return;
                    }
                }
            }

            if (collision.name.IndexOf("ATB_SwordPrefab") == 0) {
                if (collision.GetComponent<Rigidbody2D>().velocity.magnitude > 2f) {
                    vac.Break();
                    rb.velocity = Vector2.zero;
                    mode = 2;
                    return;
                }
            }

            // for ocean things
            if (hurtsWhenTrigger && collision.CompareTag("Player") == false && collision.name.IndexOf("Raft") == -1 ) {
                if (collision.GetComponent<Rigidbody2D>() != null) {
                    if (collision.GetComponent<Rigidbody2D>().velocity.magnitude > 5f) {

                        HF.SendSignal(state.children);
                        vac.Break();
                        rb.velocity = Vector2.zero;
                        mode = 2;
                    }
                }
            }

            if (collision.GetComponent<ShadowTwin>() != null) {
                ToggleShadowForm(true);
            }

            if (shadowFormOn && collision.name.IndexOf("ShadowTwinKillzone") != -1) {
                MyBreak();
            }
        }

        public void MyBreak() {

            if (hasShield && !vac.IsBreakable) {
                return;
            }

            anim.Play("break");
            if (name == "ReguloidZera") {
                AudioHelper.instance.playOneShot("ash_squeak");
            } else {
                AudioHelper.instance.playOneShot("blockExplode");
            }
            HF.SendSignal(state.children);
            vac.Break();
            rb.velocity = Vector2.zero;
            mode = 2;
        }

        private void OnTriggerStay2D(Collider2D collision) {
            if (mode == 1 && shadowFormOn && collision.name.IndexOf("ShadowTwinKillzone") != -1) {
                MyBreak();
            }
        }


        private void OnCollisionExit2D(Collision2D collision) {

            if (collision.collider.CompareTag("Player")) {
                state.onPlayer = false;
            }
        }
    }
}