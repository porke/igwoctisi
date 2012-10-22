namespace Client.View.Play
{
    using System;
    using Client.Model;

    class SelectPlanetArgs : EventArgs
    {
        public Planet Planet { get; private set; }

        public SelectPlanetArgs(Planet selectedPlanet)
        {
            Planet = selectedPlanet;
        }
    }
}
