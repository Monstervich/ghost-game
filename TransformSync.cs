using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TransformSync : NetworkTransform {

	public Vector3 RealPos;
	public Quaternion RealRot;
	public float SpeedOfInterpolationOfPos = 10;
	public float SpeedOfInterpolationOfRot = 5;

	public override void OnDeserialize (NetworkReader reader, bool initialState) {
		//base.OnDeserialize (reader, initialState);
		if (!isLocalPlayer) {
			Transform incomingTransform = reader.ReadTransform ();
			RealPos = incomingTransform.position;
			RealRot = incomingTransform.rotation;
		}
	}

	public override bool OnSerialize (NetworkWriter writer, bool initialState) {
		//return base.OnSerialize (writer, initialState);
		if (isLocalPlayer) {
			writer.Write (transform);
			return true;
		} else {
			return false;
		}
	}

	void LateUpdate () {
		if (!isLocalPlayer) {
			transform.position = Vector3.Lerp (transform.position,RealPos,Time.deltaTime*SpeedOfInterpolationOfPos);
			transform.rotation = Quaternion.Lerp (transform.rotation, RealRot, Time.deltaTime * SpeedOfInterpolationOfRot);
		}
	}
}
