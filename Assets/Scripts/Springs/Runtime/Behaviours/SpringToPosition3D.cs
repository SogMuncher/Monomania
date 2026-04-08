using System.Collections;
using UnityEngine;

namespace CustomSprings.Runtime
{
    // This behaviour allows spring motion on an objects X AND Y AND Z position ONLY
    public class SpringToPosition3D : BaseSpringBehaviour, ISpringToTarget<Vector3>, INudgeable<Vector3>
    {
        private Vector3Spring Spring;

        private void Awake()
        {
            Spring = new Vector3Spring()
            {
                StartValue = transform.position,
                GoalValue = transform.position,
                Strength = Strength,
                Damping = Damping
            };
        }

        public void SpringToTarget(Vector3 TargetPosition)
        {
            StopAllCoroutines();

            CheckInspectorChanges();

            StartCoroutine(Coroutine_SpringToTarget(TargetPosition));
        }

        public void Nudge(Vector3 Amount)
        {
            CheckInspectorChanges();

            if (Mathf.Approximately(Spring.CurrentVelocity.sqrMagnitude, 0))
            {
                StartCoroutine(Coroutine_Nudge(Amount));
            }
            else
            {
                Spring.UpdateGoalValue(Spring.GoalValue, Spring.CurrentVelocity + Amount);
            }
        }

        private void CheckInspectorChanges()
        {
            Spring.Strength = Strength;
            Spring.Damping = Damping;
        }

        private IEnumerator Coroutine_SpringToTarget(Vector3 TargetPosition)
        {
            // Pre loop
            if (Mathf.Approximately(Spring.CurrentVelocity.sqrMagnitude, 0))
            {
                Spring.Reset();
                Spring.StartValue = transform.position;
                Spring.GoalValue = TargetPosition;
            }
            else
            {
                Spring.UpdateGoalValue(TargetPosition, Spring.CurrentVelocity);
            }

            // Loop
            while (Mathf.Approximately(Vector3.SqrMagnitude(transform.position - TargetPosition), 0) == false)
            {
                transform.position = Spring.Update(Time.deltaTime);

                yield return null;
            }

            // Done
            Spring.Reset();
        }

        private IEnumerator Coroutine_Nudge(Vector3 Amount)
        {
            // Pre loop
            Spring.Reset();
            Spring.StartValue = transform.position;
            Spring.GoalValue = transform.position;
            Spring.InitialVelocity = Amount;
            Vector3 targetPosition = transform.position;
            transform.position = Spring.Update(Time.deltaTime);

            // Loop
            while (Mathf.Approximately(0, Vector3.SqrMagnitude(targetPosition - transform.position)) == false)
            {
                transform.position = Spring.Update(Time.deltaTime);

                yield return null;
            }

            // Done
            Spring.Reset();
        }
    }
}

