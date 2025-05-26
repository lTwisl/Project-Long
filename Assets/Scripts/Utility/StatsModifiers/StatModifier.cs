using System;


namespace StatsModifiers
{
    public class StatModifier : IDisposable
    {
        private readonly ICondition _condition;
        private readonly Func<float, float> _operation;

        public bool MarkedForRemoval = false;

        public event Action<StatModifier> OnDispose = delegate { };

        private float _duration = 0;

        public StatModifier(float duration, ICondition condition, Func<float, float> operation)
        {
            _duration = duration;
            _condition = condition;
            _operation = operation;
        }

        public void Update(float deltaTime)
        {
            if (_duration > 0)
            {
                _duration -= deltaTime;

                if (_duration <= 0)
                    Dispose();
            }
        }


        public void Handle(object sender, Query query)
        {
            if (!_condition.Equals(query.Condition))
                return;

            query.Value = _operation(query.Value);
        }

        public void Dispose()
        {
            MarkedForRemoval = true;
            OnDispose.Invoke(this);
        }
    }
}
