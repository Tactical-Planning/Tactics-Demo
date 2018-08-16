using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PartyManagementScript : MonoBehaviour {
	
	// reference to GameManager
	GameManager gameManager;
	
	public GameObject emptyItemPrefab;
	public GameObject equipmentButtonPrefab;
	
	// indexes
	public int partyUnitIndex;
	public int targetUnitIndex;
	public int itemListIndex;
	public int targetItemListIndex;
	
	// lists for Buttons
	public List<Button> itemButtonList;
	public List<Button> targetItemButtonList;
	public List<Button> unitButtonList;
	
	// UI elements
	public GameObject unitButtonContainer;
	public GameObject leftSheet;
	public GameObject rightSheet;
	
	//buttons along bottom of screen for navigating to other scenes
	public GameObject proceedContainer;
	public Button saveButton;
	public Button proceedButton;
	public Button quitButton;
	
	//confirm prompt buttons for quitting to main menu
	public GameObject quitConfirmationContainer;
	public Button quitConfirmButton;
	public Button quitDenyButton;
	
	//confirm prompt buttons for proceeding to next combat sequence
	public GameObject proceedConfirmationContainer;
	public Button confirmButton;
	public Button denyButton;
	
	//text elements for character sheet
	public GameObject infoContainer;
	public Text characterNameText;
	public Text levelText;
	public Text classText;
	public Text healthValueText;
	public Text agilityValueText;
	public Text attackValueText;
	public Text defenseValueText;
	public Text speedValueText;
	public Text rangeValueText;
	
	//options displayed when selecting a unit
	public GameObject buttonContainer;
	public Button swapItemsButton;
	public Button equipButton;
	
	public GameObject itemContainer;
	public GameObject targetItemContainer;
	
	public GameObject equipContainer;
	public GameObject equipmentListContainer;
	public GameObject equipmentScrollbar;
	
	// references to current object being selected
	public GameObject prevSelectedButton;
	
	//data members for keeping track of who is trading with who, what inventory should be displaying, etc
	public GameObject currentSelectedUnit;
	public UnitScript currentUnitScript;
	public GameObject currentSelectedItem;
	
	public GameObject itemToolTip;
	
	
	public GameObject currentSelectedTargetUnit;
	public UnitScript currentTargetUnitScript;
	public GameObject currentSelectedTargetItem;
	public GameObject currentSelectedEquip;
	public GameObject currentSelectedTargetEquip;
	public EquipmentScript preEquip;
	
	public GameObject targetItemToolTip;
	
	public GameObject equipToolTip;
	public GameObject targetEquipToolTip;
	
	// flags to refer to current state
	public bool choosingUnit;
	public bool buttonContainerOpen;
	public bool choosingItem;
	public bool choosingSlot;
	public bool choosingEquip;
	public bool choosingTargetUnit;
	public bool choosingTargetItem;
	public bool proceedBool;
	public bool quitBool;
	
	public int numItemSlots;
	public float numButtons;
	// handler
	public delegate void MyHandler();
	
	// Use this for initialization
	void Start () {
	
		//add listeners to buttons in scene
		swapItemsButton.onClick.AddListener(SwapItemsHandle);
		equipButton.onClick.AddListener(SwapEquipHandle);
		for(int i = 0; i<4; i++) {
			equipContainer.transform.GetChild(i).GetComponent<Button>().onClick.AddListener(EquipHandle);
		}
		equipmentListContainer.transform.GetChild(0).GetChild(0).GetComponent<Button>().onClick.AddListener(TargetEquipHandle);
		equipmentScrollbar.GetComponent<Scrollbar>().interactable = false;
		
		//get refence to gameManager and assign to dataMember for more concise use
		gameManager = GameManager.instance.GetComponent<GameManager>();
		gameManager.Load("/playerInfo.dat");
	
		numItemSlots = 5;
		for(int i=0; i<numItemSlots; i++) {
			GameObject tempButton = itemContainer.transform.GetChild(i).gameObject;
			tempButton.GetComponent<Button>().onClick.AddListener(ItemHandle);
			
			tempButton = targetItemContainer.transform.GetChild(i).gameObject;
			tempButton.GetComponent<Button>().onClick.AddListener(TargetItemHandle);
		}
		
		//initialize all indeces to 0
		//these are used to maintain a reference to what unit/item is being referenced
		partyUnitIndex = 0;
		targetUnitIndex = 0;
		itemListIndex = 0;
		targetItemListIndex = 0;
		
		//iterate through party units to populate unit buttons
		foreach(GameObject unit in gameManager.partyUnits) {
			//add an empty slot to the end of every unit's item list
			GameObject tempEmpty = Instantiate(emptyItemPrefab) as GameObject;
			tempEmpty.SetActive(false);
			unit.GetComponent<UnitScript>().itemList.Add(tempEmpty);
			
			//set UI fields for unit button
			GameObject tempButton = unitButtonContainer.transform.GetChild(partyUnitIndex).gameObject;
			tempButton.transform.GetChild(0).GetComponent<Text>().text = unit.GetComponent<UnitScript>().charName;
			tempButton.transform.GetChild(1).GetComponent<Image>().sprite = unit.GetComponent<SpriteRenderer>().sprite;

			tempButton.SetActive(true);
			tempButton.GetComponent<Button>().onClick.AddListener(UnitHandle);
			unitButtonList.Add(tempButton.GetComponent<Button>());
			partyUnitIndex++;
		}
		
		partyUnitIndex = 0;
		//start scene with first unit in list selected
		unitButtonContainer.transform.GetChild(partyUnitIndex).gameObject.GetComponent<Button>().Select();
		prevSelectedButton = unitButtonContainer.transform.GetChild(partyUnitIndex).gameObject;
		
		//add scene navigation listeners
		saveButton.onClick.AddListener(SaveHandle);
		proceedButton.onClick.AddListener(ProceedHandle);
		quitButton.onClick.AddListener(QuitToMenuHandle);
		confirmButton.onClick.AddListener(ConfirmationHandle);
		denyButton.onClick.AddListener(CancelHandle);
		quitConfirmButton.onClick.AddListener(QuitConfirmationHandle);
		quitDenyButton.onClick.AddListener(QuitCancelHandle);
		
		//set maintained references to current objects to null
		currentSelectedUnit = null;
		currentSelectedTargetUnit = null;
		currentSelectedItem = null;
		currentSelectedTargetItem = null;
		currentSelectedEquip = null;
		// set flags
		choosingUnit = true;
		buttonContainerOpen = false;
		choosingItem = false;
		choosingSlot = false;
		choosingEquip = false;
		choosingTargetUnit = false;
		choosingTargetItem = false;
		proceedBool = false;
		quitBool = false;
		
	}
	
	// Update is called once per frame
	void Update () {
		//if a new button is highlighted, play the move cursor sound
		if (prevSelectedButton != EventSystem.current.currentSelectedGameObject) {
			SoundManager.instance.MoveCursor();
			prevSelectedButton = EventSystem.current.currentSelectedGameObject;
		}
		
		//check scene state
		//depending on the state, if a new button is selected, call a method which will update text fields, move UI elements, etc
		if (choosingUnit && EventSystem.current.currentSelectedGameObject != currentSelectedUnit && EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.transform.parent != proceedContainer.transform) {
			currentSelectedUnit = EventSystem.current.currentSelectedGameObject;
			UnitOnSelect();
		}
		if (choosingItem && EventSystem.current.currentSelectedGameObject != currentSelectedItem && EventSystem.current.currentSelectedGameObject != null){
			currentSelectedItem = EventSystem.current.currentSelectedGameObject;
			ItemOnSelect();
		}
		if (choosingTargetUnit && EventSystem.current.currentSelectedGameObject != currentSelectedTargetUnit && EventSystem.current.currentSelectedGameObject != null ) {
			currentSelectedTargetUnit = EventSystem.current.currentSelectedGameObject;
			TargetUnitOnSelect();
		}
		if (choosingTargetItem && EventSystem.current.currentSelectedGameObject != currentSelectedTargetItem && EventSystem.current.currentSelectedGameObject != null){
			currentSelectedTargetItem = EventSystem.current.currentSelectedGameObject;
			TargetItemOnSelect();
		}
		if(choosingSlot && EventSystem.current.currentSelectedGameObject != currentSelectedEquip && EventSystem.current.currentSelectedGameObject != null){
			currentSelectedEquip = EventSystem.current.currentSelectedGameObject;
			EquipOnSelect();
		}
		if(choosingEquip && EventSystem.current.currentSelectedGameObject != currentSelectedTargetEquip && EventSystem.current.currentSelectedGameObject != null){
			currentSelectedTargetEquip = EventSystem.current.currentSelectedGameObject;
			TargetEquipOnSelect();
		}
		
		//behavior for cancel input in different states
		//mostly involves setting state flags and activating and deactivating different UI windows
		if (Input.GetButtonDown("Cancel")) {
			if (!choosingUnit) {
				SoundManager.instance.MenuCancel();
			}
			if (buttonContainerOpen) {
				buttonContainerOpen = false;
				choosingUnit = true;
				infoContainer.SetActive(true);
				proceedContainer.SetActive(true);
				buttonContainer.SetActive(false);
				ReengageUnitButtons(UnitHandle);
				
			}else if(choosingItem){
				choosingItem = false;
				buttonContainerOpen = true;
				itemContainer.SetActive(false);
				buttonContainer.SetActive(true);
				swapItemsButton.Select();
				prevSelectedButton = swapItemsButton.gameObject;
				
			}else if(choosingTargetUnit) {
				choosingTargetUnit = false;
				choosingItem = true;
				DisengageButtons(unitButtonList);
				ReengageItemButtons(itemButtonList);
				itemButtonList[itemListIndex].Select();
				prevSelectedButton = itemButtonList[itemListIndex].gameObject;
				targetItemContainer.SetActive(false);
				rightSheet.SetActive(false);
				currentSelectedItem = null;
				
			}else if(choosingTargetItem) {
				choosingTargetItem = false;
				choosingTargetUnit = true;
				ReengageUnitButtons(TargetUnitHandle);
				DisengageButtons(targetItemButtonList);
				
				unitButtonList[partyUnitIndex].interactable = false;
				unitButtonList[targetUnitIndex].Select();
				prevSelectedButton = unitButtonList[targetUnitIndex].gameObject;
				targetItemToolTip.SetActive(false);
				currentSelectedTargetItem = null;
			}else if(proceedBool) {
				CancelHandle();
			}else if(quitBool) {
				QuitCancelHandle();
			}else if(choosingSlot){
				choosingSlot = false;
				buttonContainerOpen = true;
				equipContainer.SetActive(false);
				buttonContainer.SetActive(true);
				equipButton.Select();
				prevSelectedButton = equipButton.gameObject;
			}else if(choosingEquip){
				currentUnitScript.Unequip(gameManager.equipDict[currentUnitScript.equipmentList[currentSelectedEquip.transform.GetSiblingIndex()]].GetComponent<EquipmentScript>(),currentSelectedEquip.transform.GetSiblingIndex());
				currentUnitScript.Equip(preEquip,currentSelectedEquip.transform.GetSiblingIndex());
				choosingEquip = false;
				choosingSlot = true;
				//rather than delete buttons, they are set inactive and can be activated and populated when needed later
				for(int i = 1; i<equipmentListContainer.transform.GetChild(0).childCount; i++){
					equipmentListContainer.transform.GetChild(0).GetChild(i).gameObject.SetActive(false);
					}
				
				rightSheet.SetActive(false);
				infoContainer.SetActive(false);
				equipmentListContainer.SetActive(false);
				equipmentScrollbar.SetActive(false);
				targetEquipToolTip.SetActive(false);
				equipContainer.SetActive(true);
				currentSelectedEquip.GetComponent<Button>().Select();
				prevSelectedButton = currentSelectedEquip;
				currentSelectedEquip = null;
				currentSelectedTargetEquip = null;

			}
			
		}
	}
	
	
	// when selecting a unit button, update the left sheet
	void UnitOnSelect() {
		currentUnitScript = gameManager.partyUnits[unitButtonList.IndexOf(EventSystem.current.currentSelectedGameObject.GetComponent<Button>())].GetComponent<UnitScript>();
		
		characterNameText.text = currentUnitScript.charName;
		levelText.text = currentUnitScript.level.ToString();
		classText.text = currentUnitScript.className;
		
		healthValueText.text = currentUnitScript.maxHealth.ToString();
		agilityValueText.text = currentUnitScript.agility.ToString();
		attackValueText.text = currentUnitScript.attack.ToString();
		defenseValueText.text = currentUnitScript.defense.ToString();
		speedValueText.text = currentUnitScript.speed.ToString();
		rangeValueText.text = currentUnitScript.range.ToString();
		
		infoContainer.SetActive(true);
		
		partyUnitIndex = unitButtonList.IndexOf(EventSystem.current.currentSelectedGameObject.GetComponent<Button>());
	}
	
	// when selecting a target unit button, update the right sheet
	void TargetUnitOnSelect() {
		currentTargetUnitScript = gameManager.partyUnits[unitButtonList.IndexOf(EventSystem.current.currentSelectedGameObject.GetComponent<Button>())].GetComponent<UnitScript>();
		
		//reset itemButtons
		currentSelectedTargetItem = null;
		
		targetItemListIndex = 0;
			
		foreach(Button button in targetItemButtonList) {
			button.gameObject.SetActive(false);
		}			
		targetItemButtonList = new List<Button>();
		
		//set new itemButtons
		for(int i =0; i < numItemSlots; i++) {
			GameObject tempButton = targetItemContainer.transform.GetChild(i).gameObject;
			tempButton.transform.GetChild(0).GetComponent<Text>().text = currentTargetUnitScript.itemList[i].GetComponent<ItemScript>().itemName;
			tempButton.SetActive(true);
			tempButton.GetComponent<Button>().interactable = false;
			targetItemButtonList.Add(tempButton.GetComponent<Button>());
			if (currentTargetUnitScript.itemList[i].GetComponent<ItemScript>().itemName == "Empty Slot"){
				break;
			}
		}
		//display the highlighted unit's items
		targetItemContainer.SetActive(true);
		targetUnitIndex = unitButtonList.IndexOf(EventSystem.current.currentSelectedGameObject.GetComponent<Button>());
	}
	
	//when an item is highlighted, a tool tip display info on item
	void ItemOnSelect(){
		
		Vector3 location = new Vector3( itemToolTip.transform.position.x, currentSelectedItem.transform.position.y, itemToolTip.transform.position.z);

		itemToolTip.transform.position = location;
		
		itemListIndex = itemButtonList.IndexOf(currentSelectedItem.GetComponent<Button>());
		itemToolTip.transform.GetChild(2).GetComponent<Text>().text = currentUnitScript.itemList[itemListIndex].GetComponent<ItemScript>().description;
		itemToolTip.transform.GetChild(3).GetComponent<Text>().text = currentUnitScript.itemList[itemListIndex].GetComponent<ItemScript>().range.ToString();
		itemToolTip.transform.GetChild(4).GetComponent<Text>().text = currentUnitScript.itemList[itemListIndex].GetComponent<ItemScript>().aoeRadius.ToString();
		itemToolTip.SetActive(true);
	}
	
	//when a target item is highlighted, a tool tip display info on item
	void TargetItemOnSelect(){

		Vector3 location = new Vector3( targetItemToolTip.transform.position.x, currentSelectedTargetItem.transform.position.y, targetItemToolTip.transform.position.z);
		
		targetItemToolTip.transform.position = location;
		
		targetItemListIndex = targetItemButtonList.IndexOf(currentSelectedTargetItem.GetComponent<Button>());
		targetItemToolTip.transform.GetChild(2).GetComponent<Text>().text = currentTargetUnitScript.itemList[targetItemListIndex].GetComponent<ItemScript>().description;
		targetItemToolTip.transform.GetChild(3).GetComponent<Text>().text = currentTargetUnitScript.itemList[targetItemListIndex].GetComponent<ItemScript>().range.ToString();
		targetItemToolTip.transform.GetChild(4).GetComponent<Text>().text = currentTargetUnitScript.itemList[targetItemListIndex].GetComponent<ItemScript>().aoeRadius.ToString();
		targetItemToolTip.SetActive(true);
	}
	//when an equipment slot is highlighted, a tooltip displays the info for the equipment in that slot
	void EquipOnSelect(){
		
		
		Vector3 location = new Vector3( equipToolTip.transform.position.x, currentSelectedEquip.transform.position.y, equipToolTip.transform.position.z);
		equipToolTip.transform.position = location;
		
		EquipmentScript tempEquip = gameManager.equipDict[currentSelectedEquip.transform.GetChild(0).GetComponent<Text>().text].GetComponent<EquipmentScript>();
		equipToolTip.transform.GetChild(2).GetComponent<Text>().text = tempEquip.equipDescription;
		equipToolTip.SetActive(true);
	}
	//When an equipment in the party equipment list is highlighted, the list scrolls, a tooltip is displayed,
	// and the units stats are updated to display what effect equipping that item would have
	void TargetEquipOnSelect(){
		
		/*
		in order to scroll through the list with the arrow keys we have to check if the button selected is outside the 
		equipment window, and shift the buttons accordingly.
		*/
		
		RectTransform containerRect = equipmentListContainer.GetComponent<RectTransform>();
		//get a reference point for where the top and bottom points of the equipment window are
		Vector3[] listOfCorners = new Vector3[4];
		containerRect.GetWorldCorners(listOfCorners);
		float bottom = listOfCorners[0][1];
		float top = listOfCorners[1][1];
		
		//count the number of buttons outside the equipment windows current view
		float buttonsOutside = 0f;
		for(int i = 0; i < (int)numButtons + 1;i++){
			float buttonY = equipmentListContainer.transform.GetChild(0).GetChild(i).position.y;
			if( (buttonY > top) || (buttonY < bottom) ){
				buttonsOutside++;
			}
		}
		
		if (buttonsOutside > 0) {
			
			//get reference points for where the top and bottom points of the selected button are
			Vector3[] buttonCorners = new Vector3[4];
			currentSelectedTargetEquip.GetComponent<RectTransform>().GetWorldCorners(buttonCorners);
			float buttonBottom = buttonCorners[0][1];
			float buttonTop = buttonCorners[1][1];
			float buttonHeight = buttonTop-buttonBottom;
	
			//scroll the list just enough to get the selected button fully in view.
			if(buttonTop> top){
				float diff = buttonTop-top;
				float ratio = diff / buttonHeight;
				equipmentScrollbar.GetComponent<Scrollbar>().value += ratio / buttonsOutside;
			}else if(buttonBottom< bottom){
				float diff = bottom-buttonBottom;
				float ratio = diff / buttonHeight;
				equipmentScrollbar.GetComponent<Scrollbar>().value -= ratio / buttonsOutside;
			}
		}
		
		//the actually equipping of equipment takes place as soon as a piece of equipment is highlighted, in order to show the updated stats in the character sheet
		//if the player cancels out of this menu, the selected unit will re-equip whatever was equipped in that slot immediately.
		
		currentUnitScript.Unequip(gameManager.equipDict[currentUnitScript.equipmentList[currentSelectedEquip.transform.GetSiblingIndex()]].GetComponent<EquipmentScript>(),currentSelectedEquip.transform.GetSiblingIndex());
		
		//turn off the tooltip if the unequip button is selected
		if (currentSelectedTargetEquip.transform.GetChild(0).GetComponent<Text>().text=="Unequip") {
			targetEquipToolTip.SetActive(false);
		} else {
			//otherwise update it's position and the information it displays
			EquipmentScript tempEquip = gameManager.equipDict[currentSelectedTargetEquip.transform.GetChild(0).GetComponent<Text>().text].GetComponent<EquipmentScript>();
			
			targetEquipToolTip.transform.GetChild(2).GetComponent<Text>().text = tempEquip.equipDescription;
			
			Vector3 location = new Vector3( targetEquipToolTip.transform.position.x, currentSelectedTargetEquip.transform.position.y, targetEquipToolTip.transform.position.z);
			targetEquipToolTip.transform.position = location;
			targetEquipToolTip.SetActive(true);
			
			currentUnitScript.Equip(tempEquip,currentSelectedEquip.transform.GetSiblingIndex());
		}
		//update character sheet, to reflect the stat changes introduced by the piece of equipment
		healthValueText.text = currentUnitScript.maxHealth.ToString();
		agilityValueText.text = currentUnitScript.agility.ToString();
		attackValueText.text = currentUnitScript.attack.ToString();
		defenseValueText.text = currentUnitScript.defense.ToString();
		speedValueText.text = currentUnitScript.speed.ToString();
		rangeValueText.text = currentUnitScript.range.ToString();
	}
	
	
	// onClick listener for unit, opens buttonContainer
	void UnitHandle() {
		SoundManager.instance.MenuSelect();
		infoContainer.SetActive(false);
		proceedContainer.SetActive(false);
		buttonContainer.SetActive(true);
		swapItemsButton.Select();
		prevSelectedButton = swapItemsButton.gameObject;
		DisengageButtons(unitButtonList);
		choosingUnit = false;
		buttonContainerOpen = true;
	}
	
	
	// onClick listener for swapItemsButton, disable buttonContainer, enable item buttons
	void SwapItemsHandle() {
		SoundManager.instance.MenuSelect();
		choosingItem = true;
		buttonContainerOpen = false;
		buttonContainer.SetActive(false);
		
		//reset itemButtons
		currentSelectedItem = null;
		
		itemListIndex = 0;
			
		foreach(Button button in itemButtonList) {
			button.gameObject.SetActive(false);
		}			
		itemButtonList = new List<Button>();
		
		//set new itemButtons
		for(int i =0; i < numItemSlots; i++) {
			GameObject tempButton = itemContainer.transform.GetChild(i).gameObject;
			tempButton.transform.GetChild(0).GetComponent<Text>().text = currentUnitScript.itemList[i].GetComponent<ItemScript>().itemName;
			tempButton.SetActive(true);
			tempButton.GetComponent<Button>().interactable = true;
			itemButtonList.Add(tempButton.GetComponent<Button>());
			itemListIndex++;
			if (currentUnitScript.itemList[i].GetComponent<ItemScript>().itemName == "Empty Slot"){
				break;
			}
		}
		itemListIndex = 0;
		prevSelectedButton = itemButtonList[itemListIndex].gameObject;
		itemContainer.SetActive(true);
		itemButtonList[itemListIndex].Select();
	}
	
	//onClick listener for selecting an item to swap.
	void ItemHandle(){
		SoundManager.instance.MenuSelect();
		
		//set state flags
		choosingItem = false;
		choosingTargetUnit = true;
		
		itemToolTip.SetActive(false);
		DisengageButtons(itemButtonList);
		
		ReengageUnitButtons(TargetUnitHandle);
		//a unit can't trade items with itself
		unitButtonList[partyUnitIndex].interactable = false;
		
		//choose which unit in the unit list to highlight first
		if (partyUnitIndex!=0) {
			unitButtonList[0].Select();
			prevSelectedButton = unitButtonList[0].gameObject;
		} else {
			unitButtonList[1].Select();
			prevSelectedButton = unitButtonList[1].gameObject;
		}
		//active right hand ui window to display unit inventories as you highlight them
		rightSheet.SetActive(true);
		targetItemContainer.SetActive(true);
		
	}
	
	//handler for choosing a unit to swap items with
	//the target unit's itemList is displayed, and the player can swap items between the two selected units
	void TargetUnitHandle() {
		SoundManager.instance.MenuSelect();
		choosingTargetUnit = false;
		choosingTargetItem = true;
		
		DisengageButtons(unitButtonList);
		
		foreach(Button itemButton in targetItemButtonList) {
			itemButton.interactable = true;
		}
		targetItemButtonList[0].Select();
		prevSelectedButton = targetItemButtonList[0].gameObject;
		
		return;
	}
	
	//handler for when a target item has been selected for swapping
	//
	void TargetItemHandle() {
		//play sound effect
		SoundManager.instance.Swap();
		
		//swap the items in the units' inventories
		GameObject tempItem = currentUnitScript.itemList[itemListIndex];
		currentUnitScript.itemList[itemListIndex] = currentTargetUnitScript.itemList[targetItemListIndex];
		currentTargetUnitScript.itemList[targetItemListIndex] = tempItem;
		
		// maintain empty slot items in both units' inventories
		if(tempItem.GetComponent<ItemScript>().itemName == "Empty Slot" &&(currentUnitScript.itemList[itemListIndex].GetComponent<ItemScript>().itemName != "Empty Slot")){
			currentUnitScript.itemList.Add(tempItem);
			currentTargetUnitScript.itemList.RemoveAt(targetItemListIndex);
		}
		else if ((currentUnitScript.itemList[itemListIndex].GetComponent<ItemScript>().itemName == "Empty Slot")&&(tempItem.GetComponent<ItemScript>().itemName != "Empty Slot" )) {
			currentTargetUnitScript.itemList.Add(currentUnitScript.itemList[itemListIndex]);
			currentUnitScript.itemList.RemoveAt(itemListIndex);
		}
		
		//set a temp index
			
		foreach(Button button in itemButtonList) {
			button.gameObject.SetActive(false);
		}			
		itemButtonList.Clear();
		
		
		//set new itemButtons for left hand unit

		for(int i =0; i < numItemSlots; i++) {
			GameObject tempButton = itemContainer.transform.GetChild(i).gameObject;
			tempButton.transform.GetChild(0).GetComponent<Text>().text = currentUnitScript.itemList[i].GetComponent<ItemScript>().itemName;
			tempButton.SetActive(true);
			itemButtonList.Add(tempButton.GetComponent<Button>());
			tempButton.GetComponent<Button>().interactable = false;
			if (currentUnitScript.itemList[i].GetComponent<ItemScript>().itemName == "Empty Slot"){
				break;
			}
		}
		
		foreach(Button button in targetItemButtonList) {
			button.gameObject.SetActive(false);
		}			
		targetItemButtonList.Clear();
		
		
		//set new itemButtons for right hand unit
		for(int i =0; i < numItemSlots; i++) {
			GameObject tempButton = targetItemContainer.transform.GetChild(i).gameObject;
			tempButton.transform.GetChild(0).GetComponent<Text>().text = currentTargetUnitScript.itemList[i].GetComponent<ItemScript>().itemName;
			tempButton.SetActive(true);
			targetItemButtonList.Add(tempButton.GetComponent<Button>());
			if (currentTargetUnitScript.itemList[i].GetComponent<ItemScript>().itemName == "Empty Slot"){
				break;
			}
		}
		
		//select/highlight the slot the player just interacted with
		targetItemButtonList[targetItemListIndex].Select();
		prevSelectedButton = targetItemButtonList[targetItemListIndex].gameObject;
		TargetItemOnSelect();
	}
	
	//makes all the buttons in a list non-interactable
	//List<Button> buttonList: list of buttons to make non-interactable
	void DisengageButtons(List<Button> buttonList) {
		foreach(Button button in buttonList) {
			button.interactable = false;
		}
	}
	
	//makes the unit buttons interactable
	//changes the listener functions on the unit buttons to the handler provided as an argument to the method
	//MyHandler handler: function delegate to be assigned to the unit buttons upon state change
	void ReengageUnitButtons(MyHandler handler) {
		
		UnityAction tempAction = new UnityAction(handler);
		
		foreach(Button unitButton in unitButtonList) {
			unitButton.interactable = true;
			unitButton.onClick.RemoveAllListeners();
			unitButton.onClick.AddListener(tempAction);
		}
		currentSelectedUnit.GetComponent<Button>().Select();
		prevSelectedButton = currentSelectedUnit;
	}
	
	
	//makes all the buttons in a list interactable
	//List<Button> buttonList: list of buttons to make interactable
	void ReengageItemButtons(List<Button> buttonList) {
		foreach(Button button in buttonList) {
			button.interactable = true;
		}
	}
	//handler for the proceed button
	//opens a confirmation dialogue before allowing the player to move on to the next combat sequence
	void ProceedHandle() {
		SoundManager.instance.MenuSelect();
		//open proceed menu
		proceedBool = true;
		choosingUnit = false;
		proceedConfirmationContainer.SetActive(true);
		leftSheet.SetActive(false);
		unitButtonContainer.SetActive(false);
		saveButton.interactable = false;
		proceedButton.interactable = false;
		quitButton.interactable = false;
		confirmButton.Select();
		prevSelectedButton = confirmButton.gameObject;
	}
	//calls proceed and loads the save menu
	void SaveHandle() {
		SoundManager.instance.MenuSelect();
		Proceed();
		//load save menu scene
		SceneManager.LoadScene("SaveMenuScene");
	}
	//part of the procedure for transitioning to another scene from the party management scene
	//saves game data, and removes "empty" items from the ends of party members' items lists (they're only used in party management)
	void Proceed() {
		SoundManager.instance.MenuSelect();
		foreach(GameObject unit in gameManager.partyUnits) {
			unit.GetComponent<UnitScript>().itemList.RemoveAt(unit.GetComponent<UnitScript>().itemList.Count-1);
		}
		
		GameManager.instance.GetComponent<GameManager>().Save("/playerInfo.dat");
	}
	//handler for confirmation that the player wishes to proceed to the next level.
	void ConfirmationHandle(){
		SoundManager.instance.MenuSelect();
		Proceed();
		gameManager.InitGame();
	}
	//called when the player backs out of the confirmation dialogue for proceeding to the next level
	void CancelHandle() {
		SoundManager.instance.MenuCancel();
		proceedBool = false;
		choosingUnit = true;
		proceedConfirmationContainer.SetActive(false);
		leftSheet.SetActive(true);
		unitButtonContainer.SetActive(true);
		saveButton.interactable = true;
		proceedButton.interactable = true;
		quitButton.interactable = true;
		proceedButton.Select();
		prevSelectedButton = proceedButton.gameObject;
	}
	//handler for the quit button
	//opens a confirmation dialogue
	void QuitToMenuHandle() {
		SoundManager.instance.MenuSelect();
		//open quit confirmation menu
		quitBool = true;
		choosingUnit = false;
		quitConfirmationContainer.SetActive(true);
		leftSheet.SetActive(false);
		unitButtonContainer.SetActive(false);
		saveButton.interactable = false;
		proceedButton.interactable = false;
		quitButton.interactable = false;
		quitConfirmButton.Select();
		prevSelectedButton = quitConfirmButton.gameObject;
	}
	//called when the player presses the button confirming that they wish to quit to the main menu
	void QuitConfirmationHandle(){
		SoundManager.instance.QuitGame();
		SceneManager.LoadScene("MainMenuScene");
	}
	//called when the player backs out of the quit confirmation window
	void QuitCancelHandle() {
		SoundManager.instance.MenuCancel();
		quitBool = false;
		choosingUnit = true;
		quitConfirmationContainer.SetActive(false);
		leftSheet.SetActive(true);
		unitButtonContainer.SetActive(true);
		saveButton.interactable = true;
		proceedButton.interactable = true;
		quitButton.interactable = true;
		quitButton.Select();
		prevSelectedButton = quitButton.gameObject;
	}
	//handler for the equipment menu button
	//when called, the equipment being used by the selected unit is display, and a slot may be selected to equip something to
	void SwapEquipHandle(){
		SoundManager.instance.MenuSelect();
	
		//set state flags
		buttonContainerOpen = false;
		choosingSlot = true;
		
		currentSelectedEquip = null;
		//hide management options
		buttonContainer.SetActive(false);
		for(int i = 0; i < 4; i++){
			EquipmentScript tempEquip = gameManager.equipDict[currentUnitScript.equipmentList[i]].GetComponent<EquipmentScript>();
			equipContainer.transform.GetChild(i).GetChild(0).GetComponent<Text>().text = tempEquip.equipName; 
		}
		//activate equipment slot buttons
		equipContainer.SetActive(true);
		//select first button
		equipContainer.transform.GetChild(0).GetComponent<Button>().Select();
		prevSelectedButton = equipContainer.transform.GetChild(0).gameObject;
	}
	
	//Handler for when an equipment slot is chosen to equip something to.
	//displays the list of usable equipment in the party inventory
	void EquipHandle(){
		SoundManager.instance.MenuSelect();
		
		//update data member that stores what was equipped in this slot when we entered this context
		//this is used to revert to that state if we back out of the equipment selection
		preEquip = gameManager.equipDict[currentSelectedEquip.transform.GetChild(0).GetComponent<Text>().text].GetComponent<EquipmentScript>();
		
		//set state flags
		choosingSlot = false;
		choosingEquip = true;
		//hide equipment slots, show character sheet in order to display stats changing as different equipments are considered
		equipContainer.SetActive(false);
		infoContainer.SetActive(true);
		rightSheet.SetActive(true);
		equipmentListContainer.SetActive(true);
		equipmentScrollbar.SetActive(true);
		
		numButtons = 0f;
		int i = 1;
		//iterate through party inventory of equipment
		foreach(string equipName in gameManager.partyEquipList) {
			if ( ((currentSelectedEquip.transform.GetSiblingIndex()==0)&&(gameManager.equipDict[equipName].GetComponent<EquipmentScript>().slot=="W")) || ((currentSelectedEquip.transform.GetSiblingIndex()!=0)&&(gameManager.equipDict[equipName].GetComponent<EquipmentScript>().slot=="A")) ){
				//only instantiate new buttons if no unused button exists
				if (i >= equipmentListContainer.transform.GetChild(0).childCount){
					GameObject tempButton = Instantiate(equipmentButtonPrefab) as GameObject;
					tempButton.transform.SetParent(equipmentListContainer.transform.GetChild(0));
					tempButton.transform.GetChild(0).GetComponent<Text>().text = equipName;
					tempButton.GetComponent<Button>().onClick.AddListener(TargetEquipHandle);
					tempButton.GetComponent<RectTransform>().localScale = new Vector3(1,1,1);
				} else {
					//otherwise change values in existing button, and reuse button
					GameObject tempButton = equipmentListContainer.transform.GetChild(0).GetChild(i).gameObject;
					tempButton.transform.GetChild(0).GetComponent<Text>().text = equipName;
					tempButton.SetActive(true);
				}
				i++;
				numButtons++;
			}	
		}
		//initialize values for scrolling
		equipmentScrollbar.GetComponent<Scrollbar>().value = 1;
		currentSelectedTargetEquip = null;
		equipmentListContainer.transform.GetChild(0).GetChild(0).GetComponent<Button>().Select();
		prevSelectedButton = equipmentListContainer.transform.GetChild(0).GetChild(0).gameObject;
		
	}
	
	//Handler for when a target equipment button is pressed
	//swaps the equipment with the equipment currently in the slot being equipped by the unit being managed
	void TargetEquipHandle() {
		//play sound effect
		SoundManager.instance.Swap();
		
		//update state flags
		choosingEquip = false;
		choosingSlot = true;
		
		//if the "equipment" button selected is actually the unequip button, simply empty the slot being equipped
		if (currentSelectedTargetEquip.transform.GetChild(0).GetComponent<Text>().text=="Unequip"){
			currentSelectedEquip.transform.GetChild(0).GetComponent<Text>().text = "Empty";
		} else {
			//otherwise trade equipments from the party inventory to the equipment slot being considered
			currentSelectedEquip.transform.GetChild(0).GetComponent<Text>().text = currentSelectedTargetEquip.transform.GetChild(0).GetComponent<Text>().text;
			gameManager.partyEquipList.Remove(currentSelectedEquip.transform.GetChild(0).GetComponent<Text>().text);
		}
		//if the slot being equipped wasn't empty, add what was previously in the slot to the pary inventory
		if (preEquip.equipName != "Empty") {
			gameManager.partyEquipList.Add(preEquip.equipName);
		}
		
		equipContainer.SetActive(true);
		//make sure the button that was pressed is highlighted again 
		currentSelectedEquip.GetComponent<Button>().Select();
		prevSelectedButton = currentSelectedEquip;
		
		//deactivate UI elements when finished
		for(int i = 1; i<equipmentListContainer.transform.GetChild(0).childCount; i++){
			equipmentListContainer.transform.GetChild(0).GetChild(i).gameObject.SetActive(false);
		}
		
		infoContainer.SetActive(false);
		rightSheet.SetActive(false);
		equipmentListContainer.SetActive(false);
		equipmentScrollbar.SetActive(false);
		targetEquipToolTip.SetActive(false);
		currentSelectedEquip = null;
		currentSelectedTargetEquip = null;
		
		
	}
	
	
}
