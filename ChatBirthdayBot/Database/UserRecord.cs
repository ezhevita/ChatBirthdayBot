using System.Collections.Generic;

#nullable disable

namespace ChatBirthdayBot.Database;

public class UserRecord
{
	public long Id { get; set; }
	public byte? BirthdayDay { get; set; }
	public byte? BirthdayMonth { get; set; }
	public ushort? BirthdayYear { get; set; }
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string Username { get; set; }

	public ICollection<ChatRecord> Chats { get; set; }
	public List<UserChat> UserChats { get; set; }
}
