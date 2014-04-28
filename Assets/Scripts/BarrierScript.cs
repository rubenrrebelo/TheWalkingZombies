using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BarrierScript: MonoBehaviour {
	
	public float _healthLevel;
	private const float FULL_HEALTH = 100.0f;

	private Vector3 currentScreenPos;
	private float lifebar_x_offset, lifebar_y_offset;
	private Texture2D life_bar_green, life_bar_red;
	private float lifebar_lenght, lifebar_height;

	private bool _dead;
	
	void Start () {
		
		_healthLevel = FULL_HEALTH;
		
		life_bar_green = (Texture2D)Resources.Load(@"Textures/life_bar_green", typeof(Texture2D));
		life_bar_red = (Texture2D)Resources.Load(@"Textures/life_bar_red", typeof(Texture2D));
		
		lifebar_lenght = 30.0f;
		lifebar_height = 4.0f;
		lifebar_x_offset = -15.0f;
		lifebar_y_offset = -8.0f;

		_dead = false;
	}

	void OnGUI(){
		if(this.renderer.isVisible){
			//Important, order matters!
			currentScreenPos = Camera.main.WorldToScreenPoint(this.transform.position);
			GUI.DrawTexture(new Rect(currentScreenPos.x + lifebar_x_offset, Screen.height - currentScreenPos.y + lifebar_y_offset, 
			                         lifebar_lenght, 
			                         lifebar_height), life_bar_red);
			GUI.DrawTexture(new Rect(currentScreenPos.x + lifebar_x_offset, Screen.height - currentScreenPos.y + lifebar_y_offset,
			                         (FULL_HEALTH - (FULL_HEALTH - _healthLevel))*lifebar_lenght/FULL_HEALTH, 
			                         lifebar_height), life_bar_green);
		}
	}
	
	void Update () {
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
	
	private IEnumerator destroyAfterDeath(){
		yield return new WaitForSeconds(0.2F);
		//Debug.Log("Destroyed: "+ this.name);
		Destroy(this.gameObject);
	}
}