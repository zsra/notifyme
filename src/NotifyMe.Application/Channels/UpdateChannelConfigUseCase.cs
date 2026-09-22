using FluentValidation;
using NotifyMe.Application.Abstractions.Repositories;
using NotifyMe.Application.Common.Exceptions;

namespace NotifyMe.Application.Channels;

public sealed class UpdateChannelConfigUseCase
{
    private readonly IChannelConfigRepository _channelConfigRepository;
    private readonly IValidator<UpdateChannelConfigRequest> _validator;

    public UpdateChannelConfigUseCase(
        IChannelConfigRepository channelConfigRepository, IValidator<UpdateChannelConfigRequest> validator)
    {
        _channelConfigRepository = channelConfigRepository;
        _validator = validator;
    }

    public async Task<ChannelConfigDto> ExecuteAsync(
        UpdateChannelConfigRequest request, CancellationToken cancellationToken, Guid? ownerUserId = null)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var channelConfig = await _channelConfigRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Channel '{request.Id}' was not found.");

        if (ownerUserId is not null && channelConfig.OwnerUserId != ownerUserId)
        {
            throw new NotFoundException($"Channel '{request.Id}' was not found.");
        }

        channelConfig.UpdateDetails(request.ChannelType, request.Target);
        await _channelConfigRepository.UpdateAsync(channelConfig, cancellationToken);

        return ChannelConfigDto.FromEntity(channelConfig);
    }
}
