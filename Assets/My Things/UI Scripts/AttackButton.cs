using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackButton : MonoBehaviour
{
    private CharacterState ActiveGuy;
    private Vector3 GuyPlace;
    public List<EnemyState> enemies = new List<EnemyState> ();
    public BattleHandler Battlehandler;

    public bool attacking;
	public bool foundTarget;
	public EnemyState attackTarget;
    public float range;

    public GameObject[] floor;
    public GameObject confirm;
    public GameObject useAllAP;
    public GameObject cancel;

    private void Start()
    {
        floor = GameObject.FindGameObjectsWithTag("Floor");
        attacking = false;
        foundTarget = false;

        enemies.AddRange (FindObjectsOfType<EnemyState> ());
    }

    void Update()
    {

        if (ActiveGuy != Battlehandler.Dude)
        {
            attacking = false;
            Debug.Log("turn change");

            ActiveGuy = Battlehandler.Dude;
            range = ActiveGuy.range;
        }

       
		for(int i = 0; i < enemies.Count; i++)
			{ if (enemies[i].currentState == EnemyState.TurnState.DEAD) enemies.Remove(enemies[i]); }

        floor = GameObject.FindGameObjectsWithTag("Floor");

        GuyPlace = ActiveGuy.transform.position;

        if (attacking)
        {
			ActiveGuy.attacking = true;
            ActiveGuy.range = (int)range;
            

            foreach (GameObject tile in floor)
            {

                if (Mathf.Abs(tile.transform.position.x - GuyPlace.x) <= range &&
                Mathf.Abs(tile.transform.position.z - GuyPlace.z) <= range)
                {
                    tile.GetComponent<IMouseInput>().AttackRange();
                }
                else
                {
                    tile.GetComponent<IMouseInput>().OutRange();
                }

				confirm.SetActive (true);
                cancel.SetActive(true);
                useAllAP.SetActive(true);
            }
        }

        if (!attacking)
        {
            cancel.SetActive(false);
            useAllAP.SetActive(false);
            confirm.SetActive(false);
			ActiveGuy.attacking = false;

			foreach (GameObject tile in floor)
            {

                tile.GetComponent<IMouseInput>().OutRange();

            }

        }
    }

    public void ActivateAttackPhase()
    {
        if (!attacking && !ActiveGuy.moving && ActiveGuy.currentState == CharacterState.TurnState.SELECTING)
        {
            ActiveGuy = Battlehandler.Dude;
            ActiveGuy.currentState = CharacterState.TurnState.ACTION;
            attacking = true;
        }
    }

    public void AttackConfirm()
    {
        
        AttackControl();
		ActiveGuy.currentState = CharacterState.TurnState.SELECTING;
		attacking = false;
        foundTarget = false;
    }

    public void UseAllAP()
    {
        for (int i = 0; i <= ActiveGuy.herosheet.curAP+1; i++)
        {
            AttackControl();
        }
        ActiveGuy.currentState = CharacterState.TurnState.SELECTING;
        attacking = false;
        foundTarget = false;
    }

    public void Cancel()
    {
        ActiveGuy.currentState = CharacterState.TurnState.SELECTING;
        attacking = false;
        foundTarget = false;
    }

    public void AttackControl()
	{
		int damage = ActiveGuy.herosheet.GetBonus("str");

        if (foundTarget && ActiveGuy.herosheet.curAP > 0)
		{

            Vector3 targetDir = new Vector3(attackTarget.tileX - ActiveGuy.tileX, 0,
                        attackTarget.tileY - ActiveGuy.tileY);

            Vector3.RotateTowards(ActiveGuy.transform.forward, targetDir, Time.deltaTime, 0.0f);

            // Update our unity world position
            ActiveGuy.model.transform.rotation = Quaternion.LookRotation(targetDir);

            attackTarget.enemysheet.curHealth -= damage;

            ActiveGuy.anim.SetTrigger("Attacking");
            Battlehandler.log.text = "\n" + ActiveGuy.gameObject.name
				+ " has hit " + attackTarget.gameObject.name + " for ("+ damage + ") damage." + Battlehandler.log.text;

			//reduces AP by 1 when attacking
			ActiveGuy.herosheet.curAP = ActiveGuy.herosheet.curAP - 1;
                
			return;
		}

		else if (ActiveGuy.herosheet.curAP <= 0)
		{
				Debug.Log("No AP left to attack!");
		}

	}


    public void SelectTarget (int tileX, int tileY)
    {
        foreach (EnemyState enemy in enemies)
		{
			if (enemy.tileX == tileX && enemy.tileY == tileY)
			{
				attackTarget = enemy;
			}				
        
		}
    }
}
