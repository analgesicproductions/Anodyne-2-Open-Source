using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Wormhole : MonoBehaviour {

    public static bool ReturningFrom2D = false;
    [Header("Debugging")]
    public bool DebugFunctionalityOn = false;
    public bool Backwards = false;
    public bool NeverEnd = false;
    public string TestDestName = "Entrance";
    public Registry.GameScenes TestDestScene = Registry.GameScenes.Cougher;

    public enum WormholeMovement { Straight, UpDownDefault, LeftRightDefault};

    [System.Serializable]
    public class WormholeData {
        public Registry.GameScenes destination;
        public WormholeMovement movementType = WormholeMovement.LeftRightDefault;
        //public GameObject optionalRingModel = null;
        public Material optionalRingMat = null;
        public Material optionalDustMat = null;
        public Color pointLightCol = new Color(126/255f,198/255f,234/255f,1);
        public float pointLightIntensity = 1.05f;
        public float pointLightRange = 18f;
        public Color ambientCol = new Color(20/255f,60/255f,69/255f,1);
        public Color fogCol = new Color(64/255f,100/255f,114/255f,1);
        public float zRotation = 360f;
        public Color bgColor = new Color();
        public Vector2 dustFade = new Vector2(2f, 4f);
        public float dustfadeIn = 10f;
    }
    [Header("Visuals")]
    public Vector2 dustFadeOut = new Vector2(2.5f, 4f);
    public float dustFadeIn = 9f;
    public WormholeData[] datas;
    public WormholeMovement movementType = WormholeMovement.LeftRightDefault;
    public Vector2 xoffsetRange = new Vector2(-24, 24);
    public Vector2 yRotationRange = new Vector2(40, -40);
    public Vector2 yOffsetRange = new Vector2(-24, 24);
    public Vector2 xRotationRange = new Vector2(40, -40);
    float t_osc;
    public float oscillationTime = 6f;
    [Tooltip("Wait at peaks of the oscillation")]
    public float oscillationHoldTime = 1.5f;
    float t_oscHold = 0;

    [Header("General")]
    public TMP_Text text_nowEntering;
    public TMP_Text text_description;
    public GameObject ParticleRingPrefab;
    GameObject[] rings;
    float[] lifespans;
    float[] t_reduceOffsets;

    Vector3[] nonOffsetPositions;

    public float spawnInterval = 1f;
    public float lifespan = 5f;
    
    float t_spawn;
    public Transform spawnPoint;
    public Transform destinationPoint;
    public float speed = 3f;
    public Vector3 spawnOffset = new Vector3(0, 0,0);
    public Vector3 spawnRotation = new Vector3(0, 0, 0);
    public float tm_ReduceOffset = 1f;
    Vector3 initRot;
    int otherInfo = 0;

    public float lerpEndDis = 3f;

    Vector3 vel = new Vector3();
    Vector3 tempPos = new Vector3();
    Material dustOverrideMat;
    Material ringOverrideMat;
    Color tempCol = new Color();
    private float dustStartAlpha;
    MaterialPropertyBlock mpb;
    void Start() {
        if (!DebugFunctionalityOn) {
            Backwards = false;
        }
        if (ReturningFrom2D) {
            MediumControl.doSpinOutAfterNano = true;
            Backwards = true;
        }


        if (Registry.DEV_MODE_ON && DebugFunctionalityOn) {
            SparkGameController.SparkGameDestScene = TestDestScene;
            SparkGameController.SparkGameDestObjectName = TestDestName;
            SparkGameController.Play2DBeamInEffect = true;
        }
        if (SparkGameController.SparkGameDestScene == Registry.GameScenes.NanoAlbumen || SparkGameController.SparkGameDestScene == Registry.GameScenes.NanoSanctuary) {
            yolkSkip = true;
            //if (SparkGameController.SparkGameDestObjectName == "Yolk1Door") otherInfo = 1;
            //if (SparkGameController.SparkGameDestObjectName == "Yolk2Door") otherInfo = 2;
            //if (SparkGameController.SparkGameDestObjectName == "Yolk3Door") otherInfo = 3;
        }
        if (SparkGameController.SparkGameDestScene == Registry.GameScenes.NanoNexus && SparkGameController.SparkGameDestObjectName == "FantasyEntrance") {
            otherInfo = 1;
        }



        Light l = GameObject.Find("WormholeLight").GetComponent<Light>();
        Registry.GameScenes dataKey = SparkGameController.SparkGameDestScene;
        if (ReturningFrom2D) dataKey = SparkGameController.ReturnFrom2D_SourceScene;
        foreach (WormholeData wd in datas) {
            if (wd.destination == dataKey) {
                //      if (wd.optionalRingModel != null) ParticleRingPrefab = wd.optionalRingModel;
                movementType = wd.movementType;
                if (wd.optionalDustMat != null) dustOverrideMat = wd.optionalDustMat;
                if (wd.optionalRingMat != null) ringOverrideMat = wd.optionalRingMat;
                l.intensity = wd.pointLightIntensity;
                l.range = wd.pointLightRange;
                l.color = wd.pointLightCol;
                RenderSettings.fogColor = wd.fogCol;
                RenderSettings.ambientLight = wd.ambientCol;
                spawnRotation.z = wd.zRotation;
                wd.bgColor.a = 1;
                Camera.main.backgroundColor = wd.bgColor;
                dustFadeOut.Set(wd.dustFade.x, wd.dustFade.y);
                dustFadeIn = wd.dustfadeIn;
                break;
            }
        }

        text_description.alpha = 0;
        text_nowEntering.alpha = 0;
        dust1Renderers = new MeshRenderer[30];
        dust2Renderers = new MeshRenderer[30];
        nonOffsetPositions = new Vector3[30];
        lifespans = new float[30];
        t_reduceOffsets = new float[30];
        rings = new GameObject[30];
        for (int i = 0; i < 30; i++) {
            GameObject ring = Instantiate(ParticleRingPrefab);
            initRot = ring.transform.localEulerAngles;
            Transform dust1 = ring.transform.Find("TubeFloaters");
            Transform dust2 = ring.transform.Find("TubeFloaters (1)");

            tempPos = dust1.localEulerAngles;
            tempPos.x = 360 * Random.value;
            dust1.localEulerAngles = tempPos;

            tempPos = dust2.localEulerAngles;
            tempPos.x = 360 * Random.value;
            dust2.localEulerAngles = tempPos;



            if (dustOverrideMat != null) dust1.GetComponent<MeshRenderer>().sharedMaterial = dustOverrideMat;
            if (dustOverrideMat != null) dust2.GetComponent<MeshRenderer>().sharedMaterial = dustOverrideMat;

            dust1Renderers[i] = dust1.GetComponent<MeshRenderer>();
            dust2Renderers[i] = dust2.GetComponent<MeshRenderer>();


            if (ringOverrideMat != null) ring.transform.Find("Tube").GetComponent<MeshRenderer>().material = ringOverrideMat;

            rings[i] = ring;
            ring.SetActive(false);
        }
        mpb = new MaterialPropertyBlock();
        tempCol = dust1Renderers[0].GetComponent<MeshRenderer>().sharedMaterial.color;
        dustStartAlpha = tempCol.a;
        tempCol.a = 0;
        mpb.SetColor("_Color", tempCol);
        for (int i = 0; i < dust1Renderers.Length; i++) {
            dust1Renderers[i].SetPropertyBlock(mpb);
            dust2Renderers[i].SetPropertyBlock(mpb);
        }


        if (Backwards) {
            tempPos = spawnPoint.position;
            spawnPoint.position = destinationPoint.position;
            destinationPoint.position = tempPos;
        }
        for (int i = 0; i < 400; i++) {
            UpdateRings(0.0167f);
        }
    }
    float t_waitToEnter = 0;
    bool yolkSkip = false;
    int mode = 0;
    bool songstuffdone = false;
    public static bool skipEpisodeTitle = false;

	void Update () {


        bool fastText = (MyInput.shortcut || SaveManager.dialogueSkip) ;
            if (!songstuffdone) {
            songstuffdone = true;
            AudioHelper.instance.StopAllSongs(true);
            if (SparkGameController.SparkGameDestScene != Registry.GameScenes.NanoHorror) {
                AudioHelper.instance.PlaySong("Wormhole", 0, 13);
            }
        }

        UpdateRings(Time.deltaTime);
        bool debugSkip = Registry.DEV_MODE_ON && Input.GetKeyDown(KeyCode.Escape);

        if (debugSkip && mode <= 5) {
            mode = 5;
        }
        // Update texts and transition scene
        if (mode == 0) {
            if (DebugFunctionalityOn && NeverEnd) {

            } else {
                if (MyInput.jpCancel || MyInput.jpConfirm) {
                    t_waitToEnter += 2.5f;
                }
                if (yolkSkip || skipEpisodeTitle) {
                    if (fastText || HF.TimerDefault(ref t_waitToEnter, 5f)) {
                        skipEpisodeTitle = false;
                        mode = 5;
                        text_description.alpha = 0;
                        text_nowEntering.alpha = 0;
                    }
                } else if (Backwards) {
                    if (fastText) t_waitToEnter = 4.4f;
                    if (HF.TimerDefault(ref t_waitToEnter, 4.4f)) {
                        mode = 5;
                    }
                } else if (fastText || HF.TimerDefault(ref t_waitToEnter, 2.4f)) {
                    mode = 1;
                   // sound
                    text_nowEntering.text = HF.GetSceneAssociatedText(SparkGameController.SparkGameDestScene, "episodeTitle",otherInfo);
                    text_nowEntering.text = text_nowEntering.text.Replace("\\n", "\n");
                }
            }
        } else if (mode == 1) {
            // fade in title
            text_nowEntering.alpha += Time.deltaTime * 0.75f;
            if (fastText) text_nowEntering.alpha = 1f;
            if (MyInput.jpConfirm) text_nowEntering.alpha = 1f;
            if (text_nowEntering.alpha >= 1) {
                mode = 2;
                t_waitToEnter = 0;

                if (fastText) t_waitToEnter = 0.6f;
            }
        } else if (mode == 2 && HF.TimerDefault(ref t_waitToEnter, 0.6f)) {
            // sound?
            mode = 3;
            text_description.text = HF.GetSceneAssociatedText(SparkGameController.SparkGameDestScene,"episodeDescription",otherInfo);
        } else if (mode == 3) {
            // fade in title
            text_description.alpha += Time.deltaTime;
            if (fastText) text_description.alpha = 1f;
            if (MyInput.jpConfirm) text_description.alpha = 1f;
            if (text_description.alpha >= 1) {
                mode = 4;
            }
        } else if (mode == 4) {
            if (MyInput.jpCancel || MyInput.jpConfirm) {
                t_waitToEnter += 1.6f;
            }
            if (fastText) t_waitToEnter = 3.2f;
            if (HF.TimerDefault(ref t_waitToEnter, 3.2f)) {
                mode = 5;
            }
        } else if (mode == 5) {
            text_description.alpha -= Time.deltaTime;
            text_nowEntering.alpha -= Time.deltaTime;
            if (fastText) text_description.alpha = 0;
            if (MyInput.jpConfirm) text_description.alpha = 0;
            if (debugSkip|| text_description.alpha <= 0) {
                mode = 6;
                ReturningFrom2D = false;
                float fadetime = 1.4f;
                if (debugSkip || fastText) fadetime = 0.05f;
                int nrRingNPCsDone = 0;
                if (DataLoader.instance.getDS("CARD4") == 1) nrRingNPCsDone++;
                if (DataLoader.instance.getDS("CARD6") == 1) nrRingNPCsDone++;
                if (DataLoader.instance.getDS("CARD8") == 1) nrRingNPCsDone++;
                if (DataLoader.instance.getDS("end-ring") == 1) {
                    MediumControl.doSpinOutAfterNano = false;
                }
                if (DataLoader.instance.getDS("ring-pal-1") == 0 && nrRingNPCsDone >= 1) {
                    UIManagerAno2.sceneStartFadeHoldTime = 0.5f;
                    MediumControl.doSpinOutAfterNano = false;
                    DataLoader.instance.enterScene("PalSceneEntry", Registry.GameScenes.DesertSpire, fadetime, -1);
                } else if (DataLoader.instance.getDS("ring-pal-2") == 0 && nrRingNPCsDone >= 3) {
                    UIManagerAno2.sceneStartFadeHoldTime = 0.5f;
                    MediumControl.doSpinOutAfterNano = false;
                    DataLoader.instance.enterScene("PalSceneEntry2", Registry.GameScenes.DesertSpireTop, fadetime, -1);
                } else {
                    if (SparkGameController.SparkGameDestScene == Registry.GameScenes.NanoDustbound) {
                        UIManagerAno2.sceneStartFadeHoldTime = 0.5f;
                    }
                    DataLoader.instance.enterScene(SparkGameController.SparkGameDestObjectName, SparkGameController.SparkGameDestScene, fadetime, -1);
                }
            }
        }
    }

    int movementSubmode = 0;
    bool held90 = false;
    bool held270 = false;
    MeshRenderer[] dust1Renderers;
    MeshRenderer[] dust2Renderers;

    void UpdateRings(float delta) {

        if (HF.TimerDefault(ref t_spawn, spawnInterval,delta)) {
            GameObject nextObj = null;
            for (int i = 0; i < rings.Length; i++) {
                if (!rings[i].activeInHierarchy) {
                    nextObj = rings[i];
                    lifespans[i] = lifespan;
                    t_reduceOffsets[i] = tm_ReduceOffset;
                    vel = (destinationPoint.position - spawnPoint.position).normalized * speed;
                    nextObj.SetActive(true);
                    nextObj.transform.position = spawnPoint.position;
                    nonOffsetPositions[i] = nextObj.transform.position;
                    break;
                }
            }
        }

        if (movementType == WormholeMovement.Straight) {

        } else {
            if (movementSubmode == 0) {
                t_osc += Time.deltaTime;
                if (t_osc > oscillationTime) {
                    held90 = held270 = false;
                    t_osc -= oscillationTime;
                }

                float degrees = (t_osc / oscillationTime) * 360f;
                if (degrees >= 90 && !held90) {
                    held90 = true;
                    movementSubmode = 1;
                } else if (degrees >= 270 && !held270) {
                    held270 = true;
                    movementSubmode = 1;
                }
                float sinValue01 = Mathf.Sin(Mathf.Deg2Rad * degrees) + 1;
                sinValue01 /= 2f;

                if (movementType == WormholeMovement.LeftRightDefault) {
                    spawnOffset.x = xoffsetRange.x + sinValue01 * (xoffsetRange.y - xoffsetRange.x);
                    spawnRotation.y = yRotationRange.x + sinValue01 * (yRotationRange.y - yRotationRange.x);
                } else if (movementType == WormholeMovement.UpDownDefault) {
                    spawnOffset.y = yOffsetRange.x + sinValue01 * (yOffsetRange.y - yOffsetRange.x);
                    spawnRotation.x = xRotationRange.x + sinValue01 * (xRotationRange.y - xRotationRange.x);
                }
            } else if (movementSubmode == 1) {
                if (HF.TimerDefault(ref t_oscHold,oscillationHoldTime)) {
                    movementSubmode = 0;
                }
            }
        }

        GameObject g = null;
        for (int i = 0; i < rings.Length; i++) {

            if (rings[i].activeInHierarchy) {
                g = rings[i];
                lifespans[i] -= delta;
                if (lifespans[i] <= 0) {
                    g.SetActive(false);
                } else {
                    tempPos = nonOffsetPositions[i];
                    tempPos += delta * vel; // move towards end
                    nonOffsetPositions[i] = tempPos; // stores position without offset x/y position.

                    float disFromEnd = 0;
                    float distanceFromStartToTend = 0;
                    // flipped, as spawn and dest points have been flipped so the rings move backwards, but so the offsets stay the same
                    if (Backwards) {
                        disFromEnd = tempPos.z - spawnPoint.position.z;
                        distanceFromStartToTend = destinationPoint.position.z - spawnPoint.position.z;
                    } else {
                        disFromEnd = tempPos.z - destinationPoint.position.z;
                        distanceFromStartToTend = spawnPoint.position.z - destinationPoint.position.z;
                    }
                    disFromEnd -= lerpEndDis;
                    distanceFromStartToTend -= lerpEndDis;
                    
                    if (disFromEnd < dustFadeIn  && disFromEnd >= dustFadeOut.y) {
                        float r = (disFromEnd - dustFadeOut.y) / (dustFadeIn - dustFadeOut.y);
                        tempCol.a =dustStartAlpha * (1-r);
                        mpb.SetColor("_Color", tempCol);
                        dust1Renderers[i].SetPropertyBlock(mpb);
                        dust2Renderers[i].SetPropertyBlock(mpb);
                    } else if (disFromEnd < dustFadeOut.y) {
                        float r = (disFromEnd - dustFadeOut.x) / (dustFadeOut.y - dustFadeOut.x);
                            if (r < 0) r = 0;
                            tempCol.a = dustStartAlpha * r;
                        mpb.SetColor("_Color", tempCol);
                        dust1Renderers[i].SetPropertyBlock(mpb);
                        dust2Renderers[i].SetPropertyBlock(mpb);
                    }

                    if (disFromEnd < 0) disFromEnd = 0;
                    tempPos += spawnOffset * Mathf.SmoothStep(0, 1, disFromEnd / distanceFromStartToTend);
                    g.transform.position = tempPos;

                    tempPos = spawnRotation * Mathf.SmoothStep(0, 1, disFromEnd / distanceFromStartToTend);
                    tempPos = initRot + tempPos;
                    g.transform.localEulerAngles = tempPos;
                }
            }
        }
    }
}
