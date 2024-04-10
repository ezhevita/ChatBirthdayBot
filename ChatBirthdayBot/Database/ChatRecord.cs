using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace ChatBirthdayBot.Database;

public class ChatRecord
{
	public long Id { get; set; }

	[MaxLength(255)]
	public string Name { get; set; }

	[MaxLength(10)]
	public string Locale { get; set; }

	public sbyte TimeZoneHourOffset { get; set; }
	public byte CustomOffsetInHours { get; set; }
	public bool ShouldPinNotify { get; set; }

	public virtual ICollection<UserRecord> Users { get; set; }
	public virtual ICollection<UserChat> UserChats { get; set; }
	public virtual ICollection<SentMessage> SentMessages { get; set; }
}
