using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GenerateMap : MonoBehaviour
{

    public MapTiles[] tiletypes;

    private int[,] tiles;
	public int scene; 
	public Node [,] graph;

	public GameObject []ground;


	public int mapSizeX;
    public int mapSizeY;


    // Start is called before the first frame update
    void Start()
    {

		//create grid for 
		CreateGrid ();
		//create pathfinding graph
		GeneratePathfindingGraph ();
        //spawn visual prefabs
        CreateMap();

		ground = GameObject.FindGameObjectsWithTag ("Floor");

	}


	void CreateGrid ()
	{
		int x, y;
			//Make map tiles
			tiles = new int [mapSizeX, mapSizeY];
			//Initialize map tiles
			for (x = 0; x < mapSizeX; x++) {
					for (y = 0; y < mapSizeY; y++) {
							tiles [x, y] = 0;
							
					}
			}

		if (scene == 0)
		{

			// Make a big swamp area
			for (x = 4; x <= 8; x++)
			{
				for (y = 0; y < 4; y++)
				{
					tiles[x, y] = 1;
				}
			}

			// Let's make a u-shaped mountain range
			tiles[4, 4] = 2;
			tiles[4, 5] = 2;
			tiles[4, 6] = 2;
			tiles[5, 6] = 2;
			tiles[5, 4] = 2;
			tiles[6, 4] = 2;
			tiles[7, 4] = 2;
			tiles[8, 4] = 2;
			tiles[8, 3] = 2;
			tiles[8, 2] = 2;
			tiles[7, 2] = 2;
		}

		if (scene == 1)
		{
			// Make a big swamp area
			for (x = 0; x <= mapSizeX /2; x++)
			{
				for (y = 0; y < 4; y++)
				{
					tiles[x, y] = 1;
				}
			}

			for (x = 2; x < mapSizeX; x++)
			{
				tiles[x, 4] = 2;
            }

			tiles[2, 4] = 0;
			tiles[2, 5] = 2;
			tiles[2, 6] = 2;
			tiles[2, 7] = 2;
			tiles[2, 8] = 2;
			tiles[2, 9] = 2;

			tiles[3, 5] = 2;
			tiles[4, 5] = 2;
			tiles[5, 5] = 2;
			tiles[6, 5] = 2;

			tiles[6, 6] = 2;
			tiles[6, 7] = 2;
			tiles[6, 8] = 2;

		}

	}

	void GeneratePathfindingGraph ()
	{
			// Initialize the array
			graph = new Node [mapSizeX, mapSizeY];

			// Initialize a Node for each spot in the array
			for (int x = 0; x < mapSizeX; x++) {
					for (int y = 0; y < mapSizeX; y++) {
							graph [x, y] = new Node ();
							graph [x, y].x = x;
							graph [x, y].y = y;
					}
			}

			// Now that all the nodes exist, calculate their neighbours
			for (int x = 0; x < mapSizeX; x++) {
					for (int y = 0; y < mapSizeX; y++) {

							

							// This is the 8-way connection version (allows diagonal movement)
							// Try left
						if (x > 0) {
								graph [x, y].neighbours.Add (graph [x - 1, y]);
								if (y > 0)
										graph [x, y].neighbours.Add (graph [x - 1, y - 1]);
								if (y < mapSizeY - 1)
										graph [x, y].neighbours.Add (graph [x - 1, y + 1]);
						}

						// Try Right
						if (x < mapSizeX - 1) {
								graph [x, y].neighbours.Add (graph [x + 1, y]);
								if (y > 0)
										graph [x, y].neighbours.Add (graph [x + 1, y - 1]);
								if (y < mapSizeY - 1)
										graph [x, y].neighbours.Add (graph [x + 1, y + 1]);
						}

						// Try straight up and down
						if (y > 0)
								graph [x, y].neighbours.Add (graph [x, y - 1]);
						if (y < mapSizeY - 1)
								graph [x, y].neighbours.Add (graph [x, y + 1]);

						// This also works with 6-way hexes and n-way variable areas (like EU4)
					}
			}
	}

	void CreateMap() 
    {
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                MapTiles mt = tiletypes[tiles[x, y]];

                GameObject floor = Instantiate(mt.myTilePrefab, new Vector3(x,-0.75f,y),
                Quaternion.identity) as GameObject;
                floor.transform.SetParent(this.transform);
				floor.GetComponent<TileMouseInput> ().tileX = x;
				floor.GetComponent<TileMouseInput> ().tileY = y;

			}
        }


    }

	public float CostToEnterTile (int sourceX, int sourceY, int targetX, int targetY)
	{
		MapTiles tt = tiletypes [tiles [targetX, targetY]];

		if (UnitCanEnterTile (targetX, targetY) == false)
			return Mathf.Infinity;

		float cost = tt.movementCost;

		if (sourceX != targetX && sourceY != targetY) {
			// We are moving diagonally!  Fudge the cost for tie-breaking
			// Purely a cosmetic thing!
			cost += 0.001f;
		}
		foreach (GameObject square in ground)
		{
            
			if (square.GetComponent<TileMouseInput>().tileUsed &&
				targetX == square.GetComponent<TileMouseInput>().tileX &&
				targetY == square.GetComponent<TileMouseInput> ().tileY)
				cost= cost + 5;
            
			//if (square.GetComponent<TileMouseInput>().tileUsed) return Mathf.Infinity; 
		}

				return cost;

	}

	public Vector3 TileCoordToWorldCoord (int x, int y)
	{
		return new Vector3 (x, 0, y);
	}

	public bool UnitCanEnterTile (int x, int y)
	{

		// We could test the unit's walk/hover/fly type against various
		// terrain flags here to see if they are allowed to enter the tile.

		return tiletypes[tiles [x, y]].isWalkable;
	}
}
