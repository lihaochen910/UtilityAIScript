namespace Pixpil.AI.UtilityAIScript.Ast
{
    /// <summary>
    /// 表示上下文定义的AST节点
    /// </summary>
    public class ContextDefine : Node
    {
        /// <summary>
        /// 上下文类型的限定名称
        /// </summary>
        public QualifiedName ContextType { get; internal set; }
        
        /// <summary>
        /// 获取上下文定义的字符串表示
        /// </summary>
        public override string ToString()
        {
            return $"context: {ContextType}";
        }
    }
} 