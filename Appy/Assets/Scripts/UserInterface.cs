using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UserInterface : MonoBehaviour {

	public ScriptManager scriptManager;
	public List<string> consoleHistory;
	public UILabel consoleHistoryLabel;
	public UIScrollView consoleSv;

	public void AddToConsoleHistory(string line) 
	{
		////Debug.Log("here");
		consoleHistory.Add(line);
		consoleHistoryLabel.text = consoleHistory.toString();
		consoleSv.UpdateScrollbars(true);
		//consoleSv.UpdatePosition();
	}

	public void ClearConsoleHistory() 
	{
		scriptManager.playClearSound();
		consoleHistory.Clear();
		consoleHistoryLabel.text = "";
	}

	public void saveScript() 
	{
		scriptManager.playButtonSound();
		scriptManager.saveScript();
	}

}
