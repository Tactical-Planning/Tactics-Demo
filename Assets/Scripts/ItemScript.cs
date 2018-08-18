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
	[SerializeField] AudioClip sound;
	
	private LevelManagerScript levelManagerScript;
	public List<List<int>> validTiles;
	public List<List<int>> tilesInRadius;

	//instantiate the item, initialize tileList values.
	public void ItemInstantiate() {
		levelManagerScript = GameObject.Find("LevelManager(Clone)").GetComponent<LevelManagerScript>();
		validTiles = new List<List<int>>();
		tilesInRadius = new List<List<int>>();
	}
	
	//use the item on any units on every tile within the effective radius of the item.
	public void UseItemOnArea() {
		foreach (List<int> tile in tilesInRadius) {
			if(levelManagerScript.tileArray[tile[0],tile[1]].GetComponent<TileScript>().occupyingObject != null){
				UseItem(levelManagerScript.tileArray[tile[0],tile[1]].GetComponent<TileScript>().occupyingObject);
			}
		}
	}
	
	//UseItem check's what the items name is, and using that, it performs different actions
	//GameObject target: unit for the item to be used on
	private void UseItem(GameObject target) {
		
		//play associated sound effect, if any
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
	
	/*identify valid tiles upon which the item could be used. Recurses out from current holder location
	  populate a list of those tiles
	  int tileX: x coordinate of current location -- initially: the center of effective range
	  int tileY: y coordinate of current location -- initially: the center of effective range
	  int distanceTraveled: how far the recursion has traveled out from the unit location
	*/
	public void FindValidTiles(int tileX, int tileY, int distanceTraveled) {
		
		//don't try to inlude a tile which does not exist (off the edge)
		if (tileX<0 || tileX>=levelManagerScript.tileArrayLength[0] || tileY<0 || tileY>=levelManagerScript.tileArrayLength[1]){
			return;
		}
		
		//don't include non traversable tiles
		if (!levelManagerScript.tileArray[tileX,tileY].GetComponent<TileScript>().traversable) {
			return;
		}
		
		//stop recursing if we have exceed the effective use range of the item
		if (distanceTraveled>range) {
			return;
		}
		
		//return if the tile being considered is already in the list 
		foreach (List<int> subList in validTiles) {
			if (subList[0]==tileX && subList[1]==tileY && subList[2]<=distanceTraveled){
				return;
			}
		}
		validTiles.Add(new List<int> {tileX,tileY,distanceTraveled});
		//recurse out
		FindValidTiles(tileX+1,tileY,distanceTraveled+1);
		FindValidTiles(tileX-1,tileY,distanceTraveled+1);
		FindValidTiles(tileX,tileY+1,distanceTraveled+1);
		FindValidTiles(tileX,tileY-1,distanceTraveled+1);
	}
	
	/*identify the tiles which would be affected by the use of this item on the current location specified. Recurses out from location
	  populate a list of those tiles
	  int tileX: x coordinate of current location -- initially: the center of effective radius
	  int tileY: y coordinate of current location -- initially: the center of effective radius
	  int distanceTraveled: how far the recursion has traveled out from the center
	*/
	public void FindValidTilesInRadius(int tileX, int tileY, int distanceTraveled) {
		
		//don't try to inlude a tile which does not exist (off the edge)
		if (tileX<0 || tileX>=levelManagerScript.tileArrayLength[0] || tileY<0 || tileY>=levelManagerScript.tileArrayLength[1]){
			return;
		}
		
		//don't include non traversable tiles
		if (!levelManagerScript.tileArray[tileX,tileY].GetComponent<TileScript>().traversable) {
			return;
		}
		//stop recursing if the considered tile is out of effect radius
		if (distanceTraveled>aoeRadius) {
			return;
		}
		//if the tile is already in our list, return
		foreach (List<int> subList in tilesInRadius) {
			if (subList[0]==tileX && subList[1]==tileY){
				return;
			}
		}
		tilesInRadius.Add(new List<int> {tileX,tileY,distanceTraveled});
		
		//recurse out
		FindValidTilesInRadius(tileX+1,tileY,distanceTraveled+1);
		FindValidTilesInRadius(tileX-1,tileY,distanceTraveled+1);
		FindValidTilesInRadius(tileX,tileY+1,distanceTraveled+1);
		FindValidTilesInRadius(tileX,tileY-1,distanceTraveled+1);
	}
	
}
