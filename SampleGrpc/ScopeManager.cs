namespace SampleGrpc;

public class ScopeManager
{
    public int Id { get; set; } 

    public ScopeManager()
    {
        Id = new Random().Next(1, 1000);
    }
}