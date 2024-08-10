using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Anodyne {
    public class Spark2D : MonoBehaviour {
        AnoControl2D player;
        Rigidbody2D rb;
        ParticleSystem SparkImpactSystem;
        public float initSpeed = 10f;
        public float lifetime = .75f;
        float t_life;
        SpriteAnimator anim;
        SpriteRenderer sr;
        float sceneEnterDelay = 0.5f;
        int mode = 0;

        void Start() {
            SparkImpactSystem = GetComponent<ParticleSystem>();
            rb = transform.GetComponent<Rigidbody2D>();
             // Spark > SparkMesh (Has Spark script, MR) /
            HF.GetPlayer(ref player);
            sr = GetComponent<SpriteRenderer>();
            sr.enabled = false;
            anim = GetComponent<SpriteAnimator>();
            transform.parent = null;

            // hide nanosparks
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.IndexOf("Pico") == 0) {
                if (name.IndexOf("Pico") == -1) gameObject.SetActive(false);
            }
        }
        void Update() {
            justBroke = false;
            if (mode == 0) {
                sceneEnterDelay -= 0.0167f;
                if (sceneEnterDelay < 0) sceneEnterDelay = 0;
                if (sceneEnterDelay <= 0 && player.CanShootSpark && MyInput.jpCancel) {
                    AudioHelper.instance.playOneShot("sparkShoot", 0.55f, 0.9f + 0.2f * Random.value);
                    MyInput.jpCancel = false;
                    mode = 1;
                    t_life = 0;
                    sr.enabled = true;
                    anim.Play("idle");
                    rb.transform.position = player.transform.position;
                    //Vector3 tempV = 0.1f * player.getFacingDirVector();
                    Vector3 tempv = rb.transform.position;
                    tempv.y += player.GetComponent<CircleCollider2D>().offset.y * player.transform.localScale.y;
                    rb.transform.position = tempv;
                    //rb.transform.position = rb.transform.position + tempV;
                    rb.velocity = initSpeed * player.getFacingDirVector();
                    //player.PlaySparkAnim();
                }
            } else if (mode == 1) {
                if (HF.TimerDefault(ref t_life, lifetime)) {
                    rb.velocity = Vector3.zero;
                    mode = 2;
                    anim.Play("fizzle");
                    return;
                }
            } else if (mode == 2) {
                if (!anim.isPlaying) {
                    sr.enabled = false;
                }
                if (SparkImpactSystem.isPlaying == false && !anim.isPlaying) {
                    mode = 0;
                }
            }
         }

        public bool justBroke = false;

        public bool IsAlive() {
            return mode == 1;
        }


        void BreakStuff() {

            AudioHelper.instance.playOneShot("sparkHit", 1, 0.9f + 0.2f * Random.value);
            SparkImpactSystem.Play(true);
            rb.velocity = Vector2.zero;
            anim.Play("fizzle");
            mode = 2;
            justBroke = true;
        }
        float damage = 1;
        private void OnTriggerEnter2D(Collider2D collision) {
            if (mode != 1) return;
            if (collision.GetComponent<Pew>()!= null) {
                if (collision.GetComponent<Pew>().IsDead()) {
                    return;
                }
            }
            if (collision.isTrigger) {
                if (collision.name.IndexOf("PicoSpiky") == 0) {
                    BreakStuff();
                    return;
                }
            }
            if (collision.isTrigger) return;
            if (collision.name.IndexOf("Raft") != -1) return;
            // Otherwise the object is visible and had a collider. It'll be stopped no matter what.
            if (!collision.CompareTag("Player") && collision.name != "PickupRegion" && collision.GetComponent<Spark2D>() == null) {
                BreakStuff();
            } else {
                return;
            }
            if (collision.GetComponent<SparkReactor>() != null) {
                collision.GetComponent<SparkReactor>().Hurt(damage);
            }
        }
    }
}