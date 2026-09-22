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

    public async Task<ChannelConfigDto> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var channelConfig = await _channelConfigRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Channel '{id}' was not found.");

        return ChannelConfigDto.FromEntity(channelConfig);
    }
}
