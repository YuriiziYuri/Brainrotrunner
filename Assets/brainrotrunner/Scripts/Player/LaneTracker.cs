using BrainrotRunner.Core;
using UnityEngine;

namespace BrainrotRunner.Player
{
    ///0 = ліва, 1 = центр, 2 = права
    public sealed class LaneTracker
    {
        public const int LaneCount = 3;
        public const int CenterLane = 1;

        private readonly GameTuning _tuning;

        public LaneTracker(GameTuning tuning) => _tuning = tuning;

        public int CurrentLane { get; private set; } = CenterLane;

        public float LaneToX(int lane) => (lane - CenterLane) * _tuning.laneWidth;

        public float CurrentX => LaneToX(CurrentLane);

        public bool TryShift(int direction)
        {
            int target = Mathf.Clamp(CurrentLane + direction, 0, LaneCount - 1);
            if (target == CurrentLane)
                return false;

            CurrentLane = target;
            return true;
        }

        public void Reset() => CurrentLane = CenterLane;
    }
}
