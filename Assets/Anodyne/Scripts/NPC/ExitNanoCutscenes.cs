using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Checks for needing to play a cutscene after getting cards
public class ExitNanoCutscenes : MonoBehaviour {

    string sceneToPlay = "";
    public static string nanoprefix = "";

    public bool doDebug = false;
    [Range(0,3)]
    public int debugSceneNumber = 0;
    [Range(0,3)]
    public int debugToPiCoRa = 0;

    GameObject bed;
	void Start () {
        if (!Registry.DEV_MODE_ON) {
            doDebug = false;
        } else if (doDebug) {
            print("DEBUGGING ExitNanoCutscenes!!");
            for (int i = 0; i <= debugSceneNumber; i++) {
                Ano2Stats.GetCard(i);
                if (i == 1) DataLoader._setDS("pal-card-1", 1);
                if (i == 2) DataLoader._setDS("pal-card-2", 1);
                if (i == 3) DataLoader._setDS("pal-card-3", 1);
            }
            setprefix(debugToPiCoRa);
        }

        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        int numCards = Ano2Stats.CountTotalCards();
        for (int i = 1; i <= numCards; i++) {
            if (i <= 4 && (sceneName == "CCC" || sceneName == "CougherHome") && DataLoader.instance.getDS("pal-card-"+i.ToString()) == 0) {
                sceneToPlay = "pal-card-" + i.ToString();
                break;
            }
        }
        if (sceneToPlay != "" && sceneName != "CCC") {
            bed = GameObject.Find("BedTrigger");
            bed.SetActive(false);
                
        }



        if (sceneToPlay == "") {
            SetPalsInactive();
            enabled = false;
        }
	}

    // called from dustcore
    public static void setprefix(int cardID) {
        if (cardID == 0) ExitNanoCutscenes.nanoprefix = "Tongue";
        if (cardID == 1) ExitNanoCutscenes.nanoprefix = "Pig";
        if (cardID == 2) ExitNanoCutscenes.nanoprefix = "Cougher";
        if (cardID == 3) ExitNanoCutscenes.nanoprefix = "Rage";
    }

    void SetPalsInactive() {
        if ("CCC" == UnityEngine.SceneManagement.SceneManager.GetActiveScene().name) {
            GameObject.Find("PALISADERage").SetActive(false);
            GameObject.Find("PALISADEPig").SetActive(false);
            GameObject.Find("PALISADETongue").SetActive(false);
        } else {
            GameObject.Find("PALISADECougher").SetActive(false);
        }
    }

    int mode = 0;
    float t = 0;
	void Update () {
		switch (mode) {
            case 0:
                if (MediumControl.doSpinOutAfterNano == false) {
                    GameObject.Find(sceneToPlay).GetComponent<DialogueAno2>().ext_ForceInteractScriptToParse = true;
                    DataLoader.instance.setDS(sceneToPlay, 1);
                    mode = 1;
                }
                break;
            case 1:
                t += Time.deltaTime;
                if (t > 1) {
                    if (DialogueAno2.AnyScriptIsParsing == false) {
                        if (bed != null) bed.SetActive(true);
                        enabled = false;
                        mode = 2;
                    }
                }
                break;
        }
	}
}
