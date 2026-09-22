using NotifyMe.Application.Abstractions.Repositories;
using NotifyMe.Application.Common.Exceptions;

namespace NotifyMe.Application.Channels;

public sealed class GetChannelConfigUseCase
{
    private readonly IChannelConfigRepository _channelConfigRepository;

    public GetChannelConfigUseCase(IChannelConfigRepository channelConfigRepository)
    {
        _channelConfigRepository = channelConfigRepository;
    }

    public async Task<ChannelConfigDto> ExecuteAsync(Guid id, CancellationToken cancellationToken, Guid? ownerUserId = null)
    {
        var channelConfig = await _channelConfigRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Channel '{id}' was not found.");

        if (ownerUserId is not null && channelConfig.OwnerUserId != ownerUserId)
        {
            throw new NotFoundException($"Channel '{id}' was not found.");
        }

        return ChannelConfigDto.FromEntity(channelConfig);
    }
}
