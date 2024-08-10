using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Anodyne {
    public class Spark : MonoBehaviour {

        MediumControl player;
        Rigidbody rb;
        public GameObject SparkPrefab;
        public GameObject SparkImpactPrefab;
        Transform SparkImpactTransform;
        ParticleSystem SparkImpactSystem;
        SphereCollider sc;
        public float initSpeed = 20f;
        public float lifetime = .75f;
        float t_life;
        Vector3 tiny = new Vector3(0.1f, 0.1f, 0.1f);
        Vector3 tempScale = new Vector3();
        Vector3 initScale = new Vector3();
        [System.NonSerialized]
        public bool isDisillusioned = false;
        public Transform shadow;
        void Start() {
            isDisillusioned = DustDropPoint.IsDisillusioned();
            SparkImpactTransform = Instantiate(SparkImpactPrefab, transform.parent).transform;
            SparkImpactTransform.localPosition = Vector3.zero;
            SparkImpactSystem = SparkImpactTransform.GetComponent<ParticleSystem>();

            rb = transform.parent.GetComponent<Rigidbody>();
            sc = GetComponent<SphereCollider>();
            rb.useGravity = false;
            sc.isTrigger = true;
            sc.enabled = false;
            SparkPrefab.SetActive(false);
             // Spark > SparkMesh (Has Spark script, MR) /Shadow
            transform.parent.parent = null;
            initScale = transform.localScale;
            HF.GetPlayer(ref player);
            shadow.gameObject.SetActive(false);

        }

        float sceneEnterDelay = 0.5f;
        int mode = 0;
        Vector3 movementvel;
        void Update() {
            // Parent RB and collider moves and retains constant size...
            // But the child mesh scales . so moves contantly
            if (mode == 0) {
                sceneEnterDelay -= 0.0167f;
                if (sceneEnterDelay < 0) sceneEnterDelay = 0;
                if (sceneEnterDelay <= 0 && player.gameObject.activeInHierarchy &&  player.CanShootSpark && MyInput.jpSpecial) {
                    if (isDisillusioned) {
                        lifetime = 0.4f;
                        initSpeed = 10f;
                        initScale.Set(0.6f, 0.6f, 0.6f);
                        transform.localScale = initScale;
                        AudioHelper.instance.playOneShot("sparkShoot", 0.55f, 0.7f);
                    } else {
                        lifetime = 0.75f;
                        initSpeed = 20f;
                        initScale.Set(2,2,2);
                        transform.localScale = initScale;
                        AudioHelper.instance.playOneShot("sparkShoot", 0.55f, 0.9f + 0.2f * Random.value);
                    }
                    MyInput.jpSpecial = false;
                    mode = 1;
                    sc.enabled = true;
                    SparkPrefab.SetActive(true);
                    t_life = 0;
                    if (!isDisillusioned) shadow.gameObject.SetActive(true);
                    rb.transform.position = player.transform.position + new Vector3(0, 0.7f, 0);
                    shadow.transform.position = rb.transform.position;
                    Vector3 v = player.transform.forward;
                    v.y = 0;
                    if (ScaleSparkOn) v = Camera.main.transform.forward;
                    v.Normalize(); v *= initSpeed;
                    if (ScaleSparkOn) v *= 3;
                    rb.velocity = v;
                    movementvel = v;
                    player.PlaySparkAnim();
                }
            } else if (mode == 1) {

                if (HF.TimerDefault(ref t_life, lifetime)) {
                    rb.velocity = Vector3.zero;
                    shadow.gameObject.SetActive(false);
                    sc.enabled = false;
                    mode = 2;
                    return;
                }

                calcScale = initScale;
                if (ScaleSparkOn) calcScale *= 2.5f;

                float cutoff1 = 0.15f;
                float cutoff2 = 0.8f;
                tempScale = transform.localScale;
                if (t_life < cutoff1*lifetime) {
                    tempScale = Vector3.Lerp(tiny, calcScale, t_life / (cutoff1 * lifetime));
                } else if (t_life >= cutoff1*lifetime && t_life <= cutoff2*lifetime) {
                    tempScale = calcScale;
                } else {
                    tempScale = Vector3.Lerp(tiny, calcScale, (lifetime-t_life)/(lifetime-cutoff2*lifetime));
                }

                float a = 0.9f;
                float b = 0.2f;
                tempScale.x *= a + b * Random.value;
                tempScale.y *= a + b * Random.value;
                tempScale.z *= a + b * Random.value;
                transform.localScale = tempScale;

            } else if (mode == 2) {
                tempScale = transform.localScale;
                tempScale.x -= Time.deltaTime * 16f; if (tempScale.x < 0) tempScale.x = 0.01f;
                tempScale.y = tempScale.z = tempScale.x; transform.localScale = tempScale;
                if (SparkImpactSystem.isPlaying == false) {
                    mode = 0;
                    SparkPrefab.SetActive(false);
                }
            } else if (mode == 3) {
                if (!MyInput.special) {
                    mode = 2;
                } else {
                    float curDis = Vector3.Distance(scaleSpark_StartPos, player.transform.position);
                    float r = curDis / scaleSpark_StartDistance;
                    float offset = r - 1f;
                    bool doscale = false;
                    offset *= 0.5f;
                    r = 1 + offset;
                    if (r < 0.25f) r = 0.25f;
                    // When sparking super close, don't start scaling until a certain distance away, and then, only scale much slower.
                    if (scaleSpark_StartDistance < 2.2f) {
                        r = curDis / 2.2f;
                        offset = r - 1f;
                        offset *= 0.15f;
                        r = 1 + offset;
                        if (r > 1.4f) r = 1.4f;
                        if (r > 1) {
                            if (curDis - scaleSpark_StartDistance > 1) {
                                r = (r - 1) * 0.5f + 1;
                                doscale = true;
                            }
                        } 
                    } else {
                        doscale = true;
                    }
                    if (doscale) {
                        scaleSpark_Scaleable.localScale = SS_StartScale * r;
                    }

                }
            }

            if (MyInput.shortcut && Input.GetKeyDown(KeyCode.L)) {
                ScaleSparkOn = !ScaleSparkOn;
                print("Scale spark set to: " + ScaleSparkOn);
            }
         }

        Vector3 calcScale = new Vector3();
        Vector3 scaleSpark_StartPos = new Vector3();
        float scaleSpark_StartDistance = 0;
        Transform scaleSpark_Scaleable;
        Vector3 SS_StartScale = new Vector3();

        public static bool ScaleSparkOn = false;
        public float impactPushback = .66f;
        float damage = 1;
        private void OnTriggerEnter(Collider collision) {
            if (mode != 1) return;

         

            // If an object is visible in editor only, it should only stop the spark if it's a spark reactor, to
            // prevent triggers and stuff from stopping it
            if (collision.gameObject.layer == 10) {
                if (ScaleSparkOn && !collision.isTrigger) {
                    // Continue to hook onto non-triggers.
                } else if (collision.GetComponent<SparkReactor>() == null) {
                    return;
                }
            }
            // Otherwise the object is visible and had a collider. It'll be stopped no matter what.

            if (!collision.CompareTag("Player")) {

                AudioHelper.instance.playOneShot("sparkHit", 1, 0.9f + 0.2f * Random.value);
                if (!isDisillusioned) SparkImpactSystem.Play(true);
                tempScale = rb.transform.position;
                rb.transform.position = tempScale - movementvel.normalized * impactPushback;
                rb.velocity = Vector3.zero;
                shadow.gameObject.SetActive(false);
                sc.enabled = false;
                mode = 2;

                // stop the scalespark from grabbing entire giant hierarchies of things
                if (ScaleSparkOn) {
                    if (collision.transform.childCount != 0) {
                        Collider[] mc = collision.transform.GetComponentsInChildren<Collider>();
                        if (mc != null) {
                            foreach (Collider col in mc) {
                                if (col.name != collision.name) {
                                    print(col.name);
                                    return;
                                }
                            }
                        }
                    }
                }

                if (ScaleSparkOn && MyInput.special) {
                    Transform t = collision.transform;
                    // If hit an invisible collider, see if a sibling has an animator and if so scale that instead.
                    // This is for most NPCs.
                    if (t.gameObject.layer == 10 && t.parent != null) {
                        Animator anim = t.parent.GetComponentInChildren<Animator>();
                        if (anim != null) {
                            t = t.parent;
                        }
                    }

                    if (t.gameObject.layer == 0 && t.gameObject.tag != "NoSparkScale") {
                        // Make sure the object is on DEFAULT layer, is NOT tagged "NoSparkScale", AND
                        // there's at least one Renderer in the hierarchy
                        if (t.GetComponentInChildren<Renderer>() != null) { 
                            scaleSpark_StartPos = rb.transform.position;
                            scaleSpark_StartDistance = Vector3.Distance(scaleSpark_StartPos, player.transform.position);
                            scaleSpark_Scaleable = t.transform;
                            SS_StartScale = scaleSpark_Scaleable.localScale;
                            mode = 3;
                        }
                    }
                }

            } else {
                return;
            }
            if (collision.GetComponent<SparkReactor>() != null && !isDisillusioned) {
                collision.GetComponent<SparkReactor>().Hurt(damage);
            }
        }
    }

}