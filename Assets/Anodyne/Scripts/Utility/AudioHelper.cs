using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioHelper : MonoBehaviour {


	[Tooltip("In order to get cross-scene playback, set this to true. Then, this script won't play audio, but will feed its data to the Loader's single AudioHelper.")]
	public bool isInSceneData = true;
	[Tooltip("Enabled by default - nothing will play this way.")]
	public bool debugTurnOff = false;


	AudioHelper sceneAudioData;


	int maxTracks = 4;
	int[] trackStateArray;
	public bool[] playOnSceneEnterArray;
	AudioSource[] trackFirstArray;
	AudioSource[] trackSecondArray;
	AudioClip[] clipArray;
	public string[] clipNameArray;
	public float[] loopStartArray;
	[Tooltip("Leave at zero for the whole clip to loop.")]
	public float[] loopEndArray;
	[Tooltip("If true, a second track AudioSource will be used to overlay the effects/new loop, while the first track finishes playing between LoopEnd and the clip end.")]
	public bool[] hasLayeredLoopArray;
	bool[] trackExistsArray;
	double[] nextStartTimeArray;
	public bool[] playOnceArray;
	[Range(0.2f,1)]
	public float[] volumeReductionArray;

	AudioSource[] sfxDuckArray;
	AudioSource[] sfxRegularArray;

	// CALLED BY SCENEDATA'S AUDIO HELPER, ON TO THE INSTANCE AUDIO HELPER.
	void SetSceneAudioData(AudioHelper _sceneAudioData) {
		sceneAudioData = _sceneAudioData;
	}


	#if UNITY_EDITOR
	// SCENEDATA's Audio Helper will refresh the instance audio helper's data.
	void OnValidate() {
		if (isInSceneData) {
			if (GameObject.Find("AudioHelper") != null) {
				GameObject.Find("AudioHelper").GetComponent<AudioHelper>().refreshSceneAudioData();
			}
		}

	}
	#endif

	public static AudioHelper instance;
	// For the single AudioHelper instance, this is called once in the game.
	// Initializes the 'tracks', sets beginning audio data (likely title music).
	// As the game goes on, other scripts will issue 'fade out'/etc. commands to the master instance.
	// Then the instance can use the scene's new sceneAudioData as needed.
	void Start () {

		if (!isInSceneData) {
			//print("Initializing global Audio Helper.");
			instance = this;

			sfxDuckArray = new AudioSource[2];
			sfxRegularArray = new AudioSource[4];

			for (int i = 0; i < sfxDuckArray.Length; i++) {
				sfxDuckArray[i] = transform.Find("SFX").Find("SFX Ducking "+(i+1).ToString()).GetComponent<AudioSource>();
			}

            for (int i = 0; i < sfxRegularArray.Length; i++) {
                sfxRegularArray[i] = transform.Find("SFX").Find("SFX " + (i + 1).ToString()).GetComponent<AudioSource>();
            }
			trackStateArray = new int[maxTracks];
			playOnSceneEnterArray = new bool[maxTracks];
			hasLayeredLoopArray = new bool[maxTracks];
			trackExistsArray = new bool[maxTracks];
			clipNameArray = new string[maxTracks];
			playOnceArray = new bool[maxTracks];
				
			trackFirstArray = new AudioSource[maxTracks];
			trackSecondArray = new AudioSource[maxTracks];
			clipArray = new AudioClip[maxTracks];
			volumeReductionArray = new float[maxTracks];

			loopStartArray = new float[maxTracks];
			loopEndArray = new float[maxTracks];
			nextStartTimeArray = new double[maxTracks];

            if (GameObject.Find("SceneData") != null) {
                SetSceneAudioData(GameObject.Find("SceneData").GetComponent<AudioHelper>());
            } else {
                SetSceneAudioData(GameObject.Find("2D SceneData").GetComponent<AudioHelper>());
            }
            refreshSceneAudioData();

            CancelCertainSongs(this);
            bool shouldPlay = ShouldSongsInSceneDataPlayOnSceneEnter();
			for (int i = 0; i < maxTracks; i++) {
				trackFirstArray[i] = transform.Find("Track "+(i+1).ToString()+"-"+"1").GetComponent<AudioSource>();
				trackSecondArray[i] = transform.Find("Track "+(i+1).ToString()+"-"+"2").GetComponent<AudioSource>();

				if (shouldPlay && clipNameArray[i] != null) {
					clipArray[i] = Resources.Load("Audio/Music/"+clipNameArray[i]) as AudioClip;	
					trackExistsArray[i] = true;
				} else if (!shouldPlay) {
                    clipNameArray[i] = "";
                }
			}


		} else {
			// Establish sceneAudioData - AudioHelper connection
			// This ensures when switching scenes, the AudioHelper instance has the new scene's copy of sceneAudioData.
			// Note: this is redundant on game startup, but that is ok.
            if (playOnceArray == null || playOnceArray.Length == 0) {
                playOnceArray = new bool[maxTracks];
            } 
            if (loopStartArray == null || loopStartArray.Length == 0) {
                loopStartArray = new float[maxTracks];
            }
            if (loopEndArray == null || loopEndArray.Length == 0) {
                loopEndArray = new float[maxTracks];
            }
            if (hasLayeredLoopArray == null || hasLayeredLoopArray.Length == 0) {
                hasLayeredLoopArray = new bool[maxTracks];
            }
            if (volumeReductionArray == null || volumeReductionArray.Length == 0) {
                volumeReductionArray = new float[maxTracks];
                for (int i = 0; i < maxTracks; i++) {
                    volumeReductionArray[i] = 1f;
                }
            }

            GameObject.Find("AudioHelper").GetComponent<AudioHelper>().SetSceneAudioData(this);
		}

	}

	// Can be used anywhere, but primarily for when testing stuff in the editor (called from OnValidate)
	// Copies clip names, loop start/ends, whether it starts by default, to current scene's audiohelper
	// Can break audio looping if you edit something while playing the game. (bc loop pt arrays are overwritten)
	public void refreshSceneAudioData() {
		if (sceneAudioData == null) return;
		//print("Scene Audio Data refreshed!");
		sceneAudioData.clipNameArray.CopyTo(clipNameArray,0);
		sceneAudioData.loopEndArray.CopyTo(loopEndArray,0);
		sceneAudioData.loopStartArray.CopyTo(loopStartArray,0);
		sceneAudioData.playOnceArray.CopyTo(playOnceArray,0);
		for (int i = 0; i < loopEndArray.Length; i++ ){
			if (loopEndArray[i] == 0) {
				hasLayeredLoopArray[i] = false;
			} else {
				hasLayeredLoopArray[i] = true;
			}
		}
		// Creating this array defaults to zero, but the min is 0.2f which is not the default of 1 so set the default
		sceneAudioData.volumeReductionArray.CopyTo(volumeReductionArray,0);
		for (int i = 0; i < volumeReductionArray.Length;i++) { 
			if (volumeReductionArray[i] <= 0.1f) volumeReductionArray[i] = 1f;
		}

		sceneAudioData.playOnSceneEnterArray.CopyTo(playOnSceneEnterArray,0);
		debugTurnOff = sceneAudioData.debugTurnOff;
        if (debugTurnOff) print("AudioHelper turned off!");
	}

	public bool doSceneCrossfade = false;
	// Update is called once per frame
	void Update () {
		if (debugTurnOff) return;
		if (isInSceneData) {
			//updateSceneData();
			return;
		}

		if (doSceneCrossfade) {
			doSceneCrossfade = false;
			CrossfadeOnSceneTransition();
		}
		UpdateSFX();
		UpdateTracks();

	}


	// just for tests
	public void PlaySongNoFade(int trackIndex,string songname) {
		clipArray[trackIndex] = Resources.Load("Audio/Music/"+songname) as AudioClip;
		clipNameArray[trackIndex] = songname;
		trackFirstArray[trackIndex].Stop();
		trackSecondArray[trackIndex].Stop();
		loopEndArray[trackIndex] = 0;
		trackStateArray[trackIndex] = 0;
		playOnSceneEnterArray[trackIndex] = true;

		if (loopEndArray[trackIndex] == 0) {
			loopEndArray[trackIndex] = clipArray[trackIndex].length;
			hasLayeredLoopArray[trackIndex] = false;
		} else {
			hasLayeredLoopArray[trackIndex] = true;
		}
	}

    public void FadeAllSongs(float fadetime, float targetVolume) {
        for (int i = 0; i < clipArray.Length; i++) {
            if (trackExistsArray[i] == true) {
                FadeSong(clipNameArray[i], fadetime, targetVolume);
            }
        }
    }
    public void UnfadeAllSongsExceptDust(float fadetime, float targetVolume) {
        for (int i = 0; i < clipArray.Length; i++) {
            if (trackExistsArray[i] == true && clipNameArray[i] != "DustCue") {
                FadeSong(clipNameArray[i], fadetime, targetVolume);
            }
        }
    }

    public void SetSongVolume(string songname, float vol) {
        for (int i = 0; i < clipArray.Length; i++) {
            if (trackExistsArray[i] == true) {
                if (clipNameArray[i] == songname) {
                    trackFirstArray[i].volume = vol;
                    trackSecondArray[i].volume = vol;
                }
            }
        }
    }

	// Called with audio triggers to fade certain layers.
	public void FadeSong(string songname, float fadeTime, float targetVolume,bool stopAtEnd=false) {
		for (int i = 0; i < clipArray.Length; i++ ){ 
			if (trackExistsArray[i] == true) {
				if (clipNameArray[i] == songname) {
					targetVolume *= volumeReductionArray[i];

					if (trackFirstArray[i].volume == targetVolume) {
						print("Volume of "+songname+" is already at target fade volume. Ignoring FadeSong.");
						return;
					}
					StartCoroutine(FadeSongCoroutine(trackFirstArray[i],fadeTime,targetVolume,stopAtEnd));
					StartCoroutine(FadeSongCoroutine(trackSecondArray[i],fadeTime,targetVolume, stopAtEnd));
				}
			}
		}
	}

	IEnumerator FadeSongCoroutine(AudioSource src, float fadeTime, float targetVolume, bool stopAtEnd=false) {

		float t = 0;
		float startVol = src.volume;
		while (t < fadeTime) {
			t += Time.deltaTime;
			float nextVol = Mathf.Lerp(startVol,targetVolume,t/fadeTime);
			src.volume = nextVol;
			yield return new WaitForEndOfFrame();
		}
        if (stopAtEnd && src.clip != null) {
            StopSongByName(src.clip.name);
        }
	}
    public bool IsSongPlaying(string songname) {
        for (int i = 0; i < clipArray.Length; i++) {
            if (trackExistsArray[i] && clipNameArray[i] == songname) {
                return true;
            }
        }
        return false;
    }

	public void PlaySong(string songname,float loopStart, float loopEnd,bool playOnce=false,float volReduce=1f )  {
		// assigns track ID here, sets track existance, sets hasLayeredLoop peropertyes...
		for (int i = 0; i < clipArray.Length; i++) {
			if (clipNameArray[i] == songname) {
				print("Already playing "+songname);
				return;	
			}
		}

		for (int i = 0; i < clipArray.Length; i++ ){ 
			if (trackExistsArray[i] == false) {
				int trackIndex = i;
				print("Playing "+songname+" on track "+i.ToString());
				clipArray[trackIndex] = Resources.Load("Audio/Music/"+songname) as AudioClip;
				clipNameArray[trackIndex] = songname;
				loopStartArray[trackIndex] = loopStart;
				loopEndArray[trackIndex] = loopEnd;
				trackStateArray[trackIndex] = 0;
				trackExistsArray[trackIndex] = true;
				playOnSceneEnterArray[trackIndex] = true;
				volumeReductionArray[trackIndex] = volReduce;


				if (playOnce) {
					playOnceArray[trackIndex] = true;
				}

				if (loopEndArray[trackIndex] == 0) {
					loopEndArray[trackIndex] = clipArray[trackIndex].length;
					hasLayeredLoopArray[trackIndex] = false;
				} else {
					hasLayeredLoopArray[trackIndex] = true;
				}
				break;
			}
		}
	}


    public void StopAllSongs(bool fade=true) {
        for (int i = 0; i < maxTracks; i++) {
            if (trackExistsArray[i]) {
                StopSong(i,fade);
            }
        }
    }

	public void StopSong(int trackID,bool fade=true) {

        if (trackStateArray[trackID] == 10) {
            //print("Note: stop was called on " + trackFirstArray[trackID].clip.name + " while it was fading. Stopping it immediately.");
            return;
        }

        print("Stopping " + clipNameArray[trackID] + " on track " + trackID);

        trackStateArray[trackID] = 10;	
		clipNameArray[trackID] = "";


        if (!fade) {
            print("Fading out song instantly.");
            trackFirstArray[trackID].volume = 0;
            trackSecondArray[trackID].volume = 0;
        }
	}
	public void StopSongByName(string clipName, bool fade=true) {
		for (int i = 0; i < trackStateArray.Length; i++) {
			if (trackExistsArray[i] && clipNameArray[i] == clipName) {
                StopSong(i, fade);
                return;
			}
		}
	}


    // Runs on the global instance in source scene
    // Called when some other script (usually OvToMemDoor or CutsceneManager) sets the 
    // crossfade boolean to true.
    // This function fades out current and fades in new songs, but preserves overlapping songs between scenes.

    bool skipSceneCrossfadeOnce = false;
    public void SkipNextSceneCrossfade() {
        skipSceneCrossfadeOnce = true;
    }

    int getDS(string scene) {
        return DataLoader.instance.getDS(scene);
    }

    public bool ShouldSongsInSceneDataPlayOnSceneEnter() {
        if (SceneManager.GetActiveScene().name == "NanoDustbound") {
            if (getDS("db-field") == 0) {
                print("No music in DB, first time entering.");
                return false;
            } else if (getDS("rites-done-2") == 1 && getDS("wrestle-2") == 0) {
                print("No music in DB, birth scene or pre-wrestle-2.");
                return false;
            } else if (getDS("rites-done-3") == 1 && getDS("wrestle-3") == 0) {
                print("No music in DB, blowup scene.");
                return false;
            }
        }
        return true;
    }

    void CancelCertainSongs(AudioHelper ah) {
        string scene = SceneManager.GetActiveScene().name;
        if (scene == "NanoGolem") {
            if (getDS("CARD4") == 1 || getDS("golem-state") == 1) {
                CancelSong("db-ambience",ah);
            } else {
                CancelSong("Golem", ah);
            }
        } else if (scene == "DesertSpire") {
            if (getDS(Registry.FLAG_DESERT_OPEN) == 1) {
                CancelSong("DustDonut", ah);
            } else {
                CancelSong("meadowAmbience", ah);
            }
        } else if (scene == "CCC") {
            if (getDS("end-ring") == 1) {
                CancelSong("CCC", ah);
            }
        } else if (scene == "DesertSpireTop") {
            if (getDS("end-ring") == 1) {
                tempignoreending = true;
                CancelSong("BirdMtnEnvironment", ah);
            }
        }  else if (scene == "DesertShore") {
            if (getDS("end-ring") == 1) {
                tempignoreending = true;
                CancelSong("DesertShore", ah);
            }
        }
    }
    void CancelSong(string s, AudioHelper ah) {
        for (int i = 0; i < ah.clipNameArray.Length; i++) { 
            if (ah.clipNameArray[i] == s) {
                print("AudioHelper: Cancelling " + s);
                ah.clipNameArray[i] = "";
            }
        }
    }
    bool tempignoreending = false;
    public bool sceneCrossfades = true;
	public void CrossfadeOnSceneTransition() {

        if (DataLoader._getDS("start-bad-end") == 1 || Anodyne.BadEnding.BadEndRunning) {
            skipSceneCrossfadeOnce = true;
            print("Skipping scene music bc badend playing");
        }
        if (SceneManager.GetActiveScene().name == "Wormhole2D" && IsSongPlaying("NanoZera")) {
            skipSceneCrossfadeOnce = true;
        }
        // notes on this - during obss, wormhole2d shouldn't play music. PicoZera only has triggered music.
        // below, nanozera's music needs to be cancelled sot he boss song keeps playing. 

        if (SceneManager.GetActiveScene().name == "Wormhole2D" && Anodyne.GlandilockBoss.diedOnce) {
            skipSceneCrossfadeOnce = true;
            print("Ignoring Wormhole2D song bc in boss fight");
            // prev scene's song keeps playing - 
        }
        if (skipSceneCrossfadeOnce) {
            skipSceneCrossfadeOnce = false;
            if (SceneManager.GetActiveScene().name == "Wormhole2D" && Anodyne.GlandilockBoss.diedOnce) {
                skipSceneCrossfadeOnce = true;
                print("Ignoring NanoZera/PicoZera default music bc on boss fight.");
            }
            return;
        }

		GameObject go = GameObject.Find("SceneData");
        if (go == null) {
            go = GameObject.Find("2D SceneData");
        }

		// Get the dest. scene's AudioHelper
		AudioHelper ah = go.GetComponent<AudioHelper>();


		// Get the clip names from the audio helper.
		if (ah != null) {
            if (!ah.sceneCrossfades) {
                return;
            }

            CancelCertainSongs(ah);
            if (tempignoreending) {
                print("ignoring song transitions");
                tempignoreending = false;
                return;
            }
            string[] nextSceneClipNames = ah.clipNameArray;

			// Stop songs unique to source scene.
			foreach (string clipName in clipNameArray) {
				if (clipName == null) continue;
				if (!Array.Exists(nextSceneClipNames,c => c == clipName)) {
					StopSongByName(clipName);
				} 
			}

            if (ShouldSongsInSceneDataPlayOnSceneEnter()) {
                // Start songs unique to destination scenes.
                // Note, shared songs will keep playing.
                for (int i = 0; i < nextSceneClipNames.Length; i++) {
                    string nextClipName = nextSceneClipNames[i];
                    if (nextClipName == null) continue;
                    if (nextClipName.Length < 2) continue;
                    if (!Array.Exists(clipNameArray, c => c == nextClipName)) {
                        PlaySong(nextClipName, ah.loopStartArray[i], ah.loopEndArray[i], ah.playOnceArray[i], ah.volumeReductionArray[i]);
                    }
                }
            }
                
		}

	}
	public int whichTrackLoopIsPlaying(int trackID) {
		if (hasLayeredLoopArray[trackID] == false) return 0;

		if (trackStateArray[trackID] == 1) return 0;
		if (trackStateArray[trackID] == 100) return 1;
		return 0;

	}

    public static bool pausePlayingOfNewTracks = false;
	void UpdateTracks() {
		for (int i = 0; i < maxTracks; i++) { 
			if (trackExistsArray[i] == false) continue;
			switch (trackStateArray[i]) {
			case 0: // Schedule looping, play first song
                if (pausePlayingOfNewTracks) {
                    continue;
                }
                trackFirstArray[i].time = 0;
                trackSecondArray[i].time = 0;
                    if (playOnSceneEnterArray[i]) {
					trackStateArray[i] = 1;
					trackFirstArray[i].clip = clipArray[i];
				}

				trackFirstArray[i].loop = false;
				// Set up two clips for layered looping
				if (hasLayeredLoopArray[i]) {
					trackFirstArray[i].Play();
					trackSecondArray[i].clip = clipArray[i];
					nextStartTimeArray[i] = AudioSettings.dspTime + loopEndArray[i];
				} else {
					trackFirstArray[i].Play();
					trackFirstArray[i].loop = true;
					if (playOnceArray[i])  trackFirstArray[i].loop = false;

					//trackFirstArray[i].PlayScheduled(AudioSettings.dspTime + 0.5F);
					//nextStartTimeArray[i] = AudioSettings.dspTime + 0.5F + trackFirstArray[i].clip.length;
				}
                    //trackFirstArray[i].ti
                if (clipNameArray[i] == Anodyne.GlandilockBoss.PicoBossSongName && "PicoOcean" != SceneManager.GetActiveScene().name) {
                        trackFirstArray[i].volume = 0;
                        trackSecondArray[i].volume = 0;
                } else {

                    trackFirstArray[i].volume = 1 * volumeReductionArray[i];
                    trackSecondArray[i].volume = 1 * volumeReductionArray[i];
                }

                    trackFirstArray[i].pitch = 1;
                    trackSecondArray[i].pitch = 1;
                    break;

			// Handle Looping
			case 1:

				// Has layered looping effects
				if (hasLayeredLoopArray[i]) {

					// The Primary track has reached the end of its entire runtime.
					// make the secondary track start from the loop point.
					if (AudioSettings.dspTime + 1.0F > nextStartTimeArray[i]) {

						trackSecondArray[i].time = loopStartArray[i];
						trackSecondArray[i].PlayScheduled(nextStartTimeArray[i]);
						nextStartTimeArray[i] += (loopEndArray[i] - loopStartArray[i]);
						trackStateArray[i] = 100;
					}
				
				// Only primary track is looping
				} else {
					if (!appPaused && playOnceArray[i] && trackFirstArray[i].isPlaying == false) {
						playOnceArray[i] = false;
                        clipNameArray[i] = "";
						trackExistsArray[i] = false;
						trackStateArray[i] = 0;
					}
				}

				break;
			
			// Check 2nd track for being past loop end, and start first again.
			case 100:
				if (AudioSettings.dspTime + 1.0F > nextStartTimeArray[i]) {
					trackFirstArray[i].time = loopStartArray[i];
					trackFirstArray[i].PlayScheduled(nextStartTimeArray[i]);
					nextStartTimeArray[i] += (loopEndArray[i] - loopStartArray[i]);
					trackStateArray[i]= 1;
				}
				break;
			// fade out for 2 seconds
			case 10:
				trackFirstArray[i].volume -= Time.deltaTime/1.0f;
				trackSecondArray[i].volume -= Time.deltaTime/1.0f;
				if (trackFirstArray[i].volume <= 0) {
					trackFirstArray[i].Stop();
					trackSecondArray[i].Stop();
					trackExistsArray[i] = false;
					trackStateArray[i] = 0;
                    clipNameArray[i] = "";
                    playOnSceneEnterArray[i] = false;
                    playOnceArray[i] = false;
				}
				break;
			// to-do: fade in?
			}
		}
	}

    bool appPaused = false;
    void OnApplicationPause(bool pause) {
        appPaused = pause;
    }
    void OnApplicationFocus(bool focus) {
        appPaused = focus;
    }

    public void PauseAllAudioTracks() {
        for (int i = 0; i < maxTracks; i++) {
            trackFirstArray[i].Pause();
            trackSecondArray[i].Pause();
        }
    }
    public void UnpauseAllAudioTracks() {
        for (int i = 0; i < maxTracks; i++) {
            trackFirstArray[i].UnPause();
            trackSecondArray[i].UnPause();
        }
    }

    public void playOneShot(string name, float volumeMultiplier = 1f, float _pitch = 1f, int forcechannel = 0) {
        sfxRegularArray[forcechannel].pitch = _pitch;
        sfxRegularArray[forcechannel].PlayOneShot(Resources.Load("Audio/Sound/" + name) as AudioClip,volumeMultiplier);
    }

    public void playSFX(string name,bool isRegular=true, float volumeMultiplier=1f,bool ignoreIfPlaying=false,float _pitch=1) {
		AudioSource _as = null;

        if (ignoreIfPlaying) {
            foreach (AudioSource src in sfxRegularArray) {
                if (src.isPlaying && src.clip != null && src.clip.name == name) {
                    return;
                }
            }
            foreach (AudioSource src in sfxDuckArray) {
                if (src.isPlaying && src.clip != null && src.clip.name == name) {
                    return;
                }
            }
        }

		if (isRegular) {
			for (int i = 0; i < sfxRegularArray.Length; i++) {
				if (!sfxRegularArray[i].isPlaying) {
					_as = sfxRegularArray[i];
				}
			}
		} else {
			for (int i = 0; i < sfxDuckArray.Length; i++) {
				if (!sfxDuckArray[i].isPlaying) {
					_as = sfxDuckArray[i];
				}
			}
		}

        //print(name);
		if (_as != null) {
			_as.clip = Resources.Load("Audio/Sound/"+name) as AudioClip;
            _as.pitch = _pitch;
			_as.volume = volumeMultiplier;
			_as.Play();
		} else {
            print("no available sound slots for "+ name);
        }
	}
	public void fadePitch(string name, float _time, float _pitch) {
		for (int i = 0; i < trackStateArray.Length; i++) {
			if (clipNameArray[i] == name) {
				StartCoroutine(IfadePitch(trackFirstArray[i],_time,_pitch));
				StartCoroutine(IfadePitch(trackSecondArray[i],_time,_pitch));
				break;
			}
		}
	}

    public void setPitch(string name, float _pitch) {
        for (int i = 0; i < trackStateArray.Length; i++) {
            if (clipNameArray[i] == name) {
                trackFirstArray[i].pitch = _pitch;
                trackSecondArray[i].pitch = _pitch;
            }
        }
    }

	public void fadeOutSFX(string name, bool isRegular=true, float fadeTime=1f) {
		if (currentlyFadingSFX == null) {
			currentlyFadingSFX = new List<string>();
		}
		if (isRegular) {
			for (int i = 0; i < sfxRegularArray.Length;i++) {
				if (sfxRegularArray[i].isPlaying && sfxRegularArray[i].clip.name == name) {
					if (currentlyFadingSFX.Contains("reg"+i.ToString())) continue;
					currentlyFadingSFX.Add("reg"+i.ToString());
					StartCoroutine(IfadeOutSFX(sfxRegularArray[i],fadeTime,"reg"+i.ToString()));
				}
			}
		} else {
			
			for (int i = 0; i < sfxDuckArray.Length;i++) {
				if (sfxDuckArray[i].isPlaying && sfxDuckArray[i].clip.name == name) {
					string key = "duck"+i.ToString();
					if (currentlyFadingSFX.Contains(key)) continue;
					currentlyFadingSFX.Add(key);
					StartCoroutine(IfadeOutSFX(sfxDuckArray[i],fadeTime,key));
				}
			}
			
		}

	}

	IEnumerator IfadePitch(AudioSource _as, float fadeTime=1f,float pitch=0.5f) {
		float t = 0;
		float startPitch = _as.pitch;
		while (t < fadeTime) {
			t += Time.deltaTime;
			_as.pitch = Mathf.Lerp(startPitch,pitch,t/fadeTime);
			yield return new WaitForEndOfFrame();
		}
	}

	List<string> currentlyFadingSFX;
	IEnumerator IfadeOutSFX(AudioSource _as, float fadeTime=1f,string key="") {
		float t = 0;
		float startVol = _as.volume;
		while (t < fadeTime) {
			t += Time.deltaTime;
			_as.volume = Mathf.Lerp(startVol,0,t/fadeTime);
			yield return new WaitForEndOfFrame();
		}
		_as.Stop();
		currentlyFadingSFX.Remove(key);

	}

	void UpdateSFX() {
//		if (currentlyFadingSFX!= null)print(currentlyFadingSFX.Count);
	}

}
