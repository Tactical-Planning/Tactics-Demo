using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AIScript : MonoBehaviour {

	[SerializeField] LevelManagerScript lmInstance;
	private string friendlyTag;
	
	private int numUnits;
	private int unitsActed;
	private bool unitActing;
	
	public GameObject activeUnit;
	
	[SerializeField] GameObject attackHighlight;
	[SerializeField] GameObject enemyCursor;

	// Use this for initialization
	void Start () {
			numUnits = 0;
			unitsActed = 0;
			activeUnit = null;
	}
	
	// Update is called once per frame
	void Update () {
		//check if all units to be played for the turn have completed their actions
		// if not, have the AI play again.
		if ( (!unitActing) && (unitsActed<numUnits) ) {
			unitActing = true;
			StartCoroutine(AIPlay());
			unitsActed++;
		}
	
	}
	
	
	// to be called at the beginning of a non player phase
	// established information necessary for AI play, such as what sort of units to manipulate. sets relevant data members
	public void NewPhase() {
		//check phase to see which AI player's turn it is
		//At the moment, only enemies have been implemented and allied units are a possible extension.
		if (lmInstance.phaseCounter == 1) {
			friendlyTag = "enemyUnit";
		} else if (lmInstance.phaseCounter == 2) {
			friendlyTag = "alliedUnit";
		}
		
		//count how many units need to be manipulated by the AI player.
		numUnits = 0;
		foreach(GameObject unit in lmInstance.unitList){
			if(unit.tag == friendlyTag){ numUnits++;};
		}
		
		unitsActed = 0;
		unitActing = false;
	}
	
	//AIPlay is called every time a unit is played. This is a greedy utility driven approach.
	//Each unit that has not yet made an action is considered. each unit is assigned a priority score based off of how much work it thinks it could do in combat that turn
	//the unit with the highest priority score is selected to act, and then performs the action associated with its high priority score (such as attack player unit A)
	//after the unit has acted AIPlay is called again, and it reconsiders the remaining units priority scores based on the updated game state.
	public IEnumerator AIPlay(){
		//consider units and assign priority scores.
		if(friendlyTag == "enemyUnit"){
			ConsiderationEnemy();
		}else{
			//ConsiderationAllied();
			
			//allied units are possible extension of project
		}

		// choose a unit to act
		GameObject tempUnit = null;
		foreach(GameObject unit in lmInstance.unitList){
			if((unit.tag == friendlyTag) &&(!unit.GetComponent<UnitScript>().finished)&&( tempUnit == null || (unit.GetComponent<UnitScript>().priority > tempUnit.GetComponent<UnitScript>().priority))){
				tempUnit = unit;
			}
		}
		
		// if chosen unit has priority 0, choose an action
		if(tempUnit.GetComponent<UnitScript>().priority == 0){
			// defender units will wait to be approached by player units, but will attack them if they are within range.
			if(tempUnit.GetComponent<UnitScript>().defender){
				tempUnit.GetComponent<UnitScript>().actionToTake = "wait";
			}else{
				tempUnit.GetComponent<UnitScript>().actionToTake = "move";
			}
		}
		
		// chosen unit takes action
		activeUnit = tempUnit;
		yield return new WaitForSeconds(.5f);
		
		StartCoroutine(TakeAction(tempUnit));
		UnitScript tempScript = tempUnit.GetComponent<UnitScript>();
		//wait until unit has finished its action for the turn
		yield return new WaitWhile(() => (!tempScript.finished));
		unitActing = false;
		
	}
	
	//ConsiderationEnemy iterates through the enemy units that are waiting to act
	//For each enemy unit, it looks for a "best" target for the unit to attack that turn, based on a simulation of combat between the unit and the target
	//after ConsiderationEnemy is called, each enemy unit's priority data member will be assigned and ready to be used to decide which unit gets to act first.
	public void ConsiderationEnemy() {
		//initialize priority scores, and associated "best" actions to 0 and null respectively
		foreach(GameObject unit in lmInstance.unitList){
			unit.GetComponent<UnitScript>().priority = 0;
			unit.GetComponent<UnitScript>().actionToTake = null;
		}
		//iterated through game's unit list
		foreach(GameObject unit in lmInstance.unitList) {
			//consider enemy units
			if (unit.tag == "enemyUnit" && !unit.GetComponent<UnitScript>().finished) {

				//populate lists of tiles the unit could move to, and tiles that it could attack this turn.
				unit.GetComponent<UnitScript>().FindAvailableMoves((int) unit.transform.position.x, (int) unit.transform.position.y, 0);
				unit.GetComponent<UnitScript>().FindAvailableAttacks((int) unit.transform.position.x, (int) unit.transform.position.y, 0);
				
				//consider potential targets
				foreach(GameObject targetUnit in lmInstance.unitList) {
					if (targetUnit.tag != "enemyUnit") {
						//check if that potential target is on a tile this unit could attack this turn
						foreach(List<int> tile in unit.GetComponent<UnitScript>().availableAttacks){
							if (((int) targetUnit.GetComponent<UnitScript>().tileCur.transform.position.x)==tile[0] && ((int) targetUnit.GetComponent<UnitScript>().tileCur.transform.position.y)==tile[1]) {
								
								//simulate combat sequence with that target, calculate a utility score for attacking that unit
								int temp = unit.GetComponent<UnitScript>().EstimateDamage(targetUnit);
								//check if that's the best combat encounter we've simulated so far, if so thats the target we want to attack at the moment
								if (temp > unit.GetComponent<UnitScript>().priority) {
									unit.GetComponent<UnitScript>().priority = temp;
									unit.GetComponent<UnitScript>().actionToTake = "attack";
									unit.GetComponent<UnitScript>().targetToTarget = targetUnit;
								}

							}
						}
					}
				}
				
			}
		}
		
		
	}
	
	//placeholder for possible allied unit extension
	
/* 	public void ConsiderationAllied() {
		foreach(GameObject unit in lmInstance.unitList) {
			
		}
	} */
	
	
	//TakeAction is called when a unit has been selected to act, and it's ideal action has been decided.
	//it takes the unit to act as its parameter.
	public IEnumerator TakeAction(GameObject unit){
		string action = unit.GetComponent<UnitScript>().actionToTake;
		
		//unit finishes its turn and waits
		if (action == "wait") {
			unit.GetComponent<UnitScript>().FinishAction();
		}
		
		else if (action == "move") {
			GameObject tempTarget = null;
			int tempPathLength = 999999;
			
			//calculate the path between this unit and all target units
			//identify the target that is closest, in terms of path distance.
			foreach(GameObject target in lmInstance.unitList){
				if ( (unit.tag == "enemyUnit" && target.tag != friendlyTag) || (unit.tag == "alliedUnit" && target.tag == "enemyUnit")) {
					//find path from unit to potential target
					unit.GetComponent<UnitScript>().FindPath(new int[] {(int) target.transform.position.x, (int) target.transform.position.y});
					if (unit.GetComponent<UnitScript>().pathToFollow.Count < tempPathLength) {
						//assign new move target if target being considered is the closest so far
						tempTarget = target;
						tempPathLength = unit.GetComponent<UnitScript>().pathToFollow.Count;
					}
				}
			}
			
			//find path to ideal target again, now that the closest target has been identified
			unit.GetComponent<UnitScript>().FindPath(new int[] {(int) tempTarget.GetComponent<UnitScript>().tileCur.transform.position.x, (int) tempTarget.GetComponent<UnitScript>().tileCur.transform.position.y});
			//a path of tiles to the target is generated
			List<List<int>> tempPath = unit.GetComponent<UnitScript>().pathToFollow;
			//truncate the path down the the number of tiles we can move this turn.
			while(tempPath.Count >  unit.GetComponent<UnitScript>().speed+1){
				tempPath.RemoveAt(tempPath.Count-1);
			}
			//check if that move destination is already occupied, if it is truncate the path further and stop before the obstruction.
			while (lmInstance.tileArray[tempPath[tempPath.Count-1][0],tempPath[tempPath.Count-1][1]].GetComponent<TileScript>().occupyingObject != null) {
				if (tempPath.Count > 1) {
					tempPath.RemoveAt(tempPath.Count-1);
				} else {
					break;
				}
			}	
			int tileInPath = 0;
			foreach(List<int> tile in tempPath){
				tileInPath++;
			}
			//move unit along calculated path.
			unit.GetComponent<UnitScript>().MoveUnit(lmInstance.tileArray[tempPath[tempPath.Count-1][0],tempPath[tempPath.Count-1][1]]);
			yield return new WaitWhile(() => unit.GetComponent<UnitScript>().moving);
			unit.GetComponent<UnitScript>().FinishAction();
			yield return new WaitForSeconds(1);
		}
		
		else if (action == "attack") {
				GameObject targetUnit = unit.GetComponent<UnitScript>().targetToTarget;
				//find tiles in range of target from which to attack with unit.
				unit.GetComponent<UnitScript>().FindAvailableImmediateAttacks((int) targetUnit.GetComponent<UnitScript>().tileCur.transform.position.x, (int) targetUnit.GetComponent<UnitScript>().tileCur.transform.position.y,0);

				List<int> possibleDest = null;
				float possibleDist = 0f;
				//considere tiles with range that unit could move to in order to stage their attack
				foreach(List<int> rangeTile in unit.GetComponent<UnitScript>().availableImmediateAttacks){
					foreach(List<int> tile in unit.GetComponent<UnitScript>().availableTiles){
						
						if((tile[0] == rangeTile[0]) && (tile[1] == rangeTile[1])){
							GameObject tileActual = lmInstance.tileArray[tile[0],tile[1]];
							//choose the tile furthest away from that target to attack from
							if(Vector3.Distance(tileActual.transform.position, targetUnit.GetComponent<UnitScript>().tileCur.transform.position)>possibleDist){
								possibleDest = tile;
								possibleDist = Vector3.Distance(tileActual.transform.position, targetUnit.GetComponent<UnitScript>().tileCur.transform.position);
							}
						}
					}
				}
				//find the path to use, and move to destination
				unit.GetComponent<UnitScript>().FindPath(new int[] {possibleDest[0],possibleDest[1]});
				unit.GetComponent<UnitScript>().MoveUnit(lmInstance.tileArray[possibleDest[0],possibleDest[1]]);
				//wait for unit to move into position before it attacks its target
				yield return new WaitWhile(() => unit.GetComponent<UnitScript>().moving);
				
				//mark target unit as such
				GameObject targetHighlight = Instantiate(enemyCursor,targetUnit.transform.position,Quaternion.identity) as GameObject;
				yield return new WaitForSeconds(1);
				
				//initiate combat sequence
				StartCoroutine(unit.GetComponent<UnitScript>().ResolveCombat(targetUnit));
				
				yield return new WaitForSeconds(1);
				Destroy(targetHighlight);
		}

	}

	
}
