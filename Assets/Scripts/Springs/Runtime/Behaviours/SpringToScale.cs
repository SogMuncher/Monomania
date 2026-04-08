using System.Collections;
using UnityEngine;

namespace CustomSprings.Runtime
{
    // This behaviour allows spring motion on an objects scale ONLY
    public class SpringToScale : BaseSpringBehaviour, ISpringToTarget<Vector3>, INudgeable<Vector3>
    {
        private Vector3Spring Spring;

        private Vector3 StartScale;

        private void Awake()
        {
            Spring = new Vector3Spring()
            {
                StartValue = transform.localScale,
                GoalValue  = transform.localScale,
                Strength   = Strength,
                Damping    = Damping,
            };

            StartScale = transform.localScale;
        }

        public void ResetScale()
        {
            transform.localScale = StartScale;

            Spring = new Vector3Spring()
            {
                StartValue = StartScale,
                GoalValue  = transform.localScale,
                Strength   = Strength,
                Damping    = Damping,
            };
        }

        public void SpringToTarget(Vector3 TargetScale)
        {
            StopAllCoroutines();

            CheckInspectorChanges();

            StartCoroutine(Coroutine_SpringToTarget(TargetScale));
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
            if (Spring != null)
            {
                Spring.Strength = Strength;
                Spring.Damping  = Damping;
            }
        }

        private IEnumerator Coroutine_SpringToTarget(Vector3 TargetScale)
        {
            // Pre loop
            if (Mathf.Approximately(Spring.CurrentVelocity.sqrMagnitude, 0))
            {
                Spring.Reset();
                Spring.StartValue = transform.localScale;
                Spring.GoalValue  = TargetScale;
            }
            else
            {
                Spring.UpdateGoalValue(TargetScale, Spring.CurrentVelocity);
            }

            // Loop
            while (Mathf.Approximately(Vector3.SqrMagnitude(transform.localScale - TargetScale), 0) == false)
            {
                transform.localScale = Spring.Update(Time.deltaTime);

                yield return null;
            }

            // Done
            Spring.Reset();
        }

        private IEnumerator Coroutine_Nudge(Vector3 Amount)
        {
            // Pre loop
            Spring.Reset();
            Spring.StartValue      = transform.localScale;
            Spring.GoalValue       = transform.localScale;
            Spring.InitialVelocity = Amount;
            Vector3 targetScale    = transform.localScale;
            transform.localScale   = Spring.Update(Time.deltaTime);

            // Loop
            while (Mathf.Approximately(0, Vector3.SqrMagnitude(targetScale - transform.localScale)) == false)
            {
                transform.localScale = Spring.Update(Time.deltaTime);

                yield return null;
            }

            // Done
            Spring.Reset();
        }
    }
}
