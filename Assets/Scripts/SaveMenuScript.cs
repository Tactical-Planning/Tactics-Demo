using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SaveMenuScript : MonoBehaviour {

	// references to buttons in the scene
	[SerializeField] Button proceedButton;
	[SerializeField] Button saveButton;
	[SerializeField] Button fileOneButton;
	[SerializeField] Button fileTwoButton;
	[SerializeField] Button fileThreeButton;
	[SerializeField] GameObject saveConfirm;
	
	private bool fileSelectOpen;
	
	private GameObject prevButtonSelected;

	// Use this for initialization
	void Start () {
		GameManager.instance.GetComponent<GameManager>().Load("/playerInfo.dat");
		
		// Set all the button listeners
		proceedButton.onClick.AddListener(ProceedHandle);
		saveButton.onClick.AddListener(SaveHandle);
		fileOneButton.onClick.AddListener(FileOneHandle);
		fileTwoButton.onClick.AddListener(FileTwoHandle);
		fileThreeButton.onClick.AddListener(FileThreeHandle);
		saveButton.Select();
		fileSelectOpen = false;
		
		// prepare lists for iterating in the loop directly below
		List<Button> buttonList = new List<Button>{fileOneButton,fileTwoButton,fileThreeButton};
		List<string> nameList = new List<string>{"/fileOneSave.dat","/fileTwoSave.dat","/fileThreeSave.dat"};
		
		// Get the file information for each saved file
		// Put the information on the file buttons
		for(int i=0; i<nameList.Count; i++) {
			Dictionary<string,string> fileDict = GameManager.instance.GetFileInfo(nameList[i]);
			if (fileDict["Empty"]=="Empty") {
				buttonList[i].transform.GetChild(1).GetComponent<Text>().text = "Empty";
				buttonList[i].transform.GetChild(2).gameObject.SetActive(false);
			} else {
				buttonList[i].transform.GetChild(1).GetComponent<Text>().text = fileDict["Chapter"];
				buttonList[i].transform.GetChild(2).GetComponent<Text>().text = fileDict["PlayTime"];
			}
		}
		
		prevButtonSelected = saveButton.gameObject;
	}
	
	// Update is called once per frame
	void Update () {
		
		// if Cancel is pressed when selecting file
		//		Set inactive the file buttons
		//		Set active the initial menu buttons
		if (Input.GetButtonDown("Cancel") && fileSelectOpen) {
			proceedButton.gameObject.SetActive(true);
			saveButton.gameObject.SetActive(true);
			fileOneButton.gameObject.SetActive(false);
			fileTwoButton.gameObject.SetActive(false);
			fileThreeButton.gameObject.SetActive(false);
			
			saveButton.Select();
			fileSelectOpen = false;
			prevButtonSelected = saveButton.gameObject;
			SoundManager.instance.CloseFileSelect();
		}
		
		// if the current selected button has changed
		//		play the NavigateMenu sound effect
		if (prevButtonSelected!=EventSystem.current.currentSelectedGameObject){
			SoundManager.instance.NavigateMenu();
			prevButtonSelected=EventSystem.current.currentSelectedGameObject;
		}
	}
	
	// ProceedHandle loads the party management scene
	void ProceedHandle() {
		SoundManager.instance.MenuSelect();
		SceneManager.LoadScene("PartyManagementScene");
	}
	
	// SaveHandle sets the file buttons active
	// sets the initial menu buttons inactive
	void SaveHandle() {
		SoundManager.instance.OpenFileSelect();
		
		proceedButton.gameObject.SetActive(false);
		saveButton.gameObject.SetActive(false);
		fileOneButton.gameObject.SetActive(true);
		fileTwoButton.gameObject.SetActive(true);
		fileThreeButton.gameObject.SetActive(true);
		
		fileOneButton.Select();
		fileSelectOpen = true;
		prevButtonSelected = fileOneButton.gameObject;
	}
	
	// FileOneHandle saves the data to fileOneSave.dat
	// sets the file buttons inactive
	// runs SaveConfirmation()
	void FileOneHandle(){
		
		SoundManager.instance.SaveGame();
		GameManager.instance.GetComponent<GameManager>().Save("/fileOneSave.dat");
		fileOneButton.gameObject.SetActive(false);
		fileTwoButton.gameObject.SetActive(false);
		fileThreeButton.gameObject.SetActive(false);
		StartCoroutine(SaveConfirmation());
			
	}
	
	// FileTwoHandle saves the data to fileTwoSave.dat
	// sets the file buttons inactive
	// runs SaveConfirmation()
	void FileTwoHandle(){
		
		SoundManager.instance.SaveGame();
		GameManager.instance.GetComponent<GameManager>().Save("/fileTwoSave.dat");
		fileOneButton.gameObject.SetActive(false);
		fileTwoButton.gameObject.SetActive(false);
		fileThreeButton.gameObject.SetActive(false);
		StartCoroutine(SaveConfirmation());
		
	}
	
	// FileThreeHandle saves the data to fileThreeSave.dat
	// sets the file buttons inactive
	// runs SaveConfirmation()
	void FileThreeHandle(){
		
		SoundManager.instance.SaveGame();
		GameManager.instance.GetComponent<GameManager>().Save("/fileThreeSave.dat");
		fileOneButton.gameObject.SetActive(false);
		fileTwoButton.gameObject.SetActive(false);
		fileThreeButton.gameObject.SetActive(false);
		StartCoroutine(SaveConfirmation());
		
	}
	
	// SaveConfirmation runs after a file buttons has been selected
	// sets the saveConfirm active
	// loads the PartyManagementScene
	private IEnumerator SaveConfirmation(){
		fileSelectOpen = false;
		saveConfirm.SetActive(true);
		yield return new WaitForSeconds(.5f);
		SceneManager.LoadScene("PartyManagementScene");
	}
}
