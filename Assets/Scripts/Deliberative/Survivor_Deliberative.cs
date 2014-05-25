using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Survivor_Deliberative : MonoBehaviour {

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

    //private bool _isCollecting;

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
	private bool _zombsAroundParty = false;

    private bool _movingEvade = false;

    private bool deliberatingNextZombieTarget;


    // estado interno
    private List<GameObject> resources_location;
    private List<GameObject> resources_depleted;

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
    private const string EXPLORE_SOLO = "explore_solo";
    private const string PATROL = "patrol";
    private const string GO_BASE = "go_base";
    private const string EXPLORE_AS_GROUP = "explore_as_group";
    private const string JOIN_PARTY = "join_party";
	

    private bool out_of_while;
    private string instructionName = "";
	private string _previousIntention = "";


    private const string HEAL_PLAN = "heal_plan";
    private const string MOVE_TO_PLAN = "move_to";
    private const string ATTACK_PLAN = "attack_plan";
    private const string COLLECT_PLAN = "collect_plan";
    private const string DEPOSIT_PLAN = "deposit_plan";
    private const string RANDOM_MOVE_TO_PLAN = "random_move";
    private const string SHARE_PLAN = "share";
    private const string ATTACK_PLAN_GROUP = "superAttack";
    private const string EXPLORE_PLAN_GROUP = "superExplore";

    private const string JOIN_PARTY_PLAN = "joinParty";

    private bool share_end = false;

    private bool lider_request_defend;
    private bool lider_request_resources;


    public bool _hasMap = false;

    public int[][] _explorerMap;
    private bool transmitedMap; //transmit map one time when in base

    private int _exploredPoints;
	private Vector3 _nextPoint2Explore;
	private bool _reachedNextPoint2Explore = true;

    private GameObject mapObj;

    private GameObject BaseLider;

    private float infoBoxWidth = 150.0f;
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

    void Start()
    {
		_survivorsInTeam = new List<GameObject>();
        mapObj = GameObject.Find("Map");

        //TODO: debug value, was full
        _healthLevel = 100.0f;
        _movSpeed = 8.0f;
        _visionRange = 20.0f;
        _attDamage = 25.0f;
        _attRange = 5.0f;
        _resourceLevel = 50.0f;

        _zombiesInSight = new List<GameObject>();
        _survivorsInSight = new List<GameObject>();
        _resourcesInSight = new List<GameObject>();
        //_isCollecting = false;
        _state = IDLE;
        _isReloading = false;
        _isReloadingCollect = false;
        out_of_while = true;

        //TODO: Deliberativo
        _minTeamMembers = 2;

        Desires = new List<string>();
        Plan = new List<string>();
        resources_location = new List<GameObject>();
        resources_depleted = new List<GameObject>();
        Desires_W = new List<Desire>();
        intention = IDLE;
        intention_position = Vector3.zero;
        lider_request_defend = false;
        lider_request_resources = false;

        //StartCoroutine("CicloBDI");

        _dead = false;

        navMeshComp = GetComponent<NavMeshAgent>();

        BaseLider = GameObject.FindWithTag("BaseLeader");
        healPosition = GameObject.FindWithTag("Heal").transform.position;
        depositPosition = GameObject.FindWithTag("Deposit").transform.position;

        SphereCollider visionRangeCollider = this.gameObject.GetComponentInChildren<SphereCollider>();
        if (visionRangeCollider != null)
        {
            visionRangeCollider.radius = _visionRange;
        }
        else
        {
            Debug.Log("Missing sphere collider");
        }

        showInfo = false;
        reloading_mat = (Material)Resources.Load(@"Materials/Reloading", typeof(Material));
        survivor_mat = (Material)Resources.Load(@"Materials/Survivor_Test_Material", typeof(Material));
        
        life_bar_green = (Texture2D)Resources.Load(@"Textures/life_bar_green", typeof(Texture2D));
        life_bar_red = (Texture2D)Resources.Load(@"Textures/life_bar_red", typeof(Texture2D));
        transparentMaterial = (Material)Resources.Load(@"Materials/Transparent", typeof(Material));

        lifebar_lenght = 30.0f;
        lifebar_height = 4.0f;
        lifebar_x_offset = -15.0f;
        lifebar_y_offset = -8.0f;

        timeWindow = PATH_RESET_TIME;
        CurrentDestination = this.transform.position;
        navMeshComp.speed = _movSpeed;


        _explorerMap = new int[Map.MAP_WIDTH][];
        for (int i = 0; i < Map.MAP_WIDTH; i++)
        {
            _explorerMap[i] = new int[Map.MAP_HEIGHT];
            for (int j = 0; j < Map.MAP_HEIGHT; j++)
            {
                _explorerMap[i][j] = Map.MAP_EMPTY_POS;
            }

        }


        _isInParty = false;
        _isPartyLeader = false;
    }

    //Actuadores-------------------------------------------------------------------

    private void attackZombie(GameObject zombie)
    {
        _state = ATTACKING;
       // navMeshComp.SetDestination(nearestZombie.transform.position);
        /** /
        float _dist2Zombie = Vector3.Distance(nearestZombie.transform.position, this.transform.position);
        
        if (!_isReloading && _dist2Zombie <= _attRange)
        {
            StartCoroutine(attackClosestZombie(nearestZombie));
        }
        /**/
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
        /**/
    }

    IEnumerator attackClosestZombie(GameObject nearestZombie)
    {
        _isReloading = true;
        nearestZombie.GetComponent<ZombieScript>().loseHealth(_attDamage);
        renderer.material = reloading_mat;
        yield return new WaitForSeconds(RELOAD_SPEED_ATTACK);
        _isReloading = false;
        renderer.material = survivor_mat;
    }

    //Collect-Resources
    private void CollectResources(GameObject nearestResource)
    {

        float _dist2Resource;

        _state = COLLECTING;
        navMeshComp.SetDestination(nearestResource.transform.position);
        _dist2Resource = Vector3.Distance(nearestResource.transform.position, this.transform.position);
        if (!_isReloadingCollect && _dist2Resource <= PICKUP_RANGE)
        {
            StartCoroutine(collectDResource(nearestResource));
        }
    }

    IEnumerator collectDResource(GameObject nearestResource)
    {
        _isReloadingCollect = true;
        float val;
        bool destroy;
        destroy = nearestResource.GetComponent<ResourcesScript>().catchResources(out val);
        _resourceLevel += val;
        if (destroy)
            removeResourceMap(nearestResource);
        yield return new WaitForSeconds(RELOAD_SPEED_COLLECT);
        _isReloadingCollect = false;
    }

    //Deposit-Resources
    private void DepositResources()
    {
        _state = DEPOSITING;
        navMeshComp.SetDestination(depositPosition);

        if ((depositPosition - transform.position).magnitude < 5)
        {
            BaseLider.GetComponent<BaseLeaderScript>().addResources(_resourceLevel);
            //Debug.Log("Deposited!");
            _resourceLevel = 0;
            //navMeshComp.Stop();
        }
    }
    // Heal
    private void Heal()
    {


        _state = HEALING;
        navMeshComp.SetDestination(healPosition);
        if ((healPosition - transform.position).magnitude < 4)
        {
            _healthLevel = FULL_HEALTH;
            //navMeshComp.Stop();
        }


    }
    //Move-to
    private void MoveTo(Vector3 position)
    {
        _state = MOVINGTO;
        if (intention == ATTACK_ZOMBIE)
            _state = ATTACKING;
        navMeshComp.SetDestination(position);
    }

    //Random-Move
    private void randomMove()
    {
        /**/
        if (!_state.Equals(IDLE))
        {
            CurrentDestination = this.transform.position;
            _state = IDLE;
        }

        if ((CurrentDestination - transform.position).magnitude < 2.0f)
        {

            CurrentDestination = new Vector3(transform.position.x + Random.Range(-40.0f, 40.0f)
                                              , transform.position.y,
                                              transform.position.z + Random.Range(-40.0f, 40.0f));
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
    public bool IsInBase()
    {
        if (this.transform.position.x > _TopLeftBase.position.x &&
            this.transform.position.x < _BottomRightBase.position.x &&
            this.transform.position.z > _TopLeftBase.position.z &&
            this.transform.position.z < _BottomRightBase.position.z
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
    private bool ResourcesAround()
    {
        if (_resourcesInSight.Count > 0)
            return true;
        else
            return false;
    }
    //Survivors-Around?
    private bool SurvivorsAround()
    {
        return _survivorsInSight.Count > 0 ? true : false;
    }
    //Zombies-Around?
    public bool ZombiesAround()
    {
        if (_zombiesInSight.Count > 0)
            return true;
        else
            return false;
    }

    //TODO:finishing this    
	private bool ZombiesAroundParty()
	{
		if(ZombiesAround()){
			_zombsAroundParty = true;
			return true;
		}
		foreach(GameObject o in _survivorsInTeam){
			if(o.GetComponent<Survivor_Deliberative>().ZombiesAround()){
				_zombsAroundParty = true;
				return true;
			}
		}
		return false;
	}
    
    //Heal-InRange?
    private bool HealInRange()
    {
        if (healInRange)
            return true;
        else
            return false;
    }
    //Deposit-InRange?
    private bool DepositInRange()
    {
        if (depositInRange)
            return true;
        else
            return false;
    }
    //Nearest-Survivor-Position
    private Vector3 NearestSurvivorPosition()
    {
        GameObject _closestSurvivor = null;

        foreach (GameObject survivor in _survivorsInSight)
        {
            if (_closestSurvivor == null)
            {
                _closestSurvivor = survivor;
            }
            else
            {
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
    private GameObject NearestSurvivor()
    {
        GameObject _closestSurvivor = null;

        foreach (GameObject survivor in _survivorsInSight)
        {
            if (_closestSurvivor == null)
            {
                _closestSurvivor = survivor;
            }
            else
            {
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
    private Vector3 NearestZombiePosition()
    {
        GameObject _closestZombie = null;

        foreach (GameObject zombie in _zombiesInSight)
        {
            if (_closestZombie == null)
            {
                _closestZombie = zombie;
            }
            else
            {
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
    private GameObject NearestZombie()
    {
        GameObject _closestZombie = null;

        foreach (GameObject zombie in _zombiesInSight)
        {
            if (_closestZombie == null)
            {
                _closestZombie = zombie;
            }
            else
            {
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
    private Vector3 NearestResourcePosition()
    {
        GameObject _closestResource = null;

        foreach (GameObject resource in _resourcesInSight)
        {
            if (_closestResource == null)
            {
                _closestResource = resource;
            }
            else
            {
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
    private GameObject NearestResource()
    {
        GameObject _closestResource = null;

        foreach (GameObject resource in _resourcesInSight)
        {
            if (_closestResource == null)
            {
                _closestResource = resource;
            }
            else
            {
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
    private string SurvivorAction(GameObject survivor)
    {
        return survivor.GetComponent<Survivor_Deliberative>().getState();

    }
    //Any-Survivor-Need-Help-Collecting
    private Vector3 anySurvivorCollecting()
    {
        GameObject survivorInNeed = null;
        foreach (GameObject survivor in _survivorsInSight)
        { //check each survivor around to see if they are collecting
            if (survivor.GetComponent<Survivor_Deliberative>().getState().Equals(COLLECTING))
            {
                if (survivorInNeed == null)
                {
                    survivorInNeed = survivor;
                }
                else
                { //chooses the closest survivor to him that is collecting
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
    private Vector3 anySurvivorAttacking()
    {
        GameObject survivorInNeed = null;
        foreach (GameObject survivor in _survivorsInSight)
        { //check each survivor around to see if they are attacking
            if (survivor.GetComponent<Survivor_Deliberative>().intention.Equals(ATTACK_ZOMBIE))
            {
                if (survivorInNeed == null)
                {
                    survivorInNeed = survivor;
                }
                else
                { //chooses the closest survivor to him that is attacking
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

    private bool isAnySurvivorAttacking()
    {
        GameObject survivorInNeed = null;
        foreach (GameObject survivor in _survivorsInSight)
        { //check each survivor around to see if they are attacking
            if (survivor.GetComponent<Survivor_Deliberative>().intention.Equals(ATTACK_ZOMBIE))
            {
                if (survivorInNeed == null)
                {
                    survivorInNeed = survivor;
                }
                else
                { //chooses the closest survivor to him that is attacking
                    if (Vector3.Distance(survivorInNeed.transform.position, this.transform.position) >
                        Vector3.Distance(survivor.transform.position, this.transform.position))
                    {
                        survivorInNeed = survivor;
                    }
                }
            }
        }
        if (survivorInNeed == null)
        {
            return false;
        }
        else
            return true;
    }

    private bool isAnySurvivorCollecting()
    {
        GameObject survivorInNeed = null;
        foreach (GameObject survivor in _survivorsInSight)
        { //check each survivor around to see if they are collecting
            if (survivor.GetComponent<Survivor_Deliberative>().getState().Equals(COLLECTING))
            {
                if (survivorInNeed == null)
                {
                    survivorInNeed = survivor;
                }
                else
                { //chooses the closest survivor to him that is collecting
                    if (Vector3.Distance(survivorInNeed.transform.position, this.transform.position) >
                        Vector3.Distance(survivor.transform.position, this.transform.position))
                    {
                        survivorInNeed = survivor;
                    }
                }
            }
        }
        if (survivorInNeed == null)
        {
            return false;
        }
        else
            return true;
    }
    //TODO
    ///////////////////
    //Delibarativo
    ///

    public class Desire
    {
        public string des;
        public int weight;

        public Desire(string newDesire, int newWeight)
        {
            des = newDesire;
            weight = newWeight;
        }
        public string getDesire()
        {
            return des;
        }
        public int getW()
        {
            return weight;
        }

    }

    //-------------------------------------------------------------------------------------
    public void addResourceMap(GameObject resource)
    {
        if (!resources_location.Contains(resource))
            resources_location.Add(resource);
    }

    public void removeResourceMap(GameObject resource)
    {
        if (!resources_depleted.Contains(resource))
        {
            resources_location.Remove(resource);
            resources_depleted.Add(resource);
        }
    }



    public Vector3 getNearestResourceMap()
    {
        float dist = Mathf.Infinity;

        Vector3 posB = Vector3.zero;
        foreach (GameObject res in resources_location)
        {
            if (dist > Vector3.Distance(this.transform.position, res.transform.position))
            {
                dist = Vector3.Distance(this.transform.position, res.transform.position);
                posB = res.transform.position;
            }
        }

        return posB;
    }

    public void UpdateResourceMap(List<GameObject> resource_map, List<GameObject> resource_map_depleted)
    {

        resources_location = Enumerable.Union<GameObject>(resources_location, resource_map).ToList();
        resources_depleted = Enumerable.Union<GameObject>(resources_depleted, resource_map_depleted).ToList();

        foreach (GameObject resToDelete in resources_depleted)
        {
            resources_location.Remove(resToDelete);
        }
    }




    private Vector3 getPositionInBase()
    {
        return new Vector3(Random.Range(_TopLeftBase.position.x, _BottomRightBase.position.x + 1),
                           transform.position.y, Random.Range(_TopLeftBase.position.z, _BottomRightBase.position.z + 1));

    }


    private GameObject getSurvivorWithoutCritical()
    {


        foreach (GameObject sur in _survivorsInSight)
        {
            if (sur.GetComponent<Survivor_Deliberative>().LevelHealth() != CRITICAL_LEVEL)
            {
                return sur;
            }
        }
        return null;

    }

    private bool isHereSurvivorWithoutCritical()
    {


        foreach (GameObject sur in _survivorsInSight)
        {
            if (sur.GetComponent<Survivor_Deliberative>().LevelHealth() != CRITICAL_LEVEL)
            {
                return true;
            }
        }
        return false;

    }



    private bool isHereSurvivorCritical()
    {


        foreach (GameObject sur in _survivorsInSight)
        {
            if (sur.GetComponent<Survivor_Deliberative>().LevelHealth() == CRITICAL_LEVEL)
            {
                return true;
            }
        }
        return false;

    }

    private void informTeamThatImTheLeader(){
		//Debug.Log ("I was given a team of " + _survivorsInTeam.Count + " people.");
		string names = "";
		foreach (GameObject newSurvivor in _survivorsInTeam) {
			names += newSurvivor.name + " ";
            newSurvivor.GetComponent<Survivor_Deliberative>()._partyLeader = gameObject;
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
            _nextPartyLeader.GetComponent<Survivor_Deliberative>().receivePreviousTeamLeaderKnowledge(_survivorsInTeam);
		}else{
			_nextPartyLeader = healthiestSurvivorInTeamNotTanking();
            _nextPartyLeader.GetComponent<Survivor_Deliberative>().receivePreviousTeamLeaderKnowledge(_survivorsInTeam);
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
            float newSurvivorHealth = newSurvivor.GetComponent<Survivor_Deliberative>()._healthLevel;
            isTanking = newSurvivor.GetComponent<Survivor_Deliberative>()._isTank;
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
            float newSurvivorHealth = newSurvivor.GetComponent<Survivor_Deliberative>()._healthLevel;
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
		_partyLeader.GetComponent<Survivor_Deliberative> ().reRollTank (gameObject);
		_isTank = false;
	}
	
	//called by previous tank, this will force the choosing of a new one
	public void reRollTank (GameObject previousTank){
		if(_tank.Equals(previousTank)){
			_tank = null;
		}
	}
	
	private void requestNewTargetForTeam(){
		_partyLeader.GetComponent<Survivor_Deliberative> ().deliberateNextZombieTarget ();
	}
	
	private void getNewTeamTarget(){
        _partyLeader.GetComponent<Survivor_Deliberative>().checkDeliberateNextZombieTarget();
	}
	
  	public void checkDeliberateNextZombieTarget(){
		if (_focusTargetZombie != null) {
			foreach (GameObject survivorInTeam in _survivorsInTeam) {
                survivorInTeam.GetComponent<Survivor_Deliberative>()._focusTargetZombie = _focusTargetZombie;
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
                tempZombie = survivorInTeam.GetComponent<Survivor_Deliberative>().NearestZombie();
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
                    survivorInTeam.GetComponent<Survivor_Deliberative>()._focusTargetZombie = nextZombie2Focus;
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
        _partyLeader.GetComponent<Survivor_Deliberative>().ISeeDeadPeople(gameObject);
	}
	
	private void updateTeam(){
		foreach (GameObject survivorInMyteam in _survivorsInTeam) {
			survivorInMyteam.GetComponent<Survivor_Deliberative>()._survivorsInTeam = _survivorsInTeam;
		}
	}
	
	public void ISeeDeadPeople(GameObject deadGuyWalking){
		_survivorsInTeam.Remove (deadGuyWalking);
		updateTeam();
		if (deadGuyWalking.Equals (_nextPartyLeader)) {
			_nextPartyLeader = healthiestSurvivorInTeamNotTanking();
		}
	}
	

	private void groupsTotalResources(){
		float total = 0.0f;
		foreach(GameObject s in _survivorsInTeam){
            total += s.GetComponent<Survivor_Deliberative>()._resourceLevel;
		}
	}

	private void joinParty(){
		BaseLider.GetComponent<BaseLeaderScript>().QueueFutureTeamMember(gameObject);
	}

	public void disbandParty(){
		_isInParty = false;
	}

	private void disbandWholeParty(){
		_isInParty = false;
		_isPartyLeader = false;
		foreach(GameObject teamMember in _survivorsInTeam){
			teamMember.GetComponent<Survivor_Deliberative>().disbandParty();
		}
	}

	private void superExplore(){

        if (_isPartyLeader)
        {
			if(_reachedNextPoint2Explore){
				_nextPoint2Explore = unexploredPosition();
				_reachedNextPoint2Explore = false;
			}

			if (_exploredPoints > 0){
	            if (Vector3.Distance(_nextPoint2Explore, transform.position) < 1){
					Debug.Log("and im near an objective! ");
					_exploredPoints--;
					_reachedNextPoint2Explore = true;
	            }else{
					MoveTo(_nextPoint2Explore);
                }
            }else{
				MoveTo(getPositionInBase());
				//Debug.Log("And it's time to go home!");
            }
        }
        else
        {
			//Debug.Log("moving to "+_partyLeader.transform.position);
			navMeshComp.SetDestination(_partyLeader.transform.position);
        }
		Plan.Add (EXPLORE_PLAN_GROUP);
    }
	
	
	private void superAttack(){
		//execute do plan(Intencao att grupo)
		if (_isPartyLeader) {
			if (_tank == null) {
				_tank = healthiestSurvivorInTeam ();
				if (_tank != null) {
                    _tank.GetComponent<Survivor_Deliberative>().becomeTank();
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
					//sphere.position = _focusTargetZombie.transform.position;
					
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
		}  else if (ZombiesAround () && LevelHealth () == CRITICAL_LEVEL && !_isSafe) {	
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
		Plan.Add ("superAttack");
    }

    private Vector3 unexploredPosition()
    {
		/** /
        Vector3 dest = new Vector3(transform.position.x + Random.Range(-200.0f, 200.0f)
                       , transform.position.y,
                       transform.position.z + Random.Range(-200.0f, 200.0f));
        int i = 0;
        while (getMapPositionInfoExplored(dest) && i < 5)
        {
            i++;
            dest = new Vector3(transform.position.x + Random.Range(-200.0f, 200.0f)
               , transform.position.y,
               transform.position.z + Random.Range(-200.0f, 200.0f));
        }

        NavMeshHit hit;

        navMeshComp.Raycast(dest, out hit);
        dest = hit.position;

        

        //Debug.Log(name + " * " + dest);
        return dest;
        
        /**/
		//TODO: debug section, delete afterwards
		Vector3 pos1 = new Vector3(60.0f,17.3f,120);
		Vector3 pos2 = new Vector3(90.0f,17.3f,150);

		if(_nextPoint2Explore.Equals(pos1)){
			return pos2;
		}else{
			return pos1;
		}
    }


    private void share()
    {
        if (IsInBase() && !transmitedMap)
        {
            UpdateMyMap(BaseLider.GetComponent<BaseLeaderScript>().TransmitMap(_explorerMap));
            transmitedMap = true;
        }
        else if (!IsInBase() && transmitedMap)
        {
            transmitedMap = false;
        }
        share_end = true;
    }



    void Options()
    {
        Desires.Clear();
        //Desires.Add (HELP_ATTACK);
        if (LevelHealth() != FULL_LEVEL)
            Desires.Add(HEAL);
        //variavel

        if (lider_request_defend)
            Desires.Add(DEFEND_BASE);
        // fixo
        if (isAnySurvivorAttacking() && !ZombiesAround())
        {
            Desires.Add(HELP_ATTACK);
            //fixo
            //Debug.Log("tenho o desejo de ajudar");
        }
        if (ZombiesAround() && !IsInParty())
        {
            Desires.Add(ATTACK_ZOMBIE);

        }
        if (LevelResources() != EMPTY_LEVEL)
            Desires.Add(DEPOSIT);
        //variavel


        if ((ResourcesAround() && LevelResources() != FULL_LEVEL) || (resources_location.Count > 0 && LevelResources() != FULL_LEVEL))

            //(lider_request_resources && LevelResources() == EMPTY_LEVEL) || ResourcesAround() || resources_location != null) && LevelResources() != FULL_LEVEL)
            Desires.Add(COLLECT);
        //variavel

        if (LevelHealth() == CRITICAL_LEVEL && SurvivorsAround() && isHereSurvivorWithoutCritical() && !IsInBase())
            Desires.Add(FOLLOW_SURVIVOR);
        //variavel

        if (LevelHealth() != CRITICAL_LEVEL && isHereSurvivorCritical() && !IsInBase())
            Desires.Add(HELP_GO_BASE);
        //fixo

        //TODO fazer mais tarde
        if (resources_location.Count() == 0 && !IsInParty())
            Desires.Add(GO_BASE);
        //fixo

        /*//TODO nao tenho de fazer!
        if(IsInBase() && intention == IDLE)  	
            Desires.Add (EXPLORE);*/
        if(!IsInParty() && IsInBase()){
			Desires.Add (JOIN_PARTY);
		}
		
        if ((!IsInParty() && !ResourcesAround() && LevelResources() != FULL_LEVEL) || (resources_location.Count == 0 && LevelResources() != FULL_LEVEL && !IsInParty()))
            Desires.Add(EXPLORE_SOLO);

		if(_isInParty && ZombiesAround()){
			Desires.Add (ATTACK_ZOMBIE_AS_GROUP);
		}

		if(_isInParty && !ZombiesAround() && !ResourcesAround()){
            Desires.Add(EXPLORE_AS_GROUP);
		}

        Desires.Add(IDLE);

    }


    void filter()
    {

        Desires_W = new List<Desire>();


        if (Desires.Contains(HEAL))
        {
            if (LevelHealth() == CRITICAL_LEVEL)
                Desires_W.Add(new Desire(HEAL, 70));
            if (LevelHealth() == NORMAL_LEVEL)
                Desires_W.Add(new Desire(HEAL, 46));


        }
        if (Desires.Contains(FOLLOW_SURVIVOR))
        {
            //if (SurvivorsAround())
                Desires_W.Add(new Desire(FOLLOW_SURVIVOR, 80));

        }
        if (Desires.Contains(DEFEND_BASE))
        {

            Desires_W.Add(new Desire(DEFEND_BASE, 64));

        }
        if (Desires.Contains(HELP_ATTACK))
        {
            //if (!ZombiesAround())
                Desires_W.Add(new Desire(HELP_ATTACK, 90));

        }
        if (Desires.Contains(ATTACK_ZOMBIE))
        {
            /*if(intention == ATTACK_ZOMBIE){
                Desires_W.Add(new Desire(ATTACK_ZOMBIE,68));
                Debug.Log("tou a considerar atacar");
            }*/
            if (ZombiesAround())
                Desires_W.Add(new Desire(ATTACK_ZOMBIE, 66));
        }

        if (Desires.Contains(DEPOSIT))
        {
            if (lider_request_resources)
                Desires_W.Add(new Desire(DEPOSIT, 57));
            else
                Desires_W.Add(new Desire(DEPOSIT, 54));
        }

        if (Desires.Contains(COLLECT))
        {
            //TODO verificar pesos do collect longe e deposit primeiro
            if (ResourcesAround())
                Desires_W.Add(new Desire(COLLECT, 56));
            else
                Desires_W.Add(new Desire(COLLECT, 51));
        }

        if (Desires.Contains(HELP_GO_BASE))
        {
            //Debug.Log("vou ajudar a ir para a base");
            Desires_W.Add(new Desire(HELP_GO_BASE, 58));
        }

        if (Desires.Contains(EXPLORE_AS_GROUP))
		{
            Desires_W.Add(new Desire(EXPLORE_AS_GROUP, 49));
		}

		if (Desires.Contains (ATTACK_ZOMBIE_AS_GROUP)) {
				Desires_W.Add(new Desire(ATTACK_ZOMBIE_AS_GROUP,101));
		}
		if (Desires.Contains (JOIN_PARTY)) {

			if((Random.Range(0,6)) == 1){
			//TODO: decide what the weight is. if true, o peso e maior que o solo, se false, tem que ser um weight mais pequeno que solo
				Desires_W.Add(new Desire(EXPLORE_SOLO,48));
			}else{
				Desires_W.Add(new Desire(JOIN_PARTY,100));
			}
		}

        if (Desires.Contains(EXPLORE_SOLO))
        {
            Desires_W.Add(new Desire(EXPLORE_SOLO, 48));

        }

        if (Desires.Contains(GO_BASE))
        {
            int random = Random.Range(0, 2);
            if (random == 0)
                Desires_W.Add(new Desire(GO_BASE, 50));
            if (random == 1)
                Desires_W.Add(new Desire(GO_BASE, 47));
        }

        else
            Desires_W.Add(new Desire(IDLE, 1));


        int weight = -1;
        foreach (Desire d in Desires_W)
        {
            if (d.getW() > weight)
            {
                intention = d.getDesire();
                weight = d.getW();
            }
        }
        if (intention == HEAL)
        {
            intention_position = healPosition;
        }
        if (intention == DEFEND_BASE)
        {
            intention_position = getPositionInBase();
        }
        if (intention == FOLLOW_SURVIVOR)
        {
            intention_position = getSurvivorWithoutCritical().transform.position;
        }
        if (intention == HELP_ATTACK)
        {
            intention_position = anySurvivorAttacking();
        }
        if (intention == ATTACK_ZOMBIE)
        {
            intention_position = NearestZombiePosition();
        }
        if (intention == DEPOSIT)
        {
            intention_position = depositPosition;
        }
        if (intention == COLLECT)
        {
		    if (ResourcesAround())
		    {
		        intention_position = NearestResourcePosition();
		    }
		    else if (resources_location.Count() > 0)
		    {
		        intention_position = getNearestResourceMap();
		    }
			_previousIntention = "collect";
        }
        if (intention == HELP_GO_BASE)
        {
            intention_position = getPositionInBase();
        }
        if (intention == EXPLORE_SOLO)
        {
			Debug.Log("Going solo");
            intention_position = unexploredPosition();
        }
        if (intention == GO_BASE)
        {
            intention_position = getPositionInBase();
        }
		if (intention == GO_BASE)
		{
			intention_position = getPositionInBase();
		}
		if (intention == ATTACK_ZOMBIE_AS_GROUP)
		{	
			_isTank = false;
			_tank = null;
			_zombsAroundParty = false;
			_nextPartyLeader = null;
			_firstTimeTanking = true;
			_isSafe = false;
			deliberatingNextZombieTarget = false;
			_previousIntention = "groupAttack";
		}

        if (intention == EXPLORE_AS_GROUP)
        {
			if(!_previousIntention.Equals("groupAttack")|| !_previousIntention.Equals("collect")){
				//TODO:set this as const
				_exploredPoints = 4;
				_reachedNextPoint2Explore = true;
				_previousIntention = "";
			}else{
				Debug.Log("came from attack");
			}
        }

    }

    void Planner()
    {
        Plan.Clear();
        if (intention == HEAL)
        {
            Plan.Add(MOVE_TO_PLAN);
            Plan.Add(HEAL_PLAN);
        }
        if (intention == DEFEND_BASE)
        {
            Plan.Add(MOVE_TO_PLAN);
            Plan.Add(ATTACK_PLAN);
        }
        if (intention == FOLLOW_SURVIVOR)
        {
            Plan.Add(MOVE_TO_PLAN);
        }
        if (intention == HELP_ATTACK)
        {
            Plan.Add(MOVE_TO_PLAN);
            Plan.Add(ATTACK_PLAN);
        }
        if (intention == ATTACK_ZOMBIE)
        {
            Plan.Add(ATTACK_PLAN);
        }
        if (intention == DEPOSIT)
        {
            Plan.Add(MOVE_TO_PLAN);
            Plan.Add(DEPOSIT_PLAN);
        }
        if (intention == COLLECT)
        {
            Plan.Add(MOVE_TO_PLAN);
            Plan.Add(COLLECT_PLAN);
        }
        if (intention == HELP_GO_BASE)
        {
            Plan.Add(MOVE_TO_PLAN);
        }
        if (intention == EXPLORE_SOLO)
        {
            Plan.Add(MOVE_TO_PLAN);
        }
        //TODO
        if (intention == GO_BASE)
        {
            Plan.Add(MOVE_TO_PLAN);
            Plan.Add(SHARE_PLAN);
        }
        if (intention == PATROL)
        {
            Plan.Add(MOVE_TO_PLAN);
        }
		if (intention == EXPLORE_AS_GROUP)
        {
			if(!_isPartyLeader && _partyLeader!=null){
			}
            Plan.Add(EXPLORE_PLAN_GROUP);
        }

        if (intention == ATTACK_ZOMBIE_AS_GROUP) {
			Plan.Add(ATTACK_PLAN_GROUP);
			
		}
		if (intention == JOIN_PARTY){
			Plan.Add(JOIN_PARTY_PLAN);
			
		}
    }

    void Execute(string step, Vector3 position)
    {
        
		//if(name.Equals("SurvivorLeader")){ Debug.Log("Executing step explore as team?: " + step.Equals (EXPLORE_PLAN_GROUP)); }
		if (step == HEAL_PLAN)
        {
            Heal();
        }
        else if (step == MOVE_TO_PLAN)
        {
            if (intention == FOLLOW_SURVIVOR)
            {
                MoveTo(getSurvivorWithoutCritical().transform.position);
            }
            else
            {
                MoveTo(position);

            }
        }
        else if (step == ATTACK_PLAN)
        {
            attackZombie(NearestZombie());

        }
        else if (step == COLLECT_PLAN)
        {
            CollectResources(NearestResource());
        }
        else if (step == SHARE_PLAN)
        {
            share();
        }
        else if (step == DEPOSIT_PLAN)
        {
            DepositResources();

        }
        else if (step == RANDOM_MOVE_TO_PLAN)
        {
            randomMove();
        } 
        else if (step == ATTACK_PLAN_GROUP){
			superAttack();
			
		}else if (step == EXPLORE_PLAN_GROUP){
			superExplore();

		}
		else if (step == JOIN_PARTY_PLAN){
			joinParty();
		}
    }

	private bool askLeaderIfZombiesAroundParty(){
		return _partyLeader.GetComponent<Survivor_Deliberative>()._zombsAroundParty;
	}

    private bool succeced()
    {
        if (intention == HEAL && LevelHealth() == FULL_LEVEL)
        {
            return true;
        }
        if (intention == DEFEND_BASE && !lider_request_defend)
        {
            return true;
        }
        if (intention == FOLLOW_SURVIVOR && IsInBase())
        {
            return true;
        }
        if (intention == HELP_ATTACK && !ZombiesAround() && !isAnySurvivorAttacking())
        {
            return true;
        }
        if (intention == ATTACK_ZOMBIE && !ZombiesAround())
        {
            return true;
        }
        if (intention == ATTACK_ZOMBIE_AS_GROUP) {
			if(_isPartyLeader && !ZombiesAroundParty()){
				return true;
			}else{
				if(!askLeaderIfZombiesAroundParty()){
				return true;
				}
			}
			
		}
        if (intention == DEPOSIT && LevelResources() == EMPTY_LEVEL)
        {
            return true;

        }
        if (intention == COLLECT && LevelResources() == FULL_LEVEL)
        {
            return true;
        }
        if (intention == HELP_GO_BASE && IsInBase())
        {
            //Debug.Log ("tou na base sucedded");
            return true;
        }
        if (intention == EXPLORE_SOLO && ((intention_position - transform.position).magnitude < 4.0f))
        {
            return true;
        }
        if (intention == GO_BASE && share_end)
        {
            share_end = false;
            return true;

        }

        if (intention == EXPLORE_AS_GROUP && IsInBase() && _exploredPoints <= 0)
        {
			if(_isPartyLeader) disbandWholeParty();
			Debug.Log("Disband!");
            return true;
        }
        if (intention == IDLE)
        {
            return true;
        }
        if (intention == JOIN_PARTY && IsInParty())
        {
            return true;
        }


        return false;

    }

    private bool impossible()
    {
        /*if (intention == HEAL && LevelHealth() == FULL_LEVEL) {
            return true;
        }*/
        if (intention == DEFEND_BASE && !ZombiesAround())
        {
            return true;
        }
        if (intention == FOLLOW_SURVIVOR && !isHereSurvivorWithoutCritical())
        {
            //Debug.Log("Impossivel");
            return true;
        }
        if (intention == HELP_ATTACK && !isAnySurvivorAttacking())
        {
            return true;
        }

        if (intention == ATTACK_ZOMBIE && !ZombiesAround())
        {
            return true;
        }

        if (intention == ATTACK_ZOMBIE_AS_GROUP){
				
			if(_isPartyLeader){
				if(!ZombiesAroundParty() && _survivorsInTeam.Count+1 <= _minTeamMembers){
					disbandWholeParty();
					return true;
				}
			}else{
				if(!IsInParty()){
					return true;
				}
			}
		}

		if (intention == EXPLORE_AS_GROUP && 
		    (!IsInParty() || _survivorsInTeam.Count+1 < _minTeamMembers) )
		{
			if(_isPartyLeader) disbandWholeParty();
			return true;
		}

        if (intention == COLLECT && ((intention_position - transform.position).magnitude < 4.0f) && !ResourcesAround())
        {
            return true;
        }
        if (intention == HELP_GO_BASE && !isHereSurvivorCritical())
        {
            return true;
        }
        if (intention == GO_BASE && IsInBase()) {
            return true; 
        }

        return false;

    }

    private bool checkStepCompleted()
    {
        if (Plan[0] == HEAL_PLAN && LevelHealth() == FULL_LEVEL)
        {
            return true;
        }
        if (Plan[0] == MOVE_TO_PLAN && (intention_position - transform.position).magnitude < 4.0f)
        {
            return true;

        }
        if (Plan[0] == ATTACK_PLAN && !ZombiesAround())
        {
            return true;

        }
        if (Plan[0] == COLLECT_PLAN && (!ResourcesAround() || LevelResources() == FULL_LEVEL))
        {
            return true;

        }
        if (Plan[0] == DEPOSIT_PLAN && LevelResources() == EMPTY_LEVEL)
        {
            return true;

        }
        if (Plan[0] == RANDOM_MOVE_TO_PLAN)
        {
            return true;
        }
        if (Plan[0] == SHARE_PLAN)
        {
            return false;
		}

		return false;

    }

    private bool reconsider()
    {

        if (intention == HEAL && LevelHealth() == FULL_LEVEL)
        {
            return true;
        }

        if (intention == FOLLOW_SURVIVOR && IsInBase())
        {
            return true;
        }
        /*if (intention == HELP_ATTACK && !ZombiesAround() && !isAnySurvivorAttacking()) {
            return true;
        }*/
        if (intention == ATTACK_ZOMBIE && LevelHealth() == CRITICAL_LEVEL)
        {
            return true;
        }

        if (intention == DEPOSIT && (ZombiesAround() || (ResourcesAround() && LevelResources() != FULL_LEVEL)))
        {
            return true;

        }

        if (intention == COLLECT && ZombiesAround())
        {
            return true;
        }
        if (intention == HELP_GO_BASE && ZombiesAround())
        {
            return true;
        }

        if (intention == EXPLORE_SOLO && (ZombiesAround() || LevelResources() == FULL_LEVEL || LevelHealth() == CRITICAL_LEVEL || ResourcesAround()))
        {
            return true;
        }

        if (intention == GO_BASE && (ZombiesAround() || ResourcesAround()))
        {
            return true;
        }
        
		if (intention == EXPLORE_AS_GROUP && (ResourcesAround() || ZombiesAround())) {
            return true;
        }
		/*
        if (intention == IDLE) {
            return true;
        }
        */
        
        //TODO: TODO TODO

        return false;

    }

    private bool DiscoveredMapPosition(Vector3 position, GameObject resourceObj, int type)
    {
        int x = (int)position.x / Map.MAP_QUAD_DIMENSIONS + Map.MAP_WIDTH / 2;
        int y = (int)position.z / Map.MAP_QUAD_DIMENSIONS + Map.MAP_HEIGHT / 2;


        float resourceLevel;

        if (resourceObj != null)
            resourceLevel = resourceObj.GetComponent<ResourcesScript>().getResourcesLevel();
        else
            resourceLevel = 0;


        mapObj.GetComponent<Map>().UpdateMap(this.gameObject, _explorerMap, new Vector2(x, y), resourceLevel, type);

        if (_explorerMap[x][y] != 0)
        {

            _explorerMap[x][y] = (_explorerMap[x][y] < resourceLevel) ? _explorerMap[x][y] : (int)resourceLevel;
            return false;
        }


        //Debug.Log("Discovered Map Position " + new Vector2(x, y) + " from " + position);

        _explorerMap[x][y] = (_explorerMap[x][y] < resourceLevel) ? _explorerMap[x][y] : (int)resourceLevel;
        return true;
    }

    public void UpdateMyMap(int[][] newMap)
    {
        _explorerMap = newMap;
    }

    private bool getMapPositionInfoExplored(Vector3 position)
    {
        int x = (int)position.x / Map.MAP_QUAD_DIMENSIONS + Map.MAP_WIDTH / 2;
        int y = (int)position.z / Map.MAP_QUAD_DIMENSIONS + Map.MAP_HEIGHT / 2;

        if (x > Map.MAP_WIDTH || x < 0 || y > Map.MAP_HEIGHT || y < 0)
        {
            return false;
        }

        return _explorerMap[x][y] != Map.MAP_EMPTY_POS;
    }

    private float getMapPositionInfoResources(Vector3 position)
    {
        int x = (int)position.x / Map.MAP_QUAD_DIMENSIONS + Map.MAP_WIDTH / 2;
        int y = (int)position.z / Map.MAP_QUAD_DIMENSIONS + Map.MAP_HEIGHT / 2;

        if (_explorerMap[x][y] > 0)
        {
            return _explorerMap[x][y];
        }
        else
            return 0;
    }



    /// ////////
    /// Coliders
    /// ////////
    private bool showDebug = false;

    void OnTriggerEnter(Collider other)
    {

        if (other.tag.Equals("Survivor") && !other.transform.root.Equals(this.transform.root))
        {
            _survivorsInSight.Add(other.gameObject);

            if (showDebug)
            {
                Debug.Log(this.name + "-New Survivor " + other.name);
                Debug.Log("#Survivors in range: " + _survivorsInSight.Count);
            }
        }
        if (other.tag.Equals("Zombie"))
        {
            _zombiesInSight.Add(other.gameObject);

            if (showDebug)
            {
                Debug.Log(this.name + "-New Zombie " + other.name);
                Debug.Log("#Zombies in range: " + _zombiesInSight.Count);
            }
        }
        if (other.tag.Equals("Resources"))
        {
            _resourcesInSight.Add(other.gameObject);
            addResourceMap(other.gameObject);

            if (showDebug)
            {
                Debug.Log(this.name + "-New Resources " + other.name);
                Debug.Log("#Resources in range: " + _resourcesInSight.Count);
            }
        }
        if (other.tag.Equals("Heal"))
        {
            healPosition = other.transform.position;
            healInRange = true;
            if (showDebug)
            {
                Debug.Log(this.name + "-New heal " + other.name);
            }
        }
        if (other.tag.Equals("Deposit"))
        {
            depositPosition = other.transform.position;
            depositInRange = true;
            if (showDebug)
            {
                Debug.Log(this.name + "-New deposit " + other.name);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("Survivor") && !other.transform.root.Equals(this.transform.root))
        {
            _survivorsInSight.Remove(other.gameObject);

            if (showDebug)
            {
                Debug.Log("Lost Survivor.. " + other.name);
                Debug.Log("#Survivors in range: " + _survivorsInSight.Count);
            }

        }
        if (other.tag.Equals("Zombie"))
        {
            _zombiesInSight.Remove(other.gameObject);

            if (showDebug)
            {
                Debug.Log("Lost Zombie.. " + other.name);
                Debug.Log("#Zombies in range: " + _zombiesInSight.Count);
            }

        }
        if (other.tag.Equals("Resources"))
        {
            _resourcesInSight.Remove(other.gameObject);

            if (showDebug)
            {
                Debug.Log("Lost Resource " + other.name);
                Debug.Log("#Resources in range: " + _resourcesInSight.Count);
            }
        }
        if (other.tag.Equals("Heal"))
        {
            healInRange = false;

            if (showDebug)
            {
                Debug.Log("Lost Heal.. " + other.name);
            }

        }
        if (other.tag.Equals("Deposit"))
        {
            depositInRange = false;

            if (showDebug)
            {
                Debug.Log("Lost Deposit.. " + other.name);
            }
        }
    }

    public string getState()
    {
        return _state;
    }

    public void setDisplayInfo(bool param)
    {
        showInfo = param;
    }

    public void loseHealth(float ammount)
    {
        _healthLevel -= ammount;
        if (_healthLevel <= 0 && !_dead)
        {
            Debug.Log(this.name + " died.");

            sayMyLastWords();
			
			if(_isPartyLeader){
				passKnowledgeToNextLeader();
			}

            //to make it "disappear"
            _dead = true;
            Instantiate(Resources.Load(@"Models/Characters/Zombie"), this.transform.position, this.transform.rotation);
            StartCoroutine("destroyAfterDeath");
        }
    }

    public void BecomePartyMember(GameObject leader, List<GameObject> team){
		//if(this.gameObject.name.Equals("SurvivorLeader")) _isPartyLeader = true;
		_isInParty = true;
		_partyLeader = GameObject.Find(leader.name);
		_isTank = false;
		deliberatingNextZombieTarget = false;
		_survivorsInTeam = new List<GameObject>();
		
		foreach(GameObject surv in team){
			if(!_survivorsInTeam.Contains(surv)){
				_survivorsInTeam.Add(surv);
			}
		}
	}

	public void BecomePartyLeader(List<GameObject> team){
		_isPartyLeader = true;
		_isInParty = true;
		_partyLeader = gameObject;
		_isTank = false;
		deliberatingNextZombieTarget = false;
		_survivorsInTeam = new List<GameObject>();
		
		
		foreach(GameObject surv in team){
			if(!_survivorsInTeam.Contains(surv)){
				_survivorsInTeam.Add(surv);
			}
		}
	}

    private IEnumerator destroyAfterDeath()
    {
        yield return new WaitForSeconds(3.0F);
        //Debug.Log("Destroyed: "+ this.name);
        Destroy(this.gameObject);
    }
	
    private void checkImpossiblePathAndReset()
    {//Calculates a new setDestination in case the previous calc isnt reached in a set reset time
        timeWindow -= Time.deltaTime;
        if (timeWindow < 0)
        {
            //Debug.Log("Reset needed by :" + this.name);
            CurrentDestination = new Vector3(transform.position.x + Random.Range(-40.0f, 40.0f)
                                              , transform.position.y,
                                              transform.position.z + Random.Range(-40.0f, 40.0f));
            navMeshComp.SetDestination(CurrentDestination);
            timeWindow = PATH_RESET_TIME;
        }

    }

    /// //////////
    /// GUI
    /// /////////


    void OnGUI()
    {
        if (!_dead)
        {
            currentScreenPos = Camera.main.WorldToScreenPoint(this.transform.position);
            if (showInfo)
            {
                //Survivors's Information Box
                if (this.renderer.isVisible)
                {
                    GUI.Box(new Rect(currentScreenPos.x, Screen.height - currentScreenPos.y, infoBoxWidth, infoBoxHeight),
                            this.name + ": \n" +
                            "IsInParty: " + _isInParty +
                            " \n" +
					        "Leader: " + ((_partyLeader != null) ? _partyLeader.name : " ") +
                            " \n" +
                            "Intention: " + intention +
                            " \n" +
                            "Step: " + ((Plan.Count > 0) ? Plan[0] :" ")+
                              " \n");
                }
            }

            if (this.renderer.isVisible)
            {
                //Important, order matters!
                GUI.DrawTexture(new Rect(currentScreenPos.x + lifebar_x_offset, Screen.height - currentScreenPos.y + lifebar_y_offset,
                                         lifebar_lenght,
                                         lifebar_height), life_bar_red);
                GUI.DrawTexture(new Rect(currentScreenPos.x + lifebar_x_offset, Screen.height - currentScreenPos.y + lifebar_y_offset,
                                         (FULL_HEALTH - (FULL_HEALTH - _healthLevel)) * lifebar_lenght / FULL_HEALTH,
                                         lifebar_height), life_bar_green);
            }
        }
    }


    void LateUpdate()
    {
		if (_hasMap && navMeshComp != null)
        {
            NavMeshHit hit;
            navMeshComp.FindClosestEdge(out hit);
            //GameObject.Find("SpherePoint").transform.position = hit.position + new Vector3(0, 30, 0);
            //Debug.Log(hit.distance);

            if (hit.distance < 4)
                DiscoveredMapPosition(transform.position, NearestResource(), Map.MAP_LIMIT_POS);
            else
                DiscoveredMapPosition(transform.position, NearestResource(), Map.MAP_NORMAL_POS);

        }
    }



    void Update()
    {
		if(_partyLeader != null){
			sphere.transform.position = _partyLeader.transform.position;
		}

		if (!_dead)
        {
            
            // CICLO PRINCIPAL

            /*if(ResourcesAround())
            Debug.Log ("estao resources a volta");

            Debug.Log ("estao estas resources a volta: " +  _resourcesInSight.Count());
            Debug.Log("numero: " + resources_location.Count());
            //Debug.Log ( "Desires: " + Desires);
            Debug.Log ( "intention: " + intention);
            Debug.Log ( "intention: " + intention_position);
            //Debug.Log ( "state: " + _state);*/

           // Debug.Log("intention: " + intention_position);

            if (out_of_while)
            {
                Options();
                filter();
                Planner();

                //Debug.Log("tou no execute: " + Plan.Count());
                out_of_while = false;
            }
            if (!out_of_while)
            {

                if (Plan.Count() != 0 && !succeced() && !impossible())
                {
                    //Debug.Log("tou no execute: " + Plan[0]);

					//Debug.Log("stepCompleted? " + checkStepCompleted());

                    if (checkStepCompleted())
                    {
                        Plan.RemoveAt(0);
                    }
                    else
                        Execute(Plan[0], intention_position);
						

                    if (reconsider())
                    {
                        Debug.Log("tou a reconsiderar");
                        Options();
                        filter();
                        Planner();
                    }
                }
                else
                {
                    out_of_while = true;
                }

            }


            //avoid navmesh pathfinding issues
            //checkImpossiblePathAndReset();
            //DO NOT DELETE This forces collision updates in every frame
            this.transform.root.gameObject.transform.position += new Vector3(0.0f, 0.0f, -0.00001f);
            //Collider[] colliders = Physics.OverlapSphere(this.transform.position,_visionRange);

            /**/
        }
        else
        {
            this.renderer.material = transparentMaterial;
            Destroy(this.GetComponent<NavMeshAgent>());
            this.rigidbody.AddForce(0, 100.0f, 0, ForceMode.Force);
        }
    }
}
