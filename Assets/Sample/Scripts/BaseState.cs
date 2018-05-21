using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum State
{
  A,
  B,
  C,
}

public enum ActionTransition
{
  A1,
  A2,
  A3,
  A4,
}
public abstract class BaseState: MonoBehaviour
{
  private Dictionary<ActionTransition, BaseState> transitions;
  protected State state;

  public void AddTransition(ActionTransition actionTransition, BaseState state)
  {
    if (this.transitions==null)
    {
      this.transitions=new Dictionary<ActionTransition, BaseState>();
    }

    if (!this.transitions.ContainsKey(actionTransition))
    {
      this.transitions.Add(actionTransition,state);
    }
    else
    {
      Debug.LogError("Cannot add same transition twice to the same state");
    }
  }

  public State GetState()
  {
    return this.state;
  }

  public abstract void EnterState(Action enterState);
  public abstract void ExitState(Action exitState);
  public abstract BaseState HandleState(ActionTransition actionTransition);
}
