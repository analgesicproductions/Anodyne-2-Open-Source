using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Used to make a yesno prompt made up of text with [Yes] [No] at the bottom
public class YesNoPrompt {

	TMP_Text text;
	Image cursor;
	string stringToDisplay;
	float spacing;
    Vector3 initcursorpos;
    Image boxBG;
    bool hasBoxBG = false;
    public string boxname = "TryAgainBox";

	public YesNoPrompt(string cursorName,string textName, string promptSceneName,int promptSceneIndex, string altYScene="", int altYindex=0, string altNScene="", int altNindex=0) {
		cursor = GameObject.Find(cursorName).GetComponent<Image>();
		text = GameObject.Find(textName).GetComponent<TMP_Text>();
        if ("2DGameYesNoCursor" == cursorName) {
            boxname = "2D Game YesNo";
        }
        if ("PauseCursor2" == cursorName) {
            boxname = "SettingsYN_BG";
        }
        if ("MetaCursor2" == cursorName) {
            boxname = "Meta_YN_BG";
            text.fontSize = 9.6f;
        }
        if (GameObject.Find(boxname) != null) {
            boxBG = GameObject.Find(boxname).GetComponent <Image>();
            hasBoxBG = true;
        }
        initcursorpos = text.rectTransform.anchoredPosition;
		stringToDisplay = DataLoader.instance.getRaw(promptSceneName, promptSceneIndex);
		if (SaveManager.language == "ru" && promptSceneIndex == 2 && promptSceneName == "pauseMenuWords") {
			stringToDisplay = stringToDisplay.Replace("\\n", "\n");
		} else if (SaveManager.language == "ru" && (promptSceneIndex == 6 || promptSceneIndex == 9) && promptSceneName == "metaclean-messages") {
			stringToDisplay = stringToDisplay.Replace("\\n", "\n");
		}
		Color c = cursor.color; c.a = 1; cursor.color = c;
		text.text = stringToDisplay;
		TMP_LineInfo  lineInfo = text.GetTextInfo(text.text).lineInfo[0];
		spacing = (lineInfo.lineHeight + text.lineSpacing);
        float yOff = spacing *Mathf.RoundToInt(text.preferredHeight / spacing);
        if ("2DGameYesNoCursor" == cursorName) {
            initcursorpos.x -= 2;
            if (SaveManager.language == "jp") {
	            if (SceneManager.GetActiveScene().name == "NanoHorror") {
		            initcursorpos.y -= 16f;
		            spacing -= 3f;
	            }
                initcursorpos.y += 3f;
            }
        }

        if ("TryAgainCursor" == cursorName) {
	        if (SaveManager.language == "zh-trad" || SaveManager.language == "zh-simp") {
		        initcursorpos.y += 3f;
		        yOff = text.preferredHeight ;
	        }

	        if (SaveManager.language == "jp") { 
		        initcursorpos.y += -5f;
		        yOff = text.preferredHeight; 
	        }
        }
        if ("MetaCursor2" == cursorName) {
            initcursorpos.x -= 4;
            initcursorpos.y -= 1.75f;
        }
        if ("PauseCursor2" == cursorName) {
            initcursorpos.x -= 4;
            initcursorpos.x -= 1.2f;
        }
        initcursorpos.y -= yOff + 4;
        
        
        cursor.rectTransform.anchoredPosition = initcursorpos;

        if (altNScene != "") {
            text.text += "\n" + DataLoader.instance.getRaw(altNScene, altNindex);
        } else {
            text.text += "\n" + DataLoader.instance.getRaw("savePoint", 6);
        }
        if (altYScene != "") {
            text.text += "\n" + DataLoader.instance.getRaw(altYScene,altYindex);
        } else {
            text.text += "\n" + DataLoader.instance.getRaw("savePoint", 5);
        }

        text.ForceMeshUpdate();
        if (hasBoxBG) {
            boxBG.enabled = true;
            boxBG.rectTransform.sizeDelta = new Vector2(text.renderedWidth+ 15f, text.preferredHeight + 12f);
        }

	}

    public void StartOnYes() {
        if (mode == 1) return;
        mode = 1;
        Vector3 cursorPos = cursor.rectTransform.localPosition;
        cursorPos.y -= spacing;
        cursor.rectTransform.localPosition = cursorPos;
    }

	int mode = 0;
	public int Update () {
		if (mode == 0) {
			if (MyInput.jpDown) {
                AudioHelper.instance.playOneShot("menuMove");
				mode = 1;
				Vector3 cursorPos = cursor.rectTransform.anchoredPosition;
				cursorPos.y -= spacing;
				cursor.rectTransform.anchoredPosition = cursorPos;
			}
		} else {

			if (MyInput.jpUp) {
				mode = 0;
                AudioHelper.instance.playOneShot("menuMove");
                Vector3 cursorPos = cursor.rectTransform.anchoredPosition;
				cursorPos.y += spacing;
				cursor.rectTransform.anchoredPosition = cursorPos;
            }
		}

        
		if (MyInput.jpCancel || MyInput.jpConfirm) {
			Color c = cursor.color; c.a = 0; cursor.color = c;
			text.text = "";
			text = null;
            if (MyInput.jpCancel) {
                if (mode == 1) {
                    Vector3 cursorPos = cursor.rectTransform.anchoredPosition;
                    cursorPos.y += spacing;
                    cursor.rectTransform.anchoredPosition = cursorPos;
                    mode = 0;
                }
            }
            if (MyInput.jpCancel) {
                AudioHelper.instance.playOneShot("menuCancel");
            } else {
                AudioHelper.instance.playOneShot("menuSelect");
            }
            //cursor.transform.localPosition = initcursorpos;
            cursor = null;

            if (hasBoxBG) boxBG.enabled = false;
            if (mode == 0) {// No 
				return 0;
			} else {
				return 1;
			}
		}
		return -1;
	}
}
