using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thorn : MonoBehaviour {

    AnoControl2D player;
    Anodyne.SpriteAnimator anim;
	void Start () {
        HF.GetPlayer(ref player);
        anim = GetComponent<Anodyne.SpriteAnimator>();
	}

    public float tmFlickerWait = 2.5f;
    int mode = 0;
    float t_flickerWait = 0;
	void Update () {
		if (mode == 0) {

        } else if (mode == 1) {
            anim.Play("off");
            mode = 2;
        } else if (mode == 2) {
            t_flickerWait += Time.deltaTime;
            if (t_flickerWait > tmFlickerWait - 1f) {
                anim.Play("warning");
                mode = 30;
                return;
            }
        } else if (mode == 30) {
            if (anim.isPlaying == false) {
                anim.Play("on");
                GetComponent<BoxCollider2D>().enabled = true;
                mode = 0;
            }
        }
	}



    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.GetComponent<Anodyne.Spark2D>() != null && collision.GetComponent<Anodyne.Spark2D>().IsAlive()) {
            if (mode == 0) {
                AudioHelper.instance.playOneShot("thornBreak",0.75f,0.93f+0.14f*Random.value);
            }
            mode = 1;
            t_flickerWait = 0;
            GetComponent<BoxCollider2D>().enabled = false;
        }

        if (mode != 0) return;

        if (collision.GetComponent<AnoControl2D>() != null) {
            player.Damage(1);
            player.Bump(true, 5f);
            mode = 1;
            t_flickerWait = 0;
            GetComponent<BoxCollider2D>().enabled = false;
        } else if (collision.GetComponent<SlimeWanderer>() != null) {
            collision.GetComponent<SlimeWanderer>().Break();
        } else if (collision.GetComponent<Pew>() != null) {
            collision.GetComponent<Pew>().MyBreak();
        } else if (collision.GetComponent<Anodyne.SpikyChaser>() != null) {
            collision.GetComponent<Anodyne.SpikyChaser>().MyBreak();
        } else if (collision.GetComponent<Anodyne.Boomer>() != null) {
            collision.GetComponent<Anodyne.Boomer>().MaybeMyBreak();
        }
    }
}
