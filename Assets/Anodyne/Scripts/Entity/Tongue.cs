using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Anodyne {

    public class Tongue : MonoBehaviour {
        Rigidbody2D rb;
        Vacuumable vac;
        AnoControl2D player;
        BoxCollider2D bc;
        EntityState2D state;
        TriggerChecker stopZoneTrigger;

        public bool isPull = false;
        public float movementSpeed = 4;
        public float retractSpeed = 1.5f;
        [FormerlySerializedAs("movementDirection")]
        public AnoControl2D.Facing extensionDirection = AnoControl2D.Facing.UP;// URDL

        public Tongue tonguePull;
        public bool tongueRetracts = false;
        [Tooltip("If this tongue moves reverse, how many tiles is it offset from the usual initial position?")]
        public int IfRetractsTileOffset = 0;
        [Tooltip("Does tongue return to initial position when its chain not pulled?")]
        public bool tongueIsOneWay = false;
        float furthestMovementPercentage = 0;
        public float maxMovementDistance = 2;
        float movementDistance = 0;

        public bool isDBTrain = false;


        [HideInInspector]
        public float movementPercentage = 0;
        void Start() {
            state = GetComponent<EntityState2D>();
            rb = GetComponent<Rigidbody2D>();
            if (!isPull) stopZoneTrigger = transform.parent.Find("StopZone").GetComponent<TriggerChecker>();
            HF.GetPlayer(ref player);
            if (isPull) {
                vac = GetComponent<Vacuumable>();
                vac.CanBeSucked_SetExternally = false;
            } else {
                bc = GetComponent<BoxCollider2D>();
                initialTongueXOffset = bc.offset.x;
            }
            state = GetComponent<EntityState2D>();
        }

        void Update() {
            if (extensionDirection == AnoControl2D.Facing.UP) state.tempPos.Set(0, 1, 0);
            if (extensionDirection == AnoControl2D.Facing.RIGHT) state.tempPos.Set(1, 0, 0);
            if (extensionDirection == AnoControl2D.Facing.DOWN) state.tempPos.Set(0, -1, 0);
            if (extensionDirection == AnoControl2D.Facing.LEFT) state.tempPos.Set(-1, 0, 0);
            if (isPull) {
                UpdatePull();
            } else {
                UpdateTongue();
            }
            if (isDBTrain) {
                UpdateTrain();
            }
        }

        int mode = 0;
        private float retractDelay;
        [HideInInspector]
        public bool tonguePullUnblock = false;
        private void UpdatePull() {

            bool doSuck = false;
            if (mode == 0) {
                if (vac.SuckZoneIsOverlapping && MyInput.special) {

                    mode = 1;
                }
            } else if (mode == 1) {
                if (!vac.SuckZoneIsOverlapping || !MyInput.special) {
                    vac.SuckZoneIsOverlapping = false;
                    mode = 0;
                } else {

                    if (player.facing == AnoControl2D.Facing.RIGHT && extensionDirection == AnoControl2D.Facing.LEFT) {
                        doSuck = true;
                    } else if (player.facing == AnoControl2D.Facing.LEFT && extensionDirection == AnoControl2D.Facing.RIGHT) {
                        doSuck = true;
                    } else if (player.facing == AnoControl2D.Facing.UP && extensionDirection == AnoControl2D.Facing.DOWN) {
                        doSuck = true;
                    } else if (player.facing == AnoControl2D.Facing.DOWN && extensionDirection == AnoControl2D.Facing.UP) {
                        doSuck = true;
                    }
                }
            }

            if (doSuck) {
                movementDistance += Time.deltaTime * movementSpeed;
                if (movementDistance > maxMovementDistance) movementDistance = maxMovementDistance;
                rb.MovePosition(state.initPos + state.tempPos * movementDistance);
                retractDelay = 0.35f;
            } else {
                if (tonguePullUnblock || tonguePullUnblockPullOnly) {
                    if (movementDistance < 1) {
                        movementDistance += Time.deltaTime * movementSpeed;
                        if (movementDistance > 1) movementDistance = 1;
                        rb.MovePosition(state.initPos + state.tempPos * movementDistance);
                    }
                } else {
                    retractDelay -= Time.deltaTime;
                    if (retractDelay <= 0) {
                        movementDistance -= Time.deltaTime * retractSpeed;
                        if (movementDistance < 0) movementDistance = 0;
                        rb.MovePosition(state.initPos + state.tempPos * movementDistance);
                    }
                }
            }

            movementPercentage = movementDistance / maxMovementDistance;

            if (isDBTrain && movementPercentage > 0.9f && trainMode == 0) {
                if (DataLoader.instance.getDS("db-burial") != 0 && DataLoader.instance.getDS("wrestling-3") == 0) {
                    trainMode = 1;
                }
            }
        }
        int trainMode = 0;

        float initialTongueXOffset = 0;
        Vector2 _size = new Vector2();
        Vector2 _offset = new Vector2();
        private void UpdateTongue() {
            if (stopZoneTrigger.onPlayer2D && !tongueIsOneWay) {
                tonguePull.tonguePullUnblock = true;
            } else {
                tonguePull.tonguePullUnblock = false;
            }

            int reversed = tongueRetracts ? -1 : 1;
            if (tonguePull.movementPercentage > furthestMovementPercentage) furthestMovementPercentage = tonguePull.movementPercentage;
            if (tongueIsOneWay && tonguePull.movementPercentage < furthestMovementPercentage) return;
            rb.MovePosition(state.initPos + state.tempPos * reversed * maxMovementDistance * tonguePull.movementPercentage);
            if (tongueRetracts) {
                float distance = maxMovementDistance * tonguePull.movementPercentage;
                _size.Set(1 + IfRetractsTileOffset - distance, bc.size.y);
                _offset.Set(initialTongueXOffset + (IfRetractsTileOffset  - distance)*0.5f, bc.offset.y);
                bc.size = _size;
                bc.offset = _offset;
            } else {
                // Assumes the default is a tongue that extends leftwards with sprite pivoted in center
                float distance = maxMovementDistance * tonguePull.movementPercentage;
                _size.Set(1 + distance, bc.size.y);
                _offset.Set(initialTongueXOffset + 0.5f*distance, bc.offset.y);
                bc.size = _size;
                bc.offset = _offset;
            }

        }


        private void OnCollisionEnter2D(Collision2D collision) {
            if (collision.collider.CompareTag("Player")) {
                state.onPlayer = true;
            }
        }

        private void OnCollisionExit2D(Collision2D collision) {
            if (collision.collider.CompareTag("Player")) {
                state.onPlayer = false;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision) {
            if (!isPull) return;
            if (collision.CompareTag("Player")) tonguePullUnblockPullOnly = true;
        }
        private void OnTriggerExit2D(Collider2D collision) {
            if (!isPull) return;
            if (collision.CompareTag("Player")) tonguePullUnblockPullOnly = false;
        }
        bool tonguePullUnblockPullOnly = false;

        Transform BugTrainTransform;
        float bugspeed = 0;
        bool bugleaving = false;
        Vector3 tempv = new Vector3();
        void UpdateTrain() {
            if (trainMode == 0) {

            } else if (trainMode == 1) {
                player.enabled = false;
                // BugTrain -> Tongue -> Entity
                player.transform.parent = transform.parent.parent;
                BugTrainTransform = GameObject.Find("BugtrainStuff").transform;
                GameObject.Find("BugtrainSprite").GetComponent<SpriteAnimator>().Play("move");
                CutsceneManager.deactivatePlayer = true;
                player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "NanoDustbound") {
                    trainMode = 2;
                } else {
                    trainMode = 3;
                }
                player.StopSuckingAnim();
                player.GetComponent<Animator>().speed = 0;
                GetComponent<CircleCollider2D>().enabled = false;
                player.GetComponent<CircleCollider2D>().enabled = false;
                GameObject.Find("TrainCol1").GetComponent<BoxCollider2D>().enabled = false;
                GameObject.Find("TrainCol2").GetComponent<BoxCollider2D>().enabled = false;

            } else if (trainMode == 2) {
                bugspeed += 3*Time.deltaTime; if (bugspeed > 10) bugspeed = 10;
                tempv = BugTrainTransform.position;
                tempv.x -= bugspeed * Time.deltaTime;
                BugTrainTransform.position = tempv;
            } else if (trainMode == 3) {
                bugspeed += 3*Time.deltaTime; if (bugspeed > 10) bugspeed = 10;
                tempv = BugTrainTransform.position;
                tempv.x += bugspeed * Time.deltaTime;
                BugTrainTransform.position = tempv;
            }

            if (bugspeed > 9 && !bugleaving) {
                bugleaving = true;
                CutsceneManager.deactivatePlayer = false;
                if (trainMode == 2) {
                    DataLoader.instance.enterScene("Entrance", Registry.GameScenes.NanoHandfruitHaven);
                } else if (trainMode == 3) {
                    DataLoader.instance.enterScene("HavenEntrance", Registry.GameScenes.NanoDustbound);
                }
            }
        }

    }
}