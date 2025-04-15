using System.Collections.Immutable;


namespace Pixpil.AI.UtilityAI;

/// <summary>
/// 按顺序遍历Appraisals, 返回固定分数. 仅当所有IAppraisal返回值都大于Threshold才计算, 否则返回0
/// </summary>
public class AllOrNothingReturnFixedScoreConsideration< T > : BaseConsideration< T > {
	
	public readonly float Threshold;
	public readonly float ReturnScore;

	private ImmutableArray< IAppraisal< T > > _appraisals = ImmutableArray< IAppraisal< T > >.Empty;


	public AllOrNothingReturnFixedScoreConsideration() {
		Threshold = 1f;
		ReturnScore = 1f;
	}
			
	public AllOrNothingReturnFixedScoreConsideration( float threshold = 1f ) => Threshold = threshold;

	public AllOrNothingReturnFixedScoreConsideration( float threshold, float score ) {
		Threshold = threshold;
		ReturnScore = score;
	}
		
	public AllOrNothingReturnFixedScoreConsideration( float threshold, float score, IAppraisal< T >[] appraisals ) {
		Threshold = threshold;
		ReturnScore = score;
		_appraisals = appraisals.ToImmutableArray();
	}
	
	public AllOrNothingReturnFixedScoreConsideration< T > AddAppraisal( IAppraisal< T > appraisal ) {
		_appraisals = _appraisals.Add( appraisal );
		return this;
	}
	
	public override float GetScore( T context ) {
#if DEBUG
		var returned = false;
		for ( var i = 0; i < _appraisals.Length; i++ ) {
			var score = _appraisals[ i ].GetScore( context );
			CacheDebugUnitScore( _appraisals[ i ], score );
			if ( score < Threshold ) {
				returned = true;
			}
			else {
				// do nothing.
			}
		}

		if ( returned ) {
#if DEBUG
			CacheResultScore( 0 );
#endif
			return 0f;
		}

#if DEBUG
		CacheResultScore( ReturnScore );
#endif
		return ReturnScore;
#else
		for ( var i = 0; i < _appraisals.Length; i++ ) {
			var score = _appraisals[ i ].GetScore( context );
			if ( score < Threshold ) {
				return 0;
			}
		}

		return ReturnScore;
#endif
	}
	
#if DEBUG
	public override void DebugDump( System.Text.StringBuilder stringBuilder, Action indent, ref int level ) {
		level++;
		indent?.Invoke(); stringBuilder.AppendLine( $"{Name} = {TryGetCachedResultScore():0.00} ({DebugTypeName()})" );
		indent?.Invoke(); stringBuilder.AppendLine( $" Threshold: {Threshold}" );
		indent?.Invoke(); stringBuilder.AppendLine( $" Fixed: {ReturnScore}" );
		level++;
		foreach ( var appraisal in _appraisals ) {
			var score = TryGetDebugUnitScore( appraisal );
			var ok = score >= Threshold;
			var okMark = ok ? DebugMarkOk : DebugMarkFail;
			if ( appraisal is BaseAppraisal< T > baseAppraisal ) {
				indent?.Invoke(); stringBuilder.AppendLine( $"{baseAppraisal.Notes} -> {score:0.00} {okMark}" );
			}
			else {
				indent?.Invoke(); stringBuilder.AppendLine( $"{appraisal.GetType().Name} -> {score:0.00}" );
			}
		}
		level--;
		level--;
	}
#endif
}
