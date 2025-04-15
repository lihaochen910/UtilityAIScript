namespace Pixpil.AI.UtilityAI;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class UtilityAIActionDefAttribute : Attribute {

	public readonly string ActionName;

	public UtilityAIActionDefAttribute( string actionName ) {
		ActionName = actionName;
	}
	
}
