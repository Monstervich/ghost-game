using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GhostController : GeneralPlayerController {

	public float speed = 5f;
	public float MaxDistanceFromControlled = 5f;
	[SyncVar]
	public float DeathTimer = 5;
	[SyncVar]
	public bool Possesing;
	[SyncVar]
	public bool isUnderExorcism;
	public GameObject QuestWindow;
	public AI ControlledAI;
	public GameObject camRig;

	GameObject GhostRadius;
	float TargetTilt;
	float Tilt = 0;
	float TiltVel;

	#region Events
	public delegate void ReleaseAiDelegate ();

	[SyncEvent]
	public event ReleaseAiDelegate EventReleaseAI;

	#endregion

	//start raycasting from the camera if it is a quest item or not, if it is then get components in children from window (return array of text)
	void Start () {
		Team = myLobbyPlayer.Teams.Ghost;
		Initialise ();
		StartCoroutine (DelayedStart ());
		GhostRadius = GameObject.Find ("GhostRadius");
		//TODO, veca versa for Escape
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	IEnumerator DelayedStart () {
		yield return new WaitForSeconds (0.2f);
		if (isLocalPlayer) {
			camRig = FindObjectOfType<CamRig> ().gameObject;
			camRig.SetActive (false);
		} else {
			Destroy (GhostRadius);
		}
	}

	void LateUpdate () {
		if (!isLocalPlayer) {
			return;
		}
		if (Alive && CanControll) {
			//TODO, clean it, i.e. move to Move method
			transform.Translate (
				speed * Time.deltaTime * Input.GetAxis ("Horizontal"),
				0,
				speed * Time.deltaTime * Input.GetAxis ("Vertical"));
			TargetTilt = -7.5f * Input.GetAxis ("Horizontal");
			Tilt = Mathf.SmoothDamp (Tilt, TargetTilt, ref TiltVel, 0.25f);
			transform.eulerAngles = new Vector3 (transform.eulerAngles.x, transform.eulerAngles.y, Tilt);
			if (!Possesing) {
				if (Input.GetKeyDown (KeyCode.E)) {
					CmdInteract (transform.position, transform.forward);
					GetComponentInChildren<AudioListener> ().enabled = !Possesing;
					camRig.SetActive (Possesing);
				}
			} else {
				if (Input.GetMouseButtonDown (0)) {
					CmdInteract (transform.position, transform.forward);
				}
				DeathTimer -= Time.deltaTime;
				if (DeathTimer < 0) {
					DeathTimer = 0;
					isUnderExorcism = false;
					Die ();
				}
				//TODO, clean it, i.e. move to Limit method
				float distance = Vector3.Distance (transform.position, ControlledAI.transform.position);
				if (distance > MaxDistanceFromControlled) {
					Vector3 fromOriginToObject = transform.position - ControlledAI.transform.position;
					fromOriginToObject *= MaxDistanceFromControlled / distance;
					transform.position = ControlledAI.transform.position + fromOriginToObject;
				}
				GhostRadius.transform.position = ControlledAI.transform.position;
			}
			#region Deprecated
			/*
			if (Input.GetKeyDown (KeyCode.Q)) {
				RaycastHit hit;
				if (Physics.Raycast (transform.position, transform.forward, out hit)) {//Just hover mouse on the method and you get it
					if (hit.collider.GetComponent<Quest> () || hit.collider.GetComponent<AI> ()) {
						hit.collider.SendMessage ("SwitchDebugMode", SendMessageOptions.DontRequireReceiver);
					}
				}
				// Analysis disable once CompareOfFloatsByEqualityOperator
				if (Time.timeScale == 1) {
					Time.timeScale = 0;
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
					// Analysis disable once CompareOfFloatsByEqualityOperator
				} else if (Time.timeScale == 0) {
					Time.timeScale = 1;
					Cursor.lockState = CursorLockMode.Locked;
					Cursor.visible = false;
				}
			}*/
			#endregion
		}
	}

	[ClientRpc]
	public void RpcStartExorcism () {
		isUnderExorcism = true;
	}

	void OnReleaseAI () {
		ControlledAI.StartInitialActivity ();
		ControlledAI = null;
	}

	[Server]
	public void Die () {
		ReasonOfDeath = "Eliminated";
		Destroy (GetComponent<SimpleSmoothMouseLook> ());
		RpcDie ();
		CmdItWasTheLastController ();
	}
}