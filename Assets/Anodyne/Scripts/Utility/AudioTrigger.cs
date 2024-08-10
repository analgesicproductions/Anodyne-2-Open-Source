using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTrigger : MonoBehaviour {

	public string stateMustBe0 = ""; 
	// Use this for initialization
	public string setStateTo1 = "";

	public string[] commands;

	// example command: "none$arg1,arg2,etc";
	void Start () {
		
	}
	
	void OnTriggerEnter(Collider other) {

	if (other.CompareTag("Player")) {
		if (stateMustBe0 != "" && DataLoader.instance.getDS(stateMustBe0) == 1) return;
			if (setStateTo1 != "")DataLoader.instance.setDS(setStateTo1,1);

			if (commands != null) {
				foreach (string s in commands) {
					string command = s.Split("$".ToCharArray())[0];
					string[] args = s.Split("$".ToCharArray())[1].Split(",".ToCharArray());
					AudioHelper ah = GameObject.Find("AudioHelper").GetComponent<AudioHelper>();
					if (command == "playonce") {
						// loop start, end
						ah.PlaySong(args[0],CutsceneManager.safeFloatParse(args[1]),CutsceneManager.safeFloatParse(args[2]),true);
					} else if (command == "play") {
						ah.PlaySong(args[0],CutsceneManager.safeFloatParse(args[1]),CutsceneManager.safeFloatParse(args[2]));
					} else if (command == "fadestop") {
						ah.StopSongByName(args[0]);
					} else if (command == "fade") {
						// time, vol
						ah.FadeSong(args[0],CutsceneManager.safeFloatParse(args[1]),CutsceneManager.safeFloatParse(args[2]));
					}
				}
			}
		}
	}
}
