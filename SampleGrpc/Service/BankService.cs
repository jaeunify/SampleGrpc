using Grpc.Core;
using GrpcDemo;
using System.Threading.Tasks;

namespace GrpcDemo.Services
{
    public class BankService : Bank.BankBase
    {
        private int _balance = 1000; // 초기 잔고

        public override Task<Account> Deposit(DepositRequest request, ServerCallContext context)
        {
            _balance += request.Amount;
            return Task.FromResult(new Account { Amount = _balance });
        }

        public override Task<Account> Withdraw(WithdrawRequest request, ServerCallContext context)
        {
            if (_balance >= request.Amount)
            {
                _balance -= request.Amount;
            }
            // 잔액 부족이어도 잔고 그대로 반환
            return Task.FromResult(new Account { Amount = _balance });
        }

        public override Task<Account> Transfer(TransferRequest request, ServerCallContext context)
        {
            // 실제 계좌 이체 로직은 생략하고 단순히 잔고 감소만 처리
            if (_balance >= request.Amount)
            {
                _balance -= request.Amount;
            }

            return Task.FromResult(new Account { Amount = _balance });
        }
    }
}