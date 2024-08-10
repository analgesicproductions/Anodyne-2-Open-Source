using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Anodyne;

public class PicoGlandilock : MonoBehaviour {

    public bool isBullet = false;
    public bool isShockTile = false;

    public Bullet regularBulPrefab;
    public GameObject thornPrefab;

    Vector3 tempPos = new Vector3();
    int mode = 0;


    float tSpeedChange = 0;
    float tmSpeedChange = 2.5f;
    int speedIdx = 0;
    float[] speedArray = new float[] { 5f };

    int thornOffsetIdx = 0;
    int thornDirIdx = 0;
    int[] thornDirs = new int[] { 0, 1 };
    float[] thornOffsets = new float[] { 2, 2, 4, 3, 2, 5, 3, 2, 2, 3, 6, 8, 7, 2, 2, 3, 4, 7 };

    float maxX = 0;
    float minX = 0;

    bool movingRight = false;

    float tMove = 0;

    int health = 6;

    int[] bulModes = new int[6];
    float[] bulTimers = new float[6];
    int bulIdx = 0;

    Vector2Int roompos = new Vector2Int();

    Color tempCol = new Color();
    SpriteRenderer shockZone;

    bool oneBulletGSDead = false;
	void Start () {

        if (isShockTile) {
            shockZone = GameObject.Find("ShockZone").GetComponent<SpriteRenderer>();
            tempCol = shockZone.color; tempCol.a = 0; shockZone.color = tempCol;
        }

        if (DataLoader._getDS("Boss"+name) == 1) {
            GameObject.Destroy(gameObject);
            return;
        }

        if (DataLoader._getDS("BossBulGS1") == 1 || DataLoader._getDS("BossBulGS2") == 1) {
            oneBulletGSDead = true;
        }

        HF.GetRoomPos(transform.position, ref roompos);
        HF.GetPlayer(ref player);
        particles = GetComponent<ParticleSystem>();
        sr = GetComponent<SpriteRenderer>();
        minX = roompos.x * 6f + 0.5f * 2;
        maxX = roompos.x * 6f + 0.5f * 10;


        for (int i = 0; i < 4; i++) {
            Bullet b = Instantiate(regularBulPrefab);
            regularBullets.Add(b);
        }
        Bullet.DieAll(regularBullets);

        for (int i = 0; i <6; i++) {
            GameObject g = Instantiate(thornPrefab);
            g.SetActive(false);
            thornBullets.Add(g);
            oldVels.Add(new Vector2());
        }

    }

    int shockMode = 0;
    float tThorn = 0;

    float tShootBullet = 0;

    bool shockonleft = true;
    bool ispaused = false;
    List<Vector2> oldVels = new List<Vector2>();
    bool gateOn = false;
	void Update () {
        tSpeedChange += Time.deltaTime;
        if (tSpeedChange >= tmSpeedChange) {
            tSpeedChange = 0;
            speedIdx++;
            if (speedIdx >= speedArray.Length) speedIdx = 0;
        }
        float curSpeed = speedArray[speedIdx];

        if (!HF.AreTheseInTheSameroom(transform,player.transform)) {
            return;
        }
        if (!ispaused) {
            if (DataLoader.instance.isPaused) {
                ispaused = true;
                int idx = 0;
                foreach (GameObject g in thornBullets) {
                    oldVels[idx] = g.GetComponent<Rigidbody2D>().velocity;
                    g.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                    idx++;
                }
                return;
            }
        } else {
            if (!DataLoader.instance.isPaused) {
                ispaused = false;
                int idx = 0;
                foreach (GameObject g in thornBullets) {
                    g.GetComponent<Rigidbody2D>().velocity = oldVels[idx];
                    idx++;
                }
            }
            return;
        }

        if (!gateOn) {
            if (player.transform.position.x >= 0.75f + roompos.x * 6f && player.transform.position.x <= (roompos.x + 1) * 6f - 0.75f && player.transform.position.y >= roompos.y * 6f + 0.75f) {
                GameObject.Find(name + "Gate").GetComponent<Gate>().SendSignal("unsignal");
                gateOn = true;
            }
            return;
        }

        if (mode == 0) {
            tMove += Time.deltaTime;
            if (tMove > 1 / curSpeed) {
                tMove = 0;
                tempPos = transform.position;
                if (movingRight) {
                    tempPos.x += 0.25f;
                    if (tempPos.x >= maxX) {
                        movingRight = false;
                    }
                } else {
                    tempPos.x -= 0.25f;
                    if (tempPos.x <= minX) {
                        movingRight = true;
                    }
                }
                transform.position = tempPos;
            }

            if (isBullet && oneBulletGSDead) {
                tShootBullet += Time.deltaTime;
                if (tShootBullet > 0.8f) {
                    tShootBullet = 0;
                    Bullet b = Bullet.GetADeadBullet(regularBullets);
                    b.StartVelocity = 4.3f;
                    if (b != null) {
                        b.LaunchInCardinalDir(transform.position, AnoControl2D.Facing.DOWN);
                    }
                }
            } else if (isShockTile) {
                if (shockMode == 0) {
                    float wait = 2f;
                    // phase shift to start at aminimum
                    // in wait time, go through 2.5 cycles.
                    // map it from -1 to 1 to 0 to 0.5f.
                    float alpha = (1 + Mathf.Sin(6.28f * (0.75f + (tShootBullet / wait) * 2.5f))) / 2f;
                    tShootBullet += Time.deltaTime;
                    if (tShootBullet >= wait) tShootBullet = wait;
                    tempCol = shockZone.color; tempCol.a = alpha; shockZone.color = tempCol;

                    if (tShootBullet >= wait) {
                        shockMode = 1;
                        tShootBullet = 0;
                        alpha = 1f;
                        shockZone.GetComponent<SpriteAnimator>().Play("on");
                        tempCol = shockZone.color; tempCol.a = alpha; shockZone.color = tempCol;
                    }
                } else if (shockMode == 1) {
                    if (shockZone.GetComponent<TriggerChecker>().onPlayer2D) {
                        player.Damage(1);
                    }
                    shockMode = 20;
                } else if (shockMode == 20) {
                    if (HF.TimerDefault(ref tShootBullet,0.5f)) {
                        shockMode = 2;

                    }
                } else if (shockMode == 2) {
                    tShootBullet += Time.deltaTime;
                    if (tShootBullet >= 0.5f) tShootBullet = 0.5f;
                    float alpha = 1 - (tShootBullet / 0.5f);
                    //alpha *= (0.4f);
                    tempCol = shockZone.color; tempCol.a = alpha; shockZone.color = tempCol;

                    if (tShootBullet >= 0.5f) {
                        shockMode = 3;
                        tShootBullet = 0;
                        shockZone.GetComponent<SpriteAnimator>().Play("off");
                    }
                } else if (shockMode == 3) {
                    if (HF.TimerDefault(ref tShootBullet,1f)) {
                        shockMode = 0;
                        if (shockonleft) {
                            tempPos = shockZone.transform.position;
                            tempPos.x += 2.5f;
                            shockZone.transform.position = tempPos;
                        } else {
                            tempPos = shockZone.transform.position;
                            tempPos.x -= 2.5f;
                            shockZone.transform.position = tempPos;
                        }
                        shockonleft = !shockonleft;
                    }
                }
            }

            tThorn += Time.deltaTime;
            if (tThorn >= 0.85f) {
                tThorn = 0;
                thornDirIdx++;
                thornOffsetIdx++;
                if (thornDirIdx >= thornDirs.Length) thornDirIdx = 0;
                if (thornOffsetIdx >= thornOffsets.Length) thornOffsetIdx = 0;
                foreach (GameObject g in thornBullets) {
                    if (!g.activeInHierarchy) {
                        g.SetActive(true);
                        tempPos = g.transform.position;
                        Vector3 tempRot = g.transform.localEulerAngles;

                        if (thornDirs[thornDirIdx] == 0) {
                            tempPos.x = roompos.x * 6f - 0.3f;
                            tempRot.z = 0;
                        } else {
                            tempPos.x = roompos.x * 6f + 6f + 0.3f;
                            tempRot.z = 180f;
                        }
                        tempPos.y = (roompos.y + 1) * 6f - 1.25f;
                        tempPos.y -= thornOffsets[thornOffsetIdx] * 0.5f;
                        g.transform.position = tempPos;
                        g.transform.localEulerAngles = tempRot;
                        g.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                        break;
                    }
                }
            }
            
            bulIdx = 0;
            foreach (GameObject g in thornBullets) {
                if (g.activeInHierarchy) {
                    int bulMode = bulModes[bulIdx];
                    float bulTimer = bulTimers[bulIdx];
                    if (bulMode == 0) {
                        if (g.transform.localEulerAngles.z == 0) { // right
                            bulMode = 1;
                            g.GetComponent<Rigidbody2D>().velocity = Vector2.right * 0.8f;
                        } else {
                            bulMode = 2;
                            g.GetComponent<Rigidbody2D>().velocity = Vector2.right * -0.8f;
                        }
                    } else if (bulMode == 1) {
                        bulTimer += Time.deltaTime;
                        if (bulTimer >= 1f) {
                            bulMode = 3;
                            bulTimer = 0;
                            g.GetComponent<Rigidbody2D>().velocity = Vector2.right * 2.5f;
                        }
                    } else if (bulMode == 2) {
                        bulTimer += Time.deltaTime;
                        if (bulTimer >= 0.5f) {
                            bulMode = 4;
                            bulTimer = 0;
                            g.GetComponent<Rigidbody2D>().velocity = Vector2.right * -2.5f;
                        }
                    } else if (bulMode == 3 || bulMode == 4) {
                        if (!HF.IsInRoom(g.transform.position, roompos.x, roompos.y)) {
                            g.SetActive(false);
                            bulMode = 0;
                        } else if (g.GetComponent<TriggerChecker>().onThingToCheckFor) {
                            bulMode = 5;
                            AudioHelper.instance.playOneShot("sparkBarHit", 0.88f, 1f, 2);
                            g.GetComponent<Rigidbody2D>().velocity = Vector2.up * 4f;
                        }
                    } else if (bulMode == 5) {
                        if (!HF.IsInRoom(g.transform.position, roompos.x, roompos.y)) {
                            g.SetActive(false);
                            g.GetComponent<TriggerChecker>().onThingToCheckFor = false;
                            bulMode = 0;
                        }
                    }
                    if (g.GetComponent<TriggerChecker>().onPlayer2D) {
                        g.GetComponent<TriggerChecker>().onPlayer2D = false;
                        g.SetActive(false);
                        player.Damage(1);
                    }

                    bulTimers[bulIdx] = bulTimer;
                    bulModes[bulIdx] = bulMode;
                }
                bulIdx++;
            }

            if (MyInput.shortcut) {
                mode = 2;
                tWaitAtEnd = 3f;
            }

        } else if (mode == 1) {
            tFlicker += Time.deltaTime;

            tFlickerInterval += Time.deltaTime;
            if (tFlickerInterval >= 0.06f) {
                tFlickerInterval -= 0.06f;
                sr.enabled = !sr.enabled;
            }
            if (tFlicker >= 0.8f) {
                mode = 0;
                tFlicker = 0;
                sr.enabled = true;
            }


        } else if (mode == 2) {
            tWaitAtEnd += Time.deltaTime;
            if (tWaitAtEnd >= 3f) {
                CutsceneManager.deactivatePlayer = false;
                Wormhole2D.ReturningFromPico = true;
                SparkGameController.SparkGameDestObjectName = "BossEntrance";
                SparkGameController.SparkGameDestScene = Registry.GameScenes.NanoZera;
                DataLoader.instance.enterScene("", Registry.GameScenes.Wormhole2D);
                mode = 3;
                int gsphase = DataLoader._getDS("gs-phase");
                gsphase++;
                DataLoader._setDS("gs-phase", gsphase);
                DataLoader._setDS("Boss" + name, 1);
            }
        }
    }

    List<Anodyne.Bullet> regularBullets = new List<Bullet>();
    List<GameObject> thornBullets = new List<GameObject>();

    private void OnTriggerEnter2D(Collider2D collision) {
        if (mode == 0 && collision.name.IndexOf("Thorn") != -1) {
            health--;
            particles.Play();
            AudioHelper.instance.playOneShot("fireGateBurn");
            player.ScreenShake(0.05f, 0.3f, true);
            mode = 1;
            if (health == 0) {
                mode = 2;
                if (name.IndexOf("BulletGS") != -1) {
                    foreach (Bullet bulGO in regularBullets) {
                        bulGO.Die();
                    }
                }
                foreach (GameObject thorrno in thornBullets) {
                    thorrno.SetActive(false);
                }
                deathParticles.Play();
                player.ScreenShake(0.05f, 5f,true);
                AudioHelper.instance.playOneShot("blowupsond");
                CutsceneManager.deactivatePlayer = true;
            }
        }
    }

    float tWaitAtEnd = 0;
    public ParticleSystem deathParticles;
    ParticleSystem particles;
    SpriteRenderer sr;
    AnoControl2D player;
    float tFlicker = 0f;
    float tFlickerInterval = 0f;
}
