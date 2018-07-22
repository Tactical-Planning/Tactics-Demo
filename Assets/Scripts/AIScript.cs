using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AIScript : MonoBehaviour {

	public LevelManagerScript lmInstance;
	public string friendlyTag;
	
	private int numUnits;
	private int unitsActed;
	public bool unitActing;
	
	public GameObject activeUnit;
	
	public GameObject attackHighlight;
	public GameObject enemyCursor;

	// Use this for initialization
	void Start () {
			numUnits = 0;
			unitsActed = 0;
			activeUnit = null;
	}
	
	// Update is called once per frame
	void Update () {
		//Debug.Log("update: unitsActed " + unitsActed + " numUnits "+ numUnits);
		if ( (!unitActing) && (unitsActed<numUnits) ) {
			//Debug.Log("this check is passing");
			unitActing = true;
			StartCoroutine(AIPlay());
			unitsActed++;
		}
	
	}
	
	public void NewPhase() {
		
		
		
		if (lmInstance.phaseCounter == 1) {
			friendlyTag = "enemyUnit";
		} else if (lmInstance.phaseCounter == 2) {
			friendlyTag = "alliedUnit";
		}
		
		numUnits = 0;
		foreach(GameObject unit in lmInstance.unitList){
			if(unit.tag == friendlyTag){ numUnits++;};
		}
		
		unitsActed = 0;
		unitActing = false;
	}
	
	public IEnumerator AIPlay(){
		//Debug.Log("Starting AIPlay");
		if(friendlyTag == "enemyUnit"){
			//Debug.Log("Considering Enemies");
			ConsiderationEnemy();
		}else{
			//ConsiderationAllied();
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
			if(tempUnit.GetComponent<UnitScript>().defender){
				tempUnit.GetComponent<UnitScript>().actionToTake = "wait";
			}else{
				tempUnit.GetComponent<UnitScript>().actionToTake = "move";
			}
		}
		
		// chosen unit takes action
		//Debug.Log("We decided on a guy, his action is "+ tempUnit.GetComponent<UnitScript>().actionToTake);
		
		activeUnit = tempUnit;
		yield return new WaitForSeconds(.5f);
		
		StartCoroutine(TakeAction(tempUnit));
		UnitScript tempScript = tempUnit.GetComponent<UnitScript>();
		yield return new WaitWhile(() => (!tempScript.finished));
		//yield return new WaitForSeconds(1);
		unitActing = false;
		
	}
	
	
	public void ConsiderationEnemy() {
		//Debug.Log("Running Consideration");
		foreach(GameObject unit in lmInstance.unitList){
			unit.GetComponent<UnitScript>().priority = 0;
			unit.GetComponent<UnitScript>().actionToTake = null;
		}
		foreach(GameObject unit in lmInstance.unitList) {
			if (unit.tag == "enemyUnit" && !unit.GetComponent<UnitScript>().finished) {
				//Debug.Log("Considering");

				// if critical health and item
					// set priority 0
					// set actionToTake to healItem
					// set targetToTarget to self
		
				unit.GetComponent<UnitScript>().FindAvailableMoves((int) unit.transform.position.x, (int) unit.transform.position.y, 0);
				unit.GetComponent<UnitScript>().FindAvailableAttacks((int) unit.transform.position.x, (int) unit.transform.position.y, 0);
				//Debug.Log("Count of attacks " + unit.GetComponent<UnitScript>().availableAttacks.Count);
				
				foreach(GameObject targetUnit in lmInstance.unitList) {
					if (targetUnit.tag != "enemyUnit") {
						//Debug.Log("see an enemy on space " + targetUnit.transform.position.x + " " + targetUnit.transform.position.y);
						foreach(List<int> tile in unit.GetComponent<UnitScript>().availableAttacks){
							if (((int) targetUnit.GetComponent<UnitScript>().tileCur.transform.position.x)==tile[0] && ((int) targetUnit.GetComponent<UnitScript>().tileCur.transform.position.y)==tile[1]) {
								// calculate some damage, store in some temp
								// if that temp is > best
									// set priority
									// set action to attack
									// set target to targetUnit
								int temp = unit.GetComponent<UnitScript>().EstimateDamage(targetUnit);
								//Debug.Log("temp priority is" + temp);
								if (temp > unit.GetComponent<UnitScript>().priority) {
									//Debug.Log("Chose enemy to atatack");
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
	
/* 	public void ConsiderationAllied() {
		foreach(GameObject unit in lmInstance.unitList) {
			
		}
	} */
	
	public IEnumerator TakeAction(GameObject unit){
		string action = unit.GetComponent<UnitScript>().actionToTake;
		
		if (action == "wait") {
			unit.GetComponent<UnitScript>().FinishAction();
		}
		
		else if (action == "move") {
			GameObject tempTarget = null;
			int tempPathLength = 999999;
			foreach(GameObject target in lmInstance.unitList){
				if ( (unit.tag == "enemyUnit" && target.tag != friendlyTag) || (unit.tag == "alliedUnit" && target.tag == "enemyUnit")) {
					unit.GetComponent<UnitScript>().FindPath(new int[] {(int) target.transform.position.x, (int) target.transform.position.y});
					if (unit.GetComponent<UnitScript>().pathToFollow.Count < tempPathLength) {
						tempTarget = target;
						tempPathLength = unit.GetComponent<UnitScript>().pathToFollow.Count;
					}
				}
			}
			
			//Debug.Log("Nearest enemy is on space " + (int) tempTarget.transform.position.x + "," + (int) tempTarget.transform.position.y);
			
			unit.GetComponent<UnitScript>().FindPath(new int[] {(int) tempTarget.GetComponent<UnitScript>().tileCur.transform.position.x, (int) tempTarget.GetComponent<UnitScript>().tileCur.transform.position.y});
			List<List<int>> tempPath = unit.GetComponent<UnitScript>().pathToFollow;
			while(tempPath.Count >  unit.GetComponent<UnitScript>().speed+1){
				tempPath.RemoveAt(tempPath.Count-1);
			}
			Debug.Log("tempPath was truncated to "+tempPath.Count.ToString()+" many tiles long");
			
			while (lmInstance.tileArray[tempPath[tempPath.Count-1][0],tempPath[tempPath.Count-1][1]].GetComponent<TileScript>().occupyingObject != null) {
				if (tempPath.Count > 1) {
					tempPath.RemoveAt(tempPath.Count-1);
				} else {
					break;
				}
			}
			Debug.Log("tempPath was truncated to "+tempPath.Count.ToString()+" many tiles long = Post occupied-check");
			
			int tileInPath = 0;
			foreach(List<int> tile in tempPath){
				Debug.Log("tile: "+tileInPath.ToString()+ " X:"+tile[0].ToString()+" Y:"+tile[1].ToString());
				tileInPath++;
			}
			
			unit.GetComponent<UnitScript>().MoveUnit(lmInstance.tileArray[tempPath[tempPath.Count-1][0],tempPath[tempPath.Count-1][1]]);
			yield return new WaitWhile(() => unit.GetComponent<UnitScript>().moving);
			unit.GetComponent<UnitScript>().FinishAction();
			yield return new WaitForSeconds(1);
		}
		
		else if (action == "attack") {
				GameObject targetUnit = unit.GetComponent<UnitScript>().targetToTarget;
				unit.GetComponent<UnitScript>().FindAvailableImmediateAttacks((int) targetUnit.GetComponent<UnitScript>().tileCur.transform.position.x, (int) targetUnit.GetComponent<UnitScript>().tileCur.transform.position.y,0);
				//Debug.Log("Count of immediate attacks " + unit.GetComponent<UnitScript>().availableImmediateAttacks.Count);
				//Debug.Log("Count of move tiles " + unit.GetComponent<UnitScript>().availableTiles.Count);
				List<int> possibleDest = null;
				float possibleDist = 0f;
				foreach(List<int> rangeTile in unit.GetComponent<UnitScript>().availableImmediateAttacks){
					foreach(List<int> tile in unit.GetComponent<UnitScript>().availableTiles){
						//Debug.Log("tile is " + tile[0] + " " + tile[1] + " and rangeTile is " + rangeTile[0] + " " + rangeTile[1]);
						
						if((tile[0] == rangeTile[0]) && (tile[1] == rangeTile[1])){
							GameObject tileActual = lmInstance.tileArray[tile[0],tile[1]];
							//Debug.Log("Distance " + Vector3.Distance(tileActual.transform.position, targetUnit.GetComponent<UnitScript>().tileCur.transform.position));
							if(Vector3.Distance(tileActual.transform.position, targetUnit.GetComponent<UnitScript>().tileCur.transform.position)>possibleDist){
								possibleDest = tile;
								possibleDist = Vector3.Distance(tileActual.transform.position, targetUnit.GetComponent<UnitScript>().tileCur.transform.position);
								//Debug.Log("possible dist " + possibleDist);
							}
						}
					}
				}
				
				unit.GetComponent<UnitScript>().FindPath(new int[] {possibleDest[0],possibleDest[1]});
				//Debug.Log("possible dest is " + possibleDest[0] + "  " + possibleDest[1]);
				unit.GetComponent<UnitScript>().MoveUnit(lmInstance.tileArray[possibleDest[0],possibleDest[1]]);
				yield return new WaitWhile(() => unit.GetComponent<UnitScript>().moving);
				
				
				GameObject targetHighlight = Instantiate(enemyCursor,targetUnit.transform.position,Quaternion.identity) as GameObject;
				yield return new WaitForSeconds(1);
				
				StartCoroutine(unit.GetComponent<UnitScript>().ResolveCombat(targetUnit));
				
				yield return new WaitForSeconds(1);
				Destroy(targetHighlight);
		}
		
		//Debug.Log("Ending TakeAction");
	}

	
}
