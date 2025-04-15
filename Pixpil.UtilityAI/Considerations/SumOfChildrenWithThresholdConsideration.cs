using System.Collections.Immutable;


namespace Pixpil.AI.UtilityAI;

/// <summary>
/// Enumeration for comparisons
/// </summary>
public enum CompareMethod : byte {
	EqualTo,
	GreaterThan,
	LessThan,
	GreaterOrEqualTo,
	LessOrEqualTo,
	NotEqualTo
}

/// <summary>
/// 按顺序遍历Appraisals, 返回总和. 仅IAppraisal返回值比对Threshold成功后才计入总和
/// ( if IAppraisal (CompareMethod) Threshold, then sum += IAppraisal. return sum. ).
/// </summary>
public class SumOfChildrenWithThresholdConsideration< T > : BaseConsideration< T > {

	public readonly CompareMethod CompareMethod;
	public readonly float Threshold;

	private ImmutableArray< IAppraisal< T > > _appraisals = ImmutableArray< IAppraisal< T > >.Empty;
	
	public SumOfChildrenWithThresholdConsideration( CompareMethod compareMethod, float threshold ) {
		CompareMethod = compareMethod;
		Threshold = threshold;
	}

	public SumOfChildrenWithThresholdConsideration( CompareMethod compareMethod, float threshold, IAppraisal< T >[] appraisals ) {
		CompareMethod = compareMethod;
		Threshold = threshold;
		_appraisals = appraisals.ToImmutableArray();
	}

	public SumOfChildrenWithThresholdConsideration< T > AddAppraisal( IAppraisal< T > appraisal ) {
		_appraisals = _appraisals.Add( appraisal );
		return this;
	}
	
	public override float GetScore( T context ) {
		var sum = 0f;
		for ( var i = 0; i < _appraisals.Length; i++ ) {
			var score = _appraisals[ i ].GetScore( context );
#if DEBUG
			CacheDebugUnitScore( _appraisals[ i ], score );
#endif
			if ( NumericCompareHelper.Compare( score, CompareMethod, Threshold ) ) {
				sum += score;
			}
		}

#if DEBUG
		CacheResultScore( sum );
#endif
		return sum;
	}
	
#if DEBUG
	public override void DebugDump( System.Text.StringBuilder stringBuilder, Action indent, ref int level ) {
		level++;
		indent?.Invoke(); stringBuilder.AppendLine( $"{Name} = {TryGetCachedResultScore():0.00} ({DebugTypeName()})" );
		indent?.Invoke(); stringBuilder.AppendLine( $" CompareMethod: {CompareMethod}" );
		indent?.Invoke(); stringBuilder.AppendLine( $" Threshold: {Threshold:0.00}" );
		level++;
		foreach ( var appraisal in _appraisals ) {
			var score = TryGetDebugUnitScore( appraisal );
			var preUnary = score > 0 ? "+" : string.Empty;
			var okMark = score != null && NumericCompareHelper.Compare( score.Value, CompareMethod, Threshold ) ? DebugMarkOk : DebugMarkFail;
			if ( appraisal is BaseAppraisal< T > baseAppraisal ) {
				indent?.Invoke(); stringBuilder.AppendLine( $"{baseAppraisal.Notes} -> {preUnary}{score:0.00} {okMark}" );
			}
			else {
				indent?.Invoke(); stringBuilder.AppendLine( $"{appraisal.GetType().Name} -> {preUnary}{score:0.00} {okMark}" );
			}
		}
		level--;
		level--;
	}
#endif
	
}


internal static class NumericCompareHelper {
		
	public static bool Compare( float value1, CompareMethod method, float value2 ) {
		switch ( method ) {
			case CompareMethod.EqualTo:          return value1 == value2;
			case CompareMethod.GreaterThan:      return value1 > value2;
			case CompareMethod.LessThan:         return value1 < value2;
			case CompareMethod.GreaterOrEqualTo: return value1 >= value2;
			case CompareMethod.LessOrEqualTo:    return value1 <= value2;
			case CompareMethod.NotEqualTo:       return value1 != value2;
		}

		throw new ArgumentException( nameof( method ) );
	}


	public static bool Compare( int value1, CompareMethod method, int value2 ) {
		switch ( method ) {
			case CompareMethod.EqualTo:          return value1 == value2;
			case CompareMethod.GreaterThan:      return value1 > value2;
			case CompareMethod.LessThan:         return value1 < value2;
			case CompareMethod.GreaterOrEqualTo: return value1 >= value2;
			case CompareMethod.LessOrEqualTo:    return value1 <= value2;
			case CompareMethod.NotEqualTo:       return value1 != value2;
		}

		throw new ArgumentException( nameof( method ) );
	}

}