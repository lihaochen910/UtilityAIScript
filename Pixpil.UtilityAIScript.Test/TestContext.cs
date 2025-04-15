using Xunit.Abstractions;


namespace Pixpil.AI.UtilityAIScript.Test;

public class TestContext ( ITestOutputHelper output ) : GrammarUnitTestBase( output ) {

	[Fact]
	public void TestContext_01() {
		TestParseSource( @"
context: MyContext
" );
	}
	
	[Fact]
	public void TestContext_02() {
		TestParseSource( @"
context: Namespace.MyContext
" );
	}
	
	[Fact]
	public void TestContext_03() {
		TestParseSource( @"
context: Namespace.A.B.MyContext
" );
	}
	
}
