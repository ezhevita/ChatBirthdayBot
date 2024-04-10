using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChatBirthdayBot.Database;

public class UserRecord
{
	public long Id { get; init; }
	public byte? BirthdayDay { get; set; }
	public byte? BirthdayMonth { get; set; }
	public ushort? BirthdayYear { get; set; }

	[MaxLength(64)]
	public string FirstName { get; set; } = "";

	[MaxLength(64)]
	public string? LastName { get; set; }

	[MaxLength(32)]
	public string? Username { get; set; }

	public virtual ICollection<ChatRecord> Chats { get; set; } = null!;
}
