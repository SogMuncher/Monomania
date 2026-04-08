using UnityEngine;

namespace CustomSprings
{
    // This spring uses two Float Springs 
    public class Vector2Spring : BaseSpring<Vector2>
    {
        private FloatSpring XSpring = new FloatSpring();
        private FloatSpring YSpring = new FloatSpring();

        public override float Strength
        {
            get { return base.Strength; }
            set
            {
                XSpring.Strength = value;
                YSpring.Strength = value;
                base.Strength    = value;
            }
        }

        public override float Damping
        {
            get { return base.Damping; }
            set
            {
                XSpring.Damping = value;
                YSpring.Damping = value;
                base.Damping    = value;
            }
        }

        public override Vector2 StartValue
        {
            get { return new Vector2(XSpring.StartValue, YSpring.StartValue); }
            set
            {
                XSpring.StartValue = value.x;
                YSpring.StartValue = value.y;
            }
        }

        public override Vector2 GoalValue
        {
            get { return new Vector2(XSpring.GoalValue, YSpring.GoalValue); }
            set
            {
                XSpring.GoalValue = value.x;
                YSpring.GoalValue = value.y;
            }
        }

        public override Vector2 InitialVelocity
        {
            get { return new Vector2(XSpring.InitialVelocity, YSpring.InitialVelocity); }
            set
            {
                XSpring.InitialVelocity = value.x;
                YSpring.InitialVelocity = value.y;
            }
        }

        public override Vector2 CurrentVelocity
        {
            get { return new Vector2(XSpring.CurrentVelocity, YSpring.CurrentVelocity); }
            set
            {
                XSpring.CurrentVelocity = value.x;
                YSpring.CurrentVelocity = value.y;
            }
        }

        public override Vector2 CurrentValue
        {
            get { return new Vector2(XSpring.CurrentValue, YSpring.CurrentValue); }
            set
            {
                XSpring.CurrentValue = value.x;
                YSpring.CurrentValue = value.y;
            }
        }

        public override void Reset()
        {
            XSpring.Reset();
            YSpring.Reset();
        }

        public override void UpdateGoalValue(Vector2 Value, Vector2 Velocity)
        {
            XSpring.UpdateGoalValue(Value.x, Velocity.x);
            YSpring.UpdateGoalValue(Value.y, Velocity.y);
        }

        public override Vector2 Update(float deltaTime)
        {
            CurrentValue    = new Vector2(XSpring.Update(deltaTime), YSpring.Update(deltaTime));
            CurrentVelocity = new Vector2(XSpring.CurrentVelocity, YSpring.CurrentVelocity);
            return CurrentValue;
        }
    }
}
