namespace MalbersAnimations
{
    public static class Int_ID
    {
        /// <summary>Any Ability Can be Activated = 0 </summary>
        public readonly static int Available = 0;
        /// <summary>The Ability is Interrupted = -2 </summary>
        public readonly static int Interrupted = -2;
        /// <summary>The Ability is Loopable = -1 </summary>
        public readonly static int Loop = -1;
        /// <summary>The Ability is Play one Time only = 1 </summary>
        public readonly static int OneTime = 1;
        /// <summary>Status of the States Allow Exit = 1 </summary>
        public readonly static int AllowExit = 1;
    }

    public static class StateEnum
    {
        /// <summary>States ID for Idle: 0</summary>
        public readonly static int Idle = 0;
        /// <summary>States ID for Locomotion: 1</summary>
        public readonly static int Locomotion = 1;
        /// <summary>States ID for Jump: 2</summary>
        public readonly static int Jump = 2;
        /// <summary>States ID for Fall: 3</summary>
        public readonly static int Fall = 3;
        /// <summary>States ID for Swim: 4</summary>
        public readonly static int Swim = 4;
        /// <summary>States ID for UndweWater: 5</summary>
        public readonly static int UnderWater = 5;
        /// <summary>States ID for Fly: 6</summary>
        public readonly static int Fly = 6;
        /// <summary>States ID for Climb: 7</summary>
        public readonly static int Climb = 7;
        /// <summary>States ID for LedgeGrab: 8</summary>
        public readonly static int LedgeGrab = 8;
        /// <summary>States ID for Slide: 9</summary>
        public readonly static int Slide = 9;
        /// <summary>States ID for Death: 10</summary>
        public readonly static int Death = 10;
        /// <summary>States ID for Glide: 11</summary>
        public readonly static int Glide = 11;
        /// <summary>States ID for WallRun: 12</summary>
        public readonly static int WallRun = 12;
        /// <summary>States ID for WallRun Vertical: 15</summary>
        public readonly static int WallRunVertical = 12;
    }
}