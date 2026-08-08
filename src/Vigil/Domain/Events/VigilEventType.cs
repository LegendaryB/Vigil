namespace Vigil.Domain.Events;

public enum VigilEventType
{
    ClientCheckedIn,
    ClientCheckedOut,
    AllClientsCheckedOut,
    ClientOverdue,
    ClientForceCheckedOut
}
