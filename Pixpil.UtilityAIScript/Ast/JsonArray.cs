using System.Collections.Generic;
using Irony.Ast;
using Irony.Parsing;
using Pixpil.AI.UtilityAIScript.Utility;

namespace Pixpil.AI.UtilityAIScript.Ast;

/// <summary>
/// 表示JSON数组的AST节点
/// </summary>
public class JsonArray : Node
{
    /// <summary>
    /// 数组内的元素
    /// </summary>
    public List<Node> Items { get; } = new List<Node>();
    
    /// <summary>
    /// 创建JSON数组节点
    /// </summary>
    public static void Create(AstContext context, ParseTreeNode treeNode)
    {
        var node = new JsonArray();
        node.Span = SpanConverter.Convert(treeNode.Span);
        treeNode.AstNode = node;
        
        // jsonArray.Rule = "[" + jsonItemsStarRule + "]";
        if (treeNode.ChildNodes.Count >= 2)
        {
            // 获取数组项
            if (treeNode.ChildNodes[1].AstNode is JsonItemsStarRule itemsNode)
            {
                // 添加所有数组项作为子节点
                foreach (var item in itemsNode.Childrens())
                {
                    node.Items.Add(item);
                    node.AddChild(item);
                }
            }
        }
    }
    
    /// <summary>
    /// 获取JSON数组的字符串表示
    /// </summary>
    public override string ToString()
    {
        return $"[ {Items.Count} items ]";
    }
} 