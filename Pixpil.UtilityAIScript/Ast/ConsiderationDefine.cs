namespace Pixpil.AI.UtilityAIScript.Ast
{
    /// <summary>
    /// 表示考虑项定义的AST节点
    /// </summary>
    public class ConsiderationDefine : Node
    {
        /// <summary>
        /// 考虑项名称
        /// </summary>
        public string Name { get; internal set; }
        
        /// <summary>
        /// 考虑项类型
        /// </summary>
        public QualifiedName Type { get; internal set; }
        
        /// <summary>
        /// 参数列表
        /// </summary>
        public ParamList Parameters { get; internal set; }
        
        /// <summary>
        /// 考虑项块（包含评估和行动定义）
        /// </summary>
        public ConsiderationBlock Block { get; internal set; }
        
        /// <summary>
        /// 获取考虑项定义的字符串表示
        /// </summary>
        public override string ToString() => $"= {Name}: {Type}";
    }
} 