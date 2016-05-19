using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
	// 하위 트리에 대한 expression tree 해석을 위한 클래스
	public static class Evaluator
	{
		public static Expression PartialEval(Expression expression, Func<Expression, bool> fnCanBeEvaluated)
		{
			return new SubtreeEvaluator(new Nominator(fnCanBeEvaluated).Nominate(expression)).Eval(expression);
		}

		public static Expression PartialEval(Expression expression)
		{
			return PartialEval(expression, Evaluator.CanBeEvaluatedLocally);
		}

		public static bool CanBeEvaluatedLocally(Expression expression)
		{
			return expression.NodeType != ExpressionType.Parameter;
		}

		class SubtreeEvaluator : ExpressionVisitor
		{
			HashSet<Expression> _candidates;

			internal SubtreeEvaluator(HashSet<Expression> candidates)
			{
				this._candidates = candidates;
			}

			internal Expression Eval(Expression expression)
			{
				return this.Visit(expression);
			}

			public override Expression Visit(Expression expression)
			{
				if (expression == null) return null;

				if (this._candidates.Contains(expression)) return this.Evaluate(expression);

				return base.Visit(expression);
			}

			private Expression Evaluate(Expression e)
			{
				if (e.NodeType == ExpressionType.Constant) return e;

				LambdaExpression lambda = Expression.Lambda(e);
				Delegate fn = lambda.Compile();

				return Expression.Constant(fn.DynamicInvoke(null), e.Type);
			}
		}

		class Nominator : ExpressionVisitor
		{
			Func<Expression, bool> _fnCanBeEvaluated;
			HashSet<Expression> _candidates;
			bool _cannotBeEvaluated;

			internal Nominator(Func<Expression, bool> fnCanBeEvaluated)
			{
				this._fnCanBeEvaluated = fnCanBeEvaluated;
			}

			internal HashSet<Expression> Nominate(Expression expression)
			{
				this._candidates = new HashSet<Expression>();
				this.Visit(expression);

				return this._candidates;
			}

			public override Expression Visit(Expression expression)
			{
				if (expression != null)
				{
					bool saveCannotBeEvaluated = this._cannotBeEvaluated;
					this._cannotBeEvaluated = false;
					base.Visit(expression);

					if (!this._cannotBeEvaluated)
					{
						if (this._fnCanBeEvaluated(expression))
						{
							this._candidates.Add(expression);
						}
						else
						{
							this._cannotBeEvaluated = true;
						}
					}

					this._cannotBeEvaluated |= saveCannotBeEvaluated;
				}

				return expression;
			}
		}
	}
}
