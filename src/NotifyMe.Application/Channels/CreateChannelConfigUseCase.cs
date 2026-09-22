using FluentValidation;
using NotifyMe.Application.Abstractions.Repositories;
using NotifyMe.Domain.Channels;

namespace NotifyMe.Application.Channels;

public sealed class CreateChannelConfigUseCase
{
    private readonly IChannelConfigRepository _channelConfigRepository;
    private readonly IValidator<CreateChannelConfigRequest> _validator;

    public CreateChannelConfigUseCase(
        IChannelConfigRepository channelConfigRepository, IValidator<CreateChannelConfigRequest> validator)
    {
        _channelConfigRepository = channelConfigRepository;
        _validator = validator;
    }

    public async Task<ChannelConfigDto> ExecuteAsync(CreateChannelConfigRequest request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var channelConfig = ChannelConfig.Create(Guid.NewGuid(), request.ChannelType, request.Target, request.IsEnabled);
        await _channelConfigRepository.AddAsync(channelConfig, cancellationToken);

        return ChannelConfigDto.FromEntity(channelConfig);
    }
}
