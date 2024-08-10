using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Wormhole2D : MonoBehaviour {

    List<Image> images;
    public GameObject wormholeImagePrefab;
    public float maxScale = 30f;
    public float scaleSpeed = 10f;
     float finalColorScale = 30f;
    Color initColor;
    Color finalColor;
    
    public static bool ReturningFromPico = false;
    public float interval = 0.2f;
    float t_interval = 0;
    Vector3 tempScale = new Vector3();

    float t_effect = 0;
    float tm_effect = 2.3f;

    // float t_oscSpawnPoint = 0;
    // float tm_oscSpawnPoint = 4f;
    // float oscAmplitude = 40f;
    public float rotationSpeed = 30f;
    public float parentRotationSpeed = 15f;

    public static string nextTintName = "default";

    void Start () {
        transform.SetParent(GameObject.Find("DoesScale").transform, true);
        transform.localScale = new Vector3(1, 1, 1);
        transform.SetAsLastSibling();
        images = new List<Image>();
        for (int i = 0; i < 60; i++) {
            GameObject g = Instantiate(wormholeImagePrefab, transform);
            images.Add(g.GetComponent<Image>());
            //SetAlpha(0, g.GetComponent<Image>());
            g.GetComponent<Image>().enabled = false;
        }

        for (int i = 0; i < 600; i++) {
            UpdateGFX(0.0167f);
        }

        foreach (Worm2DData data in colordatas) {
            if (data.tintName == nextTintName) {
                finalColor = data.finalColor;
                initColor = data.initColor;
                finalColorScale = data.finalColorScale;
            }
        }

        Camera.main.backgroundColor = initColor;


        if (Anodyne.GlandilockBoss.diedOnce) {
            if (SparkGameController.SparkGameDestScene == Registry.GameScenes.PicoZera) {
                AudioHelper.instance.FadeSong(Anodyne.GlandilockBoss.BossSongName, 1, 0);
                AudioHelper.instance.FadeSong(Anodyne.GlandilockBoss.PicoBossSongName, 1,1);
            } else {
                AudioHelper.instance.FadeSong(Anodyne.GlandilockBoss.BossSongName, 1, 1);
                AudioHelper.instance.FadeSong(Anodyne.GlandilockBoss.PicoBossSongName, 1, 0);
            }
        }
    }
    public float accel = 1.025f;

    int mode = 0;
    Color tempCol;
    void SetAlpha(float a, Image i) {
        tempCol = i.color;
        tempCol.a = a;
        i.color = tempCol;
    }


    private void UpdateGFX(float time) {

        //  t_oscSpawnPoint += time;
        // if (t_oscSpawnPoint > tm_oscSpawnPoint) t_oscSpawnPoint -= -tm_oscSpawnPoint;

        tempScale = transform.localEulerAngles;
        tempScale.z += time * parentRotationSpeed;
        transform.localEulerAngles = tempScale;

        if (HF.TimerDefault(ref t_interval, interval,time)) {
            t_interval = 0;
            for (int i = 0; i < images.Count; i++) {
                if (images[i].enabled == false) {
                    images[i].enabled = true;
                    tempScale.Set(0, 0, 0);
                    images[i].transform.localScale = tempScale;
                    images[i].transform.SetAsFirstSibling();
                    images[i].transform.localEulerAngles = tempScale;
                    images[i].color = initColor;
                    //                tempPos.Set(0, 0, 1);
                    //              tempPos.x = -oscAmplitude + 2 * oscAmplitude * Mathf.Sin((t_oscSpawnPoint / tm_oscSpawnPoint) * 6.28f);
                    //            images[i].transform.localPosition = tempPos;
                    break;
                }
            }
        }

        for (int i = 0; i < images.Count; i++) {
            if (images[i].enabled == true) {
                tempScale = images[i].transform.localScale;
                tempScale.x += time* scaleSpeed;
                tempScale.x *= accel;
                tempScale.y = tempScale.z = tempScale.x;
                images[i].transform.localScale = tempScale;

                if (tempScale.x >= maxScale) {
                    images[i].enabled = false;
                }

                tempCol = Color.Lerp(initColor, finalColor, tempScale.z / finalColorScale);
                images[i].color = tempCol;

                tempScale = images[i].transform.localEulerAngles;
                tempScale.z += time * rotationSpeed;
                images[i].transform.localEulerAngles = tempScale;

            }
        }
    }
    public Worm2DData[] colordatas;
    [System.Serializable]
    public class Worm2DData {
        public string tintName;
        public Color initColor;
        public Color finalColor;
        public float finalColorScale;
    }

    bool dochangescene = false;
    bool didchangescene = false;
    public bool neverEnd = false;
	void Update () {
        if (mode == 0) {
            t_interval = interval;
            mode = 1;
        } else if (mode == 1) {
            UpdateGFX(Time.deltaTime);

            t_effect += Time.deltaTime;
            if (MyInput.shortcut) {
                t_effect = tm_effect;
            }
            if (t_effect >= tm_effect && !neverEnd) {
                dochangescene = true;
            }
            if (MyInput.jpConfirm || MyInput.jpCancel || SaveManager.dialogueSkip) {
                t_effect += 1.5f;
            }
            if (dochangescene && !didchangescene) {
                didchangescene = true;
                DataLoader.instance.enterScene(SparkGameController.SparkGameDestObjectName, SparkGameController.SparkGameDestScene, 0.25f, -1);
            }
        }
	}
}
