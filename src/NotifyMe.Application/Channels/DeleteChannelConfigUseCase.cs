using NotifyMe.Application.Abstractions.Repositories;
using NotifyMe.Application.Common.Exceptions;

namespace NotifyMe.Application.Channels;

public sealed class DeleteChannelConfigUseCase
{
    private readonly IChannelConfigRepository _channelConfigRepository;

    public DeleteChannelConfigUseCase(IChannelConfigRepository channelConfigRepository)
    {
        _channelConfigRepository = channelConfigRepository;
    }

    public async Task ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var channelConfig = await _channelConfigRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Channel '{id}' was not found.");

        await _channelConfigRepository.DeleteAsync(channelConfig, cancellationToken);
    }
}
