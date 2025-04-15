using System.Collections.Generic;


namespace Pixpil.AI.UtilityAIScript.Ast;

/// <summary>
/// 表示参数列表的AST节点
/// </summary>
public class ParamList : Node {
    
    /// <summary>
    /// 参数列表
    /// </summary>
    public List< NamedParam > Parameters { get; } = new ();

    public Dictionary< string, Node > ToDictionary() {
        var dict = new Dictionary< string, Node >();
        foreach ( var namedParam in Parameters ) {
            dict[ namedParam.Name ] = namedParam.Value;
        }

        return dict;
    }

    public Dictionary< string, object > DecodeParameters() {
        var dictionary = new Dictionary< string, object >();
        foreach ( var namedParam in Parameters ) {
            if ( namedParam.Value is LiteralExpression literalExpression ) {
                dictionary[ namedParam.Name ] = literalExpression.Value;
            }
        }
        return dictionary;
    }

    /// <summary>
    /// 获取参数列表的字符串表示
    /// </summary>
    public override string ToString() => $"({Parameters.Count} params)";

}