using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest3D : MonoBehaviour {

    public int itemID = -1;
    public int cardID = -1;
    public bool hasHealth = false;
    public int messageID = 0;
    UIManagerAno2 ui;
    DialogueBox dbox;
    Animator anim;
    public Renderer[] rends;
    public Material EggChestFade;
    public Material GlowEggFade;
	void Start () {
        ui = HF.Get3DUI();
        anim = transform.parent.Find("Model").GetComponent<Animator>();
        HF.GetDialogueBox(ref dbox);

        if (IsChestOpened()) {
            setAlpha(0);
            mode = 2;
            _a = 0;
            tm = 0;
            tmpop = -100;
            tmchest = -100;
        }
	}

    Color tempcol = new Color();
    void setAlpha(float a) {

        if (!changedMat) {
            changedMat = true;
            foreach (Renderer r in rends) {
                if (r.name == "EggChestTop") {
                    r.material = EggChestFade;
                } else if (r.name == "ChestYolk") {
                    r.material = GlowEggFade;
                }
            }
        }
        for (int i =0; i < rends.Length; i++) {
            tempcol= rends[i].material.color;
            tempcol.a = a;
            rends[i].material.SetColor("_Color", tempcol);
        }
    }
	
    bool IsChestOpened() {
        if (itemID != -1) {
            return Ano2Stats.HasItem(itemID);
        } else if (cardID != -1) {
            return Ano2Stats.HasCard(cardID);
        } else if (hasHealth) {
            return DataLoader._getDS("HEALTH" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name) == 1;
        }
        return false;
    }
    void GetChestContents() {
        if (itemID != -1) {
            Ano2Stats.GetItem(itemID);
        } else if (cardID != -1) {
            Ano2Stats.GetCard(cardID);
        } else if (hasHealth) {
            Ano2Stats.TryUpgradeHealth("HEALTH" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
    int mode = 0;
    bool OnPlayer = false;
    float _a = 1;
    float tm = 3.2f;
    float tmchest = 0.8f;
    float tmpop = 1.8f;
	void Update () {
		if (mode == 0) {
            if (OnPlayer && MyInput.jpTalk) {
                OnPlayer = false;
                mode = 1;

                ui.setTalkAvailableIconVisibility(false);
                GetChestContents();
                transform.parent.Find("Particles").GetComponent<ParticleSystem>().Play();

                AudioHelper.instance.playOneShot("chest3Dinteract");
                anim.Play("open");
            }
        }
        if (mode != 0) {
            tmchest -= Time.deltaTime;
            if (tmchest <= 0 && tmchest >= -1) {
                AudioHelper.instance.playOneShot("openChest");
                tmchest = -100f;
            }

            tmpop -= Time.deltaTime;
            if (tmpop <= 0 && tmpop >= -1) {
                dbox.playDialogue("chest3d", messageID);
                AudioHelper.instance.playOneShot("save3dglow");
                tmpop = -100f;
            }

            tm -= Time.deltaTime;
            if (_a > 0 && tm < 0) {
                anim.StopPlayback();
                _a -= Time.deltaTime;
                setAlpha(_a);
                if (_a < 0) {
                    setAlpha(0);
                    _a = 0;
                }
            }

        }
	}

    bool changedMat = false;
    private void OnTriggerEnter(Collider other) {
        if (other.name == "MediumPlayer" && mode == 0) {
            ui.setTalkAvailableIconVisibility(true, 4);
            OnPlayer = true;
        }
    }

    private void OnTriggerExit(Collider other) {
      if (other.name == "MediumPlayer") {
            ui.setTalkAvailableIconVisibility(false);
            OnPlayer = false;
        }
    }
}
