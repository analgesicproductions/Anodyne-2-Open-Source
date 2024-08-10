using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WavyTranspAdditiveEffect : MonoBehaviour {

    Material material;
    public int queue = 3000;
    [Range(0f, 1f)]
    public float transparency = 0.7f;
    [Range(0, 0.025f)]
    public float crunch = 0.005f;
    [Range(0, 10f)]
    public float contraction = 3f;
    [Range(0f, 100f)]
    public float speed = 50f;
    [Range(0f, 10f)]
    public float xOscillation = 4f;
    // Use this for initialization
    void Start () {
        if (name == "Chalaza1") {
            queue = 3002;
        } else if (name == "Chalaza1 (1)") {
            queue = 3001;
        }
       // GetComponent<MeshRenderer>().material = new Material(Shader.Find("Unlit/UnlitWavyTranspAdditive"));
        material = GetComponent<MeshRenderer>().material;
    }
	
	// Update is called once per frame
	void Update () {
        material.renderQueue = queue;
        material.SetFloat("_Transparency", transparency);
        material.SetFloat("_Crunch", crunch);
        material.SetFloat("_Contraction", contraction);
        material.SetFloat("_XOscillation", xOscillation);
        material.SetFloat("_Speed",speed);
    }
}
