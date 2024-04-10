using System.Collections.Generic;

#nullable disable

namespace ChatBirthdayBot.Database;

public class ChatRecord
{
	public long Id { get; set; }
	public string Name { get; set; }
	public string Locale { get; set; }

	public sbyte TimeZoneHourOffset { get; set; }
	public byte CustomOffsetInHours { get; set; }
	public bool ShouldPinNotify { get; set; }

	public virtual ICollection<UserRecord> Users { get; set; }
	public virtual ICollection<UserChat> UserChats { get; set; }
	public virtual ICollection<SentMessage> SentMessages { get; set; }
}
