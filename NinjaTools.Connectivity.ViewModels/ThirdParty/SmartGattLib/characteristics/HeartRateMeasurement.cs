using System.Collections.Generic;

namespace com.movisens.smartgattlib.characteristics
{
    public enum SensorWorn
    {
        UNSUPPORTED,
        WORN,
        NOT_WORN
    }

	public class HeartRateMeasurement
	{

		internal List<float?> rrIntervals = new List<float?>();
		internal int hrmval = 0;
		internal int eeval = -1;
		internal SensorWorn sensorWorn = SensorWorn.UNSUPPORTED;

		

		public HeartRateMeasurement(byte[] value)
		{
			GattByteBuffer bb = GattByteBuffer.FromArray(value);
			byte flags = bb.Uint8();
			if (isHeartRateInUINT16(flags))
			{
				hrmval = bb.Uint16();
			}
			else
			{
				hrmval = bb.Uint8();
			}
			if (isWornStatusPresent(flags))
			{
				if (isSensorWorn(flags))
				{
					sensorWorn = SensorWorn.WORN;
				}
				else
				{
					sensorWorn = SensorWorn.NOT_WORN;
				}
			}
			if (isEePresent(flags))
			{
				eeval = bb.Uint16();
			}
			if (isRrIntPresent(flags))
			{
				while (bb.HasRemaining())
				{
					rrIntervals.Add(bb.Uint16() / 1024F);
				}
			}
		}

		private bool isHeartRateInUINT16(byte flags)
		{
			if ((flags & GattUtils.FIRST_BITMASK) != 0)
			{
				return true;
			}
			return false;
		}

		private bool isWornStatusPresent(byte flags)
		{
			if ((flags & GattUtils.THIRD_BITMASK) != 0)
			{
				return true;
			}
			return false;
		}

		private bool isSensorWorn(byte flags)
		{
			if ((flags & GattUtils.SECOND_BITMASK) != 0)
			{
				return true;
			}
			return false;
		}

		private bool isEePresent(byte flags)
		{
			if ((flags & GattUtils.FOURTH_BITMASK) != 0)
			{
				return true;
			}
			return false;
		}

		private bool isRrIntPresent(byte flags)
		{
			if ((flags & GattUtils.FIFTH_BITMASK) != 0)
			{
				return true;
			}
			return false;
		}

		/// <returns> RR-Intervals. Units: seconds </returns>
		public virtual List<float?> RrInterval
		{
			get
			{
				return rrIntervals;
			}
		}

		public virtual int Hr
		{
			get
			{
				return hrmval;
			}
		}

		/// <returns> Energy Expended, Units: kilo Joules </returns>
		public virtual int Ee
		{
			get
			{
				return eeval;
			}
		}

		public virtual SensorWorn SensorWorn
		{
			get
			{
				return sensorWorn;
			}
		}
	}

}