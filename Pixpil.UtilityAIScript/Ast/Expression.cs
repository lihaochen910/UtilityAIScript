namespace Pixpil.AI.UtilityAIScript.Ast
{
    /// <summary>
    /// 表达式类型枚举
    /// </summary>
    public enum ExpressionType
    {
        /// <summary>
        /// 未知类型
        /// </summary>
        Unknown,
        
        /// <summary>
        /// 字面量表达式
        /// </summary>
        Literal,
        
        /// <summary>
        /// 标识符表达式
        /// </summary>
        Identifier,
        
        /// <summary>
        /// 一元表达式
        /// </summary>
        Unary,
        
        /// <summary>
        /// 二元表达式
        /// </summary>
        Binary,
        
        /// <summary>
        /// 三元表达式
        /// </summary>
        Ternary,
        
        /// <summary>
        /// 函数调用表达式
        /// </summary>
        FunctionCall,
        
        /// <summary>
        /// 变量引用表达式
        /// </summary>
        VariableRef,
        
        /// <summary>
        /// 评估表达式
        /// </summary>
        Appraisal,
        
        /// <summary>
        /// 布尔表达式
        /// </summary>
        Boolean,
        
        /// <summary>
        /// 复杂评估表达式
        /// </summary>
        Complex
    }
    
    /// <summary>
    /// 表达式节点基类
    /// </summary>
    public abstract class Expression : Node
    {
        /// <summary>
        /// 表达式类型
        /// </summary>
        public ExpressionType ExprType { get; internal set; } = ExpressionType.Unknown;
    }
    
}