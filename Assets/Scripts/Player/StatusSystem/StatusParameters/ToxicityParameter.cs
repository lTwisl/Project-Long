using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ToxicityParameter : PlayerParameter
{
    [System.Serializable]
    struct LevelToxicity
    {
        public int level;
        public int countPoints;
        public ScriptableObject Mutations;
    }

    [SerializeField] List<LevelToxicity> _levels;
    public int currentLevel;

    public override void Initialize()
    {
        base.Initialize();
        Current = 0;
    }

    public override void ChangeCurrent(float deltaTime)
    {
        base.ChangeCurrent(deltaTime);

        /*if (Current < _levels[currentLevel].countPoints)
            Current = _levels[currentLevel].countPoints;

        if (currentLevel + 1 < _levels.Count && Current > _levels[currentLevel + 1].countPoints)
            currentLevel++;*/
    }

    public void SetBaseChangeRate(float rate)
        => BaseChangeRate = rate;
}

