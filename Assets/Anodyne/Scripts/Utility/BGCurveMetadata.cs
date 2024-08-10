using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGCurveMetadata : MonoBehaviour {

	public float minWindow = 2f;
	public float maxWindow = 3f;
	public bool lookAtPlayer = true;
	public Vector3 initialLookAngle;
	public Vector3 lookTargetOffset;
	[Tooltip("If true, camera will immediately move to needed position - good for closeups.")]
	public bool dontLerp = false;

}
