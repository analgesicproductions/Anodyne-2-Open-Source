using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MyAudioVisualizer : MonoBehaviour {

	int numBars = 10;
	float[] _samples =   new float[256];
	float[] _samples2 = new float[256];
//	float[] _samples3 = new float[256];
//	float[] _samples4 = new float[256];
	float[] _freqBand;
    float[] buffer;
	public float[] ampScale;

	AudioSource src1_1;
	AudioSource src1_2;
	AudioSource src2_1;
	AudioSource src2_2;
   // AudioSource src3_1;
   // AudioSource src3_2;
   // AudioSource src4_1;
   // AudioSource src4_2;

    RectTransform[] bars;
	AudioHelper ah;
    public GameObject AudioVisualizerBar;
    // Use this for initialization
    void Start () {

		ah = GameObject.Find("AudioHelper").GetComponent<AudioHelper>();
		_freqBand = new float[numBars];
		buffer = new float[numBars];
		src1_1 = GameObject.Find("Track 1-1").GetComponent<AudioSource>();
		src1_2 = GameObject.Find("Track 1-2").GetComponent<AudioSource>();
        src2_1 = GameObject.Find("Track 2-1").GetComponent<AudioSource>();
        src2_2 = GameObject.Find("Track 2-2").GetComponent<AudioSource>();
  //      src3_1 = GameObject.Find("Track 3-1").GetComponent<AudioSource>();
   //     src3_2 = GameObject.Find("Track 3-2").GetComponent<AudioSource>();
    //    src4_1 = GameObject.Find("Track 4-1").GetComponent<AudioSource>();
     //   src4_2 = GameObject.Find("Track 4-2").GetComponent<AudioSource>();
        bars = new RectTransform[numBars];
		for (int i = 0; i < numBars; i++) {
			GameObject bar = (GameObject) Instantiate(AudioVisualizerBar,transform);
			Vector3 pos = new Vector3(120, 52,0);
			Vector3 offset = new Vector3(5,0,0);
			RectTransform r = bar.GetComponent<RectTransform>();
			r.localPosition = pos + offset*i;
			bars[i] = r;
		}
        AudioVisualizerBar.SetActive(false);
	}
	[Tooltip("How high a bar can go.")]
	public float maxBarHeight = 15f;
	[Tooltip("The band-sampled amplitude that will be used for normalizing heights")]
	public float maxSampleVal = 0.15f;
	[Tooltip("How long a buffered value takes to fade down to a lower height.")]
	public float fadeDownTime = 1f;

	[Tooltip("Used in lerping a buffered value to a higher ehight")]
	public float fadeUpScaling = 0.2f;

	[Tooltip("Buffer value will not move up unless freq band is this much greater")]
	public float changeWindow = 0.01f;

	[Tooltip("Amplitudes below this appear as zero")]
	public float minimumVisualizedValue = 0.005f;
	// Update is called once per frame
	void Update () {
		// 22050 / 128 = 172.265625
		// 0 - 1 (0 - 172)
		// 1 - 1 (172-344)
		// 2 - 1 (344-516)
		// 3 - 2 (516-855)
		// 4 - 3 (855-1360)
		// 5 - 4 (1360-2000)
		// 6 - 4 (2000-2700)
		// 7 - 16 (2700 -9000ish)

		if (ah.whichTrackLoopIsPlaying(0) == 0)	src1_1.GetSpectrumData(_samples,0,FFTWindow.Hamming);		
		if (ah.whichTrackLoopIsPlaying(0) == 1)	src1_2.GetSpectrumData(_samples,0,FFTWindow.Hamming);
        if (ah.whichTrackLoopIsPlaying(1) == 0) src2_1.GetSpectrumData(_samples2, 0, FFTWindow.Hamming);
        if (ah.whichTrackLoopIsPlaying(1) == 1) src2_2.GetSpectrumData(_samples2, 0, FFTWindow.Hamming);
       // if (ah.whichTrackLoopIsPlaying(2) == 0) src3_1.GetSpectrumData(_samples3, 0, FFTWindow.Hamming);
       // if (ah.whichTrackLoopIsPlaying(2) == 1) src3_2.GetSpectrumData(_samples3, 0, FFTWindow.Hamming);
       // if (ah.whichTrackLoopIsPlaying(3) == 0) src4_1.GetSpectrumData(_samples4, 0, FFTWindow.Hamming);
       // if (ah.whichTrackLoopIsPlaying(3) == 1) src4_2.GetSpectrumData(_samples4, 0, FFTWindow.Hamming);

        int count = 0;
		for (int i = 0; i < numBars; i++) {
			int samplesToAdd = 0;
			if (i ==0) samplesToAdd = 1;
			if (i ==1) samplesToAdd = 1;
			if (i ==2) samplesToAdd = 1;
			if (i ==3) samplesToAdd = 1;
			if (i ==4) samplesToAdd = 2;
			if (i ==5) samplesToAdd = 2;
			if (i ==6) samplesToAdd = 3;
			if (i ==7) samplesToAdd = 4;
			if (i ==8) samplesToAdd = 40;
			if (i ==9) samplesToAdd = 80;
		
			_freqBand[i] = 0;
			for (int j = 0; j < samplesToAdd; j++) {
                _freqBand[i] += _samples[count];
                _freqBand[i] += _samples2[count];
               // _freqBand[i] += _samples3[count];
               // _freqBand[i] += _samples4[count];
				//if (_samples[count] > _samples2[count]) {
				//	_freqBand[i] += _samples[count];
				//} else {
				//	_freqBand[i] += _samples2[count];
				//}
				count++;
			}
			_freqBand[i] /= samplesToAdd; // Take average
			_freqBand[i] /= maxSampleVal; // Normalize to 0,1
			_freqBand[i] *= ampScale[i];
			if (_freqBand[i] > 1) _freqBand[i] = 1;
			// Now scale these freqs to make up for lower freqs usually being higher



			// Let the bar meet the max height if the current band is bigger
			// otherwise slowly decrease.
			if (_freqBand[i] > buffer[i]) {
				if (_freqBand[i] > minimumVisualizedValue && _freqBand[i] - buffer[i] > changeWindow) {
					buffer[i] = Mathf.Lerp(buffer[i],_freqBand[i],fadeUpScaling);
				}
			} else if (buffer[i] > 0) {
				buffer[i] -= (Time.deltaTime/fadeDownTime);
				if (i == 8) buffer[i] -= 4.5f*(Time.deltaTime/fadeDownTime);
				if (i == 9) buffer[i] -= 6*(Time.deltaTime/fadeDownTime);
				if (buffer[i] < 0) buffer[i] = 0;
			}
			bar_r = bars[i];
			bar_r.localScale = new Vector3(1,1 + maxBarHeight*buffer[i],1);
		}


	}
    RectTransform bar_r = new RectTransform();
}
