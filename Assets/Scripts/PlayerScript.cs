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
	private int tilesMoved = 0; // variable to check how many tiles player has moved while holding button
	private float slowTimeToWait = (float) 0.15; // time player must wait after moving
	private float fastTimeToWait = (float) 0.05; // time player must wait after moving if spacebar is held

	
	//player menu data members
	public GameObject playerMenu;
	public bool menuOpen;
	public bool menuAction;
	public bool moveAction;
	public bool attackTarget;
	public bool postMove;
	
	//player menu buttons
	public Button moveButton;
	public Button attackButton; 
	public Button inspectButton;
	public Button itemButton;
	public Button waitButton;
	public Button endTurnButton;
	
	//inspect menu data members;
	public GameObject inspectSheet;
	public bool inspectSheetOpen;
	
	//location of the levelUpSheet
	public Vector3 levelUpSheetLocation;
	
	//inspect sheet / level up sheet text elements
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
	
	// map zooming data
	public int zIndex = 0;
	public float[] zoomLevels = {5,3,5,7};
	
	// bools to prevent zooming and unit cycling while holding down buttons
	private bool mapbool;
	private bool cyclebool;
	
	// temporary Lists and GameObjects for use throughout script
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
	
	// reference to the Level Manager
	public LevelManagerScript lmInstance;
	
	
	// initialization
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
		
		//set Lists and Objects to null at start
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
		
		//check if cursor starts on unit
		CheckInfoSheet();
	}
	
	// Update is called once per frame
	void Update () {
		
		// do not run update in non-player phases
		if (lmInstance.phaseCounter != 0){
			return;
		}
		
		// do not run update if a combat is playing out
		if(combatEngaged){
			return;
		}
		
		// if a zoom button is pressed, and mapbool is false
		//		change the zoom level and set mapbool to true
		if (Input.GetAxisRaw("MapZoom")>0) {
			if (!mapbool) {
				mapbool = true;
				zIndex += 1;
				if (zIndex >= zoomLevels.GetLength(0)) { zIndex = 0; }
			}
		} else {
			// set mapbool to false if a zoom button is not pressed
			mapbool = false;
		}		
		
		
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
	
		
		

		// if a Cancel button is pressed
		if (Input.GetButtonDown("Cancel")) {
			
			EventSystem.current.SetSelectedGameObject(null);
			
			// if a menu is open, play the MenuCancel sound effect
			if((menuOpen||moveAction||attackTarget||postMove||itemTargeting)){
				SoundManager.instance.MenuCancel();
			}
			
			// pressed cancel after choosing an item, while choosing a target
			// returns to choosing an item menu
			if (itemTargeting){
				
				//move cursor back to unit
				UnitFocus(currentUnit);
				
				// if the unit had moved previously
				//		move the unit back to its initial position
				//		highlight unit's range tiles
				//		return unit to its post-move position
				if(currentUnit.GetComponent<UnitScript>().tilePrev != null){
					location[0] = (int) currentUnit.GetComponent<UnitScript>().tilePrev.transform.position.x;
					location[1] = (int) currentUnit.GetComponent<UnitScript>().tilePrev.transform.position.y;
					currentUnit.GetComponent<UnitScript>().MoveUnit(currentUnit.GetComponent<UnitScript>().tilePrev, true);
					Selection();
					location[0] = (int) currentUnit.GetComponent<UnitScript>().tilePrev.transform.position.x;
					location[1] = (int) currentUnit.GetComponent<UnitScript>().tilePrev.transform.position.y;
					currentUnit.GetComponent<UnitScript>().MoveUnit(currentUnit.GetComponent<UnitScript>().tilePrev, true);
				}else{
					// highlight unit's range tiles
					Selection();
				}
				
				
				
				itemMenu.SetActive(true);
				itemMenuOpen = true;
				
				itemTargeting = false;
				menuOpen = true;
				
			}
			
			// pressed cancel when choosing a target to attack
			// return to playermenu
			else if (attackTarget) {
				// move cursor back to unit
				UnitFocus(currentUnit);
				
				// if the unit had moved previously
				//		move the unit back to its initial position
				//		highlight unit's range tiles
				//		return unit to its post-move position
				if(currentUnit.GetComponent<UnitScript>().tilePrev != null){
					location[0] = (int) currentUnit.GetComponent<UnitScript>().tilePrev.transform.position.x;
					location[1] = (int) currentUnit.GetComponent<UnitScript>().tilePrev.transform.position.y;
					currentUnit.GetComponent<UnitScript>().MoveUnit(currentUnit.GetComponent<UnitScript>().tilePrev, true);
					Selection();
					location[0] = (int) currentUnit.GetComponent<UnitScript>().tilePrev.transform.position.x;
					location[1] = (int) currentUnit.GetComponent<UnitScript>().tilePrev.transform.position.y;
					currentUnit.GetComponent<UnitScript>().MoveUnit(currentUnit.GetComponent<UnitScript>().tilePrev, true);
				}else{
					// highlight unit's range tiles
					Selection();
				}
				
				attackTarget = false;
				playerMenu.SetActive(true);
				attackButton.Select();
				menuOpen = true;
				
			}
			
			// pressed cancel while inspectSheet is open
			// close inspect sheet
			else if(inspectSheetOpen){
				inspectSheetOpen = false;
				inspectSheet.SetActive(false);
				inspectSheet.transform.position = levelUpSheetLocation;
				
				
				if (currentUnit.tag=="playerUnit") {
					CheckIfItems();
					playerMenu.SetActive(true);
					inspectButton.Select();
				} else {
					menuOpen = false;
				}
			}
			
			// pressed cancel while item menu is open
			// close item menu
			else if(itemMenuOpen){
				itemMenuOpen = false;
				itemMenu.SetActive(false);
				playerMenu.SetActive(true);
				itemButton.Select();
				
			}
			
			// pressed cancel while in primary unit menu
			// close menu, return to tile selection
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
			
			
			// pressed cancel while selecting move destination
			// return to playermenu
			else if(moveAction){
				
				//move cursor back to unit
				UnitFocus(currentUnit);
				//open original menu - reenable move button
				moveButton.interactable = true;
				moveAction = false;
				menuOpen = true;
				CheckIfItems();
				playerMenu.SetActive(true);
				moveButton.Select();
				
			}
			
			// pressed cancel while in post-move menu
			// returns to move selection state
			else if(postMove){
			
				playerMenu.SetActive(false);
				
				postMove = false;
				moveAction = true;
				menuAction = true;
				menuOpen = false;
				
				// return unit to its previous position
				currentUnit.GetComponent<UnitScript>().MoveBack();
					
			}
			
		}
		
		//timetowait is decremented each frame
		if (timeToWait > 0) {
			timeToWait -= Time.deltaTime;
		}
		
		// if no movement buttons are held, reset tilesMoved to 0
		if ((int) Input.GetAxisRaw("Horizontal") == 0 && (int) Input.GetAxisRaw("Vertical") == 0) {
			tilesMoved = 0;
		}
		
		//if the item menu is open, listen for button inputs, do relevant action
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
				// set menu text to item's name and description
				itemNameText.text = currentUnit.GetComponent<UnitScript>().CurrentItem().itemName;
				itemDescriptionText.text = currentUnit.GetComponent<UnitScript>().CurrentItem().description;
			
				// set timeToWait to prevent fast menu cycling
				timeToWait = slowTimeToWait;
			}
			
			// if input is Submit, choose a tile to target
			if (Input.GetButtonDown("Submit")) {
				SoundManager.instance.MenuSelect();
				itemMenuOpen = false;
				itemTargeting = true;
				itemMenu.SetActive(false);
				
				menuOpen = false;
				menuAction = true;
				
				// clean up tile highlights
				GameObject tempUnit;
				tempUnit = currentUnit;
				BigCleanUp();
				currentUnit = tempUnit;
				
				// set new highlights for item radius
				currentUnit.GetComponent<UnitScript>().CurrentItem().validTiles = new List<List<int>>();
				currentUnit.GetComponent<UnitScript>().CurrentItem().FindValidTiles(location[0],location[1],0);
				HighlightRadius();
			}
			
		}
		
		
		// if the menu is open, cursor actions are unavailable
		// for 1 frame after menu closes, cursor will be locked so that the Submit-press from menu doesn't select unit
		if(menuOpen == false && menuAction == false){
			
			//cycle through units
			if (Input.GetAxisRaw("UnitCycle")<0) {
				if (!cyclebool){
					
					if(itemTargeting){
						lmInstance.unitListIndex++;
						// if the end of the unitList is hit, loop back to the start of the list
						if (lmInstance.unitListIndex >= lmInstance.unitList.Count) {
							lmInstance.unitListIndex = 0;
						}
						// focus on unit and set item range highlight around cursor
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
						// if the end of the unitList is hit, loop back to the start of the list
						if (lmInstance.unitListInRangeIndex >= lmInstance.unitListInRange.Count) {
							lmInstance.unitListInRangeIndex = 0;
						}
						// focus on unit
						UnitFocus(lmInstance.unitListInRange[lmInstance.unitListInRangeIndex]);
					} else{
						// if not in item targeting or attack targeting
						//		cycle to next player unit that has not finished
						do {
							lmInstance.unitListIndex++;
							// if the end of the unitList is hit, loop back to the start of the list
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
						// if the start of the unitList is hit, loop back to the end
						if (lmInstance.unitListIndex <0) {
							lmInstance.unitListIndex = lmInstance.unitList.Count-1;
						}
						// focus on unit and set item range highlight around cursor
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
						// if the start of the unitList is hit, loop back to the end
						if (lmInstance.unitListInRangeIndex < 0) {
							lmInstance.unitListInRangeIndex = lmInstance.unitListInRange.Count-1;
						}
						// focus on unit
						UnitFocus(lmInstance.unitListInRange[lmInstance.unitListInRangeIndex]);
					} else{
						// if not in item targeting or attack targeting
						//		cycle to next player unit that has not finished
						do {
							lmInstance.unitListIndex--;
							// if the start of the unitList is hit, loop back to the end
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
				// if no cycle button is held, set cyclebool to false
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
			
			// MultSelect is pressed to highlight a units range, but not select the unit
			if ( (Input.GetAxisRaw("MultSelect")>0 && Input.GetButtonDown("Submit")) || (Input.GetButtonDown("Submit") && Input.GetAxisRaw("MultSelect")>0) ) {
				Selection();
			} else if ( Input.GetButtonDown("Submit") ){
				// Submit is pressed without MultSelect
				
				// reset the input axes to prevent a menu button from being pressed immediately
				Input.ResetInputAxes();
				
				GameObject selectedObject = GetSelectedObject();
				EventSystem.current.SetSelectedGameObject(null);
				
				// if Submit is pressed in the moveAction state
				//		begin the unit's movement code
				if (currentUnit != null && selectedObject == null && moveAction == true) {
					//check unit's available tiles by iterating to see if selected tile is valid for movement
					foreach (List<int> tileLocation in currentUnit.GetComponent<UnitScript>().availableTiles) {
						if (location[0]==tileLocation[0] && location[1]==tileLocation[1]) {
							currentUnit.GetComponent<UnitScript>().FindPath(new int[] {location[0],location[1]} );
							
							//call movement method from unit
							currentUnit.GetComponent<UnitScript>().MoveUnit(lmInstance.tileArray[location[0],location[1]]);
							
							//open post-move menu
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
					//case that move selection is taking place, and player selected a non-empty tile
					//doing nothing
					
				} else if (itemTargeting){
					// Submit is pressed while choosing a target for an item
					// iterate through item's tiles in range to make sure a valid tile is chosen
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
					// Submit is pressed while choosing a target to attack
					// check that the chosen tile has a unit
					if (selectedObject!=null) {
						// check that the chosen unit is in range
						foreach (GameObject unit in lmInstance.unitListInRange) {
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
					// Submit is pressed in the default state while selecting an enemy
					// Inspect the enemy
					InspectEnemy();
					SoundManager.instance.MenuSelect();
					
				} else if (selectedObject!= null && selectedObject.tag == "playerUnit") {
					// Submit is pressed in the default state while selecting a player unit
					SoundManager.instance.MenuSelect();
					
					prevSelectedButton = moveButton.gameObject;
					//check if the unit has already acted, allow to inspect only
					if (selectedObject.GetComponent<UnitScript>().finished) {
						FreshSelect();
						attackButton.interactable = false;
						moveButton.interactable = false;
						itemButton.interactable = false;
						waitButton.interactable = false;
						
						playerMenu.SetActive(true);
						inspectButton.Select();
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
					//in the case unit(s) have been selected with MultSelect, and a blank tile is selected, highlights will disappear
					BigCleanUp();
				}
			}
		} else if ( menuAction == true ) {
			// after one frame has passed, set menuAction to false
			menuAction = false;
		}
		
	}
	
	// Movement is called in update, when cursor movement occurs and timeToWait is 0
	// changes the location of the cursor based on the direction(s) held
	// updates timeToWait
	void Movement () {
		
		int horizontal = 0;
		int vertical = 0;
		
		horizontal = (int) Input.GetAxisRaw("Horizontal");
		vertical = (int) Input.GetAxisRaw("Vertical");
		
		// determine the position the cursor would go to
		int tempHorizontal = location[0] + horizontal;
		int tempVertical = location[1] + vertical;
		
		// determine if cursor would move off of the map
		if (tempHorizontal < 0 || tempHorizontal >= transform.parent.GetComponent<LevelManagerScript>().tileArrayLength[0]) {
			tempHorizontal = location[0];
		}
		
		if (tempVertical < 0 || tempVertical >= transform.parent.GetComponent<LevelManagerScript>().tileArrayLength[1]) {
			tempVertical = location[1];
		}
		
		if(!((tempHorizontal == location[0])&&(tempVertical == location[1]))){
			SoundManager.instance.MoveCursor();
		}
		
		// set cursor on new position
		location[0] = tempHorizontal;
		location[1] = tempVertical;

		transform.position = transform.parent.GetComponent<LevelManagerScript>().tileArray[location[0],location[1]].transform.position;

		// set the active tile to the cursor's new tile
		transform.parent.GetComponent<LevelManagerScript>().activeTile = transform.parent.GetComponent<LevelManagerScript>().tileArray[location[0],location[1]];
		
		// increase tilesMoved
		tilesMoved++;
		if (horizontal != 0 || vertical != 0) {
			// if FastMove is held, or tilesMoved has reached a threshold
			//		set timeToWait to fastTimeToWait
			if (Input.GetButton("FastMove") || tilesMoved > 5) {
				timeToWait = fastTimeToWait;
			} else {
				// set timeToWait to slowTimeToWait
				timeToWait = slowTimeToWait;
			}
		}
	}
	
	
	// GetSelectedObject is called when the object on a tile is needed
	// returns the occupyingObject on the current tile
	GameObject GetSelectedObject() {
		GameObject selectedObject = transform.parent.GetComponent<LevelManagerScript>().tileArray[location[0],location[1]].GetComponent<TileScript>().occupyingObject;
		return selectedObject;
	}
	
	// FreshSelect is called when selecting a unit without MultSelect
	// cleans up the tile highlights currently on the level
	// creates new highlights around the unit
	void FreshSelect() {
		GameObject selectedObject = GetSelectedObject();
		// if the selected object is already selected, then deselect it
		if (selectedObject == null || selectedObject.GetComponent<UnitScript>().selectedFlag) {
			BigCleanUp();
		} else  {
			BigCleanUp();
			Selection();
		}
	}
	
	// Selection is called when Submit is pressed
	// selects a tile, and whatever is on it
	void Selection() {
		GameObject selectedObject = GetSelectedObject();
		if( selectedObject != null) {
			// if the selected unit is already selected, clean up the tile highlights
			if ( (selectedObject.GetComponent<UnitScript>().tag == "playerUnit" || selectedObject.GetComponent<UnitScript>().tag == "enemyUnit") && selectedObject.GetComponent<UnitScript>().selectedFlag) {
				CleanUp(selectedObjectList.IndexOf(selectedObject));	
			} 
			// if the selected unit is not already selected, add the unit to the selected objects list
			else if( (selectedObject.GetComponent<UnitScript>().tag == "playerUnit" || selectedObject.GetComponent<UnitScript>().tag == "enemyUnit") && !selectedObject.GetComponent<UnitScript>().selectedFlag) {
				currentUnit = selectedObject;
				selectedObject.GetComponent<UnitScript>().selectedFlag = true;
				selectedObject.GetComponent<UnitScript>().FindAvailableMoves(location[0],location[1],0);
				selectedObject.GetComponent<UnitScript>().FindAvailableAttacks(location[0],location[1],0);
				
				selectedObjectList.Add(selectedObject);
			} 
			
			// destroy the tile highlights
			TileCleanUp();
			
			// iterate through all the units in selectedObjectList
			// create move highlights in their movement ranges
			foreach (GameObject unit in selectedObjectList) {
				List<GameObject> moves = new List<GameObject>();
				foreach (List<int> subList in unit.GetComponent<UnitScript>().availableHLTiles) {
					GameObject highlightInstance = Instantiate(tileHighlight, transform.parent.GetComponent<LevelManagerScript>().tileArray[subList[0],subList[1]].transform.position, Quaternion.identity) as GameObject;
					// set the alpha of the highlight based on distance from the unit
					Color tmp = highlightInstance.GetComponent<SpriteRenderer>().color;
					float dist = (float) subList[2];
					tmp.a = (float) 0.8 - (dist/4);
					if (tmp.a<0.4f) {tmp.a=0.4f;}
					highlightInstance.GetComponent<SpriteRenderer>().color = tmp;
					
					moves.Add(highlightInstance);
				}
				// add the highlights to the highlightsList
				highlightsList.Add(moves);
			}
			
			// iterate through all the units in selectedObjectList
			// create attack highlights in their attack ranges
			foreach (GameObject unit in selectedObjectList) {
				List<GameObject> attacks = new List<GameObject>();
				foreach (List<int> subList in unit.GetComponent<UnitScript>().availableAttacks) {
					bool hlexists = false;
					
					// if a move highlight is already in this location, do not make a highlight here
					foreach(List<GameObject> sl in highlightsList){
						foreach(GameObject hl in sl){
							if((hl.transform.position.x == subList[0]) && (hl.transform.position.y == subList[1])){
								hlexists = true;
								break;
							}
						}
						if (hlexists){break;}
					}
					
					// if an attack highlight is already in this location, do not make a highlight here
					foreach(List<GameObject> sl in attackHighlightsList){
						foreach(GameObject hl in sl){
							if((hl.transform.position.x == subList[0]) && (hl.transform.position.y == subList[1])){
								hlexists = true;
								break;
							}
						}
						if (hlexists){break;}
					}
					
					// if no highlight already exists here, make an attack highlight here
					if(!hlexists){
						GameObject highlightInstance = Instantiate(attackHighlight, transform.parent.GetComponent<LevelManagerScript>().tileArray[subList[0],subList[1]].transform.position, Quaternion.identity) as GameObject;
						Color tmp = highlightInstance.GetComponent<SpriteRenderer>().color;
				
						tmp.a = (float) 0.5;
						highlightInstance.GetComponent<SpriteRenderer>().color = tmp;
						
						attacks.Add(highlightInstance);
					}
				}	
				// add the highlights to the attackHighlightsList
				attackHighlightsList.Add(attacks);
			}
		}
	}
	
	// HighlightRadius is called when choosing a target location for an item
	// makes a highlight around the cursor with radius equal to item radius
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
		// create highlights on every tile in radius
		foreach (List<int> subList in currentUnit.GetComponent<UnitScript>().CurrentItem().tilesInRadius) {
			
			GameObject highlightInstance = Instantiate(tempHighlight, transform.parent.GetComponent<LevelManagerScript>().tileArray[subList[0],subList[1]].transform.position, Quaternion.identity) as GameObject;
			Color tmp = highlightInstance.GetComponent<SpriteRenderer>().color;
			tmp.a = (float) 0.5;
			highlightInstance.GetComponent<SpriteRenderer>().color = tmp;
			
			targetedTiles.Add(highlightInstance);
		}
		targetHighlightsList = targetedTiles;
	}
	
	// BigCleanUp runs clean up on every selected object
	// removes all tile highlights
	void BigCleanUp() {
		while (selectedObjectList.Count != 0) {
			CleanUp(0);
		}
		
		foreach(GameObject hl in targetHighlightsList) {
			Destroy(hl);
		}
		targetHighlightsList = new List<GameObject>();
	}
	
	// CleanUp destroys the highlights associated with the relevant selected object
	// (int) index is the index associated with the selected object in selectedObjectList, highlightsList, and attackHighlightsList
	void CleanUp(int index) {
		
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
	
	// TileCleanUp destroys every tile highlight and clears all the highlight lists
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
	
	
	// AttackHandle is the handler on the attack button in the player menu
	// changes the state to attackTarget
	// clears all the tile highlights and sets new highlights in the current unit's attack range
	void AttackHandle(){
		
		playerMenu.SetActive(false);
		menuOpen = false;
		menuAction = true;
		
		// determine which tiles unit can currently attack
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
		lmInstance.unitListInRange.Remove(currentUnit);
		
		attackTarget = true;
		
		SoundManager.instance.MenuSelect();
		
	}
	
	// MoveHandle is the handler on the move button in the player menu
	// changes the state to moveAction
	void MoveHandle() {
		
		moveAction = true;
		playerMenu.SetActive(false);
		menuOpen = false;
		menuAction = true;
		
		SoundManager.instance.MenuSelect();
	}
	
	// InspectHandle is the handler on the inspect button in the player menu
	// opens the inspect sheet and populates with unit info
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
	
	// ItemHandle is the handler on the item button in the player menu
	// opens the item selection menu
	void ItemHandle() {
		// launch item window
		itemMenuOpen = true;
		prevItemIndex = currentUnit.GetComponent<UnitScript>().itemIndex;
		
		itemNameText.text = currentUnit.GetComponent<UnitScript>().CurrentItem().itemName;
		itemDescriptionText.text = currentUnit.GetComponent<UnitScript>().CurrentItem().description;
		itemMenu.SetActive(true);
		// close player menu
		playerMenu.SetActive(false);
		
		menuAction = true;
		
		SoundManager.instance.MenuSelect();
	}
	
	// WaitHandle is the handler on the wait button in the player menu
	// ends the current unit's action
	// changes state to default
	void WaitHandle() {
		
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
	
	// EndTurnHandle is the handler on the end turn button in the player menu
	// runs FinishAction on every remaining player unit
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
	
	// InspectEnemy runs when you press Submit on an enemy unit in the default state
	// sets the enemy to the currentUnit
	// runs InspectHandle to open the inspect sheet with the enemy's stats
	void InspectEnemy() {
		currentUnit = GetSelectedObject();
		InspectHandle();
		menuOpen = true;
	}
	
	// CheckIfItems runs when the player menu opens
	// checks if the unit can use items this turn
	// sets the itemButton to non-interactable if the unit can not use items
	// sets the itemButton to interactable otherwise
	void CheckIfItems() {
		if (currentUnit.GetComponent<UnitScript>().itemList.Count == 0 || currentUnit.GetComponent<UnitScript>().finished) { itemButton.interactable = false; }
		else { itemButton.interactable = true; }
	}
	
	// UnitFocus sets the cursor position to a given unit
	// (GameObject) unit is the unit to move the cursor to
	public void UnitFocus(GameObject unit) {
		transform.position = unit.transform.position;
		location[0] = (int) unit.GetComponent<UnitScript>().tileCur.transform.position.x;
		location[1] = (int) unit.GetComponent<UnitScript>().tileCur.transform.position.y;
		transform.parent.GetComponent<LevelManagerScript>().activeTile = transform.parent.GetComponent<LevelManagerScript>().tileArray[location[0],location[1]];
	}
	
	// CheckInfoSheet runs when the cursor moves onto a tile
	// checks if a unit is on the tile
	// if so, opens the info sheet with relevant information
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
