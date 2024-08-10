using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Anodyne;

public class GIFCutscene : MonoBehaviour {


    public bool movesWithDir;
    public float movespeed;
    Vector3 temp;

    public bool startPosIsLeftXLimit = false;

    DialogueBox dbox;
    [Header("Halloween")]
    public bool IsHalloween = false;
    public SpriteAnimator innocentAnim;
    public SpriteAnimator innocent2Anim;
    public SpriteAnimator manAnim;
    public SpriteAnimator exclamationAnim;
    Transform innocentTransform;
    Transform manTransform;
    int hm = 0;
    public Transform mobTransform;
    public SpriteAnimator halloBGAnim;


    Vector3 initpos;
	void Start () {
        initpos = transform.position;
        dbox = GameObject.Find("Dialogue").GetComponent<DialogueBox>();

        if (IsHalloween) {
            innocentTransform = innocentAnim.transform;
            manTransform = manAnim.transform;
        }

	}
	
	void Update () {
        if (IsHalloween) {
            UpdateHalloween();
            return;
        }
		if (movesWithDir) {
            temp = transform.position;
            if (MyInput.right) temp.x += Time.deltaTime * movespeed;
            if (MyInput.left) temp.x -= Time.deltaTime * movespeed;
            if (MyInput.up) temp.y += Time.deltaTime * movespeed;
            if (MyInput.down) temp.y -= Time.deltaTime * movespeed;

            if (startPosIsLeftXLimit) {
                if (temp.x < initpos.x) {
                    temp.x = initpos.x;
                    print("hit left limit");
                }
            }

            transform.position = temp;
        }
	}

    Vector2 tempPos;
    void MoveVert(Transform t, float vel) {
        tempPos = t.position;
        tempPos.y += vel * Time.deltaTime;
        t.position = tempPos;
    }

    bool act = false;
    bool nextmode = false;
    void UpdateHalloween() {
        act = false;
        nextmode = false;
        if (Input.GetKeyDown(KeyCode.Space)) nextmode = true;
        if (MyInput.jump) act = true;
        if (hm == 0) {
            if (act) MoveVert(innocentTransform, 2f);
            if (nextmode) {
                hm = 1;
                dbox.playDialogue("halloween", 0);
            }
        } else if (hm == 1 && dbox.isDialogFinished()) {
            halloBGAnim.Play("open");
            AudioHelper.instance.playSFX("ladder_step_2");
            hm = 2;

        } else if (hm == 2) {
            if (act) MoveVert(manTransform, -1f);
            if (nextmode) {
                hm = 3;
                manAnim.Play("idle");
                dbox.playDialogue("halloween", 1);
            }
        } else if (hm == 3 && dbox.isDialogFinished()) {
            AudioHelper.instance.fadePitch("CCC", 2f, 0.1f);
            innocentAnim.GetComponent<SpriteRenderer>().enabled = false;
            innocent2Anim.GetComponent<SpriteRenderer>().enabled = true;
            innocent2Anim.Play("cry");
            hm = 4;
        } else if (hm == 4) {
            if (act) MoveVert(innocentTransform, -4);
            if (nextmode) {
                AudioHelper.instance.FadeSong("CCC",1,0);
                AudioHelper.instance.PlaySong("Pig",0,0);
                AudioHelper.instance.playSFX("sparkBarShatter");

                exclamationAnim.GetComponent<SpriteRenderer>().enabled = true;
                exclamationAnim.Play("on");
                manAnim.GetComponent<PositionShaker>().enabled = true;
                manAnim.Play("scared");
                innocent2Anim.Play("scary");
                hm = 6;
            }
        } else if (hm == 5) {
            if (nextmode) {
            }
        } else if (hm == 6) {
            if (act) {
                MoveVert(innocentTransform, 1.5f);
                MoveVert(mobTransform, 1.5f);
            }
            if (nextmode) {
                GameObject.Find("2D UI").GetComponent<UIManager2D>().StartFade(0, 1, 1);
                hm = 8;
            }
        } else if (hm == 7) {
        } else if (hm == 8) {
            if (act) {
                MoveVert(innocentTransform, 1.5f);
                MoveVert(mobTransform, 1.5f);
            }
            if (nextmode) {
                dbox.playDialogue("halloween", 2, 6);
                hm = 9;
            }

        } else if (hm == 9) {

        }
    }
}
