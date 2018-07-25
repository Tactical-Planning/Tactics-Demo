using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour {

	public static SoundManager instance = null;  

	// combat sounds
	public AudioClip menuSelect;
	public AudioClip menuCancel;
	public AudioClip moveCursor;
	public AudioClip expGain;
	public AudioClip statUp;
	public AudioClip levelUp;
	public AudioClip laserGun;
	public AudioClip phaseChange;
	
	// main menu sounds
	public AudioClip newGame;
	public AudioClip loadGame;
	public AudioClip openFileSelect;
	public AudioClip closeFileSelect;
	public AudioClip navigateMenu;
	public AudioClip saveGame;
	public AudioClip quitGame;
	public AudioClip swap;
	public AudioClip endLevel;
	
	public AudioSource audioSource;

	// Use this for initialization
	void Start () {
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad(gameObject);
			
			audioSource = gameObject.GetComponent<AudioSource>();
			
		} else {
			Destroy(gameObject);
		}
			
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void MenuSelect() {
		audioSource.PlayOneShot(menuSelect, .3f);
	}
	
	public void MenuCancel() {
		audioSource.PlayOneShot(menuCancel, .5f);
	}
	
	public void MoveCursor() {
		//Debug.Log("playing movement");
		audioSource.PlayOneShot(moveCursor);
	}
	
	public void ExpGain() {
		audioSource.PlayOneShot(expGain);
	}
	
	public void NewGame(){
		audioSource.PlayOneShot(newGame);
	}
	
	public void LoadGame() {
		audioSource.PlayOneShot(loadGame);
	}
	
	public void OpenFileSelect() {
		audioSource.PlayOneShot(openFileSelect);
	}
	
	public void CloseFileSelect() {
		audioSource.PlayOneShot(closeFileSelect);
	}
	
	public void NavigateMenu() {
		audioSource.PlayOneShot(navigateMenu,0.3f);
	}
	
	public void SaveGame() {
		audioSource.PlayOneShot(saveGame);
	}
	
	public void QuitGame() {
		audioSource.PlayOneShot(quitGame);
	}
	
	public void Swap(){
		audioSource.PlayOneShot(swap, .5f);
	}
	
	public void LevelUp(){
		audioSource.PlayOneShot(levelUp);
	}
	
	public void StatUp(){
		audioSource.PlayOneShot(statUp);
	}
	
	public void EndLevel(){
		audioSource.PlayOneShot(endLevel);
	}
	
	public void LaserGun(){
		float pitch = Random.Range(0.9f,1.1f);
		audioSource.pitch = pitch;
		audioSource.PlayOneShot(laserGun);
		audioSource.pitch = 1f;
	}
	public void PhaseChange(){
		//audioSource.PlayOneShot(phaseChange);
	}

	//General use function for playing of audio files held by other game objects, such as items
	public void PlaySound(AudioClip clip){
		if(clip == null){
			return;
		}
		audioSource.PlayOneShot(clip);
	}
	
	
}
