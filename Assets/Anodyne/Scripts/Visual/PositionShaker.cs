using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionShaker : MonoBehaviour {

	float t;
	public float tShake = 0.05f;
    public Vector3 amplitude = new Vector3(0.08f, 0.08f, 0);
	Vector3 randomizedamp;

    public bool noZ = false;
    [System.NonSerialized]
    public bool onlyShakeWhenConfirmHeld = false;
    public bool IsUI = false;
    RectTransform rectTransform;

    private void Start() {
        if (IsUI) {
            rectTransform = GetComponent<RectTransform>();
        }
       if (name == "BigPlayer") {
            // Lol horrible hack
            GameObject engine = transform.Find("model").Find("Ridescale").Find("Base").Find("Body").Find("Thrusters").gameObject;
            PositionShaker ps = engine.AddComponent<PositionShaker>();
            ps.amplitude = amplitude;
            ps.tShake = tShake;
            ps.onlyShakeWhenConfirmHeld = true;
            Destroy(this);
        } else if (transform.parent != null && transform.parent.name == "BigPlayer") {
            onlyShakeWhenConfirmHeld = true;
        }
       
    }
    void Update () {


        if (IsUI) rectTransform.localPosition -= randomizedamp;
        if (!IsUI) transform.localPosition -= randomizedamp;
        randomizedamp.Set(0, 0, 0);

        // Only for the Ridescale Model right now.
        if (onlyShakeWhenConfirmHeld && !(MyInput.confirm || MyInput.cancel || MyInput.up || MyInput.down || Mathf.Abs(MyInput.moveY) > MyInput.joyMoveThreshold)) {
            return;
        }

		t += Time.deltaTime;
		if (tShake == 0 || t > tShake) {
			t -= tShake;
			randomizedamp = amplitude;
			randomizedamp.x = randomizedamp.x * Random.value;
			if (Random.value > 0.5f) randomizedamp.x *= -1;

			randomizedamp.y = randomizedamp.y * Random.value;
			if (Random.value > 0.5f) randomizedamp.y *= -1;

			randomizedamp.z = randomizedamp.z * Random.value;
			if (Random.value > 0.5f) randomizedamp.z *= -1;
            if (noZ) randomizedamp.z = 0;
			if (!IsUI) transform.localPosition = transform.localPosition + randomizedamp;
			if (IsUI) rectTransform.localPosition = rectTransform.localPosition+ randomizedamp;
        }
    }
}
