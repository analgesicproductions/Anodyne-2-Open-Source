using UnityEngine;
using System.Collections;

public class PixelizePPE : MonoBehaviour {
    Material material;
    [Range(1,20)]
    public float strength = 1;
    float maxStrength = 20;
    public bool title = false;
    bool doPixelize = false;
    bool doUnpixelize = true;
    float tTest = 0;
    float tmTest = 0.4f;

    void Awake() {
        material = new Material(Shader.Find("Hidden/Pixelize"));
        if (title) {
            maxStrength = 10;
        }
    }

    public void Pixelize(float t=-1) {
        if (t == -1) return;
        if (t != -1) tmTest = t;
        doPixelize = true;
        tTest = 0;
    }
    public void Unpixelize(float t = -1) {
        if (t == -1) return;
        if (t != -1) tmTest = t;
        doUnpixelize = true;
        tTest = 0;
    }

    private void Update() {
        if (doUnpixelize) {
            if (HF.TimerDefault(ref tTest,tmTest,Time.deltaTime)) {
                doUnpixelize = false;
                strength = 1;
                if (title) {
                    maxStrength = 20;
                }
            } else {
                strength = Mathf.Lerp(maxStrength, 1, tTest / tmTest);
            }
        }

        if (doPixelize) {
            if (HF.TimerDefault(ref tTest, tmTest, Time.deltaTime)) {
                doPixelize = false;
                strength = maxStrength;
            } else {
                strength = Mathf.Lerp(1, maxStrength, tTest / tmTest);
            }
        }
    }

    bool startedEffect = false;
    int scale = 3;
    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if (strength <= 1) {
            Graphics.Blit(source, destination);
            if (startedEffect) {
                startedEffect = false;
            }
            return;
        }
        scale = Mathf.FloorToInt(source.height / 200);

        startedEffect = true;
        material.SetFloat("_Strength", Mathf.Round(strength));
        material.SetInt("_Scale", scale);
        material.SetFloat("_W", source.width);
        material.SetFloat("_H", source.height);
        Graphics.Blit(source, destination, material, 0);
    }
}