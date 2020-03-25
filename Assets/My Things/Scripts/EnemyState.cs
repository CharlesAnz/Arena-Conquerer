using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyState : MonoBehaviour
{
    public CharacterSheet enemysheet = new CharacterSheet();

	public List<CharacterState> people = new List<CharacterState>();
	public List<Node> myPath = null;

	public int init;
	public int attackRange;

	//animator parameters
	public Animator anim;
	public bool animMove;
	public bool animDead;

	public bool attack;
	public bool stopMove;

	public Node [,] graph;
	public GenerateMap mapGrid;
	public BattleHandler battleHandler;
	public Transform model;

	public int movement;
	float remainingMovement;

	public int tileX;
	public int tileY;

	public enum TurnState
    {
        SELECTING,
        WAITING,
        ACTION,
        DEAD
    }

    public TurnState currentState;

    // Start is called before the first frame update
    void Start()
    {
		anim = GetComponentInChildren<Animator>();
		enemysheet.SetStats();
		currentState = TurnState.WAITING;
		init = enemysheet.GetBonus ("ag") + Random.Range (1, 6);

		people.AddRange(FindObjectsOfType<CharacterState>());

		movement = enemysheet.GetBonus("ag");
		graph = mapGrid.graph;
		remainingMovement = movement;
		tileX = (int)transform.position.x;
		tileY = (int)transform.position.z;

		model = GetComponentsInChildren<Transform>()[1];
	}

    // Update is called once per frame
    void FixedUpdate()
    {
		anim.SetBool("Walking", animMove);
		anim.SetBool("Dead", animDead);

		graph = mapGrid.graph;
		if (enemysheet.curHealth <= 0) currentState = TurnState.DEAD;
		
        if (currentState == TurnState.DEAD)
        {
			animDead = true;

        }
		if (currentState == TurnState.SELECTING)
        {
			if (TargetInRange(attackRange)) currentState = TurnState.ACTION;
			else if (TargetInRange(10))
			{
				MovetoTarget();	
			}

			if (enemysheet.curAP <= 0)
			{
				currentState = TurnState.WAITING;
				myPath = null;
				battleHandler.activeInit = 0;
				battleHandler.allyTurn++;
				if (battleHandler.allyTurn > people.Count - 1) battleHandler.allyTurn = 0;

				battleHandler.log.text = "\nIt is now: " + battleHandler.people[battleHandler.allyTurn].gameObject.name + "'s turn." + battleHandler.log.text;
			}
			
		}
		if (currentState == TurnState.ACTION)
        {
			if (TargetInRange(attackRange)) attack = true;

			if (attack)
				AttackControl();

			if (enemysheet.curAP <= 0)
			{
				currentState = TurnState.WAITING;
				myPath = null;
				battleHandler.activeInit = 0;
				battleHandler.allyTurn++;
				if (battleHandler.allyTurn > people.Count - 1) battleHandler.allyTurn = 0;

				battleHandler.log.text = "\nIt is now: " + battleHandler.people[battleHandler.allyTurn].gameObject.name + "'s turn." + battleHandler.log.text;
			}
		}

		if (currentState == TurnState.WAITING)
        {
			stopMove = false;
			enemysheet.curAP = 3;
			remainingMovement = movement;
			myPath = null;
			animMove = false;
		}

		if (myPath != null)
		{
			int currNode = 0;

			while (currNode < myPath.Count - 1) {

				Vector3 start = mapGrid.TileCoordToWorldCoord (myPath [currNode].x, myPath [currNode].y);
				Vector3 end = mapGrid.TileCoordToWorldCoord (myPath [currNode + 1].x, myPath [currNode + 1].y);

				Debug.DrawLine (start, end, Color.red);

				currNode++;
			}

		}

		for(int i = 0; i < people.Count; i++)
			{ if (people[i].currentState == CharacterState.TurnState.DEAD) people.Remove (people[i]); }
				
		}

	CharacterState ClosestTarget ()
	{
		CharacterState closestTarget = null;

		List<int> paths = new List<int>();
		Dictionary<int, CharacterState> findPerson = new Dictionary<int, CharacterState>();


		foreach (CharacterState person in people)
		{
			List<Node> newPath = GeneratePathTo(person.tileX, person.tileY);

			findPerson.Add(newPath.Count, person);

            paths.Add(newPath.Count);
		}

		paths.Sort((path1, path2) => path1.CompareTo(path2));

		closestTarget = findPerson[paths[0]];
		Debug.Log(closestTarget);
		return closestTarget;
	}
		
	bool TargetInRange (int range)
	{
		foreach (CharacterState person in people)
		{
			if (Mathf.Abs(person.tileY - tileY) <= range &&
				Mathf.Abs(person.tileX - tileX) <= range)
			{
				return true;
			}
		}
		return false;
	}

	bool TargetInRange (int range, CharacterState person)
	{
		if (Mathf.Abs(person.tileY - tileY) <= range &&
				Mathf.Abs(person.tileX - tileX) <= range)
		{
			return true;
		}
		return false;
	}

	void MovetoTarget()
	{
		foreach (CharacterState person in people)
		{
			if (enemysheet.curAP > 0)
			{
				if(person.herosheet.curHealth < 5)
				{
					//check what Node is closest to target
					if (remainingMovement >= movement) myPath = GeneratePathTo(person.tileX, person.tileY);
					MoveNextTile();
					return;
				}

				else if(TargetInRange(10, person))
				{
					if (remainingMovement >= movement)
					{
						CharacterState target = ClosestTarget();
						myPath = GeneratePathTo(target.tileX, target.tileY);
                    }
					
					MoveNextTile();
					return;
				}

			}	
		}
	}
	public void MoveNextTile ()
	{
		if (remainingMovement > 0)
		{
			if (myPath == null)
				return;

			float speed = Time.deltaTime * 1f;
		    animMove = true;

		    Vector3 targetDir = mapGrid.TileCoordToWorldCoord(myPath[1].x - tileX,
			    myPath[1].y - tileY);

		    Vector3.RotateTowards(transform.forward, targetDir, speed, 0.0f);

		    // Update our unity world position
		    model.transform.rotation = Quaternion.LookRotation(targetDir);

		    // Update our unity world position
		    transform.Translate((myPath[1].x - tileX) * speed, 0, (myPath[1].y - tileY) * speed);

			bool movingright;
			bool movingup;

			if (myPath[1].x - tileX > 0) movingright = true;
			else movingright = false;

			if (myPath[1].y - tileY > 0) movingup = true;
			else movingup = false;
						
			if (StopMove(movingup, movingright) > 0)
			{
				// Get cost from current tile to next tile
				remainingMovement = remainingMovement - mapGrid.CostToEnterTile (myPath [0].x, myPath [0].y,
    				myPath [1].x, myPath [1].y);

				// Move us to the next tile in the sequence
				tileX = myPath [1].x;
				tileY = myPath [1].y;

				transform.position = mapGrid.TileCoordToWorldCoord (tileX, tileY);

				// Remove the old "current" tile
				myPath.RemoveAt (0);
			}
						
			if (myPath.Count <= 2)
			{
				if (TargetInRange (1))
					stopMove = true;

				if (stopMove)
				{


					myPath = null;
					currentState = TurnState.ACTION;
				    animMove = false;
					return;
				}

				// We only have one tile left in the path, and that tile MUST be our ultimate
				// destination -- and we are standing on it!
				// So let's just clear our pathfinding info.
				if (myPath.Count == 1)
				{
					myPath = null;
				    animMove = false;
					currentState = TurnState.WAITING;
					battleHandler.activeInit = 0;
					battleHandler.allyTurn++;
				}
			}
		}

		if (remainingMovement <= 0)
		{
		    animMove = false;
			myPath = null;
			currentState = TurnState.WAITING;
			battleHandler.activeInit = 0;
			battleHandler.allyTurn++;
			battleHandler.log.text = "\n" + gameObject.name + " has ended their turn." + battleHandler.log.text;
		}


	}

		public int StopMove (bool moveup, bool moveright)
		{
				if (moveright && moveup &&
						transform.position.x >= myPath [1].x && transform.position.z >= myPath [1].y)
						return 1;
				else if (moveright && !moveup &&
						transform.position.x >= myPath [1].x && transform.position.z <= myPath [1].y)
						return 2;
				else if (moveup && !moveright &&
						transform.position.x <= myPath [1].x && transform.position.z >= myPath [1].y)
						return 3;
				else if (!moveup && !moveright &&
						transform.position.x <= myPath [1].x && transform.position.z <= myPath [1].y)
						return 4;
				else return 0;
				

		}

		public void AttackControl ()
		{
            int damage = enemysheet.GetBonus ("str");
            if (attackRange > 1) damage = enemysheet.GetBonus("ag");

		    foreach (CharacterState hero in people)
			{
				if (TargetInRange(attackRange, hero) && enemysheet.curAP > 0)
				{
				    Vector3 targetDir = new Vector3(hero.tileX - tileX, 0,
			        hero.tileY - tileY);

				    Vector3.RotateTowards(transform.forward, targetDir, Time.deltaTime, 0.0f);

				    // Update our unity world position
				    model.transform.rotation = Quaternion.LookRotation(targetDir);

				    anim.SetTrigger("Attacking");
					hero.herosheet.curHealth -= damage;

					battleHandler.log.text = "\n" + gameObject.name
						+ " has hit " + hero.gameObject.name + " for (" + damage + ") damage." + battleHandler.log.text;

					//reduces AP by 1 when attacking
					enemysheet.curAP = enemysheet.curAP - 1;

					attack = false;
					return;
				}

				else if (hero.transform.position.z - transform.position.z > 1)
				{
					Debug.Log ("Too Far Away!");
				}
				else if (enemysheet.curAP <= 0)
				{
					Debug.Log ("No AP left to attack!");
				}

			attack = false;

			}

		}


	public List<Node> GeneratePathTo(int x, int y)
	{
		// Clear out our unit's old path.
		myPath = null;

		Dictionary<Node, float> dist = new Dictionary<Node, float>();
		Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

		// Setup the "Q" -- the list of nodes we haven't checked yet.
		List<Node> unvisited = new List<Node>();

		Node source = graph [tileX, tileY];
		
		Node target = graph[x, y];
		
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
			return null;
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

		return currentPath;
	}
}
