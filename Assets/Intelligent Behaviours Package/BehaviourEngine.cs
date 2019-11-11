using System;
using System.Collections;
using System.Collections.Generic;
using Stateless;

public abstract class BehaviourEngine {

    #region variables

    public const bool IsASubmachine = true;
    public const bool IsNotASubmachine = false;

    public StateMachine<State, Perception> BehaviourMachine { get; set; }
    public bool Active { get; set; }
    public LeafNode NodeToReturn { get; set; }
    public bool IsSubMachine { get; set; }

    protected Dictionary<string, Transition> transitions;
    protected Dictionary<string, State> states;
    protected State entryState;

    #endregion variables

    #region create perceptions

    /// <summary>
    /// Creates a new <see cref="Perception"/> of type <see cref="PushPerception"/>
    /// </summary>
    /// <returns></returns>
    public PushPerception CreatePerception<PerceptionType>()
    {
        return new PushPerception(this);
    }

    /// <summary>
    /// Creates a new <see cref="Perception"/> of type <see cref="TimerPerception"/>
    /// </summary>
    /// <param name="time">The time it will take to fire the transition, in seconds</param>
    /// <returns></returns>
    public TimerPerception CreatePerception<PerceptionType>(float time)
    {
        return new TimerPerception(time, this);
    }

    /// <summary>
    /// Creates a new <see cref="Perception"/> of type <see cref="ValuePerception"/>
    /// </summary>
    /// <param name="comparisons">The comparisons that have to be fulfilled to fire the transition.
    /// If there are more than one, they must be all true to fire the transition</param>
    /// <returns></returns>
    public ValuePerception CreatePerception<PerceptionType>(params Func<bool>[] comparisons)
    {
        return new ValuePerception(comparisons, this);
    }

    /// <summary>
    /// Creates a new <see cref="Perception"/> of type <see cref="IsInStatePerception"/>
    /// </summary>
    /// <param name="stateMachineToLookIn">The State Machine where to look in</param>
    /// <param name="StateToLookFor">The name of the state to look for</param>
    /// <returns></returns>
    public IsInStatePerception CreatePerception<PerceptionType>(StateMachineEngine stateMachineToLookIn, string stateToLookFor)
    {
        return new IsInStatePerception(stateMachineToLookIn, stateToLookFor, this);
    }

    /// <summary>
    /// Creates a new <see cref="Perception"/> of type <see cref="BehaviourTreeStatusPerception"/>
    /// </summary>
    /// <param name="behaviourTreeToCheck">The behaviour tree that will be checked</param>
    /// <param name="statusToReach">The status value that needs to reach to fire the transition</param>
    /// <returns></returns>
    public BehaviourTreeStatusPerception CreatePerception<PerceptionType>(BehaviourTreeEngine behaviourTreeToCheck, ReturnValues statusToReach)
    {
        return new BehaviourTreeStatusPerception(behaviourTreeToCheck, statusToReach, this);
    }

    /// <summary>
    /// Creates a new <see cref="Perception"/> of type <see cref="AndPerception"/>
    /// </summary>
    /// <param name="leftPerception">One of the perceptions that will be checked</param>
    /// <param name="rightPerception">One of the perceptions that will be checked</param>
    /// <returns></returns>
    public AndPerception CreateAndPerception<PerceptionType>(Perception leftPerception, Perception rightPerception)
    {
        return new AndPerception(leftPerception, rightPerception, this);
    }

    /// <summary>
    /// Creates a new <see cref="Perception"/> of type <see cref="OrPerception"/>
    /// </summary>
    /// <param name="leftPerception">One of the perceptions that will be checked</param>
    /// <param name="rightPerception">One of the perceptions that will be checked</param>
    /// <returns></returns>
    public OrPerception CreateOrPerception<PerceptionType>(Perception leftPerception, Perception rightPerception)
    {
        return new OrPerception(leftPerception, rightPerception, this);
    }

    /// <summary>
    /// Creates a new <see cref="Perception"/> of a type created by the user
    /// </summary>
    /// <param name="perception">The perception created by the user</param>
    /// <returns></returns>
    public PerceptionType CreatePerception<PerceptionType>(PerceptionType perception)
    {
        Perception perceptionBase = perception as Perception;
        perceptionBase.SetBehaviourMachine(this);

        return perception;
    }

    #endregion create perceptions

    #region create exit transitions

    /// <summary>
    /// Creates a new <see cref="Transition"/> that exits from any Behaviour Engine to a State Machine. ONLY exits to State Machines
    /// </summary>
    /// <param name="transitionName">The name of the transition</param>
    /// <param name="stateFrom">The <see cref="State"/> where the transition comes from</param>
    /// <param name="perception">The <see cref="Perception"/> that will trigger the transition</param>
    /// <param name="stateTo">The <see cref="State"/> where the transition goes to. Must be a State from the super-state machine</param>
    /// <returns></returns>
    public Transition CreateExitTransition(string transitionName, State stateFrom, Perception perception, State stateTo)
    {
        if(!transitions.ContainsKey(transitionName)) {
            StateMachineEngine superStateMachine = stateTo.BehaviourEngine as StateMachineEngine;
            Transition exitTransition = new Transition(transitionName, stateFrom, perception, stateTo, superStateMachine, this);

            if(stateFrom.BehaviourEngine == superStateMachine) { // Transition managed by the super-state machine
                superStateMachine.transitions.Add(transitionName, exitTransition);
            }
            else { // Transition managed by the sub-state machine
                transitions.Add(transitionName, exitTransition);
            }

            return exitTransition;
        }
        else {
            throw new DuplicateWaitObjectException(transitionName, "The transition already exists in the behaviour engine");
        }
    }

    /// <summary>
    /// Creates a new <see cref="Transition"/> that exits from a State Machine to a Behaviour Tree. ONLY exits to Behaviour Trees
    /// </summary>
    /// <param name="transitionName">The name of the transition</param>
    /// <param name="stateFrom">The <see cref="State"/> where the transition is coming from </param>
    /// <param name="perception">The <see cref="Perception"/> that will trigger the transition</param>
    /// <param name="returnValue">The <see cref="ReturnValues"/> that will get the container node when returning to it</param>
    /// <returns></returns>
    public Transition CreateExitTransition(string transitionName, State stateFrom, Perception perception, ReturnValues returnValue)
    {
        if(!stateFrom.BehaviourEngine.transitions.ContainsKey(transitionName)) {
            Transition exitTransition = new Transition(transitionName, stateFrom, perception, NodeToReturn, returnValue, NodeToReturn.StateNode.BehaviourEngine as BehaviourTreeEngine, this);
            stateFrom.BehaviourEngine.transitions.Add(transitionName, exitTransition);

            return exitTransition;
        }
        else {
            throw new DuplicateWaitObjectException(transitionName, "The transition already exists in the behaviour engine");
        }
    }

    /// <summary>
    /// Creates a new <see cref="Transition"/> that exits from a Behaviour Tree to another Behaviour Tree. ONLY exits to Behaviour Trees
    /// </summary>
    /// <param name="transitionName">The name of the transition</param>
    /// <returns></returns>
    public Transition CreateExitTransition(string transitionName)
    {
        if(!transitions.ContainsKey(transitionName)) {
            BehaviourTreeEngine subBehaviourTree = this as BehaviourTreeEngine;
            Transition exitTransition = new Transition(transitionName, subBehaviourTree.GetRootNode(), NodeToReturn, NodeToReturn.StateNode.BehaviourEngine as BehaviourTreeEngine, subBehaviourTree);
            subBehaviourTree.GetRootNode().StateNode.BehaviourEngine.transitions.Add("Exit_Transition", exitTransition);

            return exitTransition;
        }
        else {
            throw new DuplicateWaitObjectException(transitionName, "The transition already exists in the behaviour engine");
        }
    }

    #endregion create exit transitions

    /// <summary>
    /// Fires the transition
    /// </summary>
    /// <param name="transition">The transition that will be triggered</param>
    public void Fire(Transition transition)
    {
        transition.FireTransition();
    }

    /// <summary>
    /// Fires the transition
    /// </summary>
    /// <param name="transitionName">The name of the transition that will be triggered</param>
    public void Fire(string transitionName)
    {
        if(transitions.TryGetValue(transitionName, out Transition transition)) {
            transition.FireTransition();
        }
        else {
            throw new KeyNotFoundException("The Transition '" + transitionName + "' is not found");
        }
    }

    /// <summary>
    /// Gets the entry state of the State Machine
    /// </summary>
    /// <returns></returns>
    public State GetEntryState()
    {
        return entryState;
    }

    /// <summary>
    /// Gets the state named <paramref name="stateName"/>
    /// </summary>
    /// <param name="stateName">The name of the state to look for</param>
    /// <returns></returns>
    public State GetState(string stateName)
    {
        if(states.TryGetValue(stateName, out State state)) {
            return state;
        }
        else {
            throw new KeyNotFoundException("The state '" + stateName + "' doesn't exist in the state machine");
        }
    }

    /// <summary>
    /// Gets the current state the machine is in
    /// </summary>
    /// <returns></returns>
    public State GetCurrentState()
    {
        return BehaviourMachine.State;
    }

    public virtual void Reset()
    {
        return;
    }
}