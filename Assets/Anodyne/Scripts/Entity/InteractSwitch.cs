using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Anodyne;

public class InteractSwitch : MonoBehaviour {

    public List<GameObject> children;
    public bool oneWay = true;
    public bool unsignalsFirst = false;
    bool on = false;
    SpriteAnimator anim;
    UIManager2D ui2d;
	void Start () {
        on = unsignalsFirst;
        anim = GetComponent<SpriteAnimator>();
        ui2d = GameObject.Find("2D UI").GetComponent<UIManager2D>();
        if (on) {
            anim.Play("on");
        } else {
            anim.Play("off");
        }
    }

    bool wasUsed = false;
    bool onPlayer;
    public bool isPigDarkSwitch = false;
	void Update () {
        if (wasUsed) {
            return;
        }
		if (onPlayer && (MyInput.jpConfirm || MyInput.jpTalk)) {
            onPlayer = false;
            string signal = "";
            if (on) {
                signal = "unsignal";
            }
            on = !on;
            if (isPigDarkSwitch) {
                AudioHelper.instance.playSFX("openChest");
                AudioHelper.instance.playSFX("nanobotCry",true,1,false,1.5f);
            }
            HF.SendSignal(children,signal);
            if (oneWay) {
                wasUsed = true;
            }
            if (on) {
                anim.Play("on");
            } else {
                anim.Play("off");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            onPlayer = true;
            ui2d.setTalkAvailableIconVisibility(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            onPlayer = false;
            ui2d.setTalkAvailableIconVisibility(false);
        }
    }
}
