namespace Client.Model
{
	using System.Linq;
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
        [DataMember]
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
            get { return Type == MoveType.Attack ? FleetCount - AttackerLosses : 0; }
        }

        public enum MoveType
        {
            Attack,
            Move,
            Deploy
        }

		/// <summary>
		/// Should the client player see this result's animation?
		/// </summary>
		/// <param name="scene"></param>
		/// <returns></returns>
		internal bool ShouldPlayerSeeAnimation(Scene scene)
		{
			var clientPlayer = scene.ClientPlayer;

			// Check whether it's current player's order
			if (Player.Equals(clientPlayer.Username))
				return true;

			// Check for deployment, move and attack
			if (clientPlayer.OwnedPlanets.Any(
				p => p.Id == TargetId || p.NeighbourPlanets.Any(np => np.Id == TargetId)))
			{
				return true;
			}

			return false;
		}
	}
}
