using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SurvivorScript: MonoBehaviour {
	
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
	private const float RELOAD_SPEED_ATTACK = 1.5f;
	
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
	
	
	private Material reloading_mat;
	private Material survivor_mat;
	
	private bool _isInParty;
	private bool _isPartyLeader;
	private GameObject _partyLeader = null;
	private GameObject _nextPartyLeader = null;
	public GameObject _tank = null;
	public bool _isTank;
	public GameObject _focusTargetZombie = null; 
	private bool _firstTimeTanking; //this is to make sure the tank gets minion aggro
	private bool _isSafe; //after tanking and becoming injured, he runs to the leader
	private List<GameObject> _survivorsInTeam;
	
	private bool _movingEvade = false;
	
	private bool _oneSuperStepExecuted = false; //for superFunctions, to use at checkStepCompleted
	
	private bool deliberatingNextZombieTarget;
	
	
	//private bool _isCollecting;
	
	//TODO: variaveis deliberativo
	
	// estado interno
	private List<Vector3> resources_location;
	private List<Vector3> resources_depleted;
	
	private List<string> Desires;
	private List<string> Plan;
	
	private List<Desire> Desires_W;
	
	private int _minTeamMembers;
	
	public string intention;
	private Vector3 intention_position;
	private const string HEAL = "heal";
	private const string DEFEND_BASE = "defend_base";
	private const string HELP_ATTACK = "help_attack";
	private const string ATTACK_ZOMBIE = "attack_zombie";
	private const string ATTACK_ZOMBIE_AS_GROUP = "attack_zombie_as_group";
	private const string DEPOSIT = "deposit";
	private const string COLLECT = "collect";
	private const string HELP_COLLECT = "help_collect";
	private const string FOLLOW_SURVIVOR = "follow_suvivor";
	private const string HELP_SURVIVOR_LOW_HEALTH = "help_survivor_low_health";
	private const string HELP_GO_BASE = "help_go_base";
	private const string EXPLORE = "explore";
	private const string EXPLORE_SOLO = "explore_solo";
	private const string PATROL = "patrol";
	private const string GO_BASE = "go_base";
	private const string EXPLORE_AS_GROUP = "explore_as_group";
	
	private bool out_of_while;
	private string instructionName = "";
	
	private const string HEAL_PLAN = "heal_plan";
	private const string MOVE_TO_PLAN = "move_to";
	private const string ATTACK_PLAN = "attack_plan";
	private const string COLLECT_PLAN = "collect_plan";
	private const string DEPOSIT_PLAN = "deposit_plan";
	private const string RANDOM_MOVE_TO_PLAN = "random_move";
	private const string ATTACK_PLAN_GROUP = "superAttack";
	private const string EXPLORE_PLAN_GROUP = "superExplore";
	
	private bool lider_request_defend;
	private bool lider_request_resources;
	
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
		
		/** /
			if(gameObject.name.Equals("SurvivorLeader")) _isPartyLeader = true;
			_isInParty = true;
			_partyLeader = GameObject.Find("SurvivorLeader");
			_isTank = false;
			deliberatingNextZombieTarget = false;
			_survivorsInTeam = new List<GameObject>();
		/**/
		
		//TODO: dele this, debug
		reloading_mat = (Material)Resources.Load(@"Materials/Reloading",typeof(Material));
		survivor_mat = (Material)Resources.Load(@"Materials/Survivor_Test_Material",typeof(Material));
		
		_healthLevel = 100.0f;
		_movSpeed = 8.0f;
		_visionRange = 20.0f;
		_attDamage = 25.0f;
		_attRange = 17.0f;
		_resourceLevel = 50.0f;
		
		_zombiesInSight = new List<GameObject>();
		_survivorsInSight = new List<GameObject>();
		_resourcesInSight = new List<GameObject>();
		//_isCollecting = false;
		_state = IDLE;
		_isReloading = false;
		_isReloadingCollect = false;
		out_of_while = true;
		
		// Deliberativo
		
		
		_minTeamMembers = 3;
		
		Desires = new List<string>();
		Plan = new List<string>();
		resources_location = new List<Vector3>();
		resources_depleted = new List<Vector3> ();
		Desires_W = new List<Desire>();
		intention = IDLE;
		intention_position = Vector3.zero;
		lider_request_defend = false;
		lider_request_resources = false;
		
		_dead = false;
		
		navMeshComp = GetComponent<NavMeshAgent>();
		
		BaseLider = GameObject.FindWithTag("BaseLeader");
		healPosition = GameObject.FindWithTag ("Heal").transform.position;
		depositPosition = GameObject.FindWithTag ("Deposit").transform.position;
		
		SphereCollider visionRangeCollider = gameObject.GetComponentInChildren<SphereCollider>();
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
		lifebar_height = 2.5f;
		lifebar_x_offset = -15.0f;
		lifebar_y_offset = -8.0f;
		
		timeWindow = PATH_RESET_TIME;
		CurrentDestination = transform.position;
		navMeshComp.speed = _movSpeed;
		
	}
	
	//Actuadores-------------------------------------------------------------------
	
	private void attackZombie(GameObject zombie){
		_state = ATTACKING;
		float _dist2Zombie = Vector3.Distance(zombie.transform.position, transform.position);
		
		if (_dist2Zombie > _attRange - 1) {
			navMeshComp.SetDestination (zombie.transform.position);
		}  else {
			if (_isReloading) {
				moveAwayFromZombie (NearestZombie());
			}  else {
				StartCoroutine(attackClosestZombie(zombie));
			}
		}
	}
	
	IEnumerator attackClosestZombie(GameObject nearestZombie){
		_isReloading = true;
		nearestZombie.GetComponent<ZombieScript>().loseHealth(_attDamage);
		renderer.material = reloading_mat;
		yield return new WaitForSeconds(RELOAD_SPEED_ATTACK);
		_isReloading = false;
		renderer.material = survivor_mat;
	}
	
	//Collect-Resources
	private void CollectResources(GameObject nearestResource){
		
		float _dist2Resource;
		
		_state = COLLECTING;
		navMeshComp.SetDestination(nearestResource.transform.position);
		_dist2Resource = Vector3.Distance(nearestResource.transform.position, transform.position);
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
		
		if ((depositPosition - transform.position).magnitude < 5) {
			//BaseLider.GetComponent<BaseLeaderScript>().addResources(_resourceLevel);
			Debug.Log("Deposited!");
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
		if (intention == ATTACK_ZOMBIE)
			_state = ATTACKING;
		navMeshComp.SetDestination (position);
	}
	
	//Random-Move
	private void randomMove(){
		/**/
		if (!_state.Equals(IDLE)) {
			CurrentDestination = transform.position;
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
	public int LevelHealth() 
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
		if (transform.position.x > _TopLeftBase.position.x &&
		    transform.position.x < _BottomRightBase.position.x &&
		    transform.position.z > _TopLeftBase.position.z &&
		    transform.position.z < _BottomRightBase.position.z
		    )
			return true;
		else
			return false;
	}
	
	//is-In-Group
	private bool IsInParty(){
		if (_isInParty) {
			return true;		
		}else
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
				if (Vector3.Distance(_closestSurvivor.transform.position, transform.position) >
				    Vector3.Distance(survivor.transform.position, transform.position))
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
				if (Vector3.Distance(_closestSurvivor.transform.position, transform.position) >
				    Vector3.Distance(survivor.transform.position, transform.position))
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
				if (Vector3.Distance(_closestZombie.transform.position, transform.position) >
				    Vector3.Distance(zombie.transform.position, transform.position))
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
				if (Vector3.Distance(_closestZombie.transform.position, transform.position) >
				    Vector3.Distance(zombie.transform.position, transform.position))
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
				if (Vector3.Distance(_closestResource.transform.position, transform.position) >
				    Vector3.Distance(resource.transform.position, transform.position))
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
				if (Vector3.Distance(_closestResource.transform.position, transform.position) >
				    Vector3.Distance(resource.transform.position, transform.position))
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
				}else{  //chooses the closest survivor to him that is collecting
					if (Vector3.Distance(survivorInNeed.transform.position, transform.position) >
					    Vector3.Distance(survivor.transform.position, transform.position))
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
			if( survivor.GetComponent<SurvivorScript>().intention.Equals(ATTACK_ZOMBIE)){
				if(survivorInNeed == null){
					survivorInNeed = survivor;
				}else{  //chooses the closest survivor to him that is attacking
					if (Vector3.Distance(survivorInNeed.transform.position, transform.position) >
					    Vector3.Distance(survivor.transform.position, transform.position))
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
			if( survivor.GetComponent<SurvivorScript>().intention.Equals(ATTACK_ZOMBIE)){
				if(survivorInNeed == null){
					survivorInNeed = survivor;
				}else{  //chooses the closest survivor to him that is attacking
					if (Vector3.Distance(survivorInNeed.transform.position, transform.position) >
					    Vector3.Distance(survivor.transform.position, transform.position))
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
				}else{  //chooses the closest survivor to him that is collecting
					if (Vector3.Distance(survivorInNeed.transform.position, transform.position) >
					    Vector3.Distance(survivor.transform.position, transform.position))
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
	//TODO
	///////////////////
	//Delibarativo
	///
	
	public class Desire{
		public string des;
		public int weight;
		
		public Desire (string newDesire, int newWeight){
			des = newDesire;
			weight = newWeight;
		}
		public string getDesire(){
			return des;
		}
		public int getW(){
			return weight;
		}
		
	}
	
	//-------------------------------------------------------------------------------------
	public void addResourceMap(Vector3 position){
		if(!resources_location.Contains(position))
			resources_location.Add(position);
	}
	
	public void removeResourceMap(Vector3 position){
		if(!resources_depleted.Contains(position)){
			resources_location.Remove(position);
			resources_depleted.Add(position);
		}
	}
	
	
	
	public Vector3 getNearestResourceMap(){
		float dist = Mathf.Infinity;
		
		Vector3 posB = Vector3.zero;
		foreach(Vector3 pos in resources_location){
			if(dist > Vector3.Distance(transform.position, pos)){
				dist = Vector3.Distance(transform.position, pos);
				posB = pos;
			}
		}
		
		return posB;
	}
	
	/*public void UpdateResourceMap(List<Vector3> resource_map, List<Vector3> resource_map_depleted){
		
		resources_location = resources_location.Union<Vector3>((IEnumerable)resource_map);
		resources_depleted = resources_depleted.Union<Vector3> ((IEnumerable)resource_map_depleted);
		
		foreach(Vector3 positionToDelete in resources_depleted){
			resources_location.Remove(positionToDelete);
		}
	}
	*/
	private Vector3 getPositionInBase(){
		return new Vector3( Random.Range(_TopLeftBase.position.x, _BottomRightBase.position.x + 1) , 
		                   transform.position.y , Random.Range(_TopLeftBase.position.z, _BottomRightBase.position.z + 1));
		
	}
	
	
	private GameObject getSurvivorWithoutCritical(){
		
		
		foreach (GameObject sur in _survivorsInSight) {
			if (sur.GetComponent<SurvivorScript> ().LevelHealth () != CRITICAL_LEVEL) {
				return sur;
			}
		}
		return null;
		
	}
	
	private bool isHereSurvivorWithoutCritical(){
		
		
		foreach (GameObject sur in _survivorsInSight) {
			if (sur.GetComponent<SurvivorScript> ().LevelHealth () != CRITICAL_LEVEL) {
				return true;
			}
		}
		return false;
		
	}
	
	private bool isHereSurvivorCritical(){
		
		foreach (GameObject sur in _survivorsInSight) {
			if (sur.GetComponent<SurvivorScript> ().LevelHealth () == CRITICAL_LEVEL) {
				return true;
			}
		}
		return false;
		
	}
	
	//TODO:this was added
	private void informTeamThatImTheLeader(){
		//Debug.Log ("I was given a team of " + _survivorsInTeam.Count + " people.");
		string names = "";
		foreach (GameObject newSurvivor in _survivorsInTeam) {
			names += newSurvivor.name + " ";
			newSurvivor.GetComponent<SurvivorScript>()._partyLeader = gameObject;
		}
		//Debug.Log (names);
		
	}  
	
	//TODO:this was added
	public void receivePreviousTeamLeaderKnowledge(List<GameObject> previousKnownTeam){
		Debug.Log ("Guys, I, " + name + " am now the TeamLeader!");
		_isPartyLeader = true;
		_survivorsInTeam = previousKnownTeam;
		_partyLeader = gameObject;
		if(_survivorsInTeam.Contains(gameObject)){
			_survivorsInTeam.Remove(gameObject);
		}
		informTeamThatImTheLeader ();
	}
	
	//TODO:this was added
	private void passKnowledgeToNextLeader(){
		if(_nextPartyLeader != null){
			_nextPartyLeader.GetComponent<SurvivorScript> ().receivePreviousTeamLeaderKnowledge (_survivorsInTeam);
		}else{
			_nextPartyLeader = healthiestSurvivorInTeamNotTanking();
			_nextPartyLeader.GetComponent<SurvivorScript> ().receivePreviousTeamLeaderKnowledge (_survivorsInTeam);
		}
	}
	
	//TODO: this was added
	private void evadeManouvers(GameObject zombie){
		if (!_movingEvade) {
			Vector3 zombiePos = zombie.transform.position;
			Vector3 destination = transform.position - zombiePos;
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
		
		navMeshComp.SetDestination (transform.position + rotatedVector);
		Debug.DrawRay(transform.position, rotatedVector, Color.red);
		yield return new WaitForSeconds(1.0f);
		_movingEvade = false;
	}
	
	//TODO: this was added
	private void moveAwayFromZombie(GameObject zombie){
		Debug.Log ("moving away!");
		if (!_movingEvade) {
			Vector3 zombiePos = zombie.transform.position;
			Vector3 destination = transform.position - zombiePos;
			destination.y = 0;
			destination = Vector3.Normalize (destination) * 20;
			navMeshComp.SetDestination (transform.position + destination);
			
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
			float newSurvivorHealth = newSurvivor.GetComponent<SurvivorScript>()._healthLevel;
			isTanking = newSurvivor.GetComponent<SurvivorScript>()._isTank;
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
			float newSurvivorHealth = newSurvivor.GetComponent<SurvivorScript>()._healthLevel;
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
	
	
	private float distanceToZombie(GameObject zombie){
		return Vector3.Distance(zombie.transform.position, transform.position);
	}
	
	private float distanceToSurvivor(GameObject survivor){
		return Vector3.Distance(survivor.transform.position, transform.position);
	}
	
	private void attackZombieAsTank(GameObject nearestZombie){
		_state = ATTACKING;
		float _dist2Zombie = Vector3.Distance(nearestZombie.transform.position, transform.position);
		//TODO: should be changed
		if (_dist2Zombie > 10.0f) {
			navMeshComp.SetDestination (nearestZombie.transform.position);
		}  else {
			if (_isReloading) {
				if(Random.Range(0,6) != 0){
					evadeManouvers(nearestZombie);
				}else{
					moveAwayFromZombie(nearestZombie);
				}
			}  else {
				StartCoroutine(attackClosestZombie(nearestZombie));
			}
		}
	}
	
	//Ask lider to elect a new survivor for tank role
	private void demandNewTank(){
		_partyLeader.GetComponent<SurvivorScript> ().reRollTank (gameObject);
		_isTank = false;
	}
	
	//called by previous tank, this will force the choosing of a new one
	public void reRollTank (GameObject previousTank){
		if(_tank.Equals(previousTank)){
			_tank = null;
		}
	}
	
	private void requestNewTargetForTeam(){
		_partyLeader.GetComponent<SurvivorScript> ().deliberateNextZombieTarget ();
	}
	
	private void getNewTeamTarget(){
		_partyLeader.GetComponent<SurvivorScript> ().checkDeliberateNextZombieTarget ();
	}
	
	public void checkDeliberateNextZombieTarget(){
		if (_focusTargetZombie != null) {
			foreach (GameObject survivorInTeam in _survivorsInTeam) {
				survivorInTeam.GetComponent<SurvivorScript> ()._focusTargetZombie = _focusTargetZombie;
			}
		}  else {
			deliberateNextZombieTarget();
		}
	}
	
	
	//this makes it so that a zombie who escapes the tank aggro gets killed first, 
	//since the leader choses the closest zombie to the survivors, which usually is the one closest to the tank
	public void deliberateNextZombieTarget(){
		
		if (!deliberatingNextZombieTarget) {
			deliberatingNextZombieTarget = true;
			GameObject nextZombie2Focus = null;
			GameObject tempZombie;
			float closestZombieDistanceOfParty = 1000.0f;
			float closestZombieDistanceOfSurvivor;
			
			foreach (GameObject survivorInTeam in _survivorsInTeam) {
				tempZombie = survivorInTeam.GetComponent<SurvivorScript> ().NearestZombie ();
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
					survivorInTeam.GetComponent<SurvivorScript> ()._focusTargetZombie = nextZombie2Focus;
				}
				_focusTargetZombie = nextZombie2Focus;
				//Debug.Log("Chose next target!");
				deliberatingNextZombieTarget = false;
			}
		}
	}
	
	public void becomeTank(){
		_isTank = true;
		_firstTimeTanking = true;
	}
	
	private float distance2NearestTeamMember(){
		float closestTeamMemberDistance = 1000.0f;
		float tempDist;
		
		foreach (GameObject survivorInTeam in _survivorsInTeam) {
			tempDist = Vector3.Distance(survivorInTeam.transform.position, transform.position);
			if(closestTeamMemberDistance > tempDist){
				closestTeamMemberDistance = tempDist;
			}
		}
		return closestTeamMemberDistance;
	}
	
	private void sayMyLastWords(){
		_partyLeader.GetComponent<SurvivorScript> ().ISeeDeadPeople(gameObject);
		foreach (GameObject survivorInMyteam in _survivorsInTeam) {
			survivorInMyteam.GetComponent<SurvivorScript> ()._survivorsInTeam.Remove(gameObject);
		}
	}
	
	public void ISeeDeadPeople(GameObject deadGuyWalking){
		_survivorsInTeam.Remove (deadGuyWalking);
		if (deadGuyWalking.Equals (_nextPartyLeader)) {
			_nextPartyLeader = healthiestSurvivorInTeamNotTanking();
		}
	}
	
	private void superExplore(){
		//leader defines area that will be explored.
		
		
		// group moves as one to the area
		
		
		//when enough area was discovered, everyone returns to base
		
		
		
		
		_oneSuperStepExecuted = true;
		Plan.Add ("superExplore");
	}
	
	
	private void superAttack(){
		//execute do plan(Intençã att grupo)
		
		_oneSuperStepExecuted = false;
		
		if (_isPartyLeader) {
			if (_tank == null) {
				_tank = healthiestSurvivorInTeam ();
				if (_tank != null) {
					_tank.GetComponent<SurvivorScript> ().becomeTank ();
					Debug.Log (_tank.name + " is now tanking!");
				}
			}
			if (_nextPartyLeader == null) {
				_nextPartyLeader = healthiestSurvivorInTeamNotTanking ();
			}
		}
		
		if (ZombiesAround () && LevelHealth () != CRITICAL_LEVEL) {
			if (_isTank) {
				if (_focusTargetZombie == null) {
					//requestNewTargetForTeam ();
					getNewTeamTarget ();
					attackZombieAsTank (NearestZombie ());
				}  else {
					//TODO: delete, for debug
					sphere.position = _focusTargetZombie.transform.position;
					
					if (!_firstTimeTanking) {
						attackZombieAsTank (_focusTargetZombie);
					}  else {
						MoveTo (_focusTargetZombie.transform.position);
						if (distanceToZombie (_focusTargetZombie) < 7) {
							_firstTimeTanking = false;
						}
						attackZombieAsTank (_focusTargetZombie);
					}
				}
			}  else {
				if (_focusTargetZombie == null) {
					getNewTeamTarget ();
					attackZombie (NearestZombie ());
				}  else {
					attackZombie (_focusTargetZombie);
				}
			}
		}  else if (ZombiesAround () && LevelHealth () == CRITICAL_LEVEL && SurvivorsAround () && !_isSafe) {	
			MoveTo (_partyLeader.transform.position);
			if (distanceToSurvivor (_partyLeader) < 10) {
				_isSafe = true;
				Debug.Log ("I was saved! " + name);
			}
			if (_isTank) {
				Debug.Log ("Switch!");
				demandNewTank ();
			}
		}  else if (ZombiesAround () && LevelHealth () == CRITICAL_LEVEL && SurvivorsAround () && _isSafe) {
			if (_focusTargetZombie == null) {
				getNewTeamTarget ();
				attackZombie (NearestZombie ());
			}  else {
				attackZombie (_focusTargetZombie);
			}
		}  else if (!ZombiesAround () && !ResourcesAround ()) {
			if (_tank == null) {
				MoveTo (_partyLeader.transform.position);
			}  else {
				MoveTo (_tank.transform.position);
			}
		}
		_oneSuperStepExecuted = true;
		Plan.Add ("superAttack");
	}
	
	
	
	void Options(){
		Desires.Clear ();
		//Desires.Add (HELP_ATTACK);
		if (LevelHealth () != FULL_LEVEL) 
			Desires.Add (HEAL);
		//variavel
		
		//TODO condicao de paragem e accoes para fazer
		if(lider_request_defend)   
			Desires.Add (DEFEND_BASE);
		// fixo
		if (isAnySurvivorAttacking ()) {  
			Desires.Add (HELP_ATTACK);
			//fixo
			//Debug.Log("tenho o desejo de ajudar");
		}
		if (ZombiesAround ()) {  
			Desires.Add (ATTACK_ZOMBIE);
			
		}
		if(LevelResources() != EMPTY_LEVEL)
			Desires.Add (DEPOSIT);
		//variavel
		
		//TODO
		if(ResourcesAround() && LevelResources() != FULL_LEVEL )
			//|| resources_location != null)
			//(lider_request_resources && LevelResources() == EMPTY_LEVEL) || ResourcesAround() || resources_location != null) && LevelResources() != FULL_LEVEL)
			Desires.Add (COLLECT);
		//variavel
		
		if(LevelHealth () == CRITICAL_LEVEL && SurvivorsAround() && isHereSurvivorWithoutCritical() && !IsInBase())  	
			Desires.Add (FOLLOW_SURVIVOR);
		//variavel
		
		if (LevelHealth () != CRITICAL_LEVEL && isHereSurvivorCritical() && !IsInBase())
			Desires.Add (HELP_GO_BASE); 
		//fixo
		
		//TODO
		if(resources_location == null)  	
			Desires.Add (GO_BASE);
		//fixo
		
		//TODO
		if(IsInBase() && intention == IDLE)  	
			Desires.Add (EXPLORE);
		
		//TODO
		if(intention == IDLE)  	
			Desires.Add (EXPLORE_SOLO);
		
		
		if (IsInParty ()) {
			Desires.Add (ATTACK_ZOMBIE_AS_GROUP);
		}
		
		Desires.Add (IDLE);
	}
	
	
	void filter(){
		intention_position = Vector3.zero;
		
		Desires_W.Clear ();
		
		if (Desires.Contains(HEAL))
		{
			if (LevelHealth() == CRITICAL_LEVEL)
				Desires_W.Add(new Desire(HEAL,70));
			if (LevelHealth() == NORMAL_LEVEL)
				Desires_W.Add(new Desire(HEAL,48));
		}
		if(Desires.Contains(FOLLOW_SURVIVOR))
		{
			if (SurvivorsAround())
				Desires_W.Add(new Desire(FOLLOW_SURVIVOR,80));
			
		}
		if(Desires.Contains(DEFEND_BASE))
		{
			
			Desires_W.Add(new Desire(DEFEND_BASE,64));
			
		}
		if(Desires.Contains(HELP_ATTACK))
		{
			if(!ZombiesAround())
				Desires_W.Add(new Desire(HELP_ATTACK,90));
			
		}
		if(Desires.Contains(ATTACK_ZOMBIE) )
		{
			/*if(intention == ATTACK_ZOMBIE){
				Desires_W.Add(new Desire(ATTACK_ZOMBIE,68));
				Debug.Log("tou a considerar atacar");
			} */
			if(ZombiesAround())
				Desires_W.Add(new Desire(ATTACK_ZOMBIE,66));
			
			
		}
		if(Desires.Contains(DEPOSIT))
		{
			if( lider_request_resources)
				Desires_W.Add(new Desire(DEPOSIT,57));
			else
				Desires_W.Add(new Desire(DEPOSIT,54));
			
			
		}
		if(Desires.Contains(COLLECT))
		{
			
			if(ResourcesAround())
				Desires_W.Add(new Desire(COLLECT,56));
			/*else
				Desires_W.Add(new Desire(COLLECT,51));*/
			
		}
		if(Desires.Contains(HELP_GO_BASE))
		{
			//Debug.Log("vou ajudar a ir para a base");
			Desires_W.Add(new Desire(HELP_GO_BASE,58));
			
			
		}
		//TODO: CONFIRMAR acho k n e preciso o lider fazer request para ele ir sozinho
		if(Desires.Contains(EXPLORE_SOLO))
		{	
			if( !IsInBase() && LevelResources() == EMPTY_LEVEL && resources_location == null
			   /** /
			   && lider_request_resources
			   /**/
			   )
				Desires_W.Add(new Desire(EXPLORE_SOLO,50));
			
		}
		if(Desires.Contains(GO_BASE))
		{
			if( resources_location == null)
				Desires_W.Add(new Desire(GO_BASE,52));
			
		}
		if(Desires.Contains(EXPLORE))
		{
			int random = Random.Range(1,5);
			if(random == 3 || random == 4 || random == 1 || random == 2){
				Desires_W.Add(new Desire(EXPLORE,2));
			}
			
			if(random == 5){
				Desires_W.Add(new Desire(EXPLORE_SOLO,2));
			}
		}
		if (Desires.Contains (ATTACK_ZOMBIE_AS_GROUP)) {
			if(_isInParty && ZombiesAround () && _survivorsInTeam.Count >= _minTeamMembers){
				Desires_W.Add(new Desire(ATTACK_ZOMBIE_AS_GROUP,100));
			}
		}
		
		else
			Desires_W.Add(new Desire(IDLE,1));
		
		
		int weight = -1;
		foreach (Desire d in Desires_W) {
			if ( d.getW() > weight){
				intention = d.getDesire();
				weight = d.getW();
			}
		}
		if (intention == HEAL) {
			intention_position = healPosition;
		}
		if (intention == DEFEND_BASE) {
			intention_position = getPositionInBase();
		}
		if (intention == FOLLOW_SURVIVOR) {
			intention_position = getSurvivorWithoutCritical().transform.position;
		}
		if (intention == HELP_ATTACK) {
			intention_position = anySurvivorAttacking();
		}
		if (intention == ATTACK_ZOMBIE) {
			intention_position = NearestZombiePosition();
		}
		if (intention == DEPOSIT) {
			intention_position = depositPosition;
		}
		if (intention == COLLECT) {
			if(ResourcesAround())
				intention_position = NearestResourcePosition();
			/*if(resources_location != null){
				intention_position = getNearestResourceMap();
			} */
			
			/*intention = COLLECT;

			else{
			if( IsInBase()){
					intention = EXPLORE;
					intention_position = Vector3.zero;
				}
				else{
					intention = EXPLORE_SOLO;
					intention_position = Vector3.zero;
				}
			} */
		}
		if (intention == HELP_GO_BASE) {
			intention_position = getPositionInBase();
		}
		if (intention == EXPLORE_SOLO) {
			
		}
		if (intention == GO_BASE) {
			intention_position = getPositionInBase();
		}
		if (intention == PATROL) {
			intention_position = getPositionInBase();
		}
		if (intention == EXPLORE) {
			intention_position = getPositionInBase();
		}
	}
	
	
	void Planner(){
		Plan.Clear ();
		if (intention == HEAL) {
			Plan.Add(MOVE_TO_PLAN);
			Plan.Add(HEAL_PLAN);
		}
		if (intention == DEFEND_BASE) {
			Plan.Add(MOVE_TO_PLAN);
			Plan.Add(ATTACK_PLAN);
		}
		if (intention == FOLLOW_SURVIVOR) {
			Plan.Add(MOVE_TO_PLAN);
		}
		if (intention == HELP_ATTACK) {
			Plan.Add(MOVE_TO_PLAN);
			Plan.Add(ATTACK_PLAN);
		}
		if (intention == ATTACK_ZOMBIE) {
			Plan.Add(ATTACK_PLAN);
		}
		if (intention == DEPOSIT) {
			Plan.Add(MOVE_TO_PLAN);
			Plan.Add(DEPOSIT_PLAN);
		}
		if (intention == COLLECT) {
			Plan.Add(MOVE_TO_PLAN);
			Plan.Add(COLLECT_PLAN);
		}
		if (intention == HELP_GO_BASE) {
			Plan.Add(MOVE_TO_PLAN);
		}
		if (intention == EXPLORE_SOLO) {
			Plan.Add(RANDOM_MOVE_TO_PLAN);
		}
		//TODO
		if (intention == GO_BASE) {
			Plan.Add(MOVE_TO_PLAN);
		}
		if (intention == PATROL) {
			Plan.Add(MOVE_TO_PLAN);
		}
		if (intention == EXPLORE) {
			Plan.Add(MOVE_TO_PLAN);
		}
		if (intention == ATTACK_ZOMBIE_AS_GROUP) {
			Plan.Add(ATTACK_PLAN_GROUP);
		}
	}
	
	void Execute( string step , Vector3 position){
		if (step == HEAL_PLAN) {
			Heal();
		}  else if (step == MOVE_TO_PLAN) {
			if(intention == FOLLOW_SURVIVOR){
				//Debug.Log("tou a mover para survivor");
				MoveTo(getSurvivorWithoutCritical().transform.position);
			}
			else
				MoveTo(position);
			
		}  else if (step == ATTACK_PLAN) {
			attackZombie(NearestZombie());
			
		}  else if (step == COLLECT_PLAN) { 
			CollectResources(NearestResource());
			
		}  else if (step == DEPOSIT_PLAN) {
			DepositResources();
			
		}  else  if (step == RANDOM_MOVE_TO_PLAN) {
			randomMove();
			
		}  else if (step == ATTACK_PLAN_GROUP){
			superAttack();
			_oneSuperStepExecuted = false;
			
		}else if (step == EXPLORE_PLAN_GROUP){
			superExplore();
			_oneSuperStepExecuted = false;
		}
		
	}
	private bool succeced(){
		if (intention == HEAL && LevelHealth() == FULL_LEVEL) {
			return true;
		}
		if (intention == DEFEND_BASE && !lider_request_defend) {
			return true;
		}
		if (intention == FOLLOW_SURVIVOR && IsInBase() ) {
			//Debug.Log("follow sucedded");
			return true;
		}
		if (intention == HELP_ATTACK && !ZombiesAround() && !isAnySurvivorAttacking()) {
			return true;
		}
		if (intention == ATTACK_ZOMBIE && !ZombiesAround()) {
			return true;
		}
		if (intention == ATTACK_ZOMBIE_AS_GROUP && !ZombiesAround()) {
			return true;
		}
		if (intention == DEPOSIT && LevelResources() == EMPTY_LEVEL) {
			return true;	
		}
		if (intention == COLLECT && (LevelResources() == FULL_LEVEL || !ResourcesAround())) {
			return true;
		}
		if (intention == HELP_GO_BASE && IsInBase()) {
			//Debug.Log ("tou na base sucedded");
			return true;
		}
		if (intention == EXPLORE_SOLO && LevelResources() == FULL_LEVEL) {
			//TODO: finish this, ben
			return true;
		}
		if (intention == EXPLORE && IsInBase()) {
			//if they went to explore, and returned home safely
			return true;
		}
		if (intention == GO_BASE && IsInBase()) {
			return true; 
		}
		if (intention == IDLE) {
			return true;
		}
		return false;
	}
	
	
	
	private bool impossible(){
		/*if (intention == HEAL && LevelHealth() == FULL_LEVEL) {
			return true;
		} */
		if (intention == DEFEND_BASE && !ZombiesAround()) {
			return true;
		}
		if (intention == FOLLOW_SURVIVOR && !isHereSurvivorWithoutCritical() ) {
			return true;
		}
		if (intention == HELP_ATTACK  && !isAnySurvivorAttacking() ) {
			return true;
		}
		
		if (intention == ATTACK_ZOMBIE && !ZombiesAround()) {
			return true;
		}
		
		if (intention == ATTACK_ZOMBIE_AS_GROUP && 
		    (!ZombiesAround() || !IsInParty() || _survivorsInTeam.Count <= _minTeamMembers) ){
			return true;
		}
		if (intention == EXPLORE_AS_GROUP && 
		    (!IsInParty() || _survivorsInTeam.Count <= _minTeamMembers) ){
			return true;
			//TODO: was finishing this!
		}
		
		/*
		if (intention == DEPOSIT && LevelResources() == EMPTY_LEVEL) {
			return true;
			
		}
		if (intention == COLLECT && (LevelResources() == FULL_LEVEL || !ResourcesAround())) {
			return true;
		} */
		if (intention == HELP_GO_BASE && !isHereSurvivorCritical()) {
			//Debug.Log ("tou na base sucedded");
			return true;
		}
		/*
		if (intention == EXPLORE_SOLO) {
			
		}
		if (intention == GO_BASE && IsInBase()) {
			return true; 
		}
		if (intention == EXPLORE ) {
			return true;
		}
		if (intention == IDLE) {
			return true;
		}
		*/
		
		return false;
		
	}
	private bool checkStepCompleted(){
		if (Plan[0] == HEAL_PLAN && LevelHealth() == FULL_LEVEL) {
			return true;
		}
		if (Plan[0] == MOVE_TO_PLAN && (intention_position - transform.position).magnitude < 4.0f ) {
			//TODO: delete this, debug
			Debug.Log("Finished move to plan instruction");
			return true;
			
		}  
		if (Plan[0] == ATTACK_PLAN && !ZombiesAround()) {
			return true;
			
		}  
		if (Plan[0] == COLLECT_PLAN && (!ResourcesAround() || LevelResources() == FULL_LEVEL)) { 
			//TODO: delete this, debug
			Debug.Log("Finished collect instruction");
			return true;
		}  
		if (Plan[0] == DEPOSIT_PLAN && LevelResources() == EMPTY_LEVEL ) {
			//TODO: delete this, debug
			Debug.Log("Finished deposit instruction");
			return true;
			
		}  
		if (Plan[0] == RANDOM_MOVE_TO_PLAN) {
			return true;
		}
		
		if (Plan[0] == ATTACK_PLAN_GROUP && _oneSuperStepExecuted) {
			return true;
		}
		
		return false;
		
	}
	private bool reconsider(){
		
		if (intention == HEAL && LevelHealth() == FULL_LEVEL) {
			return true;
		}
		/*
		if (intention == DEFEND_BASE && !ZombiesArounc()) {
			return true;
		} */
		if (intention == FOLLOW_SURVIVOR && IsInBase() ) {
			return true;
		}
		/*if (intention == HELP_ATTACK && !ZombiesAround() && !isAnySurvivorAttacking()) {
			return true;
		} */
		if (intention == ATTACK_ZOMBIE && LevelHealth() == CRITICAL_LEVEL) {
			return true;
		}
		
		if (intention == DEPOSIT && (ZombiesAround() || (ResourcesAround()&& LevelResources() != FULL_LEVEL))) {
			return true;
			
		}
		
		if (intention == COLLECT && ZombiesAround()) {
			return true;
		}
		if (intention == HELP_GO_BASE && ZombiesAround()) {
			//Debug.Log ("tou na base dps help");
			return true;
		}
		/*
		if (intention == EXPLORE_SOLO) {
			
		}
		if (intention == GO_BASE && IsInBase()) {
			return true; 
		}

		if (intention == EXPLORE && IsInBase()) {
			return true;
		}
		if (intention == IDLE) {
			return true;
		}
		*/
		
		return false;
		
	}
	
	/// ////////
	/// Coliders
	/// ////////
	private bool showDebug = false;
	
	void OnTriggerEnter (Collider other) {
		
		if (other.tag.Equals("Survivor") && !other.transform.root.Equals(transform.root)){
			_survivorsInSight.Add(other.gameObject);
			
			//TODO: debug, this is hardcoded way to create a team
			if(!_survivorsInTeam.Contains(other.gameObject)){
				_survivorsInTeam.Add(other.gameObject);
			}
			
			if(showDebug){
				Debug.Log(name + "-New Survivor " + other.name);
				Debug.Log("#Survivors in range: " + _survivorsInSight.Count);}
		}
		if (other.tag.Equals("Zombie")){
			_zombiesInSight.Add(other.gameObject);
			
			if(showDebug){
				Debug.Log(name + "-New Zombie " + other.name);
				Debug.Log("#Zombies in range: " + _zombiesInSight.Count);}
		}
		if (other.tag.Equals("Resources")){
			_resourcesInSight.Add(other.gameObject);
			
			if(showDebug){
				Debug.Log(name + "-New Resources " + other.name);
				Debug.Log("#Resources in range: " + _resourcesInSight.Count);}
		}
		if (other.tag.Equals("Heal")){
			healPosition = other.transform.position;
			healInRange = true;
			if(showDebug){
				Debug.Log(name + "-New heal " + other.name);
			}
		}
		if (other.tag.Equals("Deposit")){
			depositPosition = other.transform.position;
			depositInRange = true;
			if(showDebug){
				Debug.Log(name + "-New deposit " + other.name);
			}
		}
	}
	
	void OnTriggerExit (Collider other){
		if (other.tag.Equals("Survivor") && !other.transform.root.Equals(transform.root)){
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
			_dead = true;
			Debug.Log(name + " died.");
			
			sayMyLastWords();
			
			if(_isPartyLeader){
				passKnowledgeToNextLeader();
			}
			
			//to make it "disappear"
			
			Instantiate(Resources.Load(@"Models/Characters/Zombie"), transform.position, transform.rotation);
			StartCoroutine("destroyAfterDeath");
		}
	}
	
	private IEnumerator destroyAfterDeath(){
		yield return new WaitForSeconds(3.0F);
		//Debug.Log("Destroyed: "+ name);
		Destroy(gameObject);
	}
	
	
	private void checkImpossiblePathAndReset(){//Calculates a new setDestination in case the previous calc isnt reached in a set reset time
		timeWindow -= Time.deltaTime;
		if(timeWindow < 0){
			//Debug.Log("Reset needed by :" + name);
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
			currentScreenPos = Camera.main.WorldToScreenPoint(transform.position);
			if(showInfo){
				//Survivors's Information Box
				
				if(renderer.isVisible){
					GUI.Box(new Rect(currentScreenPos.x, Screen.height - currentScreenPos.y, infoBoxWidth, infoBoxHeight),
					        name + ": \n" +
					        "State: " + _state +
					        " \n" +
					        "Resources: " + _resourceLevel +
					        " \n" +
					        "Intention: " + intention + 
					        " \n" +
					        "Step: " + Plan[0] + 
					        " \n");
				}
			}
			
			if(renderer.isVisible){
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
			
			//Debug.Log ( "Desires: " + Desires);
			//Debug.Log ( "intention: " + intention);
			//Debug.Log ( "intention: " + intention_position);
			//Debug.Log ( "state: " + _state);
			
			if(out_of_while){
				Options();
				filter();
				Planner();
				
				//Debug.Log("tou no execute: " + Plan.Count());
				out_of_while = false;
			}
			else{
				
				if(Plan.Count() != 0 && !succeced() && !impossible())
				{
					if(checkStepCompleted()){
						Plan.RemoveAt(0);
					}
					else
						Execute(Plan[0],intention_position);
					
					if(reconsider()){
						Debug.Log("tou a reconsiderar");
						Options();
						filter();
						Planner();
					}
				}
				else{
					out_of_while = true;
				}
				
			}
			
			//DO NOT DELETE This forces collision updates in every frame
			transform.root.gameObject.transform.position += new Vector3(0.0f, 0.0f, -0.00001f);
			
			/**/
		}else{
			renderer.material = transparentMaterial;
			Destroy(GetComponent<NavMeshAgent>());
			rigidbody.AddForce(0,100.0f,0, ForceMode.Force);
		}
	}
}
