using i5.Toolkit.Core.ExperienceAPI;
using i5.Toolkit.Core.Utilities;
using i5.Toolkit.Core.VerboseLogging;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MirageXR
{
	public class XApiMockWebConnector : IRestConnector
	{
		public Task<WebResponse<string>> DeleteAsync(string uri, Dictionary<string, string> headers = null)
		{
			AppLog.LogDebug($"Delete request sent to {uri} (mocked because sent from editor)");
			return Task.FromResult(CreateFakeWebResponse());
		}

		public Task<WebResponse<string>> GetAsync(string uri, Dictionary<string, string> headers = null)
		{
			AppLog.LogDebug($"Get request sent to {uri} (mocked because sent from editor)");
			return Task.FromResult(CreateFakeWebResponse());
		}

		public Task<WebResponse<string>> PostAsync(string uri, string postJson, Dictionary<string, string> headers = null)
		{
			AppLog.LogDebug($"Post statement sent to {uri} (mocked because sent from editor)\nJSON:\n{postJson}");
			return Task.FromResult(CreateFakeWebResponse());
		}

		public Task<WebResponse<string>> PostAsync(string uri, byte[] postData, Dictionary<string, string> headers = null)
		{
			string json = Encoding.UTF8.GetString(postData);
			AppLog.LogDebug($"xAPI statement for {uri} (mocked because sent from editor)\nStatement JSON:\n{json}", this);

			return Task.FromResult(CreateFakeWebResponse());
		}

		public Task<WebResponse<string>> PutAsync(string uri, string putJson, Dictionary<string, string> headers = null)
		{
			AppLog.LogDebug($"Put request sent to {uri} (mocked because sent from editor)\nJSON:\n{putJson}");
			return Task.FromResult(CreateFakeWebResponse());
		}

		public Task<WebResponse<string>> PutAsync(string uri, byte[] putData, Dictionary<string, string> headers = null)
		{
			string json = Encoding.UTF8.GetString(putData);
			AppLog.LogDebug($"Put request sent to {uri} (mocked because sent from editor)\nJSON:\n{json}");
			return Task.FromResult(CreateFakeWebResponse());
		}

		private WebResponse<string> CreateFakeWebResponse()
		{
			WebResponse<string> res = new WebResponse<string>("mock call success", null, 200);
			return res;
		}
	}
}