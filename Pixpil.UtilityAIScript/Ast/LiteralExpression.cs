namespace Pixpil.AI.UtilityAIScript.Ast;

/// <summary>
/// 表示字面量表达式的AST节点
/// </summary>
public class LiteralExpression : Expression
{
    /// <summary>
    /// 字面量的值
    /// </summary>
    public object Value { get; internal set; }
    
    /// <summary>
    /// 字面量类型
    /// </summary>
    public LiteralType Type { get; internal set; }

    public UnaryOperator? PreUnaryOperator { get; internal set; }

    /// <summary>
    /// 获取字面量表达式的字符串表示
    /// </summary>
    public override string ToString() {
        if ( Value == null ) {
            return "null";
        }

        if ( Type == LiteralType.String ) {
            return $"\"{Value}\"";
        }

        if ( PreUnaryOperator.HasValue ) {
            if ( PreUnaryOperator is UnaryOperator.Negate ) {
                return $"-{Value}";
            }
            else if ( PreUnaryOperator is UnaryOperator.Not ) {
                return $"!{Value}";
            }
        }
        
        return Value.ToString();
    }
} 