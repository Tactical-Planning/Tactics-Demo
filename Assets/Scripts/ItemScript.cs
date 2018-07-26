using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ItemScript : MonoBehaviour{

	public string itemName;
	public string description;
	public bool support; // true if item is support item, false otherwise
	public GameObject holder;
	
	public int range;
	public int aoeRadius;
	//public Image image;
	public AudioClip sound;
	
	private LevelManagerScript levelManagerScript;
	public List<List<int>> validTiles;
	public List<List<int>> tilesInRadius;

	public void ItemInstantiate() {
		levelManagerScript = GameObject.Find("LevelManager(Clone)").GetComponent<LevelManagerScript>();
		validTiles = new List<List<int>>();
		tilesInRadius = new List<List<int>>();
	}
	
	public void UseItemOnArea() {
		foreach (List<int> tile in tilesInRadius) {
			if(levelManagerScript.tileArray[tile[0],tile[1]].GetComponent<TileScript>().occupyingObject != null){
				UseItem(levelManagerScript.tileArray[tile[0],tile[1]].GetComponent<TileScript>().occupyingObject);
			}
		}
	}
	
	private void UseItem(GameObject target) {
		//use the item, apply item's effect
		
		SoundManager.instance.PlaySound(sound);
		
		if(itemName == "Candy"){
			
			target.GetComponent<UnitScript>().GainHealth(3);
			return;
		}
		if(itemName == "Molotov"){
			target.GetComponent<UnitScript>().TakeDamage(8);
			return;
		}
		if(itemName == "Magic Death"){
			target.GetComponent<UnitScript>().statusList.Add("Decay");
			target.GetComponent<UnitScript>().statusDurationList.Add(5);
			return;
		}
		
	}
	
	public void FindValidTiles(int tileX, int tileY, int distanceTraveled) {
		
		if (tileX<0 || tileX>=levelManagerScript.tileArrayLength[0] || tileY<0 || tileY>=levelManagerScript.tileArrayLength[1]){
			return;
		}
		
		if (!levelManagerScript.tileArray[tileX,tileY].GetComponent<TileScript>().traversable) {
			return;
		}
		
		if (distanceTraveled>range) {
			return;
		}
		
		foreach (List<int> subList in validTiles) {
			if (subList[0]==tileX && subList[1]==tileY && subList[2]<=distanceTraveled){
				return;
			}
		}
		validTiles.Add(new List<int> {tileX,tileY,distanceTraveled});
		
		FindValidTiles(tileX+1,tileY,distanceTraveled+1);
		FindValidTiles(tileX-1,tileY,distanceTraveled+1);
		FindValidTiles(tileX,tileY+1,distanceTraveled+1);
		FindValidTiles(tileX,tileY-1,distanceTraveled+1);
	}
	
	public void FindValidTilesInRadius(int tileX, int tileY, int distanceTraveled) {
		
		if (tileX<0 || tileX>=levelManagerScript.tileArrayLength[0] || tileY<0 || tileY>=levelManagerScript.tileArrayLength[1]){
			return;
		}
		
		if (!levelManagerScript.tileArray[tileX,tileY].GetComponent<TileScript>().traversable) {
			return;
		}
		
		if (distanceTraveled>aoeRadius) {
			return;
		}
		
		foreach (List<int> subList in tilesInRadius) {
			if (subList[0]==tileX && subList[1]==tileY){
				return;
			}
		}
		tilesInRadius.Add(new List<int> {tileX,tileY,distanceTraveled});
		
		FindValidTilesInRadius(tileX+1,tileY,distanceTraveled+1);
		FindValidTilesInRadius(tileX-1,tileY,distanceTraveled+1);
		FindValidTilesInRadius(tileX,tileY+1,distanceTraveled+1);
		FindValidTilesInRadius(tileX,tileY-1,distanceTraveled+1);
	}
	
}
