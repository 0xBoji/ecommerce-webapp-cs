using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ecommerce_webapp_cs.Hubs;

[Authorize] 
public class ChatHub : Hub
{
	public async Task SendMessageToGroup(string groupName, string message)
	{
		var userName = Context.User.Identity.Name; // get the user name from the context
		await Clients.Group(groupName).SendAsync("ReceiveMessage", userName, message);
	}
	//role "customer", "employee"
	public async Task JoinRoleGroup()
	{
		var userRole = Context.User.FindFirst(ClaimTypes.Role)?.Value;

		if (userRole == "Customer")
		{
			await Groups.AddToGroupAsync(Context.ConnectionId, "Customers");
		}
		else if (userRole == "Employee")
		{
			await Groups.AddToGroupAsync(Context.ConnectionId, "Employees");
		}
	}
	public async Task LeaveRoleGroup()
	{
		var userRole = Context.User.FindFirst(ClaimTypes.Role)?.Value;

		if (userRole == "Customer")
		{
			await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Customers");
		}
		else if (userRole == "Employee")
		{
			await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Employees");
		}
	}

	public override async Task OnConnectedAsync()
	{
		await JoinRoleGroup();
		await base.OnConnectedAsync();
	}
		//leave
	public override async Task OnDisconnectedAsync(Exception? exception)
	{
		await LeaveRoleGroup();
		await base.OnDisconnectedAsync(exception);
	}
}
