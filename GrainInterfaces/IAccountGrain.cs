public interface IAccountGrain : IGrainWithStringKey

{
    [Transaction(TransactionOption.Create)]
    Task<int> GetBalance();
    
    [Transaction(TransactionOption.Join)]
    Task Deposit(int amount);
    
    // [Transaction(TransactionOption.CreateOrJoin)]
    // Task<bool> Withdraw(int amount);
}