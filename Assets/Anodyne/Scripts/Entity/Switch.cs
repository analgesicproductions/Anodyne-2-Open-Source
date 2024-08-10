using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Anodyne {
    public class Switch : MonoBehaviour {

        public List<GameObject> children;

        public SwitchType switchType = SwitchType.OneShot;
        SpriteAnimator anim;
        public bool sendsUnsignal = false;
        string signalType = "";
        public enum SwitchType { OneShot, Button, Reset, BinaryToggle, ShadowOnly, ResetsWhenLeaveRoom }
        int togglestate = 0;
        void Start() {
            anim = GetComponent<SpriteAnimator>();
            if (sendsUnsignal) signalType = "unsignal";
            HF.GetPlayer(ref player);
        }
        AnoControl2D player;
        int state = 0;
        void Update() {
            if (state == 0) {

            } else if (state == 1) {
                // Switch set to on, idk just broadcast for now?
                if (switchType == SwitchType.OneShot || switchType == SwitchType.Button || switchType == SwitchType.ShadowOnly || switchType == SwitchType.ResetsWhenLeaveRoom) {
                   
                    HF.SendSignal(children,signalType);
                    anim.Play("off");
                    state = 2;
                }
                if (switchType == SwitchType.Reset) {
                    HF.SendSignal(children, "reset");
                    state = 0;
                }
                if (switchType == SwitchType.BinaryToggle) {
                    HF.SendSignal(children, "toggle");
                    togglestate = (togglestate + 1) % 2;
                    state = 0;
                    // anim thing
                }
            } else if (state == 2) {
                if (anim.isPlaying == false) {
                    state = 3;
                }
            } else if (state == 3) {
                if (switchType == SwitchType.ShadowOnly || switchType == SwitchType.ResetsWhenLeaveRoom) {
                    if (!player.CameraIsChangingRooms() && !player.InSameRoomAs(transform.position)) {
                        state = 0;
                        anim.Play("on");
                    }
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collision) {
            if (switchType == SwitchType.ShadowOnly) {
                if (state == 0 && (collision.name.IndexOf("ShadowTwin") != -1 || (collision.name.IndexOf("ShadowSpiky") != -1 && collision.GetComponent<SpikyChaser>().shadowFormOn))) {
                    state = 1;
                    AudioHelper.instance.playSFX("button_down", true, 0.8f);
                }
                return;
            }
            if (collision.CompareTag("Player") && state == 0) {
                state = 1;
                AudioHelper.instance.playSFX("button_down", true, 0.8f);
            } else if (state != 2 && collision.gameObject.GetComponent<Bullet>() != null) {
                state = 1;
                AudioHelper.instance.playSFX("vacuumSucked");
            } else if (collision.GetComponent<SpikyChaser>() != null) {
                state = 1;
                AudioHelper.instance.playSFX("button_down", true, 0.8f);
            }
        }

            private void OnCollisionEnter2D(Collision2D collision) {
            if (state != 2 && collision.gameObject.GetComponent<Vacuumable>() != null) {
                Vacuumable rock = collision.gameObject.GetComponent<Vacuumable>();
                if (rock.isMoving() || rock.JustBrokeResetExternally) {
                    rock.JustBrokeResetExternally = false;
                    rock.Stop();
                    state = 1;
                    AudioHelper.instance.playSFX("vacuumSucked");
                }

            }
        }

    }



}
