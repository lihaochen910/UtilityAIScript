namespace Pixpil.AI.UtilityAIScript.Ast;

/// <summary>
/// 条件列表修饰符节点，表示条件是全部满足(All)还是任一满足(Any)
/// </summary>
public class ConditionListModifierNode : Node
{
	/// <summary>
	/// 修饰符类型
	/// </summary>
	public ConditionListModifier Modifier { get; internal set; }
	
	/// <summary>
	/// 获取条件列表修饰符的字符串表示
	/// </summary>
	public override string ToString()
	{
		return Modifier.ToString();
	}
}
