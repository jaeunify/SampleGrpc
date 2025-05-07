using Grpc.Core;
using GrpcDemo;
using System.Threading.Channels;

namespace GrpcDemo.Services;

public class MessengerService : Messenger.MessengerBase
{
    private readonly StreamManager _streamManager;

    public MessengerService(StreamManager streamManager)
    {
        _streamManager = streamManager;
    }

    public override async Task Chat(
        IAsyncStreamReader<RequestMessage> requestStream,
        IServerStreamWriter<ResponseMessage> responseStream,
        ServerCallContext context)
    {
        // 클라이언트가 연결되면 채널을 생성
        var channel = Channel.CreateUnbounded<ResponseMessage>();
        
        // 채널을 StreamManager에 등록
        var clientId = _streamManager.Register(channel);
        
        // 받은 메세지를 클라이언트에게 전송
        var sendTask = Task.Run(async () =>
        {
            await foreach (var msg in channel.Reader.ReadAllAsync(context.CancellationToken))
            {
                await responseStream.WriteAsync(msg);
            }
        });
        
        // 입장 찍어주기
        var enterMessage = new ResponseMessage
        {
            Receive = new ResponseMessage.Types.ReceiveMessage
            {
                Message = $"{clientId} 입장"
            }
        };
        await _streamManager.BroadcastAsync(enterMessage);

        // 앞으로는 여기서 loop
        try
        {
            // 내가 메세지를 보내면
            await foreach (var request in requestStream.ReadAllAsync(context.CancellationToken))
            {
                if (request.MsgCase == RequestMessage.MsgOneofCase.Send)
                {
                    var response = new ResponseMessage
                    {
                        Receive = new ResponseMessage.Types.ReceiveMessage
                        {
                            Message = $"{clientId}: {request.Send.Message}"
                        }
                    };

                    // 모든 클라이언트들에게 브로드캐스트
                    await _streamManager.BroadcastAsync(response);
                }
            }
        }
        finally
        {
            // 연결이 끊어지면, 채널을 해제
            _streamManager.Unregister(clientId);
            
            // 퇴장 찍어준다.
            var exitMessage = new ResponseMessage
            {
                Receive = new ResponseMessage.Types.ReceiveMessage
                {
                    Message = $"{clientId} 퇴장"
                }
            };
            await _streamManager.BroadcastAsync(exitMessage);
        }

        await sendTask;
    }
}