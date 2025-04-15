namespace Pixpil.AI.UtilityAIScript.Ast
{
    /// <summary>
    /// 表示常量定义的AST节点
    /// </summary>
    public class Constant : Node
    {
        /// <summary>
        /// 常量名称
        /// </summary>
        public string Name { get; internal set; }
        
        /// <summary>
        /// 常量值表达式
        /// </summary>
        public Node Value { get; internal set; }
        
        /// <summary>
        /// 获取常量的字符串表示
        /// </summary>
        public override string ToString()
        {
            return $"const {Name} = {Value}";
        }
    }
} 