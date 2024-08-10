using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;
public class TitleScreen : MonoBehaviour {
	SaveModule sm;
	TMP_Text pressAnyKey;
	TMP_Text creatorText;
	TMP_Text presentsText;
	Image UI_Overlay;
    Image StudioLogo;
	Image GameLogo;
	TMP_Text version;
    public static bool ringskipused = false;
    public static bool desertskipused = false;

	void Start () {
        SaveManager.fullscreen = true;
		version = GameObject.Find("version").GetComponent<TMP_Text>();
		GameLogo = GameObject.Find("GameLogo").GetComponent<Image>();
		StudioLogo = GameObject.Find("StudioLogo").GetComponent<Image>();
         
		pressAnyKey = GameObject.Find("press any key").GetComponent<TMP_Text>();
		creatorText = GameObject.Find("creatortext").GetComponent<TMP_Text>();
		presentsText = GameObject.Find("presents").GetComponent<TMP_Text>();
		sm = GameObject.Find("SaveModule").GetComponent<SaveModule>();
        pressAnyKey.text = DataLoader.instance.getLine("controlLabels",8);
        //presentsText.text = DataLoader.instance.getLine("controlLabels",17);
		UI_Overlay = GameObject.Find("UI_FadeImage").GetComponent<Image>();
		SaveManager._Load(-1,true);
		SetAlpha(StudioLogo,1);
		SetAlpha(UI_Overlay,1);
		SetAlpha(GameLogo,0);
        creatorText.alpha = 0;
		version.alpha = 0;
		pressAnyKey.alpha = 0;
		mode = 0;

		if (SaveManager.language == "zh-simp" || SaveManager.language == "zh-trad") {
			ChangeGameLogoLanguage();
		}
        print(_version);
    }
    string _version = "Version 1.5.1 (Open Source)";
	void SetAlpha(Image i, float a) {
		Color c = i.color;
		c.a = a;
		i.color =c;
	}

	public static void ChangeGameLogoLanguage() {
		GameObject logo = GameObject.Find("GameLogo");
		if (logo != null) {
			Image logoImage = logo.GetComponent<Image>();
			if (SaveManager.language == "zh-simp") {
				logoImage.sprite =
					Resources.Load<Sprite>("Visual/Sprites/UI/Anodyne2Logo_title_zhSimp_hires");
			} else if (SaveManager.language == "zh-trad") {
				logoImage.sprite = Resources.Load<Sprite>("Visual/Sprites/UI/Anodyne2Logo_title_zhTrad_hires");
			} else {
				logoImage.sprite = Resources.Load<Sprite>("Visual/Sprites/UI/Anodyne2Logo_title_hires");
			}
		}
	}
	
	int mode = 0;
	int animMode = 0;
    bool playedStudioSFX = false;
    // Update is called once per frame
    float tSizeBugHack = 0;
    int sizeBugHackMode = 0;
	void Update () {
      //  if (Input.GetKeyDown(KeyCode.Escape)) {
        //    Application.Quit();
          //  print("Quit from title");
       // }

        if (Registry.DEV_MODE_ON) {
            if (Input.GetKey(KeyCode.Space) && Input.GetKeyDown(KeyCode.R)) {
                ringskipused = true;
                desertskipused = false;
                AudioHelper.instance.playOneShot("fant-fanfare");
            }

            if (Input.GetKey(KeyCode.Space) && Input.GetKeyDown(KeyCode.P)) {
                desertskipused = true;
                ringskipused = false;
                AudioHelper.instance.playOneShot("trylockChest");
            }
        }


        if (!Registry.CONSOLE_BUILD) {
            if (sizeBugHackMode == 0) {
                tSizeBugHack += Time.deltaTime;
                if (tSizeBugHack > 2f && (Screen.width < 100 || MyInput.shortcut)) {
                    print("force resizing bc screen too small");
                    SaveManager.fullscreen = false;
                    Screen.SetResolution(1280, 720, false);
                    sizeBugHackMode = 1;
                    tSizeBugHack = 0;
                } else if (tSizeBugHack > 2f) {
                    sizeBugHackMode = 2;
                    tSizeBugHack = 0;
                }

            } else if (sizeBugHackMode == 1) {
                tSizeBugHack += Time.deltaTime;
                if (tSizeBugHack > 0.8f) {
                    SaveManager.winResX = 1280;
                    SaveManager.winResY = 720;
                    UIManagerAno2.Update3DUIScale(720, 1280);
                    sizeBugHackMode = 2;
                    tSizeBugHack = 0;
                }
            } else if (sizeBugHackMode == 2) {
                if (MyInput.right) {
                    tSizeBugHack += Time.deltaTime;
                    if (tSizeBugHack > 5f) {
                        print("force resizing");
                        sizeBugHackMode = 1;
                        SaveManager.fullscreen = false;
                        Screen.SetResolution(1280, 720, false);
                        tSizeBugHack = 0;
                    }
                } else {
                    tSizeBugHack = 0;
                }
            }
        }


        if (mode == 0) {
			UpdateIntroAnimation();
		} else if (mode == 1) {
			Color c = pressAnyKey.color; c.a += Time.deltaTime*4.0f; c.a *= 1.05f;
			if (c.a >= 1) c.a = 1;
			pressAnyKey.color = c;
			if (MyInput.jpConfirm) {
				mode = 2;
				sm.activate(true);
			} 
		} else if (mode == 2) {
			Color c = pressAnyKey.color; c.a -= Time.deltaTime*4.0f; c.a *= 0.95f;
			pressAnyKey.color = c;
			if (c.a <= 0) mode = 3;
		} else if (mode == 3) {
			if (sm.isInactive()) {
				mode = 1;
			}
		} else if (mode == 4) { // Enter game, fade in black or pixelize or whatever
			Color c = UI_Overlay.color;
			c.a += Time.deltaTime*0.4f;
			c.a *= 1.02f;
			UI_Overlay.color = c;
		} else if (mode == 5) { // fade out after returning to title from game
			Color c = UI_Overlay.color;
            Color ac = GameLogo.color;   
            c.a -= Time.deltaTime*0.4f;
			c.a *= 0.98f;
			UI_Overlay.color = c;
            ac.a = c.a;
            GameLogo.color = ac;
			if (c.a <= 0) {
				mode = 1;
			}
		}
	}

	public void doExitFade() {
        PixelizePPE p = Camera.main.GetComponent<PixelizePPE>();
        p.Pixelize(1);
        AudioHelper.instance.StopSongByName("Title");
		mode = 4;
	}

	void setVersionPressKeyText() {

        string platformPart = "";
             
        version.text = DataLoader.instance.getRaw("controlLabels", 14) + " " + _version + " "+ platformPart;
        version.text = version.text.Replace("\\n", "\n");
		pressAnyKey.text = DataLoader.instance.getLine("controlLabels",8);
	}

    bool playedsong = false;

    void playsong() {
        AudioHelper.instance.PlaySong("Title", 0, 92.055f, false, 1f);
    }
	float animT;
	void UpdateIntroAnimation() {

		if (animMode == 1 || animMode == 2 || animMode == 3 || (animMode == 0 && animT > 0.5f)) {
			if (MyInput.jpConfirm || MyInput.jpPause || MyInput.jpCancel) {
				SetAlpha(StudioLogo,0);
				SetAlpha(UI_Overlay,0);
                SetAlpha(GameLogo, 1);
                if (!playedsong) {
                    playedsong = true;
                    playsong();
                }
                creatorText.alpha = presentsText.alpha = 0;
                version.alpha =  pressAnyKey.alpha = 1;
				setVersionPressKeyText();
				mode = 1;
				return;
			}
		}

		switch (animMode) {
		case 0:
            if (animT > 0.1f && playedStudioSFX == false) {
                playedStudioSFX = true;
                AudioHelper.instance.playSFX("AnalgesicSFX");
            }
			animT += Time.deltaTime;
			if (animT > 1.5f) {
                SetAlpha(StudioLogo,Mathf.Lerp(1,0,(animT-1.5f)/1.0f));
                presentsText.alpha = StudioLogo.color.a;
                creatorText.text = DataLoader.instance.getLine("controlLabels", 18).Replace("\\n", "\n");
                if (animT > 2.5f) {
                    if (!playedsong) {
                        playsong();
                        playedsong = true;
                    }
                    animT = 0;
                    animMode = 1;
                }
			}
			break;
		case 1:
			animT += Time.deltaTime;
            if (animT > 0 && animT <= 0.5f) {
                creatorText.alpha = animT/0.5f;
            }
			if (animT > 2) {
                creatorText.alpha = (3 - animT);
                if (animT >= 3) {
                    creatorText.alpha = 0;
                    animT = 0;
                    animMode = 2;
                    setVersionPressKeyText();
                }
			}
			break;
		case 2:
			animT += Time.deltaTime;
            
			if (animT > 2) {
                SetAlpha(UI_Overlay, 0);
				animT = 0;
				animMode = 3;
			} else {
                SetAlpha(UI_Overlay, Mathf.SmoothStep(1, 0, animT / 2.0f));
            }
			break;
		case 3:
                // Showing 3d stuff now, fade in slowly after 2 seconds
            animT += Time.deltaTime;
                float fadetime = 2.0f;
                float waittime = 2f;
            if (animT > waittime) {
				version.alpha = Mathf.Lerp(0,1,Mathf.SmoothStep(0,1,(animT-waittime)/fadetime));
               SetAlpha(GameLogo, Mathf.SmoothStep(0, 1, (animT - waittime) / fadetime));
              pressAnyKey.alpha = GameLogo.color.a;
                    version.alpha = pressAnyKey.alpha;

               if (animT > waittime+fadetime || MyInput.jpConfirm) {
                    animMode = 4;
                    mode = 1;
                    animT = 0;
                }
            }
			break;
		case 4:
			break;
		case 5:
			break;
		}
	}
}
