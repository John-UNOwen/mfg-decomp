using MFG.Assets;

public class CutinAssetKey
{
	public string ObjectID;

	public CutinAssetType CutinAssetType;

	public override int GetHashCode()
	{
		return ObjectID.GetHashCode() ^ CutinAssetType.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is CutinAssetKey cutinAssetKey))
		{
			return false;
		}
		if (ObjectID == cutinAssetKey.ObjectID)
		{
			return CutinAssetType == cutinAssetKey.CutinAssetType;
		}
		return false;
	}
}
