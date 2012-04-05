using UnityEngine;
using System.Collections;

public class MouseMove : MonoBehaviour {

	float mouseX;
	float mouseY;
	float forceX;
	float forceY;
	
	Vector3 point;
	
	public float forceMultiplier;
	
	Scarecrow scarecrow;
	
	
	// Use this for initialization
	void Start () {
		point = transform.TransformPoint(Vector3.up * 3);
		scarecrow = FindObjectOfType(typeof(Scarecrow)) as Scarecrow;
		forceMultiplier = 150f;
		if(SystemInfo.deviceType == DeviceType.Handheld) {
			Input.multiTouchEnabled = false;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
		if(!scarecrow.isAlive) {
			return;
		}
		
		if(SystemInfo.deviceType == DeviceType.Desktop) {
			mouseX = Input.GetAxis("Mouse X");
			mouseY = Input.GetAxis("Mouse Y");
		} else if(SystemInfo.deviceType == DeviceType.Handheld) {
			foreach (Touch touch in Input.touches) {
				if(touch.phase == TouchPhase.Moved) {
					mouseX = touch.deltaPosition.x;
					mouseY = touch.deltaPosition.y;
				}
			}
		}
		
		forceX = mouseX * forceMultiplier;
		forceY = mouseY * forceMultiplier * 1.5f;

		if(mouseX != 0) {
			rigidbody.AddForceAtPosition(forceX*Vector3.forward, point, ForceMode.Acceleration);
		}

		if(mouseY != 0) {
			rigidbody.AddForceAtPosition(forceY*Vector3.left, point, ForceMode.Acceleration);
		}
		
		scarecrow.mMovementIntensity = Mathf.Clamp(rigidbody.GetPointVelocity(point).sqrMagnitude / 100, 0, 1);
		
	}
}
