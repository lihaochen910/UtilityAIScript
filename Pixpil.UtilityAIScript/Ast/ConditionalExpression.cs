namespace Pixpil.AI.UtilityAIScript.Ast
{
    /// <summary>
    /// 表示条件(三元)表达式的AST节点
    /// </summary>
    public class ConditionalExpression : Expression
    {
        /// <summary>
        /// 条件表达式
        /// </summary>
        public Expression Condition { get; internal set; }
        
        /// <summary>
        /// 条件为真时的表达式
        /// </summary>
        public Expression TrueExpression { get; internal set; }
        
        /// <summary>
        /// 条件为假时的表达式
        /// </summary>
        public Expression FalseExpression { get; internal set; }
        
        /// <summary>
        /// 获取条件表达式的字符串表示
        /// </summary>
        public override string ToString()
        {
            return $"{Condition} ? {TrueExpression} : {FalseExpression}";
        }
    }
} 