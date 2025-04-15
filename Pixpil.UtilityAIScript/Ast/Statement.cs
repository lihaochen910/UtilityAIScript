namespace Pixpil.AI.UtilityAIScript.Ast
{
    /// <summary>
    /// 语句类型枚举
    /// </summary>
    public enum StatementType
    {
        /// <summary>
        /// 未知类型
        /// </summary>
        Unknown,
        
        /// <summary>
        /// 上下文定义
        /// </summary>
        Context,
        
        /// <summary>
        /// 条件定义
        /// </summary>
        Conditions,
        
        /// <summary>
        /// 默认考虑项定义
        /// </summary>
        DefaultConsideration,
        
        /// <summary>
        /// 推理器定义
        /// </summary>
        Reasoner,
        
        /// <summary>
        /// 变量定义
        /// </summary>
        Variable,
        
        /// <summary>
        /// 常量定义
        /// </summary>
        Constant,
        
        /// <summary>
        /// 空语句
        /// </summary>
        Empty
    }
    
    /// <summary>
    /// 表示语句的AST节点
    /// </summary>
    public class Statement : Node
    {
        /// <summary>
        /// 语句类型
        /// </summary>
        public StatementType StatementType { get; internal set; } = StatementType.Unknown;
        
        /// <summary>
        /// 默认考虑项名称（仅当语句类型为DefaultConsideration时有效）
        /// </summary>
        public string DefaultConsiderationName { get; internal set; }
        
        /// <summary>
        /// 子节点
        /// </summary>
        public Node ChildNode { get; internal set; }
        
        /// <summary>
        /// 获取语句的字符串表示
        /// </summary>
        public override string ToString()
        {
            return StatementType switch
            {
                StatementType.Context => $"Context: {ChildNode}",
                StatementType.Conditions => $"Conditions: {ChildNode}",
                StatementType.DefaultConsideration => $"Default: {DefaultConsiderationName}",
                StatementType.Reasoner => $"Reasoner: {ChildNode}",
                StatementType.Variable => $"Variable: {ChildNode}",
                StatementType.Constant => $"Constant: {ChildNode}",
                StatementType.Empty => "Empty",
                _ => $"Unknown({ChildNode})"
            };
        }
    }
} 