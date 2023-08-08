using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Prototype.NetworkLobby;

//USELESS, it has a lot of commented debugs. I think you can remove them.
public class myLobbyPlayer : LobbyPlayer {

	public enum Teams {
		None,
		Spectator,
		Survivor,
		Ghost
	}

	[SyncVar]
	public Teams team = Teams.None;

	public override void OnClientEnterLobby () {
		base.OnClientEnterLobby ();
		if (NetworkServer.active) {
			try {
				//Debug.Log (playerName + " is connected, chosing a team.");
				//player.GetComponent<myLobbyPlayer> ().SetTeam (myLobbyPlayer.Teams.Survivor);
				float Chance = Random.Range (0f, 1f);
				//Debug.Log (playerName + "'s chance is " + Mathf.Round (Chance * 100f) / 100f);
				bool MainConditionForGhost = false;
				//MainConditionForGhost = LobbyPlayerList._instance.GetListOfLobbyPlayers ().Count < FindObjectOfType<LobbyManager> ().maxPlayers ? !CheckForGhost () && Chance < 0.5f : true;
				if (LobbyPlayerList._instance.GetListOfLobbyPlayers ().Count < FindObjectOfType<LobbyManager> ().maxPlayers) {
					//Debug.Log("There is still space in the server. Choosing a ghost...");
					MainConditionForGhost = !CheckForGhost () && Chance < 0.5f;
				} else {
					MainConditionForGhost = !CheckForGhost ();
					//Debug.Log("There is not space in the server. Ghost or Survivor?");
				}
				/*Debug.Log (playerName + "'s MainConditionForGhost is " + MainConditionForGhost.ToString () 
					+ "\nMore info about LobbyPlayerList: " + LobbyPlayerList._instance.ToString () 
					+ " out of " + FindObjectOfType<LobbyManager> ().maxPlayers);*/
				if (MainConditionForGhost) { 
					//Debug.Log (playerName + " will be Ghost");
					SetTeam (myLobbyPlayer.Teams.Ghost);
				} else {
					//Debug.Log (playerName + " will be Survivor");
					SetTeam (myLobbyPlayer.Teams.Survivor);
				}
			} catch {
				//Debug.Log ("Failed to get my Lobby Player");
			}
		} else {
			//Debug.Log (playerName + " is connected, but I am not a server.");
		}
	}

	bool CheckForGhost () {
		foreach (var item in LobbyPlayerList._instance.GetListOfLobbyPlayers()) {
			try {
				if (item.GetComponent <myLobbyPlayer> ().GetTeam () == myLobbyPlayer.Teams.Ghost) {
					//Debug.Log ("Ghost is found. It is " + item.playerName);
					return true;
				}
			} catch {
				//Debug.Log ("Failed to get my Lobby Player from item in _players");
				return false;
			}
		}
		//Debug.Log ("Ghost is not found");
		return false;
	}

	//[ClientRpc]
	public void SetTeam (Teams Team) {
		this.team = Team;
		//Debug.Log (playerName + " now is " + team.ToString ());
		LobbyManager.s_Singleton.SetPlayerTypeLobby (GetComponent <NetworkIdentity> ().connectionToClient, this.team);
	}

	public Teams GetTeam () {
		return team;
	}

	[ClientRpc]
	public void RpcUpdateGameOverCountdown (int countdown) {
		LobbyManager.s_Singleton.topPanel.ToggleVisibility (true);
		LobbyManager.s_Singleton.CountdownField.text = countdown != 0 ? "Disconnect in " + countdown : "Match is in the progress.";
	}
}