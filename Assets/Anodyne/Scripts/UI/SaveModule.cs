using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

// Put this on the SaveModule UI element or something
// Controls the text and visual elements, arrows, etc. 
// Calls SaveManager's Save/Load functions.
// Calls DataLoader's level loading stuff.

public class SaveModule : MonoBehaviour {



    float confirmcursoralpha = 1f;

    public bool debugStart = false;
	static public bool saveMenuOpen = false;
	public bool inTitleScreen = false;


	RectTransform confirmationCursor;
	RectTransform cursor;
	RectTransform uiRectTransform;
	Vector3 initialCursorPosition;
	Vector3 initialConfirmPos;
	TMP_Text text;
	float spacing;
	int index = 0;
	int confirmIndex = 0;

	int mode = 0;
	int submode = 0;
	int MODE_MAIN = 0;
	int MODE_SAVE = 1;
	int MODE_LOAD = 2;
	int MODE_FADE_OUT = 3;
	int MODE_FADE_IN = 4;
	int MODE_INACTIVE = 5;
	int MODE_NEW = 6;
    int MODE_NEWSLETTER = 7;
    int MODE_SETTINGS = 8;
   // int MODE_FANPACK = 9;
    int MODE_TWITTER = 10;
    int MODE_DISCORD = 11;
    int MODE_CREDITS = 12;
    int MODE_QUIT = 13;
    int MODE_MANUAL = 14;
    int MODE_WALKTHROUGH = 15;
	public static int MAX_SAVES = 3;

	int[] mainChoiceArray;


    AudioSource audioSrc;
    AudioClip moveClip;
    AudioClip cancelClip;
    AudioClip selectClip;

    GameObject CreditsSprites;
    void Start () {

        if (GameObject.Find("CreditsSprites") != null) {
            CreditsSprites = GameObject.Find("CreditsSprites");
            CreditsSprites.transform.SetParent(GameObject.Find("Dialogue").transform, true);
            CreditsSprites.transform.localScale = new Vector3(1, 1, 1);
            CreditsSprites.SetActive(false);
        }


        string outputmixer = "Regular SFX";
        audioSrc = gameObject.AddComponent<AudioSource>();
        AudioMixer mixer = GameObject.Find("Track 1-1").GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer;
        audioSrc.outputAudioMixerGroup = mixer.FindMatchingGroups(outputmixer)[0];
        moveClip = Resources.Load("Audio/Sound/menuMoveTitle") as AudioClip;
        cancelClip = Resources.Load("Audio/Sound/menuMoveTitle") as AudioClip;

        selectClip = Resources.Load("Audio/Sound/menuMoveTitle") as AudioClip;


        cursor = GameObject.Find("SaveSelector").GetComponent<RectTransform>();
		confirmationCursor = GameObject.Find("ConfirmationSelector").GetComponent<RectTransform>();
        spacing = 11f;
        initialCursorPosition = cursor.transform.localPosition;
        initialConfirmPos = confirmationCursor.transform.localPosition;
        index = 1;
        newpos = cursor.transform.localPosition;
        newpos.y += spacing;
        cursor.transform.localPosition = newpos;


		text = GameObject.Find("SaveMenu_Text").GetComponent<TMP_Text>();
	
		loadMainMenuText();
		mainChoiceArray = new int[]{MODE_SAVE,MODE_LOAD,MODE_FADE_OUT};



		mode = MODE_INACTIVE;
		setAlpha0();
		if (debugStart) {
			Debug.Log("SaveModule set to debugstart, calling activate.");
			activate();	
		}

		if (inTitleScreen) {
			text.text = "";
			mainChoiceArray = new int[]{MODE_NEW,MODE_LOAD,MODE_SETTINGS, MODE_MANUAL, MODE_WALKTHROUGH, MODE_NEWSLETTER,MODE_DISCORD,MODE_TWITTER,MODE_CREDITS,MODE_QUIT};
		}
	}

	void loadMainMenuText() {
		
		string s = "";

		if (inTitleScreen) {
			s += DataLoader.instance.getRaw("savePoint",7) + "\n"; // new game, load game, settings, newsletter, discord, twitter, credits, quit
			s += DataLoader.instance.getRaw("savePoint",1) + "\n";
            s += DataLoader.instance.getRaw("savePoint", 10) + "\n";
            s += DataLoader.instance.getRaw("savePoint", 16) + "\n";
            s += DataLoader.instance.getRaw("savePoint", 17) + "\n";
            s += DataLoader.instance.getRaw("savePoint", 9) + "\n";
            s += DataLoader.instance.getRaw("savePoint", 12) + "\n";
            s += DataLoader.instance.getRaw("savePoint", 13) + "\n";
            s += DataLoader.instance.getRaw("savePoint", 14) + "\n";
            s += DataLoader.instance.getRaw("savePoint", 15);
        } else {
            s += DataLoader.instance.getRaw("savePoint",0) + "\n";
			s += DataLoader.instance.getRaw("savePoint",1) + "\n";
			s += DataLoader.instance.getRaw("savePoint",2) ;
		}
		text.text = s;
	}

	void loadSaveLoadMenuText(bool save=true,bool confirmation=false,bool newgame=false) {
		string s = "";
        if (newgame) {
            s += DataLoader.instance.getRaw("savePoint", 3) + "\n";
        }  else if (save) {
            s += DataLoader.instance.getRaw("savePoint", 0) + "\n";
        } else if (!save) {
            s += DataLoader.instance.getRaw("savePoint", 1) + "\n";
        } 
		s += DataLoader.instance.getRaw("savePoint",2) + "\n";
		s += SaveManager.getMetaData();



		if (confirmation) {
			s += "\n";
			s += DataLoader.instance.getRaw("savePoint",4) + "\n";
			s += DataLoader.instance.getRaw("savePoint",6) + "\n";
			s += DataLoader.instance.getRaw("savePoint",5);
		}
		text.text = s;
	}

	void setAlpha0() {
		Color _c = cursor.GetComponent<Image>().color;
		_c.a = 0;
		cursor.GetComponent<Image>().color = _c;
		_c = confirmationCursor.GetComponent<Image>().color;
		_c.a = 0;
		confirmationCursor.GetComponent<Image>().color = _c;
		text.alpha = 0;
	}

	// Update is called once per frame

	public bool fromTitle = false;
	public void activate(bool _fromTitle = false) {
		index = 1;
		mode = MODE_FADE_IN;
		this.fromTitle = _fromTitle;
        cursor.localPosition = initialCursorPosition;
        newpos = cursor.localPosition;
        newpos.y -= spacing;
        cursor.localPosition = newpos;
        setAlpha0();
		loadMainMenuText();
	}

	public bool isInactive() {
		return mode == MODE_INACTIVE;
	}


	void Update () {



        if (MODE_MAIN == mode) {
            newpos = cursor.localPosition;
            if (MyInput.jpUp) {
                if (index > 0) {
                    newpos.y += spacing;
                    index--;
                }
            } else if (MyInput.jpDown) {
                if (index < mainChoiceArray.Length - 1) {
                    newpos.y -= spacing;
                    index++;
                }
            }
            cursor.localPosition = newpos;

            if (MyInput.jpConfirm) {
                if (mainChoiceArray[index] == MODE_NEWSLETTER) {
                    //DataLoader.instance.unlockAchievement(0);
                    Application.OpenURL("https://www.analgesic.productions");
                    return;
                }

                if (mainChoiceArray[index] == MODE_DISCORD) {
                    Application.OpenURL("https://www.analgesic.productions/discord.html");
                    return;
                }

                if (mainChoiceArray[index] == MODE_MANUAL) {
                    Application.OpenURL("https://www.analgesic.productions/manual.html");
                    return;
                }

                if (mainChoiceArray[index] == MODE_WALKTHROUGH) {
                    Application.OpenURL("https://www.analgesic.productions/manual.html");
                    return;
                }

                if (mainChoiceArray[index] == MODE_TWITTER) {
                    Application.OpenURL("https://www.analgesic.productions/twitter.html");
                    return;
                }

                if (mainChoiceArray[index] == MODE_QUIT) {
                    Application.Quit();
                    mode = 123123;
                    return;
                }

                mode = mainChoiceArray[index];
                if (mode == MODE_SAVE) {
                    loadSaveLoadMenuText(true);
                } else if (mode == MODE_LOAD) {
                    loadSaveLoadMenuText(false);
                } else if (mode == MODE_NEW) {
                    loadSaveLoadMenuText(false, false, true);
                } else if (mode == MODE_CREDITS) {
                    submode = 0;
                }
                if (mode == MODE_SAVE || mode == MODE_LOAD || mode == MODE_NEW) {
                    index = 0;
                    submode = 0;
                }
                if (mode == MODE_SETTINGS) {
                    submode = 0;
                }
            }
            if (MyInput.jpCancel) {
                mode = MODE_FADE_OUT;
            }
        } else if (MODE_SAVE == mode || MODE_LOAD == mode) {
            updateSaveLoad();
        } else if (MODE_NEW == mode) {
            updateSaveLoad();
        } else if (MODE_CREDITS == mode) {
            updateCredits();
		} else if (MODE_FADE_IN == mode) {

			if (submode == 0) {
				saveMenuOpen = true;
				StartCoroutine(fade(0.5f));

				submode = 1;
			} 

		} else if (MODE_FADE_OUT == mode) {

			if (submode == 0) {
				saveMenuOpen = false;
				StartCoroutine(fade(0.8f,true));
				submode = 1;
			} 

		} else if (MODE_INACTIVE == mode) {
			
		} else if (MODE_SETTINGS == mode) {
            if (submode == 0) {
                pm = GameObject.Find("PauseMenu").GetComponent<PauseMenu>();
                pm.activate();
                submode = 1;
            }  else if (submode == 1) {
                if (pm.isActive() == false) {
                    loadMainMenuText();
                    submode = 0;
                    mode = MODE_MAIN;
                    cursor.transform.localPosition = initialCursorPosition;
                    newpos = cursor.transform.localPosition;
                    newpos.y -= 2 * spacing;
                    cursor.transform.localPosition = newpos;
                }
            }
        }

        if (MODE_INACTIVE != mode && MODE_SETTINGS != mode && MODE_CREDITS != mode && MODE_FADE_OUT != mode && MODE_FADE_IN != mode) {
            if (MyInput.jpUp || MyInput.jpDown || MyInput.jpLeft || MyInput.jpRight) {
                audioSrc.PlayOneShot(moveClip);
            }
            if (MyInput.jpCancel) {
                audioSrc.PlayOneShot(cancelClip);
            }
            if (MyInput.jpConfirm) {
                audioSrc.PlayOneShot(selectClip);
            }
        }

		return;



	}

    int creditsMode = 0;
    int creditsStringIdx = 0;
    TMP_Text creditsText = null;
    float t_credits;
    void updateCredits() {
        if (creditsMode == 0) {
            StartCoroutine(fadeOverlay(0.5f));
            creditsMode = 1;
            creditsStringIdx = 0;
        } else if (creditsMode ==1 && HF.TimerDefault(ref t_credits,0.5f)) {
            creditsMode = 2;
            if (creditsText == null) {
                creditsText = GameObject.Find("CreditsText").GetComponent<TMP_Text>();
                creditsText.alpha = 0;
            }
            // set text
        } else if (creditsMode == 2) {
            creditsText.text = DataLoader.instance.getDialogLine("credits", creditsStringIdx).Replace("\\n", "\n");
            creditsMode = 3;
        } else if (creditsMode == 3) {
            creditsText.alpha += 8f * Time.deltaTime;
            if (creditsText.alpha >= 1) {
                if (creditsStringIdx == 12) {
	                if (SaveManager.language == "en") {
		                CreditsSprites.SetActive(true);
	                }
                }
                creditsMode = 4;
            }
        } else if (creditsMode == 4) {
            if (MyInput.jpConfirm) {
                creditsMode = 5;
                if (creditsStringIdx == 12) {
                    CreditsSprites.SetActive(false);
                }
            } else if (MyInput.jpCancel) {
                AudioHelper.instance.playOneShot("menuClose");
                creditsMode = 6;
            }
        } else if (creditsMode == 5) {
            creditsText.alpha -= 8 * Time.deltaTime;
            if (creditsText.alpha <= 0) {
                creditsMode = 2;
                creditsStringIdx++;
                if (creditsStringIdx >= 15) {
                    AudioHelper.instance.playOneShot("menuClose");
                    creditsMode = 6;
                }
            }
        } else if (creditsMode == 6) {
            creditsText.alpha -= 8 * Time.deltaTime;
            if (creditsText.alpha <= 0) {
                StartCoroutine(fadeOutOverlay(0.5f));
                creditsMode = 7;
            }
        } else if (creditsMode == 7) {
            if (HF.TimerDefault(ref t_credits,0.55f)) {
                creditsMode = 0;
                mode = MODE_MAIN;
            }
        }
    }

    PauseMenu pm;
    Vector3 newpos = new Vector3();
	void updateSaveLoad() {

		if (submode == 0) {
			cursor.localPosition = initialCursorPosition;
			newpos = cursor.localPosition;
			newpos.y -= spacing; // LOAD, Back, [1], Name ... 
			if (MyInput.jpDown) {
				if (index < MAX_SAVES) {
					index++;
				}
			} else if (MyInput.jpUp) {
				if (index > 0) {
					index--;
				}
			}

			if (index > 0) {
				newpos.y -= spacing;
				newpos.y -= (index-1) * spacing*3;
			}
            cursor.localPosition = newpos;



			if (MyInput.jpCancel || (index == 0 && MyInput.jpConfirm))  {
				mode = MODE_MAIN;
				loadMainMenuText();
				index = 1;
				cursor.localPosition = initialCursorPosition - new Vector3(0,spacing,0);

				// Open  yes/no menu
			} else 	if (MyInput.jpConfirm) {
				submode = 1;
				Color _c = confirmationCursor.GetComponent<Image>().color; _c.a = confirmcursoralpha;
				confirmationCursor.GetComponent<Image>().color = _c;
				confirmationCursor.localPosition = initialConfirmPos;
				confirmIndex = 0;
                if (mode == MODE_LOAD) {
                    loadSaveLoadMenuText(false, true);
                } else if (mode == MODE_NEW) {
                    loadSaveLoadMenuText(false, true,true);
                } else {
					loadSaveLoadMenuText(true,true);
				}
			} 


            // pick yes or no
		} else if (submode == 1) {
			if (MyInput.jpUp && confirmIndex == 1) {
				confirmIndex --;
				newpos = confirmationCursor.localPosition;
				newpos.y += spacing;
				confirmationCursor.localPosition = newpos;
			} else if (MyInput.jpDown && confirmIndex == 0) {
				confirmIndex ++;
				newpos = confirmationCursor.localPosition;
				newpos.y -= spacing;
				confirmationCursor.localPosition = newpos;
			}

			if (MyInput.jpConfirm && confirmIndex == 1) {
				submode = 2;
			} else if (MyInput.jpCancel || (MyInput.jpConfirm && confirmIndex == 0)) {
				submode = 0;
				setImageAlphaZero(confirmationCursor.GetComponent<Image>());
                if (mode == MODE_LOAD) {
                    loadSaveLoadMenuText(false);
                } else if (mode == MODE_NEW) {
                    loadSaveLoadMenuText(false,false,true);
                } else {
					loadSaveLoadMenuText(true);
				}
			}
		} else if (submode == 2) {
			if (mode == MODE_LOAD) {
				print("Loading File"+(index-1).ToString());
				// If load fails or doesn't exist...
				if (SaveManager._Load(index-1,false)) {
					if (fromTitle) StartCoroutine(fadeOverlay(1.0f));
                    DataLoader.instance.resendAchievementStats();
                    DataLoader.instance.enterNextSceneBasedOnLoadedData();
                    SavePoint.currentInUseFileIndex = index - 1;
					submode = 3;
				} else {
					submode = 0;
					loadSaveLoadMenuText(false);
					setImageAlphaZero(confirmationCursor.GetComponent<Image>());
				}
			}
            if (mode == MODE_NEW) {
                Registry.enterGameFromLoad_SceneName = "Intro";

                if (TitleScreen.ringskipused) {
                    Registry.enterGameFromLoad_SceneName = "CCC";
                    print("Starting CH3 game on file " + (index - 1).ToString());
                } else if (TitleScreen.desertskipused) {
                    Registry.enterGameFromLoad_SceneName = "NanoHandfruitHaven";
                    print("Starting CH5 game on file " + (index - 1).ToString());
                } else {
                    print("Starting new game on file " + (index - 1).ToString());
                }

                DataLoader.instance.enterNextSceneBasedOnLoadedData();
                DataLoader.instance.resetGameData();

                if (TitleScreen.ringskipused) {
                    Registry.ProgressSkip(Registry.ProgressVal.RING_OPENED);
                    Registry.destinationDoorName = "ElevatorEntrance";
                    TitleScreen.ringskipused = false;
                } else if (TitleScreen.desertskipused) {
                    Registry.ProgressSkip(Registry.ProgressVal.finalhavenscene);
                    Registry.destinationDoorName = "BlowupEntrance";
                    TitleScreen.desertskipused = false;
                } else {
                    Registry.destinationDoorName = "Exit1";
                }
                Registry.set_startedNewGameButDidntSave(true);


                SavePoint.currentInUseFileIndex = index - 1;
                Registry.justLoaded = false;
                submode = 3;
            }


			if (mode == MODE_SAVE) {
				print("Saving "+(index-1).ToString());
				SaveManager._Save(index-1);
				submode = 0;
				loadSaveLoadMenuText(true);
				setImageAlphaZero(confirmationCursor.GetComponent<Image>());
			}



		} else if (submode == 3) {
			// Do nothing - loading a file so just wait.
            // (or new gaming)
		}

	}

	void setImageAlphaZero(Image img) {
		Color _c = img.color;
		_c.a = 0;
		img.color = _c;
	}
	IEnumerator fadeOverlay(float maxTime) {
		Image fader = GameObject.Find("UI_FadeImage").GetComponent<Image>();
		Color c = fader.color;
		float t = 0;
		while (t < maxTime) {
			t += Time.deltaTime;
			c.a = Mathf.SmoothStep(0,1,t/maxTime);
			fader.color = c;
			yield return new WaitForEndOfFrame();
		}
	}

    IEnumerator fadeOutOverlay(float maxTime) {
        Image fader = GameObject.Find("UI_FadeImage").GetComponent<Image>();
        Color c = fader.color;
        float t = 0;
        while (t < maxTime) {
            t += Time.deltaTime;
            c.a = Mathf.SmoothStep(1, 0, t / maxTime);
            fader.color = c;
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator fade(float maxTime,bool fadeOut = false) {

		float time = 0;

		Image img = cursor.GetComponent<Image>();
		Color c = img.color;
		float ss = 0;
		Image pauseOverlay = GameObject.Find("Pause Overlay") != null ? GameObject.Find("Pause Overlay").GetComponent<Image>() : null;
        if (fromTitle) pauseOverlay = null;
        Color pauseC = pauseOverlay == null ? new Color() : pauseOverlay.color;

		while (time < maxTime) {
			if (fadeOut) {
				ss = Mathf.SmoothStep(1,0,time/maxTime);	
			} else {
				ss = Mathf.SmoothStep(0,1,time/maxTime);
			}
			c.a = Mathf.Lerp(0, confirmcursoralpha, ss);
			img.color = c;
			text.alpha = Mathf.Lerp(0,1,ss);

			if (pauseOverlay != null ) {
				pauseC.a = Mathf.Lerp(0,0.6f,ss);
				pauseOverlay.color = pauseC;
			}

			time += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}

		if (fadeOut) {
			submode = 0;
			mode = MODE_INACTIVE;		
		} else {
			submode = 0;
			mode = MODE_MAIN;		
		}
	}
}
