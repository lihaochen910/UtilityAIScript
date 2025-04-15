using Xunit.Abstractions;


namespace Pixpil.AI.UtilityAIScript.Test;

public class TestDumpReasoner( ITestOutputHelper output ) : GrammarUnitTestBase( output ) {

	[Fact]
	public void TestDump() {
		
		// 创建编译器
		var compiler = new UtilityAICompiler< TestUtilityAICompiler.AIContext >();
		
		compiler.RegisterCondition( "can_see_enemy", ctx => ctx.CanSeeEnemy );
		compiler.RegisterCondition( "is_hungry", ctx => ctx.IsHungry );
		
		compiler.RegisterProperty( "fatigue", ctx => ctx.Fatigue );
		compiler.RegisterProperty( "distance", ctx => ctx.Distance );
		
		compiler.RegisterAction( "do_patrol", ctx => { ctx.LastAction = "Patrol"; } );
		compiler.RegisterAction( "do_chase", ctx => { ctx.LastAction = "Chase"; } );
		compiler.RegisterAction( "do_idle", ctx => { ctx.LastAction = "Idle"; } );
		
		var script = @"
default: Idle

+
    = Patrol : AllOrNothing
        !can_see_enemy ? 1 : 0

        ->
            do_patrol

    = Chase : AllOrNothing
        can_see_enemy ? 1 : 0
        fatigue < 5 ? 0.3 : 0

        ->
            do_chase

    = Idle : FixedScore( score: 0.5 )
        ->
            do_idle
";

		var reasoner = compiler.CompileAndBuild( script );
		var context = new TestUtilityAICompiler.AIContext { CanSeeEnemy = false, Fatigue = 3.0f, Distance = 8.0f };
		var action = reasoner.Select( context );
		Assert.NotNull( action );
		
#if DEBUG
		Log( reasoner.DebugDump() );
#endif
		
	}
	
}
