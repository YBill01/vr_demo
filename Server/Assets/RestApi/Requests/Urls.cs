
namespace Chofu.RestApi {

	public class Url {

		public class Game {
			public const int GameId = 4;
			public const string GameUID = "su-pu-v1";
			public const string Source = "https://metaverse.aestar.com.ua/api";
		}

		public class Post {

			public const string Login = "https://metaverse.aestar.com.ua/api/login";
			public const string Register = "https://metaverse.aestar.com.ua/api/register";
			public const string Logout = "https://metaverse.aestar.com.ua/api/logout";

			public const string FriendRequest_Send = "https://metaverse.aestar.com.ua/api/sendFriendRequest/{0}"; // {0} - userID
			public const string FriendRequest_Approve = "https://metaverse.aestar.com.ua/api/approveRequest/{0}"; // {0} - userID
			public const string FriendRequest_Reject = "https://metaverse.aestar.com.ua/api/rejectRequest/{0}";   // {0} - userID

		}
		public class Get {

			public const string UserInfo = "https://metaverse.aestar.com.ua/api/me";
			public const string TokenRefresh = "https://metaverse.aestar.com.ua/api/refresh";

			public const string FriendsCurrent = "https://metaverse.aestar.com.ua/api/friends";
			public const string FriendsIncoming = "https://metaverse.aestar.com.ua/api/friend-requests-incoming";
			public const string FriendsOutgoing = "https://metaverse.aestar.com.ua/api/friend-requests-outgoing";

		}
		public class Put {

			public const string CahangePassword = "https://metaverse.aestar.com.ua/api/user/changePassword";
			public const string CahangePasswordWithKey = "https://metaverse.aestar.com.ua/api/user/changePasswordWithKey";

		}
		public class Delete {

			public const string FriendRemove = "https://metaverse.aestar.com.ua/api/removeFriend/{0}";            // {0} - userID
			public const string FriendRequest_Decline = "https://metaverse.aestar.com.ua/api/declineRequest/{0}"; // {0} - userID

		}

	}

}




