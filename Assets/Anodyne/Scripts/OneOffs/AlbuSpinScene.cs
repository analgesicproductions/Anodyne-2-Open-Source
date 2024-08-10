using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Anodyne;
public class AlbuSpinScene : MonoBehaviour {


    Vacuumable v;
    AnoControl2D player;
    Vector3 campos;
    Vector3 playerpos;
	// Use this for initialization
	void Start () {
        v = GetComponent<Vacuumable>();
        player = GameObject.Find("2D Ano Player").GetComponent<AnoControl2D>();
	}

    int mode = -2;
    float timer = 0;
	// Update is called once per frame

    void ShakePlayer(float magnitude) {
        magnitude *= 4f;  
        player.transform.position = new Vector3(playerpos.x - magnitude + 2 * magnitude * Random.value, playerpos.y - magnitude + 2 * magnitude * Random.value, playerpos.z);
    }

	void Update () {
        if (mode == -2) {
            UIManager2D ui = GameObject.Find("2D UI").GetComponent<UIManager2D>();
            ui.StartFade(new Color(1, 1, 1), 0, 1, 0f);
            timer = 1f;
            mode = -1;
        } else if (mode == -1) {
            timer -= Time.deltaTime;
            if (timer < 0) {
                mode = 0;
                UIManager2D ui = GameObject.Find("2D UI").GetComponent<UIManager2D>();
                ui.StartFade(new Color(1, 1, 1), 1, 0, 0.5f);
            }
        } else if (mode == 0) {
            if (v.isPickedUp()) {
                timer = 0.5f;
                campos = Camera.main.transform.position;
                mode = 10;
            }
        } else if (mode == 10) {
            timer -= Time.deltaTime;
            if (timer < 0) {
                CutsceneManager.deactivatePlayer = true;
                playerpos = player.transform.position;
                mode = 1;
                timer = 1f;
            }
        } else if (mode == 1) {
            ShakePlayer(0.01f);
            timer -= Time.deltaTime;
            if (timer < 0) {
                player.GetComponent<Animator>().speed = 1;
                player.GetComponent<Animator>().enabled = true;
                player.GetComponent<Animator>().Play("suckSpinDie");
                timer = 2f;
                mode = 2;
            }
        } else if(mode == 2) {
            ShakePlayer(0.01f);
            player.GetComponent<Animator>().enabled = true;

            if (timer < 1.2f) {
                SpriteAnmRdmScrPos.init = true;
                SpriteAnmRdmScrPos.speedmul += Time.deltaTime;
            }

            timer -= Time.deltaTime;
            if (timer < 0) {
                mode = 3;
                UIManager2D ui = GameObject.Find("2D UI").GetComponent<UIManager2D>();
                ui.StartFade(new Color(1, 1, 1), 0, 1, 3f);
                timer = 0;
            }

        } else if(mode == 3) {
            SpriteAnmRdmScrPos.speedmul += Time.deltaTime;
            ShakePlayer(0.015f);
        } else if (mode == 4) {

        }
        if (mode > 2 && mode < 5) {
            timer += Time.deltaTime;
            float magnitude = timer / 150f;
            Camera.main.transform.position = new Vector3(campos.x - magnitude + 2 * magnitude * Random.value, campos.y - magnitude + 2 * magnitude * Random.value, campos.z);

        }

        // .6, 2.6, 4.6 (faded in)

    }
}
