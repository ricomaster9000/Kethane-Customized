﻿using Kethane.UserInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Kethane.PartModules;

namespace Kethane.VesselModules
{
public class KethaneVesselScanner : VesselModule
{
	List<KethaneDetector> detectors;
	List<KethaneProtoDetector> protoDetectors;
	Dictionary<uint, IKethaneBattery> batteries;
	List<IKethaneBattery> batteryList;

	AudioSource PingEmpty;
	AudioSource PingDeposit;

	bool started;
	bool isScanning;

	void FindDetectors ()
	{
		detectors = vessel.FindPartModulesImplementing<KethaneDetector>();
		protoDetectors = new List<KethaneProtoDetector>();
		foreach (var det in detectors) {
			det.scanner = new KethaneProtoDetector(det);
			protoDetectors.Add (det.scanner);
		}
	}

	static void FindBatteries (Dictionary<uint, IKethaneBattery> batteries,
							   List<IKethaneBattery> batteryList,
							   List<Part> parts)
	{
		//FIXME should not be hard-coded
		int resID = "ElectricCharge".GetHashCode();
		foreach (var p in parts) {
			var res = p.Resources.Get (resID);
			if (res != null) {
				var bat = new KethaneBattery (res);
				batteries[p.flightID] = bat;
				batteryList.Add (bat);
			}
		}
	}

	static void FindBatteries (Dictionary<uint, IKethaneBattery> batteries,
							   List<IKethaneBattery> batteryList,
							   List<ProtoPartSnapshot> parts)
	{
		foreach (var p in parts) {
			foreach (var res in p.resources) {
				//FIXME should not be hard-coded
				if (res.resourceName == "ElectricCharge") {
					var bat = new KethaneProtoBattery (res);
					batteries[p.flightID] = bat;
					batteryList.Add (bat);
				}
			}
		}
	}

	void FindBatteries ()
	{
		batteries = new Dictionary<uint, IKethaneBattery> ();
		batteryList = new List<IKethaneBattery> ();
		if (vessel.loaded) {
			FindBatteries (batteries, batteryList, vessel.parts);
		} else {
			FindBatteries (batteries, batteryList,
						   vessel.protoVessel.protoPartSnapshots);
		}
	}

	public void UpdateDetecting (KethaneDetector det)
	{
		int index = detectors.IndexOf (det);
		protoDetectors[index].IsDetecting = det.IsDetecting;
		// FixedUpdate will set to the correct value, but need to ensure
		// FixedUpdate gets run at least once.
		isScanning = true;
	}

	public override bool ShouldBeActive ()
	{
		bool active = base.ShouldBeActive ();
		active &= (!started || isScanning);
		return active;
	}

	void onVesselCreate (Vessel v)
	{
		if (v == vessel) {
			GameEvents.onVesselCreate.Remove (onVesselCreate);
			FindBatteries ();
		}
	}

	protected override void OnLoad (ConfigNode node)
	{
		protoDetectors = new List<KethaneProtoDetector> ();
		for (int i = 0; i < node.nodes.Count; i++) {
			ConfigNode n = node.nodes[i];
			if (n.name == "Detector") {
				var det = new KethaneProtoDetector (n);
				protoDetectors.Add (det);
			}
		}

		GameEvents.onVesselCreate.Add (onVesselCreate);

		// ensure the scanner runs at least once when the vessel is not loaded
		isScanning = true;
	}

	protected override void OnSave (ConfigNode node)
	{
		if (protoDetectors == null) {
			return;
		}
		for (int i = 0; i < protoDetectors.Count; i++) {
			protoDetectors[i].Save (node);
		}
	}

	void onVesselWasModified (Vessel v)
	{
		if (v == vessel) {
			FindDetectors ();
			FindBatteries ();
		}
	}

	protected override void OnAwake ()
	{
		GameEvents.onVesselWasModified.Add (onVesselWasModified);
	}

	void OnDestroy ()
	{
		GameEvents.onVesselWasModified.Remove (onVesselWasModified);
	}

	bool ValidVesselType (VesselType type)
	{
		if (type > VesselType.Base) {
			// EVA and Flag
			return false;
		}
		if (type == VesselType.SpaceObject
			|| type == VesselType.Unknown) {
			// asteroids
			return false;
		}
		// Debris, Probe, Relay, Rover, Lander, Ship, Plane, Station, Base
		return true;
	}

	protected override void OnStart ()
	{
		started = true;

		if (!ValidVesselType (vessel.vesselType)) {
			vessel.vesselModules.Remove (this);
			Destroy (this);
			return;
		}

		PingEmpty = gameObject.AddComponent<AudioSource>();
		PingEmpty.clip = GameDatabase.Instance.GetAudioClip("Kethane/Sounds/echo_empty");
		PingEmpty.volume = 1;
		PingEmpty.loop = false;
		PingEmpty.Stop();

		PingDeposit = gameObject.AddComponent<AudioSource>();
		PingDeposit.clip = GameDatabase.Instance.GetAudioClip("Kethane/Sounds/echo_deposit");
		PingDeposit.volume = 1;
		PingEmpty.loop = false;
		PingDeposit.Stop();
	}

	public override void OnLoadVessel ()
	{
		FindDetectors ();
		FindBatteries ();
	}

	public override void OnUnloadVessel ()
	{
		Debug.LogFormat("[KethaneVesselScanner] OnUnloadVessel: {0} {1} {2}", vessel.gameObject == gameObject, enabled, gameObject.activeInHierarchy);
	}

	public void OnVesselUnload()
	{
		Debug.LogFormat("[KethaneVesselScanner] OnVesselUnload: {0}", vessel.name);
	}

	double DrawEC (double amount)
	{
		double availEC = 0;
		for (int i = batteryList.Count; i-- > 0; ) {
			if (batteryList[i].flowState) {
				availEC += batteryList[i].amount;
			}
		}
		if (amount >= availEC) {
			amount = availEC;
			if (availEC > 0) {
				for (int i = batteryList.Count; i-- > 0; ) {
					batteryList[i].amount = 0;
				}
			}
		} else {
			for (int i = batteryList.Count; i-- > 0; ) {
				var bat = batteryList[i];
				if (bat.flowState) {
					double bamt = bat.amount;
					double amt = amount * bamt / availEC;
					if (amt > bamt) {
						amt = bamt;
					}
					bat.amount = bamt - amt;
				}
			}
		}
		return amount;
	}

	void FixedUpdate ()
	{
		if (protoDetectors == null) {
			return;
		}

		double Altitude = vessel.getTrueAltitude ();
		var body = vessel.mainBody;
		var position = vessel.transform.position;
		var detected = false;
		var ping = false;
		isScanning = false;
		for (int i = protoDetectors.Count; i-- > 0; ) {
			var detector = protoDetectors[i];
			if (!detector.IsDetecting) {
				continue;
			}
			isScanning = true;
			if (Altitude < detector.DetectingHeight) {
				double req = detector.PowerConsumption * TimeWarp.fixedDeltaTime;
				double drawn = DrawEC (req);
				detector.powerRatio = drawn / req;

				detector.TimerEcho += TimeWarp.deltaTime * detector.powerRatio;
				var TimerThreshold = detector.DetectingPeriod * (1 + Altitude * 2e-6);
				if (detector.TimerEcho >= TimerThreshold) {
					var cell = MapOverlay.GetCellUnder(body, position);
					for (int j = detector.resources.Count; j-- > 0; ) {
						var resource = detector.resources[j];
						var data = KethaneData.Current[resource][body];
						if (data.IsCellScanned(cell)) {
							continue;
						}
						ping = true;
						data.ScanCell(cell);
						if (data.Resources.GetQuantity(cell) != null) {
							detected = true;
						}
					}
					MapOverlay.Instance.RefreshCellColor(cell, body);
					detector.TimerEcho = 0;
				}
			}
		}
		if (ping && KethaneDetector.ScanningSound
			&& vessel == FlightGlobals.ActiveVessel) {
			(detected ? PingDeposit : PingEmpty).Play();
			(detected ? PingDeposit : PingEmpty).loop = false;
		}
	}
}
}