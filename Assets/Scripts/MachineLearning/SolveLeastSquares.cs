using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearRegression;

public class SolveLeastSquares {

    int[][] ActiveSensors;

    //Called after gathering training data
    public Vector<double>[] Solve(double[,] X, double[,] yArray)
    {
        //Solve matrix to generate weights for runtime calibration

        double[] y = new double[yArray.GetLength(0)];
        Vector<double>[] theta = new Vector<double>[yArray.GetLength(1)];

        //Process each collum one by one
        for (int i = 0; i < yArray.GetLength(1); i++)
        {

            //Clone raw data to manipulate
            double[,] Xclone = X.Clone() as double[,];

            //Disable channels not used for this solve
            Xclone = DisableChannels(Xclone, ActiveSensors, i);

            Matrix<double> XMatrix = CreateMatrix.DenseOfArray(Xclone);

            RemoveChannels temp = RemoveEmptyChannels(XMatrix);
            XMatrix = temp.ReducedMatrix;
            //Get a solution vector
            y = getCollumn(yArray, i);
            Matrix<double> tempMat = XMatrix;

            //Perform Regression fit
            Vector<double> yVector = CreateVector.DenseOfArray(y);
            theta[i] = MultipleRegression.Svd<double>(tempMat, yVector);

            //Add removed collumns
            theta[i] = PadArray(temp.usedCol, theta[i]);

        }

        return theta;
    }

    //Called after solving/ generating weightings
    public double[] ApplyTransformation(int[] input, Vector<double>[] theta ) {
        //Apply vector multiplication to generate calibrated output on new data

        double[] result = new double[theta.GetLength(0)];
        for (int i = 0; i < theta.GetLength(0); i++)
        {
            for (int j = 0; j < input.GetLength(0); j++)
            {
                result[i] += input[j] * theta[i].ToArray()[j];   
            }
        }

        return result;
    }

    //Return a double 1d array (vector) from 2d array (matrix)
    private double[] getCollumn(double[,] Array, int index) {

        double[] result = new double[Array.GetLength(0)];
        //Get each data point in each collumn
        for (int j = 0; j < Array.GetLength(0); j++)
        {
            //Extract a single collumn 
            result[j] = Array[j, index];
        }

        return result;
    }

    //Add zeroed channels to align weights with their appropriate value
    private Vector<double> PadArray(bool [] usedCol, Vector<double> theta) {
        //Add removed collumns to array to make later calcs easier
        int index = 0;
        double[] tempArray = new double[usedCol.Length];
        for (int j = 0; j < usedCol.Length; j++)
        {
            if (usedCol[j])
            {
                tempArray[j] = theta.At(index);
                index++;
            }
            else
            {
                tempArray[j] = 0;
            }
        }
        return CreateVector.DenseOfArray(tempArray);

    }

    private RemoveChannels RemoveEmptyChannels(Matrix<double> input) {
        //Scan for and store all non-zero channels (i.e. only use loaded channels)
        //Return an updated matrix with empty channels removed, and a bool array identifying which channels are used.

        double[,] InputArray = input.ToArray();
        bool[] usedCol = new bool[InputArray.GetLength(1)];
        int count = 0;

        for (int i = InputArray.GetLength(1) - 1; i >= 0; i--) {
            if (InputArray[2,i] == 0) {
                usedCol[i] = false;
                input = input.RemoveColumn(i);

            }
            else {
                usedCol[i] = true;
                count++;
            }

        }

        RemoveChannels result = new RemoveChannels();
        result.ReducedMatrix = input;
        result.usedCol = usedCol;
        return result;

    }

    private class RemoveChannels{
        //Class to return Matrix and bool array from Remove channel function 
        public Matrix<double> ReducedMatrix;
        public bool[] usedCol;    

    }

    public void setActiveSensors(int[][] sensorsToUse) {
        ActiveSensors = sensorsToUse;
    }

    private double[,] DisableChannels(double[,] X, int[][] SensorstoUse, int index) {

        //Error catch
        if (SensorstoUse == null) {
            return X;
        }

        //Zero out value if not in use
        for (int i = 0; i < X.GetLength(1)-1; i++) {
            if (SensorstoUse[index][i] == 0) {
                X[2,i+1] = 0;
            }
        }

        return X;
    }
}