using System.Text.Json.Serialization;

namespace xyz.yewnyx.SubLink.Joystick.Client.Data.Response;

[JsonPolymorphic(
    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor,
    TypeDiscriminatorPropertyName = "type"
)]
[JsonDerivedType(typeof(Welcome), "welcome")]
[JsonDerivedType(typeof(Ping), "ping")]
[JsonDerivedType(typeof(ConfirmSubscription), "confirm_subscription")]
[JsonDerivedType(typeof(RejectSubscription), "reject_subscription")]
[JsonDerivedType(typeof(ConfirmUnsubscription), "confirm_unsubscription")]
[JsonDerivedType(typeof(RejectUnsubscription), "reject_unsubscription")]
public interface IBaseResponse { }
