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
				return MovetoPlayer(); 

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

		private Node DefaultMove(){
			return new Service(0.2f, UpdatePerception,
				new Selector(
					new BlackboardCondition("targetOffCentre",
						Operator.IS_SMALLER_OR_EQUAL, 0.1f,
						Stops.IMMEDIATE_RESTART,
						// Stop turning and fire
						new Sequence(StopTurning(),
							new Wait(0.5f),
							RandomFire())),
					new BlackboardCondition("targetOnRight",
						Operator.IS_EQUAL, true,
						Stops.IMMEDIATE_RESTART,
						// Turn right toward target
						new Action(() => Turn(0.2f))),
					// Turn left toward target
					new Action(() => Turn(-0.2f))
				)

			);
		}


		private Root MovetoPlayer()
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
								RandomTurn())),
							
						new BlackboardCondition("targetOffCentre",
							Operator.IS_SMALLER_OR_EQUAL, 0.1f,
							Stops.IMMEDIATE_RESTART,
							// Stop turning and fire
							new Sequence(StopTurning(),
								new Wait(0.3f),
								RandomFire())),
						
						new BlackboardCondition("targetDistance",
							Operator.IS_SMALLER_OR_EQUAL, 15.0f,
							Stops.LOWER_PRIORITY_IMMEDIATE_RESTART,

							new Action(() => Move(0.6f))),
						

						new BlackboardCondition("targetOnRight",
							Operator.IS_EQUAL, true,
							Stops.IMMEDIATE_RESTART,
							// Turn right toward target
							new Action(() => Turn(0.3f))),
						// Turn left toward target
						new Action(() => Turn(-0.3f))


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