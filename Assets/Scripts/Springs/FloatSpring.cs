using UnityEngine;

namespace CustomSprings
{
    public class FloatSpring : BaseSpring<float>
    {
        protected float SpringTime;

        public override void Reset()
        {
            SpringTime = 0f;
            CurrentValue = 0f;
            CurrentVelocity = 0f;
            InitialVelocity = 0f;
        }

        public override void UpdateGoalValue(float Value, float Velocity)
        {
            StartValue = CurrentValue;
            GoalValue = Value;
            InitialVelocity = Velocity;
            SpringTime = 0f;
        }

        public override float Update(float deltaTime)
        {
            SpringTime += deltaTime;

            float dampingRatio = Damping / (2f * Mathf.Sqrt(Strength * Mass));
            float frequency = Mathf.Sqrt(Strength / Mass); // Undamped angular frequency in radians per second RAD/s
            float initialOffset = GoalValue - StartValue;

            float frequencyRatio = frequency * dampingRatio;
            float value;
            float velocity;

            // Certain variable names are shortened here for a cleaner looking formula
            if (dampingRatio < 1f) // Underdamped
            {
                float x   = frequency * Mathf.Sqrt(1f - dampingRatio * dampingRatio);
                float e   = Mathf.Exp(-frequencyRatio * SpringTime);
                float c1  = initialOffset;
                float c2  = (-InitialVelocity + frequencyRatio * initialOffset) / x;
                float cos = Mathf.Cos(x * SpringTime);
                float sin = Mathf.Sin(x * SpringTime);

                value     = e * (c1 * cos + c2 * sin);
                velocity  = -e * ((initialOffset * frequencyRatio - c2 * x) * cos + (initialOffset * x + c2 * frequencyRatio) * sin);
            }
            else if (dampingRatio > 1) // Overdamped
            {
                float x  = frequency * Mathf.Sqrt(dampingRatio * dampingRatio - 1f);
                float z1 = -frequencyRatio - x;
                float z2 = -frequencyRatio + x;
                float e1 = Mathf.Exp(z1 * SpringTime);
                float e2 = Mathf.Exp(z2 * SpringTime);
                float c1 = (-InitialVelocity - initialOffset * z2) / (-2 * x);
                float c2 = initialOffset - c1;

                value    = c1 * e1 + c2 * e2;
                velocity = c1 * z1 * e1 + c2 * z2 * e2;
            }
            else // Critically damped 
            {
                float e  = Mathf.Exp(-frequency * SpringTime);

                value    = e * (initialOffset + (-InitialVelocity + frequency * initialOffset) * SpringTime);
                velocity = e * (-InitialVelocity * (1 - SpringTime * frequency) + SpringTime * initialOffset * (frequency * frequency));
            }

            CurrentValue    = GoalValue - value;
            CurrentVelocity = velocity;

            return CurrentValue;
        }
    }
}
