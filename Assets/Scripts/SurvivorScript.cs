using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SurvivorScript: MonoBehaviour {
	
	public float _healthLevel;
	public float _movSpeed;
	public float _visionRange;
	public float _attDamage;
	public float _attRange;
	
	private List<GameObject> _zombiesInSight;
	private List<GameObject> _survivorsInSight;
	
	private float infoBoxWidth = 100.0f;
	private float infoBoxHeight = 60.0f;
	private Vector3 currentScreenPos;
	
	private Texture2D life_bar_green, life_bar_red;
	private float lifebar_lenght, lifebar_height;

	private NavMeshAgent navMeshComp;
	private Vector3 CurrentDestination;

	private bool showInfo;
	
	
	void Start () {
		
		_healthLevel = 100.0f;
		_movSpeed = 5.0f;
		_visionRange = 20.0f;
		_attDamage = 50.0f;
		_attRange = 5.0f;
		
		_zombiesInSight = new List<GameObject>();
		_survivorsInSight = new List<GameObject>();
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

		lifebar_lenght = 20.0f;
		lifebar_height = 3.0f;

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
	//TODO: Random-Move
	private void randomMove(){
		/**/
			if ((CurrentDestination - transform.position).magnitude < 2.0f) {
				CurrentDestination = new Vector3 (transform.position.x + Random.Range (- 40.0f, 40.0f)
				                                  ,transform.position.y,
				                                  transform.position.z + Random.Range (- 40.0f, 40.0f));
				navMeshComp.SetDestination(CurrentDestination);
			}
			/**/
	}
	
	//Sensores---------------------------------------------------------------------
	//TODO: Level-Resources?
	//TODO: Level-Health?
	//TODO: Is-In-Base?
	//TODO: Resources-Around?
	//TODO: Survivors-Around?
	//TODO: Zombies-Around?
	
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
		if(showInfo){
			//Survivors's Information Box
			currentScreenPos = Camera.main.WorldToScreenPoint(this.transform.position);

			if(this.renderer.isVisible){
				GUI.Box(new Rect(currentScreenPos.x, Screen.height - currentScreenPos.y, infoBoxWidth, infoBoxHeight),
				        this.name + ": \n" +
				        "Health: " + _healthLevel + 
				        " \n" +
				        "Survivors: " + _survivorsInSight.Count + 
				        " \n" +
				        "Zombies: " + _zombiesInSight.Count + 
				        " \n");
			}
		}

		//Important, order matters!
		//TODO: Finishbar
		GUI.DrawTexture(new Rect(0, 0, lifebar_lenght, lifebar_height), life_bar_red);
		GUI.DrawTexture(new Rect(0, 0, lifebar_lenght, lifebar_height), life_bar_green);


	}
	
	void Update () {
		
		randomMove();

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
			//
			this.transform.position = new Vector3(550, 0, 500.0f);
		}
	}
	
}
