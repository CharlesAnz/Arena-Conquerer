using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class CharacterSheet
{
		public string pcName;
		public string pClass;

		public int strength = 40;
		public int endurance = 40;
		public int agility = 40;
		public int intelligence = 40;
		public int perception = 40;

		private int strB;
		private int endB;
		private int agB;
		private int intB;
		private int prcB;

		private int maxHealth;
		private int initRate;
		private int speed;
		private int maxAP;

		public int curHealth;
		public int curAP;
        public int turn;
		public int move;

		public CharacterSheet ()
		{
				SetStats ();
		}

		public void SetStats ()
		{
				strB = (strength / 10) % 10;
				endB = (endurance / 10) % 10;
				agB = (agility / 10) % 10;
				intB = (intelligence / 10) % 10;
				prcB = (perception / 10) % 10;

				maxHealth = endurance / 2;
				initRate = agB + intB + prcB;
				speed = (strB + agB) / 2;
				maxAP = 3;

				curHealth = maxHealth;
				curAP = maxAP;
				turn = initRate;
				move = speed;
		}


		public int GetBonus (string bnus)
		{

				if (bnus == "str") { return strB; }
				if (bnus == "end") { return endB; }
				if (bnus == "ag") { return agB; }
				if (bnus == "int") { return intB; }
				if (bnus == "prc") { return prcB; }

				return 0;



		}
}
