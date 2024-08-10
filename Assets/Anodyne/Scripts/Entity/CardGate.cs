using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Anodyne {
    public class CardGate : MonoBehaviour {

        public int CardID;
        DialogueBox dbox;
        SpriteAnimator anim;
        // Use this for initialization
        void Start() {
            dbox = GameObject.Find("Dialogue").GetComponent<DialogueBox>();
            anim = GetComponent<SpriteAnimator>();
        }


        int mode = 0;
        private bool inTrig;

        // Update is called once per frame
        void Update() {

            if (mode == 0) {
                if (inTrig) mode = 1;
            } else if (mode == 1) {
                if (MyInput.jpConfirm) {
                    if (Ano2Stats.HasCard(CardID)) {
                        anim.Play("open");
                        mode = 2;
                    } else {
                        dbox.playDialogue("cardGate");
                        mode = 3;
                    }
                } else if (!inTrig) {
                    mode = 0;
                }
            } else if (mode == 2) {
                if (anim.isPlaying == false) {
                    gameObject.SetActive(false);
                }
            } else if (mode == 3) {
                if (dbox.isDialogFinished()) {
                    mode = 0; inTrig = false;
                }
            }
        }


        private void OnTriggerEnter2D(Collider2D collision) {
            if (!collision.CompareTag("Player")) return;
            inTrig = true;
        }
        private void OnTriggerExit2D(Collider2D collision) {
            if (!collision.CompareTag("Player")) return;
            inTrig = false;    
        }


    }

}