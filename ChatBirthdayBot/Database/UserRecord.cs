using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace ChatBirthdayBot.Database;

public class UserRecord
{
	public long Id { get; init; }
	public byte? BirthdayDay { get; set; }
	public byte? BirthdayMonth { get; set; }
	public ushort? BirthdayYear { get; set; }

	[MaxLength(64)]
	public string FirstName { get; set; }

	[MaxLength(64)]
	public string LastName { get; set; }

	[MaxLength(32)]
	public string Username { get; set; }

	public ICollection<ChatRecord> Chats { get; set; }
	public List<UserChat> UserChats { get; set; }
}
