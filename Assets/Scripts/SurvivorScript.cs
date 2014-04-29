using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SurvivorScript: MonoBehaviour {
	
	public float _healthLevel;
	public float _movSpeed;
	public float _visionRange;
	public float _attDamage;
	public float _attRange;
	public float _resourceLevel;
	public Transform _TopLeftBase;
	public Transform _BottomRightBase;
	
	private const float PICKUP_RANGE = 2.0f;
	private const float FULL_HEALTH = 100.0f;
	private const float MAX_RESOURCES = 100.0f;
	private const float CRITICAL_THRESHOLD = 30.0f;
	private const string IDLE = "idle";
	private const string ATTACKING = "attacking";
	private const string COLLECTING = "collecting";
	private const string DEPOSITING = "depositing";
	private const string HEALING = "healing";
	private const string MOVINGTO = "moving";
	private const int EMPTY_LEVEL = 0;
	private const int CRITICAL_LEVEL = 1;
	private const int NORMAL_LEVEL = 2;
	private const int FULL_LEVEL = 3;
	
	private List<GameObject> _zombiesInSight;
	private List<GameObject> _survivorsInSight;
	private List<GameObject> _resourcesInSight;
	private bool depositInRange;
	private Vector3 depositPosition;
	private bool healInRange;
	private Vector3 healPosition;
	private string _state;
	private bool _dead; //to prevent multiple destroyAfterDeath calls
	private bool _isReloading; //checks if the survivor is realoading for his next attack
	
	//private bool _isCollecting;
	
	private GameObject BaseLider;
	
	private float infoBoxWidth = 100.0f;
	private float infoBoxHeight = 90.0f;
	private Vector3 currentScreenPos;
	
	private NavMeshAgent navMeshComp;
	private Vector3 CurrentDestination;
	private float timeWindow;
	private const float PATH_RESET_TIME = 5.0f;
	
	
	private bool showInfo;
	private float lifebar_x_offset, lifebar_y_offset;
	private Texture2D life_bar_green, life_bar_red;
	private float lifebar_lenght, lifebar_height;
	private Material transparentMaterial;
	
	void Start () {
		
		_healthLevel = FULL_HEALTH;
		_movSpeed = 5.0f;
		_visionRange = 20.0f;
		_attDamage = 25.0f;
		_attRange = 5.0f;
		_resourceLevel = 0.0f;
		
		_zombiesInSight = new List<GameObject>();
		_survivorsInSight = new List<GameObject>();
		_resourcesInSight = new List<GameObject>();
		//_isCollecting = false;
		_state = IDLE;

		_dead = false;
		
		navMeshComp = GetComponent<NavMeshAgent>();
		
		BaseLider = GameObject.FindWithTag("BaseLeader");

		SphereCollider visionRangeCollider = this.gameObject.GetComponentInChildren<SphereCollider>();
		if(visionRangeCollider != null){
			visionRangeCollider.radius = _visionRange;
		}else{
			Debug.Log("Missing sphere collider");
		}
		
		showInfo = false;
		
		life_bar_green = (Texture2D)Resources.Load(@"Textures/life_bar_green", typeof(Texture2D));
		life_bar_red = (Texture2D)Resources.Load(@"Textures/life_bar_red", typeof(Texture2D));
		transparentMaterial = (Material)Resources.Load(@"Materials/Transparent",typeof(Material));
		
		lifebar_lenght = 30.0f;
		lifebar_height = 4.0f;
		lifebar_x_offset = -15.0f;
		lifebar_y_offset = -8.0f;
		
		timeWindow = PATH_RESET_TIME;
		CurrentDestination = this.transform.position;
		navMeshComp.speed = _movSpeed;
	}
	
	//Actuadores-------------------------------------------------------------------
	//TODO: Attack-Zombie

	private void attackZombie(GameObject nearestZombie){
		_state = ATTACKING;
		navMeshComp.SetDestination(nearestZombie.transform.position);

		float _dist2Zombie = Vector3.Distance(nearestZombie.transform.position, this.transform.position);
		if(!_isReloading && _dist2Zombie <= _attRange){
			StartCoroutine(attackClosestZombie(nearestZombie));
		}
	}

	IEnumerator attackClosestZombie(GameObject nearestZombie){
		//TODO: finish
		_isReloading = true;
		nearestZombie.GetComponent<ZombieScript>().loseHealth(_attDamage);
		yield return new WaitForSeconds(1.5F);
		_isReloading = false;
	}

	//Collect-Resources
	private void CollectResources(GameObject nearestResource){
		
		float _dist2Resource;
		
		_state = COLLECTING;
		navMeshComp.SetDestination(nearestResource.transform.position);
		_dist2Resource = Vector3.Distance(nearestResource.transform.position, this.transform.position);
		if(_dist2Resource <= PICKUP_RANGE){
			nearestResource.GetComponent<ResourcesScript>().catchResources();
		}
		//---------------------------------------- TODO: FALTA ACTUALIZAR A VARIAVEL RESOURCE DO AGENTE

	}
	//Deposit-Resources
	private void DepositResources(){

			_state = DEPOSITING;
			navMeshComp.SetDestination (depositPosition);
			if ((depositPosition - transform.position).magnitude < 4) {
				BaseLider.GetComponent<BaseLeaderScript>().addResources(_resourceLevel);
				_resourceLevel = 0;
				//navMeshComp.Stop();
			}




	}
	// Heal
	private void Heal(){
		

			_state = HEALING;
			navMeshComp.SetDestination (healPosition);
			if ((healPosition - transform.position).magnitude < 4) {
				_healthLevel = FULL_HEALTH;
				//navMeshComp.Stop();
			}

		
	}
	//Move-to
	private void MoveTo(Vector3 position){
		_state = MOVINGTO;
		navMeshComp.SetDestination (position);
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
	}
	
	//Sensores---------------------------------------------------------------------
	//Level-Resources?
	private int LevelResources() 
	{
		if (_resourceLevel == 0)
			return 0;
		if (_resourceLevel <= CRITICAL_THRESHOLD)
			return 1;
		if (_resourceLevel > CRITICAL_THRESHOLD && _resourceLevel < MAX_RESOURCES)
			return 2;
		if (_resourceLevel == MAX_RESOURCES)
			return 3;
		return 4;
	}
	//Level-Health?
	private int LevelHealth() 
	{
		if (_healthLevel <= CRITICAL_THRESHOLD)
			return 1;
		if (_healthLevel > CRITICAL_THRESHOLD && _healthLevel < FULL_HEALTH)
			return 2;
		if (_healthLevel == FULL_HEALTH)
			return 3;
		return 0;
	}
	//Is-In-Base?
	private bool IsInBase() {
		if (this.transform.position.x > _TopLeftBase.position.x &&
		    this.transform.position.x < _BottomRightBase.position.x &&
		    this.transform.position.z > _TopLeftBase.position.z &&
		    this.transform.position.z < _BottomRightBase.position.z
		    )
			return true;
		else
			return false;
		
		
	}
	//Resources-Around?
	private bool ResourcesAround(){
		if (_resourcesInSight.Count > 0)
			return true;
		else
			return false;
	}
	//Survivors-Around?
	private bool SurvivorsAround(){
		if (_survivorsInSight.Count > 0)
			return true;
		else
			return false;
	}
	//Zombies-Around?
	private bool ZombiesAround(){
		if (_zombiesInSight.Count > 0)
			return true;
		else
			return false;
	}
	//Heal-InRange?
	private bool HealInRange(){
		if (healInRange)
			return true;
		else
			return false;
	}
	//Deposit-InRange?
	private bool DepositInRange(){
		if (depositInRange)
			return true;
		else
			return false;
	}
	//Nearest-Survivor-Position
	private Vector3 NearestSurvivorPosition(){
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
		return _closestSurvivor.transform.position;
	}
	//Nearest-Survivor
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
	//Nearest-Zombie-Position
	private Vector3 NearestZombiePosition(){
		GameObject _closestZombie = null;
		
		foreach(GameObject zombie in _zombiesInSight){
			if(_closestZombie == null){
				_closestZombie = zombie;
			}else{
				if (Vector3.Distance(_closestZombie.transform.position, this.transform.position) >
				    Vector3.Distance(zombie.transform.position, this.transform.position))
				{
					_closestZombie = zombie;
				}
			}
		}
		return _closestZombie.transform.position;
	}
	//Nearest-Zombie
	private GameObject NearestZombie(){
		GameObject _closestZombie = null;
		
		foreach(GameObject zombie in _zombiesInSight){
			if(_closestZombie == null){
				_closestZombie = zombie;
			}else{
				if (Vector3.Distance(_closestZombie.transform.position, this.transform.position) >
				    Vector3.Distance(zombie.transform.position, this.transform.position))
				{
					_closestZombie = zombie;
				}
			}
		}
		return _closestZombie;
	}
	//Nearest-Resource-Position
	private Vector3 NearestResourcePosition(){
		GameObject _closestResource = null;
		
		foreach(GameObject resource in _resourcesInSight){
			if(_closestResource == null){
				_closestResource = resource;
			}else{
				if (Vector3.Distance(_closestResource.transform.position, this.transform.position) >
				    Vector3.Distance(resource.transform.position, this.transform.position))
				{
					_closestResource = resource;
				}
			}
		}
		return _closestResource.transform.position;
	}
	//Nearest-Resource
	private GameObject NearestResource(){
		GameObject _closestResource = null;
		
		foreach(GameObject resource in _resourcesInSight){
			if(_closestResource == null){
				_closestResource = resource;
			}else{
				if (Vector3.Distance(_closestResource.transform.position, this.transform.position) >
				    Vector3.Distance(resource.transform.position, this.transform.position))
				{
					_closestResource = resource;
				}
			}
		}
		return _closestResource;
	}
	//Survivor-Action
	private string SurvivorAction(GameObject survivor){
		return survivor.GetComponent<SurvivorScript> ().getState ();
		
	}
	//Any-Survivor-Need-Help-Collecting
	private Vector3 anySurvivorCollecting(){
		GameObject survivorInNeed = null;
		foreach(GameObject survivor in _survivorsInSight){ //check each survivor around to see if they are collecting
			if( survivor.GetComponent<SurvivorScript>().getState().Equals(COLLECTING)){
				if(survivorInNeed == null){
					survivorInNeed = survivor;
				}else{ //chooses the closest survivor to him that is collecting
					if (Vector3.Distance(survivorInNeed.transform.position, this.transform.position) >
					    Vector3.Distance(survivor.transform.position, this.transform.position))
					{
						survivorInNeed = survivor;
					}
				}
			}
		}
		return survivorInNeed.transform.position;
	}
	
	//Any-Survivor-Need-Help-Attacking
	private Vector3 anySurvivorAttacking(){
		GameObject survivorInNeed = null;
		foreach(GameObject survivor in _survivorsInSight){ //check each survivor around to see if they are attacking
			if( survivor.GetComponent<SurvivorScript>().getState().Equals(ATTACKING)){
				if(survivorInNeed == null){
					survivorInNeed = survivor;
				}else{ //chooses the closest survivor to him that is attacking
					if (Vector3.Distance(survivorInNeed.transform.position, this.transform.position) >
					    Vector3.Distance(survivor.transform.position, this.transform.position))
					{
						survivorInNeed = survivor;
					}
				}
			}
		}
		return survivorInNeed.transform.position;
	}

	private bool isAnySurvivorAttacking(){
		GameObject survivorInNeed = null;
		foreach(GameObject survivor in _survivorsInSight){ //check each survivor around to see if they are attacking
			if( survivor.GetComponent<SurvivorScript>().getState().Equals(ATTACKING)){
				if(survivorInNeed == null){
					survivorInNeed = survivor;
				}else{ //chooses the closest survivor to him that is attacking
					if (Vector3.Distance(survivorInNeed.transform.position, this.transform.position) >
					    Vector3.Distance(survivor.transform.position, this.transform.position))
					{
						survivorInNeed = survivor;
					}
				}
			}
		}
		if(survivorInNeed == null){
			return false;
		}else 
			return true;
	}

	private bool isAnySurvivorCollecting(){
		GameObject survivorInNeed = null;
		foreach(GameObject survivor in _survivorsInSight){ //check each survivor around to see if they are collecting
			if( survivor.GetComponent<SurvivorScript>().getState().Equals(COLLECTING)){
				if(survivorInNeed == null){
					survivorInNeed = survivor;
				}else{ //chooses the closest survivor to him that is collecting
					if (Vector3.Distance(survivorInNeed.transform.position, this.transform.position) >
					    Vector3.Distance(survivor.transform.position, this.transform.position))
					{
						survivorInNeed = survivor;
					}
				}
			}
		}
		if(survivorInNeed == null){
			return false;
		}else 
			return true;
	}

	//TODO: investigar se moving tb torna mais efficiente

	/// ////////
	/// Coliders
	/// ////////
	private bool showDebug = false;
	
	void OnTriggerEnter (Collider other) {
		
		if (other.tag.Equals("Survivor") && !other.transform.root.Equals(this.transform.root)){
			_survivorsInSight.Add(other.gameObject);
			
			if(showDebug){
				Debug.Log(this.name + "-New Survivor " + other.name);
				Debug.Log("#Survivors in range: " + _survivorsInSight.Count);}
		}
		if (other.tag.Equals("Zombie")){
			_zombiesInSight.Add(other.gameObject);
			
			if(showDebug){
				Debug.Log(this.name + "-New Zombie " + other.name);
				Debug.Log("#Zombies in range: " + _zombiesInSight.Count);}
		}
		if (other.tag.Equals("Resources")){
			_resourcesInSight.Add(other.gameObject);
			
			if(showDebug){
				Debug.Log(this.name + "-New Resources " + other.name);
				Debug.Log("#Resources in range: " + _resourcesInSight.Count);}
		}
		if (other.tag.Equals("Heal")){
			healPosition = other.transform.position;
			healInRange = true;
			if(showDebug){
				Debug.Log(this.name + "-New heal " + other.name);
			}
		}
		if (other.tag.Equals("Deposit")){
			depositPosition = other.transform.position;
			depositInRange = true;
			if(showDebug){
				Debug.Log(this.name + "-New deposit " + other.name);
			}
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
		if (other.tag.Equals("Zombie")){
			_zombiesInSight.Remove(other.gameObject);
			
			if(showDebug){
				Debug.Log("Lost Zombie.. " + other.name);
				Debug.Log("#Zombies in range: " + _zombiesInSight.Count);
			}
			
		}
		if (other.tag.Equals("Resources")){
			_resourcesInSight.Remove(other.gameObject);
			
			if(showDebug){
				Debug.Log("Lost Resource " + other.name);
				Debug.Log("#Resources in range: " + _resourcesInSight.Count);
			}
		}
		if (other.tag.Equals("Heal")){
			healInRange = false;
			
			if(showDebug){
				Debug.Log("Lost Heal.. " + other.name);
			}
			
		}
		if (other.tag.Equals("Deposit")){
			depositInRange = false;
			
			if(showDebug){
				Debug.Log("Lost Deposit.. " + other.name);
			}
		}
	}

	public string getState(){
		return _state;
	}
	
	public void setDisplayInfo(bool param){
		showInfo = param;
	}
	
	public void loseHealth(float ammount){
		_healthLevel -= ammount;
		if(_healthLevel <= 0 && !_dead){
			Debug.Log(this.name + " died.");
			//to make it "disappear"
			_dead = true;
			StartCoroutine("destroyAfterDeath");
		}
	}

	private IEnumerator destroyAfterDeath(){
		yield return new WaitForSeconds(1.0F);
		//Debug.Log("Destroyed: "+ this.name);
		Destroy(this.gameObject);
	}


	private void checkImpossiblePathAndReset(){//Calculates a new setDestination in case the previous calc isnt reached in a set reset time
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
	
	/// //////////
	/// GUI
	/// /////////
	
	
	void OnGUI(){		
		if(!_dead){
		currentScreenPos = Camera.main.WorldToScreenPoint(this.transform.position);
		if(showInfo){
			//Survivors's Information Box
			
			if(this.renderer.isVisible){
				GUI.Box(new Rect(currentScreenPos.x, Screen.height - currentScreenPos.y, infoBoxWidth, infoBoxHeight),
				        this.name + ": \n" +
				        "State: " + _state +
				        " \n" +
				        "Resources: " + _resourcesInSight.Count +
				        " \n");
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
	
	
	void Update () {
		if(!_dead){
		/**/
		// CICLO PRINCIPAL

		if (LevelHealth () != FULL_LEVEL && IsInBase() ) 
		{
			Heal ();
			//DEPOSIT
		}
		else if(ZombiesAround() && LevelHealth () != CRITICAL_LEVEL ) 
		{
			//ATTACK
			attackZombie(NearestZombie());
		}
		else if(ZombiesAround() && LevelHealth () == CRITICAL_LEVEL ) 
		{
			//ESCAPE
			randomMove();
		}
		else if (ResourcesAround() && LevelResources() != FULL_LEVEL )
		{
			CollectResources(NearestResource());
		}
		else if (IsInBase() && !ZombiesAround())
		{
			//DEPOSIT
		}
		//TODO: n pode ser so do nearest survivor action, nearest pode dar idle e outro attacking
		else if(SurvivorsAround()){
			if(isAnySurvivorAttacking()){
				MoveTo(anySurvivorAttacking());
			}else if (isAnySurvivorCollecting()){
				MoveTo(anySurvivorCollecting());
			}else{
				randomMove();
			}
		}
		else
			randomMove();
		
			//avoid navmesh pathfinding issues
			checkImpossiblePathAndReset();
			//DO NOT DELETE This forces collision updates in every frame
			this.transform.root.gameObject.transform.position += new Vector3(0.0f, 0.0f, -0.00001f);
			//Collider[] colliders = Physics.OverlapSphere(this.transform.position,_visionRange);

		/**/
		}else{
			this.renderer.material = transparentMaterial;
			Destroy(this.GetComponent<NavMeshAgent>());
			this.rigidbody.AddForce(0,2000.0f,0, ForceMode.Force);
		}
	}
}
