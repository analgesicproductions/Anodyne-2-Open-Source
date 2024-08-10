using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Anodyne {
    public class SpriteAnimator : MonoBehaviour {

        public string folder = "Prototyping";
        public Texture2D spritesheet;
        [HideInInspector]
        public Sprite[] sprites;
        [System.Serializable]
        public class Animation {
            public string name;
            public int fps;
            [TextArea(1,5)]
            public string framestring;
            public bool looping = true;
            [HideInInspector]
            public int[] frames;
        }


        public void AddAnimation(string _name, int _fps, string _framestring, bool _looping) {
            if (GetAnimation(_name) == null) {
                Animation anim = new Animation();
                anim.name = _name;
                anim.fps = _fps;
                anim.framestring = _framestring;
                anim.looping = _looping;
                Animation[] newanims = new Animation[animations.Length + 1];
                animations.CopyTo(newanims, 0);
                newanims[newanims.Length - 1] = anim;
                animations = newanims;
            }
        }

        public bool randomizeFPSUpTo2x = false;
        public bool isUI = false;
        public Animation[] animations;
        Animation currentAnimation;
        public float speedMultiplier = 1f;

        public string animToPlayOnStart;
        int currentFrame = 0;
        float timer = 0f;
        SpriteRenderer sr;
        Image img;
        Sprite firstSprite;

        public static Sprite[] breakSpritesHack;
        public static int breakSpritesHackFPS = 16;

        public string CurrentAnimationName() {
            if (currentAnimation != null) {
                return currentAnimation.name;
            }
            return "NO_ANIM";
        }

        void Awake() {
            if (breakSpritesHack == null) {
                breakSpritesHack = Resources.LoadAll<Sprite>("Visual/Sprites/Entity/EntitySprites16");
            }
            if (isUI) {
                img = GetComponent<Image>();
            } else {
                sr = GetComponent<SpriteRenderer>();
            }
            sprites = Resources.LoadAll<Sprite>("Visual/Sprites/"+folder+"/"+ spritesheet.name);
            firstSprite = GetCurrentSprite();
            if (randomizeFPSUpTo2x) {
                foreach (Animation anim in animations) {
                    anim.fps = (int) ((1 + Random.value) * (float) anim.fps);
                }
            }
            if (animToPlayOnStart != "" && animToPlayOnStart != null)  ForcePlay(animToPlayOnStart);
        }
        
        Sprite GetCurrentSprite() {
            if (isUI) {
                return img.sprite;
            } else {
                return sr.sprite;
            }
        }

        void SetSprite(Sprite s) {
            if (isUI) {
                img.sprite = s;
            } else {
                sr.sprite = s;
            }
        }

        // Call play after this
        public void UpdateSpritesheet(string folder, string spritesheetName) {
            sprites = Resources.LoadAll<Sprite>("Visual/Sprites/" + folder + "/" + spritesheetName);
        }


        public void ChangeAnimationData(string name, string framestring) {
            Animation anim = GetAnimation(name);
            if (anim != null) {
                anim.framestring = framestring;
                anim.frames = null;
                if (name == currentAnimation.name) ForcePlay(name);
            } else {
                print("Failed to find animation " + name);
            }
        }


        public void ChangeAnimationData(string name, int fps, string framestring, bool looping) {

            Animation anim = GetAnimation(name);
            if (anim != null) {
                anim.fps = fps;
                anim.framestring = framestring;
                anim.looping = looping;
                anim.frames = null;
                if (currentAnimation != null && name == currentAnimation.name) ForcePlay(name);
            } else {
                print("Failed to find animation " + name);
            }
        }


        public void PlayInitialAnim() {
            if (animToPlayOnStart != "" && animToPlayOnStart != null) {
                ForcePlay(animToPlayOnStart);
            } else {
                currentAnimation = null;
                SetSprite(firstSprite);
                isPlaying = false;
            }
        }

        [HideInInspector]
        public bool isPlaying = false;
        // Update is called once per frame
        [HideInInspector]
        public bool paused = false;
        void Update() {

            if (!isPlaying || paused) {
                return;
            }
            
            float delay = 1 / (float) currentAnimation.fps;
            if (timer > delay) {
                while (timer > delay) {
                    timer -= delay;
                    currentFrame++;
                    if (currentFrame == currentAnimation.frames.Length) {
                        if (!currentAnimation.looping) {
                            isPlaying = false;
                            if (playBreakHack) {
                                currentAnimation = null;
                                playBreakHack = false;
                                return;
                            }
                            currentFrame--;
                            if (followUpAnimName != "") {
                                Play(followUpAnimName);
                                followUpAnimName = "";
                            }
                        } else {
                            currentFrame = 0;
                        }
                    }
                }
                if (playBreakHack) {
                    SetSprite(breakSpritesHack[currentAnimation.frames[currentFrame]]);
                } else {
                    ChangeFrame(currentFrame);
                }

            } else {
                timer += Time.deltaTime * speedMultiplier;
            }

        }
        bool playBreakHack = false;
        public void Play(string name) {
            if (currentAnimation != null && name == currentAnimation.name) return;
            ForcePlay(name);
        }

        string followUpAnimName = "";
        public void ScheduleFollowUp(string name) {
            followUpAnimName = name;
        }

        // possible errors: index into sprites[] is too big
        void ChangeFrame(int animIndex) {
            SetSprite(sprites[currentAnimation.frames[animIndex]]);
        }

        public void ForcePlay(string name) {
            bool foundanim = false;
                 
            foreach (Animation animation in animations) {
                if (animation.name == name) {
                    currentAnimation = GetAnimation(name);
                    isPlaying = true;
                    currentFrame = 0;
                    ChangeFrame(currentFrame);
                    timer = 0;
                    foundanim = true;
                    break;
                }
            }
            if (!foundanim) {
                if (name == "break") {
                    
                    playBreakHack = true;
                    currentAnimation = new Animation();
                    currentAnimation.fps = breakSpritesHackFPS;
                    currentAnimation.frames = new int[] { 106,107,108,109 };
                    currentAnimation.looping = false;
                    SetSprite(breakSpritesHack[106]);
                    currentFrame = 0;
                    timer = 0;
                    isPlaying = true;

                } else {
                    print("No animation called " + name + " on " + gameObject.name);
                }
            }

        }

        public Animation GetAnimation(string name) {
            foreach (Animation animation in animations) {
                if (animation.name == name) {
                    if (animation.frames == null || animation.frames.Length == 0) {
                        if (animation.framestring.IndexOf(".") != -1) print("Error, anim string has a period");
                        string[] dataBits = animation.framestring.Split(',');
                        animation.frames = new int[dataBits.Length];
                        for (int i = 0; i < animation.frames.Length; i++) {
                            animation.frames[i] = int.Parse(dataBits[i], System.Globalization.CultureInfo.InvariantCulture);
                        }
                    }
                    return animation;
                }
            }
            return null;
        }
    }

}