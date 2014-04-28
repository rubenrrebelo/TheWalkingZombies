using UnityEngine;
using System.Collections;

public class ResourcesScript : MonoBehaviour {

	private bool _dead; //to prevent multiple destroyAfterDeath calls

	// Use this for initialization
	void Start () {
		_dead = false;
	}
	
	// Update is called once per frame
	void Update () {

	}

	public void catchResources(){
		//to make it "disappear"
		this.transform.position = new Vector3(700, 0, 700.0f);
		if(!_dead){
			_dead = true;
			StartCoroutine("destroyAfterDeath");
		}
	}

	private IEnumerator destroyAfterDeath(){
		yield return new WaitForSeconds(0.2F);
		Destroy(this.gameObject);
	}
}