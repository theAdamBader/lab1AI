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
				return new Root (DefaultMove());
			}
		}

		/* Actions */
		private Node  StopMoving(){
			return new Action(() => Move(0));
		}

		private Node StopTurning() {
			return new Action(() => Turn(0));
		}

		private Node RandomFire() {
			return new Action(() => Fire(UnityEngine.Random.Range(0.0f, 1.0f)));
		}

		private Node RandomTurn() {
			return new Action(() => Turn(UnityEngine.Random.Range(1.0f, -1.0f)));
		}

		private Node RandomMove() {
			return new Action(() => Move(UnityEngine.Random.Range(0.0f, -0.8f)));
		}

		private Node DefaultMove(){
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

		//DONE
		private Root FightMe()
		{
			return new Root(
				new Service(0.2f, UpdatePerception,
					new Selector(

						new BlackboardCondition("targetInFront",
							Operator.IS_EQUAL, false,
							Stops.IMMEDIATE_RESTART,
							// Turn right toward target
							new Sequence(StopMoving(),
								new Wait(0.5f),
								new Action(() => Turn(1.0f)))),
							
						new BlackboardCondition("targetOffCentre",
							Operator.IS_SMALLER_OR_EQUAL, 0.1f,
							Stops.IMMEDIATE_RESTART,
							// Stop turning and fire
							new Sequence(StopTurning(),
								new Wait(0.8f),
								RandomFire())),
						
						new NPBehave.Random(0.05f,new BlackboardCondition("targetDistance",//5%
							Operator.IS_SMALLER_OR_EQUAL, 50.0f,//if player is 40 pixels near the enemy then enemy moves
							Stops.LOWER_PRIORITY,

							new Sequence(new Action(() => Move(1.0f)),
								new Wait(0.7f),
								RandomFire()))),
						
						new BlackboardCondition("targetDistance",
							Operator.IS_SMALLER_OR_EQUAL, 10.0f,
							Stops.IMMEDIATE_RESTART,

							new Action(() => Move(0.6f))),
						

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

		private Root ScaredyCat()
		{
			return new Root(
				new Service(0.2f, UpdatePerception,
					new Selector(
						

						new BlackboardCondition("targetInFront",
							Operator.IS_EQUAL, true,
							Stops.IMMEDIATE_RESTART,
							// Turn right toward target
							new Sequence(new Action(() => Move(-0.4f)),
								new Wait(1.3f),
								new Action(() => Turn(0.2f)))),


						new BlackboardCondition("targetDistance",
							Operator.IS_SMALLER_OR_EQUAL, 10.0f,
							Stops.LOWER_PRIORITY_IMMEDIATE_RESTART,

							new Action(() => Move(0.8f))),
						
								
						new BlackboardCondition("targetOnRight",
							Operator.IS_EQUAL, true,//if the targetOnRight is true then turn right towards the target
							Stops.IMMEDIATE_RESTART,
							new Action(() => Turn(-0.2f))),

						new Action(() => Turn(0.2f))//else it will turn left towards the target
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
								new Wait(1.0f),//wait a second
								RandomFire())),

						new BlackboardCondition("targetDistance",
							Operator.IS_SMALLER_OR_EQUAL, 10.0f,//if player is 10 pixels near the enemy then enemy moves
							Stops.IMMEDIATE_RESTART,

							new Action(() => Move(0.8f))),

						new NPBehave.Random(0.8f,new BlackboardCondition("targetDistance",//5%
							Operator.IS_SMALLER_OR_EQUAL, 40.0f,//if player is 40 pixels near the enemy then enemy moves
							Stops.LOWER_PRIORITY,

							new Action(() => Move(1.0f)))),
						
						new NPBehave.Random(0.35f,new BlackboardCondition("targetInFront",
							Operator.IS_EQUAL, true,
							Stops.LOWER_PRIORITY_IMMEDIATE_RESTART,
							// Turn right toward target
							new Sequence(new Action(() => Move(0.6f)),
								new Wait(0.5f),
								new Action(() => Turn(0.6f))))),

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