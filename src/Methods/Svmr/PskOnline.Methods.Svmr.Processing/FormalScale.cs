using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Methods.Svmr.Processing
{
  public class Vector
  {
    
  }

  public class FormalScale
  {
    public FormalScale(double[] scalePoints, double[] valuePoints)
    {
      
    }
    public Vector ScaleValues = new Vector();
    public Vector IntervalValues = new Vector();
  }

  public abstract class Formula
  {
    public abstract double Transform(FormalScale scale, double parameterValue);
  }

  public static class GeometricMean
  {
    public static double Calculate(double[] values)
    {
      double product = 1.0;
      for( int i = 0; i < values.Length; i++ )
      {
        product *= values[i];
      }
      return System.Math.Pow( product, 1.0 / ((double) values.Length) );
    }
  }

  /// <summary>
  /// Ћинейна€ интерпол€ци€
  /// </summary>
  class LinearFormula : Formula
  {
    public override double Transform(FormalScale scale, double parameterValue)
    {
      // ћетодом делени€ пополам ищем диапазон на шкале,
      // в который попадает parameterValue
      // ћетодом линейной интерпол€ции устанавливаем выходное значение
      return 0;
    }
    
  }

  class DecisionSubfactor
  {
    public FormalScale _scale;
    public double _weight;
    public Formula _formula;

    public DecisionSubfactor(string name, double weight, FormalScale scale, Formula formula)
    {
      _scale = scale;
      _weight = weight;
      _formula = formula;
    }

    public double Transform(double physicalValue)
    {
      return _weight * _formula.Transform(_scale, physicalValue);
    }
  }

  class EfficientyLevelProcessor
  {
    FormalScale _reactionMeanScale;
    FormalScale _reactionSigmaScale;
    FormalScale _errorRateScale;

    DecisionSubfactor _reactionMeanSubfactor;
    DecisionSubfactor _reactionSigmaSubfactor;
    DecisionSubfactor _errorRateSubfactor;

    /// <summary>
    /// 3 шкалы,
    /// конечный результат -- среднее геометрическое
    /// </summary>
    public EfficientyLevelProcessor()
    {
      _reactionMeanScale = new FormalScale(
        new double[] {     524.0,     589.0,    782.0,     891.0 },
        new double[] { 1.0,      0.75,      0.5,      0.25,     0.001 } ); // 0 нельз€, т.к. иначе среднее геометрическое станет нулЄм.

      _reactionSigmaScale = new FormalScale(
        new double[] {     86.0,     113.0,    205.0,     263.0 },
        new double[] { 1.0,     0.75,      0.5,      0.25,     0.001 }); // 0 нельз€, т.к. иначе среднее геометрическое станет нулЄм.

      _errorRateScale = new FormalScale(
        new double[] {      0.000,     0.020,    0.043,     0.090 },
        new double[] { 1.0,       0.75,      0.5,      0.25,     0.001 }); // 0 нельз€, т.к. иначе среднее геометрическое станет нулЄм.

      _reactionMeanSubfactor = new DecisionSubfactor(
        "Mean Reaction Time Subfactor", 1.0, _reactionMeanScale, new LinearFormula());

      _reactionSigmaSubfactor = new DecisionSubfactor(
        "Reaction Time Sigma Subfactor", 1.0, _reactionSigmaScale, new LinearFormula());

      _errorRateSubfactor = new DecisionSubfactor(
        "Error Rate Subfactor", 1.0, _errorRateScale, new LinearFormula());

    }

    public double GetValue(double meanReactionTime, double sigmaReactionTime, double errorRate)
    {
      return GeometricMean.Calculate(new double[]
          {
            _reactionMeanSubfactor.Transform(meanReactionTime),
            _reactionSigmaSubfactor.Transform(sigmaReactionTime),
            _errorRateSubfactor.Transform(errorRate)
          }
        );
    }
  }
}
