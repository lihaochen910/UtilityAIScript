namespace Pixpil.AI.UtilityAIScript.Ast
{
    /// <summary>
    /// 表示变量定义的AST节点
    /// </summary>
    public class Variable : Node
    {
        /// <summary>
        /// 变量名称
        /// </summary>
        public string Name { get; internal set; }
        
        /// <summary>
        /// 变量值表达式
        /// </summary>
        public Node Value { get; internal set; }
        
        /// <summary>
        /// 获取变量的字符串表示
        /// </summary>
        public override string ToString()
        {
            return $"var {Name} = {Value}";
        }
    }
} 