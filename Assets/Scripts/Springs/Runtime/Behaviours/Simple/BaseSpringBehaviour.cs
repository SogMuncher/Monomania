using UnityEngine;

namespace CustomSprings.Runtime
{
    public class BaseSpringBehaviour : MonoBehaviour
    {
        [SerializeField, Range(0f, 500f)]
        protected float Strength = 169f;

        [SerializeField, Range(0.1f, 100f)]
        protected float Damping = 26f;
    }
}
