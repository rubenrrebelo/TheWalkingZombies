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

	private float infoBoxWidth = 150.0f;
	private float infoBoxHeight = 60.0f;
	private Vector3 currentScreenPos;

	private NavMeshAgent navMeshComp;

	private bool showInfo;
	
	void Start () {
		
		_healthLevel = 10.0f;
		_movSpeed = 5.0f;
		_visionRange = 20.0f;
		_attDamage = 5.0f;
		_attRange = 1.0f;

		navMeshComp = GetComponent<NavMeshAgent>();
		_survivorsInSight = new List<GameObject>();

		SphereCollider visionRangeCollider = this.gameObject.GetComponentInChildren<SphereCollider>();
		if(visionRangeCollider != null){
			visionRangeCollider.radius = _visionRange;
		}else{
			Debug.Log("Missing sphere collider");
		}

		navMeshComp.speed = _movSpeed;

		showInfo = false;
	}
	
	
	//Actuadores-------------------------------------------------------------------
	//TODO: Attack-Survivor
	
	
	//TODO: Random-Move
	private void randomMove(){
		//mockup, zombies just walk forward
		//this.transform.root.gameObject.transform.position += new Vector3(0.0f, 0.0f, -0.05f);

		if(_closestSurvivor != null){
			navMeshComp.SetDestination(_closestSurvivor.transform.position);
		}else{
			this.transform.position += new Vector3(0.0f, 0.0f, -0.5f);
		}

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

	void Update () {

		updateClosestSurvivor();
		randomMove();



		//DO NOT DELETE This forces collision updates in every frame
		this.transform.root.gameObject.transform.position += new Vector3(0.0f, 0.0f, -0.000001f);
	}

	public void setDisplayInfo(bool param){
		showInfo = param;
	}
}
