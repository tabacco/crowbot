using UnityEngine;
using System.Collections;

public class Scarecrow : MonoBehaviour {
	public float mEnergyRate = 0.005f;
	
	public float mEnergy;
	public float mMovementIntensity;
	
	public bool isAlive;
	Game game;
	
	// Use this for initialization
	void Start () {
		mEnergy = 1f;
		mMovementIntensity = 0f;
		isAlive = true;
		game = FindObjectOfType(typeof(Game)) as Game;
	}
	
	// Update is called once per frame
	void Update () {
		float currentDrain = mEnergyRate + (mMovementIntensity / 30);
		mEnergy = Mathf.Clamp( mEnergy - currentDrain * Time.deltaTime, 0f, 1f );
		
		if(mEnergy == 0) {
			Die();
		}
	}
	
	void Die () {
		isAlive = false;
		mMovementIntensity = 0f;
	}
}
