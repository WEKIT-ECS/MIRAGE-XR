using Fusion;
using LearningExperienceEngine.DataModel;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
	public class CollaborativeSessionData : NetworkBehaviour
	{
		[Networked, OnChangedRender(nameof(OnNetworkedStepGuidChanged))]
		public Guid StepGuid { get; set; }

		public override void Spawned()
		{
			base.Spawned();

			RootObject.Instance.LEE.StepManager.OnStepChanged += OnStepChanged;

			if (HasStateAuthority)
			{
				StepGuid = RootObject.Instance.LEE.StepManager.CurrentStep.Id;
			}
			else
			{
				OnNetworkedStepGuidChanged();
			}
		}

		public override void Despawned(NetworkRunner runner, bool hasState)
		{
			base.Despawned(runner, hasState);

			RootObject.Instance.LEE.StepManager.OnStepChanged -= OnStepChanged;
		}

		private void OnStepChanged(ActivityStep newStep)
		{
			if (HasStateAuthority)
			{
				Debug.LogDebug("Setting StepGuid directly to " + newStep.Id);
				StepGuid = newStep.Id;
			}
			else
			{
				// clients that don't have the state authority need to inform the client with the state authority
				Debug.LogDebug("Sending Rpc SetStepGuidRpc to change step guid to " + newStep.Id);
				SetStepGuidRpc(newStep.Id);
			}
		}

		[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
		public void SetStepGuidRpc(Guid stepGuid)
		{
			Debug.LogDebug("Received SetStepGuidRpc on StateAuthority. Setting StepGuid to " + stepGuid);
			StepGuid = stepGuid;
		}

		private void OnNetworkedStepGuidChanged()
		{
			Debug.LogInfo("(Networked Change) Guid is now " + StepGuid, this);
			RootObject.Instance.LEE.StepManager.GoToStep(StepGuid);
		}
	}
}
