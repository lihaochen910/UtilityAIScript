namespace Pixpil.AI.UtilityAI;

public abstract class BaseAppraisal< T > : IAppraisal< T > {
	
	public string Notes { get; set; }
	public string NotesLong { get; set; }

	public abstract float GetScore( T context );

	public override string ToString() {
		return Notes != null ? Notes : base.ToString();
	}
	
}
