using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollParallaxPlane : MonoBehaviour {

    [Tooltip("Negative is further to the BG")]
    [Range(-10,10)]
    public int layerOrder = -1;
    bool InForeground = false;
    public MyBlendMode blendMode = MyBlendMode.Normal;
    public enum MyBlendMode { Normal, Add, Multiply, ADVNormal, ADVOverlay, ADVSoftLight, ADVGreyscale}

    [Range(0, 1)]
    public float transparency = 1;
    [Range(0, 1)]
    public float colorMultiplier = 1;
    public Vector2 parallax = new Vector2(1, 1);
    public Vector2 scrollingSpeed = new Vector2(0, 0);
    Camera cam;
    MeshRenderer sr;
    [Tooltip("Texture in the material will be replaced with this when the game runs. Useful if you don't want to make a bunch of materials and stuff.")]
    public Texture OverrideTexture;
    [Header("Custom Tiling & Scaling")]
    public bool autoscale = true;
    public Vector2 nonAutoscaleSize = new Vector2(12, 12);
    public bool useCustomTiling = false;
    public Vector2 customTiling = new Vector2(1, 1);
    Material mat;

    static List<int> usedBGLayerIndices = new List<int>();

    private void Awake() {
        usedBGLayerIndices = new List<int>();
    }


    void Start () {
        sr = GetComponent<MeshRenderer>();
        cam = Camera.main;
        mat = sr.material;
        if (usedBGLayerIndices.Contains(layerOrder)) {
            print("<color=red>WARNING: Two BG layers share the same index: </color>" + layerOrder);
        } else {
            usedBGLayerIndices.Add(layerOrder);
        }

            Vector3 newScale = new Vector3();
        if (autoscale) {
            newScale.x = -OverrideTexture.width / 160f; // Default quad mesh is 10x10 units, game PPU is 16
            newScale.z = -OverrideTexture.height / 160f;
        } else {
            newScale.x = -nonAutoscaleSize.x / 10f;
            newScale.z = -nonAutoscaleSize.y / 10f;
        }
        transform.localScale = newScale;

        SetLayer();
        SetBlendModeInShader(blendMode);

        if (OverrideTexture != null) mat.SetTexture("_MainTex", OverrideTexture);
        pxWidth = mat.GetTexture("_MainTex").width;
        pxHeight = mat.GetTexture("_MainTex").height;
        if (useCustomTiling) {
            mat.mainTextureScale = customTiling;
        }
        initpos = transform.position;
    }

    void SetLayer() {
        InForeground = true;
        if (layerOrder < 0) InForeground = false;
        sr.sortingLayerID = SortingLayer.NameToID("Background");
        if (InForeground) sr.sortingLayerID = SortingLayer.NameToID("Foreground");
    }
    private void OnValidate() {
        if (sr == null) return;
        SetLayer();
        SetBlendModeInShader(blendMode);   
    }

    void SetBlendModeInShader(MyBlendMode blendmode) {
        if (mat != null) {
            if (blendmode == MyBlendMode.Normal) {
                mat.SetInt("SourceMode", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("DestinationMode", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            } else if (blendmode == MyBlendMode.Add) {
                mat.SetInt("SourceMode", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("DestinationMode", (int)UnityEngine.Rendering.BlendMode.One);
            } else if (blendmode == MyBlendMode.Multiply) {
                mat.SetInt("SourceMode", (int)UnityEngine.Rendering.BlendMode.DstColor);
                mat.SetInt("DestinationMode", (int)UnityEngine.Rendering.BlendMode.Zero);
            } else if (blendmode == MyBlendMode.ADVGreyscale) { 

            } else if (blendmode == MyBlendMode.ADVOverlay) {
                mat.SetInt("_OpMode", 23); // Unity...rendering....overlay
            } else if (blendmode == MyBlendMode.ADVSoftLight) {
                mat.SetInt("_OpMode", 29);
            } else if (blendmode == MyBlendMode.ADVNormal) {
                mat.SetInt("_OpMode", 0);
            }
        }
    }

    Vector3 initpos = new Vector3();
    int pxWidth;
    int pxHeight;
    Vector2 offset = new Vector2();
    Vector2 steppedOffset = new Vector2();
    Vector3 newpos = new Vector3();
    // Update is called once per frame
    void LateUpdate() {
        if (scrollingSpeed.x != 0 || scrollingSpeed.y != 0) {
            offset.x += Time.deltaTime * -scrollingSpeed.x;
            offset.y += Time.deltaTime * -scrollingSpeed.y;
            if (offset.y > 1) offset.y--;
            if (offset.x > 1) offset.x--;
            if (offset.y < -1) offset.y++;
            if (offset.x < -1) offset.x++;

            steppedOffset.x = Mathf.Floor(offset.x * pxWidth) / pxWidth;
            steppedOffset.y = Mathf.Floor(offset.y * pxHeight) / pxHeight;

            mat.mainTextureOffset = steppedOffset;
        }
        mat.SetFloat("_Transparency", transparency);
        if (blendMode == MyBlendMode.Add) {
            mat.SetFloat("_ColorMultiplier", colorMultiplier);
        }
        float dx = cam.transform.position.x - initpos.x;
        float dy = cam.transform.position.y - initpos.y;
        newpos.x = cam.transform.position.x - dx * parallax.x;
        newpos.y = cam.transform.position.y - dy * parallax.y;
        newpos.z = -layerOrder * 0.05f;
        transform.position = newpos;

        if (fadeColorMulMode == 1) {
            t_fadeColorMul += Time.deltaTime;
            if (t_fadeColorMul > tm_fadeColorMul) {
                t_fadeColorMul = tm_fadeColorMul;
                fadeColorMulMode = 0;
            }
            colorMultiplier = Mathf.Lerp(fadeColorMul_start, fadeColorMul_end, t_fadeColorMul / tm_fadeColorMul);
        }
	}

    int fadeColorMulMode = 0;
    float t_fadeColorMul;
    float tm_fadeColorMul;
    float fadeColorMul_start = 0;
    float fadeColorMul_end = 0;
    public void fadeColorMul(float start, float end, float t) {
        if (end == colorMultiplier) {
            return;
        }
        fadeColorMul_start = start;
        fadeColorMul_end = end;
        tm_fadeColorMul = t;
        t_fadeColorMul = 0;
        fadeColorMulMode = 1;
    }

}
