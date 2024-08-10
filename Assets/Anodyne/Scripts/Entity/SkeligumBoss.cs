using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Anodyne;
public class SkeligumBoss : MonoBehaviour {
    int mode = 0;
    public TriggerChecker fightStartTrig;

    Transform keyLeftTrans;
    Transform keyRightTrans;
    //SpriteAnimator animLeft;
    //SpriteAnimator animRight;
    Rigidbody2D rbLeft;
    Rigidbody2D rbRight;
    AnoControl2D player;


    Vector2 tempVel = new Vector2();

    public Gate entryGate1;
    public Gate entryGate2;

    public GameObject loveBulletPrefab;
    public GameObject loveSkelBulletPrefab;
    List<Bullet> bullets = new List<Bullet>();

	void Start () {
        if (DataLoader._getDS("skeligumboss") == 1) {
            Destroy(gameObject);
            return;
        }

        HF.GetPlayer(ref player);
        keyLeftTrans = GameObject.Find("BossKeyLeft").transform;
        keyRightTrans= GameObject.Find("BossKeyRight").transform;
      //  animLeft = keyLeftTrans.GetComponent<SpriteAnimator>();
      //  animRight = keyRightTrans.GetComponent<SpriteAnimator>();
        rbLeft = keyLeftTrans.GetComponent<Rigidbody2D>();
        rbRight = keyRightTrans.GetComponent<Rigidbody2D>();

        for (int i = 0; i < 15; i ++) {
            GameObject g = Instantiate(loveBulletPrefab, null);
            bullets.Add(g.GetComponent<Bullet>());
            g.GetComponent<Bullet>().InitComponents();
            g.GetComponent<Bullet>().startAndStopChildParticles = true;
            g.SetActive(false);
        }
        for (int i = 0; i < 15; i++) {
            GameObject g = Instantiate(loveSkelBulletPrefab, null);
            bullets.Add(g.GetComponent<Bullet>());
            g.GetComponent<Bullet>().InitComponents();
            g.GetComponent<Bullet>().startAndStopChildParticles = true;
            g.SetActive(false);
        }
    }

    float midY = 0;
    float topY = 0;
    float bottomY = 0;

    float moveSpeed = 3f;
    float tMisc = 0;

    Vector3 tempVec = new Vector3();
    public bool TestEnding = false;
    public bool TestPhase3 = false;
    void Update () {

        if (player.IsThereAReasonToPause()) {
            tPhase1Shoot = tPhase2Shoot = tPhase3Shoot = 0;
        }

        if (mode == 0 && fightStartTrig.onPlayer2D) {
            CutsceneManager.deactivatePlayer = true;
            mode = 1;
            entryGate1.SendSignal("unsignal");
            entryGate2.SendSignal("unsignal");
            topY = HF.GetRoomY(player.transform.position.y) * 12f + 10f;
            midY = HF.GetRoomY(player.transform.position.y) * 12f + 6f;
            bottomY = HF.GetRoomY(player.transform.position.y) * 12f + 2f;
            keyLeftTrans.position = new Vector3(HF.GetRoomX(player.transform.position.x) * 12f + 2f, HF.GetRoomY(player.transform.position.y) * 12f - 2f, 0);
            keyRightTrans.position = new Vector3(HF.GetRoomX(player.transform.position.x) * 12f + 10f, HF.GetRoomY(player.transform.position.y) * 12f - 2f, 0);
            tempVel.Set(0, moveSpeed);
            if (MyInput.shortcut) {
                tempVel *= 4f;
            }
            rbLeft.velocity = tempVel;
            rbRight.velocity = tempVel;
            ps_Left = keyLeftTrans.GetComponent<PositionShaker>();
            ps_Right = keyRightTrans.GetComponent<PositionShaker>();
            if (TestEnding) {
                mode = 8;
                ps_Right.enabled = true;
                ps_Left.enabled = true;
                rbLeft.velocity = rbRight.velocity = Vector2.zero;

                tempVec = keyLeftTrans.position; tempVec.y = midY; keyLeftTrans.position = tempVec;
                tempVec = keyRightTrans.position; tempVec.y = midY; keyRightTrans.position = tempVec;


            }

        } else if (mode == 1) {
            if (keyLeftTrans.position.y >= midY) {
                rbLeft.velocity = Vector2.zero;
                rbRight.velocity = Vector2.zero;
                mode = 3;
                AudioHelper.instance.StopAllSongs();
                AudioHelper.instance.PlaySong("NanobotFight", 0, 42.857f);

                HF.GetDialogueBox(ref dbox);
                dbox.playDialogue("skel-both2");
            }
        } else if (mode == 3) {

            if (!dbox.isDialogFinished()) {
                return;
            }
            if (MyInput.shortcut) {
                tMisc = 0.4f;
            }
            if (HF.TimerDefault(ref tMisc, 0.4f)) {
                CutsceneManager.deactivatePlayer = false;
                tempVel.Set(0, moveSpeed);
                rbLeft.velocity = tempVel;
                tempVel.y = -moveSpeed;
                rbRight.velocity = tempVel;
                mode = 4;
                if (TestPhase3) {
                    mode = 7;
                }
            }
        } else if (mode == 4) {
            KeyMovementLogic();
            mode = 5;
        } else if (mode == 5) {
            KeyMovementLogic();
            if (HF.TimerDefault(ref tPhase1Shoot, tmPhase1Shoot)) {
                if (phase1Bullets == 0) {
                    mode = 6;
                } else {
                    Bullet b = getADeadBullet(phase1UseRight);
                    if (b != null) {
                        b.gameObject.SetActive(true);
                        SetBulIgnore(phase1UseRight, b);
                        b.LaunchHoming(getRightOrLeftKeyTrans(phase1UseRight), homingStrength, getRightOrLeftKeyTransForShooter(phase1UseRight).position, getRightOrLeftKeyTrans(phase1UseRight).position, 0);
                        phase1UseRight = !phase1UseRight;
                        phase1Bullets--;
                    }
                    AudioHelper.instance.playOneShot("blockexplode", 1, 1 + 0.2f * Random.value, 2);

                }
            }
        } else if (mode == 6) {
            KeyMovementLogic();

            if (HF.TimerDefault(ref tPhase2Shoot, tmPhase2Shoot)) {
                if (phase2Bullets <= 0) {
                    mode = 7;
                } else {
                    for (int i = 0; i < 2; i++) {
                        Bullet b = getADeadBullet(phase1UseRight);
                        if (b == null) continue;
                        b.StartVelocity = 8f;
                        b.gameObject.SetActive(true);
                        SetBulIgnore(phase1UseRight, b);
                        float angle = 80f;

                        if (i == 1) {
                            angle = -80f;
                        }
                        b.LaunchHoming(getRightOrLeftKeyTrans(phase1UseRight), homingStrength, getRightOrLeftKeyTransForShooter(phase1UseRight).position, getRightOrLeftKeyTrans(phase1UseRight).position, angle);

                    }
                    AudioHelper.instance.playOneShot("blockexplode", 1, 1 + 0.2f * Random.value, 2);

                    phase1UseRight = !phase1UseRight;
                    phase2Bullets -= 2;
                }
            }
        } else if (mode == 7) {
            KeyMovementLogic();

            if (HF.TimerDefault(ref tPhase3Shoot, tmPhase3Shoot)) {
                if (phase3Bullets <= 0) {
                    mode = 8;
                    rbLeft.velocity = Vector2.zero;
                    rbRight.velocity = Vector2.zero;
                    keyLeftTrans.GetComponent<PositionShaker>().enabled = true;
                    keyRightTrans.GetComponent<PositionShaker>().enabled = true;
                } else {
                    for (int i = 0; i < 3; i++) {
                        Bullet b = getADeadBullet(phase1UseRight);
                        if (b == null) continue;

                        b.StartVelocity = 8f;
                        b.gameObject.SetActive(true);
                        SetBulIgnore(phase1UseRight, b);
                        float angle = 0f;

                        if (i == 1) {
                            angle = -100f;
                        } else if (i == 2) {
                            angle = 100f;
                        }
                        b.LaunchHoming(getRightOrLeftKeyTrans(phase1UseRight), homingStrength * 0.8f, getRightOrLeftKeyTransForShooter(phase1UseRight).position, getRightOrLeftKeyTrans(phase1UseRight).position, angle);
                    }
                    AudioHelper.instance.playOneShot("blockexplode", 1, 1 + 0.2f * Random.value, 2);

                    phase1UseRight = !phase1UseRight;
                    phase3Bullets -= 3;
                }
            }
            // anohter pattern
        } else if (mode == 8) {
            ps_Left.amplitude.Set(0.04f * Time.deltaTime + ps_Left.amplitude.x, 0.04f * Time.deltaTime + ps_Left.amplitude.y, 0);
            ps_Right.amplitude.Set(0.04f * Time.deltaTime + ps_Left.amplitude.x, 0.04f * Time.deltaTime + ps_Left.amplitude.y, 0);
            tMisc += Time.deltaTime;
            if (tMisc > 3f) {
                ps_Left.enabled = false;
                ps_Right.enabled = false;
                //dbox.playDialogue("skel-love");
                tMisc = 0;
                mode = 81;
            }
            // charge and die
        } else if (mode == 81) {
                rbLeft.velocity = (keyRightTrans.position - keyLeftTrans.position).normalized * 6f;
                rbRight.velocity = (keyLeftTrans.position - keyRightTrans.position).normalized * 6f;
                mode = 9;
        } else if (mode == 9) {
            tMisc += Time.deltaTime;
            if (Vector2.Distance(keyRightTrans.position,keyLeftTrans.position) < 0.3f || tMisc >= 5f) {
                particles.transform.position = keyRightTrans.position;
                particles.Play();
                tMisc = 0;

                AudioHelper.instance.playOneShot("fireGateBurn");
                //AudioHelper.instance.playOneShot("skeligumLove",0.75f);
                rbLeft.velocity = rbRight.velocity = Vector2.zero;
                keyLeftTrans.GetComponent<SpriteRenderer>().enabled = false;
                keyRightTrans.GetComponent<SpriteRenderer>().enabled = false;
                player.ScreenShake(0.05f, 1f);
                DataLoader.instance.setDS("skeligumboss", 1);
                AudioHelper.instance.StopSongByName("NanobotFight");
                mode = 10;
            }
        } else if (mode == 10) {
            if (HF.TimerDefault(ref tMisc, 1.6f)) {
                entryGate1.SendSignal();
                entryGate2.SendSignal();
                mode = 11;
            }
        }
    }
    DialogueBox dbox;
    public ParticleSystem particles;
    PositionShaker ps_Left;
    PositionShaker ps_Right;
    void SetBulIgnore(bool giveRight, Bullet b) {
        giveRight = !giveRight;
        if (giveRight) {
            b.ignoreTrigName = keyRightTrans.name;
        } else {
            b.ignoreTrigName = keyLeftTrans.name;
        }
    }

    Bullet getADeadBullet(bool isRightSkel) {
        foreach (Bullet bul in bullets) {
            if (bul.IsDead() && isRightSkel && bul.name.IndexOf("SkelLoveBullet") == -1) {
                return bul;
            }
            if (bul.IsDead() && !isRightSkel && bul.name.IndexOf("GumLoveBullet") == -1) {
                return bul;
            }
        }
        return null;
    }

    Transform getRightOrLeftKeyTransForShooter(bool giveRight) {
        if (!giveRight) return keyRightTrans;
        return keyLeftTrans;
    }

    Transform getRightOrLeftKeyTrans(bool giveRight) {
        if (giveRight) return keyRightTrans;
        return keyLeftTrans;
    }

    int phase1Bullets = 20;
    float tPhase1Shoot = 0;
    float tmPhase1Shoot = .6f;

    int phase2Bullets = 30;
    float tPhase2Shoot = 0f;
    float tmPhase2Shoot = 0.85f;


    int phase3Bullets = 36;
    float tPhase3Shoot = 0f;
    float tmPhase3Shoot = 0.9f;

    public float homingStrength = 3f;
    bool phase1UseRight = false;

    private void KeyMovementLogic() {
        if (keyLeftTrans.position.y >= topY && rbLeft.velocity.y >= 0) {
            tempVel.y = moveSpeed * 2f* -1;
            rbLeft.velocity = tempVel;
        } else if (keyLeftTrans.position.y <= bottomY && rbLeft.velocity.y <= 0) {
            tempVel.y = moveSpeed * 1.55f;
            rbLeft.velocity = tempVel;
        }

        if (keyRightTrans.position.y >= topY && rbRight.velocity.y >= 0) {
            tempVel.y = moveSpeed * 1.2f * -1f;
            rbRight.velocity = tempVel;
        } else if (keyRightTrans.position.y <= bottomY && rbRight.velocity.y <= 0) {
            tempVel.y = moveSpeed * 1.2f;
            rbRight.velocity = tempVel;
        }
    }
}
