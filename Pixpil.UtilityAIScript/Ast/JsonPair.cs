
namespace Pixpil.AI.UtilityAIScript.Ast;

/// <summary>
/// 表示JSON键值对的AST节点
/// </summary>
public class JsonPair : Node
{
    /// <summary>
    /// 键名
    /// </summary>
    public string Key { get; internal set; }
    
    /// <summary>
    /// 值节点
    /// </summary>
    public Node Value { get; internal set; }
    
    /// <summary>
    /// 获取JSON键值对的字符串表示
    /// </summary>
    public override string ToString()
    {
        return $"{Key}: {Value}";
    }
} 