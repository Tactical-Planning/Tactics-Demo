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
	
	public GameObject fileButtonContainer;
	public Button fileOneButton;
	public Button fileTwoButton;
	public Button fileThreeButton;
	
	public bool fileSelect;
	
	public GameObject prevFileButton;
	
	// Use this for initialization
	void Start () {
		
		fileSelect = false;
		newGameButton.onClick.AddListener(NewGameHandle);
		loadButton.onClick.AddListener(LoadGameHandle);
		creditsButton.onClick.AddListener(CreditHandle);
		fileOneButton.onClick.AddListener(FileOneHandle);
		fileTwoButton.onClick.AddListener(FileTwoHandle);
		fileThreeButton.onClick.AddListener(FileThreeHandle);
		
		List<Button> buttonList = new List<Button>{fileOneButton,fileTwoButton,fileThreeButton};
		List<string> nameList = new List<string>{"/fileOneSave.dat","/fileTwoSave.dat","/fileThreeSave.dat"};
		
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
	
		if(fileSelect && Input.GetButtonDown("Cancel")){
			
			SoundManager.instance.CloseFileSelect();
			fileSelect = false;
			fileButtonContainer.SetActive(false);
			menuContainer.SetActive(true);
			loadButton.Select();
			prevFileButton = loadButton.gameObject;
			
		}

		if (prevFileButton!=EventSystem.current.currentSelectedGameObject) {
			SoundManager.instance.NavigateMenu();
			prevFileButton=EventSystem.current.currentSelectedGameObject;
		}
	
	}
	
	//Load first level
	void NewGameHandle(){
		SoundManager.instance.NewGame();
		GameManager.instance.ClearData();
		
		GameManager.instance.Save("/playerInfo.dat");
		SceneManager.LoadScene("Scene1");
		
	}
	
	void LoadGameHandle(){
		
		SoundManager.instance.OpenFileSelect();
		fileSelect = true;
		menuContainer.SetActive(false);
		fileButtonContainer.SetActive(true);
		fileOneButton.Select();
		prevFileButton = fileOneButton.gameObject;
	}
	
	void FileOneHandle(){
		
		if (GameManager.instance.Load("/fileOneSave.dat") == false) {
			return;
		}
		
		SoundManager.instance.LoadGame();
		GameManager.instance.Save("/playerInfo.dat");
		SceneManager.LoadScene("PartyManagementScene");
		
	}
	void FileTwoHandle(){
		
		if (GameManager.instance.Load("/fileTwoSave.dat") == false) {
			return;
		}
		
		SoundManager.instance.LoadGame();
		GameManager.instance.Save("/playerInfo.dat");
		SceneManager.LoadScene("PartyManagementScene");
		
	}
	void FileThreeHandle(){
		
		if (GameManager.instance.Load("/fileThreeSave.dat") == false) {
			return;
		}
		
		SoundManager.instance.LoadGame();
		GameManager.instance.Save("/playerInfo.dat");
		SceneManager.LoadScene("PartyManagementScene");
		
	}
	
	
	void CreditHandle(){
		
		SceneManager.LoadScene("CreditScene");
		
	}
	
}
