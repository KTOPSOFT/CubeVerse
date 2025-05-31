using MalbersAnimations.Reactions;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.VargrMultiplayer
{
    [AddComponentMenu("Malbers/Multiplayer/NetDamageable")]
    [RequireComponent(typeof(StatsSync))]
    public class NetDamageable : MDamageable
    {
        protected StatsSync m_statsSync;
        protected void Awake()
        {
            m_statsSync = GetComponent<StatsSync>();
        }
        
        //-*********************************************************************--
        /// <summary>  Main Receive Damage Method!!! </summary>
        /// <param name="Direction">The Direction the Damage is coming from</param>
        /// <param name="Damager">Game Object doing the Damage</param>
        /// <param name="damage">Stat Modifier containing the Stat ID, what to modify and the Value to modify</param>
        /// <param name="isCritical">is the Damage Critical?</param>
        /// <param name="react">Does the Damage that is coming has a Custom Reaction? </param>
        /// <param name="customReaction">The Attacker Brings a custom Reaction to override the Default one</param>
        /// <param name="pureDamage">Pure damage means that the multipliers wont be applied</param>
        /// <param name="element"></param>
        public override void ReceiveDamage(Vector3 Direction, Vector3 Position, GameObject Damager, StatModifier damage,
            bool isCritical, bool react, Reaction customReaction, bool pureDamage, StatElement element)
        {
            if (!enabled) return;       //This makes the Animal Immortal.

            if(m_statsSync.isServer)
            {
                base.ReceiveDamage(Direction, Position, Damager, damage, isCritical, react, customReaction, pureDamage, element);
                return;
            }

            if(!m_statsSync.isController) return;

            HitDirection = Direction;   //Store the Last Direction
            HitPosition = Position;   //Store the Last Position

            var stat = stats.Stat_Get(damage.ID);
            if (stat == null || !stat.Active || stat.IsEmpty || stat.IsImmune) return; //Do nothing if the stat is empty, null or disabled
            Debug.Log("Applying REACT to hit.");
            ReactionLogic(isCritical, react, customReaction);
        }
    }
    #if UNITY_EDITOR
    [CustomEditor(typeof(NetDamageable))]
    public class NetDamageableEditor : MDamageableEditor
    {
    
    }
    #endif
}