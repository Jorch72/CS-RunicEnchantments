﻿using Assets.draco18s.runic.init;
using Assets.draco18s.util;
using System;
using UnityEngine;

namespace Assets.draco18s.runic.runes {
	public class RuneMultiplication : IExecutableRune {
		public bool Execute(Pointer pointer, ExecutionContext context) {
			object a = pointer.Pop();
			object b = pointer.Pop();
			if(a != null && b != null) {
				if(a is ValueType && b is ValueType) {
					if(MathHelper.IsInteger((ValueType)a) && MathHelper.IsInteger((ValueType)b)) {
						int c = (int)a * (int)b;
						pointer.Push(c);
					}
					else if(a is Vector3 || b is Vector3) {
						if(a is Vector3 && b is Vector3) {
							pointer.Push(Vector3.Cross((Vector3)a, (Vector3)b));
						}
						else if(a is Vector3) {
							pointer.Push(((Vector3)a) * (float)MathHelper.GetValue((ValueType)b));
						}
						else if(b is Vector3) {
							pointer.Push(((Vector3)b) * (float)MathHelper.GetValue((ValueType)a));
						}
					}
					else {
						double c = MathHelper.GetValue((ValueType)a) * MathHelper.GetValue((ValueType)b);
						pointer.Push(c);
					}
				}
				else {

				}
			}
			return true;
		}

		public IExecutableRune Register() {
			RuneRegistry.ALL_RUNES.Add('*', this);
			return this;
		}
	}
}