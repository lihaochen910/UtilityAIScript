
namespace Pixpil.AI.UtilityAIScript.Ast;

/// <summary>
/// 表示命名参数的AST节点
/// </summary>
public class NamedParam : Node
{
    /// <summary>
    /// 参数名称
    /// </summary>
    public string Name { get; internal set; }
    
    /// <summary>
    /// 参数值
    /// </summary>
    public Node Value { get; internal set; }
    
    /// <summary>
    /// 获取命名参数的字符串表示
    /// </summary>
    public override string ToString()
    {
        return $"{Name}: {Value}";
    }
} 