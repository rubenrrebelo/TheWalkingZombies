using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZombieScript: MonoBehaviour {
	
	public float _healthLevel;
	public float _movSpeed;
	public float _visionRange;
	public float _attDamage;
	public float _attRange;

	private const float FULL_HEALTH = 100.0f;
	
	private List<GameObject> _survivorsInSight;
	private List<GameObject> _barriersInSight;
	private GameObject _closestSurvivor;
	private float dist2Survivor;
	private bool _isReloading;
	private bool _isFollowing;

	private float infoBoxWidth = 150.0f;
	private float infoBoxHeight = 60.0f;
	private Vector3 currentScreenPos;

	private NavMeshAgent navMeshComp;
	private Vector3 CurrentDestination;
	private float timeWindow;
	private const float PATH_RESET_TIME = 5.0f;
	
	private bool showInfo;
	private float lifebar_x_offset, lifebar_y_offset;
	private Texture2D life_bar_green, life_bar_red;
	private float lifebar_lenght, lifebar_height;
	
	void Start () {
		_healthLevel = FULL_HEALTH;
		_movSpeed = 8.0f;
		_visionRange = 20.0f;
		_attDamage = 30.0f;
		_attRange = 2.0f;

		navMeshComp = GetComponent<NavMeshAgent>();
		_survivorsInSight = new List<GameObject>();
		_barriersInSight = new List<GameObject>();
		_isReloading = false;
		_isFollowing = false;

		SphereCollider visionRangeCollider = this.gameObject.GetComponentInChildren<SphereCollider>();
		if(visionRangeCollider != null){
			visionRangeCollider.radius = _visionRange;
		}else{
			Debug.Log("Missing sphere collider");
		}

		navMeshComp.speed = _movSpeed;

		CurrentDestination = this.transform.position;
		timeWindow = PATH_RESET_TIME;

		showInfo = false;

		life_bar_green = (Texture2D)Resources.Load(@"Textures/life_bar_green", typeof(Texture2D));
		life_bar_red = (Texture2D)Resources.Load(@"Textures/life_bar_red", typeof(Texture2D));
		
		lifebar_lenght = 30.0f;
		lifebar_height = 4.0f;
		lifebar_x_offset = -15.0f;
		lifebar_y_offset = -8.0f;
	}

	//Actuadores-------------------------------------------------------------------
	//TODO: Attack-Survivor
	
	
	//Random-Move
	private void randomMove(){
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
	//See-Survivor (posição do survivor mais próx)


	//TODO: attack base lider also
	void OnTriggerEnter (Collider other) {
		if (other.tag.Equals("Survivor") || other.tag.Equals("BaseLeader") && !other.transform.root.Equals(this.transform.root)){
			_survivorsInSight.Add(other.gameObject);
		}
		if (other.tag.Equals("Barrier")&& !other.transform.root.Equals(this.transform.root)){
			_barriersInSight.Add(other.gameObject);
		}
	}
	
	void OnTriggerExit (Collider other){
		if (other.tag.Equals("Survivor") || other.tag.Equals("BaseLeader") && !other.transform.root.Equals(this.transform.root)){
			_survivorsInSight.Remove(other.gameObject);
		}
		if (other.tag.Equals("Barrier") && !other.transform.root.Equals(this.transform.root)){
			_barriersInSight.Remove(other.gameObject);
		}
	}

	IEnumerator attackClosestSurvivor(){
		_isReloading = true;
		if(_closestSurvivor.tag.Equals("Survivor")){
			_closestSurvivor.GetComponent<SurvivorScript>().loseHealth(_attDamage);
		}else{
			_closestSurvivor.GetComponent<BaseLeaderScript>().loseHealth(_attDamage);
		}
		yield return new WaitForSeconds(1.5F);
		_isReloading = false;
	}

	IEnumerator attackBarrier(){
		_isReloading = true;
		foreach(GameObject barrier in _barriersInSight){
			barrier.GetComponent<BarrierScript>().loseHealth(_attDamage);
			break;
		} 
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

	private void checkImpossiblePathAndReset(){ //Calculates a new setDestination in case the previous calc isnt reached in a set reset time
		timeWindow -= Time.deltaTime;
		if(timeWindow < 0){
			//Debug.Log("Reset needed by :" + this.name);
			CurrentDestination = new Vector3 (transform.position.x + Random.Range (- 40.0f, 40.0f)
			                                  ,transform.position.y,
			                                  transform.position.z + Random.Range (- 40.0f, 40.0f));
			navMeshComp.SetDestination(CurrentDestination);
			timeWindow = PATH_RESET_TIME;
		}
	}

	void OnGUI(){
		currentScreenPos = Camera.main.WorldToScreenPoint(this.transform.position);
		if(showInfo){
			//Zombie's Information Box
			if(this.renderer.isVisible && _closestSurvivor != null){
				GUI.Box(new Rect(currentScreenPos.x, Screen.height - currentScreenPos.y, infoBoxWidth, infoBoxHeight),
				        this.name + ": \n" +
				        "Closest Survivor: \n" +
				        _closestSurvivor.name + 
				        " \n");
			}else{
				GUI.Box(new Rect(currentScreenPos.x, Screen.height - currentScreenPos.y, infoBoxWidth, infoBoxHeight),
				        this.name + ": \n" +
				        "Barriers: " + _barriersInSight.Count + " \n"
				        );
			}
		}
		if(this.renderer.isVisible){
			//Important, order matters!
			GUI.DrawTexture(new Rect(currentScreenPos.x + lifebar_x_offset, Screen.height - currentScreenPos.y + lifebar_y_offset, 
			                         lifebar_lenght, 
			                         lifebar_height), life_bar_red);
			GUI.DrawTexture(new Rect(currentScreenPos.x + lifebar_x_offset, Screen.height - currentScreenPos.y + lifebar_y_offset,
			                         (FULL_HEALTH - (FULL_HEALTH - _healthLevel))*lifebar_lenght/FULL_HEALTH, 
			                         lifebar_height), life_bar_green);
		}
	}

	void Update () {
		checkImpossiblePathAndReset();
		updateClosestSurvivor();

		if(_closestSurvivor != null){ //has a survivor in his vision range

			navMeshComp.SetDestination(_closestSurvivor.transform.position); //move towards survivor

			//attack him, if in range
			dist2Survivor = Vector3.Distance(_closestSurvivor.transform.position, this.transform.position);	
			if(!_isReloading && dist2Survivor <= _attRange){
				StartCoroutine("attackClosestSurvivor");
			}
			_isFollowing = true;
		}else if(_barriersInSight.Count != 0){
			foreach(GameObject barrier in _barriersInSight){ //get any of the barriers
				navMeshComp.SetDestination(barrier.transform.position); //move towards survivor
				//attack him, it in range
				dist2Survivor = Vector3.Distance(barrier.transform.position, this.transform.position);	
				if(!_isReloading && dist2Survivor <= _attRange){
					StartCoroutine("attackBarrier");
				}
				break;
			}
			_isFollowing = true;
		}else if(_isFollowing == true){
				CurrentDestination = this.transform.position; // resets his destination, because of the bug that made him stand still
				randomMove();
				_isFollowing = false;
		}else{
			randomMove();
		}
		//DO NOT DELETE forces collision updates in every frame
		this.transform.root.gameObject.transform.position += new Vector3(0.0f, 0.0f, -0.000001f);	
	}

	public void setDisplayInfo(bool param){
		showInfo = param;
	}
}