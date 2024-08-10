using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaftBell : MonoBehaviour {

    UIManager2D ui;
    bool onPlayer = false;
    DialogueBox dbox;
    AnoControl2D player;

    bool isFant = false;
	void Start () {
        ui = HF.Get2DUI();
        HF.GetPlayer(ref player);
        HF.GetDialogueBox(ref dbox);

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "NanoFantasy") {
            isFant = true;
        }
	}

    float t = 0;
    int mode = 0;
	void Update () {
        if (mode == 0) {
            if (onPlayer) {
                if (MyInput.jpTalk) {
                    ui.setTalkAvailableIconVisibility(false);
                    mode = 1;
                    onPlayer = false;
                    if (!Raft.ARaftHasBeenMoved) {
                        mode = 10;
                        dbox.playDialogue("raftbell");
                        // otherwise some raft needs to be reset
                    } else {
                        mode = 1;
                        CutsceneManager.deactivatePlayer = true;

                        if (isFant) {
                            AudioHelper.instance.playOneShot("bell-fant");
                        } else {
                            AudioHelper.instance.playOneShot("bell-ocean");
                        }
                    }
                }
            }
        } else if (mode == 10 && dbox.isDialogFinished()) {
            mode = 0;
        } else if (mode == 1) {
            t += Time.deltaTime;
            if (t > 0.5f) {
                ui.StartFade(0, 1, 0.5f);
                mode = 2;
                t = 0;
                // raft tweak
            }
        } else if (mode == 2) {
            t += Time.deltaTime;
            if (t >= 0.8f) {
                Raft.ResetRafts = true;
                CutsceneManager.deactivatePlayer = false;
                ui.StartFade(1, 0, 0.5f);
                mode = 0;
            }
        }
	}

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.name == Registry.PLAYERNAME2D && !player.inRaftZone) {
            onPlayer = true;
            ui.setTalkAvailableIconVisibility(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.name == Registry.PLAYERNAME2D) {
            onPlayer = false;
            ui.setTalkAvailableIconVisibility(false);
        }
    }
}
