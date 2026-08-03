namespace KidsPocket.Application.Features.Reward.CreateReward;

// תגמול ידני חד פעמי ממבוגר - עובר דרך אותו Decision Flow כמו כל כסף אחר
public record CreateRewardCommand(Guid ChildId, Guid CreatedByAdultId, decimal Amount, string? Description);

public record CreateRewardResult(Guid RewardId, Guid MoneyEventId, Guid DecisionId);
