using UnityEngine;
using UnityEngine.Events;

namespace ReactiveEnvironment
{

    public class AutomaticSlider : MonoBehaviour
    {   
        [SerializeField, Min(0.01f)]
        float duration = 1;

        [System.Serializable]
        public class OnValueChangedEvent : UnityEvent<float> { }

        [SerializeField]
        OnValueChangedEvent onValueChanged = default;

        [SerializeField]
        bool autoReverse = false;

        float SmoothedValue => 3f * value * value - 2f * value * value * value;

        public bool AutoReverse {
            get => autoReverse;
            set => autoReverse = value;
        }

        public bool Reversed
        {
            get;set;
        }

        float value;

        private void Awake()
        {
            enabled = false;
        }

        void FixedUpdate()
        {
            float delta = Time.deltaTime / duration;

            if (Reversed)
            {
                value -= delta;

                if (value <= 0)
                {
                    if (autoReverse)
                    {
                        value = Mathf.Min(1, -value);
                        Reversed = false;
                    }
                    else 
                    {
                        value = 0;
                        enabled = false;
                    }
                }
            }
            else 
            {
                value += delta;
                if (value >= 1)
                {
                    if (autoReverse)
                    {
                        value = Mathf.Max(0, 2 - value);
                        Reversed = true;
                    }
                    else 
                    {
                        value = 1;
                        enabled = false;
                    }
                }
            }

            onValueChanged.Invoke(SmoothedValue);
        }
    }
}