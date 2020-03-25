using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveButton : MonoBehaviour
{
    
	//Important Scripts
	public CharacterState ActiveGuy;
	public BattleHandler Battlehandler;

	//Movement variables
    public bool moving;
	bool startMotion;
    public int movement;
	float remainingMovement;

	//Locational variables
	public Vector3 startpos;
	public GameObject[] floor;
	public GenerateMap mapGrid;
	Node [,] graph;

	//UI Buttons
	public GameObject endMove;
	public GameObject moveForward;
	public GameObject undoButton;

    private void Start()
    {
        ActiveGuy = Battlehandler.Dude;

        floor = GameObject.FindGameObjectsWithTag("Floor");
        moving = false;
		remainingMovement = movement;
		graph = mapGrid.graph;
		
    }

    void Update()
    {
		graph = mapGrid.graph;
		if (!moving)
        {
            foreach (GameObject tile in floor)
            {
                tile.GetComponent<IMouseInput>().CannotMove();
            }

            endMove.SetActive(false);
			moveForward.SetActive(false);
			undoButton.SetActive (false);

			ActiveGuy.moving = false;
			ActiveGuy.animMove = false;
		}

        if (ActiveGuy != Battlehandler.Dude)
        {

            moving = false;

            ActiveGuy = Battlehandler.Dude;
            movement = ActiveGuy.herosheet.move;
			remainingMovement = movement;
		}

        floor = GameObject.FindGameObjectsWithTag("Floor");
		if (ActiveGuy.currentState == CharacterState.TurnState.WAITING)
		{
			movement = ActiveGuy.herosheet.move;
			remainingMovement = movement;
		}

        if (moving)
        {
			endMove.SetActive(true);
			moveForward.SetActive (true);
			undoButton.SetActive (true);
			ActiveGuy.moving = true;

			if (startMotion)
			{
				if (remainingMovement > 0)
                {
					if (ActiveGuy.currentPath == null)
						return;
                    ActiveGuy.animMove = true;
                    float speed = Time.deltaTime * 3f;

					Vector3 targetDir = mapGrid.TileCoordToWorldCoord(ActiveGuy.currentPath[1].x - ActiveGuy.tileX,
                        ActiveGuy.currentPath[1].y - ActiveGuy.tileY);

					Vector3.RotateTowards(ActiveGuy.transform.forward, targetDir, speed, 0.0f);

					// Update our unity world position
					ActiveGuy.model.transform.rotation = Quaternion.LookRotation(targetDir);
					ActiveGuy.transform.Translate (targetDir.x * speed, 0, targetDir.z * speed);

					bool movingright;
					bool movingup;

					if (ActiveGuy.currentPath [1].x - ActiveGuy.tileX > 0) movingright = true;
					else movingright = false;

					if (ActiveGuy.currentPath [1].y - ActiveGuy.tileY > 0) movingup = true;
					else movingup = false;


					if (StopMove(movingup, movingright) > 0)
					{
						// Get cost from current tile to next tile
						remainingMovement = remainingMovement - mapGrid.CostToEnterTile (ActiveGuy.currentPath [0].x, ActiveGuy.currentPath [0].y,
							ActiveGuy.currentPath [1].x, ActiveGuy.currentPath [1].y);

						// Move us to the next tile in the sequence
						ActiveGuy.tileX = ActiveGuy.currentPath [1].x;
						ActiveGuy.tileY = ActiveGuy.currentPath [1].y;

						ActiveGuy.transform.position = mapGrid.TileCoordToWorldCoord (ActiveGuy.tileX, ActiveGuy.tileY);

						// Remove the old "current" tile
						ActiveGuy.currentPath.RemoveAt(0);
                        if (remainingMovement <=0) ActiveGuy.animMove = false;
					}

					if (ActiveGuy.currentPath.Count == 1) {
						// We only have one tile left in the path, and that tile MUST be our ultimate
						// destination -- and we are standing on it!
						// So let's just clear our pathfinding info.

						ActiveGuy.currentPath = null;
						startMotion = false;
						ActiveGuy.animMove = false;
						
					}
				}
			}
		}
    }

    public void MoveCharacter()
    {
        if (!moving && !ActiveGuy.attacking && movement > 0 && ActiveGuy.currentState == CharacterState.TurnState.SELECTING)
        {
            ActiveGuy = Battlehandler.Dude;

			startpos = mapGrid.TileCoordToWorldCoord (ActiveGuy.tileX, ActiveGuy.tileY);

            ActiveGuy.currentState = CharacterState.TurnState.ACTION;

            moving = true;

            Battlehandler.log.text = "\n" + ActiveGuy.gameObject.name + " is now moving." + Battlehandler.log.text;

            foreach (GameObject tile in floor)
            {
                if (Mathf.Abs(tile.transform.position.x - startpos.x) <= movement &&
                Mathf.Abs(tile.transform.position.z - startpos.z) <= movement && moving)
                {
                    tile.GetComponent<IMouseInput>().CanMove();

                }
                else
                {
                    tile.GetComponent<IMouseInput>().CannotMove();
                }

            }

        }

        if (movement <= 0)
        {
			Battlehandler.log.text = "\n" + ActiveGuy.gameObject.name + " has run out of movement." + Battlehandler.log.text;

		}
        
    }

		public void MoveNextTile ()
		{
			startMotion = true;
		}

		//function to make sure we know when to stop our movement
		//from tile to tile
		public int StopMove (bool moveup, bool moveright)
		{
				if (moveright && moveup &&
						ActiveGuy.transform.position.x >= ActiveGuy.currentPath [1].x &&
						ActiveGuy.transform.position.z >= ActiveGuy.currentPath [1].y)
				{
						return 1;
						
				}
				if (moveright && !moveup &&
						ActiveGuy.transform.position.x >= ActiveGuy.currentPath [1].x &&
						ActiveGuy.transform.position.z <= ActiveGuy.currentPath [1].y)
				{
						return 2;
						
				}
				if (moveup && !moveright &&
						ActiveGuy.transform.position.x <= ActiveGuy.currentPath [1].x &&
						ActiveGuy.transform.position.z >= ActiveGuy.currentPath [1].y)
				{
						return 3;	
				}
				if (!moveup && !moveright &&
						ActiveGuy.transform.position.x <= ActiveGuy.currentPath [1].x &&
						ActiveGuy.transform.position.z <= ActiveGuy.currentPath [1].y)
				{
						return 4;		
				}
				else return 0;
		}


		public void EndMove()
		{

			startpos = mapGrid.TileCoordToWorldCoord (ActiveGuy.tileX, ActiveGuy.tileY);
			ActiveGuy.currentState = CharacterState.TurnState.SELECTING;
			ActiveGuy.currentPath = null;
			moving = false;
			Battlehandler.log.text = "\n" + ActiveGuy.gameObject.name + " has stopped moving." + Battlehandler.log.text;

		}

		public void Undo()
		{
			ActiveGuy.transform.position = startpos;
		    ActiveGuy.currentPath = null;
			ActiveGuy.tileX = (int)startpos.x;
			ActiveGuy.tileY = (int)startpos.z;
			movement = ActiveGuy.herosheet.move;
			remainingMovement = movement;
			
		}


		public void GeneratePathTo(int x, int y)
	{
		// Clear out our unit's old path.
		ActiveGuy.currentPath = null;

		Dictionary<Node, float> dist = new Dictionary<Node, float>();
		Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

		// Setup the "Q" -- the list of nodes we haven't checked yet.
		List<Node> unvisited = new List<Node>();
		
		Node source = graph[ ActiveGuy.tileX, ActiveGuy.tileY ];
		
		Node target = graph[
		                    x, 
		                    y
		                    ];
		
		dist[source] = 0;
		prev[source] = null;

		// Initialize everything to have INFINITY distance, since
		// we don't know any better right now. Also, it's possible
		// that some nodes CAN'T be reached from the source,
		// which would make INFINITY a reasonable value
		foreach(Node v in graph) {
			if(v != source) {
				dist[v] = Mathf.Infinity;
				prev[v] = null;
			}

			unvisited.Add(v);
		}

		while(unvisited.Count > 0) {
			// "u" is going to be the unvisited node with the smallest distance.
			Node u = null;

			foreach(Node possibleU in unvisited) {
				if(u == null || dist[possibleU] < dist[u]) {
					u = possibleU;
				}
			}

			if(u == target) {
				break;	// Exit the while loop!
			}

			unvisited.Remove(u);

			foreach(Node v in u.neighbours) {
				//float alt = dist[u] + u.DistanceTo(v);
				float alt = dist[u] + mapGrid.CostToEnterTile(u.x, u.y, v.x, v.y);
				if( alt < dist[v] ) {
					dist[v] = alt;
					prev[v] = u;
				}
			}
		}

		// If we get there, then either we found the shortest route
		// to our target, or there is no route at ALL to our target.

		if(prev[target] == null) {
			// No route between our target and the source
			return;
		}

		List<Node> currentPath = new List<Node>();

		Node curr = target;

		// Step through the "prev" chain and add it to our path
		while(curr != null) {
			currentPath.Add(curr);
			curr = prev[curr];
		}

		// Right now, currentPath describes a route from out target to our source
		// So we need to invert it!

		currentPath.Reverse();

		ActiveGuy.currentPath = currentPath;
	}
}
