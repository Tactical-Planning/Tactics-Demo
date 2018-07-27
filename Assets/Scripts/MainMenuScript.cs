using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MainMenuScript : MonoBehaviour {
	
	public GameObject menuContainer;
	public Button loadButton;
	public Button newGameButton;
	public Button creditsButton;
	public Button quitButton;
	
	public GameObject fileButtonContainer;
	public Button fileOneButton;
	public Button fileTwoButton;
	public Button fileThreeButton;
	
	public bool fileSelect;
	
	public GameObject prevFileButton;
	
	// Use this for initialization
	void Start () {
		
		fileSelect = false;
		
		// Set all the button listeners
		newGameButton.onClick.AddListener(NewGameHandle);
		loadButton.onClick.AddListener(LoadGameHandle);
		creditsButton.onClick.AddListener(CreditHandle);
		quitButton.onClick.AddListener(QuitHandle);
		fileOneButton.onClick.AddListener(FileOneHandle);
		fileTwoButton.onClick.AddListener(FileTwoHandle);
		fileThreeButton.onClick.AddListener(FileThreeHandle);
		
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
		
		newGameButton.Select();
		
		prevFileButton = newGameButton.gameObject;
		
	}
	
	// Update is called once per frame
	void Update () {
	
		// if Cancel is pressed when selecting file
		//		Set inactive the file buttons
		//		Set active the menu container elements
		if(fileSelect && Input.GetButtonDown("Cancel")){
			
			SoundManager.instance.CloseFileSelect();
			fileSelect = false;
			fileButtonContainer.SetActive(false);
			menuContainer.SetActive(true);
			loadButton.Select();
			prevFileButton = loadButton.gameObject;
			
		}

		// if the current selected button has changed
		//		play the NavigateMenu sound effect
		if (prevFileButton!=EventSystem.current.currentSelectedGameObject) {
			SoundManager.instance.NavigateMenu();
			prevFileButton=EventSystem.current.currentSelectedGameObject;
		}
	
	}
	
	//Clear any data stored in playerInfo.dat
	//Load first level
	void NewGameHandle(){
		SoundManager.instance.NewGame();
		GameManager.instance.ClearData();
		
		GameManager.instance.Save("/playerInfo.dat");
		SceneManager.LoadScene("Scene1");
		
	}
	
	//Set active the file buttons
	//Set inactive the menu container elements
	void LoadGameHandle(){
		
		SoundManager.instance.OpenFileSelect();
		fileSelect = true;
		menuContainer.SetActive(false);
		fileButtonContainer.SetActive(true);
		fileOneButton.Select();
		prevFileButton = fileOneButton.gameObject;
	}
	
	//Copy the data in file one into playerInfo.dat
	//Load the party management scene
	void FileOneHandle(){
		
		if (GameManager.instance.Load("/fileOneSave.dat") == false) {
			return;
		}
		
		SoundManager.instance.LoadGame();
		GameManager.instance.Save("/playerInfo.dat");
		SceneManager.LoadScene("PartyManagementScene");
		
	}
	
	//Copy the data in file two into playerInfo.dat
	//Load the party management scene
	void FileTwoHandle(){
		
		if (GameManager.instance.Load("/fileTwoSave.dat") == false) {
			return;
		}
		
		SoundManager.instance.LoadGame();
		GameManager.instance.Save("/playerInfo.dat");
		SceneManager.LoadScene("PartyManagementScene");
		
	}
	
	//Copy the data in file three into playerInfo.dat
	//Load the party management scene
	void FileThreeHandle(){
		
		if (GameManager.instance.Load("/fileThreeSave.dat") == false) {
			return;
		}
		
		SoundManager.instance.LoadGame();
		GameManager.instance.Save("/playerInfo.dat");
		SceneManager.LoadScene("PartyManagementScene");
		
	}
	
	//Load the credits scene
	void CreditHandle(){
		
		SceneManager.LoadScene("CreditScene");
		
	}
	
	//Quits the game program
	void QuitHandle(){
		Application.Quit();
	}
	
}
