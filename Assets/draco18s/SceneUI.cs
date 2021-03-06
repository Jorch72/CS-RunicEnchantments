﻿using Assets.draco18s.generpg.init;
using Assets.draco18s.runic;
using Assets.draco18s.runic.init;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;
using Assets.draco18s.ui;

public class SceneUI : MonoBehaviour {
	public static SceneUI instance;
	public GameObject tooltip;
	public GameObject pointerPrefab;
	public GameObject source;
	public Transform canvas;
	private Coroutine execution;
	private bool doDebug;
	private bool pauseDebug;
	private Dictionary<Pointer,GameObject> pointerObjs;

	void Start () {
		instance = this;
		RuneRegistry.Initialize();
		ObjectRegistry.Initialize();
		transform.Find("Button").GetComponent<Button>().onClick.AddListener(delegate {
			transform.Find("Button").gameObject.GetComponent<Button>().interactable = false;
			execution = StartCoroutine(Execute(transform.Find("InputField").GetComponent<InputField>().text));
		});
		doDebug = false;
		Toggle pauser = canvas.Find("PauseTog").GetComponent<Toggle>();
		pauser.onValueChanged.AddListener(delegate { pauseDebug = pauser.isOn; });
		Toggle tog = canvas.Find("DebugTog").GetComponent<Toggle>();
		tog.onValueChanged.AddListener(delegate {
			doDebug = tog.isOn;
			if(!doDebug) {
				foreach(GameObject go in pointerObjs.Values) {
					Destroy(go);
				}
				pointerObjs.Clear();
			}
		});
		pointerObjs = new Dictionary<Pointer, GameObject>();
	}

	public static void ShowTooltip(Pointer p) {
		instance.tooltip.transform.Find("ManaTxt").GetComponent<Text>().text = p.GetMana().ToString();
		Text stxt = instance.tooltip.transform.Find("StackTxt").GetComponent<Text>();
		stxt.text = p.PrintStack();
		instance.tooltip.transform.Find("StackLabel").GetComponent<Text>().text = "Stack: " + p.GetStackSize();
		instance.tooltip.SetActive(true);
	}

	private IEnumerator Execute(string code) {
		foreach(GameObject go in pointerObjs.Values) {
			Destroy(go);
		}
		pointerObjs.Clear();
		yield return null;
		ExecutionContext context;
		ParseError err = Parser.Parse(code, source, out context);
		if(err.type != ParseErrorType.NONE || context == null) {
			ShowError(err);
			yield break;
		}
		context.Initialize();
		if(doDebug) {
			UpdateDebugGraphics(context);
			yield return new WaitForSeconds(2f);
		}
		bool continueExecuting = false;
		int counter = 0;
		do {
			if(pauseDebug) {
				yield return new WaitForSeconds(0.5f);
				continue;
			}
			counter++;
			continueExecuting = context.Tick();
			if(doDebug)
				UpdateDebugGraphics(context);
			yield return (doDebug ? new WaitForSeconds(2f) : null);
		} while(continueExecuting && counter < 10000);
		transform.Find("Button").gameObject.GetComponent<Button>().interactable = true;
	}

	private void UpdateDebugGraphics(ExecutionContext context) {
		ReadOnlyCollection<Pointer> pointers = context.GetPointers();
		foreach(Pointer p in pointers) {
			GameObject go;
			pointerObjs.TryGetValue(p, out go);
			if(go == null) {
				go = Instantiate(pointerPrefab, canvas);
				go.GetComponent<Button>().AddHover(delegate (Vector3 pos) {
					if(!pauseDebug) return;
					Pointer p2 = p;
					ShowTooltip(p2);
				});
				pointerObjs.Add(p, go);
			}
			((RectTransform)go.transform).anchoredPosition = new Vector2(-87 + 8 * p.position.x + (OffsetForDir(p.direction)), 85 - 19 * p.position.y);
			go.transform.localRotation = Quaternion.Euler(0, 0, RotationForDir(p.direction));
		}
		IEnumerable<KeyValuePair<Pointer, GameObject>> dead = pointerObjs.Where(x => x.Key.GetMana() <= 0);
		foreach(KeyValuePair<Pointer, GameObject> pair in dead) {
			pair.Value.GetComponent<Image>().color = Color.red;
		}
		dead = pointerObjs.Where(x => !pointers.Contains(x.Key) && x.Value != null);
		foreach(KeyValuePair<Pointer, GameObject> pair in dead) {
			//pointerObjs.Remove(pair.Key);
			StartCoroutine(WaitDestroy(pair.Key, pair.Value));
		}
	}

	private float RotationForDir(Direction direction) {
		switch(direction) {
			case Direction.DOWN:
				return 0;
			case Direction.LEFT:
				return -90;
			case Direction.RIGHT:
				return 90;
		}
		return 180;
	}

	private float OffsetForDir(Direction direction) {
		switch(direction) {
			case Direction.DOWN:
				return 0;
			case Direction.LEFT:
				return 2;
			case Direction.RIGHT:
				return 2;
		}
		return 0;
	}
/*>1234\
 /   5$$$;
 \"a"/$;
>67y$/;*/
	private IEnumerator WaitDestroy(Pointer k, GameObject obj) {
		obj.GetComponent<Image>().color = Color.red;
		yield return null;
		Destroy(obj);
		pointerObjs.Remove(k);
	}

	private void ShowError(ParseError err) {
		transform.Find("Button").gameObject.GetComponent<Button>().interactable = true;
		Debug.Log(err.type + ": '" + err.character + "' @ " + err.pos);
	}
}
