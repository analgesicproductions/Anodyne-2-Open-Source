using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Anodyne;
using UnityEngine.UI;

public class WrestlingManager : MonoBehaviour {
    #region declarations
    // State
    bool tradingBlows = false;
    int matchNumber;
    WrestleMode mode;
    WrestleMode cachedMode;
    float hypePoints = 0;
    int hypePhase = 0;
    int dremPullStunMode = 0;
    int movementMode = 0;
    Vacuumable novaAttackVac;

    // Misc
    public GameObject chairPrefab;
    GameObject chair;
    int chairMode = 0;
    Vector3 chairStartPos;
    Vacuumable[] boxList;


    // Misc. UI
    public TMP_Text sidetext;
    public Image sidetextBG;
    DialogueBox dbox;
    Vector2 tempVec2 = new Vector2();

    // Grapple stuff
    public Image grapplePromptBG;
    public TMP_Text grapplePromptText;
    public Transform grappleTransform;
    public Transform grappleHiddenPos;
    public Transform grappleActivePos;
    ParticleSystem grappleParticles;
    Vector3 grappleCachedCamPos;
    float t_grappleCharFade;
    float t_grappleCamMove = 0;
    SpriteRenderer grappleSR;
    SpriteRenderer grappleImpactSR;
    SpriteAnimator grappleAnim;
    SpriteAnimator grappleImpactAnim;
    Color tempColor = new Color();
   // float t_colorFade = 0;
   // float tm_colorFade = 0.4f;
    int grappleMode = 0;
    float t_grappleAct = 0;
    float tm_grappleAct = 0;
    string nextGrappleAnim;
    string[] grappleanims;

    // Nova stuff
    AnoControl2D player;
    public ParticleSystem ps_novaStars;
    SpriteRenderer playerSR;

    // Drem stuff
    public ParticleSystem ps_dremStars;
    public Transform axeTransform;
    SpriteAnimator axeAnim;
    public Transform dremTransform;
    SpriteAnimator dremAnim;
    TriggerChecker dremTrigger;
    SpriteRenderer dremSR;
    PositionShaker dremPS;
    float t_flicker = 0;
    float t_flickerInterval = 0;
    float tm_flicker = 0.6f;
    float t_axeScale = 0;
    float tm_axeScale = 0.8f; // set in code
    float t_dremWaitAfterHit = 0;
    float t_dremWait = 0;
    int dremFightPosIndex = 0;
    public Transform[] dremFightPositions;
    Vector3 dremInitMovePos;
    Vector3 axeAttackInitPos = new Vector3();
    Vector3 dremNextPos;
    float t_dremMove = 0;
    float tm_dremMove = 0.5f; // Set in code
    string dremAnimNameBeforePullstun = "";
    int dremAttackMode = 0;
    Vector3 axeDestPos = new Vector3();

    // Announcer
    public Transform announcerTransform;
    float t_announcerEnter = 0;
    SpriteAnimator announcerAnim;
    public Transform announcerHiddenPos;
    public Transform announcerVisiblePos;

    // Hype Meter
    public Transform hypeMeterCoverTransform;
    public SpriteAnimator hypeMeterOverlayAnim;
    //float hypeStartY = 0;
    float hypeMilestone1Y = 26;
    float hypeMilestone2Y = 52;
    float hypeMilestone3Y = 78;
    float hypeEndY = 92;

    // Temps
  //  Vector3 tempVel = new Vector3();
    Vector3 tempPos = new Vector3();
    Vector3 tempScale = new Vector3();
    #endregion

    void Start () {
        grappleAnim = grappleTransform.GetComponent<SpriteAnimator>();
        grappleImpactAnim = grappleTransform.Find("GrappleImpact").GetComponent<SpriteAnimator>();
        grappleSR = grappleAnim.GetComponent<SpriteRenderer>();
        grappleImpactSR = grappleImpactAnim.GetComponent<SpriteRenderer>();
        grappleParticles = GameObject.Find("grapplep").GetComponent<ParticleSystem>();


        centerTrans = GameObject.Find("TheCenter").transform;
        centerAnim = centerTrans.GetComponent<SpriteAnimator>();

        HF.GetPlayer(ref player);
        playerSR = player.GetComponent<SpriteRenderer>();
        GameObject.Find("PlayerWrestlingStuff").transform.parent = player.transform;
        GameObject.Find("PlayerWrestlingStuff").transform.localPosition = new Vector3(0, 0, 0);
        chairStartPos = GameObject.Find("ChairStartPos").transform.position;

        dremTrigger = dremTransform.GetComponent<TriggerChecker>();
        dremAnim = dremTransform.GetComponent<SpriteAnimator>();
        dremSR = dremTransform.GetComponent<SpriteRenderer>();
        dremPS = dremTransform.GetComponent<PositionShaker>();
        axeAnim = axeTransform.GetComponent<SpriteAnimator>();

        mode = WrestleMode.IntroIdling;

        HF.GetDialogueBox(ref dbox);

        GameObject BlocksParent = GameObject.Find("Blocks");
        boxList = new Vacuumable[BlocksParent.transform.childCount];
        for (int i = 0; i < boxList.Length; i++) {
            boxList[i] = BlocksParent.transform.GetChild(i).GetComponent<Vacuumable>();
        }
        announcerAnim = announcerTransform.GetComponent<SpriteAnimator>();

        HideThings();

        
        /*
        StartMatch(3);
        player.wrestleOn = true;
        hypePhase = 3;
        hypePoints = hypeMilestone3Y;
        mode = WrestleMode.Jumping;
        //outroMode = 16;
        //UpdateSideText("wrestle-3", 2);
        tradingBlows = false;
        */
    }

    void HideThings() {
        GameObject.Find("HealthBarMask").GetComponent<Image>().enabled = false;
        GameObject.Find("HealthBar_Empty").GetComponent<Image>().enabled = false;
        GameObject.Find("HealthBar_Red").GetComponent<Image>().enabled = false;
        GameObject.Find("HealthBar_Gold").GetComponent<Image>().enabled = false;
        GameObject.Find("DustMask").GetComponent<Image>().enabled = false;
        GameObject.Find("DustBar_Full").GetComponent<Image>().enabled = false;
        GameObject.Find("DustBar_Bottom").GetComponent<Image>().enabled = false;
        GameObject.Find("DustDigitOnes").GetComponent<Image>().enabled = false;
        GameObject.Find("DustDigitTens").GetComponent<Image>().enabled = false;
    }

    public enum WrestleMode { IntroIdling, DremAttacking, NovaAttacking, Countering, BigAxeCountering, DremSuckable, Jumping, Grappling, OutroIdling, Dialogue };

    bool changeToAttackingRequested = false;

    // Match 2: Trade, grapple, trade, suck drem
    // match 3: Trade, grapple, jumping, center
    void MaybeStateSwitchAfterTradingBlows() {
        if (matchNumber == 2) {
            if (hypePhase == 1) {
                TurnOffSideText();
                movementMode = 0;
                dremFightPosIndex = 0;
                mode = WrestleMode.Dialogue;
                cachedMode = WrestleMode.Grappling;
            } else if (hypePhase == 3) {
                UpdateSideText("wrestle-2", 1);
                mode = WrestleMode.NovaAttacking;
                tradingBlows = false;
            }
        } else if (matchNumber == 3) {
            if (hypePhase == 1) {
                TurnOffSideText();
                tradingBlows = false;
                mode = WrestleMode.Dialogue;
                cachedMode = WrestleMode.Grappling;
            }
        }
    }

    int dMode = 0;
    void UpdateDialogueMode () {
        if (dMode == 0) {
            if (matchNumber == 1) {
                AudioHelper.instance.playOneShot("wrestlecheer");
                if (hypePhase == 1) {
                    dbox.playDialogue("wrestle-1", 1);
                } else if (hypePhase == 4) {
                    dbox.playDialogue("wrestle-1", 4);
                }
            } else if (matchNumber == 2) {
                if (hypePhase == 1) {
                    AudioHelper.instance.playOneShot("wrestlecheer");
                    dbox.playDialogue("wrestle-2", 0);
                } else if (hypePhase == 4) {
                    AudioHelper.instance.playOneShot("wrestlecheer");
                    dbox.playDialogue("wrestle-2", 3);
                }
            } else if (matchNumber == 3) {
                if (hypePhase == 1) {
                    AudioHelper.instance.playOneShot("wrestlecheer");
                    dbox.playDialogue("wrestle-2", 0);
                } else if (hypePhase == 3) {
                    AudioHelper.instance.playOneShot("wrestlecheer");
                    dbox.playDialogue("wrestle-center", 1);
                }
            }
            dMode = 1;
            t_announcerEnter = 0;
            announcerAnim.Play("talk");
            announcerAnim.GetComponent<PositionShaker>().enabled = true;
        } else if (dMode == 1) {
            t_announcerEnter += Time.deltaTime;
            tempPos = announcerTransform.position;
            tempPos = Vector3.Lerp(announcerHiddenPos.position, announcerVisiblePos.position, Mathf.SmoothStep(0,1,t_announcerEnter / 1f));
            announcerTransform.position = tempPos;
            if (dbox.isDialogFinished() && t_announcerEnter >= 1) {
                dMode = 2;
                t_announcerEnter = 0;
                announcerAnim.Play("idle");
                announcerAnim.GetComponent<PositionShaker>().enabled = false;
            }
        } else if (dMode == 2) {
            t_announcerEnter += Time.deltaTime;
            tempPos = announcerTransform.position;
            tempPos = Vector3.Lerp(announcerVisiblePos.position, announcerHiddenPos.position, Mathf.SmoothStep(0, 1, t_announcerEnter / 1f));
            announcerTransform.position = tempPos;
            if (t_announcerEnter >= 1) {
                t_announcerEnter = 0;
                if (matchNumber == 1) {
                    if (hypePhase == 4) {
                        cachedMode = WrestleMode.OutroIdling;
                    } else if (cachedMode == WrestleMode.DremAttacking) {
                        UpdateSideText("wrestle-1", 2);
                    } 
                } else if (matchNumber == 2) {
                    if (hypePhase == 4) {
                        cachedMode = WrestleMode.OutroIdling;
                    }
                } else if (matchNumber == 3) {
                    if (hypePhase == 3) {
                        cachedMode = WrestleMode.OutroIdling;
                    }
                }
                mode = cachedMode;
                dMode = 0;
            }
        }
    }

    int outroMode = 0;
    float t_outro = 0;
    Transform orbTrans;
    Vector3 orbPos;
    Vector3 tempOrbScale = new Vector3();
    float t_orb = 0;

    void UpdateOutroIdle() {
        if (matchNumber == 1) {
            if (outroMode == 0) {
                CutsceneManager.deactivatePlayer = true;
                AudioHelper.instance.FadeSong("Wrestling", 1, 0);
                dbox.playDialogue("wrestle-1-end",0,2);
                outroMode = 1;
            } else if (outroMode == 1) {
                if (dbox.isDialogFinished()) {
                    player.GetComponent<Animator>().Play("yummy_W");
                    AudioHelper.instance.playOneShot("yummy_nova");
                    dbox.playDialogue("wrestle-1-end", 3);
                    player.GetComponent<Animator>().speed = 1;
                    player.GetComponent<Animator>().enabled = true;
                    outroMode = 2;
                }
            } else if (outroMode == 2 && dbox.isDialogFinished()) {
                dbox.playDialogue("wrestle-1-end", 4,6);
                outroMode = 3;
            } else if (outroMode == 3 && dbox.isDialogFinished()) {

                CutsceneManager.deactivatePlayer = false;
                DataLoader.instance.enterScene("FarmEntrance", Registry.GameScenes.NanoDustbound);
                outroMode = 4;
            }
        } else if (matchNumber == 2) {
            if (outroMode == 0) {
                CutsceneManager.deactivatePlayer = true;
                AudioHelper.instance.FadeSong("Wrestling", 1, 0);
                dbox.playDialogue("wrestle-2-end", 0);
                outroMode = 1;
            } else if (outroMode == 1) {
                if (dbox.isDialogFinished()) {
                    player.GetComponent<Animator>().Play("monologue_W");
                    dbox.playDialogue("wrestle-2-end", 1,3);
                    player.GetComponent<Animator>().enabled = true;
                    player.GetComponent<Animator>().speed = 1;
                    outroMode = 2;
                }
            } else if (outroMode == 2 && dbox.isDialogFinished()) {
                player.GetComponent<Animator>().Play("walk_d_W");
                dbox.playDialogue("wrestle-2-end",4, 5);
                player.GetComponent<Animator>().enabled = true;
                player.GetComponent<Animator>().speed = 0;
                outroMode = 3;
            } else if (outroMode == 3 && dbox.isDialogFinished()) {
                CutsceneManager.deactivatePlayer = false;
                DataLoader.instance.enterScene("BirthEntrance", Registry.GameScenes.NanoDustbound);
                outroMode = 4;
            }
        } else if (matchNumber == 3) {
            if (outroMode == 0) {
                CutsceneManager.deactivatePlayer = true;
                // Center falls down
                centerAnim.Play("laugh");

                orbTrans = GameObject.Find("Orb").transform;
                orbPos = GameObject.Find("OrbPos").transform.position;

                //ui = GameObject.Find("2D UI").GetComponent<UIManager2D>();
                //outroMode = 13;
                //return;

                dbox.playDialogue("wrestle-center", 2, 7);
                outroMode = 1;
            } else if (outroMode == 1 && dbox.isDialogFinished()) {
                ui = GameObject.Find("2D UI").GetComponent<UIManager2D>();
                ui.AngryPulse();
                t_outro = 0;
                outroMode = 2;
                // Wait for angry pulse to end
            } else if (outroMode == 2) {
                if (HF.TimerDefault(ref t_outro, 2)) {
                    dbox.playDialogue("wrestle-center-2");
                    outroMode = 3;
                }
            } else if (outroMode == 3 && dbox.isDialogFinished()) {
                outroMode = 4;
                orbTrans.position = orbPos;
                tempOrbScale.Set(0.1f, 0.1f, 0.1f);
                orbTrans.localScale = tempOrbScale;

            } else if (outroMode == 4) {
                t_orb += Time.deltaTime;
                float s = Mathf.Lerp(0.1f, 1, t_orb / 2f);
                tempOrbScale.Set(s, s, s);
                orbTrans.localScale = tempOrbScale;
                if (t_orb > 3) {
                    t_orb = 0;
                    outroMode = 5;
                    axeDestPos = dremTransform.position;
                }
            } else if (outroMode == 5) {
                t_orb += Time.deltaTime;
                tempOrbScale = Vector3.Lerp(orbPos, axeDestPos, t_orb / 0.4f);
                orbTrans.position = tempOrbScale;
                if (t_orb > 0.4f) {
                    orbTrans.GetComponent<SpriteAnimator>().Play("break");
                    orbTrans.GetComponentInChildren<ParticleSystem>().Play();
                    player.ScreenShake(0.08f, 0.5f, true);
                    AudioHelper.instance.playOneShot("fireGateBurn");
                    outroMode = 6;
                    dremAnim.Play("daze");
                    orbTrans.GetComponent<SpriteRenderer>().enabled = false;
                }
            } else if (outroMode == 6) {
                // stuff with center attacking...
                outroMode = 10;
                AudioHelper.instance.FadeSong("WrestlingCenter", 2f, 0, true);
                dbox.playDialogue("wrestle-center-3");
            } else if (outroMode == 10 && dbox.isDialogFinished()) {
                AudioHelper.instance.playSFX("fallDown");
                ui.StartFade(0, 1, 0.5f);
                AudioHelper.instance.PlaySong("wrestleambience_scary", 0, 6.5f,false,0.5f);
                outroMode = 11;
                // start 
            } else if (outroMode == 11) {
                if (HF.TimerDefault(ref t_outro, 2f)) {
                    centerAnim.Play("idle");
                    // Start scary dreams
                    outroMode = 12;
                    dbox.playDialogue("wrestle-vision");
                    player.GetComponent<Animator>().enabled = true;
                    player.GetComponent<Animator>().Play("dead_W");
                    player.GetComponent<Animator>().speed = 1;
                }
            } else if (outroMode == 12 && dbox.isDialogFinished()) {
                // if dreams done
                AudioHelper.instance.FadeSong("wrestleambience_scary",1,0,true);
                AudioHelper.instance.playSFX("GlandPulseFade");
                ui.StartFade(1, 0, 2f);
                outroMode = 13;
            }  else if (outroMode == 13) {
                if (HF.TimerDefault(ref t_outro,2)) {
                    dbox.playDialogue("wrestle-3-end");
                    outroMode = 14;
                }
            } else if (outroMode == 14 && dbox.isDialogFinished() && HF.TimerDefault(ref t_outro,1)) {
                player.GetComponent<Animator>().Play("walk_d_W");
                player.GetComponent<CircleCollider2D>().enabled = false;
                outroMode = 15;
            } else if (outroMode == 15) {
                tempPos = player.transform.position;
                tempPos.y -= Time.deltaTime * 8f;
                player.transform.position = tempPos;
                if (HF.TimerDefault(ref t_outro, 2)) {
                    ui.StartFade(0, 1, 1);
                    outroMode = 16;
                }
            } else if (outroMode == 16 && HF.TimerDefault(ref t_outro,1)) {
                CutsceneManager.deactivatePlayer = false;
                DataLoader.instance.enterScene("BlowupEntrance", Registry.GameScenes.NanoDustbound, 0, 1);
                DataLoader.lastFadeTime = 1.5f;
                DataLoader.lastPixelizeTime = 1.5f;
                outroMode = 17;
            }
        }
    }
    UIManager2D ui;
    void UpdateDremAttacking() {
        // Check for state exit condition
        if (dremAttackMode == 0) {
            if (changeToAttackingRequested) {
                changeToAttackingRequested = false;
                mode = WrestleMode.NovaAttacking;
            }
            if (matchNumber == 1 && hypePhase == 2) {
                TurnOffSideText();
                mode = WrestleMode.Grappling;
            }
            MaybeStateSwitchAfterTradingBlows();
            if (mode != WrestleMode.DremAttacking) {
                dremAnim.Play("idle_l");

                player.wrestleDodging = false;
                return;
            }
        }

        // Play charge anim, move axe in position, set axe scale speed based on match.
        if (dremAttackMode == 0) {
            player.wrestleDodging = true;
            player.wrestleAxe = axeTransform;
            dremAttackMode = 1;

            if (player.transform.position.x < dremTransform.position.x) {
                dremAnim.Play("charge_l");
                axeTransform.position = GameObject.Find("axepos_r").transform.position;
            } else {
                dremAnim.Play("charge_r");
                axeTransform.position = GameObject.Find("axepos_l").transform.position;
            }
            tempScale = new Vector3(0.1f, 0.1f, 0.1f);
            axeTransform.localScale = tempScale;
            t_dremWait = 0;
            t_axeScale = 0;
            tm_axeScale = 0.8f;
            if (matchNumber == 2) tm_axeScale = 0.6f;
            if (matchNumber == 3) {
                tm_axeScale = 0.4f;
                if (hypePhase == 3) {
                    player.wrestleCountering = true;
                    tm_axeScale = 2f;
                }
            }
        
        // Scale up the axe.
        } else if (dremAttackMode == 1) {
            t_axeScale += Time.deltaTime;
            float scaleVal = Mathf.Lerp(0.1f, 1, t_axeScale / tm_axeScale);
            if (hypePhase == 3 && matchNumber == 3) {
                scaleVal *= 3f;
            }
            tempScale.Set(scaleVal, scaleVal, scaleVal);
            axeTransform.localScale = tempScale;

            if (t_axeScale >= tm_axeScale) {
                t_axeScale = 0;
                t_dremWait = 0;
                dremAttackMode = 2;
            }

        // Pause a bit, then play attack anim and set axe speed based on match number. Set axe start/end pos.
        } else if (dremAttackMode == 2) {
            if (HF.TimerDefault(ref t_dremWait, 0.6f)) {

                t_dremWait = 0;
                dremAttackMode = 3;
                if (dremAnim.CurrentAnimationName() == "charge_l") {
                    dremAnim.Play("attack_l");
                } else {
                    dremAnim.Play("attack_r");
                }

                tm_axeScale = 0.55f;
                if (matchNumber == 2) tm_axeScale = 0.49f;
                if (matchNumber == 3) tm_axeScale = 0.4f;
                axeDestPos = player.transform.position;
                axeAttackInitPos = axeTransform.position;
                axeAttackInitPos.z = player.transform.position.z;
            }

        // Move axe towards dest pos.
        } else if (dremAttackMode == 3) {
            t_axeScale += Time.deltaTime;
            tempPos = Vector3.Lerp(axeAttackInitPos, axeDestPos, t_axeScale / tm_axeScale);
            axeTransform.position = tempPos;
            if (Vector3.Distance(axeTransform.position, player.transform.position) < 0.5f) {
                AudioHelper.instance.playOneShot("playerHurt");
                player.Flicker(0.5f);
                if (matchNumber != 1) ReduceHypePoints(3);
                if (matchNumber == 1) ReduceHypePoints(6);
                axeAnim.Play("break");
                dremAttackMode = 4;
            } else if (Vector3.Distance(axeTransform.position, player.transform.position) < 2f) {
                if (MyInput.jpSpecial) {
                    AudioHelper.instance.playOneShot("crystalHitPlayer");
                    ps_novaStars.Play();
                    // last phase: counter many times
                    if (hypePhase == 3 && matchNumber == 3) {
                        dremAttackMode = 10;
                        axeAttackInitPos = axeTransform.position;
                        axeDestPos = dremTransform.position;
                        t_axeScale = 0;
                        tm_axeScale = 0.8f;

                    } else {
                        AddHypePoints(6);
                        axeAnim.Play("break");
                        dremAttackMode = 4;
                        if (tradingBlows) {
                            changeToAttackingRequested = true;
                        }
                    }
                }
            } 
            if (t_axeScale >= tm_axeScale) {
                t_axeScale = 0;
                axeAnim.Play("break");
                dremAttackMode = 4;
            }
        } else if (dremAttackMode == 4) {
            if (axeAnim.isPlaying == false) {
                axeTransform.position = GameObject.Find("AxeInitPos").transform.position;
                axeAnim.Play("spin");
                dremAttackMode = 0;
            }

            // countering: Nova towards Drem
        } else if (dremAttackMode == 10) {
            t_axeScale += Time.deltaTime;
            tempPos = Vector3.Lerp(axeAttackInitPos, axeDestPos, t_axeScale / tm_axeScale);
            axeTransform.position = tempPos;
            if (Vector3.Distance(axeTransform.position, dremTransform.position) < 1f) {
                t_axeScale = 0;
                dremAnim.Play("spin");
                AudioHelper.instance.playOneShot("sparkBarHit");
                dremAttackMode = 11;
                axeAttackInitPos = axeTransform.position;
                axeDestPos = player.transform.position;
                axeDestPos.x += (-0.5f + 1 * Random.value);
            }
            // countering: drem to nova, nova must deflect
        } else if (dremAttackMode == 11) {
            t_axeScale += Time.deltaTime;
            tempPos = Vector3.Lerp(axeAttackInitPos, axeDestPos, t_axeScale / tm_axeScale);
            axeTransform.position = tempPos;
            if (Vector3.Distance(axeTransform.position, player.transform.position) < 2.2f && MyInput.jpSpecial) {
                t_axeScale = 0;
                AddHypePoints(3);
                ps_novaStars.Play();
                AudioHelper.instance.playOneShot("crystalHitPlayer");
                dremAnim.Play("idle_r");
                dremAttackMode = 10;
                axeAttackInitPos = axeTransform.position;
                axeDestPos = dremTransform.position;
                if (hypePoints > hypeEndY - 6) {
                    axeAnim.Play("break");
                    dremAttackMode = 12;
                    player.wrestleDodging = false;
                }
            } else if (Vector3.Distance(axeTransform.position, player.transform.position) < 0.5f || t_axeScale > tm_axeScale) {
                axeAnim.Play("break");
                ReduceHypePoints(3);
                AudioHelper.instance.playOneShot("hurtPlayer");
                dremAttackMode = 4;
                tm_axeScale = 2f;
            }
        // Set up stuff for Center appearance
        } else if (dremAttackMode == 12) {
            if (axeAnim.isPlaying == false) {
                axeTransform.position = GameObject.Find("AxeInitPos").transform.position;
                dremAttackMode = 13;
                dbox.playDialogue("wrestle-center", 0);

                grappleCachedCamPos = Camera.main.transform.position;
                tempPos = GameObject.Find("CamAnnounceStart").transform.position;
                tempPos.z = Camera.main.transform.position.z;
                t_axeScale = 0;
                CutsceneManager.deactivatePlayer = true;
                TurnOffSideText();
                dremAnim.Play("idle_l");
                player.StopSuckingAnim();
                player.GetComponent<Animator>().Play("idle_u_W");
                player.GetComponent<Animator>().speed = 1;
            }
        // Lerp camera
        } else if (dremAttackMode == 13 && dbox.isDialogFinished()) {
            t_axeScale += Time.deltaTime;
            Camera.main.transform.position = Vector3.Lerp(grappleCachedCamPos, tempPos, Mathf.SmoothStep(0, 1, t_axeScale / 0.5f));
            if (t_axeScale > 1.2f) {
                dremAttackMode = 14;
                t_axeScale = 0;
                dremInitMovePos = centerTrans.position;
                dremNextPos = dremFightPositions[2].position;
                AudioHelper.instance.playSFX("fallDown");

                AudioHelper.instance.FadeSong("Wrestling", 1f, 0, true);
                AudioHelper.instance.PlaySong("WrestlingCenter", 6.072f, 49.272f);
            }
        // Move in Center, then head to dialogue and eevntually outro state
        } else if (dremAttackMode == 14) {
            t_axeScale += Time.deltaTime;
            tempPos = Vector3.Lerp(dremInitMovePos, dremNextPos, t_axeScale / 1f);
            centerTrans.position = tempPos;
            if (t_axeScale > 1f) {

                AudioHelper.instance.playSFX("gateclose");
                centerAnim.Play("land");
                centerAnim.ScheduleFollowUp("idle");
                mode = WrestleMode.Dialogue;
                player.ScreenShake(0.08f, 0.5f, true);
            }

        }
    }
    Transform centerTrans;
    SpriteAnimator centerAnim;

    void UpdateChair() {
        if (chairMode == 0) {
            int usedblocks = 0;
            for (int i = 0; i < boxList.Length; i++) {
                if (boxList[i].isBroken()) {
                    usedblocks++;
                }
            }
            if (usedblocks == boxList.Length) {
                chairMode = 1;
                if (chair == null) {
                    chair = Instantiate(chairPrefab);
                }
                chairRB = chair.GetComponent<Rigidbody2D>();
                chairVac = chair.GetComponent<Vacuumable>();
                chair.transform.position = chairStartPos;
                chairRB.velocity = Vector2.right * 15f;
            }
        } else if (chairMode == 1) {
            if (chairRB.velocity.magnitude < 1) {
                chair.GetComponent<BoxCollider2D>().enabled = true;
                chairMode = 2;
            }
        } else if (chairMode == 2) {
            if (chairVac.isBroken()) {
                chairVac.Respawn();
                chair.GetComponent<BoxCollider2D>().enabled = false;
                chairMode = 0;
            }
        }
    }

    Rigidbody2D chairRB;
    Vacuumable chairVac;
    bool changeToDodgingRequested = false;
    void UpdateNovaAttacking() {

        if (chairMode != 1 && dremPullStunMode == 0) {
            if (changeToDodgingRequested) {
                changeToDodgingRequested = false;
                mode = WrestleMode.DremAttacking;
                return;
            }

            if (matchNumber == 1 && hypePhase == 1) {
                mode = WrestleMode.Dialogue;
                cachedMode = WrestleMode.DremAttacking;
                return;
            }

            MaybeStateSwitchAfterTradingBlows();

        }

        UpdateChair();
        
        // If drem's pullstun mode is not in 'idle', then don't move drem or change his anims.
        if (dremPullStunMode != 0) {
        } else if (movementMode == 0) {
            movementMode = 1;
        } else if (movementMode == 1) {
            dremFightPosIndex++;
            if (dremFightPosIndex == dremFightPositions.Length) dremFightPosIndex = 0;
            dremNextPos = dremFightPositions[dremFightPosIndex].position;
            dremInitMovePos = dremTransform.position;
            movementMode = 2;
            t_dremMove = 0;
            tm_dremMove = 1f;
            if (matchNumber == 2 && hypePhase == 2) tm_dremMove = 0.76f;
            if (matchNumber == 3 && hypePhase == 0) tm_dremMove = 0.7f;
            if (dremNextPos.x < dremTransform.position.x) {
                dremAnim.Play("walk_l");
            } else {
                dremAnim.Play("walk_r");
            }
        } else if (movementMode == 2) {

            t_dremMove += Time.deltaTime;
            tempPos = Vector3.Lerp(dremInitMovePos, dremNextPos, t_dremMove / tm_dremMove);
            dremTransform.position = tempPos;
            if (t_dremMove > tm_dremMove) {
                if (dremAnim.CurrentAnimationName() == "walk_l") {
                    dremAnim.Play("idle_l");
                } else {
                    dremAnim.Play("idle_r");
                }
                movementMode = 3;
                t_dremWait = 0;
            }
        } else if (movementMode == 3) {
            if (HF.TimerDefault(ref t_dremWait, tm_dremMove * 1.5f)) {
                movementMode = 1;
            }
        }

        // Wait for trigger to detect attack so the code can check the shot block.
        // The vacuumable release-to-break happens in the Vacuumable script
        if (dremPullStunMode == 0) {
            if (dremTrigger.JustTriggered()) {
                if (dremTrigger.vac != null) {
                    dremPullStunMode = 1;
                    novaAttackVac = dremTrigger.vac;
                }
            }
        }
        if (dremPullStunMode == 1) {
            if (novaAttackVac.isBreaking()) {
                if (Vector3.Distance(dremTransform.position, novaAttackVac.transform.position) < 2f) {
                    // Block successfully pulled
                    if (!MyInput.special) {
                        dremAnimNameBeforePullstun = dremAnim.CurrentAnimationName();
                        ps_dremStars.Play();
                        dremPS.enabled = true;
                        AudioHelper.instance.playOneShot("crystalHitPlayer");
                        if (player.transform.position.x < dremTransform.position.x) {
                            dremAnim.Play("hit_l");
                        } else if (player.transform.position.x >= dremTransform.position.x) {
                            dremAnim.Play("hit_r");
                        }
                        t_dremWaitAfterHit = 0;

                        if (matchNumber == 2 && hypePhase == 3) {
                            dremPullStunMode = 3;
                            dremAnim.Play("daze");
                        } else {
                            dremPullStunMode = 2;
                            if (tradingBlows) {
                                changeToDodgingRequested = true;
                            }
                        }

                        AddHypePoints(6);
                        // Block explodes but not pulled.
                    } else {
                        if (matchNumber != 1) ReduceHypePoints(3);
                        if (matchNumber == 1) ReduceHypePoints(6);
                        t_flicker = tm_flicker;
                        AudioHelper.instance.playOneShot("playerHurt");
                        dremPullStunMode = 0;
                    }
                    // Block explodes too far from drem
                } else {
                    dremPullStunMode = 0;
                }
                novaAttackVac = null;
            }
        } else if (dremPullStunMode == 2) {
            t_dremWaitAfterHit += Time.deltaTime;
            if (t_dremWaitAfterHit > 1) {
                dremPS.enabled = false;
                dremAnim.Play(dremAnimNameBeforePullstun);
                dremPullStunMode = 0;
            }
            // Match 2, drem stunned
        } else if (dremPullStunMode == 3) {
            novaAttackVac = GameObject.Find("DremVac").GetComponent<Vacuumable>();
            novaAttackVac.ext_MakeNoncollidWhenThrown = true;
            novaAttackVac.transform.position = dremTransform.position;
            novaAttackVac.updateRootedInitialPos(dremTransform.position);
            dremTransform.gameObject.SetActive(false);
            dremPullStunMode = 4;
            t_dremWaitAfterHit = 0;
        } else if (dremPullStunMode == 4) {
            if (novaAttackVac.isPickedUp()) {
                dremPullStunMode = 5;
                dbox.playDialogue("wrestle-2", 2);
                player.StopSuckingAnim();
                TurnOffSideText();
            }
        } else if (dremPullStunMode == 5 && dbox.isDialogFinished()) { 
            if (novaAttackVac.isMoving()) {
                dremPullStunMode = 6;
            } 
        } else if (dremPullStunMode == 6) {
            if (HF.TimerDefault(ref t_dremWaitAfterHit, 1f)) {
                mode = WrestleMode.Dialogue;
                AddHypePoints(40);
            }
        }
    }

    void UpdateGrappling() {

        // move grapple sprite, set cam initial position
        if (grappleMode == 0) {
            t_grappleCamMove += Time.deltaTime;
            if (t_grappleCamMove > 0.25f) {
                if (!MyInput.special && !MyInput.anyDir) {
                    grappleMode = 1;
                    t_grappleCamMove = 0;

                    CutsceneManager.deactivatePlayer = true;

                    t_grappleCamMove = 0;
                    grappleCachedCamPos = Camera.main.transform.position;
                    grappleTransform.position = grappleActivePos.position;
                    tempPos = GameObject.Find("CamAnnounceStart").transform.position;
                    tempPos.z = Camera.main.transform.position.z;

                    t_grappleCharFade = 0;
                    setSRAlpha(grappleImpactSR, 0);
                    setSRAlpha(grappleSR, 0);

                }
            }
        // Move camera, fade out player and drem
        } else if (grappleMode == 1) {
            t_grappleCamMove += Time.deltaTime;
            Camera.main.transform.position = Vector3.Lerp(grappleCachedCamPos, tempPos, Mathf.SmoothStep(0, 1, t_grappleCamMove / 0.5f));

            t_grappleCharFade += Time.deltaTime;
            setSRAlpha(playerSR, Mathf.Lerp(1, 0, t_grappleCharFade / 0.5f));
            setSRAlpha(dremSR, Mathf.Lerp(1, 0, t_grappleCharFade / 0.5f));

            if (t_grappleCamMove > 0.5f && t_grappleCharFade > 0.5f) {
                grappleMode = 2;
                t_grappleCamMove = 0;
                t_grappleCharFade = 0;
                grappleAnim.Play("idle");
            }
            // Fade in grappling sprite, set idling time
        } else if (grappleMode == 2) {
            t_grappleCharFade += Time.deltaTime;
            setSRAlpha(grappleSR, t_grappleCharFade / 0.5f);
            if (t_grappleCharFade > 0.5f) {
                t_grappleCharFade = 0;
                grappleMode = 3;
            }
        } else if (grappleMode == 3) {
            t_grappleAct = 0;
            tm_grappleAct = 0.33f;
            grappleMode = 4;
        // Idle a little, then pick the next anim
        } else if (grappleMode == 4) { 
            if (HF.TimerDefault(ref t_grappleAct,tm_grappleAct)) {
                grappleMode = 5;
                t_grappleAct = 0;
                tm_grappleAct = 0.75f;
                if (grappleanims == null) {
                    grappleanims = new string[] { "u", "u", "r", "r", "d", "d", "l", "l", "jump" };
                }
                nextGrappleAnim= grappleanims[Mathf.FloorToInt(grappleanims.Length * Random.value)];
                if (matchNumber == 1 && hypePoints + 3 > hypeMilestone3Y) nextGrappleAnim = "special";
                if (matchNumber == 2 && hypePoints + 3 > hypeMilestone2Y) nextGrappleAnim = "special";
                if (matchNumber == 3 && hypePoints + 3 > hypeMilestone2Y) nextGrappleAnim = "special";
                UpdateGrappleText(nextGrappleAnim);
            }
        // Flub or play the correct anim based on input
        } else if (grappleMode == 5) {
            if (HF.TimerDefault(ref t_grappleAct, tm_grappleAct)) {
                grappleImpactAnim.Play("flub");
                grappleAnim.Play("flub");
                AudioHelper.instance.playOneShot("playerHurt");
                ReduceHypePoints(2);
                grappleMode = 6;
                player.ScreenShake(0.08f, 0.2f,true);
            }  else {
                bool succeed = false;
                if (nextGrappleAnim == "u" && MyInput.jpUp) succeed = true;
                if (nextGrappleAnim == "r" && MyInput.jpRight) succeed = true;
                if (nextGrappleAnim == "d" && MyInput.jpDown) succeed = true;
                if (nextGrappleAnim == "l" && MyInput.jpLeft) succeed = true;
                if (nextGrappleAnim == "jump" && MyInput.jpJump) succeed = true;
                if (nextGrappleAnim == "special" && MyInput.jpSpecial) succeed = true;
                // If any other input pressed then fail
                if (!succeed) {
                    if (MyInput.jpJump || MyInput.jpSpecial || MyInput.jpUp || MyInput.jpDown || MyInput.jpRight || MyInput.jpLeft) {
                        t_grappleAct = tm_grappleAct;
                    }
                }

                if (succeed) {
                    tm_grappleAct = 0.35f;
                    player.ScreenShake(0.15f, 0.25f, true);
                    if (nextGrappleAnim == "jump" || nextGrappleAnim == "special") {
                        AudioHelper.instance.playOneShot("wrestlecheer");
                        tm_grappleAct = 0.75f;
                        player.ScreenShake(0.2f, 0.55f, true);
                        AudioHelper.instance.playOneShot("crystalHitPlayer");
                        if (nextGrappleAnim == "special") {
                            AudioHelper.instance.playOneShot("wrestleGrappleWin");
                            tm_grappleAct = 1.5f;
                            player.ScreenShake(0.2f, 1.2f, true);
                        }
                    } else {
                        AudioHelper.instance.playOneShot("crystalHitPlayer");
                    }
                    grappleAnim.Play(nextGrappleAnim);
                    grappleImpactAnim.Play(nextGrappleAnim);
                    grappleMode = 6;
                    grappleParticles.Play();
                }
            }
            if (grappleMode == 6) {
                TurnOffGrappleText();
                setSRAlpha(grappleImpactSR, 1);
            }
        } else if (grappleMode == 6) {
            if (HF.TimerDefault(ref t_grappleCharFade, tm_grappleAct)) {
                if (grappleAnim.CurrentAnimationName() != "flub") {
                    AddHypePoints(3);
                }
                grappleMode = 3;
                if (matchNumber == 1 && hypePhase == 3) grappleMode = 7;
                if (matchNumber == 2 && hypePhase == 2) grappleMode = 7;
                if (matchNumber == 3 && hypePhase == 2) grappleMode = 7;
                if (grappleMode != 7) {
                    grappleAnim.Play("idle");
                }
            }
            setSRAlpha(grappleImpactSR, Mathf.Lerp(1, 0, Mathf.SmoothStep(0,1,t_grappleCharFade / tm_grappleAct)));
            if (grappleMode != 6) {
                setSRAlpha(grappleImpactSR, 0);
            }
        // Idle a bit before going to next mode
        } else if (grappleMode == 7) {
            if (HF.TimerDefault(ref t_grappleCharFade, 0.5f)) {
                grappleMode = 8;
                tempPos = Camera.main.transform.position;
            }
        // Fade out the grapple sprite, move camera back
        } else if (grappleMode == 8) {

            t_grappleCharFade += Time.deltaTime;
            setSRAlpha(grappleSR, Mathf.Lerp(1, 0, t_grappleCharFade / 0.5f));

            t_grappleCamMove += Time.deltaTime;
            Camera.main.transform.position = Vector3.Lerp(tempPos, grappleCachedCamPos, Mathf.SmoothStep(0, 1, t_grappleCamMove / 0.5f));

            if (t_grappleCharFade >= 1 && t_grappleCamMove >= 0.5f) {
                t_grappleCamMove = 0;
                t_grappleCharFade = 0;
                dremTransform.position = dremFightPositions[0].position;
                grappleMode = 9;
                dremAnim.Play("idle_r");
            }

        } else if (grappleMode == 9) {
            t_grappleCharFade += Time.deltaTime;
            setSRAlpha(dremSR, t_grappleCharFade / 0.5f);
            setSRAlpha(playerSR, t_grappleCharFade / 0.5f);
            if (t_grappleCharFade > 1f) {
                t_grappleCharFade = 0;
                CutsceneManager.deactivatePlayer = false;
                if (matchNumber == 1) {
                    mode = WrestleMode.Countering;
                    UpdateSideText("wrestle-1", 3);
                }
                if (matchNumber == 2) {
                    mode = WrestleMode.NovaAttacking;
                    UpdateSideText("wrestle-3", 0);
                }
                if (matchNumber == 3) {
                    mode = WrestleMode.Jumping;
                    UpdateSideText("wrestle-3", 1);
                }
                grappleMode = 0;
            }
        }
    }

    int jumpMode = 0;
    float jumpHeight = 2.5f;
    float t_jump = 0;
    float tm_jump = 0.8f; // period of jump
    float tm_jumpWait = 1.8f; // period of wait after landing
    Transform jumpShadow;
    SpriteRenderer jumpShadowSR;
    Vacuumable jumpStar1;
    Vacuumable jumpStar2;
    BoxCollider2D dremHardCollider;

    void UpdateJumping() {
        if (jumpMode == 0) {
            jumpShadow = GameObject.Find("JumpShadow").transform;
            jumpShadowSR = GameObject.Find("JumpShadowSR").GetComponent<SpriteRenderer>();
            // 1 = Right, 2 = L
            jumpStar1 = GameObject.Find("WrestleAmmoStar1").GetComponent<Vacuumable>();
            jumpStar2 = GameObject.Find("WrestleAmmoStar2").GetComponent<Vacuumable>();
            jumpMode = 1;
            foreach (BoxCollider2D bc in dremTransform.GetComponents<BoxCollider2D>()) {
                if (!bc.isTrigger) {
                    dremHardCollider = bc;
                }
            }
            dremHardCollider.isTrigger = true;
        } else if (jumpMode == 1) {
            jumpShadowSR.enabled = true;
            dremInitMovePos = dremTransform.position;
            dremNextPos = player.transform.position;
            t_jump = 0;
            dremAnim.Play("jump_start");
            jumpMode = 2;
        } else if (jumpMode == 2 && HF.TimerDefault(ref t_jump, 0.1f)) {
            dremAnim.Play("jump");
            AudioHelper.instance.playOneShot("playerJump");
            t_jump = 0;
            jumpMode = 3;
        } else if (jumpMode == 3) {
            t_jump += Time.deltaTime;
            float r = t_jump / tm_jump;
            tempPos = Vector3.Lerp(dremInitMovePos, dremNextPos, r);
            jumpShadow.position = tempPos;
            float sinFactor = Mathf.Sin(3.14f * r);
            tempPos.y += sinFactor * jumpHeight;
            dremTransform.position = tempPos;

            if (t_jump > tm_jump) {
                if (Vector3.Distance(dremTransform.position, player.transform.position) < 0.5f) {
                    player.Flicker(1);
                    ReduceHypePoints(3);
                    AudioHelper.instance.playOneShot("playerHurt");
                }
                AudioHelper.instance.playOneShot("crystalHitPlayer", 0.8f, 0.7f);
                t_jump = 0;
                jumpShadowSR.enabled = false;
                jumpMode = 4;
                player.ScreenShake(0.08f, 0.4f, false);
                dremAnim.Play("jump_start");
                if (!jumpStar1.isPickedUp()) {
                    jumpStar1.Respawn();
                    jumpStar1.transform.position = dremTransform.position + new Vector3(0.5f, -0.55f, 0);
                    jumpStar1.GetComponent<Rigidbody2D>().velocity = new Vector2(5f, 0);
                }

                if (!jumpStar2.isPickedUp()) {
                    jumpStar2.Respawn();
                    jumpStar2.transform.position = dremTransform.position + new Vector3(-0.5f, -0.55f, 0);
                    jumpStar2.GetComponent<Rigidbody2D>().velocity = new Vector2(-5f, 0);
                }
                if (stars == null) {
                    stars = new Vacuumable[] { jumpStar1, jumpStar2 };
                }

            }
        } else if (jumpMode == 4) {
            t_jump += Time.deltaTime;


            foreach (Vacuumable star in stars) {

                if (t_jump > tm_jumpWait) {
                    if (star.isPickedUp()) { 

                    } else {
                        if (star.isIdle() && !star.IsBeingSucked()) star.Break();
                    }
                }

                if (star.isMoving() && !MyInput.special) {
                    star.Break();
                    if (Vector3.Distance(star.transform.position, dremTransform.position) < 2f) {
                        ps_dremStars.Play();
                        AudioHelper.instance.playOneShot("crystalHitPlayer");
                        AddHypePoints(7f);
                    }
                } else if (star.isMoving()) {
                    if (Vector3.Distance(star.transform.position, dremTransform.position) < 0.5f) {
                        star.Break();
                        t_flicker = 0.5f;
                        AudioHelper.instance.playOneShot("playerHurt");
                    }
                }
            }

            if (t_jump > tm_jumpWait) {
                t_jump = 0;
                if (hypePhase == 3) {
                    jumpMode = 5;
                    dremHardCollider.isTrigger = false;
                } else {
                    jumpMode = 1;
                }
            }
        } else if (jumpMode == 5) {
            dremNextPos = dremFightPositions[4].position;
            dremAnim.Play("walk_l");
            if (dremTransform.position.x < dremNextPos.x) {
                dremAnim.Play("walk_r");
            }
            dremInitMovePos = dremTransform.position;
            t_dremMove = 0;
            jumpMode = 6;
        } else if (jumpMode == 6) {
            t_dremMove += Time.deltaTime;
            tempPos = Vector3.Lerp(dremInitMovePos, dremNextPos, t_dremMove / 1f);
            dremTransform.position = tempPos;
            if (t_dremMove > 1f) {
                t_dremMove = 0;
                mode = WrestleMode.DremAttacking;
                tradingBlows = false;
                UpdateSideText("wrestle-3", 2);
            }
        }
    }
    Vacuumable[] stars;

    int counterMode = 0;
    Transform counteringObjT;
    float t_counter = 0;
    void UpdateCountering() {

        UpdateChair();

        // Walk drem over to center
        if (counterMode == 0) {
            dremInitMovePos = dremTransform.position;
            dremNextPos = dremFightPositions[4].position;
            t_dremMove = 0;
            dremAnim.Play("walk_l");
            counterMode = 1;
        } else if (counterMode == 1) {
            t_dremMove += Time.deltaTime;
            tempPos = Vector3.Lerp(dremInitMovePos, dremNextPos, t_dremMove / 1f);
            dremTransform.position = tempPos;
            if (t_dremMove > 1) {
                t_dremMove = 0;
                dremAnim.Play("idle_r");
                counterMode = 2;
            }
        // Wait for vac to overlap. Make it unbreakable and turn off the vac for manual control.
        } else if (counterMode == 2) {
            if (dremTrigger.JustTriggered()) {
                if (dremTrigger.vac != null) {
                    counterMode = 3;
                    novaAttackVac = dremTrigger.vac;
                    novaAttackVac.enabled = false;
                    novaAttackVac.breaksWhenMovingAndShootReleased = false;
                    novaAttackVac.IsBreakable = false;
                    counteringObjT = novaAttackVac.transform;
                }
            }

        // After a short delay of the vac moving into drem's trigger, play the counter anim
        // Set player into dodging mode too.
        } else if (counterMode == 3) {
            if (HF.TimerStayAtMax(ref t_counter,0.1f)) {
                dremAnim.Play("spin");
                t_counter = 0;
                dremInitMovePos = counteringObjT.position;
                dremNextPos = player.transform.position;
                counterMode = 4;
                player.wrestleDodging = true;
                counteringObjT.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                tm_dremMove = Vector2.Distance(novaAttackVac.transform.position, player.transform.position) / 7f;
            }
        // Move the vac towards Nova, and either 'hurt' nova (flub), and repeat (and reset vac state),
        // Or if player successfully 'fakes' a hurt, then play sounds+FX, go to dialogue state.
        } else if (counterMode == 4) {
            t_dremMove += Time.deltaTime;
            tempPos = Vector3.Lerp(dremInitMovePos, dremNextPos, t_dremMove / tm_dremMove);
            counteringObjT.position = tempPos;
            if (t_dremMove > tm_dremMove) {
                t_dremMove = 0;
                novaAttackVac.enabled = true;
                novaAttackVac.IsBreakable = true;
                novaAttackVac.Break();
                AudioHelper.instance.playOneShot("sparkBarHit");
                if (Vector2.Distance(novaAttackVac.transform.position, player.transform.position) < 0.5f) {
                    player.Flicker(1);
                }
                counterMode = 2;
                player.wrestleDodging = false;
            } else {
                if (MyInput.jpSpecial && Vector2.Distance(novaAttackVac.transform.position,player.transform.position) < 1.5f) {
                    player.wrestleDodging = false;
                    counterMode = 5;
                    CutsceneManager.deactivatePlayer = true;
                    AddHypePoints(40);
                    TurnOffSideText();
                    mode = WrestleMode.Dialogue;

                    AudioHelper.instance.playOneShot("sparkBarHit");
                    AudioHelper.instance.playOneShot("wrestleCheer");
                    player.ScreenShake(0.15f, 0.25f, true);

                    novaAttackVac.enabled = true;
                    novaAttackVac.Break();

                    player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                    player.GetComponent<Animator>().Play("dead_W");
                }
            }
            // Stop drem's spin anim
            if (counterMode != 4) {
                dremAnim.Play("idle_r");
            }
        } else if (counterMode == 5) {

        }

    }

    void setSRAlpha(SpriteRenderer sr, float alpha) {
        if (alpha > 1) alpha = 1;
        if (alpha < 0) alpha = 0;
        tempColor = sr.color;
        tempColor.a = alpha;
        sr.color = tempColor;
    }
    public void StartMatch(int _matchNumber) {
        matchNumber = _matchNumber;
        if (matchNumber == 2 || matchNumber == 3) {
            tradingBlows = true;
        }
        player.facing = AnoControl2D.Facing.RIGHT;
        AudioHelper.instance.PlaySong("Wrestling", 0, 0);
        if (matchNumber == 1) {
            mode = WrestleMode.NovaAttacking;
            UpdateSideText("wrestle-1", 0);
        }
        if (matchNumber == 2) {
            mode = WrestleMode.NovaAttacking;
            UpdateSideText("wrestle-3", 0);
        }
        if (matchNumber == 3) {
            mode = WrestleMode.NovaAttacking;
            UpdateSideText("wrestle-3", 0);
        }
    }
    void ReduceHypePoints(float points) {
        if (hypePhase == 4) return;

        hypePoints -= points;
        if (hypePhase == 0 && hypePoints < 0) hypePoints = 0;
        if (hypePhase == 1 && hypePoints < hypeMilestone1Y) hypePoints = hypeMilestone1Y;
        if (hypePhase == 2 && hypePoints < hypeMilestone2Y) hypePoints = hypeMilestone2Y;
        if (hypePhase == 3 && hypePoints < hypeMilestone3Y) hypePoints = hypeMilestone3Y;
        tempPos = hypeMeterCoverTransform.localPosition;
        tempPos.y = hypePoints;
        hypeMeterCoverTransform.localPosition = tempPos;
    }
    void AddHypePoints(float points) {
        hypePoints += points;
        if (hypePhase == 0 && hypePoints > hypeMilestone1Y) {
            hypePhase = 1;
            hypeMeterOverlayAnim.Play("1");
            AudioHelper.instance.playOneShot("wrestlebell");
            AudioHelper.instance.playOneShot("wrestlecheer");
        } else if (hypePhase == 1 && hypePoints > hypeMilestone2Y) {
            hypePhase = 2;
            hypeMeterOverlayAnim.Play("2");
            AudioHelper.instance.playOneShot("wrestlebell");
            AudioHelper.instance.playOneShot("wrestlecheer");
        } else if (hypePhase == 2 && hypePoints > hypeMilestone3Y) {
            hypePhase = 3;
            hypeMeterOverlayAnim.Play("3");
            AudioHelper.instance.playOneShot("wrestlebell");
            AudioHelper.instance.playOneShot("wrestlecheer");
        } else if (hypePhase ==3 && hypePoints > hypeEndY) {
            hypePoints = hypeEndY;
            hypePhase = 4;
            AudioHelper.instance.playOneShot("wrestlebell");
            AudioHelper.instance.playOneShot("wrestlecheer");
        }
        tempPos = hypeMeterCoverTransform.localPosition;
        tempPos.y = hypePoints;
        hypeMeterCoverTransform.localPosition = tempPos;
    }
    void Update () {

        if (DataLoader.instance.isPaused) {
            return;
        }

        if (MyInput.shortcut) {
            if (MyInput.special) {
                AddHypePoints(20 * Time.deltaTime);
            }
        }
        if (t_flicker > 0) {
            t_flicker -= Time.deltaTime;
            t_flickerInterval += Time.deltaTime;
            if (t_flickerInterval > 1/16f) {
                dremSR.enabled = !dremSR.enabled;
                t_flicker -= 1 / 16f;
            }
            if (t_flicker < 0) {
                dremSR.enabled = true;
            }
        }

        if (mode == WrestleMode.IntroIdling) {

        } else if (mode == WrestleMode.DremAttacking) {
            UpdateDremAttacking();
        } else if (mode == WrestleMode.NovaAttacking) {
            UpdateNovaAttacking();
        } else if (mode == WrestleMode.Grappling) {
            UpdateGrappling();
        } else if (mode == WrestleMode.Countering) {
            UpdateCountering();
        } else if (mode == WrestleMode.DremSuckable) {

        } else if (mode == WrestleMode.Jumping) {
            UpdateJumping();
        } else if (mode == WrestleMode.OutroIdling) {
            UpdateOutroIdle();
        } else if (mode == WrestleMode.Dialogue) {
            UpdateDialogueMode();
        }

        /*
        MyInput.tagForcePS4 = false;
        MyInput.tagForceXB1 = false;
        if (Input.GetKey(KeyCode.Space)) {
            MyInput.tagForcePS4 = true;
        } else if (Input.GetKey(KeyCode.LeftShift)) {
            MyInput.tagForceXB1 = true;
        }
        if (MyInput.jpUp) {
            UpdateGrappleText("u");
         } else if (MyInput.jpDown) {
            UpdateGrappleText("d");
        } else if (MyInput.jpLeft) {
            UpdateGrappleText("l");
        } else if (MyInput.jpRight) {
            UpdateGrappleText("r");
        } else if (MyInput.jpJump) {
            UpdateGrappleText("jump");
        } else if (MyInput.jpSpecial) {
            UpdateGrappleText("special");
        }
        */

    }
    void UpdateGrappleText(string anim) {
        int line = 0;
        if (anim == "r") line = 0;
        if (anim == "l") line = 1;
        if (anim == "d") line = 2;
        if (anim == "u") line = 3;
        if (anim == "special") line = 4;
        if (anim == "jump") line = 5;
        grapplePromptText.enabled = true;
        grapplePromptBG.enabled = true;
        grapplePromptText.text = DataLoader.instance.getDialogLine("grappletext", line);
        grapplePromptText.ForceMeshUpdate();
        tempVec2 = grapplePromptBG.rectTransform.sizeDelta;
        tempVec2.x = grapplePromptText.renderedWidth + 12f;
        grapplePromptBG.rectTransform.sizeDelta = tempVec2;
    }
    void TurnOffGrappleText() {
        grapplePromptText.enabled = false;
        grapplePromptBG.enabled = false;
    }
    void TurnOffSideText() {
        sidetext.enabled = false;
        sidetextBG.enabled = false;
    }
    // wrestle-1 1, 3, 4 (intro, drem attack countering)
    void UpdateSideText(string scene, int line) {
        sidetext.enabled = true;
        sidetextBG.enabled = true;
        sidetext.text = DataLoader.instance.getDialogLine(scene, line);
        sidetext.ForceMeshUpdate();
        tempVec2 = sidetextBG.rectTransform.sizeDelta;
        tempVec2.y = sidetext.renderedHeight + 13f;
        sidetextBG.rectTransform.sizeDelta = tempVec2;
    }
}
