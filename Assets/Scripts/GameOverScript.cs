using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown("Submit")) {
			SceneManager.LoadScene("MainMenuScene");
		}
	}
}
