using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class UIManagerAno2 : MonoBehaviour {
	// Handles fading between scenes
	Image UI_FadeImage;
	static public bool FadeInOnNextSceneEnter = true;
	float t_ColorFade = 0f;
	public int fadeMode = 0;

    MediumControl player;
    PositionShaker SparkBarShaker;
    Image SparkBar;
    Image SparkBarBG;
    Color SparkBarColor = new Color();
    int modeSparkBar = 0;

    Image cruiseIconTopLayer;
    Image cruiseIconUnderLayer;
    Color cruiseIconColor = new Color(); // for color and alpha
    float tCruiseIconColor;
    float tCruiseIconScale;
    int cruiseIconMode = 0;
    Vector3 tempV = new Vector3(); // for scaling


    // Can be called from Pause Menu when changing res.
    public static void Update3DUIScale(float overrideHeight,float overrideWidth) {
        // print("Is fullscreen: " + Screen.fullScreen);
        int nextUIWidth = SaveManager.winResX;
        int nextUIHeight = SaveManager.winResY;

        if (overrideHeight != 0) nextUIHeight = (int) overrideHeight;
        if (overrideWidth != 0) nextUIWidth = (int) overrideWidth;

        if (overrideHeight == 0 &&  Screen.fullScreen) {
            overrideHeight = Screen.resolutions[Screen.resolutions.Length - 1].height;
            nextUIHeight = (int)overrideHeight;
        }
        if (overrideWidth == 0 && Screen.fullScreen) {
            overrideWidth = Screen.resolutions[Screen.resolutions.Length - 1].width;
            nextUIWidth= (int) overrideWidth;
        }

        float idealscale = UIManager2D.getIdealScaleValue(230f,overrideHeight);
        GameObject DialogueGO = GameObject.Find("Dialogue");
        GameObject Scalable3DUIPartsGO = GameObject.Find("DoesScale");

        // Scale the in-game 3D UI and the dialogue box.
        if (DialogueGO != null) {
            Vector3 tempPos = DialogueGO.transform.localScale;
            tempPos.Set(idealscale, idealscale, idealscale);
            DialogueGO.transform.localScale = tempPos;
            if (Scalable3DUIPartsGO != null) Scalable3DUIPartsGO.transform.localScale = tempPos;
            print("Dialogue and 3D UI scale updated to " + idealscale.ToString());
        }

        // Update canvas scaler
        CanvasScaler canvasScaler = GameObject.Find("UI").GetComponent<CanvasScaler>();
        UIManager2D.UpdateCanvasScalerSizeToDefault(canvasScaler,nextUIWidth, nextUIHeight);

        // Update Render texture
        RectTransform renderTex = GameObject.Find("RawImage").GetComponent<RectTransform>();


        if (nextUIWidth/nextUIHeight < 1.76f) {
            nextUIWidth = Mathf.FloorToInt((16f / 9f) * nextUIHeight);
            print("Scaling rendertexture for non 16:9 screen");
        }
        if (!Screen.fullScreen && nextUIHeight != 0 && nextUIWidth != 0) {
            renderTex.sizeDelta = new Vector2(nextUIWidth, nextUIHeight);
        } else {
            renderTex.sizeDelta = new Vector2(overrideWidth,overrideHeight);
        }
        print("Render texture updated to " + renderTex.sizeDelta);
    }

    void Start () {


        string a = SceneManager.GetActiveScene().name;

        if (a != "Title") {
            HF.GetPlayer(ref player);
            SparkBarBG = GameObject.Find("SparkBarBG").GetComponent<Image>();
            SparkBar = GameObject.Find("SparkBar").GetComponent<Image>();
            SparkBarShaker = SparkBarBG.GetComponent<PositionShaker>();

            SetAlpha(SparkBar, SparkBarColor, 0);
            SetAlpha(SparkBarBG, SparkBarColor, 0);

            cruiseIconTopLayer = GameObject.Find("CruiseIconTopLayer").GetComponent<Image>();
            cruiseIconUnderLayer = GameObject.Find("CruiseIcon").GetComponent<Image>();
            SetAlpha(cruiseIconTopLayer, cruiseIconColor, 0);
            SetAlpha(cruiseIconUnderLayer, cruiseIconColor, 0);

            talkImage = GameObject.Find("Talk Available Icon").GetComponent<Image>();
            talkText = GameObject.Find("Talk Available Text").GetComponent<TMP_Text>();
            talkColor = new Color();
            talkColor = talkImage.color; talkColor.a = 0; talkImage.color = talkColor;
            talkText.alpha = 0;
        }

        // Move the Dialogue object to the main UI hierarchy so it draws over the pause menu (without having to deal with two prefabs)
        GameObject DialogueGO = GameObject.Find("Dialogue");
        if (DialogueGO != null) {
            DialogueGO.transform.SetParent(transform.parent, true);
            if (a != "Title") DialogueGO.transform.SetAsLastSibling();
        }
        if (a != "Title") transform.SetAsFirstSibling();

        Update3DUIScale(0,0);


        transform.parent.GetComponent<Canvas>().worldCamera = GameObject.Find("UI Camera").GetComponent<Camera>();


		UI_FadeImage = GameObject.Find("UI_FadeImage").GetComponent<Image>();

        Color co = UI_FadeImage.color;
        co.a = 1;
        UI_FadeImage.color = co;

        if (FadeInOnNextSceneEnter) {
			FadeInOnNextSceneEnter = false;
            if (a != "Title") fadeMode = 2;
		}
        
        fadeHoldTime = 0.15f;
        if (sceneStartFadeHoldTime != -1) {
            fadeHoldTime = sceneStartFadeHoldTime;
            sceneStartFadeHoldTime = -1;
        }
	}
    public static float sceneStartFadeHoldTime = -1;
    // Can be set so fading  is paused for a bit (like on scene load when something needs to trigger before anything is visible)
    public static float fadeHoldTime = 0;

    public static int force3DUIRefreshTicks = 0;

    public void CancelSceneEntryFade() {
        fadeMode = 0;
    }

    float talkFadeT = 0;
    float talkFadeTM = 0.3f;
	void Update () {

        if (force3DUIRefreshTicks > 0) {
            force3DUIRefreshTicks--;
            if (force3DUIRefreshTicks == 0) {
                print("Save file had desired config - forcing UI refresh so that the correct Screen.fullscreen etc. state is read.");
                Update3DUIScale(0, 0);
            }
        }
        /*
        if (Input.GetKeyDown(KeyCode.Q)) {
            SaveManager.winResX = 1600; SaveManager.winResY = 900;
            Update3DUIScale(900);
        }*/

        updateSparkBar();
        updateRidescaleIcons();
        if (fadeMode != 0) {
			updateFading();
		}

        if (talkAvailableFadeMode == 1) {
            talkFadeT += Time.deltaTime;
            talkText.alpha = Mathf.Lerp(0, 1, talkFadeT / talkFadeTM);
            talkColor = talkImage.color; talkColor.a = Mathf.Lerp(0, 1, talkFadeT / talkFadeTM); talkImage.color = talkColor;
            if (talkFadeT > talkFadeTM) {
                talkFadeT = 0;
                talkAvailableFadeMode = 0;
            }
        } else if (talkAvailableFadeMode == 2) {
            talkFadeT += Time.deltaTime;
            talkText.alpha = Mathf.Lerp(1,0, talkFadeT / talkFadeTM);
            talkColor = talkImage.color; talkColor.a = Mathf.Lerp(1,0, talkFadeT / talkFadeTM); talkImage.color = talkColor;
            if (talkFadeT > talkFadeTM) {
                talkFadeT = 0;
                talkAvailableFadeMode = 0;
            }
        }
	}


    Vector2 tempSize = new Vector2();
    // cur and max are the health of the bar
    public void setSparkBarSize(float cur, float  max,bool noPauseAtEnd=false) {
        if (cur <= 0) cur = 0;
        if (cur >= max) cur = max;
        if (modeSparkBar >= 4) return;
        tempSize = SparkBar.rectTransform.sizeDelta;
        tempSize.x = (int)190f * (1 - (cur / max));
        SparkBar.rectTransform.sizeDelta = tempSize;
        // Fade in
        if (cur < max && modeSparkBar== 0) {
            modeSparkBar = 1;
        // Fade out
        } else if (cur == max && modeSparkBar == 2) {
            modeSparkBar = 3;
        // Ending animation + sound
        } else if (cur == 0 & modeSparkBar == 2) {
            if (DialogueAno2.AnyScriptIsParsing) return; // prevent same frame bugs
            setTalkAvailableIconVisibility(false);
            if (noPauseAtEnd) {
                modeSparkBar = 3;
            } else {
                AudioHelper.instance.playSFX("sparkBarShatter", false);
                player.pausedBySparkBar = true;
                modeSparkBar = 4;
            }
        }
        SparkBarShaker.amplitude.x = 3 * (1- (cur / max));
        SparkBarShaker.amplitude.y = 3 * (1-(cur / max));
    }

    private void updateSparkBar() {
        if (modeSparkBar == 0) {

        } else if (modeSparkBar == 1) {
            SparkBarColor = SparkBar.color;
            SparkBarColor.a += 4 * Time.deltaTime;
            SparkBar.color = SparkBarColor;
            SparkBarBG.color = SparkBarColor;
            if (SparkBarColor.a >= 1) {
                modeSparkBar = 2;
            }
        } else if (modeSparkBar == 2) {

        } else if (modeSparkBar == 3) {
            SparkBarColor = SparkBar.color;
            SparkBarColor.a -= 4 * Time.deltaTime;
            SparkBar.color = SparkBarColor;
            SparkBarBG.color = SparkBarColor;
            if (SparkBarColor.a <= 0) {
                modeSparkBar = 0;
                
                //SparkBarShaker.amplitude.Set(0, 0, 0);
            }
        } else if (modeSparkBar == 4) {
            SparkBarShaker.amplitude.Set(1,1,1);
            t_SparkDeg += Time.deltaTime;
            t_SparkDeg *= SparkMul;
            if (t_SparkDeg > tm_SparkDeg) {
                t_SparkDeg = tm_SparkDeg;
                modeSparkBar = 5;
                //AudioHelper.instance.playOneShot("sparkBarShrunk");
                AudioHelper.instance.playSFX("sparkBarShrunk");
                //t_SparkDeg = 0;
            }
            // Uses a rising of a sine wave to create that rubberband feel
            SparkDeg = 55 + (270 - 55f) * (t_SparkDeg / tm_SparkDeg);
            float v = (1 + Mathf.Sin(Mathf.Deg2Rad * SparkDeg)) / 2f;
            float newWidth = v * 200f * 1.1f;
            tempSize = SparkBarBG.rectTransform.sizeDelta;
            tempSize.x = newWidth;
            SparkBarBG.rectTransform.sizeDelta = tempSize;

            if (newWidth < 12) v = 0;
            newWidth = v * 190f*1.1f;
            tempSize = SparkBar.rectTransform.sizeDelta;
            tempSize.x = newWidth;
            SparkBar.rectTransform.sizeDelta = tempSize;
        } else if (modeSparkBar == 5 ) {

        }
    }
    float tm_SparkDeg = 7.5f;
    float SparkMul = 1.053f;
    float SparkDeg = 55;
    float t_SparkDeg = 0;
    public void MakeFadeUIWhite() {
        Color tempCol = Color.white;
        tempCol.a = 0;
        UI_FadeImage.color = tempCol;
    }

    public void MakeFadeUIBlack() {
        Color tempCol = Color.black;
        tempCol.a = 0;
        UI_FadeImage.color = tempCol;
    }
    public void FadeUIToBlack(float t) {
        UI_FadeImage.CrossFadeColor(Color.black, t, false, false);
    }

    public bool IsSparkBarVisible() {
        return (modeSparkBar != 0);
    }
    public bool IsSparkBarClosingAnimDone() {
        return modeSparkBar == 5;
    }

    Color green = new Color();
    Color baseCol = new Color();
    private void updateRidescaleIcons() {
        float d = Time.deltaTime;
        if (cruiseIconMode == 0) {
        } else if (cruiseIconMode == 1) {
            tCruiseIconScale += d; float x = 1.5f - 0.5f*(tCruiseIconScale / 0.3f); if (x < 1) x = 1; tempV.Set(x,x,x);
            cruiseIconUnderLayer.transform.localScale = tempV;
            SetAlpha(cruiseIconTopLayer, cruiseIconColor, tCruiseIconScale * 2);
            SetAlpha(cruiseIconUnderLayer, cruiseIconColor, tCruiseIconScale * 2);
            if (tCruiseIconScale > 0.3f) { cruiseIconMode = 2; }
        } else if (cruiseIconMode == 2) {
            float ft = 0.75f;
            tCruiseIconColor += Time.deltaTime; if (tCruiseIconColor > ft) tCruiseIconColor = ft;
            float f = Mathf.Cos(3.14f +  Mathf.Deg2Rad * 360 * (tCruiseIconColor* 10f / ft)); f = (f + 1)/2;
            cruiseIconColor = Color.Lerp(baseCol, green, f);
            cruiseIconTopLayer.color = cruiseIconColor;
            if (tCruiseIconColor >= ft) { cruiseIconMode = 3; }
        } else if (cruiseIconMode == 3) {
        }
    }
    public void TurnOnCruiseIcon() {
        cruiseIconMode = 1; tempV = cruiseIconTopLayer.transform.localScale; tempV.Set(1.5f, 1.5f, 1.5f);
        cruiseIconUnderLayer.transform.localScale = tempV; tCruiseIconScale = 0;
        ColorUtility.TryParseHtmlString("#42FF82", out green); baseCol = cruiseIconTopLayer.color;
        green.a = 1; baseCol.a = 1; tCruiseIconColor = 0;
    }
    public void TurnOffCruiseIcon() {
        cruiseIconMode = 0;
        cruiseIconTopLayer.color = baseCol;
        SetAlpha(cruiseIconTopLayer, cruiseIconColor, 0); SetAlpha(cruiseIconUnderLayer, cruiseIconColor, 0);
    }


    float defaultFadeTime = 1f;
    public float fadeTime = 1f;
    public float destAlpha = 1f;
	void updateFading() {
		if (fadeMode == 1) {
			Color c = UI_FadeImage.color;
			t_ColorFade += Time.deltaTime;

			c.a = Mathf.SmoothStep(0,destAlpha,t_ColorFade/fadeTime);

			UI_FadeImage.color = c;
			if (t_ColorFade >= fadeTime) {
				t_ColorFade = 0;
				fadeMode = 0;
                fadeTime = defaultFadeTime;
			}
		}
		if (fadeMode == 2) {
			Color c = UI_FadeImage.color;
			t_ColorFade += Time.deltaTime;
            if (fadeHoldTime > 0) {
                if (Time.deltaTime > 0.0167f) {
                    fadeHoldTime -= 0.0167f;
                } else {
                    fadeHoldTime -= Time.deltaTime;
                }
                t_ColorFade = 0;
            }
			c.a = Mathf.SmoothStep(destAlpha,0,t_ColorFade/fadeTime);

           
			UI_FadeImage.color = c;

            if (t_ColorFade >= fadeTime) {
				t_ColorFade = 0;
				fadeMode = 0;
                fadeTime = defaultFadeTime;
            }
		}
        if (fadeMode == 3) {
            // paused
        }
	}

    void SetAlpha(Image image, Color col, float alpha) {
        col = image.color;
        col.a = alpha;
        image.color = col;
    }

    // alpha is actually the starting alpha for fade-outs and ending alpha for fade-tos
    public void StartFade(float _fadeTime, bool fadeToColor = false, float alpha=-1) {
        if (alpha == -1) {
            destAlpha = 1;
        } else {
            destAlpha = alpha;
        }
            fadeTime = _fadeTime;
        t_ColorFade = 0;
        if (fadeToColor) {
            fadeMode = 1;
        } else {
            fadeMode = 2;
        }
    }

    public bool isFading() {
        return fadeMode != 0;
    }

    int talkAvailableFadeMode = 0;
    Image talkImage;
    TMP_Text talkText;
    Color talkColor;

    public void setTalkAvailableIconVisibility(bool turnOn,int line =0) {
        if (turnOn) {
            talkText.text = DataLoader.instance.getDialogLine("interaction", line);
            float talkTextW = talkText.preferredWidth;
            tempSize = talkImage.rectTransform.sizeDelta;
            tempSize.x = talkTextW + 16;
            talkImage.rectTransform.sizeDelta = tempSize;
            if (talkText.alpha == 1 || talkAvailableFadeMode == 1) return;
            talkAvailableFadeMode = 1;
            talkFadeT = 0;
        } else {
            if (talkText.alpha == 0 || talkAvailableFadeMode == 2) return;
            talkAvailableFadeMode = 2;
            talkFadeT = 0;
        }
    }

}
