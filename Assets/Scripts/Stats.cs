using UnityEngine;
using System.Collections;

public class Stats : MonoBehaviour {

	bool _showStats;
	GameObject _baseLeader;
	float _resourcesCollected;
	int _zombiesSlayed;
	int _survivorsAlive;
	
	// Use this for initialization
	void Start () {
		_showStats = false;
		_baseLeader = GameObject.FindWithTag("BaseLeader");
		_resourcesCollected = 0.0f;
		_zombiesSlayed = 0;
		_survivorsAlive = 0;
	}
	
	public void zombieKilled(){
		_zombiesSlayed++;
	}
	
	void OnGUI(){
		if(_showStats){
			GUI.Box(new Rect(0, 0, 200, 100),
			        "TotalResources: " + _resourcesCollected +
			        " \n" +
			        "Survivors: " + _survivorsAlive +
			        " \n" +
			        "ZombiesSlayed: " + _zombiesSlayed
			        );
		}
	}
	
	// Update is called once per frame
	void Update () {

		if(Input.GetKeyDown(KeyCode.L)){
			_survivorsAlive = GameObject.FindGameObjectsWithTag("Survivor").Length;
			_resourcesCollected = _baseLeader.GetComponent<BaseLeaderScript>()._totalAmmountOfResources;
		}
		
		if(Input.GetKeyDown(KeyCode.K)){
			if(_showStats){
				_showStats = false;
			}else{
				_showStats = true;
			}
		}
	}
}
