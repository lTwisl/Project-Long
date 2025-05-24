using System.Collections.Generic;
using System.Linq;


namespace StatsModifiers
{
    public class StatsMediator
    {
        private readonly List<StatModifier> _modifiers = new();

        public void PerformQuery(object sender, Query query)
        {
            foreach (StatModifier modifier in _modifiers)
                modifier.Handle(sender, query);

        }

        public void AddModifier(StatModifier modifier)
        {
            _modifiers.Add(modifier);
            modifier.MarkedForRemoval = false;
            modifier.OnDispose += _ => _modifiers.Remove(modifier);
        }

        public void RemoveModifier(StatModifier modifier)
        {
            if (!Contains(modifier))
                return;

            modifier.Dispose();
        }

        public bool Contains(StatModifier modifier)
        {
            return _modifiers.Contains(modifier);
        }

        public void UpdateModifiers(float deltaTime)
        {
            foreach (StatModifier modifier in _modifiers)
                modifier.Update(deltaTime);

            foreach (StatModifier modifier in _modifiers.Where(m => m.MarkedForRemoval).ToList())
                modifier.Dispose();
        }
    }
}
