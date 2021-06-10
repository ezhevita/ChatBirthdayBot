#nullable disable

namespace ChatBirthdayBot.Database {
	public class UserChat {
		public long UserId { get; set; }
		public User User { get; set; }
		public long ChatId { get; set; }
		public Chat Chat { get; set; }
		public bool IsPublic { get; set; }
	}
}
