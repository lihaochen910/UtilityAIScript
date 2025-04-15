using Xunit.Abstractions;


namespace Pixpil.AI.UtilityAIScript.Test;

public class TestComment ( ITestOutputHelper output ) : GrammarUnitTestBase( output ) {

	[Fact]
	public void TestComment_01() {
		TestParseSource( @"
// Empty Script
" );
	}
	
	[Fact]
	public void TestComment_02() {
		TestParseSource( @"
/* 
Empty Script
*/
" );
	}
	
}