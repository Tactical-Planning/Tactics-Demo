using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour {

	[SerializeField] GameObject tile;
	[SerializeField] GameObject tileArray;
	[SerializeField] GameObject LevelManager;
	[SerializeField] GameObject Cursor;
	[SerializeField] GameObject unit;
	[SerializeField] GameObject unitHorseman;
	[SerializeField] GameObject unitArcher;
	[SerializeField] GameObject unitKnight;
	[SerializeField] GameObject enemyUnit;
	
	private Transform grid;
	private Transform map;
	
	private int mapX;
	private int mapY;
	private int startX;
	private int startY;
	
	void Awake()
	{
		//game state is set to inCombat, as this is Awake function is called at the beginning of a combat level
		GameManager.instance.GetComponent<GameManager>().inCombat = true;
		
		//find the LevelManager, then initialize its arrays/lists for holding the tiles and units present in the scene
		GameObject lmInstance = GameObject.Find("LevelManager(Clone)");
		mapX = lmInstance.GetComponent<LevelManagerScript>().tileArrayLength[0];
		mapY = lmInstance.GetComponent<LevelManagerScript>().tileArrayLength[1];
		lmInstance.GetComponent<LevelManagerScript>().tileArray = new GameObject[mapX,mapY];
		lmInstance.GetComponent<LevelManagerScript>().unitList = new List<GameObject>();
		int numTiles = mapX*mapY;
		//populate the array of tiles by iterating through the grid present in the scene.
		for (int i=0; i<numTiles; i++){
			GameObject tempTile = lmInstance.transform.GetChild(0).GetChild(0).GetChild(i).gameObject;
			lmInstance.GetComponent<LevelManagerScript>().tileArray[(int) tempTile.transform.position.x, (int) tempTile.transform.position.y] = tempTile;
		}
		
		
		GameObject unitInstance;
		//New units to be introduced to the party can be introduced on a case by case basis in each level
		//for now, the demo introduces 3 player units in the first level
		if(GameManager.instance.GetComponent<GameManager>().levelNumber == 0){
			//prefabs are instantiated, equipped, and placed on the map
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
			//otherwise, the party is loaded, and placed into start location tiles
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
		//place enemy units in level and add to unitList
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
