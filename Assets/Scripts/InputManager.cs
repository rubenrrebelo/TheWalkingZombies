using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InputManager : MonoBehaviour {

	private List<GameObject> _selectedCharacters;
	private GameObject selectedObj;
	
	void Start () {

		_selectedCharacters = new List<GameObject>();

	}

	GameObject getClickedGameObject() { // Builds a ray from camera point of view to the mouse position 
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
		RaycastHit hit; // Casts the ray and get the first game object hit 
		if (Physics.Raycast(ray, out hit)) 
			return hit.transform.gameObject; 
		else 
			return null; 
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0)){
			selectedObj = getClickedGameObject().transform.root.gameObject;

			if(selectedObj.tag.Equals("Zombie") || selectedObj.tag.Equals("Survivor") && selectedObj != null){
				if(_selectedCharacters.Contains(selectedObj)){
					//send message to not display info
					if(selectedObj.tag.Equals("Zombie")){
						selectedObj.GetComponent<ZombieScript>().setDisplayInfo(false);
					}
					if(selectedObj.tag.Equals("Survivor")){
						selectedObj.GetComponent<SurvivorScript>().setDisplayInfo(false);
					}
					//remove from list of objects displaying info
					_selectedCharacters.Remove(selectedObj);
				}else{
					if(selectedObj.tag.Equals("Zombie")){
						selectedObj.GetComponent<ZombieScript>().setDisplayInfo(true);
					}
					if(selectedObj.tag.Equals("Survivor")){
						selectedObj.GetComponent<SurvivorScript>().setDisplayInfo(true);
					}
					_selectedCharacters.Add(selectedObj);
				}
			}
		}

		// Navigation

		if(Input.GetAxis("Mouse ScrollWheel") > 0){
			Camera.main.transform.Translate(new Vector3(0, 0, 100) * Time.deltaTime);
		}
		if(Input.GetAxis("Mouse ScrollWheel") < 0){
			Camera.main.transform.Translate(new Vector3(0, 0, -100) * Time.deltaTime);
		}

		if (Input.GetMouseButton(1)){
			// Change the direction of the camera according to the mouse movement
			Camera.main.transform.rotation *= Quaternion.AngleAxis(Time.deltaTime * Input.GetAxis("Mouse Y") * 300, Vector3.left);
			Camera.main.transform.RotateAround(new Vector3(0, 1.0f, 0), Time.deltaTime * Input.GetAxis("Mouse X") * 3);
		}

		if (Input.GetKey(KeyCode.D))
		{
			Camera.main.transform.Translate(new Vector3(10, 0, 0) * Time.deltaTime);
		}
		if (Input.GetKey(KeyCode.A))
		{
			Camera.main.transform.Translate(new Vector3(-10, 0, 0) * Time.deltaTime);
		}
		if (Input.GetKey(KeyCode.W))
		{
			Camera.main.transform.Translate(new Vector3(0, 0, 10) * Time.deltaTime);
		}
		if (Input.GetKey(KeyCode.S))
		{
			Camera.main.transform.Translate(new Vector3(0, 0, -10) * Time.deltaTime);
		}



	}
}
