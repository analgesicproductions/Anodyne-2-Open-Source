using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Anodyne;
using UnityEngine.SceneManagement;
namespace Anodyne {
    public class Chest : MonoBehaviour {

        SpriteAnimator anim;
        public int YolkID = -1;
        public int CardID = -1;
        public bool has15Metacoin = false;
        public bool IsTrylockChest = false;
        public string keyname;
        bool isKeyChest = false;
        int mode = 0;
        UIManager2D ui;
        // Use this for initialization
        DialogueBox dbox;
        Vacuumable vac;
        string trylockFlag = "";
        string metacoinFlag = "";
        int trylockDialogueID = 0;
        int metaCoinValue = 0;

        void Start() {
            dbox = GameObject.Find("Dialogue").GetComponent<DialogueBox>();
            vac = GetComponent<Vacuumable>();
            ui = GameObject.Find("2D UI").GetComponent<UIManager2D>();
            GetComponent<Vacuumable>().DOESNTRESPAWN = true;
            anim = GetComponent<SpriteAnimator>();
            if (CardID != -1) {
                if (Ano2Stats.HasCard(CardID)) {
                    anim.Play("open");
                    mode = 1;
                }
            } else if (YolkID != -1) {
                //yolk1done
                // Exit1
                if (DataLoader.instance.getDS("yolk" + YolkID.ToString() + "done") == 1) {
                    anim.Play("open");
                    mode = 1;
                }
            } else if (IsTrylockChest) {
                trylockFlag = "trylock" + SceneManager.GetActiveScene().name + name;
                if (1 == DataLoader._getDS(trylockFlag)) {
                    anim.Play("open");
                    mode = 1;
                }
            } else if (has15Metacoin) {
                metaCoinValue = 10;
                metacoinFlag = "metacn" + SceneManager.GetActiveScene().name + name.Replace(' ', '_');
                if (1 == DataLoader._getDS(metacoinFlag)) {
                    anim.Play("open");
                    mode = 1;
                }
            } else if (keyname != "") {
                keyname = "Key"+SceneManager.GetActiveScene().name+keyname;
                keyname = keyname.Replace(" ", "");

                float s = DataLoader.instance.getDS(keyname);
                // 0 , closed - 1, opened, key alive - 2, opened, key used
                if (s != 0) {
                    mode = 1;
                    anim.Play("open");
                }
                isKeyChest = true;
                if (s == 1) {
                    SpawnKey();
                }
            }
        }

        void SpawnKey() {
            GameObject key = null;
            KeyAndKeyblock keyEnt = null;
            if (keyname == "KeyNanoSkeligumGoo") {
                key = (GameObject)Instantiate(Resources.Load("Prefabs/GumKey"), Vector3.zero, Quaternion.identity);
                keyEnt = key.GetComponentInChildren<KeyAndKeyblock>();
                keyEnt.isGooKey = true;
                keyEnt.SkeligumInit();
            } else if (keyname == "KeyNanoSkeligumSkel") {
                key = (GameObject)Instantiate(Resources.Load("Prefabs/SkeligumKey"), Vector3.zero, Quaternion.identity);
                keyEnt = key.GetComponentInChildren<KeyAndKeyblock>();
                keyEnt.isSkelKey = true;
                keyEnt.SkeligumInit();
            } else {
                key = (GameObject)Instantiate(Resources.Load("Prefabs/Key"), Vector3.zero, Quaternion.identity);
                keyEnt = key.GetComponentInChildren<KeyAndKeyblock>();
            }
            keyEnt.flagname = keyname;
            // Create prefab
            // set its flagname 
        }
        GameObject itemsprite;
        float tt = 0;
        float sucktime = 0.5f;
        void Update() {
            if (mode == 0) {
                if (YolkID == 1) {
                    if (dbox.isDialogFinished() && vac.IsBeingSucked()) {
                        sucktime -= Time.deltaTime;
                        if (sucktime < 0) {
                            sucktime = 0.5f;
                            dbox.playDialogue("chest-sucked");
                        }
                    } else {
                        sucktime = 0.5f;
                    }
                }
                if (onplayer && dbox.isDialogFinished() && MyInput.jpTalk) {
                    mode = 1;

                    ui.setTalkAvailableIconVisibility(false);
                    anim.Play("open");
                    AudioHelper.instance.playSFX("openChest");

                    if (isKeyChest) {
                        SpawnKey();

                        if (keyname == "KeyNanoSkeligumGoo") {
                            dbox.playDialogue("goo-key", 0);
                        }
                        if (keyname == "KeyNanoSkeligumSkel") {
                            dbox.playDialogue("skel-key", 0);
                        }
                        DataLoader.instance.setDS(keyname, 1);
                    } else if (IsTrylockChest) {
                        DataLoader._setDS(trylockFlag, 1);
                        bool doTrylockStuff = true;
                        if (!Ano2Stats.HasItem(Ano2Stats.ITEM_ID_TRYLOCK_GREEN)) {
                            Ano2Stats.GetItem(Ano2Stats.ITEM_ID_TRYLOCK_GREEN);
                            dbox.playDialogue("chest3d", 9);
                            trylockDialogueID = Ano2Stats.ITEM_ID_TRYLOCK_GREEN;
                        } else if (!Ano2Stats.HasItem(Ano2Stats.ITEM_ID_TRYLOCK_RED)) {
                            Ano2Stats.GetItem(Ano2Stats.ITEM_ID_TRYLOCK_RED);
                            dbox.playDialogue("chest3d", 7);
                            trylockDialogueID = Ano2Stats.ITEM_ID_TRYLOCK_RED;
                        } else if (!Ano2Stats.HasItem(Ano2Stats.ITEM_ID_TRYLOCK_BLUE)) {
                            Ano2Stats.GetItem(Ano2Stats.ITEM_ID_TRYLOCK_BLUE);
                            dbox.playDialogue("chest3d", 8);
                            trylockDialogueID = Ano2Stats.ITEM_ID_TRYLOCK_BLUE;
                        } else {
                            print("Got 10 Metacoins");
                            doTrylockStuff = false;
                            dbox.playDialogue("chest3d", 6);
                            SaveManager.metacoins += 10;
                            SaveManager.totalFoundCoins += 10;
                            if (SaveManager.totalFoundCoins >= 0) DataLoader.instance.unlockAchievement(DataLoader.achievement_id_COIN_1);
                            if (SaveManager.totalFoundCoins >= 200) DataLoader.instance.unlockAchievement(DataLoader.achievement_id_COIN_200);
                            if (SaveManager.totalFoundCoins >= 400) DataLoader.instance.unlockAchievement(DataLoader.achievement_id_COIN_400);
                            if (SaveManager.totalFoundCoins >= 500) DataLoader.instance.unlockAchievement(DataLoader.achievement_id_COIN_500);

                        }
                        if (doTrylockStuff) {
                            AudioHelper.instance.playSFX("trylockChest", false);
                            mode = 101;

                        } 
                    } else if (has15Metacoin) {
                        print("Got 10 Metacoins");
                        dbox.playDialogue("chest3d", 11);
                        SaveManager.metacoins += metaCoinValue;
                        SaveManager.totalFoundCoins += metaCoinValue;
                        DataLoader._setDS(metacoinFlag, 1);
                    } else if (YolkID != -1) {
                        tt = 0.5f;
                        AudioHelper.instance.FadeSong("nanoalb", 1f, 0.15f);
                        DataLoader.instance.setDS("yolk" + YolkID.ToString() + "done", 1);
                        dbox.playDialogue("yolkchest", YolkID - 1);
                        itemsprite = GameObject.Find("YolkItem" + YolkID.ToString());
                        itemsprite.GetComponent<SpriteRenderer>().enabled = true;
                        mode = 2;

                        if (YolkID == 1) Ano2Stats.GetItem(0);
                        if (YolkID == 2) Ano2Stats.GetItem(1);
                        if (YolkID == 3) Ano2Stats.GetItem(2);
                    } else if (CardID != -1) {
                        dbox.playDialogue("yolkchest", 3);
                        Ano2Stats.GetCard(CardID);
                        mode = 10;
                        itemsprite = transform.Find("Card").gameObject;
                        itemsprite.GetComponent<SpriteRenderer>().enabled = true;
                    }
                }
            } else if (mode == 1) {
                // done and open
            } else if (mode == 2) {

                tt -= Time.deltaTime;
                if (tt <= 0 && tt + Time.deltaTime > 0) {
                    AudioHelper.instance.playSFX("yolkChest",true,1,true);
                }

                if (dbox.isDialogFinished()) {
                    mode = 1;
                    SparkGameController.SparkGameDestObjectName = "Exit" + YolkID.ToString();
                    SparkGameController.SparkGameDestScene = Registry.GameScenes.Albumen;
                    DataLoader.instance.enterScene("none", Registry.GameScenes.Wormhole, 0.4f, 0.4f);
                    Wormhole.ReturningFrom2D = true;

                }
            } else if (mode == 10) {
                if (dbox.isDialogFinished()) {
                    mode = 11;
                    AudioHelper.instance.playOneShot("cardGet");
                }
            } else if (mode == 11) {


                tempPos = itemsprite.transform.position;
                tempPos.y += Time.deltaTime * 1.8f; itemsprite.transform.position = tempPos;

                tempScale = itemsprite.transform.localScale;
                tempScale.x += Time.deltaTime * 2; tempScale.y += Time.deltaTime * 2; itemsprite.transform.localScale = tempScale;

                tempRot = itemsprite.transform.localEulerAngles;
                tempRot.z += Time.deltaTime * 360f * 0.65f; itemsprite.transform.localEulerAngles = tempRot;

                tempCol = itemsprite.GetComponent<SpriteRenderer>().color;
                tempCol.a -= Time.deltaTime * 0.5f; itemsprite.GetComponent<SpriteRenderer>().color = tempCol;

                if (tempCol.a <= 0) {
                    mode = 1;
                }

            } else if (mode == 101) {
                if (dbox.isDialogFinished()) {
                    dbox.playDialogue("item-descriptions", trylockDialogueID);
                    mode = 1;
                } 
            }
        }

        public  bool IsOpen() {
            return mode == 1;
        }

        private void OnTriggerEnter2D(Collider2D collision) {
            if (mode == 1) return;
            if (collision.CompareTag("Player")) {
                ui.setTalkAvailableIconVisibility(true);
                onplayer = true;
            }
        }

        private void OnTriggerExit2D(Collider2D collision) {
            if (collision.CompareTag("Player")) {
                if (mode == 0) ui.setTalkAvailableIconVisibility(false);
                onplayer = false;
            }

        }
        bool onplayer = false;
        private Vector3 tempPos;
        private Vector3 tempScale;
        private Vector3 tempRot;
        private Color tempCol;
    }
}