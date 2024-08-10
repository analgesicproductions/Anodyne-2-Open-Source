using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Anodyne {
    public class GlandilockBoss : MonoBehaviour {



        [Header("Debug")]
        public bool DEBUG_ON = false;
        [Range(0,2)]
        public int DEBUG_PHASE = 0;
        public bool SkipIntro = false;
        public bool DeathTalk = false;
        public bool HealthHack = false;
        public GameObject BossBGs;
        public GameObject DarkBGs;

        [Header("Parts")]

        public List<Transform> chains;
        List<int> chainIndices;
        public Transform fistTransform;
        public Transform bodyTransform;
        CollisionChecker bodyColChecker;
        TriggerChecker fistTrigChecker;
        public SparkReactor chainSparkReactor;
        public SpriteAnimator bodyAnim;
        PositionShaker fistPS;
        SpriteRenderer fistSR;
        Transform eyes;
        Vacuumable fistVac;
        public SpriteAnimator fistAnim;
        SparkReactor bodySparkReactor;

        TriggerChecker BodySuckTrigChecker;

        List<Bullet> bullets;

        AnoControl2D player;
        UIManager2D ui2d;
        public static bool diedOnce = false;

        public static string BossSongName = "Glandilock-Nano";
        public static string PicoBossSongName = "Glandilock-Pico";

        int phase = 0;
        int mode = 0;
        int submode = 0;

        int state_waitForPlayer = 0;
        int state_initialTalk = 1;
        int state_fistTracking = 2;
        int state_fistWindup = 3;
        int state_fistExtend = 4;
        int state_fistWait = 5;
        int state_chainBroken = 6;
        int state_sparkable = 7;
        int state_enteringPico = 8;
        int state_preDeathSuckTalk = 9;
        int state_waitToBeSucked = 10;
        int state_deathAnim = 11;

        public float[] fistTrackingTimes = new float[] { 2f, 1.5f, 1f };
        public float[] fistWindupTimes = new float[] { 0.6f, 0.35f, 0.1f };
        public float[] fistExtendSpeeds = new float[] { 3f, 5f, 7f };
        public float[] fistWaitTimes = new float[] { 1f, 0.8f, 0.6f };

        DialogueBox dbox;

        float tColor = 0;
        float tmFistFade = 2f;

        void Start() {
            if (!Registry.DEV_MODE_ON) DEBUG_ON = false;
            if (!DEBUG_ON) {
                SkipIntro = false;
                DeathTalk = false;
                HealthHack = false;
            }
            if (HealthHack) {
                chainSparkReactor.SetMaxHealth(1f);
                chainSparkReactor.explodeHealth = 1f;
            }
            eyes = GameObject.Find("gs-eyes").transform;
            if (diedOnce) SkipIntro = true;
            BodySuckTrigChecker = GameObject.Find("Body-suckTrig").GetComponent<TriggerChecker>();
            bodyColChecker = bodyTransform.GetComponent<CollisionChecker>();
            fistTrigChecker = fistTransform.GetComponent<TriggerChecker>();
            fistPS = fistTransform.GetComponent<PositionShaker>();
            fistSR = fistTransform.GetComponent<SpriteRenderer>();
            fistVac = GameObject.Find("FistVac").GetComponent<Vacuumable>();
            fistVac.gameObject.SetActive(false);
            bodySparkReactor = GameObject.Find("Body-col").GetComponent<SparkReactor>();
            HF.GetPlayer(ref player);
            HF.GetDialogueBox(ref dbox);
            Set_IsActive_OfChains(false);
            SetSpriteAlpha(0, fistSR);
            fistTransform.position = bodyTransform.position + fistOffsetVec3;
            chainIndices = new List<int>();
            for (int i =0; i < chains.Count; i++) {
                chainIndices.Add(i);
            }
            ui2d = HF.Get2DUI();

            int curPhase = DataLoader._getDS("gs-phase");
            if (curPhase != 0) {
                BossBGs.SetActive(true);
                GameObject.Find("MainBG").SetActive(false);
                bodyAnim.Play("idle");
            }
            if (curPhase == 1) {

                GameObject.Find("BossPew2").GetComponentInChildren<Pew>().enabled = false;
            } else if (curPhase == 2) {
                
            } else {
                GameObject.Find("BossPew1").GetComponentInChildren<Pew>().enabled = false;
                GameObject.Find("BossPew2").GetComponentInChildren<Pew>().enabled = false;
            }
        }


        void SetChainIndices() {
            int i = chainIndices[0];
            chainIndices.RemoveAt(0);
            chainIndices.Add(i);
        }









        public Vector2 fistOffset = new Vector2(0, -2f);
        public Vector3 fistOffsetVec3 = new Vector3(0, -2f, 0);
        Vector2 tempVec = new Vector2();
        Color tempCol = new Color();
        void SetSpriteAlpha(float alpha, SpriteRenderer sr) {
            tempCol = sr.color; tempCol.a = alpha; sr.color = tempCol;
        }

        void Set_IsActive_OfChains(bool isActive) {
            foreach (Transform g in chains) {
                g.gameObject.SetActive(isActive);
            }
        }

        void setMode(int m) {
            mode = m;
            submode = 0;
            tColor = 0;
            tTemp = 0;
            tRetract = 0;
            tmFistExtend = 0;
        }

        Vector2 tempPos = new Vector2();
        int waitTicks = 5;
        void Update() {
            bool justCollidedBodyPlayer = bodyColChecker.checkAndResetJustCollidedPlayer();

            waitTicks--;
            if (waitTicks > 0) return;

            if (mode == state_waitForPlayer) {
                if (player.InSameRoomAs(transform.position) && !player.CameraIsChangingRooms() && Vector2.Distance(bodyTransform.position,player.transform.position) < 4.5f) {
                    // GameObject.Find("BossBlocker").GetComponent<BoxCollider2D>().enabled = true;

                    diedOnce = true;
                    Vector2 endPos = GameObject.Find("SparkBarBG").GetComponent<RectTransform>().anchoredPosition;
                    endPos.y = 105f;
                    GameObject.Find("SparkBarBG").GetComponent<RectTransform>().anchoredPosition = endPos;

                    mode = state_initialTalk;
                    if (DeathTalk) {
                        DataLoader._setDS("gs-phase", 3);
                    }
                    if (DataLoader._getDS("gs-phase") == 1) {
                        SkipIntro = false;
                        phase = 1;
                    } else if (DataLoader._getDS("gs-phase") == 2) {
                        SkipIntro = false;
                        phase = 2;
                    } else if (DataLoader._getDS("gs-phase") == 3) {
                        SkipIntro = false;
                        DataLoader._setDS("end-channel", 1);
                        DataLoader._setDS("end-zera-gs", 1);
                        DataLoader._setDS("end-ring", 1);
                        phase = 3;

                    }

                    if (phase == 0) {
                        AudioHelper.instance.StopAllSongs();
                    }
                    if (SkipIntro) {

                    } else if (phase == 0) {
                        dbox.playDialogue("zera-gs-1",0,7);
                    } else if (phase == 1) {
                        dbox.playDialogue("zera-gs-mid",0);
                    } else if (phase == 2) {
                        dbox.playDialogue("zera-gs-mid",1);
                    } else if (phase == 3) {
                        //dbox.playDialogue("zera-gs-2",0);
                        bodyAnim.Play("hurt");
                        setMode(state_preDeathSuckTalk);
                    }
                }
            } else if (mode == state_initialTalk) {
                if (submode == 0) {
                    if (SkipIntro || dbox.isDialogFinished()) {

                        if (phase == 0 && !SkipIntro) {
                            bodyAnim.Play("idle");
                            eyes.GetComponent<SpriteAnimator>().Play("small");
                            AudioHelper.instance.playOneShot("gargdoor", 0.7f, 1.3f, 3);
                            bodyColChecker.GetComponent<PositionShaker>().enabled = true;
                            dbox.playDialogue("zera-gs-1", 8, 9);
                        }
                        submode = 100;
                    }
                } else if (submode == 100) {
                    if (dbox.isDialogFinished()) {
                        if (phase == 0) {
                            bodyColChecker.GetComponent<PositionShaker>().enabled = false;
                            BossBGs.SetActive(true);
                            DarkBGs.SetActive(false);
                            player.ScreenShake(0.08f, 0.5f, true);
                            GameObject.Find("fridgel").transform.position = GameObject.Find("fl_close").transform.position;
                            GameObject.Find("fridger").transform.position = GameObject.Find("fr_close").transform.position;
                            AudioHelper.instance.PlaySong(BossSongName, 0, 75, false, 0.8f);
                            AudioHelper.instance.PlaySong(PicoBossSongName, 0, 75, false, 0.8f);
                            bodyAnim.Play("idle");
                        }
                        submode = 1;

                    }

                } else if (submode == 1) {
                    tColor += Time.deltaTime;
                    if (SkipIntro) {
                        tColor = tmFistFade + 1;
                    }
                    SetSpriteAlpha(tColor / tmFistFade, fistSR);
                    if (tColor > tmFistFade) {
                        setMode(state_fistTracking);
                    }
                }
            } else if (mode == state_fistTracking) {
                SetEyeTrack();
                tempVec = player.transform.position - bodyTransform.position;
                tempVec.Normalize(); tempVec *= fistOffset.magnitude;
                float unsignedAngle = Vector2.SignedAngle(fistOffset, tempVec);
                curFistTrackingAngle = Mathf.LerpAngle(curFistTrackingAngle, unsignedAngle, Time.deltaTime * 4f);

                tempVec = fistOffset;
                tempVec.Normalize(); tempVec *= fistOffset.magnitude;
                HF.RotateVector2(ref tempVec, curFistTrackingAngle);
                tempVec.x += bodyTransform.position.x; tempVec.y += bodyTransform.position.y;
                fistTransform.position = tempVec;

                if (player.IsThereAReasonToPause()) {
                } else {
                    tTemp += Time.deltaTime;
                }
                if (tTemp > fistTrackingTimes[phase] && !fistPS.enabled) {
                    fistPS.enabled = true;
                    fistAnim.Play("tell");
                    eyes.GetComponent<SpriteAnimator>().Play("small");
                    bodyAnim.Play("windup");
                }
                if (tTemp > fistTrackingTimes[phase] + fistWindupTimes[phase]) {
                    fistPS.enabled = false;
                    eyes.GetComponent<SpriteAnimator>().Play("idle");
                    fistAnim.Play("idle");
                    setMode(state_fistExtend);
                    Set_IsActive_OfChains(true);
                    // Calculate the dest of the fist
                    fistAttackTargetPos = fistTransform.position - bodyTransform.position; fistAttackTargetPos.Normalize();
                    fistAttackTargetPos *= Mathf.Sqrt(50f) - 0.75f; // distance to corner
                    fistAttackTargetPos.x = Mathf.Clamp(fistAttackTargetPos.x, -5, 5);
                    fistAttackTargetPos.y = Mathf.Clamp(fistAttackTargetPos.y, -5, 5);
                    fistAttackTargetPos = bodyTransform.position + fistAttackTargetPos;
                    fistAttackStartPos = fistTransform.position;
                    // using speed, calc how long it should take to get there
                    tmFistExtend = (fistAttackTargetPos - fistTransform.position).magnitude / fistExtendSpeeds[phase];

                    SetChainPositions();
                }
            } else if (mode == state_fistExtend) {

                SetEyeTrack();
                tTemp += Time.deltaTime;
                tempPos = Vector3.Lerp(fistAttackStartPos, fistAttackTargetPos, tTemp / tmFistExtend);
                fistTransform.position = tempPos;
                if (tTemp >= tmFistExtend) {
                    setMode(state_fistWait);
                    AudioHelper.instance.playOneShot("fireGateBurn");
                    player.ScreenShake(0.08f, 0.35f, true);
                    bodyAnim.Play("idle");
                }

                SetChainPositions();

            } else if (mode == state_fistWait) {
                SetEyeTrack();
                tTemp += Time.deltaTime;
                SetChainPositions();

                if (tTemp > fistWaitTimes[phase]) {
                    tRetract += Time.deltaTime;
                    fistTransform.position = Vector3.Lerp(fistAttackTargetPos, fistAttackStartPos, tRetract / 0.4f);
                    if (tRetract >= 0.4f) {
                        if (chainSparkReactor.has_barFinishedFilling()) {
                            PostBarFillSTuff();
                        } else {
                            SetChainIndices();
                            Set_IsActive_OfChains(false);
                            setMode(state_fistTracking);
                        }
                    }
                }

            } else if (mode == state_chainBroken) {
                SetEyeTrack();

                foreach (Transform t in chains) {
                    if (t.GetComponent<SpriteAnimator>().isPlaying == false) {
                        t.GetComponent<SpriteRenderer>().enabled = false;
                    }
                }
                // Fire bullets

                if (bodyColChecker.checkAndResetJustCollided()) {
                    player.ScreenShake(0.11f, 1f, true);
                    fistVac.GetComponent<CircleCollider2D>().enabled = false;
                    fistVac.GetComponent<Vacuumable>().enabled = false;
                    fistVac.GetComponent<SpriteRenderer>().enabled = false;
                    AudioHelper.instance.playOneShot("fireGateBurn");
                    AudioHelper.instance.playOneShot("GlandPulseAngry");
                    fistVac.GetComponentInChildren<ParticleSystem>().Play();
                    fistVac.GetComponentInChildren<ParticleSystem>().transform.position = bodyTransform.position + (fistVac.transform.position - bodyTransform.position) * 0.5f;
                    setMode(state_sparkable);
                    bodyAnim.Play("hurt");

                }
            } else if (mode == state_sparkable) {
                t_eyespin += Time.deltaTime;
                if (t_eyespin >= tm_eyespin) t_eyespin -= tm_eyespin;
                SetEyeTrack(360f * (t_eyespin / tm_eyespin));
                
                if (bodyAnim.isPlaying == false) {
                    int curPhase = DataLoader._getDS("gs-phase");
                    if (curPhase == 1) {
                        GameObject.Find("BossPew1").GetComponentInChildren<Pew>().enabled = false;
                        GameObject.Find("BossPew1").GetComponentInChildren<Pew>().DieBullets();
                    } else if (curPhase == 2) {
                        GameObject.Find("BossPew1").GetComponentInChildren<Pew>().enabled = false;
                        GameObject.Find("BossPew2").GetComponentInChildren<Pew>().enabled = false;
                        GameObject.Find("BossPew1").GetComponentInChildren<Pew>().DieBullets();
                        GameObject.Find("BossPew2").GetComponentInChildren<Pew>().DieBullets();
                    }

                    bodyAnim.Play("open");
                    eyes.GetComponent<SpriteAnimator>().Play("black");
                    AudioHelper.instance.playOneShot("openChest");
                    GameObject.Find("gs-seed").GetComponent<SpriteRenderer>().enabled = true;
                    GameObject.Find("BossPew1").GetComponentInChildren<Pew>().enabled = false;
                    GameObject.Find("BossPew2").GetComponentInChildren<Pew>().enabled = false;
                    bodySparkReactor.picoPointLocked = false;
                    bodySparkReactor.NanopointLocked = false;
                    setMode(state_enteringPico);
                }
            } else if (mode == state_enteringPico) {

            } else if (mode == state_preDeathSuckTalk) {
                setMode(state_waitToBeSucked);

                AudioHelper.instance.StopAllSongs(false);
                AudioHelper.instance.PlaySong("DustCue", 0, 14.4f, false, 0.45f);
                eyeMaxOffset = 0.2f;
                SetEyeTrack(180f);
            } else if (mode == state_waitToBeSucked) {
                if (submode == 0) {
                    if (tSuckBody > 0) {
                        tSuckBodySound += Time.deltaTime;
                        if (tSuckBodySound >= 0.1f) {
                            float r = tSuckBody / 5f;
                            tSuckBodySound = 0;
                            AudioHelper.instance.playOneShot("BigCrystalSuckPoof1", 1f, 1.3f + 1.2f * r);
                        }
                    }
                    if (!MyInput.special) {
                        BodySuckTrigChecker.onThingToCheckFor = false;
                    }
                    if (BodySuckTrigChecker.onThingToCheckFor) {
                        tSuckBody += Time.deltaTime;
                    } else {
                        if (tSuckBody > 0) tSuckBody -= Time.deltaTime;
                    }
                    if (tSuckBody > 0) {
                        tempScale = Vector3.one;
                        tempScale *= 1 + 0.23f * (tSuckBody / 5f) + 0.07f * Random.value;
                        bodyTransform.localScale = tempScale;
                    }
                    if (tSuckBody >= 5f) {
                        submode = 1;
                        bodyTransform.GetComponent<CircleCollider2D>().enabled = false;
                        AudioHelper.instance.playOneShot("cardTear", 1, 0.85f, 2);
                        AudioHelper.instance.StopAllSongs(false);
                        player.ScreenShake(0.05f, 0.5f, true);
                        if (player.facing == AnoControl2D.Facing.DOWN || player.facing == AnoControl2D.Facing.RIGHT) {
                            suckBodyDest = bodyTransform.position + new Vector3(-2, 2, 0);
                        } else {
                            suckBodyDest = bodyTransform.position + new Vector3(2, -2, 0);
                        }
                        player.StopSuckingAnim();
                        suckBodyStart = bodyTransform.localEulerAngles;
                        suckBodyStart.z = 22.5f;
                        bodyTransform.localEulerAngles = suckBodyStart;
                        suckBodyStart = bodyTransform.position;

                            
                    }

                } else if (submode == 1) {
                    tMoveToSuckBodyDest += Time.deltaTime;
                    bodyTransform.position = Vector3.Lerp(suckBodyStart, suckBodyDest, tMoveToSuckBodyDest / 0.3f);
                    if (tMoveToSuckBodyDest >= 0.3f) {
                        dbox.playDialogue("zera-gs-2", 0);
                        player.StopSuckingAnim();
                        submode = 2;
                    }
                } else if (submode == 2) {
                    if (dbox.isDialogFinished()) {
                        submode = 3;
                        GameObject.Find("BossP1").GetComponent<ParticleSystem>().Play();
                        GameObject.Find("BossP2").GetComponent<ParticleSystem>().Play();
                        player.ScreenShake(0.08f, 8f, true);
                        AudioHelper.instance.playOneShot("blowupsond",1f,1,1);
                        AudioHelper.instance.playOneShot("glandilockDie", 0.82f, 1f, 2);
                        tTemp = 0;
                    }
                } else if (submode == 3) {
                    tTemp += Time.deltaTime;
                    if (tTemp >= 5f) {
                        submode = 4;
                        setMode(state_deathAnim);
                    } 
                }
            } else if (mode == state_deathAnim) {
                if (submode == 0) {
                    CutsceneManager.deactivatePlayer = true;
                    ui2d.StartFade(new Color(0.99f, 0.99f, 0.99f), 0, 1, 3);
                    submode = 10;
                } else if (submode == 10) {
                    tTemp += Time.deltaTime;
                    if (tTemp > 3f) {
                        dbox.playDialogue("zera-gs-2", 1);
                        submode = 11;
                        tTemp = 0;
                    }
                } else if (submode == 11) {
                    if (dbox.isDialogFinished()) {
                        submode = 12;
                        ui2d.FadeColor(Color.black, 1);
                    }
                } else if (submode == 12) {
                    tTemp += Time.deltaTime;
                    if (tTemp > 1f) {
                        diedOnce = false;
                        SparkGameController.ReturnFrom2D_SourceScene = Registry.GameScenes.NanoZera;
                        DataLoader.instance.enterScene("none", Registry.GameScenes.Wormhole, 0,0);
                        SparkGameController.SparkGameDestObjectName = "BossEntrance";
                        SparkGameController.SparkGameDestScene = Registry.GameScenes.CenterChamber;
                        Wormhole.ReturningFrom2D = true;
                        CutsceneManager.deactivatePlayer = false;
                        submode = 13;
                    }
                }
            }

            if (mode == state_fistWait || mode == state_fistExtend) {
                if (chainSparkReactor.has_barFinishedFilling()) {
                    PostBarFillSTuff();
                }
            }

            if (tFistHurtCooldown > 0) {
                tFistHurtCooldown -= Time.deltaTime;
            }
            if (fistTrigChecker.onPlayer2D && tFistHurtCooldown <= 0) {
                fistTrigChecker.onPlayer2D = false;
                tFistHurtCooldown = 1f;
                if (mode == state_fistExtend) {
                    player.Damage(2);
                    player.Bump(true, 15f);
                    player.ScreenShake(0.11f, 0.5f, true);
                } else if (mode == state_fistTracking || mode == state_fistWait || mode == state_fistWindup) {
                    player.Damage(1);
                    player.Bump(true, 5f);
                    player.ScreenShake(0.06f, 0.2f, true);
                }
            }
            if (tBodyHurtCooldown > 0) tBodyHurtCooldown -= Time.deltaTime;

            if (justCollidedBodyPlayer && tBodyHurtCooldown <= 0 && phase != 3) {
                player.Damage(1);
                player.Bump(true, 10f);
                player.ScreenShake(0.06f, 0.2f, true);
            }
        }

        private void PostBarFillSTuff() {
            setMode(state_chainBroken);
            fistTransform.gameObject.SetActive(false);
            fistVac.transform.position = fistTransform.position;
            fistVac.gameObject.SetActive(true);
            chains.Remove(chainSparkReactor.transform);
            foreach (Transform t in chains) {
                t.GetComponent<SpriteAnimator>().Play("break");
            }
        }

        Vector3 tempScale;
        Vector3 suckBodyStart = new Vector3();
        float tSuckBody;
        float tSuckBodySound;
        Vector3 suckBodyDest;
        float tMoveToSuckBodyDest;

        float curFistTrackingAngle = 0;
        float tmFistExtend = 0;
        float tFistHurtCooldown = 0;
        float tBodyHurtCooldown = 0;

        Vector3 fistAttackStartPos = new Vector3();
        Vector3 fistAttackTargetPos = new Vector3();

        float tTemp = 0;
        float tRetract = 0;
        Vector2 setChainVec = new Vector2(); 
        void SetChainPositions() {
            int chunks = chains.Count;
            Vector2 chunkVec = (fistTransform.position - fistAttackStartPos) / chunks;
            setChainVec = fistAttackStartPos;
            for (int i = 0; i < chains.Count; i++) {
                chains[i].position = setChainVec + chainIndices[i] * chunkVec;
            }
        }

        Vector2 tempEyePos = new Vector3();
        float eyeMaxOffset = 0.12f;
        float tm_eyespin = 0.25f;
            float t_eyespin = 0f;
             
        void SetEyeTrack(float forceAngle = 0) {
            tempEyePos = player.transform.position - bodyTransform.position;
            tempEyePos.Normalize(); tempEyePos  *= fistOffset.magnitude;
            float unsignedAngle = Vector2.SignedAngle(fistOffset, tempEyePos);
            tempEyePos.Set(0, -eyeMaxOffset);
            if (forceAngle != 0) {
                unsignedAngle = forceAngle;
            }
            HF.RotateVector2(ref tempEyePos, unsignedAngle);
            eyes.localPosition = tempEyePos;
        }

    }


}