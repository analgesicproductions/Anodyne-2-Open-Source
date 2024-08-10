using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawWireframes : MonoBehaviour {
	public bool childrenArePlanes = true;
	public bool childrenAreCubes = false;
	public bool justSelf = false;
	public Color selfColor;
	void Start () {
		Mesh _mesh;
		MeshFilter[] mfs = GetComponentsInChildren<MeshFilter>();

		if (justSelf) {
			_mesh = GetComponent<MeshFilter>().mesh;
			if (childrenAreCubes) {
				_mesh.SetIndices(new int[] {0,1,1,3,3,2,2,0,4,5,5,7,7,6,6,4,0,6,1,7,3,5,2,4} ,MeshTopology.Lines,0);
			}
		} else { 
			foreach (MeshFilter mf in mfs) {
				_mesh = mf.mesh;

				if (childrenArePlanes) {
					_mesh.SetIndices(new int[] {0,1,2,3,0},MeshTopology.LineStrip,0);
				} else if (childrenAreCubes) {
					_mesh.SetIndices(new int[] {0,1,1,3,3,2,2,0,4,5,5,7,7,6,6,4,0,6,1,7,3,5,2,4} ,MeshTopology.Lines,0);
				}
			}
		}
	}

	void Update() {
		if (justSelf) {
			MeshRenderer mr = GetComponent<MeshRenderer>();
			mr.material.color = selfColor;
		}
	}


}
