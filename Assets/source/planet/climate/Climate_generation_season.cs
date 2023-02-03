namespace Earthgen.planet.climate
{
	public struct Climate_generation_tile
	{
		public Wind wind;
		public float latitude;
		public float temperature;
		public float humidity;
		public float precipitation;
	}

	public struct Climate_generation_corner
	{
		public float river_flow;
		public float river_flow_increase;
	}

	public struct Climate_generation_edge
	{
		public float river_flow;
		public float wind_velocity;
	}

    public class Climate_generation_season
	{
		public Season_variables var;
		public float tropical_equator;

		public Climate_generation_tile[] tiles;
		public Climate_generation_corner[] corners;
		public Climate_generation_edge[] edges;
	}
}
