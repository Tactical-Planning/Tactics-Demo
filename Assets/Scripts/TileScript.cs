using UnityEngine;
using System.Collections;

public class TileScript : MonoBehaviour {
	// tile properties
	
	// traversable determines if a unit can move onto the tile
	public bool traversable;
	
	// occupyingObject is the object currently in the tile
	public GameObject occupyingObject = null;
}
