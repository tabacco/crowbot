using UnityEngine;
using System.Collections;

public class Game : MonoBehaviour {
	Scarecrow 	mScarecrow;
	CornStalk[] mCornStalks;
	Crow 		mCrow;
	
	// music
	public AudioClip kNormalMusic;
	public AudioClip kDeathMusic;
	
	// Dialog
	TextMesh dialogMesh;
	int dialogSeconds;
	float dialogTimeDisplayed;
	int lastDialogDay;
	int lastDialogLine;
	
	// GUI
	Texture2D batteryIcon;
	
	Texture2D batteryMeterTexture;
	Texture2D hungerMeterTexture;
	Texture2D stressMeterTexture;
	
	Color batteryMeterColor;
	Color hungerMeterColor;
	Color stressMeterColor;
	
	string[] crowDeadLines;
	bool crowKnownDead;
	bool robotKnownDead;
	bool cornKnownDead;
	
	float menuTimer;
	
	// The day/night cycle
	DayNightCycle dnc;
	
	// Use this for initialization
	void Start () {
		mScarecrow 	= FindObjectOfType(typeof(Scarecrow)) as Scarecrow;
		mCrow		= FindObjectOfType(typeof(Crow)) as Crow;
		mCornStalks = FindObjectsOfType(typeof(CornStalk)) as CornStalk[];
		
		dialogMesh = FindObjectOfType(typeof(TextMesh)) as TextMesh;
		dialogMesh.renderer.enabled = false;
		dialogTimeDisplayed = 0f;
		lastDialogDay = 0;
		lastDialogLine = 0;
		
		batteryIcon = (Texture2D) Resources.Load("PowerIcon", typeof(Texture2D));
		
		batteryMeterColor = Color.green;
		hungerMeterColor = Color.green;
		stressMeterColor = Color.green;
		
		batteryMeterTexture = GetMeterTexture(batteryMeterColor);
		hungerMeterTexture = GetMeterTexture(hungerMeterColor);
		stressMeterTexture = GetMeterTexture(stressMeterColor);
		
		dnc = FindObjectOfType(typeof(DayNightCycle)) as DayNightCycle;
		
		crowKnownDead = false;
		robotKnownDead = false;
		cornKnownDead = false;
		
		
		crowDeadLines = new string[18];
		crowDeadLines[0] = "Oh God! What have I done!?";
		crowDeadLines[1] = "Filbert. I had named him Filbert.";
		crowDeadLines[2] = "What do I do now?";
		crowDeadLines[3] = "I'm so sorry Filbert. I'm so sorry...";
		crowDeadLines[4] = "I... don't know what to do...";
		crowDeadLines[5] = "Thought up a song for Filbert today.\nHe would have liked it, I think";
		crowDeadLines[6] = "Why did you have to die?";
		crowDeadLines[7] = "I miss you...";
		crowDeadLines[8] = "Was this all I was created for?";
		crowDeadLines[9] = "Where do I go from here?";
		crowDeadLines[10] = "Filbert... Filbert... where are you...";
		crowDeadLines[11] = "Where are you God?";
		crowDeadLines[12] = "Why am I here?";
		crowDeadLines[13] = "Life is meaningless...";
		crowDeadLines[14] = "The corn does not need me.";
		crowDeadLines[15] = "The corn does not want me.";
		crowDeadLines[16] = "I don't want this anymore.";
		crowDeadLines[17] = "What's the point?";
		
		audio.clip = kNormalMusic;
		audio.Play();
		
		menuTimer = 0f;
		
	}
	
	// Update is called once per frame
	void Update () {
		
		if(Input.GetKeyDown(KeyCode.Space)) {
			PauseUnpause();
		}
		
		dialogTimeDisplayed += Time.deltaTime;
		if(dialogTimeDisplayed > dialogSeconds) {
			HideDialogLine();
		}
		
		if(mCrow.GetCurrentStateID() == CrowStates.Dead) {
			crowKnownDead = true;
			BirdDeadDialog();
			dnc.dayLength = 10;
		}
		
		if(!mScarecrow.isAlive) {
			if(!robotKnownDead && mCrow.GetCurrentStateID() != CrowStates.Dead) {
				ShowDialogLine("My life ends. My purpose fulfilled. I die happily");
			}
			robotKnownDead = true;
		}

		if(!cornKnownDead) {
			float totalCorn = 0f;
			foreach(CornStalk corn in mCornStalks) {
				totalCorn += corn.mCorn;
			}
			
			if(totalCorn == 0) {
				cornKnownDead = true;
				ShowDialogLine("God, I have failed you.");
			}
		}
		
		if(robotKnownDead || (lastDialogLine >= crowDeadLines.Length)) {
			ResetToMenuTimer();
		}
		
		
	}
	
	void OnGUI () {
		
		if(mCrow.GetCurrentStateID() != CrowStates.Dead) {
			DrawHungerMeter();
			DrawStressMeter();
		}
		
		if(mScarecrow.isAlive) {
			DrawBatteryMeter();
		}
		
		if(!mScarecrow.isAlive || (mCrow.GetCurrentStateID() == CrowStates.Dead)) {
			if(GUI.Button(new Rect((Screen.width/2)-100, Screen.height - 100, 200, 50), "Restart Game")) {
				Application.LoadLevel("Main");
			}
		}
		
	}
	
	public void OnBirdDeath() {
		audio.clip = kDeathMusic;
		audio.Play();	
	}
	
	void BirdDeadDialog() {
		int daysSinceBirdDied = dnc.getTimeSinceEvent("CrowDied");
		if(daysSinceBirdDied > lastDialogDay) {
			if(lastDialogLine >= crowDeadLines.Length) {
				return;
			}
			
			ShowDialogLine("Day " + daysSinceBirdDied + ": " + crowDeadLines[lastDialogLine]);
			lastDialogLine += 1;
			lastDialogDay = daysSinceBirdDied;
		}
	}
	
	public void ShowDialogLine(object line) {
		HideDialogLine();
		string text = line.ToString();
		dialogSeconds = 15;
		dialogMesh.text = text;
		dialogMesh.renderer.enabled = true;
	}
	
	void HideDialogLine() {
		dialogMesh.renderer.enabled = false;
		dialogTimeDisplayed = 0f;
	}
	
	void ResetToMenuTimer() {
		menuTimer += Time.deltaTime;
		if(menuTimer > 5f) {
			Application.LoadLevel("Menu");
		}
	}
	
	void PauseUnpause() {
		if(Time.timeScale == 0f) {
			HideDialogLine();
			Time.timeScale = 1f;
		} else {
			ShowDialogLine("Game Paused");
			Time.timeScale = 0f;
		}
	}
	
	Texture2D GetMeterTexture(Color initialColor) {
		Texture2D texture = new Texture2D(1, 1);
		texture.SetPixel(0, 0, initialColor);
		texture.Apply();
		return texture;
	}
	
	void DrawStressMeter() {
		GUI.Label(new Rect((Screen.width / 2) + 20, 20, 60, 20), "Stress");
		
		if(mCrow.mStress > 0.75f) {
			stressMeterColor = Color.red;
		} else if(mCrow.mStress > 0.5f) {
			stressMeterColor = Color.yellow;
		} else {
			stressMeterColor = Color.green;
		}
		
		if(stressMeterTexture.GetPixel(0, 0) != stressMeterColor) {
			stressMeterTexture.SetPixel(0, 0, Color.Lerp(stressMeterTexture.GetPixel(0, 0), stressMeterColor, Time.deltaTime*2));
			stressMeterTexture.Apply();
		}
		
		GUI.DrawTexture(new Rect((Screen.width/2) + 80, 20, ((Screen.width/2) - 100) * mCrow.mStress, 20), stressMeterTexture);
	}
	
	void DrawHungerMeter() {
		// Hungry Meter (Hungrometer)
		GUI.Label(new Rect(20, 20, 60, 20), "Hunger");
		
		if(mCrow.mHunger > 0.75f) {
			hungerMeterColor = Color.red;
		} else if(mCrow.mHunger > 0.5f) {
			hungerMeterColor = Color.yellow;
		} else {
			hungerMeterColor = Color.green;
		}
		
		if(hungerMeterTexture.GetPixel(0, 0) != hungerMeterColor) {
			hungerMeterTexture.SetPixel(0, 0, Color.Lerp(hungerMeterTexture.GetPixel(0, 0), hungerMeterColor, Time.deltaTime*2));
			hungerMeterTexture.Apply();
		}
		
		GUI.DrawTexture(new Rect(100, 20, ((Screen.width/2) - 100) * mCrow.mHunger, 20), hungerMeterTexture);
	}
	
	void DrawBatteryMeter() {
		GUI.DrawTexture(new Rect(20, Screen.height - 40, batteryIcon.width/2, batteryIcon.height/2), batteryIcon);

		if(mScarecrow.mEnergy < 0.25f) {
			batteryMeterColor = Color.red;
		} else if(mScarecrow.mEnergy < 0.5f) {
			batteryMeterColor = Color.yellow;
		} else {
			batteryMeterColor = Color.green;
		}
		
		if(batteryMeterTexture.GetPixel(0, 0) != batteryMeterColor) {
			batteryMeterTexture.SetPixel(0, 0, Color.Lerp(batteryMeterTexture.GetPixel(0, 0), batteryMeterColor, Time.deltaTime*2));
			batteryMeterTexture.Apply();
		}
		
		GUI.DrawTexture(new Rect(60, Screen.height - 40, (Screen.width - 80) * mScarecrow.mEnergy, 20), batteryMeterTexture);
	}
	
}
