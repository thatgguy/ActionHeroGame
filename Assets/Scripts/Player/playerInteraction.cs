using UnityEngine;
using System.Collections;

public class playerInteraction : MonoBehaviour {

	 
	[SerializeField] private Camera mainCam;
	[SerializeField] private float playerReach;
	private Ray camRay;
	private RaycastHit rayInfo;
	private GameObject canvas;

	// Use this for initialization
	void Start () {
		canvas = GameObject.FindGameObjectWithTag ("Canvas");
	}
	
	// Update is called once per frame
	void Update () {
		camRay = mainCam.ScreenPointToRay (new Vector3 (mainCam.pixelWidth / 2, mainCam.pixelHeight / 2));

		//Vector3 forward = transform.TransformDirection (Vector3.forward) * playerReach;
		if (Physics.Raycast (camRay, out rayInfo, playerReach)) { 
			//Debug.Log (rayInfo.transform.tag);
			if (rayInfo.transform.GetComponent<ObjInteraction> () != null) {
				rayInfo.transform.SendMessage ("PlayerLookAt");
			}
			else {
				canvas.GetComponent<CanvasScript> ().intObj.text = "";
			}

		} 
		else {
			canvas.GetComponent<CanvasScript> ().intObj.text = "";
		}
	}
}
