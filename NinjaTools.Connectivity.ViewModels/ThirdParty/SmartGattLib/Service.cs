using System;
using System.Collections.Generic;

namespace com.movisens.smartgattlib
{
	public class Service
	{
	    public static readonly Guid ALERT_NOTIFICATION_SERVICE = GattUtils.ToGuid(0x1811);
	    public static readonly Guid BATTERY_SERVICE = GattUtils.ToGuid(0x180F);
	    public static readonly Guid BLOOD_PRESSURE = GattUtils.ToGuid(0x1810);
		public static readonly Guid CURRENT_TIME_SERVICE = GattUtils.ToGuid(0x1805);
		public static readonly Guid CYCLING_POWER = GattUtils.ToGuid(0x1818);
		public static readonly Guid CYCLING_SPEED_AND_CADENCE = GattUtils.ToGuid(0x1816);
		public static readonly Guid DEVICE_INFORMATION = GattUtils.ToGuid(0x180A);
		public static readonly Guid GENERIC_ACCESS = GattUtils.ToGuid(0x1800);
		public static readonly Guid GENERIC_ATTRIBUTE = GattUtils.ToGuid(0x1801);
		public static readonly Guid GLUCOSE = GattUtils.ToGuid(0x1808);
		public static readonly Guid HEALTH_THERMOMETER = GattUtils.ToGuid(0x1809);
		public static readonly Guid HEART_RATE = GattUtils.ToGuid(0x180D);
		public static readonly Guid HUMAN_INTERFACE_DEVICE = GattUtils.ToGuid(0x1812);
		public static readonly Guid IMMEDIATE_ALERT = GattUtils.ToGuid(0x1802);
		public static readonly Guid LINK_LOSS = GattUtils.ToGuid(0x1803);
		public static readonly Guid LOCATION_AND_NAVIGATION = GattUtils.ToGuid(0x1819);
		public static readonly Guid NEXT_DST_CHANGE_SERVICE = GattUtils.ToGuid(0x1807);
		public static readonly Guid PHONE_ALERT_STATUS_SERVICE = GattUtils.ToGuid(0x180E);
		public static readonly Guid REFERENCE_TIME_UPDATE_SERVICE = GattUtils.ToGuid(0x1806);
		public static readonly Guid RUNNING_SPEED_AND_CADENCE = GattUtils.ToGuid(0x1814);
		public static readonly Guid SCAN_PARAMETERS = GattUtils.ToGuid(0x1813);
		public static readonly Guid TX_POWER = GattUtils.ToGuid(0x1804);
		public static readonly Guid AUTOMATION_IO = GattUtils.ToGuid(0x1815);
		public static readonly Guid BATTERY_SERVICE_1_1 = GattUtils.ToGuid(0x180F);
		public static readonly Guid IMMEDIATE_ALERT_SERVICE_1_1 = GattUtils.ToGuid(0x1802);
		public static readonly Guid LINK_LOSS_SERVICE_1_1 = GattUtils.ToGuid(0x1803);
		public static readonly Guid NETWORK_AVAILABILITY_SERVICE = GattUtils.ToGuid(0x180B);
		public static readonly Guid TX_POWER_SERVICE_1_1 = GattUtils.ToGuid(0x1804);

		private static Dictionary<Guid, string> attributes = new Dictionary<Guid, string>();
		static Service()
		{
			attributes[ALERT_NOTIFICATION_SERVICE] = "Alert Notification Service";
			attributes[BATTERY_SERVICE] = "Battery Service";
			attributes[BLOOD_PRESSURE] = "Blood Pressure";
			attributes[CURRENT_TIME_SERVICE] = "Current Time Service";
			attributes[CYCLING_POWER] = "Cycling Power";
			attributes[CYCLING_SPEED_AND_CADENCE] = "Cycling Speed and Cadence";
			attributes[DEVICE_INFORMATION] = "Device Information";
			attributes[GENERIC_ACCESS] = "Generic Access";
			attributes[GENERIC_ATTRIBUTE] = "Generic Attribute";
			attributes[GLUCOSE] = "Glucose";
			attributes[HEALTH_THERMOMETER] = "Health Thermometer";
			attributes[HEART_RATE] = "Heart Rate";
			attributes[HUMAN_INTERFACE_DEVICE] = "Human Interface Device";
			attributes[IMMEDIATE_ALERT] = "Immediate Alert";
			attributes[LINK_LOSS] = "Link Loss";
			attributes[LOCATION_AND_NAVIGATION] = "Location and Navigation";
			attributes[NEXT_DST_CHANGE_SERVICE] = "Next DST Change Service";
			attributes[PHONE_ALERT_STATUS_SERVICE] = "Phone Alert Status Service";
			attributes[REFERENCE_TIME_UPDATE_SERVICE] = "Reference Time Update Service";
			attributes[RUNNING_SPEED_AND_CADENCE] = "Running Speed and Cadence";
			attributes[SCAN_PARAMETERS] = "Scan Parameters";
			attributes[TX_POWER] = "Tx Power";
			attributes[AUTOMATION_IO] = "Automation IO";
			attributes[BATTERY_SERVICE_1_1] = "Battery Service v1.1";
			attributes[IMMEDIATE_ALERT_SERVICE_1_1] = "Immediate Alert Service 1.1";
			attributes[LINK_LOSS_SERVICE_1_1] = "Link Loss Service 1.1";
			attributes[NETWORK_AVAILABILITY_SERVICE] = "Network Availability Service";
			attributes[TX_POWER_SERVICE_1_1] = "Tx Power Service 1.1";
		}

		public static string GetDescription(Guid guid, string defaultName)
		{
		    if (attributes.ContainsKey(guid))
		        return attributes[guid];
		    return defaultName;
		}
	}
}