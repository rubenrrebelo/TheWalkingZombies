using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZombieScript: MonoBehaviour {
	
	public float _healthLevel;
	public float _movSpeed;
	public float _visionRange;
	public float _attDamage;
	public float _attRange;
	
	private List<GameObject> _survivorsInSight;
	private float infoBoxWidth = 100.0f;
	private float infoBoxHeight = 150.0f;
	private Vector3 currentScreenPos;
	
	void Start () {
		
		_healthLevel = 10.0f;
		_movSpeed = 5.0f;
		_visionRange = 20.0f;
		_attDamage = 5.0f;
		_attRange = 1.0f;

		_survivorsInSight = new List<GameObject>();
		
		SphereCollider visionRangeCollider = this.gameObject.GetComponent<SphereCollider>();
		if(visionRangeCollider != null){
			visionRangeCollider.radius = _visionRange;
		}else{
			Debug.Log("Missing sphere collider");
		}
	}
	
	
	//Actuadores-------------------------------------------------------------------
	//Attack-Survivor
	
	
	//Random-Move
	private void randomMove(){
		//TODO: mockup, zombies walk forward
		this.transform.root.gameObject.transform.position += new Vector3(0.0f, 0.0f, -0.2f);
	}
	
	//Sensores---------------------------------------------------------------------
	//See-Survivor (posição do survivor mais próx)

	private bool showDebug = false;
	
	void OnTriggerEnter (Collider other) {
		//Debug.Log(this.name + "'s " + this.collider.name + " hit: " + other.name);

		Debug.Log(other.transform.root.Equals(this.transform.root));
		
		if (other.tag.Equals("Survivor") && !other.transform.root.Equals(this.transform.root)){
			_survivorsInSight.Add(other.gameObject);
			
			if(showDebug){
				Debug.Log(this.name + "-New Survivor " + other.name);
				Debug.Log("#Survivors in range: " + _survivorsInSight.Count);}
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

		//This forces collision updates in every frame
		this.transform.root.gameObject.transform.position += new Vector3(0.0f, 0.0f, -0.00001f);


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
