namespace MalbersAnimations
{
    /// <summary>  Create a Range with a max and a min float value   </summary>
    [System.Serializable]
    public struct RangedFloat
    {
        public float minValue;
        public float maxValue;

        public float Min { get => minValue; set => minValue = value; }
        public float Max { get => maxValue; set => maxValue = value; }
        public readonly float Difference => maxValue - minValue;

        public RangedFloat(float minValue, float maxValue)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        /// <summary>  Returns a RandomValue between Min and Max </summary>
        public readonly float RandomValue => UnityEngine.Random.Range(minValue, maxValue);

        /// <summary>Is the value in between the min and max of the FloatRange </summary>
        public readonly bool IsInRange(float value) => value >= minValue && value <= maxValue;
    }
}