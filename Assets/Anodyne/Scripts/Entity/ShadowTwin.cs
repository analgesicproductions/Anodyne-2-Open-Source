using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowTwin : MonoBehaviour {

    SpriteRenderer sr;
    Anodyne.SpriteAnimator anim;
    AnoControl2D player;
    public bool Y_Symmetry = true;
    float midcoord = 0;

    Vector2Int roomPos = new Vector2Int();
	void Start () {
		HF.GetRoomPos(transform.position,ref roomPos);
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Anodyne.SpriteAnimator>();
        HF.GetPlayer(ref player);
        sr.enabled = false;

        if (Y_Symmetry) {
            midcoord = roomPos.x * SceneData2D.RoomSize_X + (SceneData2D.RoomSize_X / 2.0f);
        } else {
            midcoord = roomPos.y * SceneData2D.RoomSize_Y + (SceneData2D.RoomSize_Y / 2.0f);
        }

	}
    Vector3 playerPos = new Vector3();
    int mode = 0;
    bool bb = false;
    void Update() {
        if (mode == 0) {
            if (player.InThisRoom(roomPos)) {
                mode = 1;
                bb = true;
            }
        } else if (mode == 1) {
            if (bb) {
                sr.enabled = true;
                bb = false;
            }
            playerPos = player.transform.position;
            if (Y_Symmetry) {
                float d = midcoord - player.transform.position.x;
                playerPos.x = midcoord +  d;
                transform.position = playerPos;
            } else {
                float d = midcoord - player.transform.position.y;
                playerPos.y = midcoord + d;
                transform.position = playerPos;
            }

            anim.paused = false;
            if (MyInput.up) {
                if (Y_Symmetry) anim.Play("u");
                if (!Y_Symmetry) anim.Play("d");
            } else if (MyInput.down) {
                if (Y_Symmetry) anim.Play("d");
                if (!Y_Symmetry) anim.Play("u");
            } else if (MyInput.right) {
                if (Y_Symmetry) anim.Play("l");
                if (!Y_Symmetry) anim.Play("r");
            } else if (MyInput.left) {
                if (Y_Symmetry) anim.Play("r");
                if (!Y_Symmetry) anim.Play("l");
            } else {
                anim.paused = true;
            }

            if (!player.InThisRoom(roomPos)) {
                mode = 2;
                AudioHelper.instance.playOneShot("blockExplode");
                anim.paused = false;
                anim.Play("break");
            }
        } else if (mode == 2) {
            if (anim.isPlaying == false) {
                mode = 3;
                anim.Play("u");
                sr.enabled = false;
            }
        } else if (mode == 3) {
            if (!player.InThisRoom(roomPos)) {
                mode = 0;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (mode == 1 && collision.name.IndexOf("ShadowTwinKillzone") != -1) {
            anim.Play("break");
            AudioHelper.instance.playOneShot("blockExplode");
            anim.paused = false;
            mode = 2;
        } else if (collision.GetComponent<AnoControl2D>() != null && mode != 3) {
            if (Y_Symmetry) {
                if (player.transform.position.x < transform.position.x) {
                    player.BumpInDir(-8f, 0);
                } else {
                    player.BumpInDir(8f, 0);
                }
            } else {

            }
        }
    }
}
