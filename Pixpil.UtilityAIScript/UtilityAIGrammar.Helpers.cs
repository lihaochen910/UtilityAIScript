using Irony.Ast;
using Irony.Parsing;


namespace Pixpil.AI.UtilityAIScript;

public partial class UtilityAIGrammar {

	private static NonTerminal T( string name ) => new ( name );

	private static NonTerminal T( string name, AstNodeCreator nodeCreator ) => new ( name, nodeCreator );

	private static NonTerminal TT( string name ) {
		var nonTerminal = T( name );
		nonTerminal.Flags = TermFlags.IsTransient | TermFlags.NoAstNode;
		return nonTerminal;
	}

}
