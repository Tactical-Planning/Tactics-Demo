using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class LevelManagerScript : MonoBehaviour {

	// references to UI elements
	public GameObject characterSheet;
	public GameObject progressBar;

	// data members for camera movement
	public GameObject activeTile;
	public Vector3 activePosition;
	public float[] margin;
	public float[] edgeMargin;
	public float totalDist;
	public float prevZIndex;
	public Vector3 newCamera;
	
	// data members for turn logistics
	public int[] startCoords;
	public int activeTeam;
	public int turnCounter;
	public int phaseCounter;
	
	// representations of the tile grid
	public GameObject[,] tileArray;
	public int[] tileArrayLength;
	
	// list of valid spawn locations for level, set in inspector
	public List<GameObject> spawnLocations;
	
	// lists of units and relevant indexes
	public List<GameObject> unitList;
	public int unitListIndex;
	public List<GameObject> unitListInRange;
	public int unitListInRangeIndex;
	
	// references to PlayerScript and AIScript
	public PlayerScript player;
	public AIScript aiController;

	// list of conditions the player can meet to win
	public List<string> winConditions;
	
	// current level number
	public int levelNumber;
	
	// flag for checking if the level can end after the player has finished their turn
	public bool levelCanEnd;
	
	// startTime is used to track total play time
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
		
		// activePosition is where the focus of gameplay should be
		// in the player phase, this is where the cursor has moved to
		// in AI phases, this is where the active unit currently is
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

		// if activePosition is not outside the margins of camera focus, update newCamera
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
		
		// move the camera towards newCamera
		Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, newCamera, frac);
	
		Camera.main.GetComponent<Camera>().orthographicSize = player.zoomLevels[player.zIndex];
		
		if( newPosition ) {
			totalDist = Vector3.Distance(Camera.main.transform.position,newCamera);
			
		}
	}
	
	// PopulateAttackTargetsList populates the unitListInRange list with all the units in attackRange
	// (List<List<int>>) attackRange is the range to consider
	public void PopulateAttackTargetsList(List<List<int>> attackRange) {
		unitListInRange = new List<GameObject>();
		// for every tile in attackRange
		foreach (List<int> subList in attackRange) {
			// for every unit
			foreach(GameObject unit in unitList) {
				// check if unit is in range
				if (unit.GetComponent<UnitScript>().tileCur.transform.position.x == subList[0] && unit.GetComponent<UnitScript>().tileCur.transform.position.y == subList[1]) {
					unitListInRange.Add(unit);
					break;
				}
			}
		}
	}
	
	// NextPhase runs at the end of a phase
	// updates the phase counter
	// if the level should end, runs LevelEnd()
	public void NextPhase(){
		
		if (levelCanEnd) {
			LevelEnd();
		}
		
		SoundManager.instance.PhaseChange();
		
		phaseCounter++;
		
		// if the phaseCounter has gone past the AI turn, set phaseCounter to 0 and run NextTurn()
		if(phaseCounter>1){
			phaseCounter = 0;
			unitListIndex = 0;
			NextTurn();
		}
		
		// run NewTurn() for every unit about to act
		foreach (GameObject unit in unitList) {
			if (phaseCounter == 0 && unit.tag == "playerUnit") {
				unit.GetComponent<UnitScript>().NewTurn();
			}
			if (phaseCounter == 1 && unit.tag == "enemyUnit"){
				unit.GetComponent<UnitScript>().NewTurn();
			}
		}
		
		// if it is the player's turn, set focus to the first player unit
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
		}
	}
	
	// NextTurn is called in NewPhase()
	// increases turnCounter
	public void NextTurn(){
		turnCounter += 1;
		
		return;
	}
	
	// CheckLevelEnd is run whenever a potentially leven-ending event occurs
	// looks through conditions in winConditions to see if the player has won
	// if the level can end, sets levelCanEnd to true
	// if the player has lost, runs LevelLoss()
	public void CheckLevelEnd() {
		bool playerLoss = true;
		
		// if any player units are alive, the player has not necessarily lost
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
		
		// checks each win condition with logic too see if the level can end
		foreach (string winCondition in winConditions) {
			// if "Extermination" is a winCondition and the player has killed all enemy units, the level can end
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
	
	
	// LevelEnd runs in NewPhase() if levelCanEnd is true
	// updates playTime
	// increases levelNumber
	// if this was the last level, loads CreditScene
	// otherwise, saves party data and loads SaveScene
	public void LevelEnd() {
		
		SoundManager.instance.EndLevel();
		
		GameManager.instance.playTime += Time.time - startTime;
		GameManager.instance.GetComponent<GameManager>().levelNumber += 1;

		if (GameManager.instance.GetComponent<GameManager>().levelNumber > GameManager.instance.GetComponent<GameManager>().levelCount) {
			GameManager.instance.GetComponent<GameManager>().inCombat = false;
			SceneManager.LoadScene("CreditScene");
			return;
		}
		
		// updates the party list's data for each unit
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
				// if the unit was not in the party list, adds the unit to party list
				if(!found){
					GameManager.instance.GetComponent<GameManager>().partyUnits.Add(unit);
				}
			}
			
		}
		GameManager.instance.GetComponent<GameManager>().inCombat = false;
		GameManager.instance.GetComponent<GameManager>().Save("/playerInfo.dat");
		SceneManager.LoadScene("SaveMenuScene");
	}
	
	
	// LevelLoss runs in CheckLevelEnd() if a loss condition has been method
	// loads the GameOverScene
	public void LevelLoss() {
		SoundManager.instance.QuitGame();
		SceneManager.LoadScene("GameOverScene");
	}
	
	// PlaceUnit is run at the start of a level
	// (GameObject) unit gives their start location data members, places unit in that location
	public void PlaceUnit(GameObject unit){
		
		//startX and startY are to be set before this method is called
		int spawnX = unit.GetComponent<UnitScript>().startX;
		int spawnY = unit.GetComponent<UnitScript>().startY;
		
		tileArray[spawnX,spawnY].GetComponent<TileScript>().occupyingObject = unit;	
		unit.GetComponent<UnitScript>().tileCur = tileArray[spawnX,spawnY];
		unitList.Add(unit);
		
		unit.transform.position = new Vector3((float)spawnX,(float)spawnY,unit.transform.position.z);
		
	}
	
	// RemoveUnit is used in level set-up, when choosing which units to place in the level
	// (GameObject) unit is the unit to be removed
	// removes unit from the level, and from unitList
	public void RemoveUnit(GameObject unit){
		
		GameObject locationTile = unit.GetComponent<UnitScript>().tileCur;
		int locX = (int) locationTile.transform.position.x;
		int locY = (int) locationTile.transform.position.y;
		tileArray[locX,locY].GetComponent<TileScript>().occupyingObject = null;	
		unit.GetComponent<UnitScript>().tileCur = null;
		unitList.Remove(unit);
		
	}
}
