using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Anodyne {
    public class Gate : MonoBehaviour {

        public bool isBloodGate = false;
        string nametag;
        int originalNeededSignals = 0;
        public int neededSignals = 1;
        public bool stateIsSaved = true;
        [Tooltip("If the gate starts closed, and doesn't open while player in the room, then reset its signal count when player leaves")]
        public bool resetsToOriginalIfDoesntOpen = true;
        [Tooltip("Overrides stateIsSaved. Mostly for rising gates.")]
        public bool alwaysResetsToOriginal = false;
        public bool isBombGate = false;
        AnoControl2D player;
        SpriteAnimator animator;
        public string specialstring = "";
        DialogueBox dbox;
        int dboxstate = 0;
        bool nosound = false;
        bool acceptsUnsignal = false;

        int nosoundticks = 2;
        void Start() {
            dbox = GameObject.Find("Dialogue").GetComponent<DialogueBox>();
            originalNeededSignals = neededSignals;
            HF.GetPlayer(ref player);

            if (name == "DilemmaGate") {
                acceptsUnsignal = true;
            }

            if (SceneManager.GetActiveScene().name == "NanoAlbumen") {
                isYolkGate = true;
            }

            // TestSceneRoom1Gate (1)
            nametag = SceneManager.GetActiveScene().name + transform.parent.name + name;
            // On scene load, check database for signal count.
            if (stateIsSaved) {
                if (DataLoader.instance.existsDS(nametag)) {
                    DataLoader.instance.silenceDSFlagsOnce = true;
                    neededSignals = DataLoader.instance.getDS(nametag);
                    if (originalNeededSignals > 0 && neededSignals <= 0) {
                        nosound = true;
                    }
                    //The database is only updated when being signalled.
                    // So you can sig a 2-sig gate once, save game, reload, then dont have to clear
                    // the room properly
                    // so reset here if needed
                    if (resetsToOriginalIfDoesntOpen && neededSignals > 0 && neededSignals < originalNeededSignals) {
                        print("gate reset" + name);
                        neededSignals = originalNeededSignals;
                        DataLoader.instance.silenceDSFlagsOnce = true;
                        DataLoader.instance.setDS(nametag, neededSignals);
                    }
                }
            } else if (alwaysResetsToOriginal) {
                neededSignals = originalNeededSignals;
            }

            animator = GetComponent<SpriteAnimator>();
            if (isBombGate) {
                if (neededSignals <= 0) {
                    GetComponent<BoxCollider2D>().isTrigger = true;
                    GetComponent<SpriteRenderer>().enabled = false;
                } else {
                    neededSignals = 1;
                    animator.Play("idle");
                }
            } else if (neededSignals <= 0) {
                animator.Play("open");
                state = 1;
            }
        }

        int state = 0;

        int bloodGateMode = 0;
        int bloodGateHealth = 0;

        void Update() {
            if (nosoundticks > 0) nosoundticks--;
            if (Input.GetKeyDown(KeyCode.Q)) {
               // neededSignals++;
            } else if (Input.GetKeyDown(KeyCode.W)) {
                //neededSignals--;
            }

            if (dboxstate == 1) {
                if (dbox.isDialogFinished()) {
                    dboxstate = 2;
                } else {
                    return;
                }
            }

            if (isBloodGate) {
                if (bloodGateMode == 0) {
                    if (player.InSameRoomAs(transform.position)) {
                        bloodGateMode = 1;
                        bloodGateHealth = SaveManager.currentHealth;
                    }
                } else if (bloodGateMode == 1) {
                    if (bloodGateHealth < SaveManager.currentHealth) {
                        bloodGateHealth = SaveManager.currentHealth;
                    }
                    if (bloodGateHealth > SaveManager.currentHealth) {
                        if (neededSignals > 0) SendSignal();
                        bloodGateHealth = SaveManager.currentHealth;
                    }
                    if (!player.InSameRoomAs(transform.position)) {
                        bloodGateMode = 0;
                    }
                }
            }

            if (!player.InSameRoomAs(transform.position) && !isBombGate) {
                if (alwaysResetsToOriginal) {
                    neededSignals = originalNeededSignals;
                    if (neededSignals <= 0) {
                        animator.ForcePlay("open");
                        OpenStuff();
                        state = 2;
                    } else {
                        CloseStuff();
                        state = 0;
                    }
                } else if (resetsToOriginalIfDoesntOpen && originalNeededSignals != neededSignals &&  originalNeededSignals > 0 && neededSignals > 0) {
                    neededSignals = originalNeededSignals;
                    state = 0;
                }
            }

            if (isBombGate) {
                if (state == 1) {
                    if (animator.isPlaying == false) {
                        state = 2;
                    }
                }
                return;
            }

            if (state == 0) {
                if (neededSignals <= 0) {
                    state = 1;
                    // anim
                    if (!nosound && nosoundticks <= 0) AudioHelper.instance.playSFX("gateOpenAno2",false,0.83f,true);
                    animator.ForcePlay("open");
                }
            } else if (state == 1) {
                if (animator.isPlaying == false) {
                    OpenStuff();
                    state = 2;
                }
            } else if (state == 2) { 
                if (neededSignals > 0) {
                    CloseStuff();
                    if (!nosound) AudioHelper.instance.playSFX("gateclose", false, 0.83f, true);
                    state = 0;
                }
            }
        }

        private void OpenStuff() {
            BoxCollider2D bc = GetComponent<BoxCollider2D>();
            bc.isTrigger = true;
            GetComponent<SpriteRenderer>().enabled = false;
            if (name == "DillemmaReverseGate") {
                GetComponent<SpriteRenderer>().enabled = true;
            }
        }

        private void CloseStuff() {
            animator.ForcePlay("close");
            BoxCollider2D bc = GetComponent<BoxCollider2D>();
            GetComponent<SpriteRenderer>().enabled = true;
            bc.isTrigger = false;
        }

        public void SendSignal(string type="") {
            if (specialstring == "fireslimegate") {
                if (type == "becamefire") {
                    neededSignals--;
                }
            } else if (type == "") {
                neededSignals--;
            } else if (type == "unsignal") {
                if (acceptsUnsignal) {
                    neededSignals--;
                } else {
                    neededSignals++;
                }
            }
            if (nametag == "" || nametag == null) {
                nametag = SceneManager.GetActiveScene().name + transform.parent.name + name;
            }
            if (stateIsSaved) {
                DataLoader.instance.silenceDSFlagsOnce = true;
                DataLoader.instance.setDS(nametag, neededSignals);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision) {
            if (isYolkGate && dboxstate == 0 && dbox.isDialogFinished() && collision.collider.GetComponent<Vacuumable>() != null) {
                dbox.playDialogue("gate-hit");
                dboxstate = 1;
            } else {
                BombCheck(collision.gameObject);
            }
        }

        void BombCheck (GameObject collision) {
            if (!isBombGate) return;
            bool blowup = false;
            if (neededSignals > 0) {
                if (collision.GetComponent<SlimeWanderer>() != null && collision.GetComponent<SlimeWanderer>().element == Element.Fire) {
                    blowup = true;
                } else if (collision.GetComponent<Bullet>() != null && collision.GetComponent<Bullet>().element == Element.Fire) {
                    blowup = true;
                }
            }
            if (blowup) {
                neededSignals = 0;
                state = 1;
                GetComponent<BoxCollider2D>().isTrigger = true;
                AudioHelper.instance.playOneShot("fireGateBurn");
                animator.Play("explode");
                animator.ScheduleFollowUp("off");
                DataLoader.instance.setDS(nametag, neededSignals);
            }
        }
        bool isYolkGate = false;
        private void OnTriggerEnter2D(Collider2D collision) {
                BombCheck(collision.gameObject);
           
        }

       
    }
}
