namespace Pixpil.AI.UtilityAIScript.Ast
{
    /// <summary>
    /// 字面量类型枚举
    /// </summary>
    public enum LiteralType
    {
        Number,
        String,
        Boolean,
        Null,
        Identifier
    }
    
    /// <summary>
    /// 表示字面量的AST节点
    /// </summary>
    public class Literal : Node
    {
        /// <summary>
        /// 字面量的值
        /// </summary>
        public object Value { get; internal set; }
        
        /// <summary>
        /// 字面量类型（数字、字符串、布尔、null等）
        /// </summary>
        public LiteralType Type { get; internal set; }

        public override string ToString() {
            switch ( Type ) {
                case LiteralType.String:
                case LiteralType.Boolean:
                case LiteralType.Number:
                    return $"{Type}: {Value}";
                case LiteralType.Null:
                    return "null";
                case LiteralType.Identifier:
                    return $"ref: {Value}";
            }

            return null;
        }
    }
    
}
