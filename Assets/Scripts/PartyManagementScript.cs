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
	
	public GameObject proceedContainer;
	public Button saveButton;
	public Button proceedButton;
	public Button quitButton;
	
	public GameObject quitConfirmationContainer;
	public Button quitConfirmButton;
	public Button quitDenyButton;
	
	public GameObject proceedConfirmationContainer;
	public Button confirmButton;
	public Button denyButton;
	
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
		
		swapItemsButton.onClick.AddListener(SwapItemsHandle);
		equipButton.onClick.AddListener(SwapEquipHandle);
		for(int i = 0; i<4; i++) {
			equipContainer.transform.GetChild(i).GetComponent<Button>().onClick.AddListener(EquipHandle);
		}
		equipmentListContainer.transform.GetChild(0).GetChild(0).GetComponent<Button>().onClick.AddListener(TargetEquipHandle);
		equipmentScrollbar.GetComponent<Scrollbar>().interactable = false;
		
		gameManager = GameManager.instance.GetComponent<GameManager>();
		gameManager.Load("/playerInfo.dat");
	
		numItemSlots = 5;
		for(int i=0; i<numItemSlots; i++) {
			GameObject tempButton = itemContainer.transform.GetChild(i).gameObject;
			tempButton.GetComponent<Button>().onClick.AddListener(ItemHandle);
			
			tempButton = targetItemContainer.transform.GetChild(i).gameObject;
			tempButton.GetComponent<Button>().onClick.AddListener(TargetItemHandle);
		}
		
	
		partyUnitIndex = 0;
		targetUnitIndex = 0;
		itemListIndex = 0;
		targetItemListIndex = 0;
		
		foreach(GameObject unit in gameManager.partyUnits) {
			GameObject tempEmpty = Instantiate(emptyItemPrefab) as GameObject;
			tempEmpty.SetActive(false);
			unit.GetComponent<UnitScript>().itemList.Add(tempEmpty);
			GameObject tempButton = unitButtonContainer.transform.GetChild(partyUnitIndex).gameObject;
			tempButton.transform.GetChild(0).GetComponent<Text>().text = unit.GetComponent<UnitScript>().charName;
			tempButton.transform.GetChild(1).GetComponent<Image>().sprite = unit.GetComponent<SpriteRenderer>().sprite;
			tempButton.SetActive(true);
			tempButton.GetComponent<Button>().onClick.AddListener(UnitHandle);
			unitButtonList.Add(tempButton.GetComponent<Button>());
			partyUnitIndex++;
		}
		
		partyUnitIndex = 0;
		unitButtonContainer.transform.GetChild(partyUnitIndex).gameObject.GetComponent<Button>().Select();
		prevSelectedButton = unitButtonContainer.transform.GetChild(partyUnitIndex).gameObject;
		
		saveButton.onClick.AddListener(SaveHandle);
		proceedButton.onClick.AddListener(ProceedHandle);
		quitButton.onClick.AddListener(QuitToMenuHandle);
		confirmButton.onClick.AddListener(ConfirmationHandle);
		denyButton.onClick.AddListener(CancelHandle);
		quitConfirmButton.onClick.AddListener(QuitConfirmationHandle);
		quitDenyButton.onClick.AddListener(QuitCancelHandle);
		
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
		if (prevSelectedButton != EventSystem.current.currentSelectedGameObject) {
			SoundManager.instance.MoveCursor();
			prevSelectedButton = EventSystem.current.currentSelectedGameObject;
		}
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
	
	void EquipOnSelect(){
		
		
		Vector3 location = new Vector3( equipToolTip.transform.position.x, currentSelectedEquip.transform.position.y, equipToolTip.transform.position.z);
		equipToolTip.transform.position = location;
		
		EquipmentScript tempEquip = gameManager.equipDict[currentSelectedEquip.transform.GetChild(0).GetComponent<Text>().text].GetComponent<EquipmentScript>();
		equipToolTip.transform.GetChild(2).GetComponent<Text>().text = tempEquip.equipDescription;
		equipToolTip.SetActive(true);
	}
	
	void TargetEquipOnSelect(){
		
		RectTransform containerRect = equipmentListContainer.GetComponent<RectTransform>();
		
		Vector3[] listOfCorners = new Vector3[4];
		containerRect.GetWorldCorners(listOfCorners);
		float bottom = listOfCorners[0][1];
		float top = listOfCorners[1][1];
		
		
		float buttonsOutside = 0f;
		for(int i = 0; i < (int)numButtons + 1;i++){
			float buttonY = equipmentListContainer.transform.GetChild(0).GetChild(i).position.y;
			if( (buttonY > top) || (buttonY < bottom) ){
				buttonsOutside++;
			}
		}
		
		if (buttonsOutside > 0) {
	
			Vector3[] buttonCorners = new Vector3[4];
			currentSelectedTargetEquip.GetComponent<RectTransform>().GetWorldCorners(buttonCorners);
			float buttonBottom = buttonCorners[0][1];
			float buttonTop = buttonCorners[1][1];
			float buttonHeight = buttonTop-buttonBottom;
	
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
		
		currentUnitScript.Unequip(gameManager.equipDict[currentUnitScript.equipmentList[currentSelectedEquip.transform.GetSiblingIndex()]].GetComponent<EquipmentScript>(),currentSelectedEquip.transform.GetSiblingIndex());
		
		if (currentSelectedTargetEquip.transform.GetChild(0).GetComponent<Text>().text=="Unequip") {
			targetEquipToolTip.SetActive(false);
		} else {
			EquipmentScript tempEquip = gameManager.equipDict[currentSelectedTargetEquip.transform.GetChild(0).GetComponent<Text>().text].GetComponent<EquipmentScript>();
			
			targetEquipToolTip.transform.GetChild(2).GetComponent<Text>().text = tempEquip.equipDescription;
			
			Vector3 location = new Vector3( targetEquipToolTip.transform.position.x, currentSelectedTargetEquip.transform.position.y, targetEquipToolTip.transform.position.z);
			targetEquipToolTip.transform.position = location;
			targetEquipToolTip.SetActive(true);
			
			currentUnitScript.Equip(tempEquip,currentSelectedEquip.transform.GetSiblingIndex());
		}
		
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
		choosingItem = false;
		choosingTargetUnit = true;
		
		itemToolTip.SetActive(false);
		DisengageButtons(itemButtonList);
		
		ReengageUnitButtons(TargetUnitHandle);
		unitButtonList[partyUnitIndex].interactable = false;
		
		if (partyUnitIndex!=0) {
			unitButtonList[0].Select();
			prevSelectedButton = unitButtonList[0].gameObject;
		} else {
			unitButtonList[1].Select();
			prevSelectedButton = unitButtonList[1].gameObject;
		}
		
		rightSheet.SetActive(true);
		targetItemContainer.SetActive(true);
		
	}
	
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
	
	void TargetItemHandle() {
		SoundManager.instance.Swap();
		
		GameObject tempItem = currentUnitScript.itemList[itemListIndex];
		currentUnitScript.itemList[itemListIndex] = currentTargetUnitScript.itemList[targetItemListIndex];
		currentTargetUnitScript.itemList[targetItemListIndex] = tempItem;
		
		
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
		
		
		//set new itemButtons

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
		
		
		//set new itemButtons
		for(int i =0; i < numItemSlots; i++) {
			GameObject tempButton = targetItemContainer.transform.GetChild(i).gameObject;
			tempButton.transform.GetChild(0).GetComponent<Text>().text = currentTargetUnitScript.itemList[i].GetComponent<ItemScript>().itemName;
			tempButton.SetActive(true);
			targetItemButtonList.Add(tempButton.GetComponent<Button>());
			if (currentTargetUnitScript.itemList[i].GetComponent<ItemScript>().itemName == "Empty Slot"){
				break;
			}
		}
		targetItemButtonList[targetItemListIndex].Select();
		prevSelectedButton = targetItemButtonList[targetItemListIndex].gameObject;
		TargetItemOnSelect();
	}
	
	void DisengageButtons(List<Button> buttonList) {
		foreach(Button button in buttonList) {
			button.interactable = false;
		}
	}
	
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
	
	void ReengageItemButtons(List<Button> buttonList) {
		foreach(Button button in buttonList) {
			button.interactable = true;
		}
	}
	
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
	
	void SaveHandle() {
		SoundManager.instance.MenuSelect();
		Proceed();
		//load save menu scene
		SceneManager.LoadScene("SaveMenuScene");
	}
	
	void Proceed() {
		SoundManager.instance.MenuSelect();
		foreach(GameObject unit in gameManager.partyUnits) {
			unit.GetComponent<UnitScript>().itemList.RemoveAt(unit.GetComponent<UnitScript>().itemList.Count-1);
		}
		
		GameManager.instance.GetComponent<GameManager>().Save("/playerInfo.dat");
	}
	
	void ConfirmationHandle(){
		SoundManager.instance.MenuSelect();
		Proceed();
		gameManager.InitGame();
	}
	
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
	
	void QuitConfirmationHandle(){
		SoundManager.instance.QuitGame();
		SceneManager.LoadScene("MainMenuScene");
	}
	
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
	
	void SwapEquipHandle(){
		SoundManager.instance.MenuSelect();
		buttonContainerOpen = false;
		choosingSlot = true;
		
		currentSelectedEquip = null;
		
		buttonContainer.SetActive(false);
		for(int i = 0; i < 4; i++){
			EquipmentScript tempEquip = gameManager.equipDict[currentUnitScript.equipmentList[i]].GetComponent<EquipmentScript>();
			equipContainer.transform.GetChild(i).GetChild(0).GetComponent<Text>().text = tempEquip.equipName; 
		}
		equipContainer.SetActive(true);
		equipContainer.transform.GetChild(0).GetComponent<Button>().Select();
		prevSelectedButton = equipContainer.transform.GetChild(0).gameObject;
	}
	
	void EquipHandle(){
		SoundManager.instance.MenuSelect();
		
		preEquip = gameManager.equipDict[currentSelectedEquip.transform.GetChild(0).GetComponent<Text>().text].GetComponent<EquipmentScript>();
		
		choosingSlot = false;
		choosingEquip = true;
		equipContainer.SetActive(false);
		infoContainer.SetActive(true);
		rightSheet.SetActive(true);
		equipmentListContainer.SetActive(true);
		equipmentScrollbar.SetActive(true);
		
		numButtons = 0f;
		int i = 1;
		foreach(string equipName in gameManager.partyEquipList) {
			if ( ((currentSelectedEquip.transform.GetSiblingIndex()==0)&&(gameManager.equipDict[equipName].GetComponent<EquipmentScript>().slot=="W")) || ((currentSelectedEquip.transform.GetSiblingIndex()!=0)&&(gameManager.equipDict[equipName].GetComponent<EquipmentScript>().slot=="A")) ){
				if (i >= equipmentListContainer.transform.GetChild(0).childCount){
					GameObject tempButton = Instantiate(equipmentButtonPrefab) as GameObject;
					tempButton.transform.SetParent(equipmentListContainer.transform.GetChild(0));
					tempButton.transform.GetChild(0).GetComponent<Text>().text = equipName;
					tempButton.GetComponent<Button>().onClick.AddListener(TargetEquipHandle);
					tempButton.GetComponent<RectTransform>().localScale = new Vector3(1,1,1);
				} else {
					GameObject tempButton = equipmentListContainer.transform.GetChild(0).GetChild(i).gameObject;
					tempButton.transform.GetChild(0).GetComponent<Text>().text = equipName;
					tempButton.SetActive(true);
				}
				i++;
				numButtons++;
			}	
		}
		
		equipmentScrollbar.GetComponent<Scrollbar>().value = 1;
		currentSelectedTargetEquip = null;
		equipmentListContainer.transform.GetChild(0).GetChild(0).GetComponent<Button>().Select();
		prevSelectedButton = equipmentListContainer.transform.GetChild(0).GetChild(0).gameObject;
		
	}
	
	void TargetEquipHandle() {
		SoundManager.instance.Swap();
		
		choosingEquip = false;
		choosingSlot = true;
		
		if (currentSelectedTargetEquip.transform.GetChild(0).GetComponent<Text>().text=="Unequip"){
			currentSelectedEquip.transform.GetChild(0).GetComponent<Text>().text = "Empty";
		} else {
			currentSelectedEquip.transform.GetChild(0).GetComponent<Text>().text = currentSelectedTargetEquip.transform.GetChild(0).GetComponent<Text>().text;
			gameManager.partyEquipList.Remove(currentSelectedEquip.transform.GetChild(0).GetComponent<Text>().text);
		}
		
		if (preEquip.equipName != "Empty") {
			gameManager.partyEquipList.Add(preEquip.equipName);
		}
		
		equipContainer.SetActive(true);
		currentSelectedEquip.GetComponent<Button>().Select();
		prevSelectedButton = currentSelectedEquip;
		
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
