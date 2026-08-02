namespace Vigil.Domain.Events;

internal enum VigilEventType
{
    ClientCheckedIn,
    ClientCheckedOut,
    AllClientsCheckedOut,
    ClientOverdue,
    ClientForceCheckedOut
}
