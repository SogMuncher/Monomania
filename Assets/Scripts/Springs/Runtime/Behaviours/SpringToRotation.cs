using System.Collections;
using UnityEngine;

namespace CustomSprings.Runtime
{
    // This behaviour allows spring motion on an objects rotation ONLY
    public class SpringToRotation : BaseSpringBehaviour, ISpringToTarget<Vector3>, ISpringToTarget<Quaternion>, INudgeable<Vector3>, INudgeable<Quaternion>
    {
        private Vector3Spring Spring;

        private void Awake()
        {
            Spring = new Vector3Spring()
            {
                StartValue = transform.rotation.eulerAngles,
                GoalValue  = transform.rotation.eulerAngles,
                Strength   = Strength,
                Damping    = Damping
            };
        }

        public void SpringToTarget(Vector3 TargetRotation)
        {
            SpringToTarget(Quaternion.Euler(TargetRotation));
        }

        public void SpringToTarget(Quaternion TargetRotation)
        {
            StopAllCoroutines();

            CheckInspectorChanges();

            StartCoroutine(Coroutine_SpringToTarget(TargetRotation));
        }

        public void Nudge(Vector3 Amount)
        {
            CheckInspectorChanges();

            // If not moving, normal coroutine. If moving, use the current goal and just add velocity
            if (Mathf.Approximately(Spring.CurrentVelocity.sqrMagnitude, 0))
            {
                StartCoroutine(Coroutine_Nudge(Amount));
            }
            else
            {
                Spring.UpdateGoalValue(Spring.GoalValue, Spring.CurrentVelocity + Amount);
            }
        }

        public void Nudge(Quaternion Amount)
        {
            Nudge(Amount.eulerAngles);
        }

        private void CheckInspectorChanges()
        {
            Spring.Strength = Strength;
            Spring.Damping  = Damping;
        }

        private IEnumerator Coroutine_SpringToTarget(Quaternion TargetRotation)
        {
            // Pre loop
            if (Mathf.Approximately(Spring.CurrentVelocity.sqrMagnitude, 0))
            {
                Spring.Reset();
                Spring.StartValue = transform.eulerAngles;
                Spring.GoalValue  = TargetRotation.eulerAngles;
            }
            else
            {
                Spring.UpdateGoalValue(TargetRotation.eulerAngles, Spring.CurrentVelocity);
            }

            // Loop
            while (Mathf.Approximately(0, 1 - Quaternion.Dot(transform.rotation, TargetRotation)) == false)
            {
                transform.rotation = Quaternion.Euler(Spring.Update(Time.deltaTime));

                yield return null;
            }

            // Done
            Spring.Reset();
        }

        private IEnumerator Coroutine_Nudge(Vector3 Amount)
        {
            // Pre loop
            Spring.Reset();
            Spring.StartValue         = transform.rotation.eulerAngles;
            Spring.GoalValue          = transform.rotation.eulerAngles;
            Spring.InitialVelocity    = Amount;
            Quaternion targetRotation = transform.rotation;
            transform.rotation        = Quaternion.Euler(Spring.Update(Time.deltaTime));

            // Loop
            while (Mathf.Approximately(0, 1 - Quaternion.Dot(targetRotation, transform.rotation)) == false)
            {
                transform.rotation = Quaternion.Euler(Spring.Update(Time.deltaTime));

                yield return null;
            }

            // Done
            Spring.Reset();
        }
    }
}
