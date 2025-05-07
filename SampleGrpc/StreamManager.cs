using System.Collections.Concurrent;
using System.Threading.Channels;
using GrpcDemo;

public class StreamManager
{
    private readonly ConcurrentDictionary<Guid, Channel<ResponseMessage>> _clients = new();
    
    public Guid Register(Channel<ResponseMessage> channel)
    {
        var id = Guid.NewGuid();
        _clients.TryAdd(id, channel);
        return id;
    }

    public async Task BroadcastAsync(ResponseMessage message)
    {
        foreach (var kvp in _clients)
        {
            await kvp.Value.Writer.WriteAsync(message);
        }
    }

    public void Unregister(Guid clientId)
    {
        if (_clients.TryRemove(clientId, out var channel))
        {
            channel.Writer.Complete();
        }
    }
}