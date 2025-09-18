using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Moserware.Numerics;
using Moserware.Skills.FactorGraphs;

namespace Moserware.Skills.TrueSkill.Factors;

public class GaussianWeightedSumFactor : GaussianFactor
{
	private readonly List<int[]> _VariableIndexOrdersForWeights = new List<int[]>();

	private readonly double[][] _Weights;

	private readonly double[][] _WeightsSquared;

	public override double LogNormalization
	{
		get
		{
			ReadOnlyCollection<Variable<GaussianDistribution>> variables = base.Variables;
			ReadOnlyCollection<Message<GaussianDistribution>> messages = base.Messages;
			double num = 0.0;
			for (int i = 1; i < variables.Count; i++)
			{
				num += GaussianDistribution.LogRatioNormalization(variables[i].Value, messages[i].Value);
			}
			return num;
		}
	}

	public GaussianWeightedSumFactor(Variable<GaussianDistribution> sumVariable, Variable<GaussianDistribution>[] variablesToSum)
		: this(sumVariable, variablesToSum, variablesToSum.Select((Variable<GaussianDistribution> v) => 1.0).ToArray())
	{
	}

	public GaussianWeightedSumFactor(Variable<GaussianDistribution> sumVariable, Variable<GaussianDistribution>[] variablesToSum, double[] variableWeights)
		: base(CreateName(sumVariable, variablesToSum, variableWeights))
	{
		_Weights = new double[variableWeights.Length + 1][];
		_WeightsSquared = new double[_Weights.Length][];
		_Weights[0] = new double[variableWeights.Length];
		Array.Copy(variableWeights, _Weights[0], variableWeights.Length);
		_WeightsSquared[0] = _Weights[0].Select((double w) => w * w).ToArray();
		_VariableIndexOrdersForWeights.Add(Enumerable.Range(0, 1 + variablesToSum.Length).ToArray());
		for (int num = 1; num < _Weights.Length; num++)
		{
			double[] array = new double[variableWeights.Length];
			_Weights[num] = array;
			int[] array2 = new int[variableWeights.Length + 1];
			array2[0] = num;
			double[] array3 = new double[variableWeights.Length];
			_WeightsSquared[num] = array3;
			int num2 = 0;
			for (int num3 = 0; num3 < variableWeights.Length; num3++)
			{
				if (num3 != num - 1)
				{
					double num4 = (0.0 - variableWeights[num3]) / variableWeights[num - 1];
					if (variableWeights[num - 1] == 0.0)
					{
						num4 = 0.0;
					}
					array[num2] = num4;
					array3[num2] = num4 * num4;
					array2[num2 + 1] = num3 + 1;
					num2++;
				}
			}
			double num5 = 1.0 / variableWeights[num - 1];
			if (variableWeights[num - 1] == 0.0)
			{
				num5 = 0.0;
			}
			array[num2] = num5;
			array3[num2] = num5 * num5;
			array2[^1] = 0;
			_VariableIndexOrdersForWeights.Add(array2);
		}
		CreateVariableToMessageBinding(sumVariable);
		foreach (Variable<GaussianDistribution> variable in variablesToSum)
		{
			CreateVariableToMessageBinding(variable);
		}
	}

	private double UpdateHelper(double[] weights, double[] weightsSquared, IList<Message<GaussianDistribution>> messages, IList<Variable<GaussianDistribution>> variables)
	{
		GaussianDistribution gaussianDistribution = messages[0].Value.Clone();
		GaussianDistribution gaussianDistribution2 = variables[0].Value.Clone();
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		for (int i = 0; i < weightsSquared.Length; i++)
		{
			num += weightsSquared[i] / (variables[i + 1].Value.Precision - messages[i + 1].Value.Precision);
			GaussianDistribution gaussianDistribution3 = variables[i + 1].Value / messages[i + 1].Value;
			num2 += weightsSquared[i] / gaussianDistribution3.Precision;
			num3 += weights[i] * (variables[i + 1].Value.PrecisionMean - messages[i + 1].Value.PrecisionMean) / (variables[i + 1].Value.Precision - messages[i + 1].Value.Precision);
			num4 += weights[i] * gaussianDistribution3.PrecisionMean / gaussianDistribution3.Precision;
		}
		double num5 = 1.0 / num;
		GaussianDistribution gaussianDistribution4 = GaussianDistribution.FromPrecisionMean(num5 * num3, num5);
		GaussianDistribution gaussianDistribution5 = gaussianDistribution2 / gaussianDistribution * gaussianDistribution4;
		messages[0].Value = gaussianDistribution4;
		variables[0].Value = gaussianDistribution5;
		return gaussianDistribution5 - gaussianDistribution2;
	}

	public override double UpdateMessage(int messageIndex)
	{
		ReadOnlyCollection<Message<GaussianDistribution>> messages = base.Messages;
		ReadOnlyCollection<Variable<GaussianDistribution>> variables = base.Variables;
		Guard.ArgumentIsValidIndex(messageIndex, messages.Count, "messageIndex");
		List<Message<GaussianDistribution>> list = new List<Message<GaussianDistribution>>();
		List<Variable<GaussianDistribution>> list2 = new List<Variable<GaussianDistribution>>();
		int[] array = _VariableIndexOrdersForWeights[messageIndex];
		for (int i = 0; i < messages.Count; i++)
		{
			list.Add(messages[array[i]]);
			list2.Add(variables[array[i]]);
		}
		return UpdateHelper(_Weights[messageIndex], _WeightsSquared[messageIndex], list, list2);
	}

	private static string CreateName(Variable<GaussianDistribution> sumVariable, IList<Variable<GaussianDistribution>> variablesToSum, double[] weights)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(sumVariable.ToString());
		stringBuilder.Append(" = ");
		for (int i = 0; i < variablesToSum.Count; i++)
		{
			if (i == 0 && weights[i] < 0.0)
			{
				stringBuilder.Append("-");
			}
			stringBuilder.Append(Math.Abs(weights[i]).ToString("0.00"));
			stringBuilder.Append("*[");
			stringBuilder.Append(variablesToSum[i]);
			stringBuilder.Append("]");
			if (i != variablesToSum.Count - 1)
			{
				if (weights[i + 1] >= 0.0)
				{
					stringBuilder.Append(" + ");
				}
				else
				{
					stringBuilder.Append(" - ");
				}
			}
		}
		return stringBuilder.ToString();
	}
}
