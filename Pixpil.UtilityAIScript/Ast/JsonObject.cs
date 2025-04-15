using System.Collections.Generic;


namespace Pixpil.AI.UtilityAIScript.Ast;

/// <summary>
/// 表示JSON对象的AST节点
/// </summary>
public class JsonObject : Node
{
    /// <summary>
    /// JSON对象中的键值对
    /// </summary>
    public List<JsonPair> Pairs { get; } = new ();
    
    /// <summary>
    /// 获取JSON对象的字符串表示
    /// </summary>
    public override string ToString()
    {
        return $"{{ {Pairs.Count} pairs }}";
    }
} 