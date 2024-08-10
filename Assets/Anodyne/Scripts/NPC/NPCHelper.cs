using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Anodyne;
public class NPCHelper : MonoBehaviour {

    public bool restrictToRoom = true;
    public bool resetPosWhenPlayerLeavesRoom = true;
    public bool canBeSuckedToPlayer = false;

    [Header("Special NPC")]
    public GameObject bulletPrefab;
    public int specialNPCFlag = 0;
    public int hitpoints = 2;
    public bool isRageNPC = false;
    public bool isRageSkeligum= false;
    public bool isTongueNPC = false;
    bool isYolkIntroNPC = false;
    public bool isGolemGrave = false;
    public bool isCloneDrone = false;
    public bool isNeuroPulse = false;

    public bool isTradeNPC = false;

    List<Bullet> bullets;

    PositionShaker shaker;
    Vacuumable vac;
    Rigidbody2D rb;
    AnoControl2D player;
    SpriteAnimator anim;
    Vector3 initialPos;

    Vector2 playerP = new Vector3();
    Vector2 tempPos = new Vector2();
    Vector2 targetVel = new Vector2();
    Vector2 curVel = new Vector2();
    Vector2Int room;
    string flagID = "";
    [System.NonSerialized]
    public int mode = 0;
    SpriteRenderer sr;

    public EntityState2D state;

    float suckDialogueTimer = 0;
    float suckDialogueTimerMax = 0.5f;
    public string BeingSuckedDialogue = "";
    DialogueBox dbox;

	void Start () {
        HF.GetRoomPos(transform.position, ref room);
        HF.GetPlayer(ref player);
        sr = GetComponent<SpriteRenderer>();
        initialPos = transform.position;
        anim = GetComponent<SpriteAnimator>();
        dbox = GameObject.Find("Dialogue").GetComponent<DialogueBox>();
        if (name == "yummy yolk1") isYolkIntroNPC = true;
        if (BeingSuckedDialogue != "") {
            shaker = GetComponent<PositionShaker>();
        }
        if (GetComponent<Rigidbody2D>() != null) rb = GetComponent<Rigidbody2D>(); 

        if (isNeuroPulse) {
            t_cloneDrone = 2.5f;
        }

        if (canBeSuckedToPlayer) {
            vac = GetComponent<Vacuumable>();
            vac.overrideIdleDeceleration = true;
            state = GetComponent<EntityState2D>();
        }
        if (isRageNPC) {
            // ROOM -> RageNPCParent -> Entity
            flagID = "RageNPC" + transform.parent.parent.name; // this is entity
            if (transform.parent.Find("TriggerChecker1") != null) triggerCheck1 = transform.parent.Find("TriggerChecker1").gameObject.GetComponent<TriggerChecker>();
            if (transform.parent.Find("TriggerChecker2") != null) triggerCheck2 = transform.parent.Find("TriggerChecker2").gameObject.GetComponent<TriggerChecker>();
            name = "Entity";
            if (DataLoader.instance.getDS(flagID) == 0) {
                bullets = new List<Bullet>();
                for (int i = 0; i < 3; i++) {
                    GameObject g = Instantiate(bulletPrefab, transform.parent, true); bullets.Add(g.GetComponent<Bullet>());
                    g.SetActive(false);
                }
            }
            RageNPCReset();
        } else if (isTongueNPC) {
            GetComponent<DialogueAno2>().ext_DoesNotReactToOverlap = true;
            state = GetComponent<EntityState2D>();
        }
    }

    Vector3 nextpos = new Vector2();
    bool needsToReset = false;
	void Update () {
        if (restrictToRoom) {
            nextpos = transform.position;
            HF.ConstrainVecToRoom(ref nextpos, room.x, room.y);
            transform.position = nextpos;
        }

        if (resetPosWhenPlayerLeavesRoom && !player.CameraIsChangingRooms() && !player.InSameRoomAs(transform.position)) {
            if (needsToReset) {
                needsToReset = false;
                if (isRageNPC) RageNPCReset();
                if (GetComponent<NPCWander>() != null) {
                    GetComponent<NPCWander>().resetBehavior();
                }
                transform.position = initialPos;
            }
        }  else {
            needsToReset = true;
        }

        if (remainingFlickerTime > 0) {
            remainingFlickerTime -= Time.deltaTime;
            t_flicker += Time.deltaTime;
            if (t_flicker >= tm_flicker) {
                t_flicker -= tm_flicker;
                sr.enabled = !sr.enabled;
            }
            if (remainingFlickerTime < 0) {
                sr.enabled = true;
            } 
        }


        if (BeingSuckedDialogue != "" && dbox.isDialogFinished() && suckZoneOnThis && MyInput.special) {
            shaker.enabled = true;
            suckDialogueTimer += Time.deltaTime;
            if (suckDialogueTimer  > suckDialogueTimerMax) {
                suckDialogueTimer = 0;
                rb.velocity = Vector2.zero;
                player.NeedToReleaseSuckKeyToStartSuckingAgain = true;
                dbox.playDialogue(BeingSuckedDialogue);
            }
        } else {
            if (BeingSuckedDialogue != "" && shaker.enabled) {
                shaker.enabled = false;
            }
            suckZoneOnThis = false;
            suckDialogueTimer = 0;
        }
         
        if (canBeSuckedToPlayer && player.InSameRoomAs(transform.position)) {
            if (vac != null && vac.IsBeingSucked() && MyInput.special) {
                state.SetVelocityTowardsDestination(rb, player.transform, 2.5f);
            }
        }

        // Special NPC update routines

        if (isRageNPC && player.InSameRoomAs(transform.position)) {
            UpdateRageNPC();
        }

        if (isRageSkeligum) {
            UpdateRageSkeligum();
        }

        if (isYolkIntroNPC) {
            UpdateYolkIntroNPC();
        }
        if (isGolemGrave) {
            UpdateGolemGrave();
        }
        if (isCloneDrone) {
            UpdateCloneDrone();
        }
        if (isNeuroPulse) {
            t_cloneDrone += Time.deltaTime;
            if (t_cloneDrone > 3.1f) {
                t_cloneDrone -= 3.1f;
                anim.Play("Flash");
                anim.ScheduleFollowUp("Idle");
                if (particles == null) particles = GetComponentsInChildren<ParticleSystem>();
                if (player.InSameRoomAs(transform.position)) {
                    AudioHelper.instance.playOneShot("neurobubble");
                }
                foreach (ParticleSystem p in particles) {
                    p.Play();
                }
            }
        }
        if (isTradeNPC) {
            UpdateTradeNPC();
        }
	}
    int tradeMode = 0;
    void UpdateTradeNPC() {
        if (tradeMode == 0) {
            bulletPrefab.SetActive(false);
            tradeMode = 1;
        }
    }

    ParticleSystem[] particles;
 
    [System.NonSerialized]
    public int cloneFollowMode= 0;
    Vector2 nextDestOffsetFromP = new Vector2();
    float floatDegrees = 0;
    Vector2 lastFloatOffset = new Vector2();
    int cloneDroneMode = 0;
    Vector2 targetPos = new Vector2();
    Vector2 startPos = new Vector2();
    float t_cloneDrone = 0;
    SpriteAnimator cloneDroneUIAnim;
    public static bool CloneDroneFlashing = false;
    public void CloneDroneMoveToEndPos() {
        cloneFollowMode = 2;
        startPos = transform.position;
        targetPos = GameObject.Find("CloneDroneEndPos").transform.position;
        t_cloneDrone = 0;
    }

    void UpdateCloneDrone() {
        if (cloneDroneMode == 0) {
            if (Ano2Stats.HasCard(6)) {
                cloneDroneMode = -1;
                gameObject.SetActive(false);
                // turn off shadow npcs
                return;
            } else {
                transform.position = player.transform.position;
                cloneDroneMode = 1;
            }
        } else if (cloneDroneMode == 1) {
            if (cloneFollowMode == 0) {
                // Begin following when  below the player or far from it
                // technically the last conditional shoulud be  <, but with >  the fly is always recalculating a pos which
                // leads to a nicer flying
                if (Vector2.Distance(transform.position, player.transform.position) > 1.5f || transform.position.y - player.transform.position.y > 0.5f) {
                    nextDestOffsetFromP.Set(0, 1.5f - 0.1f);
                    HF.RotateVector2(ref nextDestOffsetFromP, -60f + 120f * Random.value);
                    cloneFollowMode = 1;
                }
            } else if (cloneFollowMode == 1) {
                playerP = player.transform.position;
                tempPos = transform.position;
                targetVel = ((playerP + nextDestOffsetFromP) - tempPos).normalized * 4;

                curVel = Vector2.Lerp(curVel, targetVel, Time.deltaTime * 2);
                tempPos += curVel * Time.deltaTime;
                transform.position = tempPos;
                if (curVel.x > 0.5f) {
                    if (CloneDroneFlashing) anim.Play("blink_r");
                    if (!CloneDroneFlashing) anim.Play("idle_r");
                } else if (curVel.x <= -0.5f) {
                    if (CloneDroneFlashing) anim.Play("blink_l");
                    if (!CloneDroneFlashing) anim.Play("idle_l");
                }
                if (Vector2.Distance(playerP + nextDestOffsetFromP, tempPos) < 0.5f) {
                    cloneFollowMode = 0;
                }
            } else if (cloneFollowMode == 2) {
                t_cloneDrone += Time.deltaTime;
                transform.position = Vector2.Lerp(startPos, targetPos, t_cloneDrone / 1f);
                if (t_cloneDrone >= 1 && dbox.isDialogFinished()) {
                    cloneFollowMode = 3;
                    suckZoneOnThisClone = false;
                }
            } else if (cloneFollowMode == 3) {
                if (suckZoneOnThisClone) {
                    targetPos = player.transform.position - transform.position;
                    targetPos.Normalize();
                    Vector3 v = transform.position;
                    v.x += targetPos.x * 5f * Time.deltaTime;
                    v.y += targetPos.y * 5f * Time.deltaTime;
                    transform.position = v;
                    if (Vector2.Distance(transform.position, player.transform.position) < 0.5f) {
                        cloneFollowMode = 4;
                        AudioHelper.instance.playOneShot("vacuumSucked");
                        RectTransform trans = GameObject.Find("CloneDroneUI").GetComponent<RectTransform>();
                        cloneDroneUIAnim = trans.GetComponent<SpriteAnimator>();
                        trans.SetParent(GameObject.Find("Game Render Texture").transform);
                        trans.SetSiblingIndex(GameObject.Find("UI Overlay").transform.GetSiblingIndex() + 2);
                        trans.localScale = new Vector3(1, 1, 1);
                        player.StopSuckingAnim();
                        // -144 -89
                        trans.localPosition = new Vector2(-144f + 10f, -89f);
                        trans.GetComponent<UnityEngine.UI.Image>().enabled = true;
                        GetComponent<SpriteRenderer>().enabled = false;
                        t_cloneDrone = 0;
                        CutsceneManager.deactivatePlayer = true;

                    }
                } else if (!MyInput.special) {
                    suckZoneOnThisClone = false;
                }
            } else if (cloneFollowMode == 4) {
                if (HF.TimerDefault(ref t_cloneDrone, 1.5f)) {
                    cloneDroneUIAnim.Play("spark");
                    cloneFollowMode = 5;
                    dbox.playDialogue("clone-dustfail", 5);
                }
            } else if (cloneFollowMode == 5 && dbox.isDialogFinished()) {
                    cloneDroneUIAnim.Play("break");
                    AudioHelper.instance.playOneShot("blockExplode");
                    cloneFollowMode = 6;
            } else if (cloneFollowMode == 6 && cloneDroneUIAnim.isPlaying == false) {
                cloneDroneUIAnim.gameObject.SetActive(false);
                cloneFollowMode = 7;
            } else if (cloneFollowMode == 7) {
                if (HF.TimerDefault(ref t_cloneDrone, 3f)) {
                    cloneDroneUIAnim = GameObject.Find("DustCrystalUI").GetComponent<SpriteAnimator>();
                    cloneDroneUIAnim.Play("break");
                    AudioHelper.instance.playOneShot("fireGateBurn");
                    cloneFollowMode = 8;
                }
            } else if (cloneFollowMode == 8) {
                if (cloneDroneUIAnim.isPlaying == false) {
                    cloneDroneUIAnim.gameObject.SetActive(false);
                    cloneFollowMode = 9;
                }
            } else if (cloneFollowMode == 9) {
                if (HF.TimerDefault(ref t_cloneDrone,0.5f)) {
                    cloneFollowMode = 10;

                    CutsceneManager.deactivatePlayer = false;
                }
            }

                // Bob up and down
                tempPos = transform.position;
            tempPos -= lastFloatOffset;
            lastFloatOffset.y = 0.2f * Mathf.Sin(Mathf.Deg2Rad * floatDegrees);
            tempPos += lastFloatOffset;
            transform.position = tempPos;
            floatDegrees += Time.deltaTime * 360f * 0.7f;
            if (floatDegrees >= 360f) floatDegrees -= 360f;
        }
    }

    int golemmode = 0;
    public GameObject golemDustPrefab;
    Ano2Dust golemDust;
    void UpdateGolemGrave() {
        if (golemmode == 0) {
            name = name.Replace(' ', '_');
            if (DataLoader.instance.getDS("GOLEM_"+name) == 0) {
                golemmode = 1;
                GetComponent<DialogueAno2>().enabled = false;
                GameObject g = Instantiate(golemDustPrefab,transform.position,Quaternion.identity,transform.parent) as GameObject;
                golemDust = g.transform.Find("DustEnt").GetComponent<Ano2Dust>();
                golemDust.dustValue = 2;
                golemDust.ext_destroysWhenPlayerGone = false;
            } else {
                golemmode = 10;
            }
        } else if (golemmode == 1) {
            if (golemDust.JustSuckedUp()) {
               golemmode = 10;
               DataLoader.instance.setDS("GOLEM_" + name, 1);
            }
         } else if (golemmode == 10) {
            GetComponent<DialogueAno2>().enabled = true;
            anim.Play("clean");
            golemmode = 11;
        }
    }

    int yolkmode = 0;
    SpriteRenderer yolkExclamationSR;
    float tyolk;
    float tyolk2 = 0.125f;
    void UpdateYolkIntroNPC() {
        if (yolkmode == 0) {
            if (Vector2.Distance(transform.position, player.transform.position) < 3.2f) {
                if (DataLoader.instance.getDS("yolkrobeintro") == 1) {
                    yolkmode = -1;
                } else {
                    yolkmode = 1;
                    yolkExclamationSR = transform.Find("exclamation").GetComponent<SpriteRenderer>();
                    yolkExclamationSR.enabled = true;
                    CutsceneManager.deactivatePlayer = true;
                    DataLoader.instance.setDS("yolkrobeintro", 1);
                }
            }
        } else if (yolkmode == 1) {
            tyolk += Time.deltaTime;
            if (tyolk > 0.125f) {
                tyolk = 0;
                yolkExclamationSR.enabled = !yolkExclamationSR.enabled;
            }
            tyolk2 += Time.deltaTime;
            if (tyolk2 > 1.5f) {
                yolkmode = 2;
                yolkExclamationSR.enabled = false;
                anim.Play("spin");
            }
        } else if (yolkmode == 2 && anim.isPlaying == false) {
            dbox.playDialogue("yolk1", 6,7);
            anim.Play("walk_d");
            yolkmode = 3;
        } else if (yolkmode == 3 && dbox.isDialogFinished()) {
            yolkmode = -1;
            CutsceneManager.deactivatePlayer = false;
        }
    }

    void RageNPCReset() {
        if (DataLoader.instance.getDS(flagID) == 1) {
            mode = -1;
        } else {
            if (mode == 1) Bullet.DieAll(bullets);
            mode = 0;
        }
    }

    float t_bullet;
    float tm_bullet = 1f;
    TriggerChecker triggerCheck1;
    TriggerChecker triggerCheck2;

    void UpdateRageSkeligum() {
        if (mode == 0) {
            if (player.InTheSameRoomAs(transform.position.x,transform.position.y)) {
                mode = 1;
                if (specialNPCFlag == 0) {
                    mode = 2; // ready to be cooled down
                }
            }
        } else if (mode == 1 || mode == 2) {
            // When player leaves, based on flag, reset visual state
            if (!player.InTheSameRoomAs(transform.position.x, transform.position.y)) {

                mode = 0;
                if (specialNPCFlag == 1) {
                    anim.Play("idle");
                    GetComponent<PositionShaker>().enabled = false;
                } else {
                    anim.Play("angry");
                    GetComponent<PositionShaker>().enabled = true;
                }
            }
        }
    }

    void UpdateRageNPC() {
        if (mode == 0) {
            mode = 1;
        } else if (mode == 1) {
            if ((triggerCheck1 != null && triggerCheck1.onPlayer2D) || (triggerCheck2 != null && triggerCheck2.onPlayer2D)) return;
            if (specialNPCFlag == 1) tm_bullet = 0.4f;
            if (specialNPCFlag == 2) return; // no bullets
            if (HF.TimerDefault(ref t_bullet, tm_bullet)) {
                Bullet b = Bullet.GetADeadBullet(bullets);
                if (b != null) {
                    b.gameObject.SetActive(true);
                    AudioHelper.instance.playOneShot("fireball");
                    if (specialNPCFlag == 0) b.LaunchWithWindupIdle(player.transform.position, 0.5f, 0.75f,1);
                    if (specialNPCFlag == 1) b.LaunchWithWindupIdle(player.transform.position, 0.4f, 0.35f);
                }
            }
        } else if (mode == -1) { // Become normal NPC - stop shaking, change anim, turn on dialogue.
            anim.Play("idle");
            GetComponent<PositionShaker>().enabled = false;
            GetComponent<DialogueAno2>().enabled = true;
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1);
            mode = -2;
            Bullet.DieAll(bullets);
        } else if (mode == -2) {

        }
    }

    bool suckZoneOnThis = false;
    bool suckZoneOnThisClone = false;
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.name == "SuckZone") {
            suckZoneOnThis = true;
            suckZoneOnThisClone = true;
        }

        if (isTradeNPC) {
            if (collision.gameObject == golemDustPrefab) {
                bulletPrefab.SetActive(true);
                collision.gameObject.SetActive(false);
                if (name == "tradeTeen") {
                    AudioHelper.instance.playOneShot("fant-fanfare");
                } else {
                    AudioHelper.instance.playOneShot("gateOpen");
                }
                AudioHelper.instance.playOneShot("bounce3D");
                player.ScreenShake(.1f, 0.15f, false);
                HF.SpawnDustPoof(transform.position);
                //gameObject.SetActive(false);
                GameObject.Find("2D UI").GetComponent<UIManager2D>().setTalkAvailableIconVisibility(false);
                GetComponent<SpriteRenderer>().enabled = false;
                CircleCollider2D[] ccs = GetComponents<CircleCollider2D>();
                foreach (CircleCollider2D cc in ccs) {
                    cc.enabled = false;
                }
                Destroy(gameObject, 0.05f);
                return;
            }
        }
         if (isTongueNPC) {
            if (collision.gameObject.GetComponent<Tongue>() != null && !GetComponent<PositionShaker>().enabled) {
                GetComponent<DialogueAno2>().ext_ForceInteractScriptToParse = true;
                anim.Play("shake");
                GetComponent<PositionShaker>().enabled = true;
                Color col = new Color();
                ColorUtility.TryParseHtmlString("#FFB572FF", out col);
                sr.color = col;
                HF.SendSignal(state.children);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.name == "SuckZone") {
            suckZoneOnThis = false;
            suckZoneOnThisClone = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (isRageNPC) {
            SlimeWanderer s = collision.gameObject.GetComponent<SlimeWanderer>();
            if (s != null && s.element == Element.Water) {
                hitpoints--;
                s.Break(true);
                if (hitpoints == 0) {
                    mode = -1;
                    DataLoader.instance.setDS(flagID, 1);
                } else {
                    Flicker(0.5f);
                }
            }
        }

        if (isRageSkeligum) {
            SlimeWanderer s = collision.gameObject.GetComponent<SlimeWanderer>();
            if (s != null && s.element == Element.Water) {
                s.Break(true);
                Flicker(0.5f);
                if (mode == 2) {
                    GetComponent<PositionShaker>().enabled = false;
                    mode = 1;
                    anim.Play("idle");
                    if (name == "RageNPC1") {
                        GameObject.Find("RageNPC2").GetComponent<NPCHelper>().mode = 2;
                        GameObject.Find("RageNPC2").GetComponent<PositionShaker>().enabled = true;
                        GameObject.Find("RageNPC2").GetComponent<SpriteAnimator>().Play("angry");
                    } else if (name == "RageNPC2") {
                        GameObject.Find("RageNPC3").GetComponent<NPCHelper>().mode = 2;
                        GameObject.Find("RageNPC3").GetComponent<PositionShaker>().enabled = true;
                        GameObject.Find("RageNPC3").GetComponent<SpriteAnimator>().Play("angry");
                    } else if (name == "RageNPC3") {
                        GameObject.Find("RageNPC4").GetComponent<NPCHelper>().mode = 2;
                        GameObject.Find("RageNPC4").GetComponent<PositionShaker>().enabled = true;
                        GameObject.Find("RageNPC4").GetComponent<SpriteAnimator>().Play("angry");
                    } else if (name == "RageNPC4") {
                        GameObject.Find("RageGate").GetComponent<Gate>().SendSignal();
                    }
                }
            }
        }
    }

    float t_flicker = 0;
    float tm_flicker = 0;
    float remainingFlickerTime = 0;
    public void Flicker(float t,float FPS=30) {
        tm_flicker = 1.0f / FPS;
        remainingFlickerTime  = t;
    }
}
