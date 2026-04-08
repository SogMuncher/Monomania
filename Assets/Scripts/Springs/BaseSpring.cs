using UnityEngine;

namespace CustomSprings
{
    public abstract class BaseSpring<T>
    {
        // Defaults to a critically damped spring
        public virtual float Strength { get; set; } = 169f;
        public virtual float Damping { get; set; } = 26f;
        public virtual float Mass { get; set; } = 1f;
        public virtual T StartValue { get; set; }
        public virtual T GoalValue { get; set; }
        public virtual T CurrentValue { get; set; }
        public virtual T InitialVelocity { get; set; }
        public virtual T CurrentVelocity { get; set; }

        // Resets the spring to its initial state
        public abstract void Reset();

        // Updates the goal value, KEEPING the current velocity,
        // this makes it transition to the new goal smoothly without snapping
        public virtual void UpdateGoalValue(T Value) => UpdateGoalValue(Value, CurrentVelocity);

        // Updated the goal value, REPLACING the current velocity
        public abstract void UpdateGoalValue(T Value, T Velocity);

        // Advanced the spring simulation by deltaTime seconds
        public abstract T Update(float deltaTime);
    }
}
