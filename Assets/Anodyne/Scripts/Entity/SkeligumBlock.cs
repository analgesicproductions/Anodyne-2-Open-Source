using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Anodyne {
    public class SkeligumBlock : MonoBehaviour {

        Vacuumable vac;

        public GameObject bridgePiecePrefab;
        List<GameObject> bridgePieces = new List<GameObject>();
        List<Vector3> bridgeCoords = new List<Vector3>();
        AnoControl2D player;
        void Start() {
            vac = GetComponent<Vacuumable>();
            HF.GetPlayer(ref player);
        }

        int mode = 0;

        List<GameObject> bridgesToBreak = new List<GameObject>();

        float tBreak = 0;
        
        void Update() {
            if (mode == 0) {
                if (player.InSameRoomAs(transform.position)) {
                    mode = 1;
                }
            } else if (mode == 1) {
                if (spawnBridge) {
                    mode = 2;
                    tBreak = 0;
                    breakMode = 0;
                    spawnBridge = false;
                    Vector2Int roompos = new Vector2Int();
                    HF.GetRoomPos(player.transform.position, ref roompos);
                    bridgeCoords = TilemetaManager.instance.RemoveHolesForSkeligumBridge(transform.position, bridgeDir, roompos);
                    foreach (Vector3 v in bridgeCoords) {
                        bool breakFast = false;
                        Vector3 pos = v;
                        if (v.z == -1) {
                            pos.z = 0;
                            breakFast = true;
                        }
                        GameObject bridgePieceSprite = Instantiate(bridgePiecePrefab, pos, Quaternion.identity);
                        if (breakFast) {
                            bridgesToBreak.Add(bridgePieceSprite);
                        }
                        if (bridgeDir == AnoControl2D.Facing.DOWN || bridgeDir == AnoControl2D.Facing.UP) {
                            bridgePieceSprite.GetComponent<SpriteAnimator>().Play("vertAppear");
                        } else {
                            bridgePieceSprite.GetComponent<SpriteAnimator>().Play("horAppear");
                        }
                        bridgePieces.Add(bridgePieceSprite);
                    }
                } else if (!player.InSameRoomAs(transform.position)) {
                    mode = 0;
                }
            } else if (mode == 2 && !player.InSameRoomAs(transform.position)) {

                mode = 0;
                foreach (GameObject g in bridgePieces) {
                    Destroy(g);
                }
                bridgePieces.Clear();
                bridgesToBreak.Clear();
                vac.Respawn();
                TilemetaManager.instance.restoreHoleTiles(bridgeCoords);
            } else if (mode == 2) {

                if (breakMode == 0) {
                    tBreak += Time.deltaTime;
                    if (tBreak > 0.4f) {
                        breakMode = 1;
                        tBreak = 0;
                        foreach (GameObject go in bridgesToBreak) {
                            go.GetComponent<SpriteAnimator>().Play("break");
                        }
                    }
                } else if (breakMode == 1) {
                    foreach (GameObject go in bridgesToBreak) {
                        if (go.GetComponent<SpriteAnimator>().isPlaying == false) {
                            breakMode = 2;
                            go.GetComponent<SpriteRenderer>().enabled = false;
                        }
                    }
                } else if (breakMode == 2) {

                }

            }
        }
        int breakMode = 0;
        AnoControl2D.Facing bridgeDir = AnoControl2D.Facing.RIGHT;
        bool spawnBridge = false;

        private void OnTriggerEnter2D(Collider2D collision) {
            if (mode != 1) return;
            if (collision.name.IndexOf("SkelBlockBul") != -1) {
                if (collision.GetComponent<Bullet>().IsDead()) return;
                spawnBridge = true;
                Vector2 vel = collision.GetComponent<Bullet>().preColVel;
                vac.Break();
                if (vel.x > 1) {
                    bridgeDir = AnoControl2D.Facing.RIGHT;
                } else if (vel.x <= -1) {
                    bridgeDir = AnoControl2D.Facing.LEFT;
                } else if (vel.y > 1) {
                    bridgeDir = AnoControl2D.Facing.UP;
                } else if (vel.y <= -1) {
                    bridgeDir = AnoControl2D.Facing.DOWN;
                }
            }
        }

        private void OnCollisionEnter2D(Collision2D collision) {
            if (mode != 1) return;
            if (collision.collider.name.IndexOf("SkelBlock") == 0) {
                Vacuumable skelBlockVac = collision.collider.GetComponent<Vacuumable>();
                if (skelBlockVac.isIdle() == false) {
                    Vector2 vel = skelBlockVac.preCollisionVelocity;
                    spawnBridge = true;
                    vac.Break();
                    if (vel.x > 1) {
                        bridgeDir = AnoControl2D.Facing.RIGHT;
                    } else if (vel.x <= -1) {
                        bridgeDir = AnoControl2D.Facing.LEFT;

                    } else if (vel.y > 1) {
                        bridgeDir = AnoControl2D.Facing.UP;

                    } else if (vel.y <= -1 ) {
                        bridgeDir = AnoControl2D.Facing.DOWN;

                    }
                }
            }
        }
    }
}