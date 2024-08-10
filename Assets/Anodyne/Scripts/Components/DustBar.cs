using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Anodyne {
    public class DustBar : MonoBehaviour {

        [Range(0,90f)]
        public float CurrentDust = 0f;
        float initialY = -90f;
        RectTransform DustBarT;
        Image dustbarimage;
        Vector3 pos = new Vector3();
        Color col = new Color();
        float t = 0;
        public Texture2D spritesheet;
        public Sprite[] sprites;

        Image digitTens;
        Image digitOnes;

        bool StartFinished = false;
        public string suffix = "";
        private void Start() {
            if (suffix == "pm") initialY -= 0.5f;
            DustBarT = GameObject.Find("DustBar_Full"+suffix).GetComponent<RectTransform>();
            dustbarimage = DustBarT.transform.GetComponent<Image>();
            sprites = Resources.LoadAll<Sprite>("Visual/Sprites/UI/DustNumerals");
            digitTens = GameObject.Find("DustDigitTens" + suffix).GetComponent<Image>();
            digitOnes = GameObject.Find("DustDigitOnes" + suffix).GetComponent<Image>();
            StartFinished = true;
            CurrentDust = Ano2Stats.dust;
            UpdateGraphics();
        }


        private void OnValidate() {
            if (!Application.isPlaying) return;
            if (!StartFinished) return;
            UpdateGraphics();
        }

        void UpdateGraphics() {
            pos = DustBarT.localPosition;
            if (Ano2Stats.GetMaxDust() > 60) {
                pos.y = initialY + Mathf.Lerp(0,60f,CurrentDust/90f) + 1;
            } else {
                pos.y = initialY + CurrentDust + 1;
            }
            DustBarT.localPosition = pos;
            t = 0.6f;

            int dust = (int)CurrentDust;
            digitTens.sprite = sprites[dust / 10];
            digitOnes.sprite = sprites[dust % 10];
        }


        float t2 = 0;
        void Update() {
            if (t > 0) {
                t -= Time.deltaTime;
                col = dustbarimage.color;
                t2 -= Time.deltaTime;
                if (t2 < 0) {
                    t2 = 0.026f;
                    if (col.a == 1 && t > 0) {
                        col.a = 0.5f;
                    } else if (col.a == 0.5f) {
                        col.a = 1f;
                    }
                }
                if (t < 0) {
                    col.a = 1f;
                }
                dustbarimage.color = col;
            }
        }

        public void SetDust(float amount) {
            digitTens.gameObject.SetActive(true);
            digitOnes.gameObject.SetActive(true);
            CurrentDust = amount;
            UpdateGraphics();
            t = 0;
            col = dustbarimage.color;
            col.a = 1f; dustbarimage.color = col;
        }

        public void AddDust(float amount) {
            CurrentDust = Mathf.Clamp(CurrentDust + amount, 0, Ano2Stats.GetMaxDust());
            UpdateGraphics();
        }
    }
}