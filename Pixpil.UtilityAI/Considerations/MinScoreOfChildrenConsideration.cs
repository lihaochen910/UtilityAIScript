using System.Collections.Immutable;


namespace Pixpil.AI.UtilityAI;

/// <summary>
/// 返回Appraisals中最小的值
/// </summary>
public class MinScoreOfChildrenConsideration< T > : BaseConsideration< T > {
	
	private ImmutableArray< IAppraisal< T > > _appraisals = ImmutableArray< IAppraisal< T > >.Empty;
	
	public MinScoreOfChildrenConsideration< T > AddAppraisal( IAppraisal< T > appraisal ) {
		_appraisals = _appraisals.Add( appraisal );
		return this;
	}
	
	public override float GetScore( T context ) {
		var min = 0f;
		for ( var i = 0; i < _appraisals.Length; i++ ) {
			var score = _appraisals[ i ].GetScore( context );
#if DEBUG
			CacheDebugUnitScore( _appraisals[ i ], score );
#endif
			if ( score < min ) {
				min = score;
			}
		}

#if DEBUG
		CacheResultScore( min );
#endif
		return min;
	}
	
#if DEBUG
	public override void DebugDump( System.Text.StringBuilder stringBuilder, Action indent, ref int level ) {
		level++;
		indent?.Invoke(); stringBuilder.AppendLine( $"{Name} = {TryGetCachedResultScore():0.00} ({DebugTypeName()})" );
		level++;
		foreach ( var appraisal in _appraisals ) {
			var score = TryGetDebugUnitScore( appraisal );
			if ( appraisal is BaseAppraisal< T > baseAppraisal ) {
				indent?.Invoke(); stringBuilder.AppendLine( $"{baseAppraisal.Notes} -> {score:0.00}" );
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
