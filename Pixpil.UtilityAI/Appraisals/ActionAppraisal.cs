using System;


namespace Pixpil.AI.UtilityAI {

	/// <summary>
	/// wraps a Func for use as an Appraisal without having to create a subclass
	/// </summary>
	public class ActionAppraisal< T > : BaseAppraisal< T > {
		
		private readonly Func< T, float > _appraisalAction;

		public ActionAppraisal( Func< T, float > appraisalAction ) {
			_appraisalAction = appraisalAction;
		}

		public override float GetScore( T context ) {
			return _appraisalAction( context );
		}
		
	}

}