using System;
using System.Text;
using UnityEngine;
using ZXing;
using ZXing.QrCode;

namespace KAMUNITY;

public static class QRLogin
{
	public enum STATUS
	{
		EAX_SPPASS_ERROR_CLOSE = 0,
		EAX_SPPASS_ERROR_OPENING = 1,
		EAX_SPPASS_ERROR_PROMPTED = 2,
		EAX_SPPASS_ERROR_PRESENTED = 3,
		EAX_SPPASS_ERROR_CLOSING = 4,
		EAX_SPPASS_ERROR_PROCESSING = 1,
		EA3_SPPASS_ERROR_NOTOPEN = -4,
		EAX_SPPASS_ERROR_COMM = -13,
		EAX_SPPASS_ERROR_DISABLED = -15,
		EAX_SPPASS_ERROR_UNEXPECTED = -16,
		EAX_SPPASS_ERROR_MAINTENANCE = -17,
		EAX_SPPASS_ERROR_ABORTED = -22,
		EAX_SPPASS_ERROR_NOTPRESENTED = -28
	}

	[KamunityDllApiClass]
	internal static class InternalMethod
	{
		public delegate void FUNC_QRBegin();

		public delegate STATUS FUNC_GetQRStatus(IntPtr code, IntPtr cardType, IntPtr cardID);

		public delegate void FUNC_QREntryEnd();

		public static FUNC_QRBegin QRBegin { get; private set; }

		public static FUNC_GetQRStatus GetQRStatus { get; private set; }

		public static FUNC_QREntryEnd QREntryEnd { get; private set; }
	}

	public static void Begin()
	{
		Debug.Log("<color=yellow> --- QR BEGIN --- </color>");
		InternalMethod.QRBegin();
	}

	public static void End()
	{
		InternalMethod.QREntryEnd();
		Debug.Log("<color=yellow> ---- QR END ---- </color>");
	}

	public static Sprite GetQRCode()
	{
		Sprite result = null;
		StringBuilder stringBuilder = new StringBuilder(256);
		StringBuilder stringBuilder2 = new StringBuilder(64);
		StringBuilder stringBuilder3 = new StringBuilder(64);
		using (MarshalHeap marshalHeap = new MarshalHeap(2 * stringBuilder.Capacity))
		{
			using MarshalHeap marshalHeap2 = new MarshalHeap(2 * stringBuilder2.Capacity);
			using MarshalHeap marshalHeap3 = new MarshalHeap(2 * stringBuilder3.Capacity);
			STATUS sTATUS = InternalMethod.GetQRStatus(marshalHeap.buffer, marshalHeap2.buffer, marshalHeap3.buffer);
			Debug.Log($"QRLogin fuga {sTATUS}");
			if (sTATUS >= STATUS.EAX_SPPASS_ERROR_PROMPTED)
			{
				stringBuilder.Append(marshalHeap.ToStringAnsi());
				result = GenerateQRcode(stringBuilder.ToString());
			}
		}
		return result;
	}

	private static Sprite GenerateQRcode(string URL)
	{
		BarcodeWriter barcodeWriter = new BarcodeWriter
		{
			Format = BarcodeFormat.QR_CODE,
			Options = new QrCodeEncodingOptions
			{
				Height = 800,
				Width = 800,
				Margin = 0
			}
		};
		Texture2D texture2D = new Texture2D(800, 800);
		texture2D.SetPixels32(barcodeWriter.Write(URL));
		texture2D.Apply();
		return Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
	}

	public static (int, string) GetCardTypeID()
	{
		StringBuilder stringBuilder = new StringBuilder(256);
		StringBuilder stringBuilder2 = new StringBuilder(64);
		StringBuilder stringBuilder3 = new StringBuilder(64);
		using (MarshalHeap marshalHeap = new MarshalHeap(2 * stringBuilder.Capacity))
		{
			using MarshalHeap marshalHeap2 = new MarshalHeap(2 * stringBuilder2.Capacity);
			using MarshalHeap marshalHeap3 = new MarshalHeap(2 * stringBuilder3.Capacity);
			if (InternalMethod.GetQRStatus(marshalHeap.buffer, marshalHeap2.buffer, marshalHeap3.buffer) >= STATUS.EAX_SPPASS_ERROR_PRESENTED)
			{
				stringBuilder2.Append(marshalHeap2.ToStringAnsi());
				stringBuilder3.Append(marshalHeap3.ToStringAnsi());
				Debug.Log("<color=yellow> QR CARDType [" + stringBuilder2.ToString() + "] CARDID " + stringBuilder3.ToString() + " </color>");
				if (int.TryParse(stringBuilder2.ToString(), out var result))
				{
					return (result, stringBuilder3.ToString());
				}
				return (-1, "");
			}
		}
		return (-1, "");
	}

	public static STATUS GetStatus()
	{
		StringBuilder stringBuilder = new StringBuilder(256);
		StringBuilder stringBuilder2 = new StringBuilder(64);
		StringBuilder stringBuilder3 = new StringBuilder(64);
		using MarshalHeap marshalHeap = new MarshalHeap(2 * stringBuilder.Capacity);
		using MarshalHeap marshalHeap2 = new MarshalHeap(2 * stringBuilder2.Capacity);
		using MarshalHeap marshalHeap3 = new MarshalHeap(2 * stringBuilder3.Capacity);
		return InternalMethod.GetQRStatus(marshalHeap.buffer, marshalHeap2.buffer, marshalHeap3.buffer);
	}
}
