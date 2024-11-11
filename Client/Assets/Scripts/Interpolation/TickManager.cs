



namespace Transport.Server {

	using System;
	using Transport.Messaging;
	using Transport.Universal;

	public class TickManager : Manager, IFixedUpdate {

		public static uint CurrentTick { get; private set; } = 0;

		public override void Final() {
			base.Final();
			CurrentTick = 0;
			Game.Updating_Subscribe(this);
		}

		public void FixedUpdate(float timeDelta) {

			if (CurrentTick % 200 == 0) {
				SendSynsMsg();
			}

			CurrentTick++;

			if (CurrentTick + 1 == uint.MaxValue) {
				CurrentTick = 0;
				SendCalibrationTick();
			}

		}

		private void SendSynsMsg() {
			Multihost.ConnectionsManager.SendToAll(MsgType.TickSyns, BitConverter.GetBytes(CurrentTick), Multihost.UnreliableChanel);
		}

		private void SendCalibrationTick() {
			Multihost.ConnectionsManager.SendToAll(MsgType.TickCalibaration, BitConverter.GetBytes(0), Multihost.ReliableChanel);
		}

	}

}

namespace Transport.Universal {

	using System;
	using UnityEngine;
	using Transport.Messaging;

	public class TickManager : Manager, IFixedUpdate {

		private readonly uint _tickDivergenceTolerance = 1;

		private uint _serverTick = 2;

		public uint ServerTick {

			get => _serverTick;

			private set {
				_serverTick = value;
				InterpolationTick = value - TickBetweenPositionUpdates;
			}

		}

		public uint InterpolationTick { get; private set; }
		private uint _tickBetweenPositionUpdates = 2;

		public uint TickBetweenPositionUpdates {

			get => _tickBetweenPositionUpdates;

			private set {
				_tickBetweenPositionUpdates = value;
				InterpolationTick = ServerTick - value;
			}

		}

		public void FixedUpdate(float timeDelta) {

			ServerTick++;

		}

		public override void Final() {
			base.Final();
			Multihost.MsgReader.AddHandler(MsgType.TickSyns, SynsServerTick);
			Multihost.MsgReader.AddHandler(MsgType.TickCalibaration, ResetTicks);
			Game.Updating_Subscribe(this);
		}

		private void SynsServerTick(NetworkMsg msg) {

			uint tick = msg.reader.ReadUInt32();

			if (Mathf.Abs(ServerTick - tick) > _tickDivergenceTolerance) {
				ServerTick = tick;
			}

		}

		private void ResetTicks(NetworkMsg msg) {

			ServerTick = 0;
			InterpolationTick = 0;

		}

	}

}
