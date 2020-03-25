using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;

public class BattleHandler : MonoBehaviour
{
	public GameObject winPanel;
	public GameObject losePanel;

    public CharacterState Dude;
	public EnemyState BadDude;
    public int activeInit;
	public int enemyTurn = 0;
	public int allyTurn = 0;
	public TextMeshProUGUI log;
	public MoveButton moveButton;
	public bool gameOver;
	public bool win;

	public List<CharacterState> people = new List<CharacterState>();
	public List<EnemyState> enemies = new List<EnemyState>();


    // Start is called before the first frame update
    void Awake()
    {
        people.AddRange(FindObjectsOfType<CharacterState>());
		enemies.AddRange (FindObjectsOfType<EnemyState> ());
	}

    // Start is called before the first frame update
    void Start()
    {
        people.Sort((person1, person2) => person1.init.CompareTo(person2.init));
		Dude = people [activeInit];

		enemies.Sort((enemy1, enemy2) => enemy1.init.CompareTo(enemy2.init));
		BadDude = enemies [activeInit];


		Dude = people[activeInit];
		Dude.currentState = CharacterState.TurnState.SELECTING;

		log.text = "\nIt is: " + Dude.gameObject.name + "'s turn." + log.text;

	}

    void FixedUpdate()
    {
		if (enemyTurn > enemies.Count -1) enemyTurn = 0;
		if (allyTurn > people.Count -1) allyTurn = 0;

		if (activeInit == 1 && BadDude.currentState != EnemyState.TurnState.DEAD)
		{
			BadDude = enemies[enemyTurn];
			BadDude.currentState = EnemyState.TurnState.SELECTING;
		}
		if (activeInit == 0 && Dude.currentState != CharacterState.TurnState.DEAD)
		{
			Dude = people [allyTurn];
			Dude.currentState = CharacterState.TurnState.SELECTING;	
		}

		if (BadDude.currentState == EnemyState.TurnState.DEAD)
		{
			BadDude = enemies[enemyTurn];
		}

		if (Dude.currentState == CharacterState.TurnState.DEAD)
		{
			Dude = people[allyTurn];
		}
		
		for(int i = 0; i < people.Count; i++)
		{ if (people[i].currentState == CharacterState.TurnState.DEAD) people.Remove (people[i]); }
		for(int i = 0; i < enemies.Count; i++)
		{ if (enemies[i].currentState == EnemyState.TurnState.DEAD) enemies.Remove(enemies[i]); }

		if (enemies.Count < 1)
		{
            gameOver = true;
            win = true;
			winPanel.SetActive(true);

		}
		if (people.Count < 1)
		{
            gameOver = true;
            win = false;

			losePanel.SetActive(true);
		}
	}

    public void CommitButton()
    {
		if (activeInit == 0)
		{
			Dude.currentState = CharacterState.TurnState.WAITING;
			log.text = "\n" + Dude.gameObject.name + " has ended their turn." + log.text;
			moveButton.moving = false;
			activeInit = 1;
			enemyTurn++;

			if (enemyTurn > enemies.Count - 1) enemyTurn = 0;
			log.text = "\nIt is: " + enemies[enemyTurn].gameObject.name + "'s turn." + log.text;

		}
    }

    public void ShowPrimaryStats(GameObject ShowStats)
    {
        ShowStats.SetActive(!ShowStats.activeSelf);
    }

	public void MoveToNextScene()
	{
		if (win)
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

		}

		if (!win)
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

		}

	}


}
