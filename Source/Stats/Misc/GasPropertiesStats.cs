using System;
using RimWorld;

namespace InGameDefEditor.Stats.Misc
{
	[Serializable]
	public class GasPropertiesStats
	{
		public MinMaxFloatStats expireSeconds;
		public float rotationSpeed;

		public GasPropertiesStats() { }
		public GasPropertiesStats(GasProperties p)
		{
			this.expireSeconds = new MinMaxFloatStats(p.expireSeconds);
			this.rotationSpeed = p.rotationSpeed;
		}

		public void ApplyStats(GasProperties p)
		{
			p.expireSeconds = this.expireSeconds.ToFloatRange();
			p.rotationSpeed = this.rotationSpeed;
		}

		public override bool Equals(object obj)
		{
			if (obj != null && 
				obj is GasProperties p)
			{
				return
					object.Equals(this.expireSeconds, p.expireSeconds) &&
					this.rotationSpeed == p.rotationSpeed;
			}
			return false;
		}

		public override string ToString()
		{
			return 
				"GasPropertiesStats" +
				"\nexpireSeconds: " + expireSeconds +
				"\nrotationSpeed: " + rotationSpeed;
		}

		public override int GetHashCode()
		{
			return this.ToString().GetHashCode();
		}
	}
}
