using UnityEngine;
using System.Collections;

public class GoToMain : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void goToGame()
	{
		Application.LoadLevel("Main");
	}
}
