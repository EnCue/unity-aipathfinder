using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class npcMovement : MonoBehaviour {
	//Set to 2
	public float chaseSpeed;
	public float idleDistance;

	private bool withinRange;


	private bool avoid = false;
	private List<Vector2> avoidanceRoutine = new List<Vector2> ();
	private Vector3 d = new Vector3();

	private Vector2 P1;
	private float deltaP;

	public GameObject Target;
	public bool isTracking;

	private Rigidbody2D rbody;
	//Assignment of this instance of the script's rigidbody
	void Awake()
	{
		rbody = GetComponent<Rigidbody2D> ();

	}

	void OnTriggerStay2D(Collider2D other){
		if (other.gameObject.tag == "User") {

			float Distance = Vector2.Distance (Target.transform.position, rbody.position);
			if (Distance <= idleDistance) {
				if (!withinRange) {
					withinRange = true;
				}
				if (avoid) {
					avoid = false;
					avoidanceRoutine = new List<Vector2> ();
				}
			} else {
				if (withinRange) {
					withinRange = false;
				}
			}
		}
		if (!avoid) {
			if (other.CompareTag ("Scenery")) {
				
				Vector3 curPos = transform.position;
				Vector3 tPos = Target.transform.position;
				Vector3 fullMove = tPos - curPos;

				Bounds colBounds = other.bounds;
				Vector3 colCenter = colBounds.center; Vector3 colExtents = colBounds.extents;

				float colBound_xpos = colCenter.x + colExtents.x; float colBound_xneg = colCenter.x - colExtents.x;
				float colBound_ypos = colCenter.y + colExtents.y; float colBound_yneg = colCenter.y - colExtents.y;

				Dictionary<string, Vector2> rDict = new Dictionary<string, Vector2> ();
				rDict.Add ("curPos", new Vector2 (curPos.x, curPos.y)); 
				rDict.Add ("t", new Vector2 (tPos.x, tPos.y));

				float[] adjBounds = new float[2];
				if (curPos.x <= colBound_xpos && curPos.x >= colBound_xneg) 
				{
					//Blocked in y-direction
					rDict.Add ("Prim_adj", Vector2.right); rDict.Add ("Scnd_adj", Vector2.up);

					adjBounds = new float[]{ colBound_xpos, colBound_xneg };
				} 
				else if (curPos.y <= colBound_ypos && curPos.y >= colBound_yneg) 
				{
					//Blocked in x-direction
					rDict.Add ("Prim_adj", Vector2.up); rDict.Add ("Scnd_adj", Vector2.right);

					adjBounds = new float[]{ colBound_ypos, colBound_yneg};
				} 
				else 
				{
					//Approaching from corner

					float xThreshold = 0f;
					float yThreshold = 0f;

					Vector3 delta = colCenter - curPos;
					float xDelta = tPos.x - curPos.x; float yDelta = tPos.y - curPos.y;

					if (delta.x > 0f) {
						xThreshold = colBound_xpos;
					} else {
						xThreshold = colBound_xneg;
					}
					float xScale = xThreshold - curPos.x;

					if (delta.y > 0f) {
						yThreshold = colBound_ypos;
					} else {
						yThreshold = colBound_yneg;
					}
					float yScale = yThreshold - curPos.y;

					if (Mathf.Abs (xDelta) >= Mathf.Abs (xScale))
					{
						//Shifting along x-axis
						rDict.Add ("Prim_adj", Vector2.right); rDict.Add ("Scnd_adj", Vector2.up);

						adjBounds = new float[]{ colBound_xpos, colBound_xneg};
					} 
					else if (Mathf.Abs (yDelta) >= Mathf.Abs (yScale)) 
					{
						//Shifting along y-axis
						rDict.Add ("Prim_adj", Vector2.up); rDict.Add ("Scnd_adj", Vector2.right);

						adjBounds = new float[]{ colBound_ypos, colBound_yneg};
					} 
					else { /*Exception*/ }
				}

				try {
					Vector2 blkProj = Vector2.Scale (fullMove, rDict ["Scnd_adj"]);
					Vector2 objDir = colCenter - curPos;
					Vector2 objProj = Vector2.Scale (objDir, rDict ["Scnd_adj"]);

					if (Vector2.Dot (blkProj.normalized, objProj.normalized) > 0f) {
						
						rDict.Add("posBound", (adjBounds[0] * rDict["Prim_adj"]));
						rDict.Add("negBound", (adjBounds[1] * rDict["Prim_adj"]));

						avoidanceRoutine = CreateRoutine (rDict);
						
						avoid = true;
					}
				} catch{/*Exception*/}

			}

		}

	}

	private List<Vector2> CreateRoutine(Dictionary<string, Vector2> Vectors){
		List<Vector2> newRoutine = new List<Vector2> ();
		
		Vector2 PrimProj = Vector2.Scale (Vectors ["Prim_adj"], Vectors ["curPos"]);
		Vector2 ScndProj = Vector2.Scale (Vectors ["Scnd_adj"], Vectors ["curPos"]);

		Vector2 posDelta = (Vectors["posBound"] - PrimProj);
		Vector2 negDelta = (Vectors["negBound"] - PrimProj);

		Vector2 mpi_proj = new Vector2 ();

		Vector2 mpii_proj = Vector2.Scale (Vectors ["Scnd_adj"], Vectors ["t"]);

		if (posDelta.magnitude <= negDelta.magnitude) {
			mpi_proj = Vectors["posBound"];

		} else {
			mpi_proj = Vectors["negBound"];

		}

		Vector2 movePos_i = mpi_proj + ScndProj;
		newRoutine.Add (movePos_i);

		Vector2 movePos_ii = mpii_proj + mpi_proj;
		newRoutine.Add (movePos_ii);


		return newRoutine;
	}

	public void targetTracker(GameObject opponent){
		if (opponent != null) {
			Target = opponent;
			isTracking = true;
			Debug.Log ("Now tracking: " + Target.gameObject.tag);
		} else {
			isTracking = false;
			Target = new GameObject ();
		}

	}

	void Update(){
			
		if (isTracking) {

			Vector3 newMovement = new Vector3 ();
			if (!avoid || avoidanceRoutine.Count == 0) {
				
				newMovement = (Target.transform.position - transform.position);
			
			} else {
				
				Vector2 holder = avoidanceRoutine [0];
				Vector3 newMovePos = new Vector3 (holder.x, holder.y, 0);
				newMovement = newMovePos - transform.position;
			
				if (newMovement.magnitude <= 0.1f) {
					avoidanceRoutine.RemoveAt (0);
				
					if (avoidanceRoutine.Count == 0) {
						avoid = false;
					
					} else {
						StartCoroutine (testCast (avoidanceRoutine [0]));
					
					}
				}
			
			}


			d = newMovement.normalized * (chaseSpeed);
			Vector2 dD = d * Time.deltaTime;
		
			rbody.MovePosition (rbody.position + dD);

			float dist = getDistTo (Target);
			if (dist <= idleDistance) { isTracking = false; }

		} else {
			float dist = getDistTo (Target);

			if (dist > idleDistance) { isTracking = true; }

		}

	}

	private float getDistTo(GameObject t){
		Vector2 displacement = t.transform.position - transform.position;
		float dist = displacement.magnitude;

		return dist;
	}

	private IEnumerator testCast(Vector2 ray){
		Vector3 samplePos = new Vector3 (ray.x, ray.y, 0);
		Vector3 sampleMove = samplePos - transform.position;

		RaycastHit2D sampleCast = Physics2D.CircleCast (transform.position, 0.44f, sampleMove, sampleMove.magnitude);

		if (sampleCast.collider != null && sampleCast.collider.CompareTag("PlayerCollider")) {
			avoidanceRoutine = new List<Vector2> ();
			avoid = false;
		}

		yield break;
	}



	void OnTriggerExit2D(Collider2D other){
		if (other.CompareTag("User")) {

			withinRange = false;
		}
	}

}
