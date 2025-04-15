namespace Pixpil.AI.UtilityAIScript.Ast
{
    /// <summary>
    /// 表示Reasoner类型名称的AST节点
    /// </summary>
    public class ReasonerTypeName : Node
    {
        /// <summary>
        /// Reasoner类型名称
        /// </summary>
        public string TypeName { get; internal set; }
        
        /// <summary>
        /// 是否是内置Reasoner类型
        /// </summary>
        public bool IsBuiltIn { get; internal set; }
        
        /// <summary>
        /// 限定名称（如果是自定义类型）
        /// </summary>
        public QualifiedName QualifiedName { get; internal set; }
        
        /// <summary>
        /// 获取Reasoner类型名称的字符串表示
        /// </summary>
        public override string ToString()
        {
            return IsBuiltIn ? $"BuiltInType[{TypeName}]" : $"CustomType[{TypeName}]";
        }
    }
} 