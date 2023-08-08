using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footsteps : MonoBehaviour {

	public AudioClip[] sounds;

	void OnTriggerEnter (Collider other) { //Only works for triggers
		Debug.Log("Detected " + other.name);
		if (other.CompareTag ("Character")) {
			other.SendMessage ("SetSounds", this);
		}
	}

	public void PlaySound () { //Only works for charcters
		GetComponent <AudioSource> ().PlayOneShot (sounds[Random.Range(0,sounds.Length)]);
	}

	public void SetSounds (Footsteps newSounds) { //Only works for charcters
		Debug.Log("Updating sounds for " + newSounds.name);
		sounds = newSounds.sounds;
	}
}
