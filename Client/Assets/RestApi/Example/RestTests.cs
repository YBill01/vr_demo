
using System;
using UnityEngine;
using Chofu.RestApi;

public class RestTests : MonoBehaviour {

	private bool _isLoggedIn = false;
	public string Username = "banana01";
	public string Password = "1234567890";
	public string NewPassword = "0987654321";
	public string SecretKey = "Value54";
	public int TargetFriendId = 12345;

	[TextArea(2, 100)]
	public string TestString;

	private void OnGUI() {

		if (GUI.Button(new Rect(10, 10, 200, 24), "Register")) {

			new RegisterUser(Username, Password, Callback);

			void Callback(Responce<RegistrationResult> obj) {

				Debug.Log(string.Format("Registration callback. Staus: {0}", obj.status));
				Debug.Log(string.Format("Token: {0} Refresh: {1}", obj.data.accessToken, obj.data.refreshToken));
				Debug.Log(string.Format("UserId: {0} Username: {1} Secret: {2}", obj.data.user.id, obj.data.user.username, obj.data.user.secretkey));

				if (obj.status == true) {
					SecretKey = obj.data.user.secretkey;
				}

			}

		}

		if (GUI.Button(new Rect(10, 35, 200, 24), "Login")) {

			new LoginUser(Username, Password, Callback);

			void Callback(Responce<LoginResult> obj) {

				Debug.Log(string.Format("Login callback. Staus: {0}", obj.status));
				Debug.Log(string.Format("Token: {0} Refresh: {1}", obj.data.accessToken, obj.data.refreshToken));
				Debug.Log(string.Format("UserId: {0} Username: {1}", obj.data.user.id, obj.data.user.username));

				if (obj.status == true) {
					_isLoggedIn = true;
				}

			}

		}

		if (GUI.Button(new Rect(10, 60, 200, 24), "Logout")) {

			if (!_isLoggedIn) {
				Debug.Log("User must be authorized for this action");
				return;
			}

			new LogoutUser(Callcak);

			void Callcak(Responce<LogoutStatus> obj) {

				Debug.Log(string.Format("Logout callback. Staus: {0}", obj.status));
				Debug.Log(String.Format("fieldCount: {0} |affectedRows: {1} |insertId: {2} |info: {3} |serverStatus: {4} |warningStatus: {5}",
					obj.data.fieldCount, obj.data.affectedRows, obj.data.insertId, obj.data.info, obj.data.serverStatus, obj.data.warningStatus));
				_isLoggedIn = false;

			}

		}

		if (GUI.Button(new Rect(10, 85, 200, 24), "RefreshToken")) {

			if (!_isLoggedIn) {
				Debug.Log("User must be authorized for this action");
				return;
			}

			new RefreshToken(Callcak);

			void Callcak(Responce<LoginResult> obj) {

				Debug.Log(string.Format("Refresh token callback. Staus: {0}", obj.status));
				Debug.Log(string.Format("Token: {0} Refresh: {1}", obj.data.accessToken, obj.data.refreshToken));
				Debug.Log(string.Format("UserId: {0} Username: {1}", obj.data.user.id, obj.data.user.username));

			}

		}

		if (GUI.Button(new Rect(10, 110, 200, 24), "Inventory-Get")) {

			if (!_isLoggedIn) {
				Debug.Log("User must be authorized for this action");
				return;
			}

			new UserInfo(Callcak);

			void Callcak(Responce<UserInventory> obj) {

				Debug.Log(string.Format("User info callback. Staus: {0}", obj.status));
				Debug.Log(string.Format("Id: {0} |Balance: {1} |InvCount: {2}", obj.data.id, obj.data.balance, obj.data.inventory.Length));

				string invInfo = "";

				foreach (var item in obj.data.inventory) {
					invInfo += string.Format("Id: {0} |Name: {1} |Tradable: {2} |MaxStack: {3} |Count: {4} \n", item.id, item.name, item.isTradeble, item.maxStack, item.count);
				}

				Debug.Log("User inv: \n" + invInfo);

			}

		}

		if (GUI.Button(new Rect(10, 150, 200, 24), "PassChange")) {

			if (!_isLoggedIn) {
				Debug.Log("User must be authorized for this action");
				return;
			}

			new ChangePassword(Password, NewPassword, Callcak);

			void Callcak(Responce<ClearResponce> obj) {

				Debug.Log(string.Format("Password change callback callback. Staus: {0}", obj.status));
				Debug.Log(obj.data.messages);

			}

		}

		if (GUI.Button(new Rect(10, 175, 200, 24), "PassChange-KEY")) {

			new ChangePasswordWithKey(SecretKey, NewPassword, Callcak);

			void Callcak(Responce<SecretKey> obj) {

				Debug.Log(string.Format("Password change callback callback. Staus: {0}", obj.status));
				Debug.Log(string.Format("New Secret Key: {0}", obj.data.newSecretKey));

				if (obj.status == true) {
					SecretKey = obj.data.newSecretKey;
				}

			}

		}


		if (GUI.Button(new Rect(250, 10, 200, 24), "⇓ Firends ⇓")) {

			if (!_isLoggedIn) {
				Debug.Log("User must be authorized for this action");
				return;
			}

			new GetFriends(FrienStatus.Current, Callback);

			void Callback(Responce<FriendList> obj) {

				Debug.Log(string.Format("Friend request callback (CurrentFriends). Staus: {0}", obj.status));

				if (obj.data.friends != null && obj.data.friends.Length > 0) {
					Debug.Log(string.Format("Tatal friends: {0}", obj.data.friends.Length));
				} else {
					Debug.Log(obj.data.message);
				}

			}

		}

		if (GUI.Button(new Rect(250, 35, 200, 24), "⇑ Incoming ⇑")) {

			if (!_isLoggedIn) {
				Debug.Log("User must be authorized for this action");
				return;
			}

			new GetFriends(FrienStatus.Incoming, Callback);

			void Callback(Responce<FriendList> obj) {

				Debug.Log(string.Format("Friend request callback (IncomingFriends). Staus: {0}", obj.status));

				if (obj.data.friends != null && obj.data.friends.Length > 0) {
					Debug.Log(string.Format("Tatal Incoming friends: {0}", obj.data.friends.Length));
				} else {
					Debug.Log(obj.data.message);
				}

			}

		}

		if (GUI.Button(new Rect(250, 60, 200, 24), "⇑ Outgoing ⇑")) {

			if (!_isLoggedIn) {
				Debug.Log("User must be authorized for this action");
				return;
			}

			new GetFriends(FrienStatus.Outgoing, Callback);

			void Callback(Responce<FriendList> obj) {

				Debug.Log(string.Format("Friend request callback (OutgoingFriends). Staus: {0}", obj.status));

				if (obj.data.friends != null && obj.data.friends.Length > 0) {
					Debug.Log(string.Format("Tatal Outgoing friends: {0}", obj.data.friends.Length));
				} else {
					Debug.Log(obj.data.message);
				}

			}

		}


		if (GUI.Button(new Rect(250, 85, 200, 24), "⇑SendRerquest⇑")) {

			SendFriendRequest(FriendRequestType.Send, TargetFriendId);

		}

		if (GUI.Button(new Rect(250, 110, 200, 24), "⇑ApproveRequest⇑")) {

			SendFriendRequest(FriendRequestType.Approve, TargetFriendId);

		}

		if (GUI.Button(new Rect(250, 135, 200, 24), "⇑RejectRequest⇑")) {

			SendFriendRequest(FriendRequestType.Reject, TargetFriendId);

		}

		if (GUI.Button(new Rect(250, 160, 200, 24), "⇑DeclineRequest⇑")) {

			SendFriendRequest(FriendRequestType.Decline, TargetFriendId);

		}

		if (GUI.Button(new Rect(250, 185, 200, 24), "⇑RemoveFriend⇑")) {

			SendFriendRequest(FriendRequestType.Remove, TargetFriendId);

		}





	}

	private void SendFriendRequest(FriendRequestType reqType, int targetFiend) {

		if (!_isLoggedIn) {
			Debug.Log("User must be authorized for this action");
			return;
		}

		new FriendRequest(reqType, targetFiend, Callback);

		void Callback(Responce<ClearResponce> obj) {

			Debug.Log(string.Format("Friend request callback ({2}). Staus: {0} + {1} |FriendID: {3}", obj.status, obj.data.messages, reqType.ToString(), targetFiend));

		}

	}

}
