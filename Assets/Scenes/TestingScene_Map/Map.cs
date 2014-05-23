using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Map : MonoBehaviour {

    public GameObject _survivorObjMap;
    private List<GameObject> _mapOwners;
    private int ownersIndex = 0;

    private GameObject[][] _explorerMap;

    private GameObject target;

    public Vector2 mapCenter = new Vector2();

    public static int MAP_WIDTH = 100;
    public static int MAP_HEIGHT = 100;
    public static int MAP_QUAD_DIMENSIONS = 15;

    public static int MAP_EMPTY_POS = -2;
    public static int MAP_LIMIT_POS = -1;
    public static int MAP_NORMAL_POS = -3;

    public List<GameObject> _resources;
    
    private GameObject cameraMap;
    private GameObject cammap;

    private void ResetMap()
    {
        for (int i = 0; i < Map.MAP_WIDTH; i++)
        {
            for (int j = 0; j < Map.MAP_HEIGHT; j++)
            {
                _explorerMap[i][j].renderer.material.color = Color.green;
                _explorerMap[i][j].SetActive(false);
               
            }
        }
        /** /
        foreach (GameObject res in _resources)
        {
            Destroy(res);
        }
        /**/
    }

	// Use this for initialization
	void Start () {
        cameraMap = GameObject.Find("CameraMap");
        cammap = GameObject.Find("Map");


        _explorerMap = new GameObject[MAP_WIDTH][];
        for (int i = 0; i < MAP_WIDTH; i++)
        {
            _explorerMap[i] = new GameObject[MAP_HEIGHT];
            for (int j = 0; j < MAP_HEIGHT; j++)
            {
                _explorerMap[i][j] = GameObject.CreatePrimitive(PrimitiveType.Quad);
                _explorerMap[i][j].renderer.material.color = Color.green;
                _explorerMap[i][j].layer = 8;
                _explorerMap[i][j].transform.parent = GameObject.Find("Map").transform;

                float position_x = (i - MAP_WIDTH / 2.0f + 0.5f) + mapCenter.x;
                float position_y = (j - MAP_HEIGHT / 2.0f + 0.5f) + mapCenter.y;

                _explorerMap[i][j].transform.position = new Vector3(position_x, 0, position_y);
                _explorerMap[i][j].transform.rotation = Quaternion.Euler(90, 0, 0);
            }
        }

        cameraMap.transform.position = new Vector3((MAP_WIDTH / 2 - MAP_WIDTH / 2.0f + 0.5f) + mapCenter.x, 10, (MAP_HEIGHT/2 - MAP_HEIGHT / 2.0f + 0.5f) + mapCenter.y);

        ResetMap();

        //_survivorNameMap = "";

        target = GameObject.CreatePrimitive(PrimitiveType.Quad);
        target.transform.parent = cammap.transform;
        target.renderer.material.color = Color.blue;
        target.layer = 8;
        target.transform.rotation = Quaternion.Euler(90, 0, 0);

        _mapOwners = GameObject.FindGameObjectsWithTag("Survivor").ToList<GameObject>();
        _mapOwners.Add(GameObject.FindGameObjectWithTag("BaseLeader"));
        ownersIndex = _mapOwners.Count-1;
        if(_mapOwners.Count > 0)
            _survivorObjMap = _mapOwners[ownersIndex];

	}

    void LateUpdate()
    {
        if (Input.GetKeyUp(KeyCode.M))
        {
            ownersIndex++;
            if (ownersIndex >= _mapOwners.Count) ownersIndex = 0;
            _survivorObjMap = _mapOwners[ownersIndex];
            //RemakeMap(_mapOwners[ownersIndex].GetComponent<Survivor_Map>()._explorerMap);
        }

    }

    void OnGUI()
    {
        GUI.Label(new Rect(Screen.width - 100, Screen.height - 20, 100, 20), _survivorObjMap.name);
    }

    
	
	// Update is called once per frame
	public void UpdateMap (GameObject obj, int[][] map, Vector2 newPos, float resourceLevel, int type) {
        /** /
        
        /**/
        _explorerMap[(int)newPos.x][(int)newPos.y].SetActive(true);

        if (_explorerMap[(int)newPos.x][(int)newPos.y].renderer.material.color == Color.yellow)
        {
            if (resourceLevel == 0)
            {
                _explorerMap[(int)newPos.x][(int)newPos.y].renderer.material.color = Color.green;
            }
        }else
        if (resourceLevel >= 1)
        {
            _explorerMap[(int)newPos.x][(int)newPos.y].renderer.material.color = Color.yellow;

            //_resources.Add(_explorerMap[(int)newPos.x][(int)newPos.y]);
        }
        /**/
        if (type == Map.MAP_LIMIT_POS)
        {
            /** /
            //CODE for later
            GameObject ob = GameObject.CreatePrimitive(PrimitiveType.Quad);
            ob.renderer.material.color = Color.red;
            ob.layer = 8;
            ob.transform.parent = cammap.transform;

            float position_x = (newPos.x - MAP_WIDTH / 2.0f + 0.5f) + mapCenter.x;
            float position_y = (newPos.y - MAP_HEIGHT / 2.0f + 0.5f) + mapCenter.y;
            ob.transform.position = new Vector3(position_x, 3, position_y);
            ob.transform.rotation = Quaternion.Euler(90, 0, 0);
            /**/
            _explorerMap[(int)newPos.x][(int)newPos.y].renderer.material.color = Color.red;
            
        }
        /**/
        if (obj.Equals(_survivorObjMap))
        {
            Vector3 pos = _explorerMap[(int)newPos.x][(int)newPos.y].transform.position;
            target.transform.position = new Vector3(pos.x, 3, pos.z);

            cameraMap.transform.position = new Vector3(target.transform.position.x, 10, target.transform.position.z);
        }
	}

    private void RemakeMap(int[][] map)
    {
        
            ResetMap();

            for (int i = 0; i < MAP_WIDTH; i++)
            {
                for (int j = 0; j < MAP_HEIGHT; j++)
                {
                    _explorerMap[i][j].renderer.material.color = Color.green;

                    if (map[i][j] >= 1)
                    {
                        _explorerMap[i][j].renderer.material.color = Color.yellow;
                    }
                    else if (map[i][j] == MAP_LIMIT_POS)
                    {
                        _explorerMap[i][j].renderer.material.color = Color.red;
                    }

                    
                    _explorerMap[i][j].SetActive(map[i][j] != MAP_EMPTY_POS);
                }
           
        }
    }
}
