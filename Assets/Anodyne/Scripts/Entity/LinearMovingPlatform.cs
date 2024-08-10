using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearMovingPlatform : MonoBehaviour {


    Vector3 initpos;
    public Transform dest;
    Vector3 destpos;
    float t = 0;
    public float movetime = 8f;
    public float waittimeAtEnd = 3f;
    public bool WaitForSignalToEnable = false;
    public bool BeginsMovementWhenPlayerOn = true;
    bool returning = false;
    public bool BeginsMovementAutomatically = false;
    MediumControl player;
	// Use this for initialization



	void Start () {
        initpos = transform.position;
        destpos = dest.position;
        player = GameObject.Find("MediumPlayer").GetComponent<MediumControl>();
	}

    int mode = -1;
    bool playerOn = false;
    Vector3 lastpos;
    // Update is called once per frame

    float playerLastX = 0;
    float playerLastZ = 0;
	void FixedUpdate () {

        lastpos = transform.position;
        if (mode == -1) {
            if (!WaitForSignalToEnable) mode = 0;
        } if (mode == 0) {
            if (BeginsMovementAutomatically) mode = 1;
            if (BeginsMovementWhenPlayerOn && playerOn) mode = 1;
        } else if (mode == 1) {
            transform.position = Vector3.Lerp(initpos, destpos, t / movetime);
            if (returning) transform.position = Vector3.Lerp(destpos, initpos, t / movetime);


            Vector3 diff = transform.position - lastpos;
            if (playerOn) {
                Vector3 pos = player.transform.position;
                Rigidbody rb = player.GetComponent<Rigidbody>();

                float playerXDiff = pos.x - playerLastX;
                if (diff.y > 0 && Mathf.Abs(playerXDiff) < Mathf.Abs(rb.velocity.x * Time.fixedDeltaTime)) {
                    float extraX = (rb.velocity.x * Time.fixedDeltaTime) - playerXDiff;
                    diff.x += extraX*2;
                }
                playerLastX = player.transform.position.x;

                float playerZDiff = pos.z - playerLastZ;
                if (diff.y > 0 && Mathf.Abs(playerZDiff) < Mathf.Abs(rb.velocity.z * Time.fixedDeltaTime)) {
                    float extraZ = (rb.velocity.z * Time.fixedDeltaTime) - playerZDiff;
                    diff.z += extraZ * 2;
                }
                playerLastZ = player.transform.position.z;


                rb.MovePosition(pos + diff);
                if (diff.y < 0) {
                    float yVel = diff.y / Time.fixedDeltaTime;
                    Vector3 vel = rb.velocity;
                    if (vel.y < 1) {
                        vel.y = yVel * 1.2f;
                        rb.velocity = vel;
                    }
                }
            }

            if (HF.TimerDefault(ref t, movetime,Time.fixedDeltaTime)) {
                mode = 2;
                t = 0;
            }

        } else if (mode == 2) {

            if (HF.TimerDefault(ref t, waittimeAtEnd, Time.fixedDeltaTime)) {


                mode = 1;
                t = 0;

                returning = !returning;

                if (!returning && !BeginsMovementAutomatically) {
                    mode = 0;
                }

            }
        }
	}

    public void _Enable() {
        mode = 0;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.name == "MediumPlayer") {
            //print("player on");
            playerOn = true;
            player.OnMovingPlatform = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.name == "MediumPlayer") {
            //print("player off");
            playerOn = false;
            player.OnMovingPlatform = false;
        }

    }

}
