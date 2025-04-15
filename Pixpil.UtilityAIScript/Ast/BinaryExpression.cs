namespace Pixpil.AI.UtilityAIScript.Ast
{
    /// <summary>
    /// 表示二元操作表达式的AST节点
    /// </summary>
    public class BinaryExpression : Expression
    {
        /// <summary>
        /// 二元操作符
        /// </summary>
        public BinaryOperator Operator { get; internal set; }
        
        /// <summary>
        /// 左操作数
        /// </summary>
        public Expression Left { get; internal set; }
        
        /// <summary>
        /// 右操作数
        /// </summary>
        public Expression Right { get; internal set; }
        
        /// <summary>
        /// 获取二元表达式的字符串表示
        /// </summary>
        public override string ToString()
        {
            string opStr = Operator switch
            {
                BinaryOperator.Add => "+",
                BinaryOperator.Subtract => "-",
                BinaryOperator.Multiply => "*",
                BinaryOperator.Divide => "/",
                BinaryOperator.Modulo => "%",
                BinaryOperator.Equal => "==",
                BinaryOperator.NotEqual => "!=",
                BinaryOperator.LessThan => "<",
                BinaryOperator.GreaterThan => ">",
                BinaryOperator.LessThanOrEqual => "<=",
                BinaryOperator.GreaterThanOrEqual => ">=",
                BinaryOperator.And => "&&",
                BinaryOperator.Or => "||",
                _ => "?"
            };
            
            return $"({Left} {opStr} {Right})";
        }
    }
    
    /// <summary>
    /// 二元操作符枚举
    /// </summary>
    public enum BinaryOperator
    {
        /// <summary>
        /// 未知操作符
        /// </summary>
        Unknown,
        
        /// <summary>
        /// 加法
        /// </summary>
        Add,
        
        /// <summary>
        /// 减法
        /// </summary>
        Subtract,
        
        /// <summary>
        /// 乘法
        /// </summary>
        Multiply,
        
        /// <summary>
        /// 除法
        /// </summary>
        Divide,
        
        /// <summary>
        /// 取模
        /// </summary>
        Modulo,
        
        /// <summary>
        /// 等于
        /// </summary>
        Equal,
        
        /// <summary>
        /// 不等于
        /// </summary>
        NotEqual,
        
        /// <summary>
        /// 小于
        /// </summary>
        LessThan,
        
        /// <summary>
        /// 大于
        /// </summary>
        GreaterThan,
        
        /// <summary>
        /// 小于等于
        /// </summary>
        LessThanOrEqual,
        
        /// <summary>
        /// 大于等于
        /// </summary>
        GreaterThanOrEqual,
        
        /// <summary>
        /// 逻辑与
        /// </summary>
        And,
        
        /// <summary>
        /// 逻辑或
        /// </summary>
        Or
    }
} 