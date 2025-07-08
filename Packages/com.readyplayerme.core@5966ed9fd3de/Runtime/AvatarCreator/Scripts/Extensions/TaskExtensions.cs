using System;
using System.Threading.Tasks;

namespace ReadyPlayerMe.AvatarCreator
{
    public static class TaskExtensions
    {
        public const string ON_REQUEST_CANCELLED_MESSAGE = "Request was cancelled";
        private const string ON_OPERATION_CANCELLED_MESSAGE = "Operation was cancelled";
        public static async Task HandleCancellation(Task taskToWait, Action onSuccess = null)
        {
            try
            {
                await taskToWait;
                onSuccess?.Invoke();
            }
            catch (Exception ex)
            {
                if (ex.Message != ON_REQUEST_CANCELLED_MESSAGE && ex.Message != ON_OPERATION_CANCELLED_MESSAGE)
                {
                    throw;
                }
            }
        }
        
        public static async Task<T> HandleCancellation<T>(Task<T> taskToWait)
        {
            try
            {
                return await taskToWait;
            }
            catch (Exception ex)
            {
                if (ex.Message != ON_REQUEST_CANCELLED_MESSAGE && ex.Message != ON_OPERATION_CANCELLED_MESSAGE)
                {
                    throw;
                }
            }
            return default;
        }
    }
}
