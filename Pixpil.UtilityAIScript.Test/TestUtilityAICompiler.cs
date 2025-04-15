using Xunit.Abstractions;

namespace Pixpil.AI.UtilityAIScript.Test;

public class TestUtilityAICompiler {
    
    private readonly ITestOutputHelper _output;

    public TestUtilityAICompiler( ITestOutputHelper output ) {
        _output = output;
    }


    /// <summary>
    /// 示例上下文类
    /// </summary>
    public class AIContext {
        public float Fatigue { get; set; }
        public float Distance { get; set; }
        public bool CanSeeEnemy { get; set; }
        public bool IsHungry { get; set; }
        public string LastAction { get; set; }
    }


    private UtilityAICompiler< AIContext > CreateSampleCompiler() {
        // 创建编译器
        var compiler = new UtilityAICompiler< AIContext >();

        // 注册条件解析器
        compiler.RegisterCondition( "can_see_enemy", ctx => ctx.CanSeeEnemy );
        compiler.RegisterCondition( "is_hungry", ctx => ctx.IsHungry );
        
        // 注册属性访问器
        compiler.RegisterProperty( "fatigue", ctx => ctx.Fatigue );
        compiler.RegisterProperty( "distance", ctx => ctx.Distance );
        
        return compiler;
    }
    


    [Fact]
    public void TestCompileHighestScoreReasoner() {
        
        // 创建编译器
        var compiler = CreateSampleCompiler();
        
        // 注册动作解析器
        compiler.RegisterAction( "do_patrol", ctx => { ctx.LastAction = "Patrol"; } );
        compiler.RegisterAction( "do_chase", ctx => { ctx.LastAction = "Chase"; } );
        compiler.RegisterAction( "do_idle", ctx => { ctx.LastAction = "Idle"; } );

        // 编译脚本
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

        // 创建上下文并测试执行
        var context = new AIContext { CanSeeEnemy = false, Fatigue = 3.0f, Distance = 8.0f };

        // 执行推理
        var action = reasoner.Select( context );
        Assert.NotNull( action );

        // 执行动作
        action?.Execute( context );
        Assert.Equal( "Patrol", context.LastAction );

        // 改变条件
        context.CanSeeEnemy = true;
        context.LastAction = null;

        // 再次执行推理
        action = reasoner.Select( context );
        action?.Execute( context );

        // 不应该执行巡逻，因为看到敌人
        Assert.Equal( "Chase", context.LastAction );

        _output.WriteLine( "测试成功完成！" );
    }

    [Fact]
    public void TestCompileComplexReasoner() {
        
        // 创建编译器
        var compiler = CreateSampleCompiler();
        
        // 注册动作解析器
        compiler.RegisterAction("do_patrol", ctx => { ctx.LastAction = "Patrol"; });
        compiler.RegisterAction("do_attack", ctx => { ctx.LastAction = "Attack"; });
        compiler.RegisterAction("do_eat", ctx => { ctx.LastAction = "Eat"; });
        
        // 编译脚本
        var script = @"
+
= Patrol : AllOrNothing
    !can_see_enemy ? 1 : 0
    fatigue < 8 ? 1 : 0

    ->
        do_patrol
        
= Attack : AllOrNothingFixed( score: 1 )
    can_see_enemy ? 1 : 0
    distance < 5 ? 1 : 0

    ->
        do_attack
        
= Eat : AllOrNothing
    is_hungry ? 1 : 0
    fatigue > 5 ? 1 : 0

    ->
        do_eat
";
        
        var reasoner = compiler.CompileAndBuild(script);
        
        // 测试场景1：巡逻
        var context = new AIContext {
            CanSeeEnemy = false,
            IsHungry = false,
            Fatigue = 3.0f,
            Distance = 8.0f
        };
        
        var action = reasoner.Select(context);
        Assert.NotNull(action);
        action.Execute(context);
        Assert.Equal("Patrol", context.LastAction);
        
        // 测试场景2：攻击
        context.LastAction = null;
        context.CanSeeEnemy = true;
        context.Distance = 3.0f;
        
        action = reasoner.Select(context);
        Assert.NotNull(action);
        action.Execute(context);
        Assert.Equal("Attack", context.LastAction);
        
        // 测试场景3：吃东西
        context.LastAction = null;
        context.CanSeeEnemy = false;
        context.IsHungry = true;
        context.Fatigue = 7.0f;
        
        action = reasoner.Select(context);
        Assert.NotNull(action);
        action.Execute(context);
        Assert.Equal("Eat", context.LastAction);
        
        _output.WriteLine("复杂测试成功完成！");
    }

    [Fact]
    public void TestWithPreConditions() {
        
        var compiler = CreateSampleCompiler();
        compiler.RegisterAction("do_patrol", ctx => { ctx.LastAction = "Patrol"; });
        compiler.RegisterAction("do_eat", ctx => { ctx.LastAction = "Eat"; });
        compiler.RegisterAction("do_idle", ctx => { ctx.LastAction = "Idle"; });
        
        var script = @"
default: Idle

conditions:
    hungry_and_tired: All [
        is_hungry
        fatigue > 5.0
	]

    hungry_or_tired: Any [
        is_hungry
        fatigue > 5.0
	]

    not_hungry: All [
        !is_hungry
	]

    can_see_enemy_from_long_distance: All [
        can_see_enemy
        distance > 5
    ]

+
    = Patrol : Threshold( threshold: 0.8 )
        fatigue < 5 ? 1 : 0
        !can_see_enemy ? 1 : 0

        ->
            do_patrol

    = Eat : AllOrNothingFixed( threshold: 0.5, score: 3 )
        // hungry_and_tired ? 1 : 0
        not_hungry ? 0 : 1

        ->
            do_eat

    = Idle : FixedScore( score: 0.5 )
        ->
            do_idle
";

        var reasoner = compiler.CompileAndBuild( script, "sample.ai_script" );

        // 创建上下文并测试执行
        var context = new AIContext {
            CanSeeEnemy = false,
            Fatigue = 3.0f
        };
        
        var action = reasoner.Select( context );
        Assert.NotNull( action );
        
        action.Execute( context );
        Assert.Equal( "Patrol", context.LastAction );

        context.IsHungry = true;
        context.Fatigue = 6f;

        action = reasoner.Select( context );
        Assert.NotNull( action );
        
        action.Execute( context );
        Assert.Equal( "Eat", context.LastAction );
    }
    
}
