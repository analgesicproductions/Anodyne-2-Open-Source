using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter2D : MonoBehaviour {

    public Teleporter2D otherTeleporter;
    [System.NonSerialized]
    public ParticleSystem teleportParticles;
    [System.NonSerialized]
    public float tWait = 0;

    public bool requireInteract = false;
    int interactMode = 0;
    Color tempCol;
    int fadeMode = 0;
    float t_fade = 0;
    public float teleportWaitTime = 0.5f;
    public bool hasParticles = true;
    UIManager2D ui;
    SpriteRenderer playerSR;

	void Start () {
        if (hasParticles) {
            teleportParticles = GetComponent<ParticleSystem>();
        }
        ui = HF.Get2DUI();
	}
	
	void Update () {
		if (tWait > 0) {
            tWait -= Time.deltaTime;
        }
        if (interactMode == 1) {
            if (!onTrigger) {
                interactMode = 0;
                return;
            }
            if (teleDisable) {
                return;
            }
            if (MyInput.jpTalk || MyInput.jpConfirm) {
                interactMode = 2;
                ui.setTalkAvailableIconVisibility(false);
                AudioHelper.instance.playOneShot("enter_Door");
                AnoControl2D p = null;
                HF.GetPlayer(ref p);
                playerSR = p.GetComponent<SpriteRenderer>();

            }
        } else if (interactMode == 2) {
            if (fadeMode == 0) {
                tempCol = playerSR.color;
                tempCol.a -= 5 * Time.deltaTime;
                if (tempCol.a <= 0) tempCol.a = 0;
                playerSR.color = tempCol;
                if (tempCol.a <= 0) {
                    fadeMode = 1;
                    //otherTeleporter.tWait = teleportWaitTime;
                    otherTeleporter.teleDisable = true;
                    playerSR.transform.position = otherTeleporter.transform.position;
                    onTrigger = false;
                    t_fade = teleportWaitTime;

                }
            } else if (fadeMode == 1) {
                t_fade -= Time.deltaTime;
                playerSR.transform.position = otherTeleporter.transform.position;
                if (t_fade <= 0) {
                    tempCol = playerSR.color;
                    tempCol.a += 5 * Time.deltaTime;
                    if (tempCol.a >= 1) tempCol.a = 1;
                    playerSR.color = tempCol;
                    if (tempCol.a >= 1) {
                        fadeMode = 0;
                        interactMode = 0;
                        //ui.setTalkAvailableIconVisibility(true);
                        otherTeleporter.teleDisable = false;
                    }
                }
            }
        } 

	}

    private void OnTriggerEnter2D(Collider2D collision) {

        if (collision.name == "2D Ano Player") {
            onTrigger = true;
            if (requireInteract) {
                if (!teleDisable && tWait <= 0) ui.setTalkAvailableIconVisibility(true);
                interactMode = 1;
            } else {
                if (tWait > 0) {
                    return;
                }
                if (hasParticles) {
                    teleportParticles.Play();
                    otherTeleporter.teleportParticles.Play();
                    AudioHelper.instance.playOneShot("blockExplode");
                }
                collision.transform.position = otherTeleporter.transform.position;
                otherTeleporter.tWait = 0.25f;
            }
        }
    }
    bool onTrigger = false;
    [System.NonSerialized]
    public bool teleDisable = false;
    private void OnTriggerExit2D(Collider2D collision) {

        if (collision.name == "2D Ano Player") {
            onTrigger = false;
            if (requireInteract) {
                ui.setTalkAvailableIconVisibility(false);
            }
        }
    }

}
