using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JumpDownZone : MonoBehaviour {

    public static bool overlapping  = false;
    public static bool ladderOverlapping = false;
    public static int laddersCount = 0;
    public bool isLadder = false;

    public static float globaldistance = 0;
    public float distance = 2f;
    static bool addedladderloadthing = false;
	void Start () {
        if (!addedladderloadthing) {
            addedladderloadthing = true;
            SceneManager.sceneLoaded += OnLevelLoadedLadder;
        }
    }
	    
    public static void OnLevelLoadedLadder(Scene scene, LoadSceneMode mode) {
        ladderOverlapping = false;
        laddersCount = 0;
    }

    bool onplayer;
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.name == Registry.PLAYERNAME2D) {
            if (isLadder) {
                laddersCount++;
                ladderOverlapping = true;
            } else {
                overlapping = true;
            }
            globaldistance = distance;
        }
    }


    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.name == Registry.PLAYERNAME2D) {
            if (isLadder) {
                laddersCount--;
                if (laddersCount <= 0) {
                    ladderOverlapping = false;
                }
            }
            overlapping = false;
        }
    }
}
