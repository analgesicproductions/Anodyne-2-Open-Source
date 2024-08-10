using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureOffsetter : MonoBehaviour {


	public float xRatePerSecond = 0f;
	public float yRatePerSecond = 0f;


	Material mat;
	MeshRenderer mr;
    SkinnedMeshRenderer smr;
    public static bool globalfreeze;
	void Start () {
        globalfreeze = false;
		mr = GetComponent<MeshRenderer>();
        if (mr == null) {
            smr = GetComponent<SkinnedMeshRenderer>();
            mat = smr.material;
        } else {
            mat = mr.material;
        }
        //MaterialPropertyBlock pb;
        mat.EnableKeyword("_DETAIL_MULX2");
		off = new Vector2();
	}

	Vector2 off;
	void Update () {
        if (globalfreeze) {
            enabled = false;
            return;
        }
		off.Set(off.x+Time.deltaTime*xRatePerSecond,off.y+Time.deltaTime*yRatePerSecond);
		//mat.renderQueue = 2001;
		mat.mainTextureOffset = off;
      /*  if (xRate2 != 0 || yRate2 !=0 ) {
            off2.Set(off2.x + Time.deltaTime * xRate2, off2.y + Time.deltaTime * yRate2);
            print(off2);
            mat.SetTextureOffset(S)
            
        }*/
        // Doesn't work bc the standard shader doesnt let you change secondary stuff in scripts
    }
}
