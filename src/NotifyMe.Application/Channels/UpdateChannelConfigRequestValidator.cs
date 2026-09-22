using FluentValidation;

namespace NotifyMe.Application.Channels;

public sealed class UpdateChannelConfigRequestValidator : AbstractValidator<UpdateChannelConfigRequest>
{
    public UpdateChannelConfigRequestValidator()
    {
        RuleFor(request => request.Id).NotEmpty();
        RuleFor(request => request.ChannelType).NotEmpty();
        RuleFor(request => request.Target).NotEmpty();
    }
}
