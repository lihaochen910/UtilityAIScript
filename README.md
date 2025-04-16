# UtilityAIScript

受到mock BTScript的启发，基于Nez.AI.UtilityAI修改的UtilityAI脚本语言

## 目录

- [简介](#简介)
- [语法元素](#语法元素)
  - [Context定义](#context定义)
  - [Conditions定义](#conditions定义)
  - [Reasoner定义](#reasoner定义)
  - [Consideration定义](#consideration定义)
- [使用示例](#使用示例)
  - [脚本示例](#脚本示例)
  - [运行时使用](#运行时使用)
- [调试技巧](#调试技巧)
- [未实现功能](#未实现功能)

## 简介

UtilityAIScript是一种专门用于实现基于效用理论的AI决策系统的脚本语言。它简化了复杂AI行为的定义过程，使开发者能够直观地描述AI的决策逻辑。

---

## 语法元素

### Context定义

Context定义是可选的，用于指定AI运行的上下文类型。

```ai_script
// 使用完整的类型名
context: Namespace.MyContext
```

### Conditions定义

当环境条件变得复杂需要条件组合时，可以定义复杂的Conditions（可选）。

```ai_script
conditions:
    // 列表中的表达式返回值应该为bool类型
    // Any表示满足列表中任意条件
    cond_01: Any [
        fatigue < 5.0
        happy >= 10.0
    ]
    
    // All表示需要满足列表中所有条件
    cond_02: All [
        fatigue == 1.0
        happy != 10.0
    ]
```

### Reasoner定义

Reasoner定义是必须的，内置了三种Reasoner类型：

- `+` : 对应 `HighestScoreReasoner`
- `-` : 对应 `LowestScoreReasoner`
- `>` : 对应 `FirstScoreReasoner`

您也可以继承Reasoner类实现自定义Reasoner。更多详情可参见[Nez.AI文档](https://github.com/prime31/Nez/blob/master/FAQs/AI.md)。

### Consideration定义

Consideration的定义格式为：
```
= [Consideration Name] : [Consideration Type]
```

内置的Consideration类型有：
- AllOrNothing
- AllOrNothingFixed
- MaxScoreOfChildren
- MinScoreOfChildren
- FixedScore
- SumOfChildren
- SumOfChildrenWithThreshold
- TakeAvg
- Threshold

更多细节请参见`UtilityAICompiler::RegisterDefaultFactories`方法。

---

## 使用示例

### 脚本示例

```ai_script
// 声明下面定义的Reasoner使用Idle作为默认的Consideration
default: Idle

conditions:
    comp_cond_01: All [
        fatigue < 5.0
        happy >= 10.0
    ]

// HighestScoreReasoner
// 如果使用自定义Reasoner, 则需要写完整类型名
+
    // 定义名为Patrol的Consideration, 类型为AllOrNothingConsideration
    // 也可以继承BaseConsideration实现自定义类型
    = Patrol : AllOrNothing
        // 打分器表达式, 表达式应该返回float类型
        !can_see_enemy ? 1 : 0

        // Action定义
        ->
            do_patrol
            
    // 定义名为Idle的Consideration, 类型为FixedScoreConsideration
    // 并传递参数score给构造方法, 参数名与实际构造方法参数名应该一致
    = Idle : FixedScore( score: 0.5 ) // 构造表达式可选
        ->
            do_idle
    
    // 自定义Consideration类型
    = MyConsider : NameSpace.MyCustomConsideration( balabala: MyEnumValue )
        // 使用定义的复合条件
        comp_cond_01 ? 1 : 0
        
        -> do_balabala
```

### 运行时使用

参考TestUtilityAICompiler.cs实现：

```csharp
// 创建编译器
var compiler = new UtilityAICompiler<AIContext>();

// 注册条件解析器
compiler.RegisterCondition("can_see_enemy", ctx => ctx.CanSeeEnemy);
compiler.RegisterCondition("is_hungry", ctx => ctx.IsHungry);

// 注册属性访问器
compiler.RegisterProperty("fatigue", ctx => ctx.Fatigue);
compiler.RegisterProperty("distance", ctx => ctx.Distance);

// 注册动作
compiler.RegisterAction("do_patrol", ctx => { ctx.LastAction = "Patrol"; });
compiler.RegisterAction("do_idle", ctx => { ctx.LastAction = "Idle"; });

// 加载AI脚本
var script = ""; // ai_script内容
var reasoner = compiler.CompileAndBuild(script);

// 创建AI上下文
var context = new AIContext { CanSeeEnemy = false, Fatigue = 3.0f, Distance = 8.0f };

// 执行推理
var action = reasoner.Select(context);
action?.Execute(context);
```

---

## 调试技巧

Reasoner类带有方法`DebugDump`可以获取当前Reasoner树的详细分数值，这对于调试AI行为非常有用：

```
HighestScore
  default:
    Idle = 0.50 (FixedScore)

  Chase = 1.50 (AllOrNothing)
   Threshold: 0.1
    see_player ? 1 : 0 -> 1.00 ok
    !out_of_range ? 0.2 : 0 -> 0.20 ok
    (fatigue < 5) ? 0.3 : 0 -> 0.30 ok

  Patrol = 0.00 (AllOrNothing)
   Threshold: 0
    !see_player ? 1 : 0 -> 0.00 ok

  TriggerBattle = 0.00 (AllOrNothing)
   Threshold: 0.1
    see_player ? 1 : 0 -> 1.00 ok
    !out_of_range ? 0.2 : 0 -> 0.20 ok
    in_trigger_range ? 99 : 0 -> 0.00 x
```

---

## 未实现功能

目前有些语法特性尚未实现，计划在后续版本中添加：

```ai_script
+
    = Chase : SumOfChildren
        // TODO: 自定义打分器构造实现
        out_of_range : SumOfChildrenWithPreAppraisals { 
            threshold: 1
            preCheckMode: AllRequired
            appraisals: [
                !can_see_enemy ? 0 : 1.0 // 内嵌打分器表达式
            ]
        }
        
        // TODO: 
        random_value : RandomScoreInRange {
            min : -1.0
            max : 1.0
        }
        
        ->
            do_chasing // TODO: 是否应该支持多个action(并行、顺序)?
        
    = Idle : SumOfChildren
        fatigue > 5.0 ? 99 : default_idle_score // TODO: (未测试)取context中变量比对作为score表达式

        ->
            do_idle
```
