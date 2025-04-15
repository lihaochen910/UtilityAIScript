using System.Collections.Generic;


namespace Pixpil.AI.UtilityAIScript.Ast
{
	/// <summary>
	/// 表示完整UtilityAI脚本的AST节点
	/// </summary>
	public class Script : Node
	{
		/// <summary>
		/// 默认的考量名称
		/// </summary>
		public string DefaultConsiderationName { get; internal set; }
		
		/// <summary>
		/// 脚本中的所有语句
		/// </summary>
		public List<Node> Statements { get; } = new ();
		
		/// <summary>
		/// 上下文信息
		/// </summary>
		public QualifiedName ContextType { get; internal set; }
		
		/// <summary>
		/// 预定义条件列表
		/// </summary>
		public ConditionsDefine ConditionsDefine { get; internal set; }

		/// <summary>
		/// 推理器类型
		/// </summary>
		public ReasonerDefine Reasoner { get; internal set; }
		
		/// <summary>
		/// 获取脚本的字符串表示
		/// </summary>
		public override string ToString()
		{
			return $"Script ({Statements.Count} statements)";
		}
	}
}