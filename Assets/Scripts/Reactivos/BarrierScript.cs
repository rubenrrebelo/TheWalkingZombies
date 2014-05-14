using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BarrierScript: MonoBehaviour {
	
	public float _healthLevel;
	private const float FULL_HEALTH = 100.0f;

    private const float BARRIER_COOLDOWN = 10.0f;

	private Vector3 currentScreenPos;
	private float lifebar_x_offset, lifebar_y_offset;
	private Texture2D life_bar_green, life_bar_red;
	private float lifebar_lenght, lifebar_height;

	private bool _dead;

    private Vector3 _initialPos;
	
	void Start () {

        _initialPos = this.gameObject.transform.position;

		_healthLevel = FULL_HEALTH;
		
		life_bar_green = (Texture2D)Resources.Load(@"Textures/life_bar_green", typeof(Texture2D));
		life_bar_red = (Texture2D)Resources.Load(@"Textures/life_bar_red", typeof(Texture2D));
		
		lifebar_lenght = 30.0f;
		lifebar_height = 4.0f;
		lifebar_x_offset = -15.0f;
		lifebar_y_offset = -3.0f;

		_dead = false;
	}

	void OnGUI(){
		if(this.renderer.isVisible && !_dead){
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
		//TODO: base leader needs to know this!
		_healthLevel -= ammount;
		if(_healthLevel <= 0 && !_dead){
			//to make it "disappear"
			this.transform.position = new Vector3(700, 0, 700.0f);
			_dead = true;
			StartCoroutine("destroyAfterDeath");
		}
	}
	
	private IEnumerator destroyAfterDeath(){
		yield return new WaitForSeconds(0.2F);
		Debug.Log("Destroyed: "+ this.name);

        yield return new WaitForSeconds(BARRIER_COOLDOWN);
        this.transform.position = _initialPos;
        _dead = false;
        _healthLevel = FULL_HEALTH *0.1f; //begins again from 10% of full health

		//Destroy(this.gameObject);
        //this.gameObject.transform.GetComponent<BoxCollider>().enabled = false;
        //this.gameObject.transform.GetComponent<MeshRenderer>().renderer.enabled = false;
	}

    public void repairBase(float ammount)
    {
        if (_dead) return;

        _healthLevel += ammount;
        if (_healthLevel >=  FULL_HEALTH)
        {
            _healthLevel = FULL_HEALTH;
        }
    }

    public bool needRepair()
    {
        return !_dead && _healthLevel < FULL_HEALTH && _healthLevel >= 0; //TODO: Repair always?
    }

    public float getBarrierHealth()
    {
        return _healthLevel;
    }
}