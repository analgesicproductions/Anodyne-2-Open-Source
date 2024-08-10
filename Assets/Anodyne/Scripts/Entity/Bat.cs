using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Anodyne {
    public class Bat : MonoBehaviour {

        int mode = 0;
        public float detectionDistance = 5f;
        public float approachVelocity = 2f;
        public float beginRotationDistance = 2.5f;
        float variableRotationDistance = 0;
        public float circleAngularSpeed = 150f;
        public float rotationTime = 2.4f;
        float t_rotationTime = 0;
        public float dashVelocity = 15f;
        public float dashTime = 0.5f;
        float t_dashTime = 0;
        Rigidbody2D rb;
       // SpriteAnimator anim;
        Transform playerT;
        AnoControl2D player;
        Vacuumable vacuumable;
        Vector2Int initRoom;
        void Start() {
            vacuumable = GetComponent<Vacuumable>();
            playerT = GameObject.Find(Registry.PLAYERNAME2D).transform;
            player = GameObject.Find(Registry.PLAYERNAME2D).GetComponent<AnoControl2D>();
            rb = GetComponent<Rigidbody2D>();
         //   anim = GetComponent<SpriteAnimator>();
            HF.GetRoomPos(transform.position, ref initRoom);
            variableRotationDistance = beginRotationDistance;
        }

        Vector2 v1 = new Vector2();
        Vector3 vec3 = new Vector2();
        void Update() {

            // Update roompos when shot
            if (vacuumable.isMoving()) {
                HF.GetRoomPos(transform.position, ref initRoom);
                return;
            }

            // On Break
            if (vacuumable.JustBrokeResetExternally) {
                vacuumable.JustBrokeResetExternally = false;
                mode = 0;
                rb.velocity = Vector2.zero;
            }

            // Ignore this script code when sucked up
            if (vacuumable.isPickedUp()) {
                mode = 4;
                rb.velocity = Vector2.zero;
                return;
            }

            // Entity specific respawn code
            if (mode != 0 && !HF.IsInRoom(playerT.position,initRoom.x,initRoom.y)) {
                mode = 0;
                rb.velocity = Vector2.zero;
                HF.GetRoomPos(transform.position, ref initRoom);
                return;
            }


            if (mode == 0) {
                if (Vector2.Distance(playerT.position,transform.position) < detectionDistance) {
                    mode = 1;
                }
            } else if (mode == 1) {
                v1 = playerT.position - transform.position; v1.Normalize();
                v1 *= approachVelocity;
                rb.velocity = v1;
                if (Vector2.Distance(playerT.position,transform.position) < beginRotationDistance) {
                    mode = 2;
                    rb.velocity = Vector2.zero;
                    variableRotationDistance = Vector2.Distance(playerT.position, transform.position);
                    if (variableRotationDistance < 0.3f) return;
                }
            } else if (mode == 2) {
                v1 = transform.position - playerT.position;
                v1 = v1.normalized * variableRotationDistance;
                float c = 1;
                if (vacuumable.IsBeingSuckedAndMoving) {
                    t_rotationTime = 0;
                    variableRotationDistance -= beginRotationDistance * 2* Time.deltaTime;
                    if (variableRotationDistance < 0.3f) variableRotationDistance = 0.3f;
                    HF.jitterVector2(ref v1, 0.0925f);
                    c = 0f;
                } else {
                    variableRotationDistance = HF.ReduceToVal(variableRotationDistance, beginRotationDistance, Time.deltaTime);
                }
                if (t_rotationTime > 0.75f *rotationTime) {
                    c = 0;
                }
                HF.RotateVector2(ref v1, Time.deltaTime * circleAngularSpeed*c);
                v1.x += playerT.position.x;
                v1.y += playerT.position.y;
                transform.position = v1;

                if (HF.TimerDefault(ref t_rotationTime, rotationTime)) {
                    mode = 3;
                    v1 = playerT.position - transform.position; v1.Normalize(); v1 *= dashVelocity;
                    rb.velocity = v1;
                }
            } else if (mode == 3) {

                vec3 = transform.position;
                HF.ConstrainVecToRoom(ref vec3, initRoom.x, initRoom.y);
                transform.position = vec3;
                v1 = rb.velocity;
                v1 *= 0.95f;
                HF.ReduceVec2To0(ref v1, Time.deltaTime * dashVelocity*0.5f);
                rb.velocity = v1;
                if (HF.TimerDefault(ref t_dashTime,dashTime)) {
                    mode = 0;
                    rb.velocity = Vector2.zero;
                }
            } else if (mode == 4) { // after picked up

            }
        }

        private void OnTriggerEnter2D(Collider2D collision) {
            if (mode == 3 && !vacuumable.IsBeingSuckedAndMoving && collision.CompareTag("Player")) {
                player.Damage(10);
            } else {
                if (mode != 4 && collision.GetComponent<Vacuumable>() != null) {
                    Vacuumable v = collision.GetComponent<Vacuumable>();
                    if (v.isMoving()) {
                        vacuumable.Break();
                        mode = 4;
                    }
                }
            }
        }
    }
}