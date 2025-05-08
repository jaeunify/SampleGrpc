using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcDemo;
using static GrpcDemo.Messenger;

class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("========================");
        Console.WriteLine("= Which Mode? ");
        Console.WriteLine("= 1. Chat ");
        Console.WriteLine("= 2. Bank ");
        Console.WriteLine("========================");

        int input;
        while (true)
        {
            Console.Write(">> ");
            if (int.TryParse(Console.ReadLine(), out input))
            {
                switch (input)
                {
                    case 1:
                        await RunChatMode();
                        return;
                    case 2:
                        await RunBankMode();
                        return;
                    default:
                        Console.WriteLine("wrong");
                        break;
                }
            }
        }
    }

    public static async Task RunChatMode()
    {
        // 채널 생성 (서버는 5001 포트에서 실행 중이어야 함)
        using var channel = GrpcChannel.ForAddress("https://localhost:5001");
        var client = new MessengerClient(channel);

        // Chat 스트리밍 시작
        using var call = client.Chat();

        // 수신 Task
        var readTask = Task.Run(async () =>
        {
            try
            {
                await foreach (var response in call.ResponseStream.ReadAllAsync())
                {
                    if (response.MsgCase == ResponseMessage.MsgOneofCase.Receive)
                    {
                        Console.WriteLine($"[수신] {response.Receive.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[수신 오류] {ex.Message}");
            }
        });

        // 사용자 입력 → 서버로 전송
        while (true)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) break;

            var request = new RequestMessage
            {
                Send = new RequestMessage.Types.SendMessage
                {
                    Message = line
                }
            };

            await call.RequestStream.WriteAsync(request);
        }

        // 스트림 종료
        await call.RequestStream.CompleteAsync();
        await readTask;
    }

    public static async Task RunBankMode()
    {
        using var channel = CreateInsecureChannel("https://localhost:5001");
        // using var channel = GrpcChannel.ForAddress("https://localhost:5001");
        var client = new Bank.BankClient(channel);

        while (true)
        {
            Console.WriteLine("========================");
            Console.WriteLine("= Bank:");
            Console.WriteLine("= 1. Deposit");
            Console.WriteLine("= 2. Withdraw");
            Console.WriteLine("= 3. Transfer");
            Console.WriteLine("= 0. Exit");
            Console.WriteLine("========================");

            Console.Write(">> ");
            var input = Console.ReadLine();

            if (input == "0")
            {
                Console.WriteLine("Exit..");
                break;
            }

            try
            {
                switch (input)
                {
                    case "1": // deposit
                        Console.Write("amount: ");
                        if (int.TryParse(Console.ReadLine(), out int depositAmount))
                        {
                            var result = await client.DepositAsync(new DepositRequest { Amount = depositAmount });
                            Console.WriteLine($"[OK] now: {result.Amount}");
                        }

                        break;

                    case "2":  // withdraw
                        Console.Write("amount: ");
                        if (int.TryParse(Console.ReadLine(), out int withdrawAmount))
                        {
                            var result = await client.WithdrawAsync(new WithdrawRequest { Amount = withdrawAmount });
                            Console.WriteLine($"[OK] now: {result.Amount}");
                        }

                        break;

                    case "3": // Transfer
                        Console.Write("for who? : ");
                        var to = Console.ReadLine();
                        Console.Write("amount: ");
                        if (int.TryParse(Console.ReadLine(), out int transferAmount))
                        {
                            var result = await client.TransferAsync(new TransferRequest { Amount = transferAmount, To = to });
                            Console.WriteLine($"[OK] now: {result.Amount}");
                        }

                        break;

                    default:
                        Console.WriteLine("wrong.");
                        break;
                }
            }
            catch (RpcException ex)
            {
                Console.WriteLine($"[gRPC error] {ex.Status.Detail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[exception] {ex.Message}");
            }
        }
    }
    
    private static GrpcChannel CreateInsecureChannel(string address)
    {
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        var httpClient = new HttpClient(handler);
        return GrpcChannel.ForAddress(address, new GrpcChannelOptions
        {
            HttpClient = httpClient
        });
    }
}