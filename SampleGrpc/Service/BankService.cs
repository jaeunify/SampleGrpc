using Grpc.Core;
using GrpcDemo;
using Orleans;
using GrpcDemo.Services;

namespace GrpcDemo.Services
{
    public class BankService : Bank.BankBase
    {
        private readonly IClusterClient _orleansClient;
        private readonly ITransactionClient _transactionClient;

        public BankService(IClusterClient orleansClient, ITransactionClient transactionClient)
        {
            _orleansClient = orleansClient;
            _transactionClient = transactionClient;
        }
        
        public override async Task<Account> Deposit(DepositRequest request, ServerCallContext context)
        {
            var userId = GetUserId(context);
            var grain = _orleansClient.GetGrain<IAccountGrain>(userId);

            await _transactionClient.RunTransaction(TransactionOption.Create, async () =>
            {
                await grain.Deposit(request.Amount);
            });
            
            var balance = await grain.GetBalance();
            
            return new Account { Amount = balance };
        }

        private string GetUserId(ServerCallContext context)
        {
            return "2"; // temp
        }
    }
}