using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class GameManager : MonoBehaviour
{

	public static GameManager instance = null;     
	private LevelGenerator levelGenerator;
	[SerializeField] List<string> levelNames;
	public int levelNumber;
	public int levelCount;
	public List<GameObject> partyUnits;
	public List<string> partyEquipList;
	[SerializeField] List<string> initialEquipList;
	
	[SerializeField] List<GameObject> unitPrefabs;
	[SerializeField] List<GameObject> itemPrefabs;
	
	public Dictionary<String,GameObject> equipDict;
	[SerializeField] List<GameObject> equipList;
	
	public float playTime;
	
	public bool inCombat;
	//Awake is always called before any Start functions
	void Awake()
	{
		//Check if instance already exists
		if (instance == null) {
			
			//if not, set instance to this
			instance = this;
			inCombat = false;
			
			partyUnits = new List<GameObject>();
			
			//Sets this to not be destroyed when reloading scene
			DontDestroyOnLoad(gameObject);
	
			equipDict = new Dictionary<String, GameObject>();
			foreach(GameObject equip in equipList){
				equipDict[equip.GetComponent<EquipmentScript>().equipName] = equip;
			}
		
		}
		
		//If instance already exists and it's not this:
		else if (instance != this) {
			//Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
			Destroy(gameObject);    
		}

	}
	
	//Initializes the game for each level.
	public void InitGame()
	{
		SceneManager.LoadScene(levelNames[levelNumber]);
		
	}
	
	//saves game state to a file, given a file name
	//uses a class with serializable data to save
	public void Save(string fileName) {
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create(Application.persistentDataPath + fileName);
		
		PlayerData data = new PlayerData();
		
		//initialize lists to populate for saving
		data.currentLevel = levelNumber;
		data.unitStats = new List<Dictionary<string,int>>();
		data.unitNames = new List<Dictionary<string,string>>();
		data.unitItems = new List<List<string>>();
		data.unitEquipments = new List<List<string>>();
		data.partyEquipment = partyEquipList;
		data.playTime = playTime;
		
		
		//iterate through party units to save their information
		foreach(GameObject unit in partyUnits) {
			
			Dictionary<string,int> tempStats = new Dictionary<string,int>();
			Dictionary<string,string> tempNames = new Dictionary<string,string>();
			List<string> tempItems = new List<string>();
			List<string> tempEquips = new List<string>();
			
			//save unit stats
			tempStats.Add("speed",unit.GetComponent<UnitScript>().speed);
			tempStats.Add("maxHealth",unit.GetComponent<UnitScript>().maxHealth);
			tempStats.Add("range",unit.GetComponent<UnitScript>().range);
			tempStats.Add("triangle",unit.GetComponent<UnitScript>().triangle);
			tempStats.Add("numAttacks",unit.GetComponent<UnitScript>().numAttacks);
			tempStats.Add("defense",unit.GetComponent<UnitScript>().defense);
			tempStats.Add("agility",unit.GetComponent<UnitScript>().agility);
			tempStats.Add("attack",unit.GetComponent<UnitScript>().attack);
			tempStats.Add("level",unit.GetComponent<UnitScript>().level);
			tempStats.Add("experience",unit.GetComponent<UnitScript>().experience);
			
			//save name and class
			tempNames.Add("charName",unit.GetComponent<UnitScript>().charName);
			tempNames.Add("className",unit.GetComponent<UnitScript>().className);
			
			//iterate through items they hold, save to list
			foreach(GameObject item in unit.GetComponent<UnitScript>().itemList) {
				tempItems.Add(item.GetComponent<ItemScript>().itemName);
			}
			//add equipped gear to save list
			foreach(string equip in unit.GetComponent<UnitScript>().equipmentList){
				tempEquips.Add(equip);
			}
			//add lists to save file
			data.unitStats.Add(tempStats);
			data.unitNames.Add(tempNames);
			data.unitItems.Add(tempItems);
			data.unitEquipments.Add(tempEquips);
		}
		data.Version = "v1.0";
		
		bf.Serialize(file,data);
		file.Close();
		
	}
	
	//loads save data from file, utilizing serializable class we defined.
	public bool Load(string fileName) {
		partyUnits = new List<GameObject>();
		
		BinaryFormatter bf = new BinaryFormatter();
		//if the specified file doesn't exist, it can't be loaded.
		if (!(File.Exists(Application.persistentDataPath + fileName))) {
			return false;
		}
		
		FileStream file = File.Open(Application.persistentDataPath + fileName, FileMode.Open);
		
		PlayerData data = (PlayerData)bf.Deserialize(file);
		file.Close();
		
		levelNumber = data.currentLevel;
		partyEquipList = data.partyEquipment;
		
		playTime = data.playTime;
		
		
		//iterate through units
		for (int i=0; i < data.unitStats.Count; i++){
			GameObject tempUnit = null;
			//iterate through unit prefabs
			foreach(GameObject unit in unitPrefabs) {
				//instantiate prefab matching the name of a character listed in save file
				if ( (unit.GetComponent<UnitScript>().charName == data.unitNames[i]["charName"]) && (unit.GetComponent<UnitScript>().className == data.unitNames[i]["className"]) ) {
					tempUnit = Instantiate(unit);
				}
			}
			//modify prefab's stats according to save file
			tempUnit.GetComponent<UnitScript>().speed = data.unitStats[i]["speed"];
			tempUnit.GetComponent<UnitScript>().maxHealth = data.unitStats[i]["maxHealth"];
			tempUnit.GetComponent<UnitScript>().health = tempUnit.GetComponent<UnitScript>().maxHealth;
			tempUnit.GetComponent<UnitScript>().range = data.unitStats[i]["range"];
			tempUnit.GetComponent<UnitScript>().triangle = data.unitStats[i]["triangle"];
			tempUnit.GetComponent<UnitScript>().numAttacks = data.unitStats[i]["numAttacks"];
			tempUnit.GetComponent<UnitScript>().defense = data.unitStats[i]["defense"];
			tempUnit.GetComponent<UnitScript>().agility = data.unitStats[i]["agility"];
			tempUnit.GetComponent<UnitScript>().attack = data.unitStats[i]["attack"];
			tempUnit.GetComponent<UnitScript>().level = data.unitStats[i]["level"];
			tempUnit.GetComponent<UnitScript>().experience = data.unitStats[i]["experience"];
			
			//populate item list for unit
			tempUnit.GetComponent<UnitScript>().itemList = new List<GameObject>();
			foreach(string itemString in data.unitItems[i]) {
				foreach(GameObject item in itemPrefabs) {
					if (itemString == item.GetComponent<ItemScript>().itemName) {
						tempUnit.GetComponent<UnitScript>().itemList.Add(item);
					}
				}
			}
			//populate equipment list, but don't re-equip, stats are saved in the equipped state
			tempUnit.GetComponent<UnitScript>().equipmentList = new List<string>();
			if(data.unitEquipments!=null){
				foreach(string equipString in data.unitEquipments[i]){
					tempUnit.GetComponent<UnitScript>().equipmentList.Add(equipString);
				}
			}
			if(!inCombat){
				tempUnit.SetActive(false);
			}
			//add unit to party
			partyUnits.Add(tempUnit);
			
		}
		
		return true;
	}
	
	//delete the specified file
	public void FileDelete(string fileName) {
		
		File.Delete(Application.persistentDataPath + fileName);
		
	}
	
	//reset game state for a new game playthrough.
	public void ClearData() {
		
		levelNumber = 0;
		playTime = 0f;
		
		partyUnits = new List<GameObject>();
		partyEquipList = initialEquipList;

	}
	
	
	
	//Gets data from file to display on file slots for Save/Load, returns dictionary of values to display
	//if file empty, returns Dict with only "Empty" Key/Value
	//used to populate information on file slots in save/load menus
	public Dictionary<string, string> GetFileInfo(string fileName){
		
		BinaryFormatter bf = new BinaryFormatter();
		

		Dictionary<string,string> returnDict = new Dictionary<string,string>();
		
		//if file empty, return that value.
		if(!(File.Exists(Application.persistentDataPath + fileName))){
			returnDict["Empty"] = "Empty";
			return returnDict;
		}
		
		
		FileStream file = File.Open(Application.persistentDataPath + fileName, FileMode.Open);
		
		returnDict["Empty"] = "NotEmpty";
		
		PlayerData data = (PlayerData)bf.Deserialize(file);
		file.Close();
		
		TimeSpan span = TimeSpan.FromSeconds((double) Math.Floor(data.playTime));
		returnDict["PlayTime"] = span.ToString();
		
		returnDict["Chapter"] = "Chapter " + (data.currentLevel+1).ToString();
		
		return returnDict;
		
	}
	
}




//class for save data.
[Serializable]
class PlayerData {
	public string Version;
	
	public List<Dictionary<string,int>> unitStats;
	public List<Dictionary<string,string>> unitNames;
	public List<List<string>> unitItems;
	public List<List<string>> unitEquipments;
	public List<string> partyEquipment;
	public float playTime;

	// levels completed
	public int currentLevel;
	
}
