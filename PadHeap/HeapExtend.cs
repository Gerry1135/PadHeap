using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PadHeap
{
	public class ListItem
	{
		System.Object[] array = new System.Object[1024];
		ListItem next;
	
		public ListItem(ListItem nextItem)
		{
			next = nextItem;
	
			// make allocations in smaller blocks to avoid them to be treated in a special way, which is designed for large blocks
			for (int i = 0; i < 1024; i++)
				array[i] = new byte[1024];
		}
	}
	
	
	[KSPAddon(KSPAddon.Startup.EveryScene, false)]
	public class HeapExtend : MonoBehaviour
    {
		ApplicationLauncherButton buttonAppLaunch = null;

		static Texture2D texAppLaunch;
		static ListItem final = null;
		
        public HeapExtend()
        {
			Trace("PadHeap.HeapExtend");
            Trace("ApplicationLauncher is " + (ApplicationLauncher.Ready ? "" : "not ") + "ready");
        }

		void OnDestroy()
		{
			Trace("HeapExtend.OnDestroy");
			Trace("ApplicationLauncher is " + (ApplicationLauncher.Ready ? "" : "not ") + "ready");

			if (buttonAppLaunch != null)
			{
				ApplicationLauncher.Instance.RemoveModApplication(buttonAppLaunch);
				buttonAppLaunch = null;
			}
		}

		public void Update()
		{
			if (buttonAppLaunch == null)
			{
				if (ApplicationLauncher.Ready)
				{
					if (texAppLaunch == null)
					{
						texAppLaunch = new Texture2D(38, 38, TextureFormat.RGBA32, false);
						texAppLaunch.LoadImage(System.IO.File.ReadAllBytes(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "applaunch.png")));
					}

					buttonAppLaunch = ApplicationLauncher.Instance.AddModApplication(
						ExtendHeap,
						ExtendHeap,
						null,
						null,
						null,
						null,
						ApplicationLauncher.AppScenes.ALWAYS,
						texAppLaunch
						);

					buttonAppLaunch.SetFalse(false);
				}
				else
				{
					Trace("ApplicationLauncher is not ready in Update");
				}
			}
		}

		void ExtendHeap()
		{
			Trace("HeapExtend.ExtendHeap");

			if (buttonAppLaunch)
				buttonAppLaunch.SetFalse(false);

			ListItem listHead = null;
			long TotalMem = 128 * 1024 * 1024;
			long StartMem = GC.GetTotalMemory(false);
			Trace("StartMem = " + StartMem);

			try
			{
				do
				{
					// Create a new item at the head of the list
					listHead = new ListItem(listHead);
				}
				while ((GC.GetTotalMemory(false) - StartMem) < TotalMem);
			}
			catch (Exception e)
			{
				Trace("Exception in ExtendHeap: " + e);
			}

			final = new ListItem(null);

			Trace("EndMem = " + GC.GetTotalMemory(false));

			// Release the whole list
			listHead = null;
		}

		private void Trace(String message)
		{
			print("[PadHeap] " + message);
		}
	}
}
