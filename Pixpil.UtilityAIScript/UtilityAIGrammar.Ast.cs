using System;
using Irony.Ast;
using Irony.Parsing;
using Pixpil.AI.UtilityAIScript.Ast;
using Pixpil.AI.UtilityAIScript.Utility;


namespace Pixpil.AI.UtilityAIScript;

public partial class UtilityAIGrammar {

	/// <summary>
	/// The ast.
	/// </summary>
	/// <param name="node">
	/// </param>
	/// <typeparam name="T">
	/// </typeparam>
	/// <returns>
	/// </returns>
	private static T Ast< T >( ParseTreeNode node ) where T : class, new() {
		T value = new T();
		object obj = value;
		node.AstNode = value;
		if ( value is Node ) {
			( ( Node )obj ).Span = SpanConverter.Convert( node.Span );
		}

		return value;
	}


	private static void CreateScriptNode( AstContext context, ParseTreeNode treeNode ) {
		var node = Ast< Script >( treeNode );

		// 处理各个子节点
		foreach ( var child in treeNode.ChildNodes ) {

			if ( child.AstNode is null ) {
				switch ( child.Term.Name ) {
					case "default_consideration_define": CreateDefaultConsiderationDefineNode( context, child ); break;
					case "context_define": CreateContextDefineNode( context, child ); break;
					case "conditions_define": CreateConditionsDefineNode( context, child ); break;
					case "reasoner_define": CreateReasonerDefineNode( context, child ); break;
				}
			}
			
			if ( child.AstNode is DefaultConsiderationDefine defaultConsiderationDefine ) {
				node.AddChild( defaultConsiderationDefine );
				node.Statements.Add( defaultConsiderationDefine );
				node.DefaultConsiderationName = defaultConsiderationDefine.ConsiderationName;
			}
			else if ( child.AstNode is ContextDefine contextDefine ) {
				node.AddChild( contextDefine );
				node.Statements.Add( contextDefine );
				node.ContextType = contextDefine.ContextType;
			}
			else if ( child.AstNode is ConditionsDefine conditionsDefine ) {
				node.AddChild( conditionsDefine );
				node.Statements.Add( conditionsDefine );
				node.ConditionsDefine = conditionsDefine;
			}
			else if ( child.AstNode is ReasonerDefine reasonerDefine ) {
				node.AddChild( reasonerDefine );
				node.Statements.Add( reasonerDefine );
				node.Reasoner = reasonerDefine;
			}
		}
	}

	private static void CreateLiteralNode( AstContext context, ParseTreeNode treeNode ) {
		void DetermineType( Literal node, Token token ) {
			if ( token.Terminal.Name == "number" ) {
				node.Type = LiteralType.Number;
			}
			else if ( token.Terminal.Name == "string" ) {
				node.Type = LiteralType.String;
			}
			else if ( token.ValueString == "true" || token.ValueString == "false" ) {
				node.Type = LiteralType.Boolean;
				node.Value = token.ValueString == "true";
			}
			else if ( token.ValueString == "null" ) {
				node.Type = LiteralType.Null;
				node.Value = null;
			}
			else if ( token.Terminal.Name == "identifier" ) {
				node.Type = LiteralType.Identifier;
			}
		}

		var node = Ast< Literal >( treeNode );

		// 检查是否有字面量token
		if ( treeNode.Token != null ) {
			// 直接从token获取值
			node.Value = treeNode.Token.Value;
			DetermineType( node, treeNode.Token );
		}
		else if ( treeNode.ChildNodes.Count > 0 ) {
			if ( treeNode.ChildNodes[ 0 ].Term.Name == "identifier" ) {
				if ( treeNode.ChildNodes[ 0 ].AstNode is null ) {
					CreateIdentifierNode( context, treeNode.ChildNodes[ 0 ] );
				}
				
				if ( treeNode.ChildNodes[ 0 ].AstNode is Identifier identifier ) {
					node.Value = identifier.Name;
					node.Type = LiteralType.Identifier;
				}
			}
			else {
				// 处理可能的复合字面量（如 +-数字）
				UnaryOperator unaryOperator = UnaryOperator.Unknown;
				if ( treeNode.ChildNodes[ 0 ].Term.Name is "unary_operator?" &&
					 treeNode.ChildNodes[ 0 ].ChildNodes.Count > 0 ) {
					var valString = treeNode.ChildNodes[ 0 ].ChildNodes[ 0 ].Token.ValueString;
					if ( valString is "-" ) {
						unaryOperator = UnaryOperator.Negate;
					}
					if ( valString is "!" ) {
						unaryOperator = UnaryOperator.Not;
					}
				}

				ParseTreeNode valueNode = null;
				if ( treeNode.ChildNodes[ 0 ].Term.Name is "number" ) {
					valueNode = treeNode.ChildNodes[ 0 ];
				}
				else if ( treeNode.ChildNodes[ 1 ].Term.Name is "number" ) {
					valueNode = treeNode.ChildNodes[ 1 ];
				}

				if ( valueNode != null ) {
					var numberToken = valueNode.Token;
					node.Value = numberToken.Value;
					node.Type = LiteralType.Number;

					// 应用符号
					if ( unaryOperator is UnaryOperator.Negate ) {
						if ( node.Value is int intValue ) {
							node.Value = -intValue;
						}
						else if ( node.Value is float floatValue ) {
							node.Value = -floatValue;
						}
						else if ( node.Value is double doubleValue ) {
							node.Value = -doubleValue;
						}
					}
				}
				else {
					// TODO: string/boolean/null
					throw new NotImplementedException();
				}
				
			}
		}
	}

	private static void CreateIdentifierNode( AstContext context, ParseTreeNode treeNode ) {
		var node = Ast< Identifier >( treeNode );

		// 设置标识符名称
		if ( treeNode.Token != null ) {
			node.Name = treeNode.Token.ValueString;
		}
	}

	private static void CreateQualifiedNameNode( AstContext context, ParseTreeNode treeNode ) {
		void ProcessNamespacePart( ParseTreeNode parseNode, QualifiedName node ) {
			if ( parseNode.ChildNodes.Count >= 2 ) {
				// 第一个子节点是identifier
				if ( parseNode.ChildNodes[ 0 ].Term.Name == "identifier" && parseNode.ChildNodes[ 0 ].Token != null ) {
					node.NameParts.Add( parseNode.ChildNodes[ 0 ].Token.ValueString );
				}

				// 第三个子节点是qualified_name，继续递归
				if ( parseNode.ChildNodes.Count > 2 && parseNode.ChildNodes[ 2 ].Term.Name == "qualified_name" ) {
					if ( parseNode.ChildNodes[ 2 ].ChildNodes.Count == 1 &&
						 parseNode.ChildNodes[ 2 ].ChildNodes[ 0 ].Term.Name == "identifier" ) {
						// 最后一个部分是简单标识符
						node.NameParts.Add( parseNode.ChildNodes[ 2 ].ChildNodes[ 0 ].Token.ValueString );
					}
					else if ( parseNode.ChildNodes[ 2 ].ChildNodes.Count == 1 &&
							  parseNode.ChildNodes[ 2 ].ChildNodes[ 0 ].Term.Name == "namespace_part" ) {
						// 继续处理嵌套的命名空间部分
						ProcessNamespacePart( parseNode.ChildNodes[ 2 ].ChildNodes[ 0 ], node );
					}
				}
			}
		}

		var node = Ast< QualifiedName >( treeNode );

		// qualifiedName.Rule = identifier | namespaceIdentifier
		if ( treeNode.ChildNodes.Count == 1 ) {
			var child = treeNode.ChildNodes[ 0 ];
			if ( child.Term.Name == "identifier" && child.Token != null ) {
				// 简单标识符
				string name = child.Token.ValueString;
				node.NameParts.Add( name );
				node.FullName = name;

				if ( child.AstNode is null ) {
					CreateIdentifierNode( context, child );
					
				}

				if ( child.AstNode is Identifier identifier ) {
					node.AddChild( identifier );
				}
			}
			else if ( child.Term.Name == "namespace_part" ) {
				// 命名空间部分，需要递归处理
				ProcessNamespacePart( child, node );
				node.FullName = string.Join( ".", node.NameParts );
			}
		}
	}

	private static void CreateExpressionNode(AstContext context, ParseTreeNode treeNode) {
		// 注意：不能直接使用 Ast<Expression>，因为Expression是抽象类
		// 我们需要根据表达式类型创建具体的表达式对象
		
		/*
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
		*/
		if ( treeNode.ChildNodes.Count is <= 2 and > 0 ) {
			var isUnaryOperator = treeNode.ChildNodes[ 0 ].Token is { ValueString: "!" } or { ValueString: "-" };
			UnaryOperator? unaryOperator = null;
			if ( isUnaryOperator ) {
				if ( treeNode.ChildNodes[ 0 ].Token.ValueString == "!" ) {
					unaryOperator = UnaryOperator.Not;
				}
				else if ( treeNode.ChildNodes[ 0 ].Token.ValueString == "-" ) {
					unaryOperator = UnaryOperator.Negate;
				}
			}
			
			// 单一表达式处理
			var child = !isUnaryOperator ? treeNode.ChildNodes[0] : treeNode.ChildNodes[1];
			
			// 检查子节点类型并创建相应的AST节点
			if ( child.Term.Name == "literal" ) {
				if ( child.AstNode is null ) {
					CreateLiteralNode( context, child );
				}
				if ( child.AstNode is Literal literal ) {
					var literalExpr = Ast< LiteralExpression >( treeNode );
					literalExpr.ExprType = ExpressionType.Literal;
					literalExpr.Value = literal.Value;
					literalExpr.Type = literal.Type;
					literalExpr.AddChild( literal );

					if ( isUnaryOperator ) {
						literalExpr.PreUnaryOperator = unaryOperator.Value;
					}
				}
			}
			else if ( child.Term.Name == "identifier" ) {
				if ( child.AstNode is null ) {
					CreateIdentifierNode( context, child );
				}
				if ( child.AstNode is Identifier identifier ) {
					var identifierExpr = Ast< IdentifierExpression >( treeNode );
					identifierExpr.ExprType = ExpressionType.Identifier;
					identifierExpr.Identifier = identifier;
					identifierExpr.AddChild( identifier );
				}
			}
			else if ( child.Term.Name == "expression" ) {
				// 处理括号表达式
				if ( child.AstNode is null ) {
					CreateExpressionNode( context, child );
				}
				// 这里我们不能简单替换，而是需要复制子表达式的属性到新的具体表达式对象中
				if ( child.AstNode is Expression subExpr ) {
					// 根据子表达式的类型，创建相应的表达式对象
					switch (subExpr.ExprType) {
						case ExpressionType.Literal:
							if (subExpr is LiteralExpression literalExpr) {
								var newLiteralExpr = Ast< LiteralExpression >( treeNode );
								newLiteralExpr.ExprType = ExpressionType.Literal;
								newLiteralExpr.Value = literalExpr.Value;
								newLiteralExpr.Type = literalExpr.Type;

								if ( isUnaryOperator ) {
									newLiteralExpr.PreUnaryOperator = unaryOperator.Value;
								}
							}
							break;
						case ExpressionType.Identifier:
							if (subExpr is IdentifierExpression identExpr) {
								var newIdentExpr = Ast< IdentifierExpression >( treeNode );
								newIdentExpr.ExprType = ExpressionType.Identifier;
								newIdentExpr.Identifier = identExpr.Identifier;
								newIdentExpr.AddChild( identExpr.Identifier );
							}
							break;
						case ExpressionType.Binary:
							if (subExpr is BinaryExpression binExpr) {
								var newBinExpr = Ast< BinaryExpression >( treeNode );
								newBinExpr.ExprType = ExpressionType.Binary;
								newBinExpr.Left = binExpr.Left;
								newBinExpr.Right = binExpr.Right;
								newBinExpr.Operator = binExpr.Operator;
								newBinExpr.AddChild( binExpr.Left );
								newBinExpr.AddChild( binExpr.Right );
							}
							break;
						default:
							// 对于其他类型，创建一个默认的表达式对象
							Ast< LiteralExpression >( treeNode );
							break;
					}
				}
			}
		}
		else if ( treeNode.ChildNodes.Count >= 3 ) {
			// 二元表达式处理
			// 检查是否是二元操作符格式 (expr op expr)
			if ( treeNode.ChildNodes[1].Token != null ) {
				string op = treeNode.ChildNodes[1].Token.ValueString;
				
				// 检查操作符是否是二元操作符
				if (op == "+" || op == "-" || op == "*" || op == "/" || op == "%" || 
					op == "==" || op == "!=" || op == "<" || op == ">" || op == "<=" || op == ">=" ||
					op == "&&" || op == "||" || op == "and" || op == "or") {
					
					// 创建左表达式
					if ( treeNode.ChildNodes[0].AstNode is null ) {
						CreateExpressionNode( context, treeNode.ChildNodes[0] );
					}
					
					// 创建右表达式
					if ( treeNode.ChildNodes[2].AstNode is null ) {
						CreateExpressionNode( context, treeNode.ChildNodes[2] );
					}
					
					// 创建二元表达式
					CreateBinaryExpressionNode( context, treeNode );
				}
			}
		}
	}

	private static void CreateBinaryExpressionNode( AstContext context, ParseTreeNode treeNode ) {
		BinaryOperator GetOperatorFromString( string op ) {
			switch ( op ) {
				case "+":  return BinaryOperator.Add;
				case "-":  return BinaryOperator.Subtract;
				case "*":  return BinaryOperator.Multiply;
				case "/":  return BinaryOperator.Divide;
				case "%":  return BinaryOperator.Modulo;
				case "==": return BinaryOperator.Equal;
				case "!=": return BinaryOperator.NotEqual;
				case "<":  return BinaryOperator.LessThan;
				case ">":  return BinaryOperator.GreaterThan;
				case "<=": return BinaryOperator.LessThanOrEqual;
				case ">=": return BinaryOperator.GreaterThanOrEqual;

				case "&&":
				case "and":
					return BinaryOperator.And;

				case "||":
				case "or":
					return BinaryOperator.Or;

				default: return BinaryOperator.Unknown;
			}
		}

		var node = Ast< BinaryExpression >( treeNode );
		node.ExprType = ExpressionType.Binary;

		if ( treeNode.ChildNodes.Count >= 3 ) {
			// 确保左操作数存在
			if ( treeNode.ChildNodes[ 0 ].AstNode is null ) {
				CreateExpressionNode( context, treeNode.ChildNodes[ 0 ] );
			}
			
			// 处理左操作数
			if ( treeNode.ChildNodes[ 0 ].AstNode is Expression leftExpr ) {
				node.Left = leftExpr;
				node.AddChild( leftExpr );
			}

			// 处理操作符
			if ( treeNode.ChildNodes[ 1 ].Token != null ) {
				string op = treeNode.ChildNodes[ 1 ].Token.ValueString;
				node.Operator = GetOperatorFromString( op );
			}

			// 确保右操作数存在
			if ( treeNode.ChildNodes[ 2 ].AstNode is null ) {
				CreateExpressionNode( context, treeNode.ChildNodes[ 2 ] );
			}
			
			// 处理右操作数
			if ( treeNode.ChildNodes[ 2 ].AstNode is Expression rightExpr ) {
				node.Right = rightExpr;
				node.AddChild( rightExpr );
			}
		}
	}

	private static void CreateConditionalExpressionNode( AstContext context, ParseTreeNode treeNode ) {
		var node = Ast< ConditionalExpression >( treeNode );
		node.ExprType = ExpressionType.Ternary;

		// 确保有足够的子节点
		if ( treeNode.ChildNodes.Count >= 3 ) {
			// 条件表达式
			if ( treeNode.ChildNodes[ 0 ].AstNode is null ) {
				CreateExpressionNode( context, treeNode.ChildNodes[ 0 ] );
			}
			if ( treeNode.ChildNodes[ 0 ].AstNode is Expression condition ) {
				node.Condition = condition;
				node.AddChild( condition );
			}

			// 真值表达式
			if ( treeNode.ChildNodes[ 1 ].AstNode is null ) {
				CreateExpressionNode( context, treeNode.ChildNodes[ 1 ] );
			}
			if ( treeNode.ChildNodes[ 1 ].AstNode is Expression trueExpr ) {
				node.TrueExpression = trueExpr;
				node.AddChild( trueExpr );
			}

			// 假值表达式
			if ( treeNode.ChildNodes[ 2 ].AstNode is null ) {
				CreateExpressionNode( context, treeNode.ChildNodes[ 2 ] );
			}
			if ( treeNode.ChildNodes[ 2 ].AstNode is Expression falseExpr ) {
				node.FalseExpression = falseExpr;
				node.AddChild( falseExpr );
			}
		}
	}

	private static void CreateStatement( AstContext context, ParseTreeNode treeNode ) {
		var node = Ast< Statement >( treeNode );

		// 空语句处理
		if ( treeNode.ChildNodes.Count == 0 ||
			 ( treeNode.ChildNodes.Count == 1 && treeNode.ChildNodes[ 0 ].Term.Name == "empty_line" ) ) {
			node.StatementType = StatementType.Empty;
			return;
		}

		// 检查语句类型并设置相应属性
		var firstChild = treeNode.ChildNodes[ 0 ];
		
		// 如果子节点AstNode为空，创建相应的AstNode
		if (firstChild.AstNode is null) {
			switch (firstChild.Term.Name) {
				case "context_define":
					CreateContextDefineNode(context, firstChild);
					break;
				case "conditions_define":
					CreateConditionsDefineNode(context, firstChild);
					break;
				case "default_consideration_define":
					CreateDefaultConsiderationDefineNode(context, firstChild);
					break;
				case "reasoner_define":
					CreateReasonerDefineNode(context, firstChild);
					break;
				case "var":
					CreateVariableNode(context, firstChild);
					break;
				case "const":
					CreateConstantNode(context, firstChild);
					break;
			}
		}

		if ( firstChild.AstNode is Node childNode ) {
			node.ChildNode = childNode;
			node.AddChild( childNode );

			if ( childNode is Ast.ContextDefine ) {
				node.StatementType = StatementType.Context;
			}
			else if ( childNode is Ast.ConditionsDefine ) {
				node.StatementType = StatementType.Conditions;
			}
			else if ( firstChild.Term.Name == "default_consideration_define" ) {
				node.StatementType = StatementType.DefaultConsideration;

				// 提取默认考虑项名称
				if ( treeNode.ChildNodes.Count >= 2 &&
					 treeNode.ChildNodes[ 1 ].Term.Name == "identifier" &&
					 treeNode.ChildNodes[ 1 ].Token != null ) {
					node.DefaultConsiderationName = treeNode.ChildNodes[ 1 ].Token.ValueString;
				}
			}
			else if ( childNode is Ast.ReasonerDefine ) {
				node.StatementType = StatementType.Reasoner;
			}
			else if ( childNode is Ast.Variable ) {
				node.StatementType = StatementType.Variable;
			}
			else if ( childNode is Ast.Constant ) {
				node.StatementType = StatementType.Constant;
			}
		}
	}

	private static void CreateContextDefineNode( AstContext context, ParseTreeNode treeNode ) {
		var node = Ast< ContextDefine >( treeNode );

		// 获取上下文类型
		if ( treeNode.ChildNodes.Count >= 2 ) {
			if ( treeNode.ChildNodes[ 1 ].AstNode is null && 
				 treeNode.ChildNodes[ 1 ].Term.Name == "qualified_name" ) {
				CreateQualifiedNameNode( context, treeNode.ChildNodes[ 1 ] );
			}
			
			if ( treeNode.ChildNodes[ 1 ].AstNode is QualifiedName contextType ) {
				node.ContextType = contextType;
				node.AddChild( contextType );
			}
		}
	}

	private static void CreateVariableNode( AstContext context, ParseTreeNode treeNode ) {
		var node = Ast< Variable >( treeNode );

		if ( treeNode.ChildNodes.Count >= 4 ) {
			// 获取变量名称
			var nameNode = treeNode.ChildNodes[ 1 ];
			if ( nameNode.AstNode is null && nameNode.Term.Name == "identifier" ) {
				CreateIdentifierNode( context, nameNode );
			}
			
			if ( nameNode.Token != null ) {
				node.Name = nameNode.Token.ValueString;
			}

			// 获取变量值表达式
			var valueNode = treeNode.ChildNodes[ 3 ];
			if ( valueNode.AstNode is null && valueNode.Term.Name == "expression" ) {
				CreateExpressionNode( context, valueNode );
			}
			
			if ( valueNode.AstNode is Node valueExpr ) {
				node.Value = valueExpr;
				node.AddChild( valueExpr );
			}
		}
	}

	private static void CreateConstantNode( AstContext context, ParseTreeNode treeNode ) {
		var node = Ast< Constant >( treeNode );

		if ( treeNode.ChildNodes.Count >= 4 ) {
			// 获取常量名称
			var nameNode = treeNode.ChildNodes[ 1 ];
			if ( nameNode.AstNode is null && nameNode.Term.Name == "identifier" ) {
				CreateIdentifierNode( context, nameNode );
			}
			
			if ( nameNode.Token != null ) {
				node.Name = nameNode.Token.ValueString;
			}

			// 获取常量值表达式
			var valueNode = treeNode.ChildNodes[ 3 ];
			if ( valueNode.AstNode is null && valueNode.Term.Name == "expression" ) {
				CreateExpressionNode( context, valueNode );
			}
			
			if ( valueNode.AstNode is Node valueExpr ) {
				node.Value = valueExpr;
				node.AddChild( valueExpr );
			}
		}
	}

	private static void CreateConditionsDefineNode( AstContext context, ParseTreeNode treeNode ) {
		var node = Ast< ConditionsDefine >( treeNode );

		foreach ( var child in treeNode.ChildNodes ) {
			if ( child.Term.Name == "condition_block" ) {
				if ( child.AstNode is null ) {
					CreateConditionBlockNode( context, child );
				}
				
				if ( child.AstNode is ConditionBlock conditionBlock ) {
					node.Conditions.Add( conditionBlock );
					node.AddChild( conditionBlock );
				}
			}
		}
	}

	private static void CreateConditionBlockNode( AstContext context, ParseTreeNode treeNode ) {
		var node = Ast< ConditionBlock >( treeNode );

		foreach ( var child in treeNode.ChildNodes ) {
			// 检查并创建子节点的AstNode（如果为空）
			if ( child.AstNode is null ) {
				switch ( child.Term.Name ) {
					case "appraisal_define":
						CreateAppraisalDefineNode( context, child );
						break;
					case "action_define":
						CreateActionDefineNode( context, child );
						break;
					case "condition_expression":
						CreateConditionExpressionNode( context, child );
						break;
				}
			}
			
			if ( child.AstNode is Node item ) {
				node.AddChild( item );

				// 注意：由于ConditionBlock类型没有ActionDefine属性，这里我们只添加子节点
				// 但不尝试设置ActionDefine属性
			}

			if ( child.AstNode is ConditionExpression conditionalExpression ) {
				node.ConditionExpressions.Add( conditionalExpression );
			}
		}
	}

	private static void CreateConditionExpressionNode( AstContext context, ParseTreeNode treeNode ) {
		var node = Ast< ConditionExpression >( treeNode );

		// 获取条件名称
		if ( treeNode.ChildNodes.Count >= 1 && treeNode.ChildNodes[ 0 ].Token != null ) {
			node.Name = treeNode.ChildNodes[ 0 ].Token.ValueString;
			
			// 创建标识符节点
			if ( treeNode.ChildNodes[ 0 ].AstNode is null && 
				 treeNode.ChildNodes[ 0 ].Term.Name == "identifier" ) {
				CreateIdentifierNode( context, treeNode.ChildNodes[ 0 ] );
			}
		}

		// 获取条件修饰符
		if ( treeNode.ChildNodes.Count >= 3 ) {
			if ( treeNode.ChildNodes[ 1 ].AstNode is null && 
				 treeNode.ChildNodes[ 1 ].Term.Name == "condition_list_modifier" ) {
				CreateConditionListModifierNode( context, treeNode.ChildNodes[ 1 ] );
			}
			
			if ( treeNode.ChildNodes[ 1 ].AstNode != null ) {
				string modifierText = treeNode.ChildNodes[ 1 ].ChildNodes[ 0 ].Token.ValueString;
				node.Modifier = modifierText == "All" ? ConditionListModifier.All : ConditionListModifier.Any;
			}

			if ( treeNode.ChildNodes[ 2 ].Term.Name == "condition_items?" ) {
				if ( treeNode.ChildNodes[ 2 ].AstNode is null ) {
					CreateConditionItemsNode( context, treeNode.ChildNodes[ 2 ] );
				}

				if ( treeNode.ChildNodes[ 2 ].AstNode is ConditionItems items ) {
					// 遍历子节点并添加
					foreach ( var item in items.Childrens() ) {
						if ( item is ConditionItem conditionItem ) {
							node.Items.Add( conditionItem );
							node.AddChild( conditionItem );
						}
					}
				}
			}
		}
		
	}

	private static void CreateConditionItemNode( AstContext context, ParseTreeNode treeNode ) {
		var node = Ast< ConditionItem >( treeNode );

		if ( treeNode.ChildNodes.Count == 1 ) {
			var child = treeNode.ChildNodes[ 0 ];
			
			// 检查并创建子节点
			if ( child.AstNode is null ) {
				if ( child.Term.Name == "identifier" ) {
					CreateIdentifierNode( context, child );
				}
				else if ( child.Term.Name == "expression" ) {
					CreateExpressionNode( context, child );
				}
			}
			
			if ( child.Term.Name == "identifier" && child.Token != null ) {
				// 简单标识符
				node.IsIdentifier = true;
				node.IdentifierValue = child.Token.ValueString;

				if ( child.AstNode is Node identifierNode ) {
					node.Content = identifierNode;
					node.AddChild( identifierNode );
				}
			}
			else if ( child.Term.Name == "expression" && child.AstNode is Node expressionNode ) {
				// 表达式
				node.IsIdentifier = false;
				node.Content = expressionNode;
				node.AddChild( expressionNode );
			}
		}
	}

	private static void CreateConditionItemsNode( AstContext context, ParseTreeNode treeNode ) {
		var node = Ast< ConditionItems >( treeNode );

		foreach ( var child in treeNode.ChildNodes ) {
			
			Ast< ConditionItems >( child );
			
			foreach ( var childChild in child.ChildNodes ) {
				if ( childChild.Term.Name == "condition_item" ) {
					// 如果子节点AstNode为空，则创建
					if ( childChild.AstNode is null ) {
						CreateConditionItemNode( context, childChild );
					}
				
					if ( childChild.AstNode is ConditionItem item ) {
						node.Items.Add( item );
						node.AddChild( item );
					}
				}
			}
		}
	}

	private static void CreateReasonerDefineNode( AstContext context, ParseTreeNode treeNode ) {
		var node = Ast< ReasonerDefine >( treeNode );

		foreach ( var child in treeNode.ChildNodes ) {
			// 识别Reasoner类型
			if ( child.Term.Name == "reasoner_type_name" ) {
				if ( child.ChildNodes.Count > 0 ) {
					var typeToken = child.ChildNodes[ 0 ].Token;
					if ( typeToken != null ) {
						node.ReasonerType = typeToken.ValueString;
						node.IsBuiltIn = node.ReasonerType == "+" ||
										 node.ReasonerType == "-" ||
										 node.ReasonerType == ">";
					}
				}

				if ( child.AstNode is null ) {
					CreateReasonerTypeNameNode( context, child );
				}

				if ( child.AstNode is ReasonerTypeName typeNode ) {
					node.AddChild( typeNode );
				}
			}

			// 收集考虑项定义
			else if ( child.Term.Name == "reasoner_block" ) {
				if ( child.AstNode is null ) {
					CreateReasonerBlockNode( context, child );
				}
				
				if ( child.AstNode is Node blockNode ) {
					node.AddChild( blockNode );

					// 添加所有考虑项
					foreach ( var consideration in blockNode.Childrens() ) {
						node.Considerations.Add( consideration );
					}
				}
			}
		}
	}

	private static void CreateReasonerTypeNameNode( AstContext context, ParseTreeNode treeNode ) {
		var node = Ast< ReasonerTypeName >( treeNode );

		if ( treeNode.ChildNodes.Count > 0 ) {
			var firstChild = treeNode.ChildNodes[ 0 ];
			if ( firstChild.Token != null ) {
				// 处理内置类型
				node.TypeName = firstChild.Token.ValueString;
				node.IsBuiltIn = node.TypeName == "+" || node.TypeName == "-" || node.TypeName == ">";
			}
			else if ( firstChild.Term.Name == "qualified_name" ) {
				// 如果限定名称节点为空，则创建
				if ( firstChild.AstNode is null ) {
					CreateQualifiedNameNode( context, firstChild );
				}
				
				if ( firstChild.AstNode is QualifiedName qualifiedName ) {
					// 处理自定义类型
					node.QualifiedName = qualifiedName;
					node.TypeName = qualifiedName.FullName;
					node.IsBuiltIn = false;
					node.AddChild( qualifiedName );
				}
			}
		}
	}

	private static void CreateReasonerBlockNode( AstContext context, ParseTreeNode treeNode ) {
		var node = Ast< ReasonerBlock >( treeNode );

		foreach ( var child in treeNode.ChildNodes ) {
			if ( child.Term.Name == "consideration_define" ) {
				// 如果考虑项定义节点为空，则创建
				if ( child.AstNode is null ) {
					CreateConsiderationDefineNode( context, child );
				}
				
				if ( child.AstNode is Node consideration ) {
					node.ConsiderationDefinitions.Add( consideration );
					node.AddChild( consideration );
				}
			}
		}
	}

	private static void CreateConsiderationDefineNode( AstContext context, ParseTreeNode treeNode ) {
		var node = Ast< ConsiderationDefine >( treeNode );

		if ( treeNode.ChildNodes.Count >= 4 ) {
			// 获取考虑项名称
			if ( treeNode.ChildNodes[ 0 ].Term.Name == "identifier" &&
				 treeNode.ChildNodes[ 0 ].Token != null ) {
				node.Name = treeNode.ChildNodes[ 0 ].Token.ValueString;

				if ( treeNode.ChildNodes[ 0 ].AstNode is null ) {
					CreateIdentifierNode( context, treeNode.ChildNodes[ 0 ] );
				}
				
				if ( treeNode.ChildNodes[ 0 ].AstNode is Identifier identifier ) {
					node.AddChild( identifier );
				}
			}

			if ( treeNode.ChildNodes[ 1 ].Term.Name == "qualified_name" ) {
				if ( treeNode.ChildNodes[ 1 ].AstNode is null ) {
					CreateQualifiedNameNode( context, treeNode.ChildNodes[ 1 ] );
				}
				
				// 获取考虑项类型
				if ( treeNode.ChildNodes[ 1 ].AstNode is QualifiedName type ) {
					node.Type = type;
					node.AddChild( type );
				}
			}
			
			if ( treeNode.ChildNodes[ 2 ].Term.Name == "param_list_optional?" ) {
				if ( treeNode.ChildNodes[ 2 ].ChildNodes.Count > 0 &&
					 treeNode.ChildNodes[ 2 ].ChildNodes[ 0 ].AstNode is null ) {
					CreateParamListNode( context, treeNode.ChildNodes[ 2 ].ChildNodes[ 0 ] );
				}
				if ( treeNode.ChildNodes[ 2 ].ChildNodes.Count > 0 &&
					 treeNode.ChildNodes[ 2 ].ChildNodes[ 0 ].AstNode is ParamList paramList ) {
					node.Parameters = paramList;
					node.AddChild( paramList );
				}
			}
			
			if ( treeNode.ChildNodes[ 3 ].Term.Name == "consideration_block" ) {
				if ( treeNode.ChildNodes[ 3 ].AstNode is null ) {
					CreateConsiderationBlockNode( context, treeNode.ChildNodes[ 3 ] );
				}
				if ( treeNode.ChildNodes[ 3 ].AstNode is ConsiderationBlock considerationBlock ) {
					node.Block = considerationBlock;
					node.AddChild( considerationBlock );
				}
			}
		}
	}

	private static void CreateConsiderationBlockNode( AstContext context, ParseTreeNode treeNode ) {
		var node = Ast< ConsiderationBlock >( treeNode );

		foreach ( var child in treeNode.ChildNodes ) {
			// 检查子节点并根据其类型创建相应的AST节点
			if ( child.AstNode is null ) {
				switch ( child.Term.Name ) {
					case "appraisal_define":
						CreateAppraisalDefineNode( context, child );
						break;
					case "action_define":
						CreateActionDefineNode( context, child );
						break;
					case "expression":
						CreateExpressionNode( context, child );
						break;
					case "literal":
						CreateLiteralNode( context, child );
						break;
				}
			}
			
			if ( child.AstNode is Node item ) {
				node.Items.Add( item );
				node.AddChild( item );

				// 识别特定类型
				if ( child.AstNode is AppraisalDefine appraisalDefine ) {
					node.AppraisalDefines.Add( appraisalDefine );
				}
				else if ( child.AstNode is ActionDefine actionDefine ) {
					node.ActionDefine = actionDefine;
				}
			}
		}
	}

	private static void CreateDefaultConsiderationDefineNode( AstContext context, ParseTreeNode treeNode ) {
		var node = Ast< DefaultConsiderationDefine >( treeNode );

		// 获取考虑项名称
		if ( treeNode.ChildNodes.Count >= 2 &&
			 treeNode.ChildNodes[ 1 ].Term.Name == "identifier" &&
			 treeNode.ChildNodes[ 1 ].Token != null ) {
			node.ConsiderationName = treeNode.ChildNodes[ 1 ].Token.ValueString;
			
			CreateIdentifierNode( context, treeNode.ChildNodes[ 1 ] );
			
			if ( treeNode.ChildNodes[ 1 ].AstNode is Identifier identifierNode ) {
				node.AddChild( identifierNode );
				node.Identifier = identifierNode;
			}
		}
	}

	private static void CreateAppraisalDefineNode( AstContext context, ParseTreeNode treeNode ) {
		var node = Ast< AppraisalDefine >( treeNode );

		// 获取评估表达式
		if ( treeNode.ChildNodes.Count >= 1 ) {
			var expressionNode = treeNode.ChildNodes[ 0 ];
			
			// 检查子节点并根据类型创建相应的AST节点
			if ( expressionNode.AstNode is null ) {
				switch ( expressionNode.Term.Name ) {
					case "ternary_expression": 
						CreateConditionalExpressionNode( context, expressionNode ); 
						break;
					case "boolean_expression": 
						CreateBooleanExpressionNode( context, expressionNode ); 
						break;
					case "complex_appraisal_expression": 
						CreateComplexAppraisalExpressionNode( context, expressionNode ); 
						break;
					case "expression":
						CreateExpressionNode( context, expressionNode );
						break;
				}
			}
			
			if ( expressionNode.AstNode is Node expr ) {
				node.Expression = expr;
				node.AddChild( expr );
			}
		}
	}

	private static void CreateActionDefineNode( AstContext context, ParseTreeNode treeNode ) {
		var node = Ast< ActionDefine >( treeNode );

		// 获取行动名称
		if ( treeNode.ChildNodes.Count >= 1 &&
			 treeNode.ChildNodes[ 0 ].Term.Name == "identifier" &&
			 treeNode.ChildNodes[ 0 ].Token != null ) {
			node.ActionName = treeNode.ChildNodes[ 0 ].Token.ValueString;

			// 创建标识符节点（如果为空）
			if ( treeNode.ChildNodes[ 0 ].AstNode is null ) {
				CreateIdentifierNode( context, treeNode.ChildNodes[ 0 ] );
			}

			if ( treeNode.ChildNodes[ 0 ].AstNode is Identifier identifierNode ) {
				node.AddChild( identifierNode );
			}
		}
	}

	private static void CreateBooleanExpressionNode( AstContext context, ParseTreeNode treeNode ) {
		var node = Ast< BooleanExpression >( treeNode );
		node.ExprType = ExpressionType.Boolean;

		// 检查子节点获取条件标识符 (是否带有?)
		if ( treeNode.ChildNodes.Count > 0 ) {
			int identifierIndex = 0;
			bool isNegated = false;

			// 检查是否有问号(否定符)
			if ( treeNode.ChildNodes[ 0 ].Term?.Name == "?" && treeNode.ChildNodes.Count > 1 ) {
				isNegated = true;
				identifierIndex = 1;
			}

			node.IsNegated = isNegated;

			// 创建标识符节点（如果为空）
			if ( treeNode.ChildNodes[ identifierIndex ].AstNode is null && 
				 treeNode.ChildNodes[ identifierIndex ].Term.Name == "identifier" ) {
				CreateIdentifierNode( context, treeNode.ChildNodes[ identifierIndex ] );
			}

			// 获取标识符
			if ( treeNode.ChildNodes[ identifierIndex ].AstNode is Identifier identifier ) {
				node.Identifier = identifier;
				node.AddChild( identifier );
			}
		}
	}

	private static void CreateComplexAppraisalExpressionNode( AstContext context, ParseTreeNode treeNode ) {
		var node = Ast< ComplexAppraisalExpression >( treeNode );
		node.ExprType = ExpressionType.Complex;

		// complexAppraisalExpression.Rule = identifier + ":" + qualifiedName + "{" + jsonObject + Eos.Opt() + "}";
		if ( treeNode.ChildNodes.Count >= 5 ) {
			// 获取名称
			if ( treeNode.ChildNodes[ 0 ].AstNode is null && 
				 treeNode.ChildNodes[ 0 ].Term.Name == "identifier" ) {
				CreateIdentifierNode( context, treeNode.ChildNodes[ 0 ] );
			}
			
			if ( treeNode.ChildNodes[ 0 ].AstNode is Identifier nameIdentifier ) {
				node.Name = nameIdentifier.Name;
				node.AddChild( nameIdentifier );
			}

			// 获取类型名称
			if ( treeNode.ChildNodes[ 2 ].AstNode is null && 
				 treeNode.ChildNodes[ 2 ].Term.Name == "qualified_name" ) {
				CreateQualifiedNameNode( context, treeNode.ChildNodes[ 2 ] );
			}
			
			if ( treeNode.ChildNodes[ 2 ].AstNode is QualifiedName typeName ) {
				node.TypeName = typeName.FullName;
				node.AddChild( typeName );
			}

			// 获取参数对象
			if ( treeNode.ChildNodes[ 4 ].AstNode is null && 
				 treeNode.ChildNodes[ 4 ].Term.Name == "json_object" ) {
				CreateJsonObjectNode( context, treeNode.ChildNodes[ 4 ] );
			}
			
			if ( treeNode.ChildNodes[ 4 ].AstNode is JsonObject jsonObject ) {
				node.AddChild( jsonObject );

				// 处理JSON对象中的所有键值对
				foreach ( var pair in jsonObject.Pairs ) {
					node.Parameters[ pair.Key ] = pair.Value;
				}
			}
		}
	}

	private static void CreateLiteralExpressionNode( AstContext context, ParseTreeNode treeNode ) {
		var node = Ast< LiteralExpression >( treeNode );
		node.ExprType = ExpressionType.Literal;

		// 从Literal节点获取类型和值
		if ( treeNode.ChildNodes.Count > 0 &&
			 treeNode.ChildNodes[ 0 ].AstNode is Literal literalNode ) {
			node.Value = literalNode.Value;
			node.Type = literalNode.Type;
			node.AddChild( literalNode );
		}
	}

	private static void CreateIdentifierExpressionNode( AstContext context, ParseTreeNode treeNode ) {
		var node = Ast< IdentifierExpression >( treeNode );
		node.ExprType = ExpressionType.Identifier;

		// 从子节点获取标识符
		if ( treeNode.ChildNodes.Count > 0 && treeNode.ChildNodes[ 0 ].AstNode is Identifier identifier ) {
			node.Identifier = identifier;
			node.AddChild( identifier );
		}

		// 直接从token获取标识符
		else if ( treeNode.Token != null && treeNode.Term.Name == "identifier" ) {
			var tidentifier = new Identifier { Name = treeNode.Token.ValueString };
			node.Identifier = tidentifier;
			node.AddChild( tidentifier );
		}
	}

	private static void CreateConditionListModifierNode(AstContext context, ParseTreeNode treeNode) {
		var node = Ast<ConditionListModifierNode>(treeNode);
		
		// 设置修饰符类型 (All 或 Any)
		if (treeNode.Token != null) {
			string modifierValue = treeNode.Token.ValueString;
			node.Modifier = modifierValue == "All" ? ConditionListModifier.All : ConditionListModifier.Any;
		}
	}

	private static void CreateParamListNode( AstContext context, ParseTreeNode treeNode ) {
		var node = Ast< ParamList >( treeNode );

		// 处理所有命名参数子节点
		foreach ( var child in treeNode.ChildNodes ) {
			if ( child.Term.Name == "named_param" ) {
				// 如果参数节点为空，则创建
				if ( child.AstNode is null ) {
					CreateNamedParamNode( context, child );
				}
				
				if ( child.AstNode is NamedParam paramNode ) {
					node.Parameters.Add( paramNode );
					node.AddChild( paramNode );
				}
			}
		}
	}

	private static void CreateNamedParamNode( AstContext context, ParseTreeNode treeNode ) {
		var node = Ast< NamedParam >( treeNode );

		// namedParam.Rule = identifier + ":" + expression;
		if ( treeNode.ChildNodes.Count >= 2 ) {
			// 处理参数名称（标识符）
			if ( treeNode.ChildNodes[ 0 ].AstNode is null && 
				 treeNode.ChildNodes[ 0 ].Term.Name == "identifier" ) {
				CreateIdentifierNode( context, treeNode.ChildNodes[ 0 ] );
			}
			
			if ( treeNode.ChildNodes[ 0 ].AstNode is Identifier nameNode ) {
				node.Name = nameNode.Name;
			}

			// 处理参数值（表达式）
			if ( treeNode.ChildNodes[ 1 ].AstNode is null && 
				 treeNode.ChildNodes[ 1 ].Term.Name == "expression" ) {
				CreateExpressionNode( context, treeNode.ChildNodes[ 1 ] );
			}
			
			if ( treeNode.ChildNodes[ 1 ].AstNode is Node valueNode ) {
				node.Value = valueNode;
				node.AddChild( valueNode );
			}
		}
	}

	private static void CreateJsonObjectNode( AstContext context, ParseTreeNode treeNode ) {
		var node = Ast< JsonObject >( treeNode );

		// 添加所有JSON键值对作为子节点
		foreach ( var child in treeNode.ChildNodes ) {
			if ( child.Term.Name == "json_pair" ) {
				// 如果JSON键值对节点为空，则创建
				if ( child.AstNode is null ) {
					CreateJsonPairNode( context, child );
				}
				
				if ( child.AstNode is JsonPair pairNode ) {
					node.Pairs.Add( pairNode );
					node.AddChild( pairNode );
				}
			}
		}
	}

	private static void CreateJsonPairNode( AstContext context, ParseTreeNode treeNode ) {
		var node = Ast< JsonPair >( treeNode );

		// jsonPair.Rule = identifier + ":" + jsonValue;
		if ( treeNode.ChildNodes.Count >= 3 ) {
			// 处理键名（标识符）
			if ( treeNode.ChildNodes[ 0 ].AstNode is null && 
				 treeNode.ChildNodes[ 0 ].Term.Name == "identifier" ) {
				CreateIdentifierNode( context, treeNode.ChildNodes[ 0 ] );
			}
			
			if ( treeNode.ChildNodes[ 0 ].AstNode is Identifier keyNode ) {
				node.Key = keyNode.Name;
			}

			// 处理值
			if ( treeNode.ChildNodes[ 2 ].AstNode is null && 
				 treeNode.ChildNodes[ 2 ].Term.Name == "json_value" ) {
				CreateJsonValueNode( context, treeNode.ChildNodes[ 2 ] );
			}
			
			if ( treeNode.ChildNodes[ 2 ].AstNode is Node valueNode ) {
				node.Value = valueNode;
				node.AddChild( valueNode );
			}
		}
	}

	private static void CreateJsonValueNode( AstContext context, ParseTreeNode treeNode ) {
		var node = Ast< JsonValue >( treeNode );

		// 处理子节点，可能是literal、expression、jsonObject或jsonArray
		if ( treeNode.ChildNodes.Count > 0 ) {
			var valueChild = treeNode.ChildNodes[ 0 ];
			
			// 检查子节点并根据其类型创建相应的AST节点
			if ( valueChild.AstNode is null ) {
				switch ( valueChild.Term.Name ) {
					case "literal":
						CreateLiteralNode( context, valueChild );
						break;
					case "expression":
						CreateExpressionNode( context, valueChild );
						break;
					case "json_object":
						CreateJsonObjectNode( context, valueChild );
						break;
					case "json_array":
						CreateJsonArrayNode( context, valueChild );
						break;
				}
			}
			
			if ( valueChild.AstNode is Node valueNode ) {
				node.Value = valueNode;
				node.AddChild( valueNode );

				// 确定值类型
				if ( valueNode is Literal ) {
					node.ValueType = JsonValueType.Literal;
				}
				else if ( valueNode is Expression ) {
					node.ValueType = JsonValueType.Expression;
				}
				else if ( valueNode is JsonObject ) {
					node.ValueType = JsonValueType.Object;
				}
				else if ( valueNode is JsonArray ) {
					node.ValueType = JsonValueType.Array;
				}
			}
		}
	}

	private static void CreateJsonArrayNode( AstContext context, ParseTreeNode treeNode ) {
		var node = Ast< JsonArray >( treeNode );

		// jsonArray.Rule = "[" + jsonItemsStarRule + "]";
		if ( treeNode.ChildNodes.Count >= 2 ) {
			// 获取数组项
			if ( treeNode.ChildNodes[ 1 ].AstNode is null && 
				 treeNode.ChildNodes[ 1 ].Term.Name == "json_items_star_rule" ) {
				CreateJsonItemsStarRuleNode( context, treeNode.ChildNodes[ 1 ] );
			}
			
			if ( treeNode.ChildNodes[ 1 ].AstNode is JsonItemsStarRule itemsNode ) {
				// 添加所有数组项作为子节点
				foreach ( var item in itemsNode.Childrens() ) {
					node.Items.Add( item );
					node.AddChild( item );
				}
			}
		}
	}

	private static void CreateJsonItemsStarRuleNode( AstContext context, ParseTreeNode treeNode ) {
		var node = Ast< JsonItemsStarRule >( treeNode );

		// 处理数组内所有项目
		foreach ( var child in treeNode.ChildNodes ) {
			// 跳过逗号
			if ( child.Term?.Name == "," ) {
				continue;
			}

			// 检查并创建子节点
			if ( child.AstNode is null && child.Term.Name == "json_value" ) {
				CreateJsonValueNode( context, child );
			}

			if ( child.AstNode is Node itemNode ) {
				node.Items.Add( itemNode );
				node.AddChild( itemNode );
			}
		}
	}

}
