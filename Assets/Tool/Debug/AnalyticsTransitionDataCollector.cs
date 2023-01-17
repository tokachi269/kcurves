using CamOpr.Tool;
using System;
using UnityEngine;

namespace CamOpr.Tool
{
    public class AnalyticsTransitionDataCollector: MonoBehaviour
    {
        [SerializeField]
        private PathTool instance;
        [SerializeField]
        public Monitoring<BezierTransition> Position;
        [SerializeField]
        public Monitoring<BezierTransition> Rotation;

        void Awake()
        {
            Debug.Log(transform.gameObject.GetComponent<PathTool>().GetHashCode());
            instance = transform.gameObject.GetComponent<PathTool>();
            Debug.Log(instance.GetHashCode());

            Position = new Monitoring<BezierTransition>(instance.posTransition);
            Rotation = new Monitoring<BezierTransition>(instance.posTransition);
            Debug.Log("Position.Subscribe += ValueChanged;前");
            Position.Subscribe += ValueChanged;
        }

        private void ValueChanged(BezierTransition value)
        {
            //if (value != null)
            {
                Debug.Log($"change value : {value}");
            }
        }
    }

    public class Monitoring<T>
    {
        public event Action<T> Subscribe;
        private T _value;
        public Monitoring()
        {
            _value = default;
        }

        public Monitoring(T defaultValue)
        {
            _value = defaultValue;
        }

        public T Value
        {
            get => _value;
            set
            {
                _value = value;
                Subscribe?.Invoke(_value);
                Debug.Log("Subscribe?.Invoke(_value)");
            }
        }
    }

}
