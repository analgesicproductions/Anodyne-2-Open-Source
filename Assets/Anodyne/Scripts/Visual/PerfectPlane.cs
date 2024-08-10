using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerfectPlane : MonoBehaviour {

	// Use this for initialization

	Mesh mesh;
	Vector3[] initVerts;
	int[] indices;

	public int width = 10;
	public int height = 10;
	public int spacing = 1;

	void Start () {
		mesh = new Mesh();

		initVerts = new Vector3[(width+1)*(height+1)];

		Vector3[] normals = new Vector3[initVerts.Length];

		// Left to right, then top to bottom.
		for (int i = 0, k = 0; i <= height; i++) {
			for (int j = 0; j <= width; j++, k++) {
				initVerts[k] = new Vector3(j*spacing,-i*spacing);
				normals[k] = new Vector3(0,0,-1);
			}
		}

		// w = 2, h = 2.
		// 6 hor lines, 6 vert lines, index buffer size of 24.
		// 0 1 2
		// 3 4 5
		// 6 7 8


		// w*h hor lines, w*h vert lines to make a grid of lines.
		// 2 * (2*(2+1) + 2*(2+1)) = 24
		indices = new int[2*(width*(height+1) + height*(width+1))];


		// e.g. 5 verts wide, or 4 lines wide
		// Create horizontal lines
		for (int y = 0, i=0; y <= height; y++) {
			for (int x =0; x < width; x++, i++) {
				indices.SetValue(y*(width+1)+x,i*2);
				indices.SetValue(y*(width+1)+x+1,i*2+1);
				//print(i*2);
			}
		}

		// 2 * (2+1) * 2 = 12
		int o = width*(height+1)*2;
		for (int x =0,i=0; x <= width; x++) {
			for (int y = 0; y < height; y++,i++) {
				indices.SetValue(y*(width+1)+x,i*2+o);
				indices.SetValue((y+1)*(width+1)+x,i*2+1+o);
			}
		}
	

		mesh.vertices = initVerts;
		mesh.normals = normals;
		mesh.SetIndices(indices,MeshTopology.Lines,0);
		mesh.RecalculateBounds();

		GetComponent<MeshFilter>().mesh = mesh;


	

	}

	void shrinkX() {
		
	}

	void OnDrawGizmos() {
		if (initVerts == null) return;
		/*Gizmos.color = Color.red;
		for (int i = 0; i < verts.Length; i++) {
			print(verts[i]);
			Gizmos.DrawSphere(verts[i],1f);
		}*/
	}

	// Update is called once per frame

	public float period = 4f;
	float yAmp = 0.2f;
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
			offset_t = period * (i % (width+1))/(width+1f);
			t += offset_t;
			vertices[i].y = initVerts[i].y + yAmp * Mathf.Sin(6.28f*((t)/period));
			vertices[i].z = initVerts[i].z + yAmp * Mathf.Sin(6.28f*((t+0.5f)/period));
			i++;
		}
		mesh.vertices = vertices;
		
	}
}
