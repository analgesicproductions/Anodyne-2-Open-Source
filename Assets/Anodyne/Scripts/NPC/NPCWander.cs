using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Anodyne;
public class NPCWander : MonoBehaviour {

    public float minChangeTime = 1f;
    public float maxChangeTime = 1.5f;

    float t_change;
    float tm_change;

    public bool onlyChangeVelocityNotAnim = false;
    public bool pauseAnimWhenTalking = true;
    public bool pauseOnAnimChange = false;
    public float pauseOnAnimChangeTime = 0.5f;

    float t_pauseOnAnimChangeTime = 0;


    public bool HasDirectionalIdleAnims = false;
    public bool HasDirectionalWalkAnims = false;
    public bool fourDirWander = true;
    public bool horBackAndForth = false;
    public bool vertBackAndForth = false;


    public float movementSpeed = 0;

    Rigidbody2D rb;
    SpriteAnimator anim;
    AnoControl2D player;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<SpriteAnimator>();
        tm_change = Random.Range(minChangeTime, maxChangeTime);
        HF.GetPlayer(ref player);

        resetBehavior();


	}

    public void resetBehavior() {
        if (!enabled) return;
        t_change = tm_change;
        if (horBackAndForth) {
            nextvel.Set(movementSpeed, 0);
        } else if (vertBackAndForth) {
            nextvel.Set(0, movementSpeed);
        } else if (fourDirWander) {
            nextvel.Set(movementSpeed, 0);
        }
        if (!onlyChangeVelocityNotAnim && HasDirectionalWalkAnims) anim.Play(HF.getDirAnimBasedOnOneDirVel(rb.velocity, "walk"));
        rb.velocity = nextvel;
    }



    bool onplayer = false;
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            onplayer = true;
        }        
    }
    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            onplayer = false;
        }
    }

    // Update is called once per frame

    Vector2 nextvel = new Vector2();
    bool pausedForPlayer;
    bool pausedForOverlapPlayer = false;
	void Update () {

        if (!pausedForOverlapPlayer) {
            if (onplayer) {
                pausedForOverlapPlayer = true;
                if (pauseAnimWhenTalking) anim.paused = true;
                if (!pauseAnimWhenTalking) velWasSetToZeroDuringAnimChange = true;
                rb.velocity = Vector2.zero;
            }
        }else {
            if (!onplayer) {
                pausedForOverlapPlayer = false;
            } else {
                if (MyInput.jpTalk && HasDirectionalIdleAnims) {
                    if (player.facing == AnoControl2D.Facing.RIGHT) anim.Play("idle_l");
                    if (player.facing == AnoControl2D.Facing.LEFT) anim.Play("idle_r");
                    if (player.facing == AnoControl2D.Facing.UP) anim.Play("idle_d");
                    if (player.facing == AnoControl2D.Facing.DOWN) anim.Play("idle_u");
                } else if (MyInput.jpTalk && HasDirectionalWalkAnims) {
                    if (player.facing == AnoControl2D.Facing.RIGHT) anim.Play("walk_l");
                    if (player.facing == AnoControl2D.Facing.LEFT) anim.Play("walk_r");
                    if (player.facing == AnoControl2D.Facing.UP) anim.Play("walk_d");
                    if (player.facing == AnoControl2D.Facing.DOWN) anim.Play("walk_u");
                }
            }
        }

       if (!pausedForPlayer && player.isPaused) {
            pausedForPlayer = true;
            if (pauseAnimWhenTalking) anim.paused = true;
            if (!pauseAnimWhenTalking) velWasSetToZeroDuringAnimChange = true;
            rb.velocity = Vector2.zero;
        } else {
            if (!player.isPaused) {
                pausedForPlayer = false;
            }
        }

        if (anim.paused || velWasSetToZeroDuringAnimChange) {
            t_pauseOnAnimChangeTime -= Time.deltaTime;
            if (t_pauseOnAnimChangeTime < 0 && !onplayer && !pausedForPlayer) {
                anim.paused = false;
                velWasSetToZeroDuringAnimChange = false;
                rb.velocity = nextvel;
                if (!onlyChangeVelocityNotAnim && HasDirectionalWalkAnims) anim.Play(HF.getDirAnimBasedOnOneDirVel(rb.velocity, "walk"));
            }
            return;
        }

        t_change += Time.deltaTime;
        if (t_change > tm_change) {
            t_change = 0;
            tm_change = Random.Range(minChangeTime, maxChangeTime);
            Vector2 newvel = new Vector2();
            if (fourDirWander) {
                newvel = rb.velocity;
                //SeanHF.randomizeVec2ToOneDir(ref newvel, movementSpeed);
                if (newvel.x > 0 && newvel.y == 0) {
                    newvel.Set(0, -movementSpeed);
                } else if (newvel.x == 0 && newvel.y < 0) {
                    newvel.Set(-movementSpeed, 0);
                } else if (newvel.x < 0 && newvel.y == 0) {
                    newvel.Set(0, movementSpeed);
                } else {
                    newvel.Set(movementSpeed, 0);
                }
                rb.velocity = newvel;
            } else if (horBackAndForth) {
                newvel = rb.velocity;
                newvel.x = Mathf.Clamp(newvel.x, -movementSpeed, movementSpeed);
                newvel.x *= -1;
                newvel.y = 0;
                rb.velocity = newvel;
            } else if (vertBackAndForth) {
                newvel = rb.velocity;
                newvel.y = Mathf.Clamp(newvel.y, -movementSpeed, movementSpeed);
                newvel.y *= -1;
                newvel.x = 0;
                rb.velocity = newvel;
            }
            nextvel = rb.velocity;

            if (onlyChangeVelocityNotAnim) {
            } else { 
                if (pauseOnAnimChange) {
                    t_pauseOnAnimChangeTime = pauseOnAnimChangeTime;
                    rb.velocity = Vector2.zero;
                    if (pauseAnimWhenTalking) anim.paused = true;
                    if (!pauseAnimWhenTalking) velWasSetToZeroDuringAnimChange = true;
                } else {
                    if (HasDirectionalWalkAnims) anim.Play(HF.getDirAnimBasedOnOneDirVel(rb.velocity, "walk"));
                }
            }
        }

	}
       bool velWasSetToZeroDuringAnimChange = false;
}
