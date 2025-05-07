public interface IAccountGrain : IGrainWithStringKey
{
    Task<decimal> GetBalance();
    Task Deposit(decimal amount);
    Task<bool> Withdraw(decimal amount);
}