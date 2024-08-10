using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raft : MonoBehaviour {
    Transform player_T;
    Vector3 initPos;
    Vector2Int initRoomPos;
    AnoControl2D player;
    bool movedAwayFromInitPos = false;
    CapsuleCollider2D[] ccs;
    float playerCC_y_Offset = 0;
    TilemetaManager tmm;
    Vector3 tempVec = new Vector3();
    bool attachedToPlayer;
    bool playerInThisRaftZone = false;
    BoxCollider2D bc;
    int mode = 0;
    Vector3 lastOnWaterPos;
    float tWaterPos = 0;
    Vector2 playerCCPos;
    public static bool ARaftHasBeenMoved = false;

    public static bool ResetRafts = false;

    void Start () {
        Raft.ARaftHasBeenMoved = false;
        ccs = GetComponents<CapsuleCollider2D>();
        SetCCsIsTrigger(true);
        initPos = transform.position;
        HF.GetRoomPos(transform.position, ref initRoomPos);
        HF.GetPlayer(ref player);
        player_T = GameObject.Find(Registry.PLAYERNAME2D).transform;
        playerCC_y_Offset = player_T.GetComponent<CircleCollider2D>().offset.y;
        tmm = DataLoader.instance.GetComponent<TilemetaManager>();
        bc = GetComponent<BoxCollider2D>();
	}
    bool playerWasInThisRaftZone = false;

    public bool hasBeenMoved() {
        return movedAwayFromInitPos;
    }

    float tLerp = 0f;
    bool needsToReturnToOffshore = false;
    Vector2 offshorepos = new Vector2();    
    Vector2 offshorepos_start = new Vector2();    
	void Update () {
        tempVec = player_T.position;
        tempVec.y += playerCC_y_Offset;
        playerCCPos = tempVec;
        playerInThisRaftZone = bc.OverlapPoint(playerCCPos);

        bool shouldGoOnRaft = false;
        if (Mathf.Abs(transform.position.x - playerCCPos.x) <= 0.56f && Mathf.Abs(transform.position.y - playerCCPos.y) <= 0.56f) {
            shouldGoOnRaft = true;
        }
        bool onWater = tmm.IsWater(transform.position);
        if (onWater) {
            if (HF.TimerDefault(ref tWaterPos, 0.025f)) {
                lastOnWaterPos = transform.position;
            }
        }

        if (mode == 0) {
            if (needsToReturnToOffshore) {
                needsToReturnToOffshore = false;
                Vector2 delta = lastOnWaterPos - transform.position;
                delta.Normalize();
                transform.position = lastOnWaterPos;
                offshorepos_start = transform.position;
                offshorepos = transform.position;
                offshorepos += delta * 0.8f;
                tLerp = 0.2f;
            }
            if (tLerp > 0) {
                transform.position = Vector2.Lerp(offshorepos_start, offshorepos, Mathf.SmoothStep(0,1, 1 - (tLerp / 0.2f)));
                tLerp -= Time.deltaTime;
                return;
            }

            if (Raft.ARaftHasBeenMoved && !movedAwayFromInitPos) {
                mode = 100;
                GetComponentInChildren<SpriteRenderer>().enabled = false;
                return;
            }

            // note - rafts are now reset only by bells.

            if (playerInThisRaftZone) {
                playerWasInThisRaftZone = true;
                player.inRaftZone = true;
            } else {
                if (playerWasInThisRaftZone) {
                    player.inRaftZone = false;
                }
            }
            // Start raft movement logic
            if (shouldGoOnRaft && !player.ridingRaft && (movedAwayFromInitPos || !Raft.ARaftHasBeenMoved)) {
                Raft.ARaftHasBeenMoved = true;
                SetCCsIsTrigger(false);
                print("Player enter raft mode");
                player.ridingRaft = true;
                mode = 1;
                player.activeRaftRB = GetComponent<Rigidbody2D>();
                movedAwayFromInitPos = true;
            }
        } else if (mode == 100) { 
            // A raft will not reappear until a raft is not being ridden and has returned to its original spot
            if (!Raft.ARaftHasBeenMoved) {
                GetComponentInChildren<SpriteRenderer>().enabled = true;
                mode = 0;
            }
        }else if (mode == 1) {
            // When raft is not on the water, turn off its colliders, make the player stop controlling it, and stop its velocity.
            if (!onWater) {
                print("raft not on water");
                mode = 2;
                player.ridingRaft = false;
                SetCCsIsTrigger(true);
                GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            }
        } else if (mode == 2) {

            player.inRaftZone = playerInThisRaftZone;
            // now the player needs to disembark - but only consider the player disembarked if they are not standing in the water
            if (!playerInThisRaftZone) {
                if (!tmm.IsWater(playerCCPos)) {
                    print("player disembarked to shore");
                    mode = 3;
                } else {
                    print("player ran into water");
                    mode = 4;
                }
            }
        // Once raft off water, wait for player to walk away from it 
        } else if (mode == 3) {
            if (!shouldGoOnRaft) {
                print("player off raft. raft will return to water.");
                mode = 0;
                player.setWaterRespawnPositions(transform.position);
                needsToReturnToOffshore = true;
            }
        // player ran off the raft but into water
        } else if (mode == 4) {

            player.inRaftZone = playerInThisRaftZone;
            player.setWaterRespawnPositions(transform.position);
            // player ran back onto the raft, go back to 'need to disembark'
            if (playerInThisRaftZone) {
                print("player is back on raft.");
                mode = 2;
            // Player ran onto land, so move the raft back to oldpost
            } else if (!tmm.IsWater(playerCCPos)) {
                print("player ran onto shore.");
                mode = 3;
            }
        }

        if (Raft.ResetRafts && ((mode >= 1 && mode <= 4) || (mode == 0 && movedAwayFromInitPos))) {
            Raft.ResetRafts = false;
            Raft.ARaftHasBeenMoved = false;

            mode = 0;
            tLerp = 0;
            needsToReturnToOffshore = false;
            transform.position = initPos;
            movedAwayFromInitPos = false;
            playerWasInThisRaftZone = false;

            player.inRaftZone = false;
            

        }

    }

    public bool currentlyHidden() {
        return mode == 100;
    }

    void SetCCsIsTrigger(bool b) {
        foreach (CapsuleCollider2D cc in ccs) {
            cc.isTrigger = b;
        }
    }
}
