using Xunit.Abstractions;


namespace Pixpil.AI.UtilityAIScript.Test;

public class TestPreConditions ( ITestOutputHelper output ) : GrammarUnitTestBase( output ) {
	
	[Fact]
	public void TestPreCondition_01() {
		TestParseSource( @"
conditions:
	cond_01: Any [
	]
" );
	}
	
	[Fact]
	public void TestPreCondition_02() {
		TestParseSource( @"
conditions:
	cond_02: Any []
" );
	}
	
	[Fact]
	public void TestPreCondition_03() {
		TestParseSource( @"
conditions:
	composite_cond: Any [
		dis < 10.0
		dis > 5.0
		dis >= 1
		dis <= 0
		dis != 2
		dis == 3
		bool_val == true
		bool_val == false
		!bool_val == true
		bool_val
		!bool_val
	]
" );
	}
	
	[Fact]
	public void TestPreCondition_04() {
		TestParseSource( @"
conditions:
	can_happy: Any [
		fatigue < 5.0
		happy >= 10.0
	]
" );
	}

	[Fact]
	public void TestPreCondition_05() {
		TestParseSource( @"
conditions:
	can_see_player: All [
		distance < 10.0
		condi_0
	]

	can_do: Any [
		fatigue < 5.0
	]
" );
	}

	[Fact]
	public void TestPreCondition_06() {
		TestParseSource( @"
conditions:
	composite_cond: All [
		dis < 10.0
		dis > 5.0
		dis >= 1
		dis <= 0
		dis != 2
		dis == 3
		bool_val == true
		bool_val == false
		!bool_val == true
		bool_val
		!bool_val
	]
" );
	}
	
}