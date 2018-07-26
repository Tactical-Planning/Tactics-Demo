using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SaveMenuScript : MonoBehaviour {

	public Button proceedButton;
	public Button saveButton;
	public Button fileOneButton;
	public Button fileTwoButton;
	public Button fileThreeButton;
	public GameObject saveConfirm;
	
	public bool fileSelectOpen;
	
	public GameObject prevButtonSelected;

	// Use this for initialization
	void Start () {
		GameManager.instance.GetComponent<GameManager>().Load("/playerInfo.dat");
		
		proceedButton.onClick.AddListener(ProceedHandle);
		saveButton.onClick.AddListener(SaveHandle);
		fileOneButton.onClick.AddListener(FileOneHandle);
		fileTwoButton.onClick.AddListener(FileTwoHandle);
		fileThreeButton.onClick.AddListener(FileThreeHandle);
		saveButton.Select();
		fileSelectOpen = false;
		
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
		
		prevButtonSelected = saveButton.gameObject;
	}
	
	// Update is called once per frame
	void Update () {
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
		
		if (prevButtonSelected!=EventSystem.current.currentSelectedGameObject){
			SoundManager.instance.NavigateMenu();
			prevButtonSelected=EventSystem.current.currentSelectedGameObject;
		}
	}
	
	void ProceedHandle() {
		// load the party management scene
		SoundManager.instance.MenuSelect();
		SceneManager.LoadScene("PartyManagementScene");
	}
	
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
	
	void FileOneHandle(){
		
		SoundManager.instance.SaveGame();
		GameManager.instance.GetComponent<GameManager>().Save("/fileOneSave.dat");
		fileOneButton.gameObject.SetActive(false);
		fileTwoButton.gameObject.SetActive(false);
		fileThreeButton.gameObject.SetActive(false);
		StartCoroutine(SaveConfirmation());
		
		
	}
	
	void FileTwoHandle(){
		
		SoundManager.instance.SaveGame();
		GameManager.instance.GetComponent<GameManager>().Save("/fileTwoSave.dat");
		fileOneButton.gameObject.SetActive(false);
		fileTwoButton.gameObject.SetActive(false);
		fileThreeButton.gameObject.SetActive(false);
		StartCoroutine(SaveConfirmation());
		
		
	}
	
	void FileThreeHandle(){
		
		SoundManager.instance.SaveGame();
		GameManager.instance.GetComponent<GameManager>().Save("/fileThreeSave.dat");
		fileOneButton.gameObject.SetActive(false);
		fileTwoButton.gameObject.SetActive(false);
		fileThreeButton.gameObject.SetActive(false);
		StartCoroutine(SaveConfirmation());
		
		
	}
	
	private IEnumerator SaveConfirmation(){
		fileSelectOpen = false;
		saveConfirm.SetActive(true);
		yield return new WaitForSeconds(.5f);
		SceneManager.LoadScene("PartyManagementScene");
	}
}
