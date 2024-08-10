using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Anodyne {
    public class Enabler : MonoBehaviour {
        public bool _TilemapRenderer = false;
        public bool _SpriteRenderer = false;
        public bool reverseInputSignal = false;
        void Start() {

        }

        void Update() {

        }

        // "" = enable the component
        // unsignal = disable the compenent
        public void SendSignal(string signal) {
            if (reverseInputSignal) {
                if (signal == "") {
                    signal = "unsignal";
                } else {
                    signal = "";
                }
            }

            if (signal == "") {
                if (_TilemapRenderer) GetComponent<UnityEngine.Tilemaps.TilemapRenderer>().enabled = true;
                if (_SpriteRenderer) GetComponent<SpriteRenderer>().enabled = true;
            } else if (signal == "unsignal") {
                if (_TilemapRenderer) GetComponent<UnityEngine.Tilemaps.TilemapRenderer>().enabled = false;
                if (_SpriteRenderer) GetComponent<SpriteRenderer>().enabled = false;
            }
        }
    }
}