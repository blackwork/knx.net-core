
namespace KnxCli
{

    public enum ExitCode : int
    {
        OK = 0,
        Error = 1,
        NoActorSpecified = 2,
        NoActionSpecified = 3,
        NoSettingsAvailable = 4,
        DifferentActorsActionConfiguration = 5,
        ActionOrGroupNotFound = 6,
        CannotParseAction = 7,
        ActionExecutionFailed = 8
    }
}