namespace Pixpil.AI.UtilityAIScript.Ast
{
    /// <summary>
    /// 表示条件项的AST节点
    /// </summary>
    public class ConditionItem : Node
    {
        /// <summary>
        /// 条件项内容（可能是标识符或表达式）
        /// </summary>
        public Node Content { get; internal set; }
        
        /// <summary>
        /// 是否是简单标识符
        /// </summary>
        public bool IsIdentifier { get; internal set; }
        
        /// <summary>
        /// 标识符值（如果是标识符）
        /// </summary>
        public string IdentifierValue { get; internal set; }
        
        /// <summary>
        /// 获取条件项的字符串表示
        /// </summary>
        public override string ToString()
        {
            return IsIdentifier ? IdentifierValue : Content?.ToString() ?? "(empty)";
        }
    }
} 