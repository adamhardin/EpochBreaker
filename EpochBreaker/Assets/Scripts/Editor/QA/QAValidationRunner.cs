#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EpochBreaker.Editor.QA
{
    public static class QAValidationRunner
    {
        private const string LastRunKey = "EpochBreaker.QA.LastValidationSummary";
        private const string LastRunTimeKey = "EpochBreaker.QA.LastValidationTimeUtc";

        [MenuItem("Epoch Breaker/QA/Run Automated Validation", priority = 31)]
        public static void RunAll()
        {
            var messages = new List<string>();
            int totalPassed = 0;
            int totalFailed = 0;

            messages.Add("================================================");
            messages.Add("Epoch Breaker - Automated Validation");
            messages.Add($"Run at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            messages.Add("================================================");

            RunSuite("XORShift64 Tests", "EpochBreaker.Tests.XORShift64Tests", ref totalPassed, ref totalFailed, messages);
            RunSuite("LevelID Tests", "EpochBreaker.Tests.LevelIDTests", ref totalPassed, ref totalFailed, messages);
            RunSuite("Level Generator Tests", "EpochBreaker.Tests.LevelGeneratorTests", ref totalPassed, ref totalFailed, messages);

            messages.Add("================================================");
            messages.Add($"TOTAL: {totalPassed} passed, {totalFailed} failed");
            messages.Add(totalFailed == 0 ? "ALL TESTS PASSED" : "SOME TESTS FAILED");
            messages.Add("================================================");

            foreach (var message in messages)
            {
                if (message.Contains("FAIL"))
                    Debug.LogError(message);
                else if (message.Contains("WARN"))
                    Debug.LogWarning(message);
                else
                    Debug.Log(message);
            }

            var summary = $"Passed={totalPassed}, Failed={totalFailed}";
            EditorPrefs.SetString(LastRunKey, summary);
            EditorPrefs.SetString(LastRunTimeKey, DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss 'UTC'"));
        }

        public static string GetLastRunSummary()
        {
            var summary = EditorPrefs.GetString(LastRunKey, "Not run");
            var time = EditorPrefs.GetString(LastRunTimeKey, "N/A");
            return $"Last Validation: {summary} at {time}";
        }

        private static void RunSuite(string name, string typeName,
            ref int totalPassed, ref int totalFailed, List<string> output)
        {
            output.Add($"=== {name} ===");
            if (!TryInvokeRunAll(typeName, out int passed, out int failed, out List<string> messages))
            {
                output.Add($"  WARN: Unable to run {name}. Ensure Unity Test assemblies are compiled (UNITY_INCLUDE_TESTS). ");
                return;
            }

            totalPassed += passed;
            totalFailed += failed;
            foreach (var msg in messages)
            {
                output.Add(msg);
            }
        }

        private static bool TryInvokeRunAll(string typeName, out int passed, out int failed, out List<string> messages)
        {
            passed = 0;
            failed = 0;
            messages = new List<string>();

            var type = FindType(typeName);
            if (type == null)
            {
                messages.Add($"  WARN: Type not found: {typeName}");
                return false;
            }

            var method = type.GetMethod("RunAll", BindingFlags.Public | BindingFlags.Static);
            if (method == null)
            {
                messages.Add($"  WARN: RunAll() not found on {typeName}");
                return false;
            }

            var result = method.Invoke(null, null);
            if (result == null) return false;

            var resultType = result.GetType();
            var passedField = resultType.GetField("Item1");
            var failedField = resultType.GetField("Item2");
            var messagesField = resultType.GetField("Item3");

            if (passedField == null || failedField == null || messagesField == null)
            {
                messages.Add($"  WARN: Unexpected result format from {typeName}.RunAll()");
                return false;
            }

            passed = (int)passedField.GetValue(result);
            failed = (int)failedField.GetValue(result);
            messages = messagesField.GetValue(result) as List<string> ?? new List<string>();
            return true;
        }

        private static System.Type FindType(string typeName)
        {
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(typeName);
                if (type != null) return type;
            }
            return null;
        }
    }
}
#endif
