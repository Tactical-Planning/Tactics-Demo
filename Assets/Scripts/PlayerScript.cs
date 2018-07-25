using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PlayerScript : MonoBehaviour {

	public int[] location;
	
	private float timeToWait = 0; // variable to check if player has recently moved
	private float slowTimeToWait = (float) 0.15; // time player must wait after moving
	private float fastTimeToWait = (float) 0.05; // time player must wait after moving if spacebar is held

	
	//player menu data members
	public GameObject playerMenu;
	public bool menuOpen;
	public bool menuAction;
	public bool moveAction;
	public bool attackTarget;
	public bool postMove;
	

	
	public Button attackButton; 
	public Button moveButton;
	public Button inspectButton;
	public Button itemButton;
	public Button waitButton;
	public Button endTurnButton;
	
	//inspect menu data members;
	public GameObject inspectSheet;
	public bool inspectSheetOpen;
	
	public Vector3 levelUpSheetLocation;
	
	public Text characterNameText;
	public Text levelText;
	public Text classText;
	public Text healthValueText;
	public Text agilityValueText;
	public Text attackValueText;
	public Text defenseValueText;
	public Text speedValueText;
	public Text rangeValueText;
	public Text experienceValueText;
	
	public Text nameText;
	public Text healthText;
	
	//Info sheet data members
	public GameObject infoSheet;
	
	public Text infoNameText;
	public Text infoClassText;
	public Text infoHealthText;
	
	//item menu data members
	public GameObject itemMenu;
	public bool itemMenuOpen;
	public bool itemTargeting;
	public bool combatEngaged;
	
	public Text itemNameText;
	
	public Text itemDescriptionText;
	
	public int zIndex = 0;
	public float[] zoomLevels = {5,3,5,7};
	private bool mapbool;
	private bool cyclebool;
	
	public GameObject currentUnit;
	public List<GameObject> selectedObjectList;
	private List<List<GameObject>> highlightsList;
	private List<List<GameObject>> attackHighlightsList;
	private List<GameObject> targetHighlightsList;
	public GameObject tileHighlight;
	public GameObject attackHighlight;
	public GameObject supportHighlight;
	
	public GameObject prevSelectedButton;
	public int prevItemIndex; 
	
	public LevelManagerScript lmInstance;
	
	void Start () {
		
		lmInstance = this.transform.parent.gameObject.GetComponent<LevelManagerScript>();
		
		//get player menu and button data members
		playerMenu = GameObject.Find("PlayerMenu");
		
		moveButton = playerMenu.transform.GetChild(0).gameObject.GetComponent<Button>();
		moveButton.onClick.AddListener(MoveHandle);
		
		attackButton = playerMenu.transform.GetChild(1).gameObject.GetComponent<Button>();
		attackButton.onClick.AddListener(AttackHandle);
		
		inspectButton = playerMenu.transform.GetChild(2).gameObject.GetComponent<Button>();
		inspectButton.onClick.AddListener(InspectHandle);
		
		itemButton = playerMenu.transform.GetChild(3).gameObject.GetComponent<Button>();
		itemButton.onClick.AddListener(ItemHandle);
		
		waitButton = playerMenu.transform.GetChild(4).gameObject.GetComponent<Button>();
		waitButton.onClick.AddListener(WaitHandle);
		
		endTurnButton = playerMenu.transform.GetChild(5).gameObject.GetComponent<Button>();
		endTurnButton.onClick.AddListener(EndTurnHandle);
		
		//get info sheet data members
		infoSheet = GameObject.Find("InfoSheet");
		infoNameText = infoSheet.transform.GetChild(1).gameObject.GetComponent<Text>();
		infoClassText = infoSheet.transform.GetChild(2).gameObject.GetComponent<Text>();
		infoHealthText = infoSheet.transform.GetChild(3).gameObject.GetComponent<Text>();
		// get inspect sheet data members
		inspectSheet = lmInstance.characterSheet;
		levelUpSheetLocation = inspectSheet.transform.position;
		
		characterNameText = inspectSheet.transform.GetChild(2).Find("CharacterNameText").GetComponent<Text>();
		levelText = inspectSheet.transform.GetChild(2).Find("LevelText").GetComponent<Text>();
		classText = inspectSheet.transform.GetChild(2).Find("ClassText").GetComponent<Text>();
		healthValueText = inspectSheet.transform.GetChild(2).Find("HealthValueText").GetComponent<Text>();
		agilityValueText = inspectSheet.transform.GetChild(2).Find("AgilityValueText").GetComponent<Text>();
		attackValueText = inspectSheet.transform.GetChild(2).Find("AttackValueText").GetComponent<Text>();
		defenseValueText = inspectSheet.transform.GetChild(2).Find("DefenseValueText").GetComponent<Text>();
		speedValueText = inspectSheet.transform.GetChild(2).Find("SpeedValueText").GetComponent<Text>();
		rangeValueText = inspectSheet.transform.GetChild(2).Find("RangeValueText").GetComponent<Text>();
		experienceValueText = inspectSheet.transform.GetChild(2).Find("ExperienceValueText").GetComponent<Text>();
		
		
		// get item menu data members
		itemMenu = GameObject.Find("ItemMenu");
		itemNameText = itemMenu.transform.GetChild(1).gameObject.GetComponent<Text>();
		itemDescriptionText = itemMenu.transform.GetChild(2).gameObject.GetComponent<Text>();
		
		//set all menu components to false at beginning
		playerMenu.SetActive(false);
		inspectSheet.SetActive(false);
		infoSheet.SetActive(false);
		itemMenu.SetActive(false);
		
		selectedObjectList = new List<GameObject>();
		highlightsList = new List<List<GameObject>>();
		attackHighlightsList = new List<List<GameObject>>();
		targetHighlightsList = new List<GameObject>();
		currentUnit = null;
		
		
		//set state flags to false
		menuOpen = false;
		menuAction = false;
		moveAction = false;
		combatEngaged = false;
		attackTarget = false;
		
		
		inspectSheetOpen = false;
		itemMenuOpen = false;
		itemTargeting = false;
		
		mapbool = false;
		cyclebool = false;
		
		prevSelectedButton = null;
		
		CheckInfoSheet();
	}
	
	// Update is called once per frame
	void Update () {
		if (lmInstance.phaseCounter != 0){
			return;
		}
		if(combatEngaged){
			return;
		}
		
		if (Input.GetAxisRaw("MapZoom")>0) {
			if (!mapbool) {
				mapbool = true;
				zIndex += 1;
				if (zIndex >= zoomLevels.GetLength(0)) { zIndex = 0; }
			}
		} else {
			mapbool = false;
		}
		//float soundWait = timeToWait;
		
		
		
		// play sound effects for directional inputs
		if (Input.GetAxisRaw("Horizontal")!=0||Input.GetAxisRaw("Vertical")!=0) {
			
			if(menuOpen){
				if(prevSelectedButton!= null && prevSelectedButton != EventSystem.current.currentSelectedGameObject){
					SoundManager.instance.MoveCursor();
				}else{
					//play failed sound
				}

				prevSelectedButton = EventSystem.current.currentSelectedGameObject;
				
			}	
		}
		if(itemMenuOpen && prevItemIndex!=currentUnit.GetComponent<UnitScript>().itemIndex){
			prevItemIndex = currentUnit.GetComponent<UnitScript>().itemIndex;
			SoundManager.instance.MoveCursor();
		}
	
		
		

		//X is back button
		if (Input.GetButtonDown("Cancel")) {
			
			if((menuOpen||moveAction||attackTarget||postMove||itemTargeting)){
				SoundManager.instance.MenuCancel();
			}
			//after choosing an item, while choosing a target
			//returns to choosing an item menu
			if (itemTargeting){
				
				//move cursor back to unit
				UnitFocus(currentUnit);
				
				if(currentUnit.GetComponent<UnitScript>().tilePrev != null){
					location[0] = (int) currentUnit.GetComponent<UnitScript>().tilePrev.transform.position.x;
					location[1] = (int) currentUnit.GetComponent<UnitScript>().tilePrev.transform.position.y;
					currentUnit.GetComponent<UnitScript>().MoveUnit(currentUnit.GetComponent<UnitScript>().tilePrev, true);
					Selection();
					location[0] = (int) currentUnit.GetComponent<UnitScript>().tilePrev.transform.position.x;
					location[1] = (int) currentUnit.GetComponent<UnitScript>().tilePrev.transform.position.y;
					currentUnit.GetComponent<UnitScript>().MoveUnit(currentUnit.GetComponent<UnitScript>().tilePrev, true);
				}else{
					Selection();
				}
				
				
				
				itemMenu.SetActive(true);
				itemMenuOpen = true;
				
				itemTargeting = false;
				menuOpen = true;
				
			}
			
			else if (attackTarget) {
				UnitFocus(currentUnit);
				
				if(currentUnit.GetComponent<UnitScript>().tilePrev != null){
					location[0] = (int) currentUnit.GetComponent<UnitScript>().tilePrev.transform.position.x;
					location[1] = (int) currentUnit.GetComponent<UnitScript>().tilePrev.transform.position.y;
					currentUnit.GetComponent<UnitScript>().MoveUnit(currentUnit.GetComponent<UnitScript>().tilePrev, true);
					Selection();
					location[0] = (int) currentUnit.GetComponent<UnitScript>().tilePrev.transform.position.x;
					location[1] = (int) currentUnit.GetComponent<UnitScript>().tilePrev.transform.position.y;
					currentUnit.GetComponent<UnitScript>().MoveUnit(currentUnit.GetComponent<UnitScript>().tilePrev, true);
				}else{
					Selection();
				}
				
				attackTarget = false;
				playerMenu.SetActive(true);
				menuOpen = true;
				
			}
			
			//while inspectSheet is open
			else if(inspectSheetOpen){
				inspectSheetOpen = false;
				inspectSheet.SetActive(false);
				inspectSheet.transform.position = levelUpSheetLocation;
				
				
				if (currentUnit.tag=="playerUnit") {
					CheckIfItems();
					playerMenu.SetActive(true);
				} else {
					menuOpen = false;
				}
			}
			//while item menu is open
			else if(itemMenuOpen){
				itemMenuOpen = false;
				itemMenu.SetActive(false);
				playerMenu.SetActive(true);
				
			}
			//while in primary unit menu
			else if(menuOpen && !postMove){
				
				playerMenu.SetActive(false);
				attackButton.interactable = true;
				moveButton.interactable = true;
				inspectButton.interactable = true;
				itemButton.interactable = true;
				waitButton.interactable = true;
				menuOpen = false;
				BigCleanUp();
				
				
			}
			
			
			//while selecting move destination
			else if(moveAction){
				
				//move cursor back to unit
				UnitFocus(currentUnit);
				//open original menu - reenable move button
				moveButton.interactable = true;
				moveAction = false;
				menuOpen = true;
				CheckIfItems();
				playerMenu.SetActive(true);
				
			}
			
			//while in post-move menu
			//returns to move selection state
			else if(postMove){
			
				playerMenu.SetActive(false);
				
				postMove = false;
				moveAction = true;
				menuAction = true;
				menuOpen = false;
				
				currentUnit.GetComponent<UnitScript>().MoveBack();
					
			}
			
		}
		
		//timetowait is decremented each frame
		if (timeToWait > 0) {
			timeToWait -= Time.deltaTime;
		}
		
		//if the item menu is open listen for button inputs, do relevant action
		if(itemMenuOpen && !menuAction){
			//if input is left or right, cycle through item menu
			if(((int) Input.GetAxisRaw("Horizontal") != 0) && (timeToWait<=0)){
				if ((int) Input.GetAxisRaw("Horizontal")>0){
					currentUnit.GetComponent<UnitScript>().itemIndex++;
					//if we've reached the end of the item list loop back around to beginning
					if(currentUnit.GetComponent<UnitScript>().itemIndex == currentUnit.GetComponent<UnitScript>().itemList.Count){
						currentUnit.GetComponent<UnitScript>().itemIndex = 0;
					}
				}else{
					currentUnit.GetComponent<UnitScript>().itemIndex--;
					//if left is pressed and we are the beginning of list update index to end of list
					if(currentUnit.GetComponent<UnitScript>().itemIndex == -1){
						currentUnit.GetComponent<UnitScript>().itemIndex = currentUnit.GetComponent<UnitScript>().itemList.Count-1;
					}	
				}
				itemNameText.text = currentUnit.GetComponent<UnitScript>().CurrentItem().itemName;
				itemDescriptionText.text = currentUnit.GetComponent<UnitScript>().CurrentItem().description;
			
				timeToWait = slowTimeToWait;
			}
			
			// if input is Z, choose a tile to target
			if (Input.GetButtonDown("Submit")) {
				SoundManager.instance.MenuSelect();
				itemMenuOpen = false;
				itemTargeting = true;
				itemMenu.SetActive(false);
				
				menuOpen = false;
				menuAction = true;
				
				GameObject tempUnit;
				tempUnit = currentUnit;
				BigCleanUp();
				currentUnit = tempUnit;
				
				currentUnit.GetComponent<UnitScript>().CurrentItem().validTiles = new List<List<int>>();
				currentUnit.GetComponent<UnitScript>().CurrentItem().FindValidTiles(location[0],location[1],0);
				
				HighlightRadius();
			}
			
		}
		
		
		//check if the menu is open, if it is we don't want cursor actions available
		//for 1 frame after menu closes, cursor will be locked so that the z-press from menu doesn't select unit
		if(menuOpen == false && menuAction == false){
			
			//cycle through units
			if (Input.GetAxisRaw("UnitCycle")<0) {
				if (!cyclebool){
					
					if(itemTargeting){
						lmInstance.unitListIndex++;
						if (lmInstance.unitListIndex >= lmInstance.unitList.Count) {
							lmInstance.unitListIndex = 0;
						}
						UnitFocus(lmInstance.unitList[lmInstance.unitListIndex]);
						TileCleanUp();
						foreach (List<int> tileLocation in currentUnit.GetComponent<UnitScript>().CurrentItem().validTiles) {
							if (location[0]==tileLocation[0] && location[1]==tileLocation[1]) {
								HighlightRadius();
								break;
							}
						}
						
					} else if (attackTarget) {
						lmInstance.unitListInRangeIndex++;
						if (lmInstance.unitListInRangeIndex >= lmInstance.unitListInRange.Count) {
							lmInstance.unitListInRangeIndex = 0;
						}
						UnitFocus(lmInstance.unitListInRange[lmInstance.unitListInRangeIndex]);
					} else{
						do {
							lmInstance.unitListIndex++;
							if (lmInstance.unitListIndex >= lmInstance.unitList.Count) {
								lmInstance.unitListIndex = 0;
							}
						} while (lmInstance.unitList[lmInstance.unitListIndex].GetComponent<UnitScript>().finished || lmInstance.unitList[lmInstance.unitListIndex].tag != "playerUnit");
						UnitFocus(lmInstance.unitList[lmInstance.unitListIndex]);
					}
					
					cyclebool = true;
					CheckInfoSheet();
				}
			} else if (Input.GetAxisRaw("UnitCycle")>0) {
				if (!cyclebool){
					if(itemTargeting){
						lmInstance.unitListIndex--;
						if (lmInstance.unitListIndex <0) {
							lmInstance.unitListIndex = lmInstance.unitList.Count-1;
						}
						UnitFocus(lmInstance.unitList[lmInstance.unitListIndex]);
						TileCleanUp();
						foreach (List<int> tileLocation in currentUnit.GetComponent<UnitScript>().CurrentItem().validTiles) {
							if (location[0]==tileLocation[0] && location[1]==tileLocation[1]) {
								HighlightRadius();
								break;
							}
						}
					} else if (attackTarget) {
						lmInstance.unitListInRangeIndex--;
						if (lmInstance.unitListInRangeIndex < 0) {
							lmInstance.unitListInRangeIndex = lmInstance.unitListInRange.Count-1;
						}
						UnitFocus(lmInstance.unitListInRange[lmInstance.unitListInRangeIndex]);
					} else{
						do {
							lmInstance.unitListIndex--;
							if (lmInstance.unitListIndex < 0) {
								lmInstance.unitListIndex = lmInstance.unitList.Count-1;
							}
						} while (lmInstance.unitList[lmInstance.unitListIndex].GetComponent<UnitScript>().finished || lmInstance.unitList[lmInstance.unitListIndex].tag != "playerUnit");
						UnitFocus(lmInstance.unitList[lmInstance.unitListIndex]);
					}
					cyclebool = true;
					CheckInfoSheet();
				}
			} else {
				cyclebool = false;
			}
			
			//cursor movement
			if ( ((int) Input.GetAxisRaw("Horizontal") != 0) || ((int) Input.GetAxisRaw("Vertical") != 0)) {
				//if timetowait has reached 0, we allow movement.
				if(timeToWait<=0){
					Movement();
					CheckInfoSheet();
					// if choosing target for item, make a highlight around cursor
					if (itemTargeting){
						TileCleanUp();
						foreach (List<int> tileLocation in currentUnit.GetComponent<UnitScript>().CurrentItem().validTiles) {
							if (location[0]==tileLocation[0] && location[1]==tileLocation[1]) {
								HighlightRadius();
								break;
							}
						}
					}
				}
			}
			//leftshift+z is pressed to highlight a units ranges, but not select it
			if ( (Input.GetAxisRaw("MultSelect")>0 && Input.GetButtonDown("Submit")) || (Input.GetButtonDown("Submit") && Input.GetAxisRaw("MultSelect")>0) ) {
				Selection();
			} else if ( Input.GetButtonDown("Submit") ){
				//normal z is pressed
				Input.ResetInputAxes();
				GameObject selectedObject = GetSelectedObject();
				//move code
				if (currentUnit != null && selectedObject == null && moveAction == true) {
					//check unit's available tiles by iterating see if selected tile is valid for movement
					foreach (List<int> tileLocation in currentUnit.GetComponent<UnitScript>().availableTiles) {
						if (location[0]==tileLocation[0] && location[1]==tileLocation[1]) {
							currentUnit.GetComponent<UnitScript>().FindPath(new int[] {location[0],location[1]} );
							
							//call movement method from unit
							currentUnit.GetComponent<UnitScript>().MoveUnit(lmInstance.tileArray[location[0],location[1]]);
							
							//open secondary post move menu
							moveButton.interactable = false;
							CheckIfItems();
							playerMenu.SetActive(true);
							prevSelectedButton = attackButton.gameObject;
							attackButton.Select();
							menuOpen = true;
							moveAction = false;
							menuAction = true;
							postMove = true;
							break;
						}
					}
				} else if(moveAction){
					//case that move selection is taking place, dont want to select new unit to consider
					//doing nothing
					
				} else if (itemTargeting){
					foreach (List<int> tileLocation in currentUnit.GetComponent<UnitScript>().CurrentItem().validTiles) {
						if (location[0]==tileLocation[0] && location[1]==tileLocation[1]) {
							//call UseItem method from unit
							currentUnit.GetComponent<UnitScript>().UseItem();
							
							//clean up itemtargeting's flags
							BigCleanUp();
							itemTargeting = false;
							postMove = false;
							menuAction = true;
							break;
						}
					}
				} else if (attackTarget) {
					if (selectedObject!=null) {
						foreach (GameObject unit in lmInstance.unitListInRange) {
							//Debug.Log("Unit list in range has stuff");
							if (unit == selectedObject){
								infoSheet.SetActive(false);
								combatEngaged = true;
								StartCoroutine(currentUnit.GetComponent<UnitScript>().ResolveCombat(selectedObject));
								
								//clean up attackTarget's flags
								BigCleanUp();
								attackTarget = false;
								postMove = false;
								menuAction = true;
								break;
							}
						}
					}

				} else if (selectedObject!= null && selectedObject.tag == "enemyUnit") {
					InspectEnemy();
					SoundManager.instance.MenuSelect();
					
				} else if (selectedObject!= null && selectedObject.tag == "playerUnit") {
					//pressing z on a unit with no special state
					SoundManager.instance.MenuSelect();
					
					prevSelectedButton = moveButton.gameObject;
					//check if the unit has already acted, allow to inspect only
					if (selectedObject.GetComponent<UnitScript>().finished) {
						FreshSelect();
						attackButton.interactable = false;
						moveButton.interactable = false;
						inspectButton.Select();
						itemButton.interactable = false;
						waitButton.interactable = false;
						
						playerMenu.SetActive(true);
						menuOpen = true;
					} else {
						//normal unit selection, hasn't acted this turn
						FreshSelect();
						menuOpen = true;
						CheckIfItems();
						attackButton.interactable = true;
						moveButton.interactable = true;
						waitButton.interactable = true;
						playerMenu.SetActive(true);
						moveButton.Select();
					}

					
				} else {
					//in the case unit(s) have been selected with shift+z, and a blank selected, highlights will disappear
					BigCleanUp();
				}
			}
		} else if ( menuAction == true ) {
			menuAction = false;
		}
		
	}
	
	// Movement is called once per frame that timeToWait is not greater than zero
	void Movement () {
		
		int horizontal = 0;
		int vertical = 0;
		
		horizontal = (int) Input.GetAxisRaw("Horizontal");
		vertical = (int) Input.GetAxisRaw("Vertical");
		
		int tempHorizontal = location[0] + horizontal;
		int tempVertical = location[1] + vertical;
		
		
		if (tempHorizontal < 0 || tempHorizontal >= transform.parent.GetComponent<LevelManagerScript>().tileArrayLength[0]) {
			tempHorizontal = location[0];
		}
		
		if (tempVertical < 0 || tempVertical >= transform.parent.GetComponent<LevelManagerScript>().tileArrayLength[1]) {
			tempVertical = location[1];
		}
		
		if(!((tempHorizontal == location[0])&&(tempVertical == location[1]))){
			SoundManager.instance.MoveCursor();
		}
		
		location[0] = tempHorizontal;
		location[1] = tempVertical;
		/*
		if (location[0]==0 && location[1]==0) {
			GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().level_number = 1;
			SceneManager.LoadScene("SceneA");
		}
		*/
		transform.position = transform.parent.GetComponent<LevelManagerScript>().tileArray[location[0],location[1]].transform.position;
	
		transform.parent.GetComponent<LevelManagerScript>().activeTile = transform.parent.GetComponent<LevelManagerScript>().tileArray[location[0],location[1]];
	
		if (horizontal != 0 || vertical != 0) {
			if (Input.GetButton("FastMove")) {
				timeToWait = fastTimeToWait;
			} else {
				timeToWait = slowTimeToWait;
			}
		}
	}
	
	
	
	GameObject GetSelectedObject() {
		GameObject selectedObject = transform.parent.GetComponent<LevelManagerScript>().tileArray[location[0],location[1]].GetComponent<TileScript>().occupyingObject;
		return selectedObject;
	}
	
	void FreshSelect() {
		GameObject selectedObject = GetSelectedObject();
		if (selectedObject == null || selectedObject.GetComponent<UnitScript>().selectedFlag) {
			BigCleanUp();
		} else  {
			BigCleanUp();
			Selection();
		}
	}
	
	//Selection is called when the Z key is pressed, it selects a tile, and whatever is on it
	void Selection() {
		//Debug.Log("Running Selection");
		GameObject selectedObject = GetSelectedObject();
		if( selectedObject != null) {
			if ( (selectedObject.GetComponent<UnitScript>().tag == "playerUnit" || selectedObject.GetComponent<UnitScript>().tag == "enemyUnit") && selectedObject.GetComponent<UnitScript>().selectedFlag) {
				CleanUp(selectedObjectList.IndexOf(selectedObject));	
			} else if( (selectedObject.GetComponent<UnitScript>().tag == "playerUnit" || selectedObject.GetComponent<UnitScript>().tag == "enemyUnit") && !selectedObject.GetComponent<UnitScript>().selectedFlag) {
				currentUnit = selectedObject;
				selectedObject.GetComponent<UnitScript>().selectedFlag = true;
				selectedObject.GetComponent<UnitScript>().FindAvailableMoves(location[0],location[1],0);
				selectedObject.GetComponent<UnitScript>().FindAvailableAttacks(location[0],location[1],0);
				
				selectedObjectList.Add(selectedObject);
			} 
			
			TileCleanUp();
			
			foreach (GameObject unit in selectedObjectList) {
				List<GameObject> moves = new List<GameObject>();
				foreach (List<int> subList in unit.GetComponent<UnitScript>().availableHLTiles) {
					GameObject highlightInstance = Instantiate(tileHighlight, transform.parent.GetComponent<LevelManagerScript>().tileArray[subList[0],subList[1]].transform.position, Quaternion.identity) as GameObject;
					Color tmp = highlightInstance.GetComponent<SpriteRenderer>().color;
					float dist = (float) subList[2];
					tmp.a = (float) 0.8 - (dist/4);
					if (tmp.a<0.4f) {tmp.a=0.4f;}
					highlightInstance.GetComponent<SpriteRenderer>().color = tmp;
					
					moves.Add(highlightInstance);
				}
				highlightsList.Add(moves);
			}
			
			foreach (GameObject unit in selectedObjectList) {
				List<GameObject> attacks = new List<GameObject>();
				foreach (List<int> subList in unit.GetComponent<UnitScript>().availableAttacks) {
					bool hlexists = false;
					foreach(List<GameObject> sl in highlightsList){
						
						foreach(GameObject hl in sl){
							
							if((hl.transform.position.x == subList[0]) && (hl.transform.position.y == subList[1])){
								
								hlexists = true;
								break;
							}
						}
						if (hlexists){break;}
						
					}
					
					foreach(List<GameObject> sl in attackHighlightsList){
						
						foreach(GameObject hl in sl){
							
							if((hl.transform.position.x == subList[0]) && (hl.transform.position.y == subList[1])){
								
								hlexists = true;
								break;
							}
						}
						if (hlexists){break;}
						
					}
					
					if(!hlexists){
						GameObject highlightInstance = Instantiate(attackHighlight, transform.parent.GetComponent<LevelManagerScript>().tileArray[subList[0],subList[1]].transform.position, Quaternion.identity) as GameObject;
						Color tmp = highlightInstance.GetComponent<SpriteRenderer>().color;
				
						tmp.a = (float) 0.5;
						highlightInstance.GetComponent<SpriteRenderer>().color = tmp;
						
						attacks.Add(highlightInstance);
					}
				}
				
				
				attackHighlightsList.Add(attacks);
			}
		}
	}
	
	//make a highlight around the cursor with radius equal to item radius
	void HighlightRadius() {
		TileCleanUp();
		
		currentUnit.GetComponent<UnitScript>().CurrentItem().tilesInRadius = new List<List<int>>();
		currentUnit.GetComponent<UnitScript>().CurrentItem().FindValidTilesInRadius(location[0],location[1],0);
		
		// if the item is for support, make the highlight green
		GameObject tempHighlight;
		if (currentUnit.GetComponent<UnitScript>().CurrentItem().support) {
			tempHighlight = supportHighlight;
		} else {
			tempHighlight = attackHighlight;
		}
		
		List<GameObject> targetedTiles = new List<GameObject>();
		foreach (List<int> subList in currentUnit.GetComponent<UnitScript>().CurrentItem().tilesInRadius) {
			
			GameObject highlightInstance = Instantiate(tempHighlight, transform.parent.GetComponent<LevelManagerScript>().tileArray[subList[0],subList[1]].transform.position, Quaternion.identity) as GameObject;
			Color tmp = highlightInstance.GetComponent<SpriteRenderer>().color;
			tmp.a = (float) 0.5;
			highlightInstance.GetComponent<SpriteRenderer>().color = tmp;
			
			targetedTiles.Add(highlightInstance);
		}
		//Debug.Log("Number of tiles: " + currentUnit.GetComponent<UnitScript>().CurrentItem().tilesInRadius.Count);
		targetHighlightsList = targetedTiles;
	}
	
	
	void BigCleanUp() {
		while (selectedObjectList.Count != 0) {
			CleanUp(0);
		}
		
		foreach(GameObject hl in targetHighlightsList) {
			Destroy(hl);
		}
		targetHighlightsList = new List<GameObject>();
	}
	
	void CleanUp(int index) {
		//Debug.Log("Running Clean Up\nSelectedObjectList.Count = " + selectedObjectList.Count + "\nhighlightsList.Count = " + highlightsList.Count + "\nattackHighlightsList.Count = " + attackHighlightsList.Count);
		
		
		currentUnit = null;
		selectedObjectList[index].GetComponent<UnitScript>().selectedFlag = false;
		if (highlightsList.Count>index) {
			foreach(GameObject highlight in highlightsList[index]) {
				Destroy(highlight);
			}
			highlightsList.RemoveAt(index);
		}
		if (attackHighlightsList.Count>index) {
			foreach(GameObject hl in attackHighlightsList[index]){
				Destroy(hl);
			}
			attackHighlightsList.RemoveAt(index);
		}
		
		selectedObjectList.RemoveAt(index);

	}
	
	void TileCleanUp() {
		
		
		foreach(List<GameObject> subList in highlightsList) {
			foreach(GameObject highlight in subList) {
				Destroy(highlight);
			}
		}
		foreach(List<GameObject> subList in attackHighlightsList) {
			foreach(GameObject hl in subList){
				Destroy(hl);
			}
		}

		foreach(GameObject hl in targetHighlightsList) {
			Destroy(hl);
		}
		
		highlightsList = new List<List<GameObject>>();
		attackHighlightsList = new List<List<GameObject>>();
		targetHighlightsList = new List<GameObject>();
	}
	
	void AttackHandle(){
		Debug.Log("we're coding real good now");
		
		playerMenu.SetActive(false);
		menuOpen = false;
		menuAction = true;
		
		currentUnit.GetComponent<UnitScript>().FindAvailableImmediateAttacks(location[0],location[1],0);
		
		if (currentUnit!=null) {
			currentUnit.GetComponent<UnitScript>().selectedFlag = false;
			selectedObjectList.Remove(currentUnit);
		}
		
		TileCleanUp();
		
		// populate attack range highlights
		List<GameObject> attacks = new List<GameObject>();
		foreach (List<int> subList in currentUnit.GetComponent<UnitScript>().availableImmediateAttacks) {
			GameObject highlightInstance = Instantiate(attackHighlight, transform.parent.GetComponent<LevelManagerScript>().tileArray[subList[0],subList[1]].transform.position, Quaternion.identity) as GameObject;
			Color tmp = highlightInstance.GetComponent<SpriteRenderer>().color;
			tmp.a = 0.3f;
			highlightInstance.GetComponent<SpriteRenderer>().color = tmp;
			
			attacks.Add(highlightInstance);
		}
		targetHighlightsList = attacks;

		// populate attack targets list in level manager
		lmInstance.PopulateAttackTargetsList(currentUnit.GetComponent<UnitScript>().availableImmediateAttacks);
		
		attackTarget = true;
		
		SoundManager.instance.MenuSelect();
		
	}
	
	void MoveHandle() {
		//Debug.Log("moving state");
		
		moveAction = true;
		playerMenu.SetActive(false);
		menuOpen = false;
		menuAction = true;
		
		SoundManager.instance.MenuSelect();
	}
	
	void InspectHandle() {
		
		inspectSheetOpen = true;
		
		UnitScript currentUnitScript = currentUnit.GetComponent<UnitScript>();
		
		characterNameText.text = currentUnitScript.charName;
		levelText.text = currentUnitScript.level.ToString();
		classText.text = currentUnitScript.className;
		
		healthValueText.text = currentUnitScript.maxHealth.ToString();
		agilityValueText.text = currentUnitScript.agility.ToString();
		attackValueText.text = currentUnitScript.attack.ToString();
		defenseValueText.text = currentUnitScript.defense.ToString();
		speedValueText.text = currentUnitScript.speed.ToString();
		rangeValueText.text = currentUnitScript.range.ToString();
		experienceValueText.text = currentUnitScript.experience.ToString()+"/100";
		
		inspectSheet.transform.position = playerMenu.transform.GetChild(playerMenu.transform.childCount-1).position;
		inspectSheet.SetActive(true);
		playerMenu.SetActive(false);
		
		SoundManager.instance.MenuSelect();
	}
	
	void ItemHandle() {
		// launch item window
		// close playermenu
		itemMenuOpen = true;
		prevItemIndex = currentUnit.GetComponent<UnitScript>().itemIndex;
		
		itemNameText.text = currentUnit.GetComponent<UnitScript>().CurrentItem().itemName;
		itemDescriptionText.text = currentUnit.GetComponent<UnitScript>().CurrentItem().description;
		itemMenu.SetActive(true);
		playerMenu.SetActive(false);
		
		menuAction = true;
		
		SoundManager.instance.MenuSelect();
		//currentUnit.GetComponent<UnitScript>().UseItem(0);
	}
	
	void WaitHandle() {
		//Debug.Log("unit ended turn");
		
		currentUnit.GetComponent<UnitScript>().Wait();

		
		attackButton.interactable = true;
		moveButton.interactable = true;
		inspectButton.interactable = true;
		itemButton.interactable = true;
		
		menuOpen = false;
		menuAction = true;
		postMove = false;
		playerMenu.SetActive(false);
		BigCleanUp();
	
		SoundManager.instance.MenuSelect();
	}
	
	void EndTurnHandle() {
		attackButton.interactable = true;
		moveButton.interactable = true;
		inspectButton.interactable = true;
		itemButton.interactable = true;
		
		menuOpen = false;
		menuAction = true;
		postMove = false;
		playerMenu.SetActive(false);
		BigCleanUp();
		
		foreach (GameObject unit in lmInstance.unitList) {
			if (unit.tag == "playerUnit" && ! (unit.GetComponent<UnitScript>().finished)) {
				unit.GetComponent<UnitScript>().FinishAction();
			}
		}
		SoundManager.instance.MenuSelect();
	}
	
	void InspectEnemy() {
		currentUnit = GetSelectedObject();
		InspectHandle();
		menuOpen = true;
	}
	
	void CheckIfItems() {
		if (currentUnit.GetComponent<UnitScript>().itemList.Count == 0 || currentUnit.GetComponent<UnitScript>().finished) { itemButton.interactable = false; }
		else { itemButton.interactable = true; }
	}
	
	public void UnitFocus(GameObject unit) {
		transform.position = unit.transform.position;
		location[0] = (int) unit.GetComponent<UnitScript>().tileCur.transform.position.x;
		location[1] = (int) unit.GetComponent<UnitScript>().tileCur.transform.position.y;
		transform.parent.GetComponent<LevelManagerScript>().activeTile = transform.parent.GetComponent<LevelManagerScript>().tileArray[location[0],location[1]];
	}
	
	public void CheckInfoSheet() {
		GameObject selectedObject = GetSelectedObject();
		if(selectedObject !=null){
			infoSheet.SetActive(true);
			infoNameText.text = selectedObject.GetComponent<UnitScript>().charName;
			infoClassText.text = selectedObject.GetComponent<UnitScript>().className;
			infoHealthText.text = selectedObject.GetComponent<UnitScript>().health.ToString()+"/"+selectedObject.GetComponent<UnitScript>().maxHealth.ToString();
		}else{
			infoSheet.SetActive(false);
		}
	}
	
}
