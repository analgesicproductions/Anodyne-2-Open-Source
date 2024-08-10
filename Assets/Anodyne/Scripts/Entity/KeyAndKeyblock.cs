using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Anodyne;
public class KeyAndKeyblock : MonoBehaviour {

    public bool IsKey = false;
    [Header("Keyblock Properties")]
    public string flagname = "";
    SpriteAnimator anim;
    EntityState2D state;
    UIManager2D ui;
    Transform keyMaskT;
    DialogueBox dbox;

    SpriteRenderer skelBub;

	void Start () {
        HF.GetDialogueBox(ref dbox);
        ui = GameObject.Find("2D UI").GetComponent<UIManager2D>();
        anim = GetComponent<SpriteAnimator>();
		if (!IsKey) {
            state = GetComponent<EntityState2D>();
            flagname = "Keyblock" + SceneManager.GetActiveScene().name+flagname;
            flagname = flagname.Replace(" ", "");
            if (DataLoader.instance.getDS(flagname) == 1) {
                anim.Play("on");
                mode = 4;
            }
        } else {
            keyMaskT = transform.parent;
            anim.Play("idle_l");
            HF.GetPlayer(ref player);
            if (!skippos) transform.position = player.transform.position + new Vector3(0, 1,0);
        }


    }

    bool skippos = false;
    public void SkeligumInit() {

        if (isGooKey || isSkelKey) {
            skelBub = transform.Find("bubble").GetComponent<SpriteRenderer>();
            skelBub.enabled = false;
            if (isGooKey) {
                skeligumPos1 = GameObject.Find("GooPos1").transform.position;
                skeligumPos2 = GameObject.Find("GooPos2").transform.position;
                skeligumPos3 = GameObject.Find("GooPos3").transform.position;
                skeligumPos4 = GameObject.Find("GooPos4").transform.position;
            } else {
                skeligumPos1 = GameObject.Find("SkelPos1").transform.position;
                skeligumPos2 = GameObject.Find("SkelPos2").transform.position;
                skeligumPos3 = GameObject.Find("SkelPos3").transform.position;
                skeligumPos4 = GameObject.Find("SkelPos4").transform.position;
            }

            if (isGooKey) {
                name = "GooKey";

                int skelGooState = DataLoader._getDS("skel-goo-state");
                int gooKeyFound = DataLoader._getDS("goo-key-found");

                // If chest was opened but not done with the goo sequence, hover around first point
                if (gooKeyFound == 1 && skelGooState == 0) {
                    skeligumMode = 1;
                    transform.position = skeligumPos1;
                    skippos = true;

                }
                if (gooKeyFound == 0) DataLoader._setDS("goo-key-found", 1);

                // 0 = follow normally (beginning)
                if (skelGooState == 1) {
                    skeligumMode = 8;
                    transform.position = skeligumPos4;
                    skippos = true;
                }
            } else if (isSkelKey) {
                name = "SkelKey";
                int skelSkelState = DataLoader._getDS("skel-skel-state");
                int skelKeyFound = DataLoader._getDS("skel-key-found");
                if (skelKeyFound == 1 && skelSkelState == 0) {
                    skeligumMode = 1;
                    transform.position = skeligumPos1;
                    skippos = true;
                }
                if (skelKeyFound == 0) DataLoader._setDS("skel-key-found", 1);

                // 0 = follow normally (beginning)
                if (skelSkelState == 1) {
                    skeligumMode = 8;

                    transform.position = skeligumPos4;
                    skippos = true;
                }
            }
        }
    }

    Vector3 skeligumPos1;
    Vector3 skeligumPos2;
    Vector3 skeligumPos3;
    Vector3 skeligumPos4;

    AnoControl2D player;
    public static string nameOfAskingKeyblock = "";
    [System.NonSerialized]
    public int mode = 0;
    GameObject keyblock;

    public void TurnKeyBlockOn() {
        if (mode == 2) {
            mode = 3;
            DataLoader.instance.setDS(flagname, 1);
            anim.Play("on");
            HF.SendSignal(state.children);
        }
    }
    int followmode = 0;
    Vector2 nextDestOffsetFromP = new Vector2();
    Vector2 curVel = new Vector2();
    Vector2 targetVel = new Vector2();
    Vector2 playerP = new Vector2();
    Vector2 tempPos = new Vector2();
    Vector2 targetPos = new Vector2();
    Vector2 cachedWorldPos = new Vector2();

    [System.NonSerialized]
    public bool isGooKey = false;

    [System.NonSerialized]
    public bool isSkelKey = false;

    [System.NonSerialized]
    public int skeligumMode = 0;

    public float maxSpeed = 4f;
    public float followThreshold = 1.5f;
    public float lerpSpeed = 2f;

    float floatDegrees = 0;
    Vector2 lastFloatOffset = new Vector2();
	void Update () {
		if (IsKey) {
            if (mode == 0) {
                if (skeligumMode == 0 && (isGooKey || isSkelKey)) {
                    if (player.InSameRoomAs(skeligumPos1)) {
                        skelBub.enabled = true;
                        skeligumMode = 1; // orbit around pt 1
                    }
                } else if (skeligumMode == 1) {
                    // Say dialogue then start to follow player again
                    if (onplayer && MyInput.jpTalk && Vector2.Distance(transform.position, skeligumPos1) < 1f) {
                        if (isGooKey) dbox.playDialogue("goo-key", 1);
                        if (isSkelKey) dbox.playDialogue("skel-key", 1);
                        skeligumMode = 2;
                        skelBub.enabled = false;
                    }
                } else if (skeligumMode == 2 && dbox.isDialogFinished() && player.InSameRoomAs(skeligumPos2)) {
                    skeligumMode = 3;
                    skelBub.enabled = true;
                    // when in room with pt 2, start to follow pt 2.
                } else if (skeligumMode == 3) {
                    if (onplayer && MyInput.jpTalk && Vector2.Distance(transform.position, skeligumPos2) < 1f) {
                        if (isGooKey) dbox.playDialogue("goo-key", 2);
                        if (isSkelKey) dbox.playDialogue("skel-key", 2);
                        skeligumMode = 4;
                        skelBub.enabled = false;
                    }
                } else if (skeligumMode == 4 && dbox.isDialogFinished() && player.InSameRoomAs(skeligumPos3)) {
                    skeligumMode = 5;
                    skelBub.enabled = true;
                } else if (skeligumMode == 5) {
                    if (onplayer && MyInput.jpTalk && Vector2.Distance(transform.position, skeligumPos3) < 1f) {
                        skelBub.enabled = false;
                        if (isGooKey) dbox.playDialogue("goo-key", 3);
                        if (isSkelKey) dbox.playDialogue("skel-key", 3);
                        skeligumMode = 6;
                    }
                } else if (skeligumMode == 6 && dbox.isDialogFinished()) {
                    skeligumMode = 7;
                } else if (skeligumMode == 7) {
                    // when in last room, set a state var (so if scene reloaded, key starts off from here)
                    if (player.InSameRoomAs(skeligumPos4)) {
                        if (isGooKey) DataLoader._setDS("skel-goo-state", 1);
                        if (isSkelKey) DataLoader._setDS("skel-skel-state", 1);
                        skeligumMode = 10;
                    }
                } else if (skeligumMode == 10) {
                    // check if both keys reached the gate. play dialog as needed
                    if (DataLoader._getDS("skel-goo-state") == 1 && DataLoader._getDS("skel-skel-state") == 1) {
                        skeligumMode = 9;
                        dbox.playDialogue("skel-both");

                        // make the other key move
                        if (isSkelKey) GameObject.Find("GooKey").GetComponent<KeyAndKeyblock>().skeligumMode = 9;
                        if (isGooKey) GameObject.Find("SkelKey").GetComponent<KeyAndKeyblock>().skeligumMode = 9;
                    } else {
                        skeligumMode = 8; // this was the first key, hover around end point
                        if (isGooKey) dbox.playDialogue("goo-key", 4);
                        if (isSkelKey) dbox.playDialogue("skel-key", 4);
                    }
                } else if (skeligumMode == 8) {
                    // hover and wait for other key to arrive
                } else if (skeligumMode == 9 && dbox.isDialogFinished()) {
                    if (isGooKey) {
                        nameOfAskingKeyblock = "GooKeyblock";
                    } else {
                        nameOfAskingKeyblock = "SkelKeyblock";
                    }
                    skeligumMode = 11;
                }
                playerP = player.transform.position;

                if (skeligumMode == 1) playerP = skeligumPos1;
                if (skeligumMode == 3) playerP = skeligumPos2;
                if (skeligumMode == 5) playerP = skeligumPos3;
                if (skeligumMode == 8) playerP = skeligumPos4;
                if (skeligumMode == 9) playerP = skeligumPos4;
                if (followmode == 0) {
                    // Begin following when  below the player or far from it
                    // technically the last conditional shoulud be  <, but with >  the fly is always recalculating a pos which
                    // leads to a nicer flying
                    if (Vector2.Distance(transform.position, playerP) > followThreshold || transform.position.y - playerP.y > 0.5f) {
                        nextDestOffsetFromP.Set(0, followThreshold - 0.1f);
                        HF.RotateVector2(ref nextDestOffsetFromP, -60f + 120f * Random.value);
                        followmode = 1;
                    }
                } else if (followmode == 1) {
                    tempPos = transform.position;
                    targetVel = ((playerP + nextDestOffsetFromP) - tempPos).normalized * maxSpeed;

                    curVel = Vector2.Lerp(curVel, targetVel, Time.deltaTime * lerpSpeed);
                    tempPos += curVel * Time.deltaTime;
                    transform.position = tempPos;
                    if (curVel.x > 0.5f) {
                        anim.Play("idle_r");
                    } else if (curVel.x <= -0.5f) {
                        anim.Play("idle_l");
                    }
                    if (Vector2.Distance(playerP + nextDestOffsetFromP, tempPos) < 0.5f) {
                        followmode = 0;
                    }
                }

                tempPos = transform.position;
                tempPos -= lastFloatOffset;
                lastFloatOffset.y = 0.2f * Mathf.Sin(Mathf.Deg2Rad * floatDegrees);
                tempPos += lastFloatOffset;
                transform.position = tempPos;
                floatDegrees += Time.deltaTime * 360f * 0.7f;
                if (floatDegrees >= 360f) floatDegrees -= 360f;

                // A Keyblock will set this if you interact near a keyblock
                if (nameOfAskingKeyblock != "") {
                    keyblock = GameObject.Find(nameOfAskingKeyblock);
                    nameOfAskingKeyblock = "";
                    mode = 1;
                    targetPos = keyblock.transform.position;
                    targetPos.y += 1;
                }

                // Float key to 1 above the block
            } else if (mode == 1) {
                // Will this get stuck? No, as there's a 3 pixel radius around the destination and the velocity is always changing towards it.
                tempPos = transform.position;
                targetVel = (targetPos - tempPos).normalized * maxSpeed;
                curVel = Vector2.Lerp(curVel, targetVel, Time.deltaTime * lerpSpeed);
                tempPos += curVel * Time.deltaTime;
                transform.position = tempPos;
                if (Vector2.Distance(tempPos, targetPos) < 3 / 16f) {
                    mode = 2;
                }

            } else if (mode == 2) {
                // Move precisely to 1 above keyblock
                tempPos = transform.position;
                HF.ReduceVec2ToVec(ref tempPos, targetPos, 2 * Time.deltaTime);
                transform.position = tempPos;

                //Will this get stuck? No, because the position is being moved precisely
                // when close enough:
                // change anim
                // cache key world pos
                // change masking behavior
                // Move keyMask to 0.33 below block, move key world pos back
                if (Vector2.Distance(tempPos, targetPos) < 1 / 16f) {
                    transform.position = targetPos;
                    if (curVel.x > 0) anim.Play("enter_r");
                    if (curVel.x <= 0) anim.Play("enter_l");
                    cachedWorldPos = transform.position;
                    GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;

                    tempPos = keyblock.transform.position;
                    tempPos.y -= 0.33f;
                    keyMaskT.position = tempPos;

                    transform.position = cachedWorldPos;
                    targetPos = cachedWorldPos;
                    targetPos.y -= 0.8f;

                    mode = 3;
                }
                // move key down 0.8f
            } else if (mode == 3) {
                tempPos = transform.position;
                tempPos.y -= Time.deltaTime;
                transform.position = tempPos;
                if (tempPos.y <= targetPos.y) {
                    transform.position = targetPos;
                    AudioHelper.instance.playOneShot("openChest",1,1f);
                    mode = 10;
                }
            } else if (mode == 10) { 
                // when in place, turn keyblock on and kill self
                if (isGooKey || isSkelKey) {
                    keyblock.GetComponent<KeyAndKeyblock>().mode = 2;
                }
                keyblock.GetComponent<KeyAndKeyblock>().TurnKeyBlockOn();
                DataLoader.instance.setDS(flagname, 2);
                Destroy(gameObject);
                return;
            }
        } else {
            if (mode == 0) {
                if (onplayer && dbox.isDialogFinished() && (MyInput.jpTalk || MyInput.jpConfirm)) {
                    nameOfAskingKeyblock = name;
                    mode = 1;
                }
            } else if (mode == 1) {
                if (nameOfAskingKeyblock != "") {
                    nameOfAskingKeyblock = "";
                    dbox.playDialogue("keyblock-text");
                    onplayer = false; ui.setTalkAvailableIconVisibility(false);
                    mode = 0;
                } else {
                    ui.setTalkAvailableIconVisibility(false);
                    mode = 2;
                }
            } else if (mode == 2) {
                // Wait for key to enter
            } else if (mode == 3) {
                // Is on, do nothing
            } else if (mode == 4) {
                // signal so gates have their transforms set up 
                HF.SendSignal(state.children);
                mode = 3;
            }
        }
	}

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            onplayer = true;
            if (isGooKey || isSkelKey) {
                if (skeligumMode == 1 || skeligumMode == 3 || skeligumMode == 5) {
                } else {
                    return;
                }
            }

            if (mode == 0) ui.setTalkAvailableIconVisibility(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            onplayer = false;
            if (mode == 0) ui.setTalkAvailableIconVisibility(false);
        }

    }
    bool onplayer = false;
}
