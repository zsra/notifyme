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

    public async Task ExecuteAsync(Guid id, CancellationToken cancellationToken, Guid? ownerUserId = null)
    {
        var channelConfig = await _channelConfigRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Channel '{id}' was not found.");

        if (ownerUserId is not null && channelConfig.OwnerUserId != ownerUserId)
        {
            throw new NotFoundException($"Channel '{id}' was not found.");
        }

        await _channelConfigRepository.DeleteAsync(channelConfig, cancellationToken);
    }
}
