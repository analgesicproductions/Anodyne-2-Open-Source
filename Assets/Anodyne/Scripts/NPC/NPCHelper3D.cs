using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCHelper3D : MonoBehaviour {

    public enum NPCHelper3DType { Drumbird };
    public NPCHelper3DType type;

    Animator anim;
    AudioSource audioSrc;
    DialogueBox dbox;
        

    float t;
    int mode = 0;

	void Start () {
        HF.GetDialogueBox(ref dbox);
		if (type == NPCHelper3DType.Drumbird) {
            audioSrc = GetComponent<AudioSource>();
            anim = GameObject.Find("BeringiaDrumbird").GetComponent<Animator>();
        }
	}

    void Update() {

        if (type == NPCHelper3DType.Drumbird) {
            if (mode == 0) {
                anim.Play("Play", 0, 0);
                audioSrc.loop = false;
                audioSrc.Play();
                mode = 1;
            } else if (mode == 1) {
                t += Time.deltaTime;
                if (t >= 8) {
                    audioSrc.Stop();
                    audioSrc.Play();
                    anim.Play("Play", 0, 0);
                    t = 0;
                } else if (dbox.isDialogFinished() == false) {
                    audioSrc.Stop();
                    anim.CrossFadeInFixedTime("Idle", 0.3f, 0);
                    mode = 2;
                    t = 0;
                }
            } else if (mode == 2) {
                if (dbox.isDialogFinished()) {
                    t += Time.deltaTime;
                    if (t >= 3) {
                        anim.CrossFadeInFixedTime("Transition",0.25f);
                        t = 0;
                        mode = 3;
                    }
                } else {
                    t = 0;
                }
            } else if (mode == 3) {
                t += Time.deltaTime;
                if (t >= 0.833f) {
                    t = 0;
                    audioSrc.Play();
                    anim.Play("Play", 0, 0);
                    mode = 1;
                }
            }
        }
    }
}
