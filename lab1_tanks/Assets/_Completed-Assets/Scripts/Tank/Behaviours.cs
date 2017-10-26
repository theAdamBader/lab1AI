/*
These are 3 cases of my own behaviours of the AI and a default which was teaked from Jeremy Gow's TrackBehaviour AI

REFERENCE:
Jeremy Gow's Tanks! Behaviour Tree AI - modified scripts File (Track Behaviour code): https://learn.gold.ac.uk/course/view.php?id=6843&section=2
NP Behave Unity List: http://unitylist.com/r/69f/np-behave

*/

using UnityEngine;
using NPBehave;
using System.Collections.Generic;

namespace Complete
{
	/*
    Example behaviour trees for the Tank AI.  This is partial definition:
    the core AI code is defined in TankAI.cs.

    Use this file to specifiy your new behaviour tree.
     */
	public partial class TankAI : MonoBehaviour
	{
		private Root CreateBehaviourTree() {

			switch (m_Behaviour) {

			case 1:
				return FightMe(); 
			case 2:
				return ScaredyCat();
			case 3:
				return YouAreUnbelievable();

			default:
				return new Root (SuchFun());//this Root calls the default case, SuchFun
			}
		}

		/* Actions */
		private Node  StopMoving(){
			return new Action(() => Move(0)); //This would stop the movement of the AI
		}

		private Node StopTurning() {
			return new Action(() => Turn(0));//This would stop the turning of the AI
		}

		private Node RandomFire() {
			return new Action(() => Fire(UnityEngine.Random.Range(0.0f, 1.0f)));//This would randomly fire at the target
		}

		//SuchFun is case 0 (Fun)
		//this is a private node that contains the selectors for the default case
		//it has an interval of 0.2 and calls UpdatePerception 
		//the first blackboard uses the of centre which once it tracks the target it stops turns and randomly fires
		//the second blackboard checks the x axis of the plane to track where the tank is heading to, if right then turns; else it turns left
		private Node SuchFun(){
			return new Service(0.2f, UpdatePerception,
				new Selector(
					new BlackboardCondition("targetOffCentre",
						Operator.IS_SMALLER_OR_EQUAL, 0.1f,
						Stops.IMMEDIATE_RESTART,
						new Sequence(StopTurning(),//this child node allows to stop the turn and fire random from 1 to -1
							new Wait(0.5f),
							RandomFire())),//this function is called from the Node Random Fire
					
					new BlackboardCondition("targetOnRight",
						Operator.IS_EQUAL, true,//if the targetOnRight is true then turn right towards the target
						Stops.IMMEDIATE_RESTART,
						new Action(() => Turn(0.2f))),
					
					new Action(() => Turn(-0.2f))//else it will turn left towards the target
				)
			);
		}

		//FightMe is case 1 (Deadly)
		//first backboard does NOT see the target at front as the operator is false so when the player is behind then the AI stops moving and turns
		//second blackboard uses the of centre which once it tracks the target it stops turns and randomly fires; however, unlike the default case it does NOT wait for 0.5f, it waits 0.1f before randomly firing
		//third backboard is a random behaviour which would run every time the selector changes, probablity at 5%, and as it is set at a low priority, it would stop once the condition is met and the root proceeds to its next node
		//forth blackboard checks that if the target is near and when near it slows down compared to the third blackboard
		//fifth blackboard checks if the target is on the right then move right else move left
		private Root FightMe()
		{
			return new Root(
				new Service(0.2f, UpdatePerception,
					new Selector(

						new BlackboardCondition("targetInFront",
							Operator.IS_EQUAL, false,//as it's false, it detects the target from behind
							Stops.IMMEDIATE_RESTART,
							new Sequence(StopMoving(),//calls the node, StopMoving
								new Wait(0.5f),
								new Action(() => Turn(1.0f)))),//turns to the right as fast as possible
			
						new BlackboardCondition("targetOffCentre",
							Operator.IS_SMALLER_OR_EQUAL, 0.1f,
							Stops.IMMEDIATE_RESTART,
							new Sequence(StopTurning(),
								new Wait(0.1f),
								RandomFire())),//fires after 0.1f

						new NPBehave.Random(0.05f,new BlackboardCondition("targetDistance",//it would randomly, at 5%, go through this node
							Operator.IS_SMALLER_OR_EQUAL, 50.0f,//if target is 50 pixels near the enemy then enemy moves
							Stops.LOWER_PRIORITY,

							new Sequence(new Action(() => Move(1.0f)),
								new Wait(0.7f),
								RandomFire()))),

						new BlackboardCondition("targetDistance",
							Operator.IS_SMALLER_OR_EQUAL, 10.0f,//if target is 10 pixels near the enemy then enemy moves
							Stops.IMMEDIATE_RESTART,
							//moves forward towards the enemy
							new Action(() => Move(0.6f))),
						
						new BlackboardCondition("targetOnRight",
							Operator.IS_EQUAL, true,
							Stops.IMMEDIATE_RESTART,
							// turns right toward target
							new Action(() => Turn(0.6f))),
						// turns left toward target
						new Action(() => Turn(-0.6f))
					)
				)
			);
		}

		//ScaredyCat is case 2 (Frightened)
		//first blackboard detects if there are anything, target or objects, in front of it and would reverse and turn slowly 
		//second blackboard tracks if the target is near, if it is it moves faster than the third blackboard
		//third blackboard tracks if the target is quite near however, it is random that this node would pass through and using low priority, it would stop once the condition is met 
		//forth blackboard checks if the target is on the right then it would move to the left to try and avoid the target; else it moves right when target is on the left
		private Root ScaredyCat()
		{
			return new Root(
				new Service(0.2f, UpdatePerception,
					new Selector(

						new BlackboardCondition("targetInFront",
							Operator.IS_EQUAL, true,
							Stops.IMMEDIATE_RESTART,
					
							new Sequence(new Action(() => Move(-0.4f)),
								new Wait(1.2f),
								new Action(() => Turn(0.2f)))),
						
						new BlackboardCondition("targetDistance",
							Operator.IS_SMALLER_OR_EQUAL, 10.0f,
							Stops.IMMEDIATE_RESTART,

							new Action(() => Move(1.5f))),

						new NPBehave.Random(0.15f,new BlackboardCondition("targetDistance",//5%
							Operator.IS_SMALLER_OR_EQUAL, 20.0f,//if player is 20 pixels near the enemy then enemy moves
							Stops.LOWER_PRIORITY,

							new Action(() => Move(1.0f)))),
								
						new BlackboardCondition("targetOnRight",
							Operator.IS_EQUAL, true,//if the targetOnRight is true then turn left towards the target
							Stops.IMMEDIATE_RESTART,
							new Action(() => Turn(-0.2f))),

						new Action(() => Turn(0.2f))//else it will right left towards the target
					)
				)
			);
		}

		private Root YouAreUnbelievable()
		{
			return new Root(
				new Service(0.2f, UpdatePerception,
					new Selector(

						new BlackboardCondition("targetInFront",
							Operator.IS_EQUAL, false,
							Stops.IMMEDIATE_RESTART,	//if false then when player is behind enemy then enemy turns around
							// Turn right toward target
							new Sequence(StopMoving(),
								new Wait(0.5f),
								new Action(() => Turn(1.0f)))),

						new BlackboardCondition("targetOffCentre",
							Operator.IS_SMALLER_OR_EQUAL, 0.1f,
							Stops.LOWER_PRIORITY_IMMEDIATE_RESTART,
							// Stop turning and fire
							new Sequence(StopTurning(),
								new Wait(1.0f))),//wait a second
								//RandomFire())),

						new BlackboardCondition("targetDistance",
							Operator.IS_SMALLER_OR_EQUAL, 10.0f,//if player is 10 pixels near the enemy then enemy moves
							Stops.IMMEDIATE_RESTART,

							new Sequence(new Action(() => Move(0.5f)),
								//new Wait(0.1f),
								RandomFire())),

						new NPBehave.Random(0.8f,new BlackboardCondition("targetDistance",//5%
							Operator.IS_SMALLER_OR_EQUAL, 40.0f,//if player is 40 pixels near the enemy then enemy moves
							Stops.LOWER_PRIORITY,

							new Sequence(new Action(() => Move(0.5f)),
								//new Wait(0.1f),
								RandomFire()))),
							

						new NPBehave.Random(0.15f,new BlackboardCondition("targetInFront",
							Operator.IS_EQUAL, true,
							Stops.LOWER_PRIORITY_IMMEDIATE_RESTART,
							// Turn right toward target
							new Sequence(new Action(() => Move(-0.1f)),
								new Wait(0.5f),
								new Action(() => Turn(-0.1f))))),

						new BlackboardCondition("targetOnRight",
							Operator.IS_EQUAL, true,
							Stops.IMMEDIATE_RESTART,
							// Turn right toward target
							new Action(() => Turn(0.6f))),
						// Turn left toward target
						new Action(() => Turn(-0.6f))
					)
				)
			);
		}

		private void UpdatePerception() {
			Vector3 targetPos = TargetTransform().position;
			Vector3 localPos = this.transform.InverseTransformPoint(targetPos);
			Vector3 heading = localPos.normalized;
			blackboard["targetDistance"] = localPos.magnitude;
			blackboard["targetInFront"] = heading.z > 0;
			blackboard["targetOnRight"] = heading.x > 0;
			blackboard["targetOffCentre"] = Mathf.Abs(heading.x);
		}

	}
}