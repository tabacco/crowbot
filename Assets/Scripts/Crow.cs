using UnityEngine;
using System.Collections;



public class Crow : MonoBehaviour {
	public enum CrowStates { Idle, Eating, FlyingToCorn, Fleeing, Dead }
	
	public float kHungerRate 			= 0.01f;
	public float kStressRate 			= -0.01f;
	public float kEatTime				= 3f;
	public float kTimeAfterEating 		= 3f;
	public float kFleeStressThreshold 	= 0.75f;
	public float kNormalStressThreshold = 0.5f;
	public float kHungerReturnThreshold = 0.8f;
	public float kHitStressPercentage	= 0.5f;
	
	public AudioClip kCawSound;
	public AudioClip kFlapSound;
	public AudioClip[] kPeckSounds;
	public AudioClip kHitSound;
	
	public GameObject featherPrefab;
	
	public float 	mHunger;
	public float 	mStress;
	
	public CrowStates mCurState;
	
	Scarecrow mScarecrow;
	
	//----------------------------------------------------------------------------------------------------
	// Use this for initialization
	//----------------------------------------------------------------------------------------------------
	void Start () {
		mHunger = 0.5f;
		mStress = 0f;
		
		mScarecrow = FindObjectOfType(typeof(Scarecrow)) as Scarecrow;
		
		StartFlyingToCornStalk( PickBestCornStalk(null)  );
		
		StartCoroutine( "PlayCawSound" );
	}
	
	//----------------------------------------------------------------------------------------------------
	// Update is called once per frame
	//----------------------------------------------------------------------------------------------------
	void Update () {
		if( mCurState == CrowStates.Dead ) {
			DayNightCycle dnc = FindObjectOfType(typeof(DayNightCycle)) as DayNightCycle;
			print( dnc.getTimeSinceEvent( "CrowDied" ) );			
			return;
		}
		
		// increase hunger if not eating
		if( mCurState != CrowStates.Eating ) {
			mHunger = Mathf.Clamp(mHunger + kHungerRate * Time.deltaTime, 0f, 1f);
		}
				
		// increase stress if near flailing scarecrow
		if( mCurState != CrowStates.Fleeing ) {
			float distanceMultiplier = ( 8f - Vector3.Distance( transform.position, mScarecrow.transform.position ) ) / 8f;
			distanceMultiplier = Mathf.Clamp( distanceMultiplier, 0f, 1f );
			float stressAmount = mScarecrow.mMovementIntensity * distanceMultiplier * Time.deltaTime;
			//print( "stressing bird: " + stressAmount );
			Scare( stressAmount );
		}
		// decrease stress every frame
		mStress = Mathf.Clamp(mStress + kStressRate * Time.deltaTime, 0f, 1f);
		
		// if too hungry or stressed, die
		if( mHunger >= 1f || mStress >= 1f ) {
			Die();
			return;
		}
	}
	
	IEnumerator PlayCawSound() {
		while(true) {
			if( mCurState != CrowStates.Eating && mCurState != CrowStates.Dead ) {
				audio.PlayOneShot(kCawSound);
				yield return new WaitForSeconds( Random.Range( 0.25f, 3f ) );
			}
			else yield return 0;
		}
	}
	
	void SwitchFacing( bool bFaceLeft ) {
		Vector3 scale = transform.localScale;
		if( bFaceLeft ) scale.z = -1f;
		else scale.z = 1f;
		transform.localScale = scale;
	}
	
	//----------------------------------------------------------------------------------------------------
	// Handle collision events
	//----------------------------------------------------------------------------------------------------
	void OnCollisionEnter( Collision c ) {
		if( c.gameObject.transform.root.name == "Scarecrow" ) {
			print("Hit Scarecrow");
			Hit( kHitStressPercentage );
		}
	}
	
	//----------------------------------------------------------------------------------------------------
	// Fly to the specified position
	//----------------------------------------------------------------------------------------------------
	IEnumerator FlyToPosition( Vector3 targetPos ) {
		float maxSpeed = 0.08f;
		
		// face the direction we're going
		SwitchFacing( transform.position.z > targetPos.z );
		
		while( Vector3.Distance( transform.position, targetPos ) > 0.05f ) {
			transform.position = Vector3.MoveTowards( transform.position, targetPos, maxSpeed );
			yield return 0;
		}		
	}
	
	void TurnOffSprites() {
		transform.Find("Sprite_Calm").renderer.enabled = false;	
		transform.Find("Sprite_Flee").renderer.enabled = false;	
		transform.Find("Sprite_Dead").renderer.enabled = false;	
		transform.Find("Sprite_Eat").renderer.enabled = false;	
	}
	
	void SwitchSprite( string name ) {
		TurnOffSprites();
		transform.Find(name).renderer.enabled = true;
	}
	
	//----------------------------------------------------------------------------------------------------
	// Behavior - Idle
	//----------------------------------------------------------------------------------------------------
	void StartIdling() {
		mCurState = CrowStates.Idle;
		SwitchSprite("Sprite_Idle");
	}
	
	//----------------------------------------------------------------------------------------------------
	// Behavior - Eating
	//----------------------------------------------------------------------------------------------------
	void StartEating( CornStalk stalk ) {
		if( stalk.mCorn <= 0f ) {
			StartIdling();
			return;
		}
		
		mCurState = CrowStates.Eating;
		SwitchFacing( false );
		SwitchSprite("Sprite_Eat");
		StartCoroutine( "Eat", stalk );
	}
	
	IEnumerator PlayEatSound() {
		while(true) {
			audio.PlayOneShot(kPeckSounds[Random.Range(0, kPeckSounds.Length)]);
			yield return new WaitForSeconds(Random.Range(0.25f, 1f));
		}
	}
	
	IEnumerator Eat( CornStalk stalk ) {
		float endTime = Time.time + kEatTime;
		
		StartCoroutine("PlayEatSound");
		
		while( mCurState == CrowStates.Eating && Time.time <= endTime && stalk.mCorn > 0f ) {
			mHunger = Mathf.Clamp( mHunger - kHungerRate * Time.deltaTime, 0f, 1f );
			stalk.Deplete();			
			yield return 0; // wait 1 frame
		}
		
		StopCoroutine("PlayEatSound");
		
		// if the crow ate the full amount of time, fly to the next
		// otherwise, let the current state dictate what happens
		if( mCurState == CrowStates.Eating ) {
			SwitchSprite("Sprite_Calm");
			
			// fly up
			yield return StartCoroutine( "FlyToPosition", this.gameObject.transform.position + new Vector3(0f, 3f, 0f) );
			
			// hang out for a little bit
			yield return new WaitForSeconds(kTimeAfterEating);
			
			// on to the next corn stalk
			StartFlyingToCornStalk( PickBestCornStalk(stalk)  );
		}
	}
	
	//----------------------------------------------------------------------------------------------------
	// Behavior - FlyingToCorn
	//----------------------------------------------------------------------------------------------------
	void StartFlyingToCornStalk( CornStalk stalk ) {
		mCurState = CrowStates.FlyingToCorn;
		SwitchSprite("Sprite_Calm");
		StartCoroutine( "FlyToCornStalk", stalk );
	}
	
	IEnumerator FlyToCornStalk( CornStalk stalk ) {
		// fly to the corn stalk pos
		yield return StartCoroutine( "FlyToPosition", stalk.transform.position + new Vector3(0f, 0.9f, -0.7f) );
		
		// eat!
		StartEating( stalk );		
	}
	
	CornStalk PickBestCornStalk(CornStalk excludeStalk) {
		CornStalk[] stalks = FindObjectsOfType(typeof(CornStalk)) as CornStalk[];
		CornStalk selectedStalk = excludeStalk;
		float highestCorn = 0;
		foreach( CornStalk stalk in stalks ) {
			if( stalk != excludeStalk && stalk.mCorn > highestCorn ) {
				highestCorn = stalk.mCorn;
				selectedStalk = stalk;
			}
		}
		return selectedStalk;
	}
	
	//----------------------------------------------------------------------------------------------------
	// Behavior - Fleeing
	//----------------------------------------------------------------------------------------------------
	void StartFleeing( bool goLeft ) {
		mCurState = CrowStates.Fleeing;
		SwitchSprite("Sprite_Flee");
		audio.PlayOneShot(kFlapSound);
		StartCoroutine( "Flee", goLeft );
	}
	
	IEnumerator Flee( bool goLeft ) {
		// fly away!
		GameObject[] safePoints = GameObject.FindGameObjectsWithTag( "SafePoints" );
		GameObject safePoint;
		bool validSafePoint = false;
		int bpCounter = 0;
		do {
			bpCounter++;
			safePoint = safePoints[Random.Range(0, safePoints.Length - 1)];
			if( ( goLeft && safePoint.transform.position.z <= mScarecrow.transform.position.z )
				|| ( !goLeft && safePoint.transform.position.z >= mScarecrow.transform.position.z ) ) 
				validSafePoint = true;
		} while( !validSafePoint && bpCounter <= 6 ); 
		yield return StartCoroutine( "FlyToPosition", safePoint.transform.position );
		
		SwitchSprite("Sprite_Calm");
		
		// hang out until the crow isn't super stressed
		// or he's too hungry
		while( mStress > kNormalStressThreshold && mHunger < kHungerReturnThreshold ) {
			yield return 0;	
		}
		
		// fly to a new corn stalk
		StartFlyingToCornStalk( PickBestCornStalk(null)  );
	}
	
	void Scare( float stressAmount ) {
		mStress += stressAmount;
		if( mCurState != CrowStates.Fleeing && mStress > kFleeStressThreshold ) {
			StopAllCoroutines();
			StartFleeing( transform.position.z < mScarecrow.transform.position.z );
		}
	}
	
	void Hit( float stressAmount ) {
		GameObject spriteObj = transform.Find("Sprite_Calm").gameObject;
		Instantiate( featherPrefab, spriteObj.transform.position, spriteObj.transform.rotation );
		audio.PlayOneShot(kHitSound);
		Scare( stressAmount );
	}

	//----------------------------------------------------------------------------------------------------
	// Behavior - Dead
	//----------------------------------------------------------------------------------------------------
	void Die() {
		Game game = FindObjectOfType(typeof(Game)) as Game;
		game.OnBirdDeath();
		
		SwitchSprite("Sprite_Dead");
		
		StopAllCoroutines();
		
		this.gameObject.transform.Translate(1.5f, 0f, 0f);
		this.gameObject.rigidbody.useGravity = true;
		this.gameObject.rigidbody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
		mCurState = CrowStates.Dead;
		print( "Crow has died" );
		
		// record the event
		DayNightCycle dnc = FindObjectOfType(typeof(DayNightCycle)) as DayNightCycle;
		dnc.recordEvent("CrowDied");
		
		// Sadden the robot
		MouseMove robotmover = FindObjectOfType(typeof(MouseMove)) as MouseMove;
		robotmover.forceMultiplier = 1f;
	}
	

}
