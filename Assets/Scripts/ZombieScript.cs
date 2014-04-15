using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZombieScript : MonoBehaviour {

	private float _healthLevel;
	private float _movSpeed;
	private float _visionRange;
	private float _attDamage;
	private float _attRange;

	private List<GameObject> _survivorsInSight;



	
	// Use this for initialization
	void Start () {
		_healthLevel = 10.0f;
		_movSpeed = 5.0f;
		_visionRange = 10.0f;
		_attDamage = 5.0f;
		_attRange = 1.0f;

		_survivorsInSight = new List<GameObject>();

		//TODO: Inicializar o radius do VisionRange collider do zombie com o valor _visionRange
	}
	
	//Actuadores-------------------------------------------------------------------
	//Attack-Survivor
	
	
	//Random-Move
	private void randomMove(){}
	
	//Sensores---------------------------------------------------------------------
	//See-Survivor (posição do survivor mais próx)


	void OnTriggerEnter (Collider other) {
		if (other.tag.Equals("Survivor")){
			_survivorsInSight.Add(other.gameObject);
			Debug.Log( "Enter");
		}
	}
	void OnTriggerExit (Collider other){
		_survivorsInSight.Remove(other.gameObject);
		Debug.Log( "Exit");
	}



	// Update is called once per frame
	void Update () {
		
		randomMove();

		/*
		 * 
		 * 
		 if(_survivorsInSight.Count != 0){
			foreach(GameObject survivor in _survivorsInSight){
				Debug.Log( "Zombie: " + survivor.gameObject.transform.position);
			}
		}
		 */
		
		
	}
}
