using UnityEngine;
using System.Collections;

public class CicloE : MonoBehaviour {

    private bool executed = false;

    IEnumerator Start()
    {
		StartCoroutine("DoSomething", 2.0);
        yield return new WaitForSeconds(1);
		//StopCoroutine("DoSomething");
	}

    IEnumerator DoSomething(float someParameter)
    {
        /** /
		while (true) {
			print("DoSomething Loop");
            
            yield return null;
		}
        /**/

        int i = 0, j = 0;
        while (true)
        {
            Debug.Log("Ciclo1: " + i);
            while (j < 10)
            {
                Debug.Log( i + " Ciclo2: " + j);
                executed = false;

                
                while (!executed) { yield return null; };
                //Debug.Log(executed);
                j++;

                yield return null;
            }
            i++;
            j = 0;
        }
	}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            executed = true;
            Debug.Log("DONE");
        }
    }
}
