using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace Anodyne {
    public class BadEnding : MonoBehaviour {

        DialogueBox dbox;
        UIManagerAno2 ui;
        string sceneName;
        int mode = 0;
        public static bool BadEndRunning = false;
        public bool debugTest = false;
        public bool testScreenshakeOff = false;
        bool isCenterChamber = false;
        void Start() {
            if (DataLoader._getDS("start-bad-end") == 1) {
                DataLoader._setDS("start-bad-end", 0);
                print("Starting bad end");
                BadEndRunning = true;
            }
            if (debugTest && Registry.DEV_MODE_ON) {

                if (sceneName != "RingCCC") {
                    debugSoChangeSong = true;
                }
                print("<color=red>Warning, BadEnding being tested</color>");
                BadEndRunning = true;
                if (testScreenshakeOff) {
                    print("Turning screenshake off");
                    SaveManager.screenshake = false;
                }
                   
            }


            ui = HF.Get3DUI();
            HF.GetDialogueBox(ref dbox);
            sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneName == "CenterChamber") isCenterChamber = true;
            if (BadEndRunning) {
                CutsceneManager.deactivatePlayer = true;
                if (!isCenterChamber) {
                    MediumControl player = null;
                    HF.GetPlayer(ref player);
                    player.gameObject.SetActive(false);
                }
                Camera.main.GetComponent<MedBigCam>().enabled = false;
                Camera.main.GetComponent<CinemachineBrain>().enabled = true;
                if (!isCenterChamber) {
                    CmCutTo("BadCam");
                } else {
                    GameObject.Find(Registry.PLAYERNAME3D_Walkscale).transform.position = GameObject.Find("chapel-pos").transform.position;


                    GameObject lookTarget = GameObject.Find("chapel-look");
                    GameObject rotatee = GameObject.Find(Registry.PLAYERNAME3D_Walkscale);
                    Vector3 oldEuler = rotatee.transform.localEulerAngles;
                    rotatee.transform.LookAt(lookTarget.transform.position);
                    oldEuler.y = rotatee.transform.eulerAngles.y;
                    rotatee.transform.localEulerAngles = oldEuler;
                    GameObject.Find("TalkTrigger").transform.position = GameObject.Find("TalkTrigger_posInvis").transform.position;
                    CmCutTo("talkvc2");
                }
                if (sceneName == "RingCCC") {
                    mode = 99;
                    DustMesh = GameObject.Find("DustPrism").transform.Find("Dust").transform;
                    ColorUtility.TryParseHtmlString("#DF684580", out destSkyboxColor);
                    ColorUtility.TryParseHtmlString("#DF684580", out destFogColor);
                    ColorUtility.TryParseHtmlString("#DF684580", out destAmbColor);
                } else {
                    ColorUtility.TryParseHtmlString("#DF684580", out destSkyboxColor);
                    ColorUtility.TryParseHtmlString("#DF684580", out destFogColor);
                    ColorUtility.TryParseHtmlString("#DF684580", out destAmbColor);
                }
                // -104
                // Talk LonweiClone
                if (sceneName == "RingClone") {
                    GameObject lon = GameObject.Find("LonweiClone");
                    Vector3 tempRot = lon.transform.localEulerAngles; tempRot.y = -104f; lon.transform.localEulerAngles = tempRot;
                    lon.GetComponent<Animator>().Play("Talk");
                }
            } else {
                gameObject.SetActive(false);
            }
        }

        Transform DustMesh;
        Vector3 tempVec = new Vector3();
        Vector3 startVec = new Vector3();
        Color startSkyboxColor = new Color();
        Color startFogColor = new Color();
        Color startAmbColor = new Color();
        Color destSkyboxColor = new Color();
        Color destFogColor = new Color();
        Color destAmbColor = new Color();

        bool setColors = false;
        bool debugSoChangeSong = false;
        float t = 0;
        void Update() {

            if (debugSoChangeSong) {
                debugSoChangeSong = false;
                AudioHelper.instance.StopAllSongs();
                AudioHelper.instance.PlaySong("AnodyneEnding", 0, 64,false,0.8f);
            }
            if (mode == 99) {
                AudioHelper.instance.StopAllSongs();
                AudioHelper.instance.PlaySong("AnodyneEnding", 0, 64, false, 0.8f);
                mode = 100;
            } else if (mode == 100 && HF.TimerDefault(ref t, 3)) {
                mode = 101;
                startVec = DustMesh.localPosition;
                tempVec = startVec;
                tempVec.y = -100f;


                Material mat = new Material(RenderSettings.skybox);
                mat.name = mat.name + "(Instance)";
                RenderSettings.skybox = mat;
                startSkyboxColor = mat.GetColor("_Tint");
                startFogColor = RenderSettings.fogColor;
                startAmbColor = RenderSettings.ambientLight;

                //AudioHelper.instance.playOneShot("DustPrismDrain");
            } else if (mode == 101) {
                t += Time.deltaTime;
                float maxT = 5f;
                DustMesh.localPosition = Vector3.Lerp(startVec, tempVec, t / maxT);
                RenderSettings.skybox.SetColor("_Tint", Color.Lerp(startSkyboxColor, destSkyboxColor, t / maxT));
                RenderSettings.ambientLight = Color.Lerp(startAmbColor, destAmbColor, t / maxT);
                RenderSettings.fogColor = Color.Lerp(startFogColor, destFogColor, t / maxT);

                if (t >= 5f) {
                    t = 0;
                    mode = 102;
                }
            } else if (mode == 102) {
                mode = 1;
            }

            if (mode == 0) {
                if (!setColors) {
                    setColors = true;

                    if (!isCenterChamber) {
                        Material mat = new Material(RenderSettings.skybox);
                        mat.name = mat.name + "(Instance)";
                        RenderSettings.skybox = mat;
                        RenderSettings.skybox.SetColor("_Tint", destSkyboxColor);
                        RenderSettings.fogColor = destFogColor;
                        RenderSettings.ambientLight = destAmbColor;
                    }
                }
                if (HF.TimerDefault(ref t, 2f)) {
                    if (sceneName == "DesertOrb") {
                        dbox.playDialogue("bad-end-skel");
                    } else if (sceneName == "RingClone") {
                        dbox.playDialogue("bad-end-clone");
                    } else if (sceneName == "RingGolem") {
                        dbox.playDialogue("bad-end-geof");
                    } else if (sceneName == "CCC") {
                        dbox.playDialogue("bad-end-rage");
                    } else if (isCenterChamber) {
                        dbox.playDialogue("bad-final");
                    }
                    mode = 1;
                    t = 0;
                }
            } else if (mode == 1) {
                if (dbox.isDialogFinished() && isCenterChamber && t == 0) {
                    AudioHelper.instance.StopAllSongs(false);

                    GameObject.Find("PrismConsole").GetComponent<Animator>().enabled = false;
                    GameObject.Find("DustConsole").GetComponent<Animator>().enabled = false;
                    GameObject.Find("CenterNeedle").GetComponent<Animator>().enabled = false;
                    TextureOffsetter.globalfreeze = true;

                    AudioHelper.instance.playOneShot("anodyne-freeze");
                    StartCoroutine(CutsceneManager.lightIntensity("PointLight1", 0, 1f));
                    StartCoroutine(CutsceneManager.lightIntensity("PointLight2", 0, 1f));
                    t = 1f;
                }
                if (dbox.isDialogFinished() && HF.TimerDefault(ref t, 2f)) {
                    mode = 222;

                    ui.MakeFadeUIWhite();
                    if (isCenterChamber) {
                        if (MyInput.shortcut) {
                            ui.StartFade(0.4f, true, 1);
                        } else {
                            ui.StartFade(4f, true, 1);
                        }
                    } else {
                        AudioHelper.instance.playOneShot("anodyne-freeze");
                        ui.StartFade(5, true, 1);
                    }
                }
            } else if (mode == 222) {
                if (isCenterChamber) {
                    if (MyInput.shortcut) {
                        t += Time.deltaTime * 8f;
                    }
                    if (HF.TimerDefault(ref t, 5)) {
                        mode = 2;
                    }
                } else {
                    if (HF.TimerDefault(ref t, 5)) {
                        ui.FadeUIToBlack(1.8f);
                        mode = 2;
                    }
                }

            } else if (mode == 2) {
                if (MyInput.shortcut) t += Time.deltaTime * 3f;
                if (HF.TimerDefault(ref t, 2.5f)) {
                    mode = 200;
                    if (sceneName == "RingCCC") {
                        DataLoader.instance.enterScene("SpireEntrance", Registry.GameScenes.DesertOrb, 0, 0);
                    } else if (sceneName == "DesertOrb") {
                        DataLoader.instance.enterScene("CCCEntrance", Registry.GameScenes.RingClone, 0, 0);
                    } else if (sceneName == "RingClone") {
                        DataLoader.instance.enterScene("CCCEntrance", Registry.GameScenes.RingGolem, 0, 0);
                    } else if (sceneName == "RingGolem") {
                        DataLoader.instance.enterScene("ElevatorEntrance", Registry.GameScenes.CCC, 0, 0);
                    } else if (sceneName == "CCC") {
                        DataLoader.instance.enterScene("chapel-pos", Registry.GameScenes.CenterChamber, 0, 0);
                    } else if (isCenterChamber) {
                        mode = 3;

                        AudioHelper.instance.playOneShot("freeze");
                    }
                }
            } else if (mode == 200) {
                // wait
            } else if (mode == 3) {
                GameObject.Find("CPsalmistFBX").GetComponent<Animator>().enabled = false;
                GameObject.Find("CVisionary").GetComponent<Animator>().enabled = false;
                GameObject.Find("NovaFBX").GetComponent<Animator>().enabled = false;

                setToMat(GameObject.Find("CPsalmistFBX").GetComponentsInChildren<Renderer>());
                setToMat(GameObject.Find("NovaFBX").GetComponentsInChildren<Renderer>());
                setToMat(GameObject.Find("Level").GetComponentsInChildren<Renderer>());
                setToMat(GameObject.Find("DustConsole").GetComponentsInChildren<Renderer>());
                setToMat(GameObject.Find("PrismConsole").GetComponentsInChildren<Renderer>());
                setToMat(GameObject.Find("CVisionary").GetComponentsInChildren<Renderer>());

                GameObject.Find("PointLight1").gameObject.SetActive(false);
                GameObject.Find("PointLight2").gameObject.SetActive(false);
                CmCutTo("talkvc3");
                // make screen grey
                if (MyInput.shortcut) {
                    ui.StartFade(1, false);
                } else {
                    ui.StartFade(6, false);
                }
                mode = 4;
            } else if (mode == 4) {
                if (MyInput.shortcut) t = 4f;
                if (HF.TimerDefault(ref t, 4f)) {
                    mode = 5;
                    if (MyInput.shortcut) t = 1f;
                }
            } else if (mode == 5) {
                t += Time.deltaTime;
                if (t > 1 && (MyInput.jpConfirm || MyInput.jpCancel || MyInput.jpSpecial || MyInput.jpTalk)) {
                    t = 0;
                    mode = 6;
                    theEndText = GameObject.Find("TheEndText").GetComponent<TMPro.TMP_Text>();
                    theEndText.text = DataLoader.instance.getRaw("the-end-only", 0) + "\n";
                    theEndText.text += "<size=16>(c) 2019 Analgesic Productions";
                }
            } else if (mode == 6) {
                if (HF.TimerDefault(ref t,0.5f)) {
                    colFactor += 0.1f;
                    theEndText.color = Color.Lerp(Color.black, Color.white, colFactor / 1f);
                }
                if (colFactor >= 1) {
                    mode = 7;
                }
            } else if (mode == 7) { // wait for input
                if (MyInput.jpConfirm || MyInput.jpCancel || MyInput.jpSpecial || MyInput.jpTalk) {
                    mode = 8;
                    ui.MakeFadeUIBlack();
                    ui.StartFade(3, true, 1);
                }
            } else if (mode == 8) {
                if (HF.TimerDefault(ref t, 5f)) {
                    dbox.playDialogue("bad-marina");
                    mode = 9;
                }
            } else if (mode == 9) {
                if (dbox.isDialogFinished()) {
                    mode = 10;
                }
            } else if (mode == 10) {
                // show credits?
                BadEndRunning = false;
                CutsceneManager.deactivatePlayer = false;
                DataLoader.instance.enterScene("", "Title", 0, 0);
                mode = 11;
            }
        }
        CinemachineVirtualCamera ActiveVirtualCamera;
        private void CmCutTo(string cameraName) {
            CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
            brain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
            if (ActiveVirtualCamera != null) ActiveVirtualCamera.Priority = 0;
            ActiveVirtualCamera = GameObject.Find(cameraName).GetComponent<CinemachineVirtualCamera>();
            if (!SaveManager.screenshake && ActiveVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>() != null) {
                ActiveVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0;
            }
            ActiveVirtualCamera.Priority = 100;
        }
        public Material frozenMat;
        TMPro.TMP_Text theEndText;
        float colFactor = 0;
        public void setToMat(Renderer[] rs) {
            foreach (Renderer r in rs) {
                r.material = frozenMat;
            }

        }
    }
}
