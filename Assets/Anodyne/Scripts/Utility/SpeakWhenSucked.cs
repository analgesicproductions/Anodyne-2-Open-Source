using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeakWhenSucked : MonoBehaviour {

    public string sceneName = "";
    public int lineIndex = -1;
    public int endLineIndex = -1;
    public bool looped = false;
    [Header("State Stuff")]
    public bool saveFlagWhenSucked = true;
    public bool dontSpawnIfSaveFlagIsSet = true;
    public bool signalWhenSucked = true;
    [Tooltip("Use this if there are two NPCs with same flagname")]
    public string uniqueSceneChunk = "";
    Anodyne.Vacuumable vac;
    DialogueBox box;
    Anodyne.SpriteAnimator anim;
    string flagname;
	void Start () {
        vac = GetComponent<Anodyne.Vacuumable>();
        vac.DoesntBreakWhenHitByOtherThings_External = true;
        anim = GetComponent<Anodyne.SpriteAnimator>();
        box = GameObject.Find("Dialogue").GetComponent<DialogueBox>();
        flagname = sceneName + "-" + lineIndex.ToString() + "-" + endLineIndex.ToString() + uniqueSceneChunk;
        if (dontSpawnIfSaveFlagIsSet && DataLoader.instance.getDS(flagname) != 0) {
            gameObject.SetActive(false);
        }
    }

    int mode = 0;
	void Update () {

        if (mode == 0) {
            if (vac.state == Anodyne.Vacuumable.VacuumMode.PickedUp || vac.state == Anodyne.Vacuumable.VacuumMode.Broken) {
                if (signalWhenSucked) {
                    EntityState2D state = GetComponent<EntityState2D>();
                    HF.SendSignal(state.children);
                }
                if (saveFlagWhenSucked) {
                    DataLoader.instance.setDS(flagname, 1);
                }

                if (vac.state == Anodyne.Vacuumable.VacuumMode.Broken) {
                    mode = 1;
                }

                if (looped) {
                    box.playLoopedDialogue(sceneName);
                } else {
                    if (lineIndex == -1 && endLineIndex == -1) {
                        box.playDialogue(sceneName);
                    } else if (endLineIndex == -1) {
                        box.playDialogue(sceneName, lineIndex);
                    } else {
                        box.playDialogue(sceneName, lineIndex,endLineIndex);
                    }
                }
                mode = 1;
            }
        } else if (mode == 1) {
            if (saveFlagWhenSucked && dontSpawnIfSaveFlagIsSet && !anim.isPlaying) {
                gameObject.SetActive(false);
            }
            if (vac.state == Anodyne.Vacuumable.VacuumMode.Idle) {
                mode = 0;
            }
        }
    }
}
