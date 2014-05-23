
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BaseLeaderScript: MonoBehaviour {

	private float _healthLevel;
	//private float _barrierLevel;
	private float _visionRange = 10.0f;
	private float _resourcesLevel;

	private bool _dead;

	private const float FULL_HEALTH = 100.0f;

	private const float FULL_RESOURCES = 500.0f;

	private const float REPAIR_BARRIER_SPEED = 0.1f;
	private const float RESOURCE_DECAY_SPEED = 0.01f;

	private const float CRITICAL_BARRIER_THRESHOLD = 30.0f;
	private const float CRITICAL_RESOURCES_THRESHOLD = 30.0f;
	
	private List<GameObject> _zombiesInSight;
	private List<GameObject> _survivorsInSight;
    //private List<GameObject> _barriersInSight;

    private GameObject _barrier1;
    private GameObject _barrier2;
	
	private float infoBoxWidth = 100.0f;
	private float infoBoxHeight = 90.0f;
	private Vector3 currentScreenPos;

	private bool showInfo;
	private float lifebar_x_offset, lifebar_y_offset;
	private Texture2D life_bar_green, life_bar_red, resource_bar_blue;
	private float lifebar_lenght, lifebar_height;

    public bool _hasMap = false;
    public int[][] _explorerMap;

    private GameObject mapObj;
	
	void Start () {

		_healthLevel = FULL_HEALTH;
		//_barrierLevel = LIFE_FULL_HEALTH;
        _resourcesLevel = 100.0f;
		_visionRange = 10.0f;

		_dead = false;

		_zombiesInSight = new List<GameObject>();
		_survivorsInSight = new List<GameObject>();
        //_barriersInSight = new List<GameObject>();
		
		SphereCollider visionRangeCollider = this.gameObject.GetComponentInChildren<SphereCollider>();
		if(visionRangeCollider != null){
			visionRangeCollider.radius = _visionRange;
		}else{
			Debug.Log("Missing sphere collider");
		}

		showInfo = false;

        _barrier1 = GameObject.Find("Barrier_Bottom_Lane");
        _barrier2 = GameObject.Find("Barrier_Top_Lane");

		life_bar_green = (Texture2D)Resources.Load(@"Textures/life_bar_green", typeof(Texture2D));
		life_bar_red = (Texture2D)Resources.Load(@"Textures/life_bar_red", typeof(Texture2D));
		resource_bar_blue = (Texture2D)Resources.Load(@"Textures/resource_bar_blue", typeof(Texture2D));

		lifebar_lenght = 30.0f;
		lifebar_height = 4.0f;
		lifebar_x_offset = -15.0f;
		lifebar_y_offset = -8.0f;

        // Set Map
        mapObj = GameObject.Find("Map");
        resetMap();
	}

    private void resetMap()
    {
        _explorerMap = new int[Map.MAP_WIDTH][];
        for (int i = 0; i < Map.MAP_WIDTH; i++)
        {
            _explorerMap[i] = new int[Map.MAP_HEIGHT];
            for (int j = 0; j < Map.MAP_HEIGHT; j++)
            {
                _explorerMap[i][j] = Map.MAP_EMPTY_POS;
            }

        }
    }
	
	//Actuadores-------------------------------------------------------------------
	//Repair-Barrier1
	private void RepairBarrier1(){
       
		if (_barrier1.GetComponent<BarrierScript>().needRepair() && _resourcesLevel <= 0) {
			Debug.Log (this.name + " Barrier Gone.");
			return;
		}
        _barrier1.GetComponent<BarrierScript>().repairBase(REPAIR_BARRIER_SPEED);
		_resourcesLevel -= REPAIR_BARRIER_SPEED;
	}

    //Repair-Barrier2
    private void RepairBarrier2()
    {

        if (_barrier2.GetComponent<BarrierScript>().needRepair() && _resourcesLevel <= 0)
        {
            Debug.Log(this.name + " Barrier Gone.");
            return;
        }
        _barrier2.GetComponent<BarrierScript>().repairBase(REPAIR_BARRIER_SPEED);
        _resourcesLevel -= REPAIR_BARRIER_SPEED;
    }

	//TODO: Request-Resources
	// Only available in Deliberative Agents

	//TODO: Request-Protection
	// Only available in Deliberative Agents

	//Idle
	private void Idle(){
		//Debug.Log ("Nothing to do");
	}

	//Resource-Decay
	private void ResourceDecay(){
		if(_resourcesLevel >= 0)
			_resourcesLevel -= RESOURCE_DECAY_SPEED;
		//Debug.Log ("Resource");
	}

	
	//Sensores---------------------------------------------------------------------
	//Need-RepairBarrier1?
	private bool NeedRepairBarrier1(){
		return _barrier1.GetComponent<BarrierScript>().needRepair();
	}

    //Need-RepairBarrier2?
    private bool NeedRepairBarrier2()
    {
        return _barrier2.GetComponent<BarrierScript>().needRepair();
    }

    //Get-Barrier1-Health?
    private float GetBarrier1Health()
    {
        return _barrier1.GetComponent<BarrierScript>().getBarrierHealth();
    }

    //Get-Barrier2-Health?
    private float GetBarrier2Health()
    {
        return _barrier2.GetComponent<BarrierScript>().getBarrierHealth();
    }

	//Need-Resources?
	private bool NeedResources(){
		return _resourcesLevel <= CRITICAL_RESOURCES_THRESHOLD;
	}

	//Number-Survivors-Around?
	private int NumberSurvivorsAround(){
		return _survivorsInSight.Count;
	}

	//Number-Zombies-Around?
	private int NumberZombiesAround(){
		return _zombiesInSight.Count;
	}



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
	}


	void OnGUI(){		
		currentScreenPos = Camera.main.WorldToScreenPoint(this.transform.position);
		if(showInfo){
			//Survivors's Information Box

			if(this.renderer.isVisible){
				GUI.Box(new Rect(currentScreenPos.x, Screen.height - currentScreenPos.y, infoBoxWidth, infoBoxHeight),
				        this.name + ": \n" +
				        "Health: " + _healthLevel + 
				        " \n" +
				        "Resources: " + _resourcesLevel + 
				        " \n" +
				        "Survivors: " + _survivorsInSight.Count + 
				        " \n" +
				        "Zombies: " + _zombiesInSight.Count + 
				        " \n");
			}
		}

		if(this.renderer.isVisible && !_dead){
			//Important, order matters!
			GUI.DrawTexture(new Rect(currentScreenPos.x + lifebar_x_offset, Screen.height - currentScreenPos.y + lifebar_y_offset, 
			                         lifebar_lenght, 
			                         lifebar_height), life_bar_red);
			GUI.DrawTexture(new Rect(currentScreenPos.x + lifebar_x_offset, Screen.height - currentScreenPos.y + lifebar_y_offset,
                                     (FULL_HEALTH - (FULL_HEALTH - _healthLevel)) * lifebar_lenght / FULL_HEALTH, 
			                         lifebar_height), life_bar_green);
			GUI.DrawTexture(new Rect(currentScreenPos.x + lifebar_x_offset, Screen.height - currentScreenPos.y + lifebar_y_offset + lifebar_height,
			                         (FULL_RESOURCES - (FULL_RESOURCES - _resourcesLevel))*lifebar_lenght/FULL_RESOURCES, 
			                         lifebar_height), resource_bar_blue);
		}
	}
	
	void Update () {


		ResourceDecay ();

		if ((NeedRepairBarrier1 () || NeedRepairBarrier2()) && !NeedResources ()) {
            if (GetBarrier1Health() < GetBarrier2Health())
            {
                RepairBarrier1();
            }
            else if (GetBarrier1Health() > GetBarrier2Health())
            {
                RepairBarrier2();
            }
            else
            {
                int r = Random.Range(0, 1);
                if (r == 0) { RepairBarrier1(); }
                else{ RepairBarrier2(); }
               
            }
        }
        
        else if (NeedResources())
        {
				//Request Resources
		} else if (NumberSurvivorsAround() < NumberZombiesAround()) {
				// Request protection
		} else {
				Idle ();
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
		if(_healthLevel <= 0 && !_dead){
			Debug.Log(this.name + " died.");
			//to make it "disappear"
			this.transform.position = new Vector3(700, 0, 700.0f);
			_dead = true;
			StartCoroutine("destroyAfterDeath");
		}
	}

	public void addResources(float ammount){
		_resourcesLevel += ammount;
	}

	private IEnumerator destroyAfterDeath(){
		yield return new WaitForSeconds(0.2F);
		Destroy(this.gameObject);
		//TODO: Finish game menu
		Time.timeScale = 0;

	}

    void LateUpdate()
    {
        DiscoveredMapPosition(transform.position, null, Map.MAP_LIMIT_POS);
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

    public int[][] TransmitMap(int[][] newMap)
    {
        for (int i = 0; i < Map.MAP_WIDTH; i++)
        {
            for (int j = 0; j < Map.MAP_HEIGHT; j++)
            {
                if (newMap[i][j] >= 0 && newMap[i][j] < _explorerMap[i][j])
                    _explorerMap[i][j] = newMap[i][j];
            }
        }

        //GameObject.Find("Map").GetComponent<Map>().UpdateTheMap(gameObject.name, _explorerMap);
        return _explorerMap;
    }

    private bool getMapPositionInfoExplored(Vector3 position)
    {
        int x = (int)position.x / Map.MAP_QUAD_DIMENSIONS + Map.MAP_WIDTH / 2;
        int y = (int)position.z / Map.MAP_QUAD_DIMENSIONS + Map.MAP_HEIGHT / 2;

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

}
