namespace Pixpil.AI.UtilityAI
{
	public interface IAction<T>
	{
		void Execute(T context);
	}
}