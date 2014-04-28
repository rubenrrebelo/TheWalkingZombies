
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BaseLiderScript: MonoBehaviour {
	
	private float _barrierLevel;
	private float _visionRange = 10.0f;
	private float _resourcesLevel;

	private const float BARRIER_FULL_HEALTH = 100.0f;
	private const float FULL_RESOURCES = 100.0f;

	private const float REPAIR_BASE_SPEED = 1.0f;
	private const float RESOURCE_DECAY_SPEED = 0.01f;

	private const float CRITICAL_BARRIER_THRESHOLD = 30.0f;
	private const float CRITICAL_RESOURCES_THRESHOLD = 30.0f;
	
	private List<GameObject> _zombiesInSight;
	private List<GameObject> _survivorsInSight;
	
	private float infoBoxWidth = 100.0f;
	private float infoBoxHeight = 90.0f;
	private Vector3 currentScreenPos;

	private bool showInfo;
	private float lifebar_x_offset, lifebar_y_offset;
	private Texture2D life_bar_green, life_bar_red, resource_bar_blue;
	private float lifebar_lenght, lifebar_height;
	
	void Start () {
		
		_barrierLevel = BARRIER_FULL_HEALTH;
		_visionRange = 10.0f;
		
		_zombiesInSight = new List<GameObject>();
		_survivorsInSight = new List<GameObject>();

		
		SphereCollider visionRangeCollider = this.gameObject.GetComponentInChildren<SphereCollider>();
		if(visionRangeCollider != null){
			visionRangeCollider.radius = _visionRange;
		}else{
			Debug.Log("Missing sphere collider");
		}

		showInfo = false;

		life_bar_green = (Texture2D)Resources.Load(@"Textures/life_bar_green", typeof(Texture2D));
		life_bar_red = (Texture2D)Resources.Load(@"Textures/life_bar_red", typeof(Texture2D));
		resource_bar_blue = (Texture2D)Resources.Load(@"Textures/resource_bar_blue", typeof(Texture2D));

		lifebar_lenght = 30.0f;
		lifebar_height = 4.0f;
		lifebar_x_offset = -15.0f;
		lifebar_y_offset = -8.0f;

	
	}
	
	//Actuadores-------------------------------------------------------------------
	//TODO: Repair-Base
	private void RepairBase(){
		if (_barrierLevel <= 0) {
			Debug.Log (this.name + " GAME OVER.");
			return;
		}
		_barrierLevel += REPAIR_BASE_SPEED;
		_resourcesLevel -= REPAIR_BASE_SPEED;
	}

	//TODO: Request-Resources
	// Only available in Deliberative Agents

	//TODO: Request-Protection
	// Only available in Deliberative Agents

	//TODO: Idle
	private void Idle(){
		//Debug.Log ("Nothing to do");
	}

	//TODO: Resource-Decay
	private void ResourceDecay(){
		if(_resourcesLevel >= 0)
			_resourcesLevel -= RESOURCE_DECAY_SPEED;
		//Debug.Log ("Resource");
	}

	
	//Sensores---------------------------------------------------------------------
	//TODO: Need-Repair?
	private bool NeedRepair(){
		return _barrierLevel <= CRITICAL_BARRIER_THRESHOLD;
	}

	//TODO: Need-Resources?
	private bool NeedResources(){
		return _resourcesLevel <= CRITICAL_RESOURCES_THRESHOLD;
	}

	//TODO: Number-Survivors-Around?
	private int NumberSurvivorsAround(){
		return _survivorsInSight.Count;
	}

	//TODO: Number-Zombies-Around?
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
				        "Health: " + _barrierLevel + 
				        " \n" +
				        "Resources: " + _resourcesLevel + 
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
			                         (BARRIER_FULL_HEALTH - (BARRIER_FULL_HEALTH - _barrierLevel))*lifebar_lenght/BARRIER_FULL_HEALTH, 
			                         lifebar_height), life_bar_green);
			GUI.DrawTexture(new Rect(currentScreenPos.x + lifebar_x_offset, Screen.height - currentScreenPos.y + lifebar_y_offset + lifebar_height,
			                         (FULL_RESOURCES - (FULL_RESOURCES - _resourcesLevel))*lifebar_lenght/FULL_RESOURCES, 
			                         lifebar_height), resource_bar_blue);
		}


	}
	
	void Update () {

		ResourceDecay ();

		if (NeedRepair () && !NeedResources ()) {
				RepairBase ();
		} else if (NeedResources ()) {
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


}
