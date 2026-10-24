using System.IO;
using UnityEngine;

namespace Konami.System.Net;

public class PolicyAgreeFile : SerializableFile
{
	private static PolicyAgreeFile instance;

	[SerializeField]
	private int contentsPolicy = -1;

	[SerializeField]
	private int stonePolicy = -1;

	[SerializeField]
	private int privacyPolicy = -1;

	[SerializeField]
	private int specifiedCommercialTransactionPolicy = -1;

	[SerializeField]
	private int fundSettlementPolicy = -1;

	[SerializeField]
	private int advertisePolicy = -1;

	public static int Contents => Instance.contentsPolicy;

	public static int Stone => instance.stonePolicy;

	public static int Privacy => Instance.privacyPolicy;

	public static int FundSettlement => Instance.fundSettlementPolicy;

	public static int SpecifiedCommercialTransactionPolicy => Instance.specifiedCommercialTransactionPolicy;

	public static int Advertise => instance.advertisePolicy;

	private static PolicyAgreeFile Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new PolicyAgreeFile();
			}
			return instance;
		}
	}

	private string Path => global::System.IO.Path.Combine(Application.persistentDataPath, "policyagree");

	public static void Reset(int contents = -1, int stone = -1, int privacy = -1, int specifiedCommercialTransaction = -1, int fundSettlement = -1, int advertise = -1)
	{
		instance = new PolicyAgreeFile(contents, stone, privacy, specifiedCommercialTransaction, fundSettlement, advertise);
	}

	public static int Get(Policy policy)
	{
		return policy switch
		{
			Policy.Contents => Contents, 
			Policy.Privacy => Privacy, 
			Policy.Stone => Stone, 
			Policy.FundSettlement => FundSettlement, 
			Policy.SpecifiedCommercialTransaction => SpecifiedCommercialTransactionPolicy, 
			Policy.Advertise => Advertise, 
			_ => -1, 
		};
	}

	private PolicyAgreeFile()
		: base(isCrypt: true, isCompress: true)
	{
		if (!base.Load(Path))
		{
			Debug.Log("利用規約同意済みバージョンファイルを新規作成します");
			base.Save(Path);
		}
	}

	private PolicyAgreeFile(int contents, int stone, int privacy, int specifiedCommercialTransaction, int fundSettlement, int advertise)
		: base(isCrypt: true, isCompress: true)
	{
		contentsPolicy = contents;
		stonePolicy = stone;
		privacyPolicy = privacy;
		specifiedCommercialTransactionPolicy = specifiedCommercialTransaction;
		fundSettlementPolicy = fundSettlement;
		advertisePolicy = advertise;
		Debug.Log("利用規約同意済みバージョンファイルを作成します");
		base.Save(Path);
	}
}
