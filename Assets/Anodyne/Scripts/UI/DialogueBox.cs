using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class DialogueBox : MonoBehaviour {
    TMP_Text text_dialog;
    TMP_Text text_dialog_under;
    bool textHasUnderlayer = false;
    public bool interactiveDialogPlaying = false;
    List<string> dialogueQueue;
    List<List<string>> dialogueMetadataQueue;
    List<string> lineStrings;
    List<string> visibleLinesCache;
    bool autoOnForCurrentLine = false;
    Image DialogueBoxImage;
    Image UnderDialogueFadeImage;
    Image FadePartImage;
    Color tempColor = new Color();
    bool allowLineSkipWithCancel = false;
    bool skipCurrentLineNormal = false;
    bool skipCurrentLineFade = false;

    public Sprite generic_nameplate;
    public Sprite pal_nameplate;
    public Sprite cp_nameplate;
    public Sprite cv_nameplate;
    Color nametext_color = new Color();
    Image nameplate;
    TMP_Text nametext;
    TMP_Text text_skip;

    //string currentRightPortrait = "";
    //Image rightPortrait;
    //Sprite yuitoSprite;

    int lineStringsIndex = 0;
    int state_text_dialog = 0;
    int curVislineCharIdx = 0;
    int maxVisualLines = 4;
    int linesLeftToShowBeforeInput = 4; // Lines left to show before AdvDialogIcon appears and requires Player input
    float tChar = 0f;
    float tmChar = 0.03f;
    bool is2d = false;

    AudioClip clipDialogBlip;
    AudioClip clipDialogBloop;
    float blipVol = 0.6f;
    float bloopVol = 0.6f;
    float blipVolQuiet = 0.27f;
    float bloopVolQuiet = 0.27f;
    float curBlipVol = 0;
    float curBloopVol = 0;

    float tBlip = 0f;
    float tmBlip = 0.065f;
    AudioSource _audioSource;
    AudioSource _asBloop;

    Image text_skip_image;
    Transform AdvDialogIcon;
    // Use this for initialization
    void Start () {
        curBlipVol = blipVol;
        curBloopVol = bloopVol;

        if (GameObject.Find("2D UI") != null) is2d = true;
        nameplate = GameObject.Find("nameplate").GetComponent<Image>(); nameplate.enabled = false;
        nametext = GameObject.Find("nametext").GetComponent<TMP_Text>(); nametext.alpha = 0;
        if (GameObject.Find("text_skip") != null) {
            text_skip = GameObject.Find("text_skip").GetComponent<TMP_Text>(); text_skip.text = "";
        }
        if (is2d && GameObject.Find("text_skip_box") != null) {
            text_skip_image = GameObject.Find("text_skip_box").GetComponent<Image>();
            text_skip_image.enabled = false;
        }

        nametext.text = "";
        _audioSource = GameObject.Find("UIAudioSource").GetComponent<AudioSource>();
        _asBloop = gameObject.AddComponent<AudioSource>();
        DialogueBoxImage = GameObject.Find("Dialog Box").GetComponent<Image>();
        DialogueBoxImage.enabled = false;
        if (GameObject.Find("UI_FadeImage") != null) {
            UnderDialogueFadeImage = GameObject.Find("UI_FadeImage").GetComponent<Image>();
        } else {
            UnderDialogueFadeImage = GameObject.Find("Under Dialogue Fade Layer").GetComponent<Image>();
        }
        if (GameObject.Find("fadepart") != null) {
            FadePartImage = GameObject.Find("fadepart").GetComponent<Image>();
        }

        GameObject g = GameObject.Find("text_dialog");
        text_dialog = g.GetComponent<TMP_Text>();
        text_dialog.text = "";
        if (GameObject.Find("fadetext") != null) {
            GameObject.Find("fadetext").GetComponent<TMP_Text>().text = "";
        }
        if (GameObject.Find("text_dialog_under") != null) {
            text_dialog_under = GameObject.Find("text_dialog_under").GetComponent<TMP_Text>();
            textHasUnderlayer = true;
            SetUnderText();
        }


        dialogueQueue = new List<string>();
        dialogueMetadataQueue = new List<List<string>>();
        visibleLinesCache = new List<string>();


        AdvDialogIcon = GameObject.Find("Advance Dialog Icon").GetComponent<Transform>();
        AdvDialogIcon.gameObject.SetActive(false);


        clipDialogBlip = Resources.Load("Audio/Sound/dialogBlip") as AudioClip;
        clipDialogBloop = Resources.Load("Audio/Sound/dialogBlip2") as AudioClip;

        if (SceneManager.GetActiveScene().name.IndexOf("Pico") != -1 ) {
            inPico = true;
            text_dialog_under.alpha = 0;
            DialogueBoxImage.sprite = picoboxsprite;
        }
    }

    public Sprite picoboxsprite;
    bool inPico = false;

    public void playLoopedDialogue(string sceneName) {
        int numberOfLines = DataLoader.instance.getDialogLines(sceneName).Length;
        int curLine = DataLoader.instance.getDS(sceneName + "-LOOPINDEX");
        playDialogue(sceneName, curLine);
        curLine++;
        if (curLine == numberOfLines) {
            curLine = 0;
            DataLoader.instance.setDS(sceneName + "-LOOPCOUNT", DataLoader.instance.getDS(sceneName + "-LOOPCOUNT") + 1);
        }
        DataLoader.instance.setDS(sceneName + "-LOOPINDEX",curLine);
    }

    string currentDialogueScene = "";
    int currentDialogueIndexFromScene = 0;
    string readStateSceneToUpdate = "";
    int readStateStartIdx = 0;
    int readStateEndIdx = 0;
    List<int> dialogueIndexFromSceneQueue = new List<int>();

    float initBoxY = -1000;
    [System.NonSerialized]
    public int constructedColor = 0;
    public void playDialogue(string sceneName, int StartWithThisLine = -1, int EndWithThisLine = -1, string forcedOutput = "") {
        print("Playing dialogue: " + sceneName);
        dialogueIndexFromSceneQueue.Clear();
        InFadeTextMode = false;
        text_dialog = GameObject.Find("text_dialog").GetComponent<TMP_Text>();
        string[] linesToPush = new string[1];
        if (forcedOutput != "") {
            linesToPush = DataLoader.instance.getConstructedDialogueLines(forcedOutput,constructedColor);
            constructedColor = 0;
        } else {
            linesToPush = DataLoader.instance.getDialogLines(sceneName);
        }

        int _i = 0;
        readStateSceneToUpdate = sceneName;
        currentDialogueScene = sceneName;
        if (StartWithThisLine == -1 && EndWithThisLine == -1) {
            readStateStartIdx = 0;
            readStateEndIdx = linesToPush.Length - 1;
        } else if (EndWithThisLine == -1) {
            readStateStartIdx = readStateEndIdx = StartWithThisLine;
        } else {
            readStateStartIdx = StartWithThisLine;
            readStateEndIdx = EndWithThisLine;
        }
        foreach (string line in linesToPush) {
            if (StartWithThisLine > -1 && _i < StartWithThisLine) {
                _i++;
                continue;
            }
            dialogueIndexFromSceneQueue.Add(_i);
            dialogueQueue.Add(line);
            dialogueMetadataQueue.Add(DataLoader.instance.lastDialogTags[_i]);
            if (StartWithThisLine == _i && EndWithThisLine == -1) break;
            _i++;
            if (EndWithThisLine > -1 && _i > EndWithThisLine) break;
        }

        if (oneChunkBoxY != -1 && initBoxY == -1000) {
            tempPos = DialogueBoxImage.transform.localPosition;
            initBoxY = tempPos.y;
            tempPos.y = oneChunkBoxY;
            DialogueBoxImage.transform.localPosition = tempPos;
        }
    }

    public bool isDialogFinished() {
        if (state_text_dialog == 0 && dialogueQueue.Count == 0) {
            return true;
        }
        return false;
    }
    // Update is called once per frame
    List<int> colorTagStartIndices;
    List<int> colorTagEndIndices;
    List<string> colorTagColorInfo;
    int currentDialogCharIndex;
    private string mostRecentColorTagColor;
    bool colorNotClosedInCurrentBuffer = false;
    bool InFadeTextMode = false;
    bool FadeFull = false;
    bool FadeTextWithNoFade = false;
    string currentGrowingLineBuffer = "";
    [System.NonSerialized]
    public string nameOfCameraToCutTo = "";
    [System.NonSerialized]
    public string nameOfCameraToSmoothTo = "";
    [System.NonSerialized]
    public float cameraLerpTime = 1;


    void DisableImagesOfRegularDialogBox  () {
        DialogueBoxImage.enabled = false;
        nameplate.enabled = false;
    }

    void EnableImagesOfRegularDialogBox() {
        DialogueBoxImage.enabled = true;
        nameplate.enabled = true;
    }

    void ClearDialogBoxAndNameplateTextAndClearVisibleLinesCache(bool dontclearname=false) {
        if (!dontclearname) nametext.text = "";
        visibleLinesCache.Clear();
        text_dialog.text = "";
        SetUnderText();
    }

    // expects [COMMAND arg arg arg]
    float floatParse(string arg, int index) {
        string[] args = arg.Split(' ');
        if (index == args.Length -1) {
            return float.Parse(args[index].Split(']')[0], System.Globalization.CultureInfo.InvariantCulture);
        } else {
            return float.Parse(args[index], System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    List<int> spriteTagIndices = new List<int>();
    List<string> spriteTags = new List<string>();
    float bumpPauseTime;
    [System.NonSerialized]
    public bool oneChunkNoBox = false;
    [System.NonSerialized]
    public bool oneChunkNoUnderlayer = false;
    [System.NonSerialized]
    public float oneChunkBoxY = -1;
    [System.NonSerialized]
    public bool isHorror = false;
    void Update () {
        // Eventually add 'ambient' option
        if (!isDialogFinished()) {
            interactiveDialogPlaying = true;
        } else {
            interactiveDialogPlaying = false;
        }
        bool useFastText = MyInput.shortcut || SaveManager.dialogueSkip;
        switch (state_text_dialog) {

            case 0:

                // Get the next <Line> from dialogQueue
                if (dialogueQueue.Count > 0) {


                    //  if (DialogCutTrig.doAutoTriggerDelay > 0) {
                    //    DialogCutTrig.doAutoTriggerDelay -= Time.deltaTime;
                    //   return;
                    // }

                    string nextDataLine = dialogueQueue[0];
                    if (is2d) {
                        //nextDataLine = nextDataLine.Replace("„", "\"");
                    }
                    List<string> metadata = dialogueMetadataQueue[0];
                    currentDialogueIndexFromScene = dialogueIndexFromSceneQueue[0];

                    dialogueQueue.RemoveAt(0);
                    dialogueMetadataQueue.RemoveAt(0);
                    dialogueIndexFromSceneQueue.RemoveAt(0);

                    if (nextDataLine == "[FADE]" || nextDataLine == "[FADEFULL]" || nextDataLine == "[FADENOFADE]" || nextDataLine == "[FADEPART]") {
                        nametext.text = "";
                        DisableImagesOfRegularDialogBox();
                        InFadeTextMode = true;
                        if (nextDataLine == "[FADEFULL]") {
                            FadeFull = true;
                        } else if (nextDataLine == "[FADEPART]") {
                            FadePart = true;
                        } else if (nextDataLine == "[FADENOFADE]") {
                            FadeTextWithNoFade = true;
                        }
                        text_dialog = GameObject.Find("fadetext").GetComponent<TMP_Text>();
                        if (FadePart) {
                            CachedFadetextAnchoredPos = text_dialog.rectTransform.anchoredPosition;
                            Vector3 tempaPos = text_dialog.rectTransform.anchoredPosition;
                            tempaPos.x = -64;
                            tempaPos.y = -78;
                            text_dialog.rectTransform.anchoredPosition = tempaPos;
                            text_dialog.fontSize = GetFadetextPartFontSize();
                            AdvDialogIcon.GetComponent<Oscillate>().UseTempPos(187f, -47f);
                        }
                        state_text_dialog = 100;
                        return;
                    } else if (nextDataLine == "[QUIET]") {
                        curBlipVol = blipVolQuiet;
                        curBloopVol = bloopVolQuiet;
                        return;
                    } else if (nextDataLine == "[FADEOFF]") {
                        DisableImagesOfRegularDialogBox();
                        ClearDialogBoxAndNameplateTextAndClearVisibleLinesCache();
                        text_dialog = GameObject.Find("text_dialog").GetComponent<TMP_Text>();
                        state_text_dialog = 101;
                        return;
                    } else if (nextDataLine == "[CLEAR]") {
                        ClearDialogBoxAndNameplateTextAndClearVisibleLinesCache();
                        visibleLinesCache.Clear();
                        return;
                        // [FADESONG name time vol]
                    } else if (0 == nextDataLine.IndexOf("[FADESONG", 0)) {
                        AudioHelper.instance.FadeSong(nextDataLine.Split(' ')[1], floatParse(nextDataLine, 2), floatParse(nextDataLine, 3));
                        return;
                        // [SOUND name volume pitch]
                    } else if (0 == nextDataLine.IndexOf("[SOUND", 0)) {
                        string soundName = nextDataLine.Split(' ')[1];
                        float vol = floatParse(nextDataLine, 2);
                        float pitch = floatParse(nextDataLine, 3);
                        AudioHelper.instance.playOneShot(soundName, vol, pitch);
                        return;
                    } else if (nextDataLine == "[NEWLINE]") {
                        text_dialog.text += "\n";
                        SetUnderText();
                        return;
                    } else if (0 == nextDataLine.IndexOf("[CUT TO")) {
                        // [CUT TO vvv]
                        string camname = nextDataLine.Split(' ')[2].Split(']')[0];
                        nameOfCameraToCutTo = camname;
                        return;
                    } else if (0 == nextDataLine.IndexOf("[SMOOTHCAM TO")) {
                        // [SMOOTHCAM TO targetVC in TIME ]
                        string camname = nextDataLine.Split(' ')[2];
                        nameOfCameraToSmoothTo = camname;
                        cameraLerpTime =  float.Parse(nextDataLine.Split(' ')[4].Split(']')[0],System.Globalization.CultureInfo.InvariantCulture);
                        return;
                    // faceanim THINGWITHMESHRENDERER x,y sizexsize (e.g. 0,1  1x2)
                    } else if (0 == nextDataLine.IndexOf("[faceanim ")) {
                        nextDataLine = nextDataLine.Replace("]", "");
                        string[] args = nextDataLine.Split(' ');
                        GameObject g = GameObject.Find(args[1]);
                        Material mat = null;
                        if (g.GetComponent<MeshRenderer>() != null) mat = g.GetComponent<MeshRenderer>().material;
                        if (g.GetComponent<SkinnedMeshRenderer>() != null) mat = g.GetComponent<SkinnedMeshRenderer>().material;
                        string pos = args[2];
                        int pos_x = int.Parse(pos.Split(',')[0],System.Globalization.CultureInfo.InvariantCulture);
                        int pos_y = int.Parse(pos.Split(',')[1],System.Globalization.CultureInfo.InvariantCulture);
                        // y neg to go down, x pos to go right
                        string size = args[3];
                        float size_x = float.Parse(size.Split('x')[0], System.Globalization.CultureInfo.InvariantCulture);
                        float size_y = float.Parse(size.Split('x')[1], System.Globalization.CultureInfo.InvariantCulture);
                        mat.SetTextureOffset("_MainTex", new Vector2(pos_x / size_x, -pos_y / size_y));
                        return;
                    }
                    if (InFadeTextMode == false && !oneChunkNoBox) EnableImagesOfRegularDialogBox();
                    if (InFadeTextMode) text_dialog.text += "\n";
                    if (oneChunkNoUnderlayer) {
                        text_dialog.color = Color.black;
                    }

                    colorTagStartIndices = new List<int>();
                    colorTagEndIndices = new List<int>();
                    colorTagColorInfo = new List<string>();
                    currentDialogCharIndex = 0;

                    spriteTagIndices.Clear();
                    spriteTags.Clear();



                    // Untag line, then un color tag it
                    string taggedLine = nextDataLine;
                    nextDataLine = "";
                    int removeTagsMode = 0;
                    string curTag = "";
                    int tagIndex = 0;
                    for (int i = 0; i < taggedLine.Length; i++) {
                        if (removeTagsMode == 0) {
                            if (taggedLine[i] == '<' && taggedLine[i+1] != 'c' && taggedLine[i + 1] != '/') {
                                curTag = "<";
                                removeTagsMode = 1;
                                nextDataLine += "OO"; // filler hack
                            } else {
                                tagIndex++;
                                nextDataLine += taggedLine[i];
                            }
                        } else if (removeTagsMode == 1) {
                            curTag += taggedLine[i];
                            if (taggedLine[i] == '>') {
                                removeTagsMode = 0;
                                spriteTagIndices.Add(tagIndex);
                                spriteTags.Add(curTag);
                            }
                        }
                    }

                    int colorIndex = 0;
                    colorIndex = nextDataLine.IndexOf("<color=");
                    string curColorInfo = "";
                    removeTagsMode = 0;
                    taggedLine = nextDataLine;
                    nextDataLine = "";
                    for (int i = 0; i < taggedLine.Length; i++) {
                        if (removeTagsMode == 0) {
                            if (colorIndex != -1 && i == colorIndex) { // start removing <color
                                colorTagStartIndices.Add(nextDataLine.Length);
                                removeTagsMode = 1;
                            } else {
                                nextDataLine += taggedLine[i];
                            }
                        } else if (removeTagsMode == 1) { // inside <tag>, skip all chars
                            if (taggedLine[i] == '>') {
                                colorTagColorInfo.Add(curColorInfo);
                                // print(curColorInfo);
                                curColorInfo = "";
                                removeTagsMode = 2;
                            } else {
                                if (i > colorIndex + 6) curColorInfo += taggedLine[i];
                            }
                        } else if (removeTagsMode == 2) { // add chars till </tag>
                            if (taggedLine[i] == '<') {

                                removeTagsMode = 3;
                            } else {
                                nextDataLine += taggedLine[i];
                            }
                        } else if (removeTagsMode == 3) {
                            if (taggedLine[i] == '>') {
                                removeTagsMode = 0;
                                colorTagEndIndices.Add(nextDataLine.Length);
                                colorIndex = taggedLine.IndexOf("<color", i);
                            }
                        }
                    }
                    

                    if (metadata.Contains("auto")) {
                        autoOnForCurrentLine = true;
                    }
                    if (metadata.Contains("color")) {
                        int i = metadata.IndexOf("color") + 1;
                        string colorID = metadata[i];
                        if (is2d) {
                            if (colorID == "1") ColorUtility.TryParseHtmlString("#54256AFF", out tempColor);
                            if (colorID == "2") ColorUtility.TryParseHtmlString("#621456FF", out tempColor);
                            DialogueBoxImage.color = tempColor;
                        }
                    } else if (is2d) {
                        if (isHorror) {
                            text_skip = null;
                            text_skip_image = null;
                            ColorUtility.TryParseHtmlString("#2A2C3500", out tempColor);
                            DialogueBoxImage.color = tempColor;
                            DialogueBoxImage.enabled = false;
                            AdvDialogIcon.GetComponent<Image>().enabled = false;
                        } else {
                            ColorUtility.TryParseHtmlString("#2A2C35FF", out tempColor);
                            DialogueBoxImage.color = tempColor;
                        }
                    }
                    if (inPico) {
                        ColorUtility.TryParseHtmlString("#0000A8FF", out tempColor);
                        DialogueBoxImage.color = tempColor;
                    }
                    if (text_skip_image != null) {
                        text_skip_image.color = DialogueBoxImage.color;
                    }

                    if (!InFadeTextMode && metadata.Contains("speaker")) {

                        nameplate.enabled = true;
                        //ID is either P: (which is removed then repaced with a speaker tag),
                        // or is NAME_NAME_NAME: (which has the _ removed before being a speaker tag)
                        int i = metadata.IndexOf("speaker") + 1;
                        string speakerID = metadata[i];
                        if (speakerID == "P" || speakerID == "Pal" || speakerID == "PAL" || speakerID == "PAL2") {
                            nameplate.sprite = pal_nameplate;
                            ColorUtility.TryParseHtmlString("#7FFFD3", out nametext_color);
                        } else if (speakerID == "CP" || speakerID == "CP2") {
                            nameplate.sprite = cp_nameplate;
                            ColorUtility.TryParseHtmlString("#E187A3", out nametext_color);
                        } else if (speakerID == "CV") {
                            nameplate.sprite = cv_nameplate;
                            ColorUtility.TryParseHtmlString("#FFCD4B", out nametext_color);
                        } else {
                            nameplate.sprite = generic_nameplate;
                            ColorUtility.TryParseHtmlString("#8BD6D7", out nametext_color);
                        }
                        nametext.color = nametext_color;
                        string[] names = DataLoader.instance.getDialogLines("nameTable");
                        string fullname = "";
                        foreach (string s in names) {
                            fullname = s.Split('=')[0];
                            if (fullname == speakerID) {
                                fullname = s.Split('=')[1]; break;
                            }
                            fullname = "";
                        }
                        if (fullname == "") fullname = speakerID;
                        nametext.text = fullname;
                        if (is2d) nametext.text = "";
                        float nameplateWidth = 11 + nametext.preferredWidth + 16;
                        // preferred width + 11 (left margin) + 16 (padding)
                        nameplate.rectTransform.sizeDelta = new Vector2(nameplateWidth, nameplate.rectTransform.sizeDelta.y);
                    } else {
                        nametext.text = "";
                        nameplate.enabled = false;
                    }

                    string currentText = text_dialog.text;
                    // Set the TMP object to show all of the currentDialog string, so we can
                    // calculate line lengths in order to show characters one at a time. 
                    nextDataLine = nextDataLine.Replace("\\n", "\n");
                    text_dialog.text = nextDataLine;
                    SetUnderText();
                    if (GameObject.Find("2D UI") != null) maxVisualLines = 3;
                    text_dialog.maxVisibleLines = maxVisualLines;
                    if (InFadeTextMode) text_dialog.maxVisibleLines = 100;
                    text_dialog.ForceMeshUpdate();

                    // Use the lastCharacterIndex of each line to split up the source string into lines, 
                    // in order to manage line display
                    TMP_LineInfo[] lineInfoList = text_dialog.GetTextInfo(text_dialog.text).lineInfo;
                    lineStrings = new List<string>();

                    int prevLineLast = -1;
                    foreach (TMP_LineInfo lineInfo in lineInfoList) {
                        int last = lineInfo.lastCharacterIndex;
                        int first = lineInfo.firstCharacterIndex;
                        if (first <= prevLineLast) break; // Sometimes lineInfo will give a FIRST that is <= the prev line's LAST...
                        prevLineLast = last;
                        if (first == 0 && last == 0) break;
                        if (first >= nextDataLine.Length) break;
                        if (last >= nextDataLine.Length) break;
                        string stringToAdd = nextDataLine.Substring(first, (last - first) + 1);
                        // The nextDataLine still has non-literal newlines so remove them
                        stringToAdd = stringToAdd.Replace("\n", "");
                        lineStrings.Add(stringToAdd);
                    }

                    // Keeps track of where to start pulling lines out of lineStrings
                    lineStringsIndex = 0;
                    curVislineCharIdx = 0;
                    text_dialog.text = currentText; // Restore the text.
                    SetUnderText();

                    state_text_dialog = 2;

                    linesLeftToShowBeforeInput = maxVisualLines;

                    if (DataLoader.instance.hasLineBeenRead(currentDialogueScene,currentDialogueIndexFromScene)) {
                        allowLineSkipWithCancel = true;
                        if (text_skip != null) text_skip.text = "";
                        if (text_skip_image != null) text_skip_image.enabled = false;
                        if (!InFadeTextMode && text_skip != null) {
                            text_skip.text = DataLoader.instance.getLine("pause-info-text", 7);
                            if (is2d && text_skip_image != null) {
                                text_skip.text = text_skip.text.Replace(" :", ":");
                                text_skip_image.enabled = true;
                                text_skip_image.rectTransform.sizeDelta = new Vector2(text_skip.preferredWidth + 8f, text_skip_image.rectTransform.sizeDelta.y);
                            }
                        }
                    } else {
                        allowLineSkipWithCancel = false;
                    }

                }
                break;
            case 1:
                // Check if TMP object needs to be pushed up.

                visibleLinesCache.Add(currentGrowingLineBuffer);
                currentGrowingLineBuffer = "";
                state_text_dialog = 2;
                // Fade Text Mode grows until reaching a [CLEAR] command.
                if (InFadeTextMode) {
                } else {
                    // When dialogue box hits max lines, remove the top line so text can be pushed up
                    if (visibleLinesCache.Count == maxVisualLines) {
                        if (is2d) NudgeTextVertically(-6);
                        if (!is2d) NudgeTextVertically(-7);
                        state_text_dialog = 1000;
                        bumpPauseTime = 0.21f;
                        visibleLinesCache.RemoveAt(0);
                        text_dialog.text = "";
                        foreach (string s in visibleLinesCache) { text_dialog.text += s; text_dialog.text += "\n"; }
                        SetUnderText();
                        if (useFastText || SaveManager.instantText) {
                            if (is2d) NudgeTextVertically(6);
                            if (!is2d) NudgeTextVertically(7);
                            state_text_dialog = 2;
                            bumpPauseTime = -1;
                        }
                    }
                }

                // If a color tag is still open from the previous line, start a new tag on this line (in case the start tag
                // line gets pushed off screen)
                if (colorNotClosedInCurrentBuffer) {
                    text_dialog.text += "<color="+mostRecentColorTagColor+">";
                    SetUnderText();
                    currentGrowingLineBuffer += "<color=" + mostRecentColorTagColor +">";
                }

                break;
            case 1000:
                bumpPauseTime -= Time.deltaTime;
                if (bumpPauseTime < 0) {
                    if (is2d) NudgeTextVertically(6);
                    if (!is2d) NudgeTextVertically(7);
                    state_text_dialog = 2;
                }
                break;
            case 2:
                int nrCharsToAdd = 1;
                float tmCharScaled = tmChar;
                if (MyInput.confirm || MyInput.cancel || MyInput.talk) {
                    tmCharScaled = 0.5f * tmChar;
                }

                if (useFastText) {
                    if (MyInput.confirm) {
                        nrCharsToAdd = 10;
                    } else {
                        tChar = tmCharScaled;
                        nrCharsToAdd = 100;
                    }
                } else if (SaveManager.instantText) {
                    tChar = tmCharScaled;
                    nrCharsToAdd = 100;
                }

                if (MyInput.jpCancel && allowLineSkipWithCancel) {
                    if (InFadeTextMode) {
                        skipCurrentLineFade = true;
                    } else {
                        skipCurrentLineNormal = true;
                    }
                }

                // add characters 1 by 1
                tChar += Time.deltaTime;
                if (tChar > tmCharScaled) {
                    if (tChar / tmCharScaled >= 2f) {
                        nrCharsToAdd = (int)(tChar / tmCharScaled);
                        tChar = 0.01f;
                    } else {
                        tChar -= tmCharScaled;
                    }

                    // Fade text mode should just show the entire dialogue
                    if (skipCurrentLineFade) {
                       nrCharsToAdd = lineStrings[lineStringsIndex].Length;
                    }

                    // Add the characters out of the current chunk of the line
                    // Insert in color tags.
                    // Add newlines at the end of chunks.
                    for (int aaa = 0; aaa < nrCharsToAdd; aaa++) {

                        // Sprite tags insert OO so text wraps right. detect 
                        if (spriteTagIndices.Contains(currentDialogCharIndex)) {
                            int spriteTagIndex = spriteTagIndices.IndexOf(currentDialogCharIndex);
                            
                            text_dialog.text += spriteTags[spriteTagIndex];
                            currentGrowingLineBuffer += spriteTags[spriteTagIndex];
                            currentDialogCharIndex += 2;
                            curVislineCharIdx += 2;
                            spriteTags.RemoveAt(0);
                            spriteTagIndices.RemoveAt(0);
                            // hack for multiple sprites
                            for (int i = 0; i < spriteTagIndices.Count; i++) {
                                spriteTagIndices[i] += 2;
                            }
                        }

                        // [1,44,65]
                        if (colorTagStartIndices.Contains(currentDialogCharIndex)) {
                            // 0, 1, 2
                            int colortagidx = colorTagStartIndices.IndexOf(currentDialogCharIndex);
                            // [#...,#...,#...]
                            text_dialog.text += "<color="+ colorTagColorInfo[colortagidx] + ">";
                            currentGrowingLineBuffer += "<color="+colorTagColorInfo[colortagidx]+">";
                            mostRecentColorTagColor = colorTagColorInfo[colortagidx];
                            colorNotClosedInCurrentBuffer = true;
                        }
                        if (colorTagEndIndices.Contains(currentDialogCharIndex)) {
                            text_dialog.text += "</color>";
                            currentGrowingLineBuffer += "</color>";
                            colorNotClosedInCurrentBuffer = false;
                        }


                        char charToAdd = lineStrings[lineStringsIndex][curVislineCharIdx];
                        text_dialog.text += charToAdd;
                        currentGrowingLineBuffer += charToAdd;
                        currentDialogCharIndex++;
                        curVislineCharIdx++;

                        if (skipCurrentLineNormal  || curVislineCharIdx == lineStrings[lineStringsIndex].Length) {
                            curVislineCharIdx = 0;

                            if (skipCurrentLineNormal) {
                                colorNotClosedInCurrentBuffer = false;
                            }
                            if (colorNotClosedInCurrentBuffer) {
                                text_dialog.text += "</color>";
                                currentGrowingLineBuffer += "</color>";
                            }

                            lineStrings[lineStringsIndex] = currentGrowingLineBuffer;

                            text_dialog.text += "\n";
                            currentGrowingLineBuffer += "";
                            if (skipCurrentLineNormal || lineStringsIndex + 1 == lineStrings.Count) {
                                // Showing stuff from this <Line> is done, so set advdialog to visible
                                state_text_dialog = 3;
                                colorNotClosedInCurrentBuffer = false;
                                _asBloop.clip = clipDialogBloop; _asBloop.volume = curBloopVol*(SaveManager.volume/100f); _asBloop.Play();

                                if (autoOnForCurrentLine) {

                                } else {
                                    AdvDialogIcon.gameObject.SetActive(true);
                                }
                                break;
                            } else {
                                // Otherwise there are more visual lines to print out, so print the next one.
                                lineStringsIndex++;
                                linesLeftToShowBeforeInput--;
                                // If showed enough new visual lines (after pushing), wait for next input.
                                if (linesLeftToShowBeforeInput == 0 && InFadeTextMode == false) {
                                    state_text_dialog = 5;
                                    AdvDialogIcon.gameObject.SetActive(true);
                                    _asBloop.clip = clipDialogBloop; _asBloop.volume = curBloopVol*(SaveManager.volume / 100f); _asBloop.Play();
                                } else {
                                    // Add line to displayed lines, maybe delete al ine to push up
                                    state_text_dialog = 1;
                                }
                                break;
                            }
                        } else {
                            // If the current character is not the last character in the string, and
                            // If the current character is a comma or period,
                            // and the next character is a space or period, then make a pause in the dialogue.
                            if (!useFastText && !SaveManager.instantText && "!,.?".IndexOf(lineStrings[lineStringsIndex][curVislineCharIdx - 1]) != -1) {
                                if (" .".IndexOf(lineStrings[lineStringsIndex][curVislineCharIdx]) != -1) {
                                    if (!MyInput.confirm && !MyInput.cancel) tChar = -0.08f; 
                                    
                                }
                            }
                        }
                    }

                    SetUnderText();

                }


                tBlip += Time.deltaTime;
                if (tBlip >= tmBlip && tChar > 0) {
                    tBlip = 0;
                    _audioSource.volume = curBlipVol * (SaveManager.volume/100f); _audioSource.PlayOneShot(clipDialogBlip);
                }
                break;
            case 3: // Wait for player input  to close out the dialogue.
                if (MyInput.jpConfirm || MyInput.jpTalk ||  MyInput.jpCancel || useFastText || autoOnForCurrentLine || skipCurrentLineNormal || skipCurrentLineFade) {

                    MyInput.jpConfirm = false;
                    MyInput.jpCancel = false;
                    MyInput.jpJump = false;
                    MyInput.jpTalk = false;

                    skipCurrentLineNormal = false;
                    skipCurrentLineFade = false;
                    state_text_dialog = 0;
                    AdvDialogIcon.gameObject.SetActive(false);

                    if (InFadeTextMode && dialogueQueue.Count >= 1) {
                        if (dialogueQueue.Count == 1) {
                            if (dialogueQueue[0].IndexOf("[CLEAR]") != -1 || dialogueQueue[0].IndexOf("[FADEOFF]") != -1) {
                                if (readStateSceneToUpdate != "") {
                                    DataLoader.instance.updateReadDialogueState(readStateSceneToUpdate, readStateStartIdx, readStateEndIdx);
                                    readStateSceneToUpdate = "";
                                }
                            }
                        }
                        return;
                    }
                    if (autoOnForCurrentLine) {
                        autoOnForCurrentLine = false;
                        if (dialogueQueue.Count == 0) {
                        }
                    } else {
                        ClearDialogBoxAndNameplateTextAndClearVisibleLinesCache(true);
                        if (InFadeTextMode) {
                            state_text_dialog = 101;
                        }
                    }

                    if (dialogueQueue.Count == 0) {
                        if (readStateSceneToUpdate != "") {
                            DataLoader.instance.updateReadDialogueState(readStateSceneToUpdate, readStateStartIdx, readStateEndIdx);
                            readStateSceneToUpdate = "";
                        }

                        curBlipVol = blipVol;
                        curBloopVol = bloopVol;
                        nametext.text = "";
                        if (text_skip != null) text_skip.text = "";
                        if (text_skip_image != null) text_skip_image.enabled = false;
                        DisableImagesOfRegularDialogBox();
                        if (oneChunkNoUnderlayer) {
                           // print("turn white");
                            text_dialog.color = Color.white;
                        }
                        oneChunkNoBox = false;
                        oneChunkNoUnderlayer = false;
                        if (oneChunkBoxY != -1) {
                            tempPos = DialogueBoxImage.transform.localPosition;
                            tempPos.y = initBoxY;
                            DialogueBoxImage.transform.localPosition = tempPos;
                        }
                        oneChunkBoxY = -1;
                    }
                }
                break;
            case 4:
                break;
            case 5: // Wait for player input before showing more dialogue. In FadeTextMode, this state is never reached.
                if (MyInput.confirm || MyInput.cancel || MyInput.talk || useFastText) {
                    AdvDialogIcon.gameObject.SetActive(false);
                    linesLeftToShowBeforeInput = maxVisualLines;
                    state_text_dialog = 1;
                }
                break;
            // Fade in FadeText layer
            case 100:
                if (FadeTextWithNoFade) {
                    state_text_dialog = 0;
                } else {
                    if (FadePart) {
                        tempColor = FadePartImage.color;
                    } else {
                        tempColor = UnderDialogueFadeImage.color;
                    }
                    tempColor.a += Time.deltaTime * 2.5f;
                    if (useFastText) tempColor.a += Time.deltaTime *12.5f;
                    float maxAlpha = 0.8f;
                    if (FadeFull) maxAlpha = 1;
                    tempColor.a = Mathf.Clamp(tempColor.a, 0, maxAlpha);
                    if (FadePart) {
                        FadePartImage.color = tempColor;
                    } else {
                        UnderDialogueFadeImage.color = tempColor;
                    }
                    if (tempColor.a >= maxAlpha) {
                        state_text_dialog = 0;
                        FadeFull = false;
                    }
                }
                    
                break;
            case 101:
                // neeeded when changign scesns at end of a fade text without showing the game area again
                if (dontFadeOutAtEndOfFadeText || FadeTextWithNoFade) {
                    InFadeTextMode = false;
                    FadeTextWithNoFade = false;
                    state_text_dialog = 0;
                } else {
                    if (FadePart) {
                        tempColor = FadePartImage.color;
                    } else {
                        tempColor = UnderDialogueFadeImage.color;
                    }
                    tempColor.a -= Time.deltaTime * 2.5f;
                    if (useFastText) tempColor.a -= Time.deltaTime * 12.5f;
                    tempColor.a = Mathf.Clamp(tempColor.a, 0, 1);
                    if (FadePart) {
                        FadePartImage.color = tempColor;
                    } else {
                        UnderDialogueFadeImage.color = tempColor;
                    }
                    if (tempColor.a == 0) {
                        InFadeTextMode = false;
                        if (FadePart) {
                            FadePart = false;
                            GameObject.Find("fadetext").GetComponent<TMP_Text>().fontSize = GetFadetextFontSize();
                            GameObject.Find("fadetext").GetComponent<RectTransform>().anchoredPosition = CachedFadetextAnchoredPos;
                            AdvDialogIcon.GetComponent<Oscillate>().UncachePos();
                        }
                        state_text_dialog = 0;
                    }
                }


                break;
        }
    }

    float GetFadetextFontSize() {
        return 48f;
    }
    float GetFadetextPartFontSize() {
        return 40f;
    }

    [System.NonSerialized]
    public bool dontFadeOutAtEndOfFadeText = false;

    Vector3 tempPos = new Vector3();
    private bool FadePart;
    private Vector3 CachedFadetextAnchoredPos;

    void NudgeTextVertically(float amount) {
        if (textHasUnderlayer) {
            tempPos = text_dialog_under.transform.localPosition;
            tempPos.y += amount;
            text_dialog_under.transform.localPosition = tempPos;
        } else {
            tempPos = text_dialog.transform.localPosition;
            tempPos.y += amount;
            text_dialog.transform.localPosition = tempPos;
        }
    }

    void SetUnderText() {
        if (textHasUnderlayer) {
            string s = text_dialog.text;

            // color tag hack
            int lastindex = 0;
            for (int i = 0; i < 5; i++) {
                int a = 0;
                a = s.IndexOf("<color=#",lastindex);
                lastindex = a+1;
                if (a != -1) {
                    string ss = s.Substring(a, "<color=#000000".Length);
                    s = s.Replace(ss, "<color=#000000");
                } else {
                    break;
                }
            }
            text_dialog_under.text = s;
            if (InFadeTextMode || ("rock-messages" == readStateSceneToUpdate && readStateStartIdx == 3) || ("yolk1" == readStateSceneToUpdate && readStateEndIdx == 7)) {
                text_dialog_under.text = "";
            }
            if (oneChunkNoUnderlayer) {
                text_dialog_under.text = "";
            }
        }
    }

    float en_2DTextFontSize = -1;

    public void RefreshFonts() {
        TMP_Text _fadeText = GameObject.Find("fadetext").GetComponent<TMP_Text>();
        TMP_Text _text_dialog = GameObject.Find("text_dialog").GetComponent<TMP_Text>();
        if (is2d) {
            if (en_2DTextFontSize == -1) en_2DTextFontSize = _text_dialog.fontSize;
            if (SaveManager.language == "zh-simp") {
                maxVisualLines = 2;

                _fadeText.fontSize = _text_dialog.fontSize = 9;
                text_dialog_under.fontSize = text_skip.fontSize = 9;

                _text_dialog.lineSpacing = text_dialog_under.lineSpacing = 1.5f;
                _fadeText.characterSpacing = _text_dialog.characterSpacing = text_dialog_under.characterSpacing = 0.6f;

                _fadeText.lineSpacing = 8f;
            } else if (SaveManager.language == "zh-trad") {
                maxVisualLines = 2;

                _fadeText.fontSize = _text_dialog.fontSize = 9;
                text_dialog_under.fontSize = text_skip.fontSize = 9;

                _text_dialog.lineSpacing = text_dialog_under.lineSpacing = 1.5f;
                _fadeText.characterSpacing = _text_dialog.characterSpacing = text_dialog_under.characterSpacing = 0.6f;

                _fadeText.lineSpacing = 8f;
            } else if (SaveManager.language == "jp") {
                maxVisualLines = 3;

                _fadeText.fontSize = _text_dialog.fontSize = 9;
                text_dialog_under.fontSize = text_skip.fontSize = 9;

                _fadeText.lineSpacing = _text_dialog.lineSpacing = text_dialog_under.lineSpacing = 9f; 
                _fadeText.characterSpacing = _text_dialog.characterSpacing = text_dialog_under.characterSpacing = 0;

                text_skip.characterSpacing = 0;
            } else {
                if (SaveManager.language != "en") {
                    if (SaveManager.language == "ru") {
                        en_2DTextFontSize = 9f;
                    } else {
                        en_2DTextFontSize = 16f;
                    }
                } else {
                    en_2DTextFontSize = 8f;
                }
                maxVisualLines = 3;
                if (en_2DTextFontSize != -1) {
                    _fadeText.fontSize = _text_dialog.fontSize = en_2DTextFontSize;
                    text_dialog_under.fontSize = text_skip.fontSize = en_2DTextFontSize;

                    _text_dialog.lineSpacing = text_dialog_under.lineSpacing = 0f;
                    _fadeText.characterSpacing =_text_dialog.characterSpacing = text_dialog_under.characterSpacing = 0f;
                    if (SaveManager.language == "ru") {
                        _fadeText.characterSpacing = _text_dialog.characterSpacing = text_dialog_under.characterSpacing = -2f;
                    }
                }

                _fadeText.lineSpacing = 0f;
            }
            string curSceneName = SceneManager.GetActiveScene().name;
            int otherSceneInfo = 0;
            if (curSceneName == "NanoAlbumen") otherSceneInfo = 1;
            if (SparkGameController.SparkGameDestObjectName == "Yolk1Door") otherSceneInfo = 1;
            if (SparkGameController.SparkGameDestObjectName == "Yolk2Door") otherSceneInfo = 2;
            if (SparkGameController.SparkGameDestObjectName == "Yolk3Door") otherSceneInfo = 3;
            if (curSceneName == "NanoNexus") otherSceneInfo = 2;
            GameObject.Find("2DAreaTitle_Bottom_Lower").GetComponent<TMP_Text>().text = HF.GetSceneAssociatedText(HF.SceneNameToEnum(curSceneName), "areanames", otherSceneInfo).ToUpper();
            GameObject.Find("2DAreaTitle_Bottom_Top").GetComponent<TMP_Text>().text = HF.GetSceneAssociatedText(HF.SceneNameToEnum(curSceneName), "areanames", otherSceneInfo).ToUpper();

        } else {
            if (SaveManager.language == "zh-simp") {
                _fadeText.font = DataLoader.instance.Font_ChineseForum;
                _text_dialog.font = DataLoader.instance.Font_ChineseKreon;
                text_skip.font = DataLoader.instance.Font_ChineseKreon;
                nametext.font = DataLoader.instance.Font_ChineseKreon;

                _fadeText.characterSpacing = nametext.characterSpacing = text_skip.characterSpacing = _text_dialog.characterSpacing = 0f;

                _fadeText.lineSpacing = 0f;
                _fadeText.fontSize = 48f;
                _text_dialog.lineSpacing = 0f;
                _text_dialog.fontSize = 53f;
            } else if (SaveManager.language == "zh-trad") {
                _fadeText.font = DataLoader.instance.Font_TradChineseForum;
                _text_dialog.font = DataLoader.instance.Font_TradChineseKreon;
                text_skip.font = DataLoader.instance.Font_TradChineseKreon;
                nametext.font = DataLoader.instance.Font_TradChineseKreon;

                _fadeText.characterSpacing = nametext.characterSpacing = text_skip.characterSpacing = _text_dialog.characterSpacing = 0f;

                _fadeText.lineSpacing = 0f;
                _fadeText.fontSize = 48f;
                _text_dialog.lineSpacing = 0f;
                _text_dialog.fontSize = 53f;
            } else if (SaveManager.language == "jp") {
                _fadeText.font = DataLoader.instance.Font_JpForum;
                _text_dialog.font = DataLoader.instance.Font_JpKreon;
                text_skip.font = DataLoader.instance.Font_JpKreon;
                nametext.font = DataLoader.instance.Font_JpKreon;

                _fadeText.characterSpacing = nametext.characterSpacing = text_skip.characterSpacing = _text_dialog.characterSpacing = -3f;

                _fadeText.lineSpacing = -10f;
                _fadeText.fontSize = 42f;
                _text_dialog.lineSpacing = -7f;
                _text_dialog.fontSize = 46f;
            } else {
                _fadeText.characterSpacing = nametext.characterSpacing = text_skip.characterSpacing = _text_dialog.characterSpacing = 0f;

                _fadeText.lineSpacing = 0f;
                _fadeText.fontSize = 48f;
                _text_dialog.lineSpacing = 0f;
                _text_dialog.fontSize = 46f;
            }
        }
    }
}
