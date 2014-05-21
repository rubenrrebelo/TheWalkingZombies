using UnityEngine;
using System.Collections;

public class Ciclo : MonoBehaviour {

    private bool executed = false;

	// Use this for initialization
	void Start () {

        StartCoroutine(myCiclo());
	}

    IEnumerator myCiclo()
    {
        /** /
        int i = 0, j = 0;
        while (true)
        {
            Debug.Log("Ciclo1: " + i);
            while (j < 10)
            {
                Debug.Log("Ciclo2: " + j);
                executed = false;
                while (!executed) ;
                Debug.Log(executed);
                j++;
            }
            i++;
            j = 0;
        }
        /**/
        /**/
        while(true)
            Debug.Log("Doing");
        /**/
    }

	// Update is called once per frame
    /** /
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            executed = true;
            Debug.Log("DONE");
        }

    }

    /**/
}
