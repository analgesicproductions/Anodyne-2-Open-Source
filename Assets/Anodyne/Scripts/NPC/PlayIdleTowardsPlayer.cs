using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Anodyne {
    public class PlayIdleTowardsPlayer : MonoBehaviour {

        AnoControl2D player;
        SpriteAnimator anim;
        // Use this for initialization
        void Start() {
            player = GameObject.Find(Registry.PLAYERNAME2D).GetComponent<AnoControl2D>();

            anim = GetComponent<SpriteAnimator>();
        }

        Vector2 v;
        // Update is called once per frame
        float time = 0;
        void Update() {
            time++;
            if (time > 10) {
                time = 0;
            } else {
                return;
            }
             
            v = player.transform.position - transform.position;


            float a =  Vector2.SignedAngle(v, Vector2.up);
            if (a > -45 && a < 45) {
                anim.Play("idle_u");
            } else if (a >= 45 && a < 135) {
                anim.Play("idle_r");
            } else if (a <= -45 && a > -135) {
                anim.Play("idle_l");
            } else {
                anim.Play("idle_d");
            }
        }
    }
}