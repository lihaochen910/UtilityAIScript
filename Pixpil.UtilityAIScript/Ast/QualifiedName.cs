using System.Collections.Generic;


namespace Pixpil.AI.UtilityAIScript.Ast
{
    /// <summary>
    /// 表示限定名称的AST节点
    /// </summary>
    public class QualifiedName : Node
    {
        /// <summary>
        /// 完整限定名称
        /// </summary>
        public string FullName { get; internal set; }
        
        /// <summary>
        /// 各部分名称
        /// </summary>
        public List<string> NameParts { get; } = new ();
        
        /// <summary>
        /// 获取限定名称的字符串表示
        /// </summary>
        public override string ToString() => FullName ?? string.Join(".", NameParts);
    }
    
}
