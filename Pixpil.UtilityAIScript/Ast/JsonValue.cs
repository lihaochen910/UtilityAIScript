namespace Pixpil.AI.UtilityAIScript.Ast;

/// <summary>
/// JSON值类型枚举
/// </summary>
public enum JsonValueType
{
    /// <summary>
    /// 未知类型
    /// </summary>
    Unknown,
    
    /// <summary>
    /// 字面量
    /// </summary>
    Literal,
    
    /// <summary>
    /// 表达式
    /// </summary>
    Expression,
    
    /// <summary>
    /// 对象
    /// </summary>
    Object,
    
    /// <summary>
    /// 数组
    /// </summary>
    Array
}

/// <summary>
/// 表示JSON值的AST节点
/// </summary>
public class JsonValue : Node
{
    /// <summary>
    /// 值节点
    /// </summary>
    public Node Value { get; internal set; }
    
    /// <summary>
    /// 值类型
    /// </summary>
    public JsonValueType ValueType { get; internal set; }
    
    /// <summary>
    /// 获取JSON值的字符串表示
    /// </summary>
    public override string ToString()
    {
        return Value?.ToString() ?? "null";
    }
}
