using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Moserware.Skills.FactorGraphs;

public abstract class Factor<TValue>
{
	private readonly List<Message<TValue>> _Messages = new List<Message<TValue>>();

	private readonly Dictionary<Message<TValue>, Variable<TValue>> _MessageToVariableBinding = new Dictionary<Message<TValue>, Variable<TValue>>();

	private readonly string _Name;

	private readonly List<Variable<TValue>> _Variables = new List<Variable<TValue>>();

	public virtual double LogNormalization => 0.0;

	public int NumberOfMessages => _Messages.Count;

	protected ReadOnlyCollection<Variable<TValue>> Variables => _Variables.AsReadOnly();

	protected ReadOnlyCollection<Message<TValue>> Messages => _Messages.AsReadOnly();

	protected Factor(string name)
	{
		_Name = "Factor[" + name + "]";
	}

	public virtual double UpdateMessage(int messageIndex)
	{
		Guard.ArgumentIsValidIndex(messageIndex, _Messages.Count, "messageIndex");
		return UpdateMessage(_Messages[messageIndex], _MessageToVariableBinding[_Messages[messageIndex]]);
	}

	protected virtual double UpdateMessage(Message<TValue> message, Variable<TValue> variable)
	{
		throw new NotImplementedException();
	}

	public virtual void ResetMarginals()
	{
		foreach (Variable<TValue> value in _MessageToVariableBinding.Values)
		{
			value.ResetToPrior();
		}
	}

	public virtual double SendMessage(int messageIndex)
	{
		Guard.ArgumentIsValidIndex(messageIndex, _Messages.Count, "messageIndex");
		Message<TValue> message = _Messages[messageIndex];
		Variable<TValue> variable = _MessageToVariableBinding[message];
		return SendMessage(message, variable);
	}

	protected abstract double SendMessage(Message<TValue> message, Variable<TValue> variable);

	public abstract Message<TValue> CreateVariableToMessageBinding(Variable<TValue> variable);

	protected Message<TValue> CreateVariableToMessageBinding(Variable<TValue> variable, Message<TValue> message)
	{
		_ = _Messages.Count;
		_Messages.Add(message);
		_MessageToVariableBinding[message] = variable;
		_Variables.Add(variable);
		return message;
	}

	public override string ToString()
	{
		return _Name ?? base.ToString();
	}
}
