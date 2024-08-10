using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

public class MelosEditorTools : EditorWindow {


	// https://docs.unity3d.com/Manual/editor-EditorWindows.html

	[MenuItem ("Window/My Tools")]
    
	public static void ShowWindow() {   
		EditorWindow.GetWindow(typeof(MelosEditorTools));
	}


	public static string setTo1Flag_1 = "";
	public static string setTo1Flag_2 = "";
	public static string setTo1Flag_3 = "";
    bool initializationallowed = false;
    bool initialization2dallowed = false;
    string flagname = "";
    int flagval = 0;
	void OnGUI() {

        if (GUILayout.Button("Set Flag",GUILayout.MaxWidth(80))) {
            if (DataLoader.instance != null) {
                DataLoader.instance.setDS(flagname, flagval);
            }
        }

        flagname = EditorGUILayout.TextField(flagname, GUILayout.MaxWidth(80));
        flagval = EditorGUILayout.IntField(flagval, GUILayout.MaxWidth(20));

        if (GUILayout.Button("Make English Text", GUILayout.MaxWidth(160))) {
			MakeDialogueXML();
        }
        
        
        if (GUILayout.Button("Make Russian Text", GUILayout.MaxWidth(160))) {
            MakeDialogueXML("ru/","","_ru");
        }
        
        if (GUILayout.Button("Make zh-trad Text", GUILayout.MaxWidth(160))) {
            MakeDialogueXML("zh-trad/","","_zh-trad");
        }
        
        /*
        
        if (GUILayout.Button("Make zh-simp Text", GUILayout.MaxWidth(160))) {
            MakeDialogueXML("zh-simp/","_zho-CN","_zh-simp");
        }

        if (GUILayout.Button("Make pt-br Text", GUILayout.MaxWidth(160))) {
            MakeDialogueXML("pt-br/", "", "_pt-br");
        }

        if (GUILayout.Button("Make German Text", GUILayout.MaxWidth(160))) {
            MakeDialogueXML("de/", "", "_de",false);
        }
*/
        if (GUILayout.Button("Make Spanish Text", GUILayout.MaxWidth(160))) {
            MakeDialogueXML("es/", "", "_es");
        }
/*
        if (GUILayout.Button("Make French Text", GUILayout.MaxWidth(160))) {
            MakeDialogueXML("fr/", "", "_fr");
        }
        */
        if (GUILayout.Button("Make Japanese Text", GUILayout.MaxWidth(160))) {
            MakeDialogueXML("jp/", "", "_jp");
        }

        initializationallowed = EditorGUILayout.Toggle("Tick to allow 3D init", initializationallowed);

        if (GUILayout.Button("Initialize empty 3D Scene", GUILayout.MaxWidth(160))) {
            if (!initializationallowed) {
                Debug.LogWarning("Scene not init'd, tick the box");
            } else {
                InitScene3D();
                initializationallowed = false;
            }
        }

        GUILayout.Space(32);

        initialization2dallowed = EditorGUILayout.Toggle("Tick to allow 2D init", initialization2dallowed);

        if (GUILayout.Button("Initialize empty 2D Scene", GUILayout.MaxWidth(160)) && initialization2dallowed) {
            InitScene2D();
            initialization2dallowed = false;
        }
        //		GUILayout.Label("Auto-set flag #1");
        //		setTo1Flag_1 = GUILayout.TextField(setTo1Flag_1);
        //		GUILayout.Label("Autdo-set flag #2");
        //		setTo1Flag_2= GUILayout.TextField(setTo1Flag_2);
        //		GUILayout.Label("Auto-set flag #3");
        //		setTo1Flag_3 = GUILayout.TextField(setTo1Flag_3);
    }

    void InitScene3D() {
        GameObject.DestroyImmediate(GameObject.Find("Main Camera"));
        GameObject.DestroyImmediate(GameObject.Find("Directional Light"));


        string[] Prefabs = new string[] { "3D UI", "UI Camera", "Loader", "MedBigCam", "BigPlayer", "MediumPlayer" };
        foreach (string s in Prefabs) {
            if (GameObject.Find(s) == null) {
                PrefabUtility.InstantiatePrefab(Resources.Load("Prefabs/Sean/"+s));
            }
        }

        GameObject.Find("UI Camera").transform.position = new Vector3(300, 300, 300);

        GameObject g = null;
        if (GameObject.Find("SceneData") == null) {
            g = new GameObject("SceneData", new System.Type[] { typeof(SceneData), typeof(AudioHelper), typeof(MelosFogEffect) });
            g.GetComponent<MelosFogEffect>().moveThisToCameraAndDestroy = true;
            g = null;
        }

        string[] GameObjects = new string[] { "NPCs", "Colliders", "Decoration", "Lights", "Audio", "Doors" };
        foreach (string s in GameObjects) {
            if (GameObject.Find(s) == null) {
                g = new GameObject(s);
                g = null;
            }
        }

        if (g != null) g = null;

    }
    void InitScene2D() {
        GameObject.DestroyImmediate(GameObject.Find("Main Camera"));
        GameObject.DestroyImmediate(GameObject.Find("Directional Light"));


        string[] Prefabs = new string[] { "2D UI", "2D 160px Camera", "Loader", "2D Ano Player"};
        foreach (string s in Prefabs) {
            if (GameObject.Find(s) == null) {
                PrefabUtility.InstantiatePrefab(Resources.Load("Prefabs/Sean/" + s));
            }
        }

        if (GameObject.Find("2D SceneData") == null) {
            GameObject scenedata = new GameObject("2D SceneData", new System.Type[] {typeof(AudioHelper), typeof(SceneData2D) });
            if (scenedata != null) scenedata = null;
        }

        string[] GameObjects = new string[] { "NPCs", "Colliders", "Decoration", "Tilemaps", "Audio", "Dust", "Doors" };
        foreach (string s in GameObjects) {
            if (GameObject.Find(s) == null) {
                GameObject g = new GameObject(s);
                if (g != null) g = null;
            }
        }

    }

    void MakeDialogueXML(string subfolder="",string inputSuffix="",string outputSuffix="",bool replaceDoubleQuotes=true) {

        // Update this with each part of the dialogue.
        string[] filenames = new string[] { "Misc" ,"Intro", "Ring", "Dustbound","Desert","Ending"};

        bool chinese = false;
        if (outputSuffix == "_zh-simp" || outputSuffix == "_zh-trad") chinese = true;

		// Will be written out as Dialogue Data.xml
		string out_str = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\n<Root>\n<DialogHolder>";

		// Maps raw dialogue names to actual dialogue names
		Dictionary<string,string> nameTable = new Dictionary<string,string>();
        List<string> keyTerms = new List<string>();

		bool notDialog = false;
        string[] _2dbgCols = new string[] { "1", "2", "drem", "elegy" };
		foreach (string filename in filenames) {
			// Note: I need to save these in notepad as utf8 - monodevelop won't do it
			string raw = File.ReadAllText("Assets/Resources/Dialogue/"+subfolder+filename+inputSuffix+".txt",System.Text.Encoding.UTF8);

            // Replace annoying quotation/ellipsis
            raw = raw.Replace("…", "...");
            raw = raw.Replace("’", "'");
            raw = raw.Replace("‘", "'");
            raw = raw.Replace("​", " ");
            if (subfolder == "jp/") {
            }
            if (replaceDoubleQuotes) {
                raw = raw.Replace("”", "\"");
                raw = raw.Replace("“", "\"");
            }

            int mode = 0;
            bool isNameTable = false;
			string[] rawLines = raw.Split(new string[]{"\r\n","\n"},System.StringSplitOptions.None);

            string word1 = "";
            string word0 = "";
            string wordbuf = "";
            int spacesFound = 0;
            foreach (string line in rawLines) {
                spacesFound = 0;
                word1 = word0 = "";
                wordbuf = "";
                for (int i = 0; i < line.Length; i++) {
                    if (line[i] == ' ' || line[i] == '\r' || line[i] == '\n') {
                        spacesFound++;
                        if (spacesFound == 1) {
                            word0 = wordbuf;
                            wordbuf = "";
                        } else if (spacesFound == 2) {
                            word1 = wordbuf;
                            break;
                        }
                    } else {
                        wordbuf += line[i];
                    }
                }
                if (wordbuf != "" && spacesFound == 0) word0 = wordbuf;
                if (wordbuf != "" && spacesFound > 0) word1 = wordbuf;

				// Decide between parsing the name table or a normal scene.
				if (mode == 0) {
					if (word0 == "scene") {
						notDialog = false;
                        isNameTable = false;
                        if (word1 == "nameTable") {
                            isNameTable = true;
                            mode = 1;
                            out_str += "<d name=\"" + word1 + "\">\n<lines>\n";
                        } else if (word1 == "key-terms") {
                            mode = 3;
						} else {
							if (word1 == "pauseMenuChoices" || word1 =="keybind") notDialog = true; 
							mode = 1;
							out_str += "<d name=\""+word1+"\">\n<lines>\n";
						}
					}
				// Normal scene. Just add the xml metadata, as well as some other metadata if specified.
				} else if (mode == 1) {
					string nextLine = "";
					if (line.Length < 1 || line == " " || line.IndexOf("/") == 0) {
						// Skip short lines/comments
					} else {
						// Scene is done
						if (word0 == "endscene") {
							mode = 0;
							out_str += "</lines>\n</d>\n";
						} else {
							nextLine = "\t<l";
							// Store copy of raw line in tempRawLine so stuff like "_a" can be removed.
							string tempRawLine = line;


                            if (isNameTable) {
                                string[] parts = line.Split(new string[] { "=" }, System.StringSplitOptions.None);
                                nameTable.Add(parts[0], parts[1]);
                            }

							if (line.Length > 4 && line.Substring(0,3) == "__c") {
                                foreach (string colType2D in _2dbgCols) {
                                    if (line.IndexOf("__c"+colType2D+" ") != -1) {
                                        nextLine += " c=\""+colType2D+"\"";
                                        tempRawLine = tempRawLine.Replace("__c"+colType2D+" ", "");
                                    }
                                }
                            }
                            if (line.IndexOf("!fade") != -1) {
                                nextLine += " fadescreen=\"true\"";
                                tempRawLine = tempRawLine.Replace("!fade ", "");
                            }

                            if (!isNameTable) {
                                foreach (string k in keyTerms) {
                                    if (k.IndexOf("/") != -1) {
                                        // A bad hack
                                        // If there are terms like AB/AC/A, then
                                        // IF AB or AC gets replaced, then only #A gets replaced
                                        // If not, then A gets replaced on its own.
                                        // Hashtags do't matter for AB/AC
                                        // This is just used for Dust Drop Point/Nano Dust/Dust issue.
                                        //E.g. The Dustbound have Dust goes to 
                                        string[] a = k.Split('/');
                                        bool useHashtagReplace = false;
                                        foreach (string term in a) {
                                            int termIndex = -1;
                                            string _term = term;
                                            if (useHashtagReplace && _term == a[a.Length-1]) _term = "#" + _term;
                                            termIndex = tempRawLine.IndexOf(_term);
                                            if (termIndex != -1) {
                                                if (termIndex > 0 && tempRawLine[termIndex - 1] != '$') {
                                                    tempRawLine = tempRawLine.Replace(_term, "<![CDATA[<color=#90f3c0>]]>" + term + "<![CDATA[</color>]]>");
                                                } else if (termIndex > 0) {
                                                    tempRawLine = tempRawLine.Replace("$"+_term, term );
                                                } else {
                                                    tempRawLine = tempRawLine.Replace(_term, "<![CDATA[<color=#90f3c0>]]>" + term + "<![CDATA[</color>]]>");
                                                }
                                                if (term != a[a.Length-1]) useHashtagReplace = true;
                                            }
                                        }
                                    } else {
                                        // Use a "$" to ignore a term
                                        int __idx = tempRawLine.IndexOf(k);
                                        if (__idx != -1) {
                                            if (__idx > 0 && tempRawLine[__idx - 1] != '$') {
                                                tempRawLine = tempRawLine.Replace(k, "<![CDATA[<color=#90f3c0>]]>" + k + "<![CDATA[</color>]]>");
                                            } else if (__idx > 0) {
                                                tempRawLine = tempRawLine.Replace("$"+k, k);
                                            } else {
                                                tempRawLine = tempRawLine.Replace(k, "<![CDATA[<color=#90f3c0>]]>" + k + "<![CDATA[</color>]]>");
                                            }
                                        }
                                    }
                                }

                                // Replace NAME_ID: with tags for the name card.
                                foreach (string k in nameTable.Keys) {
                                    if (chinese) {
                                        if (tempRawLine.IndexOf(k + ":") == 0 && !notDialog && tempRawLine.IndexOf(k+":n") == -1) {
                                            nextLine += string.Format(" pic=\"{0}\"", k);
                                            tempRawLine = tempRawLine.Replace(k + ":", "");
                                        }
                                    } else {
                                        if (tempRawLine.IndexOf(k + ": ") == 0 && !notDialog) {
                                            nextLine += string.Format(" pic=\"{0}\"", k);
                                            tempRawLine = tempRawLine.Replace(k + ": ", "");
                                        }
                                    }
                                }
                                // Adding :n after a name makes it appear in the generic box
                                int embednameindex = tempRawLine.IndexOf(":n");
                                if (embednameindex != -1) {
                                    nextLine += string.Format(" pic=\"{0}\"", tempRawLine.Substring(0, embednameindex));
                                    tempRawLine = tempRawLine.Remove(0, embednameindex + 3);
                                }

                                // Replace $NAME_ID$ with name (for 2D areas where I need the name in the dialogue itself)
                                foreach (string k in nameTable.Keys) {
                                    //if (tempRawLine.IndexOf("$" + k + "$") != -1) {
                                        tempRawLine = tempRawLine.Replace("$" + k + "$", nameTable[k]);
                                   // }
                                }
                            }

							nextLine += ">";
							nextLine += tempRawLine;



                            nextLine += "</l>\n";
							out_str += nextLine;
						}
					}

				} else if (mode == 3) {
                    if (word0 == "endscene") {
                        mode = 0;
                    } else {
                        keyTerms.Add(line);
                    }
                }
            }

		}





		out_str += "\n</DialogHolder>\n</Root>";

		string path = "Assets/Resources/Dialogue/Dialogue Data"+outputSuffix+".xml";
		StreamWriter writer = new StreamWriter(path,false,System.Text.Encoding.UTF8);
		writer.Write(out_str);
		writer.Close();
		// Unity will update the new text faster this way
		AssetDatabase.ImportAsset(path);

	}
}
