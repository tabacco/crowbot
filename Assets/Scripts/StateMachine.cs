using UnityEngine;
using System.Collections;

public abstract class State {
	protected int mStateID; // unique ID specified in the state definition
	
	public int GetID() {
		return mStateID;	
	}
	
	// override these in the state definition
	public abstract void Enter();
	public abstract void Exit();
	public abstract void Update( float deltaTime );
}

public class StateMachine {
	State 		mCurState;	// the currently active state
	ArrayList 	mStates;	// all states on this state machine
	
	public StateMachine() {
		mStates = new ArrayList();	
	}
	
	// update the current state
	public void Update( float deltaTime ) {
		if( mCurState != null ) {
			mCurState.Update( deltaTime );	
		}
	}
	
	// change to a new state, specified by the string name given in the state's definition
	public void ChangeState( int stateID ) {
		// make sure the state specified is actually in the list
		State newState = null;
		foreach( State s in mStates ) {
			if( s.GetID() == stateID ) {
				newState = s;
			}
		}
		if( newState == null ) return;
		
		// exit the current state
		if( mCurState != null ) {
			mCurState.Exit();
		}
		
		// enter the new state, and make it the current state
		newState.Enter();
		mCurState = newState;
	}
	
	// add a new state to this state machine
	public void AddState( State stateToAdd ) {
		mStates.Add( stateToAdd );
	}
	
	// get the current state's name, if any
	public int GetCurrentStateID() {
		if( mCurState != null ) {
			return mCurState.GetID();	
		}
		else return -1;
	}
}


