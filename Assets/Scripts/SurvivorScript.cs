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
	private float infoBoxHeight = 150.0f;
	private Vector3 currentScreenPos;


	
	void Start () {
		
		_healthLevel = 100.0f;
		_movSpeed = 5.0f;
		_visionRange = 20.0f;
		_attDamage = 5.0f;
		_attRange = 5.0f;
		
		_zombiesInSight = new List<GameObject>();
		_survivorsInSight = new List<GameObject>();

		SphereCollider visionRangeCollider = this.gameObject.GetComponent<SphereCollider>();
		if(visionRangeCollider != null){
			visionRangeCollider.radius = _visionRange;
		}else{
			Debug.Log("Missing sphere collider");
		}
		
		
	}
	
	//Actuadores-------------------------------------------------------------------
	//Attack-Zombie
	//Collect-Resources
	//Deposit-Resources
	//Random-Move
	private void randomMove(){}
	
	//Sensores---------------------------------------------------------------------
	//Level-Resources?
	//Level-Health?
	//Is-In-Base?
	//Resources-Around?
	//Survivors-Around?
	//Zombies-Around?

	private bool showDebug = true;
	
	void OnTriggerEnter (Collider other) {


		if (other.tag.Equals("Survivor")){
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
		if (other.tag.Equals("Survivor")){
			_survivorsInSight.Remove(other.gameObject);

			if(showDebug){
				Debug.Log("Lost Survivor.. " + other.name);
				if(_survivorsInSight.Count != 0){
					Debug.Log( "#Survivors in range: " + _survivorsInSight.Count);
				}
			}

		}
		if (other.tag.Equals("Zombie")){
			_zombiesInSight.Remove(other.gameObject);

			if(showDebug){
				Debug.Log("Lost Zombie.. " + other.name);
				if(_zombiesInSight.Count != 0){
					Debug.Log("#Zombies in range: " + _zombiesInSight.Count);
				}
			}

		}
	}
	
	void OnGUI(){
		
		/** /
		//DEBUG window top-left
		GUI.Box(new Rect(0,0, 300, 200), "Debug: \n" +
		        "x: " + currentScreenPos.x + "\n" +
		        "y: " + currentScreenPos.y + "\n" +
		        "mouse x: " + Input.mousePosition.x + "\n" +
		        "mouse y: " + (Screen.height - currentScreenPos.y) + "\n"
		        );
		/**/
		
		
		
		/** /
		//Survivors's Information Box
		currentScreenPos = Camera.main.WorldToScreenPoint(this.transform.position);

		if(this.GetComponentInChildren<Renderer>().isVisible){
			GUI.Box(new Rect(currentScreenPos.x, Screen.height - currentScreenPos.y, infoBoxWidth, infoBoxHeight),
			        "Zombie info: \n" + 
			        " \n" +
			        "end.");
		}
		/**/
		
	}
	
	void Update () {
		
		randomMove();
		
		/** /
		//DEBUG
		 if(_survivorsInSight.Count != 0){
			foreach(GameObject survivor in _survivorsInSight){
				Debug.Log( "Zombie: " + survivor.gameObject.transform.position);
			}
		}
		/**/
		
		/**/
		//DEBUG
		//Collider[] colliders = Physics.OverlapSphere(this.transform.position,_visionRange);
		/**/
	}
}
