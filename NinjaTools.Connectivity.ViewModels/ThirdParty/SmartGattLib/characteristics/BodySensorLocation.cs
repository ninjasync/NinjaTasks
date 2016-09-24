
namespace com.movisens.smartgattlib.characteristics
{
    public enum BodyLocation
    {
        Other, Chest, Wrist, Finger, Hand, EarLobe, Foot
    }

	public class BodySensorLocation
	{
	    public BodyLocation Location { get; private set; }

	    public BodySensorLocation(byte[] value)
		{
			int loc = GattByteBuffer.FromArray(value).Uint8();

			switch (loc)
			{
			case 0:
                    Location = BodyLocation.Other;
				break;
			case 1:
                Location = BodyLocation.Chest;
				break;
			case 2:
                Location = BodyLocation.Wrist;
				break;
			case 3:
                Location = BodyLocation.Finger;
				break;
			case 4:
                Location = BodyLocation.Hand;
				break;
			case 5:
                Location = BodyLocation.EarLobe;
				break;
			case 6:
                Location = BodyLocation.Foot;
				break;
			}
		}
	}

}