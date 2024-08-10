using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Anodyne;
public class NexusDoor : MonoBehaviour {

    public Registry.GameScenes scene;
    public string destination;
    public SpriteRenderer overlay;
    private bool onPlayer;

    float t_jump = 0;
    float tm_jump = 0.52f;
    Vector3 startJump = new Vector3();
    Vector3 endJump = new Vector3();
    SpriteAnimator jumpsprite;
    AnoControl2D player;

    public string activationFlag = "";

    Color tempCol = new Color();
    void Start () {
        HF.GetPlayer(ref player);
        ui = HF.Get2DUI();
        jumpsprite = GameObject.Find("NovaJump").GetComponent<SpriteAnimator>();
	}
    bool asdf = false;
    int mode = 0;
	void Update () {
        if (!asdf) {
            asdf = true;

            if (activationFlag != "" && DataLoader._getDS(activationFlag) == 0) {
                gameObject.SetActive(false);
            }
        }
		if (mode == 0) {
            if (onPlayer) {
                mode = 1;
            }

            tempCol = overlay.color;
            tempCol.a += Time.deltaTime * 3.5f;
            if (tempCol.a >= 0.6f) tempCol.a = 0.6f;
            overlay.color = tempCol;
        } else if (mode == 1) {
            tempCol = overlay.color;
            tempCol.a -= Time.deltaTime * 3.5f;
            if (tempCol.a <= 0) tempCol.a = 0;
            overlay.color = tempCol;
            mode = 1;
            if (!onPlayer) {
                mode = 0;
            } else {

                if ((MyInput.jpTalk || MyInput.jpConfirm) && player.IsThereAReasonToPause()) {
                } else if (MyInput.jpTalk || MyInput.jpConfirm) {
                    mode = 2;

                    GameObject.Find("PlayerInteractionIcon").GetComponent<SpriteRenderer>().enabled = false;
                    GameObject.Find(Registry.PLAYERNAME2D).GetComponent<SpriteRenderer>().enabled = false;
                    CutsceneManager.deactivatePlayer = true;
                    jumpsprite.transform.position = GameObject.Find(Registry.PLAYERNAME2D).transform.position;
                    AudioHelper.instance.playSFX("player_jump_up");
                    endJump =startJump = jumpsprite.transform.position;
                    endJump.y += 0.85f;
                }
            }
        } else if (mode == 2) {
            t_jump += Time.deltaTime;

            jumpsprite.transform.position = Vector3.Lerp(startJump, endJump, t_jump / tm_jump);
            Vector3 tempPos = jumpsprite.transform.position;
            tempPos.y += 1f* Mathf.Sin(6.28f * 0.5f * (t_jump / tm_jump));
            jumpsprite.transform.position = tempPos;

            if (t_jump >= tm_jump / 2) {
                jumpsprite.Play("down");
            }
            if (t_jump >= tm_jump) {
                mode = 3;
                CutsceneManager.deactivatePlayer = false;
                DataLoader.instance.enterScene(destination, scene,1,1);
                AudioHelper.instance.playSFX("teleport_up");
                AudioHelper.instance.playSFX("enter_Door");
                t_jump = 0;
            }
        } else if (mode == 3) {
            t_jump += Time.deltaTime;
            if (t_jump >= 0.9f) {
                AudioHelper.instance.playSFX("teleport_down");
                mode = 4;
            }


        } else if (mode == 4) {

        }
    }

    UIManager2D ui;
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.name == Registry.PLAYERNAME2D) {
            onPlayer = true;
            ui.setTalkAvailableIconVisibility(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (mode >= 2) return;
        if (collision.name == Registry.PLAYERNAME2D) {
            onPlayer = false;
            ui.setTalkAvailableIconVisibility(false);
        }
    }
}
