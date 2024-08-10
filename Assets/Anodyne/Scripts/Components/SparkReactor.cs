using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Anodyne {
    public class SparkReactor : MonoBehaviour {

        public enum SparkReactionType { SizeOscillate };
        public SparkReactionType  reactiontype = SparkReactionType.SizeOscillate;
        [Tooltip("Set to something else. Oscillating a collider is screwy")]
        public Transform transformToOsc;
        public Transform transformToExplode;
        public Transform transformToSendFlying;
        public float forceValue = 10f;

        public bool isExplodableGlandChain = false;


        public bool isExplodable = false;
        public string explodeableFlag = "none";
        public float explodeHealth = 0.5f;
        public GameObject explodeParticles;


        [Header("Nanopoint stuff")]
        public bool isNanopoint = false;
        public bool NanopointLocked = true;
        [Tooltip("Should match the one used in DA2 script")]
        public string NanopointFlag = "";
        [Tooltip("E.g. geof nanopt 2. Deactivates if flag is 1")]
        public string DeactivatesIfOne = "";
        Door door;
        float maxHealth = 4f;
        float curHealth = 4f;
        [System.NonSerialized]
        public float healSpeed = 0.54f;
        UIManagerAno2 ui;
        UIManager2D ui2d;
        DialogueBox dbox;
        public bool is2D = false;

        public bool isHandfruit = false;
        public bool isCarwashSwitch = false;
        public bool isPrismCap = false;
        public Material CarwashLightMat;
        public void SetMaxHealth(float val) {
            maxHealth = curHealth = val;
        }

        public bool picoPointLocked = false;


        void Start() {
            curSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (is2D) {
                ui2d = GameObject.Find("2D UI").GetComponent<UIManager2D>();
            } else {
                ui = GameObject.Find("3D UI").GetComponent<UIManagerAno2>();
            }
            HF.GetDialogueBox(ref dbox);
            if (isNanopoint) {
                if (DeactivatesIfOne != "" && 1 == DataLoader.instance.getDS(DeactivatesIfOne)) {
                    enabled = false;
                    return;
                }

                if (NanopointFlag == "" || DataLoader.instance.getDS(NanopointFlag + "-LOOPCOUNT") >= 1 || DataLoader.instance.getDS(NanopointFlag + "-sawunlock") >= 1) {
                    NanopointLocked = false;
                }
                door = GetComponent<Door>();
                if (!door.isPicoPoint) {
                    door.isANanoDoor = true;
                }
            } else if (isExplodable) {
                if (explodeableFlag != "none") {
                    if (DataLoader._getDS(explodeableFlag) == 1) {
                        if (transformToExplode != null) {
                            transformToExplode.gameObject.SetActive(false);
                        }
                        gameObject.SetActive(false);
                        return;
                    }
                }
                maxHealth = explodeHealth;
                curHealth = maxHealth;
            } else if (isExplodableGlandChain) {
                maxHealth = explodeHealth;
                curHealth = maxHealth;
            } else if (isHandfruit) {
                curHealth = maxHealth = 6;
            } else if (isCarwashSwitch) {
                if (DataLoader._getDS("carwashSwitch") == 1) {

                    GameObject.Find("BackGate").transform.position = GameObject.Find("BackGatePos").transform.position;
                    GameObject.Find("EggSwitch").transform.Find("Egg").GetComponent<Renderer>().material = CarwashLightMat;
                    GameObject.Find("CarwashLight").GetComponent<Light>().enabled = true;
                    gameObject.SetActive(false);
                    return;
                }
            }
            if (transformToOsc == null) {
                transformToOsc = transform;
            }
            initScale = transformToOsc.localScale;
        }

        float mode = 0;

        Vector3 initScale;
        Vector3 tempScale = new Vector3();
        float t_SizeOsc = 0;
        float period_SizeOsc = 0.125f;
        public float amp_SizeOsc = 0.04f;
        bool barFinishedFilling = false;
        public bool has_barFinishedFilling() {
            return barFinishedFilling;
        }
        void Update() {
            if (isNanopoint) {
                if (mode == 0) {
                    // Change spark bar size
                    if (curHealth < 0 || barFinishedFilling) {
                        barFinishedFilling = true; // Prevent bar from changing its currenthealth property anymore
                        curHealth = 0; // So bar doesn't shrink again during closing bar anim
                        // This can be left off if you don't need to see the nanounlock message to get inside
                        if (!picoPointLocked && !NanopointLocked && IsSparkBarClosingAnimDone()) {
                            mode = 1;
                            door.ext_ForceEnterDoor = true;
                        }
                    }
                } else if (mode == 1) {

                }
            } else if (isExplodable) {
                if (mode == 0) {
                    if (curHealth < 0 || barFinishedFilling) {
                        ParticleSystem p = Object.Instantiate(explodeParticles).GetComponent<ParticleSystem>();
                        p.transform.position = transform.position;
                        p.Play();
                        AudioHelper.instance.playOneShot("crystalHitPlayer");

                        if (explodeableFlag != "none") DataLoader._setDS(explodeableFlag, 1);
                        Object.Destroy(p.gameObject, 1.5f);
                        barFinishedFilling = true;
                        curHealth = 0;
                        mode = 1;
                        if (isPrismCap) {
                            GameObject.Find("end-ring").GetComponent<DialogueAno2>().ext_ForceInteractScriptToParse = true;
                        }
                    }
                } else if (mode == 1) {


                    if (transformToSendFlying != null) {
                        Collider[] cols = transformToSendFlying.GetComponentsInChildren<Collider>();
                        foreach (Collider col in cols) {
                            Destroy(col);
                        }
                        MediumControl p = null;
                        HF.GetPlayer(ref p);
                        transformToSendFlying.gameObject.AddComponent<Rigidbody>();
                        transformToSendFlying.gameObject.GetComponent<Rigidbody>().mass *= 2;
                        transformToSendFlying.gameObject.GetComponent<Rigidbody>().AddForce(Vector3.Lerp(p.transform.forward, Vector3.up, 0.8f) * forceValue * 1.66f, ForceMode.Impulse);
                        transformToSendFlying.gameObject.GetComponent<Rigidbody>().AddTorque(new Vector3(forceValue, 0.1f, 0.1f), ForceMode.Impulse);
                    }

                    if (transformToExplode != null) {
                        transformToExplode.gameObject.SetActive(false);
                    }
                    Destroy(gameObject);
                    return;
                }
            } else if (isExplodableGlandChain) {
                if (mode == 0) {
                    if (curHealth < 0 || barFinishedFilling) {
                        ParticleSystem p = Object.Instantiate(explodeParticles).GetComponent<ParticleSystem>();
                        p.transform.position = transform.position;
                        p.Play();
                        AudioHelper.instance.playOneShot("fireGateBurn");
                        mode = 1;
                        barFinishedFilling = true; curHealth = 0;
                        transformToOsc.GetComponent<SpriteRenderer>().enabled = false;
                        Destroy(gameObject, 1f);
                    }
                } else if (mode == 1) {
                    // has finished filling can now be checked by the boss
                    return;
                }
            } else if (isHandfruit) {
                if (mode == 0) {
                    if (curHealth < 4) {
                        setSparkBarSize(curHealth, maxHealth);
                        mode = 1;
                    }
                } else if (mode == 1) {
                    DialogueAno2 d = GameObject.Find("HandfruitEatScene").GetComponent<DialogueAno2>();
                    d.ext_ForceInteractScriptToParse = true;
                    mode = 2;
                } else if (mode == 2 && curHealth >= maxHealth) {
                    mode = 3;
                } else if (mode == 3) {
                    return;
                }
            } else if (isCarwashSwitch) {
                if (mode == 0) {
                    if (curHealth < 0 || barFinishedFilling) {
                        ParticleSystem p = Object.Instantiate(explodeParticles).GetComponent<ParticleSystem>();
                        AudioHelper.instance.playOneShot("crystalHitPlayer");

                        p.transform.position = transformToOsc.position;
                        p.Play();
                        barFinishedFilling = true;
                        curHealth = 0;
                        GameObject.Find("EggSwitch").transform.Find("Egg").GetComponent<Renderer>().material = CarwashLightMat;
                        GameObject.Find("CarwashLight").GetComponent<Light>().enabled = true;
                        DataLoader._setDS("carwashSwitch", 1);
                        GameObject.Find("CarwashSwitchCutscene").GetComponent<DialogueAno2>().ext_ForceInteractScriptToParse = true;
                        mode = 1;
                    }
                } else if (mode == 1) {
                    return;
                }
            }

            if (curHealth < maxHealth) {
                if (!barFinishedFilling) {
                    curHealth += Time.deltaTime * healSpeed;
                    if (isNanopoint && (NanopointLocked || picoPointLocked)) {
                        curHealth += 3*Time.deltaTime * healSpeed;
                    } else if (isHandfruit && mode != 0) {
                        curHealth += 6  * Time.deltaTime * healSpeed;
                    }
                }

                if (!NanopointLocked && !picoPointLocked) {
                    if (isCarwashSwitch || isExplodable || isExplodableGlandChain) {
                        setSparkBarSize(curHealth, maxHealth, true);
                    } else {
                        setSparkBarSize(curHealth, maxHealth);
                    }
                }
                if (reactiontype == SparkReactionType.SizeOscillate) {
                    t_SizeOsc += Time.deltaTime;
                    tempScale.Set(amp_SizeOsc, amp_SizeOsc, amp_SizeOsc);
                    tempScale *= Mathf.Sin(Mathf.Deg2Rad * 360f * (t_SizeOsc / period_SizeOsc));
                    tempScale *= (1 - curHealth / maxHealth);
                    transformToOsc.localScale = initScale + tempScale;
                    if (curHealth >= maxHealth) {
                        transformToOsc.localScale = initScale;
                    }
                }
            }

        }

        bool IsSparkBarClosingAnimDone() {
            if (is2D) {
                return ui2d.IsSparkBarClosingAnimDone();
            } else {
                return ui.IsSparkBarClosingAnimDone();
            }
        }

        void setSparkBarSize(float cur, float max, bool noPauseAtEnd=false) {
            if (is2D) {
                ui2d.setSparkBarSize(cur, max, noPauseAtEnd);
            } else {
                ui.setSparkBarSize(cur, max, noPauseAtEnd);
            }
        }

        private void LateUpdate() {
            justHurt = false;
        }

        string curSceneName;
        [System.NonSerialized]
        public bool justHurt = false;
        public void Hurt(float damage = 1) {
            if (isHandfruit && mode != 0) {
                return;
            }
            if (isNanopoint) {
                if (name == "WolgaliCleanCol" && DataLoader._getDS("desert-db-enter") == 1) {
                    return;
                }
                if (curSceneName == "CCC" || curSceneName == "CougherHome") {
                    if (DataLoader.instance.getDS("ddp-open-ring1") == 1 && DataLoader.instance.getDS("ddp-open-ring2") == 0) {
                        if (Ano2Stats.CountTotalCards() >= 12) {
                            if (dbox.isDialogFinished()) {
                                dbox.playDialogue("cccnanoerror");
                            }
                            return;
                        }

                    }
                }
            }
            if (name == "Body-col" && picoPointLocked) {

            } else {
                curHealth -= damage;
                justHurt = true;
            }
            if (isNanopoint) {
                AudioHelper.instance.playSFX("sparkBarHit", true, 0.5f, false, 0.95f + 0.1f * Random.value);
                if (curHealth < 0 && NanopointLocked) {
                    curHealth = 0.1f;
                }
            } else if (isExplodable || isExplodableGlandChain) {
                AudioHelper.instance.playSFX("sparkBarHit", true, 0.5f, false, 0.95f + 0.1f * Random.value);
                //AudioHelper.instance.playOneShot("crystalHitPlayer");
            } else if (isCarwashSwitch) {
                AudioHelper.instance.playSFX("sparkBarHit", true, 0.5f, false, 0.95f + 0.1f * Random.value);

            }
        }
    }
}