using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcDemo;
using static GrpcDemo.Messenger;

class Program
{
    static async Task Main(string[] args)
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
}