using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//class inherits IMouseInput Interface that allows this script to define the functions held within the interface
public class TileMouseInput : MonoBehaviour, IMouseInput
{
    MeshRenderer meshRenderer;
    public Material hoverMat;
    public Material moveMat;
	public Material attackMat;
	public Material rangeMat;
	public Material selectMat;

	private Material defaultMat;
	public bool moving;
	public bool attacking;
	public bool hovering;
	public bool selected;

	public bool tileUsed;

	public MoveButton moveButton;
	public AttackButton attackButton;

    public int tileX;
	public int tileY;

	private readonly List<TileMouseInput> floor = new List<TileMouseInput>();


	public List<CharacterState> people = new List<CharacterState> ();
	public List<EnemyState> enemies = new List<EnemyState> ();


	public void Hover() { hovering = true; SetTile(); }

    public void CanMove() { moving = true; SetTile(); }

    public void CannotMove()
    {
        if (!attacking)
		    selected = false;
		moving = false;
		SetTile();
    }

	public void AttackRange() { attacking = true; SetTile(); }

	public void OutRange()
	{
        if (!moving)
		    selected = false;
		attacking = false;	
	}

	//When the mouse enters the object that is being hovered, this function changes the material back to a hovering material
	public void OnMouseEnter() { hovering = true; SetTile(); }

    //When the mouse leaves the object that is being hovered, this function changes the material back to it's default
    public void OnMouseExit() { hovering = false; SetTile(); }

    void OnMouseDown ()
	{
		if (moving && !tileUsed)
		{
			moveButton.GeneratePathTo(tileX, tileY);
			foreach (TileMouseInput tile in floor)
			{
				tile.selected = false;
				tile.SetTile();
			}
			selected = true;
			SetTile();
		}

		if (attacking && tileUsed && attackButton.foundTarget != true)
		{ 
			attackButton.foundTarget = true;
			attackButton.SelectTarget(tileX, tileY);

			foreach (TileMouseInput tile in floor)
			{
				tile.selected = false;
				tile.SetTile();
			}
			selected = true;
			SetTile();
		}

		
	}

	void SetTile()
	{
		if (selected)
		{
			meshRenderer.material = selectMat;
			return;
		}
		else if (hovering)
		{
            meshRenderer.material = hoverMat;
			return;
		}
		else if (moving)
		{
			meshRenderer.material = moveMat;
			return;
		}
		else if (attacking && tileUsed)
		{
			meshRenderer.material = rangeMat;
		}
		else if (attacking && !tileUsed)
		{
			meshRenderer.material = attackMat;
		}
        else
			meshRenderer.material = defaultMat;

	}

	// Start is called before the first frame update
	void Start()
    {
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        defaultMat = meshRenderer.material;

		people.AddRange (FindObjectsOfType<CharacterState> ());
		enemies.AddRange (FindObjectsOfType<EnemyState> ());
		floor.AddRange(FindObjectsOfType<TileMouseInput>());
	}

    // Update is called once per frame
    void FixedUpdate()
    {
		moveButton = FindObjectOfType<MoveButton> ();
		attackButton = FindObjectOfType<AttackButton> ();

        if (!attacking && !hovering && !moving && !selected) meshRenderer.material = defaultMat;
		
		TileCheck ();
		
		
		for(int i = 0; i < people.Count; i++)
			{ if (people[i].currentState == CharacterState.TurnState.DEAD) people.Remove (people[i]); }
		for(int i = 0; i < enemies.Count; i++)
			{ if (enemies[i].currentState == EnemyState.TurnState.DEAD) enemies.Remove(enemies[i]); }
			
	}

	void TileCheck ()
	{
				
		foreach (CharacterState person in people)
		{
			if (tileX != person.tileX && tileY != person.tileY)
					tileUsed = false;

			if(tileX == person.tileX && tileY == person.tileY)
			{ 
				tileUsed = true;
				return;
			}
		}

		foreach (EnemyState enemy in enemies)
		{
			if (tileX != enemy.tileX && tileY != enemy.tileY)
				tileUsed = false;

			if (tileX == enemy.tileX && tileY == enemy.tileY)
			{ 
				tileUsed = true;
				return;
			}
		}

	}

}
