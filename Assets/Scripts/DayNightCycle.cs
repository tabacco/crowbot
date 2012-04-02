using UnityEngine;
using System.Collections;

public class DayNightCycle : MonoBehaviour {
	
	public int dayLength;
	public int daysElapsed;
	Hashtable eventDays;
	float currentTime;
	float fadeTime;
	GameObject sun;

	// Use this for initialization
	void Start () {
		sun = GameObject.Find("Sun");
		dayLength = 30;
		currentTime = 0.0f;
		daysElapsed = 0;
		eventDays = new Hashtable();
	}
	
	// Update is called once per frame
	void Update () {
		fadeTime = Mathf.Max(dayLength / 10, 1f);
		currentTime += Time.deltaTime;
		
		if(currentTime > dayLength) {
			currentTime = 0f;
			daysElapsed += 1;
		}
		
		if(currentTime < (dayLength / 2)) {
			sun.light.color = Color.Lerp(sun.light.color, Color.white, Time.deltaTime / fadeTime);
		} else {
			sun.light.color = Color.Lerp(sun.light.color, Color.blue, Time.deltaTime / fadeTime);
		}
		
	}
	
	public void recordEvent(string name) {
		eventDays.Add (name, daysElapsed);
	}
	
	public int getTimeSinceEvent(string name) {
		if(!eventDays.ContainsKey(name)) {
			return 0;
		}
		
		return daysElapsed - (int) eventDays[name];
	}
	
}
