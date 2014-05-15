﻿using System;
using System.Collections;
using dotMath.Exceptions;

namespace dotMath.Core
{
	/// <summary>
	/// This base is the basic building block of all operations, variables, functions, etc.  
	/// Any object may call to a CValue object asking for it to resolve itself and return it's processed value.
	/// </summary>
	internal abstract class CValue
	{
		public abstract double GetValue();
	}

	/// <summary>
	/// Derived from CValue, CVariable implements a named expression variable, holding the name and associated value.
	///	This object is accessed when an expression is evaluated or when a user sets a variable value.
	/// </summary>
	internal class CVariable : CValue
	{
		private double _value;

		/// <summary>
		/// Creates an empty CVariable object.
		/// </summary>
		public CVariable()
		{
		}

		/// <summary>
		/// Creates the CVariable object with the assigned value.
		/// </summary>
		/// <param name="value">double containing the assigned variable value</param>
		public CVariable(double value)
		{
			_value = value;
		}

		/// <summary>
		/// Sets the variable's value.
		/// </summary>
		/// <param name="value"></param>
		public void SetValue(double value)
		{
			_value = value;
		}

		/// <summary>
		/// Returns the value of the variable.
		/// </summary>
		/// <returns>double</returns>
		public override double GetValue()
		{
			return _value;
		}
	}

	/// <summary>
	/// A CValue-derived class that implements a static numeric value in an expression.
	/// </summary>
	internal class CNumber : CValue
	{
		private double _value;

		/// <summary>
		/// Takes a string representation of a static number and stores it for future retrieval.
		/// </summary>
		/// <param name="value">string/text representation of a number</param>
		public CNumber(string value)
		{
			_value = Convert.ToDouble(value);
		}

		/// <summary>
		/// Takes a double represenation of a static number and stores it for future retrieval.
		/// </summary>
		/// <param name="value"></param>
		public CNumber(double value)
		{
			_value = value;
		}

		/// <summary>
		/// Returns the static value.
		/// </summary>
		/// <returns>double</returns>
		public override double GetValue()
		{
			return _value;
		}
	}

	/// <summary>
	/// Provides negative number functionality within an equation. 
	/// </summary>
	internal class CSignNeg : CValue
	{
		CValue _value;

		/// <summary>
		/// Grabs onto the assigned CValue object and retains it for processing requested operations.
		/// </summary>
		/// <param name="value">Child operation this object operates upon.</param>
		public CSignNeg(CValue value)
		{
			_value = value;
		}

		/// <summary>
		/// Performs the negative operation on the child operation and returns the value.
		/// </summary>
		/// <returns>A double value evaluated and returned with the opposite sign.</returns>
		public override double GetValue()
		{
			return _value.GetValue() * -1;
		}
	}

	/// <summary>
	/// A CValue derived class responsible for identifying and implementing operations during the parsing and evaluation processes.
	/// </summary>
	internal class COperator : CValue
	{
		private Func<double, double, double> _function;
		private CValue _param1;
		private CValue _param2;

		public COperator(Func<double, double, double> function)
		{
			_function = function;
		}

		public void SetParameters(CValue param1, CValue param2)
		{
			_param1 = param1;
			_param2 = param2;
		}

		public override double GetValue()
		{
			return _function(_param1.GetValue(), _param2.GetValue());
		}
	}

	/// <summary>
	/// A CValue derived class that provdes the base for all functions implemented in the compiler.  
	/// This class also allows for external clients to create and register functions with the compiler,
	/// thereby extending the compiler's syntax and functionality.
	/// </summary>
	internal class CFunction : CValue
	{
		private string _name;
		private object _function;
		private ArrayList _parameters;
		private int _expectedArgCount = -1;

		public CFunction(string name, Func<double, double> function)
		{
			_name = name;
			_function = function;
			_expectedArgCount = 1;
		}

		public CFunction(string name, Func<double, double, double> function)
		{
			_name = name;
			_function = function;
			_expectedArgCount = 2;
		}

		public CFunction(string name, Func<bool, double, double, double> function)
		{
			_name = name;
			_function = function;
			_expectedArgCount = 3;
		}

		public void SetParameters(ArrayList values)
		{
			// validate argument count
			if (_expectedArgCount != values.Count)
				throw new ArgumentCountException(values.Count);

			// validate arguments aren't null
			for (int i = 0; i < values.Count; i++)
			{
				if (values[i] == null)
					throw new ArgumentNullException(string.Format("Argument {0} of function {1} cannot be null.", i + 1, _name));
			}

			_parameters = values;
		}

		public override double GetValue()
		{
			switch (_parameters.Count)
			{
				case 1:
					return (_function as Func<double, double>)(((CValue) _parameters[0]).GetValue());
				case 2:
					return (_function as Func<double, double, double>)(((CValue) _parameters[0]).GetValue(), ((CValue) _parameters[1]).GetValue());
				case 3:
					return (_function as Func<bool, double, double, double>)(Convert.ToBoolean(((CValue) _parameters[0]).GetValue()), ((CValue) _parameters[1]).GetValue(), ((CValue) _parameters[2]).GetValue());
				default:
					throw new ArgumentCountException(_parameters.Count);
			}
		}
	}
}
