using System;
using Irony.Ast;
using Irony.Parsing;


namespace Pixpil.AI.UtilityAIScript;

[Language("UtilityAI")]
public partial class UtilityAIGrammar : Grammar {
	
	public UtilityAIGrammar() : base( caseSensitive: true ) {
		// Language flags
		LanguageFlags = LanguageFlags.EmitLineStartToken;

		// Comments
		var singleLineComment = new CommentTerminal("SingleLineComment", "//", "\r", "\n", "\u2085", "\u2028", "\u2029");
		var delimitedComment = new CommentTerminal( "MultiLineComment", "/*", "*/" );
		NonGrammarTerminals.Add(singleLineComment);
		NonGrammarTerminals.Add(delimitedComment);
		
		// 添加分号作为语句终结符
		var semi = ToTerm(";", "semi");
		semi.SetFlag( TermFlags.NoAstNode );
		
		// Keywords
		var kwContext = Keyword("context:");
		var kwConditions = Keyword("conditions:");
		var kwDefaultConsideration = Keyword("default:");
		var kwAll = Keyword("All");
		var kwAny = Keyword("Any");
		var kwArrow = Keyword("->");
		
		kwContext.SetFlag( TermFlags.NoAstNode );
		kwConditions.SetFlag( TermFlags.NoAstNode );
		kwDefaultConsideration.SetFlag( TermFlags.NoAstNode );
		
		// Reasoner symbols
		var firstScoreReasoner = ToTerm(">");
		var highestScoreReasoner = ToTerm("+");
		var lowestScoreReasoner = ToTerm("-");
		
		// Punctuation
		// var leftBracket = ToTerm("[");
		// var rightBracket = ToTerm("]");
		// var leftParen = ToTerm("(");
		// var rightParen = ToTerm(")");
		// var leftBrace = ToTerm("{");
		// var rightBrace = ToTerm("}");
		// var colon = ToTerm(":");
		// var comma = ToTerm(",");
		// var questionMark = ToTerm("?");
		// var equals = ToTerm("=");
		
		// Operators
		var lessThan = Operator("<");
		var greaterThan = Operator(">");
		var lessThanOrEqual = Operator("<=");
		var greaterThanOrEqual = Operator(">=");
		var notEquals = Operator("!=");
		var equalsEquals = Operator("==");
		var not = Operator("!");
		
		RegisterOperators(1, Associativity.Left, "||", "or");
		RegisterOperators(2, Associativity.Left, "&&", "and");
		RegisterOperators(3, Associativity.Left, equalsEquals, notEquals);
		RegisterOperators(4, Associativity.Left, lessThan, greaterThan, lessThanOrEqual, greaterThanOrEqual);
		RegisterOperators(5, Associativity.Left, "+", "-");
		RegisterOperators(6, Associativity.Left, "*", "/", "%");
		RegisterOperators(7, Associativity.Right, not);
		
		var identifier = new IdentifierTerminal("identifier");
		identifier.AstConfig.NodeCreator = CreateIdentifierNode;
		
		var number = new NumberLiteral("number", NumberOptions.None);
		number.Options = NumberOptions.Default;
		number.DefaultFloatType = TypeCode.Single;
		number.DefaultIntTypes = new [] { TypeCode.Int32 };
		number.AddPrefix("0x", NumberOptions.Hex);
		number.AstConfig.NodeCreator = CreateLiteralNode;
		
		var stringLiteral = new StringLiteral("string", "\"", StringOptions.None);
		stringLiteral.AstConfig.NodeCreator = CreateLiteralNode;
		
		// Non-terminals
		var script = T("script", CreateScriptNode);
		var statement = T("statement", CreateStatement);
		
		var qualifiedName = T("qualified_name", CreateQualifiedNameNode);
		var namespaceIdentifier = TT("namespace_part");
		
		qualifiedName.Rule = identifier | namespaceIdentifier;
		namespaceIdentifier.Rule = identifier + "." + qualifiedName;

		var defaultConsiderationDefine = T("default_consideration_define", CreateDefaultConsiderationDefineNode);
		
		var contextDefine = T("context_define", CreateContextDefineNode);
		var conditionsDefine = T("conditions_define", CreateConditionsDefineNode);
		var conditionBlock = T("condition_block", CreateConditionBlockNode);
		var conditionExpression = T("condition_expression", CreateConditionExpressionNode);
		var conditionItem = T("condition_item", CreateConditionItemNode);
		var conditionItems = T("condition_items", CreateConditionItemsNode);
		var conditionListModifier = T("condition_list_modifier", CreateConditionListModifierNode);
		
		var reasonerDefine = T("reasoner_define", CreateReasonerDefineNode);
		var reasonerTypeName = T("reasoner_type_name", CreateReasonerTypeNameNode);
		var reasonerBlock = T("reasoner_block", CreateReasonerBlockNode);
		var considerationDefine = T("consideration_define", CreateConsiderationDefineNode);
		var considerationBlock = T("consideration_block", CreateConsiderationBlockNode);
		var considerationBlockItem = TT("consideration_block_item");
		var appraisalExpression = TT("appraisal_expression");
		
		var expression = T("expression", CreateExpressionNode);
		var literal = T("literal", CreateLiteralNode); 
		var paramList = T("param_list", CreateParamListNode);
		var namedParam = T("named_param", CreateNamedParamNode);
		var jsonObject = T("json_object", CreateJsonObjectNode);
		var jsonPair = T("json_pair", CreateJsonPairNode);
		var jsonArray = T("json_array", CreateJsonArrayNode);
		var jsonValue = T("json_value", CreateJsonValueNode);
		var ternaryExpression = T("ternary_expression", CreateConditionalExpressionNode);
		var booleanExpression = T("boolean_expression", CreateBooleanExpressionNode);
		var complexAppraisalExpression = T("complex_appraisal_expression", CreateComplexAppraisalExpressionNode);
		
		var appraisalDefine = T("appraisal_define", CreateAppraisalDefineNode);
		var actionDefine = T("action_define", CreateActionDefineNode);

		var emptyLine = TT("empty_line");
		
		// 为Unnamed非终结符设置NodeType
		// var jsonItems = T("json_items");
		
		var jsonItemsStarRule = T("json_items_star_rule", CreateJsonItemsStarRuleNode);
		
		// 命名所有 Q() 方法创建的非终结符
		var paramListOptional = TT("param_list_optional");
		paramListOptional.Rule = ToTerm("(") + paramList + ToTerm(")");
		
		var unaryOperator = TT("unary_operator");
		unaryOperator.Rule = ToTerm("+") | ToTerm("-");
		
		// Rules
		literal.Rule = unaryOperator.Opt() + number
					   | stringLiteral
					   | "true"
					   | "false"
					   | "null"
					   | identifier;
		
		script.Rule = MakePlusRule(script, statement);
		
		statement.Rule = contextDefine
						 | conditionsDefine
						 | defaultConsiderationDefine
						 | reasonerDefine
						 | emptyLine;

		emptyLine.Rule = Empty + Eos;
		
		// Context definition
		contextDefine.Rule = kwContext + qualifiedName + Eos;
		
		// Conditions definition
		conditionsDefine.Rule = kwConditions + Eos + conditionBlock + Eos.Opt();

		// 使用显式命名的规则而不是MakePlusRule
		conditionBlock.Rule = MakePlusRule(conditionBlock, conditionExpression);
		
		conditionExpression.Rule = identifier + ":" + conditionListModifier +
								   "[" + conditionItems.Opt() + "]" + Eos;

		conditionListModifier.Rule = kwAll | kwAny;
		
		// 使用显式命名的规则而不是MakePlusRule				   
		conditionItems.Rule = MakePlusRule(conditionItems, conditionItem);
		
		conditionItem.Rule = identifier | expression;
		
		defaultConsiderationDefine.Rule = kwDefaultConsideration + identifier + Eos;
		
		// Reasoner definition
		reasonerDefine.Rule = 
			reasonerTypeName + Eos +
			reasonerBlock;

		reasonerTypeName.Rule = firstScoreReasoner
								| highestScoreReasoner
								| lowestScoreReasoner
								| qualifiedName;

		// 使用显式命名的规则而不是MakePlusRule
		reasonerBlock.Rule = MakePlusRule(reasonerBlock, considerationDefine);
		
		// Consideration definition
		considerationDefine.Rule = "=" + identifier + ":" + qualifiedName + 
								   paramListOptional.Opt() + Eos + considerationBlock;
		
		// 使用显式命名的规则而不是MakePlusRule
		considerationBlock.Rule = MakePlusRule(considerationBlock, considerationBlockItem);
		
		considerationBlockItem.Rule = appraisalDefine | actionDefine;
		considerationBlockItem.SetFlag( TermFlags.IsTransient | TermFlags.NoAstNode );
		
		// Appraisal format
		appraisalDefine.Rule = appraisalExpression + Eos;
		
		appraisalExpression.Rule = ternaryExpression |
								   booleanExpression |
								   complexAppraisalExpression;
		MarkTransient( appraisalExpression );
		
		// Action format
		actionDefine.Rule = kwArrow + Eos + identifier + Eos;
		
		// 使用显式命名的规则而不是MakeStarRule
		paramList.Rule = MakeStarRule(paramList, ToTerm(","), namedParam);
		
		namedParam.Rule = identifier + ":" + expression;
		
		// Expressions
		expression.Rule = literal
						  | expression + "+" + expression
						  | expression + "-" + expression
						  | expression + "*" + expression
						  | expression + "/" + expression
						  | expression + "%" + expression
						  | expression + equalsEquals + expression
						  | expression + notEquals + expression
						  | expression + lessThan + expression
						  | expression + greaterThan + expression
						  | expression + lessThanOrEqual + expression
						  | expression + greaterThanOrEqual + expression
						  | not + expression
						  | "(" + expression + ")";
		
		// 三元表达式
		ternaryExpression.Rule = expression + "?" + expression + ":" + expression;
		booleanExpression.Rule = ToTerm("?").Opt() + identifier;
		complexAppraisalExpression.Rule = identifier + ":" + qualifiedName +
										  "{" + jsonObject + Eos.Opt() + "}";
		
		// JSON support
		jsonObject.Rule = MakeStarRule(jsonObject, ToTerm(","), jsonPair);
		
		jsonPair.Rule = identifier + ":" + jsonValue;
		
		// 使用显式命名的规则而不是MakeStarRule
		jsonItemsStarRule.Rule = Empty
								 | jsonValue
								 | jsonItemsStarRule + "," + jsonValue;
		jsonArray.Rule = "[" + jsonItemsStarRule + "]";
		
		jsonValue.Rule = literal
						 | appraisalExpression
						 | jsonObject
						 | jsonArray;
		
		// Set the root rule
		Root = script;
		
		// Mark punctuation and keywords
		MarkPunctuation(".");
		MarkPunctuation("[", "]", "(", ")", "{", "}", ":", ",", "?", "->", "=", ";");
		MarkReservedWords("context:", "conditions:", "All", "Any", "->");
		
		MarkTransient( statement );
		
		// 添加代码大纲过滤器，处理语法结构
		RegisterBracePair("(", ")");
		RegisterBracePair("[", "]");
		RegisterBracePair("{", "}");
		
		// ternaryExpression.ErrorRule = SyntaxError + Eos;
		script.ErrorRule = SyntaxError + Eos;
		statement.ErrorRule = SyntaxError + Eos;
		AddToNoReportGroup(Eos); 
	}
	
	// 使用代码大纲过滤器
	public override void CreateTokenFilters( LanguageData language, TokenFilterList filters ) {
		var outlineFilter = new CodeOutlineFilter( language.GrammarData,
			OutlineOptions.CheckBraces,
			null );
		filters.Add( outlineFilter );
	}

	public static void BuildAst( LanguageData language, ParseTree parseTree ) {
		// 确保解析树根节点有效
		if ( parseTree.Root == null ||
			 parseTree.Status != ParseTreeStatus.Parsed ) {
			throw new Exception();
		}

		// 创建AST上下文
		var astContext = new AstContext( language );

		// 如果根节点已经有AST节点创建器，则不需要额外处理
		if ( parseTree.Root.AstNode != null ) {
			return;
		}

		// 根据AST节点类型应用相应的Create方法
		// 文件的根节点通常是script节点
		if ( parseTree.Root.Term.Name is "script" ) {
			CreateScriptNode( astContext, parseTree.Root );
		}
		
	}

	public static void BuildAstAdditionalInformation( ParseTree parseTree, string fileSource ) {
		// 确保解析树根节点有效
		if ( parseTree.Root == null ||
			parseTree.Status != ParseTreeStatus.Parsed ) {
			return;
		}
		
		void UpdateNodeSpan( ParseTreeNode node, string fileSource ) {
			// 更新当前节点的Span属性
			if ( node.AstNode is Ast.Node astNode ) {
				// 使用SourceSpan的UpdateFileSource方法创建新的带有文件源的Span
				astNode.Span = astNode.Span.UpdateFileSource( fileSource );
			}
		
			// 递归处理所有子节点
			if ( node.ChildNodes is { Count: > 0 } ) {
				foreach ( var childNode in node.ChildNodes ) {
					UpdateNodeSpan( childNode, fileSource );
				}
			}
		}
		
		UpdateNodeSpan( parseTree.Root, fileSource );
	}
	
	private void ProcessChildNodes( AstContext context, ParseTreeNode node ) {
		if ( node?.ChildNodes == null ||
			 node.ChildNodes.Count == 0 ) {
			return;
		}

		// 遍历所有子节点
		foreach ( var child in node.ChildNodes ) {
			// 如果节点已经有AST节点，跳过处理
			if ( child.AstNode != null ) {
				continue;
			}

			// 根据节点类型选择合适的Create方法
			switch ( child.Term.Name ) {
				case "statement":                    CreateStatement( context, child ); break;
				case "default_consideration_define": CreateDefaultConsiderationDefineNode( context, child ); break;
				case "context_define":               CreateContextDefineNode( context, child ); break;
				case "conditions_define":            CreateConditionsDefineNode( context, child ); break;
				case "reasoner_define":              CreateReasonerDefineNode( context, child ); break;
				case "reasoner_type_name":           CreateReasonerTypeNameNode( context, child ); break;
				case "reasoner_block":               CreateReasonerBlockNode( context, child ); break;
				case "consideration_define":         CreateConsiderationDefineNode( context, child ); break;
				case "consideration_block":          CreateConsiderationBlockNode( context, child ); break;
				case "condition_block":              CreateConditionBlockNode( context, child ); break;
				case "condition_expression":         CreateConditionExpressionNode( context, child ); break;
				case "condition_item":               CreateConditionItemNode( context, child ); break;
				case "condition_items":              CreateConditionItemsNode( context, child ); break;
				case "condition_list_modifier":      CreateConditionListModifierNode( context, child ); break;
				case "appraisal_define":             CreateAppraisalDefineNode( context, child ); break;
				case "action_define":                CreateActionDefineNode( context, child ); break;
				case "expression":                   CreateExpressionNode( context, child ); break;
				case "identifier":                   CreateIdentifierNode( context, child ); break;
				case "qualified_name":               CreateQualifiedNameNode( context, child ); break;
				case "literal":                      CreateLiteralNode( context, child ); break;
				case "ternary_expression":           CreateConditionalExpressionNode( context, child ); break;
				case "boolean_expression":           CreateBooleanExpressionNode( context, child ); break;
				case "complex_appraisal_expression": CreateComplexAppraisalExpressionNode( context, child ); break;
				case "param_list":                   CreateParamListNode( context, child ); break;
				case "named_param":                  CreateNamedParamNode( context, child ); break;
				case "json_object":                  CreateJsonObjectNode( context, child ); break;
				case "json_pair":                    CreateJsonPairNode( context, child ); break;
				case "json_array":                   CreateJsonArrayNode( context, child ); break;
				case "json_value":                   CreateJsonValueNode( context, child ); break;
				case "json_items_star_rule":         CreateJsonItemsStarRuleNode( context, child ); break;
			}

			// 递归处理子节点
			ProcessChildNodes( context, child );
		}
	}

	private KeyTerm Keyword( string keyword ) {
		var term = ToTerm( keyword );

		// term.SetOption(TermOptions.IsKeyword, true);
		// term.SetOption(TermOptions.IsReservedWord, true);

		MarkReservedWords( keyword );
		term.EditorInfo = new TokenEditorInfo( TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None );
		return term;
	}
	
	private KeyTerm Operator( string op ) {
		string opCased = CaseSensitive ? op : op.ToLower();
		var term = new KeyTerm( opCased, op );

		//term.SetOption(TermOptions.IsOperator, true);

		term.EditorInfo = new TokenEditorInfo( TokenType.Operator, TokenColor.Keyword, TokenTriggers.None );

		return term;
	}
	
}
