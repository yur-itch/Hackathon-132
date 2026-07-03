namespace PlantCare.Api.Dtos;

public sealed record PushSubscriptionKeysDto(string P256dh, string Auth);

public sealed record PushSubscribeDto(string Endpoint, PushSubscriptionKeysDto Keys);

public sealed record PushUnsubscribeDto(string Endpoint);

public sealed record VapidPublicKeyDto(string PublicKey);
