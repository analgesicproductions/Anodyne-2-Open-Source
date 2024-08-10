using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsEnd : MonoBehaviour {

    int numberOfCreditsSegments = 15;
    float start_y = -160f;
   // float end_y = 360f;
    float spawnNextChunk_y = 75f;

    public float scrollSpeed = 48f;
    Vector3 tempPos;

    TMPro.TMP_Text text1;
    TMPro.TMP_Text text2;
    TMPro.TMP_Text text3;
    RectTransform CreditsSpritesParent;
    TMPro.TMP_Text[] texts;
    void Start() {
        text1 = GameObject.Find("credits1").GetComponent<TMPro.TMP_Text>();
        text2 = GameObject.Find("credits2").GetComponent<TMPro.TMP_Text>();
        text3 = GameObject.Find("credits3").GetComponent<TMPro.TMP_Text>();
        text1.text = text2.text = text3.text = "";
        texts = new TMPro.TMP_Text[] { text1, text2, text3 };
        CreditsSpritesParent = GameObject.Find("CreditsSpritesParent").GetComponent<RectTransform>(); // 12
        CreditsSpritesParent.gameObject.SetActive(false);
    }
    int mode = 0;
    bool spawnAChunk = false;
    int nextTMP_Index = 0;
    int nextLineIndex = 0;

    public void MyActivate() {
        mode = 1;
        AudioHelper.instance.PlaySong("Credits", 0, 0, true);
        spawnAChunk = true;
    }
    public bool IsFinished() {
        return mode == 0;
    }

    int spawnerIndex = 0;
    bool doLastWait = false;
    float t_doLastWait = 3;
    bool movingspritesactive = false;
	void Update () {
        if (mode == 0) {

        } else if (mode == 1) {
            if (spawnAChunk && nextLineIndex < numberOfCreditsSegments) {
                spawnAChunk = false;
                TMPro.TMP_Text next = null;
                if (nextTMP_Index == 0) next = text1;
                if (nextTMP_Index == 1) next = text2;
                if (nextTMP_Index == 2) next = text3;
                spawnerIndex = nextTMP_Index;
                nextTMP_Index++;
                if (nextTMP_Index > 2) nextTMP_Index = 0;
                next.text = DataLoader.instance.getRaw("credits", nextLineIndex).Replace("\\n", "\n");
                tempPos = next.rectTransform.anchoredPosition;
                tempPos.y = start_y;
                next.rectTransform.anchoredPosition = tempPos;
                if (nextLineIndex == 12) {
                    movingspritesactive = true;
                    if (SaveManager.language == "en") {
                        CreditsSpritesParent.gameObject.SetActive(true);
                    }

                    CreditsSpritesParent.anchoredPosition = tempPos;
                }
                nextLineIndex++;

            }

            if (movingspritesactive) {
                tempPos = CreditsSpritesParent.anchoredPosition;
                tempPos.y += scrollSpeed * Time.deltaTime;
                if (MyInput.confirm || MyInput.cancel || MyInput.special) {
                    tempPos.y += scrollSpeed * 2 * Time.deltaTime;
                    if (MyInput.shortcut) {
                        tempPos.y += scrollSpeed * 16 * Time.deltaTime;
                    }
                }
                CreditsSpritesParent.anchoredPosition = tempPos;
            }

            int idx = 0;
            foreach (TMPro.TMP_Text t in texts) {
                tempPos = t.rectTransform.anchoredPosition;
                tempPos.y += scrollSpeed * Time.deltaTime;
                if (MyInput.confirm || MyInput.cancel || MyInput.special) {
                    tempPos.y += scrollSpeed * 2 * Time.deltaTime;
                    if (MyInput.shortcut) {
                        tempPos.y += scrollSpeed * 16 * Time.deltaTime;
                    }
                }
                // Set when the chunk is spawned, makes it so that the most recent spawned chunk is only able to spawn the next
                if (spawnerIndex == idx && (tempPos.y >= spawnNextChunk_y + 20 || (tempPos.y >= spawnNextChunk_y && nextLineIndex == numberOfCreditsSegments))) {
                    // If this is the last chunk, make it stop
                    if (nextLineIndex == numberOfCreditsSegments) {
                        tempPos.y = spawnNextChunk_y;
                        doLastWait = true;
                    } else {
                        spawnAChunk = true;
                        spawnerIndex = -1;
                    }
                }

                t.rectTransform.anchoredPosition = tempPos;
                idx++;
            }

            if (doLastWait) {
                t_doLastWait -= Time.deltaTime;
                if (t_doLastWait <= 0) {
                    mode = 2;
                }
            }
            // set game save position to ringCCC, undo flags, set goodend flag. (need override in savemanager i think)
        } else if (mode == 2) {
            if (MyInput.jpConfirm || MyInput.jpCancel || MyInput.jpSpecial) {
                DataLoader._setDS(Registry.FLAG_SAW_GOODEND, 1);
                DataLoader.instance.unlockAchievement(DataLoader.achievement_id_GOODEND);
                Registry.ResetEndingFlags();
                SaveManager.forceCreditsData = true;
                SaveManager._Save(SavePoint.currentInUseFileIndex);
                mode = 3;
            }
        } else if (mode == 3) {
            text1.alpha -= Time.deltaTime;
            text2.alpha -= Time.deltaTime;
            text3.alpha -= Time.deltaTime;
            if (text1.alpha <= 0) {
                mode = 0;
            }
        }
    }
}
