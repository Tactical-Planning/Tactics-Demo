using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UnitScript : MonoBehaviour {

	public GameObject tileCur;
	public GameObject tilePrev;
	
	public GameObject levelUpSheet;
	public GameObject progressBar;
	
	//only used on AI units
	public int priority; 
	public string actionToTake; 
	public GameObject targetToTarget;
	public bool defender;
	
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
	
	public List<List<int>> availableTiles;
	public List<List<int>> availableHLTiles;
	public List<List<int>> availableAttacks;
	public List<List<int>> availableImmediateAttacks;
	
	//pathing data members
	public float[,] pathCostTable;
	public List<List<int>> tilesToCheck;
	public int[,] pathPrevXTable;
	public int[,] pathPrevYTable;
	public List<List<int>> pathToFollow;
	
	private float startTime;
	private float totalDist;
	public bool moving;
	private int moveIndex;
	
	private LevelManagerScript levelManagerScript;
	public int startX;
	public int startY;
	
	public bool selectedFlag;
	public bool finished;
	
	public List<GameObject> itemList;
	public int itemIndex;

	public List<string> equipmentList;
	
	public List<string> statusList;
	public List<int> statusDurationList;
	
	public bool dead;
	
	public GameObject damageText;

	//Use this for initialization
	void Start () {
		
		
		
		statArray = new int[]{maxHealth, attack, defense, agility};
		statGrowthArray = new float[]{maxHealthGrowth, attackGrowth, defenseGrowth, agilityGrowth};
		experienceUpdating = false;
		
		//tileCur = null;
		tilePrev= null;
		priority = 0;
		//typeOfThing="playerUnit";
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
		
		foreach (GameObject item in itemList) {
			item.GetComponent<ItemScript>().ItemInstantiate();
			item.GetComponent<ItemScript>().holder = gameObject;
		}
		
		statusList = new List<string>();
		statusDurationList = new List<int>();
	}
	
	//Update is called once per frame
	void Update () {
		if(moving){
			
			float frac = (Time.time - startTime)*3/totalDist;
			
			this.transform.position = Vector3.Lerp(this.transform.position, levelManagerScript.tileArray[pathToFollow[moveIndex][0],pathToFollow[moveIndex][1]].transform.position, frac);
			
			if(this.transform.position == levelManagerScript.tileArray[pathToFollow[moveIndex][0],pathToFollow[moveIndex][1]].transform.position){
				moveIndex++;
				if(moveIndex>=pathToFollow.Count){
					moving = false;
					//levelManagerScript.aiController.unitActing = false;
				}else{
					startTime = Time.time;
					totalDist = Vector3.Distance(transform.position,levelManagerScript.tileArray[pathToFollow[moveIndex][0],pathToFollow[moveIndex][1]].transform.position);
				}
			}
		}
	}
	
	public void FindPath(int[] goal) {
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
		AStarPath(goal, 0f, (int) tileCur.transform.position.x, (int) tileCur.transform.position.y, -1, -1);
		
		// construct path to follow list
		pathToFollow = new List<List<int>>();
		int tempX = pathPrevXTable[goal[0],goal[1]];
		int tempY = pathPrevYTable[goal[0],goal[1]];
		int prevX;
		int prevY;
		
		while (tempX!=-1 && tempY!=-1) {
			pathToFollow.Insert(0, new List<int>{tempX,tempY});
			prevX = tempX;
			prevY = tempY;
			tempX = pathPrevXTable[prevX,prevY];
			tempY = pathPrevYTable[prevX,prevY];
			
		}
		pathToFollow.Add(new List<int>{goal[0],goal[1]});
		Debug.Log("pathToFollow is "+ pathToFollow.Count.ToString()+" many tiles long");
		
		return;
	}
	
	public void AStarPath(int[] goal, float firstCost, int firstCurrentX, int firstCurrentY, int prevX, int prevY){

		//calculate the cost to each neighbor tile
		// if the cost is less than cost in table, add to tilesToCheck (priority queue of tile locations)
		
		float cost = firstCost;
		int currentX = firstCurrentX;
		int currentY = firstCurrentY;
		
		pathCostTable[currentX,currentY] = cost;
		pathPrevXTable[currentX,currentY] = prevX;
		pathPrevYTable[currentX,currentY] = prevY;
		
		
		tilesToCheck.Add(new List<int>{currentX,currentY});

		
		while (tilesToCheck.Count>0) {
			
			tilesToCheck.RemoveAt(0);
			
			if (currentX == goal[0] && currentY == goal[1]) {
				return;
			}
			
			List<List<int>> neighborTiles = new List<List<int>>();
			neighborTiles.Add(new List<int> {currentX-1,currentY});
			neighborTiles.Add(new List<int> {currentX+1,currentY});
			neighborTiles.Add(new List<int> {currentX,currentY-1});
			neighborTiles.Add(new List<int> {currentX,currentY+1});
			
			foreach(List<int> tile in neighborTiles) {
				// only calculate tile cost, if tile is not off the board or a non-traversable tile
				if (!(tile[0]<0 || tile[0]>=levelManagerScript.tileArrayLength[0] || tile[1]<0 || tile[1]>=levelManagerScript.tileArrayLength[1] || !levelManagerScript.tileArray[tile[0],tile[1]].GetComponent<TileScript>().traversable)) {	
					//run some logic to find next best tile
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
	
	public void FindAvailableMoves(int tileX, int tileY, int distanceTraveled) {
		availableTiles = new List<List<int>>();
		availableHLTiles = new List<List<int>>();
		FindAvailable(tileX,tileY,distanceTraveled,0);
	}
	
	public void FindAvailableAttacks(int tileX, int tileY, int distanceTraveled) {
		availableAttacks = new List<List<int>>();
		foreach(List<int> subList in availableTiles){
			FindAvailable(subList[0],subList[1],distanceTraveled,1);
		}
	}
	
	public void FindAvailableImmediateAttacks(int tileX, int tileY, int distanceTraveled) {
		availableImmediateAttacks = new List<List<int>>();
		FindAvailable(tileX,tileY,distanceTraveled,2);
	}
	
	//generalized function for finding valid tiles for movement and attack range
	//int type, 0 is movement, 1 is total attack range, 2 is immediate attack range
	public void FindAvailable(int tileX, int tileY, int distanceTraveled, int type) {
		
		List<List<int>> updateList;
		int maxDistance;
		
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
		
		
		if (tileX<0 || tileX>=levelManagerScript.tileArrayLength[0] || tileY<0 || tileY>=levelManagerScript.tileArrayLength[1]){
			return;
		}
		
		if ((type==0)&&(!levelManagerScript.tileArray[tileX,tileY].GetComponent<TileScript>().traversable)) {
			return;
		}
		
		if (distanceTraveled>maxDistance) {
			return;
		}
		
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
		
		if (((type==1)||(type==2))&&(levelManagerScript.tileArray[tileX,tileY].GetComponent<TileScript>().traversable)) {
			updateList.Add(new List<int> {tileX,tileY,distanceTraveled});
		}else if( type == 0){
			if( (levelManagerScript.tileArray[tileX,tileY].GetComponent<TileScript>().occupyingObject==null) || (levelManagerScript.tileArray[tileX,tileY].GetComponent<TileScript>().occupyingObject == gameObject) ){ 
				updateList.Add(new List<int> {tileX,tileY,distanceTraveled});
			}
			availableHLTiles.Add(new List<int> {tileX,tileY,distanceTraveled});
		}
		
		
		
		FindAvailable(tileX+1,tileY,distanceTraveled+1, type);
		FindAvailable(tileX-1,tileY,distanceTraveled+1, type);
		FindAvailable(tileX,tileY+1,distanceTraveled+1, type);
		FindAvailable(tileX,tileY-1,distanceTraveled+1, type);
	}
	
	
	
	
	
	public void MoveUnit(GameObject tileDest, bool moveBack = false) {
		
		if (tileDest == tileCur) {
			return;
		}
		//Debug.Log("starting to modify "+ className);
		tilePrev = tileCur;
		tileCur.GetComponent<TileScript>().occupyingObject = null;
		tileDest.GetComponent<TileScript>().occupyingObject = this.gameObject;
		//Debug.Log("ending modify " + className);
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
	
	public void MoveBack() {
		moving = false;
		MoveUnit(tilePrev,true);
		tilePrev = null;
	}
	
	public void Wait(){
		
		FinishAction();
		
	}
	
	public void UseItem(){
		//remove Item from list
		// call UseItem in ItemScript
		ItemScript tempItem = itemList[itemIndex].GetComponent<ItemScript>();
		itemList.RemoveAt(itemIndex);
		tempItem.UseItemOnArea();
		//Debug.Log("Used item in Unit Script");
		itemIndex = 0;
		FinishAction();
	}
	
	public ItemScript CurrentItem(){
		
		return itemList[itemIndex].GetComponent<ItemScript>();
		
	}
	
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
	
	public void TakeDamage(int damage){
		Vector3 dmgSpawnLocation = transform.position;
		dmgSpawnLocation.y += .25f;
		GameObject dmgTxt = Instantiate(damageText,dmgSpawnLocation,Quaternion.identity) as GameObject;
		dmgTxt.GetComponent<DamageTextScript>().damage = damage;
		health -= damage;
		if(health<=0){
			
			StartCoroutine(Death());
			
		}
		
	}
	public void GainHealth(int gain){
		
		Vector3 dmgSpawnLocation = transform.position;
		dmgSpawnLocation.y += .25f;
		GameObject dmgTxt = Instantiate(damageText,dmgSpawnLocation,Quaternion.identity) as GameObject;
		dmgTxt.GetComponent<DamageTextScript>().damage = gain;
		dmgTxt.GetComponent<TextMesh>().color = new Color(0.1f,1.0f,0.1f,1f);
		
		foreach(string status in statusList) {
			if (status == "Decay") {
				TakeDamage(gain);
				return;
			}
		}
		if(health + gain < maxHealth){
				health += gain;
			}else{
				health = maxHealth;
				
			}
		
	}
	
	public IEnumerator ResolveCombat(GameObject targetUnit) {
		
		if(targetUnit == gameObject){
			TakeDamage(attack-defense);
			FinishAction();
			levelManagerScript.player.combatEngaged = false;
			return true; 
		}
		
		int numUnitAttacks = CalculateAttackNumber(agility-5>targetUnit.GetComponent<UnitScript>().agility);
		int numTargetAttacks = targetUnit.GetComponent<UnitScript>().CalculateAttackNumber(targetUnit.GetComponent<UnitScript>().agility-5>agility);
		int damageAmount;
		int totalDamage = 0;
		int targetTotalDamage = 0;
		float levelRatio = (float) targetUnit.GetComponent<UnitScript>().level / (float) level;
		float targetLevelRatio = (float) level / (float) targetUnit.GetComponent<UnitScript>().level;
		int i =0;
		
		// check if unit is in target's range
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
		
		while (i<numUnitAttacks || i<numTargetAttacks) {
			
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
					if (gameObject != null && gameObject.tag == "playerUnit") {
						targetUnit.GetComponent<UnitScript>().experienceUpdating = true;
					}
				}
				SoundManager.instance.LaserGun();
				targetUnit.GetComponent<UnitScript>().TakeDamage(damageAmount);
				totalDamage += damageAmount;
				yield return new WaitForSeconds(.5f);
			}
			
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
		
		if (this != null && gameObject != null && gameObject.tag == "playerUnit") {
			progressBar.transform.GetChild(0).gameObject.GetComponent<Slider>().value = ((float)experience)/100f;
			progressBar.SetActive(true);
			StartCoroutine(AddExperience(totalDamage * levelRatio));
			yield return new WaitWhile(() => progressBar.activeSelf);
		}
		
		if (targetUnit != null && targetUnit.tag == "playerUnit") {
			progressBar.transform.GetChild(0).gameObject.GetComponent<Slider>().value = ((float)targetUnit.GetComponent<UnitScript>().experience)/100f;
			progressBar.SetActive(true);
			/* Debug.Log("Damage = " + targetTotalDamage);
			Debug.Log("TargetLevel = " + targetUnit.GetComponent<UnitScript>().level.ToString());
			Debug.Log("Unit Level = " + level.ToString());
			Debug.Log("Level Ratio = " + levelRatio.ToString()); */
			targetUnit.GetComponent<UnitScript>().ExperienceCaller(targetTotalDamage * targetLevelRatio);
			yield return new WaitWhile(() => progressBar.activeSelf);
			
		} 
		
		experienceUpdating = false;
		if(targetUnit!=null){
			targetUnit.GetComponent<UnitScript>().experienceUpdating = false;
		}
		levelManagerScript.player.combatEngaged = false;
		if(!dead){
			FinishAction();
		}
	}
	
	public int CalculateAttackNumber(bool doubleAttack) {
		int numAttacksReturn = numAttacks;
		
		if (doubleAttack) {
			numAttacksReturn = numAttacksReturn * 2;
		}
		
		return numAttacksReturn;
	}
	
	public int EstimateDamage(GameObject targetUnit) {
		
		int numUnitAttacks = CalculateAttackNumber(agility-5>targetUnit.GetComponent<UnitScript>().agility);
		int numTargetAttacks = targetUnit.GetComponent<UnitScript>().CalculateAttackNumber(targetUnit.GetComponent<UnitScript>().agility-5>agility);
		int damageAmount;
		
		int tempUnitHealth = health;
		int tempTargetHealth = targetUnit.GetComponent<UnitScript>().health;
		
		bool unitDeath = false;
		bool targetDeath = false;
		
		Debug.Log("Numtargetattacks = "+numTargetAttacks.ToString());
		// check if unit is in target's range
		if (targetUnit.GetComponent<UnitScript>().range < range) {
			numTargetAttacks = 0;
		}
		Debug.Log("Numtargetattacks = "+numTargetAttacks.ToString());
		
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
		
		Debug.Log("taken damage against unit "+targetUnit.GetComponent<UnitScript>().charName+" = "+totalTakenDamage.ToString());
		Debug.Log("dealt damage against unit "+targetUnit.GetComponent<UnitScript>().charName+" = "+totalDealtDamage.ToString());
		
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
		
		if (unitDeath) {
			tempDeathPriority = -5;
		} else if (targetDeath) {
			tempDeathPriority = 50;
		}
		
		tempPriority = tempDealtPriority - tempTakenPriority + tempDeathPriority;
		
		Debug.Log("priority against unit "+targetUnit.GetComponent<UnitScript>().charName+" = "+tempPriority.ToString());
		
		return tempPriority;
		
	}
	
	// wrapper method for calling coroutines on other object
	public void ExperienceCaller(float addedExp) {
		StartCoroutine(AddExperience(addedExp));
	}
	
	public IEnumerator AddExperience(float addedExp) {
		//Debug.Log("New Experience = " + addedExp.ToString());
		int oldExp = experience;
		//Debug.Log("Old Experience = " + oldExp.ToString());
		experience += (int) addedExp;
		if ( (int) addedExp == 0 ) {
			experience += 1;
		}
		while (experience >= 100) {
			//start bar from old experience
			gainingExperience = true;
			StartCoroutine(ExperienceProgress(oldExp, 100-oldExp));
			experience-=100;
			oldExp = 0;
			yield return new WaitWhile(() => gainingExperience);
		}
		gainingExperience = true;
		StartCoroutine(ExperienceProgress(oldExp, experience-oldExp));
		yield return new WaitWhile(() => gainingExperience);
		//star bar from old exp for remainder
		progressBar.SetActive(false);
	}
	
	public IEnumerator ExperienceProgress(int startVal, int ExpVal){
		yield return new WaitForSeconds(0.5f);
		int totalIncrease = ExpVal;
		int increase = 0;
		while(totalIncrease!=0){
			yield return new WaitForSeconds(0.02f);
			float barPosition = ((float)(startVal+increase))/ 100f;
			increase++;
			totalIncrease--;
			progressBar.transform.GetChild(0).gameObject.GetComponent<Slider>().value = barPosition;
			SoundManager.instance.ExpGain();
			//apply barPosition to slider
		}
		
		if(startVal+ExpVal == 100){
			yield return new WaitForSeconds(1.0f);
			levelUpSheet.SetActive(true);
			StartCoroutine(LevelUp());
			yield return new WaitWhile(() => levelUpSheet.activeSelf);
		}
		yield return new WaitForSeconds(1.0f);
		gainingExperience = false;
	}
	
	public IEnumerator LevelUp(){
		SoundManager.instance.LevelUp();
		level++;
		
		GameObject[] statTextArray = new GameObject[4];
		statTextArray[0] = levelUpSheet.transform.GetChild(2).GetChild(7).gameObject;
		statTextArray[1] = levelUpSheet.transform.GetChild(2).GetChild(10).gameObject;
		statTextArray[2] = levelUpSheet.transform.GetChild(2).GetChild(13).gameObject;
		statTextArray[3] = levelUpSheet.transform.GetChild(2).GetChild(14).gameObject;
		
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
	
	public void FinishAction() {
		if (this == null || gameObject == null) {
			return;
		}
		
		finished = true;
		
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
		
		foreach(GameObject unit in levelManagerScript.unitList){
			if (!unit.GetComponent<UnitScript>().finished && (unit.tag == gameObject.tag)) {
				phaseOver = false;
			}
		}
		
		if (phaseOver) {
			Debug.Log("GOT HERE FIRST SUCKA");
			levelManagerScript.NextPhase();
		}
	}
	
	public IEnumerator Death(){
		gameObject.GetComponent<SpriteRenderer>().enabled = false;
		dead = true;
		levelManagerScript.unitList.Remove(gameObject);
		yield return new WaitWhile(() => experienceUpdating);
		//levelManagerScript.levelEndChecking = true;
		levelManagerScript.CheckLevelEnd();
		//yield return new WaitWhile(() => levelManagerScript.levelEndChecking);
		FinishAction();
		
		tileCur.GetComponent<TileScript>().occupyingObject = null;
		
		Destroy(gameObject);
		
	}
	
}
 