namespace Pixpil.AI.UtilityAIScript.Ast;

/// <summary>
/// 布尔表达式节点，用于表示条件判断
/// </summary>
public class BooleanExpression : Expression {
	
	/// <summary>
	/// 标识符
	/// </summary>
	public Identifier Identifier { get; internal set; }
	
	/// <summary>
	/// 是否为否定形式（使用?前缀）
	/// </summary>
	public bool IsNegated { get; internal set; }
	
	/// <summary>
	/// 获取标识符名称
	/// </summary>
	public string IdentifierName => Identifier?.Name;
	
	/// <summary>
	/// 获取布尔表达式的字符串表示
	/// </summary>
	public override string ToString()
	{
		return IsNegated
			? $"?{Identifier}"
			: $"{Identifier}";
	}
}