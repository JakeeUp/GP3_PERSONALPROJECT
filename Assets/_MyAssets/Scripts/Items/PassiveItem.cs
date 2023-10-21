using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PassiveItem : Item
{
	public abstract void OnEquip(Controller controller);
	public abstract void OnUnEquip(Controller controller);

}