using UnityEngine;
using System.Collections;

public class LookAtObjScript : MonoBehaviour {

	public GameObject camObj;
	public GameObject mainCamObj;
	public GameObject player;
	public float moveSenseX = 1f;
	public float moveSenseY = .5f;
	public float reach;

	private Vector3 objPos;
	private int posTop = 6;
	private float posBottom = -2.25f;

	// Use this for initialization
	void Start () {
		player = GameObject.FindGameObjectWithTag ("Player");

		reach = 1f;

		transform.localPosition = new Vector3 (camObj.transform.position.x, camObj.transform.position.y + 0.5f, camObj.transform.position.z + reach);
		objPos = transform.position;
	}

	// Update is called once per frame
	void Update () {

		//moves object
		if (Input.GetAxis ("Mouse Y") != 0) {
			objPos = new Vector3 (transform.position.x, transform.position.y + moveSenseY * (Input.GetAxis ("Mouse Y")), transform.position.z);
			transform.position = objPos;
		}
		/*
		if (Input.GetAxis ("Mouse X") != 0) {
			objPos = new Vector3 (transform.position.x + moveSenseY * (Input.GetAxis ("Mouse X")), transform.position.y, transform.position.z);
			transform.position = objPos;
		}
*/
		//top border
		if (transform.position.y > posTop) {
			transform.position = new Vector3 (transform.position.x, posTop, transform.position.z);
		}
		//bottom border
		if (transform.position.y < posBottom) {
			transform.position = new Vector3 (transform.position.x, posBottom, transform.position.z);
		}
	}
}
