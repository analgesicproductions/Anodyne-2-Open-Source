using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Anodyne {
    public class Boomeranger : MonoBehaviour {

        public Bullet bul1;
        public Bullet bul2;

        List<Bullet> bullets;
        AnoControl2D player;
        SpriteAnimator anim;
        EntityState2D state;

        int mode = 0;
        float t_wait = 0;
        int health = 2;
        Vector3 tempRot = new Vector3();
        void Start() {
            state = GetComponent<EntityState2D>();
            anim = GetComponent<SpriteAnimator>();
            bullets = new List<Bullet>();
            bullets.Add(bul1);
            bullets.Add(bul2);
            bul1.gameObject.SetActive(false);
            bul2.gameObject.SetActive(false);
            sr = GetComponent<SpriteRenderer>();
            
            HF.GetPlayer(ref player);
        }
        SpriteRenderer sr;
        void Update() {
            if (DataLoader.instance.isPaused) {
                return;
            }

            if (flickerTime > 0) {
                flickerTime -= Time.deltaTime;
                if (flickerTime <= 0) {
                    sr.enabled = true;
                } else {
                    if (HF.TimerDefault(ref tmFlicker,0.05f)) {
                        sr.enabled = !sr.enabled;
                    }
                }
            }

            if (!player.InSameRoomAs(transform.position) && !player.CameraIsChangingRooms()) {
                if (mode != 0) {
                    bul1.Die();
                    bul2.Die();
                    mode = 0;
                    health = 2;
                    t_wait = 0;
                    GetComponent<SpriteRenderer>().enabled = true;
                    anim.Play("idle");
                    GetComponent<BoxCollider2D>().isTrigger = false;
                }
                return;
            }

            tempRot = bul1.transform.localEulerAngles;
            tempRot.z += Time.deltaTime * 1080f;
            bul1.transform.localEulerAngles = tempRot;
            tempRot.z *= -1;
            bul2.transform.localEulerAngles = tempRot;

            if (mode == 0) {
                if (Vector2.Distance(transform.position,player.transform.position) < 3f) {
                    anim.Play("attack");
                    mode = 1;
                    bul1.GetComponent<SpriteRenderer>().enabled = false;
                    bul2.GetComponent<SpriteRenderer>().enabled = false;
                    bul1.gameObject.SetActive(true);
                    bul2.gameObject.SetActive(true);
                }
            } else if (mode == 1) {
                if (HF.TimerDefault(ref t_wait,0.75f)) {
                    anim.Play("idle");
                    AudioHelper.instance.playOneShot("bluntExplosion");
                    if (Random.value > 0.5f) {
                        Bullet b = Bullet.GetADeadBullet(bullets);
                        b.LaunchAt(transform.position, player.transform.position);
                    } else {
                        bul1.LaunchAt(transform.position, player.transform.position, 30f);
                        bul2.LaunchAt(transform.position, player.transform.position, -30f);
                    }
                    mode = 2;
                }
            } else if (mode == 2) {
                if (Bullet.AllDead(bullets)) {
                    mode = 3;
                }
            } else if (mode == 3) {
                if (HF.TimerDefault(ref t_wait,0.75f)) {
                    mode = 0;
                    anim.Play("idle");
                }
            } else if (mode ==4 ) {
                if (anim.isPlaying == false) {
                    GetComponent<SpriteRenderer>().enabled = false;
                }
            }
        }

        float flickerTime = 0;
        float tmFlicker = 0.05f;
        private void OnCollisionEnter2D(Collision2D collision) {
            if (collision.collider.GetComponent<Vacuumable>() != null && !collision.collider.GetComponent<Vacuumable>().isIdle()) {
                hurtstuff();
            }


        }

        void hurtstuff() {

                health--;
                flickerTime = 0.6f;
                AudioHelper.instance.playOneShot("bluntExplosion");
                if (health == 0) {
                    anim.Play("break");
                    mode = 4;
                    HF.SendSignal(state.children);
                    GetComponent<BoxCollider2D>().isTrigger = true;
                }
        }

        private void OnTriggerEnter2D(Collider2D collision) {
            if (collision.GetComponent<Bullet>() != null) {
                if (collision.name.IndexOf("Boomeranger") == -1) {
                    hurtstuff();
                }
            }
        }
    }
}