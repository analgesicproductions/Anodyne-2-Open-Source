using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Anodyne;
public class MoonDasher : MonoBehaviour {

    public float maxSpeed = 7.5f;
    public float accelFactor = 14f;
    public float speedUpFactor = 1.05f;
    bool startsMovingRight = true;
    Vector2 tempVel = new Vector2();
    SpriteAnimator anim;
    Rigidbody2D rb;
    public float distanceToTravel = 10f;
    public float startOffset = 0f;
    Vector2 initPos;

    public bool oscillates = false;
    public float oscillation_amp = 1.5f;
    float t_osc = 0;
    public float oscillation_period = 1f;
    float init_osc_y = 0;
    public bool usesConstantVel = false;
    public float constantVel = 3f;

	void Start () {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<SpriteAnimator>();
        initPos = transform.position;
        Vector2 v = transform.position; v.x += startOffset; transform.position = v;
        if (startsMovingRight) {
            mode = 0;
            anim.Play("right");
        }
        if (!startsMovingRight) {
            mode = 1;
            anim.Play("left");
        }
        HF.GetPlayer(ref player);
        HF.GetRoomPos(transform.position, ref initroom);
        init_osc_y = transform.position.y;
	}
    AnoControl2D player;

    int mode = 0;
    Vector2Int initroom;
    bool playerinroom = false;
    bool isPaused = false;
    Vector2 cachedVel;
	void FixedUpdate () {


        if (!playerinroom) {
            if (HF.AreTheseInTheSameroom(player.transform,transform)){
                playerinroom = true;
            }
            return;
        } else {
            if (!player.InThisRoom(initroom)) {
                playerinroom = false;
                mode = 0;
                t_osc = 0;
                rb.velocity = Vector2.zero;
                transform.position = initPos;
                if (startsMovingRight) {
                    anim.Play("right");
                } else {
                    anim.Play("left");
                }
                Vector2 v = transform.position; v.x += startOffset; transform.position = v;
                return;
            }
        }


        if (!isPaused) {
            if (player.IsThereAReasonToPause()) {
                isPaused = true;
                cachedVel = rb.velocity;
                rb.velocity = Vector2.zero;
                return;
            }
        } else if (isPaused) {
            if (!player.IsThereAReasonToPause()) {
                isPaused = false;
                rb.velocity = cachedVel;
            }
            return;
        }

        tempVel = rb.velocity;
		if (mode == 0) {
            tempVel.x += accelFactor * Time.fixedDeltaTime;
            tempVel.x *= speedUpFactor;
            if (usesConstantVel) {
                tempVel.x = constantVel;
            }
            rb.velocity = tempVel;

            if ((startsMovingRight && transform.position.x >= initPos.x + distanceToTravel) || (!startsMovingRight && transform.position.x >= initPos.x)) {
                rb.velocity = Vector2.zero;
                mode = 1;
                anim.Play("left");
            }
        } else if (mode == 1) {
            tempVel.x -= accelFactor * Time.fixedDeltaTime;
            tempVel.x *= speedUpFactor;

            if (usesConstantVel) {
                tempVel.x = -constantVel;
            }
            rb.velocity = tempVel;
            if ((startsMovingRight && transform.position.x <= initPos.x) || (!startsMovingRight && transform.position.x <= initPos.x - distanceToTravel)) {
                rb.velocity = Vector2.zero;
                mode = 0;
                anim.Play("right");
            }
        }

        if (oscillates) {
            t_osc += Time.fixedDeltaTime;
            if (t_osc >= oscillation_period) {
                t_osc = 0;
            }
            float new_y = init_osc_y + oscillation_amp * Mathf.Sin(6.28f * (t_osc / oscillation_period));
            tempPos = transform.position;
            tempPos.y = new_y;
            transform.position = tempPos;
        }

	}

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.name == Registry.PLAYERNAME2D) {
            if (!GameObject.Find(Registry.PLAYERNAME2D).GetComponent<AnoControl2D>().CameraIsChangingRooms()) {
                GameObject.Find(Registry.PLAYERNAME2D).GetComponent<AnoControl2D>().Damage();
            }
        }
    }

    Vector3 tempPos = new Vector3();
}
