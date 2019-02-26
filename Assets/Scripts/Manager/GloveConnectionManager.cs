using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GloveConnectionManager : MonoBehaviour {

	
	
	//THESE ARE OUR GLOVE BLE PROPERTIES USED FOR CONNECTING TO GLOVE
	private String DeviceName               = "StretchSense"; //Device Name
	private String ServiceUUID              = "00001701-7374-7265-7563-6873656e7365";
	private String SensorCharacteristic     = "00001702-7374-7265-7563-6873656e7365";//Sensor Characteristic
	private String FilterCharacteristic     = "00001706-7374-7265-7563-6873656e7365"; //Filtering Characteristic
	
	public Boolean initBluetooth = false;
	
	//List of devices
	public Dictionary<String, Boolean> _peripheralList;
	private Boolean showConnectionPopUp = false;
	private String discoveredDeviceAddress = "";
	private String discoveredDeviceName = "";
	private GloveDataProcessor gloveDataProcessor;

	private int filterValue = 20;

	public GloveDataProcessor getGloveDataProcessor()
	{
		return gloveDataProcessor;
	}
	

	private Boolean isFilterSet;
	
	// Use this for initialization
	void Start () {

		if (!initBluetooth)
		{
			BluetoothLEHardwareInterface.Initialize(true, false, () => { initBluetooth = true; }, (error) => { });

		}
		
		// start scanning after 1 second 
		Invoke("scan", 1f);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	//---------- Scaning for BLE Devices ----------------//

	public void scan()
	{
		// the first callback will only get called the first time this device is seen 
		// this is because it gets added to a list in the BluetoothDeviceScript 
		// after that only the second callback will get called and only if there is 
		// advertising data available 
		BluetoothLEHardwareInterface.Log("SslAPI - Starting scan");
		BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(null,
			(address, name) => { AddPeripheral(name, address); },
			(address, name, rssi, advertisingInfo) => { AddPeripheral(name, address); }, false, false);
	}
	
	
	
	//---------- Adding available BLE Devices to a List ----------------//

	private void AddPeripheral(string name, string address)
	{
		BluetoothLEHardwareInterface.Log("Found: " + name + " Address: " + address);

		//Device discovered that matches requirements
		if (_peripheralList == null) _peripheralList = new Dictionary<string, bool>();

		if (!_peripheralList.ContainsKey(address) || (_peripheralList.ContainsKey(address) && !_peripheralList[address]))
		{
			if (name == DeviceName)
			{
				BluetoothLEHardwareInterface.Log("SslAPI - AddPeripheral to _peripheralList: " + name + " " + address);

				_peripheralList[address] = false;

				//Stop scanning and connect to this device

				if (!showConnectionPopUp && !_peripheralList[address])
				{
					discoveredDeviceName = name;
					discoveredDeviceAddress = address;
					showConnectionPopUp = true;
					BluetoothLEHardwareInterface.StopScan(); //Todo check if this works??
				}
			}
		}
		else
		{
			BluetoothLEHardwareInterface.Log("SslAPI - No address found");
		}
	}
	
	//ONCE WE START & SCAN AND FOUND THE DEVICE WE SHOW USER PROMPT FOR CONNECTION
	private void ShowGUI(int windowID)
	{
		// You may put a label to show a message to the player
		var myButtonStyle = new GUIStyle(GUI.skin.button);
		myButtonStyle.fontSize = 25;

		GUI.Label(new Rect(25, 15, 500, 150),
			"Do you wish to connect to \n" + discoveredDeviceName + "\n" + discoveredDeviceAddress, myButtonStyle);

		// You may put a button to close the pop up too

		var actionButtonStyle = new GUIStyle(GUI.skin.button);
		actionButtonStyle.fontSize = 25;

		if (GUI.Button(new Rect(350, 200, 150, 60), "Ok", actionButtonStyle))
		{
			showConnectionPopUp = false;
			connectBluetooth(discoveredDeviceAddress);
			//Connect to this device
		}

		if (GUI.Button(new Rect(50, 200, 150, 60), "No", actionButtonStyle)) showConnectionPopUp = false;
	}

	private void OnGUI()
	{
		if (showConnectionPopUp)
		{
			var myButtonStyle = new GUIStyle(GUI.skin.button);
			myButtonStyle.fontSize = 25;

			GUI.Window(0, new Rect(Screen.width / 2 - 150, Screen.height / 2 - 75, 550, 300), ShowGUI,
				"Device Discovered", myButtonStyle);
		}
	}
	
	/**+
	 * Method connects with bluetooth device.
	 * 
	 * @addr : Address of the bluetooth device.
	 */
	public void connectBluetooth(string addr)
    {
        BluetoothLEHardwareInterface.ConnectToPeripheral(addr, (address) => { }, (address, serviceUUID) => { },
            (address, serviceUUID, characteristicUUID) =>
            {
	            BluetoothLEHardwareInterface.StopScan(); //Stop scanning

	            // this will get called when the device connects 
                //Update connected control circuit

                    BluetoothLEHardwareInterface.Log("Found  Glove: " + address);
	            gloveDataProcessor = new GloveDataProcessor {GloveUuid = address};

	            _peripheralList[address] = true;
                subscribeToCharacteristic(address, ServiceUUID,
                    SensorCharacteristic); //Enable notifications on sensing characteristic
                //subscribeToCharacteristic(address, ServiceUUID, IMUCharacteristic);    //Enable notifications on sensing characteristic
            }, (address) =>
            {
                // Device on disconnect
                
                 BluetoothLEHardwareInterface.Log("Glove Disconnected : " + address);
	             gloveDataProcessor = null;
                _peripheralList.Remove(address);
		
            });
    }
	
	
	/**
	 * Subscribes to characteristic after connection is establish.
	 *
	 * @address : Address of the device
	 * @serviceUUID : Service UUID of the device
	 * @characteristicUUID : Characteristics of the device offered under given serviceUUID
	 */
	private void subscribeToCharacteristic(string address, string serviceUUID, string characteristicUUID)
    {
        BluetoothLEHardwareInterface.SubscribeCharacteristic(address, serviceUUID, characteristicUUID, null,
            (characteristic, bytes) =>
            {
                if (bytes.Length == 0)
                {
                    // do nothing 
                }
                else
                {
                    if (gloveDataProcessor != null && address == gloveDataProcessor.GloveUuid)
                    {
                        if (!isFilterSet) //Set filtering to remove noise
                        {
                            var filter = (byte)(filterValue);
                            SendByte(address, ServiceUUID, FilterCharacteristic, filter);
                            isFilterSet = true;
                        }
                    }

                    // convert and store the notification into a StretchSense circuit object
                    SSLNotificationReceived(address, characteristicUUID, bytes);
                }
            });
    }
	
	//------------- Writing to Characteristic -------------------//	
	private void SendByte(string address, string serviceUUID, string characteristicUUID, byte value)
	{
		BluetoothLEHardwareInterface.Log("SslAPI - SendByte()");
		var data = new byte[] {value};
		BluetoothLEHardwareInterface.WriteCharacteristic(address, serviceUUID, characteristicUUID, data, data.Length,
			true, (characteristic) => { BluetoothLEHardwareInterface.Log("SslAPI - Write Succeeded " + address); });
	}

	
	//ONCE THE GLOVES ARE CONNECTED WE WILL START RECEIVING DATA 
	//THIS DATA IS PASSED TO GLOVEDATAPROCESSOR
		
	//---------- Converting the Sensor data into capacitances ----------------//

	public void SSLNotificationReceived(string address, string characteristic, byte[] dataBytes)
	{
		if (dataBytes != null)
		{
			//Notificaiton recieved update capacitance values
			var array_capacitance_int = Utility.RandomUtility.convertBytetoArray(dataBytes);

			var capacitance = new float[10];

			for (var index = 0; index < 10; index++)
			{
				capacitance[index] = array_capacitance_int[index] / 10;
                
			}

			if (gloveDataProcessor != null && gloveDataProcessor.GloveUuid == address)
			{
				if (characteristic == SensorCharacteristic)
				{
					gloveDataProcessor.onNotificationUpdate(array_capacitance_int);
				} //Update SSL circuit with new data
			}


		}
	}




}
