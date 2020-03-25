using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations;

public class CharacterState : MonoBehaviour
{
    public CharacterSheet herosheet = new CharacterSheet();
    public GameObject showStats;    //Gameobject Parent of the stats shown in the left of the screen
    public int init;

    public int movement;
    public int range;

	public int tileX;
	public int tileY;

    //animator parameters
    public bool animMove;
    public bool animHit;
    public bool animDead;

    public bool moving;
	public bool attacking;

    public Transform model;

    public GenerateMap map;
    public Animator anim;

	public List<Node> currentPath = null;

	private Component[] stats; //Component array of all the text components in showStats

    public enum TurnState
    {
        SELECTING,
        WAITING,
        ACTION,
        DEAD
    }

    public TurnState currentState;

    private void Awake()
    {

		init = herosheet.GetBonus("ag") + Random.Range(1,6);
        model = GetComponentsInChildren<Transform>()[1];
    }

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponentInChildren<Animator>();
		moving = false;
		attacking = false;
        ShowStats();

		tileX = (int)transform.position.x;
		tileY = (int)transform.position.z;

        movement = 3;

		currentState = TurnState.WAITING;
        herosheet.SetStats();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
   
        anim.SetBool("Walking", animMove);
        anim.SetBool("Hit", animHit);
        anim.SetBool("Dead", animDead);

        if (herosheet.curHealth <= 0)
        {
            currentState = TurnState.DEAD;

        }

        if (currentState == TurnState.WAITING)
        {
			moving = false;
            currentPath = null;
            herosheet.curAP = 3;
        }

        if (currentState == TurnState.DEAD)
        {
            animDead = true;
        }

		if (currentPath != null)
		{
			int currNode = 0;

			while (currNode < currentPath.Count - 1) {

				Vector3 start = map.TileCoordToWorldCoord (currentPath [currNode].x, currentPath [currNode].y);
				Vector3 end = map.TileCoordToWorldCoord (currentPath [currNode + 1].x, currentPath [currNode + 1].y); 

				Debug.DrawLine (start, end, Color.blue);

				currNode++;
			}

		}
        ShowStats();
    }

    void ShowStats() 
    {
        //shows the main character's baseline stats to the left of the screen

        stats = showStats.GetComponentsInChildren<Text>();

        for (int i = 0; i < stats.Length; i++) 
        {
            Text txt = stats[i].GetComponent<Text>();

            if (i == 0)
            {
               txt.text = "Damage: " + herosheet.GetBonus("str");
            }
            if (i == 1)
            {
                txt.text = "Health: " + herosheet.curHealth;
            }
            if (i == 2)
            {
                txt.text = "Movement: " + herosheet.move;
            }
            if (i == 3)
            {
                txt.text = "Action Points: " + herosheet.curAP;
            }
        }
    }
}

