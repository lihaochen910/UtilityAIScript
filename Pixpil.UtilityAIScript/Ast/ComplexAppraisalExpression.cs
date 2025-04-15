using System.Collections.Generic;


namespace Pixpil.AI.UtilityAIScript.Ast;

/// <summary>
/// 复杂评估表达式节点
/// </summary>
public class ComplexAppraisalExpression : Expression {
	
	/// <summary>
	/// 评估名称
	/// </summary>
	public string Name { get; internal set; }
	
	/// <summary>
	/// 评估类型名称
	/// </summary>
	public string TypeName { get; internal set; }
	
	/// <summary>
	/// 参数字典
	/// </summary>
	public Dictionary<string, Node> Parameters { get; private set; } = new ();

	/// <summary>
	/// 获取复杂评估表达式的字符串表示
	/// </summary>
	public override string ToString() => $"{Name}:{TypeName} {{ ... }}";

}
