using UnityEngine;
using System.Collections;
using SimpleJSON;

public class LocationFunctions : MonoBehaviour {
	private string locationinfo;
	private WWW www;
	//private string urlstring = "http://maps.googleapis.com/maps/api/geocode/json?latlng=49.4762,29.087&sensor=false";
	private string urlstring1 = "http://maps.googleapis.com/maps/api/geocode/json?latlng=";
	private string urlstring2 = "&sensor=false";
	private string teststring = "http://maps.googleapis.com/maps/api/geocode/json?latlng=49.4762,29.087&sensor=false";
	private string locaddress = "";

	IEnumerator Start() {
		//var result;
		//StartCoroutine (GetAddressString(teststring));

		if (!Input.location.isEnabledByUser)
			yield return 0;
		
		Input.location.Start(10f);
		int maxWait = 20;
		while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0) {
			yield return new WaitForSeconds(1);
			maxWait--;
		}
		if (maxWait < 1) {
			locationinfo = "Timed out";
			print("Timed out");
			yield return 0;
		}
		if (Input.location.status == LocationServiceStatus.Failed) {
			locationinfo = "Unable to determine device location";
			print("Unable to determine device location");
			yield return 0;
		} else
		{
			string completeurl;
			completeurl = urlstring1 + Input.location.lastData.latitude + "," + Input.location.lastData.longitude + urlstring2;
			StartCoroutine (GetAddressString(completeurl));

			//var result = JSON.Parse(www.text);
			//locationinfo = result["formatted_address"].Value;
			//locationinfo = "Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude;
			print("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
			Input.location.Stop();
		}
	}
	void Update()
	{
		if(www.isDone)
		{
			if(locaddress == "")
			{
				var R = JSON.Parse(www.text);
				locationinfo=R["results"][0]["formatted_address"].Value;
			}

		}

		if(Input.touchCount >= 1)
		{
			Application.LoadLevel("AndroidNoMultiplayer");
		}

		if(Input.GetMouseButtonDown(0))
		{
			Application.LoadLevel("gameSceneMac");
		}
	}
	void OnGUI()
	{
		GUILayout.Label("GPS Status: " + Input.location.status.ToString());
		GUILayout.Label("Your Location: " + locationinfo);
	}

	IEnumerator GetAddressString(string url)
	{
		//Debug.Log("url: " + url);
		www = new WWW(url);
		//Debug.Log(www.text);
		yield return www;
	}
}
