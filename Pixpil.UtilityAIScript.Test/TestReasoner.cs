using Xunit.Abstractions;


namespace Pixpil.AI.UtilityAIScript.Test;

public class TestReasoner ( ITestOutputHelper output ) : GrammarUnitTestBase( output ) {
	
	[Fact]
	public void TestReasoner_01() {
		TestParseSource( @"
+
	= Patrol : AllOrNothing
		!can_see_enemy ? 1 : 0

		->
			do_patrol
" );
	}
	
	[Fact]
	public void TestReasoner_02() {
		TestParseSource( @"
-
	= Patrol : AllOrNothing()
		!can_see_enemy ? 1 : 0

		->
			do_patrol
" );
	}
	
	[Fact]
	public void TestReasoner_03() {
		TestParseSource( @"
Namespace.A.CustomReasoner

	= Patrol : FixedScore( score: 1 )
		!can_see_enemy ? 1 : 0

		->
			do_something
" );
	}
	
	[Fact]
	public void TestReasoner_04() {
		TestParseSource( @"
Namespace.A.CustomReasoner

	= Patrol : FixedScore( score: 1 )

		// begin Appraisal
		random_value : RandomScoreInRange {
			min : -1.0,
			max : 1.0
		}
		// end Appraisal

		->
			do_something
" );
	}
	
	[Fact]
	public void TestReasoner_05() {
		TestParseSource( @"
Namespace.A.CustomReasoner

	= Patrol : FixedScore( score: 1 )

		// begin Appraisal
		out_of_range : SumOfChildrenWithPreAppraisals { 
			Threshold: 1,
			PreCheckMode: AllRequired,
			OptionalAppraisals: [],
			Appraisals: [
				!can_see_enemy ? 0 : 1.0 // 内嵌打分器表达式
			]
		}
		// end Appraisal

		->
			do_something
" );
	}
	
}
