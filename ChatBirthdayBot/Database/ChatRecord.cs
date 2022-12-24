using System.Collections.Generic;

#nullable disable

namespace ChatBirthdayBot.Database;

public class ChatRecord {
	public long Id { get; set; }
	public string Name { get; set; }
	public string Locale { get; set; }

	public virtual ICollection<UserRecord> Users { get; set; }
	public virtual List<UserChat> UserChats { get; set; }
}
