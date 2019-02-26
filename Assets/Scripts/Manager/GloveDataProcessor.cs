using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GloveDataProcessor
{

	private String gloveUUID;

	private Boolean isCalibrating;

	public GloveDataProcessor()
	{
		if (calibrationAlgorithm == null)
		{
			BluetoothLEHardwareInterface.Log("Glove address : " + GloveUuid);

			calibrationAlgorithm = new CalibrationAlgorithm();
		}
	}

	public bool IsCalibrating
	{
		get {
			if (calibrationAlgorithm != null)
			{
				return !calibrationAlgorithm.IsTrainingComplete;
			}
			else
			{
				return isCalibrating;
			}
			
		}
		set { isCalibrating = value; }
	}

	public string GloveUuid
	{
		get { return gloveUUID; }
		set { gloveUUID = value; }
	}
	
	private int[] array_capacitance_raw = new int[10];
	private CalibrationAlgorithm calibrationAlgorithm;
	
	
	
	//THIS IS WHERE GLOVE DATA IS RECEIVED
	//Alternate Notification handler
	public void onNotificationUpdate(int[] array_capacitance_int)
	{ 
		for (int i = 0; i < array_capacitance_int.Length; i++) {
			//Load data to array capacitance raw
			array_capacitance_raw[i] = array_capacitance_int[i];
		}

		//Update training algorthm
		calibrationAlgorithm.processRawCapacitance(array_capacitance_raw); //IF CALIBRATION IS STARTED THE DATA IS SEND TO CALIBRATIONROUTINE CLASS FOR PROCESSING
	}

	public void startCalibration()
	{
		BluetoothLEHardwareInterface.Log("Start Calibrating");
		calibrationAlgorithm.CurrentTrainingIndex = 0;
		calibrationAlgorithm.IsTrainingComplete = false;
		IsCalibrating = true;
	}
	
	public double[] getCurrentTrainingPosition()
	{
		return calibrationAlgorithm.getCurrentTrainingPosition();
	}

	public void captureDataAgainstTrainingPosition()
	{
		calibrationAlgorithm.startCalibrationRoutine();
	}
	
	public double[] getCalibratedData() //TAKES THE CURRENT RAW CAPACITANCE AND CALCULATE THE ACTUAL COORDINATE DATA AND RETURN TO GLOVE CONTROLLER
	{


		if (calibrationAlgorithm.getTheta() == null)
		{
			return new Double[]{};
		}
        
		int[] DCbiased = new int[array_capacitance_raw.Length + 1];
		DCbiased[0] = 1;

		for (int index = 0; index < array_capacitance_raw.Length; index++)
		{
			DCbiased[index + 1] = array_capacitance_raw[index];
		}

		//Retrieve weightings (theta) from training algo and multiply against current capacitance data
		double[] Position = calibrationAlgorithm.ApplyTransformation(DCbiased, calibrationAlgorithm.getTheta());
		//Set range limits
		for (int index = 0; index < Position.Length; index++)
		{
			if (Position[index] < calibrationAlgorithm.TrainingPositionMinY[index])
			{
				Position[index] = calibrationAlgorithm.TrainingPositionMinY[index];
			}
			else if (Position[index] > calibrationAlgorithm.TrainingPositionMaxY[index])
			{
				Position[index] = calibrationAlgorithm.TrainingPositionMaxY[index];
			}
		}

		return Position;

	}
	
}
