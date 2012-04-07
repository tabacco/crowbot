using UnityEngine;
using System.Collections;

public abstract class State {
	protected string mStateName;
	
	public State() {

	}
	
	public string GetName() {
		return mStateName;	
	}
	
	public virtual void Enter() {}
	public virtual void Exit() {}
	public virtual void Update( float deltaTime ) {}
}

public class StateMachine {
	State 	mCurState;
	ArrayList mStates;
	
	public StateMachine() {
		mStates = new ArrayList();	
	}
	
	public void Update( float deltaTime ) {
		if( mCurState != null ) {
			mCurState.Update( deltaTime );	
		}
	}
	
	public void ChangeState( State newState ) {
		if( mCurState != null ) {
			mCurState.Exit();
		}
		newState.Enter();
		mCurState = newState;
	}
	
	public void AddState( State stateToAdd ) {
		mStates.Add( stateToAdd );
	}
	
	public string GetCurrentStateName() {
		if( mCurState != null ) {
			return mCurState.GetName();	
		}
		else return "";
	}
}


