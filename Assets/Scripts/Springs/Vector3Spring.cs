using UnityEngine;

namespace CustomSprings
{
    // This spring uses 3 float springs
    public class Vector3Spring : BaseSpring<Vector3>
    {
        private FloatSpring XSpring = new FloatSpring();
        private FloatSpring YSpring = new FloatSpring();
        private FloatSpring ZSpring = new FloatSpring();

        public override float Strength
        {
            get { return base.Strength; }
            set
            {
                XSpring.Strength = value;
                YSpring.Strength = value;
                ZSpring.Strength = value;
                base.Strength = value;
            }
        }
        public override float Damping
        {
            get { return base.Damping; }
            set
            {
                XSpring.Damping = value;
                YSpring.Damping = value;
                ZSpring.Damping = value;
                base.Damping = value;
            }
        }

        public override Vector3 StartValue
        {
            get { return new Vector3(XSpring.StartValue, YSpring.StartValue, ZSpring.StartValue); }
            set
            {
                XSpring.StartValue = value.x;
                YSpring.StartValue = value.y;
                ZSpring.StartValue = value.z;
            }
        }

        public override Vector3 GoalValue
        {
            get { return new Vector3(XSpring.GoalValue, YSpring.GoalValue, ZSpring.GoalValue); }
            set
            {
                XSpring.GoalValue = value.x;
                YSpring.GoalValue = value.y;
                ZSpring.GoalValue = value.z;
            }
        }

        public override Vector3 InitialVelocity
        {
            get { return new Vector3(XSpring.InitialVelocity, YSpring.InitialVelocity, ZSpring.InitialVelocity); }
            set
            {
                XSpring.InitialVelocity = value.x;
                YSpring.InitialVelocity = value.y;
                ZSpring.InitialVelocity = value.z;
            }
        }

        public override Vector3 CurrentVelocity
        {
            get { return new Vector3(XSpring.CurrentVelocity, YSpring.CurrentVelocity, ZSpring.CurrentVelocity); }
            set
            {
                XSpring.CurrentVelocity = value.x;
                YSpring.CurrentVelocity = value.y;
                ZSpring.CurrentVelocity = value.z;
            }
        }

        public override Vector3 CurrentValue
        {
            get { return new Vector3(XSpring.CurrentValue, YSpring.CurrentValue, ZSpring.CurrentValue); }
            set
            {
                XSpring.CurrentValue = value.x;
                YSpring.CurrentValue = value.y;
                ZSpring.CurrentValue = value.z;
            }
        }

        public override void Reset()
        {
            XSpring.Reset();
            YSpring.Reset();
            ZSpring.Reset();
        }

        public override void UpdateGoalValue(Vector3 Value, Vector3 Velocity)
        {
            XSpring.UpdateGoalValue(Value.x, Velocity.x);
            YSpring.UpdateGoalValue(Value.y, Velocity.y);
            ZSpring.UpdateGoalValue(Value.z, Velocity.z);
        }

        public override Vector3 Update(float deltaTime)
        {
            CurrentValue = new Vector3(XSpring.Update(deltaTime), YSpring.Update(deltaTime), ZSpring.Update(deltaTime));
            CurrentVelocity = new Vector3(XSpring.CurrentVelocity, YSpring.CurrentVelocity, ZSpring.CurrentVelocity);
            return CurrentValue;
        }
    }
}
