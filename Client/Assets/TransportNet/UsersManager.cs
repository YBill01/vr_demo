

namespace Transport.Universal {

	using System.Collections.Generic;

	public delegate void OnUserChanged(User user);

	public class UsersManager {

		public UsersManager(ConnectionsManager connectionsManager) {
			connectionsManager.OnConnectionCreated += OnConnectionCreated;
			connectionsManager.OnConnectionDestroyed += OnConnectionDestroyed;
		}

		public OnUserChanged OnUserCreated;
		public OnUserChanged OnUserDestroyed;
		public OnUserChanged OnUserChanged;

		private Dictionary<uint, User> _users = new Dictionary<uint, User>();

		public bool GetUser(uint networkId, out User user) {
			return _users.TryGetValue(networkId, out user);
		}

		public void SetUserInfo(uint networkId, string userId, string username) {
			if (_users.TryGetValue(networkId, out var user)) {
				user.UserId = userId;
				user.Username = username;
				OnUserChanged?.Invoke(user);
			}
		}

		private void OnConnectionCreated(Connection conn) {
			if (_users.TryAdd(conn.NetworkId, new User(conn))) {
				OnUserCreated?.Invoke(_users[conn.NetworkId]);
			}
		}

		private void OnConnectionDestroyed(Connection conn) {
			if (_users.Remove(conn.NetworkId, out var user)) {
				OnUserDestroyed?.Invoke(user);
			}
		}

	}

	public class User {

		public string UserId;
		public string Username;
		public readonly Connection Connection;

		public User(Connection connection) {
			Connection = connection;
		}

	}

}