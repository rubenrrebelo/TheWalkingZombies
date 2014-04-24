using UnityEngine;
using System.Collections;

public class ResourcesScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void catchResources(){
		//to make it "disappear"
		this.transform.position = new Vector3(550, 0, 500.0f);
	}
}