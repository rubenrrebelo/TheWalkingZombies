using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZombieScript: MonoBehaviour {
	
	public float _healthLevel;
	public float _movSpeed;
	public float _visionRange;
	public float _attDamage;
	public float _attRange;
	
	private List<GameObject> _survivorsInSight;
	private GameObject _closestSurvivor;
	private float dist2Survivor;
	private bool _isReloading;

	private float infoBoxWidth = 150.0f;
	private float infoBoxHeight = 60.0f;
	private Vector3 currentScreenPos;

	private NavMeshAgent navMeshComp;
	private Vector3 CurrentDestination;

	private bool showInfo;
	
	void Start () {
		
		_healthLevel = 100.0f;
		_movSpeed = 8.0f;
		_visionRange = 20.0f;
		_attDamage = 30.0f;
		_attRange = 2.0f;

		navMeshComp = GetComponent<NavMeshAgent>();
		_survivorsInSight = new List<GameObject>();
		_isReloading = false;

		SphereCollider visionRangeCollider = this.gameObject.GetComponentInChildren<SphereCollider>();
		if(visionRangeCollider != null){
			visionRangeCollider.radius = _visionRange;
		}else{
			Debug.Log("Missing sphere collider");
		}

		navMeshComp.speed = _movSpeed;

		CurrentDestination = this.transform.position;


		showInfo = false;
	}
	
	
	//Actuadores-------------------------------------------------------------------
	//TODO: Attack-Survivor
	
	
	//TODO: Random-Move
	private void randomMove(){

		Debug.Log("random: " + (navMeshComp.destination - transform.position).magnitude);


		/**/
		if ((CurrentDestination - transform.position).magnitude < 2.0f) {
			CurrentDestination = new Vector3 (transform.position.x + Random.Range (- 40.0f, 40.0f)
		                                  ,transform.position.y,
		                                  transform.position.z + Random.Range (- 40.0f, 40.0f));
			navMeshComp.SetDestination(CurrentDestination);
		}
		/**/
		/** /
		this.transform.position += new Vector3 (0,0,-2);
		/**/
	}

	//Sensores---------------------------------------------------------------------
	//TODO: See-Survivor (posição do survivor mais próx)

	private bool showDebug = false;
	
	void OnTriggerEnter (Collider other) {
		
		if (other.tag.Equals("Survivor") && !other.transform.root.Equals(this.transform.root)){
			_survivorsInSight.Add(other.gameObject);
			
			if(showDebug){
				Debug.Log(this.name + "-New Survivor " + other.name);
				Debug.Log("#Survivors in range: " + _survivorsInSight.Count);}
		}

	}
	
	void OnTriggerExit (Collider other){
		if (other.tag.Equals("Survivor") && !other.transform.root.Equals(this.transform.root)){
			_survivorsInSight.Remove(other.gameObject);
			
			if(showDebug){
				Debug.Log("Lost Survivor.. " + other.name);
				Debug.Log( "#Survivors in range: " + _survivorsInSight.Count);
			}
		}
	}

	IEnumerator attackClosestSurvivor(){
		_isReloading = true;
		_closestSurvivor.GetComponent<SurvivorScript>().loseHealth(_attDamage);
		yield return new WaitForSeconds(1.5F);
		_isReloading = false;
	}

	void updateClosestSurvivor(){
		if (_survivorsInSight.Count > 0){
			foreach(GameObject survivor in _survivorsInSight){
				if(_closestSurvivor == null){
					_closestSurvivor = survivor;
				}else{
					if (Vector3.Distance(_closestSurvivor.transform.position, this.transform.position) >
					    Vector3.Distance(survivor.transform.position, this.transform.position))
					{
						_closestSurvivor = survivor;
					}
				}
			}
		}else{
			_closestSurvivor = null;
		}
	}

	void OnGUI(){

		if(showInfo){
			//TODO: Zombie's Information Box
			currentScreenPos = Camera.main.WorldToScreenPoint(this.transform.position);
			if(this.renderer.isVisible && _closestSurvivor != null){
				GUI.Box(new Rect(currentScreenPos.x, Screen.height - currentScreenPos.y, infoBoxWidth, infoBoxHeight),
				        this.name + ": \n" +
				        "Closest Survivor: \n" +
				        _closestSurvivor.name + 
				        " \n");
			}
		}
	}

	void Update () {
		updateClosestSurvivor();

		if(_closestSurvivor != null){ //has a survivor in his vision range

			navMeshComp.SetDestination(_closestSurvivor.transform.position); //move towards survivor
			Debug.Log("following: " + (navMeshComp.destination - transform.position).magnitude);

			//attack him, if in range
			dist2Survivor = Vector3.Distance(_closestSurvivor.transform.position, this.transform.position);	
			if(!_isReloading && dist2Survivor <= _attRange){
				StartCoroutine("attackClosestSurvivor");
			}
		}else{
			randomMove();
		}



		//DO NOT DELETE This forces collision updates in every frame
		this.transform.root.gameObject.transform.position += new Vector3(0.0f, 0.0f, -0.000001f);
	}

	public void setDisplayInfo(bool param){
		showInfo = param;
	}
}