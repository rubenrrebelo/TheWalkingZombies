using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Zombie_PlannedAtt: MonoBehaviour {
	
	public float _healthLevel;
	public float _movSpeed;
	public float _visionRange;
	public float _attDamage;
	public float _attRange;

	private const float FULL_HEALTH = 100.0f;
	
	private List<GameObject> _survivorsInSight;
	private List<GameObject> _barriersInSight;
	//private GameObject _closestSurvivor;
	//private float dist2Survivor;
	private bool _isReloading;
	//private bool _isFollowing;
	private bool _dead;
	private const string ATTACKING_B = "attackingBarrier";
	private const string ATTACKING_S = "attackingSurvivor";
	private const string IDLE = "idle";

	private float infoBoxWidth = 150.0f;
	private float infoBoxHeight = 60.0f;
	private Vector3 currentScreenPos;
	private string _state;

	private NavMeshAgent navMeshComp;
	private Vector3 CurrentDestination;
	private float timeWindow;
	private const float PATH_RESET_TIME = 5.0f;
	
	private bool showInfo;
	private float lifebar_x_offset, lifebar_y_offset;
	private Texture2D life_bar_green, life_bar_red;
	private float lifebar_lenght, lifebar_height;
	private Material transparentMaterial;

	void Awake(){
		_survivorsInSight = new List<GameObject>();
		navMeshComp = GetComponent<NavMeshAgent>();
		_barriersInSight = new List<GameObject>();
	}

	void Start () {
		_healthLevel = FULL_HEALTH;
		_movSpeed = 8.0f;
		_visionRange = 20.0f;
		_attDamage = 30.0f;
		_attRange = 2.0f;


		_isReloading = false;
		//_isFollowing = false;
		_dead = false;
		_state = IDLE;

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
		transparentMaterial = (Material)Resources.Load(@"Materials/Transparent",typeof(Material));

		lifebar_lenght = 30.0f;
		lifebar_height = 3.5f;
		lifebar_x_offset = -15.0f;
		lifebar_y_offset = -8.0f;
	}

	//Actuadores-------------------------------------------------------------------
	//Attack-Survivor
	private void attackSurvivor(GameObject nearestSurvivor){
		_state = ATTACKING_S;
		navMeshComp.SetDestination(nearestSurvivor.transform.position);
		
		float _dist2Zombie = Vector3.Distance(nearestSurvivor.transform.position, this.transform.position);
		if(!_isReloading && _dist2Zombie <= _attRange){
			StartCoroutine(attackClosestSurvivor(nearestSurvivor));
		}
	}
	IEnumerator attackClosestSurvivor(GameObject nearestSurvivor){
		_isReloading = true;
		nearestSurvivor.GetComponent<Survivor_PlannedAtt>().loseHealth(_attDamage);
		yield return new WaitForSeconds(1.5F);
		_isReloading = false;
	}
	//Attack-BaseLeader
	private void attackBaseLeader(GameObject nearestBaseleader){
		_state = ATTACKING_S;
		navMeshComp.SetDestination(nearestBaseleader.transform.position);
		
		float _dist2Zombie = Vector3.Distance(nearestBaseleader.transform.position, this.transform.position);
		if(!_isReloading && _dist2Zombie <= _attRange){
			StartCoroutine(attackClosestSurvivorB(nearestBaseleader));
		}
	}
	IEnumerator attackClosestSurvivorB(GameObject nearestBaseleader){
		_isReloading = true;
		nearestBaseleader.GetComponent<BaseLeaderScript>().loseHealth(_attDamage);
		yield return new WaitForSeconds(1.5F);
		_isReloading = false;
	}
	//Attack-Barrier
	private void attackBarrier(GameObject nearestBarrier){
		_state = ATTACKING_B;
		navMeshComp.SetDestination(nearestBarrier.transform.position);
		
		float _dist2Barrier = Vector3.Distance(nearestBarrier.transform.position, this.transform.position);
		if(!_isReloading && _dist2Barrier <= _attRange){
			StartCoroutine(attackClosestBarrier(nearestBarrier));
		}
	}
	IEnumerator attackClosestBarrier(GameObject nearestBarrier){
		_isReloading = true;
		nearestBarrier.GetComponent<BarrierScript>().loseHealth(_attDamage);
		yield return new WaitForSeconds(1.5F);
		_isReloading = false;
	}





	//Random-Move
	private void randomMove(){
		/**/
		if (!_state.Equals(IDLE)) {
			CurrentDestination = this.transform.position;
			_state = IDLE;
		}
		
		if ((CurrentDestination - transform.position).magnitude < 2.0f) {
			
			CurrentDestination = new Vector3 (transform.position.x + Random.Range (- 40.0f, 40.0f)
			                                  ,transform.position.y,
			                                  transform.position.z + Random.Range (- 40.0f, 40.0f));
			navMeshComp.SetDestination(CurrentDestination);
			timeWindow = PATH_RESET_TIME;
		}
		/**/
		/** /
		this.transform.position += new Vector3 (0,0,-0.2f);
		/**/
	}

	//Sensores---------------------------------------------------------------------
	//See-Survivor (posição do survivor mais próx)
	//Survivor-Around
	private bool SurvivorsAround(){
		if (_survivorsInSight.Count > 0)
			return true;
		else
			return false;
	}
	//Barrier-Around
	private bool BarrierAround(){
		if (_barriersInSight.Count > 0) {
			float distance2Barrier = Vector3.Distance(NearestBarrier().transform.position, this.transform.position);
			if(distance2Barrier < 7){
				return true;
			}else
				return false;
		}
		else
			return false;
	}
	//BaseLeader-Around
	private bool BaseLeaderAround(){

		
		foreach(GameObject survivor in _survivorsInSight){
			if(survivor.tag.Equals("BaseLeader"))
				return true;
			
		}
		return false;
	}
	//Nearest survivor
	private GameObject NearestSurvivor(){
		GameObject _closestSurvivor = null;
		
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
		return _closestSurvivor;
	}
	//Nearest Barrier
	private GameObject NearestBarrier(){
		GameObject _closestBarrier = null;
		
		foreach(GameObject barrier in _barriersInSight){
			if(_closestBarrier == null){
				_closestBarrier = barrier;
			}else{
				if (Vector3.Distance(_closestBarrier.transform.position, this.transform.position) >
				    Vector3.Distance(barrier.transform.position, this.transform.position))
				{
					_closestBarrier = barrier;
				}
			}
		}
		return _closestBarrier;
	}
	//Nearest BaseLeader
	private GameObject NearestBaseLeader(){
		GameObject _closestSurvivor = null;
		
		foreach(GameObject survivor in _survivorsInSight){
			if(survivor.tag.Equals("BaseLeader"))
				_closestSurvivor=survivor;
			
		}
		return _closestSurvivor;
	}

	// attack base lider also
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
		if(!_dead){
			currentScreenPos = Camera.main.WorldToScreenPoint(this.transform.position);
			if(showInfo){
				//Zombie's Information Box
				if(this.renderer.isVisible && SurvivorsAround()){
					GUI.Box(new Rect(currentScreenPos.x, Screen.height - currentScreenPos.y, infoBoxWidth, infoBoxHeight),
					        this.name + ": \n" +
					        "Closest Survivor: \n" +
					        NearestSurvivor().name + 
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
	}

	void FixedUpdate () {
		if(!_dead){
			checkImpossiblePathAndReset();
			//updateClosestSurvivor();

			if(BarrierAround())
			{
				attackBarrier(NearestBarrier());
			}
			else if(BaseLeaderAround())
			{
				attackBaseLeader(NearestBaseLeader());
			}
			else if(SurvivorsAround())
			{
				attackSurvivor(NearestSurvivor());
			}
			else
				randomMove();

				//DO NOT DELETE forces collision updates in every frame
				this.transform.root.gameObject.transform.position += new Vector3(0.0f, 0.0f, -0.000001f);	
		}else{
			this.renderer.material = transparentMaterial;
			Destroy(this.GetComponent<NavMeshAgent>());
			this.rigidbody.AddForce(0,2000.0f,0, ForceMode.Force);
		}
	}

	public void loseHealth(float ammount){
		_healthLevel -= ammount;
		if(_healthLevel <= 0 && !_dead){
			Debug.Log(this.name + " died.");
			_dead = true;
			StartCoroutine("destroyAfterDeath");
		}
	}

	private IEnumerator destroyAfterDeath(){
		yield return new WaitForSeconds(1.0F);
		//Debug.Log("Destroyed: "+ this.name);
		Destroy(this.gameObject);
	}

	public void setDisplayInfo(bool param){
		showInfo = param;
	}
}