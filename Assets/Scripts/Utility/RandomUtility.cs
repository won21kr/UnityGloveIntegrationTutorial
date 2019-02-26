namespace Utility
{
    public class RandomUtility
    {
        public static int[] convertBytetoArray(byte[] dataBytes) {
            int index = 0;
            int bufferSize = dataBytes.Length;
            string[] arr_string_hex_data = new string[bufferSize];              // Array of the raw packets received from the notification 
            string[] array_capacitance_string = new string[bufferSize / 2];     // Array of the hex values of capacitance
            int[] array_capacitance_int = new int[bufferSize / 2];              // Array of the int values of capacitance (Multiply by 0.1 to have pF)

            // Store each packet from the notification in an array of string
            foreach (var packetValue in dataBytes)
            {
                arr_string_hex_data[index] = packetValue.ToString("x2");
                index++;
            }

            // Convert and store the capacitance as integer in an array
            for (int i = 0; i < bufferSize / 2; i++)
            {
                // Cast 2 by 2 the hex packets
                array_capacitance_string[i] = arr_string_hex_data[i * 2] + arr_string_hex_data[i * 2 + 1];
                // Convert the hex to decimal
                array_capacitance_int[i] = int.Parse(array_capacitance_string[i], System.Globalization.NumberStyles.HexNumber);  //TODO check that there is not an issue the LSB casting (i.e. is 0x0A = 0A or A) would effect cap values
            }

            return array_capacitance_int;
        }
    }
}