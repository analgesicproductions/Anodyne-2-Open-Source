using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Anodyne {
    public class DogSpark : MonoBehaviour {

        public Transform[] transforms;

        public float jumpHeight = 5;
        public float tm_jump = 1f;
        float tm_wait = 1.2f;
        public float tm_rotate = 0.5f;

        float timer = 0;
        Vector3 tempRot;
        Vector3 targetRot;
        Vector3 startRot;
        SparkReactor sparkReactor;
        Animator animator;
        void Start() {
            sparkReactor = GetComponentInChildren<SparkReactor>();
            animator = GetComponentInChildren<Animator>();
            sparkReactor.healSpeed = 0.25f;
            sparkReactor.SetMaxHealth(16f);

            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(1).gameObject.SetActive(false);
            transform.GetChild(2).gameObject.SetActive(false);
        }

        Vector3 startPos;
        Vector3 endPos;

        public bool debugon = false;
        int mode = 0;
        int[] offs = new int[] { 1, 3, 2, 1, 1, 3, 2, 2, 1, 3, 2, 3, 1, 1, 2, 3, 2, 1, 3, 2};
        int offIndex = 0;

        void Update() {
            if (mode == 100) {
                return;
            }
            if (mode == 0) {
                if (debugon) {
                    Init();
                }
            } else if (mode == 1) {

                // wait after landing
                mode = 2;
                int r = offs[offIndex];
                for (int i = 0; i < r; i++) {
                    nextTransformIndex++;
                    if (nextTransformIndex >= transforms.Length) nextTransformIndex = 0;
                }
                offIndex++;
                if (offIndex >= offs.Length) offIndex = 0;
                tempRot = transform.localEulerAngles;
                transform.LookAt(transforms[nextTransformIndex]);
                targetRot = transform.localEulerAngles;
                targetRot.x = tempRot.x;
                targetRot.z = tempRot.z;
                transform.localEulerAngles = tempRot;
                startRot = tempRot;
                endPos = transforms[nextTransformIndex].position;
                startPos = transform.position;
            } else if (mode == 2) {
                timer += Time.deltaTime;
                tempRot.y = Mathf.LerpAngle(startRot.y, targetRot.y, timer / tm_rotate);
                transform.localEulerAngles = tempRot;
                if (timer >= tm_rotate) {
                    timer = 0;
                    timer -= difficultyTweak;
                    difficultyTweak += 0.05f;
                    if (difficultyTweak > 3f) difficultyTweak = 3f;
                    mode = 20;
                }
            } else if (mode == 20) {
                timer += Time.deltaTime;
                if (timer >= 0) {
                    animator.CrossFadeInFixedTime("Launch", 0.15f);
                    mode = 21;
                    timer = 0;
                }
            } else if (mode == 21) {
                timer += Time.deltaTime;
                if (timer > tm_wait/2) {
                    timer = 0;
                    animator.CrossFadeInFixedTime("Jumping", 0.15f);
                    mode = 3;
                }
            } else if (mode == 3) {
                timer += Time.deltaTime;
                if (timer >= tm_jump) timer = tm_jump;
                tempRot = Vector3.Lerp(startPos, endPos, timer / tm_jump);
                tempRot.y += jumpHeight * Mathf.Sin((timer / tm_jump) * 3.14f);
                transform.position = tempRot;
                if (timer > tm_jump - 0.15f  && !transitioned) {
                    transitioned = true;
                    animator.CrossFadeInFixedTime("IdleDusty", 0.15f);
                }

                if (timer >= tm_jump) {
                    timer = 0;
                    mode = 1;
                    transitioned = false;
                }
            }
            if (sparkReactor.has_barFinishedFilling()) {
                mode = 100;
            }
        }
        bool transitioned = false;
        public void Init() {
            mode = 1;
            transform.position = transforms[0].position;
            endPos = GameObject.Find("SparkBarBG").GetComponent<RectTransform>().anchoredPosition;
            endPos.y = 105f;
            GameObject.Find("SparkBarBG").GetComponent<RectTransform>().anchoredPosition = endPos;

            transform.GetChild(0).gameObject.SetActive(true);
            transform.GetChild(1).gameObject.SetActive(true);
            transform.GetChild(2).gameObject.SetActive(true);
        }
        float difficultyTweak = 0;
        int nextTransformIndex = 0;
    }
}
