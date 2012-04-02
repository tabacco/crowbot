using UnityEngine;
using System.Collections;

public class SelfRighting : MonoBehaviour {
	public int force;
	public int offset;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		Vector3 point = transform.TransformPoint(offset*Vector3.up);
		rigidbody.AddForceAtPosition(force*Vector3.up, point);
	}
}
