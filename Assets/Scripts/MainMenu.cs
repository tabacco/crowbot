using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour {
	Object lawsOfRobotics;
	bool introPlaying;

	// Use this for initialization
	void Start () {
		introPlaying = false;
	}
	
	// Update is called once per frame
	void Update () {
		
		if (introPlaying){
			return;
		}
		
		if(SystemInfo.deviceType == DeviceType.Desktop) {
			if(Input.GetKeyDown(KeyCode.Return)) {
				GameBegin();
			}
		} else if(SystemInfo.deviceType == DeviceType.Handheld) {
			foreach (Touch touch in Input.touches) {
				if(touch.phase == TouchPhase.Stationary) {
					GameBegin();
				}
	        }
		}
		

		
	}
	
	void GameBegin() {
		GameObject.Find("Splash").guiTexture.enabled = false;
		lawsOfRobotics = Instantiate(Resources.Load("3LawsIntro"), new Vector3(1,1,1), Quaternion.identity);
		camera.orthographicSize = 30f;
		introPlaying = true;
	}

}
