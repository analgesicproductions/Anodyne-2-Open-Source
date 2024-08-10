using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeRenderQueue : MonoBehaviour {

    // Fixes transparency order problems
    public int renderQueue = 3001;
    public int materialIndex = 0;
	void Start () {
        GetComponent<Renderer>().materials[materialIndex].renderQueue = renderQueue;
	}
	
}
