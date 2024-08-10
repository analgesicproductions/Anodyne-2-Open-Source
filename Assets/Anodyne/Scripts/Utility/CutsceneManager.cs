using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class CutsceneManager : MonoBehaviour {


	// The parent to a bunch of transforms that can be used for lerping,
	// as well as bansheegz bgcurve
	Transform cutsceneInfo;
    [TextArea(6,12)]
	public string cutscene;
	int mode;
	int submode = 0;
	// Used in other camera scripts to stop player following, etc.
	public static bool deactivateCameras = false;
	public static bool deactivatePlayer = false;
	Vector3 initCamPos;
	Quaternion initCamRot;
	UIManagerAno2 UI3D;
    UIManager2D UI2D;
    DialogueBox dbox;


	List<List<string>> commands;
	int commandIndex = 0;

	Camera cam;
	bool isLerpingCam = false;
	float t_lerpCam;
	float tm_lerpCam;

	Vector3 startCamPos;
	Quaternion startCamRot;
	Vector3 destCamPos;
	Quaternion destCamRot;
	float startFOV;
	float destFOV;
	float initFOV;

	bool isWaiting = false;
	float t_wait = 0;
	float tm_wait = 0;
	float t_cutAfterTime = 0;

	bool isMovingObject = false;
	float t_moveObject = 0;
	float tm_moveObject = 0;
	Transform moveObjectTransform;
	Vector3 moveObjectTargetPos;
	Vector3 moveObjectInitPos;

	int activationType = 0;
	[Tooltip("Child this to a DialogCutTrig. When the player enters and interacts, the cutscene will start.")]
	public bool activatesViaExternal = false;
	public bool activatesViaKeypress = false;
	public bool activatesAutomatically = false;
	[Tooltip("Will still let the camera move - good for cutscenes on elevators.")]
	public bool dontDeactivateCameras = false;
	Transform playerT;

	bool isWaitingForDialog = false;
	bool skipLerpAfterD = false;
	bool isFadingInTitleCard = false;


	void Start () {
		cutsceneInfo = transform.Find("info");
		if (cutsceneInfo == null) {
			cutsceneInfo = transform;
		}
        dbox = GameObject.Find("Dialogue").GetComponent<DialogueBox>();
		if (GameObject.Find("2D Ano Player") != null) {
			playerT = GameObject.Find("2D Ano Player").GetComponent<Transform>();
        }
        if (GameObject.Find("MediumPlayer") != null) {
            playerT = GameObject.Find("MediumPlayer").GetComponent<Transform>();
        }
		mode = 0;

        int suppress = 0;
        if (GameObject.Find("2D UI") != null) {
            UI2D = GameObject.Find("2D UI").GetComponent<UIManager2D>();
            if (UI2D.name == "asdf" && suppress == 1) suppress = 0;
        }

        if (GameObject.Find("3D UI") != null) {
            UI3D = GameObject.Find("3D UI").GetComponent<UIManagerAno2>();
            if (UI3D.name == "asdf") suppress = 0;
        }

        if (activatesViaExternal) {
			activationType = 2;
		}
		if (activatesViaKeypress) {
			activationType = 1;
		}
		if (activatesAutomatically) {
			activationType = 3;
		}
	}


	public string mustBe0Dialog_1 = "";
	public string mustbe1Dialog_1 = "";
	public bool debugDontPlay =  false;
	[Tooltip("If the conditions don't hold, turn this object off on scene entry")]
	public bool makeGameObjNonexistantIfCutsceneOff = false;


	public void StartCutscene() {
		mode = 1;
	}

	public bool isCutsceneFinished() {
		if (mode == 4) return true;
		return false;
	}
	void Update () {

		if (mode == 0) {
			if (debugDontPlay) {
				return;
			}


			if (makeGameObjNonexistantIfCutsceneOff && DataLoader.instance.getDS(mustBe0Dialog_1) != 0) {
				this.gameObject.SetActive(false);
				return;
			}
			if (activationType == 0 || activationType == 1) {
				if (insidetrigger) {
					// Allow setting a dialog name in the inspector that must be 0 for the
					// cutscene to start.
					if (mustBe0Dialog_1 != "") {

						// Don't play cutscene if this variable is not 0.
						if (0 != DataLoader.instance.getDS(mustBe0Dialog_1)) {

						} else {
							// if activationType is 1 then require a keypress before starting cutscene
							if (activationType == 1) {
								if (submode == 0) {
									//OvUI.setStatusCubeReadyState(true);
									submode = 1;
								} else if (submode == 1) {
									if (MyInput.talk) {
										//OvUI.setStatusCubeReadyState(false);
										mode = 1;
										submode = 0;
									}
								}
							} else {
								mode = 1;
							}
						}
					} else {
						mode = 1;
					}
				} else {
					if (activationType == 1 && submode == 1) {
						//OvUI.setStatusCubeReadyState(false);
						submode = 0;
					}
				}

		// Waits for another script (usually DialogCutTrig) to start the cutscene
			} else if (activationType == 2) {
// autoplays if conditions true, no player needed
			} else if (activationType == 3) {
				if (DataLoader.instance.getDS(mustBe0Dialog_1) == 0) {
					if (mustbe1Dialog_1 != "") {
						if (DataLoader.instance.getDS(mustbe1Dialog_1) == 1) {
							mode = 1;
						}
					} else {
						mode = 1;
					}
				}
			} else {
				mode = 1;
			}

		} else if (mode == 1) {
            commands = getCommands(cutscene);
			mode = 150;

			deactivatePlayer = true; // turn off player first - wait until it stops, then turn off cameras
									// so that the initial position is correct
		} else if (mode == 150) {
			if (playerT == null || HF.PlayerHasZeroVelocity() || activatesAutomatically) {
				mode = 2;
				cam = Camera.main; // Just a default for now, so can be changed later if needed
				initCamPos = cam.transform.position;
				initCamRot = cam.transform.rotation;
				initFOV = cam.fieldOfView;
				if (dontDeactivateCameras == false) deactivateCameras = true;
			}
		} else if (mode == 2) {

			if (isWaiting) {
				t_wait += Time.deltaTime;
				if (t_wait >= tm_wait) {
					t_wait = 0;
					isWaiting = false;
				}
			}

			if (isFadingInTitleCard) {
				return;
			}

			if (isLerpingCam) {
				t_lerpCam += Time.deltaTime;
				if (t_lerpCam >= tm_lerpCam) {
					isLerpingCam = false;
				}
				if (t_cutAfterTime > 0 && t_lerpCam >= t_cutAfterTime) {
					isLerpingCam = false;
				}
				float _time = Mathf.SmoothStep(0.0f,1.0f,t_lerpCam/tm_lerpCam);
				cam.transform.position = Vector3.Lerp(startCamPos,destCamPos,_time);
				cam.transform.rotation = Quaternion.Slerp(startCamRot,destCamRot,_time);
				cam.fieldOfView = Mathf.Lerp(startFOV,destFOV,_time);

			}

			if (isWaitingForDialog) {
                if (dbox.isDialogFinished()) {
                     isWaitingForDialog = false;
					// Set with "skipLerpAfterD" cutscene command.
					// Stops the current camera lerp, so that the next command can be played.
					// Usually just another camera movement.
					if (skipLerpAfterD) {
						skipLerpAfterD = false;
						isLerpingCam = false;
					}
				}
			}

			if (isMovingObject) {
				t_moveObject += Time.deltaTime;
				Vector3 newpos = Vector3.Lerp(moveObjectInitPos,moveObjectTargetPos,t_moveObject/tm_moveObject);
				moveObjectTransform.position = newpos;
				if (t_moveObject >= tm_moveObject) {
					t_moveObject = 0;
					isMovingObject = false;
				}
			}

			if (!isLerpingCam && !isWaiting && !isWaitingForDialog  && !isMovingObject) {

				if (commandIndex == commands.Count) {
					mode = 3;
				} else {
					bool _continue = true;
					while (_continue) {
						_continue = parseNextCommand(commands[commandIndex]);
						commandIndex++;
						if (commandIndex == commands.Count) {
							break;
						}
					}
				}
			}
		// Finalize 
		} else if (mode == 3) {
            
            print("Cams off");
			deactivateCameras = false;
			deactivatePlayer = false;
			mode = 4;
		// final
		} else if (mode == 4) {
			
		}
	}




	bool skipping = false;

	// Returns false if next command shouldn't be parsed (blocking command)
	// Or true if next command should be parsed (nonblocking command)
	bool parseNextCommand(List<string> c) {

		string aa = "";
		foreach (string p in c) {
			aa += p + " ";
		}
		bool retval = false;
		Transform t;

		// Use skip/unskip to 'comment out' parts of the script
		switch (c[0]) {
		case "skip":
			skipping = true;
			break;
		case "unskip":
			skipping = false;
			break;
		}

		if (skipping) return retval;

		switch (c[0]) {


		case "enableComponent":
			bool ena = false;
			if (int.Parse(c[3]) == 1) ena = true;
			if (c[2] == "PositionShaker") GameObject.Find(c[1]).GetComponent<PositionShaker>().enabled = ena;
            if (c[2] == "Oscillate") GameObject.Find(c[1]).GetComponent<Oscillate>().enabled = ena;
			if (c[2] == "ObjectRotator") GameObject.Find(c[1]).GetComponent<ObjectRotator>().enabled = ena;
			if (c[2] == "ParticleSystem") GameObject.Find(c[1]).GetComponent<ParticleSystem>().Play();
            if (c[2] == "ChangeChildrenAlpha") GameObject.Find(c[1]).GetComponent<ChangeChildrenAlpha>().StartFade();
                break;
		case "fadePitch":
			AudioHelper aa3 = GameObject.Find("AudioHelper").GetComponent<AudioHelper>();
			aa3.fadePitch(c[1],safeFloatParse(c[2]),safeFloatParse(c[3]));
			break;
		case "fadeSongOut":
			AudioHelper aa2 = GameObject.Find("AudioHelper").GetComponent<AudioHelper>();
			aa2.StopSongByName(c[1]);
			break;
		case "playSong":
			AudioHelper aaa= GameObject.Find("AudioHelper").GetComponent<AudioHelper>();
			aaa.PlaySong(c[1],safeFloatParse(c[2]),safeFloatParse(c[3]));
			break;
		case "playSongNoFade":
			AudioHelper a = GameObject.Find("AudioHelper").GetComponent<AudioHelper>();
			a.PlaySongNoFade(1,c[1]);
			break;
		// titlecard 1,2,3 etc - these four play before a scene transition, so only need
			// a fade-in coroutine, then the cutscene should call map NEXTMAP.
		case "titlecard":
			isFadingInTitleCard = true;
			StartCoroutine(fadeInTitleCard(int.Parse(c[1])));
			break;
		case "setInactive":
			GameObject go1 = GameObject.Find(c[1]);
			go1.SetActive(false);
			break;
		case "setActive":
			GameObject go2 = GameObject.Find(c[1]);
			go2.SetActive(true);
			break;
		case "setActiveDoor":
			GameObject go2d = GameObject.Find(c[1]);
			go2d.GetComponent<Anodyne.Door>().enabled = true;
			break;
			// map SCENENAME  DOORNAME DOFADE(0/1, default 1.)
			// DOFADE = use this if defaultFadeIn was used already (so the screen doesn't re-fade)
		case "map":
            DataLoader.instance.enterScene(c[2], c[1]);
			break;
        // say SCENENAME 0/1 (0 for wait to proceed)
        case "say":
            DialogueBox box = GameObject.Find("Dialogue").GetComponent<DialogueBox>();
            box.playDialogue(c[1]);
            isWaitingForDialog = true;
            if (c.Count > 2 && c[2] == "0") isWaitingForDialog = false;
            break;
        // d DIALOG_NAME
        case "d":
			string[] linesToPush = DataLoader.instance.getDialogLines(c[1]);
			int _i = 0;
			foreach (string line in linesToPush) {
				//OvUI.pushDialog(line,DataLoader.instance.lastDialogTags[_i]);
				_i++;
			}
			break;
			// setState DIALOG_NAME NEW_VALUE
		case "skipLerpAfterD":
			skipLerpAfterD = true;
			break;
		case "setState":
			DataLoader.instance.setDS(c[1],int.Parse(c[2]));
//			print("Set dialog "+c[1]+" to new value: "+c[2]);
			break;
			// dw
			// dialogue wait
		case "dw":
			isWaitingForDialog = true;
			break;
			// lerp CAM_ID TIME CUT_AFTER_TIME NO_BLOCK(=0/1, 0 by def)
			// If cut_afteR_timei s provided, camera will cut after that time (so you can move camera while cutting)
		case "lerp":

			if (c[1] == "0") {
				destCamPos = initCamPos;
				destCamRot = initCamRot;
				destFOV = initFOV;
			} else {
				string destCamName = "cam"+c[1];
				t = cutsceneInfo.Find(destCamName);
				destCamPos = t.position;
				destCamRot = t.rotation;

				if (null != t.GetComponent<Camera>()) { 
					destFOV = t.GetComponent<Camera>().fieldOfView;
				} else {
					destFOV = initFOV;
				}
			}

			startCamPos = cam.transform.position;
			startCamRot = cam.transform.rotation;
			startFOV = cam.fieldOfView;

			t_lerpCam = 0;
			tm_lerpCam = safeFloatParse(c[2]);
			isLerpingCam = true;

			t_cutAfterTime = 0;
			if (c.Count > 3 && c[3] != "0") {
				t_cutAfterTime = safeFloatParse(c[3]);
			}
			// Prevents this from blocking - so that you can put a dw after, and then a lerp cancel or something.
			if (c.Count > 4 && c[4] == "1") {
				retval = true;
			}

			break;

		// wait TIME
		case "wait":
			tm_wait = safeFloatParse(c[1]);
			isWaiting = true;
			break;

		// cam CAM_ID
		// Non-blocking
		// Snaps the camera to this camera immediately.
		// Cancels any existing camera LERP.
		case "cam":
			
			isLerpingCam = false;
			if (c[1] == "0") {
				cam.transform.position = initCamPos;
				cam.transform.rotation = initCamRot;
				cam.fieldOfView = initFOV;
			} else {
				t = cutsceneInfo.Find("cam"+c[1]);
				cam.transform.position = t.position;
				cam.transform.rotation = t.rotation;
				cam.fieldOfView = t.GetComponent<Camera>().fieldOfView;
			}

			retval = true;
			break;
			// moveObject OBJECT_NAME x y z LERP_TIME
		case "moveObject":
			GameObject g = GameObject.Find(c[1]);
			if (g != null) {
				isMovingObject = true;
				moveObjectTransform = g.transform;
				moveObjectTargetPos = new Vector3(safeFloatParse(c[2]),safeFloatParse(c[3]),safeFloatParse(c[4]));
				moveObjectInitPos = moveObjectTransform.position;
				tm_moveObject = safeFloatParse(c[5]);
			} 
			break;
			// moveObjectAsync OBJECT_NAME x y z LERP_TIME
		case "moveObjectAsync":
			GameObject gmv = GameObject.Find(c[1]);
			if (gmv != null) {
				StartCoroutine(moveObject(gmv.transform,new Vector3(safeFloatParse(c[2]),safeFloatParse(c[3]),safeFloatParse(c[4])),safeFloatParse(c[5])));
			} 
			break;
        case "fogColor":
                Color fogC = new Color();
                ColorUtility.TryParseHtmlString(c[1],out fogC);
                RenderSettings.fogColor = fogC;
                break;
		// fogRange blocking[0/1] [near] [far] [time]
		case "fogRange":
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.Linear;
                RenderSettings.fogStartDistance = safeFloatParse(c[2]);
                RenderSettings.fogEndDistance = safeFloatParse(c[3]);
			break;
        // fogDensity [density] [time]
        case "fogDensity":
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.Exponential;
			StartCoroutine(fogDensity(safeFloatParse(c[1]),safeFloatParse(c[2])));
			break;
        case "pausefadeout":
            //UIManager ui3 = GameObject.Find("Overworld UI").GetComponent<UIManager>();

            //if (ui3.fadeMode == 3) {
              //  ui3.fadeMode = 2;
            //} else {
             //   ui3.fadeMode = 3;
           // }
                break;
            // Usually use this when needing to transition to a new map to hide the camera returning to the player beofre scene change
        case  "defaultFadeIn":
//			UIManager ui2 = GameObject.Find("Overworld UI").GetComponent<UIManager>();

			AudioHelper.instance.playSFX("door_open",false);
	//		ui2.fadeMode = 1;
	//		ui2.setStatusCubeReadyState(false);
			break;
			// fadeOut [time]
		case "fadeOut":
            StartCoroutine(fadeLayer("Under Dialogue Fade Layer", 0, safeFloatParse(c[1])));
            break;
		// fadeIn [time]
		case "fadeIn":
            StartCoroutine(fadeLayer("Under Dialogue Fade Layer", 1, safeFloatParse(c[1])));
			break;
		// fadeSkinAlpha destAlpha fadeTime objName
		case "fadeSkinAlpha":
			StartCoroutine(fadeMeshAlpha(safeFloatParse(c[1]),safeFloatParse(c[2]),true,c[3]));
			break;
		case "fadeMeshAlpha":
			StartCoroutine(fadeMeshAlpha(safeFloatParse(c[1]),safeFloatParse(c[2]),false,c[3]));
			break;
		// hexcodeColor time
		case "ambientColor":
			StartCoroutine(ambienceColor(c[1],safeFloatParse(c[2]),0));
			break;
        case "bgColor":
            StartCoroutine(ambienceColor(c[1], safeFloatParse(c[2]), 1));
            break;
		// name intensity time
		case "lightIntensity":
			StartCoroutine(lightIntensity(c[1],safeFloatParse(c[2]),safeFloatParse(c[3])));
			break;
			// toggleLight NAME 0/1 (off/on)
		case "toggleLight":
			GameObject toggleLight = GameObject.Find(c[1]);
			toggleLight.GetComponent<Light>().enabled = c[2] == "0" ? false : true;
			break;
		}


		return retval;
	}


	IEnumerator fadeInTitleCard(int titleID) {
		string[] lines = DataLoader.instance.getDialogLines("actTitles");
		string partString = lines[titleID-1].Replace("\\n","\n");
		string title = lines[titleID+3].Replace("\\n","\n");


		Image BG = GameObject.Find("Title Card BG").GetComponent<Image>();
		Image Overlay = GameObject.Find("Title Card Overlay").GetComponent<Image>(); // 104/255 alpha. COLOR!!!
		// colorS: A75C5068
		TMP_Text AOA = GameObject.Find("Title Card AOA").GetComponent<TMP_Text>();
		TMP_Text Title = GameObject.Find("Title Card Title").GetComponent<TMP_Text>();
			
		BG.enabled = true;
		Overlay.enabled = true;
		AOA.enabled = true;
		Title.enabled = true;

		Color overlayColor;

		if (titleID == 1) { ColorUtility.TryParseHtmlString("#A75C50",out overlayColor); }
		else {ColorUtility.TryParseHtmlString("#A75C50",out overlayColor);  }

		overlayColor.a = 0;

		Overlay.color = overlayColor;
		AOA.alpha = 0;
		Title.alpha = 0;

		AOA.text = lines[8].Replace("\\n","\n");
		Title.text = partString+"\n"+title;


		float maxTime = 7f;
		while (maxTime > 0) {

			maxTime -= Time.deltaTime;
			AOA.alpha += Time.deltaTime*(1.0f/4f);
			Title.alpha += Time.deltaTime*(1.0f/4f);
			overlayColor.a = overlayColor.a + Time.deltaTime*(1.0f/2.5f);
			if (overlayColor.a > 107/255f) overlayColor.a = 107/255f;
			Overlay.color = overlayColor;

			yield return new WaitForEndOfFrame();
		}
		isFadingInTitleCard = false;
	}

	IEnumerator ambienceColor(string hexString, float maxTime, int kind) {
		float time = 0f;

		Color dest = new Color();
		Color start = RenderSettings.ambientLight;
		ColorUtility.TryParseHtmlString(hexString, out dest);

        if (kind == 0) { // Ambient lighting
            while (time < maxTime) {
                RenderSettings.ambientLight = Color.Lerp(start, dest, Mathf.SmoothStep(0, 1, time / maxTime));
                time += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        } else if (kind == 1) {
            Camera cam = GameObject.Find("OverworldCamera").GetComponent<Camera>();
            start = cam.backgroundColor;
            while (time < maxTime) {
                cam.backgroundColor = Color.Lerp(start, dest, Mathf.SmoothStep(0, 1, time / maxTime));
                time += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }
	}

	IEnumerator moveObject(Transform obj, Vector3 dest, float maxTime) {
		Vector3 start = obj.position;
		float time = 0;
		Vector3 newPos = new Vector3();
		while (time < maxTime) {
			newPos = Vector3.Lerp(start,dest,Mathf.SmoothStep(0,1,time/maxTime));
			obj.position = newPos;
			time += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}

	}


    // Waits and then loads the scene.
    IEnumerator fadeLayer(string name, float dest, float maxTime) {
        Image i = GameObject.Find(name).GetComponent<Image>();
        Color c = i.color;
        float start = i.color.a;
        float time = 0;
        while (time < maxTime) {
            c.a = Mathf.Lerp(start, dest, Mathf.SmoothStep(0, 1, time / maxTime));
            time += Time.deltaTime;
            i.color = c;
            yield return new WaitForEndOfFrame();
        }
    }

    // Waits and then loads the scene.
    public static IEnumerator lightIntensity(string name, float dest, float maxTime) {
		Light l = GameObject.Find(name).GetComponent<Light>();
		float start = l.intensity;
		float time = 0;
		while (time < maxTime) {
			l.intensity = Mathf.Lerp(start,dest,Mathf.SmoothStep(0,1,time/maxTime));
			time += Time.deltaTime;

            if (MyInput.shortcut) time += 9 * Time.deltaTime;
            yield return new WaitForEndOfFrame();
		}
	}

	// Waits and then loads the scene.
	IEnumerator fogDensity(float dest, float maxTime) {
		float start = RenderSettings.fogDensity;
		float time = 0;
		while (time < maxTime) {
			RenderSettings.fogDensity = Mathf.Lerp(start,dest,Mathf.SmoothStep(0,1,time/maxTime));
			time += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
	}

	IEnumerator fadeMeshAlpha(float dest, float maxTime, bool isskinned, string objname) {
		GameObject go = GameObject.Find(objname);
		float time = 0;
		if (isskinned) {
			SkinnedMeshRenderer mr = go.GetComponent<SkinnedMeshRenderer>();
			Color col = mr.material.GetColor("_TintColor");
			float start = col.a;
			while (time < maxTime) {
				time += Time.deltaTime;
				col.a = Mathf.Lerp(start,dest,Mathf.SmoothStep(0,1,time/maxTime));
				mr.material.SetColor("_TintColor",col);
				yield return new WaitForEndOfFrame();
			}
		} else {
			MeshRenderer _mr = go.GetComponent<MeshRenderer>();
			Color col = _mr.material.GetColor("_TintColor");
			float start = col.a;
			while (time < maxTime) {
				time += Time.deltaTime;
				col.a = Mathf.Lerp(start,dest,Mathf.SmoothStep(0,1,time/maxTime));
				_mr.material.SetColor("_TintColor",col);
				yield return new WaitForEndOfFrame();
			}
		}
	}


    bool insidetrigger = false;
    // Deprecated for now
	void OnTriggerEnter(Collider other) {
		if (other.CompareTag("Player")) {
            insidetrigger = true;
		}
	}

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            insidetrigger = false;
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            insidetrigger = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            insidetrigger = false;
        }
    }



    // Read from file
    List<List<string>> getCommands(string rawcommands) {

        // Use this for stff that's only in the editor 
        //StreamReader sr = new StreamReader(Path.Combine (Application.dataPath, "Resources/Scripts/cutscene/"+_name+".txt"));
        // Resources are packed into our game assets for building, so use resources.load
        // Note you don't need the file extension here
        //	TextAsset ta =Resources.Load("Scripts/cutscene/"+_name) as TextAsset;
    //    string raw = ta.text;
        string raw = rawcommands;

        string[] rawLines = raw.Split(new string[]{"\n","\r\n"},System.StringSplitOptions.None);
		List<List<string>> commands = new List<List<string>>();

		foreach (string rawLine in rawLines) {
			string[] parts = rawLine.Split(new string[]{" "},System.StringSplitOptions.None);
			List<string> command = new List<string>();
			foreach (string part in parts) {
				command.Add(part);
			}
			commands.Add(command);
		}
		return commands;
	}

    static public float safeFloatParse(string s) {
        return float.Parse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
    }

    static public void DeactivateCamAndPlayer() {
        deactivateCameras = deactivatePlayer = true;
    }
    static public void ActivateCamAndPlayer() {
        deactivateCameras = deactivatePlayer = false;
    }
}
