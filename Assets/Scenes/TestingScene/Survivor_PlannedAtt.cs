using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Survivor_PlannedAtt: MonoBehaviour {
	
	public Transform sphere;
	
	public float _healthLevel;
	public float _movSpeed;
	public float _visionRange;
	public float _attDamage;
	public float _attRange;
	public float _resourceLevel;
	public Transform _TopLeftBase;
	public Transform _BottomRightBase;
	
	private const float PICKUP_RANGE = 2.0f;
	
	//TODO: changed to debug, was 100
	private const float FULL_HEALTH = 100.0f;
	
	private const float MAX_RESOURCES = 100.0f;
	private const float CRITICAL_THRESHOLD = 40.0f;
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
	private const float RELOAD_SPEED_COLLECT = 1.0f;
	
	//TODO: was changed
	private const float RELOAD_SPEED_ATTACK = 3.0f;
	
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
	private bool _isReloadingCollect;
	private Vector3 _previousMoveAwayDirection;
	
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
	
	private Material reloading_mat;
	private Material survivor_mat;
	
	private bool _isInParty;
	private bool _isPartyLeader;
	private GameObject _partyLeader = null;
	private GameObject _nextPartyLeader = null;
	public GameObject _tank = null;
	public ParticleSystem shots;
	public bool _isTank;
	public GameObject _focusTargetZombie = null;
	private bool _firstTimeTanking; //this is to make sure the tank gets minion aggro
	private bool _isSafe; //after tanking and becoming injured, he runs to the leader
	private List<GameObject> _survivorsInTeam;

	private bool _movingEvade = false;
	
	
	private bool deliberatingNextZombieTarget;

	private List<string> plan;
	private bool _isExecutingAPlansIntruction;
	
	void Start () {
		
		_isTank = false;
		_firstTimeTanking = true;
		_isSafe = false;

		_healthLevel = FULL_HEALTH;
		
		_movSpeed = 8.0f;
		_visionRange = 20.0f;
		_attDamage = 25.0f;
		
		//TODO: was changed
		_attRange = 17.0f;
		
		if(this.gameObject.name.Equals("SurvivorLeader")) _isPartyLeader = true;
		_isInParty = true;
		_partyLeader = GameObject.Find("SurvivorLeader");
		_isTank = false;
		deliberatingNextZombieTarget = false;
		_survivorsInTeam = new List<GameObject>();

		plan = new List<string>() ;
		plan.Add ("superAttack");
		
		_resourceLevel = 0.0f;
		
		_zombiesInSight = new List<GameObject>();
		_survivorsInSight = new List<GameObject>();
		_resourcesInSight = new List<GameObject>();
		//_isCollecting = false;
		_state = IDLE;
		_isReloading = false;
		_isReloadingCollect = false;
		
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
		
		//TODO: dele this, debug
		reloading_mat = (Material)Resources.Load(@"Materials/Reloading",typeof(Material));
		survivor_mat = (Material)Resources.Load(@"Materials/Survivor_Test_Material",typeof(Material));
		
		lifebar_lenght = 30.0f;
		lifebar_height = 2.5f;
		lifebar_x_offset = -15.0f;
		lifebar_y_offset = -8.0f;
		
		timeWindow = PATH_RESET_TIME;
		CurrentDestination = this.transform.position;
		navMeshComp.speed = _movSpeed;

		_isExecutingAPlansIntruction = false;

		StartCoroutine("CicloBDI");
	}
	
	//Actuadores-------------------------------------------------------------------
	
	IEnumerator attackClosestZombie(GameObject nearestZombie){
		_isReloading = true;
		nearestZombie.GetComponent<Zombie_PlannedAtt>().loseHealth(_attDamage);
		//Instantiate (shots, this.transform.position, this.transform.rotation);
		this.renderer.material = reloading_mat;
		yield return new WaitForSeconds(RELOAD_SPEED_ATTACK);
		_isReloading = false;
		this.renderer.material = survivor_mat;
	}
	
	//Collect-Resources
	private void CollectResources(GameObject nearestResource){
		
		float _dist2Resource;
		
		_state = COLLECTING;
		navMeshComp.SetDestination(nearestResource.transform.position);
		_dist2Resource = Vector3.Distance(nearestResource.transform.position, this.transform.position);
		if(!_isReloadingCollect && _dist2Resource <= PICKUP_RANGE){
			StartCoroutine(collectDResource(nearestResource));
		}
	}
	
	IEnumerator collectDResource(GameObject nearestResource){
		_isReloadingCollect = true;
		_resourceLevel += nearestResource.GetComponent<ResourcesScript>().catchResources();
		yield return new WaitForSeconds(RELOAD_SPEED_COLLECT);
		_isReloadingCollect = false;
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
		return survivor.GetComponent<Survivor_PlannedAtt> ().getState ();
		
	}
	//Any-Survivor-Need-Help-Collecting
	private Vector3 anySurvivorCollecting(){
		GameObject survivorInNeed = null;
		foreach(GameObject survivor in _survivorsInSight){ //check each survivor around to see if they are collecting
			if( survivor.GetComponent<Survivor_PlannedAtt>().getState().Equals(COLLECTING)){
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
			if( survivor.GetComponent<Survivor_PlannedAtt>().getState().Equals(ATTACKING)){
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
			if( survivor.GetComponent<Survivor_PlannedAtt >().getState().Equals(ATTACKING)){
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
			if( survivor.GetComponent<Survivor_PlannedAtt>().getState().Equals(COLLECTING)){
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

	/// ////////
	/// Coliders
	/// ////////
	private bool showDebug = false;
	
	void OnTriggerEnter (Collider other) {
		
		if (other.tag.Equals("Survivor") && !other.transform.root.Equals(this.transform.root)){
			_survivorsInSight.Add(other.gameObject);

			//TODO: debug, this is hardcoded way to create a team
			if(!_survivorsInTeam.Contains(other.gameObject)){
				_survivorsInTeam.Add(other.gameObject);
			}
			
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

	//TODO:this was added
	private void informTeamThatImTheLeader(){
		//Debug.Log ("I was given a team of " + _survivorsInTeam.Count + " people.");
		string names = "";
		foreach (GameObject newSurvivor in _survivorsInTeam) {
			names += newSurvivor.name + " ";
			newSurvivor.GetComponent<Survivor_PlannedAtt>()._partyLeader = this.gameObject;
		}
		//Debug.Log (names);

	} 

	//TODO:this was added
	public void receivePreviousTeamLeaderKnowledge(List<GameObject> previousKnownTeam){
		Debug.Log ("Guys, I, " + this.name + " am now the TeamLeader!");
		_isPartyLeader = true;
		_survivorsInTeam = previousKnownTeam;
		_partyLeader = this.gameObject;
		if(_survivorsInTeam.Contains(this.gameObject)){
			_survivorsInTeam.Remove(this.gameObject);
		}
		informTeamThatImTheLeader ();
	}

	//TODO:this was added
	private void passKnowledgeToNextLeader(){
		if(_nextPartyLeader != null){
			_nextPartyLeader.GetComponent<Survivor_PlannedAtt> ().receivePreviousTeamLeaderKnowledge (_survivorsInTeam);
		}else{
			_nextPartyLeader = healthiestSurvivorInTeamNotTanking();
			_nextPartyLeader.GetComponent<Survivor_PlannedAtt> ().receivePreviousTeamLeaderKnowledge (_survivorsInTeam);
		}
	}

	public void loseHealth(float ammount){
		_healthLevel -= ammount;
		if(_healthLevel <= 0 && !_dead){
			_dead = true;
			Debug.Log(this.name + " died.");

			sayMyLastWords();

			if(_isPartyLeader){
				passKnowledgeToNextLeader();
			}

			//to make it "disappear"

			Instantiate(Resources.Load(@"Models/Characters/ZombieAD"), this.transform.position, this.transform.rotation);
			StartCoroutine("destroyAfterDeath");
		}
	}
	
	private IEnumerator destroyAfterDeath(){
		yield return new WaitForSeconds(3.0F);
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
					        _partyLeader.name +
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

	//TODO: this was added
	private void evadeManouvers(GameObject zombie){
		if (!_movingEvade) {
			Vector3 zombiePos = zombie.transform.position;
			Vector3 destination = this.transform.position - zombiePos;
			destination.y = 0;
			destination = Vector3.Normalize(destination);
			
			//Debug.Log ("set destination: " + destination.x + " " + destination.z);
			
			if (Random.Range (0, 2) == 1) {
				StartCoroutine(rotateAndMoveTo(70.0f, destination));
				//Debug.Log("Going Right!");
			}else{
				StartCoroutine(rotateAndMoveTo(-70.0f, destination));
				//Debug.Log("Going Left!");
			}
		}
	}
	
	//TODO: this was added
	IEnumerator rotateAndMoveTo(float angle, Vector3 dest){
		_movingEvade = true;
		Vector3 rotatedVector = Quaternion.AngleAxis(angle, Vector3.up) * dest * 20;

		navMeshComp.SetDestination (this.transform.position + rotatedVector);
		Debug.DrawRay(this.transform.position, rotatedVector, Color.red);
		yield return new WaitForSeconds(1.0f);
		_movingEvade = false;
	}
		
	//TODO: this was added
	private void moveAwayFromZombie(GameObject zombie){
		if (!_movingEvade) {
			Vector3 zombiePos = zombie.transform.position;
			Vector3 destination = this.transform.position - zombiePos;
			destination.y = 0;
			destination = Vector3.Normalize (destination) * 20;
			navMeshComp.SetDestination (this.transform.position + destination);

		}
	}

	//TODO: this was added
	//Since leaders are never tanking, running to them means they'll be running 
	//towards a team member far away from the combat, when they are !_isSafe
	private GameObject healthiestSurvivorInTeamNotTanking(){
		GameObject healthiestSurv = null;
		float healthiestSurvHealth = 0.0f;
		bool isTanking;
		foreach (GameObject newSurvivor in _survivorsInTeam) {
			//If its full health, pick the first one and return
			float newSurvivorHealth = newSurvivor.GetComponent<Survivor_PlannedAtt>()._healthLevel;
			isTanking = newSurvivor.GetComponent<Survivor_PlannedAtt>()._isTank;
			if(!isTanking){
				if(newSurvivorHealth == FULL_HEALTH){
					return newSurvivor;
				}
				
				if(healthiestSurv != null){
					if (newSurvivorHealth > healthiestSurvHealth){
						healthiestSurv = newSurvivor;
						healthiestSurvHealth = newSurvivorHealth;
					}
				}else{
					healthiestSurv = newSurvivor;
					healthiestSurvHealth = newSurvivorHealth;
				}
			}
		}
		return healthiestSurv;
	}

	//TODO: this was added
	private GameObject healthiestSurvivorInTeam(){
		GameObject healthiestSurv = null;
		float healthiestSurvHealth = 0.0f;
		//Debug.Log ("Survivors in team: " + _survivorsInTeam.Count);
		foreach (GameObject newSurvivor in _survivorsInTeam) {
			//If its full health, pick the first one and return
			float newSurvivorHealth = newSurvivor.GetComponent<Survivor_PlannedAtt>()._healthLevel;
			if(newSurvivorHealth == FULL_HEALTH ){
				return newSurvivor;
			}
			
			if(healthiestSurv != null){
				if (newSurvivorHealth > healthiestSurvHealth){
					healthiestSurv = newSurvivor;
					healthiestSurvHealth = newSurvivorHealth;
				}
			}else{
				healthiestSurv = newSurvivor;
				healthiestSurvHealth = newSurvivorHealth;
			}
		}
		return healthiestSurv;
	}
	
	//TODO: this was created
	private float distanceToZombie(GameObject zombie){
		return Vector3.Distance(zombie.transform.position, this.transform.position);
	}
	
	//TODO: this was created
	private float distanceToSurvivor(GameObject survivor){
		return Vector3.Distance(survivor.transform.position, this.transform.position);
	}
	
	//TODO: this was changed
	private void attackZombie(GameObject zombie){
		_state = ATTACKING;
		float _dist2Zombie = Vector3.Distance(zombie.transform.position, this.transform.position);
		
		if (_dist2Zombie > _attRange-1) {
			navMeshComp.SetDestination (zombie.transform.position);
		} else {
			if (_isReloading) {
				moveAwayFromZombie (NearestZombie());
			} else {
				StartCoroutine(attackClosestZombie(zombie));
			}
		}
	}
	//TODO: this was added
	private void attackZombieAsTank(GameObject nearestZombie){
		_state = ATTACKING;
		float _dist2Zombie = Vector3.Distance(nearestZombie.transform.position, this.transform.position);
		//TODO: should be changed
		if (_dist2Zombie > 10.0f) {
			navMeshComp.SetDestination (nearestZombie.transform.position);
		} else {
			if (_isReloading) {
				if(Random.Range(0,6) != 0){
					evadeManouvers(nearestZombie);
				}else{
					moveAwayFromZombie(nearestZombie);
				}
			} else {
				StartCoroutine(attackClosestZombie(nearestZombie));
			}
		}
	}
	
	//TODO: this was added
	//Ask lider to elect a new survivor for tank role
	private void demandNewTank(){
		_partyLeader.GetComponent<Survivor_PlannedAtt> ().reRollTank (this.gameObject);
		_isTank = false;
	}
	
	//TODO: this was added
	//called by previous tank, this will force the choosing of a new one
	public void reRollTank (GameObject previousTank){
		if(_tank.Equals(previousTank)){
			_tank = null;
		}
	}
	
	//TODO: this was added
	private void requestNewTargetForTeam(){
		_partyLeader.GetComponent<Survivor_PlannedAtt> ().deliberateNextZombieTarget ();
	}
	
	//TODO: this was added
	private void getNewTeamTarget(){
		_partyLeader.GetComponent<Survivor_PlannedAtt> ().checkDeliberateNextZombieTarget ();
	}

	//TODO: this was added
	public void checkDeliberateNextZombieTarget(){
		if (this._focusTargetZombie != null) {
			foreach (GameObject survivorInTeam in _survivorsInTeam) {
				survivorInTeam.GetComponent<Survivor_PlannedAtt> ()._focusTargetZombie = _focusTargetZombie;
			}
		} else {
			deliberateNextZombieTarget();
		}
	}


	
	//TODO: this was added
	//this makes it so that a zombie who escapes the tank aggro gets killed first, since the leader choses the closest zombie to the survivors, which usually is the one closest to the tank
	public void deliberateNextZombieTarget(){
		
		if (!deliberatingNextZombieTarget) {
			deliberatingNextZombieTarget = true;
			GameObject nextZombie2Focus = null;
			GameObject tempZombie;
			float closestZombieDistanceOfParty = 1000.0f;
			float closestZombieDistanceOfSurvivor;

			foreach (GameObject survivorInTeam in _survivorsInTeam) {
				tempZombie = survivorInTeam.GetComponent<Survivor_PlannedAtt> ().NearestZombie ();
				if(tempZombie != null){
					closestZombieDistanceOfSurvivor = Vector3.Distance (
						tempZombie.transform.position, 
						survivorInTeam.transform.position);
					
					if (closestZombieDistanceOfParty > closestZombieDistanceOfSurvivor) {
						closestZombieDistanceOfParty = closestZombieDistanceOfSurvivor;
						nextZombie2Focus = tempZombie;
					}
				}
			}
			
			if (nextZombie2Focus != null) {
				foreach (GameObject survivorInTeam in _survivorsInTeam) {
					survivorInTeam.GetComponent<Survivor_PlannedAtt> ()._focusTargetZombie = nextZombie2Focus;
				}
				_focusTargetZombie = nextZombie2Focus;
				//Debug.Log("Chose next target!");
				deliberatingNextZombieTarget = false;
			}
		}
	}
	
	//TODO: this was added
	public void becomeTank(){
		_isTank = true;
		_firstTimeTanking = true;
	}
	
	//TODO: this was added
	private float distance2NearestTeamMember(){
		float closestTeamMemberDistance = 1000.0f;
		float tempDist;

		foreach (GameObject survivorInTeam in _survivorsInTeam) {
			tempDist = Vector3.Distance(survivorInTeam.transform.position, this.transform.position);
			if(closestTeamMemberDistance > tempDist){
				closestTeamMemberDistance = tempDist;
			}
		}
		return closestTeamMemberDistance;
	}
	
	//TODO:this was added
	private void sayMyLastWords(){
		_partyLeader.GetComponent<Survivor_PlannedAtt> ().ISeeDeadPeople(this.gameObject);
	}
	//TODO:this was added
	public void ISeeDeadPeople(GameObject deadGuyWalking){
		_survivorsInTeam.Remove (deadGuyWalking);
		if (deadGuyWalking.Equals (_nextPartyLeader)) {
			_nextPartyLeader = healthiestSurvivorInTeamNotTanking();
		}
	}

	private string instruction = "";
	
	public List<string> NewPlan(){
		List<string> plan = new List<string>() ;
		
		int number = Random.Range(0, 10);
		for (int i = 0; i < number; i++)
		{
			if (Random.Range(0, 2) == 0)
				plan.Add("I1");
			else
				plan.Add("I2");
		}
		return plan;
	}
	
	void Intruction1()
	{
		for (int i = 0; i < 10; i++)
			Debug.Log("Counting " + i);
	}
	
	void Intruction2()
	{
		for (int i = 10; i > 0; i--)
			Debug.Log("Counting " + i);
	}



	IEnumerator CicloBDI()
	{
		int i = 0, j = 0;
		while (true)
		{
			//List<string> thePlan = NewPlan();
			//Debug.Log("Plan " + i + " #" + plan.Count);
			
			while (j < plan.Count)
			{
				//Debug.Log("Plano " + i + " instrucao " + j);
				
				instruction = plan[j];
				
				_isExecutingAPlansIntruction = true;
				
				while (_isExecutingAPlansIntruction) { yield return null; };
				

				
				j++;
				
				//yield return null;
			}

			i++;
			j = 0;
		}
	}

	private void superAttack(){
		/**/
		//execute do plan(Intenção att grupo)
		//TODO: this was added
		/**/
		if(_isPartyLeader){
			if(_tank == null){
				_tank = healthiestSurvivorInTeam();
				if(_tank != null){
					_tank.GetComponent<Survivor_PlannedAtt>().becomeTank();
					Debug.Log( _tank.name + " is now tanking!");
				}
			}
			if(_nextPartyLeader == null){
				_nextPartyLeader = healthiestSurvivorInTeamNotTanking();
			}
		}
		
		if (ZombiesAround () && LevelHealth () != CRITICAL_LEVEL) {
			if (_isTank) {
				if (_focusTargetZombie == null) {
					//requestNewTargetForTeam ();
					getNewTeamTarget();
					attackZombieAsTank (NearestZombie ());
				} else {
					//TODO: delete, for debug
					sphere.position = _focusTargetZombie.transform.position;
					
					if (!_firstTimeTanking) {
						attackZombieAsTank (_focusTargetZombie);
					} else {
						MoveTo (_focusTargetZombie.transform.position);
						if (distanceToZombie (_focusTargetZombie) < 7) {
							_firstTimeTanking = false;
						}
						attackZombieAsTank (_focusTargetZombie);
					}
				}
			} else {
				if (_focusTargetZombie == null) {
					getNewTeamTarget();
					attackZombie (NearestZombie ());
				} else {
					attackZombie (_focusTargetZombie);
				}
			}
		}else if (ZombiesAround () && LevelHealth () == CRITICAL_LEVEL && SurvivorsAround () && !_isSafe) 
		{	
			MoveTo (_partyLeader.transform.position);
			if (distanceToSurvivor (_partyLeader) < 10) {
					_isSafe = true;
				Debug.Log("I was saved! " + this.name);
			}
			if (_isTank) {
					Debug.Log ("Switch!");
					demandNewTank ();
			}
		}
		else if (ZombiesAround () && LevelHealth () == CRITICAL_LEVEL && SurvivorsAround () && _isSafe) 
		{
			if (_focusTargetZombie == null) {
				getNewTeamTarget();
				attackZombie (NearestZombie ());
			} else {
				attackZombie (_focusTargetZombie);
			}
		}
		else if(!ZombiesAround() && !ResourcesAround()) 
		{
			if(_tank == null){
				MoveTo(_partyLeader.transform.position);
			}else{
				MoveTo(_tank.transform.position);
			}

		}
		else if(!ZombiesAround() && LevelResources() != 0 && DepositInRange()) 
		{
			DepositResources();
		}
		else if (ResourcesAround() && LevelResources() != FULL_LEVEL )
		{
			CollectResources(NearestResource());
		}
		else if(SurvivorsAround())
		{
			if(isAnySurvivorAttacking()){
				MoveTo(anySurvivorAttacking());
			}
			else if (isAnySurvivorCollecting()){
				MoveTo(anySurvivorCollecting());
			}
			else{
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
		
	}
	
	void Update () {
		if(!_dead){
			/**/
			if (_isExecutingAPlansIntruction)
			{
				if (instruction == "I1")
					{
						Intruction1();
						_isExecutingAPlansIntruction = false;
					}
					else if (instruction == "I2")
					{
						Intruction2();
						_isExecutingAPlansIntruction = false;
					}
					else if (instruction == "superAttack")
					{
						superAttack();
						_isExecutingAPlansIntruction = false;
					}
	        }
	        /**/

			 
		}else{
			this.renderer.material = transparentMaterial;
			Destroy(this.GetComponent<NavMeshAgent>());
			this.rigidbody.AddForce(0,100.0f,0, ForceMode.Force);
		}
	}
}