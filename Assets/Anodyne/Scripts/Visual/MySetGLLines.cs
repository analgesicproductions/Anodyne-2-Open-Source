using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MySetGLLines : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
		Mesh  mesh = GetComponent<MeshFilter>().mesh;
		int[] indices = mesh.GetIndices(0);
		mesh.SetIndices(indices,MeshTopology.LineStrip,0);
		mesh.RecalculateBounds();
		GetComponent<MeshFilter>().mesh = mesh;

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
