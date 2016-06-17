using UnityEngine;
using System.Collections;

public class ObjInteraction : MonoBehaviour {

	public string objName;
	GameObject canvas;

	// Use this for initialization
	void Start () {
		canvas = GameObject.FindGameObjectWithTag ("Canvas");
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void PlayerLookAt() {
		//Debug.Log ("Yes");
		canvas.GetComponent<CanvasScript>().intObj.text = objName;
	}
}
