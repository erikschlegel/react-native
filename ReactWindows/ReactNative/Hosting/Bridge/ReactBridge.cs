﻿using Newtonsoft.Json.Linq;
using ReactNative.Bridge;
using ReactNative.Bridge.Queue;
using System;

namespace ReactNative.Hosting.Bridge
{
    /// <summary>
    /// Class to the JavaScript execution environment and means of transport
    /// for messages between JavaScript and the native environment.
    /// </summary>
    public class ReactBridge : IReactBridge
    {
        private readonly IJavaScriptExecutor _jsExecutor;
        private readonly IReactCallback _reactCallback;
        private readonly IMessageQueueThread _nativeModulesQueueThread;

        /// <summary>
        /// Instantiates the <see cref="IReactBridge"/>.
        /// </summary>
        /// <param name="jsExecutor">The JavaScript executor.</param>
        /// <param name="reactCallback">The native callback handler.</param>
        /// <param name="nativeModulesQueueThread">
        /// The native modules queue thread.
        /// </param>
        public ReactBridge(
            IJavaScriptExecutor jsExecutor,
            IReactCallback reactCallback,
            IMessageQueueThread nativeModulesQueueThread)
        {
            if (jsExecutor == null)
                throw new ArgumentNullException(nameof(jsExecutor));
            if (reactCallback == null)
                throw new ArgumentNullException(nameof(reactCallback));
            if (nativeModulesQueueThread == null)
                throw new ArgumentNullException(nameof(nativeModulesQueueThread));

            _jsExecutor = jsExecutor;
            _reactCallback = reactCallback;
            _nativeModulesQueueThread = nativeModulesQueueThread;
        }

        /// <summary>
        /// Calls a JavaScript function.
        /// </summary>
        /// <param name="moduleId">The module ID.</param>
        /// <param name="methodId">The method ID.</param>
        /// <param name="arguments">The arguments.</param>
        public void CallFunction(int moduleId, int methodId, JArray arguments)
        {
            var allArgs = new JArray
            {
                moduleId,
                methodId,
                arguments,
            };

            var message = new JObject
            {
                { "module", "BatchedBridge" },
                { "method", "callFunctionReturnFlushedQueue" },
                { "context", 15 },
                { "args", allArgs },
            };

            var messageArray = new JArray
            {
                message,
            };

            // TODO: actually introduce batching here...
            var processBatchArgs = new JArray
            {
                messageArray,
            };

            var response = _jsExecutor.Call("BatchedBridge", "processBatch", processBatchArgs);

            ProcessResponse(response);
        }

        /// <summary>
        /// Invokes a JavaScript callback.
        /// </summary>
        /// <param name="callbackId">The callback ID.</param>
        /// <param name="arguments">The arguments.</param>
        public void InvokeCallback(int callbackId, JArray arguments)
        {
            var allArgs = new JArray
            {
                callbackId,
                arguments,
            };

            var message = new JObject
            {
                { "module", "BatchedBridge" },
                { "method", "invokeCallbackAndReturnFlushedQueue" },
                { "args", allArgs },
            };

            var messageArray = new JArray
            {
                message,
            };

            var processBatchArgs = new JArray
            {
                messageArray,
            };

            var response = _jsExecutor.Call("BatchedBridge", "processBatch", processBatchArgs);

            ProcessResponse(response);
        }

        /// <summary>
        /// Sets a global JavaScript variable.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <param name="jsonEncodedArgument">The JSON-encoded value.</param>
        public void SetGlobalVariable(string propertyName, string jsonEncodedArgument)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            _jsExecutor.SetGlobalVariable(propertyName, JToken.Parse(jsonEncodedArgument));
        }

        private void ProcessResponse(JToken response)
        {
            var messages = response as JArray;
            if (messages == null)
            {
                return;
            }

            var moduleIds = messages[0].ToObject<int[]>();
            var methodIds = messages[1].ToObject<int[]>();
            var paramsArray = (JArray)messages[2];

            _nativeModulesQueueThread.RunOnQueue(() =>
            {
                for (var i = 0; i < moduleIds.Length; i++)
                {
                    var moduleId = moduleIds[i];
                    var methodId = methodIds[i];
                    var args = (JArray)paramsArray[i];

                    _reactCallback.Invoke(moduleId, methodId, args);
                };

                _reactCallback.OnBatchComplete();
            });
        }
    }
}