using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateA : BaseState {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public override void EnterState(Action enterState)
	{
		throw new NotImplementedException();
	}

	public override void ExitState(Action exitState)
	{
		throw new NotImplementedException();
	}

	public override BaseState HandleState(ActionTransition actionTransition)
	{
		throw new NotImplementedException();
	}
}
