using System;
using System.Collections.Generic;
using System.Linq;
using Irony.Parsing;
using Pixpil.AI.UtilityAI;
using Pixpil.AI.UtilityAIScript.Ast;
using Pixpil.AI.UtilityAIScript.Exceptions;


namespace Pixpil.AI.UtilityAIScript {

    /// <summary>
    /// 评价表达式类型
    /// </summary>
    public enum AppraisalExpressionType {
        /// <summary>
        /// 三元评价表达式
        /// </summary>
        Ternary,

        /// <summary>
        /// 布尔评价表达式
        /// </summary>
        Boolean,

        /// <summary>
        /// 复杂评价表达式
        /// </summary>
        Complex,

        /// <summary>
        /// 未知类型
        /// </summary>
        Unknown
    }


    /// <summary>
    /// 一元操作符枚举
    /// </summary>
    public enum UnaryOperator {
        /// <summary>
        /// 取负
        /// </summary>
        Negate,

        /// <summary>
        /// 取非
        /// </summary>
        Not,

        /// <summary>
        /// 未知操作符
        /// </summary>
        Unknown
    }


    /// <summary>
    /// AST节点扩展方法
    /// </summary>
    public static class AstNodeExtensions {
        
        /// <summary>
        /// 获取表达式类型
        /// </summary>
        public static AppraisalExpressionType ExprType( this Node node ) {
            if ( node is Expression expr ) {
                switch ( expr.ExprType ) {
                    case ExpressionType.Ternary: return AppraisalExpressionType.Ternary;
                    case ExpressionType.Boolean: return AppraisalExpressionType.Boolean;
                    default:                     return AppraisalExpressionType.Complex;
                }
            }

            return AppraisalExpressionType.Unknown;
        }

        /// <summary>
        /// 获取三元表达式对象
        /// </summary>
        public static ConditionalExpression TernaryExpression( this Node node ) => node as ConditionalExpression;

        /// <summary>
        /// 获取布尔表达式对象
        /// </summary>
        public static BooleanExpression BooleanExpression( this Node node ) => node as BooleanExpression;

        /// <summary>
        /// 获取复杂表达式对象
        /// </summary>
        public static ComplexAppraisalExpression ComplexExpression( this Node node ) =>
            node as ComplexAppraisalExpression;

        /// <summary>
        /// 获取标识符名称
        /// </summary>
        public static string IdentifierName( this Expression expr ) {
            if ( expr is IdentifierExpression identifier ) return identifier.Name;
            return expr.ToString();
        }

        /// <summary>
        /// 获取考虑项类型名称
        /// </summary>
        public static string TypeName( this ConsiderationDefine considerationNode ) {
            if ( considerationNode.Type != null ) {
                return considerationNode.Type.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// 获取考虑项的评价列表
        /// </summary>
        public static List< AppraisalDefine > Appraisals( this ConsiderationDefine considerationNode ) {
            if ( considerationNode.Block != null ) {
                // 遍历所有子节点，找出AppraisalDefine类型的节点
                // foreach (var child in considerationNode.GetChildNodes())
                // {
                //     if (child is AppraisalDefine appraisal)
                //     {
                //         result.Add(appraisal);
                //     }
                // }
                return considerationNode.Block.AppraisalDefines;
            }

            var result = new List< AppraisalDefine >();
            return result;
        }

        /// <summary>
        /// 获取考虑项的动作
        /// </summary>
        public static ActionDefine Action( this ConsiderationDefine considerationNode ) {
            if ( considerationNode.Block != null ) {
                // 遍历所有子节点，找出ActionDefine类型的节点
                // foreach (var child in considerationNode.GetChildNodes())
                // {
                //     if (child is ActionDefine action)
                //     {
                //         return action;
                //     }
                // }
                return considerationNode.Block.ActionDefine;
            }

            return null;
        }

    }


    internal class TernaryAppraisal< T > (
        Func< T, bool > condition,
        Func< T, float > trueValueFunc,
        Func< T, float > falseValueFunc )
        : BaseAppraisal< T > {

        public override float GetScore( T context ) {
            return condition( context ) ? trueValueFunc( context ) : falseValueFunc( context );
        }
    }


    internal class BooleanAppraisal< T > ( Func< T, bool > condition ) : BaseAppraisal< T > {
        public override float GetScore( T context ) {
            return condition( context ) ? 1.0f : 0.0f;
        }
    }


    internal class SimpleAction< T > ( Action< T > action ) : IAction< T > {

        public void Execute( T context ) {
            action( context );
        }
    }


    internal class ConstantAppraisal< T > ( float value ) : BaseAppraisal< T > {

        public override float GetScore( T context ) {
            return value;
        }
    }


    internal class LinearAppraisal< T > ( float slope, float offset ) : BaseAppraisal< T > {
        public override float GetScore( T context ) {
            return slope + offset;
        }
    }


    internal class CurveAppraisal< T > ( float minInput, float maxInput, float exponent ) : BaseAppraisal< T > {
        
        private readonly float _minInput = minInput;
        private readonly float _maxInput = maxInput;
        private readonly float _exponent = exponent;

        public override float GetScore( T context ) {
            // 简单返回一个固定值，实际应该根据上下文计算曲线
            return 0.5f;
        }
        
    }


    /// <summary>
    /// Utility AI脚本编译器
    /// 将UtilityAI脚本转换为Reasoner实例
    /// </summary>
    public class UtilityAICompiler< TContext > {

        private readonly UtilityAIParser _parser;
        private readonly Dictionary< string, Func< TContext, bool > > _conditionResolvers = new ();
        private readonly Dictionary< string, Action< TContext > > _actionResolvers = new ();
        private readonly Dictionary< string, Func< string, Dictionary< string, object >, IAppraisal< TContext > > > _appraisalFactories = new ();
        private readonly Dictionary< string, Func< string, Dictionary< string, object >, IConsideration< TContext > > > _considerationFactories = new ();
        private readonly Dictionary< string, Func< TContext, float > > _propertyGetters = new ();
        private readonly Dictionary< string, IConsideration< TContext > > _namedConsiderations = new ();

        /// <summary>
        /// 创建一个新的Utility AI编译器实例
        /// </summary>
        public UtilityAICompiler() {
            _parser = new UtilityAIParser();
            RegisterDefaultFactories();
        }

        /// <summary>
        /// 注册条件解析器函数
        /// </summary>
        /// <param name="conditionName">脚本中的条件名称</param>
        /// <param name="resolver">检查条件是否满足的函数</param>
        public void RegisterCondition( string conditionName, Func< TContext, bool > resolver ) {
            _conditionResolvers[ conditionName ] = resolver;
        }

        /// <summary>
        /// 注册动作解析器函数
        /// </summary>
        /// <param name="actionName">脚本中的动作名称</param>
        /// <param name="resolver">执行动作的函数</param>
        public void RegisterAction( string actionName, Action< TContext > resolver ) {
            _actionResolvers[ actionName ] = resolver;
        }

        /// <summary>
        /// 注册属性获取函数
        /// </summary>
        /// <param name="propertyName">脚本中的属性名称</param>
        /// <param name="getter">返回属性值的函数</param>
        public void RegisterProperty( string propertyName, Func< TContext, float > getter ) {
            _propertyGetters[ propertyName ] = getter;
        }

        /// <summary>
        /// 注册评价工厂函数
        /// </summary>
        /// <param name="appraisalType">脚本中的评价类型名称</param>
        /// <param name="factory">创建评价实例的工厂函数</param>
        public void RegisterAppraisalFactory( string appraisalType,
                                              Func< string, Dictionary< string, object >, IAppraisal< TContext > >
                                                  factory ) {
            _appraisalFactories[ appraisalType ] = factory;
        }

        /// <summary>
        /// 注册考虑项工厂函数
        /// </summary>
        /// <param name="considerationType">脚本中的考虑项类型名称</param>
        /// <param name="factory">创建考虑项实例的工厂函数</param>
        public void RegisterConsiderationFactory( string considerationType,
                                                  Func< string, Dictionary< string, object >,
                                                      IConsideration< TContext > > factory ) {
            _considerationFactories[ considerationType ] = factory;
        }

        /// <summary>
        /// 编译UtilityAI脚本为抽象语法树
        /// </summary>
        /// <param name="script">UtilityAI脚本文本</param>
        /// <returns>编译后的Reasoner实例</returns>
        public ParseTree Compile( string script, string fileSourceHint = null ) {
            var parseTree = _parser.Parse( script );

            if ( parseTree.Status is ParseTreeStatus.Error &&
                 parseTree.ParserMessages.Count > 0 ) {
                // PrintNode( parseTree.Root, 0, compilerLog );
                throw new ScriptCompileException( parseTree );
            }
            
            UtilityAIGrammar.BuildAst( _parser.LanguageData, parseTree );
            
            if ( fileSourceHint != null ) {
                UtilityAIGrammar.BuildAstAdditionalInformation( parseTree, fileSourceHint );
            }

            if ( parseTree.Root.AstNode is not Script ) {
                // PrintNode( parseTree.Root, 0, compilerLog );
                // GrammarAnalyzer.AnalyzeGrammar( _parser.Grammar, compilerLog );
                throw new ScriptCompileException( parseTree );
            }

            return parseTree;
        }

        /// <summary>
        /// 从抽象语法树到Reasoner实例
        /// </summary>
        /// <param name="parseTree"></param>
        /// <returns></returns>
        public Reasoner< TContext > BuildReasoner( ParseTree parseTree ) {
            if ( parseTree.Root.AstNode is Script scriptNode ) {
                return ProcessScriptNode( scriptNode );
            }

            return null;
        }

        public Reasoner< TContext > CompileAndBuild( string script, string fileSourceHint = null ) {
            var parseTree = Compile( script, fileSourceHint );
            return BuildReasoner( parseTree );
        }

        private void PrintNode( ParseTreeNode node, int level, Action< string > printFunc = null ) {
            string indent = new string( ' ', level * 2 );
            string nodeText = node.Term.Name;

            if ( node.Token != null ) {
                nodeText += $" [{node.Token.ValueString}]";
            }

            printFunc?.Invoke( $"{indent}{nodeText}" );
            Console.WriteLine( $"{indent}{nodeText}" );

            foreach ( var child in node.ChildNodes ) {
                PrintNode( child, level + 1, printFunc );
            }
        }

        /// <summary>
        /// 从文件编译UtilityAI脚本为Reasoner实例
        /// </summary>
        /// <param name="filePath">脚本文件路径</param>
        /// <returns>编译后的Reasoner实例</returns>
        public Reasoner< TContext > CompileFromFile( string filePath ) {
            ParseTree parseTree = _parser.ParseFile( filePath );

            if ( parseTree.ParserMessages.Count > 0 ) {
                var errors = string.Join( "\n", parseTree.ParserMessages.Select( m => $"{m.Location}: {m.Message}" ) );
                throw new Exception( $"解析脚本失败:\n{errors}" );
            }

            if ( !( parseTree.Root.AstNode is Script scriptNode ) ) {
                throw new Exception( "无法创建AST节点" );
            }

            return ProcessScriptNode( scriptNode );
        }

        /// <summary>
        /// 处理脚本节点
        /// </summary>
        private Reasoner< TContext > ProcessScriptNode( Script script ) {
#if DEBUG
            // DebugPrintScriptNode( script );
#endif

            // 检查Reasoner是否存在
            if ( script.Reasoner == null ) {
                throw new Exception( "脚本中没有找到Reasoner定义" );
            }

            ProcessPreConditions( script.ConditionsDefine );

            // 创建Reasoner
            Reasoner< TContext > reasoner = CreateReasoner( script.Reasoner );

            // 处理考虑项
            ProcessConsiderations( script.Reasoner, reasoner );

            // 设置默认考虑项（如果有指定）
            if ( !string.IsNullOrEmpty( script.DefaultConsiderationName ) ) {
                SetDefaultConsideration( reasoner, script.DefaultConsiderationName );
            }

            return reasoner;
        }

        /// <summary>
        /// 根据ReasonerDefineNode创建Reasoner
        /// </summary>
        private Reasoner< TContext > CreateReasoner( ReasonerDefine reasonerNode ) {
            if ( reasonerNode.IsBuiltIn ) {
                switch ( reasonerNode.ReasonerType ) {
                    case "+": return new HighestScoreReasoner< TContext >();
                    case "-": return new LowestScoreReasoner< TContext >();
                    case ">": return new FirstScoreReasoner< TContext >();
                    default:  throw new NotImplementedException( $"不支持的内置Reasoner类型: '{reasonerNode.ReasonerType}'" );
                }
            }

            throw new NotImplementedException( $"不支持自定义Reasoner类型: '{reasonerNode.ReasonerType}'" );
        }

        private void ProcessPreConditions( ConditionsDefine conditionsDefine ) {
            if (conditionsDefine == null || conditionsDefine.Conditions == null) {
                return;
            }
            
            foreach ( var conditionsDefineCondition in conditionsDefine.Conditions ) {
                foreach ( var conditionExpression in conditionsDefineCondition.ConditionExpressions ) {
                    if ( string.IsNullOrEmpty( conditionExpression.Name ) ) {
                        continue;
                    }

                    // 注册复合条件到条件解析器
                    RegisterCondition( conditionExpression.Name, context => {
                        
                        // 根据修饰符确定如何组合条件项
                        bool isAllModifier = conditionExpression.Modifier == ConditionListModifier.All;
                        
                        // All修饰符：所有条件都为true才返回true
                        // Any修饰符：任一条件为true就返回true
                        bool result = isAllModifier; // All初始为true，Any初始为false
                        
                        // 如果没有条件项，则返回初始值
                        if ( conditionExpression.Items.Count == 0 ) {
                            return result;
                        }

                        foreach (var item in conditionExpression.Items) {
                            bool itemResult;

                            if ( item.IsIdentifier ) {
                                // 如果是引用其他预定义条件
                                if ( _conditionResolvers.TryGetValue( item.IdentifierValue, out var resolver ) ) {
                                    itemResult = resolver( context );
                                }
                                else {
                                    throw new Exception( $"未找到条件解析器: {item.IdentifierValue}" );
                                }
                            }
                            else if ( item.Content is Expression expr ) {
                                // 如果是表达式，则计算表达式结果
                                itemResult = EvaluateCondition(expr, context);
                            }
                            else {
                                // 未知类型
                                throw new Exception($"无效的条件项: {item}");
                            }
                            
                            // 根据修饰符提前返回
                            if (isAllModifier && !itemResult) {
                                // All模式下任一条件为false则整体为false
                                return false;
                            } 
                            else if (!isAllModifier && itemResult) {
                                // Any模式下任一条件为true则整体为true
                                return true;
                            }
                            
                            // 更新结果
                            result = itemResult;
                        }
                        
                        return result;
                    } );
                }
            }
        }

        /// <summary>
        /// 处理考虑项定义
        /// </summary>
        private void ProcessConsiderations( ReasonerDefine reasonerNode, Reasoner< TContext > reasoner ) {
            foreach ( var considerationNode in reasonerNode.Considerations ) {
                if ( considerationNode is ConsiderationDefine considerationDefine ) {
                    var consideration = CreateConsideration( considerationDefine );
                    reasoner.AddConsideration( consideration );
                }
            }
        }

        /// <summary>
        /// 创建考虑项实例
        /// </summary>
        private IConsideration< TContext > CreateConsideration( ConsiderationDefine considerationNode ) {
            if ( string.IsNullOrEmpty( considerationNode.Name ) ||
                 string.IsNullOrEmpty( considerationNode.TypeName() ) ) {
                throw new Exception( "无效的考虑项定义" );
            }

            // 创建考虑项实例
            IConsideration< TContext > consideration = CreateConsiderationInstance(
                considerationNode.TypeName(),
                considerationNode.Name,
                considerationNode.Parameters != null
                    ? considerationNode.Parameters.DecodeParameters()
                    : new Dictionary< string , object >() );

            if ( consideration is BaseConsideration< TContext > baseConsideration ) {
#if !DEBUG
                baseConsideration.Name = considerationNode.Name;
#else
                // baseConsideration.Name = $"{considerationNode.Name} {considerationNode.Span.Location.ToString( useShortFileName: true )}";
                baseConsideration.Name = $"{considerationNode.Name}";
#endif
            }

            // 将考虑项名称和实例关联存储
            _namedConsiderations[ considerationNode.Name ] = consideration;

            // 处理评价和动作
            ProcessAppraisals( considerationNode, consideration );
            ProcessAction( considerationNode, consideration );

            return consideration;
        }

        /// <summary>
        /// 处理考虑项中的评价
        /// </summary>
        private void ProcessAppraisals( ConsiderationDefine considerationNode, IConsideration< TContext > consideration ) {
            foreach ( var appraisalNode in considerationNode.Appraisals() ) {
                var appraisal = CreateAppraisal( appraisalNode );
                if ( appraisal is BaseAppraisal< TContext > baseAppraisal ) {
                    // baseAppraisal.Notes = appraisalNode.ToString();
                    baseAppraisal.Notes = $"{appraisalNode.Expression}";
                    baseAppraisal.NotesLong = $"{appraisalNode.Expression} {appraisalNode.Span.Location.ToString( useShortFileName: true )}";
                }

                if ( consideration is AllOrNothingConsideration< TContext > allOrNothingConsideration ) {
                    allOrNothingConsideration.AddAppraisal( appraisal );
                }
                else if ( consideration is AllOrNothingReturnFixedScoreConsideration< TContext > allOrNothingReturnFixedScoreConsideration ) {
                    allOrNothingReturnFixedScoreConsideration.AddAppraisal( appraisal );
                }
                else if ( consideration is MaxScoreOfChildrenConsideration< TContext > maxScoreOfChildrenConsideration ) {
                    maxScoreOfChildrenConsideration.AddAppraisal( appraisal );
                }
                else if ( consideration is MinScoreOfChildrenConsideration< TContext > minScoreOfChildrenConsideration ) {
                    minScoreOfChildrenConsideration.AddAppraisal( appraisal );
                }
                else if ( consideration is SumOfChildrenConsideration< TContext > sumOfChildrenConsideration ) {
                    sumOfChildrenConsideration.AddAppraisal( appraisal );
                }
                else if ( consideration is SumOfChildrenWithThresholdConsideration< TContext > sumOfChildrenWithThresholdConsideration ) {
                    sumOfChildrenWithThresholdConsideration.AddAppraisal( appraisal );
                }
                else if ( consideration is TakeTheAverageOfChildrenConsideration< TContext > takeTheAverageOfChildrenConsideration ) {
                    takeTheAverageOfChildrenConsideration.AddAppraisal( appraisal );
                }
                else if ( consideration is ThresholdConsideration< TContext > thresholdConsideration ) {
                    thresholdConsideration.AddAppraisal( appraisal );
                }
            }
            
            // TODO: Optional Appraisals
            
        }

        /// <summary>
        /// 处理考虑项中的动作
        /// </summary>
        private void ProcessAction( ConsiderationDefine considerationNode, IConsideration< TContext > consideration ) {
            if ( considerationNode.Action() != null ) {
                consideration.Action = CreateAction( considerationNode.Action() );
            }
        }

        /// <summary>
        /// 创建动作实例
        /// </summary>
        private IAction< TContext > CreateAction( ActionDefine actionNode ) {
            if ( string.IsNullOrEmpty( actionNode.ActionName ) ) {
                throw new Exception( $"Invalid Action Name: {actionNode.ActionName}" );
            }

            if ( _actionResolvers.TryGetValue( actionNode.ActionName, out var resolver ) ) {
                return new SimpleAction< TContext >( resolver );
            }
            else {
                var delegateResolver = ( TContext context ) => {
                    if ( _actionResolvers.TryGetValue( actionNode.ActionName, out var resolver ) ) {
                        resolver.Invoke( context );
                    }
                };
                
                return new SimpleAction< TContext >( delegateResolver );
            }

            // throw new Exception( $"Can not find action: {actionNode.ActionName}" );
        }

        /// <summary>
        /// 创建评价实例
        /// </summary>
        private IAppraisal< TContext > CreateAppraisal( AppraisalDefine appraisalNode ) {
            if ( appraisalNode.Expression == null ) {
                throw new Exception( "无效的评价定义" );
            }

            switch ( appraisalNode.Expression.ExprType() ) {
                case AppraisalExpressionType.Ternary:
                    return CreateTernaryAppraisal( appraisalNode.Expression.TernaryExpression() );

                case AppraisalExpressionType.Boolean:
                    return CreateBooleanAppraisal( appraisalNode.Expression.BooleanExpression() );

                case AppraisalExpressionType.Complex:
                    return CreateComplexAppraisal( appraisalNode.Expression.ComplexExpression() );

                default:
                    throw new NotImplementedException( $"不支持的评价表达式类型: {appraisalNode.Expression.ExprType()}" );
            }
        }

        /// <summary>
        /// 创建三元评价实例
        /// </summary>
        private IAppraisal< TContext > CreateTernaryAppraisal( Expression ternaryNode ) {
            if ( ternaryNode is ConditionalExpression expr ) {
                return new TernaryAppraisal< TContext >(
                    context => EvaluateCondition( expr.Condition, context ),
                    context => EvaluateValue( expr.TrueExpression, context ),
                    context => EvaluateValue( expr.FalseExpression, context )
                );
            }

            return null;
        }

        /// <summary>
        /// 创建布尔评价实例
        /// </summary>
        private IAppraisal< TContext > CreateBooleanAppraisal( BooleanExpression booleanNode ) {
            Func< TContext, bool > conditionFunc;

            if ( _conditionResolvers.TryGetValue( booleanNode.IdentifierName(), out var resolver ) ) {
                conditionFunc = booleanNode.IsNegated
                    ? context => !resolver( context )
                    : resolver;
            }
            else {
                throw new Exception( $"未找到条件解析器: {booleanNode.IdentifierName()}" );
            }

            return new BooleanAppraisal< TContext >( conditionFunc );
        }

        /// <summary>
        /// 创建复杂评价实例
        /// </summary>
        private IAppraisal< TContext > CreateComplexAppraisal( ComplexAppraisalExpression complexNode ) {
            if ( string.IsNullOrEmpty( complexNode.TypeName ) ) {
                throw new Exception( "无效的复杂评价定义" );
            }

            if ( !_appraisalFactories.TryGetValue( complexNode.TypeName, out var factory ) ) {
                throw new Exception( $"未找到评价工厂: {complexNode.TypeName}" );
            }

            // TODO: 将参数转换为数组
            // var parameterArray = complexNode.Parameters.Values
            //     .Select(expr => EvaluateConstantExpression(expr))
            //     .ToArray();

            return factory( complexNode.Name, default );
        }

        /// <summary>
        /// 评估条件表达式的结果
        /// </summary>
        private bool EvaluateCondition( Expression expression, TContext context ) {
            if ( expression is BooleanExpression booleanExpr ) {
                if ( _conditionResolvers.TryGetValue( booleanExpr.IdentifierName(), out var resolver ) ) {
                    bool result = resolver( context );
                    return booleanExpr.IsNegated ? !result : result;
                }

                throw new Exception( $"未找到条件解析器: {booleanExpr.IdentifierName()}" );
            }
            else if ( expression is LiteralExpression literalExpr ) {
                if ( literalExpr.Value is bool boolValue ) {
                    return literalExpr.PreUnaryOperator is UnaryOperator.Not ? !boolValue : boolValue;
                }

                if ( literalExpr.Type is LiteralType.Identifier ) {
                    if ( _conditionResolvers.TryGetValue( literalExpr.Value.ToString(), out var resolver ) ) {
                        bool result = resolver( context );
                        return literalExpr.PreUnaryOperator is UnaryOperator.Not ? !result : result;
                    }
                }

                throw new Exception( "条件表达式必须返回布尔值" );
            }
            else if ( expression is IdentifierExpression identExpr ) {
                if ( _conditionResolvers.TryGetValue( identExpr.Name, out var resolver ) ) {
                    return resolver( context );
                }
                
                throw new Exception( $"未找到条件解析器: {identExpr.Name}" );
            }
            else if ( expression is BinaryExpression binaryExpr ) {
                // 处理比较操作符
                switch ( binaryExpr.Operator ) {
                    case BinaryOperator.Equal:
                        return Math.Abs(EvaluateValue(binaryExpr.Left, context) - EvaluateValue(binaryExpr.Right, context)) < 0.001f;
                    case BinaryOperator.NotEqual:
                        return Math.Abs(EvaluateValue(binaryExpr.Left, context) - EvaluateValue(binaryExpr.Right, context)) >= 0.001f;
                    case BinaryOperator.LessThan:
                        return EvaluateValue(binaryExpr.Left, context) < EvaluateValue(binaryExpr.Right, context);
                    case BinaryOperator.GreaterThan:
                        return EvaluateValue(binaryExpr.Left, context) > EvaluateValue(binaryExpr.Right, context);
                    case BinaryOperator.LessThanOrEqual:
                        return EvaluateValue(binaryExpr.Left, context) <= EvaluateValue(binaryExpr.Right, context);
                    case BinaryOperator.GreaterThanOrEqual:
                        return EvaluateValue(binaryExpr.Left, context) >= EvaluateValue(binaryExpr.Right, context);
                    case BinaryOperator.And:
                        return EvaluateCondition(binaryExpr.Left, context) && EvaluateCondition(binaryExpr.Right, context);
                    case BinaryOperator.Or:
                        return EvaluateCondition(binaryExpr.Left, context) || EvaluateCondition(binaryExpr.Right, context);
                    default:
                        // 对于其他操作符，使用数值计算并判断结果
                        return EvaluateValue(binaryExpr, context) > 0.5f;
                }
            }

            // 对于其他类型的表达式，先计算结果然后判断是否为true
            return EvaluateValue( expression, context ) > 0.5f;
        }

        /// <summary>
        /// 评估值表达式的结果
        /// </summary>
        private float EvaluateValue( Expression expression, TContext context ) {
            if ( expression is LiteralExpression literalExpr ) {
                if ( literalExpr.Value is float floatValue )
                    return floatValue;
                else if ( literalExpr.Value is int intValue )
                    return intValue;
                else if ( literalExpr.Value is bool boolValue )
                    return boolValue ? 1f : 0f;
                else {
                    if ( literalExpr.Type is LiteralType.Identifier ) {
                        if ( _propertyGetters.TryGetValue( literalExpr.Value.ToString(), out var getter ) ) {
                            return getter( context );
                        }
                    }
                }

                throw new Exception( $"无法将值转换为浮点数: {literalExpr.Value}" );
            }
            else if ( expression is IdentifierExpression identExpr ) {
                if ( _propertyGetters.TryGetValue( identExpr.Name, out var getter ) ) {
                    return getter( context );
                }

                throw new Exception( $"未找到属性getter: {identExpr.Name}" );
            }
            else if ( expression is BinaryExpression binaryExpr ) {
                float left = EvaluateValue( binaryExpr.Left, context );
                float right = EvaluateValue( binaryExpr.Right, context );

                switch ( binaryExpr.Operator ) {
                    case BinaryOperator.Add: return left + right;
                    case BinaryOperator.Subtract: return left - right;
                    case BinaryOperator.Multiply: return left * right;
                    case BinaryOperator.Divide: return right != 0 ? left / right : 0;
                    case BinaryOperator.Modulo: return right != 0 ? left % right : 0;
                    case BinaryOperator.Equal: return Math.Abs( left - right ) < 0.001f ? 1f : 0f;
                    case BinaryOperator.NotEqual: return Math.Abs( left - right ) >= 0.001f ? 1f : 0f;
                    case BinaryOperator.LessThan: return left < right ? 1f : 0f;
                    case BinaryOperator.GreaterThan: return left > right ? 1f : 0f;
                    case BinaryOperator.LessThanOrEqual: return left <= right ? 1f : 0f;
                    case BinaryOperator.GreaterThanOrEqual: return left >= right ? 1f : 0f;
                    case BinaryOperator.And: return left > 0.5f && right > 0.5f ? 1f : 0f;
                    case BinaryOperator.Or: return left > 0.5f || right > 0.5f ? 1f : 0f;
                    default: throw new NotImplementedException( $"不支持的二元操作符: {binaryExpr.Operator}" );
                }
            }

            // 一元表达式类型已不存在，所以不再需要处理

            throw new NotImplementedException( $"不支持的表达式类型: {expression.GetType().Name}" );
        }

        /// <summary>
        /// 评估常量表达式的值
        /// </summary>
        private object EvaluateConstantExpression( Expression expression ) {
            if ( expression is LiteralExpression literalExpr ) {
                return literalExpr.Value;
            }

            throw new Exception( "只支持字面量作为常量表达式" );
        }

        /// <summary>
        /// 创建考虑项实例
        /// </summary>
        private IConsideration< TContext > CreateConsiderationInstance( string type, string name,
                                                                        Dictionary< string, object > parameters ) {
            if ( _considerationFactories.TryGetValue( type, out var factory ) ) {
                // 将参数转换为数组
                // var paramArray = parameters.Values.ToArray();
                return factory( name, parameters );
            }

            throw new Exception( $"未找到考虑项工厂: {type}" );
        }

        /// <summary>
        /// 设置默认考虑项
        /// </summary>
        private void SetDefaultConsideration( Reasoner< TContext > reasoner, string defaultConsiderationName ) {
            if ( _namedConsiderations.TryGetValue( defaultConsiderationName, out var consideration ) ) {
                reasoner.SetDefaultConsideration( consideration );
            }
            else {
                throw new Exception( $"未找到默认考虑项: {defaultConsiderationName}" );
            }
        }

        /// <summary>
        /// 调试输出脚本节点结构
        /// </summary>
        private void DebugPrintScriptNode( Script script ) {
            // Debug.WriteLine("Script AST结构:");
            // Debug.WriteLine($"- 默认考虑项: {script.DefaultConsiderationName ?? "无"}");
            //
            // if (script.Context != null)
            //     Debug.WriteLine($"- 上下文类型: {script.Context.ContextTypeName}");
            //
            // if (script.Reasoner != null)
            // {
            //     Debug.WriteLine($"- Reasoner类型: {script.Reasoner.ReasonerType} (内置: {script.Reasoner.IsBuiltIn})");
            //     Debug.WriteLine($"- 考虑项数量: {script.Reasoner.Considerations.Count}");
            //     
            //     foreach (var consideration in script.Reasoner.Considerations)
            //     {
            //         Debug.WriteLine($"  - 考虑项: {consideration.Name} (类型: {consideration.TypeName})");
            //         Debug.WriteLine($"    参数: {string.Join(", ", consideration.Parameters.Select(p => $"{p.Key}={p.Value}"))}");
            //         Debug.WriteLine($"    评价数量: {consideration.Appraisals.Count}");
            //         
            //         if (consideration.Action != null)
            //             Debug.WriteLine($"    动作: {consideration.Action.ActionName}");
            //     }
            // }
        }

        /// <summary>
        /// 注册默认工厂
        /// </summary>
        private void RegisterDefaultFactories() {

            // 默认评价工厂
            // _appraisalFactories[ "Curve" ] = ( name, args ) => {
            //     float minInput = args.TryGetValue( "min", out var min ) ? Convert.ToSingle( min ) : 0f;
            //     float maxInput = args.TryGetValue( "max", out var max ) ? Convert.ToSingle( max ) : 1f;
            //     float exponent = args.TryGetValue( "exp", out var exp ) ? Convert.ToSingle( exp ) : 1f;
            //     return new CurveAppraisal< TContext >( minInput, maxInput, exponent );
            // };
            //
            // _appraisalFactories[ "Linear" ] = ( name, args ) => {
            //     float slope = args.TryGetValue( "slope", out var slopeVal ) ? Convert.ToSingle( slopeVal ) : 1f;
            //     float offset = args.TryGetValue( "offset", out var offsetVal ) ? Convert.ToSingle( offsetVal ) : 0f;
            //     return new LinearAppraisal< TContext >( slope, offset );
            // };
            //
            // _appraisalFactories[ "Constant" ] = ( name, args ) => {
            //     float value = args.TryGetValue( "value", out var val ) ? Convert.ToSingle( val ) : 1f;
            //     return new ConstantAppraisal< TContext >( value );
            // };

            // 默认考虑项工厂
            _considerationFactories[ "AllOrNothingConsideration" ] = ( name, args ) => {
                float threshold = args.TryGetValue( "threshold", out var thresholdVal )
                    ? Convert.ToSingle( thresholdVal )
                    : 0f;
                return new AllOrNothingConsideration< TContext >( threshold );
            };
            _considerationFactories[ "AllOrNothing" ] = _considerationFactories[ "AllOrNothingConsideration" ];
            
            _considerationFactories[ "AllOrNothingReturnFixedScoreConsideration" ] = ( name, args ) => {
                float threshold = args.TryGetValue( "threshold", out var thresholdVal )
                    ? Convert.ToSingle( thresholdVal )
                    : 1f;
                float fixedScore = args.TryGetValue( "score", out var fixedScoreVal )
                    ? Convert.ToSingle( fixedScoreVal )
                    : 1f;
                return new AllOrNothingReturnFixedScoreConsideration< TContext >( threshold: threshold, score: fixedScore );
            };
            _considerationFactories[ "AllOrNothingFixed" ] = _considerationFactories[ "AllOrNothingReturnFixedScoreConsideration" ];
            
            _considerationFactories[ "MaxScoreOfChildrenConsideration" ] = ( name, args ) => {
                return new MaxScoreOfChildrenConsideration< TContext >();
            };
            _considerationFactories[ "MaxScoreOfChildren" ] = _considerationFactories[ "MaxScoreOfChildrenConsideration" ];
            _considerationFactories[ "MaxOf" ] = _considerationFactories[ "MaxScoreOfChildrenConsideration" ];
            
            _considerationFactories[ "MinScoreOfChildrenConsideration" ] = ( name, args ) => {
                return new MinScoreOfChildrenConsideration< TContext >();
            };
            _considerationFactories[ "MinScoreOfChildren" ] = _considerationFactories[ "MinScoreOfChildrenConsideration" ];
            _considerationFactories[ "MinOf" ] = _considerationFactories[ "MinScoreOfChildrenConsideration" ];
            
            _considerationFactories[ "FixedScoreConsideration" ] = ( name, args ) => {
                float score = args.TryGetValue( "score", out var scoreVal )
                    ? Convert.ToSingle( scoreVal )
                    : 1f;
                return new FixedScoreConsideration< TContext >( score );
            };
            _considerationFactories[ "FixedScore" ] = _considerationFactories[ "FixedScoreConsideration" ];

            _considerationFactories[ "SumOfChildrenConsideration" ] = ( name, args ) => {
                return new SumOfChildrenConsideration< TContext >();
            };
            _considerationFactories[ "SumOfChildren" ] = _considerationFactories[ "SumOfChildrenConsideration" ];
            
            _considerationFactories[ "TakeTheAverageOfChildrenConsideration" ] = ( name, args ) => {
                return new TakeTheAverageOfChildrenConsideration< TContext >();
            };
            _considerationFactories[ "TakeAvg" ] = _considerationFactories[ "TakeTheAverageOfChildrenConsideration" ];
            _considerationFactories[ "Avg" ] = _considerationFactories[ "TakeTheAverageOfChildrenConsideration" ];
            
            _considerationFactories[ "SumOfChildrenWithPre" ] = ( name, args ) => {
                float threshold = args.TryGetValue( "threshold", out var thresholdVal )
                    ? Convert.ToSingle( thresholdVal )
                    : 0f;
                
                PreAppraisalsCheckMode preCheckMode = args.TryGetValue( "pre_check_mode", out var preCheckModeVal )
                    ? Enum.TryParse< PreAppraisalsCheckMode >( preCheckModeVal.ToString(), out var preCheckModeReal ) ? preCheckModeReal : PreAppraisalsCheckMode.AllRequired
                    : PreAppraisalsCheckMode.AllRequired;
                return new SumOfChildrenWithPreAppraisalsConsideration< TContext >( threshold: threshold, preCheckMode: preCheckMode );
            };
            
            _considerationFactories[ "SumOfChildrenWithThresholdConsideration" ] = ( name, args ) => {
                float threshold = args.TryGetValue( "threshold", out var thresholdVal )
                    ? Convert.ToSingle( thresholdVal )
                    : 0f;
                
                CompareMethod compareMethod = args.TryGetValue( "compare_method", out var compareMethodVal )
                    ? Enum.TryParse< CompareMethod >( compareMethodVal.ToString(), out var compareMethodReal ) ? compareMethodReal : CompareMethod.GreaterThan
                    : CompareMethod.GreaterThan;
                return new SumOfChildrenWithThresholdConsideration< TContext >( compareMethod: compareMethod, threshold: threshold );
            };
            _considerationFactories[ "SumOfChildrenWithThreshold" ] = _considerationFactories[ "SumOfChildrenWithThresholdConsideration" ];

            _considerationFactories[ "ThresholdConsideration" ] = ( name, args ) => {
                float threshold = args.TryGetValue( "threshold", out var thresholdVal )
                    ? Convert.ToSingle( thresholdVal )
                    : 0.5f;
                return new ThresholdConsideration< TContext >( threshold );
            };
            _considerationFactories[ "Threshold" ] = _considerationFactories[ "ThresholdConsideration" ];
        }
    }

}
