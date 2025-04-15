using Irony.Parsing;


namespace Pixpil.AI.UtilityAIScript;

/// <summary>
/// Extensions to BnfTerm.
/// </summary>
public static class BnfTermExtensions {
	
	/// <summary>
	/// Makes a non terminal optional.
	/// </summary>
	/// <param name="term">The term.</param>
	/// <returns>An optional non terminal.</returns>
	public static NonTerminal Opt( this BnfTerm term ) {
		var nonTerminal = new NonTerminal( term.Name + "?" );
		nonTerminal.Rule = term | Grammar.CurrentGrammar.Empty;
		nonTerminal.SetFlag( TermFlags.NoAstNode );
		return nonTerminal;
	}

	/// <summary>
	/// Makes a list of non terminals.
	/// </summary>
	/// <param name="term">The term.</param>
	/// <returns>A list of non temrinal</returns>
	public static NonTerminal List( this BnfTerm term ) {
		var nonTerminal = new NonTerminal( term.Name + "+" );
		nonTerminal.Rule = Grammar.CurrentGrammar.MakePlusRule( nonTerminal, term );
		nonTerminal.SetFlag( TermFlags.NoAstNode );
		return nonTerminal;
	}

	/// <summary>
	/// Makes an optional list of non terminals.
	/// </summary>
	/// <param name="term">The term.</param>
	/// <returns>An optional list of non terminals.</returns>
	public static NonTerminal ListOpt( this BnfTerm term ) {
		var nonTerminal = new NonTerminal( term.Name + "*" );
		nonTerminal.Rule = Grammar.CurrentGrammar.MakeStarRule( nonTerminal, term );
		nonTerminal.SetFlag( TermFlags.NoAstNode );
		return nonTerminal;
	}

}
