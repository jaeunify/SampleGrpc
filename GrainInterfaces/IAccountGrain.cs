public interface IAccountGrain : IGrainWithStringKey

{
    [Transaction(TransactionOption.Create)]
    Task<int> GetBalance();
    
    [Transaction(TransactionOption.Create)]
    Task Deposit(int amount);
    
    [Transaction(TransactionOption.Create)]
    Task Withdraw(int amount);
    
    [Transaction(TransactionOption.Create)]
    Task Transfer(int amount, string targetId);
}