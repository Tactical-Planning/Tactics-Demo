using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour {

	public static SoundManager instance = null;  

	// combat sounds
	[SerializeField] AudioClip menuSelect;
	[SerializeField] AudioClip menuCancel;
	[SerializeField] AudioClip moveCursor;
	[SerializeField] AudioClip expGain;
	[SerializeField] AudioClip statUp;
	[SerializeField] AudioClip levelUp;
	[SerializeField] AudioClip laserGun;
	[SerializeField] AudioClip phaseChange;
	
	// main menu sounds
	[SerializeField] AudioClip newGame;
	[SerializeField] AudioClip loadGame;
	[SerializeField] AudioClip openFileSelect;
	[SerializeField] AudioClip closeFileSelect;
	[SerializeField] AudioClip navigateMenu;
	[SerializeField] AudioClip saveGame;
	[SerializeField] AudioClip quitGame;
	[SerializeField] AudioClip swap;
	[SerializeField] AudioClip endLevel;
	
	[SerializeField] AudioSource audioSource;

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
	
	// plays in combat when an option in a menu is selected
	public void MenuSelect() {
		audioSource.PlayOneShot(menuSelect, .3f);
	}
	
	// plays in combat when cancel is pressed
	public void MenuCancel() {
		audioSource.PlayOneShot(menuCancel, .5f);
	}
	
	// plays in combat when the cursor is moved
	public void MoveCursor() {
		audioSource.PlayOneShot(moveCursor);
	}
	
	// plays in combat while the experience bar is filling
	public void ExpGain() {
		audioSource.PlayOneShot(expGain);
	}
	
	// plays in the main menu when a new game is started
	public void NewGame(){
		audioSource.PlayOneShot(newGame);
	}
	
	// plays in the main menu when a game is loaded
	public void LoadGame() {
		audioSource.PlayOneShot(loadGame);
	}
	
	// plays in the main menu or in save menu when the file select is opened
	public void OpenFileSelect() {
		audioSource.PlayOneShot(openFileSelect);
	}
	
	// plays in the main menu or in save menu when the file select is closed
	public void CloseFileSelect() {
		audioSource.PlayOneShot(closeFileSelect);
	}
	
	// plays in the main menu or in save menu when navigating between buttons
	public void NavigateMenu() {
		audioSource.PlayOneShot(navigateMenu,0.3f);
	}
	
	// plays in save menu when the game is saved
	public void SaveGame() {
		audioSource.PlayOneShot(saveGame);
	}
	
	// plays in party management when the game is quit
	public void QuitGame() {
		audioSource.PlayOneShot(quitGame);
	}
	
	// plays in party management when items or equipment are swapped
	public void Swap(){
		audioSource.PlayOneShot(swap, .5f);
	}
	
	// plays in combat when a unit levels up
	public void LevelUp(){
		audioSource.PlayOneShot(levelUp);
	}
	
	// plays in combat during level up when a stat increases
	public void StatUp(){
		audioSource.PlayOneShot(statUp);
	}
	
	// plays in combat when a level ends
	public void EndLevel(){
		audioSource.PlayOneShot(endLevel);
	}
	
	// plays in combat when an attack happens
	public void LaserGun(){
		float pitch = Random.Range(0.9f,1.1f);
		audioSource.pitch = pitch;
		audioSource.PlayOneShot(laserGun);
		audioSource.pitch = 1f;
	}
	
	// plays in combat when the phase changes
	// currently no sound has been chosen
	public void PhaseChange(){
		return;
	}

	//General use function for playing of audio files held by other game objects, such as items
	public void PlaySound(AudioClip clip){
		if(clip == null){
			return;
		}
		audioSource.PlayOneShot(clip);
	}
	
	
}
