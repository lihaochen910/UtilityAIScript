namespace Pixpil.AI.UtilityAIScript.Ast
{
	/// <summary>
	/// 表示标识符的AST节点
	/// </summary>
	public class Identifier : Node
	{
		/// <summary>
		/// 标识符名称
		/// </summary>
		public string Name { get; internal set; }
		
		/// <summary>
		/// 获取标识符的字符串表示
		/// </summary>
		public override string ToString()
		{
			return Name ?? "(unnamed)";
		}
	}
}