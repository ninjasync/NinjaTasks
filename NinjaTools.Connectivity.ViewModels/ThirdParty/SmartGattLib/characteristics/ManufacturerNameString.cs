namespace com.movisens.smartgattlib.characteristics
{

	public class ManufacturerNameString
	{
		internal string content = "";

		public ManufacturerNameString(byte[] value)
		{
			content = GattByteBuffer.FromArray(value).String();
		}

		public virtual string getManufacturerNameString()
		{
			return content;
		}
	}

}