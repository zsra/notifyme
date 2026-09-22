using FluentValidation;
using NotifyMe.Application.AlertRules;
using NotifyMe.Application.Tests.TestDoubles;
using NotifyMe.Domain.Events;
using Xunit;
using Severity = NotifyMe.Domain.Common.Severity;

namespace NotifyMe.Application.Tests.AlertRules;

public class CreateAlertRuleUseCaseTests
{
    private static CreateAlertRuleUseCase CreateUseCase(out InMemoryAlertRuleRepository repository)
    {
        repository = new InMemoryAlertRuleRepository();
        return new CreateAlertRuleUseCase(repository, new CreateAlertRuleRequestValidator());
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_PersistsAndReturnsDto()
    {
        var useCase = CreateUseCase(out var repository);
        var request = new CreateAlertRuleRequest(
            "Earthquake watch", EventCategory.NaturalDisaster, new[] { "earthquake" }, Severity.Low);

        var dto = await useCase.ExecuteAsync(request, CancellationToken.None);

        Assert.Equal("Earthquake watch", dto.Name);
        Assert.Single(repository.Seeded);
    }

    [Fact]
    public async Task ExecuteAsync_WithBlankName_ThrowsValidationException()
    {
        var useCase = CreateUseCase(out _);
        var request = new CreateAlertRuleRequest(
            "", EventCategory.NaturalDisaster, new[] { "earthquake" }, Severity.Low);

        await Assert.ThrowsAsync<ValidationException>(() => useCase.ExecuteAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_WithDegenerateCriteria_ThrowsArgumentException()
    {
        var useCase = CreateUseCase(out _);
        var request = new CreateAlertRuleRequest(
            "Everything watch", EventCategory.NaturalDisaster, Keywords: null, Severity.Low);

        await Assert.ThrowsAsync<ArgumentException>(() => useCase.ExecuteAsync(request, CancellationToken.None));
    }
}
