using System.Collections.Generic;


namespace Pixpil.AI.UtilityAIScript.Ast;

/// <summary>
/// 表示JSON数组项列表的AST节点
/// </summary>
public class JsonItemsStarRule : Node
{
    /// <summary>
    /// 数组项列表
    /// </summary>
    public List<Node> Items { get; } = new ();
    
    /// <summary>
    /// 获取JSON数组项列表的字符串表示
    /// </summary>
    public override string ToString()
    {
        return $"{Items.Count} items";
    }
}
