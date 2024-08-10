using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Anodyne {
    public class NonsuckRegrowHurter : MonoBehaviour {
        AnoControl2D player;
        SpriteAnimator anim;
        SpriteRenderer sr;
        public Element element = Element.None;
        void Start() {
            player = GameObject.Find(Registry.PLAYERNAME2D).GetComponent<AnoControl2D>();
            sr = GetComponent<SpriteRenderer>();
            HF.GetPlayer(ref player);
            anim = GetComponent<SpriteAnimator>();

        }

        int mode = 0;
        float t_regrow = 0;
        public float tm_regrow = 3f;
        void Update() {
            if (mode == 0) {

            } else if (mode == 1) {
                if (anim.isPlaying == false) {
                    mode = 2;
                    sr.enabled = false;
                }
            } else if (mode == 2) {
                if (tm_regrow < 0) {

                } else {
                    if (HF.TimerDefault(ref t_regrow,tm_regrow)) {
                        mode = 3;
                        sr.enabled = true;
                        anim.Play("regrow");
                    }
                }
            } else if (mode == 3) {
                if (!anim.isPlaying) {
                    anim.Play("idle");
                    mode = 0;
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collision) {
            if (mode != 0) return;
            GameObject g = collision.gameObject;


            if (g.CompareTag("Player")) {
                mode = 1;
                player.Damage(1);
                player.Bump(true);
            }
            if (element == Element.Fire) {
               if (g.GetComponent<SlimeWanderer>() != null) {
                    if (g.GetComponent<SlimeWanderer>().element == Element.Water) {
                        AudioHelper.instance.playSFX("blockExplode");
                        mode = 1;
                    }
                }
            } else {
               // mode = 1;
            }
            if (mode == 1) {
                anim.Play("ungrow");
            }
        }
    }
}