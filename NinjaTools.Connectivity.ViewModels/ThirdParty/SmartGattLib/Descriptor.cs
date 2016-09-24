using System;
using System.Collections.Generic;

namespace com.movisens.smartgattlib
{
	public class Descriptor
	{
		public static readonly Guid CHARACTERISTIC_EXTENDED_PROPERTIES = GattUtils.ToGuid(0x2900);
		public static readonly Guid CHARACTERISTIC_USER_DESCRIPTION = GattUtils.ToGuid(0x2901);
		public static readonly Guid CLIENT_CHARACTERISTIC_CONFIGURATION = GattUtils.ToGuid(0x2902);
		public static readonly Guid SERVER_CHARACTERISTIC_CONFIGURATION = GattUtils.ToGuid(0x2903);
		public static readonly Guid CHARACTERISTIC_PRESENTATION_FORMAT = GattUtils.ToGuid(0x2904);
		public static readonly Guid CHARACTERISTIC_AGGREGATE_FORMAT = GattUtils.ToGuid(0x2905);
		public static readonly Guid VALID_RANGE = GattUtils.ToGuid(0x2906);
		public static readonly Guid EXTERNAL_REPORT_REFERENCE = GattUtils.ToGuid(0x2907);
		public static readonly Guid REPORT_REFERENCE = GattUtils.ToGuid(0x2908);
		public static readonly Guid NUMBER_OF_DIGITALS = GattUtils.ToGuid(0x2909);
		public static readonly Guid TRIGGER_SETTING = GattUtils.ToGuid(0x290A);
		public static readonly Guid TEST_COMPLEX_BITFIELD = GattUtils.ToGuid(0);

		private static Dictionary<Guid, string> attributes = new Dictionary<Guid, string>();
		static Descriptor()
		{
			attributes[CHARACTERISTIC_EXTENDED_PROPERTIES] = "Characteristic Extended Properties";
			attributes[CHARACTERISTIC_USER_DESCRIPTION] = "Characteristic User Description";
			attributes[CLIENT_CHARACTERISTIC_CONFIGURATION] = "Client Characteristic Configuration";
			attributes[SERVER_CHARACTERISTIC_CONFIGURATION] = "Server Characteristic Configuration";
			attributes[CHARACTERISTIC_PRESENTATION_FORMAT] = "Characteristic Presentation Format";
			attributes[CHARACTERISTIC_AGGREGATE_FORMAT] = "Characteristic Aggregate Format";
			attributes[VALID_RANGE] = "Valid Range";
			attributes[EXTERNAL_REPORT_REFERENCE] = "External Report Reference";
			attributes[REPORT_REFERENCE] = "Report Reference";
			attributes[NUMBER_OF_DIGITALS] = "Number of Digitals";
			attributes[TRIGGER_SETTING] = "Trigger Setting";
			attributes[TEST_COMPLEX_BITFIELD] = "Test Complex BitField";
		}

        public static string GetDescription(Guid guid, string defaultName)
        {
            if (attributes.ContainsKey(guid))
                return attributes[guid];
            return defaultName;
        }
	}
}