namespace Pixpil.AI.UtilityAIScript.Ast;

/// <summary>
/// 表示标识符表达式的AST节点
/// </summary>
public class IdentifierExpression : Expression
{
    /// <summary>
    /// 标识符
    /// </summary>
    public Identifier Identifier { get; internal set; }
    
    /// <summary>
    /// 标识符名称
    /// </summary>
    public string Name => Identifier?.Name;
    
    /// <summary>
    /// 获取标识符表达式的字符串表示
    /// </summary>
    public override string ToString()
    {
        return Name ?? "<unknown>";
    }
} 