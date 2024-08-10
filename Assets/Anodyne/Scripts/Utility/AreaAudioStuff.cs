using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class AreaAudioStuff : MonoBehaviour {

    public enum AreaAudioNames { Ring_Driving, Ring_Ambience, None, RingHurt };
    [FormerlySerializedAs("audioCueToStart")]
    public AreaAudioNames songToQueue;
    public bool isHighwayEntry;

    [Header("Player Tracker Stuff")]
    public bool isPlayerDetector = false;
    Transform playerTrans;
    Transform carTrans;

    AudioHelper ah;
	void Start () {
        deactivateHighwayTicks = 0;

        gameObject.layer = 21;
        lastPlayedSong = AreaAudioNames.None;
        if (GameObject.Find(Registry.PLAYERNAME3D_Walkscale) == null) {
            gameObject.SetActive(false);
            return;
        }
        playerTrans = GameObject.Find(Registry.PLAYERNAME3D_Walkscale).transform;
        if (DataLoader._getDS("pal-ring-3") == 1 && 0 == DataLoader._getDS("db-field")) {
            if (songToQueue == AreaAudioNames.Ring_Driving || songToQueue == AreaAudioNames.Ring_Ambience) {
                songToQueue = AreaAudioNames.RingHurt;
                isHighwayEntry = false;
            }
        }
	}

    //bool moveToPlayer = false;
    int mode = 0;
    float t = 0;
    Vector3 moveHackOffset = new Vector3(0, 3f, 0);
    Vector3 tempV = new Vector3();
    int hackmode = 0;
	void Update () {
        if (ah == null) {
            ah = AudioHelper.instance;
        }

        if (carTrans == null) {
            if (Camera.main.GetComponent<MedBigCam>().GetBigPlayerControl() == null) return;
            carTrans = Camera.main.GetComponent<MedBigCam>().GetBigPlayerControl().transform;
        }
        if (isPlayerDetector) {
            if (playerTrans.gameObject.activeInHierarchy) {
                tempV = playerTrans.position;
            } else if (carTrans != null) {
                tempV = carTrans.position;
            } else {
                return;
            }
            t += Time.deltaTime;
            if (hackmode == 0) {
                tempV += moveHackOffset;
                if (t > 0.1f) {
                    t = 0;
                    hackmode = 1;
                }
            } else if (hackmode == 1) {
                if (t > 0.1f) {
                    hackmode = 0;
                    t = 0;
                }
            }
            transform.position = tempV;
        } else {
            if (name == deactivateHighwayName) {
                if (deactivateHighwayTicks > 0) {
                    deactivateHighwayTicks--;
                }
            }
            if (mode == 0) {
            
            // Song was just queued, determine whether it should be played.
            } else if (mode == 1) {
                // Ignore triggers at highway entrances if the driving song is already playing
                if (isHighwayEntry && ah.IsSongPlaying("Ring_Driving")) {
                    mode = 0;
                    return;
                }
                // Dequeue if the queued song is already playing
                if (songToQueue == AreaAudioNames.Ring_Ambience && ah.IsSongPlaying("Ring_Ambience")) {
                    deactivateHighwayTicks = 2;
                    deactivateHighwayName = name;
                    mode = 0;
                } else if (songToQueue == AreaAudioNames.Ring_Driving && ah.IsSongPlaying("Ring_Driving")) {
                    mode = 0;
                } else if (songToQueue == AreaAudioNames.RingHurt && ah.IsSongPlaying("RingHurt")) {
                    mode = 0;
                } else {
                    //print("Queue: " + songToQueue.ToString());
                    lastPlayedSong = songToQueue;
                    if (songToQueue == AreaAudioNames.Ring_Ambience) {
                        ah.StopAllSongs();
                        mode = 2;
                    } else if (songToQueue == AreaAudioNames.Ring_Driving) {
                        mode = 3;
                    } else if (songToQueue == AreaAudioNames.RingHurt) {
                        ah.StopAllSongs();
                        mode = 2;
                    }
                }
            } else if (mode == 2) {
                if (songToQueue == AreaAudioNames.Ring_Ambience) {
                    ah.PlaySong("Ring_Ambience", 0, 144f);
                } else if (songToQueue == AreaAudioNames.RingHurt) {
                    ah.PlaySong("RingHurt", 20.645f, 154.839f);
                } else if (songToQueue == AreaAudioNames.Ring_Driving) {
                    ah.StopSongByName("Ring_Ambience", true);
                    ah.PlaySong("Ring_Driving", 0, 106.105f);
                }
                mode = 0;
            } else if (mode == 3) {
                // wait for car form, or uncue 
                if (lastPlayedSong != AreaAudioNames.Ring_Driving || deactivateHighwayTicks > 0) {
                    mode = 0;
                    //print("Cancel: " + songToQueue.ToString());
                    if (lastPlayedSong == AreaAudioNames.Ring_Driving) lastPlayedSong = AreaAudioNames.None;
                } else if (carTrans.gameObject.activeInHierarchy) {
                    mode = 2;
                }
            }
        }
	}

    public static int deactivateHighwayTicks = 0;
    public static string deactivateHighwayName = "";
    public static AreaAudioNames lastPlayedSong;
    // Try to get an audio trigger to play a song, but ignore this if the song was already cued+played
    public void Queue() {
        if (lastPlayedSong == songToQueue) return;
        mode = 1;
    }
    // PlayerDetector when detecting another trigger, will play its music (maybe)
    private void OnTriggerEnter(Collider other) {
        if (isPlayerDetector) {
            if (other.GetComponent<AreaAudioStuff>() != null) {
                other.GetComponent<AreaAudioStuff>().Queue();
            }
        }
    }

}
