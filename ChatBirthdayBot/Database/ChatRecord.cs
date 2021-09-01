using System.Collections.Generic;

#nullable disable

namespace ChatBirthdayBot.Database {
	public class ChatRecord {
		public long Id { get; set; }
		public string Name { get; set; }

		public ICollection<UserRecord> Users { get; set; }
		public List<UserChat> UserChats { get; set; }
	}
}
