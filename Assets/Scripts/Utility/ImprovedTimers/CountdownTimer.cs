using UnityEngine;

namespace ImprovedTimers
{
    public class CountdownTimer : Timer
    {
        public override bool IsFinished => CurrentTime <= 0;

        public CountdownTimer(float initialTime) : base(initialTime) { }

        public override void Tick()
        {
            if (IsRunning)
            {
                if (CurrentTime > 0)
                    CurrentTime -= Time.deltaTime;
                else
                    Stop();
            }
        }
    }
}
