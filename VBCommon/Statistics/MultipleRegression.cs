﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Accord.Statistics;
using Accord.Statistics.Models.Regression.Linear;
using Accord.Math;
using Accord.Math.Decompositions;
//using Extreme.Statistics;
//using Extreme.Mathematics.LinearAlgebra;
//using Extreme.Statistics.Tests;

namespace VBCommon.Statistics
{
    public class MultipleRegression
    {
        private MultipleLinearRegression _model;

        //private VariableCollection _data = null;
        private DataTable _dataTable = null;
        private string _dependentVar = "";
        private string[] _independentVars = null;
        private double _adjR2;
        private double _R2;
        private double _AIC;
        private double _AICC;
        private double _BIC;
        private double _Press;
        private double _RMSE;

        //private Dictionary<string, double> _parameters = null;
        private double[] _studentizedResiduals = null;
        private double[] _dffits = null;
        private double[] _cooks = null;
        private DataTable _parameters = null;
        private double[] _predictedValues = null;
        private double[] _observedValues = null;

        private double[] arrOutputData = null;
        private double[][] arrInputData = null;
        private string strOutputName;
        private string strInputName;

        private Dictionary<string, double> _VIF = null;
        private double _maxVIF = 0;
        private string _maxVIFParameter = "";

        private double _ADresidPvalue = double.NaN;
        private double _ADresidNormStatVal = double.NaN;

        private double _WSresidPvalue = double.NaN;
        private double _WSresidNormStatVal = double.NaN;

        public MultipleRegression(DataTable dataTable, string dependentVariable, string[] independentVariables)
        {
            _dataTable = dataTable;
            strOutputName = _dependentVar = dependentVariable;
            _independentVars = independentVariables;

            arrOutputData = dataTable.Columns[dependentVariable].ToArray();
            arrInputData = new double[dataTable.Rows.Count][];

            Dictionary<int, int> dictColMap = new Dictionary<int, int>();
            for (int k = 0; k < independentVariables.Length; k++)
            {
                for (int j = 0; j < dataTable.Columns.Count; j++)
                    {
                        if (dataTable.Columns[j].Caption == independentVariables[k])
                            dictColMap.Add(k,j);
                    }
            }

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                double[] temp = new double[independentVariables.Length];
                for (int k = 0; k < independentVariables.Length; k++)
                {
                    temp[k] = Convert.ToDouble(dataTable.Rows[i].ItemArray[dictColMap[k]]);
                }
                arrInputData[i] = temp;
            }
            //_data = new VariableCollection(dataTable);
        }


        public MultipleRegression(double[] OutputData, double[][] InputData, string OutputName = "", string InputName = "")
        {
            arrOutputData = OutputData;
            arrInputData = InputData;

            strOutputName = OutputName;
            strInputName = InputName;

            _independentVars = new string[1] { InputName };

            //_model = new MultipleRegression(
        }


        public MultipleRegression(double[] OutputData, double[] InputData, string OutputName = "", string InputName = "")
        {
            //Set up the DataTable with two columns:
            _dataTable = new DataTable();
            _dataTable.Columns.Add(OutputName, typeof(double));
            _dataTable.Columns.Add(InputName, typeof(double));

            arrOutputData = OutputData;
            arrInputData = new double[InputData.Length][];
            for (int i = 0; i < InputData.Length; i++)
            {
                arrInputData[i] = new double[] { InputData[i] };
                _dataTable.LoadDataRow(new object[] { OutputData[i], InputData[i] }, true);
            }

            strOutputName = OutputName;
            strInputName = InputName;

            _independentVars = new string[1] { InputName };
            strOutputName = _dependentVar = OutputName;

            //_model = new MultipleRegression(new DataTable(), OutputName, _independentVars);
        }


        public double R2
        {
            get { return _R2; }
        }

        public double AdjustedR2
        {
            get { return _adjR2; }
        }

        public double AIC
        {
            get { return _AIC; }
        }

        public double AICC
        {
            get { return _AICC; }
        }

        public double BIC
        {
            get { return _BIC; }
        }

        public double Press
        {
            get { return _Press; }
        }

        public double RMSE
        {
            get { return _RMSE; }
        }

        public double[] DFFITS
        {
            get { return _dffits; }
        }

        public double[] Cooks
        {
            get { return _cooks; }
        }

        public double[] StudentizedResiduals
        {
            get { return _studentizedResiduals; }
        }

        public DataTable Parameters
        {
            get { return _parameters; }
        }

        public double[] PredictedValues
        {
            get { return _predictedValues; }
        }

        public double[] ObservedValues
        {
            get { return _observedValues; }
        }

        public double ADResidPvalue 
        {
            get { return _ADresidPvalue; }
        }

        public double ADResidNormStatVal
        {
            get { return _ADresidNormStatVal; }
        }

        public double WSResidPvalue
        {
            get { return _WSresidPvalue; }
        }

        public double WSResidNormStatVal
        {
            get { return _WSresidNormStatVal; }
        }

        //public Dictionary<string, double> VIFs
        //{
        //    get { return _VIF; }
        //}

        public double MaxVIF
        {
            get { return _maxVIF; }
        }

        public void Compute()
        {
            // Now create the regression model. Parameters are the name 
            // of the dependent variable, a string array containing 
            // the names of the independent variables, and the VariableCollection
            // containing all variables.
            double[][] input = new double[arrInputData.Length][];
            double[][] input2 = new double[arrInputData.Length][];
            double[,] inputMat = new double[arrInputData.Length, arrInputData[0].Length+1];
            for(int i=0; i<arrInputData.Length; i++)
            {
                List<double> datarow = new List<double> {1};
                datarow.AddRange(arrInputData[i].ToList());
                input[i] = datarow.ToArray();
                input2[i] = arrInputData[i];

                inputMat[i, 0] = 1;
                for (int j = 1; j <= arrInputData[0].Length; j++)
                {
                    inputMat[i,j] = arrInputData[i][j - 1];
                }
            }

            //Make sure the intercept appears first in the list of results:
            List<string> inputNames = new List<string>();
            inputNames.Add("(Intercept)");
            foreach (string varname in _independentVars)
            {
                inputNames.Add(varname);
            }

            Accord.Statistics.Analysis.MultipleLinearRegressionAnalysis M2 = new Accord.Statistics.Analysis.MultipleLinearRegressionAnalysis(inputs: input, outputs: arrOutputData, inputNames: inputNames.ToArray(), outputName: strOutputName, intercept: false);
            _model = new MultipleLinearRegression(inputs:arrInputData[0].Length, intercept:true);

            // The Compute method performs the actual regression analysis.
            M2.Compute();
            _model.Regress(inputs: input2, outputs: arrOutputData);
                        
            _adjR2 = M2.RSquareAdjusted;
            _R2 = M2.RSquared;
            _RMSE = Math.Sqrt(M2.Table[M2.Table.Count - 2].MeanSquares);
            
            //Calculate the selection criteria
            double sse = M2.Table[M2.Table.Count-2].SumOfSquares;            
            int n = Convert.ToInt32(M2.Results.Length);
            double p = M2.CoefficientValues.Length;
            double[,] H = inputMat.Multiply((inputMat.Transpose().Multiply(inputMat)).Inverse().Multiply(inputMat.Transpose()));
            
            double[] SquaredResiduals = new double[M2.Results.Length];
            double[] Residuals = new double[M2.Results.Length];
            double[] Leverage = new double[M2.Results.Length];
            for (int i=0; i<M2.Results.Length; i++)
            {
                SquaredResiduals[i] = Math.Pow(M2.Outputs[i] - M2.Results[i], 2);
                Residuals[i] = M2.Outputs[i] - M2.Results[i];
                Leverage[i] = H[i,i];
            }

            double SSR = SquaredResiduals.Sum();
            double sigma = SSR/(n-p);

            double[] ExternallyStudentizedResiduals = new double[M2.Results.Length];
            _dffits = new double[M2.Results.Length];
            _cooks = new double[M2.Results.Length];
            for (int i=0; i<M2.Results.Length; i++)
            {
                ExternallyStudentizedResiduals[i] = Residuals[i] / Math.Sqrt((SSR - SquaredResiduals[i]) / (n - p - 1) * (1 - H[i, i]));
                _dffits[i] = ExternallyStudentizedResiduals[i] * Math.Sqrt(H[i, i] / (1 - H[i, i]));
                _cooks[i] = SquaredResiduals[i] / (p * M2.Table[M2.Table.Count - 2].MeanSquares) * H[i, i] / Math.Pow((1 - H[i, i]), 2);
            }

            _AIC = n * Math.Log(sse / n) + (2 * p) + n + 2;
            _AICC = _AIC + (2 * (p + 1) * (p + 2)) / (n - p - 2);
            _BIC = n * (Math.Log(sse / n)) + (p * Math.Log(n));
                        
            _Press = 0.0;
            double leverage = 0.0;
            for (int i = 0; i < M2.Results.Length; i++)
            {
                leverage = Math.Min(Leverage[i], 0.99);                
                _Press += Math.Pow((Residuals[i]) / (1 - leverage),2);
            }
            
            _parameters = createParametersDataTable();
            DataRow dr = null;
            foreach (Accord.Statistics.Analysis.LinearRegressionCoefficient param in M2.Coefficients)
            {
                dr = _parameters.NewRow();
                dr["Name"] = param.Name;
                dr["Value"] = param.Value;
                dr["StandardError"] = param.StandardError;
                dr["TStatistic"] = param.Value / param.StandardError;
                dr["PValue"] = param.TTest.PValue;
                dr["StandardizedCoefficient"] = getStandardCoeff(param.Name, param.Value);
                _parameters.Rows.Add(dr);
            }
                        
            _predictedValues = M2.Results;            
            _observedValues = M2.Outputs;            
            _studentizedResiduals = ExternallyStudentizedResiduals;

            Accord.Statistics.Distributions.Univariate.NormalDistribution distribution = new Accord.Statistics.Distributions.Univariate.NormalDistribution(0, 1);

            double[] standardizedResid = new double[M2.Results.Length];
            for (int i = 0; i < M2.Results.Length; i++)
            {
                standardizedResid[i] = (Residuals[i] - Residuals.Mean()) / Residuals.StandardDeviation();
            }
            Array.Sort(standardizedResid);

            //Anderson-Darling normality test for residuals:
            double AD_stat = 0;
            for (int i = 0; i < M2.Results.Length; i++)
            {
                AD_stat += (2 * i + 1) * (Math.Log(distribution.DistributionFunction(standardizedResid[i])) + Math.Log(1-distribution.DistributionFunction(standardizedResid[n-1-i])));
            }

            AD_stat = -Convert.ToDouble(n) - AD_stat / Convert.ToDouble(n);                        
            _ADresidNormStatVal = AD_stat;
            _ADresidPvalue = 1 - adinf(AD_stat);

            double[,] centered = new double[arrInputData.Length, _independentVars.Length];
            for (int i=0; i<M2.Results.Length; i++)
            {
                for(int j=0; j<_independentVars.Length; j++)
                {
                    centered[i, j] = inputMat[i,j + 1];
                }
            }

            double[,] corrMatrix = inputMat.Transpose().Multiply(inputMat);              
            double[,] InvCorrMatrix = corrMatrix.Inverse();
            
            /*Extreme.Mathematics.Matrix InvCorrMatrix = corrMatrix.GetInverse();
            Extreme.Mathematics.Vector VIFVector = InvCorrMatrix.GetDiagonal();
            Extreme.Mathematics.Vector vifs = InvCorrMatrix.GetDiagonal().ToArray();

            _VIF = new Dictionary<string, double>();
            for (int i = 0; i < vifs.Count(); i++)
                _VIF.Add(_independentVars[i].ToString(), vifs.GetValue(i));

            _maxVIF = VIFVector.AbsoluteMax();
            _maxVIFParameter = _independentVars[VIFVector.AbsoluteMaxIndex()];           */ 
        }


        private double adinf(double z)
        {
            /* Short, practical version of full ADinf(z), z>0.   */
            if (z < 2)
                return Math.Exp(-1.2337141/z) / Math.Sqrt(z)*(2.00012+(.247105-(.0649821-(.0347962-(.011672-.00168691*z)*z)*z)*z)*z);
            else
                return Math.Exp(-Math.Exp(1.0776-(2.30695-(.43424-(.082433-(.008056 -.0003146*z)*z)*z)*z)*z));
        }


        private double getStandardCoeff(string paramName, double coeff)
        {
            //throw new NotImplementedException();
            if (paramName == "(Intercept)") return double.NaN;
            double[] nv = _dataTable.Columns[_dependentVar].ToArray();
            double stdevY = nv.StandardDeviation();

            nv = _dataTable.Columns[paramName].ToArray();
            double stdevX = nv.StandardDeviation();
            return coeff * stdevX / stdevY;
        }


        public double Predict(DataRow independentValues)
        {
            double[] indVals = new double[_independentVars.Length];
            for (int i = 0; i < _independentVars.Length; i++)
            {
                indVals[i] = Convert.ToDouble(independentValues[_independentVars[i]]);
            }

            return Predict(indVals);
        }


        public double Predict(double[] independentValues)
        {
            return _model.Compute(independentValues);
        }


        private DataTable createParametersDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Name",typeof(string));
            dt.Columns.Add("Value", typeof(double));
            dt.Columns.Add("StandardError", typeof(double));
            dt.Columns.Add("TStatistic", typeof(double));
            dt.Columns.Add("PValue", typeof(double));
            dt.Columns.Add("StandardizedCoefficient", typeof(double));

            return dt;
        }


        public Dictionary<string, double> Model
        {
            get
            {
                Dictionary<string, double> parameters = new Dictionary<string, double>();
                for (int i = 0; i < _parameters.Rows.Count; i++)
                {
                    parameters.Add(_parameters.Rows[i][0].ToString(), Convert.ToDouble(_parameters.Rows[i][1]));
                }

                return parameters;
            }
        }        
    }
}
