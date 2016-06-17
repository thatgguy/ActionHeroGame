using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public GameObject player;
	public GameObject lookAtObj;
	public GameObject mainCam;

	public float camXDist = 0.58f;
	public float camYDist = 0.9f;
	public float camZDist = -1.75f;

	private Vector3 lookAtPos;

	//private Vector3 cameraLookPos;

	// Use this for initialization
	void Start () {
		player = GameObject.FindGameObjectWithTag ("Player");


	//	

		//Hide Cursor at start of game
		Cursor.visible = false;
	}

	// Update is called once per frame
	void Update () {

		transform.position = player.transform.position;
		mainCam.transform.localPosition = new Vector3 (transform.position.x + camXDist, transform.position.y + camYDist, transform.position.z + camZDist);

		lookAtPos = new Vector3 (lookAtObj.transform.position.x, lookAtObj.transform.position.y, lookAtObj.transform.position.z);

		//show cursor
		if (Input.GetKeyDown (KeyCode.I)) {
			Cursor.visible = !Cursor.visible;
		}
	}

	void LateUpdate () { 
		mainCam.transform.LookAt (lookAtPos);
	}
}
