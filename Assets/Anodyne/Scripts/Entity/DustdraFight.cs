using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Anodyne;


public class DustdraFight : MonoBehaviour {
    Vector2Int initRoom;
    AnoControl2D player;
    DialogueBox dbox;

    SpriteRenderer atbSR;
    Vector2 tempPos = new Vector2();
    Color tempCol;
    void Start() {
        HF.GetDialogueBox(ref dbox);
        HF.GetPlayer(ref player);
        HF.GetRoomPos(transform.position, ref initRoom);
        ColorUtility.TryParseHtmlString("#fc5454", out ATBColor);
        tempCol = new Color();

        ATB_Bar = GameObject.Find("ATB_Bar").transform;
        atbSR = ATB_Bar.GetComponent<SpriteRenderer>();
    }

    int health = 10;
    float t_spawn = 0;
    float tm_spawn = 1.5f;
    int spawnArrayIdx = 0;
    int spawnRowIdx = 0;
    public Transform spawnPoint1;
    public Transform spawnPoint2;
    public Transform spawnPoint3;

    int[] spawnArray = new int[] { 1, 0, 1, 0, 1, 1, 0, 1, 0, 3, 0, 0, 2, 1, 1, 0, 2, 1, 0, 0, 0, 1, 1, 2, 0, 1, 0, 0, 0, 1, 0, 2, 3 };

    public GameObject swordPrefab;
    public GameObject shieldPrefab;
    public GameObject healthPrefab;
    public GameObject spikyPrefab;

    public Transform ATB_Top_Pos;
    public Transform ATB_Bottom_Pos;

    public Transform LeftPos;
    public Transform RightPos;
    public Transform TopPos;
    public Transform BottomPos;

    public GameObject BulletPrefab;

    Transform ATB_Bar;
    float t_ATB;
    float tm_ATB_Move = 2f;
    float tm_ATB_Wait = 2f;
    int ATB_Mode = 0;

    int mode = 0;
    float t = 0;

    float tVert = 0;
    float tHor = 0;
    float tmVert = 4f;
    float tmHor = 5.4f;

    float tBullet = 0;
    float tmBullet = 1f;

    int colMode = 0;
    float tATBColor = 0;
    Color ATBColor;

    void Update() {

        if (mode == 0) {
            if (player.InThisRoom(initRoom)) {
                mode = 1;
            }
        } else if (mode == 1 && HF.TimerDefault(ref t, 1)) {
            dbox.playDialogue("fant-dustdra", 1, 2);
            mode = 2;
        } else if (mode == 2 && dbox.isDialogFinished()) {
            AudioHelper.instance.PlaySong("fant-fight",0,0);
            mode = 3;
        } else if (mode == 3) {

            if (DataLoader.instance.isPaused) {
                return;
            }

            float vertSin = (Mathf.Sin(6.28f * tVert / tmVert) + 1) / 2f;
            float horSin = (Mathf.Sin(6.28f * tHor / tmHor) + 1) / 2f;
            HF.TimerDefault(ref tVert, tmVert);
            HF.TimerDefault(ref tHor, tmHor);
            tempPos.x = LeftPos.position.x + (RightPos.position.x - LeftPos.position.x) * horSin;
            tempPos.y = BottomPos.position.y + (TopPos.position.y - BottomPos.position.y) * vertSin;
            transform.position = tempPos;



            #region ATB Movement
            t_ATB += Time.deltaTime;
            if (ATB_Mode == 0) {
                float r = t_ATB / tm_ATB_Move;
                tempPos = Vector2.Lerp(ATB_Top_Pos.position, ATB_Bottom_Pos.position, r);
                ATB_Bar.position = tempPos;
                if (HF.TimerDefault(ref t_ATB, tm_ATB_Move)) {
                    ATB_Mode = 1;
                }
            } else if (ATB_Mode == 1) {
                if (HF.TimerDefault(ref t_ATB, 7.5f+tm_ATB_Wait)) {
                    ATB_Mode = 2;
                }
            } else if (ATB_Mode == 2) {
                float r = t_ATB / tm_ATB_Move;
                tempPos = Vector2.Lerp(ATB_Top_Pos.position, ATB_Bottom_Pos.position, 1 - r);
                ATB_Bar.position = tempPos;
                if (HF.TimerDefault(ref t_ATB, tm_ATB_Move)) {
                    ATB_Mode = 3;

                    Vector3 yOff = new Vector3(0, 0, 0);
                    for (int i = 0; i < 5; i++) {
                        yOff.y = 1 - 0.5f * i;
                        GameObject g = Instantiate(BulletPrefab, LeftPos.position + yOff, Quaternion.identity, null);
                        g.GetComponent<Anodyne.Bullet>().StartVelocity = 5f;
                        g.GetComponent<Anodyne.Bullet>().tPauseBeforeMove = 1f;
                        g.GetComponent<Anodyne.Bullet>().destroyWhenDead = true;
                        g.GetComponent<Anodyne.Bullet>().LaunchInCardinalDir(g.transform.position, AnoControl2D.Facing.RIGHT);
                    }

                }
            } else if (ATB_Mode == 3) {
                if (HF.TimerDefault(ref t_ATB, 3f+tm_ATB_Wait)) {
                    ATB_Mode = 0;
                }
            }
            #endregion

            #region bullets
            tBullet += Time.deltaTime;
            float modifier = 1f + 0.3f * (health / 10f);
            if (ATB_Mode != 3 && tBullet > tmBullet*modifier) {
                GameObject g = Instantiate(BulletPrefab, transform.position, Quaternion.identity, null);
                g.GetComponent<Anodyne.Bullet>().StartVelocity = 3.5f;
                g.GetComponent<Anodyne.Bullet>().destroyWhenDead = true;
                g.name += "dontbreak";
                g.GetComponent<Anodyne.Bullet>().LaunchInCardinalDir(g.transform.position, AnoControl2D.Facing.RIGHT);

                tBullet = 0;
            }
            #endregion


            #region Item Spawning

            t_spawn += Time.deltaTime;
            if (t_spawn > tm_spawn) {
                t_spawn = 0;

                int thingToSpawn = spawnArray[spawnArrayIdx];
                Vector3 spawnPoint;
                if (spawnRowIdx == 0) {
                    spawnPoint = spawnPoint1.position;
                } else if (spawnRowIdx == 1) {
                    spawnPoint = spawnPoint2.position;
                } else {
                    spawnPoint = spawnPoint3.position;
                }

                if (thingToSpawn == 0) {
                    GameObject g = Instantiate(swordPrefab, spawnPoint, Quaternion.identity,null);
                    g.GetComponent<Anodyne.Vacuumable>().enabled = true;
                } else if (thingToSpawn == 1) {
                    GameObject g = Instantiate(shieldPrefab, spawnPoint, Quaternion.identity, null);
                    g.GetComponent<Anodyne.Vacuumable>().enabled = true;
                } else if (thingToSpawn == 2) {
                    GameObject g = Instantiate(healthPrefab, spawnPoint, Quaternion.identity, null);
                    g.GetComponent<Anodyne.Vacuumable>().enabled = true;
                } else if (thingToSpawn == 3) {
                    GameObject g = Instantiate(spikyPrefab, spawnPoint, Quaternion.identity, null);
                    spikyVacs.Add(g.GetComponent<Vacuumable>());
                }

                spawnArrayIdx = (spawnArrayIdx + 1) % spawnArray.Length;
                spawnRowIdx = (spawnRowIdx + 1) % 3;
               
            }
            #endregion

            if (health <= 0) {
                mode = 4;
                foreach (Vacuumable v in spikyVacs) {
                    if (v != null) {
                        v.gameObject.SetActive(false);
                    }
                }
                dbox.playDialogue("fant-dustdra", 3, 14);
                t = 0;
                tBullet = 0;
            }

        } else if (mode == 4) {
            if (dbox.isDialogFinished()) {
                mode = 5;
                GetComponent<ParticleSystem>().Play();
                GetComponent<PositionShaker>().enabled = true;

            }
        } else if (mode == 5) {
            tBullet += Time.deltaTime;
            if (tBullet > 0.15f) {
                tBullet = 0;
                //SeanHF.SpawnDustPoof(new Vector3(initRoom.x * SceneData2D.RoomSize_X + Random.value * SceneData2D.RoomSize_X, initRoom.y * SceneData2D.RoomSize_Y + Random.value * SceneData2D.RoomSize_Y, 1f));
                AudioHelper.instance.playOneShot("fireGateBurn");
                player.ScreenShake(0.07f, 0.06f, false);
            }

            t += Time.deltaTime;
            if (t >= 4f) {
                DataLoader._setDS("fant-dustdra", 1);
                AudioHelper.instance.StopSongByName("fant-fight", false);
                AudioHelper.instance.PlaySong("fant-pico", 0, 0);
                mode = 6;
                GameObject.Find("DustdraDoor").GetComponent<Anodyne.Door>().enabled = true;
                GameObject.Find("DustdraDoor").GetComponent<SpriteRenderer>().enabled = true;
                AudioHelper.instance.playOneShot("fant-fanfare");
                Destroy(GameObject.Find("bossblocker"));
                Destroy(gameObject);

            }
        }

        if (colMode == 0) {
            if (HF.TimerDefault(ref tATBColor,0.67f)) {
                colMode = 1;
                tempCol = atbSR.color;
                atbSR.color = ATBColor;
            }
        } else if (colMode == 1) {
            if (HF.TimerDefault(ref tATBColor, 0.67f)) {
                colMode = 0;
                atbSR.color = tempCol;
            }
        }

    }

    List<Vacuumable> spikyVacs = new List<Vacuumable>();

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.name.IndexOf("ATB_Sword") != -1) {
            health--;
            collision.GetComponent<Anodyne.Vacuumable>().Break();
            HF.SpawnDustPoof(collision.transform.position);
            AudioHelper.instance.playOneShot("nanobotHurt");
            player.ScreenShake(0.08f, 0.15f, false);
        }
    }
}