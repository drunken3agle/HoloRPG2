#define SHOW_DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
	Displays Unity's Debug.Log() output in a UI text field
 */
 [RequireComponent(typeof(Text))]
public class DebugToUI : MonoBehaviour {
#if SHOW_DEBUG

	[SerializeField] private Text debugOutput;
	[SerializeField] private int maxLines;

	private Queue<string> LogMessages;

	void OnEnable() {
		Application.logMessageReceived += PrintLogToSceen;

		LogMessages = new Queue<string>(maxLines);
	}

	void OnDisable() {
		Application.logMessageReceived -= PrintLogToSceen;
	}

	void Update() {
		string logString = "";
		foreach (string message in LogMessages) {
			logString += message;
		}

		debugOutput.text = logString;
	}

	private void PrintLogToSceen(string logString, string stackTrace, LogType type) {
		// Delete old messages before memory needs to be reallocated
		if (LogMessages.Count == maxLines) { 
			LogMessages.Dequeue();
		}

		string outputLine = "\n[" + type + "]: " + logString;

		switch (type) {
			case LogType.Exception: // Add stacktrace for exceptions
				outputLine += "\n" + stackTrace;
				break;
			default:
				break;
		}

		LogMessages.Enqueue(outputLine);
	}
	
#endif
}
