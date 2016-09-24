namespace com.movisens.smartgattlib.characteristics
{

	public class BatteryLevel
	{
		internal int level = -1;

		public BatteryLevel(byte[] value)
		{
			level = GattByteBuffer.FromArray(value).Uint8();
		}

		/// <returns> The current charge level of a battery in %. 100% represents fully
		///         charged while 0% represents fully discharged. </returns>
		public virtual int getBatteryLevel()
		{
			return level;
		}
	}

}