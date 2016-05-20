using System.Linq.Expressions;

namespace Common
{
	internal class TranslateResult
	{
		internal string CommandText;
		internal LambdaExpression Projector;
	}
}
