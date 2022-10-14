namespace Test;

public class Program
{
	public static void Main(string[] args)
	{
		MyBusinessLogic m = new MyBusinessLogicImplemented();
		m.Run();
	}
}

public class MyBusinessLogic
{
	public virtual void Run()
	{
		Thread.Sleep(100);
	}
}