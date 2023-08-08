using UnityEngine;
using UnityEngine.Networking;

[ExecuteInEditMode]
public class CamRig : MonoBehaviour {

	public Transform target;
	public bool autoTargetSurvivor;
	public LayerMask wallLayers;

	public enum Shoulder {
		Right,
		Left
	}

	public Shoulder shoulder;

	[System.Serializable]
	public class CamSettings {
		[Header ("Positioning")]
		public Vector3 camPosOffsetR;
		//Right is called first
		public Vector3 camPosOffsetL;
		//Will create an Editor for this Script, so pos is easily positioned with a buttonpress.

		[Header ("Camera Options")]
		public float mouseXSensitivity = 5.0f;
		public float mouseYSensitivity = 5.0f;
		public float minAngle = -30.0f;
		public float maxAngle = 70.0f;
		public float rotationSpeed = 5.0f;
		public float maxCheckDist = 0.1f;

		[Header ("Zoom")]
		public float defaultFOV = 70.0f;
		//Field of View
		public float zoomFOV = 30.0f;
		public float zoomSpeed = 3.0f;

		[Header ("Visual Options")]
		public float hideMeshWhenDistance = 0.5f;
		//If too close to the camera, it will hide the (survivor) playerMesh from the camera.
	}

	[SerializeField]
	public CamSettings camSettings;

	//	[System.Serializable]
	//	public class InputSettings { //Controller Settings.
	//		public string verticalAxis = "Mouse X";
	//		public string horizontalAxis = "Mouse Y";
	//		public string aimButton = "Fire2";
	//		public string switchShoudlerButton = "Fire4";
	//	}
	//	[SerializeField]
	//	public InputSettings input;

	[System.Serializable]
	public class MovementSettings {
		public float movementLerpSpeed = 5.0f;
		//Linear Interpolation
	}

	[SerializeField]
	public MovementSettings movement;

	Transform pivot;
	public Camera mainCam;
	float newX;
	//X and Y are the values pass when we move the mouse.
	float newY;

	// Use this for initialization
	void Start () {
		pivot = transform.GetChild (0);
	}

	// Update is called once per frame
	void Update () {
		if (target) {
			if (Application.isPlaying) {
				if (Input.GetAxis ("Horizontal") < 0) {
					shoulder = Shoulder.Left;
				} else if (Input.GetAxis ("Horizontal") > 0) {
					shoulder = Shoulder.Right;
				}
				RotateCam ();
				CheckWall (); // CheckWall func makes the camera check for the wall behind it, to make sure we are not hitting anything. Moves cam upwards if we hit a wall.
				CheckMeshRenderer ();
				Zoom (Input.GetMouseButton (1));

				//if (input.GetButtonDown (input.switchShoulderButton)) {
				//SwitchShoulders ();
				//}
			}
		}
	}

	void LateUpdate () { //All the functionalities for following the flower - will need to work on this for more suspense!!!
		if (target) {
			Vector3 targetPosition = target.position;
			Quaternion targetRotation = target.rotation;

			FollowTarget (targetPosition, targetRotation);
		}
	}

	public void TargetSurvivor (Transform Survivor) { //Find the survivor gameObject and sets it as target.
		if (Survivor) {
			target = Survivor;
		}
	}

	void FollowTarget (Vector3 targetPosition, Quaternion targetRotation) { //Following the target with Time.deltaTime SMOOTHLY.
		if (!Application.isPlaying) {
			transform.position = targetPosition; //Vector3
			transform.rotation = targetRotation; //Quaternion
		} else {
			Vector3 newPos = Vector3.Lerp (transform.position, targetPosition, Time.deltaTime * movement.movementLerpSpeed);
			transform.position = newPos;
		}
	}

	void RotateCam () {
		newX += camSettings.mouseXSensitivity * Input.GetAxis ("Mouse Y") * -1; //quaternions
		newY += camSettings.mouseYSensitivity * Input.GetAxis ("Mouse X");

		Vector3 eulerAngleAxis = new Vector3 (newX, newY); //it's pronounced Oiler... LUL

		newX = Mathf.Clamp (newX, camSettings.minAngle, camSettings.maxAngle); //Removes the camera bouncing
		newY = Mathf.Repeat (newY, 360); //Clamping the Y Axis of the rotation so that we 
										 //do not rotate in a 360 degree over the survivor,
										 //prevents the cam from being upside down at times.
		Quaternion newRotation = Quaternion.Slerp (
									 pivot.localRotation,
									 Quaternion.Euler (eulerAngleAxis),
									 Time.deltaTime * camSettings.rotationSpeed); //Interpolate in a shprecal form

		pivot.localRotation = newRotation;
	}
	//Rotate the camera with input.

	void CheckWall () { //Checks the wall and moves the camera up if we hit.
		RaycastHit hit;

		Transform mainCamT = mainCam.transform;
		Vector3 mainCamPos = mainCamT.position;
		Vector3 pivotPos = pivot.position;

		Vector3 start = pivotPos;
		Vector3 dir = mainCamPos - pivotPos;

		float dist = Mathf.Abs (shoulder == Shoulder.Left ? camSettings.camPosOffsetL.z : camSettings.camPosOffsetR.z); //Switching shoulder view.
		Debug.DrawLine (start, mainCamPos);
		if (Physics.SphereCast (start, camSettings.maxCheckDist, dir, out hit, dist, wallLayers)) {
			MoveCamUp (hit, pivotPos, dir, mainCamT);
		} else {
			switch (shoulder) {
				case Shoulder.Left:
					PositionCam (camSettings.camPosOffsetL);
					break;
				case Shoulder.Right:
					PositionCam (camSettings.camPosOffsetR);
					break;
			}
		}
	}

	void OnDrawGizmosSelected () {
		// Display the explosion radius when selected

		Gizmos.color = new Color (23f / 255f, 109f / 255f, 140f / 255f, 0.75F);
		Gizmos.DrawWireSphere (pivot.position, camSettings.maxCheckDist);
	}

	void MoveCamUp (RaycastHit hit, Vector3 pivotPos, Vector3 dir, Transform camT) {//This moves the camera forward when we hit a wall or any object.
		float hitDist = hit.distance;
		Vector3 sphereCastCenter = pivotPos + (dir.normalized * hitDist); //Center of the sphere cast, so we can move the camera in the middle, so the camera does not clip through the wall.
		camT.position = sphereCastCenter;
	}

	void PositionCam (Vector3 camPos) {//Positions the camera's local position to a given location in the worldspace.
		if (!mainCam)
			return;

		Transform mainCamT = mainCam.transform;
		Vector3 mainCamPos = mainCamT.localPosition;
		Vector3 newPos = Vector3.Lerp (mainCamPos, camPos, Time.deltaTime * movement.movementLerpSpeed);
		mainCamT.localPosition = newPos;
	}

	void CheckMeshRenderer () { //Hides the meshe target's mesh renderers when too close to target.
		SkinnedMeshRenderer [] meshes = target.GetComponentsInChildren<SkinnedMeshRenderer> (); //Holds all the mesh renderers.
		Transform mainCamT = mainCam.transform;
		Vector3 mainCamPos = mainCamT.position;
		Vector3 targetPos = target.position;
		float dist = Vector3.Distance (mainCamPos, (targetPos + target.up)); //Middle of the target.

		if (meshes.Length > 0) { //If no meshes in target for some reason.
			for (int i = 0; i < meshes.Length; i++) {
				if (dist <= camSettings.hideMeshWhenDistance) {
					meshes [i].enabled = false;
				} else {
					meshes [i].enabled = true; //If the camera is too close, it will make sure the meshes are hidden.
											   //TODO, change the opacity of the material instead or hiding it completely.
				}
			}
		}
	}

	void Zoom (bool isZooming) { //Zooms the camera in-and-out.
		if (!mainCam)
			return;
		if (isZooming) {
			float newFOV = Mathf.Lerp (mainCam.fieldOfView, camSettings.zoomFOV, Time.deltaTime * camSettings.zoomSpeed);
			mainCam.fieldOfView = newFOV;
		} else {
			float originalFOV = Mathf.Lerp (
									mainCam.fieldOfView,
									camSettings.defaultFOV,//change to camSettings.fieldOfView ???
									Time.deltaTime * camSettings.zoomSpeed);
			mainCam.fieldOfView = originalFOV;
		}
	}

	public void SwitchShoulders () { //Switches the camera's shoulder view.
		switch (shoulder) {
			case Shoulder.Left:
				shoulder = Shoulder.Right;
				break;
			case Shoulder.Right:
				shoulder = Shoulder.Left;
				break;
		}
	}
	//TODO, Review if methods are necessary to be kept public.
}