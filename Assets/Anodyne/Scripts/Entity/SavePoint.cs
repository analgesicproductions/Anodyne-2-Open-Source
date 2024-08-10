using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePoint : MonoBehaviour {

	UIManagerAno2 ui;

    UIManager2D ui2d;

    ParticleSystem saveParticle;
    Transform ring3D;
    Transform ring3D_2;
    float maxRingScale = 3f;
    float tm_RingScale = 0.23f;
    float t_RingScale = 0;

    bool is2D;
    int ticksfromstart = 0;
    Anodyne.SpriteAnimator anim;
    DialogueBox dbox;
    MediumControl player3D;
    // Use this for initialization
    public static bool AutoSaveOn2D = true;

	void Start () {
        is2D = false;
        if (GameObject.Find("2D UI") != null) {
            is2D = true;
        }
        HF.GetDialogueBox(ref dbox);


        if (!is2D) {
            ui = GameObject.Find("3D UI").GetComponent<UIManagerAno2>();
            saveParticle = transform.parent.Find("SaveParticle").GetComponent<ParticleSystem>();
            ring3D = transform.parent.Find("EggSavePoint").Find("SaveEgg_C");
            ring3D_2 = transform.parent.Find("EggSavePoint").Find("SaveEgg_C2");
            HF.GetPlayer(ref player3D);
        } else {
            anim = GetComponent<Anodyne.SpriteAnimator>();
            ui2d = GameObject.Find("2D UI").GetComponent<UIManager2D>();
        }
	}

	int mode = 0;
	// Update is called once per frame
	void Update () {
        ticksfromstart++;
		if (mode == 0) {
		} else if (mode == 1) {
            if (!is2D) {
                if (player3D.gameObject.activeInHierarchy == false || MyInput.jpToggleRidescale) {
                    ExitTrig3D();
                    return;
                }
            }
            if (!DataLoader.instance.isPaused && ((AutoSaveOn2D && is2D) ||MyInput.jpTalk)) {
                if (is2D) {
                    AudioHelper.instance.playSFX("save2dflash", true, 0.8f);
                    anim.Play("flash");
                    GameObject.Find(Registry.PLAYERNAME2D).GetComponent<Anodyne.HealthBar>().FullHeal();
                    ui2d.StartSavingTextFade();
                    anim.ScheduleFollowUp("off");
                    ui2d.setTalkAvailableIconVisibility(false);
                }
                if (!is2D) {
                    player3D.StopRunning();
                    AudioHelper.instance.playSFX("save2dflash", true, 0.8f,false,1f);
                    ui.setTalkAvailableIconVisibility(false);
                    saveParticle.Play();
                }
                bool savedSuccessfully = SaveManager._Save(currentInUseFileIndex);
                if (savedSuccessfully) {
                    print("saved to file " + currentInUseFileIndex);
                } else {
                    print("saving to file " + currentInUseFileIndex + " failed");
                }


                if (is2D) {
                    mode = 2;
                } else {
                    mode = 3;
                    if (savedSuccessfully) {
                        dbox.playDialogue("savedGame",0);
                    } else {
                        dbox.playDialogue("savedGame",1);
                    }
                }
            }
		} else if (mode == 2) {
			mode = 0;
		} else if (mode == 3) {
            if (dbox.isDialogFinished()) {
                mode = 0;
            }
        }
		
        if (scalingMode == 0) {

        } else if (scalingMode == 1) {
            tempV = ring3D.localScale;
            t_RingScale += Time.deltaTime;
            float f = Mathf.Lerp(1, maxRingScale, t_RingScale / tm_RingScale);
            tempV.Set(f, f, f);
            ring3D.localScale = tempV;
            ring3D_2.localScale = tempV;
            f = Mathf.Lerp(0,-3, t_RingScale / tm_RingScale);
            tempV = ring3D.localPosition; tempV.y = f; ring3D.localPosition = tempV;
            tempV = ring3D_2.localPosition; tempV.y = f; ring3D_2.localPosition = tempV;

            if (t_RingScale >= tm_RingScale) {
                scalingMode = 2;
                t_RingScale = 0;
            }
        } else if (scalingMode == 2) {

        } else if (scalingMode == 3) {
            tempV = ring3D.localScale;
            t_RingScale += Time.deltaTime;
            float f = Mathf.Lerp(1, maxRingScale, 1 - (t_RingScale / tm_RingScale));
            tempV.Set(f, f, f);
            ring3D.localScale = tempV;
            ring3D_2.localScale = tempV;

            f = Mathf.Lerp(0, -3, 1 - (t_RingScale / tm_RingScale));
            tempV = ring3D.localPosition; tempV.y = f; ring3D.localPosition = tempV;
            tempV = ring3D_2.localPosition; tempV.y = f; ring3D_2.localPosition = tempV;

            if (t_RingScale >= tm_RingScale) {
                scalingMode = 0;
                t_RingScale = 0;
            }

        }

    }

    Vector3 tempV = new Vector3();
    public static int currentInUseFileIndex = 0;

    int scalingMode = 0;
	void OnTriggerEnter (Collider o) {
        if (ticksfromstart < 10) return;
        if (o.CompareTag("Player") == false || o.name != "MediumPlayer" ) return;

        AudioHelper.instance.playSFX("save3dglow", true, 1f);
        ring3D.GetComponent<ObjectRotator>().localYRotateSpeed = -130f;
        ring3D_2.GetComponent<ObjectRotator>().localYRotateSpeed = 60f;
        ui.setTalkAvailableIconVisibility(true,2);
        scalingMode = 1;
        mode = 1;
    }

    void ExitTrig3D() {
        ui.setTalkAvailableIconVisibility(false, 2);
        ring3D.GetComponent<ObjectRotator>().localYRotateSpeed = -50f;
        ring3D_2.GetComponent<ObjectRotator>().localYRotateSpeed = 30f;
        if (scalingMode == 2) scalingMode = 3;
        if (mode == 1) mode = 0;
    }

    void OnTriggerExit (Collider o) {
		if (o.CompareTag("Player") == false || o.name != "MediumPlayer") return;
        ExitTrig3D();
	}

    private void OnTriggerEnter2D(Collider2D o) {
        if (ticksfromstart < 10) return;
        if (o.CompareTag("Player") == false || o.name != "2D Ano Player") return;
        anim.Play("glow");
        AudioHelper.instance.playSFX("save2dglow",true,1f);
        if (!AutoSaveOn2D) ui2d.setTalkAvailableIconVisibility(true);
        mode = 1;

    }
    private void OnTriggerExit2D(Collider2D o) {
        if (o.CompareTag("Player") == false || o.name != "2D Ano Player") return;
        ui2d.setTalkAvailableIconVisibility(false);
        if (mode == 1) {
            anim.Play("off");
            mode = 0;
        }
    }
}
