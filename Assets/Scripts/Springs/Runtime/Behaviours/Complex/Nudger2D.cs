using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CustomSprings.Runtime
{
    public class Nudger2D : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        public BaseSpringBehaviour SpringBehaviour;
        private Button _button;

        public Vector2 NudgeAmount = new Vector2(50, 50);
        public bool HasNudgeVarianceMultiplier = false;
        public Vector2 NudgeVarianceMultiplierMinMax = new Vector2(0.5f, 2);
        private float _nudgeVarianceAmount = 1f;
        public bool NudgesOnHover = true;
        private bool _isHovering;

        public bool AutoNudge = true;
        public Vector2 NudgeFrequencyMinMax = new Vector2(2, 10);
        private float _lastNudgeTime;
        private float _nextNudgeTime;


        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void Update()
        {
            if (AutoNudge == false) return;

            if (Time.time - _lastNudgeTime > _nextNudgeTime)
            {
                Nudge();
            }

        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_isHovering == true) return;

            _isHovering = true;

            if (NudgesOnHover == true)
            {
                Nudge();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isHovering == false) return;

            _isHovering = false;
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (_isHovering == true) return;

            _isHovering = true;

            if (NudgesOnHover == true)
            {
                Nudge();
            }
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (_isHovering == false) return;

            _isHovering = false;
        }

        public void Nudge()
        {
            if (SpringBehaviour == null) return;

            (SpringBehaviour as INudgeable<Vector2>).Nudge(NudgeAmount * _nudgeVarianceAmount);

            if (HasNudgeVarianceMultiplier == true)
            {
                _nudgeVarianceAmount = Random.Range(NudgeVarianceMultiplierMinMax.x, NudgeVarianceMultiplierMinMax.y);
            }

            _lastNudgeTime = Time.time;
            _nextNudgeTime = Random.Range(NudgeFrequencyMinMax.x, NudgeFrequencyMinMax.y);
        }
    }
}
