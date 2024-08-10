using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FashionController : MonoBehaviour {

    DialogueBox dbox;
    GameObject top;
    GameObject bottom;
    GameObject hat;
    public Anodyne.SpriteAnimator anim;
	void Start () {
        HF.GetDialogueBox(ref dbox);
	}
    int mode = 0;
	void Update () {
        if (mode == 0) {

        } else if (mode == 1) {
            //let's begin! OR wait..
            if (top == null || bottom == null || hat == null) {
                dbox.playDialogue("orb-fab-done", 1);
                mode = 0;
            } else {
                mode = 2;
                dbox.playDialogue("orb-fab-done", 2, 4);
            }
        } else if (mode == 2 && dbox.isDialogFinished()) {
            string sentence = "";
            string hatType = hat.name.Split('_')[2];
            string topType = top.name.Split('_')[2];
            string bottomType = bottom.name.Split('_')[2];
            // Heroic, Basic, Cute, Professional, Goofy
            string[] a = new string[] { hatType, topType, bottomType };
            string[] b = new string[] { "hats", "tops", "bottoms" };
            int idx = 0;
            foreach (string s in a) {
                int sceneIdx = 0;
                if (s == "Heroic") {
                    sceneIdx = 0;
                    HeroicPoints++;
                }
                if (s == "Basic") {
                    sceneIdx = 1;
                    BasicPoints++;
                }
                if (s == "Cute") {
                    CutePoints++;
                    sceneIdx = 2;
                }
                if (s == "Professional") {
                    ProfPoints++;
                    sceneIdx = 3;
                }
                if (s == "Goofy") {
                    GoofyPoints++;
                    sceneIdx = 4;
                }
                sentence += DataLoader.instance.getRaw("orb-" + b[idx], sceneIdx);
                if (idx != 2) sentence += " ";
                idx++;
            }
            dbox.constructedColor = 2;
            dbox.playDialogue("fashionrating", -1, -1, sentence);
            mode = 3;
        } else if (mode == 3 && dbox.isDialogFinished()) {
            if (DataLoader._getDS("orb-fab-3-2") == 1) {
                if (HeroicPoints < 2) {
                    anim.Play("upset");
                    dbox.playDialogue("orb-fab-done", 10);
                } else {
                    anim.Play("happy");
                    dbox.playDialogue("orb-fab-done", 9);
                }
            } else if (DataLoader._getDS("orb-fab-2-2") == 1) {
                if (ProfPoints < 2) {
                    anim.Play("upset");
                    dbox.playDialogue("orb-fab-done", 8);
                } else {
                    anim.Play("happy");
                    dbox.playDialogue("orb-fab-done", 7);
                }
            } else {
                if (BasicPoints < 2) {
                    anim.Play("upset");
                    dbox.playDialogue("orb-fab-done", 5);
                } else {
                    anim.Play("happy");
                    dbox.playDialogue("orb-fab-done", 6);
                }
            }
            mode = 4;
        } else if (mode == 4 && dbox.isDialogFinished()) {
            if (BasicPoints == 3) dbox.playDialogue("orb-fullsets", 4);
            if (ProfPoints == 3) dbox.playDialogue("orb-fullsets", 1);
            if (HeroicPoints == 3) dbox.playDialogue("orb-fullsets", 2);
            if (GoofyPoints == 3) dbox.playDialogue("orb-fullsets", 3);
            if (CutePoints == 3) dbox.playDialogue("orb-fullsets", 0);
            if (BasicPoints == 3 || ProfPoints == 3 || HeroicPoints == 3 || GoofyPoints == 3 || BasicPoints == 3) {
                anim.Play("happy");
            }
        
            mode = 5;
        } else if (mode == 5 && dbox.isDialogFinished()) {

            anim.Play("idle");

            if (DataLoader._getDS("orb-fab-3-2") == 1) {
                DataLoader._setDS("orb-fab-3-2", 2);
                dbox.playDialogue("orb-fab-4", 0,1);
            } else if (DataLoader._getDS("orb-fab-2-2") == 1) {
                DataLoader._setDS("orb-fab-2-2", 2);
                dbox.playDialogue("orb-fab-3-1");
            } else {
                DataLoader._setDS("orb-fab-1-2", 2);
                dbox.playDialogue("orb-fab-2-1");
            }
            mode = 0;
        }
	}
    int BasicPoints = 0;
    int CutePoints = 0;
    int ProfPoints = 0;
    int GoofyPoints = 0;
    int HeroicPoints = 0;

    public void Begin() {
        mode = 1;
        BasicPoints = 0;
        CutePoints = 0;
        ProfPoints = 0;
        GoofyPoints = 0;
        HeroicPoints = 0;

    }

    public SpriteRenderer hatSR;
    public SpriteRenderer topSR;
    public SpriteRenderer bottomSR;

    private void OnTriggerEnter2D(Collider2D collision) {
        string thingName = collision.name;
        if (thingName.IndexOf("Clothes_") == -1) return;

        string type = thingName.Split('_')[1];
        if (type == "Hat") {
            if (hat != null) {
                hat.GetComponent<Anodyne.Vacuumable>().respawnImmediately = true;
            }
            hat = collision.gameObject;
            hatSR.sprite = collision.GetComponent<SpriteRenderer>().sprite;
            hat.GetComponent<Anodyne.Vacuumable>().Break();
            hat.GetComponent<Anodyne.Vacuumable>().respawnImmediately = false;
        } else if (type == "Top") {
            if (top != null) {
                top.GetComponent<Anodyne.Vacuumable>().respawnImmediately = true;
            }
            top = collision.gameObject;
            topSR.sprite = collision.GetComponent<SpriteRenderer>().sprite;
            top.GetComponent<Anodyne.Vacuumable>().Break();
            top.GetComponent<Anodyne.Vacuumable>().respawnImmediately = false;
        } else if (type == "Bottom") {
            if (bottom != null) {
                bottom.GetComponent<Anodyne.Vacuumable>().respawnImmediately = true;
            }
            bottom = collision.gameObject;
            bottomSR.sprite = collision.GetComponent<SpriteRenderer>().sprite;
            bottom.GetComponent<Anodyne.Vacuumable>().Break();
            bottom.GetComponent<Anodyne.Vacuumable>().respawnImmediately = false;
        }
    }

}
