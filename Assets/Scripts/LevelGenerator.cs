using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour {

	public GameObject tile;
	public GameObject tileArray;
	public GameObject LevelManager;
	public GameObject Cursor;
	public GameObject unit;
	public GameObject unitHorseman;
	public GameObject unitArcher;
	public GameObject unitKnight;
	public GameObject enemyUnit;
	
	private Transform grid;
	private Transform map;
	
	private int mapX;
	private int mapY;
	private int startX;
	private int startY;
	
	// Use this for initialization
	//public void SetupScene (int dx, int dy) 
	void Awake()
	{
		GameManager.instance.GetComponent<GameManager>().inCombat = true;
		
		GameObject lmInstance = GameObject.Find("LevelManager(Clone)");
		mapX = lmInstance.GetComponent<LevelManagerScript>().tileArrayLength[0];
		mapY = lmInstance.GetComponent<LevelManagerScript>().tileArrayLength[1];
		lmInstance.GetComponent<LevelManagerScript>().tileArray = new GameObject[mapX,mapY];
		lmInstance.GetComponent<LevelManagerScript>().unitList = new List<GameObject>();
		int numTiles = mapX*mapY;
		for (int i=0; i<numTiles; i++){
			GameObject tempTile = lmInstance.transform.GetChild(0).GetChild(0).GetChild(i).gameObject;
			lmInstance.GetComponent<LevelManagerScript>().tileArray[(int) tempTile.transform.position.x, (int) tempTile.transform.position.y] = tempTile;
		}
		
		GameObject unitInstance;
		if(GameManager.instance.GetComponent<GameManager>().levelNumber == 1){
			unitInstance = Instantiate(unitArcher, new Vector3(3,3,0f),Quaternion.identity) as GameObject;
			lmInstance.GetComponent<LevelManagerScript>().tileArray[3,3].GetComponent<TileScript>().occupyingObject = unitInstance;
			unitInstance.GetComponent<UnitScript>().tileCur = lmInstance.GetComponent<LevelManagerScript>().tileArray[3,3];
			unitInstance.GetComponent<UnitScript>().Equip(GameManager.instance.equipDict["Bow"].GetComponent<EquipmentScript>(),0);
			lmInstance.GetComponent<LevelManagerScript>().unitList.Add(unitInstance);
			
			unitInstance = Instantiate(unitHorseman, new Vector3(4,6,0f),Quaternion.identity) as GameObject;
			lmInstance.GetComponent<LevelManagerScript>().tileArray[4,6].GetComponent<TileScript>().occupyingObject = unitInstance;	
			unitInstance.GetComponent<UnitScript>().tileCur = lmInstance.GetComponent<LevelManagerScript>().tileArray[4,6];
			unitInstance.GetComponent<UnitScript>().Equip(GameManager.instance.equipDict["Lance"].GetComponent<EquipmentScript>(),0);
			lmInstance.GetComponent<LevelManagerScript>().unitList.Add(unitInstance);
			
			unitInstance = Instantiate(unitKnight, new Vector3(4,4,0f),Quaternion.identity) as GameObject;
			lmInstance.GetComponent<LevelManagerScript>().tileArray[4,4].GetComponent<TileScript>().occupyingObject = unitInstance;	
			unitInstance.GetComponent<UnitScript>().tileCur = lmInstance.GetComponent<LevelManagerScript>().tileArray[4,4];
			unitInstance.GetComponent<UnitScript>().Equip(GameManager.instance.equipDict["Ruby Sword"].GetComponent<EquipmentScript>(),0);
			lmInstance.GetComponent<LevelManagerScript>().unitList.Add(unitInstance);
		}else{
			GameManager.instance.GetComponent<GameManager>().Load("/playerInfo.dat");
			List<GameObject> party = GameManager.instance.GetComponent<GameManager>().partyUnits;
			List<GameObject> tiles = lmInstance.GetComponent<LevelManagerScript>().spawnLocations;
			
			int p = 0;
			foreach(GameObject tile in tiles){
				
				if(!(p>=party.Count)){
					party[p].GetComponent<UnitScript>().startX = (int) tile.transform.position.x;
					party[p].GetComponent<UnitScript>().startY = (int) tile.transform.position.y;
					
					lmInstance.GetComponent<LevelManagerScript>().PlaceUnit(party[p]);
					p++;
				}
				
			}
			
			
		}
		
		for (int j = 0; j < lmInstance.transform.GetChild(0).GetChild(1).childCount; j++ ) {
			unitInstance = lmInstance.transform.GetChild(0).GetChild(1).GetChild(j).gameObject;
			UnitScript tempUnit = unitInstance.GetComponent<UnitScript>();
			lmInstance.GetComponent<LevelManagerScript>().tileArray[tempUnit.startX,tempUnit.startY].GetComponent<TileScript>().occupyingObject = unitInstance;
			tempUnit.tileCur = lmInstance.GetComponent<LevelManagerScript>().tileArray[tempUnit.startX,tempUnit.startY];
			unitInstance.GetComponent<UnitScript>().charName = "brigand";
			lmInstance.GetComponent<LevelManagerScript>().unitList.Add(unitInstance);
		}

	}
	
	
}
