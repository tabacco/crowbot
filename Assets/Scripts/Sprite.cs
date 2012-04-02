using UnityEngine;
using System.Collections;

public class Sprite : MonoBehaviour {
	public int kFrameRate 	= 30;
	public int kGridSizeX 	= 4;
	public int kGridSizeY 	= 4;
	public int kTotalFrames = 16;
	
	float mTimeSinceLastFrame;
	int mCurFrame;
	
	// Use this for initialization
	void Start () {
		mTimeSinceLastFrame = 0f;
		mCurFrame = 0;
		
		Vector2 scale = new Vector2( 1f / kGridSizeX, 1f / kGridSizeY );
		renderer.material.SetTextureScale( "_MainTex", scale );
	}
	
	// Update is called once per frame
	void Update () {
		float secondsPerFrame = 1f / (float) kFrameRate;
		
		if( mTimeSinceLastFrame >= secondsPerFrame ) {
			AdvanceFrame();
			mTimeSinceLastFrame = mTimeSinceLastFrame - secondsPerFrame;
		}
		
		mTimeSinceLastFrame += Time.deltaTime;
	}
	
	void AdvanceFrame() {
		int uIndex = mCurFrame % kGridSizeX;
		int vIndex = mCurFrame / kGridSizeX;
		
		//print( "u: " + uIndex + " v: " + vIndex );
		
		Vector2 scale = renderer.material.GetTextureScale("_MainTex");		
		Vector2 offset = new Vector2( uIndex * scale.x, 1.0f - scale.y - vIndex * scale.y );
		
		renderer.material.SetTextureOffset( "_MainTex", offset );
		
		mCurFrame = ( mCurFrame + 1 ) % kTotalFrames;
	}
}
