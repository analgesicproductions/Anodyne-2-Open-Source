using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Anodyne;

public class NameCards : MonoBehaviour {

    public TMP_Text name1;
    public TMP_Text desc1;
    public TMP_Text name2;
    public TMP_Text desc2;
    public TMP_Text caption1;
    public SpriteAnimator imageAnimator;
    Image image;

    public bool isPalAndCP = false;
    public bool isCenter = false;
    public bool isZera = false;

    Color tempCol = new Color();

    void SetAlpha(Image img, float a) {
        tempCol = img.color;
        tempCol.a = a;
        img.color = tempCol;
    }

    int mode = 0;
    float t_fade = 0;

    private void Start() {
        isPalAndCP = true;
        isCenter = isZera = false;
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "CenterChamber") {
            isCenter = true;
            isPalAndCP = false;
            isZera = false;
        }

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "DesertShore") {
            isCenter = false;
            isPalAndCP = false;
            isZera = true;
        }
        name1.alpha = 0;
        desc1.alpha = 0;
        caption1.alpha = 0;
        image = imageAnimator.GetComponent<Image>();
        SetAlpha(image, 0);
        if (isPalAndCP || isZera) {
            name2.alpha = 0;
            desc2.alpha = 0;
        }

    }
    public void StartAnimating() {
        mode = 1;
    }
    public bool IsDone() {
        return mode == 0;
    }

    string openSFX = "menuOpenAno2";
    string scratchSFX = "pencilWrite";
    string eraseSFX = "pencilErase";
    int idx = 0;
    int charIdx = 0;
    string nextLine = "";
    string curDesc = "";
    bool showedPal = false;
	void Update () {

        bool useFastText = MyInput.shortcut || SaveManager.dialogueSkip;

        if (mode == 0) {
        } else if (mode == 1) {
            if (useFastText) {
                GameObject.Find("3D UI").GetComponent<UIManagerAno2>().StartFade(1, true, 0.05f);
            } else {
                if (isCenter) {
                    GameObject.Find("3D UI").GetComponent<UIManagerAno2>().StartFade(1, true, 1f);
                } else {
                    GameObject.Find("3D UI").GetComponent<UIManagerAno2>().StartFade(1, true, 0.75f);
                }
            }
            transform.localScale = GameObject.Find("Dialogue").transform.localScale;
            AudioHelper.instance.playOneShot(openSFX);
            if (isPalAndCP) {
                name1.text = DataLoader.instance.getRaw("pal-name-card", 0).Replace("\\n", "\n");
                name2.text = DataLoader.instance.getRaw("pal-name-card", 4).Replace("\\n", "\n");
                desc1.text = "";
                desc2.text = "";
                caption1.text = "";
                desc1.alpha = desc2.alpha = caption1.alpha = 1;
                curDesc = "";
            } else if (isZera) {
                name1.text = DataLoader.instance.getRaw("zera-name-card", 0).Replace("\\n", "\n");
                name2.text = DataLoader.instance.getRaw("zera-name-card", 4).Replace("\\n", "\n");
                desc1.text = "";
                desc2.text = "";
                caption1.text = "";
                desc1.alpha = desc2.alpha = caption1.alpha = 1;
                curDesc = "";
            } else if (isCenter) {
                name1.text = DataLoader.instance.getRaw("center-name-card", 0).Replace("\\n", "\n");
                desc1.text = "";
                caption1.text = "";
                curDesc = "";
                desc1.alpha = caption1.alpha = 1;
            }
            mode = 2;
            // Fade in image and name text
        } else if (mode == 2) {
            // cp image
            t_fade += Time.deltaTime;
            if (useFastText) t_fade = 1.01f;
            if (!showedPal) SetAlpha(image, t_fade);
            if (!showedPal) name1.alpha = t_fade;
            if (showedPal) name2.alpha = t_fade;
            if (t_fade > 1) {
                t_fade = 0;
                mode = 3;
                idx = -1;
            }
            //After pressing, get one line of the description
        } else if (mode == 3 && (MyInput.jpConfirm || useFastText)) {
            idx++;
            if (isPalAndCP) {
                if (showedPal) {
                    nextLine = DataLoader.instance.getRaw("pal-name-card", idx + 5);
                } else {
                    nextLine = DataLoader.instance.getRaw("pal-name-card", idx + 1);
                }
            } else if (isZera) {
                if (showedPal) {
                    nextLine = DataLoader.instance.getRaw("zera-name-card", idx + 5).Replace("\\n","\n");
                } else {
                    nextLine = DataLoader.instance.getRaw("zera-name-card", idx + 1).Replace("\\n","\n");
                }
            } else if (isCenter) {
                nextLine = DataLoader.instance.getRaw("center-name-card", idx + 1);
            }
            mode = 4;
            AudioHelper.instance.playOneShot(scratchSFX,0.35f,0.93f+0.14f*Random.value);
            charIdx = 0;
            // Add the line char by char
        } else if (mode == 4) {
            if (HF.TimerDefault(ref t_fade, 0.02f) || useFastText) {
                charIdx++;

                if (useFastText) {
                    charIdx = nextLine.Length;
                }

                if (!showedPal) desc1.text = curDesc + nextLine.Substring(0, charIdx);
                if (showedPal) desc2.text = curDesc + nextLine.Substring(0, charIdx);
                // When done:
                if (charIdx == nextLine.Length) {
                    if (!showedPal) curDesc = desc1.text + "\n";
                    if (showedPal) curDesc = desc2.text + "\n";
                    // Move on if showed 3 lines
                    if (idx == 2) {
                        // If pal and CP, show another description.
                        if (isPalAndCP && !showedPal) {
                            showedPal = true;
                            mode = 40;
                            curDesc = "";
                        } else if (isZera && !showedPal) {
                            showedPal = true;
                            mode = 40;
                            curDesc = "";
                        } else {
                            caption1.text = "";
                            if (isPalAndCP) nextLine = DataLoader.instance.getRaw("pal-name-card", 8);
                            if (isZera) nextLine = DataLoader.instance.getRaw("zera-name-card", 8);
                            if (isCenter) nextLine = DataLoader.instance.getRaw("center-name-card", 4);
                            charIdx = 0;
                            mode = 5;
                        }
                        // otherwise show next two lines
                    } else {
                        mode = 3;
                    }
                }
            }
        } else if (mode == 40 && (MyInput.jpConfirm || useFastText)) {
            mode = 2;
            AudioHelper.instance.playOneShot(openSFX);
            imageAnimator.Play("both");
            // show 'mommies'
        } else if (mode == 5 && (MyInput.jpConfirm || useFastText)) {
            mode = 6;
            AudioHelper.instance.playOneShot(scratchSFX, 0.35f, 0.93f + 0.14f * Random.value);
        } else if (mode == 6) {
            if (HF.TimerDefault(ref t_fade, 0.02f) || useFastText) {
                charIdx++;

                if (useFastText) {
                    charIdx = nextLine.Length;
                }
                caption1.text = nextLine.Substring(0, charIdx);
                if (charIdx == nextLine.Length) {
                    mode = 7;
                }
            }
            // erase
        } else if (mode == 7 && (MyInput.jpConfirm || useFastText)) {
            mode = 8;
            AudioHelper.instance.playOneShot(eraseSFX,0.35f);
        } else if (mode == 8) {
            if (HF.TimerDefault(ref t_fade, 0.05f) || useFastText) {
                charIdx--;
                if (useFastText) {
                    charIdx = 0;
                }
                caption1.text = nextLine.Substring(0, charIdx);
                if (charIdx == 0) {
                    mode = 9;
                    if (isPalAndCP) nextLine = DataLoader.instance.getRaw("pal-name-card", 9);
                    if (isZera) nextLine = DataLoader.instance.getRaw("zera-name-card", 9);
                    if (isCenter) nextLine = DataLoader.instance.getRaw("center-name-card", 5);
                }
            }
            // show 'caretakers and ...'
        } else if (mode == 9 && (MyInput.jpConfirm || useFastText)) {
            mode = 10;
            AudioHelper.instance.playOneShot(scratchSFX, 0.35f, 0.93f + 0.14f * Random.value);
        } else if (mode == 10) {
            if (HF.TimerDefault(ref t_fade, 0.02f) || useFastText) {
                charIdx++;
                caption1.text = nextLine.Substring(0, charIdx);
                if (charIdx == nextLine.Length) {
                    mode = 11;
                }
            }
        // Fade out
        } else if (mode == 11 && (MyInput.jpConfirm || useFastText)) {
            GameObject.Find("3D UI").GetComponent<UIManagerAno2>().StartFade(1, false, 0.75f);
            AudioHelper.instance.playOneShot("menuClose");
            mode = 12;
            t_fade = 1;
        } else if (mode == 12) {
            t_fade -= Time.deltaTime;
            if (useFastText) t_fade -= 4 * Time.deltaTime;
            name1.alpha = desc1.alpha = caption1.alpha = t_fade;
            if (isPalAndCP) name2.alpha = desc2.alpha = t_fade;
            if (isZera) name2.alpha = desc2.alpha = t_fade;
            SetAlpha(image, t_fade);
            if (t_fade <= 0) {
                mode = 0;
            }
        }
     }
}
