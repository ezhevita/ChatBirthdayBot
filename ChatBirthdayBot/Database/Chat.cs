using System.Collections.Generic;

#nullable disable

namespace ChatBirthdayBot.Database {
	public class Chat {
		public long Id { get; set; }
		public string Name { get; set; }

		public ICollection<User> Users { get; set; }
		public List<UserChat> UserChats { get; set; }
	}
}
