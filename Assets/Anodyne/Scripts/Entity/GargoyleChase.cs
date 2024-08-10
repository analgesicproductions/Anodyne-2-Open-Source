using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Anodyne {
    public class GargoyleChase : MonoBehaviour {

        int mode = 0;
        public static int randValIdx = 0;
        int[] randValArray = new int[] { 0, 1, 2, 1, 0, 2, 0, 2, 1, 0, 2, 2, 1, 0, 2, 0, 1, 0, 2, 0, 1, 2, 1, 0, 1, 2, 0, 2, 2, 0 };

        public float chaseSpeed = 2.0f;
        SpriteAnimator anim;
        SpriteRenderer sr;
        Rigidbody2D rb;
        CircleCollider2D cc;
        public string room = "a1";
        public static bool Door_TurnOff = false;

        Transform spawnPoint;
        public static bool stopincremenitingrandomval = false;
        List<Transform> doorSiblings = new List<Transform>();
        bool isStartGarg = false;
        bool isFinalGarg = false;
        void Start() {
            Door_TurnOff = false;
            anim = GetComponentInChildren<SpriteAnimator>();
            sr = GetComponentInChildren<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
            cc = GetComponent<CircleCollider2D>();

            cc.enabled = false;

            HF.GetPlayer(ref player);
            initPos = transform.position;

            if (room == "none") {
                isStartGarg = true;
                return;
            } else if (room == "final") {
                isFinalGarg = true;
                return;
            }


            //Anodyne.Door.SetHorrorMode(2);
            if (Anodyne.Door.horrorMode != 2) {
                gameObject.SetActive(false);
            }

            foreach (Door d in GameObject.Find(room).GetComponentsInChildren<Door>()) {
                doorSiblings.Add(d.transform);
            }
            spawnPoint = GameObject.Find("p" + room).transform;

            tempcol = sr.color;
            tempcol.a = 0;
            sr.color = tempcol;

            if (stopincremenitingrandomval) {

            } else {
                stopincremenitingrandomval = true;
                //print(randValIdx);
                randValIdx++;
                if (randValIdx >= randValArray.Length) {
                    randValIdx = 0;
                }
            }
        }
        AnoControl2D player;

        bool startGargBegin = false;
        public void ActivateFromDA2() {
            startGargBegin = true;
        }
        public void pausefromDA2() {
            submode = 1;
        }

        public void unpauseFromDA2() {
            submode = 0;
        }

        public TriggerChecker ceilingSpawner;
        public float exitSpawnThreshold = 3f;
        //Death: When gargoyle catches you--maybe pause player/gargoyle animations/movement and fade screen to red? Along with some scary noise?

        public static int timesDied = 0;

        float t_startWait = 0;
        float tm_startWait = 1.3f;
        int behavior = 0;
        int BEHAVIOR_FROM_BEHIND = 0;
        int BEHAVIOR_FROM_CEILING = 1;
        int BEHAVIOR_FROM_DOOR = 2;
        int submode = 0;
        Vector3 initPos;

        void Update() {
            if (mode == 0) {
                if (isStartGarg) {
                    if (startGargBegin) {
                        behavior = 3;
                        mode = 1;
                    }
                    return;
                } else if (isFinalGarg) {
                    if (startGargBegin) {
                        behavior = 4;
                        mode = 1;
                    }
                    return;
                }

                if (player.InSameRoomAs(transform.position)) {
                    cc.enabled = false;
                    transform.position = initPos;
                    tempcol = sr.color; tempcol.a = 0; sr.color = tempcol;
                    rb.velocity = Vector2.zero;
                    t_startWait = 0;
                    submode = 0;
                    mode = 1;
                    behavior = randValArray[randValIdx];
                    if (forceBehavior != -1) {
                        behavior = forceBehavior;
                    }
                    SetVelocity();
                    UpdateAnim();
                    rb.velocity = Vector2.zero;
                }
            } else if (mode == 1) {
                if (behavior == BEHAVIOR_FROM_BEHIND) {
                    UpdateFromBehind();
                } else if (behavior == BEHAVIOR_FROM_CEILING) {
                    UpdateFromCeil();
                } else if (behavior == BEHAVIOR_FROM_DOOR) {
                    UpdateFromDoor();
                } else if (behavior == 3) {
                    cc.enabled = true;
                    SetVelocity();
                    UpdateAnim();
                } else if (behavior == 4) {
                    cc.enabled = true;
                    if (submode == 0) {
                        anim.paused = false;
                        tempcol = sr.color; tempcol.a += Time.deltaTime; if (tempcol.a >= 1) tempcol.a = 1; sr.color = tempcol;
                        SetVelocity();
                        UpdateAnim();
                    } else {
                        anim.paused = true;
                        rb.velocity = Vector2.zero;
                    }
                }

                if (DataLoader.instance.isChangingScenes) {
                    mode = 200;
                    anim.paused = true;
                    rb.velocity = Vector2.zero;
                }
                // player escaped, pause movement
            } else if (mode == 2) {
                // killed player
                player.GetComponent<Animator>().speed = 0;
                anim.paused = true;

                if (isStartGarg) {
                    GameObject.Find("chasestart_d1").SetActive(false);
                } else if (!isFinalGarg) {
                    foreach (Transform dt in doorSiblings) {
                        dt.gameObject.SetActive(false);
                    }
                }

                player.ScreenShake(0.1f, 1f);
                AudioHelper.instance.playOneShot("horrordie");
                AudioHelper.instance.StopSongByName("NanoHorror-Chase");
                AudioHelper.instance.StopSongByName("crt");
                AudioHelper.instance.StopSongByName("NanoHorror-Garg");

                ui = GameObject.Find("2D UI").GetComponent<UIManager2D>();
                player.enabled = false;
                player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                ui.StartFade(new Color(192 / 256f, 16 / 256f, 16 / 256f), 0, 1, 1);
                t_startWait = 0;
                mode = 3;
                rb.velocity = Vector2.zero;
            } else if (mode == 3) {
                rb.velocity = Vector2.zero;
                if (HF.TimerDefault(ref t_startWait, 1.2f)) {
                    mode = 4;
                }
            } else if (mode == 4 && MyInput.jpConfirm) {
                ui.FadeColor(Color.black, 1f);
                mode = 5;
            } else if (mode == 5) { 
                if (HF.TimerDefault(ref t_startWait,1f)) {
                    timesDied++;
                    DataLoader.instance.enterScene("GravelExitPos", Registry.GameScenes.NanoHorror, 0, 0);
                    DataLoader.lastFadeTime = 1.0f;
                    DataLoader.lastPixelizeTime = 1.0f;
                    mode = 6;
                }
            } else if (mode == 200) {

            }

            if (MyInput.shortcut) {
                mode = 0;
            }

            if (!isFinalGarg && player.IsThereAReasonToPause()) {
                rb.velocity = Vector2.zero;
            }
        }

        Vector3 curVel = new Vector3();
        void UpdateAnim() {
            curVel = rb.velocity;
            if (curVel.x >= 0) { 
                if (curVel.y >= 0) {
                    anim.Play("u");
                } else {
                    anim.Play("r");
                }
            } else {
                if (curVel.y >= 0) {
                    anim.Play("l");
                } else {
                    anim.Play("d");
                }
            }
        }

        UIManager2D ui;

        private void OnCollisionEnter2D(Collision2D collision) {
            if (mode >= 2 && mode <= 6) {
                print("Col while player killed. ignore");
                return;
            }
            if (!isFinalGarg && SaveManager.invincibility) {
                cc.enabled = false;
                cc.isTrigger = true;
                return;
            }
            if (collision.gameObject.name == Registry.PLAYERNAME2D) {
                if (Door_TurnOff) return;
                Door.horrorStopBecause_GargKilledPlayer = true;
                mode = 2;
                rb.velocity = Vector2.zero;
            }
        }

        [Range(1f,5f)]
        public float fadeInMultiplier = 3f;
        Color tempcol = new Color();
        Vector2 tempvel = new Vector2();
        RaycastHit2D hit = new RaycastHit2D();
        Vector2 tempnorm = new Vector2();
        Vector2 rv1 = new Vector2();
        Vector2 rv2 = new Vector2();
        void UpdateFromBehind() {
            if (submode == 0) {
                anim.paused = true;


                if (!isFinalGarg && player.IsThereAReasonToPause()) {
                    return;
                }

                if (HF.TimerDefault(ref t_startWait, tm_startWait)) {
                    transform.position = spawnPoint.position;
                    submode = 1;
                    chaseSpeed = 2.4f;
                    SetVelocity();
                    UpdateAnim();

                    AudioHelper.instance.playOneShot("gargdoor",1,1f+0.15f*Random.value);
                }
            } else if (submode == 1) {

                tempcol = sr.color;
                tempcol.a += Time.deltaTime * fadeInMultiplier;
                if (tempcol.a >= 1) tempcol.a = 1;
                sr.color = tempcol;
                if (tempcol.a >= 1) {
                    submode = 2;
                    anim.paused = false;
                    cc.enabled = true;
                }
            } else if (submode == 2) {
                SetVelocity(true);
                UpdateAnim();
            } else if (submode == 3) {

            }
        }

        private void SetVelocity(bool gradual = false) {
            tempvel = rb.velocity;
            tempvel = player.transform.position - transform.position;
            // only holetilemap
            hit = Physics2D.CircleCast(transform.position, cc.radius, tempvel, 0.1f, 1 << 15);
            if (hit.collider != null) {
                tempnorm = hit.normal;
                rv1 = tempnorm;
                rv2 = tempnorm;
                HF.RotateVector2(ref rv1, 90f);
                HF.RotateVector2(ref rv2, -90f);
                if (Vector2.Distance(hit.point + rv1, player.transform.position) <= Vector2.Distance(hit.point + rv2, player.transform.position)) {
                    tempvel = rv1;
                } else {
                    tempvel = rv2;
                }
                gradual = false;
            }

            tempvel.Normalize();
            float diff = Mathf.Lerp(1, 0.5f, timesDied / 10f);
            tempvel *= chaseSpeed*diff;
            if (gradual) {
                tempvel = Vector2.Lerp(rb.velocity, tempvel, Time.deltaTime * 2f);
            }
            rb.velocity = tempvel;
        }

        Vector3 tempPos = new Vector3();
        float distofall = 0;
        void UpdateFromCeil() {
            if (submode == 0) {
                if (ceilingSpawner.onPlayer2D) {
                    // 6, 5
                    tempPos.Set(HF.GetRoomX(transform.position.x) * 12f + 6f, HF.GetRoomY(transform.position.y) * 12f + 5f,0);

                    transform.position = tempPos;
                    SetVelocity();
                    UpdateAnim();
                    rb.velocity = Vector2.zero;

                    tempPos.y += 8f;
                    transform.position = tempPos;
                    submode = 1;
                    distofall = 8f;
                    tempcol = sr.color;
                    tempcol.a = 1;
                    sr.color = tempcol;
                    AudioHelper.instance.playOneShot("gargfall", 0.9f + 0.2f * Random.value);
                }
            } else if (submode == 1) {


                distofall -= Time.deltaTime * 9f;
                tempPos = transform.position;
                tempPos.y -= Time.deltaTime * 9f;
                transform.position = tempPos;
                if (distofall <= 0) {
                    submode = 2;
                    AudioHelper.instance.playOneShot("gargland");
                    player.ScreenShake(0.1f, 0.2f);
                    cc.enabled = true;
                }
            } else if (submode == 2) {
                SetVelocity();
                UpdateAnim();
            }
        }
        public int forceBehavior = -1;
        void UpdateFromDoor() {
            if (submode == 0) {
                foreach (Transform t in doorSiblings) {
                    if (Vector2.Distance(t.position,player.transform.position) < exitSpawnThreshold) {
                        submode = 1;
                        chaseSpeed = 2.4f;
                        transform.position = t.position;

                        SetVelocity();
                        UpdateAnim();

                        AudioHelper.instance.playOneShot("gargdoor", 1f + 0.15f * Random.value);
                        rb.velocity = Vector2.zero;
                    }
                }
            } else if (submode == 1) {
                SetVelocity();
                UpdateAnim();
                tempcol = sr.color;
                tempcol.a += Time.deltaTime * fadeInMultiplier;
                if (tempcol.a >= 1) tempcol.a = 1;
                sr.color = tempcol;
                if (tempcol.a >= 1) {
                    cc.enabled = true;
                    submode = 2;
                }
            } else if (submode == 2) {
                SetVelocity(true);

                UpdateAnim();
            }
        }
    }

}