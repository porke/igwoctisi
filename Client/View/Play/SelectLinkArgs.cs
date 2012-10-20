namespace Client.View.Play
{
	using System;
	using Client.Model;

	class SelectLinkArgs : EventArgs
	{
		public PlanetLink Link { get; private set; }

		public SelectLinkArgs(PlanetLink selectedLink)
		{
			Link = selectedLink;
		}
	}
}
