public interface IAccountGrain : IGrainWithStringKey

{
    [Transaction(TransactionOption.CreateOrJoin)]
    Task<int> GetBalance();
    
    [Transaction(TransactionOption.Join)]
    Task Deposit(int amount);
    
    // [Transaction(TransactionOption.CreateOrJoin)]
    // Task<bool> Withdraw(int amount);
}