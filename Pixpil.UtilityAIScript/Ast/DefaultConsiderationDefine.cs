namespace Pixpil.AI.UtilityAIScript.Ast
{
    /// <summary>
    /// 表示默认考虑项定义的AST节点
    /// </summary>
    public class DefaultConsiderationDefine : Node
    {
        /// <summary>
        /// 默认考虑项名称
        /// </summary>
        public string ConsiderationName { get; internal set; }

        public Identifier Identifier { get; internal set; }

        /// <summary>
        /// 获取默认考虑项定义的字符串表示
        /// </summary>
        public override string ToString()
        {
            return $"default: {ConsiderationName}";
        }
    }
}