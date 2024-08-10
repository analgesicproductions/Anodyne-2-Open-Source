using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// assumes you have a 1 unit by 1 unit quad, changes material's tiling so that
// the texture apperas at the correct size undistorted
public class QuadAutosetTexScale : MonoBehaviour {

    [Tooltip("The bigger this is, the bigger the textre appears")]
    public float textureSize = 1f;
    
	// Use this for initialization
	void Start () {
        Material mat = GetComponent<MeshRenderer>().material;
        mat.SetTextureScale("_MainTex", new Vector2(transform.localScale.x/textureSize,transform.localScale.y/textureSize));
      
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
