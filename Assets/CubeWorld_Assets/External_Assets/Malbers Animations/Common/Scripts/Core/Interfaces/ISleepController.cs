namespace MalbersAnimations
{
    /// <summary> Used to put to sleep Character Controllers</summary>
    public interface ISleepController
    {
        bool Sleep { get; set; }

        bool Grounded { get; set; }
    }

    /// <summary>  Locks the character with no movement and no action inputs. Lock Input, Lock Movement</summary>
    public interface ILockCharacter
    {
        void Lock(bool value);
    }
}