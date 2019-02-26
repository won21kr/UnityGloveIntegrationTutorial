using System;
using System.Collections;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;
using UnityEngine.Experimental.UIElements;


public class CalibrationAlgorithm
{

    private int TrainingIndex = 0;

    public int CurrentTrainingIndex
    {
        get { return TrainingIndex; }
        set { TrainingIndex = value; }
    }

    private Boolean CaptureCalibrationData = false;
    private Boolean isFirstPress = true;
    public double[] TrainingPositionMinY { get; set; }

    public double[] TrainingPositionMaxY { get; set; }

    private int samplesIndex = 0;
    private static int samplesPerTrainingPosition = 50;

    public bool IsTrainingComplete
    {
        get { return isTrainingComplete; }
        set { isTrainingComplete = value; }
    }

    private Boolean isTrainingComplete = true;

    private int nOutputs;  //I.e. Right Hip, Right Knee, Left Hip, Left Kneey

    private int trainingPositionsCount;
    //X     Y      Z//         
    private double[,] TrainingPositions;
    private int[][] ActiveSensor;
    int[] All = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }; // USE THIS WHEN WE CALCULATE ALL SENSOR DATA FOR ALL JOINTS ELSE MARK 0 AGAINST SENSOR TO IGNORE
    private double[] currentTrainingPosition; // THESE ARE 

    private static int nSamples;
    private int[] LastX = new int[nSensors];

    //Results - to pass to regression fit eq
    private double[,] X;
    private double[,] y;

    private static int nSensors = 10;  //I.e. number of sensor inputs (10 stretchsensors + 9 axis IMU = 19)
    private int index = 0;


    private Vector<double>[] mtheta;



    public CalibrationAlgorithm() {

        
        //HERE WE HAVE DECLARED THE 18 POSITIONS WHICH LOOKS AT 8 JOINTS
        
        //DEFINE TOTAL TRAINING POSITIONS AND THEIR RESPECTIVE ROTATION ANGLES //REFEREING TO THESE ONE BY ONE
        TrainingPositions = new double[,] {
            
//                         0  1  2   3    4    5    6    7
                       { 0, 0, 0}, //open finger
                       {0, -60, -45}, //half open
                       { -90, -90, -90}//closed
                        ,

                };

                //WE CAN USE THIS WHEN WE ARE TELLING SYSTEM WHICH SENSOR IS AT WHAT POSITIONS  EG THUMB IS USING
                //SENSOR ON 4th, 9th & 10th CHANNEL    
//                Thumb   = new int[] { 0, 0, 0, 1, 0, 0, 0, 0, 1, 1 };    
//                Index   = new int[] { 0, 0, 0, 0, 1, 0, 0, 0, 0, 0 };    
//                Middle  = new int[] { 0, 0, 0, 0, 0, 1, 0, 0, 0, 0 };
//                Ring    = new int[] { 0, 0, 0, 0, 0, 0, 1, 0, 0, 0 };
//                Pinky   = new int[] { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0 };

//                  ActiveSensor = new int[][] {
//                      Thumb ,Thumb,Thumb,Thumb,Index,Middle,Ring,Pinky
//                  };
        int[] INDEX = { 0, 0, 0, 0, 1, 0, 0, 0, 0, 0 }; //  INDEX TO ONLY USE INDEX SENSOR AND NOT LOOK AT ALL

                //Define which sensors to use during training   //BY SAYING ALL WE ARE TELLING SYSTEM TO SEE ALL SENSOR FOR EACH POSITIONS   
                ActiveSensor = new int[][] {
                    INDEX,INDEX,INDEX  //HERE WE ARE TELLING SYSTEM TO LOOK AT EACH SENSOR FOR EVERY JOINTS
                };

        TrainingPositionMaxY = getTrainingMax(TrainingPositions);
        TrainingPositionMinY = getTrainingMin(TrainingPositions);
        currentTrainingPosition = getCurrentTrainingPostion(TrainingPositions);

        trainingPositionsCount = TrainingPositions.GetLength(0);
        nOutputs = TrainingPositions.GetLength(1);

        nSamples = samplesPerTrainingPosition * trainingPositionsCount;

        //Results - to pass to regression fit eq
        X = new double[nSamples, nSensors + 1];
        y = new double[nSamples, nOutputs];

    }

  

    private double[] getTrainingMin(double[,] TrainingY) {

        double[] TrainY = new double[TrainingY.GetLength(1)];

        for (int i = 0; i < TrainingY.GetLength(1); i++) {
            TrainY[i] = TrainingY[0,i];
            for (int j = 0; j < TrainingY.GetLength(0); j++)
            {
                if (TrainY[i] > TrainingY[j, i])
                {
                    TrainY[i] = TrainingY[j, i];
                }
            }
        }

        return TrainY;
    }

    private double[] getTrainingMax(double[,] TrainingY)
    {

        double[] TrainY = new double[TrainingY.GetLength(1)];

        for (int i = 0; i < TrainingY.GetLength(1); i++)
        {
            TrainY[i] = TrainingY[0, i];
            for (int j = 0; j < TrainingY.GetLength(0); j++)
            {
                if (TrainY[i] < TrainingY[j, i])
                {
                    TrainY[i] = TrainingY[j, i];
                }
            }
        }

        return TrainY;
    }

    private double[] getCurrentTrainingPostion(double[,] TrainingY) {

        double[] current = new double[TrainingY.GetLength(1)];

        for (int i = 0; i < TrainingY.GetLength(1); i++) {
            current[i] = TrainingY[0,i];
        }

        return current;

    }

    public void startCalibrationRoutine()
    {
        //Require 2 presses to start first capture (TODO determine more stable way to manage first press)
        if (index == 0 && isFirstPress)
        {
            isFirstPress = false;
        }
        else
        {
            CaptureCalibrationData = true;
            Debug.Log("Start calibration");
        }
    }

    public double[,] getTrainingData()
    {
        return TrainingPositions;
    }

    // Update is called once per frame
    public void processRawCapacitance(int[] currentX)
    {

        if (IsTrainingComplete) 
        {
            return;
        }
        /*
        if (currentX == LastX)
        {
            Debug.Log("Repeated value set");
            return;
        }
        else
        {*/
            LastX = currentX;
            //Check if the user has stopped moving
            if (CaptureCalibrationData) //HERE WE ARE COLLECTING SAMPLES ACROSS EACH POSITION
            {

                //Start recording data
                //Include DC offset
                X[index, 0] = 1;
                //Sensor data
                for (int i = 0; i < nSensors; i++)
                {
                    X[index, i + 1] = currentX[i];
                }

                for (int i = 0; i < nOutputs; i++)
                {
                    y[index, i] = currentTrainingPosition[i];
                }
                samplesIndex++; //Increment position samples index
                index++;    //Increment global samples index

                if (samplesIndex >= samplesPerTrainingPosition)
                {
                    //Update to new position
                    moveToNextTrainingPosition();
                    //Reset Flags
                    CaptureCalibrationData = false;
                    //Reset counters
                    samplesIndex = 0;
                }
            }
        //}

    }


    private void moveToNextTrainingPosition()
    {
        //Has training aqusition completed
        if (IsTrainingComplete)
        {
            return;
        }

        //Increment Training position
        TrainingIndex++;

        if (TrainingIndex < trainingPositionsCount)
        {
            for (int i = 0; i < nOutputs; i++)
            {
                currentTrainingPosition[i] = TrainingPositions[TrainingIndex, i];
            }
        }
        else
        {
            performCalculationOnTrainingData(); //ONCE DATA AGAINST EACH POSITION IS CAPTURED WE RUN THE ALGORITHM
        }
        
    }
    
    private void performCalculationOnTrainingData()
    {
        SolveLeastSquares Solver = new SolveLeastSquares();

        IsTrainingComplete = true;
        //Use data to calculate fitting equation

        Solver.setActiveSensors(ActiveSensor);
        Vector<double>[] trainingData = Solver.Solve(X, y);
 
        setTheta(trainingData);
    }

    public double[] ApplyTransformation(int[] input, Vector<double>[] theta)
    {

        SolveLeastSquares Solver = new SolveLeastSquares();
        return Solver.ApplyTransformation(input, theta);
    }

    public void setTheta(Vector<double>[] theta)
    {

        mtheta = theta;

    }

    public Vector<double>[] getTheta()
    {
        return mtheta;
    }


    public double[] getCurrentTrainingPosition()
    {
        //Return current target so UI can be updated
        return currentTrainingPosition;
    }


    public double[,] getX()
    {
        return X;
    }
    public double[,] getY()
    {
        return y;
    }

}