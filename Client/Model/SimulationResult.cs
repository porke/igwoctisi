namespace Client.Model
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Packet being received in RoundEnd message.
    /// </summary>
    [DataContract]
    public class SimulationResult
    {
        /// <summary>
        /// Owner of move.
        /// </summary>
        public string Player;

        /// <summary>
        /// Planet id.
        /// </summary>
        [DataMember]
        public int SourceId;

        /// <summary>
        /// Planet id.
        /// </summary>
        [DataMember]
        public int TargetId;

        [DataMember]
        public int FleetCount;

        [DataMember]
        public MoveType Type;

        [DataMember]
        public int SourceLeft;

        [DataMember]
        public int TargetLeft;

        [DataMember]
        public bool TargetOwnerChanged;

        /// <summary>
        /// Player username.
        /// </summary>
        [DataMember]
        public string TargetOwner;

        [DataMember]
        public int AttackerLosses;

        [DataMember]
        public int DefenderLosses;

        
        public int AttackerFleetsBack
        {
            get { return FleetCount - AttackerLosses; }
        }


        public enum MoveType
        {
            Attack,
            Move,
            Deploy
        }
    }
}
