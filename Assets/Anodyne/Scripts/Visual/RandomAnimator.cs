using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAnimator : MonoBehaviour {
    [Tooltip("If this flag is not 0, then the random animator doesn't play.")]
    public string deactivateIfNonzeroFlag = "";
    [Tooltip("If this gets deactivated, then the connected animator will play this")]
    public string animatorStateIfDeactivated = "";
    [Header("Drag the Animator to this field!")]
    public GameObject objectWithAnimatorComponent;

    public RandomAnimation[] anims;

    [System.Serializable]
    public class RandomAnimation {
        public string name = "idle";
        public int minimumLoops = 3;
        public int maximumLoops = 5;
        [Range(0,1)]
        public float probability = 0;
        public bool doesntRepeat = false;
        public float entryCrossfadeTime = 0.15f;
    }
    [System.NonSerialized]
    public bool isMaster = true;

    Animator animator;
    RandomAnimation currentAnim;
    int loopLimit = 0;
	void Start () {
        animator = GetComponent<Animator>();
        if (isMaster) {
            RandomAnimator other = objectWithAnimatorComponent.AddComponent<RandomAnimator>();
            other.anims = anims;
            other.isMaster = false;
            other.deactivateIfNonzeroFlag = deactivateIfNonzeroFlag;
            other.animatorStateIfDeactivated = animatorStateIfDeactivated;
            Destroy(this);
            return;
        }
        if (deactivateIfNonzeroFlag != "" && 0 != DataLoader.instance.getDS(deactivateIfNonzeroFlag)) {
            animator.Play(animatorStateIfDeactivated);
            enabled = false;
        }
	}

    int loops;
	void Update () {
        // Force animation to reset if it isn't embedded as a looping anim
        if (currentAnimCantEmbeddedLoop) {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName(currentAnim.name) && animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1) {
                animator.CrossFade(currentAnim.name, currentAnim.entryCrossfadeTime);
                loops++;
            }
        } else {
            loops = (int)(animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
        }
        if (loops >= loopLimit && (currentAnim == null || animator.GetCurrentAnimatorStateInfo(0).IsName(currentAnim.name))) {
            //print(loops);
            PlayNextAnim();
            loops = 0;
        }
	}
    bool currentAnimCantEmbeddedLoop = false;
    List<float> probs = new List<float>();
    List<string> names = new List<string>();


    void PlayNextAnim() {
        // Sort of shitty hack lol
        // First makes an array of probabilities, but skips current animation if set to not repeat.
        // Based on the array of probabilities (say .2 .3 .5)
        // Creates an array 'probs' of their sums (plus zero) - (0, .2, .5, 1)
        // Generates a value from 0 to that sum 'pValue'
        // Then finds which range that value falls in, and uses that to pick the next animation
        probs.Clear();
        names.Clear();
        float currentPSum = 0;
        probs.Add(0);
        foreach (RandomAnimation r in anims) {
            if (currentAnim != null && currentAnim.name == r.name && r.doesntRepeat) {
                continue;
            }
            currentPSum += r.probability;
            probs.Add(currentPSum);
            names.Add(r.name);
        }
        float pValue = currentPSum * Random.value;
        //print("pvalue " + pValue);
        for (int i = 1; i < probs.Count; i++) {
           // print(names[i-1] + " " + probs[i]);
            if (pValue >= probs[i-1] && pValue <= probs[i]) {
                foreach (RandomAnimation ran in anims) {
                    if (ran.name == names[i-1]) {
                        // 3 + floor([0,2.999])
                        animator.CrossFade(ran.name, ran.entryCrossfadeTime);
                        currentAnim = ran;
                        loops = 0;
                        loopLimit = ran.minimumLoops + Mathf.FloorToInt((ran.maximumLoops - ran.minimumLoops + 0.999f) * Random.value);
                        //print(ran.name + " playing with loop limit" + loopLimit);
                        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
                        currentAnimCantEmbeddedLoop = false;
                        foreach (AnimationClip clip in clips) {
                           if (clip.name == ran.name && !clip.isLooping) {
                               //print("This anim doesn't have embedded looping");
                                currentAnimCantEmbeddedLoop = true;
                            }
                        }
                    }
                }
                break;
            }
        }
    }
}
