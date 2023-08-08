using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// This is parent controller of all controller in the game.
/// </summary>
public class GeneralPlayerController : NetworkBehaviour {

	[SyncVar]
	public string NickName = "NoNickName";
	[SyncVar]
	public string ReasonOfDeath = "Alive";
	public myLobbyPlayer.Teams Team;
	public bool Alive = true;
	public bool CanControll = true;

	public void Initialise () {
		//Debug.Log ("Cleaning useless components");
		gameObject.name = NickName;
		if (!isLocalPlayer) {
			try {
				Destroy (GetComponentInChildren<AudioListener> ());
				Destroy (GetComponentInChildren<FlareLayer> ());
				Destroy (GetComponentInChildren<Camera> ());
			} catch {
				Debug.Log ("It is clean");
			}
		}
	}

	/// <summary>
	/// Call this to end life of the controller
	/// </summary>
	[ClientRpc]
	public void RpcDie () {
		Alive = false;
		BlockControll ();
	}

	public void BlockControll () {
		CanControll = false;
	}

	[ClientRpc]
	public void RpcBlockControll () {
		CanControll = false;
	}

	/// <summary>
	/// If no controllers are left, this will be called to display game results
	/// </summary>
	[Command]
	public void CmdItWasTheLastController () {
		GeneralPlayerController [] GeneralPlayerControllers = FindObjectsOfType<GeneralPlayerController> ();
		foreach (var item in GeneralPlayerControllers) {
			item.RpcBlockControll ();
		}
		try {
			Prototype.NetworkLobby.LobbyManager.s_Singleton.GameOver ();
		} catch {
			Debug.Log ("[GeneralPlayerController] Can't trigger Game Over. LobbyManager is not created.");
			Debug.Log ("[GeneralPlayerController] Press X to disconnect.");
		}
	}

	[Command]
	protected void CmdInteract (Vector3 origin, Vector3 dir) {
		RaycastHit hit;//reference
		if (Physics.Raycast (origin, dir, out hit, 60f)) {
			if (hit.collider.GetComponent<Interactable> ()) {//hit contains a lot of info about hit object
				hit.collider.GetComponent<Interactable> ().Interact (gameObject.name);
			}
		}
	}

	public string GetReasonOfDeath () {
		return ReasonOfDeath;
	}
}