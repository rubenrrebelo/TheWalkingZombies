using UnityEngine;
using System.Collections;

public class Map : MonoBehaviour {

    private string _survivorNameMap;
    private GameObject[][] _explorerMap;

    private GameObject target;

    public Vector2 mapCenter = new Vector2();

    public static int MAP_WIDTH = 50;
    public static int MAP_HEIGHT = 50;
    public static int MAP_QUAD_DIMENSIONS = 20;

    private void ResetMap()
    {
        for (int i = 0; i < Map.MAP_WIDTH; i++)
            for (int j = 0; j < Map.MAP_HEIGHT; j++)
                _explorerMap[i][j].SetActive(false);

        
    }

	// Use this for initialization
	void Start () {
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

        GameObject.Find("CameraMap").transform.position = new Vector3((MAP_WIDTH / 2 - MAP_WIDTH / 2.0f + 0.5f) + mapCenter.x, 10, (MAP_HEIGHT/2 - MAP_HEIGHT / 2.0f + 0.5f) + mapCenter.y);

        ResetMap();

        _survivorNameMap = "";

        target = GameObject.CreatePrimitive(PrimitiveType.Quad);
        target.renderer.material.color = Color.blue;
        target.layer = 8;
        target.transform.rotation = Quaternion.Euler(90, 0, 0);

	}

    
	
	// Update is called once per frame
	public void UpdateMap (string name, bool[][] map, Vector2 newPos) {
        if (name != _survivorNameMap)
        {
            ResetMap();
            for (int i = 0; i < MAP_WIDTH; i++)
                for (int j = 0; j < MAP_HEIGHT; j++)
                    _explorerMap[i][j].SetActive(map[i][j]);
        }

        _explorerMap[(int)newPos.x][(int)newPos.y].SetActive(true);

        Vector3 pos = _explorerMap[(int)newPos.x][(int)newPos.y].transform.position;
        target.transform.position = new Vector3(pos.x, 3, pos.z);

        GameObject.Find("CameraMap").transform.position = new Vector3(target.transform.position.x, 10, target.transform.position.z);
	}


}
