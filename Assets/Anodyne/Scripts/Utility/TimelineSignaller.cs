using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
public class TimelineSignaller : MonoBehaviour {

    public bool TalksToDialogueAno2 = true;
    public bool PausesTimelineWhenEnabled = true;
    public DialogueAno2 npc;
    public PlayableDirector director;

    private void OnEnable() {
        if (TalksToDialogueAno2) {
            if (PausesTimelineWhenEnabled) print("Pausing Timeline + Enabling DialogueAno2");
            npc.Unpause_YieldToTimeline();
        }
        if (PausesTimelineWhenEnabled) {
            director.playableGraph.GetRootPlayable(0).SetSpeed(0);
        }
    }
}
