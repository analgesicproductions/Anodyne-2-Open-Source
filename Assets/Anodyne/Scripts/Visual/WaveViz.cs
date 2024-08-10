using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveViz : MonoBehaviour {

	Mesh mesh;
	Vector3[] initVerts;
	int[] indices;

	public int quality = 50;
	public float unitWidth = 1f;

	void Start () {
		mesh = new Mesh();
		initVerts = new Vector3[quality+1];

		for (int i = 0; i <= quality; i++) {
			initVerts[i] = new Vector3((i/(1.0f*quality))*unitWidth,0,0);
		}
		indices = new int[quality*2];

		for (int i = 0; i < quality; i++) {
			indices[i*2] = i;
			indices[i*2+1] = i+1;
		}

		mesh.vertices = initVerts;
		mesh.SetIndices(indices,MeshTopology.Lines,0);
		mesh.RecalculateBounds();

		GetComponent<MeshFilter>().mesh = mesh;
	}


	public float period = 4f;
	float yAmp = 1f;
	float timer = 0f;
	void Update () {
		timer += Time.deltaTime;
		if (timer >= period) timer -= period;

		mesh = GetComponent<MeshFilter>().mesh;
		Vector3[] vertices = mesh.vertices;
		int i = 0;
		//
		float t = 0f;
		float offset_t = 0f;
		while (i < vertices.Length) {
			t = timer;
			// Use the x-position on the mesh to offset the timing of the oscillation
			offset_t = period * (i*1.0f/vertices.Length);
			t += offset_t;
			vertices[i].y = initVerts[i].y + yAmp * Mathf.Sin(6.28f*((t)/period));
			i++;
		}
		mesh.vertices = vertices;
		
	}
}
