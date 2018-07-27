using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UnitScript : MonoBehaviour {

	// the tile the unit is currently occupying
	public GameObject tileCur;
	// the tile the unit occupied before moving
	public GameObject tilePrev;
	
	// references for leveling up
	public GameObject levelUpSheet;
	public GameObject progressBar;
	
	// data members for AI units to determine what action to take
	public int priority; 
	public string actionToTake; 
	public GameObject targetToTarget;
	public bool defender;
	
	// unit stats
	public string charName;
	public string className;
	public int level;
	public int experience;
	public int health;
	public int maxHealth;
	public float maxHealthGrowth;
	public int speed;
	public int range;
	public int triangle;
	public int numAttacks;
	public int attack;
	public float attackGrowth;
	public int defense;
	public float defenseGrowth;
	public int agility;
	public float agilityGrowth;
	
	public int[] statArray;
	public float[] statGrowthArray;
	
	public bool gainingExperience;
	public bool experienceUpdating;
	
	// lists of tiles to determine what is in range
	public List<List<int>> availableTiles;
	public List<List<int>> availableHLTiles;
	public List<List<int>> availableAttacks;
	public List<List<int>> availableImmediateAttacks;
	
	// pathing data members
	public float[,] pathCostTable;
	public List<List<int>> tilesToCheck;
	public int[,] pathPrevXTable;
	public int[,] pathPrevYTable;
	public List<List<int>> pathToFollow;
	
	// data members for moving
	private float startTime;
	private float totalDist;
	public bool moving;
	private int moveIndex;
	
	// reference to level manager script
	private LevelManagerScript levelManagerScript;
	
	// start locations for the unit
	public int startX;
	public int startY;
	
	// flag to see if unit is selected
	public bool selectedFlag;
	//flag to see if unit has finished actions for the round
	public bool finished;
	
	// list of items the unit is carrying
	public List<GameObject> itemList;
	// the index of the currently chosen item
	public int itemIndex;

	// list of equipment the unit has
	public List<string> equipmentList;
	
	// list of statuses afflicting the unit
	public List<string> statusList;
	// list of how long the statuses last
	public List<int> statusDurationList;
	
	// flag to see if unit is alive or dead
	public bool dead;
	
	public GameObject damageText;

	//Use this for initialization
	void Start () {

		// initialize data members
		statArray = new int[]{maxHealth, attack, defense, agility};
		statGrowthArray = new float[]{maxHealthGrowth, attackGrowth, defenseGrowth, agilityGrowth};
		experienceUpdating = false;
		
		tilePrev= null;
		priority = 0;
		
		// if unit is in combat, find the level manager and UI elements
		if(GameManager.instance.GetComponent<GameManager>().inCombat){
			availableTiles = new List<List<int>>();
			levelManagerScript = GameObject.Find("LevelManager(Clone)").GetComponent<LevelManagerScript>();
			levelUpSheet = levelManagerScript.characterSheet;
			progressBar = levelManagerScript.progressBar;
		}
		
		selectedFlag = false;
		finished = false;
		dead = false;
		
		defender = false;
		
		
		moving = false;
		itemIndex = 0;
		
		// instantiate the items held by the unit
		foreach (GameObject item in itemList) {
			item.GetComponent<ItemScript>().ItemInstantiate();
			item.GetComponent<ItemScript>().holder = gameObject;
		}
		
		statusList = new List<string>();
		statusDurationList = new List<int>();
	}
	
	//Update is called once per frame
	void FixedUpdate () {
		// if the unit is moving, update the unit's position
		if(moving){
			
			float frac = Time.deltaTime*(Time.time - startTime)*300/totalDist;
			
			this.transform.position = Vector3.Lerp(this.transform.position, levelManagerScript.tileArray[pathToFollow[moveIndex][0],pathToFollow[moveIndex][1]].transform.position, frac);
			
			if(this.transform.position == levelManagerScript.tileArray[pathToFollow[moveIndex][0],pathToFollow[moveIndex][1]].transform.position){
				moveIndex++;
				if(moveIndex>=pathToFollow.Count){
					moving = false;
				}else{
					startTime = Time.time;
					totalDist = Vector3.Distance(transform.position,levelManagerScript.tileArray[pathToFollow[moveIndex][0],pathToFollow[moveIndex][1]].transform.position);
				}
			}
		}
	}
	
	
	// FindPath is run when a unit chooses to move to a location
	// (int[]) goal is the coordinates of the destination location
	// FindPath uses AStarPath() to determine the best path to the destination
	// FindPath populates pathToFollow, which is used for movement in Update()
	public void FindPath(int[] goal) {
		
		// initialize pathCostTable
		pathCostTable = new float[levelManagerScript.tileArrayLength[0],levelManagerScript.tileArrayLength[1]];
		for (int i=0; i<levelManagerScript.tileArrayLength[0]; i++){
			for (int j=0; j<levelManagerScript.tileArrayLength[1]; j++) {
				pathCostTable[i,j] = 999999f;
			}
		}
		
		pathPrevXTable = new int[levelManagerScript.tileArrayLength[0],levelManagerScript.tileArrayLength[1]];
		pathPrevYTable = new int[levelManagerScript.tileArrayLength[0],levelManagerScript.tileArrayLength[1]];
		
		// find best path to goal from every tile
		tilesToCheck = new List<List<int>>();
		// AStarPath populates the path tables
		AStarPath(goal, 0f, (int) tileCur.transform.position.x, (int) tileCur.transform.position.y, -1, -1);
		
		// construct path to follow list
		pathToFollow = new List<List<int>>();
		int tempX = pathPrevXTable[goal[0],goal[1]];
		int tempY = pathPrevYTable[goal[0],goal[1]];
		int prevX;
		int prevY;
		
		// starting at the destination
		// 		choose the best tile to move from
		//		add that tile to the start of pathToFollow
		while (tempX!=-1 && tempY!=-1) {
			pathToFollow.Insert(0, new List<int>{tempX,tempY});
			prevX = tempX;
			prevY = tempY;
			tempX = pathPrevXTable[prevX,prevY];
			tempY = pathPrevYTable[prevX,prevY];
			
		}
		pathToFollow.Add(new List<int>{goal[0],goal[1]});
		
		return;
	}
	
	
	// AStarPath is called in FindPath()
	// (int[]) goal is the destination the unit wants to reach
	// (float) firstCost is the cost of reaching the starting tile
	// (int) firstCurrentX is the X coordinate of the starting tile
	// (int) firstCurrentY is the Y coordinate of the starting tile
	// (int) prevX is the X coordinate of the tile before the starting tile
	// (int) prevY is the Y coordinate of the tile before the starting tile
	// AStarPath populates pathCostTable, pathPrevXTable, and pathPrevYTable with values to show the best way to reach the destination
	public void AStarPath(int[] goal, float firstCost, int firstCurrentX, int firstCurrentY, int prevX, int prevY){

		
		float cost = firstCost;
		int currentX = firstCurrentX;
		int currentY = firstCurrentY;
		
		pathCostTable[currentX,currentY] = cost;
		pathPrevXTable[currentX,currentY] = prevX;
		pathPrevYTable[currentX,currentY] = prevY;
		
		// tilesToCheck is a priority queue of tiles to consider
		tilesToCheck.Add(new List<int>{currentX,currentY});

		
		while (tilesToCheck.Count>0) {
			
			tilesToCheck.RemoveAt(0);
			
			// if the function reaches the goal tile, the best path has been found
			if (currentX == goal[0] && currentY == goal[1]) {
				return;
			}
			
			// get a list of neighboring tiles
			List<List<int>> neighborTiles = new List<List<int>>();
			neighborTiles.Add(new List<int> {currentX-1,currentY});
			neighborTiles.Add(new List<int> {currentX+1,currentY});
			neighborTiles.Add(new List<int> {currentX,currentY-1});
			neighborTiles.Add(new List<int> {currentX,currentY+1});
			
			foreach(List<int> tile in neighborTiles) {
				// only calculate tile cost, if tile is traversable or if the tile is not off the board
				if (!(tile[0]<0 || tile[0]>=levelManagerScript.tileArrayLength[0] || tile[1]<0 || tile[1]>=levelManagerScript.tileArrayLength[1] || !levelManagerScript.tileArray[tile[0],tile[1]].GetComponent<TileScript>().traversable)) {	
					//heuristic is equal to the distance between the tile and the goal
					float heuristic = Vector3.Distance(levelManagerScript.tileArray[goal[0],goal[1]].transform.position, levelManagerScript.tileArray[tile[0],tile[1]].transform.position);
					float curCost = cost + heuristic;
					
					// if the cost in the table is worse than curCost, update the table and add the tile to the list
					if (pathCostTable[tile[0],tile[1]] > curCost) {
						
						//update the tables
						pathCostTable[tile[0],tile[1]] = curCost;
						pathPrevXTable[tile[0],tile[1]] = currentX;
						pathPrevYTable[tile[0],tile[1]] = currentY;
						
						int index = 0;
						while (index < tilesToCheck.Count && pathCostTable[tilesToCheck[index][0],tilesToCheck[index][1]]<curCost) {
							index++;
						}
						tilesToCheck.Insert(index, new List<int>{tile[0],tile[1]});	
					}
				}
			}
			
			cost = pathCostTable[tilesToCheck[0][0],tilesToCheck[0][1]];
			currentX = tilesToCheck[0][0];
			currentY = tilesToCheck[0][1];
			
		}
	}
	
	// FindAvailableMoves uses FindAvailable() to determine the unit's movement range
	// (int) tileX is the starting X coordinate of the unit
	// (int) tileY is the starting Y coordinate of the unit
	// (int) distanceTraveled is the distance from the unit the function has traveled so far
	// the parameters are passed directly into FindAvailable()
	public void FindAvailableMoves(int tileX, int tileY, int distanceTraveled) {
		availableTiles = new List<List<int>>();
		availableHLTiles = new List<List<int>>();
		FindAvailable(tileX,tileY,distanceTraveled,0);
	}
	
	// FindAvailableAttacks uses FindAvailable() to determine the unit's total attack range
	// (int) tileX is the starting X coordinate of the unit
	// (int) tileY is the starting Y coordinate of the unit
	// (int) distanceTraveled is the distance from the unit the function has traveled so far
	// the parameters are passed directly into FindAvailable()
	public void FindAvailableAttacks(int tileX, int tileY, int distanceTraveled) {
		availableAttacks = new List<List<int>>();
		foreach(List<int> subList in availableTiles){
			FindAvailable(subList[0],subList[1],distanceTraveled,1);
		}
	}
	
	// FindAvailableImmediateAttacks uses FindAvailable() to determine the unit's immediate attack range
	// (int) tileX is the starting X coordinate of the unit
	// (int) tileY is the starting Y coordinate of the unit
	// (int) distanceTraveled is the distance from the unit the function has traveled so far
	// the parameters are passed directly into FindAvailable()
	public void FindAvailableImmediateAttacks(int tileX, int tileY, int distanceTraveled) {
		availableImmediateAttacks = new List<List<int>>();
		FindAvailable(tileX,tileY,distanceTraveled,2);
	}
	
	// FindAvailable is a generalized, recursive function for finding valid tiles for movement and attack range
	// (int) tileX is the starting X coordinate of the function call
	// (int) tileY is the starting Y coordinate of the function call
	// (int) distanceTraveled is the distance from the unit the function has traveled so far
	// (int) type, 0 is movement, 1 is total attack range, 2 is immediate attack range
	// FindAvailable populates availableTiles, availableAttacks, or availableImmediateAttacks, depending on type
	public void FindAvailable(int tileX, int tileY, int distanceTraveled, int type) {
		
		List<List<int>> updateList;
		int maxDistance;
		
		// determine which list to be updating
		if(type == 0){
			updateList = availableTiles;
			maxDistance = speed;
		}else if( type == 1){
			updateList = availableAttacks;
			maxDistance = range;
		}else if( type == 2){
			updateList = availableImmediateAttacks;
			maxDistance = range;
		}else{
			return;
		}
		
		// check if tile is out of bounds
		if (tileX<0 || tileX>=levelManagerScript.tileArrayLength[0] || tileY<0 || tileY>=levelManagerScript.tileArrayLength[1]){
			return;
		}
		
		// check if tile is traversable
		if ((type==0)&&(!levelManagerScript.tileArray[tileX,tileY].GetComponent<TileScript>().traversable)) {
			return;
		}
		
		// check if function has traveled too far
		if (distanceTraveled>maxDistance) {
			return;
		}
		
		// check if this call has found tileX and tileY in a shorter distance
		foreach (List<int> subList in updateList) {
			if (subList[0]==tileX && subList[1]==tileY && subList[2]<=distanceTraveled){
				return;
			}
			if(subList[0]==tileX && subList[1]==tileY && subList[2]>distanceTraveled){
				updateList.Remove(subList);
				if(type==0){
					availableHLTiles.Remove(subList);
				}
				break;
			}
		}
		
		// check if this call has found tileX and tileY in a worse distance
		if(type == 0){
			foreach (List<int> subList in availableHLTiles) {
				if (subList[0]==tileX && subList[1]==tileY && subList[2]<=distanceTraveled){
					return;
				}
				if(subList[0]==tileX && subList[1]==tileY && subList[2]>distanceTraveled){
					availableHLTiles.Remove(subList);
					break;
				}
			}
		}
		
		// add tile to updateList
		if (((type==1)||(type==2))&&(levelManagerScript.tileArray[tileX,tileY].GetComponent<TileScript>().traversable)) {
			updateList.Add(new List<int> {tileX,tileY,distanceTraveled});
		}else if( type == 0){
			if( (levelManagerScript.tileArray[tileX,tileY].GetComponent<TileScript>().occupyingObject==null) || (levelManagerScript.tileArray[tileX,tileY].GetComponent<TileScript>().occupyingObject == gameObject) ){ 
				updateList.Add(new List<int> {tileX,tileY,distanceTraveled});
			}
			availableHLTiles.Add(new List<int> {tileX,tileY,distanceTraveled});
		}
		
		
		// recursively call FindAvailable on neighboring tiles
		FindAvailable(tileX+1,tileY,distanceTraveled+1, type);
		FindAvailable(tileX-1,tileY,distanceTraveled+1, type);
		FindAvailable(tileX,tileY+1,distanceTraveled+1, type);
		FindAvailable(tileX,tileY-1,distanceTraveled+1, type);
	}
	
	
	// MoveUnit is called to move a unit to a destination
	// (GameObject) tileDest is the destination tile
	// (bool) moveBack is used to determine if the unit should move instantly
	public void MoveUnit(GameObject tileDest, bool moveBack = false) {
		
		if (tileDest == tileCur) {
			return;
		}
		
		// set relevant data members
		tilePrev = tileCur;
		tileCur.GetComponent<TileScript>().occupyingObject = null;
		tileDest.GetComponent<TileScript>().occupyingObject = this.gameObject;
		tileCur = tileDest;
		
		if(moveBack){
			this.transform.position = tileDest.transform.position;
		}else{
			moveIndex = 1;
			startTime = Time.time;
			totalDist = Vector3.Distance(this.transform.position,levelManagerScript.tileArray[pathToFollow[moveIndex][0],pathToFollow[moveIndex][1]].transform.position);
			moving = true;
		}
		
	}
	
	// MoveBack returns a unit to tilePrev
	// uses MoveUnit() with moveBack set to true
	public void MoveBack() {
		moving = false;
		MoveUnit(tilePrev,true);
		tilePrev = null;
	}
	
	// Wait is used to finish a unit's action early
	public void Wait(){
		
		FinishAction();
		
	}
	
	// UseItem uses the unit's item in itemList[itemIndex]
	// removes Item from list
	// calls UseItem in ItemScript
	// finishes unit's action
	public void UseItem(){
		ItemScript tempItem = itemList[itemIndex].GetComponent<ItemScript>();
		itemList.RemoveAt(itemIndex);
		tempItem.UseItemOnArea();
		itemIndex = 0;
		FinishAction();
	}
	
	// CurrentItem returns the item script of the currently chosen item
	public ItemScript CurrentItem(){
		
		return itemList[itemIndex].GetComponent<ItemScript>();
		
	}
	
	// Equip modifies the unit's stats based on the equipment
	// (EquipmentScript) equipment is the equipment to be equipped
	// (int) equipIndex is the equipment slot to be filled
	public void Equip(EquipmentScript equipment, int equipIndex) {
		// take equipment from party inventory, add to equipment list
		equipmentList[equipIndex] = equipment.equipName;
		// modify all the stats
		maxHealth += equipment.maxHealthMod;
		speed += equipment.speedMod;
		range += equipment.rangeMod;
		numAttacks += equipment.numAttacksMod;
		attack += equipment.attackMod;
		defense += equipment.defenseMod;
		agility += equipment.agilityMod;
	}
	
	// Unequip modifies the unit's stats based on the equipment
	// (EquipmentScript) equipment is the equipment to be unequipped
	// (int) equipIndex is the equipment slot to be emptied
	public void Unequip(EquipmentScript equipment, int equipIndex) {
		// remove item from equipmentList
		equipmentList[equipIndex] = "Empty";
		// modify all the stats
		maxHealth -= equipment.maxHealthMod;
		speed -= equipment.speedMod;
		range -= equipment.rangeMod;
		numAttacks -= equipment.numAttacksMod;
		attack -= equipment.attackMod;
		defense -= equipment.defenseMod;
		agility -= equipment.agilityMod;
	}
	
	// TakeDamage runs when a unit needs to take damage
	// (int) damage is the amount of damage to take
	// spawns damageText above the unit
	// if health falls below 0, runs Death()
	public void TakeDamage(int damage){
		
		// spawn damageText
		Vector3 dmgSpawnLocation = transform.position;
		dmgSpawnLocation.y += .25f;
		GameObject dmgTxt = Instantiate(damageText,dmgSpawnLocation,Quaternion.identity) as GameObject;
		dmgTxt.GetComponent<DamageTextScript>().damage = damage;
		health -= damage;
		
		if(health<=0){
			
			StartCoroutine(Death());
			
		}
		
	}
	
	// GainHealth runs when a unit gains health
	// (int) gain is the amount of health to gain
	// spawns damageText above the unit
	// if health is over maxHealth, sets health to maxHealth
	public void GainHealth(int gain){
		
		// if unit has "Decay" status, unit takes damage instead of healing
		foreach(string status in statusList) {
			if (status == "Decay") {
				TakeDamage(gain);
				return;
			}
		}
		
		// spawn damageText
		Vector3 dmgSpawnLocation = transform.position;
		dmgSpawnLocation.y += .25f;
		GameObject dmgTxt = Instantiate(damageText,dmgSpawnLocation,Quaternion.identity) as GameObject;
		dmgTxt.GetComponent<DamageTextScript>().damage = gain;
		dmgTxt.GetComponent<TextMesh>().color = new Color(0.1f,1.0f,0.1f,1f);
		
		if(health + gain < maxHealth){
			health += gain;
		}else{
			health = maxHealth;
		}
		
	}
	
	
	// ResolveCombat runs when a combat occurs
	// (GameObject) targetUnit is the unit being attacked
	// units take turns attacking each other based on how many attacks they get
	// damage is applied after each attack
	// after attacks have finished, experience is applied to player units
	public IEnumerator ResolveCombat(GameObject targetUnit) {
		
		// if target unit is the acting unit, the unit takes damage and finishes its action
		if(targetUnit == gameObject){
			TakeDamage(attack-defense);
			FinishAction();
			levelManagerScript.player.combatEngaged = false;
			yield break; 
		}
		
		// calculate the number of attacks each unit gets
		// unit gets to attack twice if its agility is significantly higher than opponent's
		int numUnitAttacks = CalculateAttackNumber(agility-5>targetUnit.GetComponent<UnitScript>().agility);
		int numTargetAttacks = targetUnit.GetComponent<UnitScript>().CalculateAttackNumber(targetUnit.GetComponent<UnitScript>().agility-5>agility);
		
		int damageAmount;
		int totalDamage = 0;
		int targetTotalDamage = 0;
		
		// calculate the ratio of levels for experience gain
		float levelRatio = (float) targetUnit.GetComponent<UnitScript>().level / (float) level;
		float targetLevelRatio = (float) level / (float) targetUnit.GetComponent<UnitScript>().level;
		
		
		// check if unit is in targetUnit's range
		targetUnit.GetComponent<UnitScript>().FindAvailableImmediateAttacks((int) targetUnit.GetComponent<UnitScript>().tileCur.transform.position.x,(int) targetUnit.GetComponent<UnitScript>().tileCur.transform.position.y,0);
		bool outOfRange = true;
		foreach(List<int> tile in targetUnit.GetComponent<UnitScript>().availableImmediateAttacks) {
			if (tile[0]==tileCur.transform.position.x && tile[1]==tileCur.transform.position.y) {
				outOfRange = false;
				break;
			}
		}
		if (outOfRange) {
			numTargetAttacks = 0;
		}
		
		int i =0;
		while (i<numUnitAttacks || i<numTargetAttacks) {
			
			// if the unit has died, don't continue the loop
			if (dead) {
				break;
			}
			
			if (i<numUnitAttacks) {
				//do unit attack
				damageAmount = attack - targetUnit.GetComponent<UnitScript>().defense;
				if (damageAmount<1) {
					damageAmount = 1;
				}
				if (targetUnit.GetComponent<UnitScript>().health <= damageAmount) {
					// if attacking a player unit, targetUnit will need to gain experience
					if (gameObject != null && gameObject.tag == "playerUnit") {
						targetUnit.GetComponent<UnitScript>().experienceUpdating = true;
					}
				}
				
				SoundManager.instance.LaserGun();
				targetUnit.GetComponent<UnitScript>().TakeDamage(damageAmount);
				totalDamage += damageAmount;
				yield return new WaitForSeconds(.5f);
			}
			
			// if targetUnit is dead, don't continue the loop
			if (targetUnit == null || targetUnit.GetComponent<UnitScript>().dead) {
				break;
			}
			
			if (i<numTargetAttacks) {
				//do target attack
				if (targetUnit!=null) {
					damageAmount = targetUnit.GetComponent<UnitScript>().attack - defense;
					if (damageAmount<1) {
						damageAmount = 1;
					}
					// if unit is a player unit, unit will need to gain experience
					if (health <= damageAmount) {
						if (targetUnit != null && targetUnit.tag == "playerUnit") {
							experienceUpdating = true;
						}
					}
					SoundManager.instance.LaserGun();
					TakeDamage(damageAmount);
					targetTotalDamage += damageAmount;
					yield return new WaitForSeconds(.5f);
				}
			}
			i++;
		}
		
		// if this is player unit
		//		set the progressBar active
		//		apply experience to unit
		if (this != null && gameObject != null && gameObject.tag == "playerUnit") {
			progressBar.transform.GetChild(0).gameObject.GetComponent<Slider>().value = ((float)experience)/100f;
			progressBar.SetActive(true);
			StartCoroutine(AddExperience(totalDamage * levelRatio));
			yield return new WaitWhile(() => progressBar.activeSelf);
		}
		
		// if targetUnit is player unit
		// 		set the progressBar active
		// 		apply experience to targetUnit
		if (targetUnit != null && targetUnit.tag == "playerUnit") {
			progressBar.transform.GetChild(0).gameObject.GetComponent<Slider>().value = ((float)targetUnit.GetComponent<UnitScript>().experience)/100f;
			progressBar.SetActive(true);
			targetUnit.GetComponent<UnitScript>().ExperienceCaller(targetTotalDamage * targetLevelRatio);
			yield return new WaitWhile(() => progressBar.activeSelf);
			
		} 
		
		// set flags to false
		experienceUpdating = false;
		if(targetUnit!=null){
			targetUnit.GetComponent<UnitScript>().experienceUpdating = false;
		}
		levelManagerScript.player.combatEngaged = false;
		
		// combat has ended, finish unit's action
		if(!dead){
			FinishAction();
		}
	}
	
	// CalculateAttackNumber determines how many attacks a unit gets in combat
	// (bool) doubleAttack is used to determine if a unit gets twice as many attacks as normal
	public int CalculateAttackNumber(bool doubleAttack) {
		int numAttacksReturn = numAttacks;
		
		if (doubleAttack) {
			numAttacksReturn = numAttacksReturn * 2;
		}
		
		return numAttacksReturn;
	}
	
	// EstimateDamage is used by AI units to determine which attack they should preform
	// (GameObject) targetUnit is the unit they are targeting
	// this function is nearly identical to ResolveCombat(), however no damage is applied, and no experience is gained
	// returns (int) tempPriority, which is used by AI to determine which unit is best to attack
	public int EstimateDamage(GameObject targetUnit) {
		
		// calculate the number of attacks each unit gets
		// unit gets to attack twice if its agility is significantly higher than opponent's
		int numUnitAttacks = CalculateAttackNumber(agility-5>targetUnit.GetComponent<UnitScript>().agility);
		int numTargetAttacks = targetUnit.GetComponent<UnitScript>().CalculateAttackNumber(targetUnit.GetComponent<UnitScript>().agility-5>agility);
		
		int damageAmount;
		int tempUnitHealth = health;
		int tempTargetHealth = targetUnit.GetComponent<UnitScript>().health;
		
		bool unitDeath = false;
		bool targetDeath = false;
		
		// check if unit is in target's range
		if (targetUnit.GetComponent<UnitScript>().range < range) {
			numTargetAttacks = 0;
		}
		
		int i =0;
		while (i<numUnitAttacks || i<numTargetAttacks) {
			if (i<numUnitAttacks) {
				//do unit attack
				damageAmount = attack - targetUnit.GetComponent<UnitScript>().defense;
				if (damageAmount<1) {
					damageAmount = 1;
				}
				tempTargetHealth -= damageAmount;
				if (tempTargetHealth <= 0) {
					targetDeath = true;
					break;
				}
			}
			
			if (i<numTargetAttacks) {
				//do target attack
				if (targetUnit!=null) {
					damageAmount = targetUnit.GetComponent<UnitScript>().attack - defense;
					if (damageAmount<1) {
						damageAmount = 1;
					}
					tempUnitHealth -= damageAmount;
					if (tempUnitHealth <= 0) {
						unitDeath = true;
						break;
					}
					
				}
			}
			i++;
		}
		
		int tempPriority = 0;
		int tempDealtPriority = 0;
		int tempTakenPriority = 0;
		int tempDeathPriority = 0;
		int totalDealtDamage = targetUnit.GetComponent<UnitScript>().health - tempTargetHealth;
		int totalTakenDamage = health - tempUnitHealth;
		
		// tier lists for determining how many points damage dealt and damage taken are worth
		List<int> damageTiers = new List<int>{20,10,0};
		List<int> dealtMods = new List<int>{8,7,6};
		List<int> takenMods = new List<int>{3,2,1};
		
		for (i=0; i<damageTiers.Count; i++) {
			if (totalDealtDamage > damageTiers[i]) {
				tempDealtPriority += dealtMods[i] * (totalDealtDamage - damageTiers[i]);
				totalDealtDamage -= totalDealtDamage - damageTiers[i];
			} 
			if (totalTakenDamage > damageTiers[i]) {
				tempTakenPriority += takenMods[i] * (totalTakenDamage - damageTiers[i]);
				totalTakenDamage -= totalTakenDamage - damageTiers[i];
			} 
		}
		
		// assigns points for dying or killing the opponent
		if (unitDeath) {
			tempDeathPriority = -5;
		} else if (targetDeath) {
			tempDeathPriority = 50;
		}
		
		// calculate tempPriority
		tempPriority = tempDealtPriority - tempTakenPriority + tempDeathPriority;
		
		// tempPriority needs to be at least 1
		if (tempPriority<=0) {
			tempPriority = 1;
		}
		return tempPriority;
		
	}
	
	// wrapper method for calling coroutine AddExperience on other object
	public void ExperienceCaller(float addedExp) {
		StartCoroutine(AddExperience(addedExp));
	}
	
	// AddExperience adds experience to the unit's experience stat
	// (float) addedExp is the amount of experience to add
	public IEnumerator AddExperience(float addedExp) {
		int oldExp = experience;
		experience += (int) addedExp;
		// unit gains at least 1 experience
		if ( (int) addedExp == 0 ) {
			experience += 1;
		}
		
		// run this loop until the unit doesn't have enough experience to level up
		while (experience >= 100) {
			//start bar from old experience
			gainingExperience = true;
			//start bar from old exp for remainder
			StartCoroutine(ExperienceProgress(oldExp, 100-oldExp));
			experience-=100;
			oldExp = 0;
			yield return new WaitWhile(() => gainingExperience);
		}
		
		// run ExperienceProgress() one more time to bring progressBar to correct value
		gainingExperience = true;
		//start bar from old exp for remainder
		StartCoroutine(ExperienceProgress(oldExp, experience-oldExp));
		yield return new WaitWhile(() => gainingExperience);
		progressBar.SetActive(false);
	}
	
	// ExperienceProgress is called in AddExperience()
	// (int) startVal is the amount of experience to start at
	// (int) ExpVal is the amount of experience to increase by
	// if the progressBar fills, runs LevelUp()
	public IEnumerator ExperienceProgress(int startVal, int ExpVal){
		yield return new WaitForSeconds(0.5f);
		int totalIncrease = ExpVal;
		int increase = 0;
		while(totalIncrease!=0){
			// wait a little bit before each increase
			yield return new WaitForSeconds(0.02f);
			float barPosition = ((float)(startVal+increase))/ 100f;
			increase++;
			totalIncrease--;
			
			//apply barPosition to slider
			progressBar.transform.GetChild(0).gameObject.GetComponent<Slider>().value = barPosition;
			SoundManager.instance.ExpGain();
		}
		
		// if experience has reached 100, run LevelUp()
		if(startVal+ExpVal == 100){
			yield return new WaitForSeconds(1.0f);
			levelUpSheet.SetActive(true);
			StartCoroutine(LevelUp());
			yield return new WaitWhile(() => levelUpSheet.activeSelf);
		}
		yield return new WaitForSeconds(1.0f);
		gainingExperience = false;
	}
	
	
	// LevelUp runs in ExperienceProgress when experience has reached 100
	// increases units stats randmoly, based on statGrowthArray
	public IEnumerator LevelUp(){
		SoundManager.instance.LevelUp();
		level++;
		
		GameObject[] statTextArray = new GameObject[4];
		statTextArray[0] = levelUpSheet.transform.GetChild(2).GetChild(7).gameObject;
		statTextArray[1] = levelUpSheet.transform.GetChild(2).GetChild(10).gameObject;
		statTextArray[2] = levelUpSheet.transform.GetChild(2).GetChild(13).gameObject;
		statTextArray[3] = levelUpSheet.transform.GetChild(2).GetChild(14).gameObject;
		
		// assign values to levelUpSheet
		levelUpSheet.transform.GetChild(2).GetChild(0).gameObject.GetComponent<Text>().text = charName;
		levelUpSheet.transform.GetChild(2).GetChild(1).gameObject.GetComponent<Text>().text = "lvl " + level.ToString();
		levelUpSheet.transform.GetChild(2).GetChild(2).gameObject.GetComponent<Text>().text = className;
		levelUpSheet.transform.GetChild(2).GetChild(7).gameObject.GetComponent<Text>().text = maxHealth.ToString();
		levelUpSheet.transform.GetChild(2).GetChild(10).gameObject.GetComponent<Text>().text = attack.ToString();
		levelUpSheet.transform.GetChild(2).GetChild(11).gameObject.GetComponent<Text>().text = speed.ToString();
		levelUpSheet.transform.GetChild(2).GetChild(12).gameObject.GetComponent<Text>().text = range.ToString();
		levelUpSheet.transform.GetChild(2).GetChild(13).gameObject.GetComponent<Text>().text = defense.ToString();
		levelUpSheet.transform.GetChild(2).GetChild(14).gameObject.GetComponent<Text>().text = agility.ToString();
		levelUpSheet.transform.GetChild(2).Find("ExperienceValueText").GetComponent<Text>().text = "100/100";

		
		yield return new WaitForSeconds(1);
		
		// calculate stat increases
		//   after each calculation, change statText
		
		for (int i=0; i < statTextArray.Length; i++) {
			float tempRandom = Random.Range(0.0f,1.0f);
			if (statGrowthArray[i] > tempRandom) {
				statTextArray[i].GetComponent<Text>().text = statArray[i].ToString() + "+1";
				statArray[i] += 1;
				SoundManager.instance.StatUp();
				yield return new WaitForSeconds(0.4f);
			}
		}
		
		for (int i=0; i < statTextArray.Length; i++) {
			if (statTextArray[i].GetComponent<Text>().text != statArray[i].ToString()) {
				statTextArray[i].GetComponent<Text>().text = statArray[i].ToString();
				yield return new WaitForSeconds(0.4f);
			}
		} 
		
		maxHealth = statArray[0];
		attack = statArray[1];
		defense = statArray[2];
		agility = statArray[3];
		
		yield return new WaitForSeconds(1);
		
		levelUpSheet.SetActive(false);
		
	}
	
	// NewTurn is called in LevelManagerScript when a new turn starts
	// resets finished flag
	// decreases each status duration by one
	public void NewTurn() {
		finished = false;
		
		for (int i=statusDurationList.Count-1; i>=0; i--) {
			statusDurationList[i] -= 1;
			if (statusDurationList[i]<=0) {
				statusDurationList.RemoveAt(i);
				statusList.RemoveAt(i);
			}
		}
		
	}
	
	// FinishAction is called whenever a unit will finish its actions for the round
	public void FinishAction() {
		if (this == null || gameObject == null) {
			return;
		}
		
		finished = true;
		
		// if the phase is not correct for this type of unit, return
		if (levelManagerScript.phaseCounter != 0 && gameObject.tag == "playerUnit") {
			return;
		}
		
		if (levelManagerScript.phaseCounter != 1 && gameObject.tag == "enemyUnit") {
			return;
		}
		
		if (levelManagerScript.phaseCounter != 2 && gameObject.tag == "alliedUnit") {
			return;
		}
		
		bool phaseOver = true;
		tilePrev = null;
		
		// check if all units of this type have finished, and if so if the phase should end
		foreach(GameObject unit in levelManagerScript.unitList){
			if (!unit.GetComponent<UnitScript>().finished && (unit.tag == gameObject.tag)) {
				phaseOver = false;
			}
		}
		
		// if the phase should end, call NextPhase()
		if (phaseOver) {
			levelManagerScript.NextPhase();
		}
	}
	
	
	// Death is called when the unit dies
	// sets flags and does logistic clean up
	public IEnumerator Death(){
		// stop drawing the unit sprite
		gameObject.GetComponent<SpriteRenderer>().enabled = false;
		// set the dead flag to true
		dead = true;
		// remove the unit from the unitList in the level manager
		levelManagerScript.unitList.Remove(gameObject);
		
		// before destroying the unit, wait for a combat it may be a part of to end
		yield return new WaitWhile(() => experienceUpdating);
		
		// check if this triggers a level end
		levelManagerScript.CheckLevelEnd();
		// run finish action to check if the phase should end
		FinishAction();
		
		// unit is no longer an occupyingObject on the tile
		tileCur.GetComponent<TileScript>().occupyingObject = null;
		
		// destroy the gameObject
		Destroy(gameObject);
	}
	
}
