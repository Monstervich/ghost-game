using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AI : MonoBehaviour {

	//Need to create Modes or States of the AI
	[HideInInspector]
	public NavMeshAgent Agent;
	public float walkSpeed = 0.45f;
	public float runSpeed = 1.5f;
	public Transform Eyes;
	public Interactable InitialInteractable;

	[Header ("For Timing")]
	public float Timer = 2;
	[Tooltip ("Reaction Limit")]
	public float ReactionLimit = 2;
	[Tooltip ("Reaction Time")]
	public float ReactionTimer;

	[Header ("For Distance")]
	public float Distance;

	Animator AC;
	float vel = 7;
	bool isActivityStarted;

	Vector3 defaultChildPos;
	Quaternion defaultChildRot;

	Transform Child;

	AIActivityReaction m_AIActivity;

	void Start () {
		Child = transform.GetChild (0);
		defaultChildRot = Child.localRotation;
		defaultChildPos = Child.localPosition;
		Agent = GetComponent<NavMeshAgent> ();
		AC = GetComponent<Animator> ();
		if (InitialInteractable) {
			InitialInteractable.Interact ("AI");
		}
	}

	void Update () {
		AC.SetFloat ("velocity", Agent.velocity.magnitude);
		DecisionMaking ();
	}

	public void UpdateDestination (Vector3 pos) {
		Agent.SetDestination (pos);
	}

	void DecisionMaking () {
		Agent.speed = walkSpeed;
		if (Agent.remainingDistance <= (Agent.stoppingDistance + 0.25f)) {
			if (isActivityStarted) {
				if (Timer > 0) {
					Timer -= Time.deltaTime;
				}
				if (m_AIActivity.currentInteractable.interactionLocation) {
					Child.position = Vector3.Lerp (Child.position, m_AIActivity.currentInteractable.interactionLocation.position, Time.deltaTime * vel);
					Child.rotation = Quaternion.Lerp (Child.rotation, m_AIActivity.currentInteractable.interactionLocation.rotation, Time.deltaTime * vel);
				}
			} else {
				StartActivity ();
			}
		} else {
			Child.localPosition = Vector3.Lerp (Child.localPosition, defaultChildPos, Time.deltaTime * vel);
			Child.localRotation = Quaternion.Lerp (Child.localRotation, defaultChildRot, Time.deltaTime * vel);
		}
		if (Timer <= 0) {
			RepeatActivity ();
		}
		AC.SetBool ("isBusy", Agent.remainingDistance < (Agent.stoppingDistance + 0.25f));
	}

	#region Activities
	public void StartActivity () {
		Debug.Log ("[AI] Start activity " + m_AIActivity.Name);//Starts activity
		isActivityStarted = true;
		Timer = m_AIActivity.DelayBetweenAttempts;
	}

	public void RepeatActivity () {
		Debug.Log ("[AI] Repeat activity " + m_AIActivity.Name);
		//Restarts activity. This also may finish it
		m_AIActivity.currentInteractable.Interact ("AI");
		Timer = m_AIActivity.DelayBetweenAttempts;
	}

	public void SetActivity (AIActivityReaction activity) {
		if (m_AIActivity != null) {
			Debug.Log ("[AI] End activity " + m_AIActivity.Name);
		}
		Debug.Log ("[AI] Setting new activity " + activity.Name);
		isActivityStarted = false;
		Timer = activity.DelayBetweenAttempts;
		m_AIActivity = activity;
		UpdateDestination (activity.currentInteractable.transform.position);
	}

	public void StartInitialActivity () {
		if (InitialInteractable) {
			InitialInteractable.Interact ("AI");
		}
	}
	#endregion

	public IEnumerator StopAndGo (float TimeToWait, float defaultSpeed = 0) {
		if (defaultSpeed <= 0) {
			defaultSpeed = walkSpeed;
		}
		AC.SetFloat ("velocity", 0);
		Agent.speed = 0;
		yield return new WaitForSeconds (TimeToWait);
		Agent.speed = defaultSpeed;
	}
}