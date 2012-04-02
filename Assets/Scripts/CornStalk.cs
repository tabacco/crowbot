using UnityEngine;
using System.Collections;

public class CornStalk : MonoBehaviour {
	public float mDepletionRate = 0.05f;
	
	public float mCorn;
	
	// Use this for initialization
	void Start () {
		mCorn = 1f;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
		
	public void Deplete() {
		mCorn = Mathf.Clamp( mCorn - mDepletionRate * Time.deltaTime, 0f, 1f );
		//renderer.material.color = Color.Lerp(Color.black, Color.yellow, mCorn);
		SetTextureOffset();
		if( mCorn <= 0f ) {
			Die();
		}
	}
	
	void SetTextureOffset() {
		int frame = (int) ( ( 1f - mCorn ) * 5f );
		Vector2 scale = renderer.material.GetTextureScale("_MainTex");
		Vector2 offset = new Vector2(frame * scale.x, 0f);
		print( "frame: " + frame + " offset: " + offset.x );
		renderer.material.SetTextureOffset( "_MainTex", offset );
	}
	
	void Die() {
		print (this.name + " is dead.");		
	}
}
