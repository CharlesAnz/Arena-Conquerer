using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMouseInput
{
    void OnMouseEnter();
    void OnMouseExit();
    void Hover();
	void AttackRange ();
	void OutRange ();
    void CanMove();
    void CannotMove();
}
