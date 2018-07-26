using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class LevelManagerScript : MonoBehaviour {

	public GameObject characterSheet;
	public GameObject progressBar;

	public GameObject activeTile;
	public Vector3 activePosition;
	public float[] margin;
	public float[] edgeMargin;
	public float totalDist;
	public float prevZIndex;
	public Vector3 newCamera;
	
	
	public int[] startCoords;
	public int activeTeam;
	public int turnCounter;
	public int phaseCounter;
	
	public GameObject[,] tileArray;
	public int[] tileArrayLength;
	
	//list of valid spawn locations for level, set in inspector
	public List<GameObject> spawnLocations;
	
	public List<GameObject> unitList;
	public int unitListIndex;
	public List<GameObject> unitListInRange;
	public int unitListInRangeIndex;
	
	public PlayerScript player;
	public AIScript aiController;

	public List<string> winConditions;
	public int levelNumber;
	
	public bool levelCanEnd;
	
	public float startTime;

	// Use this for initialization
	void Start () {
		levelCanEnd = false;
		
		turnCounter = 0;
		phaseCounter = 0;
		
		activeTile = tileArray[startCoords[0],startCoords[1]];
		Camera.main.transform.position = new Vector3(activeTile.transform.position[0],activeTile.transform.position[1],Camera.main.transform.position[2]);
		margin = new float[] {2.0f, 1.0f, 2.0f, 3.0f};
		edgeMargin = new float[] {2.0f, 0.0f, 2.0f, 3.0f};
		totalDist = 1;
		newCamera = Camera.main.transform.position;
		
		//tileArrayLength = new int[] {tileArray.GetLength(0),tileArray.GetLength(1)};
		
		unitListIndex = 0;
		unitListInRangeIndex = 0;
		
		player = this.gameObject.transform.GetChild(1).GetComponent<PlayerScript>();
		
		prevZIndex = player.zIndex;
		
		winConditions.Add("Extermination");
		
		startTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		
		if (phaseCounter == 0) {
			activePosition = new Vector3(activeTile.transform.position[0],activeTile.transform.position[1],Camera.main.transform.position[2]);
		} else {
			if (aiController.activeUnit != null) {
				activePosition = new Vector3(aiController.activeUnit.transform.position[0],aiController.activeUnit.transform.position[1],Camera.main.transform.position[2]);
			}
		}
			
		bool newPosition = false;
		//Vector3 newCamera = Camera.main.transform.position;
		//Vector3 activePosition = new Vector3(activeTile.transform.position[0],activeTile.transform.position[1],Camera.main.transform.position[2]);
		//int distance = Vector3.Distance(Camera.main.transform.position,activePosition);
		
		float tempMarg = edgeMargin[player.zIndex];
		if (prevZIndex!=player.zIndex) {
			tempMarg = 0;
		} 
		prevZIndex = player.zIndex;

		if (activePosition.x-newCamera.x > margin[player.zIndex] && (!(activePosition.x > tileArrayLength[0]-tempMarg)||(!(newCamera.x > tileArrayLength[0]-tempMarg)) )) {
			newCamera.x += 1.0f;
			newPosition = true;
		}
		else if (newCamera.x-activePosition.x > margin[player.zIndex] && (!(activePosition.x < tempMarg) ||(!(newCamera.x < tempMarg)))) {
			newCamera.x -= 1.0f;
			newPosition = true;
		}
		if (activePosition.y-newCamera.y> margin[player.zIndex] && (!(activePosition.y > tileArrayLength[1]-tempMarg) ||(!(newCamera.y > tileArrayLength[1]-tempMarg)))) {
			newCamera.y += 1.0f;
			newPosition = true;
		}
		else if (newCamera.y-activePosition.y > margin[player.zIndex] && (!(activePosition.y < tempMarg) ||(!(newCamera.y < tempMarg)))) {
			newCamera.y -= 1.0f;
			newPosition = true;
		}
		
		if(totalDist<1.0f){
			totalDist = 1.0f;
		}
		float timeCap = Time.deltaTime;
		if(timeCap > (1f/60f)){
			timeCap = (1f/60f);
		}
		
		float frac = timeCap*totalDist;
		
		Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, newCamera, frac);
	
		Camera.main.GetComponent<Camera>().orthographicSize = player.zoomLevels[player.zIndex];
		
		if( newPosition ) {
			totalDist = Vector3.Distance(Camera.main.transform.position,newCamera);
			
		}


	}
	
	public void PopulateAttackTargetsList(List<List<int>> attackRange) {
		unitListInRange = new List<GameObject>();
		
		foreach (List<int> subList in attackRange) {
			foreach(GameObject unit in unitList) {
				if (unit.GetComponent<UnitScript>().tileCur.transform.position.x == subList[0] && unit.GetComponent<UnitScript>().tileCur.transform.position.y == subList[1]) {
					unitListInRange.Add(unit);
					break;
				}
			}
		}
	}
	
	public void NextPhase(){
		if (levelCanEnd) {
			LevelEnd();
		}
		SoundManager.instance.PhaseChange();
		phaseCounter++;
		if(phaseCounter>1){
			phaseCounter = 0;
			unitListIndex = 0;
			NextTurn();
		}
		foreach (GameObject unit in unitList) {
			if (phaseCounter == 0 && unit.tag == "playerUnit") {
				unit.GetComponent<UnitScript>().NewTurn();
			}
			if (phaseCounter == 1 && unit.tag == "enemyUnit"){
				unit.GetComponent<UnitScript>().NewTurn();
			}
		}
		if(phaseCounter == 0){
			foreach(GameObject unit in unitList){
				if(unit.tag == "playerUnit"){
					player.UnitFocus(unit);
					break;
				}
			}
			player.CheckInfoSheet();
		} else {
			player.infoSheet.SetActive(false);
		}
		
		if(phaseCounter == 1){
			aiController.NewPhase();
			//aiController.AIPlay();
		}
	}
	
	public void NextTurn(){
		turnCounter += 1;
		
		return;
	}
	
	public void CheckLevelEnd() {
		bool playerLoss = true;
		foreach (GameObject unit in unitList) {
			if ( unit.tag == "playerUnit" && !unit.GetComponent<UnitScript>().dead) {
				playerLoss = false;
			}
		}
		/* foreach(string lossCondition in lossConditions) {
			if (lossCondition == "Escort" && escortUnit.dead) {
				
			}
		} */
		
		bool playerWin = true;
		foreach (string winCondition in winConditions) {
			if (winCondition == "Extermination") {
				foreach (GameObject unit in unitList) {
					if ( unit.tag == "enemyUnit" && !unit.GetComponent<UnitScript>().dead) {
						playerWin = false;
					}
				}
			}
		}
		
		if (playerLoss) {
			levelCanEnd = true;
			LevelLoss();
		}
		
		if (playerWin) {
			levelCanEnd = true;
		}
	}
	
	public void LevelEnd() {
		
		SoundManager.instance.EndLevel();
		
		GameManager.instance.playTime += Time.time - startTime;
		GameManager.instance.GetComponent<GameManager>().levelNumber += 1;

		if (GameManager.instance.GetComponent<GameManager>().levelNumber > GameManager.instance.GetComponent<GameManager>().levelCount) {
			SceneManager.LoadScene("CreditScene");
			return;
		}
		
		foreach(GameObject unit in unitList){
			bool found = false;
			if(unit.tag == "playerUnit"){
				foreach(GameObject partyMember in GameManager.instance.GetComponent<GameManager>().partyUnits){
					if(unit.GetComponent<UnitScript>().charName == partyMember.GetComponent<UnitScript>().charName){
						int ind = GameManager.instance.GetComponent<GameManager>().partyUnits.IndexOf(partyMember);
						GameManager.instance.GetComponent<GameManager>().partyUnits[ind] = unit;
						found = true;
					}
				}
				if(!found){
					GameManager.instance.GetComponent<GameManager>().partyUnits.Add(unit);
				}
			}
			
		}
		GameManager.instance.GetComponent<GameManager>().inCombat = false;
		GameManager.instance.GetComponent<GameManager>().Save("/playerInfo.dat");
		SceneManager.LoadScene("SaveMenuScene");
	}
	
	public void LevelLoss() {
		SoundManager.instance.QuitGame();
		SceneManager.LoadScene("GameOverScene");
	}
	
	//method for placing units on map at beginning of level
	//takes unit as argument, reads their start location data members, places them in that location
	public void PlaceUnit(GameObject unit){
		
		//startX and startY are to be set before this method is called
		int spawnX = unit.GetComponent<UnitScript>().startX;
		int spawnY = unit.GetComponent<UnitScript>().startY;
		
		tileArray[spawnX,spawnY].GetComponent<TileScript>().occupyingObject = unit;	
		unit.GetComponent<UnitScript>().tileCur = tileArray[spawnX,spawnY];
		unitList.Add(unit);
		
		unit.transform.position = new Vector3((float)spawnX,(float)spawnY,unit.transform.position.z);
		
	}
	
	public void RemoveUnit(GameObject unit){
		
		GameObject locationTile = unit.GetComponent<UnitScript>().tileCur;
		int locX = (int) locationTile.transform.position.x;
		int locY = (int) locationTile.transform.position.y;
		//startX and startY are to be set before this method is called
		tileArray[locX,locY].GetComponent<TileScript>().occupyingObject = null;	
		unit.GetComponent<UnitScript>().tileCur = null;
		unitList.Remove(unit);
		
	}
}
