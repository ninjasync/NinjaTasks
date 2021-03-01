//using System;

//namespace com.movisens.smartgattlib
//{

//    using HeartRateMeasurement = com.movisens.smartgattlib.characteristics.HeartRateMeasurement;


//    public class Example
//    {

//        public static void Main(string[] args)
//        {
//            // onConnected
//            // TODO: iterate over available services
//            UUID serviceUuid = null; // service.getUuid();
//            if (Service.HEART_RATE.Equals(serviceUuid))
//            {

//                // TODO: iterate over characteristics
//                UUID characteristicUuid = null; // characteristic.getUuid();
//                if (Characteristic.HEART_RATE_MEASUREMENT.Equals(characteristicUuid))
//                {
//                    // TODO: Enable notification
//                    //BluetoothGattDescriptor descriptor = characteristic.getDescriptor(Descriptor.CLIENT_CHARACTERISTIC_CONFIGURATION);
//                    //descriptor.setValue(BluetoothGattDescriptor.ENABLE_NOTIFICATION_VALUE);
//                    //mBluetoothGatt.writeDescriptor(descriptor);
//                }
//            }
//            else
//            {
//                Console.WriteLine("Found unused Service: " + Service.GetDescription(serviceUuid, "unknown"));
//            }

//            // onCharacteristicChanged
//            UUID characteristicUuid = null; // characteristic.getUuid();
//            if (Characteristic.HEART_RATE_MEASUREMENT.Equals(characteristicUuid))
//            {
//                sbyte[] value = null; // characteristic.getValue();
//                HeartRateMeasurement hrm = new HeartRateMeasurement(value);
//                hrm.Hr;
//                hrm.Ee;
//            }
//        }
//    }

//}