using UnityEngine;
using System.Collections;

//This script is used to initially generate the tile grids to be used for level creation in the editor.

public class GridGenerator : MonoBehaviour {

	public GameObject tile;
	public GameObject tileArray;
	public GameObject LevelManager;
	public GameObject Cursor;
	
	private Transform grid;
	private Transform map;

	public int mapX;
	public int mapY;
	private int startX;
	private int startY;

	// Use this for initialization
	void Start () {
		startX = 3;
		startY = 3; 
		map = new GameObject("Map").transform;
		grid = new GameObject("Grid").transform;
		grid.SetParent(map);
		
		
		//place cursor at start location
		GameObject cursorInstance = Instantiate(Cursor,new Vector3(startX,startY,0f),Quaternion.identity) as GameObject;
		cursorInstance.GetComponent<PlayerScript>().location = new int[] {startX,startY};
		
		//instantiate a LevelManager object
		GameObject lmInstance = Instantiate(LevelManager,new Vector3(0,0,0f),Quaternion.identity) as GameObject;
		lmInstance.GetComponent<LevelManagerScript>().tileArray = new GameObject[mapX,mapY];
		
		
		map.SetParent(lmInstance.transform);
		cursorInstance.transform.SetParent(lmInstance.transform);
		
		//generate a tile grid of the given size
		for (int y=0; y < mapY; y++)
		{
			for (int x=0; x < mapX; x++)
			{	
				GameObject instance = Instantiate(tile, new Vector3(x,y,0f),Quaternion.identity) as GameObject;
				instance.transform.SetParent(grid);
				lmInstance.GetComponent<LevelManagerScript>().tileArray[x,y] = instance;
			}
		}
	
	}
	
}
