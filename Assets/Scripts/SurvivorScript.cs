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

	private const float PICKUP_RANGE = 2.0f;
	private const float FULL_HEALTH = 100.0f;
	private const float MAX_RESOURCES = 100.0f;
	private const float CRITICAL_THRESHOLD = 30.0f;
	
	private List<GameObject> _zombiesInSight;
	private List<GameObject> _survivorsInSight;
	private List<GameObject> _resourcesInSight;
	private GameObject _closestResource;
	private GameObject _closestSurvivor;
	private GameObject _closestZombie;

	private float _dist2Resource;
	private bool _isCollecting;
	private Vector3 healPosition;
	private bool healInRange;
	
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
	
	void Start () {
		
		_healthLevel = FULL_HEALTH;
		_movSpeed = 5.0f;
		_visionRange = 20.0f;
		_attDamage = 50.0f;
		_attRange = 5.0f;
		_resourceLevel = 0.0f;
		
		_zombiesInSight = new List<GameObject>();
		_survivorsInSight = new List<GameObject>();
		_resourcesInSight = new List<GameObject>();
		_isCollecting = false;


		navMeshComp = GetComponent<NavMeshAgent>();

		
		SphereCollider visionRangeCollider = this.gameObject.GetComponentInChildren<SphereCollider>();
		if(visionRangeCollider != null){
			visionRangeCollider.radius = _visionRange;
		}else{
			Debug.Log("Missing sphere collider");
		}

		showInfo = false;

		life_bar_green = (Texture2D)Resources.Load(@"Textures/life_bar_green", typeof(Texture2D));
		life_bar_red = (Texture2D)Resources.Load(@"Textures/life_bar_red", typeof(Texture2D));

		lifebar_lenght = 30.0f;
		lifebar_height = 4.0f;
		lifebar_x_offset = -15.0f;
		lifebar_y_offset = -8.0f;

		timeWindow = PATH_RESET_TIME;
		CurrentDestination = this.transform.position;
		navMeshComp.speed = _movSpeed;
		//TODO: STOP DANCING
		//navMeshComp.updatePosition = false;
		//navMeshComp.updateRotation = false;
	}
	
	//Actuadores-------------------------------------------------------------------
	//TODO: Attack-Zombie
	//TODO: Collect-Resources
	//TODO: Deposit-Resources
	//TODO: Heal
	private void Heal(){
		if (healInRange) {
			navMeshComp.SetDestination (healPosition);
			if ((healPosition - transform.position).magnitude < 4) {
				_healthLevel = FULL_HEALTH;
				//healInRange = false;
				CurrentDestination = this.transform.position;
				}
			}
	}
	//TODO: Random-Move
	private void randomMove(){
		/**/

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
	//TODO: Level-Resources?
	private int LevelResources() 
	{
 		if (_resourceLevel <= CRITICAL_THRESHOLD)
						return 1;
		if (_resourceLevel > CRITICAL_THRESHOLD && _resourceLevel < MAX_RESOURCES)
						return 2;
		if (_resourceLevel == MAX_RESOURCES)
						return 3;
		return 0;
	}
	//TODO: Level-Health?
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
	//TODO: Is-In-Base?
	//TODO: Resources-Around?
	private bool ResourcesAround(){
		if (_resourcesInSight.Count > 0)
			return true;
		else
			return false;
	}
	//TODO: Survivors-Around?
	private bool SurvivorsAround(){
		if (_survivorsInSight.Count > 0)
			return true;
		else
			return false;
	}
	//TODO: Zombies-Around?
	private bool ZombiesAround(){
		if (_zombiesInSight.Count > 0)
			return true;
		else
			return false;
	}
	//TODO: Nearest-Survivor-Position
	private Vector3 NearestSurvivorPosition(){
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
	//TODO: Nearest-Zombie-Position
	private Vector3 NearestZombiePosition(){
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



	/// ////////
	/// Coliders
	/// ////////
	private bool showDebug = false;
	
	void OnTriggerEnter (Collider other) {
		//Debug.Log(this.name + "'s " + this.collider.name + " hit: " + other.name);
		
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
	}

	/// ////////
	/// AUX
	/// ////////



	void updateClosestResource(){
		if (_resourcesInSight.Count > 0){
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
		}else{
			_closestResource = null;
		}
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
		currentScreenPos = Camera.main.WorldToScreenPoint(this.transform.position);
		if(showInfo){
			//Survivors's Information Box

			if(this.renderer.isVisible){
				GUI.Box(new Rect(currentScreenPos.x, Screen.height - currentScreenPos.y, infoBoxWidth, infoBoxHeight),
				        this.name + ": \n" +
				        "Health: " + _healthLevel + 
				        " \n" +
				        "Resources: " + _resourcesInSight.Count + 
				        " \n" +
				        "Survivors: " + _survivorsInSight.Count + 
				        " \n" +
				        "Zombies: " + _zombiesInSight.Count + 
				        " \n");
			}
		}

		if(this.renderer.isVisible){
			//Important, order matters!
			//TODO: Finishbar
			GUI.DrawTexture(new Rect(currentScreenPos.x + lifebar_x_offset, Screen.height - currentScreenPos.y + lifebar_y_offset, 
			                         lifebar_lenght, 
			                         lifebar_height), life_bar_red);
			GUI.DrawTexture(new Rect(currentScreenPos.x + lifebar_x_offset, Screen.height - currentScreenPos.y + lifebar_y_offset,
			                         (FULL_HEALTH - (FULL_HEALTH - _healthLevel))*lifebar_lenght/FULL_HEALTH, 
			                         lifebar_height), life_bar_green);
		}


	}
	
	void Update () {
		//if( SurvivorsAround())
		//Debug.Log (NearestSurvivorPosition()); 


		checkImpossiblePathAndReset();

		if(_zombiesInSight.Count == 0){
			updateClosestResource();
			if(_closestResource != null){
				navMeshComp.SetDestination(_closestResource.transform.position);
				_dist2Resource = Vector3.Distance(_closestResource.transform.position, this.transform.position);
				if(_dist2Resource <= PICKUP_RANGE){
					_closestResource.GetComponent<ResourcesScript>().catchResources();
				}
				_isCollecting = true;
			}else{
				if(_isCollecting == true){
					CurrentDestination = this.transform.position; // resets his destination, because of the bug that made him stand still
					randomMove();
					_isCollecting = false;
				}else{
					randomMove();
				}
			}
		}else{
			if(_isCollecting == true){
				CurrentDestination = this.transform.position; // resets his destination, because of the bug that made him stand still
				randomMove();
				_isCollecting = false;
			}else{
				randomMove();
			}
		}
		//DO NOT DELETE This forces collision updates in every frame
		this.transform.root.gameObject.transform.position += new Vector3(0.0f, 0.0f, -0.00001f);
		//Collider[] colliders = Physics.OverlapSphere(this.transform.position,_visionRange);
	}

	public void setDisplayInfo(bool param){
		showInfo = param;
	}

	public void loseHealth(float ammount){
		_healthLevel -= ammount;
		if(_healthLevel <= 0){
			Debug.Log(this.name + " died.");
			//to make it "disappear"
			this.transform.position = new Vector3(550, 0, 500.0f);
		}
	}
	
}
