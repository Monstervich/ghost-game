using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class HumanController : GeneralPlayerController {

	public float Speed = 4;
	CharacterController m_CharacterController;
	CamRig camRig;
	Vector3 direction = Vector3.zero;
	Transform charCam;

	void Start () {
		Team = myLobbyPlayer.Teams.Survivor;
		Initialise ();
		m_CharacterController = GetComponent<CharacterController> ();
		StartCoroutine (DelayedStart ());
	}

	//TODO, don't try to find CamRig, just spawn it with player. It will keep the scene clean
	IEnumerator DelayedStart () {
		yield return new WaitForSeconds (0.2f);
		gameObject.name = NickName;
		if (isLocalPlayer) {
			camRig = GameObject.Find ("CamRig").GetComponent<CamRig> ();
			camRig.TargetSurvivor (transform);
			charCam = GameObject.Find ("CamRigCamera").transform;
		}
	}

	void Update () {
		if (!isLocalPlayer) {
			return;
		}
		if (Alive && CanControll) {
			MovePlayer (Input.GetAxis ("Horizontal"), Input.GetAxis ("Vertical"), 0);
			if (camRig) {
				transform.Rotate (Vector3.up * camRig.camSettings.mouseXSensitivity * Input.GetAxis ("Mouse X"));
			}
			if (Input.GetKeyDown (KeyCode.E)) {
				CmdInteract (charCam.position, charCam.forward);
			}
		}
#if UNITY_EDITOR
		if (Input.GetKeyDown (KeyCode.K)) {
			Die ();
		}
#endif
	}

	public void MovePlayer (float x = 0f, float z = 0f, float y = 0f) {
		if (m_CharacterController.isGrounded) {
			direction = new Vector3 (x, y, z);
			direction = transform.TransformDirection (direction); //from dot to normal direction with rotation
			direction *= Speed;
		}
		direction.y -= 20 * Time.deltaTime; //Time.deltaTime means difference between frames
		m_CharacterController.Move (direction * Time.deltaTime); //Normal worldspeed
	}

	/// <summary>
	/// Dead on server
	/// This makes player dead on client
	/// </summary>
	[Server]
	public void Die (string Reason = "Dead by Daylight") {
		Debug.Log ("[HumanController] " + NickName + " died because of " + Reason);
		ReasonOfDeath = Reason;
		Alive = false;
		if (camRig) {
			camRig.target = null;
		}
		RpcDie ();
		if (!IsThereAnySurvivors ()) {
			CmdItWasTheLastController ();
		}
	}

	public bool IsThereAnySurvivors () {
		HumanController [] HumanControllers = FindObjectsOfType<HumanController> ();
		foreach (var item in HumanControllers) {
			if (item.Alive) {
				return true;
			}
		}
		return false;
	}
}