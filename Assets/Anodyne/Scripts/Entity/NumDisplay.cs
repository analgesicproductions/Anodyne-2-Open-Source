using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumDisplay : MonoBehaviour {
    public int startingValue = 15;
    public int currentValue = 15;
    public bool isTens = false;
    public bool isOnes = true;

    Anodyne.SpriteAnimator animator;

    public List<GameObject> children;

	void Start () {
        animator = GetComponent<Anodyne.SpriteAnimator>();
	}
    bool didinit = false;
	
	void Update () {
        if (!didinit) {
            didinit = true;
            UpdateNumber();
        }

        if (MyInput.jpConfirm) {
         //   SendSignal("1");
        } else if (MyInput.jpCancel) {
        //    SendSignal("reset");
        }
	}

    void UpdateNumber() {
        if (isTens) {
            animator.Play(((currentValue - currentValue % 10) / 10).ToString());
        } else if (isOnes) {
            animator.Play((currentValue % 10).ToString());
        }
    }
    public void SendSignal(string signal) {
        if (signal == "reset") {
            currentValue = startingValue;
            UpdateNumber();
        } else {
            if (currentValue == 0) return;
            currentValue -= int.Parse(signal, System.Globalization.CultureInfo.InvariantCulture);
            if (currentValue < 0) currentValue = 0;
            UpdateNumber();
        }
        if (currentValue == 0) {
            HF.SendSignal(children, "");
        }
    }
}
