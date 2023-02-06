using System;

namespace Earthgen.planet.climate
{
	[Serializable]
    public struct Wind
	{
		public float directionInDegrees;
		public AngleFloat direction
		{
			get => AngleFloat.FromDegrees(directionInDegrees);
			set => directionInDegrees = value.Degrees;
		}
		public float speed;
	}
}
