using SampleRack.Application.Interfaces;

namespace SampleRack.Infrastructure.Services;

public sealed class ClockService : IClockService
{
    public DateTime UtcNow => DateTime.UtcNow;
}
