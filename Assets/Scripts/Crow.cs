using UnityEngine;
using System.Collections;



public class Crow : MonoBehaviour {
	public float kHungerRate 			= 0.01f;
	public float kStressRate 			= -0.01f;
	public float kEatTime				= 3f;
	public float kTimeAfterEating 		= 3f;
	public float kFleeStressThreshold 	= 0.75f;
	public float kNormalStressThreshold = 0.5f;
	public float kHungerReturnThreshold = 0.8f;
	public float kHitStressPercentage	= 0.25f;
	public float kMaxSpeed				= 0.08f;
	
	public AudioClip 	kCawSound;
	public AudioClip 	kFlapSound;
	public AudioClip[] 	kPeckSounds;
	public AudioClip 	kHitSound;
	
	public GameObject featherPrefab;
	
	public float 	mHunger;
	public float 	mStress;
	
	private StateMachine mStateMachine;
	
	public CornStalk mCurrentCornStalk;
	
	private float mNextCawSoundTime;
	
	Scarecrow mScarecrow;
	
	//----------------------------------------------------------------------------------------------------
	// Use this for initialization
	//----------------------------------------------------------------------------------------------------
	void Start () {
		mHunger = 0.5f;
		mStress = 0f;
		
		// set up the AI
		mStateMachine = new StateMachine();
		mStateMachine.AddState( new CrowState_Idle() );
		mStateMachine.AddState( new CrowState_Dead() );
		mStateMachine.AddState( new CrowState_Eating() );
		mStateMachine.AddState( new CrowState_Fleeing() );
		mStateMachine.AddState( new CrowState_FlyingToCorn() );
		mStateMachine.AddState( new CrowState_WaitingForStress() );
		
		// find the scarecrow
		mScarecrow = FindObjectOfType(typeof(Scarecrow)) as Scarecrow;
		
		// start moving
		FlyToCorn();
		
		// pick the next time for the caw sound to play
		mNextCawSoundTime = Time.time + Random.Range( 0f, 2f );
	}
	
	//----------------------------------------------------------------------------------------------------
	// Update is called once per frame
	//----------------------------------------------------------------------------------------------------
	void Update () {
		int currentStateID = GetCurrentStateID();
		
		// if the bird is too hungry or stressed, he be dead
		if( currentStateID != CrowStates.Dead && ( mHunger >= 1f || mStress >= 1f ) ) {
			Die();
		}
		
		// increase hunger if not eating or dead
		if( currentStateID != CrowStates.Eating && currentStateID != CrowStates.Dead ) {
			mHunger = Mathf.Clamp(mHunger + kHungerRate * Time.deltaTime, 0f, 1f);
		}
		
		// increase stress if near flailing scarecrow
		if( currentStateID != CrowStates.Dead ) {
			float distanceMultiplier = ( 8f - Vector3.Distance( transform.position, mScarecrow.transform.position ) ) / 8f;
			distanceMultiplier = Mathf.Clamp( distanceMultiplier, 0f, 1f );
			float stressAmount = mScarecrow.mMovementIntensity * distanceMultiplier * Time.deltaTime;
			Scare( stressAmount );
			// also, decrease it over time
			mStress = Mathf.Clamp(mStress + kStressRate * Time.deltaTime, 0f, 1f);
		}
		
		// play the cawing sound if flying around
		if( currentStateID == CrowStates.Fleeing 
			|| currentStateID == CrowStates.FlyingToCorn 
			|| currentStateID == CrowStates.Idle 
			|| currentStateID == CrowStates.WaitingForStress ) {
			PlayCawSound();
		}
		
		mStateMachine.Update( Time.deltaTime );
		
	}
	
	public int GetCurrentStateID() {
		return mStateMachine.GetCurrentStateID();	
	}
	
	void PlayCawSound() {
		if( Time.time >= mNextCawSoundTime ) {
			audio.PlayOneShot( kCawSound );
			mNextCawSoundTime = Time.time + Random.Range( 0.25f, 3f );
		}
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
	// Crow display manipulation
	//----------------------------------------------------------------------------------------------------
	void TurnOffSprites() {
		transform.Find("Sprite_Calm").renderer.enabled = false;	
		transform.Find("Sprite_Flee").renderer.enabled = false;	
		transform.Find("Sprite_Dead").renderer.enabled = false;	
		transform.Find("Sprite_Eat").renderer.enabled = false;	
	}
	
	public void SwitchSprite( string name ) {
		TurnOffSprites();
		transform.Find(name).renderer.enabled = true;
	}
	
	public void SwitchFacing( bool bFaceLeft ) {
		Vector3 scale = transform.localScale;
		if( bFaceLeft ) scale.z = -1f;
		else scale.z = 1f;
		transform.localScale = scale;
	}
	
	//----------------------------------------------------------------------------------------------------
	// Behavior - Idle
	//----------------------------------------------------------------------------------------------------
	public void Idle() {
		mStateMachine.ChangeState( CrowStates.Idle );
	}
	
	//----------------------------------------------------------------------------------------------------
	// Behavior - Eating
	//----------------------------------------------------------------------------------------------------
	public void Eat( CornStalk corn ) {
		mCurrentCornStalk = corn;
		mStateMachine.ChangeState( CrowStates.Eating );	
	}
	
	
	//----------------------------------------------------------------------------------------------------
	// Behavior - FlyingToCorn
	//----------------------------------------------------------------------------------------------------
	public void FlyToCorn() {
		mStateMachine.ChangeState( CrowStates.FlyingToCorn );	
	}
		
	//----------------------------------------------------------------------------------------------------
	// Behavior - Fleeing
	//----------------------------------------------------------------------------------------------------
	public void Flee() {
		mStateMachine.ChangeState( CrowStates.Fleeing );	
	}
	
	public void Scare( float stressAmount ) {
		mStress += stressAmount;
		if( GetCurrentStateID() != CrowStates.Fleeing && mStress > kFleeStressThreshold ) {
			Flee();
		}
	}
	
	public void Hit( float stressAmount ) {
		GameObject spriteObj = transform.Find("Sprite_Calm").gameObject;
		Instantiate( featherPrefab, spriteObj.transform.position, spriteObj.transform.rotation );
		audio.PlayOneShot( kHitSound );
		Scare( stressAmount );
	}
	
	//----------------------------------------------------------------------------------------------------
	// Behavior - Wait for stress to die down
	//----------------------------------------------------------------------------------------------------
	public void WaitForStress() {
		mStateMachine.ChangeState( CrowStates.WaitingForStress );	
	}
	
	//----------------------------------------------------------------------------------------------------
	// Behavior - Dead
	//----------------------------------------------------------------------------------------------------
	public void Die() {
		mStateMachine.ChangeState( CrowStates.Dead );	
	}
}

// put this in a struct instead of an enum to make the code read better (grr...)
public struct CrowStates {
	public static int Idle 				= 0;
	public static int Eating 			= 1;
	public static int FlyingToCorn 		= 2;
	public static int Fleeing 			= 3;
	public static int Dead 				= 4;
	public static int WaitingForStress 	= 5;
}

//----------------------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------------------
public class CrowState_Idle : State {
	private Crow mCrow;
	
	public CrowState_Idle() {
		mStateID = CrowStates.Idle;
		mCrow = GameObject.FindObjectOfType( typeof( Crow ) ) as Crow;
	}
	
	public override void Enter ()
	{
		mCrow.SwitchSprite( "Sprite_Idle" );
	}
	
	public override void Update (float deltaTime) {}	
	public override void Exit () {}
}

//----------------------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------------------
public class CrowState_Eating : State {
	private Crow 	mCrow;
	private float 	mEndEatingTime;
	private Vector3 mFlyUpTargetPos;
	private float	mNextEatSoundTime;
	private bool	mbDoneEating;
	
	public CrowState_Eating() {
		mStateID = CrowStates.Eating;	
		mCrow = GameObject.FindObjectOfType( typeof( Crow ) ) as Crow;
	}
	
	public override void Enter ()
	{
		// face right
		mCrow.SwitchFacing( false );
		
		// switch to eating sprite
		mCrow.SwitchSprite( "Sprite_Eat" );
		
		// set the time to stop eating and go to a different stalk
		mEndEatingTime = Time.time + mCrow.kEatTime;
		
		// set the position to fly to before going to the next stalk
		mFlyUpTargetPos = mCrow.transform.position + new Vector3(0f, 3f, 0f);
		
		// always play the eat sound right away
		mNextEatSoundTime = Time.time;
		
		mbDoneEating = false;
	}
	
	public override void Update (float deltaTime)
	{
		// if there's no corn left, idle
		if( mCrow.mCurrentCornStalk.mCorn <= 0f ) {
			mCrow.Idle();
			return;
		}
		
		// eat
		if( Time.time < mEndEatingTime ) {
			// decrease hunger
			mCrow.mHunger = Mathf.Clamp( mCrow.mHunger - mCrow.kHungerRate * deltaTime, 0f, 1f );
			
			// decrease corn
			mCrow.mCurrentCornStalk.Deplete();
			
			// play an eating sound every now and then
			PlayEatSound();
		}
		
		// fly up
		else if( Vector3.Distance( mCrow.transform.position, mFlyUpTargetPos ) > 0.05f ) {
			if( !mbDoneEating ) {
				mCrow.SwitchSprite( "Sprite_Calm" );
				mbDoneEating = true;	
			}
			mCrow.transform.position = Vector3.MoveTowards( mCrow.transform.position, mFlyUpTargetPos, mCrow.kMaxSpeed );
		}		

		// go to the next stalk
		else {
			mCrow.FlyToCorn();	
		}
	}
	
	public override void Exit () {}
	
	private void PlayEatSound() {
		if( Time.time >= mNextEatSoundTime ) {
			mCrow.audio.PlayOneShot( mCrow.kPeckSounds[ Random.Range( 0, mCrow.kPeckSounds.Length - 1 ) ] );
			mNextEatSoundTime = Time.time + Random.Range( 0.25f, 1f );
		}
	}
}

//----------------------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------------------
public class CrowState_FlyingToCorn : State {
	private Crow 		mCrow;
	private CornStalk 	mTargetCorn;
	private Vector3		mTargetCornLandPos;
	
	public CrowState_FlyingToCorn() {
		mStateID = CrowStates.FlyingToCorn;	
		mCrow = GameObject.FindObjectOfType( typeof( Crow ) ) as Crow;
	}
	
	public override void Enter ()
	{
		// switch to the calm flying sprite
		mCrow.SwitchSprite( "Sprite_Calm" );
		
		// pick a corn stalk pos
		mTargetCorn = PickCornStalk();
		
		// set the landing position (offset from corn transform)
		mTargetCornLandPos = mTargetCorn.transform.position + new Vector3(0f, 0.9f, -0.7f);
		
		// face the direction we're going
		mCrow.SwitchFacing( mCrow.transform.position.z > mTargetCorn.transform.position.z );		
	}
	
	public override void Update (float deltaTime)
	{
		// move toward the corn until we're in eating range
		if( Vector3.Distance( mCrow.transform.position, mTargetCornLandPos ) > 0.05f ) {
			mCrow.transform.position = Vector3.MoveTowards( mCrow.transform.position, mTargetCornLandPos, mCrow.kMaxSpeed );
		}		
		else {
			mCrow.Eat( mTargetCorn );
		}

	}
	
	public override void Exit () {}
	
	private CornStalk PickCornStalk() {
		CornStalk[] stalks = GameObject.FindObjectsOfType(typeof(CornStalk)) as CornStalk[];
		CornStalk selectedStalk = null;
		float highestCorn = 0f;
		foreach( CornStalk stalk in stalks ) {
			if( stalk.mCorn >= highestCorn ) {
				highestCorn = stalk.mCorn;
				selectedStalk = stalk;
			}
		}
		return selectedStalk;
	}
}

//----------------------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------------------
public class CrowState_Fleeing : State {
	private Crow 			mCrow;
	private Scarecrow 		mScarecrow;
	private GameObject[] 	mSafePoints;
	private GameObject 		mTargetSafeSpot;
	
	public CrowState_Fleeing() {
		mStateID = CrowStates.Fleeing;	
		mCrow = GameObject.FindObjectOfType( typeof( Crow ) ) as Crow;
		mScarecrow = GameObject.FindObjectOfType( typeof( Scarecrow ) ) as Scarecrow;
		mSafePoints = GameObject.FindGameObjectsWithTag( "SafePoints" );
	}
	
	public override void Enter ()
	{
		// switch to the flee sprite
		mCrow.SwitchSprite("Sprite_Flee");
		
		// play the flapping sound
		mCrow.audio.PlayOneShot( mCrow.kFlapSound );
		
		// pick a safe spot
		mTargetSafeSpot = GetSafePoint();
		
		// face the direction we're going
		mCrow.SwitchFacing( mCrow.transform.position.z > mTargetSafeSpot.transform.position.z );		
	}
	
	public override void Update (float deltaTime)
	{
		if( Vector3.Distance( mCrow.transform.position, mTargetSafeSpot.transform.position ) > 0.05f ) {
			mCrow.transform.position = Vector3.MoveTowards( mCrow.transform.position, mTargetSafeSpot.transform.position, mCrow.kMaxSpeed );
		}		
		else {
			mCrow.WaitForStress();
		}
	}
	
	public override void Exit () {}
	
	private GameObject GetSafePoint() {
		// first, find all the safe points on the same side of the scarecrow as the bird
		ArrayList sameSidePoints = new ArrayList();
		bool crowIsOnLeft = mCrow.transform.position.z < mScarecrow.transform.position.z;
		foreach( GameObject sp in mSafePoints ) {
			if( crowIsOnLeft && sp.transform.position.z <= mScarecrow.transform.position.z
				|| !crowIsOnLeft && sp.transform.position.z >= mScarecrow.transform.position.z ) {
				sameSidePoints.Add( sp );	
			}
		}
		
		// if there are any, pick a random one
		if( sameSidePoints.Count > 0 ) {
			return (GameObject) sameSidePoints[ Random.Range( 0, sameSidePoints.Count - 1 ) ];
		}
		
		// if not, just pick a random safe spot anywhere
		else {
			return mSafePoints[ Random.Range( 0, mSafePoints.Length - 1 ) ];
		}		
	}
}

//----------------------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------------------
public class CrowState_WaitingForStress : State {
	private Crow mCrow;
	
	public CrowState_WaitingForStress() {
		mStateID = CrowStates.WaitingForStress;
		mCrow = GameObject.FindObjectOfType( typeof( Crow ) ) as Crow;
	}
	
	public override void Enter () {
		mCrow.SwitchSprite( "Sprite_Calm" );	
	}
	
	public override void Update (float deltaTime)
	{
		if( mCrow.mStress < mCrow.kNormalStressThreshold || mCrow.mHunger > mCrow.kHungerReturnThreshold ) {
			mCrow.FlyToCorn();	
		}
	}
	
	public override void Exit () {}
}

//----------------------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------------------
public class CrowState_Dead : State {
	private Crow mCrow;
	
	public CrowState_Dead() {
		mStateID = CrowStates.Dead;	
		mCrow = GameObject.FindObjectOfType( typeof( Crow ) ) as Crow;
	}
	
	public override void Enter () {
		// report the death to the game
		Game game = GameObject.FindObjectOfType(typeof(Game)) as Game;
		game.OnBirdDeath();
		
		// record the event
		DayNightCycle dnc = GameObject.FindObjectOfType(typeof(DayNightCycle)) as DayNightCycle;
		dnc.recordEvent("CrowDied");
		
		// Sadden the robot
		MouseMove robotmover = GameObject.FindObjectOfType(typeof(MouseMove)) as MouseMove;
		robotmover.forceMultiplier = 1f;
		
		// switch to the dead sprite
		mCrow.SwitchSprite( "Sprite_Dead" );
		
		// shift the bird forward, and let him drop
		mCrow.gameObject.transform.Translate( 1.5f, 0f, 0f );
		mCrow.gameObject.rigidbody.useGravity = true;
		mCrow.gameObject.rigidbody.constraints = RigidbodyConstraints.FreezeRotation 
													| RigidbodyConstraints.FreezePositionX 
													| RigidbodyConstraints.FreezePositionZ;
	}
	
	public override void Update (float deltaTime) {}	
	public override void Exit () {}
}

