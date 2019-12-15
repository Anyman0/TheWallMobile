using UnityEngine;
using System.Collections;

public class screenshot : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKey(KeyCode.Space))
		{
			ScreenCapture.CaptureScreenshot("shot_" + Time.time + ".png", 2);
		}
	}
}
