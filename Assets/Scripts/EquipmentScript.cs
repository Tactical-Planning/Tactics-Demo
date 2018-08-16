using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EquipmentScript : MonoBehaviour {
	
	
	public string equipName;
	public string equipDescription;
	//public Image equipSprite;
	
	//stats of equipped unit are modified by the amound stored in these data members
	public int maxHealthMod;
	public int speedMod;
	public int rangeMod;
	public int numAttacksMod;
	public int attackMod;
	public int defenseMod;
	public int agilityMod;
	
	//list of status effects to impose
	public List<string> statusListMod;
	
	//defines whether this equipment is a weapon or an accessory (only one weapon may be equipped at a time)
	public string slot;


}
