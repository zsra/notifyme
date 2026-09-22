using FluentValidation;

namespace NotifyMe.Application.Channels;

public sealed class CreateChannelConfigRequestValidator : AbstractValidator<CreateChannelConfigRequest>
{
    public CreateChannelConfigRequestValidator()
    {
        RuleFor(request => request.ChannelType).NotEmpty();
        RuleFor(request => request.Target).NotEmpty();
    }
}
