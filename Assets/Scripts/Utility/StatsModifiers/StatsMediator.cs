using System.Collections.Generic;


namespace StatsModifiers
{
    public class StatsMediator<T>
    {
        private readonly List<StatModifier<T>> _modifiers = new();

        public void PerformQuery(object sender, Query<T> query)
        {
            for (int i = 0; i < _modifiers.Count; ++i)
                _modifiers[i].Handle(sender, query);
        }

        public void AddModifier(StatModifier<T> modifier)
        {
            _modifiers.Add(modifier);
            modifier.MarkedForRemoval = false;
            modifier.OnDispose += _ => _modifiers.Remove(modifier);
        }

        public void RemoveModifier(StatModifier<T> modifier)
        {
            if (!Contains(modifier))
                return;

            modifier.Dispose();
        }

        public bool Contains(StatModifier<T> modifier)
        {
            return _modifiers.Contains(modifier);
        }

        public void UpdateModifiers(float deltaTime)
        {
            for (int i = _modifiers.Count - 1; i >= 0; --i)
            {
                _modifiers[i].Update(deltaTime);
                if (_modifiers[i].MarkedForRemoval)
                    _modifiers[i].Dispose();
            }
        }
    }
}
