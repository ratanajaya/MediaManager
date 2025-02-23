using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace CoreAPI.AL.Hubs;

[AllowAnonymous]
public class ProgressHub : Hub
{
    /// <summary>
    /// Allows a client to join a group identified by the operationId.
    /// </summary>
    /// <param name="operationId">The unique identifier for the operation.</param>
    public async Task JoinOperationGroup(string operationId) {
        Console.WriteLine($"JoinOperationGroup called with operationId: {operationId}");
        await Groups.AddToGroupAsync(Context.ConnectionId, operationId);
    }

    /// <summary>
    /// Allows a client to leave a group identified by the operationId.
    /// </summary>
    /// <param name="operationId">The unique identifier for the operation.</param>
    public async Task LeaveOperationGroup(string operationId) {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, operationId);
    }
}