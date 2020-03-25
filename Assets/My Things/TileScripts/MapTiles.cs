using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class MapTiles
{
    public string name;
    public GameObject myTilePrefab;
	public bool isWalkable;
	public float movementCost = 1;
}
